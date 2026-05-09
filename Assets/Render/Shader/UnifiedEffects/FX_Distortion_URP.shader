Shader "Project/FX/FX_Distortion_URP"
{
    Properties
    {
        [Header(Base Setup)]
        [Enum(DistortOnly,0,DistortOverlay,1)] _EffectMode("效果模式", Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("颜色源混合", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("颜色目标混合", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("Alpha源混合", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("Alpha目标混合", Float) = 10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("剔除模式", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("深度测试", Float) = 4
        [Toggle] _ZWrite("写入深度", Float) = 0

        [Header(Textures)]
        _MainTex("叠加纹理", 2D) = "white" {}
        _Noise("噪声纹理", 2D) = "white" {}
        _Mask("遮罩纹理", 2D) = "white" {}
        _Flow("流动扰动纹理", 2D) = "gray" {}
        _NormalMap("法线扰动纹理", 2D) = "bump" {}

        [Header(Color And Legacy)]
        [HDR] _Color("主颜色", Color) = (1,1,1,1)
        _SpeedMainTexUVNoiseZW("旧版主图/噪声滚动", Vector) = (0,0,0,0)
        _DistortionSpeedXYPowerZ("旧版扰动滚动/强度", Vector) = (0,0,0,0)

        [Header(Controls)]
        _Emission("发光强度", Float) = 1
        _Opacity("整体透明度", Range(0,4)) = 1
        _Distortionpower("屏幕扭曲强度", Float) = 0.05
        _FlowStrength("流动附加强度", Float) = 0.1
        _Softedges("启用边缘柔化", Float) = 0
        _Sideopacitymult("边缘衰减强度", Float) = 4

        [Header(Soft Particle)]
        _UseSoftParticle("启用软粒子", Float) = 1
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

            #include "Assets/Render/Shader/UnifiedEffects/FX_Common.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _EffectMode;
                float4 _Color;
                float4 _MainTex_ST;
                float4 _Noise_ST;
                float4 _Mask_ST;
                float4 _Flow_ST;
                float4 _NormalMap_ST;
                float4 _SpeedMainTexUVNoiseZW;
                float4 _DistortionSpeedXYPowerZ;
                float _Emission;
                float _Opacity;
                float _Distortionpower;
                float _FlowStrength;
                float _UseSoftParticle;
                float _SoftParticleNearFadeDistance;
                float _SoftParticleFarFadeDistance;
                float _Softedges;
                float _Sideopacitymult;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Noise); SAMPLER(sampler_Noise);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);
            TEXTURE2D(_Flow); SAMPLER(sampler_Flow);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            // Distortion keeps a dedicated path because it samples scene color.
            FXVaryings vert(FXAttributes input)
            {
                return FXVertex(input);
            }

            half4 frag(FXVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float softFade = FXSoftParticleFade(input.screenPos, _UseSoftParticle, _SoftParticleNearFadeDistance, _SoftParticleFarFadeDistance);
                float2 screenUV = GetNormalizedScreenSpaceUV(input.screenPos);

                float2 mainUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _MainTex), _SpeedMainTexUVNoiseZW.xy);
                float2 noiseUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _Noise), _SpeedMainTexUVNoiseZW.zw);
                float2 flowUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _Flow), _DistortionSpeedXYPowerZ.xy);
                float2 normalUV = TRANSFORM_TEX(input.uv0.xy, _NormalMap);
                float2 maskUV = TRANSFORM_TEX(input.uv0.xy, _Mask);

                half4 overlay = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                half4 noiseSample = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);
                half4 flowSample = SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV);
                half4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, normalUV);
                half4 maskSample = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV);

                float2 normalOffset = (UnpackNormal(normalSample).rg * _Distortionpower);
                float2 flowOffset = ((flowSample.rg * 2.0) - 1.0) * (_FlowStrength + _DistortionSpeedXYPowerZ.z);
                float2 opaqueUV = screenUV + (normalOffset + flowOffset) * maskSample.r;

                half3 sceneColor = SampleSceneColor(opaqueUV);
                half overlayAlpha = overlay.a * noiseSample.a * maskSample.a * _Opacity * input.color.a * softFade;

                if (_Softedges > 0.5)
                {
                    float fresnel = FXViewFresnel(input.normalWS, input.positionWS, 3.0);
                    overlayAlpha *= saturate(1.0 - fresnel * _Sideopacitymult);
                }

                half3 finalRgb = sceneColor;
                if (_EffectMode > 0.5)
                {
                    // Overlay mode adds emissive sprite color on top of distorted scene color.
                    finalRgb += overlay.rgb * noiseSample.rgb * _Color.rgb * input.color.rgb * _Emission;
                }

                finalRgb = FXApplyFog(finalRgb, input.fogFactor);
                return half4(finalRgb, saturate(overlayAlpha * _Color.a));
            }
            ENDHLSL
        }
    }
}
