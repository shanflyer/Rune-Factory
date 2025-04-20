Shader "SampleEffectMulCloud"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise ("_Noise", 2D) = "white" {}
        _HightOffset("HightOffset",float)=0
        _ColSlipValue("ColSlipValue",Range(0,1))=0.4
        _BlendValue("BlendValue",Range(0,1))=0.4
        _SunSize("_SunSize",Range(0,1))=0.4
        _SunMul("SunMu",Range(0,1))=1
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
        #include "Assets/Render/Shader/UnityAction.cginc"
            half4 _SunColor;
            half4 _SkyBottomColor; 
            half _SkyHalfValue;
            half4 _SkyTopColor;
            half3 _SunPos;
            float _DampValue;
            half4 _DirectionColor;
           CBUFFER_START(UnityPerMaterial)
           half4 _Color;
           float4 _SpeedMainTexUVNoiseZW;
           half _HightOffset;
           half _ColSlipValue;
           half _BlendValue;
           half _SunSize;
           half _SunMul;
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
                float2 cloudColor:TEXCOORD2;
                float4 SunScreenPos: TEXCOORD3;
                float4 ScreenUV: TEXCOORD4;
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color=v.color*_Color;
                o.uv = v.uv+_SpeedMainTexUVNoiseZW.xy*_TimeParameters.x;
                o.noiseUv = v.uv+_SpeedMainTexUVNoiseZW.zw*_TimeParameters.x;
                o.cloudColor.x=(_SkyTopColor.x+_SkyTopColor.b+_SkyTopColor.z)/3;
                o.cloudColor.y=(_SkyBottomColor.x+_SkyBottomColor.b+_SkyBottomColor.z)/3;
               
                half4 SunCs=TransformWorldToHClip(_SunPos);
                o.SunScreenPos =ComputeScreenPos(SunCs); 
                o.ScreenUV=ComputeScreenPos(o.vertex); 
               return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
                float4 noiseCol=SAMPLE_TEXTURE2D(_Noise,sampler_Noise,i.noiseUv); 
                col*=noiseCol;
                float d_v=(_DirectionColor.x+_DirectionColor.y+_DirectionColor.z)*0.33;
                 d_v=clamp(d_v,0,1);

                float colValue=(col.x+col.y+col.z)/3;
                float value=step(_ColSlipValue,colValue); 

                float3 cloudColorT=i.cloudColor.x*_SunColor.xyz;
                float3 cloudColorB=i.cloudColor.yyy;

                //col.xyz*=cloudColorB;
                
                float3 cloudColor=cloudColorB.xyz+(cloudColorT.xyz-cloudColorB.xyz)*value; 

                col.xyz=col.xyz*(1-_BlendValue)+cloudColor*_BlendValue;


                float2 sunUV=i.SunScreenPos.xy/i.SunScreenPos.w;
                sunUV=UnityStereoTransformScreenSpaceTex(sunUV);

               float2 ScreenUV=i.ScreenUV.xy/i.ScreenUV.w;
               ScreenUV=UnityStereoTransformScreenSpaceTex(ScreenUV);
               float screenScale=_ScreenParams.x/_ScreenParams.y;
               ScreenUV.y/=screenScale;
               sunUV.y/=screenScale;

               float SunDistance=_SunSize-distance(sunUV,ScreenUV);
               SunDistance=clamp(SunDistance,0,1);
                
              
               col.xyz+=SunDistance*_SunColor.xyz*_SunMul;
               col.xyz*=1-0.5*_DampValue;

                col.xyz*=d_v;

                return col;
            }
             ENDHLSL
        }
        /*Pass
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
                float4 ScreenUV: TEXCOORD4;
                float4 color:COLOR;
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color=v.color*_Color;
                o.uv = v.uv+_SpeedMainTexUVNoiseZW.xy*_TimeParameters.x;
                o.noiseUv = v.uv+_SpeedMainTexUVNoiseZW.zw*_TimeParameters.x;
                o.ScreenUV=ComputeScreenPos(o.vertex); 
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
                float4 noiseCol=SAMPLE_TEXTURE2D(_Noise,sampler_Noise,i.noiseUv);
                 col=col*noiseCol;
                 
                
                float2 ScreenUV=i.ScreenUV.xy/i.ScreenUV.w;
                ScreenUV=UnityStereoTransformScreenSpaceTex(ScreenUV); 
                col.xyz=ScreenUV.y;
                return col;
            }
             ENDHLSL
        }*/
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
                float2 cloudColor:TEXCOORD2;
                float4 SunScreenPos: TEXCOORD3;
                float4 ScreenUV: TEXCOORD4;
            };
 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.color=v.color*_Color;
                o.uv = v.uv+_SpeedMainTexUVNoiseZW.xy*_TimeParameters.x;
                o.noiseUv = v.uv+_SpeedMainTexUVNoiseZW.zw*_TimeParameters.x;
                o.cloudColor.x=(_SkyTopColor.x+_SkyTopColor.b+_SkyTopColor.z)/3;
                o.cloudColor.y=(_SkyBottomColor.x+_SkyBottomColor.b+_SkyBottomColor.z)/3;
               
                half4 SunCs=TransformWorldToHClip(_SunPos);
                o.SunScreenPos =ComputeScreenPos(SunCs); 
                o.ScreenUV=ComputeScreenPos(o.vertex); 
               return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*i.color;
                float4 noiseCol=SAMPLE_TEXTURE2D(_Noise,sampler_Noise,i.noiseUv); 
                col*=noiseCol;
                float d_v=(_DirectionColor.x+_DirectionColor.y+_DirectionColor.z)*0.33;
                 d_v=clamp(d_v,0,1);

                float colValue=(col.x+col.y+col.z)/3;
                float value=step(_ColSlipValue,colValue); 

                float3 cloudColorT=i.cloudColor.x*_SunColor.xyz;
                float3 cloudColorB=i.cloudColor.yyy;

                //col.xyz*=cloudColorB;
                
                float3 cloudColor=cloudColorB.xyz+(cloudColorT.xyz-cloudColorB.xyz)*value; 

                col.xyz=col.xyz*(1-_BlendValue)+cloudColor*_BlendValue;


                float2 sunUV=i.SunScreenPos.xy/i.SunScreenPos.w;
                sunUV=UnityStereoTransformScreenSpaceTex(sunUV);

               float2 ScreenUV=i.ScreenUV.xy/i.ScreenUV.w;
               ScreenUV=UnityStereoTransformScreenSpaceTex(ScreenUV);
               float screenScale=_ScreenParams.x/_ScreenParams.y;
               ScreenUV.y/=screenScale;
               sunUV.y/=screenScale;

               float SunDistance=_SunSize-distance(sunUV,ScreenUV);
               SunDistance=clamp(SunDistance,0,1);
                
              
               col.xyz+=SunDistance*_SunColor.xyz*_SunMul;
               col.xyz*=1-0.5*_DampValue;

                col.xyz*=d_v;

                return col;
            }
             ENDHLSL
        }
    }
}
