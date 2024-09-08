// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon/TFF_CustomToonVegetation2D"
{
	Properties
	{
		_BlendColor("BlendColor",Color)=(0,1,1,1)
        _BlendValue("BlendValue",Range(0,1))=0
		_BlendRmapMin("BlendRmapMin",Range(0,1))=0
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_TextureSample("Texture Sample", 2D) = "white" {}
		_TextureRamp("Texture Ramp", 2D) = "white" {}
		_WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
		_WindScroll("Wind Scroll", Range( 0 , 3)) = 0.1
		_WindJitter("Wind Jitter", Range( 0 , 3)) = 0.1
		_WindValue("WindValue", Range( 0 , 3)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		_OutlineWidth("Outline  Width", Range( 0.0000 , 0.5)) = 0.0065
		_OutlineColor("Outline Color", Color) = (0,0,0,0)
 
 
	}

	SubShader
	{ 
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        //ZWrite Off
		 //Cull Back
		// AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 3.0
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
		void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
		{
			Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
		}
		void Unity_Remap_float3(float3 In, float2 InMinMax, float2 OutMinMax, out float3 Out)
		{
			Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
		}
	 
		  
		#endif //ASE_TESS_FUNCS
		ENDHLSL
 
		
		Pass
		{ 
			Name "Forward"
			Tags { "LightMode" = "Universal2D" }

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZWrite Off

			

			HLSLPROGRAM
  
		    #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
			 
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
 

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
 
			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD0;
				#endif
			
			 
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 lightmapUVOrVertexSH : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
             
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
 
			CBUFFER_START(UnityPerMaterial)
			float3 _BlendColor;
			float _BlendValue;
			float _BlendRmapMin;
			float _WindValue;
			
			float4 _TextureSample_ST;
			float _WindJitter;
			float _WindScroll; 
			CBUFFER_END

			sampler2D _WindNoiseTexture;
			sampler2D _TextureSample;
			sampler2D _TextureRamp;


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
			
	 

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				float2 appendResult60 = (float2(ase_worldPos.x , ase_worldPos.z));
				float2 temp_output_61_0 = ( appendResult60 * 0.1 );
				float2 panner63 = ( ( (0.0 + (_WindScroll - 0.0) * (0.3 - 0.0) / (1.0 - 0.0)) * _TimeParameters.x ) * float2( 1,1 ) + temp_output_61_0);
				float2 panner74 = ( ( _TimeParameters.x * (0.0 + (_WindJitter - 0.0) * (0.5 - 0.0) / (1.0 - 0.0)) ) * float2( 1,1 ) + ( temp_output_61_0 * float2( 2,2 ) ));

                float4 WindNoise0=pow( tex2Dlod( _WindNoiseTexture, float4( panner63, 0, 0.0) ) , 2.5 );
				float4 WindNoise1=tex2Dlod( _WindNoiseTexture, float4( panner74, 0, 0.0) );
                 
				float4 WindScroll = WindNoise0*WindNoise1 * v.ase_color;
				
				float4 ase_clipPos = TransformObjectToHClip((v.positionOS).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord3 = screenPos;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.normalOS);
				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( ase_worldNormal, o.lightmapUVOrVertexSH.xyz );
				o.ase_texcoord6.xyz = ase_worldNormal;
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				o.ase_texcoord6.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				//Unity_Remap_float3(WindScroll.rgb,float2(0,1),float2(-0.1,1),WindScroll.rgb);

				float3 vertexValue = WindScroll.rgb*_WindValue;

				

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
                float3 positionWS1=positionWS;
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					positionWS1= vertexValue;
				#else
					positionWS1+= vertexValue;
				#endif

				float4 positionCS = TransformWorldToHClip(positionWS1 );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = positionWS;
				#endif

				 
				o.positionCS = positionCS;

				return o;
			}

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"
			half4 frag ( VertexOutput IN ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
 
 
				float4 screenPos = IN.ase_texcoord3;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 ScreenUV75_g2 = (ase_screenPosNorm).xy;
				float2 ScreenUV91_g2 = ScreenUV75_g2; 
				float2 uv_TextureSample = IN.ase_texcoord4.xy * _TextureSample_ST.xy + _TextureSample_ST.zw;
				float4 tex2DNode134 = tex2D( _TextureSample, uv_TextureSample ); 



				float3 ase_worldNormal = IN.ase_texcoord6.xyz; 
				float dotResult124 = dot( ase_worldNormal , _MainLightPosition.xyz );
				float temp_output_127_0 = (dotResult124*0.495 + 0.5);
				float2 temp_cast_3 = (temp_output_127_0).xx; 
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0; 
				float3 Color =  (tex2DNode134*tex2D( _TextureRamp, temp_cast_3 )).rgb;

				float ColorValue=(Color.r+Color.g+Color.b)/3;
				Unity_Remap_float(ColorValue,float2(0,1),float2(_BlendRmapMin,1),ColorValue);
				Color=Color*(1-_BlendValue)+_BlendColor*_BlendValue*ColorValue;


				 SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(Color, tex2DNode134.a, float4(0,0,0,0), surfaceData);
                InitializeInputData(uv_TextureSample, ScreenUV91_g2, inputData);

                //SETUP_DEBUG_TEXTURE_DATA_2D(inputData, i.positionWS, i.positionCS, _MainTex);

                return CombinedShapeLightShared(surfaceData, inputData); 
			 
			}
			ENDHLSL
		}

		
		 
		Pass
        {
            // Name "DepthNormals"
			Tags { "LightMode"="NormalsRendering"}

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZWrite Off

			HLSLPROGRAM
 

			#pragma vertex vert
			#pragma fragment frag
 

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl" 
 

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TextureSample_ST;
			float _WindJitter;
			float _WindScroll;
			float _WindValue;

			sampler2D _WindNoiseTexture;
			sampler2D _TextureSample;
 
			CBUFFER_END

			

 
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				float2 appendResult60 = (float2(ase_worldPos.x , ase_worldPos.z));
				float2 temp_output_61_0 = ( appendResult60 * 0.1 );
				float2 panner63 = ( ( (0.0 + (_WindScroll - 0.0) * (0.3 - 0.0) / (1.0 - 0.0)) * _TimeParameters.x ) * float2( 1,1 ) + temp_output_61_0);
				float2 panner74 = ( ( _TimeParameters.x * (0.0 + (_WindJitter - 0.0) * (0.5 - 0.0) / (1.0 - 0.0)) ) * float2( 1,1 ) + ( temp_output_61_0 * float2( 2,2 ) ));

                float4 WindNoise0=pow( tex2Dlod( _WindNoiseTexture, float4( panner63, 0, 0.0) ) , 2.5 );
				float4 WindNoise1=tex2Dlod( _WindNoiseTexture, float4( panner74, 0, 0.0) );
                 
				float4 WindScroll = WindNoise0*WindNoise1 * v.ase_color;
			  

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				//Unity_Remap_float3(WindScroll.rgb,float2(0,1),float2(-0.1,1),WindScroll.rgb);

				float3 vertexValue = WindScroll.rgb*_WindValue;

				o.ase_texcoord1= v.ase_texcoord;

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
                float3 positionWS1=positionWS;
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					positionWS1= vertexValue;
				#else
					positionWS1+= vertexValue;
				#endif

				float4 positionCS = TransformWorldToHClip(positionWS1 );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = positionWS;
				#endif
 

			 	v.normalOS = v.normalOS;
				float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.normalWS.xyz =  normalWS;


				o.positionCS = positionCS;
				return o;
			}

			 
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			} 

			void frag( VertexOutput IN , out half4 outNormalWS : SV_Target0 
				 )
			{ 

				float2 uv_TextureSample = IN.ase_texcoord1.xy * _TextureSample_ST.xy + _TextureSample_ST.zw;
				float4 tex2DNode134 = tex2D( _TextureSample, uv_TextureSample );
             
				 float3 normalWS = IN.normalWS;
				 clip(tex2DNode134.a-0.5);
				 outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 1);
				 outNormalWS*=tex2DNode134.a; 
				 Unity_Remap_float3(outNormalWS.xyz,float2(-1,1),float2(0,1),outNormalWS.xyz);
				 Unity_Remap_float(outNormalWS.z,float2(0,1),float2(0,0.5),outNormalWS.z);
              
			}

			ENDHLSL
        }


	
	}
	 
	Fallback "Hidden/InternalErrorShader"
}
 