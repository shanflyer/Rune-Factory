Shader "SampleEffectMul"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite off
		ZTest LEqual
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

           CBUFFER_START(UnityPerMaterial)
           half4 _Color;
           CBUFFER_END 

           TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex); 
          ENDHLSL

        Pass
        {
             HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag 

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; 
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color=v.color*_Color;
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
                return col;
            }
             ENDHLSL
        }
    }
}
