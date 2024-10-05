Shader "EffectBlendDepth"
{
    Properties
    {   _MainTex("Diffuse", 2D) = "white" {}
        _BlurOffsetPos("_BlurOffsetPos",Range(0,0.5))=0
        _ReMapValue("_ReMapValue",vector)=(0,1,0,0)
        _Color("_Color", color) = (1, 1, 1, 1) 
        _DistanceRemap("_DistanceRemap",vector)=(0,1,0,1)
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite off
		ZTest LEqual

     

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

          
            struct appdata_t
            {
                float3 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION; 
                float2 playerUV:Normal;
                float2 uv  : TEXCOORD0; 
                float2 uv1  : TEXCOORD1;  
                float2 worldPos  : TEXCOORD2; 
                float4 color:COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _BlurOffsetPos;
            float2 _ReMapValue; 
            half3 _PlayerPos;
            half4 _Color;
            half4 _DistanceRemap;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlurTex);
            SAMPLER(sampler_BlurTex);
        
            TEXTURE2D(_MyDepthTex);
            SAMPLER(sampler_MyDepthTex); 

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT); 
                OUT.vertex =TransformObjectToHClip(v.positionOS); 
                OUT.uv= v.uv;  
                OUT.uv1=half2(ComputeScreenPos(OUT.vertex / OUT.vertex.w).xy); 
                OUT.color=v.color*_Color;  
                OUT.worldPos=TransformObjectToWorld(v.positionOS);               
                
                float4 playerCS=TransformWorldToHClip(_PlayerPos);
                OUT.playerUV=half2(ComputeScreenPos(playerCS/playerCS.w).xy); 
               // Unity_Remap_float2(OUT.playerUV,float2(-1,1),float2(0,1),OUT.playerUV);

                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {  
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv)*IN.color; 

                half centerY=IN.playerUV.y;  
                half4 myDepthColor=SAMPLE_TEXTURE2D(_MyDepthTex,sampler_MyDepthTex, IN.uv1); 
                float x=myDepthColor.x;
                Unity_Remap_float(x,float2(centerY+_BlurOffsetPos,1),_ReMapValue.xy,x);
                x=clamp(x,0,1)*step(centerY-_BlurOffsetPos,myDepthColor.x); 
                float x1=myDepthColor.x;
                Unity_Remap_float(x1,float2(centerY-_BlurOffsetPos,0),_ReMapValue.xy,x1);
                x1=clamp(x1,0,1)*(1-step(centerY-_BlurOffsetPos,myDepthColor.x));
                x+=x1; 

                float nowDistance=distance(IN.worldPos.xy,_PlayerPos.xy);
                half mul_a=1;
                Unity_Remap_float(nowDistance,_DistanceRemap.xy,_DistanceRemap.zw,mul_a);
                mul_a=clamp(mul_a,0,1);
                half resultA=x*mul_a+(1-step(mul_a,0))*mul_a;
                resultA=clamp(resultA,0,1);

                color.a*=resultA;
                return color;
            }
             ENDHLSL
        }
    }
    
}