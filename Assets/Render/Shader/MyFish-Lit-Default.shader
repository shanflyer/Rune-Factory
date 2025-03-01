Shader "MyFish-Lit-Default"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}  
        _ZWrite("ZWrite", Float) = 0
  
        _NormalMap("Normal Map", 2D) = "bump" {} 
        _DepthTex("DepthTex", 2D) ="gray"{} 
        _WetValue("WetValue",Range(0,1))=0 
        _LightBlend("LightBlend",float)=1 
        [Toggle]_BackBlend("BackBlend",int)=1
        [Toggle]_BlendVertexColor("BlendVertexColor",int)=0
 
         
         _BlendColor("BlendColor",Color)=(0,1,1,1)
        _BlendValue("BlendValue",Range(0,1))=0
		_BlendRmapMin("BlendRmapMin",Range(0,1))=0 
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
        ZWrite off
		ZTest LEqual

        HLSLINCLUDE
        // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
         #include "Assets/Render/Shader/UnityAction.cginc"

            
            Texture2D _MainTex;
            SamplerState sampler_MainTex;  
            Texture2D _DepthTex;
            Texture2D _NormalMap; 
 

            TEXTURE2D(_LightingTex);
            SAMPLER(sampler_LightingTex); 
 
  

         half4 GlobalColor; 
         half2 LightDirection;
         half _ShadowValue;
         int _backColor;
 
        
         float _DampValue;
        float _DampNoise;
        float _HighLighStep; 
        float4 _DampWaterColor;
        float4 _HightLightColor;
        float _HighLightNoise;

        float4 _GlobalColor;
        half4 _SunColor; 
        float _CloudValue;  
        CBUFFER_START(UnityPerMaterial) 
			float3 _BlendColor;
			float _BlendValue;
			float _BlendRmapMin;
            
            float _ScaleValue;  
            float _ClipValue; 
        
             
            half4 _Color; 
            int _shadowStep; 

            float _LightBlend;
            int _BackBlend;
            int _BlendVertexColor;
  
                      
        CBUFFER_END 
        
         
        ENDHLSL 
         
        Pass
        {
            Tags { "Queue"="Transparent" "RenderType"="Transparent"}
            
            HLSLPROGRAM
              
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
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
                float4   lightingUV  : TEXCOORD1;  
                half2   fixScreenUV: TEXCOORD3;
                float3 normal:NORMAL; 
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            
 

            float3 BlendLightCol(float3 col,float2 screenUV)
            {
                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,screenUV);
                 col*=lightCol.xyz;
                 return col;
            }
 
            Varyings CombinedShapeLightVertex(Attributes v)
            { 
                  Varyings o = (Varyings)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                float4 clipPos = TransformObjectToHClip(v.positionOS.xyz);
				o.uv=v.uv;    
				o.positionCS =TransformObjectToHClip(v.positionOS.xyz); //TransformWorldToHClip(worldPos); 
                o.lightingUV   = ComputeScreenPos(o.positionCS);
				return o;
            }
 

         

            half4 CombinedShapeLightFragment(Varyings IN) : SV_Target
            {   
				float4 texColor = _MainTex.Sample(sampler_MainTex,IN.uv.xy);
                //return texColor;
                float2 lightingUV=IN.lightingUV.xy/IN.lightingUV.w;
                lightingUV=UnityStereoTransformScreenSpaceTex(lightingUV);

                half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,lightingUV);
                lightCol.xyz*=4;
                texColor.xyz=_LightBlend*texColor.xyz*lightCol.xyz+(1-_LightBlend)*texColor.xyz;
				float Alpha = texColor.a;  
                clip(Alpha-_ClipValue); 
              
                return texColor;
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
 
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);   
				return o;
			} 
            

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                 return TreeVert(attributes);
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 TreeFrag( Varyings IN: SV_Target0)
			{ 
               half4 outNormalWS;
               float4 texColor = _MainTex.Sample(sampler_MainTex,IN.uv.xy);  
               float3 normalWS = IN.normalWS;  
                outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 1);
                outNormalWS*=texColor.a; 
                
                Unity_Remap_float3(outNormalWS.xyz,float2(-1,1),float2(0,1),outNormalWS.xyz); 
                Unity_Remap_float(outNormalWS.z,float2(0,1),float2(0,0.5),outNormalWS.z); 

                clip(texColor.a-_ClipValue); 
                return outNormalWS;
			}
            
            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {   return TreeFrag(i);
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
                float2  uv              : TEXCOORD0;
                float4  screenUV        : TEXCOORD1;
                float4  worldSUV        : TEXCOORD3;
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

                float3 ObjPos=unity_ObjectToWorld._m03_m13_m23;
                float stepPosZ=1-step(49,ObjPos.z);

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
                 
                o.worldSUV=ComputeScreenPos(worldClip); 
                
                o.screenUV=ComputeScreenPos(o.positionCS); 
                o.screenUV.z=clamp(high,0,1);      
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv); 
                float4 DepthTex =_DepthTex.Sample(sampler_MainTex,i.uv); 
                float clipA=1-step(DepthTex.a,0);
                DepthTex.xyz*=clipA;
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex,i.uv);

                float2 worldSUV=i.worldSUV.xy/i.worldSUV.w;
                worldSUV=UnityStereoTransformScreenSpaceTex(worldSUV);
                 
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

               
                half depth=worldSUV.y+offset*clearColor;
                half setpHigh=depthStep_G; 

                //return float4(i.color.zzz,mainTex.a);

                half high=i.screenUV.z*(1-setpHigh)+DepthTex.g*2*setpHigh;
                
                mainTex.xyz=half3(depth,high,_NormalColor.g*0.5+stepDepthOne);
               
                

               // half absUv=length(i.screenUV-i.color.yz);
                //int stepMul=step(absUv,0.001)*_Character;
                

                mainTex.a=mainTex.a*(1-stepDepthOne)+DepthTex.a*stepDepthOne;

                //return mainTex.aaaa;
                //clip(mainTex.a);

               // mainTex.xyz=otherStep.xxx;
                

                return mainTex;
                
            }
            ENDHLSL
        }
 
       
    }

    Fallback "Sprites/Default"
}
