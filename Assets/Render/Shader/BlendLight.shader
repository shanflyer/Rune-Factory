Shader "MyLight/BlendLight"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
            CBUFFER_END

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            TEXTURE2D(_LightingTex);
            SAMPLER(sampler_LightingTex);

            struct appdata
            {
                uint vertexID : VERTEXID_SEMANTIC;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 GetDrawProceduralVertexPosition(uint vertexID)
            {
                return GetFullScreenTriangleVertexPosition(vertexID, UNITY_NEAR_CLIP_VALUE);
            }


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = GetDrawProceduralVertexPosition(v.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(v.vertexID);


                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // sample the texture
                float2 uv = i.uv;
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
                float4 lightCol = SAMPLE_TEXTURE2D(_LightingTex, sampler_LightingTex, uv);
                lightCol.xyz *= 4;
                col.xyz *= lightCol.xyz;
                // apply fog 
                return col;
            }
            ENDHLSL
        }
    }
}