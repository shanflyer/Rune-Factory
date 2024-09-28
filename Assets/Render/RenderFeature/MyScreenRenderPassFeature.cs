using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

public class MyScreenRenderPassFeature : ScriptableRendererFeature
{
    public RenderPassEvent renderPassEvent;
    public Material material;
    public string blitName;
    public float scale = 1;
    public bool blitTexture = false;
    class MyScreenRenderPass : ScriptableRenderPass
    {
        private string tagName="Test";
        private Material material;
        private bool blitTexture = false;
        private string blitName;
        public float scale = 1;
        public MyScreenRenderPass(Material material, string blitName, string tagName,float scale,bool blitTexture, RenderPassEvent renderPassEvent)
        {
            this.renderPassEvent = renderPassEvent;
            this.blitName = blitName;
            this.material = material;
            this.tagName = tagName;
            this.scale = scale;
            this.blitTexture = blitTexture;
        }
        private class PassData
        {
            internal TextureHandle source;
            internal Material material;
            internal TextureHandle outTexHandle;
        }
         
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            if(data.material == null)
            {
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0.0f, false);
            }
            else
            {
                data.material.mainTexture = data.source;
                context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3); 
            } 
        }
         
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
           
            if (material == null && !blitTexture)
            {
                return;
            }
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
         

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.Default;
            desc.depthStencilFormat = GraphicsFormat.R8_SRGB;
            desc.width = (int)(desc.width * scale);
            desc.height= (int)(desc.height * scale);
            TextureHandle outTexHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, blitTexture ? blitName : "TempScreenTex", false);
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(blitTexture ?tagName:$"{tagName}/Copy", out var passData))
            {
                passData.source = resourceData.activeColorTexture;
                if (blitTexture)
                {
                    passData.material = material;
                }
                else
                {
                    passData.material = null;
                }
                   

                builder.SetRenderAttachment(outTexHandle, 0);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));

                int BlitTextureID = Shader.PropertyToID(blitName);
                if (blitTexture)
                    builder.SetGlobalTextureAfterPass(outTexHandle, BlitTextureID);
            }
            
            if (!blitTexture)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>($"{tagName}/blit", out var passData))
                {
                    passData.source = outTexHandle;
                    passData.material = material;
                    builder.UseTexture(outTexHandle);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context)); 
                }
            }
        }
         
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    MyScreenRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new MyScreenRenderPass(material,blitName,name,scale,blitTexture,renderPassEvent); 
    }
     
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
