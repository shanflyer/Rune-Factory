Shader "MyTree-Lit-Default"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {} 
        _SnowTex("_SnowTex", 2D) = "black" {}
        _ZWrite("ZWrite", Float) = 0
 

        _WaterNormalMap("WaterNormalMap", 2D) = "bump" {} 
        _NormalMap("Normal Map", 2D) = "bump" {}
        _MoveMask("WaterMaskTex", 2D) ="black"{}
        _DepthTex("DepthTex", 2D) ="gray"{} 
        _WetValue("WetValue",Range(0,1))=0 
        _LightBlend("LightBlend",float)=1 
        [Toggle]_BackBlend("BackBlend",int)=1
        [Toggle]_BlendVertexColor("BlendVertexColor",int)=0
 
        _WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
        _WindScroll("Wind Scroll", Range( 0 , 3)) = 0.1
		_WindJitter("Wind Jitter", Range( 0 , 3)) = 0.1
        _WindNoiseValue("WindNoiseValue",Range(0,1))=0 
        [Toggle]_GrassBlend("_GrassBlend",int)=0
        

        _PlantSpringColor("_PlantSpringColor",color)=(0,0,0)
        _PlantSpringColor1("_PlantSpringColor1",color)=(0,0,0)
        _PlantAutumnColor0("_PlantAutumnColor0",color)=(0,0,0)
        _PlantAutumnColor1("_PlantAutumnColor1",color)=(1,1,1)
        _PlantWinterColor("_PlantWinterColor",color)=(0,0,0)
        _PlantWinterColor1("_PlantWinterColor1",color)=(0,0,0)
        //_SeasonValue("_SeasonValue",Range(0,4))=0
        _PlantAutumnNoiseScale("_PlantAutumnNoiseScale",float)=1
        [Toggle]_PlantAutumnBlend("_PlantAutumnBlend",int)=0
 
        [Toggle]_SnowBlend("_SnowBlend",int)=1
        
          
         _BlendColor("BlendColor",Color)=(0,1,1,1)
        _BlendValue("BlendValue",Range(0,1))=0
		_BlendRmapMin("BlendRmapMin",Range(0,1))=0
        _SnowRange("SnowRange",vector)=(0,1,0,1) 
        _SnowColor("SnowColor",color)=(1,1,1,1)
        _ScaleValue("ScaleValue", Range(0 , 2)) = 0.5 
         _ClipValue("ClipValue",Range(0,2))=0.5
  
        

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite off
		ZTest LEqual

        HLSLINCLUDE
        // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
         #include "Assets/Render/Shader/UnityAction.cginc"

            
            Texture2D _MainTex;
            SamplerState sampler_MainTex;  
            Texture2D _DepthTex;
            Texture2D _NormalMap;
            Texture2D _MoveMask;
            Texture2D _GrassTex;
            Texture2D _SnowTex; 

            TEXTURE2D(_WindNoiseTexture);
            SAMPLER(sampler_WindNoiseTexture);

            TEXTURE2D(_LightingTex);
            SAMPLER(sampler_LightingTex);
 
            TEXTURE2D(_ObjDepthTex);
            SAMPLER(sampler_ObjDepthTex); 

            TEXTURE2D(_ShadowTex);
            SAMPLER(sampler_ShadowTex);
            
            TEXTURE2D(_BackMaskTex);
            SAMPLER(sampler_BackMaskTex);
  

         half4 GlobalColor; 
         half2 LightDirection;
         half _ShadowValue;
         int _backColor;

        float _SnowValue;
        
         float _DampValue;
        float _DampNoise;
        float _HighLighStep; 
        float4 _DampWaterColor;
        float4 _HightLightColor;
        float _HighLightNoise;

        float4 _GlobalColor;
        half4 _SunColor;
        float _SeasonValue;
        float _CloudValue;
        float4 _WindDir;
        float4 _NoiseSet0;
        float4 _NoiseSet1; 
        float _WindValue; 
        CBUFFER_START(UnityPerMaterial) 
			float3 _BlendColor;
			float _BlendValue;
			float _BlendRmapMin;
            
            float _ScaleValue;  
            float _ClipValue; 
            float4 _SnowRange;
             half4 _SnowColor;
           
            int _DampBlend;
            int _Damp;
            int _SnowBlend;
            int _GrassBlend; 

            half3 _PlantSpringColor1;
            half3 _PlantSpringColor;
            half3 _PlantAutumnColor0;
            half3 _PlantAutumnColor1;
            half3 _PlantWinterColor;
            half3 _PlantWinterColor1; 
            float _PlantAutumnNoiseScale;  
             
            half4 _Color; 
            int _shadowStep; 

            float _LightBlend;
            int _BackBlend;
            int _BlendVertexColor;

            float _WindJitter;
			float _WindScroll;
            float _WindNoiseValue;
             
            
                      
        CBUFFER_END 
        
         
        ENDHLSL 
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}
            
            HLSLPROGRAM
             

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
 
            
 
             
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float3 normalOS : NORMAL;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                half2   lightingUV  : TEXCOORD1; 
                float4  worldPos : TEXCOORD4;
                half2   fixScreenUV: TEXCOORD3;
                float3 normal:NORMAL;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
           // #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
           
            float2 MoveUV(float2 uv,float2 screenUV,float SnowMove,out float2 offset)
            {
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;

                float2 panner63 = _WindScroll * 0.3 * _TimeParameters.x + screenUV;
				float2 panner74 =_TimeParameters.x * _WindJitter * 0.5  + screenUV *2;

                float4 WindNoise0=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner63);
                WindNoise0=pow(abs(WindNoise0), 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner74);

                float4 moveValue=_MoveMask.Sample(sampler_MainTex,uv);

                float windValue=lerp(1,2,abs(_WindValue));
                //return float2(moveValue.x,moveValue.x);
                float value=moveValue.x*_WindNoiseValue*windValue;
                offset=WindNoise0.x*WindNoise1.x*value*SnowMove;
                int stepWind=step(0,_WindValue);
                offset.x=offset.x*stepWind-offset.x*(1-stepWind);

                return offset+uv;
            }

            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
 

            float3 BlendLightCol(float3 col,float2 screenUV)
            {
                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,screenUV);
                 col*=lightCol.xyz;
                 return col;
            }

            
            Varyings TreeVert(Attributes v)
            {
                Varyings o = (Varyings)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

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

                float4 WindNoise0=pow(SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner63,1) , 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner74,1); 
				float4 WindScroll = WindNoise0*WindNoise1 * v.color;
                float windValue=lerp(0.5,3,abs(_WindValue));
                int stepWind=step(0,_WindValue);
                windValue=-stepWind*windValue+(1-stepWind)*windValue;
                vertexValue += WindScroll.rgb*windValue*(1-s_w); 
		  
                o.worldPos=half4(worldPos.xyz,1);
                v.positionOS.xz += vertexValue; 
                v.positionOS.y+=abs(vertexValue);
				o.positionCS =TransformObjectToHClip(v.positionOS.xyz); //TransformWorldToHClip(worldPos); 
                o.lightingUV   = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);
				return o;
            }

            
            Varyings CombinedShapeLightVertex(Attributes v)
            { 
                 return TreeVert(v);
            }
 

            
            half4 TreeFrag (Varyings IN) : SV_Target
			{   
				float2 ScreenUV = IN.lightingUV; 
				float4 texColor = _MainTex.Sample(sampler_MainTex,IN.uv.xy);
  

                float ColorValue=(texColor.r+texColor.g+texColor.b)/3; 

				Unity_Remap_float(ColorValue,float2(0,1),float2(_BlendRmapMin,1),ColorValue);
				texColor.xyz=texColor.xyz*(1-_BlendValue)+_BlendColor*_BlendValue*ColorValue; 

               

                float noiseValue;
                Unity_SimpleNoise_float(IN.worldPos.xy,_PlantAutumnNoiseScale,noiseValue); 
                float noiseValue1;
                Unity_SimpleNoise_float(IN.worldPos.xy,_PlantAutumnNoiseScale*2,noiseValue1); 
                
                int springBlend=1-step(1,_SeasonValue);
                
                float w_s=_SeasonValue;
                Unity_Remap_float(w_s,float2(0,0.15),float2(0,1),w_s); 
                int w_sBlend=1-step(0.5,w_s);
                
                float springValue=_SeasonValue;
                Unity_Remap_float(springValue,float2(0.15,0.5),float2(0,1),springValue);
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
                  
                float4 SnowColor =_SnowTex.Sample(sampler_MainTex,IN.uv.xy );
              
                float normalY=IN.normal.y;
                  
                Unity_Remap_float(normalY,float2(0,1),_SnowRange.xy,normalY);
                normalY=clamp(normalY,0,1);
                float snowValue=normalY; 

               
                SnowColor=texColor*(1-snowValue)+SnowColor*snowValue; 
                
                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.1,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1;
                texColor=texColor*(1-s_w)+SnowColor*s_w*_SnowColor;

               //texColor.xyz=BlendScreenCloudColor(texColor.xyz,IN.lightingUV);

                 half4 lightCol=SAMPLE_TEXTURE2D(_LightingTex,sampler_LightingTex,IN.lightingUV);
                 lightCol.xyz*=4;
                //texColor.xyz*=lightCol.xyz;
				float Alpha = texColor.a;  
                clip(Alpha-_ClipValue); 
              
                return texColor;
			}

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {  
                return TreeFrag(i);
            } 
            ENDHLSL
        }


        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM 

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            #pragma multi_compile _ SKINNED_SPRITE 

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float3 normalOS : NORMAL;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 tangent      : TANGENT;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                half4   color           : COLOR;
                float2  uv              : TEXCOORD0;
                half3   normalWS        : TEXCOORD1;
                half3   tangentWS       : TEXCOORD2;
                half3   bitangentWS     : TEXCOORD3;
                half4   lightingUV  : TEXCOORD4; 
                //half3   screenUV : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
             float2 MoveUV(float2 uv,float2 screenUV,float SnowMove)
            {
                float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                screenUV+=offsetUv;

                float2 panner63 = _WindScroll * 0.3 * _TimeParameters.x + screenUV;
				float2 panner74 =_TimeParameters.x * _WindJitter * 0.5  + screenUV *2;

                float4 WindNoise0=pow(abs(SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner63)) , 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D( _WindNoiseTexture,sampler_WindNoiseTexture, panner74);

                float4 moveValue=_MoveMask.Sample(sampler_MainTex,uv);
                float windValue=lerp(1,2,abs(_WindValue));
                //return float2(moveValue.x,moveValue.x);
                float value=moveValue.x*_WindNoiseValue*windValue;
                float  offset=WindNoise0.x*WindNoise1.x*value*SnowMove;
                int stepWind=step(0,_WindValue);
                offset.x=offset.x*stepWind-offset.x*(1-stepWind);

                return offset+uv;
            }
            Varyings TreeVert (Attributes v )
			{ 
				Varyings o = (Varyings)0; 
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

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

                float4 WindNoise0=pow(abs(SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner63,1)) , 2.5);
				float4 WindNoise1=SAMPLE_TEXTURE2D_LOD( _WindNoiseTexture,sampler_WindNoiseTexture, panner74,1); 
                float4 WindScroll = WindNoise0*WindNoise1 * v.color;

                float s_w=0;
                Unity_Remap_float(_SeasonValue,float2(2.95,3.05),float2(0,1),s_w);
                s_w=clamp(s_w,0,1);

                float s_w1=0;
                Unity_Remap_float(_SeasonValue,float2(0.05,0),float2(0,1),s_w1);
                s_w1=clamp(s_w1,0,1);
                s_w+=s_w1; 
                float windValue=lerp(0.5,3,abs(_WindValue));
                int stepWind=step(0,_WindValue);
                windValue=-stepWind*windValue+(1-stepWind)*windValue;
                vertexValue += WindScroll.rgb*windValue*(1-s_w); 
                v.positionOS.xz += vertexValue; 
                v.positionOS.y+=abs(vertexValue);  
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);   
				return o;
			} 
            

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                 return TreeVert(attributes);
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 TreeFrag( Varyings IN: SV_Target0)
			{ 
               half4 outNormalWS;
               float4 texColor = _MainTex.Sample(sampler_MainTex,IN.uv.xy);  
               float3 normalWS = IN.normalWS;
                float4 SnowColor =_SnowTex.Sample(sampler_MainTex,IN.uv.xy );
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

                clip(texColor.a-_ClipValue); 
                return outNormalWS;
			}
            
            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {   return TreeFrag(i);
            }
            ENDHLSL
        }
       
      

        Pass
        {
            Tags { "LightMode" = "ObjDepth" "Queue"="Transparent" "RenderType"="Transparent"} 
            HLSLPROGRAM 

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment 
            #pragma multi_compile _ SKINNED_SPRITE 

            struct Attributes
            {
                float3 positionOS   : POSITION; 
                float2 uv           : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float3  color           : COLOR;
                float2  uv              : TEXCOORD0;
                float2  screenUV        : TEXCOORD1;
                #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                attributes.positionOS = UnityFlipSprite( attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                float3 objWroldPos=TransformObjectToWorld(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                    o.positionWS = objWroldPos;
                #endif
                o.uv = attributes.uv;

                float3 ObjPos=UNITY_MATRIX_M._m03_m13_m23;
                float stepPosZ=1-step(49,ObjPos.z);

                float3 _objSortPos=ObjPos; 
                _objSortPos.y+=_objSortPos.z;

               // int stepFixed=1-step(_FixedDepth,0);
               // _objSortPos.y=(1-stepFixed)*_objSortPos.y+stepFixed*_FixedDepth; 

                float4 worldClip=TransformWorldToHClip(_objSortPos); 
                float high=stepPosZ*(objWroldPos.y-ObjPos.y)*0.5;
                float positionCSY=o.positionCS.y;  

                //stepPosZ+=stepFixed;
                stepPosZ=clamp(stepPosZ,0,1);
 
                worldClip.y=(1-stepPosZ)*positionCSY+stepPosZ*worldClip.y;  
                 

                worldClip.xy=half2(ComputeScreenPos(worldClip/worldClip.w).xy); 
                
                o.screenUV.xy=half2(ComputeScreenPos(o.positionCS/o.positionCS.w).xy); 
                
                o.color.x=clamp(high,0,1);                 
                o.color.yz= worldClip.xy;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex =_MainTex.Sample(sampler_MainTex,i.uv); 
                float4 DepthTex =_DepthTex.Sample(sampler_MainTex,i.uv); 
                float clipA=1-step(DepthTex.a,0);
                DepthTex.xyz*=clipA;
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex,i.uv);
                 
                half depthStep_R=step(0.01,abs(DepthTex.r-0.5));
                half depthStep_G=1-step(abs(DepthTex.g-0.5),0.01);
                half depthStep_B=step(0.01,abs(DepthTex.b-0.5));
                half depthStep_ZeroB=step(0.01,DepthTex.b);
                half stepDepthOne=step(1,DepthTex.b);

                int clearColor=1-step(DepthTex.b,0)*step(DepthTex.r,0)*step(DepthTex.g,0);

                half otherStep=depthStep_R*depthStep_G+depthStep_B; 
                otherStep=clamp(otherStep,0,1)*depthStep_ZeroB;
               

                half depthValue=(DepthTex.r-0.5)*(1-otherStep)+(DepthTex.r+DepthTex.b-1)*(1-stepDepthOne)*otherStep; 
                half offset=depthValue*512*4/_ScreenParams.y;

               
                half depth=i.color.z+offset*clearColor;
                half setpHigh=depthStep_G; 

                //return float4(i.color.zzz,mainTex.a);

                half high=i.color.x*(1-setpHigh)+DepthTex.g*2*setpHigh;
                
                mainTex.xyz=half3(depth,high,_NormalColor.g*0.5+stepDepthOne);
               
                

               // half absUv=length(i.screenUV-i.color.yz);
                //int stepMul=step(absUv,0.001)*_Character;
                

                mainTex.a=mainTex.a*(1-stepDepthOne)+DepthTex.a*stepDepthOne;

                //return mainTex.aaaa;
                //clip(mainTex.a);

               // mainTex.xyz=otherStep.xxx;
                

                return mainTex;
                
            }
            ENDHLSL
        }

  
        
        Pass
        {
             Tags { "LightMode" = "Grass" }

            HLSLPROGRAM 
            #pragma vertex  Vertex
            #pragma fragment  Fragment 
  
            struct Attributes
            {
                float3 positionOS   : POSITION; 
                float2 uv           : TEXCOORD0; 
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION; 
                float2  uv          : TEXCOORD0; 
                UNITY_VERTEX_OUTPUT_STEREO
            }; 

            Varyings  Vertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS); 
                o.uv = v.uv;
                 
                return o;
            }
 

            half4 Fragment(Varyings i) : SV_Target
            {
                half4 mainTex = _MainTex.Sample(sampler_MainTex,i.uv); 
                float4 moveValue=_MoveMask.Sample(sampler_MainTex,i.uv);
                moveValue.a=mainTex.a;
                return moveValue;
            }
            ENDHLSL
        }

       
    }

    Fallback "Sprites/Default"
}
