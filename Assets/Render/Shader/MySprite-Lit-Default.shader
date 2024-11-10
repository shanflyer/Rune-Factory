Shader "MySprite-Lit-Default"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
       // _MaskTex("Mask", 2D) = "white" {}
        _MoveMask("_MoveMask", 2D) = "black" {}
        _SnowTex("_SnowTex", 2D) = "black" {}
        _ZWrite("ZWrite", Float) = 0

        [Toggle]_Character("Character",int)=0

        _WaterNormalMap("WaterNormalMap", 2D) = "bump" {} 
        _NormalMap("Normal Map", 2D) = "bump" {}
        _WaterMaskTex("WaterMaskTex", 2D) ="black"{}
        _DepthTex("DepthTex", 2D) ="gray"{} 
        _WetValue("WetValue",Range(0,1))=0
        [Toggle]_shadowStep("ShadowStep",int)=0
        _LightBlend("LightBlend",float)=1 
        [Toggle]_BackBlend("BackBlend",int)=1
        [Toggle]_BlendVertexColor("BlendVertexColor",int)=0

        _Water("Water",int)=0 

        _WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
        _WindScroll("Wind Scroll", Range( 0 , 3)) = 0.1
		_WindJitter("Wind Jitter", Range( 0 , 3)) = 0.1
        _WindNoiseValue("WindNoiseValue",Range(0,1))=0 
        [Toggle]_MoveSelfUV("_MoveSelfUV",int)=0
        [Toggle]_GrassBlend("_GrassBlend",int)=0
        

        _PlantSpringColor("_PlantSpringColor",color)=(0,0,0)
        _PlantSpringColor1("_PlantSpringColor1",color)=(0,0,0)
        _PlantAutumnColor0("_PlantAutumnColor0",color)=(0,0,0)
        _PlantAutumnColor1("_PlantAutumnColor1",color)=(1,1,1)
        _PlantWinterColor("_PlantWinterColor",color)=(0,0,0)
        _PlantWinterColor1("_PlantWinterColor1",color)=(0,0,0)
        //_SeasonValue("_SeasonValue",Range(0,4))=0
        _PlantAutumnNoiseScale("_PlantAutumnNoiseScale",float)=1
        [Toggle]seasonColorBlend("seasonColorBlend",int)=0

        [Toggle]_DampBlend("_DampBlend",int)=0 
        [Toggle]_Damp("_Damp",int)=0
        [Toggle]_SnowBlend("_SnowBlend",int)=1
        
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

        
       
        //[Toggle]_Tree3D("_Tree3D",int)=0
         _BlendColor("BlendColor",Color)=(0,1,1,1)
        _BlendValue("BlendValue",Range(0,1))=0
		_BlendRmapMin("BlendRmapMin",Range(0,1))=0
        _SnowRange("SnowRange",vector)=(0,1,0,1) 
        _SnowColor("SnowColor",color)=(1,1,1,1)
        _ScaleValue("ScaleValue", Range(0 , 2)) = 0.5 
         _ClipValue("ClipValue",Range(0,2))=0.5
  
        

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {  "Queue"="Transparent" "RenderType"="Transparent"}

         Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
         Cull Off
         ZWrite Off

        HLSLINCLUDE
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
         #include "Assets/Render/Shader/UnityAction.cginc"

            
            Texture2D _MainTex;
            SamplerState sampler_MainTex;  
            Texture2D _DepthTex;
            Texture2D _NormalMap;
            Texture2D _MoveMask;
            //Texture2D _GrassTex;
            Texture2D _SnowTex;
            //exture2D _WaterNormalMap; 
             TEXTURE2D(_WaterMaskTex);
             SAMPLER(sampler_WaterMaskTex);
            TEXTURE2D(_WindNoiseTexture);
            SAMPLER(sampler_WindNoiseTexture);

            TEXTURE2D(_LightingTex);
            SAMPLER(sampler_LightingTex);
           
            TEXTURE2D(_GrassTex);
            SAMPLER(sampler_GrassTex); 
            
            TEXTURE2D(_MirrorTex);
            SAMPLER(sampler_MirrorTex); 
            TEXTURE2D(_ObjDepthTex);
            SAMPLER(sampler_ObjDepthTex); 

            TEXTURE2D(_ShadowTex);
            SAMPLER(sampler_ShadowTex);
            
            TEXTURE2D(_BackMaskTex);
            SAMPLER(sampler_BackMaskTex);

            TEXTURE2D(_WaterNormalMap);
            SAMPLER(sampler_WaterNormalMap); 
  

         half4 GlobalColor; 
         half2 LightDirection;
         half _ShadowValue;
         int _backColor;

        float _SnowValue;
        
         float _DampValue;
        float _DampNoise;
        float _HighLighStep;
        float4 _DampColor;
        float4 _DampWaterColor;
        float4 _HightLightColor;
        float _HighLightNoise;

        float4 _GlobalColor;
        half4 _SunColor;
        float _SeasonValue;
        float _CloudValue;
        float4 _WindDir;
        float4 _NoiseSet0;
        float4 _NoiseSet1; 
        float _WindValue; 
        CBUFFER_START(UnityPerMaterial)
            int seasonColorBlend;
            int _Character;
			float3 _BlendColor;
			float _BlendValue;
			float _BlendRmapMin;
            
            float _ScaleValue;  
            float _ClipValue;
            int _Tree3D;
            float4 _SnowRange;
             half4 _SnowColor;
           
            int _DampBlend;
            int _Damp;
            int _SnowBlend;
            int _GrassBlend; 

            half3 _PlantSpringColor1;
            half3 _PlantSpringColor;
            half3 _PlantAutumnColor0;
            half3 _PlantAutumnColor1;
            half3 _PlantWinterColor;
            half3 _PlantWinterColor1; 
            float _PlantAutumnNoiseScale;  
             
            half4 _Color;
            half _WetValue;
            int _shadowStep;
            int _Water;

            float _LightBlend;
            int _BackBlend;
            int _BlendVertexColor;

            float _WindJitter;
			float _WindScroll;
            float _WindNoiseValue;
            int _MoveSelfUV;
            
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
        float My_SimpleNoise_float(float2 uv,float scale)
        {
                half4 col=SAMPLE_TEXTURE2D(_WindNoiseTexture,sampler_WindNoiseTexture,uv/scale);
                return col.r;
        }
         
        ENDHLSL 

         Pass
        {
          Tags { "LightMode" = "Universal2D" "Queue"="Transparent" "RenderType"="Transparent"}
             
            HLSLPROGRAM
             

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
 
            
 
             
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float3 normalOS : NORMAL;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                half2   lightingUV  : TEXCOORD1; 
                float4  worldPos : TEXCOORD4;
                half2   fixScreenUV: TEXCOORD3;
                float3 normal:NORMAL;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            float3 WaterFragment(float2 uv,float2 screenUV,float4 _MainTexColor)
            {
                float2 mirrorUV=screenUV; 

                float3 _WaterMask=SAMPLE_TEXTURE2D(_WaterMaskTex,sampler_WaterMaskTex, uv.xy).xyz;
                //水域范围
                float stepMask=step(0.06,_WaterMask.r); 

                half edgeOffsetValue=_SinTime.w*_EdgeWaveSpeed; 
                edgeOffsetValue=abs(edgeOffsetValue); 
                 edgeOffsetValue=clamp(edgeOffsetValue,0,1);
                _WaterHigh=_WaterHigh+_EdgeWaveOffset*edgeOffsetValue;
                 //return _EdgeWaveOffset*edgeOffsetValue;

                
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;
                
                //波纹1
                float angle0=radians(_WaveAngle0);//转换角度为弧度
                float2 waveValue0=float2(cos(angle0),sin(angle0))*_WaveSpeed0;  

                float2 _WaveT0=(_TimeParameters.x.xx)*waveValue0; 
  
                float2 _TilingAndOffset0=screenUV*WaveScale0+_WaveT0;
                float4 _WaveCol0 =   SAMPLE_TEXTURE2D( _WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset0); 
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);	
                //波纹2
                float angle1=radians(_WaveAngle1);
                float2 waveValue1=float2(cos(angle1),sin(angle1))*_WaveSpeed1;  
                float2 _WaveT2=(_TimeParameters.x.xx)*waveValue1;				
                float2 _TilingAndOffset1=screenUV*WaveScale1+_WaveT2; 
                float4 _WaveCol1=  SAMPLE_TEXTURE2D( _WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset1); 
                _WaveCol1.rgb = UnpackNormal(_WaveCol1);
               
                
                //波纹叠加
                float3 _endWave=_WaveCol0.xyz +_WaveCol1.xyz;   
                //波纹r、g叠加
                float waveBlendCol=_endWave[0]+_endWave[1]; 
                waveBlendCol=clamp(waveBlendCol,0,1); 
                waveBlendCol*=waterValue;
                //噪声
                float _waterNoise;
                Unity_SimpleNoise_float(screenUV.xy, waterNoiseScale, _waterNoise);  
                waveBlendCol*=_waterNoise;
                

                //波纹与边缘混合 
                //映射水面深度
                Unity_Remap_float(_WaterMask.r,float2(0,1),float2(_WaterZero,_WaterBottom),_WaterMask.r);
                _WaterMask.r=clamp(_WaterMask.r,0,1);
                

                float _WaterMask1=step(_WaterHigh,_WaterMask.r);	 
                float _WaterMask2=step(_WaterHigh+EdgeValue,_WaterMask.r); 
                stepMask*=_WaterMask1;

                float _EdgeMaskValue=_WaterMask.r;
                Unity_Remap_float(_EdgeMaskValue,float2(_WaterHigh,_WaterHigh+EdgeValue),float2(0,1),_EdgeMaskValue);
                
                float edge=(_WaterMask1-_WaterMask2)*_EdgeMaskValue; 
               

               
                //float3 edgeAddColor=edge*float3(0,1,1)*2; 
                float3 endWaveColor=edge*EdgeColor.xyz*waveBlendCol+waveBlendCol*_WaterMask1.rrr;
                endWaveColor=clamp(endWaveColor,0,1);
                
                half sunValue=(_SunColor.x+_SunColor.y+_SunColor.z)/3;

                //主颜色
                float3 _MainColor=waterColor.xyz*waterColor.a;	 
                _MainColor+=(1-waterColor.a)*_MainTexColor.xyz;
                _MainColor.xyz*=_WaterMask.r;

              endWaveColor=endWaveColor.xyz*_SunColor.xyz/sunValue;

                float3 outWater=endWaveColor+_MainColor; 
                outWater=clamp(outWater,0,1);   
                
                outWater=outWater+waterColor.xyz*waterColor.a;
               
               float water_valueX=outWater.r;
				float water_valueY=outWater.g;
                Unity_Remap_float(water_valueX,float2(0,1),float2(-0,0.1),water_valueX);
				Unity_Remap_float(water_valueY,float2(0,1),float2(-0.02,0.02),water_valueY);

                mirrorUV.x+=water_valueX;
                mirrorUV.y+=water_valueY;
 
                float3 MirrorTexColor= SAMPLE_TEXTURE2D(_MirrorTex, sampler_MirrorTex, mirrorUV).xyz;  
                float MirrorValue=(MirrorTexColor.x+MirrorTexColor.y+MirrorTexColor.z)/3;
                //return MirrorTexColor;

                outWater=outWater*(1-MirrorValue)+MirrorTexColor*MirrorValue;
                outWater=stepMask*outWater+_MainTexColor.xyz*(1-stepMask);
                return outWater;
            }

            float2 MoveUV(float2 uv,float2 screenUV,float SnowMove,out float2 offset)
            {
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;
                screenUV=_MoveSelfUV*uv+(1-_MoveSelfUV)*screenUV;

                float2 panner63 = _WindScroll * 0.3 * _TimeParameters.x + screenUV;
				float2 panner74 =_TimeParameters.x * _WindJitter * 0.5  + screenUV *2;

                float4 WindNoise0=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner63);
                WindNoise0=pow(abs(WindNoise0), 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner74);

                float4 moveValue=_MoveMask.Sample(sampler_MainTex,uv);

                float windValue=lerp(1,2,abs(_WindValue));
                //return float2(moveValue.x,moveValue.x);
                float value=moveValue.x*_WindNoiseValue*windValue;
                offset=WindNoise0.x*WindNoise1.x*value*SnowMove;
                int stepWind=step(0,_WindValue);
                offset.x=offset.x*stepWind-offset.x*(1-stepWind);

                return offset+uv;
            }

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            

           
            

             float3 DampColor(float3 col,float2 uv,float2 objUV)
            {
                float r=col.r*col.r;
                float g=col.g*col.g;
                float b=col.b*col.b;
                float3 _col=float3(r,g,b);
                float3 col1=_col;
                
                float _DampNoiseValue;	 

                 float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;  


				Unity_SimpleNoise_float(uv+_WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy,_DampNoise,_DampNoiseValue);
                float3 d=float3(_DampNoiseValue,_DampNoiseValue,_DampNoiseValue);    


                float3 water=float3(1-_DampNoiseValue,1-_DampNoiseValue,1-_DampNoiseValue);    
                float waterValue=clamp((_DampValue-0.5),0,0.5)/0.5;      

                 float _HighLightNoiseValue;	
				Unity_SimpleNoise_float(uv+_WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy,_HighLightNoise,_HighLightNoiseValue);
                float3 h=float3(_HighLightNoiseValue,_HighLightNoiseValue,_HighLightNoiseValue);

                

                h*=(1-_DampNoiseValue);       
                             
                float dValue=1-waterValue;
                d*=dValue;               
				d*=d; 
                d=clamp(d,0,1);

               
               
                water*=waterValue;
                water*=water;  
                water=clamp(water,0,1);
                //return water*_WaterColor;

                h*=waterValue;
                h*=step(_HighLighStep,h);
                h*=h;

                h=clamp(h,0,1);
                
                
                float c=(col.r+col.g+col.b)/3; 

                d*=_DampColor.xyz*c;
                water*=_DampWaterColor.xyz*c;
                h*=_HightLightColor.xyz*c; 

                _col+=d+water+h;  
                half4 normal =_NormalMap.Sample(sampler_MainTex,objUV);
                //half3 normalUnpacked = UnpackNormalRGBNoScale(normal);
                float gv=normal.z;
                //return normal.zzz;
                //float stepGv=step(0.5,gv);
                //gv=stepGv+(1-stepGv)*gv; 
                // return gv.xxx;
                Unity_Remap_float(gv,float2(0,1),float2(0.2,1),gv);

                float3 result=lerp(col,_col,clamp(_DampValue/0.5,0,1)*gv);
                //return _DampValue.xxx;
                 //result=lerp(col,result,_DampValue); 
               return result*(1-_Damp)+_col*_Damp; 
            }

            float3 BlendLightCol(float3 col,float2 screenUV)
            {
                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,screenUV);
                 col*=lightCol.xyz;
                 return col;
            }

             

            Varyings DefaultVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
              
                o.worldPos.xyz=TransformObjectToWorld(v.positionOS);
                //half3 worldPos=o.worldPos.xyz;
               // worldPos.z+=worldPos.y;
               // o.positionCS =TransformWorldToHClip(worldPos);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                float3 worldCS=o.worldPos.xyz;
                worldCS.y=UNITY_MATRIX_M._m13;
                //o.worldPos.w=o.worldPos.z;
                //
                
                half4 worldPosCs=TransformWorldToHClip(worldCS.xyz);
                half2 worldScreen=half2(ComputeScreenPos(worldPosCs / worldPosCs.w).xy);
                o.worldPos.zw=worldScreen;
               // half4 grassColor=  SAMPLE_TEXTURE2D_LOD(_GrassTex, sampler_GrassTex, worldScreen,0); 
               // float GrassColorValue=abs(grassColor.r-0.5)/0.5;
               // o.worldPos.w=GrassColorValue;

                int seasonColorBlend=_PlantSpringColor.x+_PlantSpringColor.y+_PlantSpringColor.z; 
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);


                half3 pos=TransformObjectToWorld(_WorldSpaceCameraPos.xyz);
                half4 carmeraPos=TransformWorldToHClip(pos); 

                o.fixScreenUV=o.lightingUV-half2(ComputeScreenPos(carmeraPos / carmeraPos.w).xy);

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }

            Varyings CombinedShapeLightVertex(Attributes v)
            { 
                return DefaultVertex(v);
            }
 

            half4 DefaultFragment(Varyings i) : SV_Target
            {
                float2 uv=i.uv;
                float2 offset;

                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.1,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;
 
                uv=MoveUV(uv,i.lightingUV,1-s_w,offset);
                //return half4(uv.xxx,1);
                half4 main =_MainTex.Sample(sampler_MainTex,uv); 
              //  return float4(main.xyz,main.a);

                const half4 grassTex=_MoveMask.Sample(sampler_MainTex,uv);
                float mainValue=(main.y);
               
                mainValue=clamp(mainValue,0,1);

                 float noiseValue;
                Unity_SimpleNoise_float(i.worldPos.xy,_PlantAutumnNoiseScale,noiseValue); 
                float noiseValue1;
                Unity_SimpleNoise_float(i.worldPos.xy,_PlantAutumnNoiseScale*2,noiseValue1); 
                
                int seasonColorBlend=_PlantSpringColor.x+_PlantSpringColor.y+_PlantSpringColor.z;
                
               //春季颜色
                int springBlend=1-step(1,_SeasonValue);
                float w_s=_SeasonValue;
                Unity_Remap_float(w_s,float2(0,0.15),float2(0,1),w_s); 
                int w_sBlend=1-step(0.5,w_s);
                float springValue=_SeasonValue;
                Unity_Remap_float(springValue,float2(0.15,0.5),float2(0,1),springValue);
                springValue=clamp(springValue,0,1);
                float3 winterColor=(_PlantWinterColor*noiseValue+_PlantWinterColor1*(1-noiseValue))*mainValue; 
                float3 springColor0=(_PlantWinterColor*noiseValue1+_PlantSpringColor*(1-noiseValue1))*mainValue; 
                float3 springColor=(_PlantSpringColor*noiseValue+_PlantSpringColor1*(1-noiseValue))*mainValue; 
                springColor=(winterColor*(1-w_s)+springColor0*w_s)*w_sBlend
                        +(1-w_sBlend)*(springColor0*(1-springValue)+springColor*springValue);
                
                //夏季颜色
                float s_s=_SeasonValue;
                Unity_Remap_float(s_s,float2(1,1.25),float2(0,1),s_s);
                s_s=clamp(s_s,0,1);
                float3 summerColor=springColor*(1-s_s)+main.xyz*s_s;

                //秋季颜色
                float s_a=_SeasonValue;
                Unity_Remap_float(s_a,float2(2,2.25),float2(0,1),s_a);
                s_a=clamp(s_a,0,1); 

                float3 AutumnColor0=(main.xyz*noiseValue1+_PlantAutumnColor0*(1-noiseValue1))*mainValue; 
                AutumnColor0=AutumnColor0*s_a+summerColor*(1-s_a);


                float a_a=_SeasonValue;
                Unity_Remap_float(a_a,float2(2.25,2.5),float2(0,1),a_a);
                a_a=clamp(a_a,0,1); 

                float3 AutumnColor=(_PlantAutumnColor0*noiseValue+_PlantAutumnColor1*(1-noiseValue))*mainValue; 
                AutumnColor=AutumnColor*a_a+AutumnColor0*(1-a_a);

            

                //冬季颜色
                float a_w=_SeasonValue;
                Unity_Remap_float(a_w,float2(2.85,3.15),float2(0,1),a_w);
                a_w=clamp(a_w,0,1); 
                float3 winterColor0=(_PlantAutumnColor0*noiseValue1+_PlantWinterColor1*(1-noiseValue1))*mainValue; 
                
                winterColor0=winterColor0*a_w+AutumnColor*(1-a_w);

                float w_w=_SeasonValue;
                Unity_Remap_float(w_w,float2(3.15,3.35),float2(0,1),w_w);
                w_w=clamp(w_w,0,1); 
                winterColor=winterColor*w_w+winterColor0*(1-w_w); 
                

                half _BlendValue=1-step(grassTex.g,0);

                int seasonStep=1-step(seasonColorBlend,0);
                main.xyz=main.xyz*(1-_BlendValue)*seasonStep+winterColor*_BlendValue*seasonStep+main.xyz*(1-seasonStep);    
                
                 //  return main;

                
                     
                half4 snow =_SnowTex.Sample(sampler_MainTex,uv);
                //return snow;
                half3 mainSnow=main.xyz*(1-snow.a)+snow.xyz*snow.a;
                half snowA=main.a*(1-snow.a)+snow.a;
                half snowValue= s_w*_SnowBlend;
                main=main*(1-snowValue)+half4(mainSnow.xyz,snowA)*snowValue;
         
                

                half4 _DepthColor =_DepthTex.Sample(sampler_MainTex,uv);
                half4 _GrassColor =SAMPLE_TEXTURE2D(_GrassTex,sampler_GrassTex,i.worldPos.zw);// _GrassTex.Sample(sampler_MainTex,i.worldPos.zw);
                float GrassColorValue=_GrassColor.r*_GrassColor.g*0.6+_GrassColor.g*0.25; 
                
                //return float4(GrassColorValue.xxx,1);
                
                float HightValue=step(GrassColorValue,_DepthColor.r)*step(0.01,_DepthColor.r);
                //return float4(HightValue.xxx,1);
                main.a=main.a*HightValue*_GrassBlend+(1-_GrassBlend)*main.a; 
                if(_GrassBlend==1)
                { 
                    half4 _DepthColor =_DepthTex.Sample(sampler_MainTex,uv);
                    half4 _GrassColor = _GrassTex.Sample(sampler_MainTex,i.worldPos.zw);
                    float GrassColorValue=_GrassColor.r*_GrassColor.g*0.6+_GrassColor.g*0.25; 
                    
                    //return float4(GrassColorValue.xxx,1);
                    
                    float HightValue=step(GrassColorValue,_DepthColor.r)*step(0.01,_DepthColor.r);
                    //return float4(HightValue.xxx,1);
                    main.a=main.a*HightValue; 
                }
                
 
                half4 result=main;
                
                float singleValue=(main.x+main.y+main.z)/3;
                float3 singleColor=main.xyz*(i.color.a)+singleValue.xxx*(1-i.color.a);
                float3 waterColor=main.xyz*i.color.xyz;
              
                waterColor.xyz=waterColor.xyz*(1-_BlendVertexColor)+singleColor*_BlendVertexColor; 
               
               /*
                if(_DampBlend>=1)
                {
                   waterColor=DampColor(waterColor,i.lightingUV,uv); 
                } 
                */
                // waterColor.xyz=BlendScreenCloudColor(waterColor.xyz,i.lightingUV);
                main.xyz=waterColor.xyz;
               if(_Water==1)
                {
                    waterColor=WaterFragment(uv,i.lightingUV,main);
                }
               return float4(waterColor.xyz,main.a);
 

                half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,i.lightingUV);
                lightCol.xyz*=4;

                result.xyz=waterColor.xyz;
                result.xyz=_LightBlend*result.xyz+(1-_LightBlend)*waterColor.xyz; 
                result.a=result.a*(1-_BlendVertexColor)*i.color.a+result.a*_BlendVertexColor; 
 
                half _light_value=(lightCol.x+lightCol.y+lightCol.z)/3;
                float globalValue=(_GlobalColor.x+_GlobalColor.y+_GlobalColor.z)/3;
                
                
                _light_value-=globalValue*_ShadowValue; 
                _light_value=clamp(_light_value,0,1);
                _light_value=(1-_light_value*0.5);

                

                
               // result.xyz=result.xyz*(1-snowValue.x)+snowValue;


                half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, i.lightingUV); 
                shadow.xyz*=_light_value;
                half3 shadowColor=GlobalColor.xyz*GlobalColor.a; 

                half3 shadowResult=shadowColor*result.xyz+result.xyz*(1-shadow.r);  
                result.xyz=result.xyz*(1-_shadowStep)+shadowResult*_shadowStep;     
                
                /*if(_BackBlend&&_backColor)
                {
                    half4 backColor=SAMPLE_TEXTURE2D(_BackMaskTex, sampler_BackMaskTex, i.lightingUV); 
                    half backColorValue=(backColor.r+backColor.g+backColor.b)/3;
                    result.xyz=half3(0,0.5,0.8)*backColorValue+result.xyz*(1-backColorValue);

                }*/
                    
                //result.xyz=waterColor.xyz; 
                //clip(result.a-0.01);

                return result;
            }
          

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {  
                 return DefaultFragment(i); 
            } 
            ENDHLSL
        }

         

        Pass
        {
          Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}
             
            HLSLPROGRAM
             

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
             #pragma multi_compile _ SKINNED_SPRITE 
            
 
             
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float3 normalOS : NORMAL;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                half2   lightingUV  : TEXCOORD1; 
                float4  worldPos : TEXCOORD4;
                half2   fixScreenUV: TEXCOORD3;
                float3 normal:NORMAL;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
             float3 WaterFragment(float2 uv,float2 screenUV,float4 _MainTexColor)
            {
                float2 mirrorUV=screenUV; 

                float3 _WaterMask=SAMPLE_TEXTURE2D(_WaterMaskTex,sampler_WaterMaskTex, uv.xy).xyz;
                //水域范围
                float stepMask=step(0.06,_WaterMask.r); 

                half edgeOffsetValue=_SinTime.w*_EdgeWaveSpeed; 
                edgeOffsetValue=abs(edgeOffsetValue); 
                 edgeOffsetValue=clamp(edgeOffsetValue,0,1);
                _WaterHigh=_WaterHigh+_EdgeWaveOffset*edgeOffsetValue;
                 //return _EdgeWaveOffset*edgeOffsetValue;

                
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;
                
                //波纹1
                float angle0=radians(_WaveAngle0);//转换角度为弧度
                float2 waveValue0=float2(cos(angle0),sin(angle0))*_WaveSpeed0;  

                float2 _WaveT0=(_TimeParameters.x.xx)*waveValue0; 
  
                float2 _TilingAndOffset0=screenUV*WaveScale0+_WaveT0;
                float4 _WaveCol0 =   SAMPLE_TEXTURE2D( _WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset0); 
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);	
                //波纹2
                float angle1=radians(_WaveAngle1);
                float2 waveValue1=float2(cos(angle1),sin(angle1))*_WaveSpeed1;  
                float2 _WaveT2=(_TimeParameters.x.xx)*waveValue1;				
                float2 _TilingAndOffset1=screenUV*WaveScale1+_WaveT2; 
                float4 _WaveCol1=  SAMPLE_TEXTURE2D( _WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset1); 
                _WaveCol1.rgb = UnpackNormal(_WaveCol1);
               
                
                //波纹叠加
                float3 _endWave=_WaveCol0.xyz +_WaveCol1.xyz;   
                //波纹r、g叠加
                float waveBlendCol=_endWave[0]+_endWave[1]; 
                waveBlendCol=clamp(waveBlendCol,0,1); 
                waveBlendCol*=waterValue;
                //噪声
                float _waterNoise;
                Unity_SimpleNoise_float(screenUV.xy, waterNoiseScale, _waterNoise);  
                waveBlendCol*=_waterNoise;
                

                //波纹与边缘混合 
                //映射水面深度
                Unity_Remap_float(_WaterMask.r,float2(0,1),float2(_WaterZero,_WaterBottom),_WaterMask.r);
                _WaterMask.r=clamp(_WaterMask.r,0,1);
                

                float _WaterMask1=step(_WaterHigh,_WaterMask.r);	 
                float _WaterMask2=step(_WaterHigh+EdgeValue,_WaterMask.r); 
                stepMask*=_WaterMask1;

                float _EdgeMaskValue=_WaterMask.r;
                Unity_Remap_float(_EdgeMaskValue,float2(_WaterHigh,_WaterHigh+EdgeValue),float2(0,1),_EdgeMaskValue);
                
                float edge=(_WaterMask1-_WaterMask2)*_EdgeMaskValue; 
               

               
                //float3 edgeAddColor=edge*float3(0,1,1)*2; 
                float3 endWaveColor=edge*EdgeColor.xyz*waveBlendCol+waveBlendCol*_WaterMask1.rrr;
                endWaveColor=clamp(endWaveColor,0,1);
                
                half sunValue=(_SunColor.x+_SunColor.y+_SunColor.z)/3;

                //主颜色
                float3 _MainColor=waterColor.xyz*waterColor.a;	 
                _MainColor+=(1-waterColor.a)*_MainTexColor.xyz;
                _MainColor.xyz*=_WaterMask.r;

              endWaveColor=endWaveColor.xyz*_SunColor.xyz/sunValue;

                float3 outWater=endWaveColor+_MainColor; 
                outWater=clamp(outWater,0,1);   
                
                outWater=outWater+waterColor.xyz*waterColor.a;
               
               float water_valueX=outWater.r;
				float water_valueY=outWater.g;
                Unity_Remap_float(water_valueX,float2(0,1),float2(-0,0.1),water_valueX);
				Unity_Remap_float(water_valueY,float2(0,1),float2(-0.02,0.02),water_valueY);

                mirrorUV.x+=water_valueX;
                mirrorUV.y+=water_valueY;
 
                float3 MirrorTexColor= SAMPLE_TEXTURE2D(_MirrorTex, sampler_MirrorTex, mirrorUV).xyz;  
                float MirrorValue=(MirrorTexColor.x+MirrorTexColor.y+MirrorTexColor.z)/3;
                //return MirrorTexColor;

                outWater=outWater*(1-MirrorValue)+MirrorTexColor*MirrorValue;
                outWater=stepMask*outWater+_MainTexColor.xyz*(1-stepMask);
                return outWater;
            }

            float2 MoveUV(float2 uv,float2 screenUV,float SnowMove,out float2 offset)
            {
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;

                float2 panner63 = _WindScroll * 0.3 * _TimeParameters.x + screenUV;
				float2 panner74 =_TimeParameters.x * _WindJitter * 0.5  + screenUV *2;

                float4 WindNoise0=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner63);
                WindNoise0=pow(abs(WindNoise0), 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner74);

                float4 moveValue=_MoveMask.Sample(sampler_MainTex,uv);

                float windValue=lerp(1,2,abs(_WindValue));
                //return float2(moveValue.x,moveValue.x);
                float value=moveValue.x*_WindNoiseValue*windValue;
                offset=WindNoise0.x*WindNoise1.x*value*SnowMove;
                int stepWind=step(0,_WindValue);
                offset.x=offset.x*stepWind-offset.x*(1-stepWind);

                return offset+uv;
            }

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            

           
            

             float3 DampColor(float3 col,float2 uv,float2 objUV)
            {
                float r=col.r*col.r;
                float g=col.g*col.g;
                float b=col.b*col.b;
                float3 _col=float3(r,g,b);
                float3 col1=_col;
                
                float _DampNoiseValue;	 

                 float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;  


				 _DampNoiseValue=My_SimpleNoise_float(uv+_WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy,_DampNoise);
                float3 d=float3(_DampNoiseValue,_DampNoiseValue,_DampNoiseValue);    


                float3 water=float3(1-_DampNoiseValue,1-_DampNoiseValue,1-_DampNoiseValue);    
                float waterValue=clamp((_DampValue-0.5),0,0.5)/0.5;      

                 float _HighLightNoiseValue=My_SimpleNoise_float(uv+_WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy,_HighLightNoise);
                float3 h=float3(_HighLightNoiseValue,_HighLightNoiseValue,_HighLightNoiseValue);

                

                h*=(1-_DampNoiseValue);       
                             
                float dValue=1-waterValue;
                d*=dValue;               
				d*=d; 
                d=clamp(d,0,1);

               
               
                water*=waterValue;
                water*=water;  
                water=clamp(water,0,1);
                //return water*_WaterColor;

                h*=waterValue;
                h*=step(_HighLighStep,h);
                h*=h;

                h=clamp(h,0,1);
                
                
                float c=(col.r+col.g+col.b)/3; 

                d*=_DampColor.xyz*c;
                water*=_DampWaterColor.xyz*c;
                h*=_HightLightColor.xyz*c; 

                _col+=d+water+h;  
                half4 normal =_NormalMap.Sample(sampler_MainTex,objUV);
                //half3 normalUnpacked = UnpackNormalRGBNoScale(normal);
                float gv=normal.z;
                //return normal.zzz;
                //float stepGv=step(0.5,gv);
                //gv=stepGv+(1-stepGv)*gv; 
                // return gv.xxx;
                Unity_Remap_float(gv,float2(0,1),float2(0.2,1),gv);

                float3 result=lerp(col,_col,clamp(_DampValue/0.5,0,1)*gv);
                //return _DampValue.xxx;
                 //result=lerp(col,result,_DampValue); 
               return result*(1-_Damp)+_col*_Damp; 
            }

            float3 BlendLightCol(float3 col,float2 screenUV)
            {
                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,screenUV);
                 col*=lightCol.xyz;
                 return col;
            }

             

            Varyings DefaultVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
              
                o.worldPos.xyz=TransformObjectToWorld(v.positionOS);
                //half3 worldPos=o.worldPos.xyz;
               // worldPos.z+=worldPos.y;
               // o.positionCS =TransformWorldToHClip(worldPos);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                float3 worldCS=o.worldPos.xyz;
                worldCS.y=UNITY_MATRIX_M._m13;
                //o.worldPos.w=o.worldPos.z;
                //
                
                half4 worldPosCs=TransformWorldToHClip(worldCS.xyz);
                half2 worldScreen=half2(ComputeScreenPos(worldPosCs / worldPosCs.w).xy);
                o.worldPos.zw=worldScreen;
               // half4 grassColor=  SAMPLE_TEXTURE2D_LOD(_GrassTex, sampler_GrassTex, worldScreen,0); 
               // float GrassColorValue=abs(grassColor.r-0.5)/0.5;
               // o.worldPos.w=GrassColorValue;

                int seasonColorBlend=_PlantSpringColor.x+_PlantSpringColor.y+_PlantSpringColor.z; 
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);


                half3 pos=TransformObjectToWorld(_WorldSpaceCameraPos.xyz);
                half4 carmeraPos=TransformWorldToHClip(pos); 

                o.fixScreenUV=o.lightingUV-half2(ComputeScreenPos(carmeraPos / carmeraPos.w).xy);

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }

            Varyings CombinedShapeLightVertex(Attributes v)
            { 
                return DefaultVertex(v);
            }
 

            half4 DefaultFragment(Varyings i) : SV_Target
            {
                float2 uv=i.uv;
                float2 offset;

                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.1,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;
 
                uv=MoveUV(uv,i.lightingUV,1-s_w,offset);
                //return half4(uv.xxx,1);
                half4 main =_MainTex.Sample(sampler_MainTex,uv); 
              //  return float4(main.xyz,main.a);

                const half4 grassTex=_MoveMask.Sample(sampler_MainTex,uv);
                float mainValue=(main.y);
               
                mainValue=clamp(mainValue,0,1);

                 float noiseValue=My_SimpleNoise_float(i.worldPos.xy,_PlantAutumnNoiseScale); 
                float noiseValue1=My_SimpleNoise_float(i.worldPos.xy,_PlantAutumnNoiseScale*2); 
                
                int seasonColorBlend=_PlantSpringColor.x+_PlantSpringColor.y+_PlantSpringColor.z;
                 
                      
                half4 snow =_SnowTex.Sample(sampler_MainTex,uv);
              // return snow;
                half3 mainSnow=main.xyz*(1-snow.a)+snow.xyz*snow.a;
                half snowA=main.a*(1-snow.a)+snow.a;
                half snowValue= s_w*_SnowBlend;
                main=main*(1-snowValue)+half4(mainSnow.xyz,snowA)*snowValue;
             
                half4 _DepthColor =_DepthTex.Sample(sampler_MainTex,uv);
                half4 _GrassColor =SAMPLE_TEXTURE2D(_GrassTex,sampler_GrassTex,i.worldPos.zw);
                float GrassColorValue=_GrassColor.r*_GrassColor.g*0.5+_GrassColor.g*0.5; 
                GrassColorValue=GrassColorValue;
               
                 
                float HightValue=step(GrassColorValue,_DepthColor.g)*(1-step(_DepthColor.g,0));  
                main.a=main.a*HightValue*_GrassBlend+(1-_GrassBlend)*main.a;  
                
 
                half4 result=main;
                
                float singleValue=(main.x+main.y+main.z)/3;
                float3 singleColor=main.xyz*(i.color.a)+singleValue.xxx*(1-i.color.a);
                float3 waterColor=main.xyz*i.color.xyz;
              
                waterColor.xyz=waterColor.xyz*(1-_BlendVertexColor)+singleColor*_BlendVertexColor; 
              // waterColor=DampColor(waterColor,i.lightingUV,uv)*_DampBlend+(1-_DampBlend)*waterColor; 
               
                main.xyz=waterColor.xyz;
                waterColor=WaterFragment(uv,i.lightingUV,main)*_Water+(1-_Water)*main;
             
 

                half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,i.lightingUV);
                lightCol.xyz*=4; 

                result.xyz=waterColor.xyz*lightCol.xyz;
              
                result.xyz=_LightBlend*result.xyz+(1-_LightBlend)*waterColor.xyz; 
                 
                half _light_value=(lightCol.x+lightCol.y+lightCol.z)/3;
                float globalValue=(_GlobalColor.x+_GlobalColor.y+_GlobalColor.z)/3;
                
                
                _light_value-=globalValue*_ShadowValue; 
                _light_value=clamp(_light_value,0,1);
                _light_value=(1-_light_value*0.5);
 


                half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, i.lightingUV);  
                  
                shadow.xyz*=_light_value;
                half3 shadowColor=GlobalColor.xyz*GlobalColor.a*shadow.r;  

                half3 shadowResult=shadowColor*result.xyz+result.xyz*(1-shadow.r);  
                result.xyz=result.xyz*(1-_shadowStep)+shadowResult*_shadowStep;     
                
                /*
                int stepBack=_BackBlend*_backColor;
                half4 backColor=SAMPLE_TEXTURE2D(_BackMaskTex, sampler_BackMaskTex, i.lightingUV); 
                half backColorValue=(backColor.r+backColor.g+backColor.b)/3;
                result.xyz=(half3(0,0.5,0.8)*backColorValue+result.xyz*(1-backColorValue))*stepBack+(1-stepBack)* result.xyz;
                */
                    
                //result.xyz=waterColor.xyz; 
                //clip(result.a-0.01);

                return result;
            }
          

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {  
                 return DefaultFragment(i); 
            } 
            ENDHLSL
        }


        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM 

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile _ SKINNED_SPRITE 

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float3 normalOS : NORMAL;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 tangent      : TANGENT;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                half4   color           : COLOR;
                float2  uv              : TEXCOORD0;
                half3   normalWS        : TEXCOORD1;
                half3   tangentWS       : TEXCOORD2;
                half3   bitangentWS     : TEXCOORD3;
                half4   lightingUV  : TEXCOORD4; 
                //half3   screenUV : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
             float2 MoveUV(float2 uv,float2 screenUV,float SnowMove)
            {
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;

                float2 panner63 = _WindScroll * 0.3 * _TimeParameters.x + screenUV;
				float2 panner74 =_TimeParameters.x * _WindJitter * 0.5  + screenUV *2;

                float4 WindNoise0=pow(abs(SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner63)) , 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner74);

                float4 moveValue=_MoveMask.Sample(sampler_MainTex,uv);
                float windValue=lerp(1,2,abs(_WindValue));
                //return float2(moveValue.x,moveValue.x);
                float value=moveValue.x*_WindNoiseValue*windValue;
                float  offset=WindNoise0.x*WindNoise1.x*value*SnowMove;
                int stepWind=step(0,_WindValue);
                offset.x=offset.x*stepWind-offset.x*(1-stepWind);

                return offset+uv;
            }
            
            Varyings DefaultVert(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);

               /* o.screenUV.xy=o.positionCS.xy;        
                o.screenUV.z=unity_SpriteProps.x;       
                Unity_Remap_float(o.screenUV.x,float2(-1,1),float2(0,1),o.screenUV.x);
                Unity_Remap_float(o.screenUV.y,float2(-1,1),float2(1,0),o.screenUV.y);*/
                
                o.uv = attributes.uv;
                o.color = attributes.color;
                o.normalWS = -GetViewForwardDir();
                //o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.tangentWS = attributes.tangent.xyz;
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                o.lightingUV.xy = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                half3 worldPos=TransformObjectToWorld(attributes.positionOS.xyz);
                worldPos.y=UNITY_MATRIX_M._m13;
                half4 worldPosCs=TransformWorldToHClip(worldPos);
                half2 worldScreen=half2(ComputeScreenPos(worldPosCs / worldPosCs.w).xy);
                o.lightingUV.zw=worldScreen;
                return o;
            }
 

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                  return DefaultVert(attributes);
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

           
            half4 DefaultFrag(Varyings i) : SV_Target
            { 
                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.05,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;


                float2 uv=MoveUV(i.uv,i.lightingUV.xy,1-s_w);
                half4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv); 
                half4 _NormalColor =_NormalMap.Sample(sampler_MainTex,i.uv);
                 
                half4 snow =_SnowTex.Sample(sampler_MainTex,uv); 
                half3 mainSnow=mainTex.xyz*(1-snow.a)+snow.xyz*snow.a;
                half snowA=mainTex.a*(1-snow.a)+snow.a;
                half snowValue= s_w*_SnowBlend;
                mainTex=mainTex*(1-snowValue)+half4(mainSnow.xyz,snowA)*snowValue;

                half3 normalTS;
                half4 result=half4(1,1,1,1);
                normalTS = UnpackNormal(_NormalColor);
                result=NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                result.x=unity_SpriteProps.x*result.x+(1-unity_SpriteProps.x)*(1-result.x);
               result.z=0;
               
                // result.z-=i.positionCS.y; 
                result=result*i.color; 
                // normalTS=WaterFragment(i.uv,i.screenUV,normalTS);
                
                return result;
            }

            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {   return DefaultFrag(i);
            }
            ENDHLSL
        }
       
        Pass
        {
            Tags { "LightMode" = "Shadow" "Queue"="Transparent" "RenderType"="Transparent"}
            BlendOp Max 
            

            HLSLPROGRAM 

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            #pragma multi_compile _ SKINNED_SPRITE 

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float4  color           : COLOR;
                float2  uv              : TEXCOORD0;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                float4x4 m_Data=UNITY_MATRIX_M;
                float lightAngleValue=sin(LightDirection.x);
                m_Data[0][0]+=m_Data[0][0]*abs(lightAngleValue)*0.5*LightDirection.y;

                attributes.positionOS = UnityFlipSprite( attributes.positionOS, unity_SpriteProps.xy);
                float3 worldPos=mul(m_Data, float4(attributes.positionOS.xyz, 1.0)).xyz;
                 
                float scaleZ=UNITY_MATRIX_M._m22*LightDirection.y;
                scaleZ+=  scaleZ*abs(lightAngleValue)*0.5*LightDirection.y;

                float2 offset=scaleZ.xx*float2(sin(LightDirection.x),cos(LightDirection.x));
                worldPos.xy+=offset;
                
                o.positionCS = TransformWorldToHClip(worldPos);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = worldPos;
                #endif
                o.uv = attributes.uv;
                o.color = attributes.color * _Color * unity_SpriteColor;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color *_MainTex.Sample(sampler_MainTex,i.uv); 
                mainTex.xyz=float3(1,1,1)*mainTex.a; 
                 

                return mainTex;
                
            }
            ENDHLSL
        } 

        Pass
        {
            Tags { "LightMode" = "CharacterDepth" "Queue"="Transparent" "RenderType"="Transparent"} 
            HLSLPROGRAM 

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment 
            #pragma multi_compile _ SKINNED_SPRITE 

            struct Attributes
            {
                float3 positionOS   : POSITION; 
                float2 uv           : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float3  color           : COLOR;
                float2  uv              : TEXCOORD0; 
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                attributes.positionOS = UnityFlipSprite( attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                float3 objWroldPos=TransformObjectToWorld(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = objWroldPos;
                #endif
                o.uv = attributes.uv;

                float3 ObjPos=UNITY_MATRIX_M._m03_m13_m23;
                float stepPosZ=step(49,ObjPos.z);

                float3 _objSortPos=ObjPos; 
                float4 worldClip=TransformWorldToHClip(_objSortPos); 
                float high=(1-stepPosZ)*(objWroldPos.y-ObjPos.y)*0.5;
                float positionCSY=o.positionCS.y;  
                worldClip.y=stepPosZ*positionCSY+(1-stepPosZ)*worldClip.y;  
                worldClip.xy=half2(ComputeScreenPos(worldClip/worldClip.w).xy); 

                
                o.color.x=clamp(high,0,1);                 
                o.color.yz= worldClip.xy;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv); 
                float4 DepthTex =_DepthTex.Sample(sampler_MainTex,i.uv); 
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex,i.uv);

                float4 ObjDepthTex=SAMPLE_TEXTURE2D(_ObjDepthTex, sampler_ObjDepthTex, i.color.yz);
                 
                half depthStep_R=step(0.01,abs(DepthTex.r-0.5));
                half depthStep_G=1-step(abs(DepthTex.g-0.5),0.01);
                half depthStep_B=step(0.01,abs(DepthTex.b-0.5));
                half depthStep_ZeroB=step(0.01,DepthTex.b);
                half stepDepthOne=step(1,DepthTex.b);

                half otherStep=depthStep_R*depthStep_G+depthStep_B; 
                otherStep=clamp(otherStep,0,1)*depthStep_ZeroB;
               // return float4(otherStep.xxx,mainTex.a);

                half depthValue=(DepthTex.r-0.5)*(1-otherStep)+(DepthTex.r+DepthTex.b-1)*(1-stepDepthOne)*otherStep; 
                half offset=depthValue*512*4/_ScreenParams.y;

               
                half depth=i.color.z+offset;
                half setpHigh=depthStep_G; 

                half high=i.color.x*(1-setpHigh)+DepthTex.g*2*setpHigh;

              
                mainTex.xyz=half3(depth,high,_NormalColor.g*0.5+stepDepthOne);
               // mainTex.y+=ObjDepthTex.y;
                mainTex.z+=ObjDepthTex.z;
                mainTex.a=mainTex.a*(1-stepDepthOne)+DepthTex.a*stepDepthOne;

               // mainTex.xyz=half3(0,0,ObjDepthTex.z); 



 
               // mainTex.xyz=otherStep.xxx;
                

                return mainTex;
                
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "ObjDepth" "Queue"="Transparent" "RenderType"="Transparent"} 
            HLSLPROGRAM 

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment 
            #pragma multi_compile _ SKINNED_SPRITE 

            struct Attributes
            {
                float3 positionOS   : POSITION; 
                float2 uv           : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float3  color           : COLOR;
                float2  uv              : TEXCOORD0;
                float2  screenUV        : TEXCOORD1;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                attributes.positionOS = UnityFlipSprite( attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                float3 objWroldPos=TransformObjectToWorld(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = objWroldPos;
                #endif
                o.uv = attributes.uv;

                float3 ObjPos=UNITY_MATRIX_M._m03_m13_m23;
                float stepPosZ=1-step(100,ObjPos.z);

                float3 _objSortPos=ObjPos; 
                _objSortPos.y+=_objSortPos.z;

               // int stepFixed=1-step(_FixedDepth,0);
               // _objSortPos.y=(1-stepFixed)*_objSortPos.y+stepFixed*_FixedDepth; 

                float4 worldClip=TransformWorldToHClip(_objSortPos); 
                float high=stepPosZ*(objWroldPos.y-ObjPos.y)*0.5;
                float positionCSY=o.positionCS.y;   
                //stepPosZ+=stepFixed; 
              
                stepPosZ=clamp(stepPosZ,0,1);
                worldClip.y=(1-stepPosZ)*positionCSY+stepPosZ*worldClip.y;  
                worldClip.xy=half2(ComputeScreenPos(worldClip/worldClip.w).xy); 
                o.screenUV.xy=half2(ComputeScreenPos(o.positionCS/o.positionCS.w).xy); 
                 o.color.x=clamp(high,0,1);    
                 o.color.yz=worldClip.xy; 
               
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv); 
                float4 DepthTex =_DepthTex.Sample(sampler_MainTex,i.uv); 
                float clipA=1-step(DepthTex.a,0);
                DepthTex.xyz*=clipA;
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex,i.uv);
                 
                half depthStep_R=step(0.01,abs(DepthTex.r-0.5));
                half depthStep_G=1-step(abs(DepthTex.g-0.5),0.01);
                half depthStep_B=step(0.01,abs(DepthTex.b-0.5));
                half depthStep_ZeroB=step(0.01,DepthTex.b);
                half stepDepthOne=step(1,DepthTex.b);

                int clearColor=1-step(DepthTex.b,0)*step(DepthTex.r,0)*step(DepthTex.g,0);

                half otherStep=depthStep_R*depthStep_G+depthStep_B; 
                otherStep=clamp(otherStep,0,1)*depthStep_ZeroB;
               

                half depthValue=(DepthTex.r-0.5)*(1-otherStep)+(DepthTex.r+DepthTex.b-1)*(1-stepDepthOne)*otherStep; 
                half offset=depthValue*512*4/_ScreenParams.y;

               
                half depth=i.color.z+offset*clearColor;
                half setpHigh=depthStep_G; 

                //return float4(i.color.zzz,mainTex.a);

                half high=i.color.x*(1-setpHigh)+DepthTex.g*2*setpHigh;
                
                mainTex.xyz=half3(depth,high,_NormalColor.g*0.5+stepDepthOne)*(1-_Character);
               
                

                half absUv=length(i.screenUV-i.color.yz);
                int stepMul=step(absUv,0.001)*_Character;
                

                mainTex.a=(mainTex.a*(1-stepDepthOne)+DepthTex.a*stepDepthOne)*(1-stepMul);

                //return mainTex.aaaa;
                //clip(mainTex.a);

               // mainTex.xyz=otherStep.xxx;
                

                return mainTex;
                
            }
            ENDHLSL
        }

        Pass
        {
            Name "BackColor" 
            Tags {"LightMode" = "BackColor" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
           
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            #pragma multi_compile _ SKINNED_SPRITE 
 

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float4  color           : COLOR;
                float2  uv              : TEXCOORD0;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                attributes.positionOS = UnityFlipSprite( attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = attributes.uv;
                o.color = attributes.color * _Color * unity_SpriteColor;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color *_MainTex.Sample(sampler_MainTex,i.uv); 

              
                float value=(mainTex.x+mainTex.y+mainTex.z)/3;
                float stepValue=1-step(0.5,value);
                return float4(stepValue.xxx,mainTex.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Water" 
            Tags {"LightMode" = "Water" }

            HLSLPROGRAM
             
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
 
          

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;  
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                half2   lightingUV  : TEXCOORD1; 
                float4  worldPos : TEXCOORD4;
                half2   fixScreenUV: TEXCOORD3; 
            };

          

            float3 WaterFragment(float2 uv,float2 screenUV)
            {
                float2 mirrorUV=screenUV;  

                float3 _WaterMask= SAMPLE_TEXTURE2D(_WaterMaskTex,sampler_WaterMaskTex, uv.xy).xyz;
                //水域范围
                float stepMask=step(0.06,_WaterMask.r); 

                half edgeOffsetValue=_SinTime.w*_EdgeWaveSpeed; 
                edgeOffsetValue=abs(edgeOffsetValue); 
                 edgeOffsetValue=clamp(edgeOffsetValue,0,1);
                _WaterHigh=_WaterHigh+_EdgeWaveOffset*edgeOffsetValue;
                 //return _EdgeWaveOffset*edgeOffsetValue;

                
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;
                
                //波纹1
                float angle0=radians(_WaveAngle0);//转换角度为弧度
                float2 waveValue0=float2(cos(angle0),sin(angle0))*_WaveSpeed0;  

                float2 _WaveT0=(_TimeParameters.x.xx)*waveValue0; 
                float2 _TilingAndOffset0=screenUV*WaveScale0+_WaveT0;
                float4 _WaveCol0 = SAMPLE_TEXTURE2D( _WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset0); 
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);	
                //波纹2
                float angle1=radians(_WaveAngle1);
                float2 waveValue1=float2(cos(angle1),sin(angle1))*_WaveSpeed1;  
                float2 _WaveT2=(_TimeParameters.x.xx)*waveValue1;				
                float2 _TilingAndOffset1=screenUV*WaveScale1+_WaveT2; 
                float4 _WaveCol1= SAMPLE_TEXTURE2D( _WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset1); 
                _WaveCol1.rgb = UnpackNormal(_WaveCol1);
                
                
                //波纹叠加
                float3 _endWave=_WaveCol0.xyz +_WaveCol1.xyz;   
                //波纹r、g叠加
                float waveBlendCol=_endWave[0]+_endWave[1]; 
                waveBlendCol=clamp(waveBlendCol,0,1); 
                waveBlendCol*=waterValue;
                //噪声
                float _waterNoise;
                Unity_SimpleNoise_float(screenUV.xy, waterNoiseScale, _waterNoise);  
                waveBlendCol*=_waterNoise;
                

                //波纹与边缘混合 
                //映射水面深度
                Unity_Remap_float(_WaterMask.r,float2(0,1),float2(_WaterZero,_WaterBottom),_WaterMask.r);
                _WaterMask.r=clamp(_WaterMask.r,0,1);
                

                float _WaterMask1=step(_WaterHigh,_WaterMask.r);	 
                float _WaterMask2=step(_WaterHigh+EdgeValue,_WaterMask.r); 
                stepMask*=_WaterMask1;
                return stepMask; 
                
            } 
 

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.worldPos.xyz=UNITY_MATRIX_M._m03_m13_m23;
                o.worldPos.w=o.worldPos.z;
                o.worldPos.z+=o.worldPos.y;
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);


                half3 pos=TransformObjectToWorld(_WorldSpaceCameraPos.xyz);
                half4 carmeraPos=TransformWorldToHClip(pos);



                o.fixScreenUV=o.lightingUV-half2(ComputeScreenPos(carmeraPos / carmeraPos.w).xy);

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }
 

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                
                float3  result=WaterFragment(i.uv,i.lightingUV);
                 

                return float4(result.xyz,1);
            }
            ENDHLSL
        }
        Pass
        {
             Tags { "LightMode" = "Grass" }

            HLSLPROGRAM 
            #pragma vertex  Vertex
            #pragma fragment  Fragment 
  
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
                UNITY_VERTEX_OUTPUT_STEREO
            }; 

            Varyings  Vertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS); 
                o.uv = v.uv;
                 
                return o;
            }
 

            half4 Fragment(Varyings i) : SV_Target
            {
                half4 mainTex = _MainTex.Sample(sampler_MainTex,i.uv); 
                float4 moveValue=_MoveMask.Sample(sampler_MainTex,i.uv);
                moveValue.a=mainTex.a;
                //moveValue.xyz=moveValue.ggg;
                return moveValue;
            }
            ENDHLSL
        }

         Pass
        {
             Tags { "LightMode" = "GroundFoot" }

            HLSLPROGRAM 
            #pragma vertex  Vertex
            #pragma fragment  Fragment 
  
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
                UNITY_VERTEX_OUTPUT_STEREO
            }; 

            Varyings  Vertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS); 
                o.uv = v.uv;
                return o;
            }
 

            half4 Fragment(Varyings i) : SV_Target
            {
                half4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv);  
                float4 moveValue=_MoveMask.Sample(sampler_MainTex,i.uv);
                int groundStep=1-step(moveValue.z,0);
                moveValue.a*=mainTex.a;
                
                int grassStep=1-step(moveValue.x+moveValue.y,0);
                int groundIndex=moveValue.z;

                float outValue=grassStep*(moveValue.z+0.8)+moveValue.z;

                return float4(outValue.xxx,moveValue.a);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
