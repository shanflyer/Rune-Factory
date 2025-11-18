Shader "MyShadow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        [Toggle] _simpleShadow("SimpleShadow",int)=0
        [Toggle]_SpriteShadow("SpriteShow",int)=0
        _Color ("Tint", Color) = (1,1,1,1)
        _DirIndex("DirIndex",int)=0
        _ClearDir("_ClearDir",int)=0

        _ScaleLength("_ScaleLength",float)=0.5
        _OffSetValue("_OffSetValue",float)=0.5
        [Toggle]_HoldScale("_HoldScale",int)=0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        BlendOp Max
        Cull Off
        ZWrite Off
        Pass
        {
            Tags
            {
                "LightMode" = "Shadow" "Queue"="Transparent" "RenderType"="Transparent"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Assets/Render/Shader/UnityAction.cginc"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment


            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 uv2: TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half2 LightDirection;
            half _ShadowValue;
            half4 GlobalColor;
            half2 _Direction;
            // NOTE: Do not ifdef the properties here as SRP batcher can not handle different layouts.
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                int _SpriteShadow;
                int _DirIndex;
                int _ClearDir;
                int _HoldScale;
                int _simpleShadow;

                float _ScaleLength;
                float _OffSetValue;
            CBUFFER_END

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                [branch] if (_simpleShadow == 1)
                {
                    o.positionCS = TransformObjectToHClip(attributes.positionOS);
                    o.color = attributes.color * unity_SpriteColor;

                    o.uv = attributes.uv;
                }
                else
                {
                    float4x4 m_Data = UNITY_MATRIX_M;
                    LightDirection.x = (1 - abs(_DirIndex)) * LightDirection.x + clamp(LightDirection.x, 0, 90) *
                        step(1, _DirIndex) + clamp(LightDirection.x, -90, 0) * step(1, -_DirIndex);

                    float lightAngleValue = sin(LightDirection.x);
                    m_Data[0][0] += m_Data[0][0] * abs(lightAngleValue) * 0.5 * LightDirection.y;
                    float scaleY = 1;
                    Unity_Remap_float(_Direction.y, float2(0, 1), float2(1, 0.75), scaleY);
                    scaleY = scaleY * (1 - _HoldScale) + _HoldScale;
                    m_Data[1][1] *= scaleY;
                    float3 worldPos = mul(m_Data, float4(attributes.positionOS, 1.0));
                    float length = attributes.uv2.x * LightDirection.y;
                    length += length * abs(lightAngleValue) * 0.5 * LightDirection.y;

                    float scaleZ = unity_ObjectToWorld._m22 * LightDirection.y;
                    scaleZ += scaleZ * abs(lightAngleValue) * 0.5 * LightDirection.y;

                    length = length * (1 - _SpriteShadow) + scaleZ * _SpriteShadow;


                    float2 offset = length.xx * float2(sin(LightDirection.x), cos(LightDirection.x)) * (1 - _Direction.
                        y);

                    worldPos.xy += offset * attributes.uv2.y * (1 - _SpriteShadow) + offset * _SpriteShadow;

                    o.positionCS = TransformWorldToHClip(worldPos);
                    attributes.color = attributes.color * (1 - _SpriteShadow) + attributes.color * _SpriteShadow *
                        unity_SpriteColor;

                    o.uv = attributes.uv;
                    o.uv2 = attributes.uv2;
                    o.color = attributes.color;
                    o.color.xyz = o.color.xyz * (1 - o.uv.x) * (1 - _SpriteShadow) + o.color.xyz * _SpriteShadow;
                }


                return o;
            }

            half4 UnlitFragment(Varyings i) : SV_Target
            {
                half4 mainTex = i.color * _MainTex.Sample(sampler_MainTex, i.uv);
                mainTex.xyz = mainTex.aaa;

                _ShadowValue = _ShadowValue * (1 - _ClearDir) + _ShadowValue * _ClearDir *
                (step(1, _DirIndex) * step(_Direction.x, 0) +
                    step(1, -_DirIndex) * (1 - step(_Direction.x, 0)));

                [branch] if (_simpleShadow == 1)
                {
                    mainTex.xyz = mainTex.aaa;
                    mainTex *= _ShadowValue;
                }
                else
                {
                    mainTex = i.color * (1 - _SpriteShadow) + mainTex * _SpriteShadow;
                    mainTex *= _ShadowValue;
                }

                return mainTex;
            }
            ENDHLSL
        }
    }
}