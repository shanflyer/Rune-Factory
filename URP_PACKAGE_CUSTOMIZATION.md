# URP 包自定义修改说明

本文档记录项目内嵌包 `Packages/com.unity.render-pipelines.universal` 相对 Unity 官方同版本包的修改，用于后续 Unity/URP 升级、替换官方包或拆分自定义功能时参考。

## 对比基线

- 项目包：`Packages/com.unity.render-pipelines.universal`
- 项目包版本：`17.3.0`
- 官方参考包：`D:\Init3.2\Library\PackageCache\com.unity.render-pipelines.universal@66e99ffa09c7`
- 官方参考包版本：`17.3.0`
- 项目包状态：`packages-lock.json` 中为 `source: embedded`
- 对比方式：`git diff --no-index` 对比项目 embedded 包与本机 Unity 官方 PackageCache 同版本包

总体差异：

- 修改文件数：16
- 代码/资源差异：381 insertions, 156 deletions
- 改动范围集中在 RendererData、透明排序、per-renderer render scale、Pixel Perfect、Bloom、DepthOfField、FinalBlit、RenderGraph 工具暴露。

## 文件清单

以下文件相对官方 `17.3.0` 有改动：

- `Editor/Overrides/DepthOfFieldEditor.cs`
- `Editor/UniversalRendererDataEditor.cs`
- `Runtime/2D/PixelPerfectCamera.cs`
- `Runtime/Materials/Lit.mat`
- `Runtime/Overrides/Bloom.cs`
- `Runtime/Overrides/DepthOfField.cs`
- `Runtime/Passes/DrawObjectsPass.cs`
- `Runtime/Passes/FinalBlitPass.cs`
- `Runtime/Passes/PostProcessPassRenderGraph.cs`
- `Runtime/RenderingUtils.cs`
- `Runtime/UniversalRenderPipeline.cs`
- `Runtime/UniversalRenderer.cs`
- `Runtime/UniversalRendererData.cs`
- `Shaders/PostProcessing/Bloom.shader`
- `Shaders/PostProcessing/BokehDepthOfField.shader`
- `Shaders/PostProcessing/UberPost.shader`

## 功能分类

### 1. RendererData 自定义透明排序

涉及文件：

- `Runtime/UniversalRendererData.cs`
- `Editor/UniversalRendererDataEditor.cs`
- `Runtime/Passes/DrawObjectsPass.cs`
- `Runtime/UniversalRenderer.cs`

新增字段：

```csharp
[SerializeField] TransparencySortMode m_transparencySortMode;
[SerializeField] Vector3 m_transparencySortAxis = new Vector3(0, 0, 1);
```

新增公开属性：

```csharp
public TransparencySortMode transparencySortMode { get; set; }
public Vector3 transparencySortAxis { get; set; }
```

编辑器面板新增：

```csharp
SortMode:
SortAxis:
```

运行时接入：

- `UniversalRenderer` 创建 `DrawObjectsPass` 时，把 `data.transparencySortAxis` 和 `data.transparencySortMode` 传入。
- `DrawObjectsPass` 在透明物体渲染时修改 `DrawingSettings.sortingSettings`。
- 当模式为 `TransparencySortMode.Default` 时，按相机类型回退到正交/透视排序。
- 当模式不是 Perspective/Orthographic 时，使用 `DistanceMetric.CustomAxis` 和自定义轴排序。

核心逻辑：

```csharp
if (!m_IsOpaque)
{
    var sortSettings = drawSettings.sortingSettings;
    GetTransparencySortingMode(camera, ref sortSettings);
    drawSettings.sortingSettings = sortSettings;
}
```

项目依赖：

- `Assets/Render/Setting/ForwardAsset_Renderer.asset` 使用 `m_transparencySortMode: 3`，`m_transparencySortAxis: {x: 0, y: 1, z: 1}`。
- `Assets/Render/Setting/EditorAsset_Renderer.asset` 多处使用 `m_transparencySortMode: 3`，`m_transparencySortAxis: {x: 0, y: 1, z: 1}`。
- `Assets/Render/Setting/UIAsset_Renderer.asset` 保留默认排序字段。

作用判断：

- 这是项目 2D/伪 3D 渲染排序的重要功能。
- 目标大概率是让透明物体按 Y/Z 组合轴排序，解决角色、树、地物、特效等前后遮挡问题。
- 官方 URP `UniversalRendererData` 没有这个字段，直接换官方包会导致字段丢失，透明排序行为回退官方逻辑。

### 2. RendererData 级别 render scale/upscaling 覆盖

