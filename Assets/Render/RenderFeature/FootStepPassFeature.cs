using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.Mathematics;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class FootStepPassFeature : ScriptableRendererFeature
    {
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

            public ClearFlag clearFlag;
            public Color clearColor = Color.black;

            public ComputeShader computeShader;
        }

        private class FootStepPass : ScriptableRenderPass
        {
            // This class stores the data needed by the RenderGraph pass.
            // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
            internal class PassData
            {
                internal RendererListHandle rendererList;
                internal ClearFlag clearFlag;
                internal Color clearColor;
                internal TextureHandle textureHandle;
                internal ComputeShader computeShader;
                internal List<CharacterFootStep> characterFootSteps;
            }
            [SerializeField]
            private Settings settings;
            private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
            private RenderStateBlock m_RenderStateBlock;

            public FootStepPass(Settings settings)
            {
                this.settings = settings;

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

            // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
            // It is used to execute draw commands.
            private static void ExecutePass(PassData data, RasterGraphContext context)
            {
                context.cmd.ClearRenderTarget(data.clearFlag == ClearFlag.Depth || data.clearFlag == ClearFlag.All, data.clearFlag == ClearFlag.Color || data.clearFlag == ClearFlag.All, data.clearColor);
                context.cmd.DrawRendererList(data.rendererList);
 
               
                var kernelID = data.computeShader.FindKernel("CSMain");
                data.computeShader.SetTexture(kernelID, "Result", data.textureHandle);
                Vector4[] pos = new Vector4[64];
                for (int i = 0; i < data.characterFootSteps.Count; i++)
                {
                    if (i < 64)
                    {
                        Vector3 screenPos = CameraManager.WorldPointToScreenPoint(data.characterFootSteps[i].transform.position); 
                        pos[i] = new Vector4((int)screenPos.x,(int)screenPos.y,0,0);
                    }
                }
                data.computeShader.SetVectorArray("pos", pos);
                data.computeShader.SetInt("trueCount", data.characterFootSteps.Count);

                var appendBuffer = new ComputeBuffer(64, sizeof(int), ComputeBufferType.Append);
                appendBuffer.SetCounterValue(0);
                data.computeShader.SetBuffer(kernelID, "outFootTexIndex", appendBuffer);
                data.computeShader.Dispatch(kernelID, Screen.width / 8, Screen.height / 8, 1);

                var countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
                ComputeBuffer.CopyCount(appendBuffer, countBuffer, 0);

                int[] counter = new int[1] { 0 };
                countBuffer.GetData(counter);
                int count = counter[0];

               // Debug.Log("FootStepCount: " + count);

                var outData = new int[count];
                appendBuffer.GetData(outData);
                for (int i = 0; i < outData.Length; i++)
                {
                    if (i < data.characterFootSteps.Count)
                    {
                        data.characterFootSteps[i].FootStepAction(outData[i]);
                    }
                    //Debug.Log(data[i] * 256);
                }

                appendBuffer.Release();
                appendBuffer.Dispose();

                countBuffer.Release();
                countBuffer.Dispose();
            }
            private int BlitTextureID = Shader.PropertyToID("FootStepTex");
            List<CharacterFootStep> characterFootSteps = new List<CharacterFootStep>();
            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "FootStepPass";
               
                if (Application.isPlaying)
                {
                     characterFootSteps = EnvironmentManger.instance.GetCharacterFootSteps();
                    if (characterFootSteps == null || characterFootSteps.Count == 0)
                    {
                         return;
                    }
                }
                else
                {
                    return;
                }
               
               // Debug.Log("FootStepGraph: " + characterFootSteps.Count);
                // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                    UniversalLightData lightData = frameData.Get<UniversalLightData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                    passData.clearFlag = settings.clearFlag;
                    passData.clearColor = settings.clearColor;
                    passData.characterFootSteps = characterFootSteps;
                    passData.computeShader = settings.computeShader;

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
                    desc.colorFormat = RenderTextureFormat.Default;
                    desc.depthStencilFormat = Experimental.Rendering.GraphicsFormat.None;
                    TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "FootStepTex",
                        settings.clearFlag == ClearFlag.Color || settings.clearFlag == ClearFlag.All);
                    passData.textureHandle= destination;

                    builder.SetRenderAttachment(destination, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                    builder.SetGlobalTextureAfterPass(destination, BlitTextureID);
                }
            }

            // NOTE: This method is part of the compatibility rendering path, please use the Render Graph API above instead.
            // Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }
        }

        private FootStepPass m_ScriptablePass;

        [SerializeField]
        private Settings settings;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new FootStepPass(settings);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (InitCheckCamera(renderingData.cameraData.camera))
                renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}