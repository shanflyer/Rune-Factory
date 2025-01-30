using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

public class SceneRenderPassFeature : ScriptableRendererFeature
{
    public int VolumeLevel;
    public RenderPassEvent renderPassEvent;
    public Material blurMaterial, blendBlurMaterial, cycleMaterial;  
    class SceneRenderPass : ScriptableRenderPass
    {
        private string tagName="Test";
        private Material blurMaterial, blendBlurMaterial, cycleMaterial;
        public int VolumeLevel = 0;
        public SceneRenderPass( Material blurMaterial,Material blendBlurMaterial,Material cycleMaterial, string tagName, RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
            this.blurMaterial = blurMaterial;
            this.blendBlurMaterial = blendBlurMaterial;
            this.cycleMaterial = cycleMaterial;
            this.tagName = tagName;
        }
        private class BlendPassData
        {
            internal TextureHandle blurSource;
            internal TextureHandle source;
            internal Material material;
        }
        private class PassData
        {
            internal TextureHandle source;
            internal Material material; 
        } 
        static void ExecutePass(PassData data, RasterGraphContext context,bool blite=false)
        {
            data.material.mainTexture = data.source;
            context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);

        }
        static void ExecuteBlendPass(BlendPassData data, RasterGraphContext context, bool blite = false)
        {
            data.material.mainTexture = data.source;
            data.material.SetTexture("_BlurTex", data.blurSource);
            context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);

        }
        private int BlitTextureID = Shader.PropertyToID("_BlurTex");
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
           
            if (blurMaterial==null||blendBlurMaterial==null||cycleMaterial==null)
            {
                return;
            }
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

          

            var targetDesc = renderGraph.GetTextureDesc(resourceData.cameraColor);
            targetDesc.name = "_BlurTex";
            
            TextureHandle BlurTexHandle = renderGraph.CreateTexture(targetDesc);
            targetDesc.clearBuffer = false;
           
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Blur", out var passData))
            {
                passData.source = resourceData.activeColorTexture;
                passData.material = blurMaterial;
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                builder.SetRenderAttachment(BlurTexHandle, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context,true));
                //builder.SetGlobalTextureAfterPass(BlurTexHandle, BlitTextureID);
            }

            var targetDesc1 = renderGraph.GetTextureDesc(resourceData.cameraColor);
            targetDesc1.name = "_BlendTex";
            targetDesc.clearBuffer = false;
            TextureHandle BlendTexHandle = renderGraph.CreateTexture(targetDesc1);
            using (var builder = renderGraph.AddRasterRenderPass<BlendPassData>("Blend", out var passData))
            {
                passData.source= resourceData.activeColorTexture;
                passData.blurSource = BlurTexHandle;
                passData.material = blendBlurMaterial;
                builder.UseTexture(BlurTexHandle, AccessFlags.Read);
                builder.SetRenderAttachment(BlendTexHandle, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((BlendPassData data, RasterGraphContext context) => ExecuteBlendPass(data, context,true));

            }

             
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Cycle", out var passData))
            {
                passData.source = BlendTexHandle;
                passData.material = cycleMaterial;
                builder.UseTexture(BlendTexHandle, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));

            } 
        }
         
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    SceneRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new SceneRenderPass(blurMaterial,blendBlurMaterial,cycleMaterial,name,renderPassEvent); 
    }
     
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (Application.isPlaying && GameVolumeManager.instance.volumeLevel < VolumeLevel)
        {
            return;
        }
        //if (InitCheckCamera(renderingData.cameraData.camera))
            renderer.EnqueuePass(m_ScriptablePass);
    }
}
