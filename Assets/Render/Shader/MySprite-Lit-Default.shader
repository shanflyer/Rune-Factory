Shader "MySprite-Lit-Default"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _WaterMaskTex("_MoveMask", 2D) = "black" {}
        _SnowTex("_SnowTex", 2D) = "black" {}
        _ZWrite("ZWrite", Float) = 0

        _WaterNormalMap("WaterNormalMap", 2D) = "bump" {} 
        _NormalMap("Normal Map", 2D) = "bump" {}
        _MoveMask("WaterMaskTex", 2D) ="black"{}
        _DepthTex("DepthTex", 2D) ="black"{}
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
        _WindValue("WindValue", Range( 0 , 3)) = 1
        [Toggle]_GrassBlend("_GrassBlend",int)=0
        

        _PlantSpringColor("_PlantSpringColor",color)=(0,0,0)
        _PlantSpringColor1("_PlantSpringColor1",color)=(0,0,0)
        _PlantAutumnColor0("_PlantAutumnColor0",color)=(0,0,0)
        _PlantAutumnColor1("_PlantAutumnColor1",color)=(1,1,1)
        _PlantWinterColor("_PlantWinterColor",color)=(0,0,0)
        _PlantWinterColor1("_PlantWinterColor1",color)=(0,0,0)
        //_SeasonValue("_SeasonValue",Range(0,4))=0
        _PlantAutumnNoiseScale("_PlantAutumnNoiseScale",float)=1
        [Toggle]_PlantAutumnBlend("_PlantAutumnBlend",int)=0

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

        
       
        [Toggle]_Tree3D("_Tree3D",int)=0
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
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]
		ZTest LEqual

        HLSLINCLUDE
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"


            float2 Posterize_float2(float2 In, float2 Steps)
            {
                return floor(In / (1 / Steps)) * (1 / Steps);
            }

            float2 Unity_GradientNoise_Dir_float(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                p = p % 289;
                // need full precision, otherwise half overflows when p > 1
                float x = float(34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            float Unity_GradientNoise_float(float2 UV, float Scale)
            { 
                float2 p = UV * Scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
                float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
                
            }
            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            void Unity_Remap_float2(float2 In, float2 InMinMax, float2 OutMinMax, out float2 Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            void Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax, out float4 Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            void Unity_Remap_float3(float3 In, float2 InMinMax, float2 OutMinMax, out float3 Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            
            float3 Unity_Rotate_About_Axis_Radians_float(float3 In, float3 Axis, float Rotation)
            {
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;

                Axis = normalize(Axis);

                float3x3 rot_mat = { one_minus_c * Axis.x * Axis.x + c,            one_minus_c * Axis.x * Axis.y - Axis.z * s,     one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s,   one_minus_c * Axis.y * Axis.y + c,              one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s,   one_minus_c * Axis.y * Axis.z + Axis.x * s,     one_minus_c * Axis.z * Axis.z + c
                };

                return mul(rot_mat,  In);
            }


            float2 Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation)
            {
                //rotation matrix
                UV -= Center;
                float s = sin(Rotation);
                float c = cos(Rotation);
                
                //center rotation matrix
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.5;
                rMatrix += 0.5;
                rMatrix = rMatrix*2 - 1;
                
                //multiply the UVs by the rotation matrix
                UV.xy = mul(UV.xy, rMatrix);
                UV += Center;
                
                return UV;
            }

            float4 NoiseSineWave_float4(float4 In, float2 MinMax)
            {
                float sinIn = sin(In.x);
                float sinInOffset = sin(In.x + 1.0);
                float randomno =  frac(sin((sinIn - sinInOffset) * (12.9898 + 78.233))*43758.5453);
                float noise = lerp(MinMax.x, MinMax.y, randomno);
                return sinIn + noise;
            }

            inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
            }
            
            inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
            {
                return (1.0-t)*a + (t*b);
            }
            
            
            inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                
                uv = abs(frac(uv) - 0.5);
                float2 c0 = i + float2(0.0, 0.0);
                float2 c1 = i + float2(1.0, 0.0);
                float2 c2 = i + float2(0.0, 1.0);
                float2 c3 = i + float2(1.0, 1.0);
                float r0 = Unity_SimpleNoise_RandomValue_float(c0);
                float r1 = Unity_SimpleNoise_RandomValue_float(c1);
                float r2 = Unity_SimpleNoise_RandomValue_float(c2);
                float r3 = Unity_SimpleNoise_RandomValue_float(c3);
                
                float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
                float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
                float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
                return t;
            }

            void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
            {
                float t = 0.0;
                
                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3-0));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
                
                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3-1));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
                
                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3-2));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
                
                Out = t;
            }
            void Unity_SimpleNoise_float2(float2 UV, float2 Scale, out float Out)
            {
                float t = 0.0;
                
                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3-0));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale.x/freq, UV.y*Scale.y/freq))*amp;
                
                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3-1));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale.x/freq, UV.y*Scale.y/freq))*amp;
                
                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3-2));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale.x/freq, UV.y*Scale.y/freq))*amp;
                
                Out = t;
            }

            float3 StandardColorRevise(float3 baseColor,float3 standardColor,float colorThresHold)
            {

                float3 color = baseColor-standardColor;
                float thresHold0 = pow(color.r*color.r+color.g*color.g+color.b*color.b,0.5);
                //float thresHold0=abs(baseColor.r-standardColor.r)+abs(baseColor.g-standardColor.g)+abs(baseColor.b-standardColor.b);
                thresHold0=step(colorThresHold,thresHold0);

                return  1-thresHold0;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex); 
            TEXTURE2D(_MirrorTex);
            SAMPLER(sampler_MirrorTex); 

            TEXTURE2D(_ShadowTex);
            SAMPLER(sampler_ShadowTex);
            
            TEXTURE2D(_BackMaskTex);
            SAMPLER(sampler_BackMaskTex);

            TEXTURE2D(_WaterMaskTex);
            SAMPLER(sampler_WaterMaskTex);
            TEXTURE2D(_WaterNormalMap);
            SAMPLER(sampler_WaterNormalMap);
            TEXTURE2D(_DepthTex);
            SAMPLER(sampler_DepthTex); 
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap); 
            TEXTURE2D(_WindNoiseTexture);
            SAMPLER(sampler_WindNoiseTexture);

            TEXTURE2D(_MoveMask);
            SAMPLER(sampler_MoveMask); 

            TEXTURE2D(_GrassTex);
            SAMPLER(sampler_GrassTex); 
            TEXTURE2D(_SnowTex);
            SAMPLER(sampler_SnowTex);

            TEXTURE2D(_CloudTex);
            SAMPLER(sampler_CloudTex); 

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
        CBUFFER_START(UnityPerMaterial)
			float3 _BlendColor;
			float _BlendValue;
			float _BlendRmapMin;
            float _WindValue; 
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
            
            
           // half4 _MainTex_TexelSize;
            //half4 _MainTex_ST;
           // half4 _NormalMap_ST;  // Is this the right way to do this?
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
        
         
        ENDHLSL

         
        Pass
        {
            Tags {"Name"="Universal2D" "LightMode" = "Universal2D" }

            HLSLPROGRAM
             

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE
            
 
             
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

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            float3 WaterFragment(float2 uv,float2 screenUV,float4 _MainTexColor)
            {
                float2 mirrorUV=screenUV; 

                float3 _WaterMask= SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).xyz; 
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
                float4 _WaveCol0 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset0); 
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);	
                //波纹2
                float angle1=radians(_WaveAngle1);
                float2 waveValue1=float2(cos(angle1),sin(angle1))*_WaveSpeed1;  
                float2 _WaveT2=(_TimeParameters.x.xx)*waveValue1;				
                float2 _TilingAndOffset1=screenUV*WaveScale1+_WaveT2; 
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

                float4 moveValue=SAMPLE_TEXTURE2D(_MoveMask,sampler_MoveMask, uv);
                //return float2(moveValue.x,moveValue.x);
                float value=moveValue.x*_WindNoiseValue;
                offset=WindNoise0.x*WindNoise1.x*value*SnowMove;
                return offset+uv;
            }

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            

            #if USE_SHAPE_LIGHT_TYPE_0
                SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
                SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
                SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
                SHAPE_LIGHT(3)
            #endif

            

             float3 DampColor(float3 col,float2 uv,float2 objUV)
            {
                float r=col.r*col.r;
                float g=col.g*col.g;
                float b=col.b*col.b;
                float3 _col=float3(r,g,b);
                
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

                half4 normal = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, objUV);
                //half3 normalUnpacked = UnpackNormalRGBNoScale(normal);
                float gv=normal.z;
                //return normal.zzz;
                //float stepGv=step(0.5,gv);
                //gv=stepGv+(1-stepGv)*gv; 
                //return gv.xxx;
                Unity_Remap_float(gv,float2(0,1),float2(0.2,1),gv);

                float3 result=lerp(col,_col,clamp(_DampValue/0.5,0,1)*gv);

                
                
               return result*(1-_Damp)+_col*_Damp; 
            }

            
            Varyings TreeVert(Attributes v)
            {
                Varyings o = (Varyings)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                float4 clipPos = TransformObjectToHClip(v.positionOS.xyz);
				 
                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.05,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;          
 
				float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);  
				o.uv.xy = v.uv.xy;   
                Unity_Remap_float3(worldNormal,float2(-1,1),float2(0,1),o.normal); 
               // Unity_Remap_float3(o.normal,float2(0.5,1),float2(0,1),worldNormal);  
				float3 vertexValue = v.normalOS * _ScaleValue* min(clipPos.w , 1.5);

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz); 
				float2 appendResult60 = float2(worldPos.x , worldPos.z)* 0.1; 
				float2 panner63 = _WindScroll * 0.3* _TimeParameters.x + appendResult60;
				float2 panner74 = _TimeParameters.x * _WindJitter * 0.5+ appendResult60  * float2(2,2);

                float4 WindNoise0=pow(SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner63,1) , 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner74,1); 
				float4 WindScroll = WindNoise0*WindNoise1 * v.color;
                vertexValue += WindScroll.rgb*_WindValue*(1-s_w); 
		  
                o.worldPos=half4(worldPos.xyz,1);
                v.positionOS.xyz += vertexValue; 
				o.positionCS =TransformObjectToHClip(v.positionOS.xyz); //TransformWorldToHClip(worldPos); 
                o.lightingUV   = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
				return o;
            }

            Varyings DefaultVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.worldPos.xyz=TransformObjectToWorld(v.positionOS);
                float3 worldCS=o.worldPos.xyz;
                worldCS.y=UNITY_MATRIX_M._m13;
                //o.worldPos.w=o.worldPos.z;
                //o.worldPos.z+=o.worldPos.y;
                
                half4 worldPosCs=TransformWorldToHClip(worldCS.xyz);
                half2 worldScreen=half2(ComputeScreenPos(worldPosCs / worldPosCs.w).xy);
                o.worldPos.zw=worldScreen;
               // half4 grassColor=  SAMPLE_TEXTURE2D_LOD(_GrassTex, sampler_GrassTex, worldScreen,0); 
               // float GrassColorValue=abs(grassColor.r-0.5)/0.5;
               // o.worldPos.w=GrassColorValue;


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
                if(_Tree3D==1)
                {
                    return TreeVert(v);
                }else 
                {
                    return DefaultVertex(v);
                }
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half3 BlendScreenCloudColor(half3 col,half2 screenUV)
            {

                 float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;

                 float2 noiseUV=screenUV+_TimeParameters.x*_WindDir.xy*_NoiseSet0.w+offsetUv;
                float2 noiseUV1=screenUV+_TimeParameters.x*_WindDir.zw*_NoiseSet1.w+offsetUv;
                 float noise0; 
                Unity_SimpleNoise_float2(noiseUV,float2(_NoiseSet0.z,_NoiseSet0.z*2),noise0);
                float noise1; 
                Unity_SimpleNoise_float2(noiseUV1,float2(_NoiseSet1.z,_NoiseSet1.z*2),noise1);
                
                
                Unity_Remap_float(noise0,_NoiseSet0.xy,float2(0,_CloudValue),noise0);
                Unity_Remap_float(noise1,_NoiseSet1.xy,float2(0,_CloudValue),noise1);
                noise0=clamp(noise0,0,1);
                noise1=clamp(noise1,0,1);
                float cloud=noise0+noise1; 
                cloud=clamp(cloud,0,1);
                 // return noise1.xxx;
                
                col=col.xyz*(1-cloud.x);
                return col;
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
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);

                const half4 grassTex=SAMPLE_TEXTURE2D(_MoveMask,sampler_MoveMask,uv);
                float mainValue=(main.y);
               
                mainValue=clamp(mainValue,0,1);

                 float noiseValue;
                Unity_SimpleNoise_float(i.worldPos.xy,_PlantAutumnNoiseScale,noiseValue); 
                float noiseValue1;
                Unity_SimpleNoise_float(i.worldPos.xy,_PlantAutumnNoiseScale*2,noiseValue1); 
                
                //春季颜色
                int springBlend=1-step(1,_SeasonValue);
                float w_s=_SeasonValue;
                Unity_Remap_float(w_s,float2(0,0.25),float2(0,1),w_s); 
                int w_sBlend=1-step(0.5,w_s);
                float springValue=_SeasonValue;
                Unity_Remap_float(springValue,float2(0.25,0.5),float2(0,1),springValue);
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
                main.xyz=main.xyz*(1-_BlendValue)+winterColor*_BlendValue;  
               // return float4(main.xyz,main.a);

                

                half4 snow = SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, uv);
                //return snow;
                half3 mainSnow=main.xyz*(1-snow.a)+snow.xyz*snow.a;
                half snowA=main.a*(1-snow.a)+snow.a;
                half snowValue= s_w*_SnowBlend;
                main=main*(1-snowValue)+half4(mainSnow.xyz,snowA)*snowValue;
                
                /*
                float snowNoise=noiseValue+noiseValue1;
                snowNoise=clamp(noiseValue,0,1);
                snowNoise=1-snowNoise;
                Unity_Remap_float(snowNoise,float2(0,1),float2(0.8,1),snowNoise);
                half grassValue=(1-snowNoise)*main.xyz;
                snowNoise=snowNoise*snowNoise+grassValue;

                main.xyz=main.xyz*(1-_BlendValue)+snowNoise*_BlendValue; */

                
                //float grassValue=step(0.001,grassTex.r*noiseValue);
                //main.xyz=mainValue*AutumnColor*_PlantAutumnBlend*grassValue+main.xyz*(1-_PlantAutumnBlend*grassValue);


                if(_GrassBlend==1)
                { 
                    half4 _DepthColor = SAMPLE_TEXTURE2D(_DepthTex, sampler_DepthTex, uv);
                    half4 _GrassColor = SAMPLE_TEXTURE2D(_GrassTex, sampler_GrassTex, i.worldPos.zw);
                    float GrassColorValue=_GrassColor.r*_GrassColor.g*0.6+_GrassColor.g*0.25; 
                    
                    //return float4(GrassColorValue.xxx,1);
                    
                    float HightValue=step(GrassColorValue,_DepthColor.r)*step(0.01,_DepthColor.r);
                    //return float4(HightValue.xxx,1);
                    main.a=main.a*HightValue; 
                }
                
 
                half4 result;
                
                float singleValue=(main.x+main.y+main.z)/3;
                float3 singleColor=main.xyz*(i.color.a)+singleValue.xxx*(1-i.color.a);
                float3 waterColor=main.xyz*i.color.xyz;
              
                waterColor.xyz=waterColor.xyz*(1-_BlendVertexColor)+singleColor*_BlendVertexColor; 
 
                if(_DampBlend)
                {
                   waterColor=DampColor(waterColor,i.lightingUV,uv); 
                } 
                 main.xyz*=i.color.xyz;
                if(_Water==1)
                {
                    waterColor=WaterFragment(uv,i.lightingUV,main);
                }

                waterColor.xyz=BlendScreenCloudColor(waterColor.xyz,i.lightingUV);
               
             
               // return float4( waterColor.xyz,main.a);
                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(waterColor, main.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData); 

                SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData, i.positionWS, i.positionCS, _MainTex);

                result=CombinedShapeLightShared(surfaceData, inputData);
                result.xyz=_LightBlend*result.xyz+(1-_LightBlend)*waterColor; 
                result.a=result.a*(1-_BlendVertexColor)*i.color.a+result.a*_BlendVertexColor; 

                half4 _light=CombinedShapeLightSharedTrueValue(surfaceData, inputData);
                half _light_value=(_light.x+_light.y+_light.z)/3;
                float globalValue=(_GlobalColor.x+_GlobalColor.y+_GlobalColor.z)/3;
                
                
                _light_value-=globalValue*_ShadowValue; 
                _light_value=clamp(_light_value,0,1);
                _light_value=(1-_light_value*0.5);

                
               // result.xyz=result.xyz*(1-snowValue.x)+snowValue;


                half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, i.lightingUV); 
                shadow.xyz*=_light_value;
                half3 shadowColor=GlobalColor.xyz*shadow.r*GlobalColor.a; 

                half3 shadowResult=shadowColor*result.xyz+result.xyz*(1-shadow.r);  
                result.xyz=result.xyz*(1-_shadowStep)+shadowResult*_shadowStep;     
                
                if(_BackBlend&&_backColor)
                {
                    half4 backColor=SAMPLE_TEXTURE2D(_BackMaskTex, sampler_BackMaskTex, i.lightingUV); 
                    half backColorValue=(backColor.r+backColor.g+backColor.b)/3;
                    result.xyz=half3(0,0.5,0.8)*backColorValue+result.xyz*(1-backColorValue);

                }
                
                //result.xyz=waterColor.xyz; 

                return result;
            }
            half4 TreeFrag (Varyings IN) : SV_Target
			{   
				float2 ScreenUV = IN.lightingUV; 
				float4 texColor =SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv.xy );
  

                float ColorValue=(texColor.r+texColor.g+texColor.b)/3; 

				Unity_Remap_float(ColorValue,float2(0,1),float2(_BlendRmapMin,1),ColorValue);
				texColor.xyz=texColor.xyz*(1-_BlendValue)+_BlendColor*_BlendValue*ColorValue; 

               

                float noiseValue;
                Unity_SimpleNoise_float(IN.worldPos.xy,_PlantAutumnNoiseScale,noiseValue); 
                float noiseValue1;
                Unity_SimpleNoise_float(IN.worldPos.xy,_PlantAutumnNoiseScale*2,noiseValue1); 
                
                int springBlend=1-step(1,_SeasonValue);
                
                float w_s=_SeasonValue;
                Unity_Remap_float(w_s,float2(0,0.25),float2(0,1),w_s); 
                int w_sBlend=1-step(0.5,w_s);
                
                float springValue=_SeasonValue;
                Unity_Remap_float(springValue,float2(0.25,0.5),float2(0,1),springValue);
                springValue=clamp(springValue,0,1);

                float3 winterColor=(_PlantWinterColor*noiseValue+_PlantWinterColor1*(1-noiseValue))*ColorValue; 
                float3 springColor0=(_PlantWinterColor*noiseValue1+_PlantSpringColor*(1-noiseValue1))*ColorValue; 
                float3 springColor=(_PlantSpringColor*noiseValue+_PlantSpringColor1*(1-noiseValue))*ColorValue; 
                springColor=(winterColor*(1-w_s)+springColor0*w_s)*w_sBlend
                          +(1-w_sBlend)*(springColor0*(1-springValue)+springColor*springValue);

                //return float4(springColor.xyz,texColor.a);

                float s_s=_SeasonValue;
                Unity_Remap_float(s_s,float2(1,1.25),float2(0,1),s_s);
                s_s=clamp(s_s,0,1);
                float3 summerColor=springColor*(1-s_s)+_BlendColor*s_s*ColorValue;

                //return float4(summerColor.xyz,texColor.a);
                
                float s_a=_SeasonValue;
                Unity_Remap_float(s_a,float2(2,2.25),float2(0,1),s_a);
                s_a=clamp(s_a,0,1); 

                float3 AutumnColor0=(_BlendColor*noiseValue1+_PlantAutumnColor0*(1-noiseValue1))*ColorValue; 
                AutumnColor0=AutumnColor0*s_a+summerColor*(1-s_a);

                float a_a=_SeasonValue;
                Unity_Remap_float(a_a,float2(2.25,2.5),float2(0,1),a_a);
                a_a=clamp(a_a,0,1); 
 
                float3 AutumnColor=(_PlantAutumnColor0*noiseValue+_PlantAutumnColor1*(1-noiseValue))*ColorValue; 
                AutumnColor=AutumnColor*a_a+AutumnColor0*(1-a_a);

                float a_w=_SeasonValue;
                Unity_Remap_float(a_w,float2(2.85,3.15),float2(0,1),a_w);
                a_w=clamp(a_w,0,1); 
                float3 winterColor0=(_PlantAutumnColor0*noiseValue1+_PlantWinterColor1*(1-noiseValue1))*ColorValue; 
                winterColor0=winterColor0*a_w+AutumnColor*(1-a_w);

                 float w_w=_SeasonValue;
                Unity_Remap_float(w_w,float2(3.15,3.35),float2(0,1),w_w);
                w_w=clamp(w_w,0,1); 
                winterColor=winterColor*w_w+winterColor0*(1-w_w);
 
                texColor.xyz=texColor.xyz*(1-_BlendValue)+winterColor*_BlendValue;  
     
               
                //texColor.xyz=((1-IN.normal.y)*texColor.xyz+IN.normal.y)*(1-_NormalTex)+(_NormalTex)*texColor.xyz;
                  
                float4 SnowColor =SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, IN.uv.xy );
              
                float normalY=IN.normal.y;
                  
                Unity_Remap_float(normalY,float2(0,1),_SnowRange.xy,normalY);
                normalY=clamp(normalY,0,1);
                float snowValue=normalY; 

               
                SnowColor=texColor*(1-snowValue)+SnowColor*snowValue; 
                
                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.1,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;
                texColor=texColor*(1-s_w)+SnowColor*s_w*_SnowColor;

                texColor.xyz=BlendScreenCloudColor(texColor.xyz,IN.lightingUV);


				float Alpha = texColor.a;  
                clip(Alpha-_ClipValue);
				SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(texColor.xyz,Alpha, float4(0,0,0,0), surfaceData);
                InitializeInputData(IN.uv.xy, ScreenUV, inputData);
                SETUP_DEBUG_TEXTURE_DATA_2D_NO_TS(inputData,IN.positionWS, IN.positionCS, _MainTex);

                //SETUP_DEBUG_TEXTURE_DATA_2D(inputData, i.positionWS, i.positionCS, _MainTex);
                float4 result=CombinedShapeLightShared(surfaceData, inputData); 
                result.xyz=clamp(result.xyz,0,1);
                return result;
			}

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {  
                 if(_Tree3D==1)
                 {
                     return TreeFrag(i);
                 }else 
                 {
                    return DefaultFragment(i); 
                 }
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

                float4 moveValue=SAMPLE_TEXTURE2D(_MoveMask,sampler_MoveMask, uv);
                //return float2(moveValue.x,moveValue.x);
                float value=moveValue.x*_WindNoiseValue;
                return WindNoise0.x*WindNoise1.x*value*SnowMove+uv;
            }
            Varyings TreeVert (Attributes v )
			{ 
				Varyings o = (Varyings)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                float4 clipPos = TransformObjectToHClip(v.positionOS.xyz); 
                float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);  
                o.uv.xy = v.uv.xy;  

                Unity_Remap_float3(worldNormal,float2(-1,1),float2(0,1),o.normalWS); 
                Unity_Remap_float3(o.normalWS,float2(0.5,1),float2(0,1),worldNormal);  

                float3 vertexValue = v.normalOS * (_ScaleValue) * min(clipPos.w , 1.5);

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                float2 appendResult60 = float2(worldPos.x , worldPos.z)* 0.1; 
                float2 panner63 = _WindScroll * 0.3* _TimeParameters.x + appendResult60;
                float2 panner74 = _TimeParameters.x * _WindJitter * 0.5+ appendResult60  * float2(2,2);

                float4 WindNoise0=pow(abs(SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner63,1)) , 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner74,1); 
                float4 WindScroll = WindNoise0*WindNoise1 * v.color;

                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.05,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1; 
                vertexValue += WindScroll.rgb*_WindValue*(1-s_w);


                v.positionOS.xyz += vertexValue;  
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);   
				return o;
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
                 if(_Tree3D==1)
                 {
                    return TreeVert(attributes);
                 }else{
                    return DefaultVert(attributes);
                 }
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 TreeFrag( Varyings IN: SV_Target0)
			{ 
               half4 outNormalWS;
               float4 texColor =SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv.xy ); 
               float3 normalWS = IN.normalWS;
                float4 SnowColor =SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, IN.uv.xy );
                float normalY=IN.normalWS.y;
                Unity_Remap_float(normalY,float2(0,1),_SnowRange.xy,normalY);
                normalY=clamp(normalY,0,1);
                //normalWS.y=normalY;
                float snowValue=normalY; 
                texColor=texColor*(1-snowValue)+SnowColor*snowValue;
                //texColor=SnowColor;
                
                outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 1);
                outNormalWS*=texColor.a; 
                
                Unity_Remap_float3(outNormalWS.xyz,float2(-1,1),float2(0,1),outNormalWS.xyz); 
                Unity_Remap_float(outNormalWS.z,float2(0,1),float2(0,0.5),outNormalWS.z); 

                clip(texColor.a-_ClipValue); 
                return outNormalWS;
			}
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
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 _NormalColor = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv);

               if(_GrassBlend==1)
                {  
                    half4 _GrassColor = SAMPLE_TEXTURE2D(_GrassTex, sampler_GrassTex, i.lightingUV.zw);
                    float GrassColorValue=_GrassColor.r*_GrassColor.g*0.45; 
                    
                    //return float4(GrassColorValue.xxx,1);
                    
                    float HightValue=step(GrassColorValue,_NormalColor.a)*step(0.01,_NormalColor.a);
                    //return float4(HightValue.xxx,1);
                    mainTex.a=mainTex.a*HightValue;
                }

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
            {   if(_Tree3D==1)
                 {
                    return TreeFrag(i);
                 }else{
                    return DefaultFrag(i);
                 }
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

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
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                #if defined(DEBUG_DISPLAY)
                    SurfaceData2D surfaceData;
                    InputData2D inputData;
                    half4 debugColor = 0;

                    InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                    InitializeInputData(i.uv, inputData);
                    SETUP_DEBUG_DATA_2D(inputData, i.positionWS);

                    if(CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                    {
                        return debugColor;
                    }
                #endif

                return mainTex;
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
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                mainTex.xyz=float3(1,1,1)*mainTex.a; 
                #if defined(DEBUG_DISPLAY)
                    SurfaceData2D surfaceData;
                    InputData2D inputData;
                    half4 debugColor = 0;

                    InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                    InitializeInputData(i.uv, inputData);
                    SETUP_DEBUG_DATA_2D(inputData, i.positionWS);

                    if(CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                    {
                        return debugColor;
                    }
                #endif

                return mainTex;
                
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "MyDepth" "Queue"="Transparent" "RenderType"="Transparent"}
            // BlendOp Max 

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
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = attributes.uv;

                float3 ObjPos=UNITY_MATRIX_M._m03_m13_m23;
                float stepPosZ=step(49,ObjPos.z);
                float3 _objSortPos=ObjPos;
                _objSortPos.y+=_objSortPos.z*(1-stepPosZ);
                float3 worldClip=TransformWorldToHClip(_objSortPos).xyz;

               // worldClip.z=0;
                float positionCSY=o.positionCS.y;
                
                 Unity_Remap_float(worldClip.y,float2(-1,1),float2(0,1),worldClip.y);
                Unity_Remap_float(positionCSY,float2(-1,1),float2(0,1),positionCSY);
 

               // worldClip.y=stepPosZ;
                 worldClip.y=stepPosZ*positionCSY+(1-stepPosZ)*worldClip.y;

                //float myDepth=(ObjPos.y+ObjPos.z+200)/400;
                o.color = worldClip;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 DepthTex = SAMPLE_TEXTURE2D(_DepthTex, sampler_DepthTex, i.uv); 
                half offset=DepthTex.r*512/_ScreenParams.y;


                mainTex.xyz=i.color.yyy-offset; 
                

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
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                #if defined(DEBUG_DISPLAY)
                    SurfaceData2D surfaceData;
                    InputData2D inputData;
                    half4 debugColor = 0;

                    InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                    InitializeInputData(i.uv, inputData);
                    SETUP_DEBUG_DATA_2D(inputData, i.positionWS);

                    if(CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                    {
                        return debugColor;
                    }
                #endif

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
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

          

            float3 WaterFragment(float2 uv,float2 screenUV)
            {
                float2 mirrorUV=screenUV; 

                float3 _WaterMask= SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).xyz; 
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
                float4 _WaveCol0 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap,_TilingAndOffset0); 
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);	
                //波纹2
                float angle1=radians(_WaveAngle1);
                float2 waveValue1=float2(cos(angle1),sin(angle1))*_WaveSpeed1;  
                float2 _WaveT2=(_TimeParameters.x.xx)*waveValue1;				
                float2 _TilingAndOffset1=screenUV*WaveScale1+_WaveT2; 
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
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 moveValue=SAMPLE_TEXTURE2D(_MoveMask,sampler_MoveMask, i.uv);
                moveValue.a=mainTex.a;
                return moveValue;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
