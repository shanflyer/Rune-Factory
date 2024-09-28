Shader "NewSampleBlur"
{
    Properties
    { 
        _MainTex("Diffuse", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
         _BlurOffsetPos("_BlurOffsetPos",Range(0,0.5))=0
        _ReMapValue("_ReMapValue",vector)=(0,1,0,0)
         _BlurAmount("_BlurAmount", Vector) = (1, 1, 0, 0) 
        _MyDepthTex("_MyDepthTex",2D) = "white" {}
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
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float _BlurOffsetPos;
                float2 _ReMapValue;
                half2 _BlurAmount; 
                half2 _PlayerPos;
                half4 _Color;
                half4 _TextureSampleAdd;
                half4 _ClipRect; 
                  TEXTURE2D(_MyDepthTex);
                  SAMPLER(sampler_MyDepthTex); 
 
               // sampler2D _MyBlurTex;  

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 
                    OUT.vertex = GetDrawProceduralVertexPosition(v.vertexID); 
                    OUT.uv= OUT.vertex* 0.5 + 0.5; 
                    OUT.uv.y = 1 - OUT.uv.y;
                   // OUT.uv= OUT.uv*_ScreenSize.xy;
                    _BlurAmount.xy=_BlurAmount.xy/_ScreenSize.xy;

                    OUT.uv01 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1);
                    OUT.uv23 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 2.0;
                    OUT.uv45 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 3.0;

                    OUT.color = v.color * _Color;
                    return OUT;
                }

                half4 frag(v2f IN) : SV_Target
                {
                   // uint2 pixelCoords = uint2(i.uv.xy * _ScreenSize.xy);
                    half4 col=SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                    half4 color = 0.40 * col;
                    //return color;
                    
                    color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv01.xy); 
                    color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv01.zw); 
                    color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv23.xy); 
                    color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,IN.uv23.zw); 
                    color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv45.xy); 
                    color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv45.zw); 


                    color *= IN.color;

                    half centerY=1-_PlayerPos.y;
                    half4 myDepthColor=SAMPLE_TEXTURE2D(_MyDepthTex,sampler_MyDepthTex, IN.uv);
                    float x=myDepthColor.x;
                     Unity_Remap_float(x,float2(centerY+_BlurOffsetPos,1),_ReMapValue.xy,x);
                     x=clamp(x,0,1)*step(centerY-_BlurOffsetPos,myDepthColor.x);

                     float x1=myDepthColor.x;
                     Unity_Remap_float(x1,float2(centerY-_BlurOffsetPos,0),_ReMapValue.xy,x1);
                     x1=clamp(x1,0,1)*(1-step(centerY-_BlurOffsetPos,myDepthColor.x));
                     x+=x1;

                    color=color*x+col*(1-x);

                   
                    return color;
                }
             ENDHLSL
        }
    }
    
}