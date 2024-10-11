Shader "ObjectScreenUV"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {} 
 
         
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
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"


             

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
             
        CBUFFER_START(UnityPerMaterial)
		  
                      
        CBUFFER_END 
        
         
        ENDHLSL

         
        Pass
        {
            Tags {"Name"="Universal2D" "LightMode" = "Universal2D" }

            HLSLPROGRAM
             

            #pragma vertex DefaultVertex
            #pragma fragment DefaultFragment
 
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE
            
 
             
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
                float2 uv           : TEXCOORD0; 
                half4   color       : COLOR;  
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            

            Varyings DefaultVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                float3  worldPos=UNITY_MATRIX_M._m03_m13_m23; 
                half4 worldPosCs=TransformWorldToHClip(worldPos);
                o.color.xy=half2(ComputeScreenPos(worldPosCs / worldPosCs.w).xy); 
               
                o.uv = v.uv;
                
                return o;
            }

            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            

            half4 DefaultFragment(Varyings i) : SV_Target
            {
                
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv); 
                main.xy=i.color.xy;
                main.z=0; 

                return main;
            }
          
            ENDHLSL
        }
 
    }

    Fallback "Sprites/Default"
}
