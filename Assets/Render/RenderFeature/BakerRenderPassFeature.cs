using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BakerRenderPassFeature : ScriptableRendererFeature
{
    private class BakerRenderPass : ScriptableRenderPass
    { 
        private Settings settings;
        private RTHandle source { get; set; }
        private RTHandle destination;

        private RTHandle dstTextureId;
        private RTHandle temp;

        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        private string m_ProfilerTag = "BakerRenderPassFeature";

        private FilteringSettings filteringSettings;
        RenderStateBlock m_RenderStateBlock;
        public BakerRenderPass(Settings settings,string profilerTag, RenderPassEvent _event)
        {
            this.settings = settings; 
            this.renderPassEvent = _event;
            m_ProfilerTag = profilerTag;

            if (settings.PassNames != null && settings.PassNames.Length > 0)
            {
                foreach (var passName in settings.PassNames)
                    m_ShaderTagIdList.Add(new ShaderTagId(passName));
            }
            else
            {
                m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit")); 
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
                m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            }
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            filteringSettings = new FilteringSettings(RenderQueueRange.transparent, settings.layerMask);

            if (settings.overrideDepthState)
            {
                SetDetphState(settings.enableWrite, settings.depthCompareFunction);
            }
            if (settings.overrideStencilState)
            {
                SetStencilState(settings.stencilStateData.stencilReference, settings.stencilStateData.stencilCompareFunction,
                    settings.stencilStateData.passOperation, settings.stencilStateData.failOperation, settings.stencilStateData.zFailOperation);
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            this.ConfigureTarget(this.destination);
            this.ConfigureClear(settings.clearFlag,settings.clearColor);
        }
        public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
        {
            m_RenderStateBlock.mask |= RenderStateMask.Depth;
            m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
        }
        public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
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
        private void GetTransparencySortingMode(Camera camera, ref SortingSettings sortingSettings)
        {
            var mode =settings.m_transparencySortMode;

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
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTargetHandle;

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0; // Color and depth cannot be combined in RTHandles

            //RenderingUtils.ReAllocateIfNeeded(ref temp, Vector2.one, desc, name: "_TemporaryColorTexture");
            // These resizable RTHandles seem quite glitchy when switching between game and scene view :\
            // instead,
            if (!string.IsNullOrEmpty(settings.textureName))
            {
                RenderingUtils.ReAllocateIfNeeded(ref dstTextureId, Vector2.one, desc, name: settings.textureName);
            }

          
            RenderingUtils.ReAllocateIfNeeded(ref temp, Vector2.one, desc, name: "temp");
            if (settings.blitToCameraTarget|| string.IsNullOrEmpty(settings.textureName))
            {
                destination = renderer.cameraColorTargetHandle;
            }
            else
            {
                RenderingUtils.ReAllocateIfNeeded(ref destination, Vector2.one, desc, name: settings.textureName);
            }
            
            //Nothing here yet.
        }
        
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {  
            if (Camera.main == null)
            {
                return;
            }
      

            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;

            var sortFlags = SortingCriteria.CommonTransparent;
            var drawSettings = this.CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);

            var sortSettings = drawSettings.sortingSettings;
            GetTransparencySortingMode(camera, ref sortSettings);
            drawSettings.sortingSettings = sortSettings;


            drawSettings.overrideMaterial = settings.overrideMat;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref this.filteringSettings,ref m_RenderStateBlock);

            if (settings.afterRenderMaterial)
            {
                Blitter.BlitCameraTexture(cmd, source, temp, settings.afterRenderMaterial,settings.afterPassId);
                if (!string.IsNullOrEmpty(settings.textureName))
                {

                    Blitter.BlitCameraTexture(cmd, destination, dstTextureId, Vector2.one);
                    cmd.SetGlobalTexture(settings.textureName, dstTextureId);
                } 
            }
            else
            {
                if (!string.IsNullOrEmpty(settings.textureName))
                {

                    Blitter.BlitCameraTexture(cmd, destination, dstTextureId, Vector2.one);
                    cmd.SetGlobalTexture(settings.textureName, dstTextureId);
                } 
            }
             


            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);



        }
        public void Dispose()
        {
            destination?.Release();
            dstTextureId?.Release();
            temp?.Release();
        }
       
        
    }

    private BakerRenderPass m_Pass;  

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

        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public LayerMask layerMask = -1;
        public Material overrideMat;
        public string[] PassNames;

        public Material afterRenderMaterial;
        public int afterPassId;
        public string textureName;

        public bool blitToCameraTarget;
        public ClearFlag clearFlag;
        public Color clearColor = Color.black;
    }

    public Settings settings = new Settings();

    /// <inheritdoc/>
    public override void Create()
    {
        m_Pass = new BakerRenderPass(settings,name, settings.renderPassEvent);
          
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    { 
    }
    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_Pass);

    }
    protected override void Dispose(bool disposing)
    {
        m_Pass.Dispose();
    }
}