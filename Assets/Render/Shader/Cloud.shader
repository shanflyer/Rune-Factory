Shader "Cloud"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
   
        _Color("Tint", Color) = (1,1,1,1)
 
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
         half4 _CloudColor;
         half2 LightDirection;
        CBUFFER_START(UnityPerMaterial)

            half4 _MainTex_ST;
            half4 _Color; 
        CBUFFER_END 
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_MaskTex);
        SAMPLER(sampler_MaskTex); 
        

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
                o.worldPos=UNITY_MATRIX_M._m03_m13_m23;
                o.worldPos.z+=o.worldPos.y;
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); 
                

                o.color = v.color * _Color * unity_SpriteColor*_CloudColor;
                return o;
            }
 

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                const half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                //const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                 
                half4 result=main; 

                return result;
            }
            ENDHLSL
        }

         
    }

    Fallback "Sprites/Default"
}