涉及文件：

- `Runtime/UniversalRendererData.cs`
- `Editor/UniversalRendererDataEditor.cs`
- `Runtime/UniversalRenderPipeline.cs`
- `Runtime/UniversalRenderer.cs`
- `Runtime/2D/PixelPerfectCamera.cs`
- `Runtime/Passes/FinalBlitPass.cs`

新增字段：

```csharp
[SerializeField] bool m_OverrideCameraScaling = false;
[SerializeField] float m_CameraRenderScale = 1.0f;
[SerializeField] UpscalingFilterSelection m_CameraUpscalingFilter = UpscalingFilterSelection.Auto;
```

新增公开属性：

```csharp
public bool overrideCameraScaling { get; set; }
public float cameraRenderScale { get; set; }
public UpscalingFilterSelection cameraUpscalingFilter { get; set; }
```

编辑器面板新增：

```csharp
Override Camera Scaling
Render Scale
Upscaling Filter
```

运行时接入：

- 官方 URP 默认从 `UniversalRenderPipelineAsset` 读取全局 `renderScale` 和 `upscalingFilter`。
- 项目改为先取全局设置，再判断当前相机使用的 `UniversalRenderer` 是否启用 `rendererDataAsset.overrideCameraScaling`。
- 如果启用，则用 renderer data 上的 `cameraRenderScale` 和 `cameraUpscalingFilter` 覆盖全局设置。

核心逻辑：

```csharp
if (GetRenderer(camera, additionalCameraData) is UniversalRenderer renderer &&
    renderer.rendererDataAsset != null &&
    renderer.rendererDataAsset.overrideCameraScaling)
{
    renderScale = renderer.rendererDataAsset.cameraRenderScale;
    upscalingFilter = renderer.rendererDataAsset.cameraUpscalingFilter;
}
```

项目依赖：

- `Assets/Render/Setting/ForwardAsset_Renderer.asset`
  - `m_OverrideCameraScaling: 1`
  - `m_CameraRenderScale: 0.5`
  - `m_CameraUpscalingFilter: 2`
- `Assets/Render/Setting/UIAsset_Renderer.asset`
  - 字段存在，但 `m_OverrideCameraScaling: 0`

作用判断：

- 这是按 Renderer 区分渲染分辨率的功能。
- 主渲染可能用 `0.5` render scale 降 GPU 成本，UI 或其他 Renderer 保持不同策略。
- 官方 URP 只有 PipelineAsset 全局 render scale，不能按 renderer data 覆盖。
- 直接换官方包会导致主 Renderer 的 `0.5` 缩放策略丢失或需要改成全局设置，影响 UI/Camera Stack/Pixel Perfect。

### 3. Pixel Perfect Camera 适配 per-renderer render scale 和 Camera Stack

涉及文件：

- `Runtime/2D/PixelPerfectCamera.cs`
- `Runtime/UniversalRenderer.cs`

新增字段：

```csharp
UniversalAdditionalCameraData m_AdditionalCameraData;
```

核心变化：

- 官方逻辑直接使用 `cameraRTSize` 计算 Pixel Perfect。
- 项目逻辑先取得输出 RT size，再根据当前 renderer render scale 计算用于 Pixel Perfect 的内部 RT size。
- Overlay camera 会反查所属 Base camera。
- 如果 Base camera 与 Overlay camera 的 render scale 接近，则 Overlay 使用 Base camera 作为 Pixel Perfect 计算来源。

新增方法：

- `GetBaseCameraForOverlay()`
- `GetRendererRenderScale(Camera sourceCamera)`
- `GetPixelPerfectSourceCamera()`
- `GetPixelPerfectCalculationRTSize(Vector2Int outputRTSize)`

核心逻辑：

```csharp
var outputRTSize = cameraRTSize;
var calculationRTSize = GetPixelPerfectCalculationRTSize(outputRTSize);
m_Internal.CalculateCameraProperties(calculationRTSize.x, calculationRTSize.y);
```

作用判断：

- 这是为了修复 per-renderer render scale 下 Pixel Perfect 的计算问题。
- 如果主 Renderer render scale 为 `0.5`，Pixel Perfect 仍按输出尺寸计算，会出现像素网格、镜头裁剪或最终 blit rect 不一致。
- Camera Stack 下 Overlay 和 Base 的 render scale 如果一致，应使用 Base camera 计算，避免 Overlay/UI/叠加相机和主相机像素对不齐。

官方替换风险：

