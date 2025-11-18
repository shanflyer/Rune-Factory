Shader "Sky"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}

        _WaterMaskTex("WaterMaskTex", 2D) ="black" {}
        _DepthTex("DepthTex", 2D) ="gray"{} 

        _topColor("topColor", Color) = (1,1,1,1)
        _bottomColor("bottomColor", Color) = (1,1,1,1)
        _halfValue("halfValue",float)=0.5

        _WaterNormalMap("WaterNormalMap", 2D) = "bump" {} 
        [Toggle(WATER)] _Water("Water",int)=0
        [Toggle(MAINTEXCOL)] _MainTexCol("MainTexCol",int)=0
        //水面颜色
        [HDR]waterColor("waterColor", Color) = (0,0.5,0.5,0.5)
        //初始透明
        _WaterZero("_WaterZero", Range(0,1)) = 0
        //水低调整
        _WaterBottom("_WaterBottom", Range(0,4)) = 1
        //水面高度
        _WaterHigh("_WaterHigh", Range(0,1)) = 0 
        //波纹亮度补偿
        waterValue("waterValue", Range(0, 0.4)) = 0.2 
        //噪声纹理系数
        waterNoiseScale("waterNoiseScale", Range(0, 300)) = 0  
        
        //噪声运动方向角1
        _WaveAngle0("_WaveAngle0",  Range(-180, 180)) = 0
        //噪声运动速度1
        _WaveSpeed0("_WaveSpeed0",  Range(0, 0.2)) = 0
        //噪声碎片大小1
        WaveScale0("WaveScale0", Vector) = (1, 1, 0, 0)

        //噪声运动方向角2
        _WaveAngle1("_WaveAngle1",  Range(-180, 180)) = 0
        //噪声运动速度2
        _WaveSpeed1("_WaveSpeed1",  Range(0, 0.2)) = 0
        //噪声碎片大小2
        WaveScale1("WaveScale1", Vector) = (1, 1, 0, 0) 

        

        [Title(water,Edge)] 
        //边缘颜色
        [HDR]EdgeColor("EdgeColor", Color) = (0.990566, 0.9765486, 0.9765486, 0)
        //边缘宽度
        EdgeValue("EdgeValue",  Range(0, 0.2))=0.1
        //边缘速度
        _EdgeWaveSpeed("EdgeWaveSpeed",Range(0,4))=0
        //边缘偏移
        _EdgeWaveOffset("EdgeWaveOffset",Range(0,0.5))=0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest LEqual

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
         #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"

         half4 _SkyTopColor;
         half4 _SkyBottomColor; 
         half _SkyHalfValue;
         half4 _SunColor;
         half _CloudValue;
        CBUFFER_START(UnityPerMaterial)
            int _Water;
            half4 waterColor;
            half _WaterZero;
            half _WaterBottom;   
            half _WaveAngle0;
            half _WaveSpeed0;
            half _WaveAngle1;
            half _WaveSpeed1;  
            half WaveColorValue;   
            half waterNoiseScale;
            half waterValue;   
            half2 WaveScale0;
            half2 WaveScale1; 
            half4 EdgeColor;
            half EdgeValue;  
            half _WaterHigh;  

            half _EdgeWaveSpeed;
            half _EdgeWaveOffset;
            
        CBUFFER_END  
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_WaterMaskTex);
        SAMPLER(sampler_WaterMaskTex);
        TEXTURE2D(_WaterNormalMap);
        SAMPLER(sampler_WaterNormalMap);

        TEXTURE2D(_MirrorTex);
        SAMPLER(sampler_MirrorTex);  
        TEXTURE2D(_DepthTex);
        SAMPLER(sampler_DepthTex); 
        ENDHLSL

        Pass
        { 
            //Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
       
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma shader_feature_local _ WATER
            #pragma shader_feature_local _ MAINTEXCOL
 

            struct Attributes
            {
                float3 positionOS   : POSITION; 
                float2 uv           : TEXCOORD0; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION; 
                float2  uv          : TEXCOORD0;
                float4   lightingUV  : TEXCOORD1; 
                float3  worldPos : TEXCOORD4;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };


            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.lightingUV = ComputeScreenPos(o.positionCS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;  
                return o;
            }

            float3 WaterFragment(float2 uv,float2 screenUV,float4 _MainTexColor)
            { 
                float2 sunUV=screenUV;
               
                half edgeOffsetValue=_SinTime.w*_EdgeWaveSpeed; 
                edgeOffsetValue=abs(edgeOffsetValue); 
                 edgeOffsetValue=clamp(edgeOffsetValue,0,1);
                _WaterHigh=_WaterHigh+_EdgeWaveOffset*edgeOffsetValue;
                 //return _EdgeWaveOffset*edgeOffsetValue;

                
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
               // float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
               // screenUV+=offsetUv;
                
                //波纹1
                float angle0=radians(_WaveAngle0);//转换角度为弧度
                float2 waveValue0=float2(cos(angle0),sin(angle0))*_WaveSpeed0;  

                float2 _WaveT0=(_TimeParameters.x.xx)*waveValue0; 
                float2 _TilingAndOffset0=uv*WaveScale0+_WaveT0;
                float4 _WaveCol0 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset0); 
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);	
                //波纹2
                float angle1=radians(_WaveAngle1);
                float2 waveValue1=float2(cos(angle1),sin(angle1))*_WaveSpeed1;  
                float2 _WaveT2=(_TimeParameters.x.xx)*waveValue1;				
                float2 _TilingAndOffset1=uv*WaveScale1+_WaveT2; 
                float4 _WaveCol1= SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, _TilingAndOffset1);
                _WaveCol1.rgb = UnpackNormal(_WaveCol1);
                
                
                //波纹叠加
                float3 _endWave=_WaveCol0.xyz +_WaveCol1.xyz;   
                //波纹r、g叠加
                float waveBlendCol=_endWave[0]+_endWave[1]; 
                waveBlendCol=clamp(waveBlendCol,0,1); 
                waveBlendCol*=waterValue;
                //噪声
                float _waterNoise;
                Unity_SimpleNoise_float(uv.xy, waterNoiseScale, _waterNoise);  
                waveBlendCol*=_waterNoise;
                
 
                float3 endWaveColor=waveBlendCol;
                endWaveColor=clamp(endWaveColor,0,1); 
                float endWaveColorValue=endWaveColor.x; 
                endWaveColorValue=clamp(endWaveColorValue,0,1); 

                float2 halfStep=1-step(0.5,screenUV); 

                float water_valueX=endWaveColor.r;
				float water_valueY=endWaveColor.g;
                Unity_Remap_float(water_valueX,float2(0,1),float2(-0,0.1),water_valueX);
				Unity_Remap_float(water_valueY,float2(0,1),float2(-0.02,0.02),water_valueY);
 

                //return float4(halfValue.xxx,1); 
                
                 
                sunUV=float2(sunUV.x+water_valueX*halfStep.x-water_valueX*(1-halfStep.x),
                                    sunUV.y-water_valueY);           
                
                float3 MirrorTexColor= SAMPLE_TEXTURE2D(_MirrorTex, sampler_MirrorTex, sunUV).xyz;  
                float MirrorValue=(MirrorTexColor.x+MirrorTexColor.y+MirrorTexColor.z)/3;
                
                 Unity_Remap_float(endWaveColorValue,float2(0,0.15),float2(0.06,1),endWaveColorValue);
                  
                 MirrorTexColor=MirrorTexColor*endWaveColorValue; 


                //主颜色
                float3 _MainColor=waterColor.xyz*waterColor.a;	 
                _MainColor+=(1-waterColor.a)*_MainTexColor.xyz;
                //_MainColor.xyz*=_WaterMask.r;

                 endWaveColor=endWaveColor.xyz*(1-MirrorValue)*_SunColor.xyz+MirrorValue*MirrorTexColor;

                float3 outWater=endWaveColor+_MainColor; 
                outWater=clamp(outWater,0,1);    
                outWater=outWater+waterColor.xyz*waterColor.a; 
                outWater*=1-0.25*_CloudValue;
                //return outWater;

               
                return outWater;
            }
 

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                float4 result=float4(1,1,1,1);
                float stepMask=1;
                #if MAINTEXCOL
                 float3 _WaterMask= SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex,i.uv).xyz;  
                 stepMask=1-step(_WaterMask.r,0); 
                 result= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);  
                #else 
                float colorValue0=0;
                Unity_Remap_float(i.uv.y,float2(0,_SkyHalfValue),float2(0,0.5),colorValue0);
                float colorValue1=0;
                Unity_Remap_float(i.uv.y,float2(_SkyHalfValue,1),float2(0.5,1),colorValue1);
                float setpValue=step(_SkyHalfValue,i.uv.y);

                float value=colorValue0*(1-setpValue)+colorValue1*setpValue; 
                
                result.xyz=_SkyBottomColor.xyz+(_SkyTopColor.xyz-_SkyBottomColor.xyz)*value;
                #endif 
               
                 #if WATER
                 float2 lightingUV=i.lightingUV.xy/i.lightingUV.w;
                 lightingUV=UnityStereoTransformScreenSpaceTex(lightingUV);
                 float3 waterColor=WaterFragment(i.uv,lightingUV,result);
                 result.xyz=waterColor*stepMask+(1-stepMask)*result.xyz;
                 #endif
                 

                return result;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "ObjDepth" "Queue"="Transparent" "RenderType"="Transparent"} 
            HLSLPROGRAM 

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment  

            struct Attributes
            {
                float3 positionOS   : POSITION; 
                float2 uv           : TEXCOORD0; 
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION; 
                float2  uv              : TEXCOORD0;  
            };
            
            

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0; 
 
                o.positionCS = TransformObjectToHClip(attributes.positionOS); 
                o.uv = attributes.uv; 
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv); 
                 
                mainTex.x=0;
                mainTex.y=1;
                mainTex.z=1;

                return mainTex;
                
            }
            ENDHLSL
        }

         
    }
         
    Fallback "Sprites/Default"
}
