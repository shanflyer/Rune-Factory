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
                if (settings.overrideDepthState)
                {
                    SetDepthState(settings.enableWrite, settings.depthCompareFunction);
                }
                if (settings.overrideStencilState)
                {
                    SetStencilState(settings.stencilStateData.stencilReference, settings.stencilStateData.stencilCompareFunction,
                        settings.stencilStateData.passOperation, settings.stencilStateData.failOperation, settings.stencilStateData.zFailOperation);
                }
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

            private static void ExecutePass(PassData data, RasterGraphContext context)
            {
                context.cmd.ClearRenderTarget(data.clearFlag == ClearFlag.Depth || data.clearFlag == ClearFlag.All, data.clearFlag == ClearFlag.Color || data.clearFlag == ClearFlag.All, data.clearColor);
                context.cmd.DrawRendererList(data.rendererList); 
            }
            private static void OutExecutePass(PassData data, RasterGraphContext context)
            {
                data.material.mainTexture = data.outTexHandle;  
                context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);
            }
            private int BlitTextureID = Shader.PropertyToID("DestTexture");

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
                using (var builder = renderGraph.AddRasterRenderPass<PassData>($"{passName}/destination", out var passData))
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                    UniversalLightData lightData = frameData.Get<UniversalLightData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                    passData.clearFlag = settings.clearFlag;
                    passData.clearColor = settings.clearColor;
                  

                  

                    SortingCriteria sortingCriteria = settings.opaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
                    DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData, cameraData, lightData, sortingCriteria);
                    drawSettings.overrideMaterial = settings.overrideMat;
                    var sortSettings = drawSettings.sortingSettings;
                    GetTransparencySortingMode(cameraData.camera, ref sortSettings);
                    drawSettings.sortingSettings = sortSettings;
                    var filteringSettings = new FilteringSettings(settings.opaque ? RenderQueueRange.opaque : RenderQueueRange.transparent, settings.layerMask);

                    RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawSettings, filteringSettings, m_RenderStateBlock, ref passData.rendererList);
                     
                    builder.UseRendererList(passData.rendererList);

                    RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                    desc.width = (int)(settings.blitScale * desc.width);
                    desc.height = (int)(settings.blitScale * desc.height);

                    desc.colorFormat = RenderTextureFormat.Default;
                    desc.depthStencilFormat = Experimental.Rendering.GraphicsFormat.R8_SRGB;
                    TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc,settings.afterRenderMaterial!=null? $"{passName}_destination":settings.textureName,
                        settings.clearFlag == ClearFlag.Color || settings.clearFlag == ClearFlag.All);
                   
                    builder.SetRenderAttachment(destination, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                    if (settings.afterRenderMaterial == null)
                    {
                        builder.SetGlobalTextureAfterPass(destination, BlitTextureID);
                    }
                    else
                    {
                        var customData = frameData.GetOrCreate<MyCustomData>();
                        customData.textureToTransfer = destination;
                    }
                   
                }

                if (settings.afterRenderMaterial != null)
                {
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
                    desc.width = (int)(settings.blitScale * desc.width);
                    desc.height = (int)(settings.blitScale * desc.height);
                    desc.colorFormat = RenderTextureFormat.Default;
                    desc.depthStencilFormat = Experimental.Rendering.GraphicsFormat.R8_SRGB; 
                    TextureHandle outTexHandle =settings.blitToCameraTarget?resourceData.activeColorTexture: UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, settings.textureName,
                    false);
                    var customData = frameData.Get<MyCustomData>();

                    passData.material = settings.afterRenderMaterial;
                    passData.passId = settings.afterPassId;

                    builder.UseTexture(customData.textureToTransfer);
                    builder.SetRenderAttachment(outTexHandle, 0);
                    passData.outTexHandle = customData.textureToTransfer;

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => OutExecutePass(data, context));
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

            public void SetDepthState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
            {
                m_RenderStateBlock.mask |= RenderStateMask.Depth;
                m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
            }

            private void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
            {
                StencilState stencilState = StencilState.defaultValue;
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

        [System.Serializable]
        public class Settings
        {
            public TransparencySortMode m_transparencySortMode;
            public Vector3 m_transparencySortAxis = new Vector3(0, 0, 1);

            public bool overrideDepthState = false;
            public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
            public bool enableWrite = true;

            public bool overrideStencilState;
            public StencilStateData stencilStateData = new StencilStateData();

            public bool opaque = false;
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
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}