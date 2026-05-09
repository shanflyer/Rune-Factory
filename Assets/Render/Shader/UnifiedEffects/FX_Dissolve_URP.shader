Shader "Project/FX/FX_Dissolve_URP"
{
    Properties
    {
        [Enum(NoiseDissolve,0,SoftNoise,1,MeshDissolve,2)] _EffectMode("Effect Mode", Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("Src Blend Alpha", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("Dst Blend Alpha", Float) = 10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Toggle] _ZWrite("ZWrite", Float) = 0

        _MainTex("Main Tex", 2D) = "white" {}
        _TextureNoise("Texture Noise", 2D) = "white" {}
        _Dissolvenoise("Dissolve Noise", 2D) = "white" {}

        [HDR] _Maincolor("Main Color", Color) = (1,1,1,1)
        [HDR] _Noisecolor("Noise Color", Color) = (1,1,1,1)
        [HDR] _Dissolvecolor("Dissolve Edge Color", Color) = (1,1,1,1)

        _NoisespeedXYEmissonZPowerW("Noise Scroll/Emission/Power", Vector) = (0,0,1,1)
        _DissolvespeedXY("Dissolve Scroll XY", Vector) = (0,0,0,0)

        _Opacity("Opacity", Range(0,4)) = 1
        _DissolveThreshold("Dissolve Threshold", Range(0,1)) = 0.5
        _DissolveSoftness("Dissolve Softness", Range(0.001,1)) = 0.08
        _DissolveEdgeWidth("Dissolve Edge Width", Range(0.001,1)) = 0.1
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
                float4 _MainTex_ST;
                float4 _TextureNoise_ST;
                float4 _Dissolvenoise_ST;
                float4 _Maincolor;
                float4 _Noisecolor;
                float4 _Dissolvecolor;
                float4 _NoisespeedXYEmissonZPowerW;
                float4 _DissolvespeedXY;
                float _Opacity;
                float _DissolveThreshold;
                float _DissolveSoftness;
                float _DissolveEdgeWidth;
                float _UseSoftParticle;
                float _SoftParticleNearFadeDistance;
                float _SoftParticleFarFadeDistance;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_TextureNoise); SAMPLER(sampler_TextureNoise);
            TEXTURE2D(_Dissolvenoise); SAMPLER(sampler_Dissolvenoise);

            FXVaryings vert(FXAttributes input)
            {
                return FXVertex(input);
            }

            half4 frag(FXVaryings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float softFade = FXSoftParticleFade(input.screenPos, _UseSoftParticle, _SoftParticleNearFadeDistance, _SoftParticleFarFadeDistance);

                float2 mainUV = TRANSFORM_TEX(input.uv0.xy, _MainTex);
                float2 noiseUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _TextureNoise), _NoisespeedXYEmissonZPowerW.xy);
                float2 dissolveUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _Dissolvenoise), _DissolvespeedXY.xy);

                half4 mainSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                half4 noiseSample = SAMPLE_TEXTURE2D(_TextureNoise, sampler_TextureNoise, noiseUV);
                half4 dissolveSample = SAMPLE_TEXTURE2D(_Dissolvenoise, sampler_Dissolvenoise, dissolveUV);

                float dissolveValue = dissolveSample.r;
                float bodyMask = smoothstep(_DissolveThreshold - _DissolveSoftness, _DissolveThreshold + _DissolveSoftness, dissolveValue);
                float edgeMask = smoothstep(_DissolveThreshold - _DissolveEdgeWidth, _DissolveThreshold, dissolveValue) - bodyMask;

                half3 baseColor = lerp(_Maincolor.rgb, _Noisecolor.rgb, noiseSample.r) * mainSample.rgb;
                if (_EffectMode > 1.5)
                {
                    baseColor = mainSample.rgb;
                }

                half3 finalRgb = (baseColor * _NoisespeedXYEmissonZPowerW.z) + (_Dissolvecolor.rgb * edgeMask);
                finalRgb *= input.color.rgb;
                finalRgb = FXApplyFog(finalRgb, input.fogFactor);

                half finalAlpha = saturate(mainSample.a * noiseSample.a * bodyMask * _Opacity * input.color.a * softFade);
                return half4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
