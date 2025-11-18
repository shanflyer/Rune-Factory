using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class MyScreenRenderPassFeature : ScriptableRendererFeature
{
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    public Material material;

    class MyScreenRenderPass : ScriptableRenderPass
    {
        private readonly string tagName = "MyScreenPass";
        private readonly Material material;

        public MyScreenRenderPass(Material mat, RenderPassEvent evt, string tagName)
        {
            material = mat;
            renderPassEvent = evt;
            this.tagName = tagName;
        }

        private class PassData
        {
            public TextureHandle source;
            public Material material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
                return;

            var resources = frameData.Get<UniversalResourceData>();

            // 1. 创建一个和 cameraColor 一样的临时 RT
            var desc = renderGraph.GetTextureDesc(resources.cameraColor);
            desc.name = $"{tagName}_Temp";
            desc.clearBuffer = false;
            var temp = renderGraph.CreateTexture(desc);

            // ---------- Pass1：cameraColor -> temp（纯复制，不加材质） ----------
            using (var builder = renderGraph.AddRasterRenderPass<PassData>($"{tagName}/Copy", out var passData))
            {
                passData.source = resources.activeColorTexture;
                passData.material = null;

                builder.UseTexture(passData.source); // 读 camera
                builder.SetRenderAttachment(temp, 0); // 写 temp
                builder.AllowPassCulling(false);

                builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1, 1, 0, 0),
                        0.0f,
                        false);
                });
            }

            // ---------- Pass2：temp -> activeColor（用你的材质） ----------
            using (var builder = renderGraph.AddRasterRenderPass<PassData>($"{tagName}/Effect", out var passData))
            {
                passData.source = temp;
                passData.material = material;

                builder.UseTexture(passData.source); // 读 temp
                builder.SetRenderAttachment(resources.activeColorTexture, 0); // 写回 cameraColor
                builder.AllowPassCulling(false);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.source,
                        new Vector4(1, 1, 0, 0),
                        data.material,
                        0);
                });
            }
        }
    }

    private MyScreenRenderPass m_Pass;

    public override void Create()
    {
        m_Pass = new MyScreenRenderPass(material, renderPassEvent, name);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 先别加任何 volume 条件，确保能跑起来
        renderer.EnqueuePass(m_Pass);
    }
}
