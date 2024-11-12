Shader "BlendBlur"
{
    Properties
    {    _MainTex("Diffuse", 2D) = "white" {}
         _BlurTex("_BlurTex", 2D) = "white" {}
        _BlurOffsetPos("_BlurOffsetPos",Range(0,0.5))=0
        _ReMapValue("_ReMapValue",vector)=(0,1,0,0)
         _BlurAmount("_BlurAmount", Vector) = (1, 1, 0, 0) 
         _TestIndex("_TestIndex",int)=0
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
             float _BlurOffsetPos;
            float2 _ReMapValue;
            half2 _BlurAmount; 
            
            int _TestIndex;
        CBUFFER_END 
        half3 _PlayerPos;
         TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlurTex);
            SAMPLER(sampler_BlurTex);
        
            TEXTURE2D(_MyDepthTex);
            SAMPLER(sampler_MyDepthTex); 

            TEXTURE2D(_CharacterDepthTex);
            SAMPLER(sampler_CharacterDepthTex); 

            TEXTURE2D(_ObjDepthTex);
            SAMPLER(sampler_ObjDepthTex); 
        

        ENDHLSL

     

        Pass
        {
            Name "ForwardLit" 
            HLSLPROGRAM
            
            // Pragmas
           // #pragma target 3.0
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
               // UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION; 
                float2 playerUV:Normal;
                float2 uv  : TEXCOORD0;  
                float4 uv01 : TEXCOORD2;
                float4 uv23 : TEXCOORD3;
                float4 uv45 : TEXCOORD4; 

               // UNITY_VERTEX_OUTPUT_STEREO
            };

           

           

            v2f vert(appdata_t v)
            {
                v2f OUT;
               // UNITY_SETUP_INSTANCE_ID(v);
               // UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 
                OUT.vertex = GetDrawProceduralVertexPosition(v.vertexID); 
                OUT.uv= half2(ComputeScreenPos(OUT.vertex / OUT.vertex.w).xy);
               // OUT.uv= OUT.uv*_ScreenSize.xy;
                //OUT.uv1=half2(ComputeScreenPos(OUT.vertex / OUT.vertex.w).xy); 

                OUT.uv01 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1);
                OUT.uv23 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 2.0;
                OUT.uv45 =  OUT.uv.xyxy + _BlurAmount.xyxy * float4(1, 1, -1, -1) * 3.0;

                float4 playerCS=TransformWorldToHClip(_PlayerPos);
                OUT.playerUV=half2(ComputeScreenPos(playerCS/playerCS.w).xy); 
               // Unity_Remap_float2(OUT.playerUV,float2(-1,1),float2(0,1),OUT.playerUV);

                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            { 
               // return half4(IN.uv.xy,0,1);
                // uint2 pixelCoords = uint2(i.uv.xy * _ScreenSize.xy);
                half4 color =   SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
              
                half4 BlurColor=1*SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv);
                  /*
                BlurColor += 0.15 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex,IN.uv01.xy); 
               
                BlurColor += 0.15 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex,IN.uv01.zw); 
                
                BlurColor += 0.10 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv23.xy); 
                
                BlurColor += 0.10 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv23.zw); 
                 
                BlurColor += 0.05 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex,IN.uv45.xy); 
               
                BlurColor += 0.05 * SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex, IN.uv45.zw); */
                 

                half centerY=IN.playerUV.y;
                //return half4(IN.playerUV.yyy,1);

                half4 objDepthColor=SAMPLE_TEXTURE2D(_ObjDepthTex,sampler_ObjDepthTex, IN.uv);
                half4 characterDepthColor=SAMPLE_TEXTURE2D(_CharacterDepthTex,sampler_CharacterDepthTex, IN.uv);
                int stepCharacter=step(objDepthColor.r+objDepthColor.g+objDepthColor.b,0);
                half4 myDepthColor=stepCharacter*characterDepthColor+(1-stepCharacter)*objDepthColor;
                //return myDepthColor;

                //return myDepthColor;
                float x=myDepthColor.x;
                Unity_Remap_float(x,float2(centerY+_BlurOffsetPos,1),_ReMapValue.xy,x);
                x=clamp(x,0,1)*step(centerY-_BlurOffsetPos,myDepthColor.x);

               // return half4(BlurColor.xyz,1) ;


                float x1=myDepthColor.x;
                Unity_Remap_float(x1,float2(centerY-_BlurOffsetPos,0),_ReMapValue.xy,x1);
                x1=clamp(x1,0,1)*(1-step(centerY-_BlurOffsetPos,myDepthColor.x));
                x+=x1;
                 

                 //

                color=BlurColor*x+color*(1-x);
                return color;
            }
             ENDHLSL
        }
    }
    
}