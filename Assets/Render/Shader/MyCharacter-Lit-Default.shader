Shader "MyCharacter-Lit-Default"
{
    Properties
    {
        [HideInInspector] _FeatureFlags ("Feature Flags", Int) = 0

        _Color("Color", Color) = (1,1,1,1)
        _FixedColor("FixedColor",color) = (1,1,1,0)
        _MainTex("Diffuse", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _DepthTex("DepthTex", 2D) ="gray" {}
        [Toggle]_BlendVertexColor("BlendVertexColor",int)=0

        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
             "RenderType"="Opaque"
             "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite On
        ZTest LEqual

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

        Texture2D _MainTex;
        SamplerState sampler_MainTex;
        Texture2D _DepthTex;
        Texture2D _NormalMap;

        TEXTURE2D(_ObjDepthTex);
        SAMPLER(sampler_ObjDepthTex);

        half4 GlobalColor;
        half2 LightDirection;

        CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _FixedColor;
            float _BlendVertexColor;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 color : COLOR;
            float2 uv : TEXCOORD0;
            float4 tangent : TANGENT;
            UNITY_SKINNED_VERTEX_INPUTS
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            half4 color : COLOR;
            float2 uv : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            half3 tangentWS : TEXCOORD2;
            half3 bitangentWS : TEXCOORD3;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        struct SurfaceInput
        {
            float2 uv;
            half4 mainTex;
        };

        float3 ApplyYSortToWorldPos(float3 worldPos)
        {
            return worldPos;
        }

        Varyings DefaultVertex(Attributes attributes)
        {
            Varyings o = (Varyings)0;

            UNITY_SETUP_INSTANCE_ID(attributes);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            UNITY_SKINNED_VERTEX_COMPUTE(attributes);

            float3 objectWorldPos = TransformObjectToWorld(attributes.positionOS);
            float3 sortedWorldPos = ApplyYSortToWorldPos(objectWorldPos);
            o.positionCS = TransformWorldToHClip(sortedWorldPos);
            o.color = attributes.color * unity_SpriteColor;
            o.uv = attributes.uv.xy;
            o.normalWS = -GetViewForwardDir();
            o.tangentWS = attributes.tangent.xyz;
            o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;

            float3 xAxis = unity_ObjectToWorld._m00_m10_m20;
            float dotX = dot(normalize(xAxis), float3(1, 0, 0));
            int stepX = step(0, dotX);
            o.tangentWS.x = o.tangentWS.x * stepX - (1 - stepX) * o.tangentWS.x;

            return o;
        }

        Varyings CombinedShapeLightVertex(Attributes v)
        {
            return DefaultVertex(v);
        }

        SurfaceInput BuildSurfaceInput(Varyings i)
        {
            SurfaceInput surface = (SurfaceInput)0;
            surface.uv = i.uv.xy;
            surface.mainTex = _MainTex.Sample(sampler_MainTex, surface.uv);
            return surface;
        }

        half4 DefaultNormal(Varyings i)
        {
            SurfaceInput surface = BuildSurfaceInput(i);
            half4 mainTex = surface.mainTex;
            half4 normalColor = _NormalMap.Sample(sampler_MainTex, surface.uv);

            half3 normalTS = UnpackNormal(normalColor);
            half4 result = NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            result.x = unity_SpriteProps.x * result.x + (1 - unity_SpriteProps.x) * (1 - result.x);
            result.z = 0;
            result = result * i.color;
            return result;
        }

        float4 DefaultObjDepth(Varyings i)
        {
            float4 mainTex = _MainTex.Sample(sampler_MainTex, i.uv.xy);
            float alpha = mainTex.a;
            mainTex.xyz = 0;
            clip(alpha - 0.5);
            return mainTex;
        }

        half4 DefaultColor(Varyings i)
        {
            SurfaceInput surface = BuildSurfaceInput(i);
            half4 main = surface.mainTex;
            half4 result = 0;

            float singleValue = (main.x + main.y + main.z) / 3;
            float3 singleColor = main.xyz * i.color.a + singleValue.xxx * (1 - i.color.a);
            float3 baseColor = main.xyz * i.color.xyz;

            baseColor = baseColor * (1 - _BlendVertexColor) + singleColor * _BlendVertexColor;
            main.a = main.a * i.color.a * (1 - _BlendVertexColor) + main.a * _BlendVertexColor;

            result.xyz = baseColor;
            result.a = main.a;
            result.xyz = result.xyz * (1 - _FixedColor.a) + _FixedColor.xyz * _FixedColor.a;
            clip(main.a - 0.4);
            return result;
        }
        ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward" "Queue"="Geometry"
            }

            HLSLPROGRAM
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma multi_compile _ SKINNED_SPRITE

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                return DefaultColor(i);
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "Out_Nor_Depth_Water"
            }
            HLSLPROGRAM
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma multi_compile _ SKINNED_SPRITE

            struct OutData
            {
                float4 normalColor : SV_Target0;
                float4 depthColor : SV_Target1;
                float4 waterStepMask : SV_Target2;
            };

            half GetAuxAlpha(Varyings i)
            {
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
                main.a *= i.color.a;
                clip(main.a - 0.4);
                return main.a;
            }

            OutData CombinedShapeLightFragment(Varyings i)
            {
                OutData outData = (OutData)0;
                half alpha = GetAuxAlpha(i);
                outData.normalColor = DefaultNormal(i);
                outData.depthColor = DefaultObjDepth(i);
                outData.waterStepMask = float4(0, 0, 0, alpha);
                return outData;
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "Shadow" "Queue"="Transparent" "RenderType"="Transparent"
            }
            BlendOp Max

            HLSLPROGRAM
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment
            #pragma multi_compile _ SKINNED_SPRITE

            struct ShadowAttributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            ShadowVaryings UnlitVertex(ShadowAttributes attributes)
            {
                ShadowVaryings o = (ShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                float4x4 m_Data = unity_ObjectToWorld;
                float lightAngleValue = sin(LightDirection.x);
                m_Data[0][0] += m_Data[0][0] * abs(lightAngleValue) * 0.5 * LightDirection.y;

                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                float3 worldPos = mul(m_Data, float4(attributes.positionOS.xyz, 1.0)).xyz;

                float scaleZ = unity_ObjectToWorld._m22 * LightDirection.y;
                scaleZ += scaleZ * abs(lightAngleValue) * 0.5 * LightDirection.y;

                float2 offset = scaleZ.xx * float2(sin(LightDirection.x), cos(LightDirection.x));
                worldPos.xy += offset;
                worldPos = ApplyYSortToWorldPos(worldPos);

                o.positionCS = TransformWorldToHClip(worldPos);
                o.uv = attributes.uv;
                o.color = attributes.color * unity_SpriteColor;
                return o;
            }

            float4 UnlitFragment(ShadowVaryings i) : SV_Target
            {
                float4 mainTex = i.color * _MainTex.Sample(sampler_MainTex, i.uv);
                mainTex.xyz = float3(1, 1, 1) * mainTex.a;
                return mainTex;
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "CharacterDepth"
            }
            HLSLPROGRAM
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct CharacterDepthAttributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct CharacterDepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldScreenPos : TEXCOORD1;
            };

            CharacterDepthVaryings UnlitVertex(CharacterDepthAttributes attributes)
            {
                CharacterDepthVaryings o = (CharacterDepthVaryings)0;
                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                float3 objectWorldPos = TransformObjectToWorld(attributes.positionOS);
                float3 sortedWorldPos = ApplyYSortToWorldPos(objectWorldPos);
                o.positionCS = TransformWorldToHClip(sortedWorldPos);
                o.uv = attributes.uv;

                float3 ObjPos = unity_ObjectToWorld._m03_m13_m23;
                float stepPosZ = step(49, ObjPos.z);

                float3 objectSortPos = ObjPos;
                objectSortPos = ApplyYSortToWorldPos(objectSortPos);
                float4 worldClip = TransformWorldToHClip(objectSortPos);
                float high = (1 - stepPosZ) * (objectWorldPos.y - ObjPos.y) * 0.5;
                float positionCSY = o.positionCS.y;
                worldClip.y = stepPosZ * positionCSY + (1 - stepPosZ) * worldClip.y;
                o.worldScreenPos = ComputeScreenPos(worldClip);
                o.worldScreenPos.z = clamp(high, 0, 1);
                return o;
            }

            float4 UnlitFragment(CharacterDepthVaryings i) : SV_Target
            {
                float4 mainTex = _MainTex.Sample(sampler_MainTex, i.uv);
                float4 DepthTex = _DepthTex.Sample(sampler_MainTex, i.uv);
                half4 normalColor = _NormalMap.Sample(sampler_MainTex, i.uv);

                float2 worldScreenPos = i.worldScreenPos.xy / i.worldScreenPos.w;
                worldScreenPos = UnityStereoTransformScreenSpaceTex(worldScreenPos);

                float4 ObjDepthTex = SAMPLE_TEXTURE2D(_ObjDepthTex, sampler_ObjDepthTex, worldScreenPos.xy);

                half depthStep_R = step(0.01, abs(DepthTex.r - 0.5));
                half depthStep_G = 1 - step(abs(DepthTex.g - 0.5), 0.01);
                half depthStep_B = step(0.01, abs(DepthTex.b - 0.5));
                half depthStep_ZeroB = step(0.01, DepthTex.b);
                half stepDepthOne = step(1, DepthTex.b);

                half otherStep = depthStep_R * depthStep_G + depthStep_B;
                otherStep = clamp(otherStep, 0, 1) * depthStep_ZeroB;

                half depthValue = (DepthTex.r - 0.5) * (1 - otherStep) + (DepthTex.r + DepthTex.b - 1) * (1 - stepDepthOne) * otherStep;
                half offset = depthValue * 512 * 4 / _ScreenParams.y;

                half depth = worldScreenPos.y + offset;
                half depthHighStep = depthStep_G;
                half high = i.worldScreenPos.z * (1 - depthHighStep) + DepthTex.g * 2 * depthHighStep;

                mainTex.xyz = half3(depth, high, normalColor.g * 0.5 + stepDepthOne);
                mainTex.z += ObjDepthTex.z;
                mainTex.a = mainTex.a * (1 - stepDepthOne) + DepthTex.a * stepDepthOne;

                return mainTex;
            }
            ENDHLSL
        }
    }
}
