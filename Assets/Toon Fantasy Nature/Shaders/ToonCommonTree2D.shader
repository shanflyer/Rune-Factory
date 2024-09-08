Shader "Toon/ToonCommonTree2d"
{
    Properties
    {
        _BlendColor("BlendColor",Color)=(0,1,1,1)
        _BlendValue("BlendValue",Range(0,1))=0
		_BlendRmapMin("BlendRmapMin",Range(0,1))=0
        
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1) 
        _MainTex("Diffuse", 2D) = "white" {}

        _WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
		_WindScroll("Wind Scroll", Range( 0 , 3)) = 0.1
		_WindJitter("Wind Jitter", Range( 0 , 3)) = 0.1
		_WindValue("WindValue", Range( 0 , 3)) = 1
        _NormalMap("Normal Map", 2D) = "bump" {}
        [Toggle]_NormalTex("NormalTex",int)=0

        _OutlineWidth("Outline  Width", Range( 0.0000 , 0.5)) = 0.0065
		_OutlineColor("Outline Color", Color) = (0,0,0,0)
        _ScaleValue("ScaleValue", Range(0 , 2)) = 0.5
        _ClipValue("ClipValue",Range(0,2))=0.5
        _Color("_Color",Color)=(1,1,1,1)

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
            float3 _BlendColor;
			float _BlendValue;
			float _BlendRmapMin;
			float _WindValue; 
			float _WindJitter;
			float _WindScroll;  

			float4 _OutlineColor; 
			float _OutlineWidth;
			float _ScaleValue; 
            float _ClipValue;
			float4 _Color;

            half4 _MainTex_ST;
            half4 _NormalMap_ST; 
			 
             int _NormalTex;
        CBUFFER_END
            sampler2D _WindNoiseTexture; 

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

        
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
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION; 
		        half4   color       : COLOR;
				float4 texcoord3 : TEXCOORD3;
				float4 texcoord4 : TEXCOORD4;   
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
                UNITY_SKINNED_VERTEX_COMPUTE(v);

				float4 clipPos = TransformObjectToHClip(v.positionOS.xyz);
				o.texcoord3 =  ComputeScreenPos(clipPos); 
				float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);  
				o.texcoord4.xy = v.texcoord.xy;  
 
				float3 vertexValue = v.normalOS * _ScaleValue * min(clipPos.w , 1.5);

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
				float2 appendResult60 = float2(worldPos.x , worldPos.z)* 0.1; 
				float2 panner63 = _WindScroll * 0.3* _TimeParameters.x + appendResult60;
				float2 panner74 = _TimeParameters.x * _WindJitter * 0.5+ appendResult60  * float2(2,2);

                float4 WindNoise0=pow( tex2Dlod( _WindNoiseTexture, float4( panner63, 0, 0.0) ) , 2.5);
				float4 WindNoise1=tex2Dlod( _WindNoiseTexture, float4( panner74, 0, 0.0) ); 
				float4 WindScroll = WindNoise0*WindNoise1 * v.color;
                vertexValue += WindScroll.rgb*_WindValue;


				v.positionOS.xyz += vertexValue; 
				v.normalOS = v.normalOS; 
				o.positionCS = TransformObjectToHClip(v.positionOS.xyz); 

				return o;
            }
 
           
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"
            half4 frag ( VertexOutput IN ) : SV_Target
			{  
				float4 screenPos = IN.texcoord3;
				float4 screenPosNorm = screenPos / screenPos.w;
				screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPosNorm.z : screenPosNorm.z * 0.5 + 0.5;
				float2 ScreenUV = (screenPosNorm).xy; 
				float4 texColor =SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord4.xy );
			 
			    texColor*=_Color;

                float ColorValue=(texColor.r+texColor.g+texColor.b)/3;
				Unity_Remap_float(ColorValue,float2(0,1),float2(_BlendRmapMin,1),ColorValue);
				texColor.xyz=texColor.xyz*(1-_BlendValue)+_BlendColor*_BlendValue*ColorValue;

				float Alpha = texColor.a;  
                clip(Alpha-_ClipValue);
				SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(texColor,Alpha, float4(0,0,0,0), surfaceData);
                InitializeInputData(IN.texcoord4.xy, ScreenUV, inputData);

                //SETUP_DEBUG_TEXTURE_DATA_2D(inputData, i.positionWS, i.positionCS, _MainTex);

                return CombinedShapeLightShared(surfaceData, inputData); 
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
                UNITY_SKINNED_VERTEX_COMPUTE(v);

				

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
    
                    float3 vertexValue = v.normalOS * _ScaleValue * min(clipPos.w , 1.5);

                    float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                    float2 appendResult60 = float2(worldPos.x , worldPos.z)* 0.1; 
                    float2 panner63 = _WindScroll * 0.3* _TimeParameters.x + appendResult60;
                    float2 panner74 = _TimeParameters.x * _WindJitter * 0.5+ appendResult60  * float2(2,2);

                    float4 WindNoise0=pow( tex2Dlod( _WindNoiseTexture, float4( panner63, 0, 0.0) ) , 2.5);
                    float4 WindNoise1=tex2Dlod( _WindNoiseTexture, float4( panner74, 0, 0.0) ); 
                    float4 WindScroll = WindNoise0*WindNoise1 * v.color;
                    vertexValue += WindScroll.rgb*_WindValue;


                    v.positionOS.xyz += vertexValue; 
                    v.normalOS = v.normalOS; 
                    o.positionCS = TransformObjectToHClip(v.positionOS.xyz); 
                    float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                    o.normalWS.xyz =  normalWS;  
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
