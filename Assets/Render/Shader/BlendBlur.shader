Shader "BlendBlur"
{
    Properties
    {   
        _BlurOffsetPos("_BlurOffsetPos",Range(0,0.5))=0
        _ReMapValue("_ReMapValue",vector)=(0,1,0,0)
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
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION; 
                //float2 playerUV:Normal;
                float2 uv  : TEXCOORD0; 
                float2 uv1  : TEXCOORD1; 
                float4 uv01 : TEXCOORD2;
                float4 uv23 : TEXCOORD3;
                float4 uv45 : TEXCOORD4; 

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _BlurOffsetPos;
            float2 _ReMapValue;
            half2 _BlurAmount; 
            half2 _PlayerPos;

            
            TEXTURE2D(_BlurTex);
            SAMPLER(sampler_BlurTex);
        
            TEXTURE2D(_MyDepthTex);
            SAMPLER(sampler_MyDepthTex);
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
                OUT.uv1=half2(ComputeScreenPos(OUT.vertex / OUT.vertex.w).xy); 

                OUT.uv01 =  OUT.uv1.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1);
                OUT.uv23 =  OUT.uv1.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 2.0;
                OUT.uv45 =  OUT.uv1.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 3.0;

               // float4 playerCS=TransformWorldToHClip(_PlayerPos);
                //OUT.playerUV=half2(ComputeScreenPos(playerCS/playerCS.w).xy); 
                // Unity_Remap_float2(OUT.playerUV,float2(-1,1),float2(0,1),OUT.playerUV);

                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            { 
                // uint2 pixelCoords = uint2(i.uv.xy * _ScreenSize.xy);
                half4 color =   LOAD_TEXTURE2D_X_LOD(_BlitTexture, IN.uv,0);
                half4 BlurColor=0.40 *SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv1);
                BlurColor += 0.15 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex,IN.uv01.xy); 
                BlurColor += 0.15 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex,IN.uv01.zw); 
                BlurColor += 0.10 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv23.xy); 
                BlurColor += 0.10 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv23.zw); 
                BlurColor += 0.05 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex,IN.uv45.xy); 
                BlurColor += 0.05 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv45.zw); 

                half centerY=1-_PlayerPos.y;
                //return half4(centerY.xxx,1);

                half4 myDepthColor=SAMPLE_TEXTURE2D(_MyDepthTex,sampler_MyDepthTex, IN.uv1);
                float x=myDepthColor.x;
                Unity_Remap_float(x,float2(centerY+_BlurOffsetPos,1),_ReMapValue.xy,x);
                x=clamp(x,0,1)*step(centerY-_BlurOffsetPos,myDepthColor.x);

                


                float x1=myDepthColor.x;
                Unity_Remap_float(x1,float2(centerY-_BlurOffsetPos,0),_ReMapValue.xy,x1);
                x1=clamp(x1,0,1)*(1-step(centerY-_BlurOffsetPos,myDepthColor.x));
                x+=x1;

                // return half4(x.xxx,1);

                color=BlurColor*x+color*(1-x);
                return color;
            }
             ENDHLSL
        }
    }
    
}