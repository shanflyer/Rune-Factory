Shader "MyShadow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        _Color ("Tint", Color) = (1,1,1,1) 
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        BlendOp Max 
        Cull Off
        ZWrite Off 
        Pass
        {
            Tags { "LightMode" = "Shadow" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl" 

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment
            

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0; 
                float2 uv2: TEXCOORD1; 
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float4  color           : COLOR;
                float2  uv              : TEXCOORD0; 
                float2 uv2: TEXCOORD1; 
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            CBUFFER_START( UnityPerMaterial )
                half4 _MainTex_ST;
                half4 _Color;
                half2 LightDirection;
            half4 GlobalColor;    
            CBUFFER_END

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0; 

                float4x4 m_Data=UNITY_MATRIX_M;
                float lightAngleValue=sin(LightDirection.x);
                m_Data[0][0]+=m_Data[0][0]*abs(lightAngleValue)*0.5*LightDirection.y;

                float3 worldPos=mul(m_Data, float4(attributes.positionOS, 1.0));
                float length=attributes.uv2.x*LightDirection.y;
                length+=  length*abs(lightAngleValue)*0.5*LightDirection.y;

                float2 offset=length.xx*float2(sin(LightDirection.x),cos(LightDirection.x));
                worldPos.xy+=offset*attributes.uv2.y;
                
                o.positionCS = TransformWorldToHClip(worldPos);
 
                o.uv = attributes.uv; 
                o.uv2=attributes.uv2;
                o.color=attributes.color;
                o.color.xyz*=1-o.uv.x;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    } 
}
