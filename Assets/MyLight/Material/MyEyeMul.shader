Shader "MyLight/EyeMul"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _LightColor("LightColor",Color)=(0,0,0,0)
        _MaxSize("MaxSize",Range(0,1))=1
        _MinSize("MinSize",Range(0,1))=0 
        [Toggle]_BlendTex("BlendTex",int)=1
        [Toggle] _PointLight("PointLight",int)=1 

    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        //Blend SrcAlpha One
        BlendOp Max
        //Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite off
		ZTest LEqual

        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Assets/Render/Shader/UnityAction.cginc"
            CBUFFER_START(UnityPerMaterial)     
            half4 _LightColor; 
            half _MaxSize;
            half _MinSize; 
            int _BlendTex;
            int _PointLight; 
            CBUFFER_END 

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);  
        ENDHLSL

        

        Pass
        {
            Tags { "LightMode" = "Lighting"}
            HLSLPROGRAM 
            #pragma vertex vert
            #pragma fragment frag 

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0; 
                float4 color        : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;  
                half2 screenUV:TEXCOORD4; 
                float4 vertex : SV_POSITION;
                half2 ObjectPosition:TEXCOORD2; 
                
                float4 color        : COLOR;
            };

            /*void TRANSFER_NORMALS_LIGHTING(out v2f output,float3 worldSpacePos)
            { 
                half3 planeNormal = -GetViewForwardDir();
                //half3 projLightPos = lightPosition.xyz - (dot(lightPosition.xyz - worldSpacePos.xyz, planeNormal) - lightPosition.w) * planeNormal;

                half3 projLightPos = lightPosition.xyz - (dot(0, planeNormal) - _normalMapDistance) * planeNormal;

                output.lightDirection.xyz = projLightPos - worldSpacePos.xyz;
                output.lightDirection.xyz=normalize(output.lightDirection.xyz);
                output.lightDirection.w = 0;
            }*/
         

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.screenUV=half2(ComputeScreenPos(o.vertex / o.vertex.w).xy);
                o.ObjectPosition=v.vertex.xy;
 
               
            
                o.color=v.color*((1-_BlendTex)+_BlendTex* unity_SpriteColor*_LightColor);

                return o;
            }


            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 col =SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,i.uv);  
                half distance=length(i.ObjectPosition.xy);
                half value=1;
                Unity_Remap_float(distance,float2(0.5,0),float2(_MinSize,_MaxSize),value);
                

                value=clamp(value,0,1)*_PointLight+(1-_PointLight)*clamp(i.uv,0,1);
 
                half4 lightColor = i.color*value;   
                 
                lightColor=(1-_BlendTex)*lightColor+_BlendTex*col*lightColor;
                lightColor.xyz*=0.25;

                return lightColor; 
            }
            ENDHLSL
        }
    }
}
