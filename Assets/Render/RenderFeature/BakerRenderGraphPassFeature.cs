using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class BakerRenderGraphPassFeature : ScriptableRendererFeature
    {
        public class MyCustomData : ContextItem
        {
            public TextureHandle textureToTransfer;

            public override void Reset()
            {
                textureToTransfer = TextureHandle.nullHandle;
            }
        }

        private class BakerRenderGraphPass : ScriptableRenderPass
        {
            private Settings settings;
            private string passName = "BakerRenderGraphPass";
            private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
            private RenderStateBlock m_RenderStateBlock;

            public BakerRenderGraphPass(Settings settings, string passName)
            {
                this.settings = settings;
                this.passName = passName;
                BlitTextureID = Shader.PropertyToID(settings.textureName);
                if (settings.ShaderTags != null && settings.ShaderTags.Length > 0)
                {
                    foreach (var ShaderTag in settings.ShaderTags)
                        m_ShaderTagIdList.Add(new ShaderTagId(ShaderTag));
                }
                else
                {
                    m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
                    m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                    m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
                    m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
                }

                m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing); 
                renderPassEvent = settings.renderPassEvent;
            }

            internal class PassData
            {
                internal RendererListHandle rendererList;
                internal Material material;
                internal int passId;

                internal TextureHandle source;
                // internal TextureHandle outTexHandle;
            }


            private int BlitTextureID = Shader.PropertyToID("DestTexture");

            private void AddPass(RenderGraph renderGraph, ContextContainer contextContainer, bool isOpaque,
                TextureHandle targetTex, TextureHandle depthTex, bool setGlobal)
            {
                var passName = isOpaque ? $"{this.passName}_Opaque" : $"{this.passName}_Transparent";
                using (var builder = renderGraph.AddRasterRenderPass(passName, out PassData passData))
                {
                    var resourceData = contextContainer.Get<UniversalResourceData>();
                    var renderingData = contextContainer.Get<UniversalRenderingData>();
                    var lightData = contextContainer.Get<UniversalLightData>();
                    var cameraData = contextContainer.Get<UniversalCameraData>();

                    var sortingCriteria = isOpaque
                        ? cameraData.defaultOpaqueSortFlags
                        : SortingCriteria.CommonTransparent;
                    var drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList,
                        renderingData, cameraData, lightData, sortingCriteria);
                    drawSettings.overrideMaterial = settings.overrideMat;
                    var sortSettings = drawSettings.sortingSettings;
                    GetTransparencySortingMode(cameraData.camera, ref sortSettings);
                    drawSettings.sortingSettings = sortSettings;
                    var filteringSettings = new FilteringSettings(
                        isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent,
                        settings.layerMask);

                    RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph,
                        ref renderingData.cullResults, drawSettings, filteringSettings, m_RenderStateBlock,
                        ref passData.rendererList);

                    builder.UseRendererList(passData.rendererList);
                    builder.SetRenderAttachment(targetTex, 0);
                    if (depthTex.IsValid()) builder.SetRenderAttachmentDepth(depthTex);

                    builder.SetRenderFunc((PassData passData, RasterGraphContext context) =>
                    {
                        context.cmd.DrawRendererList(passData.rendererList);
                    });
                    if (setGlobal) builder.SetGlobalTextureAfterPass(targetTex, BlitTextureID);
                }
            }
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                BlitTextureID = Shader.PropertyToID(settings.textureName);
                var blit = settings.afterRenderMaterial != null;
                var destination = TextureHandle.nullHandle;
                var cameraData = frameData.Get<UniversalCameraData>();
                var resourceData = frameData.Get<UniversalResourceData>();
                var desc = cameraData.cameraTargetDescriptor;
                var targetDesc = renderGraph.GetTextureDesc(resourceData.cameraColor);
                if (blit || !settings.blitToCameraTarget)
                {
                    var colorDesc = new TextureDesc((int)(settings.blitScale * desc.width),
                        (int)(settings.blitScale * desc.height))
                    {
                        clearColor = settings.clearColor,
                        clearBuffer = settings.clearFlag == ClearFlag.Color ||
                                      settings.clearFlag == ClearFlag.All,
                        format = targetDesc.colorFormat,
                        msaaSamples = targetDesc.msaaSamples,
                        name = $"{passName}_destination",
                        bindTextureMS = false
                    };
                    destination = renderGraph.CreateTexture(colorDesc);
                }
                else
                {
                    destination = resourceData.activeColorTexture;
                }

                var depthTexture = settings.needDepth ? resourceData.cameraDepth : TextureHandle.nullHandle;
                var singleBlit = !blit && !settings.blitToCameraTarget;
                switch (settings.objectType)
                {
                    case ObjectType.Opaque:
                        AddPass(renderGraph, frameData, true, destination, depthTexture, singleBlit);
                        break;
                    case ObjectType.Transparent:
                        AddPass(renderGraph, frameData, false, destination, depthTexture, singleBlit);
                        break;
                    case ObjectType.All:
                        AddPass(renderGraph, frameData, true, destination, depthTexture, false);
                        AddPass(renderGraph, frameData, false, destination, depthTexture, singleBlit);
                        break;
                }

                if (blit)
                {
                    var customData = frameData.GetOrCreate<MyCustomData>();
                    customData.textureToTransfer = destination;
                    RecordRenderGraphBlit(renderGraph, frameData);
                }
            }


            private void RecordRenderGraphBlit(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>($"{passName}/blit", out var passData))
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();


                    RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;

                    var targetDesc = renderGraph.GetTextureDesc(resourceData.cameraColor);
                    if (!settings.blitToCameraTarget)
                    {
                        targetDesc.name = settings.textureName;
                    }

                    targetDesc.clearBuffer =
                        settings.clearFlag == ClearFlag.Color || settings.clearFlag == ClearFlag.All;
                    targetDesc.clearColor = settings.clearColor;
                    targetDesc.width = (int)(settings.blitScale * desc.width);
                    targetDesc.height = (int)(settings.blitScale * desc.height);

                    var outTexHandle = settings.blitToCameraTarget
                        ? resourceData.cameraColor
                        : renderGraph.CreateTexture(targetDesc);

                    var customData = frameData.Get<MyCustomData>();

                    passData.material = settings.afterRenderMaterial;
                    passData.passId = settings.afterPassId;

                    builder.UseTexture(customData.textureToTransfer);
                    builder.SetRenderAttachment(outTexHandle, 0);
                    passData.source = customData.textureToTransfer;

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(
                            context.cmd,
                            data.source, // 源：TextureHandle
                            new Vector4(1, 1, 0, 0), // scaleBias
                            data.material,
                            data.passId
                        );
                    });
                    if (!settings.blitToCameraTarget)
                    {
                        builder.SetGlobalTextureAfterPass(outTexHandle, BlitTextureID);
                    }
                }
            }

            private void GetTransparencySortingMode(Camera camera, ref SortingSettings sortingSettings)
            {
                var mode = settings.m_transparencySortMode;

                if (mode == TransparencySortMode.Default)
                {
                    mode = camera.orthographic ? TransparencySortMode.Orthographic : TransparencySortMode.Perspective;
                }

                switch (mode)
                {
                    case TransparencySortMode.Perspective:
                        sortingSettings.distanceMetric = DistanceMetric.Perspective;
                        break;

                    case TransparencySortMode.Orthographic:
                        sortingSettings.distanceMetric = DistanceMetric.Orthographic;
                        break;

                    default:
                        sortingSettings.distanceMetric = DistanceMetric.CustomAxis;
                        sortingSettings.customAxis = settings.m_transparencySortAxis;
                        break;
                }
            }

         
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }
        }

        public enum ObjectType
        {
            Opaque,
            Transparent,
            All
        }

        [Serializable]
        public class Settings
        {
            public TransparencySortMode m_transparencySortMode;
            public Vector3 m_transparencySortAxis = new Vector3(0, 0, 1);

            public bool needDepth;
            public ObjectType objectType;
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
            public LayerMask layerMask = -1;
            public Material overrideMat;
            public string[] ShaderTags;

            public Material afterRenderMaterial;
            public int afterPassId;
            public string textureName;

            public float blitScale = 1;
            public bool blitToCameraTarget;
            public ClearFlag clearFlag;
            public Color clearColor = Color.black;
        }

        public int VolumeLevel = 0;
        public Settings settings = new Settings();
        private BakerRenderGraphPass m_ScriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new BakerRenderGraphPass(settings, name);

            // Configures where the render pass should be injected. 
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (Application.isPlaying && GameVolumeManager.instance.volumeLevel < VolumeLevel)
            {
                return;
            }

            // if (InitCheckCamera(renderingData.cameraData.camera))
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}