Shader "MyShadowMask"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _DirValue("Dir",float)=1
        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        _Color ("Tint", Color) = (1,1,1,1) 
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            Tags {"LightMode" = "Universal2D" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl" 
            #include "Assets/Render/Shader/UnityAction.cginc"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment
            

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;  
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

            half2 LightDirection;
            half4 GlobalColor;   

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            CBUFFER_START( UnityPerMaterial )
                half4 _MainTex_ST;
                half4 _Color;
                half _DirValue;
                
            CBUFFER_END

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;  
                float4x4 m_Data=UNITY_MATRIX_M;

                m_Data[0][0]=UNITY_MATRIX_M[0][0]*LightDirection.y;
                float3 worldPos=mul(m_Data, float4(attributes.positionOS, 1.0));
                
                o.positionCS = TransformWorldToHClip(worldPos);
                // o.positionCS = TransformObjectToHClip(attributes.positionOS);
                float lightAngleValue=sin(LightDirection.x);
                o.uv = attributes.uv; 
                o.uv2.x= abs(lightAngleValue)*UNITY_MATRIX_M[2][2]; 
                o.uv2.y=step(0,lightAngleValue*_DirValue);

                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                const half4 main =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv); 
                float aplhaValue=0;
                Unity_Remap_float(main.r,float2(i.uv2.x-0.2,i.uv2.x),float2(0,1),aplhaValue);
                
                // 
                aplhaValue=clamp(aplhaValue,0,1);
                aplhaValue=1-aplhaValue;
                return float4(0,0,0,aplhaValue*0.4*i.uv2.y);
            }
            ENDHLSL
        }
    } 
}
