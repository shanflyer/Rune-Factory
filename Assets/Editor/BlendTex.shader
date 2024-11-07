Shader "Unlit/BlendTex"
{
    Properties
    {
      _MainTex("Diffuse", 2D) = "white" {}
      _Texture1("_Texture1", 2D) = "white" {}
      _Texture2("_Texture2", 2D) = "white" {}
      _Texture3("_Texture3", 2D) = "white" {}
      _Texture4("_Texture4", 2D) = "white" {}
      TextureCount("TextureCount",int)=0
      _TexelSize("_TexelSize",vector)=(0,0,0,0)

    }
    SubShader
    {
         Tags { "Queue"="Transparent" "RenderType"="Transparent" } 
         Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

           Pass
        {
          Tags { "LightMode" = "Universal2D" "Queue"="Transparent" "RenderType"="Transparent"}
             
            HLSLPROGRAM
             

            #pragma vertex DefaultVertex
            #pragma fragment CombinedShapeLightFragment
 
            
 
            Texture2D _MainTex;
            SamplerState sampler_MainTex;  
            Texture2D _Texture1;
            SamplerState sampler_Texture1;  
            Texture2D _Texture2;
            SamplerState sampler_Texture2;  
            Texture2D _Texture3;
            SamplerState sampler_Texture3;  
            Texture2D _Texture4;
            SamplerState sampler_Texture4;  
 
            float4 texoffset[4]; 
            int TextureCount;

            struct Attributes
            {
                float3 positionOS   : POSITION;  
                float2 uv           : TEXCOORD0;  
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION; 
                float2  uv          : TEXCOORD0;  
            };
 
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            Varyings DefaultVertex(Attributes v)
            {
                Varyings o = (Varyings)0; 
 
                o.positionCS = TransformObjectToHClip(v.positionOS); 
                o.uv = v.uv;  
                return o;
            }
            float Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax)
            {
              return  OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            
 

            half4 DefaultFragment(Varyings i) : SV_Target
            {  
                half4 col =_MainTex.Sample(sampler_MainTex,i.uv);  

                float4 offset1=texoffset[0];
                 float4 offset2=texoffset[1];
                 float4 offset3=texoffset[2];
                 float4 offset4=texoffset[3];

                float2 uv1=float2(Unity_Remap_float(i.uv.x,offset1.xz,float2(0,1)),Unity_Remap_float(i.uv.y,offset1.yw,float2(0,1)));
                float2 uv2=float2(Unity_Remap_float(i.uv.x,offset2.xz,float2(0,1)),Unity_Remap_float(i.uv.y,offset2.yw,float2(0,1)));
                float2 uv3=float2(Unity_Remap_float(i.uv.x,offset3.xz,float2(0,1)),Unity_Remap_float(i.uv.y,offset3.yw,float2(0,1)));
                float2 uv4=float2(Unity_Remap_float(i.uv.x,offset4.xz,float2(0,1)),Unity_Remap_float(i.uv.y,offset4.yw,float2(0,1)));

                half4 col1 = _Texture1.Sample(sampler_Texture1, uv1); 
                half4 col2 = _Texture2.Sample(sampler_Texture2, uv2); 
                half4 col3 = _Texture3.Sample(sampler_Texture3, uv3); 
                half4 col4 = _Texture4.Sample(sampler_Texture4, uv4);  

                int stepB1=step(offset1.x,i.uv.x)*(1-step(offset1.z,i.uv.x))*step(offset1.y,i.uv.y)*(1-step(offset1.w,i.uv.y));
                int stepB2=step(offset2.x,i.uv.x)*(1-step(offset2.z,i.uv.x))*step(offset2.y,i.uv.y)*(1-step(offset2.w,i.uv.y));
                int stepB3=step(offset3.x,i.uv.x)*(1-step(offset3.z,i.uv.x))*step(offset3.y,i.uv.y)*(1-step(offset3.w,i.uv.y));
                int stepB4=step(offset4.x,i.uv.x)*(1-step(offset4.z,i.uv.x))*step(offset4.y,i.uv.y)*(1-step(offset4.w,i.uv.y));
                stepB1*=step(1,TextureCount);
                stepB2*=step(2,TextureCount);
                stepB3*=step(3,TextureCount);
                stepB4*=step(4,TextureCount);

                col.xyz=(col.xyz*(1-col1.a)+col1.xyz*col1.a)*stepB1+(1-stepB1)*col.xyz;
                col.a+=col1.a*stepB1;
                col.a=clamp(col.a,0,1);

                col.xyz=(col.xyz*(1-col2.a)+col2.xyz*col2.a)*stepB2+(1-stepB2)*col.xyz;
                col.a+=col2.a*stepB2;
                col.a=clamp(col.a,0,1);

                 col.xyz=(col.xyz*(1-col3.a)+col3.xyz*col3.a)*stepB3+(1-stepB3)*col.xyz;
                col.a+=col3.a*stepB3;
                col.a=clamp(col.a,0,1);

                 col.xyz=(col.xyz*(1-col4.a)+col4.xyz*col4.a)*stepB4+(1-stepB4)*col.xyz;
                col.a+=col4.a*stepB4;
                col.a=clamp(col.a,0,1);
 
                
                return col;
            }
          

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {  
                 return DefaultFragment(i); 
            } 
            ENDHLSL
        }
         
    }
}
