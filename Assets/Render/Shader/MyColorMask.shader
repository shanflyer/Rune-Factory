Shader "MyColorMask"
{
	 Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
         _BColor("Color", Color) = (1,1,1,1) 
		 _Speed("Speed",float)=1
    }

    SubShader
    {
        Tags{"Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"

       
        CBUFFER_START(UnityPerMaterial)
            
            half4 _MainTex_TexelSize;
            half4 _MainTex_ST;      
			half4 _BColor;
			half _Speed;
        CBUFFER_END 
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex); 

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
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0; 
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl" 
            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0; 
 
                o.positionCS = TransformObjectToHClip(v.positionOS);  
                o.uv =v.uv;  
				o.color=_BColor;//*abs(sin(_Time.y*_Speed));
 
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                 
                main.xyz=i.color.xyz*i.color.a;
				main.a*=i.color.a;

                return  main;
            }
            ENDHLSL
        }
 
    }

    Fallback "Sprites/Default"
}