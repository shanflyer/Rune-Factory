Shader "MyLight/DirectionLightMul"
{
    Properties
    {
     
      // _TestDir("_TestDir",vector)=(0,0,0,0)

    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha One, One OneMinusSrcAlpha
        //Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite off
		ZTest LEqual

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Assets/Render/Shader/UnityAction.cginc"

            half4 _DirectionColor;
            half3 _Direction;
            half4 _GlobalColor;
            CBUFFER_START(UnityPerMaterial)     
            half3 _TestDir;
            CBUFFER_END

        TEXTURE2D(_BlitTexture);
        SAMPLER(sampler_BlitTexture); 
            TEXTURE2D(_Normalmap);
            SAMPLER(sampler_Normalmap);
        ENDHLSL

        

        Pass
        {
            Tags { "LightMode" = "Lighting"}
            HLSLPROGRAM 
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag 

            struct appdata
            {
                uint vertexID : VERTEXID_SEMANTIC;
                float2 uv : TEXCOORD0;  
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;  
                float4 vertex : SV_POSITION;  
                half3 lightDirection:TEXCOORD3;
            };
            float4 GetDrawProceduralVertexPosition(uint vertexID)
            {
                return GetFullScreenTriangleVertexPosition(vertexID, UNITY_NEAR_CLIP_VALUE);
            }
     

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = GetDrawProceduralVertexPosition(v.vertexID); 
                o.uv = GetFullScreenTriangleTexCoord(v.vertexID);

                o.lightDirection.xyz=normalize(_Direction.xyz); 

                return o;
            }


            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, i.uv);  
                
                 half4 normal_col =SAMPLE_TEXTURE2D(_Normalmap, sampler_Normalmap,i.uv);
                half3 normalUnpacked = UnpackNormalRGBNoScale(normal_col);
                half3 dirToLight = i.lightDirection.xyz;
                 half4 lightColor = _DirectionColor;
                lightColor.xyz = _DirectionColor.xyz*_GlobalColor.xyz * saturate(dot(dirToLight, normalUnpacked));

                // return lightColor;

                lightColor.xyz=lightColor.xyz*lightColor.a*0.25;

                col.xyz+=lightColor;
                col.xyz+=_GlobalColor.xyz*0.25;
                 col.a=1; 
               // half lightValue=(col.x+col.y+col.z)/3*5; 
               // col.xyz=col.xyz*lightValue+_GlobalColor.xyz*(1-lightValue)*0.2; 
                return col; 
            }
            ENDHLSL
        }
    }
}
