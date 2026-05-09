Shader "Project/FX/FX_TwoSided_URP"
{
    Properties
    {
        [Header(Base Setup)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("颜色源混合", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("颜色目标混合", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("Alpha源混合", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("Alpha目标混合", Float) = 10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("剔除模式", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("深度测试", Float) = 4
        [Toggle] _ZWrite("写入深度", Float) = 0

        [Header(Textures)]
        _FrontTex("正面纹理", 2D) = "white" {}
        _BackTex("背面纹理", 2D) = "white" {}
        _Noise("噪声纹理", 2D) = "white" {}
        _Mask("遮罩纹理", 2D) = "white" {}

        [Header(Colors)]
        [HDR] _FrontColor("正面颜色", Color) = (1,1,1,1)
        [HDR] _BackColor("背面颜色", Color) = (1,1,1,1)
        [HDR] _FrontFresnelColor("正面边缘光颜色", Color) = (1,1,1,1)
        [HDR] _BackFresnelColor("背面边缘光颜色", Color) = (1,1,1,1)

        [Header(UV Scroll)]
        _FrontScroll("正面滚动 XY", Vector) = (0,0,0,0)
        _BackScroll("背面滚动 XY", Vector) = (0,0,0,0)
        _NoiseScroll("噪声滚动 XY", Vector) = (0,0,0,0)

        [Header(Controls)]
        _Emission("发光强度", Float) = 1
        _Opacity("整体透明度", Range(0,4)) = 1
        _SideOpacity("双面透明度倍率", Range(0,4)) = 1
        _FrontFresnelStrength("正面边缘光强度", Float) = 0
        _BackFresnelStrength("背面边缘光强度", Float) = 0
        _FresnelPower("边缘光锐度", Float) = 4

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

            // Two-sided path stays separate because front and back faces have different textures and colors.
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
                // Front/back branches intentionally keep different texture and color paths.
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
