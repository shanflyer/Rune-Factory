Shader "Project/FX/FX_SpriteCore_URP"
{
    Properties
    {
        [Header(Base Setup)]
        [Enum(Simple,0,CenterGlow,1,Trail,2,LinePath,3,Fire,4,Lightning,5,Ice,6,Dissolve,7,SoftNoise,8,MeshDissolve,9,Shockwave,10,Marker,11,RadialPulse,12)] _EffectMode("效果模式", Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("颜色源混合", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("颜色目标混合", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("Alpha源混合", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("Alpha目标混合", Float) = 10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("剔除模式", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("深度测试", Float) = 4
        [Toggle] _ZWrite("写入深度", Float) = 0

        [Header(Textures)]
        [MainTexture] _MainTex("主纹理", 2D) = "white" {}
        _SecondaryTex("副纹理", 2D) = "white" {}
        _Noise("噪声纹理", 2D) = "white" {}
        _Mask("遮罩纹理", 2D) = "white" {}
        _Flow("流动扰动纹理", 2D) = "gray" {}
        _DissolveTex("溶解纹理", 2D) = "white" {}
        _Tex1("旧版火焰纹理1", 2D) = "white" {}
        _Tex2("旧版火焰纹理2", 2D) = "white" {}
        _FlowMap("旧版流向图", 2D) = "gray" {}

        [Header(Optional Sampling)]
        [Toggle(_FX_USE_SECONDARY_TEX)] _UseSecondaryTex("启用副纹理采样", Float) = 0
        [Toggle(_FX_USE_DISSOLVE_TEX)] _UseDissolveTex("启用溶解纹理采样", Float) = 0
        [Toggle(_FX_USE_TEX1_TEX)] _UseTex1Tex("启用旧版火焰纹理1", Float) = 0
        [Toggle(_FX_USE_TEX2_TEX)] _UseTex2Tex("启用旧版火焰纹理2", Float) = 0
        [Toggle(_FX_USE_FLOWMAP_TEX)] _UseFlowMapTex("启用流向图采样", Float) = 0

        [Header(Colors)]
        [HDR] _Color("主颜色", Color) = (1,1,1,1)
        [HDR] _SecondaryColor("副颜色", Color) = (1,1,1,1)
        [HDR] _StartColor("起始颜色", Color) = (1,1,1,1)
        [HDR] _EndColor("结束颜色", Color) = (1,1,1,1)
        [HDR] _EdgeColor("边缘颜色", Color) = (1,1,1,1)

        [Header(UV Scroll)]
        _MainScroll("主纹理滚动 XY", Vector) = (0,0,0,0)
        _SecondaryScroll("副纹理滚动 XY", Vector) = (0,0,0,0)
        _NoiseScroll("噪声滚动 XY", Vector) = (0,0,0,0)
        _FlowScroll("扰动滚动 XY", Vector) = (0,0,0,0)
        _DissolveScroll("溶解滚动 XY", Vector) = (0,0,0,0)

        [Header(Legacy Compatibility)]
        _SpeedMainTexUVNoiseZW("旧版主图/噪声滚动", Vector) = (0,0,0,0)
        _DistortionSpeedXYPowerZ("旧版扰动滚动/强度", Vector) = (0,0,0,0)
        _SpeedTex1("旧版纹理1滚动", Vector) = (0,0,0,0)
        _SpeedTex2XYEmission("旧版纹理2滚动/发光", Vector) = (0,0,1,0)
        _NoisespeedXYEmissonZPowerW("旧版噪声滚动/发光/强度", Vector) = (0,0,1,1)

        [Header(Controls)]
        _Emission("发光强度", Float) = 1
        _Opacity("整体透明度", Range(0,4)) = 1
        _NoisePower("噪声影响强度", Float) = 1
        _FlowStrength("流动扰动强度", Float) = 0
        _SecondaryBlend("主副纹理混合", Range(0,1)) = 0
        _MaskPower("遮罩影响强度", Float) = 1
        _GradientPower("渐变强度", Float) = 1
        _GradientRange("渐变范围", Float) = 1
        _CenterGlowStrength("中心发光强度", Float) = 0
        _CenterGlowPower("中心发光收缩", Float) = 2
        _FresnelStrength("边缘光强度", Float) = 0
        _FresnelPower("边缘光锐度", Float) = 4
        _LineWidth("线条宽度", Range(0.01, 1)) = 0.25
        _LineSoftness("线条软化", Range(0.001, 1)) = 0.15
        _DissolveThreshold("溶解阈值", Range(0,1)) = 0
        _DissolveSoftness("溶解软化", Range(0.001,1)) = 0.1
        _DissolveEdgeWidth("溶解边缘宽度", Range(0.001,1)) = 0.1
        _InnerRadius("内圈半径", Range(0,1)) = 0.2
        _OuterRadius("外圈半径", Range(0,1)) = 0.8
        _RingSoftness("圆环软化", Range(0.001,1)) = 0.1

        [Header(Soft Particle)]
        _UseSoftParticle("启用软粒子", Float) = 0
        _SoftParticleNearFadeDistance("近端淡出距离", Float) = 0
        _SoftParticleFarFadeDistance("远端淡出距离", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]
            ZWrite [_ZWrite]
            ZTest [_ZTest]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _FX_USE_SECONDARY_TEX
            #pragma shader_feature_local_fragment _FX_USE_DISSOLVE_TEX
            #pragma shader_feature_local_fragment _FX_USE_TEX1_TEX
            #pragma shader_feature_local_fragment _FX_USE_TEX2_TEX
            #pragma shader_feature_local_fragment _FX_USE_FLOWMAP_TEX

            #include "Assets/Render/Shader/UnifiedEffects/FX_Common.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _EffectMode;
                float4 _Color;
                float4 _SecondaryColor;
                float4 _StartColor;
                float4 _EndColor;
                float4 _EdgeColor;
                float4 _MainTex_ST;
                float4 _SecondaryTex_ST;
                float4 _Noise_ST;
                float4 _Mask_ST;
                float4 _Flow_ST;
                float4 _DissolveTex_ST;
                float4 _Tex1_ST;
                float4 _Tex2_ST;
                float4 _FlowMap_ST;
                float _UseSecondaryTex;
                float _UseDissolveTex;
                float _UseTex1Tex;
                float _UseTex2Tex;
                float _UseFlowMapTex;
                float4 _MainScroll;
                float4 _SecondaryScroll;
                float4 _NoiseScroll;
                float4 _FlowScroll;
                float4 _DissolveScroll;
                float4 _SpeedMainTexUVNoiseZW;
                float4 _DistortionSpeedXYPowerZ;
                float4 _SpeedTex1;
                float4 _SpeedTex2XYEmission;
                float4 _NoisespeedXYEmissonZPowerW;
                float _Emission;
                float _Opacity;
                float _NoisePower;
                float _FlowStrength;
                float _SecondaryBlend;
                float _MaskPower;
                float _GradientPower;
                float _GradientRange;
                float _CenterGlowStrength;
                float _CenterGlowPower;
                float _FresnelStrength;
                float _FresnelPower;
                float _LineWidth;
                float _LineSoftness;
                float _DissolveThreshold;
                float _DissolveSoftness;
                float _DissolveEdgeWidth;
                float _InnerRadius;
                float _OuterRadius;
                float _RingSoftness;
                float _UseSoftParticle;
                float _SoftParticleNearFadeDistance;
                float _SoftParticleFarFadeDistance;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_SecondaryTex); SAMPLER(sampler_SecondaryTex);
            TEXTURE2D(_Noise); SAMPLER(sampler_Noise);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);
            TEXTURE2D(_Flow); SAMPLER(sampler_Flow);
            TEXTURE2D(_DissolveTex); SAMPLER(sampler_DissolveTex);
            TEXTURE2D(_Tex1); SAMPLER(sampler_Tex1);
            TEXTURE2D(_Tex2); SAMPLER(sampler_Tex2);
            TEXTURE2D(_FlowMap); SAMPLER(sampler_FlowMap);

            // Shared unlit vertex path.
            FXVaryings vert(FXAttributes input)
            {
                return FXVertex(input);
            }

            half4 frag(FXVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                int mode = (int)round(_EffectMode);
                float softFade = FXSoftParticleFade(input.screenPos, _UseSoftParticle, _SoftParticleNearFadeDistance, _SoftParticleFarFadeDistance);

                float2 mainUV = TRANSFORM_TEX(input.uv0.xy, _MainTex);
                float2 secondaryUV = TRANSFORM_TEX(input.uv0.xy, _SecondaryTex);
                float2 noiseUV = TRANSFORM_TEX(input.uv0.xy, _Noise);
                float2 maskUV = TRANSFORM_TEX(input.uv0.xy, _Mask);
                float2 flowUV = TRANSFORM_TEX(input.uv0.xy, _Flow);
                float2 dissolveUV = TRANSFORM_TEX(input.uv0.xy, _DissolveTex);
                float2 tex1UV = TRANSFORM_TEX(input.uv0.xy, _Tex1);
                float2 tex2UV = TRANSFORM_TEX(input.uv0.xy, _Tex2);
                float2 flowMapUV = TRANSFORM_TEX(input.uv0.xy, _FlowMap);

                mainUV = FXScroll(mainUV, _MainScroll.xy + _SpeedMainTexUVNoiseZW.xy);
                secondaryUV = FXScroll(secondaryUV, _SecondaryScroll.xy + _SpeedTex1.zw);
                noiseUV = FXScroll(noiseUV, _NoiseScroll.xy + _SpeedMainTexUVNoiseZW.zw + _NoisespeedXYEmissonZPowerW.xy);
                flowUV = FXScroll(flowUV, _FlowScroll.xy + _DistortionSpeedXYPowerZ.xy);
                dissolveUV = FXScroll(dissolveUV, _DissolveScroll.xy);
                tex1UV = FXScroll(tex1UV, _SpeedTex1.xy);
                tex2UV = FXScroll(tex2UV, _SpeedTex2XYEmission.xy);
                flowMapUV = FXScroll(flowMapUV, float2(_SpeedTex2XYEmission.x, _SpeedTex2XYEmission.y));

                // Keep high frequency paths cheap: only sample extra maps when the active mode needs them.
                bool useMask = (mode == 1) || (mode == 2) || (mode == 4) || (mode == 6) || (mode >= 10);
                bool useFlow = (mode == 1) || (mode == 3) || (mode >= 10);

                half4 maskSample = half4(1.0, 1.0, 1.0, 1.0);
                if (useMask)
                {
                    maskSample = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV);
                }

                half4 flowSample = half4(0.5, 0.5, 0.0, 0.0);
                if (useFlow)
                {
                    flowSample = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV);
                }

                float2 distortion = ((flowSample.rg * 2.0) - 1.0) * (_FlowStrength + _DistortionSpeedXYPowerZ.z) * maskSample.r;
                float2 distortedMainUV = mainUV + distortion;
                float2 distortedSecondaryUV = secondaryUV + distortion;

                half4 mainSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedMainUV);
                half4 noiseSample = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);
                half4 secondarySample = mainSample;
            #if defined(_FX_USE_SECONDARY_TEX)
                secondarySample = SAMPLE_TEXTURE2D(_SecondaryTex, sampler_SecondaryTex, distortedSecondaryUV);
            #endif
                half4 dissolveSample = noiseSample;
            #if defined(_FX_USE_DISSOLVE_TEX)
                dissolveSample = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, dissolveUV);
            #endif
                half4 tex1Sample = half4(1.0, 1.0, 1.0, 1.0);
            #if defined(_FX_USE_TEX1_TEX)
                tex1Sample = SAMPLE_TEXTURE2D(_Tex1, sampler_Tex1, tex1UV);
            #endif
                half4 tex2Sample = half4(1.0, 1.0, 1.0, 1.0);
            #if defined(_FX_USE_TEX2_TEX)
                tex2Sample = SAMPLE_TEXTURE2D(_Tex2, sampler_Tex2, tex2UV);
            #endif
                half4 flowMapSample = half4(0.5, 0.5, 0.0, 0.0);
            #if defined(_FX_USE_FLOWMAP_TEX)
                flowMapSample = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, flowMapUV);
            #endif

                half4 combined = mainSample;
                half3 tint = _Color.rgb;
                half3 extraRgb = 0.0;
                half alpha = mainSample.a;

                // 0-6: original sprite-family presets.
                if (mode == 0)
                {
                    combined.rgb *= lerp(1.0, noiseSample.rgb, saturate(_NoisePower));
                    alpha *= lerp(1.0, noiseSample.a, saturate(_NoisePower));
                }
                else if (mode == 1)
                {
                    float centerGlow = FXCenterMask(input.uv0.xy, _CenterGlowPower) * _CenterGlowStrength;
                    combined.rgb *= noiseSample.rgb;
                    combined.rgb += combined.rgb * centerGlow;
                    alpha *= noiseSample.a * maskSample.a;
                }
                else if (mode == 2)
                {
                    float gradient = FXGradient(input.uv0.x, _GradientRange, _GradientPower);
                    tint = lerp(_StartColor.rgb, _EndColor.rgb, gradient);
                    combined.rgb *= noiseSample.rgb;
                    alpha *= noiseSample.a * lerp(1.0, maskSample.a, saturate(_MaskPower));
                }
                else if (mode == 3)
                {
                    float lineMask = FXLineMask(input.uv0.xy, _LineWidth, _LineSoftness);
                    combined.rgb *= noiseSample.rgb;
                    alpha *= noiseSample.a * lineMask;
                }
                else if (mode == 4)
                {
                    float fireFactor = saturate((tex1Sample.r + tex1Sample.a) * tex2Sample.g * maskSample.b);
                    combined.rgb = lerp(_Color.rgb, _SecondaryColor.rgb, fireFactor);
                    alpha = fireFactor * _Opacity * input.color.a;
                }
                else if (mode == 5)
                {
                    float2 flowOffset = ((flowMapSample.rg * 2.0) - 1.0) * max(_FlowStrength, 0.001);
                    half4 lightningSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV + flowOffset);
                    combined.rgb = lightningSample.rgb * noiseSample.g;
                    alpha = lightningSample.a * noiseSample.a;
                }
                else if (mode == 6)
                {
                    float fresnel = FXViewFresnel(input.normalWS, input.positionWS, _FresnelPower) * _FresnelStrength;
                    combined.rgb = lerp(mainSample.rgb * _Color.rgb, secondarySample.rgb * _SecondaryColor.rgb, saturate(_SecondaryBlend));
                    combined.rgb += _EdgeColor.rgb * fresnel;
                    alpha *= maskSample.a;
                }
                // 7-9: dissolve family merged from FX_Dissolve_URP.
                else if (mode == 7 || mode == 8 || mode == 9)
                {
                    float dissolveValue = dissolveSample.r;
                    float bodyMask = smoothstep(_DissolveThreshold - _DissolveSoftness, _DissolveThreshold + _DissolveSoftness, dissolveValue);
                    float edgeMask = smoothstep(_DissolveThreshold - _DissolveEdgeWidth, _DissolveThreshold, dissolveValue) - bodyMask;
                    half3 baseColor = lerp(_Color.rgb, _SecondaryColor.rgb, saturate(noiseSample.r * max(_NoisePower, 0.0))) * mainSample.rgb;
                    if (mode == 9)
                    {
                        baseColor = mainSample.rgb;
                    }

                    combined.rgb = baseColor;
                    tint = 1.0;
                    extraRgb = _EdgeColor.rgb * saturate(edgeMask);
                    alpha = mainSample.a * noiseSample.a * bodyMask;
                }
                // 10-12: radial family merged from FX_Shockwave_URP.
                else if (mode == 10 || mode == 11 || mode == 12)
                {
                    half4 shockwaveSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV + (((flowSample.rg * 2.0) - 1.0) * _DistortionSpeedXYPowerZ.z));
                    float radialMask = FXRingMask(input.uv0.xy, _InnerRadius, _OuterRadius, _RingSoftness);
                    if (mode == 11)
                    {
                        radialMask = FXCenterMask(input.uv0.xy, 1.5);
                    }
                    else if (mode == 12)
                    {
                        radialMask = 1.0 - smoothstep(_InnerRadius, _OuterRadius, length(input.uv0.xy * 2.0 - 1.0));
                    }

                    combined.rgb = shockwaveSample.rgb * noiseSample.rgb * maskSample.rgb;
                    alpha = shockwaveSample.a * noiseSample.a * maskSample.a * radialMask;
                }

                // Legacy sprite family still keeps the lightweight single-noise dissolve gate.
                if (mode <= 6)
                {
                    float dissolveSource = noiseSample.r;
                    float dissolveMask = smoothstep(_DissolveThreshold - _DissolveSoftness, _DissolveThreshold + _DissolveSoftness, dissolveSource);
                    alpha *= dissolveMask;
                }

                half3 finalRgb = (combined.rgb * tint + extraRgb) * input.color.rgb * _Emission;
                finalRgb = FXApplyFog(finalRgb, input.fogFactor);
                half finalAlpha = saturate(alpha * _Color.a * _Opacity * input.color.a * softFade);
                return half4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
