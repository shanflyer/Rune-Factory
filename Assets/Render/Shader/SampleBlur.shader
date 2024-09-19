Shader "SampleBlur"
{
    Properties
    { 
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurAmount("_BlurAmount", Vector) = (1, 1, 0, 0) 
 
 
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

     

        Pass
            {
                 Name "ForwardLit" 
                HLSLPROGRAM
                
                // Pragmas
                #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Assets/Render/Shader/UnityAction.cginc" 

                #define FULLSCREEN_SHADERGRAPH

                 float4 GetDrawProceduralVertexPosition(uint vertexID)
                {
                    return GetFullScreenTriangleVertexPosition(vertexID, UNITY_NEAR_CLIP_VALUE);
                }

                struct appdata_t
                {
                     uint vertexID : VERTEXID_SEMANTIC;
                    float4 color    : COLOR;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    half4 color    : COLOR;
                    float2 uv  : TEXCOORD0; 

                    float4 uv01 : TEXCOORD2;
                    float4 uv23 : TEXCOORD3;
                    float4 uv45 : TEXCOORD4; 

                    UNITY_VERTEX_OUTPUT_STEREO
                };

                half2 _BlurAmount; 
                half4 _Color;
                half4 _TextureSampleAdd;
                half4 _ClipRect; 
 
               // sampler2D _MyBlurTex; 
                TEXTURE2D_X(_BlitTexture);  

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 
                    OUT.vertex = GetDrawProceduralVertexPosition(v.vertexID); 
                    OUT.uv= OUT.vertex* 0.5 + 0.5; 
                    OUT.uv.y = 1 - OUT.uv.y;
                    OUT.uv= OUT.uv*_ScreenSize.xy;


                    OUT.uv01 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1);
                    OUT.uv23 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 2.0;
                    OUT.uv45 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 3.0;

                    OUT.color = v.color * _Color;
                    return OUT;
                }

                half4 frag(v2f IN) : SV_Target
                {
                   // uint2 pixelCoords = uint2(i.uv.xy * _ScreenSize.xy);
                    half4 color = 0.40 * LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv,0);
                    //return color;
                    
                    color += 0.15 * LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv01.xy,0); 
                    color += 0.15 * LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv01.zw,0); 
                    color += 0.10 * LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv23.xy,0); 
                    color += 0.10 * LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv23.zw,0); 
                    color += 0.05 * LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv45.xy,0); 
                    color += 0.05 * LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv45.zw,0); 


                    color *= IN.color;

                   
                    return color;
                }
             ENDHLSL
        }
    }
    
}