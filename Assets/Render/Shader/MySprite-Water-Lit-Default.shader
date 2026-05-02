Shader "MySprite-Water-Lit-Default"
{
    Properties
    {
        [HideInInspector] _FeatureFlags ("Feature Flags", Int) = 0

        [Header(Keyword)]
        [Toggle(_FEATURE_SHADOW)] _FEATURE_SHADOW("Keyword Shadow", Float) = 0

        _Color("Color", Color) = (1,1,1,1)
        _FixedColor("FixedColor",color)=(1,1,1,0)
        _MainTex("Diffuse", 2D) = "white" {}

        [Toggle]_HideNormal("HideNormal",int)=0

        _WaterNormalMap("WaterNormalMap", 2D) = "bump" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _WaterMaskTex("WaterMaskTex", 2D) ="black"{}
        _DepthTex("DepthTex", 2D) ="gray"{}
        _MirrorBlend("MirrorBlend",Color)=(1,1,1,1)

        [Toggle]_BlendVertexColor("BlendVertexColor",int)=0


        // 水面颜色
        [HDR]waterColor("waterColor", Color) = (0,0.5,0.5,0.5)
        // 初始透明
        _WaterZero("_WaterZero", Range(0,1)) = 0
        // 水深映射范围
        _WaterBottom("_WaterBottom", Range(0,4)) = 1
        // 水面高度
        _WaterHigh("_WaterHigh", Range(0,1)) = 0
        // 波纹亮度补偿
        waterValue("waterValue", Range(0, 0.4)) = 0.2
        // 水面噪声缩放
        waterNoiseScale("waterNoiseScale", Range(0, 300)) = 0

        // 波纹方向角 1
        _WaveAngle0("_WaveAngle0", Range(-180, 180)) = 0
        // 波纹速度 1
        _WaveSpeed0("_WaveSpeed0", Range(0, 0.2)) = 0
        // 波纹缩放 1
        WaveScale0("WaveScale0", Vector) = (1, 1, 0, 0)

        // 波纹方向角 2
        _WaveAngle1("_WaveAngle1", Range(-180, 180)) = 0
        // 波纹速度 2
        _WaveSpeed1("_WaveSpeed1", Range(0, 0.2)) = 0
        // 波纹缩放 2
        WaveScale1("WaveScale1", Vector) = (1, 1, 0, 0)


        // 水边缘颜色
        [HDR]EdgeColor("EdgeColor", Color) = (0.990566, 0.9765486, 0.9765486, 0)
        // 边缘宽度
        EdgeValue("EdgeValue", Range(0, 0.2))=0.1
        // 边缘速度
        _EdgeWaveSpeed("EdgeWaveSpeed",Range(0,4))=0
        // 边缘偏移
        _EdgeWaveOffset("EdgeWaveOffset",Range(0,0.5))=0


        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
             "RenderType"="Opaque"
        }

        //Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite on
        ZTest LEqual

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"


        Texture2D _MainTex;
        SamplerState sampler_MainTex;
        Texture2D _DepthTex;
        Texture2D _NormalMap;
        TEXTURE2D(_WaterMaskTex);
        SAMPLER(sampler_WaterMaskTex);

        TEXTURE2D(_LightingTex);
        SAMPLER(sampler_LightingTex);

        TEXTURE2D(_MirrorTex);
        SAMPLER(sampler_MirrorTex);
        TEXTURE2D(_ObjDepthTex);
        SAMPLER(sampler_ObjDepthTex);

        TEXTURE2D(_ShadowTex);
        SAMPLER(sampler_ShadowTex);

        TEXTURE2D(_WaterNormalMap);
        SAMPLER(sampler_WaterNormalMap);


        half4 GlobalColor;
        half2 LightDirection;
        half4 _SunColor;
        CBUFFER_START(UnityPerMaterial)
            int _HideNormal;
            float4 _FixedColor;
            float4 _MirrorBlend;

            int NativePos;
            float4 _Color;
            int _BlendVertexColor;

            half4 waterColor;
            half _WaterZero;
            half _WaterBottom;
            half _WaveAngle0;
            half _WaveSpeed0;
            half _WaveAngle1;
            half _WaveSpeed1;
            half WaveColorValue;
            half waterNoiseScale;
            half waterValue;
            half2 WaveScale0;
            half2 WaveScale1;
            half4 EdgeColor;
            half EdgeValue;
            half _WaterHigh;

            half _EdgeWaveSpeed;
            half _EdgeWaveOffset;

            uint _FeatureFlags; // 原始功能位配置，仅用于工具同步 keyword。
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
            float4 fixScreenUV:TEXCOORD7;
            float4 fullWorldPos : TEXCOORD5;
            float4 worldScreenPos: TEXCOORD6;
            float3 normal:NORMAL;
            half3 normalWS : TEXCOORD1;
            half3 tangentWS : TEXCOORD2;
            half3 bitangentWS : TEXCOORD3;
            float4 lightingUV : TEXCOORD4;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        struct SurfaceInput
        {
            float2 uv;
            float2 lightingUV;
            half4 mainTex;
        };

        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

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

            float3 ObjPos = unity_ObjectToWorld._m03_m13_m23;
            float3 objectWorldPos = TransformObjectToWorld(attributes.positionOS);
            float3 sortedWorldPos = ApplyYSortToWorldPos(objectWorldPos);
            float3 worldOS = objectWorldPos;

            o.positionCS = TransformWorldToHClip(sortedWorldPos);

            o.color = attributes.color * unity_SpriteColor;
            float stepPosZ = 1 - step(50, ObjPos.z);

            float3 _objSortPos = ObjPos;
            float offsetPosZ = step(ObjPos.z, -10);
            _objSortPos.y += _objSortPos.z * offsetPosZ;
            _objSortPos = ApplyYSortToWorldPos(_objSortPos);
            float4 worldClip = TransformWorldToHClip(_objSortPos);

            // 带排序 Z 偏移的精灵，复用对象原点来构造稳定的屏幕空间深度代理。
            float high = stepPosZ * (objectWorldPos.y - ObjPos.y) * 0.5;
            float positionCSY = o.positionCS.y;

            stepPosZ = clamp(stepPosZ, 0, 1);
            worldClip.y = (1 - stepPosZ) * positionCSY + stepPosZ * worldClip.y;
            o.worldScreenPos = ComputeScreenPos(worldClip);
            o.worldScreenPos.z = clamp(high, 0, 1);

            objectWorldPos.y = unity_ObjectToWorld._m13;
            half4 worldPosCs = TransformWorldToHClip(ApplyYSortToWorldPos(objectWorldPos.xyz));
            float4 worldScreenPos = ComputeScreenPos(worldPosCs);
            worldScreenPos.xy = worldScreenPos.xy / worldScreenPos.w;
            o.fullWorldPos = float4(objectWorldPos.xy, worldScreenPos.xy);

            half3 cameraOffsetPos = _WorldSpaceCameraPos.xyz - unity_ObjectToWorld._m03_m13_m23;
            half3 pos = worldOS.xyz + cameraOffsetPos;
            half4 cameraClipPos = TransformWorldToHClip(pos);
            o.fixScreenUV = ComputeScreenPos(cameraClipPos);

            o.uv = attributes.uv.xy;
            o.normalWS = -GetViewForwardDir();
            o.tangentWS = attributes.tangent.xyz;
            o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
            o.lightingUV = ComputeScreenPos(o.positionCS);

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

        float4 DefaultObjDepth(Varyings i)
        {
            float4 mainTex = _MainTex.Sample(sampler_MainTex, i.uv.xy);
            float a = mainTex.a;
                float2 worldScreenPos = i.worldScreenPos.xy / i.worldScreenPos.w;
                worldScreenPos = UnityStereoTransformScreenSpaceTex(worldScreenPos);

                float4 DepthTex = _DepthTex.Sample(sampler_MainTex, i.uv.xy);
                float clipA = 1 - step(DepthTex.a, 0.01);
                DepthTex.xyz *= clipA;
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex, i.uv.xy);

                // R == 0.5 表示当前像素走“扩展深度编码”。
                // G == 0.5 表示高度直接取贴图里的高度值，而不是取顶点阶段算出来的高度代理。
                // B == 0.5 表示这是另一种扩展深度分支；B > 1 表示角色/覆盖物这类特殊层级，需要把 alpha 也切过去。
                half depthStep_R = 1 - step(abs(DepthTex.r - 0.5), 0.01);
                half depthStep_G = 1 - step(abs(DepthTex.g - 0.5), 0.01);
                half depthStep_B = 1 - step(abs(DepthTex.b - 0.5), 0.01);
                // B <= 0 说明没有额外深度信息；B > 1 说明这部分已经切到特殊层级，不能再参与普通扩展深度分支。
                half depthStep_ZeroB = 1 - step(DepthTex.b, 0);
                half stepDepthOne = step(1, DepthTex.b);
                depthStep_ZeroB *= (1 - stepDepthOne);

                // RGB 全为 0 时表示这个像素没有额外深度覆盖，需要把最后的深度偏移清掉。
                int clearColor = 1 - step(DepthTex.b, 0) * step(DepthTex.r, 0) * step(DepthTex.g, 0);
                // 只要命中 R 分支或 B 分支，并且 B 仍在普通范围内，就走扩展深度解码。
                half otherStep = depthStep_R * depthStep_G + depthStep_B;
                otherStep = clamp(otherStep, 0, 1) * depthStep_ZeroB;

                // 普通分支：只用 R 表示相对深度偏移，0.5 为中心。
                // 扩展分支：R + B - 1，把 B 也并进来表达更大的深度跨度。
                half depthValue = (DepthTex.r - 0.5) * (1 - otherStep) + (DepthTex.r + DepthTex.b - 1) * (1 - stepDepthOne) * otherStep;
                // 把贴图里的相对深度换算成当前屏幕分辨率下的实际屏幕 Y 偏移。
                half offset = depthValue * 512 * 4 / _ScreenParams.y;
                half depth = worldScreenPos.y + offset * clearColor;
                half depthHighStep = depthStep_G;
                // G 命中时，直接用贴图里的高度值；否则用顶点阶段算出来的高度代理。
                half high = i.worldScreenPos.z * (1 - depthHighStep) + DepthTex.g * 2 * depthHighStep;

                // 输出编码：
                // R: 最终屏幕深度
                // G: 最终高度
                // B: 法线贴图 G 通道 + 特殊层级标记
                mainTex.xyz = half3(depth, high, _NormalColor.g * 0.5 + stepDepthOne);
                mainTex.a = (mainTex.a * (1 - stepDepthOne) + DepthTex.a * stepDepthOne);
                mainTex.a = clamp(mainTex.a, 0, 1);

            clip(a - 0.5);
            return mainTex;
        }

        SurfaceInput BuildSurfaceInput(Varyings i)
        {
            SurfaceInput surface = (SurfaceInput)0;
            surface.uv = i.uv.xy;
            surface.lightingUV = i.lightingUV.xy / i.lightingUV.w;
            surface.lightingUV = UnityStereoTransformScreenSpaceTex(surface.lightingUV);
            surface.mainTex = _MainTex.Sample(sampler_MainTex, surface.uv);
            return surface;
        }

        float4 WaterFragment(float2 uv, float2 fixedScreenUV, float2 screenUV, float4 mainTexColor)
        {
            float2 mirrorUV = screenUV;
            float waterMask = SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).r;
            // waterMask.r <= 0.06 时，这个像素直接按非水域处理。
            float stepMask = step(0.06, waterMask);

            half edgeOffsetValue = _SinTime.w * _EdgeWaveSpeed;
            edgeOffsetValue = abs(edgeOffsetValue);
            edgeOffsetValue = clamp(edgeOffsetValue, 0, 1);
            // waterHigh 是当前时刻的实际水位线，会随着边缘波动做上下偏移。
            half waterHigh = _WaterHigh + _EdgeWaveOffset * edgeOffsetValue;
            float angle0 = radians(_WaveAngle0);
            float2 waveValue0 = float2(cos(angle0), sin(angle0)) * _WaveSpeed0;
            float angle1 = radians(_WaveAngle1);
            float2 waveValue1 = float2(cos(angle1), sin(angle1)) * _WaveSpeed1;
            float2 timeXX = _TimeParameters.x.xx;
            float2 waveUV0 = fixedScreenUV * WaveScale0 + timeXX * waveValue0;
            float2 waveUV1 = fixedScreenUV * WaveScale1 + timeXX * waveValue1;
            half3 wave0 = UnpackNormal(SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, waveUV0));
            half3 wave1 = UnpackNormal(SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, waveUV1));

            // 两层法线波叠加后，只取 xy 分量：
            // x/y 越大，边缘亮纹越强，后面镜面 UV 扰动也越明显。
            float waveBlendCol = saturate(wave0.x + wave0.y + wave1.x + wave1.y) * waterValue;
            float _waterNoise;
            Unity_SimpleNoise_float(fixedScreenUV.xy, waterNoiseScale, _waterNoise);
            // 再乘一层噪声，避免整片水面的亮纹节奏完全同步。
            waveBlendCol *= _waterNoise;

            // 把 0~1 的原始遮罩 remap 到可调水深范围，后面所有边缘判断都基于这个值。
            waterMask = lerp(_WaterZero, _WaterBottom, waterMask);
            waterMask = saturate(waterMask);
            // waterMask1: 像素是否已经进入主体水域。
            // waterMask2: 像素是否已经越过边缘带。
            float waterMask1 = step(waterHigh, waterMask);
            float waterMask2 = step(waterHigh + EdgeValue, waterMask);
            // 主体水域之外，整段水面逻辑直接失效。
            stepMask *= waterMask1;

            // edgeMaskValue 只在 [waterHigh, waterHigh + EdgeValue] 这段边缘带里从 0 线性涨到 1。
            float edgeMaskValue = saturate((waterMask - waterHigh) / EdgeValue);
            // 只有落在边缘带里的像素，edge 才会非 0。
            float edge = (waterMask1 - waterMask2) * edgeMaskValue;
            float3 endWaveColor = saturate(edge * EdgeColor.xyz * waveBlendCol + waveBlendCol * waterMask1.rrr);

            half sunValue = (_SunColor.x + _SunColor.y + _SunColor.z) / 3;
            sunValue = max(sunValue, 0.001h);

            float3 baseWaterColor = waterColor.xyz * waterColor.a;
            baseWaterColor += (1 - waterColor.a) * mainTexColor.xyz;
            // 主体水色按 remap 后的水深衰减，越深越接近水本身的颜色。
            baseWaterColor *= waterMask;

            endWaveColor = endWaveColor * _SunColor.xyz / sunValue;

            float3 outWater = saturate(endWaveColor + baseWaterColor);
            outWater = outWater + waterColor.xyz * waterColor.a;

            // 用当前水色的 RG 去扰动镜像采样坐标，制造反射随波纹摆动的效果。
            mirrorUV.x += outWater.r * 0.1;
            mirrorUV.y += outWater.g * 0.04 - 0.02;

            float3 MirrorTexColor = SAMPLE_TEXTURE2D(_MirrorTex, sampler_MirrorTex, mirrorUV).xyz;
            MirrorTexColor.xyz *= _MirrorBlend.xyz;
            // 反射图越亮，越倾向用反射色覆盖当前水色。
            float MirrorValue = (MirrorTexColor.x + MirrorTexColor.y + MirrorTexColor.z) / 3;

            outWater = outWater * (1 - MirrorValue) + MirrorTexColor * MirrorValue;
            // 非主体水域像素仍然回退到原始主贴图颜色。
            outWater = stepMask * outWater + mainTexColor.xyz * (1 - stepMask);
            return float4(outWater.xyz, stepMask);
        }

        float4 ShadowColor(float4 result, float2 screenUv)
        {
            half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, screenUv);

            half3 shadowColor = GlobalColor.xyz * GlobalColor.a * 0.5 * shadow.r;
            result.xyz = shadowColor * result.xyz + result.xyz * (1 - shadow.r);
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
            #pragma shader_feature_local _FEATURE_SHADOW

            half4 DefaultNormal(Varyings i)
            {
                SurfaceInput surface = BuildSurfaceInput(i);
                half4 mainTex = surface.mainTex;
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex, surface.uv);

                half3 normalTS = _NormalColor.xyz;
                half4 result = half4(1, 1, 1, 1);
                normalTS = UnpackNormal(_NormalColor);

                result = NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                result.x = unity_SpriteProps.x * result.x + (1 - unity_SpriteProps.x) * (1 - result.x);
                result.z = 0;
                result = result * i.color;

                return result;
            }

            half4 DefaultColor(Varyings i, out float waterStepMask)
            {
                float2 uv = i.uv.xy;
                half4 main = 0;
                waterStepMask = 0;

                SurfaceInput surface = BuildSurfaceInput(i);
                float2 lightingUV = surface.lightingUV;
                uv = surface.uv;
                main = surface.mainTex;

                half4 result = 0;
                float singleValue = (main.x + main.y + main.z) / 3;
                float3 singleColor = main.xyz * (i.color.a) + singleValue.xxx * (1 - i.color.a);
                float3 waterColor = main.xyz * i.color.xyz;

                waterColor.xyz = waterColor.xyz * (1 - _BlendVertexColor) + singleColor * _BlendVertexColor;
                main.a = main.a * i.color.a * (1 - _BlendVertexColor) + main.a * _BlendVertexColor;

                main.xyz = waterColor.xyz;
                float2 fixScreenUV = i.fixScreenUV.xy / i.fixScreenUV.w;
                fixScreenUV = UnityStereoTransformScreenSpaceTex(fixScreenUV);
                float4 outWaterColor = WaterFragment(uv, fixScreenUV, lightingUV, main);
                waterColor = outWaterColor.xyz;
                waterStepMask = outWaterColor.w;
                result.xyz = waterColor.xyz;

                result.a = main.a;


                #if defined(_FEATURE_SHADOW)
                {
                    result = ShadowColor(result, lightingUV);
                }
                #endif

                result.xyz = result.xyz * (1 - _FixedColor.a) + _FixedColor.xyz * _FixedColor.a;
                clip(main.a - 0.4);
                return result;
            }


            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                float waterStepMask = 0;
                return DefaultColor(i, waterStepMask);
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
                float4 normalColor:SV_Target0;
                float4 depthColor:SV_Target1;
                float4 waterStepMask:SV_Target2;
            };

            half4 DefaultNormal(Varyings i)
            {
                SurfaceInput surface = BuildSurfaceInput(i);
                half4 mainTex = surface.mainTex;
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex, surface.uv);

                half3 normalTS = _NormalColor.xyz;
                half4 result = half4(1, 1, 1, 1);
                normalTS = UnpackNormal(_NormalColor);

                result = NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                result.x = unity_SpriteProps.x * result.x + (1 - unity_SpriteProps.x) * (1 - result.x);
                result.z = 0;
                result = result * i.color;

                return result;
            }

            float GetAuxWaterStepMask(Varyings i)
            {
                SurfaceInput surface = BuildSurfaceInput(i);
                float2 uv = surface.uv;

                float waterMask = SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv).r;
                float stepMask = step(0.06, waterMask);

                half edgeOffsetValue = abs(_SinTime.w * _EdgeWaveSpeed);
                edgeOffsetValue = clamp(edgeOffsetValue, 0, 1);
                half waterHigh = _WaterHigh + _EdgeWaveOffset * edgeOffsetValue;

                Unity_Remap_float(waterMask, float2(0, 1), float2(_WaterZero, _WaterBottom), waterMask);
                waterMask = clamp(waterMask, 0, 1);

                float waterMask1 = step(waterHigh, waterMask);
                stepMask *= waterMask1;
                return stepMask;
            }

            half GetAuxAlpha(Varyings i)
            {
                float2 uv = i.uv.xy;
                half4 main = 0;


                SurfaceInput surface = BuildSurfaceInput(i);
                uv = surface.uv;
                main = surface.mainTex;

                main.a = main.a * i.color.a * (1 - _BlendVertexColor) + main.a * _BlendVertexColor;

                clip(main.a - 0.4);
                return main.a;
            }


            OutData CombinedShapeLightFragment(Varyings i)
            {
                OutData outData = (OutData)0;
                half alpha = GetAuxAlpha(i);
                float waterStepMask = GetAuxWaterStepMask(i);
                outData.waterStepMask = float4(waterStepMask.xxx, alpha);
                outData.normalColor = DefaultNormal(i);
                outData.depthColor = DefaultObjDepth(i);
                return outData;
            }
            ENDHLSL
        }
        

        Pass
        {
            Name "Water"
            Tags
            {
                "LightMode" = "Water"
            }

            HLSLPROGRAM
            #pragma vertex WaterPassVertex
            #pragma fragment WaterPassFragment


            struct WaterAttributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct WaterVaryings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 lightingUV : TEXCOORD1;
                float4 worldPos : TEXCOORD4;
                float4 fixScreenUV: TEXCOORD3;
            };


            WaterVaryings WaterPassVertex(WaterAttributes v)
            {
                WaterVaryings o = (WaterVaryings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                float3 objectWorldPos = TransformObjectToWorld(v.positionOS);
                float3 sortedWorldPos = ApplyYSortToWorldPos(objectWorldPos);
                o.positionCS = TransformWorldToHClip(sortedWorldPos);
                o.worldPos.xyz = ApplyYSortToWorldPos(unity_ObjectToWorld._m03_m13_m23);
                o.worldPos.w = o.worldPos.z;
                o.uv = v.uv;
                o.lightingUV = ComputeScreenPos(o.positionCS);


                half3 pos = TransformObjectToWorld(_WorldSpaceCameraPos.xyz);
                half4 cameraClipPos = TransformWorldToHClip(pos);


                o.fixScreenUV = o.lightingUV - ComputeScreenPos(cameraClipPos);

                o.color = v.color * unity_SpriteColor;
                return o;
            }

            float WaterPassMaskFragment(float2 uv, float2 screenUV)
            {
                float2 mirrorUV = screenUV;

                float3 _WaterMask = SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).xyz;
                // 水域范围
                float stepMask = step(0.06, _WaterMask.r);

                half edgeOffsetValue = _SinTime.w * _EdgeWaveSpeed;
                edgeOffsetValue = abs(edgeOffsetValue);
                edgeOffsetValue = clamp(edgeOffsetValue, 0, 1);
                _WaterHigh = _WaterHigh + _EdgeWaveOffset * edgeOffsetValue;


                float svalue = _ScreenParams.y / 1920;
                svalue = floor(svalue);
                svalue = clamp(svalue, 1, svalue);
                svalue /= 2;
                float2 offsetUv = _WorldSpaceCameraPos.xy * svalue * 800 / _ScreenParams.xy;
                screenUV += offsetUv;

                // 波纹 1
                float angle0 = radians(_WaveAngle0);
                float2 waveValue0 = float2(cos(angle0), sin(angle0)) * _WaveSpeed0;

                float2 _WaveT0 = (_TimeParameters.x.xx) * waveValue0;
                float2 _TilingAndOffset0 = screenUV * WaveScale0 + _WaveT0;
                float4 _WaveCol0 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, _TilingAndOffset0);
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);
                // 波纹 2
                float angle1 = radians(_WaveAngle1);
                float2 waveValue1 = float2(cos(angle1), sin(angle1)) * _WaveSpeed1;
                float2 _WaveT2 = (_TimeParameters.x.xx) * waveValue1;
                float2 _TilingAndOffset1 = screenUV * WaveScale1 + _WaveT2;
                float4 _WaveCol1 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, _TilingAndOffset1);
                _WaveCol1.rgb = UnpackNormal(_WaveCol1);


                float3 _endWave = _WaveCol0.xyz + _WaveCol1.xyz;
                float waveBlendCol = _endWave[0] + _endWave[1];
                waveBlendCol = clamp(waveBlendCol, 0, 1);
                waveBlendCol *= waterValue;
                float _waterNoise;
                Unity_SimpleNoise_float(screenUV.xy, waterNoiseScale, _waterNoise);
                waveBlendCol *= _waterNoise;
                Unity_Remap_float(_WaterMask.r, float2(0, 1), float2(_WaterZero, _WaterBottom), _WaterMask.r);
                _WaterMask.r = clamp(_WaterMask.r, 0, 1);


                float _WaterMask1 = step(_WaterHigh, _WaterMask.r);
                float _WaterMask2 = step(_WaterHigh + EdgeValue, _WaterMask.r);
                stepMask *= _WaterMask1;
                return stepMask;
            }

            half4 WaterPassFragment(WaterVaryings i) : SV_Target
            {
                float2 lightingUV = i.lightingUV.xy / i.lightingUV.w;
                lightingUV = UnityStereoTransformScreenSpaceTex(lightingUV);
                float result = WaterPassMaskFragment(i.uv, lightingUV);
                return float4(result.xxx, 1);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
    CustomEditor "FeatureFlagsGUI"
}

