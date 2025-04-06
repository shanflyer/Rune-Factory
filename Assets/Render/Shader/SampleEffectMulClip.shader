Shader "SampleEffectMulClip"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise ("_Noise", 2D) = "white" {}
        _SpeedMainTexUVNoiseZW("_SpeedMainTexUVNoiseZW",vector)=(0,0,0,0)
        [HDR]_Color("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite off
		ZTest LEqual
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

           CBUFFER_START(UnityPerMaterial)
           half4 _Color;
           float4 _SpeedMainTexUVNoiseZW;
           CBUFFER_END 

           TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex); 
            TEXTURE2D(_Noise);
            SAMPLER(sampler_Noise); 
            
            TEXTURE2D(_LightingTex);
            SAMPLER(sampler_LightingTex);
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
                float3 redColor: TEXCOORD1;
                float3 greenColor: TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; 
                float2 noiseUv: TEXCOORD3; 
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
                float3 redColor: TEXCOORD1;
                float3 greenColor: TEXCOORD2;
                half4 screenUV:TEXCOORD4; 
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color=v.color;
                o.uv = v.uv+_SpeedMainTexUVNoiseZW.xy*_TimeParameters.x;
                o.noiseUv = v.uv+_SpeedMainTexUVNoiseZW.zw*_TimeParameters.x;
                o.redColor=v.redColor;
                o.greenColor=v.greenColor;

                o.screenUV= ComputeScreenPos(o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                float4 noiseCol=SAMPLE_TEXTURE2D(_Noise,sampler_Noise,i.noiseUv);
                col=col*noiseCol;
                float3 redColor=col.r*i.redColor;
                float3 greenColor=col.g*i.greenColor;
                float3 blueColor=col.b*i.color.xyz;
            
                float4 result=float4(redColor+greenColor+blueColor,col.a*i.color.a);
                result*=_Color;

                float2 screenUV=i.screenUV.xy/i.screenUV.w;
                screenUV=UnityStereoTransformScreenSpaceTex(screenUV);
                half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,screenUV);
                lightCol.xyz*=4;  
                result.xyz*=lightCol.xyz;
            

                return result;
            }
             ENDHLSL
        }
        
    }
}
