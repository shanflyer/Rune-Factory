# FX Shader Consolidation Plan

## Target set

The effect prefabs under `Assets/Resources/Prefabs/Effect` can be consolidated into three project shaders:

1. `Project/FX/FX_SpriteCore_URP`
2. `Project/FX/FX_Distortion_URP`
3. `Project/FX/FX_TwoSided_URP`

This split keeps only genuinely different render paths separated while collapsing repeated transparent particle logic into one core shader.
`FX_Dissolve_URP` and `FX_Shockwave_URP` have been merged into `FX_SpriteCore_URP` logically, but the files are still kept as compatibility layers until all legacy materials are re-saved away from them.

## Legacy to new mapping

| Legacy shader / graph | New shader | Mode / notes |
| --- | --- | --- |
| `SampleEffectAdd` | `FX_SpriteCore_URP` | `Simple`, additive blend, `MainTex + Noise` scroll |
| `SampleEffectMul` | `FX_SpriteCore_URP` | `Simple`, alpha blend, `MainTex + Noise` scroll |
| `Hovl/Particles/Add_CenterGlow` | `FX_SpriteCore_URP` | `CenterGlow`, additive blend, `Flow + Mask + Noise` |
| `Hovl/Particles/Blend_CenterGlow` | `FX_SpriteCore_URP` | `CenterGlow`, alpha blend, `Flow + Mask + Noise` |
| `Hovl/Particles/Add_Fresnel` | `FX_SpriteCore_URP` | `Ice`, set `FresnelStrength > 0` |
| `Hovl/Particles/AddTrail` | `FX_SpriteCore_URP` | `Trail`, `StartColor/EndColor + Noise + MainTexture` |
| `Hovl/Particles/Blend_LinePath` | `FX_SpriteCore_URP` | `LinePath`, `MainTex + Noise + LineWidth/Softness` |
| `Hovl/Particles/Fire` | `FX_SpriteCore_URP` | `Fire`, uses legacy `_Tex1/_Tex2/_Mask` fields |
| `Hovl/Particles/Lightning` | `FX_SpriteCore_URP` | `Lightning`, uses legacy `_FlowMap` and noise flow |
| `Hovl/Particles/Scroll` | `FX_SpriteCore_URP` | `LinePath`, scroll-only preset |
| `Shader Graphs/URP_Add_CG` | `FX_SpriteCore_URP` | `CenterGlow`, additive preset |
| `Shader Graphs/URP_Blend_CG` | `FX_SpriteCore_URP` | `CenterGlow`, alpha preset |
| `Shader Graphs/URP_SwordSlash` | `FX_SpriteCore_URP` | `LinePath` or `Trail`, depends on material mask |
| `Shader Graphs/URP_Ice` | `FX_SpriteCore_URP` | `Ice`, `Main + Secondary + EdgeColor` |
| `Universal Render Pipeline/Particles/Unlit` | `FX_SpriteCore_URP` | `Simple`; current effect usage does not need the full URP particle uber feature set |
| `Cartoon FX/Remaster/Particle Ubershader` | `FX_SpriteCore_URP` | `Simple` or `Trail`; current effect usage is limited enough to collapse into project shader |
| `Unity built-in shader users` | `FX_SpriteCore_URP` | `Simple`; applies to current `乐曲` and `使用药物` usage |
| `Hovl/Particles/Distortion` | `FX_Distortion_URP` | `DistortOnly`, opaque texture distortion only |
| `Shader Graphs/URP_BlendDistort` | `FX_Distortion_URP` | `DistortOverlay`, scene distortion plus emissive overlay |
| `Shader Graphs/URP_Distortion` | `FX_Distortion_URP` | `DistortOnly` or `DistortOverlay`, depending on material alpha |
| `Hovl/Particles/DissolveNoise` | `FX_SpriteCore_URP` | `Dissolve`, `MainTex + Noise + DissolveTex` |
| `Shader Graphs/URP_SoftNoise` | `FX_SpriteCore_URP` | `SoftNoise`, same path with softer threshold / edge |
| `Shader Graphs/Fx_ParticleDissolve_apb` | `FX_SpriteCore_URP` | `MeshDissolve`, mesh dissolve preset |
| `Shader Graphs/Fx_RockDissolve` | `FX_SpriteCore_URP` | `MeshDissolve`, rock dissolve preset |
| `Hovl/Particles/ShockWave` | `FX_SpriteCore_URP` | `Shockwave`, `MainTex + Noise + Flow + Mask + Radial` |
| `Shader Graphs/URP_ShockWave` | `FX_SpriteCore_URP` | `Shockwave`, same preset family |
| `Shader Graphs/URP_Blend_TwoSides` | `FX_TwoSided_URP` | front/back separate texture and color |