- 官方 PixelPerfectCamera 不知道项目的 per-renderer render scale。
- 直接换官方包后，主相机 `0.5` render scale 和 Pixel Perfect 组合可能出现画面比例、采样或像素对齐问题。

### 4. FinalBlit nearest sampling 修正

涉及文件：

- `Runtime/Passes/FinalBlitPass.cs`
- `Runtime/UniversalRenderer.cs`

新增方法：

```csharp
static bool ShouldUseNearestSampling(UniversalCameraData cameraData, RTHandle source)
{
    if (cameraData.imageScalingMode == ImageScalingMode.Upscaling &&
        cameraData.upscalingFilter == ImageUpscalingFilter.Point)
    {
        return true;
    }

    return source.rt?.filterMode != FilterMode.Bilinear;
}
```

原官方逻辑：

- 主要根据 `source.rt.filterMode` 判断 bilinear/nearest pass。

项目逻辑：

- 如果当前 camera 使用 point upscaling，即使 source RT 不是 nearest filter，也强制走 nearest sampler pass。

相关 `UniversalRenderer` 修改：

- 调整 final post-processing 判断。
- 非 linear upscaling 时，强制通过 FinalPost 路径处理。
- 注释说明目标是让相机堆叠时保持和单相机一致的 point/non-linear sampling 路径，避免回退到 CoreBlit 的 bilinear。

作用判断：

- 这是与 Pixel Perfect/per-renderer render scale 绑定的采样修正。
- 主要解决放大采样变糊、Camera Stack 最终输出采样路径不一致的问题。

官方替换风险：

- 直接换官方后，Point upscaling 下某些最终 blit 可能走 bilinear，导致像素画面变糊。

### 5. DepthOfField 增强：角色/物件深度参与景深混合

涉及文件：

- `Runtime/Overrides/DepthOfField.cs`
- `Editor/Overrides/DepthOfFieldEditor.cs`
- `Shaders/PostProcessing/BokehDepthOfField.shader`

新增 Volume 参数：

```csharp
public MinFloatParameter BlurOffsetPos = new MinFloatParameter(0f, 0f);
public MinFloatParameter ReMapValueX = new MinFloatParameter(0f, 0f);
public MinFloatParameter ReMapValueY = new MinFloatParameter(2f, 0f);
```

编辑器面板新增：

```csharp
BlurOffsetPos
ReMapValueX
ReMapValueY
```

Shader 新增输入：

```hlsl
TEXTURE2D(_CharacterDepthTex);
SAMPLER(sampler_CharacterDepthTex);

TEXTURE2D(_ObjDepthTex);
SAMPLER(sampler_ObjDepthTex);

float _BlurOffsetPos;
float2 _ReMapValue;
```

Shader 功能：

- 采样 `_ObjDepthTex` 和 `_CharacterDepthTex`。
- 如果物件深度为空，则使用角色深度。
- 根据屏幕中心/当前 UV 的 Y 值、`_BlurOffsetPos` 和 `_ReMapValue` 计算混合权重。
- 用自定义深度权重在 `outColor` 与原始 `color` 之间混合，控制角色/物件受景深影响程度。
- 支持 `testShowType` debug 显示：
  - `0`：正常输出
  - `1`：显示 obj depth
  - `2`：显示合成深度

项目依赖：

- `Assets/DefaultVolumeProfile.asset` 序列化了 `BlurOffsetPos / ReMapValueX / ReMapValueY`。
- `Assets/Scenes/002/Main Camera Profile.asset` 序列化了 `BlurOffsetPos / ReMapValueX / ReMapValueY`。
- `Assets/Scripts/Main/CameraManager.cs` 直接访问 `DepthOfField.ReMapValueY`。
- `Assets/Render/Setting/ForwardAsset_Renderer.asset` 配置了 `_CharacterDepthTex` 和 `_ObjDepthTex`。
- `Assets/Render/Setting/EditorAsset_Renderer.asset` 配置了 `_CharacterDepthTex` 和 `_ObjDepthTex`。
- 多个项目 shader/material 使用 `_CharacterDepthTex`、`_ObjDepthTex` 或 `_BlurOffsetPos`。

作用判断：

- 这是项目景深系统的核心自定义。
- 用普通官方 DOF 只能按 camera depth 模糊，无法直接使用项目自定义角色/物件深度纹理控制混合。
- 当前项目代码直接引用新增字段，换官方会编译失败。

官方替换风险：

- 编译错误：`DepthOfField` 不存在 `ReMapValueY` 等字段。
- Volume Profile 中自定义字段丢失。
- 景深效果变化，角色/物件的深度遮挡和模糊混合会失效。

