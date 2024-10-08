Shader "Hidden/Light2D"
{
    Properties
    {
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend [_SrcBlend][_DstBlend]
            ZWrite Off
            ZTest Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local USE_NORMAL_MAP __
            #pragma multi_compile_local USE_ADDITIVE_BLENDING __
            #pragma multi_compile_local USE_VOLUMETRIC __
            #pragma multi_compile_local USE_POINT_LIGHT_COOKIES __
            #pragma multi_compile_local LIGHT_QUALITY_FAST __

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                half2   uv          : TEXCOORD0;
                //half2   screenUV    : TEXCOORD2;
                half2   lookupUV    : TEXCOORD1;  // This is used for light relative direction

                SHADOW_COORDS(TEXCOORD2)
                NORMALS_LIGHTING_COORDS(TEXCOORD3, TEXCOORD4)
                LIGHT_OFFSET(TEXCOORD5)
            };

            TEXTURE2D(_CookieTex);          // This can either be a sprite texture uv or a falloff texture
            SAMPLER(sampler_CookieTex);

            TEXTURE2D(_FalloffLookup);
            SAMPLER(sampler_FalloffLookup);

            TEXTURE2D(_LightLookup);
            SAMPLER(sampler_LightLookup);
            half4 _LightLookup_TexelSize;

#if USE_POINT_LIGHT_COOKIES
            TEXTURE2D(_PointLightCookieTex);
            SAMPLER(sampler_PointLightCookieTex);
#endif

            NORMALS_LIGHTING_VARIABLES
            SHADOW_VARIABLES
            UNITY_LIGHT2D_DATA

            half _InverseHDREmulationScale;
            half4 GlobalColor;
            int blendGloble;

             float _CloudValue;
            float4 _WindDir;
            float4 _NoiseSet0;
            float4 _NoiseSet1; 
            float _WindValue;  

             void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
            void Unity_Remap_float2(float2 In, float2 InMinMax, float2 OutMinMax, out float2 Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }
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
            void Unity_SimpleNoise_float2(float2 UV, float2 Scale, out float Out)
            {
                float t = 0.0;
                
                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3-0));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale.x/freq, UV.y*Scale.y/freq))*amp;
                
                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3-1));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale.x/freq, UV.y*Scale.y/freq))*amp;
                
                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3-2));
                t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x*Scale.x/freq, UV.y*Scale.y/freq))*amp;
                
                Out = t;
            }

            Varyings vert_shape(Attributes a, PerLight2D light)
            {
                Varyings o = (Varyings)0;
                float3 positionOS = a.positionOS;

                positionOS.x = positionOS.x + _L2D_FALLOFF_DISTANCE * a.color.r;
                positionOS.y = positionOS.y + _L2D_FALLOFF_DISTANCE * a.color.g;

                o.positionCS = TransformObjectToHClip(positionOS);
                o.color = _L2D_COLOR * _InverseHDREmulationScale;
                o.color.a = a.color.a;
#if USE_VOLUMETRIC
                o.color.a = _L2D_COLOR.a * _L2D_VOLUME_OPACITY;
#endif

                // If Sprite use UV.
                o.uv = (_L2D_LIGHT_TYPE == 2) ? a.uv : float2(a.color.a, _L2D_FALLOFF_INTENSITY);

                float4 worldSpacePos;
                worldSpacePos.xyz = TransformObjectToWorld(positionOS);
                worldSpacePos.w = 1;
 
                if(_L2D_LIGHT_TYPE!=5)
                {
                     TRANSFER_NORMALS_LIGHTING(o, worldSpacePos, _L2D_POSITION.xyz, _L2D_POSITION.w)
                }else
                {
                     Directional_NORMALS_LIGHTING(o,  _L2D_DIRECTION.xyz, _L2D_POSITION.w)
                }
                TRANSFER_SHADOWS(o)
                return o;
            }

            Varyings vert_point(Attributes a, PerLight2D light)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(a.positionOS);
                output.uv = a.uv;

                float4 worldSpacePos;
                worldSpacePos.xyz = TransformObjectToWorld(a.positionOS);
                worldSpacePos.w = 1;

                float4 lightSpacePos = mul(_L2D_INVMATRIX, worldSpacePos);
                float halfTexelOffset = 0.5 * _LightLookup_TexelSize.x;
                output.lookupUV = 0.5 * (lightSpacePos.xy + 1) + halfTexelOffset;

                TRANSFER_NORMALS_LIGHTING(output, worldSpacePos, _L2D_POSITION.xyz, _L2D_POSITION.w)
                TRANSFER_SHADOWS(output)
                return output;
            }

            Varyings vert(Attributes attributes)
            {

                PerLight2D light;
#if USE_STRUCTURED_BUFFER_FOR_LIGHT2D_DATA
                light = GetPerLight2D(attributes.color);
#endif

                switch (_L2D_LIGHT_TYPE)
                {
                    case 0:
                    case 1:
                    case 5:
                    case 2:
                    {
                        Varyings v = vert_shape(attributes, light);
                        v.lightOffset = attributes.color;
                        return v;
                    }
                    break;
                    case 3:
                    {
                        Varyings v = vert_point(attributes, light);
                        v.lightOffset = attributes.color;
                        return v;
                    }
                     break;
                }

                Varyings v = (Varyings)0;
                return v;
            }

            FragmentOutput frag_shape(Varyings i, PerLight2D light)
            {
                half4 lightColor = i.color;
                lightColor.xyz=lightColor.xyz*GlobalColor.xyz;

                if (_L2D_LIGHT_TYPE == 2)
                {
                    half4 cookie = SAMPLE_TEXTURE2D(_CookieTex, sampler_CookieTex, i.uv);
#if USE_ADDITIVE_BLENDING
                    lightColor *= cookie * cookie.a;
#else
                    lightColor *= cookie;
#endif
                }
                else
                {
#if USE_ADDITIVE_BLENDING
                    lightColor *= SAMPLE_TEXTURE2D(_FalloffLookup, sampler_FalloffLookup, i.uv).r;
#elif USE_VOLUMETRIC
                    lightColor.a = i.color.a * SAMPLE_TEXTURE2D(_FalloffLookup, sampler_FalloffLookup, i.uv).r;
#else
                    lightColor.a = SAMPLE_TEXTURE2D(_FalloffLookup, sampler_FalloffLookup, i.uv).r;
#endif
                }

#if !USE_VOLUMETRIC
                APPLY_NORMALS_LIGHTING(i, lightColor, _L2D_POSITION.xyz, _L2D_POSITION.w);
#endif
                APPLY_SHADOWS(i, lightColor, _L2D_SHADOW_INTENSITY);
                return ToFragmentOutput(lightColor);
            }

            half3 BlendScreenCloudColor(half3 col,half2 screenUV)
            {

                 float svalue =_ScreenParams.y/ 1920;
                svalue=floor(svalue);
                svalue=clamp(svalue,1,svalue);
                svalue/=2;
                float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
                
                float windValue=abs(_WindValue);
                windValue=clamp(windValue,0.1,1);
                _WindDir.xz*=_WindValue;
                _WindDir.yw*=windValue;

                float2 noiseUV=screenUV+_TimeParameters.x*_WindDir.xy*_NoiseSet0.w+offsetUv;
                float2 noiseUV1=screenUV+_TimeParameters.x*_WindDir.zw*_NoiseSet1.w+offsetUv;
                float noise0; 
                Unity_SimpleNoise_float2(noiseUV,float2(_NoiseSet0.z,_NoiseSet0.z*2),noise0);
                float noise1; 
                Unity_SimpleNoise_float2(noiseUV1,float2(_NoiseSet1.z,_NoiseSet1.z*2),noise1);
                
                
                Unity_Remap_float(noise0,_NoiseSet0.xy,float2(0,_CloudValue*2),noise0);
                Unity_Remap_float(noise1,_NoiseSet1.xy,float2(0,_CloudValue*2),noise1);
                noise0=clamp(noise0,0,1);
                noise1=clamp(noise1,0,1);
                float cloud=noise0+noise1; 
                cloud=clamp(cloud,0,1);
                 // return noise1.xxx;
                
                col=col.xyz*(1-cloud.x);
                return col;
            }
            FragmentOutput frag_direct(Varyings i, PerLight2D light)
            {
                half4 lightColor = i.color;
                lightColor.xyz=lightColor.xyz*GlobalColor.xyz;

                if (_L2D_LIGHT_TYPE == 2)
                {
                    half4 cookie = SAMPLE_TEXTURE2D(_CookieTex, sampler_CookieTex, i.uv);
#if USE_ADDITIVE_BLENDING
                    lightColor *= cookie * cookie.a;
#else
                    lightColor *= cookie;
#endif
                }
                else
                {
#if USE_ADDITIVE_BLENDING
                    lightColor *= SAMPLE_TEXTURE2D(_FalloffLookup, sampler_FalloffLookup, i.uv).r;
#elif USE_VOLUMETRIC
                    lightColor.a = i.color.a * SAMPLE_TEXTURE2D(_FalloffLookup, sampler_FalloffLookup, i.uv).r;
#else
                    lightColor.a = SAMPLE_TEXTURE2D(_FalloffLookup, sampler_FalloffLookup, i.uv).r;
#endif
                }

#if !USE_VOLUMETRIC
                APPLY_NORMALS_LIGHTING(i, lightColor, _L2D_POSITION.xyz, _L2D_POSITION.w);
#endif
                APPLY_SHADOWS(i, lightColor, _L2D_SHADOW_INTENSITY);
#if USE_NORMAL_MAP
                lightColor.xyz=BlendScreenCloudColor(lightColor.xyz,i.screenUV);
#endif
                
                return ToFragmentOutput(lightColor);
            }

            FragmentOutput frag_point(Varyings i, PerLight2D light)
            {
                half4 lookupValue = SAMPLE_TEXTURE2D(_LightLookup, sampler_LightLookup, i.lookupUV);  // r = distance, g = angle, b = x direction, a = y direction

                // Inner Radius
                half attenuation = saturate(_L2D_INNER_RADIUS_MULT * lookupValue.r);   // This is the code to take care of our inner radius

                // Spotlight
                half isFullSpotlight = _L2D_INNER_ANGLE == 1.0f;
                half spotAttenuation = saturate((_L2D_OUTER_ANGLE - lookupValue.g + isFullSpotlight) * (1.0f / (_L2D_OUTER_ANGLE - _L2D_INNER_ANGLE)));
                attenuation = attenuation * spotAttenuation;

                half2 mappedUV;
                mappedUV.x = attenuation;
                mappedUV.y = _L2D_FALLOFF_INTENSITY;
                attenuation = SAMPLE_TEXTURE2D(_FalloffLookup, sampler_FalloffLookup, mappedUV).r;

                half4 lightColor = _L2D_COLOR;
#if USE_POINT_LIGHT_COOKIES
                half4 cookieColor = SAMPLE_TEXTURE2D(_PointLightCookieTex, sampler_PointLightCookieTex, i.lookupUV);
                lightColor = cookieColor * _L2D_COLOR;
#endif

#if USE_ADDITIVE_BLENDING || USE_VOLUMETRIC
                lightColor *= attenuation;
#else
                lightColor.a = attenuation;
#endif

#if !USE_VOLUMETRIC
                APPLY_NORMALS_LIGHTING(i, lightColor, _L2D_POSITION.xyz, _L2D_POSITION.w);
#endif
                APPLY_SHADOWS(i, lightColor, _L2D_SHADOW_INTENSITY);

#if USE_VOLUMETRIC
                lightColor *= _L2D_VOLUME_OPACITY;
#endif

                return ToFragmentOutput(lightColor * _InverseHDREmulationScale);
            }

            FragmentOutput frag(Varyings i)
            {

                PerLight2D light;
#if USE_STRUCTURED_BUFFER_FOR_LIGHT2D_DATA
                light = GetPerLight2D(i.lightOffset);
#endif

                switch (_L2D_LIGHT_TYPE)
                {
                    case 0:
                    case 1:
                    case 2:
                    {
                        FragmentOutput output = frag_shape(i, light);
                        return output;
                    }
                    break;
                    case 5:
                    {
                        FragmentOutput output = frag_direct(i, light);
                        return output;
                    }
                    break;
                    case 3:
                    {
                        FragmentOutput output = frag_point(i, light);
                        return output;
                    }
                }

                half4 color = i.color;
                return ToFragmentOutput(color);
            }
            ENDHLSL
        }
    }
}
