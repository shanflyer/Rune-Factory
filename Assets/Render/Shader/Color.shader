Shader "Unlit/TestColor"
{
    Properties
    {
        _BlitTexture("Texture", 2D) = "white" {}
        _Color("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            // RenderType: <None>
            // Queue: <None>
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalFullscreenSubTarget"
        }
        Cull Off
        Blend Off
        ZTest Off
        ZWrite Off
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardLit" 
            HLSLPROGRAM
            
            // Pragmas
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #define FULLSCREEN_SHADERGRAPH

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

            TEXTURE2D_X(_BlitTexture);  
            float4 _Color;

            float4 GetDrawProceduralVertexPosition(uint vertexID)
            {
                return GetFullScreenTriangleVertexPosition(vertexID, UNITY_NEAR_CLIP_VALUE);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = GetDrawProceduralVertexPosition(v.vertexID); 
                o.uv= o.vertex* 0.5 + 0.5;

                o.uv.y = 1 - o.uv.y;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            { 
                //return float4(_ScreenSize.xy,0,1);
                // sample the texture
                uint2 pixelCoords = uint2(i.uv.xy * _ScreenSize.xy);
                float4 col =LOAD_TEXTURE2D_X_LOD(_BlitTexture, pixelCoords, 0);
                col=col*_Color;
                return col;
            }
            ENDHLSL
        }
    }
}
