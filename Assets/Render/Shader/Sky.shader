Shader "Sky"
{
    Properties
    {
        _topColor("topColor", Color) = (1,1,1,1)
        _bottomColor("bottomColor", Color) = (1,1,1,1)
        _halfValue("halfValue",float)=0.5
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
         half4 _SkyColor;
         half2 LightDirection;
        CBUFFER_START(UnityPerMaterial)
            half _halfValue;
            half4 _topColor;
            half4 _bottomColor; 
        CBUFFER_END  
        

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
                float2 uv           : TEXCOORD0; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION; 
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
                 
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = v.uv;  
                return o;
            }
 

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                float colorValue0=0;
                Unity_Remap_float(i.uv.y,float2(0,_halfValue),float2(0,0.5),colorValue0);
                float colorValue1=0;
                Unity_Remap_float(i.uv.y,float2(_halfValue,1),float2(0.5,1),colorValue1);
                float setpValue=step(_halfValue,i.uv.y);

                float value=colorValue0*(1-setpValue)+colorValue1*setpValue; 
                float4 result=float4(1,1,1,1);
                result.xyz=_bottomColor.xyz+(_topColor.xyz-_bottomColor.xyz)*value;

                return result;
            }
            ENDHLSL
        }

         
    }

    Fallback "Sprites/Default"
}
