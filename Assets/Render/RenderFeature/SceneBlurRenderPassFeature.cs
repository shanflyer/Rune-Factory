using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic; 

public class SceneBlurRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public TransparencySortMode m_transparencySortMode;
        public Vector3 m_transparencySortAxis = new Vector3(0, 0, 1);
        public bool opaque = false;
        public LayerMask layerMask = -1;
        public ClearFlag clearFlag;
        public Color clearColor = Color.black;
        public Material overrideMat;
        public string[] ShaderTags;

        public Material blurMaterial;
        public Material blendBlurMaterial; 
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
         
    }
    class SceneBlurRenderPass : ScriptableRenderPass
    {
        Settings settings;
        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        public SceneBlurRenderPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;

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
        }
        internal class PassData
        {
            internal RendererListHandle rendererList;
            internal ClearFlag clearFlag;
            internal Color clearColor; 
            internal Material blendBlurMaterial; 
        } 
        internal class BlurPassData
        {
            internal Material blurMaterial;
            internal TextureHandle depthTextureHandle;
            internal TextureHandle textureHandle;
        }
        internal class BlendPassData
        {
            internal Material blendMaterial;
            internal TextureHandle textureHandle;
            internal TextureHandle depthTextureHandle;
        }
        static void ExecuteDepthPass(PassData data, RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(data.clearFlag == ClearFlag.Depth || data.clearFlag == ClearFlag.All, data.clearFlag == ClearFlag.Color || data.clearFlag == ClearFlag.All, data.clearColor);
            context.cmd.DrawRendererList(data.rendererList);
        }
        static void ExecuteBlurPass(BlurPassData data, RasterGraphContext context)
        {
            data.blurMaterial.mainTexture = data.textureHandle;
            data.blurMaterial.SetTexture("_MyDepthTex", data.depthTextureHandle);
            context.cmd.DrawProcedural(Matrix4x4.identity, data.blurMaterial, 0, MeshTopology.Triangles, 3);
        }
        static void ExecuteBlendPass(BlendPassData data, RasterGraphContext context)
        {
            data.blendMaterial.mainTexture = data.textureHandle;
            data.blendMaterial.SetTexture("_MyDepthTex", data.depthTextureHandle); 
            context.cmd.DrawProcedural(Matrix4x4.identity, data.blendMaterial, 0, MeshTopology.Triangles, 3);
        }
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "SceneBlurRenderPass";

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

         

            SortingCriteria sortingCriteria = settings.opaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, renderingData, cameraData, lightData, sortingCriteria);
            drawSettings.overrideMaterial = settings.overrideMat;
            var sortSettings = drawSettings.sortingSettings;
            GetTransparencySortingMode(cameraData.camera, ref sortSettings);
            drawSettings.sortingSettings = sortSettings;
            var filteringSettings = new FilteringSettings(settings.opaque ? RenderQueueRange.opaque : RenderQueueRange.transparent, settings.layerMask);

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor; 

            desc.colorFormat = RenderTextureFormat.Default;
            desc.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
            TextureHandle depthTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, $"{passName}_destination",
                settings.clearFlag == ClearFlag.Color || settings.clearFlag == ClearFlag.All);

            TextureHandle blurTexHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "blurTex", false);
            using (var builder = renderGraph.AddRasterRenderPass<PassData>($"{passName}/MyDepth", out var passData))
            {
                passData.clearFlag = settings.clearFlag;
                passData.clearColor = settings.clearColor;

                RenderStateBlock m_RenderStateBlock =new RenderStateBlock(RenderStateMask.Nothing); 
                RenderingUtils.CreateRendererListWithRenderStateBlock(renderGraph, ref renderingData.cullResults, drawSettings, filteringSettings, m_RenderStateBlock, ref passData.rendererList);

                builder.UseRendererList(passData.rendererList);

                builder.SetRenderAttachment(depthTextureHandle, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteDepthPass(data, context));
            }
           
            using(var builder = renderGraph.AddRasterRenderPass<BlurPassData>($"{passName}/Blur", out var passData))
            {
                passData.blurMaterial = settings.blurMaterial;
                passData.depthTextureHandle = depthTextureHandle;
                passData.textureHandle = resourceData.activeColorTexture;
               // builder.UseTexture(depthTextureHandle, 0);
                builder.SetRenderAttachment(blurTexHandle, 0);
                builder.SetRenderFunc((BlurPassData data, RasterGraphContext context) => ExecuteBlurPass(data, context));
                int BlitTextureID = Shader.PropertyToID("blit");
                builder.SetGlobalTextureAfterPass(blurTexHandle, BlitTextureID);
            } 
            /*
           using(var builder=renderGraph.AddRasterRenderPass<BlendPassData>($"{passName}/Blend", out var passData))
           {
               passData.blendMaterial = settings.blendBlurMaterial;
               passData.textureHandle = blurTexHandle;
               passData.depthTextureHandle = depthTextureHandle;
               builder.UseTexture(blurTexHandle, 0);
               builder.SetRenderAttachment(resourceData.activeColorTexture,0);
               builder.SetRenderFunc((BlendPassData data, RasterGraphContext context) => ExecuteBlendPass(data, context));
           } */
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

   
    SceneBlurRenderPass m_ScriptablePass;
    [SerializeField]
    public Settings settings = new Settings();
    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new SceneBlurRenderPass(settings);
          
    }
     
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
