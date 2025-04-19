Shader "MySpriteShadow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)  
        _DirIndex("DirIndex",int)=0
        _ClearDir("_ClearDir",int)=0
        _ScaleLength("_ScaleLength",float)=0.5
        _OffSetValue("_OffSetValue",float)=0.5
        [Toggle]_HoldScale("_HoldScale",int)=0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        BlendOp Max 
        Cull Off
        ZWrite Off 
        Pass
        { 
            Tags { "LightMode" = "Shadow" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl" 
            #include "Assets/Render/Shader/UnityAction.cginc"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment
            

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;  
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float4  color           : COLOR;
                float2  uv              : TEXCOORD0;  
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half2 LightDirection;
            half _ShadowValue;
            half4 GlobalColor;    
            half2 _Direction;
            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            CBUFFER_START( UnityPerMaterial ) 
                half4 _Color; 
                int _DirIndex;
                int _ClearDir;
                int _HoldScale;
                float _ScaleLength;
                float _OffSetValue;
            CBUFFER_END

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0; 
                
                
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.color=attributes.color* unity_SpriteColor;
                
                o.uv = attributes.uv;   

                 
                return o;
            }

            half4 UnlitFragment(Varyings i) : SV_Target
            {
                half4 mainTex = i.color *_MainTex.Sample(sampler_MainTex,i.uv);  

                _ShadowValue=_ShadowValue*(1-_ClearDir)+_ShadowValue*_ClearDir*
                                         (step(1,_DirIndex)*step(_Direction.x,0)+
                                           step(1,-_DirIndex)*(1-step(_Direction.x,0)));
                mainTex.xyz=mainTex.aaa; 
                 mainTex*=_ShadowValue;
                return mainTex;
            }
            ENDHLSL
        }
    } 
}
