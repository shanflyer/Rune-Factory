// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon/TFF_CustomToonOutline2D"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_TextureSample("Texture Sample", 2D) = "white" {}
		_TextureRamp("Texture Ramp", 2D) = "white" {}
		_OutlineWidth("Outline  Width", Range( 0.0000 , 0.5)) = 0.0065
		_OutlineColor("Outline Color", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		_ScaleValue("ScaleValue", Range(0 , 2)) = 0.5
        _ClipValue("ClipValue",Range(0,2))=0.5
		_Color("_Color",Color)=(1,1,1,1)
 
	}

	SubShader
	{ 
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
		

		HLSLINCLUDE
		#pragma target 3.0
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

  
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
				float4 ase_texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION; 
		 
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 lightmapUVOrVertexSH : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
			CBUFFER_START(UnityPerMaterial)
			float4 _OutlineColor;
			float4 _TextureSample_ST;
			float _OutlineWidth;
			float _ScaleValue;
			float _ClipValue;
			float4 _Color;
			 
			CBUFFER_END

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

				float4 unityObjectToClipPos98 = TransformWorldToHClip(TransformObjectToWorld(v.positionOS.xyz));

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = ( v.normalOS * _ScaleValue * min( unityObjectToClipPos98.w , 1.5 ) );

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );
 

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
				 
				float2 uv_TextureSample = IN.ase_texcoord4.xy * _TextureSample_ST.xy + _TextureSample_ST.zw;
				float4 tex2DNode85 = tex2D( _TextureSample, uv_TextureSample );
				float3 ase_worldNormal = IN.ase_texcoord6.xyz;
			 
				float dotResult77 = dot( ase_worldNormal , _MainLightPosition.xyz );
				float temp_output_79_0 = (dotResult77*0.495 + 0.5);
				float2 temp_cast_4 = (temp_output_79_0).xx;

                float3 Color = (  tex2DNode85   ) * ( tex2D( _TextureRamp, temp_cast_4 )) .rgb;
                 Color*=_Color.xyz;
				float Alpha = _Color.a;
				float AlphaClipThreshold = 0.5;
				clip( Alpha - _ClipValue );

				 SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(Color,1, float4(0,0,0,0), surfaceData);
                InitializeInputData(uv_TextureSample, ScreenUV75_g2, inputData);

                //SETUP_DEBUG_TEXTURE_DATA_2D(inputData, i.positionWS, i.positionCS, _MainTex);

                return CombinedShapeLightShared(surfaceData, inputData); 
			}
			ENDHLSL
		}

	 
	 
		Pass
		{
			
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float3 normalWS : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _OutlineColor;
			float4 _TextureSample_ST;
			float _OutlineWidth;
			float _ClipValue;
			float4 _Color; 
			CBUFFER_END 

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
				float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

				o.positionCS = TransformWorldToHClip(positionWS);
				o.normalWS.xyz =  normalWS;

				return o;
			}

			 
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			void frag( VertexOutput IN
				, out half4 outNormalWS : SV_Target0 )
			{
				float3 normalWS = IN.normalWS;
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
					clip(_Color.a-_ClipValue);
			}

			ENDHLSL
		}

	
	}
	 
	Fallback "Hidden/InternalErrorShader"
}
 