### 6. Bloom 性能裁剪和固定低成本路径

涉及文件：

- `Runtime/Overrides/Bloom.cs`
- `Runtime/Passes/PostProcessPassRenderGraph.cs`
- `Shaders/PostProcessing/Bloom.shader`
- `Shaders/PostProcessing/UberPost.shader`

参数变化：

```csharp
// 官方
public ClampedIntParameter maxIterations = new ClampedIntParameter(6, 2, 8);

// 项目
public ClampedIntParameter maxIterations = new ClampedIntParameter(4, 2, 6);
```

RenderGraph Bloom 变化：

- 固定从 half-res 开始，不再根据 `m_Bloom.downscale` 选择 half/quarter。
- 固定最多 4 个 mip。
- 固定 `BloomFilterMode.Gaussian`。
- 固定 `highQualityFiltering = false`。
- 不再根据 `m_Bloom.filter` 选择 `BloomDual` 或 `BloomKawase`。
- Lens flare bloom mip 逻辑删除 Kawase 特例。

Shader 变化：

- `Bloom.shader`
  - 移除 `_BLOOM_HQ` prefilter 路径。
  - Prefilter 从 HQ 多采样简化为单点 `SamplePrefilter`。
  - Horizontal blur 从 9 tap gaussian 改为 5 tap bilinear 近似。
  - Upsample 移除 bicubic/HQ 路径，统一普通采样。
  - 删除 `_BLOOM_HQ` variant。
- `UberPost.shader`
  - `_BLOOM_LQ / _BLOOM_HQ / _BLOOM_LQ_DIRT / _BLOOM_HQ_DIRT` 多变体改为只保留 `_BLOOM_LQ`。
  - 删除 Bloom dirt 混合。
  - 删除 HQ bicubic 采样。

项目依赖：

- 多个 Volume Profile 使用 `highQualityFiltering`、`filter`、`maxIterations` 字段。
- 但 RenderGraph 路径下当前项目修改会忽略 `filter/highQualityFiltering` 的真实选择，强制低成本路径。

作用判断：

- 这是明确的性能优化/画质取舍。
- 目标是降低 Bloom pass 成本、减少 shader variant、减少采样次数、固定稳定的低成本 Bloom。
- 代价是官方 Bloom 的 HQ、Dirt、Dual、Kawase、Quarter downscale 等可配置能力在 RenderGraph 路径被弱化或失效。

官方替换风险：

- 直接换官方后 Bloom 成本可能上升。
- 如果 Volume Profile 中启用了 HQ/filter/dirt，官方会恢复这些效果，画面和性能都会变。
- 需要重新评估移动端/低端设备性能。

### 7. RenderingUtils RenderGraph 工具方法公开

涉及文件：

- `Runtime/RenderingUtils.cs`

变化：

```csharp
// 官方
internal static void CreateRendererListWithRenderStateBlock(...)

// 项目
public static void CreateRendererListWithRenderStateBlock(...)
```

项目依赖：

- `Assets/Render/RenderFeature/MyRenderGraphPassFeature.cs`
- `Assets/Render/RenderFeature/FootStepPassFeature.cs`
- `Assets/Render/RenderFeature/BakerRenderGraphPassFeature.cs`

这些项目 RenderFeature 直接调用：

```csharp
RenderingUtils.CreateRendererListWithRenderStateBlock(...)
```

作用判断：

- 这是为了让项目自定义 RenderGraph RenderFeature 复用 URP 内部 RendererList 创建逻辑。
- 不改变 URP 渲染效果，但影响编译。

官方替换风险：

- 官方方法是 `internal`，项目代码无法访问。
- 直接换官方包会编译失败。
- 迁移时需要在项目侧复制一个等价工具方法，或者改用官方公开 API 自己创建 RendererList。

### 8. Lit.mat 附带修改

涉及文件：

- `Runtime/Materials/Lit.mat`

变化：

- 增加 `_AddPrecomputedVelocity: 0`
- 增加 `_XRMotionVectorsPass: 1`
- 默认颜色从灰色改为偏红：

```yaml
_BaseColor: {r: 1, g: 0.3820755, b: 0.3820755, a: 1}
_Color: {r: 1, g: 0.3820755, b: 0.3820755, a: 1}
```

作用判断：

- 这不像核心功能修改，更像编辑器保存或测试材质时产生的资源差异。
- 如果项目没有直接依赖包内默认 `Lit.mat`，迁移时可以忽略。
- 但因为文件确实不同，升级时需要确认是否有材质引用包内该默认材质。

