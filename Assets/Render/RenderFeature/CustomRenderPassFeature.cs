using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        Setting setting;
        RTHandle temp;
        RTHandle target;
        public CustomRenderPass(Setting setting)
        {
            this.setting = setting;
        }

        private string m_ProfilerTag = "CustomRenderPass";
        private static readonly int m_BlitTextureShaderID = Shader.PropertyToID("_MainTex");
        // This method is called before executing the render pass.

        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.

        // When empty this render pass will render to the active camera render target.

        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.

        // The render pipeline will ensure target setup and clearing happens in a performant manner.

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var renderer = renderingData.cameraData.renderer;
           

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref target, Vector2.one, desc, name: setting.bakerName);
            RenderingUtils.ReAllocateIfNeeded(ref temp, Vector2.one, desc, name: "temp");
            //ConfigureTarget(selection);
           // ConfigureClear(ClearFlag.All, new Color(0, 0, 0, 0));

        }

        private RTHandle cameraColor;
        public void SetTarget(RTHandle cameraColor)
        {
            this.cameraColor = cameraColor;
        }
        public void Dispose()
        {
            target?.Release(); 
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (Camera.main == null)
            {
                return;
            }
            if (setting.material == null)
            {
                return;
            }
            ref var cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            using (new ProfilingScope(profilingSampler))
            {
                //context.ExecuteCommandBuffer(cmd);

                var source = cameraData.renderer.cameraColorTargetHandle;

                Blitter.BlitCameraTexture(cmd, source, temp);

                Blitter.BlitCameraTexture(cmd, temp, source, setting.material, 0);
                //setting.material.SetTexture(m_BlitTextureShaderID, temp);
                //Blitter.BlitCameraTexture(cmd, temp, cameraColor);

                // 执行
                context.ExecuteCommandBuffer(cmd);
               cmd.Clear();
            }
           
            CommandBufferPool.Release(cmd);
        }
       

       
    }
    [System.Serializable]
    public class Setting
    {
        public Material material;
        public RenderPassEvent RenderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public string bakerName;
    }

    public Setting setting=new Setting();
    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(setting);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = setting.RenderPassEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        // 把当前相机的 RenderTarget 传给创建的 ScriptableRenderPass
        m_ScriptablePass.SetTarget(renderer.cameraColorTargetHandle);
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        m_ScriptablePass.Dispose();
    }
}


