Shader "MySprite-Simple"
{
    Properties
    {
        [HideInInspector] _FeatureFlags("Feature Flags", Int) = 0
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Diffuse", 2D) = "white" {}
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex SimpleVertex
            #pragma fragment SimpleFragment

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            Varyings SimpleVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                // SIMPLE 路径直接用顶点色乘材质色，RGB 完全由这个结果控制。
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 SimpleFragment(Varyings input) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                // 这条路径保留原 shader 的 SIMPLE 行为：
                // 1. 贴图只提供 alpha 轮廓。
                // 2. 最终 RGB 直接来自顶点色 / 材质色。
                mainTex.rgb = input.color.rgb;
                mainTex.a *= input.color.a;
                return mainTex;
            }
            ENDHLSL
        }
    }
}
