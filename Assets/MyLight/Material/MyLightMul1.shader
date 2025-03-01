Shader "MyLight/LightMul1"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _LightColor("LightColor",Color)=(0,0,0,0)
        _MaxSize("MaxSize",Range(0,1))=1
        _MinSize("MinSize",Range(0,1))=0
        _normalMapDistance("normalMapDistance",float)=0 
        [Toggle]_BlendTex("BlendTex",int)=1
        [Toggle] _PointLight("PointLight",int)=1
        [Toggle] _NormalLight("NormalLight",int)=1
        [Toggle] _PivotNormal("_PivotNormal",int)=0

    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha One, One OneMinusSrcAlpha
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
            float _normalMapDistance;
            int _BlendTex;
            int _PointLight;
            int _NormalLight;
            int _PivotNormal;
            CBUFFER_END 

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex); 
            TEXTURE2D(_Normalmap);
            SAMPLER(sampler_Normalmap);
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
                half3 uv1 : TEXCOORD1;
                float4 color        : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; 
                half3 uv1 : TEXCOORD1;
                float4 screenUV:TEXCOORD4; 
                float4 vertex : SV_POSITION;
                half2 ObjectPosition:TEXCOORD2;
                half4 lightDirection:TEXCOORD3;
                
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
                o.screenUV=ComputeScreenPos(o.vertex); 
                o.ObjectPosition=v.vertex.xy;

                float3 worldPos=TransformObjectToWorld(v.vertex);
                float3 objPos=unity_ObjectToWorld._m03_m13_m23; 
                float3 pointPos=TransformObjectToWorld(v.uv1);
                
                int stepX=1-step(v.uv1.x,0);
                _PointLight+=_PivotNormal;
                _PointLight=clamp(_PointLight,0,1);

                objPos=(objPos*(1-stepX)+pointPos*v.uv1.x)*(1-_PointLight)+_PointLight*pointPos;
                worldPos=_PointLight*worldPos+(1-_PointLight)*(objPos*(1-stepX)+worldPos*stepX);

                half3 planeNormal = -GetViewForwardDir();
                half3 projLightPos = objPos.xyz - (dot(objPos-worldPos, planeNormal) - _normalMapDistance) * planeNormal;
                o.lightDirection.xyz = projLightPos - worldPos.xyz; 

                o.lightDirection.xyz=normalize(o.lightDirection.xyz);
                o.lightDirection.w = 0;
                o.uv1=v.uv1;

                o.color=v.color*((1-_BlendTex)+_BlendTex* unity_SpriteColor*_LightColor);

                return o;
            }


            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //half4 col =SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,i.uv);  
                half distance=length(i.ObjectPosition.xy);
                half value=1;
                Unity_Remap_float(distance,float2(0.5,0),float2(_MinSize,_MaxSize),value);
                

                value=clamp(value,0,1)*_PointLight+(1-_PointLight)*clamp(i.uv,0,1);

                float2 screenUV=i.screenUV.xy/i.screenUV.w;
                screenUV=UnityStereoTransformScreenSpaceTex(screenUV);

                half4 normal_col =SAMPLE_TEXTURE2D(_Normalmap, sampler_Normalmap,screenUV);
                half3 normalUnpacked = UnpackNormalRGBNoScale(normal_col);
                half3 dirToLight = normalize(i.lightDirection.xyz); 
               

                half4 lightColor = i.color*value;   
                lightColor = lightColor * saturate(dot(dirToLight, normalUnpacked))*_NormalLight+(1-_NormalLight)*lightColor;
 
                lightColor=(1-_BlendTex)*lightColor+_BlendTex*lightColor;
                lightColor.xyz*=0.25;

                return lightColor; 
            }
            ENDHLSL
        }
    }
}