## Parameter remap

### `FX_SpriteCore_URP`

- Shared base:
  `MainTex / BaseMap / MainTexture / Texture2D_F593E37E / Texture2D_EDA87E5 -> _MainTex`
- Shared noise:
  `Noise / OpacityTex / EmissionTex -> _Noise`
- Shared tint:
  `Color / BaseColor / TintColor -> _Color`
- Shared UV scroll:
  `SpeedMainTexUVNoiseZW -> _SpeedMainTexUVNoiseZW`
- Shared soft particle:
  `InvFade / SoftParticlesEnabled / UseSP / Usedepth / SoftParticlesNearFadeDistance / SoftParticlesFarFadeDistance -> _UseSoftParticle / _SoftParticleNearFadeDistance / _SoftParticleFarFadeDistance`

- Center glow family:
  `Mask -> _Mask`
  `Flow -> _Flow`
  `DistortionSpeedXYPowerZ -> _DistortionSpeedXYPowerZ`
  `Usecenterglow -> _CenterGlowStrength`
  `Emission -> _Emission`
  `Opacity -> _Opacity`

- Trail family:
  `MainTexture -> _MainTex`
  `StartColor -> _StartColor`
  `EndColor -> _EndColor`
  `Colorpower -> _GradientPower`
  `Colorrange -> _GradientRange`
  `Maskpower -> _MaskPower`
  `Emission -> _Emission`

- Line / sword slash family:
  `MainTex / MainTexture -> _MainTex`
  `Noise / EmissionTex -> _Noise`
  `Flow -> _Flow`
  `AddColor -> _EdgeColor`
  `Flowpower -> _FlowStrength`
  `SpeedFlow -> _FlowScroll`
  `Dissolve -> _DissolveThreshold`
  `Usesmoothdissolve -> _DissolveSoftness`
  `Opacity -> _Opacity`
  `Emission -> _Emission`

- Fire family:
  `Tex1 / Tex0 -> _Tex1`
  `Tex2 -> _Tex2`
  `Mask -> _Mask`
  `Color1 -> _Color`
  `Color2 -> _SecondaryColor`
  `SpeedTex1 -> _SpeedTex1`
  `SpeedTex2XYEmission -> _SpeedTex2XYEmission`
  `Opacity -> _Opacity`

- Lightning family:
  `MainTexture -> _MainTex`
  `Noise -> _Noise`
  `FlowMap -> _FlowMap`
  `UFlowSpeed / VFlowSpeed -> _SpeedTex2XYEmission.xy`
  `FlowStrength -> _FlowStrength`
  `Emission -> _Emission`

- Ice / fresnel family:
  `MainTex -> _MainTex`
  `Color -> _Color`
  `UpColor -> _SecondaryColor`
  `FresnelColor -> _EdgeColor`
  `FresnelScale / FresnelEmission -> _FresnelStrength`
  `FresnelPower -> _FresnelPower`
  `ColorPosition -> _SecondaryBlend`
  `Emission -> _Emission`

