Shader "MySprite-Lit-Default"
{
    Properties
    {
        [HideInInspector] _FeatureFlags ("Feature Flags", Int) = 0

        _Color("Color", Color) = (1,1,1,1)
        _FixedColor("FixedColor",color)=(1,1,1,0)
        _MainTex("Diffuse", 2D) = "white" {}
        _MoveMask("_MoveMask", 2D) = "black" {}
        _SnowTex("_SnowTex", 2D) = "black" {}
        _FlowerTex("_FlowerTex",2D)="black" {}
        _FlowerRemap("_FlowerRemap",vector)=(0,0,0,0)

        _ZWrite("ZWrite", Float) = 0
        _ObjectWorldPos("_ObjectWorldPos",vector)=(0,0,0,0) 
        [Toggle]_HideNormal("HideNormal",int)=0
        [Toggle]_ZOffset("_ZOffset",int)=1

        _WaterNormalMap("WaterNormalMap", 2D) = "bump" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _WaterMaskTex("WaterMaskTex", 2D) ="black"{}
        _DepthTex("DepthTex", 2D) ="gray"{}
        _WetValue("WetValue",Range(0,1))=0
        _LightBlend("LightBlend",float)=1
        _MirrorBlend("MirrorBlend",Color)=(1,1,1,1)

        [Toggle]_BlendVertexColor("BlendVertexColor",int)=0



        _WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
        _WindScroll("Wind Scroll", Range( 0 , 3)) = 0.1
        _WindJitter("Wind Jitter", Range( 0 , 3)) = 0.1
        _WindNoiseValue("WindNoiseValue",Range(0,1))=0
        [Toggle]_MoveSelfUV("_MoveSelfUV",int)=0



        _PlantSpringColor("_PlantSpringColor",color)=(0,0,0)
        _PlantSpringColor1("_PlantSpringColor1",color)=(0,0,0)
        _PlantAutumnColor0("_PlantAutumnColor0",color)=(0,0,0)
        _PlantAutumnColor1("_PlantAutumnColor1",color)=(1,1,1)
        _PlantWinterColor("_PlantWinterColor",color)=(0,0,0)
        _PlantWinterColor1("_PlantWinterColor1",color)=(0,0,0)
        _PlantAutumnNoiseScale("_PlantAutumnNoiseScale",float)=1



        [Toggle]_Damp("_Damp",int)=0


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
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite off
        ZTest LEqual

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
        #include "Assets/Render/Shader/UnityAction.cginc"


        Texture2D _MainTex;
        SamplerState sampler_MainTex;
        Texture2D _DepthTex;
        Texture2D _NormalMap;
        Texture2D _MoveMask;
        Texture2D _SnowTex;
        Texture2D _FlowerTex;
        TEXTURE2D(_WaterMaskTex);
        SAMPLER(sampler_WaterMaskTex);
        TEXTURE2D(_WindNoiseTexture);
        SAMPLER(sampler_WindNoiseTexture);

        TEXTURE2D(_LightingTex);
        SAMPLER(sampler_LightingTex);

        TEXTURE2D(_GrassTex);
        SAMPLER(sampler_GrassTex);

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
        half _ShadowValue;

        float _SnowValue;

        float _DampValue;
        float _DampNoise;
        float _HighLighStep;
        float4 _DampColor;
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
            int _ZOffset;
            int _Character;
            int _Damp;
            int _HideNormal;
            float4 _FixedColor;
            float4 _MirrorBlend;

            int NativePos;
            float4 _FlowerRemap;
            float4 _Color;

            half3 _PlantSpringColor1;
            half3 _PlantSpringColor;
            half3 _PlantAutumnColor0;
            half3 _PlantAutumnColor1;
            half3 _PlantWinterColor;
            half3 _PlantWinterColor1;
            float _PlantAutumnNoiseScale;

            half _WetValue;

            float _LightBlend;
            int _BlendVertexColor;

            float _WindJitter;
            float _WindScroll;
            float _WindNoiseValue;
            int _MoveSelfUV;

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

        #define FEAT_WATER (1u<<0)
        #define FEAT_DAMPBLEND (1u<<1)
        #define FEAT_MOVE (1u<<2)
        #define FEAT_SEASONCOLORBLEND (1u<<3)
        #define FEAT_SNOWBLEND (1u<<4)
        #define FEAT_GRASSBLEND (1u<<5)
        #define FEAT_SHADOWSTEP (1u<<6)
        #define FEAT_FLOWERSTEP (1u<<7)
        #define FEAT_SIMPLE (1u<<8)
        #define FEAT_CHARACTER (1u<<9)

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
            float2 worldScreenPos;
            half snowValue;
            half4 mainTex;
        };

        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

        Varyings DefaultVertex(Attributes attributes)
        {
            Varyings o = (Varyings)0;

            float3 ObjPos = unity_ObjectToWorld._m03_m13_m23;
            float3 objectWorldPos = TransformObjectToWorld(attributes.positionOS);
            float3 worldOS = objectWorldPos;
            UNITY_SETUP_INSTANCE_ID(attributes);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            UNITY_SKINNED_VERTEX_COMPUTE(attributes);

            #if defined(_FEATURE_CHARACTER)
            {
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
            }
            #else
            {
                o.positionCS = TransformWorldToHClip(worldOS);
            }
            #endif

            o.color = attributes.color * unity_SpriteColor;
            #if defined(_FEATURE_SIMPLE)
            {
                o.color = attributes.color * _Color * unity_SpriteColor;
            }
            #endif
            float stepPosZ = 1 - step(50, ObjPos.z);

            float3 _objSortPos = ObjPos;
            float offsetPosZ = step(ObjPos.z, -10);
            _objSortPos.y += _objSortPos.z * offsetPosZ;
            float4 worldClip = TransformWorldToHClip(_objSortPos);

            // 带排序 Z 偏移的精灵，复用对象原点来构造稳定的屏幕空间深度代理。
            float high = stepPosZ * (objectWorldPos.y - ObjPos.y) * 0.5;
            float positionCSY = o.positionCS.y;

            stepPosZ = clamp(stepPosZ, 0, 1);
            worldClip.y = (1 - stepPosZ) * positionCSY + stepPosZ * worldClip.y;
            o.worldScreenPos = ComputeScreenPos(worldClip);
            o.worldScreenPos.z = clamp(high, 0, 1);

            objectWorldPos.y = unity_ObjectToWorld._m13;

            half4 worldPosCs = TransformWorldToHClip(objectWorldPos.xyz);
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
            #if defined(_FEATURE_CHARACTER)
            {
                mainTex.xyz = 0;
            }
            #else
            {
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
            }
            #endif

            clip(a - 0.5);
            return mainTex;
        }

        float2 MoveUV(float2 uv, float2 screenUV, float SnowMove, out float2 offset);
        float4 SnowColor(float4 main, float2 uv, float s_w);

        SurfaceInput BuildSurfaceInput(Varyings i)
        {
            SurfaceInput surface = (SurfaceInput)0;
            surface.uv = i.uv.xy;
            surface.lightingUV = i.lightingUV.xy / i.lightingUV.w;
            surface.lightingUV = UnityStereoTransformScreenSpaceTex(surface.lightingUV);
            surface.worldScreenPos = UnityStereoTransformScreenSpaceTex(i.fullWorldPos.zw);

            float s_w = 0;
            #if defined(_FEATURE_SNOW)
            {
                Unity_Remap_float(_SeasonValue, float2(2.95, 3.05), float2(0, 1), s_w);
                s_w = clamp(s_w, 0, 1);

                float s_w1 = 0;
                Unity_Remap_float(_SeasonValue, float2(0.1, 0), float2(0, 1), s_w1);
                s_w1 = clamp(s_w1, 0, 1);
                s_w += s_w1;
            }
            #endif

            surface.snowValue = s_w;

            #if defined(_FEATURE_MOVE)
            {
                float2 offset;
                surface.uv = MoveUV(surface.uv, surface.lightingUV, 1 - s_w, offset);
            }
            #endif

            surface.mainTex = _MainTex.Sample(sampler_MainTex, surface.uv);

            return surface;
        }

        float My_SimpleNoise_float(float2 uv, float scale)
        {
            half4 col = SAMPLE_TEXTURE2D(_WindNoiseTexture, sampler_WindNoiseTexture, uv/scale);
            return col.r;
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

        float3 DampColor(float3 col, float2 uv, float2 objUV)
        {
            float r = col.r * col.r;
            float g = col.g * col.g;
            float b = col.b * col.b;
            float3 dampBaseColor = float3(r, g, b);

            float _DampNoiseValue;

            float svalue = _ScreenParams.y / 1920;
            svalue = floor(svalue);
            svalue = clamp(svalue, 1, svalue);
            svalue /= 2;


            float2 noiseUV = uv + _WorldSpaceCameraPos.xy * svalue * 800 / _ScreenParams.xy;
            Unity_SimpleNoise_float(noiseUV, _DampNoise,
                                    _DampNoiseValue);
            float3 d = float3(_DampNoiseValue, _DampNoiseValue, _DampNoiseValue);


            float3 water = float3(1 - _DampNoiseValue, 1 - _DampNoiseValue, 1 - _DampNoiseValue);
            float waterValue = clamp((_DampValue - 0.5), 0, 0.5) / 0.5;

            float _HighLightNoiseValue;
            Unity_SimpleNoise_float(noiseUV, _HighLightNoise,
                                    _HighLightNoiseValue);
            float3 h = float3(_HighLightNoiseValue, _HighLightNoiseValue, _HighLightNoiseValue);


            h *= (1 - _DampNoiseValue);

            float dValue = 1 - waterValue;
            d *= dValue;
            d *= d;
            d = clamp(d, 0, 1);


            water *= waterValue;
            water *= water;
            water = clamp(water, 0, 1);
            h *= waterValue;
            h *= step(_HighLighStep, h);
            h *= h;

            h = clamp(h, 0, 1);


            float c = (col.r + col.g + col.b) / 3;

            d *= _DampColor.xyz * c;
            water *= _DampWaterColor.xyz * c;
            h *= _HightLightColor.xyz * c;

            dampBaseColor += d + water + h;
            half4 normal = _NormalMap.Sample(sampler_MainTex, objUV);
            float normalInfluence = normal.z;
            Unity_Remap_float(normalInfluence, float2(0, 1), float2(0.2, 1), normalInfluence);

            float3 result = lerp(col, dampBaseColor, clamp(_DampValue / 0.5, 0, 1) * normalInfluence);
            return result * (1 - _Damp) + dampBaseColor * _Damp;
        }

        float2 MoveUV(float2 uv, float2 screenUV, float SnowMove, out float2 offset)
        {
            float svalue = _ScreenParams.y / 1920;
            svalue = floor(svalue);
            svalue = clamp(svalue, 1, svalue);
            svalue /= 2;
            float timeX = _TimeParameters.x;
            float2 cameraOffsetUV = _WorldSpaceCameraPos.xy * svalue * 800 / _ScreenParams.xy;
            // _MoveSelfUV 为 0 时使用屏幕空间风场；为 1 时完全退回模型自身 UV。
            screenUV = lerp(screenUV + cameraOffsetUV, uv, _MoveSelfUV);

            float2 panner63 = screenUV + _WindScroll * 0.3 * timeX;
            float2 panner74 = screenUV * 2 + timeX * _WindJitter * 0.5;

            // 第一层噪声决定主摆动幅度。
            float windNoise0 = SAMPLE_TEXTURE2D(_WindNoiseTexture, sampler_WindNoiseTexture, panner63).x;
            windNoise0 = abs(windNoise0);
            windNoise0 = windNoise0 * windNoise0 * sqrt(windNoise0);
            // 第二层噪声只负责打散同步感，不直接做位移方向判断。
            float windNoise1 = SAMPLE_TEXTURE2D(_WindNoiseTexture, sampler_WindNoiseTexture, panner74).x;

            float windValue = lerp(1, 2, abs(_WindValue));
            // _MoveMask.r 是局部可摆动权重，值越大说明这个像素越容易被风推开。
            float value = _MoveMask.Sample(sampler_MainTex, uv).x * _WindNoiseValue * windValue;
            // SnowMove 为 0 时，雪压状态下的风摆会完全停掉；为 1 时保持正常摆动。
            offset = windNoise0 * windNoise1 * value * SnowMove;
            int stepWind = step(0, _WindValue);
            // _WindValue 的正负只决定左右方向，幅度已经提前折到 abs(_WindValue) 里。
            offset.x = offset.x * stepWind - offset.x * (1 - stepWind);

            return offset + uv;
        }

        float3 NoiseBlendColor(float3 colorA, float3 colorB, float noiseValue)
        {
            return lerp(colorB, colorA, noiseValue);
        }

        float3 BlendSeasonColor(float3 main, float blendMask, float2 worldUV)
        {
            // blendMask <= 0 时，这个像素不参与季节色替换，最后直接回退到原始主贴图颜色。
            half _BlendValue = 1 - step(blendMask, 0);
            float3 baseColor = main.xyz;
            float mainValue = clamp(main.y, 0, 1);

            float noiseValue = My_SimpleNoise_float(worldUV, _PlantAutumnNoiseScale);
            float noiseValue1 = My_SimpleNoise_float(worldUV, _PlantAutumnNoiseScale * 2);
            float seasonValue = _SeasonValue;

            float seasonColorBlend = _PlantSpringColor.x + _PlantSpringColor.y + _PlantSpringColor.z;
            // main.y 用作“可染色强度”，越高说明这部分越容易被季节色替换。
            float mainWeight = mainValue.xxx;

            // 按季节时间轴分四段：
            // 0~0.15   冬 -> 春过渡起始
            // 0.15~0.5 春季内部过渡
            // 1.0~1.25 春 -> 夏
            // 2.0~2.5 夏 -> 秋
            // 2.85~3.35 秋 -> 冬
            // 两次 noise 分别给不同阶段提供块状差异，避免整片植被同步变色。
            float winterToSpring = saturate(seasonValue / 0.15);
            float springBlend = saturate((seasonValue - 0.15) / 0.35);
            // seasonValue < 0.15 时，仍优先保留冬 -> 春的第一段过渡。
            float springPhase = 1 - step(0.15, seasonValue);

            float3 winterColor = NoiseBlendColor(_PlantWinterColor, _PlantWinterColor1, noiseValue) * mainWeight;
            float3 springColor0 = NoiseBlendColor(_PlantWinterColor, _PlantSpringColor, noiseValue1) * mainWeight;
            float3 springColor1 = NoiseBlendColor(_PlantSpringColor, _PlantSpringColor1, noiseValue) * mainWeight;
            // springFromWinter: 冬色向春色第一层缓慢过渡。
            float3 springFromWinter = lerp(winterColor, springColor0, winterToSpring);
            // springFromSpring: 春季内部两套绿色块之间继续细分。
            float3 springFromSpring = lerp(springColor0, springColor1, springBlend);
            float3 springColor = lerp(springFromSpring, springFromWinter, springPhase);

            float summerBlend = saturate((seasonValue - 1.0) / 0.25);
            float3 summerColor = lerp(springColor, baseColor, summerBlend);

            float summerToAutumn = saturate((seasonValue - 2.0) / 0.25);
            float3 autumnColor0 = NoiseBlendColor(baseColor, _PlantAutumnColor0, noiseValue1) * mainWeight;
            autumnColor0 = lerp(summerColor, autumnColor0, summerToAutumn);

            float autumnBlend = saturate((seasonValue - 2.25) / 0.25);
            float3 autumnColor1 = NoiseBlendColor(_PlantAutumnColor0, _PlantAutumnColor1, noiseValue) * mainWeight;
            float3 autumnColor = lerp(autumnColor0, autumnColor1, autumnBlend);

            float autumnToWinter = saturate((seasonValue - 2.85) / 0.3);
            float3 winterColor0 = NoiseBlendColor(_PlantAutumnColor0, _PlantWinterColor1, noiseValue1) * mainWeight;
            winterColor0 = lerp(autumnColor, winterColor0, autumnToWinter);

            float winterBlend = saturate((seasonValue - 3.15) / 0.2);
            // winterColor0 是秋 -> 冬的过渡色，winterColor 是冬季稳定后的目标色。
            winterColor = lerp(winterColor0, winterColor, winterBlend);


            float seasonStep = step(0.0001, seasonColorBlend);
            // _BlendValue 控制“是否参与季节染色”，seasonStep 控制“当前材质有没有配置季节色”。
            float3 blendResult = baseColor * (1 - _BlendValue) + winterColor * _BlendValue;
            return lerp(baseColor, blendResult, seasonStep);
        }


        float4 SnowColor(float4 main, float2 uv, float s_w)
        {
            half4 snow = _SnowTex.Sample(sampler_MainTex, uv);
            half3 mainSnow = main.xyz * (1 - snow.a) + snow.xyz * snow.a;
            half snowA = main.a * (1 - snow.a) + snow.a;
            half snowValue = s_w;
            return main * (1 - snowValue) + half4(mainSnow.xyz, snowA) * snowValue;
        }

        float4 GrassColor(float4 main, float2 uv, float2 posUV)
        {
            half4 _DepthColor = _DepthTex.Sample(sampler_MainTex, uv);
            half4 _GrassColor = SAMPLE_TEXTURE2D(_GrassTex, sampler_GrassTex, posUV);
            float GrassColorValue = _GrassColor.r * _GrassColor.g * 0.5 + _GrassColor.g * 0.5;
            GrassColorValue = GrassColorValue;


            float heightMask = step(GrassColorValue, _DepthColor.g) * (1 - step(_DepthColor.g, 0));
            main.a = main.a * heightMask;
            return main;
        }

        float GetShadowMoveStep(float4 moveValue)
        {
            float moveStep = (1 - step(moveValue.y, 0)) + step(moveValue.a, 0) + (1 - step(moveValue.x, 0)) + (1 -
                step(moveValue.z, 0));
            moveStep = clamp(moveStep, 0, 1);
            return (1 - _HideNormal) * moveStep + _HideNormal;
        }

        float4 ShadowColor(float4 result, float3 lightCol, float2 screenUv, float4 moveValue)
        {
            half _light_value = (lightCol.x + lightCol.y + lightCol.z) / 3;
            float globalValue = (_GlobalColor.x + _GlobalColor.y + _GlobalColor.z) / 3;


            _light_value -= globalValue * _ShadowValue;
            _light_value = clamp(_light_value, 0, 1);
            _light_value = (1 - _light_value * 0.5);


            half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, screenUv);
            shadow.xyz *= _light_value;
            half3 shadowColor = GlobalColor.xyz * GlobalColor.a * 0.5;
            float moveStep = GetShadowMoveStep(moveValue);
            shadow *= moveStep;

            result.xyz = shadowColor * result.xyz * shadow.r + result.xyz * (1 - shadow.r);
            return result;
        }

        float4 ShadowColor(float4 result, float2 screenUv, float4 moveValue)
        {
            float globalValue = (_GlobalColor.x + _GlobalColor.y + _GlobalColor.z) / 3;

            half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, screenUv);

            half3 shadowColor = GlobalColor.xyz * GlobalColor.a * 0.5 * shadow.r;
            float moveStep = GetShadowMoveStep(moveValue);


            result.xyz = shadowColor * result.xyz * moveStep + result.xyz * (1 - shadow.r * moveStep);
            return result;
        }
        ENDHLSL



        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma multi_compile _ SKINNED_SPRITE
            #pragma shader_feature_local _FEATURE_WATER
            #pragma shader_feature_local _FEATURE_SEASON
            #pragma shader_feature_local _FEATURE_SNOW
            #pragma shader_feature_local _FEATURE_MOVE
            #pragma shader_feature_local _FEATURE_CHARACTER
            #pragma shader_feature_local _FEATURE_DAMP
            #pragma shader_feature_local _FEATURE_GRASS
            #pragma shader_feature_local _FEATURE_SHADOW
            #pragma shader_feature_local _FEATURE_FLOWER
            #pragma shader_feature_local _FEATURE_SIMPLE

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
                #if defined(_FEATURE_SIMPLE)
                {
                    main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                    main.xyz = i.color.xyz;
                    main.a *= i.color.a;

                    return main;
                }
                #endif

                SurfaceInput surface = BuildSurfaceInput(i);
                float2 lightingUV = surface.lightingUV;
                float2 worldScreenPos = surface.worldScreenPos;
                uv = surface.uv;
                main = surface.mainTex;

                half4 moveMaskSample = 0;
                #if defined(_FEATURE_SEASON) || defined(_FEATURE_SHADOW)
                {
                    moveMaskSample = _MoveMask.Sample(sampler_MainTex, uv);
                }
                #endif
                #if defined(_FEATURE_SEASON)
                {
                    main.xyz = BlendSeasonColor(main.xyz, moveMaskSample.g, i.fullWorldPos.xy);
                }
                #endif
                #if defined(_FEATURE_SNOW)
                {
                    main = SnowColor(main, uv, surface.snowValue);
                }
                #endif
                #if defined(_FEATURE_FLOWER)
                {
                    half4 flower = _FlowerTex.Sample(sampler_MainTex, uv);
                    flower.xyz = flower.xyz * flower.a * 1.2;
                    half flowerColorValue = (flower.x + flower.y + flower.z) / 3;
                    half flowerRemapValue = 0;
                    half flowerRemapValue0 = 0;
                    half flowerRemapValue1 = 0;
                    Unity_Remap_float(_SeasonValue, _FlowerRemap.xy, float2(0, _FlowerRemap.w), flowerRemapValue0);
                    Unity_Remap_float(_SeasonValue, _FlowerRemap.yz, float2(_FlowerRemap.w, 0), flowerRemapValue1);
                    half remapStep = step(_FlowerRemap.y, _SeasonValue);
                    flowerRemapValue = flowerRemapValue0 * (1 - remapStep) + flowerRemapValue1 * remapStep;

                    half flowerStep = step(1 - flowerRemapValue, flowerColorValue);
                    flowerColorValue *= flowerStep;
                    main.xyz = main.xyz * (1 - flowerColorValue) + flower.xyz * flowerColorValue;
                }
                #endif


                half4 result = 0;
                float singleValue = (main.x + main.y + main.z) / 3;
                float3 singleColor = main.xyz * (i.color.a) + singleValue.xxx * (1 - i.color.a);
                float3 waterColor = main.xyz * i.color.xyz;


                waterColor.xyz = waterColor.xyz * (1 - _BlendVertexColor) + singleColor * _BlendVertexColor;
                main.a = main.a * i.color.a * (1 - _BlendVertexColor) + main.a * _BlendVertexColor;
                #if defined(_FEATURE_DAMP)
                {
                    waterColor = DampColor(waterColor, lightingUV, uv);
                }
                #endif


                main.xyz = waterColor.xyz;
                #if defined(_FEATURE_WATER)
                {
                    float2 fixScreenUV = i.fixScreenUV.xy / i.fixScreenUV.w;
                    fixScreenUV = UnityStereoTransformScreenSpaceTex(fixScreenUV);
                    float4 outWaterColor = WaterFragment(uv, fixScreenUV, lightingUV, main);
                    waterColor = outWaterColor.xyz;
                    waterStepMask = outWaterColor.w;
                }
                #endif

                #if defined(_FEATURE_GRASS)
                {
                    main = GrassColor(main, uv, worldScreenPos.xy);
                }
                #endif
                result.xyz = waterColor.xyz;

                result.a = main.a;


                #if defined(_FEATURE_SHADOW)
                {
                    result = ShadowColor(result, lightingUV, moveMaskSample);
                }
                #endif

                result.xyz = result.xyz * (1 - _FixedColor.a) + _FixedColor.xyz * _FixedColor.a;
                clip(main.a - 0.1);
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
            #pragma shader_feature_local _FEATURE_WATER
            #pragma shader_feature_local _FEATURE_SEASON
            #pragma shader_feature_local _FEATURE_SNOW
            #pragma shader_feature_local _FEATURE_MOVE
            #pragma shader_feature_local _FEATURE_CHARACTER
            #pragma shader_feature_local _FEATURE_GRASS
            #pragma shader_feature_local _FEATURE_SIMPLE

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
                #if defined(_FEATURE_SIMPLE)
                {
                    return 0;
                }
                #endif

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
                #if defined(_FEATURE_SIMPLE)
                {
                    main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                    main.a *= i.color.a;
                    clip(main.a - 0.1);
                    return main.a;
                }
                #endif


                SurfaceInput surface = BuildSurfaceInput(i);
                float2 worldScreenPos = surface.worldScreenPos;
                uv = surface.uv;
                main = surface.mainTex;

                #if defined(_FEATURE_SNOW)
                {
                    main = SnowColor(main, uv, surface.snowValue);
                }
                #endif

                main.a = main.a * i.color.a * (1 - _BlendVertexColor) + main.a * _BlendVertexColor;

                #if defined(_FEATURE_GRASS)
                {
                    main = GrassColor(main, uv, worldScreenPos.xy);
                }
                #endif

                clip(main.a - 0.1);
                return main.a;
            }


            OutData CombinedShapeLightFragment(Varyings i)
            {
                OutData outData = (OutData)0;
                half alpha = GetAuxAlpha(i);
                float waterStepMask = 0;
                #if defined(_FEATURE_WATER)
                {
                    waterStepMask = GetAuxWaterStepMask(i);
                }
                #endif
                outData.waterStepMask = float4(waterStepMask.xxx, alpha);
                outData.normalColor = DefaultNormal(i);
                outData.depthColor = DefaultObjDepth(i);
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
                #if defined(DEBUG_DISPLAY)
                float3 positionWS : TEXCOORD2;
                #endif
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

                o.positionCS = TransformWorldToHClip(worldPos);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = worldPos;
                #endif
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
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                float3 objectWorldPos = TransformObjectToWorld(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = objectWorldPos;
                #endif
                o.uv = attributes.uv;

                float3 ObjPos = unity_ObjectToWorld._m03_m13_m23;
                float stepPosZ = step(49, ObjPos.z);

                float3 objectSortPos = ObjPos;
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
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex, i.uv);

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

                half depthValue = (DepthTex.r - 0.5) * (1 - otherStep) + (DepthTex.r + DepthTex.b - 1) * (1 -
                    stepDepthOne) * otherStep;
                half offset = depthValue * 512 * 4 / _ScreenParams.y;


                half depth = worldScreenPos.y + offset;
                half depthHighStep = depthStep_G;

                half high = i.worldScreenPos.z * (1 - depthHighStep) + DepthTex.g * 2 * depthHighStep;


                mainTex.xyz = half3(depth, high, _NormalColor.g * 0.5 + stepDepthOne);
                mainTex.z += ObjDepthTex.z;
                mainTex.a = mainTex.a * (1 - stepDepthOne) + DepthTex.a * stepDepthOne;


                return mainTex;
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
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.worldPos.xyz = unity_ObjectToWorld._m03_m13_m23;
                o.worldPos.w = o.worldPos.z;
                o.worldPos.z += o.worldPos.y;
                o.uv = v.uv;
                o.lightingUV = ComputeScreenPos(o.positionCS);


                half3 pos = TransformObjectToWorld(_WorldSpaceCameraPos.xyz);
                half4 cameraClipPos = TransformWorldToHClip(pos);


                o.fixScreenUV = o.lightingUV - ComputeScreenPos(cameraClipPos);

                o.color = v.color * unity_SpriteColor;
                return o;
            }

            float3 WaterPassMaskFragment(float2 uv, float2 screenUV)
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
                float3 result = WaterPassMaskFragment(i.uv, lightingUV);
                return float4(result.xyz, 1);
            }
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "Grass"
            }

            HLSLPROGRAM
            #pragma vertex  Vertex
            #pragma fragment  Fragment

            struct GrassAttributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct GrassVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            GrassVaryings Vertex(GrassAttributes v)
            {
                GrassVaryings o = (GrassVaryings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;

                return o;
            }


            half4 Fragment(GrassVaryings i) : SV_Target
            {
                half4 mainTex = _MainTex.Sample(sampler_MainTex, i.uv);
                float4 moveValue = _MoveMask.Sample(sampler_MainTex, i.uv);
                moveValue.a = mainTex.a;
                return moveValue;
            }
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "GroundFoot"
            }

            HLSLPROGRAM
            #pragma vertex  Vertex
            #pragma fragment  Fragment

            struct GroundFootAttributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct GroundFootVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            GroundFootVaryings Vertex(GroundFootAttributes v)
            {
                GroundFootVaryings o = (GroundFootVaryings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(v);

                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }


            half4 Fragment(GroundFootVaryings i) : SV_Target
            {
                half4 mainTex = _MainTex.Sample(sampler_MainTex, i.uv);
                float4 moveValue = _MoveMask.Sample(sampler_MainTex, i.uv);
                moveValue.a *= mainTex.a;

                int grassStep = 1 - step(moveValue.x + moveValue.y, 0);
                float outValue = grassStep * (moveValue.z + 0.8) + moveValue.z;

                return float4(outValue.xxx, moveValue.a);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
    CustomEditor "FeatureFlagsGUI"
}
