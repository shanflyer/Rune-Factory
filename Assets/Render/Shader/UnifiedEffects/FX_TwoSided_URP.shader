Shader "Project/FX/FX_TwoSided_URP"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("Src Blend Alpha", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("Dst Blend Alpha", Float) = 10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Toggle] _ZWrite("ZWrite", Float) = 0

        _FrontTex("Front Tex", 2D) = "white" {}
        _BackTex("Back Tex", 2D) = "white" {}
        _Noise("Noise", 2D) = "white" {}
        _Mask("Mask", 2D) = "white" {}

        [HDR] _FrontColor("Front Color", Color) = (1,1,1,1)
        [HDR] _BackColor("Back Color", Color) = (1,1,1,1)
        [HDR] _FrontFresnelColor("Front Fresnel Color", Color) = (1,1,1,1)
        [HDR] _BackFresnelColor("Back Fresnel Color", Color) = (1,1,1,1)

        _FrontScroll("Front Scroll XY", Vector) = (0,0,0,0)
        _BackScroll("Back Scroll XY", Vector) = (0,0,0,0)
        _NoiseScroll("Noise Scroll XY", Vector) = (0,0,0,0)

        _Emission("Emission", Float) = 1
        _Opacity("Opacity", Range(0,4)) = 1
        _SideOpacity("Side Opacity", Range(0,4)) = 1
        _FrontFresnelStrength("Front Fresnel Strength", Float) = 0
        _BackFresnelStrength("Back Fresnel Strength", Float) = 0
        _FresnelPower("Fresnel Power", Float) = 4
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
                float4 _FrontTex_ST;
                float4 _BackTex_ST;
                float4 _Noise_ST;
                float4 _Mask_ST;
                float4 _FrontColor;
                float4 _BackColor;
                float4 _FrontFresnelColor;
                float4 _BackFresnelColor;
                float4 _FrontScroll;
                float4 _BackScroll;
                float4 _NoiseScroll;
                float _Emission;
                float _Opacity;
                float _SideOpacity;
                float _FrontFresnelStrength;
                float _BackFresnelStrength;
                float _FresnelPower;
                float _UseSoftParticle;
                float _SoftParticleNearFadeDistance;
                float _SoftParticleFarFadeDistance;
            CBUFFER_END

            TEXTURE2D(_FrontTex); SAMPLER(sampler_FrontTex);
            TEXTURE2D(_BackTex); SAMPLER(sampler_BackTex);
            TEXTURE2D(_Noise); SAMPLER(sampler_Noise);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);

            FXVaryings vert(FXAttributes input)
            {
                return FXVertex(input);
            }

            half4 frag(FXVaryings input, FRONT_FACE_TYPE facing : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float softFade = FXSoftParticleFade(input.screenPos, _UseSoftParticle, _SoftParticleNearFadeDistance, _SoftParticleFarFadeDistance);

                bool isFront = IS_FRONT_VFACE(facing, true, false);
                float2 texUV = isFront ? FXScroll(TRANSFORM_TEX(input.uv0.xy, _FrontTex), _FrontScroll.xy) : FXScroll(TRANSFORM_TEX(input.uv0.xy, _BackTex), _BackScroll.xy);
                half4 texSample = isFront ? SAMPLE_TEXTURE2D(_FrontTex, sampler_FrontTex, texUV) : SAMPLE_TEXTURE2D(_BackTex, sampler_BackTex, texUV);
                half4 sideColor = isFront ? _FrontColor : _BackColor;

                float2 noiseUV = FXScroll(TRANSFORM_TEX(input.uv0.xy, _Noise), _NoiseScroll.xy);
                half4 noiseSample = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, noiseUV);
                half4 maskSample = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, TRANSFORM_TEX(input.uv0.xy, _Mask));

                float fresnel = FXViewFresnel(input.normalWS, input.positionWS, _FresnelPower);
                half3 fresnelColor = isFront ? _FrontFresnelColor.rgb : _BackFresnelColor.rgb;
                float fresnelStrength = isFront ? _FrontFresnelStrength : _BackFresnelStrength;

                half3 finalRgb = texSample.rgb * noiseSample.rgb * maskSample.rgb * sideColor.rgb * input.color.rgb * _Emission;
                finalRgb += fresnelColor * fresnel * fresnelStrength;
                finalRgb = FXApplyFog(finalRgb, input.fogFactor);
                half finalAlpha = saturate(texSample.a * noiseSample.a * maskSample.a * sideColor.a * _Opacity * _SideOpacity * input.color.a * softFade);
                return half4(finalRgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
