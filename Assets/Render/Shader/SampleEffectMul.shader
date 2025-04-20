Shader "SampleEffectMul"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise ("_Noise", 2D) = "white" {}
        _HightOffset("HightOffset",float)=0
        _SpeedMainTexUVNoiseZW("_SpeedMainTexUVNoiseZW",vector)=(0,0,0,0)
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
           float4 _SpeedMainTexUVNoiseZW;
           half _HightOffset;
           CBUFFER_END 

           TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex); 
            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise); 
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
                float2 noiseUv: TEXCOORD1; 
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color=v.color*_Color;
                o.uv = v.uv+_SpeedMainTexUVNoiseZW.xy*_TimeParameters.x;
                o.noiseUv = v.uv+_SpeedMainTexUVNoiseZW.zw*_TimeParameters.x;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
                float4 noiseCol=SAMPLE_TEXTURE2D(_Noise,sampler_Noise,i.noiseUv);
                col=col*noiseCol;
                return col;
            }
             ENDHLSL
        }
        Pass
        {
            Tags { "LightMode" = "ObjDepth" } 
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
                float2 noiseUv: TEXCOORD1; 
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color=v.color*_Color;
                o.uv = v.uv+_SpeedMainTexUVNoiseZW.xy*_TimeParameters.x;
                o.noiseUv = v.uv+_SpeedMainTexUVNoiseZW.zw*_TimeParameters.x;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
                float4 noiseCol=SAMPLE_TEXTURE2D(_Noise,sampler_Noise,i.noiseUv);
                col=col*noiseCol;
                 col.xyz=0.25;
                return col;
            }
             ENDHLSL
        }
        Pass
        {
            Tags { "LightMode" = "Mirror" }
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
                float2 noiseUv: TEXCOORD1; 
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos=TransformObjectToWorld(v.vertex);
                float offsetY=worldPos.y-_WorldSpaceCameraPos.y;
                worldPos.y=-offsetY*0.75+_WorldSpaceCameraPos.y+2*_HightOffset;

                o.vertex = TransformWorldToHClip(worldPos);
                o.color=v.color*_Color;
                o.uv = v.uv+_SpeedMainTexUVNoiseZW.xy*_TimeParameters.x;
                o.noiseUv = v.uv+_SpeedMainTexUVNoiseZW.zw*_TimeParameters.x;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
                float4 noiseCol=SAMPLE_TEXTURE2D(_Noise,sampler_Noise,i.noiseUv);
                col=col*noiseCol;
                return col;
            }
             ENDHLSL
        }
    }
}
