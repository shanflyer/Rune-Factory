using System;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class MyRenderGraphPassFeature : ScriptableRendererFeature
    {
        private class MyRenderGraphPass : ScriptableRenderPass
        {
            private readonly Settings settings;
            private readonly string passName = "MyRenderGraphPass";
            private readonly List<ShaderTagId> m_ShaderTagIdList = new();
            private RenderStateBlock m_RenderStateBlock;

            public MyRenderGraphPass(Settings settings, string passName)
            {
                this.settings = settings;
                this.passName = passName;
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
                if (settings.overrideDepthState) SetDepthState(settings.enableWrite, settings.depthCompareFunction);
                if (settings.overrideStencilState)
                    SetStencilState(settings.stencilStateData.stencilReference,
                        settings.stencilStateData.stencilCompareFunction,
                        settings.stencilStateData.passOperation, settings.stencilStateData.failOperation,
                        settings.stencilStateData.zFailOperation);
                renderPassEvent = settings.renderPassEvent;
            }

            internal class PassData
            {
                internal RendererListHandle rendererList;
                internal ClearFlag clearFlag;
                internal Color clearColor;
                internal Material material;
                internal int passId;
                internal TextureHandle outTexHandle;
            }


            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
                using (var builder =
                       renderGraph.AddRasterRenderPass<PassData>($"{passName}/destination", out var passData))
                {
                    var resourceData = frameData.Get<UniversalResourceData>();
                    var renderingData = frameData.Get<UniversalRenderingData>();
                    var lightData = frameData.Get<UniversalLightData>();
                    var cameraData = frameData.Get<UniversalCameraData>();

                    passData.clearFlag = settings.clearFlag;
                    passData.clearColor = settings.clearColor;


                    var sortingCriteria = settings.opaque
                        ? cameraData.defaultOpaqueSortFlags
                        : SortingCriteria.CommonTransparent;
                    var drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData,
                        cameraData, lightData, sortingCriteria);
                    drawSettings.overrideMaterial = settings.overrideMat;
                    var sortSettings = drawSettings.sortingSettings;
                    GetTransparencySortingMode(cameraData.camera, ref sortSettings);
                    drawSettings.sortingSettings = sortSettings;
                    var filteringSettings = new FilteringSettings(
                        settings.opaque ? RenderQueueRange.opaque : RenderQueueRange.transparent, settings.layerMask);

                    RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults,
                        drawSettings, filteringSettings, m_RenderStateBlock, ref passData.rendererList);

                    builder.UseRendererList(passData.rendererList);
                    //  builder.UseTexture(resourceData.cameraColor);
                    builder.SetRenderAttachmentDepth(resourceData.cameraDepth, AccessFlags.ReadWrite);

                    if (settings.outCameraTarget) builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    var desc = cameraData.cameraTargetDescriptor;
                    var targetDesc = renderGraph.GetTextureDesc(resourceData.cameraColor);
                    if (settings.outRenderDatas != null && settings.outRenderDatas.Count > 0)
                    {
                        var outBlitNames = new HashSet<string>();
                        var outBlitIndexes = new HashSet<int>();
                        for (var i = 0; i < settings.outRenderDatas.Count; i++)
                        {
                            var outRenderData = settings.outRenderDatas[i];

                            if (string.IsNullOrEmpty(outRenderData.outTextureName) ||
                                outBlitNames.Contains(outRenderData.outTextureName) ||
                                outBlitIndexes.Contains(outRenderData.outIndex)) continue;
                            outBlitNames.Add(outRenderData.outTextureName);
                            outBlitIndexes.Add(outRenderData.outIndex);
                            targetDesc.name = outRenderData.outTextureName;

                            targetDesc.clearBuffer = settings.clearFlag == ClearFlag.Color ||
                                                     settings.clearFlag == ClearFlag.All;
                            targetDesc.clearColor = settings.clearColor;
                            targetDesc.width = (int)(settings.blitScale * desc.width);
                            targetDesc.height = (int)(settings.blitScale * desc.height);
                            var outTexHandle = renderGraph.CreateTexture(targetDesc);
                            builder.SetRenderAttachment(outTexHandle, outRenderData.outIndex);
                            var BlitTextureID = Shader.PropertyToID(outRenderData.outTextureName);
                            builder.SetGlobalTextureAfterPass(outTexHandle, BlitTextureID);
                        }
                    }

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                        {
                            context.cmd.ClearRenderTarget(
                                data.clearFlag == ClearFlag.Depth || data.clearFlag == ClearFlag.All,
                                data.clearFlag == ClearFlag.Color || data.clearFlag == ClearFlag.All, data.clearColor);
                            context.cmd.DrawRendererList(data.rendererList);
                        }
                    );
                }
            }


            private void GetTransparencySortingMode(Camera camera, ref SortingSettings sortingSettings)
            {
                var mode = settings.m_transparencySortMode;

                if (mode == TransparencySortMode.Default)
                    mode = camera.orthographic ? TransparencySortMode.Orthographic : TransparencySortMode.Perspective;

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

            public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
            {
                m_RenderStateBlock.mask |= RenderStateMask.Depth;
                m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
            }

            private void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp,
                StencilOp failOp, StencilOp zFailOp)
            {
                var stencilState = StencilState.defaultValue;
                stencilState.enabled = true;
                stencilState.SetCompareFunction(compareFunction);
                stencilState.SetPassOperation(passOp);
                stencilState.SetFailOperation(failOp);
                stencilState.SetZFailOperation(zFailOp);

                m_RenderStateBlock.mask |= RenderStateMask.Stencil;
                m_RenderStateBlock.stencilReference = reference;
                m_RenderStateBlock.stencilState = stencilState;
            }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }
        }

        [Serializable]
        public struct OutRenderData
        {
            public string outTextureName;
            public int outIndex;
        }

        [Serializable]
        public class Settings
        {
            public TransparencySortMode m_transparencySortMode;
            public Vector3 m_transparencySortAxis = new(0, 0, 1);

            public bool overrideDepthState;
            public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
            public bool enableWrite = true;

            public bool overrideStencilState;
            public StencilStateData stencilStateData = new();

            public bool opaque;
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
            public LayerMask layerMask = -1;
            public Material overrideMat;
            public string[] ShaderTags;
            public float blitScale = 1;

            public bool outCameraTarget;
            public List<OutRenderData> outRenderDatas;
            public ClearFlag clearFlag;
            public Color clearColor = Color.black;
        }

        public int VolumeLevel;
        public Settings settings = new();
        private MyRenderGraphPass m_ScriptablePass;

        /// <inheritdoc />
        public override void Create()
        {
            m_ScriptablePass = new MyRenderGraphPass(settings, name);

            // Configures where the render pass should be injected. 
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (Application.isPlaying && GameVolumeManager.instance.volumeLevel < VolumeLevel) return;
            // if (InitCheckCamera(renderingData.cameraData.camera))
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}