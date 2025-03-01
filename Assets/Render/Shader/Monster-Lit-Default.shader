Shader "MyGame/Monster-Lit-Default"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _MyMaskTex("MyMask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _LightBlend("LightBlend",float)=1

        _NoiseValue("NoiseValue",int)=500
        _NoiseAlpha("NoiseAlpha",Range(0,1))=1
        _ForceColor("ForceColor",Color)=(0,0,0,0)

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

        #include "UnityAction.cginc"

         // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            CBUFFER_START( UnityPerMaterial )
                int _NoiseValue;
                half _NoiseAlpha;
                half4 _ForceColor; 
                float _LightBlend;

                half4 _MainTex_ST; 
                half4 _NormalMap_ST;  // Is this the right way to do this?
                half4 _Color;
            CBUFFER_END
           TEXTURE2D(_LightingTex);
            SAMPLER(sampler_LightingTex); 
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex); 
            TEXTURE2D(_ObjDepthTex);
            SAMPLER(sampler_ObjDepthTex); 
            Texture2D _MainTex;
            SamplerState sampler_MainTex;  
            Texture2D _DepthTex;
            Texture2D _NormalMap; 
        ENDHLSL

        Pass
        {
           // Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
           // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            

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
                float4   lightingUV  : TEXCOORD1;
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

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.lightingUV = ComputeScreenPos(o.positionCS);

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }
 
            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                const half4 main = i.color * _MainTex.Sample(sampler_MainTex,i.uv); 
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);

                float2 lightingUV=i.lightingUV.xy/i.lightingUV.w;
                lightingUV=UnityStereoTransformScreenSpaceTex(lightingUV); // 处理Y轴翻转和XR适配

                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,lightingUV);
                lightCol.xyz*=4;
                
                half4 result=main;
                result.xyz=main.xyz*lightCol.xyz;
                result.xyz=_LightBlend*result.xyz+(1-_LightBlend)*main.xyz; 
 
                float noise=1;
                Unity_SimpleNoise_float(lightingUV,_NoiseValue,noise);
                result.a*=step(noise,_NoiseAlpha);
                result.xyz=lerp(result.xyz,_ForceColor.xyz,_ForceColor.a);
  
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
                UNITY_VERTEX_OUTPUT_STEREO
            }; 

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
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
                const half4 mainTex = i.color * _MainTex.Sample(sampler_MainTex,i.uv); 
                const half3 normalTS = UnpackNormal(_NormalMap.Sample(sampler_MainTex,i.uv));

                return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }

         Pass
        {
            Tags { "LightMode" = "CharacterDepth" "Queue"="Transparent" "RenderType"="Transparent"} 
            HLSLPROGRAM 

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment 
            #pragma multi_compile _ SKINNED_SPRITE 

            struct Attributes
            {
                float3 positionOS   : POSITION; 
                float2 uv           : TEXCOORD0; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION; 
                float4  screenUV        : TEXCOORD3;
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

                attributes.positionOS = UnityFlipSprite( attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                float3 objWroldPos=TransformObjectToWorld(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = objWroldPos;
                #endif
                o.uv = attributes.uv;

                float3 ObjPos=unity_ObjectToWorld._m03_m13_m23;
                float stepPosZ=step(49,ObjPos.z);

                float3 _objSortPos=ObjPos; 
                float4 worldClip=TransformWorldToHClip(_objSortPos); 
                float high=(1-stepPosZ)*(objWroldPos.y-ObjPos.y)*0.5;
                float positionCSY=o.positionCS.y;  
                worldClip.y=stepPosZ*positionCSY+(1-stepPosZ)*worldClip.y;  
                o.screenUV=ComputeScreenPos(worldClip);  
                o.screenUV.z=clamp(high,0,1);      
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv); 
                float4 DepthTex =_DepthTex.Sample(sampler_MainTex,i.uv); 
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex,i.uv);

                float2 screenUV=i.screenUV.xy/i.screenUV.w;
                screenUV=UnityStereoTransformScreenSpaceTex(screenUV);

                float4 ObjDepthTex=SAMPLE_TEXTURE2D(_ObjDepthTex, sampler_ObjDepthTex, screenUV);
                 
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

               
                half depth=screenUV.y+offset;
                half setpHigh=depthStep_G; 

                half high=i.screenUV.z*(1-setpHigh)+DepthTex.g*2*setpHigh;

              
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
            Tags { "LightMode" = "ObjDepth" }

            HLSLPROGRAM
           // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            

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
                float4   lightingUV  : TEXCOORD1;
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

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;
                o.lightingUV = ComputeScreenPos(o.positionCS);

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }
 
            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                const half4 main = i.color * _MainTex.Sample(sampler_MainTex,i.uv); 
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                float2 lightUV=i.lightingUV.xy/i.lightingUV.w;
                lightUV=UnityStereoTransformScreenSpaceTex(lightUV);
                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,lightUV);
                lightCol.xyz*=4;
                
                half4 result=main;
                result.xyz=main.xyz*lightCol.xyz;
                result.xyz=_LightBlend*result.xyz+(1-_LightBlend)*main.xyz; 
 
                float noise=1;
                Unity_SimpleNoise_float(lightUV,_NoiseValue,noise);
                result.a*=step(noise,_NoiseAlpha);
                result.xyz=0.25;
  
                return result;
            }
            ENDHLSL
        }
       
    }

    Fallback "Sprites/Default"
}
