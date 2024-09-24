Shader "ScreenCloud"
{
    Properties
    { 
        _CloudValue("CloudValue",float)=1
        _WindDir("WindDir",vector)=(1,1,0,0) 
        _NoiseSet0("NoiseSet0",vector)=(0,1,10,1)
        _NoiseSet1("NoiseSet1",vector)=(0,1,10,1)
        _cloudColor("CloudColor",Color)=(1,1,1,0.5)
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
          Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"
            
        CBUFFER_START(UnityPerMaterial)
            float4 _WindDir;
            float4 _NoiseSet0;
            float4 _NoiseSet1;
            float4 _cloudColor;
            float _CloudValue;
        CBUFFER_END

      
        ENDHLSL
        Pass
        {
            HLSLPROGRAM

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
                float4 uv1:TEXCOORD1;
            };
 
             

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv = (0.5-o.vertex*0.5);
               

                float2 noiseUV=o.uv+_TimeParameters.x*_WindDir.xy*_NoiseSet0.w;
                float2 noiseUV1=o.uv+_TimeParameters.x*_WindDir.zw*_NoiseSet1.w;
                o.uv1=float4(noiseUV.xy,noiseUV1.xy);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {  
                float noise0;
                Unity_SimpleNoise_float(i.uv1.xy,_NoiseSet0.z,noise0);
                float noise1;
                Unity_SimpleNoise_float(i.uv1.zw,_NoiseSet1.z,noise1);

                noise0*=_CloudValue;
                noise1*=_CloudValue;
                Unity_Remap_float(noise0,_NoiseSet0.xy,float2(0,1),noise0);
                Unity_Remap_float(noise1,_NoiseSet1.xy,float2(0,1),noise1);
                noise0=clamp(noise0,0,1);
                noise1=clamp(noise1,0,1);
                float cloud=noise0+noise1;
                cloud=clamp(cloud,0,1);
                float4 cloudColor=cloud*_cloudColor; 

                return  cloudColor;
            }
            ENDHLSL
        }
    }
}