- Dissolve family:
  `MainTex / Texture2D_F593E37E / Texture2D_EDA87E5 -> _MainTex`
  `TextureNoise / Noise / OpacityTex / DissolveTex -> _Noise`
  `Dissolvenoise / Noise / OpacityTex / DissolveTex -> _DissolveTex`
  `Maincolor / Color / BaseColor -> _Color`
  `Noisecolor / Color -> _SecondaryColor`
  `Dissolvecolor / AddColor / EmissionColor -> _EdgeColor`
  `NoisespeedXYEmissonZPowerW / Vector2_176F980A -> _NoiseScroll / _Emission / _NoisePower`
  `DissolvespeedXY -> _DissolveScroll`
  `Dissolve / Cutoff / Vector1_D283BF28 -> _DissolveThreshold`
  `DissolveSoftness -> _DissolveSoftness`
  `DissolveEdgeWidth -> _DissolveEdgeWidth`
  `UseDissolveTex -> keyword _FX_USE_DISSOLVE_TEX`

- Shockwave family:
  `MainTexture -> _MainTex`
  `Noise -> _Noise`
  `Flow -> _Flow`
  `Mask -> _Mask`
  `NoiseSpeedXYPowerZ -> _NoiseScroll`
  `DistortionSpeedXYPowerZ -> _DistortionSpeedXYPowerZ`
  `Color -> _Color`
  `Emission -> _Emission`
  `Opacity -> _Opacity`
  `InnerRadius / OuterRadius / RingSoftness -> same names`

### `FX_Distortion_URP`

- `NormalMap -> _NormalMap`
- `MainTex -> _MainTex`
- `Noise -> _Noise`
- `Flow -> _Flow`
- `Mask -> _Mask`
- `Color -> _Color`
- `SpeedMainTexUVNoiseZW -> _SpeedMainTexUVNoiseZW`
- `DistortionSpeedXYPowerZ -> _DistortionSpeedXYPowerZ`
- `Distortionpower -> _Distortionpower`
- `Softedges -> _Softedges`
- `Sideopacitymult -> _Sideopacitymult`
- `Emission -> _Emission`
- `Opacity -> _Opacity`
- `InvFade / soft particle values -> _UseSoftParticle and fade distances`

### `FX_TwoSided_URP`

- `MainTex -> _FrontTex and _BackTex`
- `Noise -> _Noise`
- `Mask -> _Mask`
- `FrontFacesColor -> _FrontColor`
- `BackFacesColor -> _BackColor`
- `FresnelColor -> _FrontFresnelColor`
- `BackFresnelColor -> _BackFresnelColor`
- `Fresnel / FresnelEmission / UseFresnel -> _FrontFresnelStrength / _FresnelPower`
- `BackFresnel / BackFresnelEmission / UseBackFresnel -> _BackFresnelStrength / _FresnelPower`
- `Sideopacity -> _SideOpacity`
- `SpeedMainTexUVNoiseZW -> _FrontScroll / _BackScroll / _NoiseScroll`
- `Emission -> _Emission`
- `Opacity -> _Opacity`

## Coverage notes

- `FX_SpriteCore_URP` intentionally absorbs the largest repeated family: simple additive / alpha particles, center glow, trail, slash, fire, lightning, ice-like edge highlight, dissolve, and shockwave.
- `FX_Distortion_URP` is isolated so distortion keeps its own opaque-texture path instead of polluting the sprite core.
- `FX_TwoSided_URP` only exists because `URP_Blend_TwoSides` has a genuinely different front/back requirement.

## Migration rules

1. Migrate by legacy shader family, not by prefab name.
2. Keep the original blend state when switching materials.
3. Preserve original property names in migration tooling where possible:
   - `MainTex`, `MainTexture`, `Tex1`, `Tex2`
   - `Noise`, `TextureNoise`, `Dissolvenoise`
   - `Mask`, `Flow`, `FlowMap`
4. Replace Shader Graph generated materials first. They are the heaviest source of pass and compile duplication.
5. Validate these cases before deleting legacy shaders:
   - additive hit flashes
   - alpha blended buff loops
   - line / trail slashes
   - distortion skills
   - dissolve / vanish skills
   - radial markers and shockwaves
   - front/back two-sided cards
6. Use the editor tool in `Assets/Editor/Tool/FXShaderMigrationTool.cs` to scan, preview, and batch replace material assets repeatedly.

## Expected end state

- Current used shader / graph classes: `25+`
- Planned project runtime shader set: `3`
- Reduction principle: keep render-path differences, merge repeated math and texture flow logic, and only use keyword toggles for optional extra texture paths
