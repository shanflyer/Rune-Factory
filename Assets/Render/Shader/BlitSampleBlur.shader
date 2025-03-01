Shader "BlitSampleBlur"
{
    Properties
    { 
        _MainTex("Diffuse", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurAmount("_BlurAmount", Vector) = (1, 1, 0, 0)  
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
        CBUFFER_START(UnityPerMaterial)
            int4 _BlurAmount; 
                half4 _Color;
                half4 _TextureSampleAdd;
                half4 _ClipRect; 
        CBUFFER_END 
         TEXTURE2D(_MainTex);
         SAMPLER(sampler_MainTex);
        ENDHLSL
     

        Pass
            {
                 Name "ForwardLit" 
                HLSLPROGRAM
                
                // Pragmas
               // #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag

             

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
                   // UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    half4 color    : COLOR;
                    float2 uv  : TEXCOORD0; 

                    float4 uv01 : TEXCOORD2;
                    float4 uv23 : TEXCOORD3;
                    float4 uv45 : TEXCOORD4; 

                   // UNITY_VERTEX_OUTPUT_STEREO
                };
               
                TEXTURE2D_X(_BlitTexture);  
 
               // sampler2D _MyBlurTex;  

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                   // UNITY_SETUP_INSTANCE_ID(v);
                    //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 
                    OUT.vertex = GetDrawProceduralVertexPosition(v.vertexID); 
                    OUT.uv=GetFullScreenTriangleTexCoord(v.vertexID);
                   // OUT.uv.y = 1 - OUT.uv.y;
                   // OUT.uv= OUT.uv*_ScreenSize.xy;
                   // _BlurAmount.xy=_BlurAmount.xy/_ScreenParams.xy;

                    OUT.uv01 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1);
                    OUT.uv23 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 2.0;
                    OUT.uv45 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 3.0;

                    OUT.color = v.color * _Color;
                    return OUT;
                }

                half4 frag(v2f IN) : SV_Target
                {
                    int2 pixelCoords = int2(IN.uv.xy * _ScreenSize.xy);
                    half4 color = 0.4* LOAD_TEXTURE2D_X_LOD(_BlitTexture, pixelCoords , 0);
                    //int2 pixelCoords01=pixelCoords
                    int4 pixelCoords1=pixelCoords.xyxy+_BlurAmount*int4(1, 1, -1, -1);
                    int4 pixelCoords2=pixelCoords.xyxy+_BlurAmount*int4(1, 1, -1, -1)* 2.0;
                    int4 pixelCoords3=pixelCoords.xyxy+_BlurAmount*int4(1, 1, -1, -1)* 3.0;

                    color += 0.15 * LOAD_TEXTURE2D_X_LOD(_BlitTexture,  pixelCoords1.xy , 0);
                    color += 0.15 * LOAD_TEXTURE2D_X_LOD(_BlitTexture,  pixelCoords1.zw , 0);
                    color += 0.10 * LOAD_TEXTURE2D_X_LOD(_BlitTexture,  pixelCoords2.xy , 0); 
                    color += 0.10 * LOAD_TEXTURE2D_X_LOD(_BlitTexture,  pixelCoords2.zw, 0);
                    color += 0.05 * LOAD_TEXTURE2D_X_LOD(_BlitTexture,  pixelCoords3.xy , 0);
                    color += 0.05 * LOAD_TEXTURE2D_X_LOD(_BlitTexture,  pixelCoords3.zw , 0);
                    color.a=1;

                    //color *= IN.color;

                   
                    return color;
                }
             ENDHLSL
        }
    }
    
}