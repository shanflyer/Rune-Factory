Shader "Toon/ToonCommonTree2d"
{
    Properties
    {
        _BlendColor("BlendColor",Color)=(0,1,1,1)
        _BlendValue("BlendValue",Range(0,1))=0
		_BlendRmapMin("BlendRmapMin",Range(0,1))=0
         
        _MainTex("Diffuse", 2D) = "white" {}
        _SnowTex("Snow", 2D) = "black" {}
        _SnowRange("SnowRange",vector)=(0,1,0,1) 
        _SnowColor("SnowColor",color)=(1,1,1,1)

        _WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
		_WindScroll("Wind Scroll", Range( 0 , 3)) = 0.1
		_WindJitter("Wind Jitter", Range( 0 , 3)) = 0.1
		_WindValue("WindValue", Range( 0 , 3)) = 1
        
        _PlantSpringColor("_PlantSpringColor",color)=(0,0,0)
        _PlantSpringColor1("_PlantSpringColor1",color)=(0,0,0)
        _PlantAutumnColor0("_PlantAutumnColor0",color)=(0,0,0)
        _PlantAutumnColor1("_PlantAutumnColor1",color)=(1,1,1)
        _PlantWinterColor("_PlantWinterColor",color)=(0,0,0)
        _PlantWinterColor1("_PlantWinterColor1",color)=(0,0,0) 
        
        _PlantAutumnNoiseScale("_PlantAutumnNoiseScale",float)=1
        [Toggle]_PlantAutumnBlend("_PlantAutumnBlend",int)=0 
        //_SeasonValue("_SeasonValue",Range(0,4))=0

        _NormalMap("Normal Map", 2D) = "bump" {}
        [Toggle]_NormalTex("NormalTex",int)=0
  
        _ScaleValue("ScaleValue", Range(0 , 2)) = 0.5 
        _ClipValue("ClipValue",Range(0,2))=0.5 

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
        float _SeasonValue;
         CBUFFER_START(UnityPerMaterial)
            float3 _BlendColor;
			float _BlendValue;
			float _BlendRmapMin;
			float _WindValue; 
			float _WindJitter;
			float _WindScroll;
           
            half3 _PlantSpringColor1;
            half3 _PlantSpringColor;
            half3 _PlantAutumnColor0;
            half3 _PlantAutumnColor1;
            half3 _PlantWinterColor;
            half3 _PlantWinterColor1;
            int _PlantAutumnBlend;
            float _PlantAutumnNoiseScale; 
            
 
			float _ScaleValue;  
            float _ClipValue; 

            half4 _MainTex_ST;
            half4 _NormalMap_ST; 
			 
             int _NormalTex;
             float4 _SnowRange;
             half4 _SnowColor;
        CBUFFER_END
            sampler2D _WindNoiseTexture; 

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SnowTex);
            SAMPLER(sampler_SnowTex);

        
        ENDHLSL

        Pass
        {
            Name "Forward"
			Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            
            

            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE


            struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
                float4 color : COLOR;
				float4 uv  : TEXCOORD0; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION; 
		        half4   color       : COLOR;
                float3 worldPos:TEXCOORD0;
				half2   lightingUV  : TEXCOORD3;
				float4 uv : TEXCOORD4;   
                float3 normal:NORMAL;
                UNITY_VERTEX_OUTPUT_STEREO
			};
  
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
			#if USE_SHAPE_LIGHT_TYPE_0
                SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
                SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
                SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
                SHAPE_LIGHT(3)
            #endif 

            VertexOutput vert( VertexInput v)
            {
               VertexOutput o = (VertexOutput)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                //UNITY_SKINNED_VERTEX_COMPUTE(v);

                float4 clipPos = TransformObjectToHClip(v.positionOS.xyz);
				

                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.05,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;          

               
				float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);  
				o.uv.xy = v.uv.xy;   
                Unity_Remap_float3(worldNormal,float2(-1,1),float2(0,1),o.normal); 
               // Unity_Remap_float3(o.normal,float2(0.5,1),float2(0,1),worldNormal);  
				float3 vertexValue = v.normalOS * _ScaleValue* min(clipPos.w , 1.5);

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz); 
				float2 appendResult60 = float2(worldPos.x , worldPos.z)* 0.1; 
				float2 panner63 = _WindScroll * 0.3* _TimeParameters.x + appendResult60;
				float2 panner74 = _TimeParameters.x * _WindJitter * 0.5+ appendResult60  * float2(2,2);

                float4 WindNoise0=pow( tex2Dlod( _WindNoiseTexture, float4( panner63, 0, 0.0) ) , 2.5);
				float4 WindNoise1=tex2Dlod( _WindNoiseTexture, float4( panner74, 0, 0.0) ); 
				float4 WindScroll = WindNoise0*WindNoise1 * v.color;
                vertexValue += WindScroll.rgb*_WindValue*(1-s_w); 
		  
                o.worldPos=worldPos;
                v.positionOS.xyz += vertexValue; 
				o.positionCS =TransformObjectToHClip(v.positionOS.xyz); //TransformWorldToHClip(worldPos); 
                o.lightingUV   = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
				return o;
            }
 
           
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"
            half4 frag ( VertexOutput IN ) : SV_Target
			{   
				float2 ScreenUV = IN.lightingUV; 
				float4 texColor =SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv.xy );
  

                float ColorValue=(texColor.r+texColor.g+texColor.b)/3;
				Unity_Remap_float(ColorValue,float2(0,1),float2(_BlendRmapMin,1),ColorValue);
				texColor.xyz=texColor.xyz*(1-_BlendValue)+_BlendColor*_BlendValue*ColorValue; 

                float noiseValue;
                Unity_SimpleNoise_float(IN.worldPos.xy,_PlantAutumnNoiseScale,noiseValue); 
                float noiseValue1;
                Unity_SimpleNoise_float(IN.worldPos.xy,_PlantAutumnNoiseScale*2,noiseValue1); 
                
                int springBlend=1-step(1,_SeasonValue);
                
                float w_s=_SeasonValue;
                Unity_Remap_float(w_s,float2(0,0.25),float2(0,1),w_s); 
                int w_sBlend=1-step(0.5,w_s);
                
                float springValue=_SeasonValue;
                Unity_Remap_float(springValue,float2(0.25,0.5),float2(0,1),springValue);
                springValue=clamp(springValue,0,1);

                float3 winterColor=(_PlantWinterColor*noiseValue+_PlantWinterColor1*(1-noiseValue))*ColorValue; 
                float3 springColor0=(_PlantWinterColor*noiseValue1+_PlantSpringColor*(1-noiseValue1))*ColorValue; 
                float3 springColor=(_PlantSpringColor*noiseValue+_PlantSpringColor1*(1-noiseValue))*ColorValue; 
                springColor=(winterColor*(1-w_s)+springColor0*w_s)*w_sBlend
                          +(1-w_sBlend)*(springColor0*(1-springValue)+springColor*springValue);

                //return float4(springColor.xyz,texColor.a);

                float s_s=_SeasonValue;
                Unity_Remap_float(s_s,float2(1,1.25),float2(0,1),s_s);
                s_s=clamp(s_s,0,1);
                float3 summerColor=springColor*(1-s_s)+_BlendColor*s_s*ColorValue;

                //return float4(summerColor.xyz,texColor.a);
                
                float s_a=_SeasonValue;
                Unity_Remap_float(s_a,float2(2,2.25),float2(0,1),s_a);
                s_a=clamp(s_a,0,1); 

                float3 AutumnColor0=(_BlendColor*noiseValue1+_PlantAutumnColor0*(1-noiseValue1))*ColorValue; 
                AutumnColor0=AutumnColor0*s_a+summerColor*(1-s_a);

                float a_a=_SeasonValue;
                Unity_Remap_float(a_a,float2(2.25,2.5),float2(0,1),a_a);
                a_a=clamp(a_a,0,1); 
 
                float3 AutumnColor=(_PlantAutumnColor0*noiseValue+_PlantAutumnColor1*(1-noiseValue))*ColorValue; 
                AutumnColor=AutumnColor*a_a+AutumnColor0*(1-a_a);

                float a_w=_SeasonValue;
                Unity_Remap_float(a_w,float2(2.85,3.15),float2(0,1),a_w);
                a_w=clamp(a_w,0,1); 
                float3 winterColor0=(_PlantAutumnColor0*noiseValue1+_PlantWinterColor1*(1-noiseValue1))*ColorValue; 
                winterColor0=winterColor0*a_w+AutumnColor*(1-a_w);

                 float w_w=_SeasonValue;
                Unity_Remap_float(w_w,float2(3.15,3.35),float2(0,1),w_w);
                w_w=clamp(w_w,0,1); 
                winterColor=winterColor*w_w+winterColor0*(1-w_w);
 
                texColor.xyz=texColor.xyz*(1-_BlendValue)+winterColor*_BlendValue;  
     
               
                //texColor.xyz=((1-IN.normal.y)*texColor.xyz+IN.normal.y)*(1-_NormalTex)+(_NormalTex)*texColor.xyz;
                  
                float4 SnowColor =SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, IN.uv.xy );
                float normalY=IN.normal.y;
                Unity_Remap_float(normalY,float2(0,1),_SnowRange.xy,normalY);
                normalY=clamp(normalY,0,1);
                float snowValue=normalY*(1-_NormalTex); 
                SnowColor=texColor*(1-snowValue)+SnowColor*snowValue; 
                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.1,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;
                texColor=texColor*(1-s_w)+SnowColor*s_w*_SnowColor;


				float Alpha = texColor.a;  
                clip(Alpha-_ClipValue);
				SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(texColor,Alpha, float4(0,0,0,0), surfaceData);
                InitializeInputData(IN.uv.xy, ScreenUV, inputData);

                //SETUP_DEBUG_TEXTURE_DATA_2D(inputData, i.positionWS, i.positionCS, _MainTex);
                float4 result=CombinedShapeLightShared(surfaceData, inputData); 
                result.xyz=clamp(result.xyz,0,1);
                return result;
			}
            ENDHLSL
        }

        Pass
        {   
            Name "Normals"
			Tags { "LightMode"="NormalsRendering"}  

			HLSLPROGRAM 
             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

			#pragma vertex vert
			#pragma fragment frag 

              #pragma multi_compile _ SKINNED_SPRITE

              TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap); 

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 color : COLOR;
				float4 uv : TEXCOORD0; 
                float4 tangent      : TANGENT;
                 UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float3 normalWS : TEXCOORD0; 
                float2 uv : TEXCOORD4;
                half4   color           : COLOR;
                half3   tangentWS       : TEXCOORD2;
                half3   bitangentWS     : TEXCOORD3; 
                UNITY_VERTEX_OUTPUT_STEREO
			};
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"
			VertexOutput vert ( VertexInput v )
			{ 
				VertexOutput o = (VertexOutput)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                //UNITY_SKINNED_VERTEX_COMPUTE(v);

				

                if(_NormalTex==1)
                {
                    v.positionOS.xyz = UnityFlipSprite(v.positionOS.xyz, unity_SpriteProps.xy);
                    o.positionCS = TransformObjectToHClip(v.positionOS); 

                    o.uv = TRANSFORM_TEX(v.uv, _NormalMap);
                    o.color = v.color;
                    o.normalWS = -GetViewForwardDir();
                    //o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                    o.tangentWS = v.tangent.xyz;
                    o.bitangentWS = cross(o.normalWS, o.tangentWS) * v.tangent.w;

                }else
                {
                    float4 clipPos = TransformObjectToHClip(v.positionOS.xyz); 
                    float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);  
                    o.uv.xy = v.uv.xy;  

                    Unity_Remap_float3(worldNormal,float2(-1,1),float2(0,1),o.normalWS); 
                    Unity_Remap_float3(o.normalWS,float2(0.5,1),float2(0,1),worldNormal);  
    
                    float3 vertexValue = v.normalOS * (_ScaleValue) * min(clipPos.w , 1.5);

                    float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                    float2 appendResult60 = float2(worldPos.x , worldPos.z)* 0.1; 
                    float2 panner63 = _WindScroll * 0.3* _TimeParameters.x + appendResult60;
                    float2 panner74 = _TimeParameters.x * _WindJitter * 0.5+ appendResult60  * float2(2,2);

                    float4 WindNoise0=pow( tex2Dlod( _WindNoiseTexture, float4( panner63, 0, 0.0) ) , 2.5);
                    float4 WindNoise1=tex2Dlod( _WindNoiseTexture, float4( panner74, 0, 0.0) ); 
                    float4 WindScroll = WindNoise0*WindNoise1 * v.color;

                    float s_w=0;
                    Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                    s_w=clamp(s_w,0,1);

                    float s_w1=0;
                    Unity_Remap_float(_SeasonValue,float2(0.05,0),float2(0,1),s_w1);
                    s_w1=clamp(s_w1,0,1);
                    s_w+=s_w1; 
                    vertexValue += WindScroll.rgb*_WindValue*(1-s_w);


                    v.positionOS.xyz += vertexValue;  
                    o.positionCS = TransformObjectToHClip(v.positionOS.xyz);   
                }
               
				return o;
			} 

			void frag( VertexOutput IN , out half4 outNormalWS : SV_Target0)
			{ 
            
               float4 texColor =SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv.xy ); 
                if(_NormalTex==1)
                {
                    half3 normalTS;
                    half4 result=half4(1,1,1,1);
                    normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                    result=NormalsRenderingShared(texColor, normalTS, IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz);
                    result.x=unity_SpriteProps.x*result.x+(1-unity_SpriteProps.x)*(1-result.x);
                    result.z=0; 
                    outNormalWS=result*IN.color; 
                }else
                {
                  
                    float3 normalWS = IN.normalWS;
                    float4 SnowColor =SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, IN.uv.xy );
                    float normalY=IN.normalWS.y;
                    Unity_Remap_float(normalY,float2(0,1),_SnowRange.xy,normalY);
                    normalY=clamp(normalY,0,1);
                    //normalWS.y=normalY;
                    float snowValue=normalY; 
                    texColor=texColor*(1-snowValue)+SnowColor*snowValue;
                    //texColor=SnowColor;
                  
                    outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 1);
                    outNormalWS*=texColor.a; 
                   
                    Unity_Remap_float3(outNormalWS.xyz,float2(-1,1),float2(0,1),outNormalWS.xyz); 
                    Unity_Remap_float(outNormalWS.z,float2(0,1),float2(0,0.5),outNormalWS.z); 

                }

              

                clip(texColor.a-_ClipValue);
          
               
			}

			ENDHLSL
        }
    }
    Fallback "Hidden/InternalErrorShader"
}
