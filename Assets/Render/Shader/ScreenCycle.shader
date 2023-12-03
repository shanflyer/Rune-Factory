Shader "ScreenCycle"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "black" {} 
  
        _CycleSize("CycleSize",float)=0
        _Offset("Offset",vector)=(0,0,0,0) 
        _CycleValue("CycleValue",Range(0,1))=0
  
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
            half _CycleSize;
            half2 _Offset; 
            half _CycleValue;
        CBUFFER_END 
        TEXTURE2D(_MainTex); 
        SAMPLER(sampler_MainTex); 

        TEXTURE2D(_MaskTex);
        SAMPLER(sampler_MaskTex); 
        

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
                float2 cycleUV : TEXCOORD1; 
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D_X(_BlitTexture);  

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

                _CycleSize*=1-_CycleValue;

                half screenValue=_ScreenParams.x/_ScreenParams.y;
                half y=o.uv.y-0.5f; 
                half sizeValue= _CycleSize/_ScreenParams.x;
                Unity_Remap_float(y,float2(-screenValue*sizeValue,screenValue*sizeValue),float2(0,1),y);

                half x=o.uv.x-0.5f;
                Unity_Remap_float(x,float2(-sizeValue,sizeValue),float2(0,1),x);

                half2 uv=half2(x,y);
                //uv.y+=0.5f;
                half2 uvValue=o.uv/uv;
                //half2 uvOffset=_Offset/uvValue;
                _Offset.x=_Offset.x/sizeValue/2; 

                 half sizeValueY= _CycleSize/_ScreenParams.y;
                _Offset.y=_Offset.y/sizeValueY/2; 
                o.cycleUV=uv+float2(0.25,0.25/screenValue)/sizeValue-_Offset;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            { 
                //return float4(_ScreenSize.xy,0,1);
                // sample the texture
                uint2 pixelCoords = uint2(i.uv.xy * _ScreenSize.xy);
                float4 col =LOAD_TEXTURE2D_X_LOD(_BlitTexture, pixelCoords, 0);
                  
                float4 cycleColor=SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.cycleUV.xy); 
                float stepX=step(0,i.cycleUV.x)*(1-step(1,i.cycleUV.x));
                float stepY=step(0,i.cycleUV.y)*(1-step(1,i.cycleUV.y)); 
                cycleColor.xyz*=stepX*stepY;
                //float4 result=cycleColor;
                cycleColor.xyz*=cycleColor.a;
                cycleColor.a=1;
                
                cycleColor.xyz*=col.xyz;

                return cycleColor;   
            }
            ENDHLSL
        }

         
    }

    Fallback "Sprites/Default"
}
