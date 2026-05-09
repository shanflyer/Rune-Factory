Shader "Project/FX/FX_Shockwave_URP"
{
    Properties
    {
        [Enum(Shockwave,0,Marker,1,RadialPulse,2)] _EffectMode("Effect Mode", Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("Src Blend Alpha", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("Dst Blend Alpha", Float) = 10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Toggle] _ZWrite("ZWrite", Float) = 0

        _MainTexture("Main Texture", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _Flow("Flow", 2D) = "gray" {}
        _Mask("Mask", 2D) = "white" {}

        [HDR] _Color("Color", Color) = (1,1,1,1)
        _DistortionSpeedXYPowerZ("Distortion Scroll/Power", Vector) = (0,0,0,0)
        _NoiseSpeedXYPowerZ("Noise Scroll/Power", Vector) = (0,0,1,0)

        _Emission("Emission", Float) = 1
        _Opacity("Opacity", Range(0,4)) = 1
        _InnerRadius("Inner Radius", Range(0,1)) = 0.2
        _OuterRadius("Outer Radius", Range(0,1)) = 0.8
        _RingSoftness("Ring Softness", Range(0.001,1)) = 0.1
        _UseSoftParticle("Use Soft Particle", Float) = 1
        _SoftParticleNearFadeDistance("Soft Particle Near Fade", Float) = 0
        _SoftParticleFarFadeDistance("Soft Particle Far Fade", Float) = 1
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
                float4 _MainTexture_ST;
                float4 _Noise_ST;
                float4 _Flow_ST;
                float4 _Mask_ST;
                float4 _Color;
                float4 _DistortionSpeedXYPowerZ;
                float4 _NoiseSpeedXYPowerZ;
                float _Emission;
                float _Opacity;
                float _InnerRadius;
                float _OuterRadius;
                float _RingSoftness;
                float _UseSoftParticle;
                float _SoftParticleNearFadeDistance;
                float _SoftParticleFarFadeDistance;
            CBUFFER_END

            TEXTURE2D(_MainTexture); SAMPLER(sampler_MainTexture);
            TEXTURE2D(_Noise); SAMPLER(sampler_Noise);
            TEXTURE2D(_Flow); SAMPLER(sampler_Flow);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);

            FXVaryings vert(FXAttributes input)
            {
                return FXVertex(input);
            }

            half4 frag(FXVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float softFade = FXSoftParticleFade(input.screenPos, _UseSoftParticle, _SoftParticleNearFadeDistance, _SoftParticleFarFadeDistance);

                float2 mainUV = TRANSFORM_TEX(input.uv0.xy, _MainTexture);
                float2 noiseUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _Noise), _NoiseSpeedXYPowerZ.xy);
                float2 flowUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _Flow), _DistortionSpeedXYPowerZ.xy);
                float2 maskUV = TRANSFORM_TEX(input.uv0.xy, _Mask);

                half4 mainSample = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, mainUV + (((SAMPLE_TEXTURE2D(_Flow, sampler_Flow, flowUV).rg * 2.0) - 1.0) * _DistortionSpeedXYPowerZ.z));
                half4 noiseSample = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);
                half4 maskSample = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUV);

                float radialMask = FXRingMask(input.uv0.xy, _InnerRadius, _OuterRadius, _RingSoftness);
                if (_EffectMode > 0.5 && _EffectMode < 1.5)
                {
                    radialMask = FXCenterMask(input.uv0.xy, 1.5);
                }
                else if (_EffectMode > 1.5)
                {
                    radialMask = 1.0 - smoothstep(_InnerRadius, _OuterRadius, length(input.uv0.xy * 2.0 - 1.0));
                }

                half3 finalRgb = mainSample.rgb * noiseSample.rgb * maskSample.rgb * _Color.rgb * input.color.rgb * _Emission;
                finalRgb = FXApplyFog(finalRgb, input.fogFactor);
                half finalAlpha = saturate(mainSample.a * noiseSample.a * maskSample.a * radialMask * _Color.a * _Opacity * input.color.a * softFade);
                return half4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
