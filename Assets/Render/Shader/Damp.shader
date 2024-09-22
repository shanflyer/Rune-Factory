Shader "Unlit/Damp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DampValue("_DampValue",Range(0,1))=0
        _DampNoise ("_DampNoise", Range(10,200)) =50
		[HDR]_DampColor ("_DampColor", Color) = (1,1,1,1)

        
        [HDR]_WaterColor ("_WaterColor", Color) = (1,1,1,1)

        _HighLighStep("_HighLighStep",Range(0,1))=0

        _HighLightNoise ("_HightLightNoise", Range(10,200)) =50
        [HDR]_HightLightColor ("_HightLightColor", Color) = (1,1,1,1)
        
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


            inline float Unity_SimpleNoise_RandomValue_float (float2 uv)
			{
				return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
			}
			
			inline float Unity_SimpleNnoise_Interpolate_float (float a, float b, float t)
			{
				return (1.0-t)*a + (t*b);
			}
			
			
			inline float Unity_SimpleNoise_ValueNoise_float (float2 uv)
			{
				float2 i = floor(uv);
				float2 f = frac(uv);
				f = f * f * (3.0 - 2.0 * f);
				
				uv = abs(frac(uv) - 0.5);
				float2 c0 = i + float2(0.0, 0.0);
				float2 c1 = i + float2(1.0, 0.0);
				float2 c2 = i + float2(0.0, 1.0);
				float2 c3 = i + float2(1.0, 1.0);
				float r0 = Unity_SimpleNoise_RandomValue_float(c0);
				float r1 = Unity_SimpleNoise_RandomValue_float(c1);
				float r2 = Unity_SimpleNoise_RandomValue_float(c2);
				float r3 = Unity_SimpleNoise_RandomValue_float(c3);
				
				float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
				float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
				float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
				return t;
			}
			void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
			{
				float t = 0.0;
				
				float freq = pow(2.0, float(0));
				float amp = pow(0.5, float(3-0));
				t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
				
				freq = pow(2.0, float(1));
				amp = pow(0.5, float(3-1));
				t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
				
				freq = pow(2.0, float(2));
				amp = pow(0.5, float(3-2));
				t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
				
				Out = t;
			}


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DampValue;
            float _DampNoise;
            float _HighLighStep;
		    float4 _DampColor;
            float4 _WaterColor;

            float4 _HightLightColor;
            float _HighLightNoise;


            float Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax)
			{
				return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
			}
			float4 Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax)
			{
				return  OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
			}
			float3 Unity_Remap_float3(float3 In, float2 InMinMax, float2 OutMinMax)
			{
				return  OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv); 
                float r=col.r*col.r;
                float g=col.g*col.g;
                float b=col.b*col.b;
                float4 _col=float4(r,g,b,col.a);
                
                float _DampNoiseValue;	 

                 float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2; 

               // return float4(svalue,svalue,svalue,1);



				Unity_SimpleNoise_float(i.uv+_WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy,_DampNoise,_DampNoiseValue);
                float4 d=float4(_DampNoiseValue,_DampNoiseValue,_DampNoiseValue,col.a);    


                float4 water=float4(1-_DampNoiseValue,1-_DampNoiseValue,1-_DampNoiseValue,col.a);    
                float waterValue=clamp((_DampValue-0.5),0,0.5)/0.5;      

                 float _HighLightNoiseValue;	
				Unity_SimpleNoise_float(i.uv+_WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy,_HighLightNoise,_HighLightNoiseValue);
                float4 h=float4(_HighLightNoiseValue,_HighLightNoiseValue,_HighLightNoiseValue,col.a);

                

                h*=(1-_DampNoiseValue);       
                             
                float dValue=1-waterValue;
                d*=dValue;               
				d*=d; 
                d=clamp(d,0,1);

               
               
                water*=waterValue;
                water*=water;  
                water=clamp(water,0,1);
                //return water*_WaterColor;

                h*=waterValue;
                h*=step(_HighLighStep,h);
                h*=h;

                h=clamp(h,0,1);
                
                
                float c=(col.r+col.g+col.b)/3; 

                d*=_DampColor*c;
                water*=_WaterColor*c;
                h*=_HightLightColor*c;

               // return h;

               // d=d+h+water;




                _col+=d+water+h;            

               return lerp(col,_col,clamp(_DampValue/0.5,0,1));
  
            }
            ENDCG
        }
    }
}
