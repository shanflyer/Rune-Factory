Shader "MySprite-Lit-Default"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _WaterMaskTex("WaterMaskTex", 2D) = "black" {}

        _WaterNormalMap("WaterNormalMap", 2D) = "bump" {} 
        _NormalMap("Normal Map", 2D) = "bump" {}
        _WetValue("WetValue",Range(0,1))=0
        _shadowStep("ShadowStep",int)=0


        _Water("Water",int)=0
        
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
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"

         half4 GlobalColor; 
         half2 LightDirection;
         half _ShadowValue;
        CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_ST;
            half4 _NormalMap_ST;  // Is this the right way to do this?
            half4 _Color;
            half _WetValue;
            int _shadowStep;
            int _Water;
            
            half4 waterColor;
            half _WaterZero;
            half _WaterBottom;   
            half _WaveAngle0;
            half _WaveSpeed0;
            half _WaveAngle1;
            half _WaveSpeed1; 
            half _EdgeSpeed;
            half _EdgeOffset;  
            half WaveColorValue;   
            half waterNoiseScale;
            half waterValue;   
            half2 WaveScale0;
            half2 WaveScale1; 
            half4 EdgeColor;
            half EdgeValue;  
            half _WaterHigh;  
                      
        CBUFFER_END 
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_MaskTex);
        SAMPLER(sampler_MaskTex);
        TEXTURE2D(_WaterMaskTex);
        SAMPLER(sampler_WaterMaskTex);

        TEXTURE2D(_WaterNormalMap);
        SAMPLER(sampler_WaterNormalMap);

        TEXTURE2D(_ShadowTex);
        SAMPLER(sampler_ShadowTex);
        

        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

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
                float3  worldPos : TEXCOORD4;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            float3 WaterFragment(float2 uv,float2 screenUV,float4 _MainTexColor)
            {
                float3 _WaterMask= SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).xyz; 
                //水域范围
                float stepMask=step(0.04,_WaterMask.r);

                
                float svalue =_ScreenParams.y/ 1136;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*400/_ScreenParams.xy;
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
                float3 endWaveColor=edge*EdgeColor*waveBlendCol+waveBlendCol*_WaterMask1.rrr;
                endWaveColor=clamp(endWaveColor,0,1);
                
                

                //主颜色
                float3 _MainColor=waterColor.xyz*waterColor.a;	 
                _MainColor+=(1-waterColor.a)*_MainTexColor.xyz;
                _MainColor.xyz*=_WaterMask.r;

                float3 outWater=endWaveColor+_MainColor; 
                outWater=clamp(outWater,0,1);   
                
                outWater=outWater+waterColor.xyz*waterColor.a;
                outWater=stepMask*outWater+_MainTexColor.xyz*(1-stepMask);
                return outWater;
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

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.worldPos=UNITY_MATRIX_M._m03_m13_m23;
                o.worldPos.z+=o.worldPos.y;
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
                

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                const half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                //const half4 water = SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, i.uv);
                
                float3 waterColor=main.xyz;
                if(_Water==1)
                {
                    waterColor=WaterFragment(i.uv,i.lightingUV,main);
                }
                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(waterColor, main.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);

                half4 result=CombinedShapeLightShared(surfaceData, inputData);
                half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, i.lightingUV); 
                half3 shadowColor=GlobalColor*shadow.r*GlobalColor.a; 

                half3 shadowResult=shadowColor*result.xyz+result.xyz*(1-shadow.r);  
                result.xyz=result.xyz*(1-_shadowStep)+shadowResult*_shadowStep;     
                
                //result.xyz=waterColor.xyz; 

                return result;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile _ SKINNED_SPRITE

            struct Attributes
            {
                float3 positionOS   : POSITION;
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
                half3   screenUV : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float3 WaterFragment(float2 uv,float2 screenUV,float3 _MainTexColor)
            {
                float3 _WaterMask= SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).xyz; 
                //水域范围
                float stepMask=step(0.04,_WaterMask.r);

                
                float svalue =_ScreenParams.y/ 1136;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*400/_ScreenParams.xy;
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
                

                //波纹与边缘混合 
                //映射水面深度
                Unity_Remap_float(_WaterMask.r,float2(0,1),float2(_WaterZero,_WaterBottom),_WaterMask.r);
                _WaterMask.r=clamp(_WaterMask.r,0,1);
                

                float _WaterMask1=step(_WaterHigh,_WaterMask.r);	  
                stepMask*=_WaterMask1;

                float3 outWater=_WaveCol0.rgb +_WaveCol1.rgb;
                outWater=clamp(outWater,0,1);


                return stepMask*outWater+_MainTexColor.xyz*(1-stepMask); 
                
            }
            

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);

                o.screenUV.xy=o.positionCS.xy;        
                o.screenUV.z=unity_SpriteProps.x;       
                Unity_Remap_float(o.screenUV.x,float2(-1,1),float2(0,1),o.screenUV.x);
                Unity_Remap_float(o.screenUV.y,float2(-1,1),float2(1,0),o.screenUV.y);
                
                o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap);
                o.color = attributes.color;
                o.normalWS = -GetViewForwardDir();
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                const half4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                // normalTS=WaterFragment(i.uv,i.screenUV,normalTS);
                half4 result=NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                result.x=unity_SpriteProps.x*result.x+(1-unity_SpriteProps.x)*(1-result.x);
                result.z=0;
                return result;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

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
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

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
                float3 worldPos=mul(m_Data, float4(attributes.positionOS, 1.0));
                 
                float scaleZ=UNITY_MATRIX_M._m22*LightDirection.y;
                scaleZ+=  scaleZ*abs(lightAngleValue)*0.5*LightDirection.y;

                float2 offset=scaleZ.xx*float2(sin(LightDirection.x),cos(LightDirection.x));
                worldPos.xy+=offset;
                
                o.positionCS = TransformWorldToHClip(worldPos);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = worldPos;
                #endif
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
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
            BlendOp Max 

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

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
                float  color           : COLOR;
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
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);

                float3 ObjPos=UNITY_MATRIX_M._m03_m13_m23;
                float myDepth=(ObjPos.y+ObjPos.z+200)/400;
                o.color = myDepth;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                mainTex.xyz=i.color.xxx; 
                

                return mainTex;
                
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
