Shader "Sun"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "black" {}
        _MoonMask("MoonMask", 2D) = "white" {}
   
        [HDR]_Color("Tint", Color) = (1,1,1,1)
        _RemapMinValue("RemapMinValue",float)=0
        _RemapMaxValue("RemapMaxValue",float)=1
        _ScaleValue("ScaleValue",Range(0,1))=1
 
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        BlendOp Max 
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"

         half4 GlobalColor; 
         half4 _CloudColor;
         half2 LightDirection;
        CBUFFER_START(UnityPerMaterial)
            half _RemapMinValue;
            half _RemapMaxValue;
            half _ScaleValue;
            half4 _MainTex_ST;
            half4 _Color; 
        CBUFFER_END 
        TEXTURE2D(_MainTex);

        SAMPLER(sampler_MainTex);
        TEXTURE2D(_MoonMask);
        SAMPLER(sampler_MoonMask);

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
                

                o.color = v.color * _Color * unity_SpriteColor;
                return o;
            }

            
 

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                //const half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
               
                const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                half maskR=mask.r;
                Unity_Remap_float(maskR,float2(_RemapMinValue,_RemapMaxValue),float2(0,1),maskR);
                maskR=clamp(maskR,0,1);

                half4 mask1 = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv+float2(0.4,0.4));
                half maskR1=mask1.r;
                Unity_Remap_float(maskR1,float2(_RemapMinValue,_RemapMaxValue),float2(0,1),maskR1);
                maskR1=clamp(maskR1,0,1); 
                maskR-=maskR1;

                 
                Unity_Remap_float(i.uv.x,float2(1-_ScaleValue,_ScaleValue),float2(0,1),i.uv.x);
                Unity_Remap_float(i.uv.y,float2(1-_ScaleValue,_ScaleValue),float2(0,1),i.uv.y);  

                half4 moonMask=SAMPLE_TEXTURE2D(_MoonMask, sampler_MoonMask, i.uv);  
                half4 moonMask1=SAMPLE_TEXTURE2D(_MoonMask, sampler_MoonMask, i.uv+float2(0.4,0.4)); 

                half3 moonColor=moonMask.xyz*(moonMask.a-moonMask1.a);
                moonColor+=moonMask.xyz*(1-moonMask.a+moonMask1.a)*0.3;
                moonMask.xyz=moonColor*moonMask.a;

                maskR=(1-step(1,maskR))*maskR;
                moonMask.xyz=moonMask.xyz*(1-maskR)+maskR*i.color.a;
                moonMask.xyz*=i.color;
                moonMask.a+=maskR;
                moonMask.a=clamp(moonMask.a,0,1);
                return moonMask;

               // 
                //moonMask.a*=stepValue;
                
   
            }
            ENDHLSL
        }

         
    }

    Fallback "Sprites/Default"
}