## 项目依赖点

### 会导致编译失败的依赖

如果直接换官方 URP，以下依赖会出问题：

- `Assets/Scripts/Main/CameraManager.cs` 访问 `DepthOfField.ReMapValueY`。
- `Assets/Render/RenderFeature/MyRenderGraphPassFeature.cs` 调用 `RenderingUtils.CreateRendererListWithRenderStateBlock(RenderGraph...)`。
- `Assets/Render/RenderFeature/FootStepPassFeature.cs` 调用 `RenderingUtils.CreateRendererListWithRenderStateBlock(RenderGraph...)`。
- `Assets/Render/RenderFeature/BakerRenderGraphPassFeature.cs` 调用 `RenderingUtils.CreateRendererListWithRenderStateBlock(RenderGraph...)`。

### 会导致序列化字段丢失的资产

如果直接换官方 URP，以下字段官方类中不存在，会成为无效序列化数据：

- RendererData：
  - `m_transparencySortMode`
  - `m_transparencySortAxis`
  - `m_OverrideCameraScaling`
  - `m_CameraRenderScale`
  - `m_CameraUpscalingFilter`
- DepthOfField：
  - `BlurOffsetPos`
  - `ReMapValueX`
  - `ReMapValueY`

已确认包含这些字段的资产：

- `Assets/Render/Setting/ForwardAsset_Renderer.asset`
- `Assets/Render/Setting/UIAsset_Renderer.asset`
- `Assets/Render/Setting/EditorAsset_Renderer.asset`
- `Assets/DefaultVolumeProfile.asset`
- `Assets/Scenes/002/Main Camera Profile.asset`

### 会导致画面变化的功能

直接换官方 URP 后可能变化：

- 透明物体排序轴回退官方逻辑。
- 主 Renderer `0.5` render scale 覆盖失效。
- Camera Stack 与 Pixel Perfect 的计算路径变化。
- Point upscaling 最终 blit 可能变成 bilinear。
- 自定义角色/物件深度参与 DOF 失效。
- Bloom 恢复官方高成本/多模式路径，性能和画面都可能变化。

## 是否适合迁移到官方 URP

当前结论：不适合直接迁移。

原因：

- 当前项目已经把 URP fork 当成渲染系统的一部分使用。
- 这些修改不是单纯编辑器 bug fix，而是实际运行时功能。
- 至少存在明确编译依赖和资产序列化依赖。
- 直接删除 embedded URP 会导致编译错误、字段丢失、画面排序/景深/Bloom/Pixel Perfect 变化。

## 后续迁移建议

如果以后要回到官方 URP，应按功能拆迁，而不是一次性替换。

建议顺序：

1. 先迁移 `RenderingUtils.CreateRendererListWithRenderStateBlock`。

   在项目 `Assets/Render/RenderFeature` 下建立自己的 RenderGraph utility，复制或重写所需 RendererList 创建逻辑，让项目 RenderFeature 不再依赖 URP internal 方法。

2. 再迁移 DepthOfField 自定义。

   不建议继续改官方 `DepthOfField`。更合理的是实现项目自己的 VolumeComponent 和 RenderFeature/PostProcess Pass，把 `_CharacterDepthTex / _ObjDepthTex` 深度混合逻辑放到项目 shader/pass 中。

3. 再处理透明排序。

   如果必须按 RendererData 配置全局透明排序轴，官方 URP 没有等价入口。可选方案是自定义 RenderFeature 接管特定透明层渲染，或者继续保留 URP fork。

4. 再处理 per-renderer render scale。

   官方 URP 是 PipelineAsset 级 render scale。若必须按 Renderer 区分主相机/UI/编辑器相机缩放，需要继续 fork 或改成项目自己的相机/渲染策略。

5. 最后评估 Bloom。

   Bloom 性能裁剪可以迁到项目自定义后处理，也可以接受官方 Bloom 并重新调低 Volume Profile。但如果当前性能依赖这些裁剪，不能直接恢复官方路径。

## 保留策略

短期建议：

- 保留 `Packages/com.unity.render-pipelines.universal` embedded 自定义包。
- 升级 Unity/URP 时，把本文档中的 6 类核心功能作为移植清单。
- 不要把这个包标记为“可直接换官方”。

长期建议：

- 如果项目稳定后不计划频繁升级 Unity，继续维护 URP fork 成本可接受。
- 如果后续要长期跟 Unity 官方 URP 升级，应优先把 DepthOfField、RenderFeature 工具、Bloom 优化拆到 `Assets` 下，减少直接 patch URP 的面积。

