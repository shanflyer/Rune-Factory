Shader "Sample3D"
{
    Properties
    {
        
        _ZWrite("ZWrite", Float) = 0
 
 
         
          
        _Color("Tint", Color) = (1,1,1,1)
        _ClipValue("ClipValue",Range(0,2))=0.5
  
        

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
         
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite on
		ZTest LEqual

        HLSLINCLUDE
        // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
         #include "Assets/Render/Shader/UnityAction.cginc"
 

         half4 GlobalColor; 
         half2 LightDirection; 
        float4 _GlobalColor;
    
        CBUFFER_START(UnityPerMaterial) 
			 
            half4 _Color;  
                      
        CBUFFER_END 
        TEXTURE2D(_LightingTex);
        SAMPLER(sampler_LightingTex);
        
         
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
 
          

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
 

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

                o.positionCS  = TransformObjectToHClip(v.positionOS.xyz);
				 
         
                o.lightingUV   = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
				return o;
            }
 

            
            half4 TreeFrag (Varyings IN) : SV_Target
			{   
				float2 ScreenUV = IN.lightingUV; 
				float4 texColor = _Color;

                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,IN.lightingUV);
                 lightCol.xyz*=4;
                 texColor.xyz*=lightCol.xyz;
				float Alpha = texColor.a;    
                return texColor;
			}

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {  
                return TreeFrag(i);
            } 
            ENDHLSL
        }

      
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}
            
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
 
          

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
 

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

                o.positionCS  = TransformObjectToHClip(v.positionOS.xyz);
				 
         
                o.lightingUV   = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
				return o;
            }
 

            
            half4 TreeFrag (Varyings IN) : SV_Target
			{   
				float2 ScreenUV = IN.lightingUV; 
				float4 texColor = _Color;

                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,IN.lightingUV);
                 lightCol.xyz*=4;
                 texColor.xyz*=lightCol.xyz;
				float Alpha = texColor.a;   
              
                return texColor;
			}

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {  
                return TreeFrag(i);
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
            
           

            Varyings NormalsRenderingVertex(Attributes v)
            {
                Varyings o = (Varyings)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);
                 float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);   

                Unity_Remap_float3(worldNormal,float2(-1,1),float2(0,1),o.normalWS); 
                Unity_Remap_float3(o.normalWS,float2(0.5,1),float2(0,1),worldNormal);  
               
                
                
                 
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);   
				return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 TreeFrag( Varyings IN: SV_Target0)
			{ 
               half4 outNormalWS; 
               float3 normalWS = IN.normalWS; 
                
                outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 1);
                
                Unity_Remap_float3(outNormalWS.xyz,float2(-1,1),float2(0,1),outNormalWS.xyz); 
                Unity_Remap_float(outNormalWS.z,float2(0,1),float2(0,0.5),outNormalWS.z); 

             
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
                 

                worldClip.xy=half2(ComputeScreenPos(worldClip/worldClip.w).xy); 
                
                o.screenUV.xy=half2(ComputeScreenPos(o.positionCS/o.positionCS.w).xy); 
                
                o.color.x=clamp(high,0,1);                 
                o.color.yz= worldClip.xy;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex;  
               
                half depth=i.color.z; 
                
                mainTex=half4(depth,1,0,_Color.a); 
                return mainTex;
                
            }
            ENDHLSL
        }

  
        
       
    }

    Fallback "Sprites/Default"
}
