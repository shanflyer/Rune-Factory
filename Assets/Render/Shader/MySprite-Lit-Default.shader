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
        //_SeasonValue("_SeasonValue",Range(0,4))=0
        _PlantAutumnNoiseScale("_PlantAutumnNoiseScale",float)=1



        [Toggle]_Damp("_Damp",int)=0


        //水面颜色
        [HDR]waterColor("waterColor", Color) = (0,0.5,0.5,0.5)
        //初始透明
        _WaterZero("_WaterZero", Range(0,1)) = 0
        //水低调整
        _WaterBottom("_WaterBottom", Range(0,4)) = 1
        //水面高度
        _WaterHigh("_WaterHigh", Range(0,1)) = 0
        //波纹亮度补偿
        waterValue("waterValue", Range(0, 0.4)) = 0.2
        //噪声纹理系数
        waterNoiseScale("waterNoiseScale", Range(0, 300)) = 0

        //噪声运动方向角1
        _WaveAngle0("_WaveAngle0", Range(-180, 180)) = 0
        //噪声运动速度1
        _WaveSpeed0("_WaveSpeed0", Range(0, 0.2)) = 0
        //噪声碎片大小1
        WaveScale0("WaveScale0", Vector) = (1, 1, 0, 0)

        //噪声运动方向角2
        _WaveAngle1("_WaveAngle1", Range(-180, 180)) = 0
        //噪声运动速度2
        _WaveSpeed1("_WaveSpeed1", Range(0, 0.2)) = 0
        //噪声碎片大小2
        WaveScale1("WaveScale1", Vector) = (1, 1, 0, 0)


        // [Title(water,Edge)] 
        //边缘颜色
        [HDR]EdgeColor("EdgeColor", Color) = (0.990566, 0.9765486, 0.9765486, 0)
        //边缘宽度
        EdgeValue("EdgeValue", Range(0, 0.2))=0.1
        //边缘速度
        _EdgeWaveSpeed("EdgeWaveSpeed",Range(0,4))=0
        //边缘偏移
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
        Texture2D _MoveMask;
        Texture2D _SnowTex;
        Texture2D _FlowerTex;
        //exture2D _WaterNormalMap;  
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

            uint _FeatureFlags; // 打包所有开关
        CBUFFER_END

        #define FEAT_WATER (1u<<0) // 第0位：水面
        #define FEAT_DAMPBLEND     (1u<<1) // 第1位：湿润
        #define FEAT_MOVE    (1u<<2) // 第2位：网格动态
        #define FEAT_SEASONCOLORBLEND    (1u<<3) // 第3位：季节变化
        #define FEAT_SNOWBLEND     (1u<<4) // 第4位：雪地
        #define FEAT_GRASSBLEND     (1u<<5) // 第5位：草地
        #define FEAT_SHADOWSTEP     (1u<<6) // 第6位：接受影子
        #define FEAT_FLOWERSTEP    (1u<<7) // 第7位：花
        #define FEAT_SIMPLE    (1u<<8) // 第7位：花
        #define FEAT_CHARACTER    (1u<<9) // 第8位：角色 

        float My_SimpleNoise_float(float2 uv, float scale)
        {
            half4 col = SAMPLE_TEXTURE2D(_WindNoiseTexture, sampler_WindNoiseTexture, uv/scale);
            return col.r;
        }

        float4 WaterFragment(float2 uv, float2 fixedScreenUV, float2 screenUV, float4 _MainTexColor)
        {
            float2 mirrorUV = screenUV;
            float3 _WaterMask = SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).xyz;
            //水域范围
            float stepMask = step(0.06, _WaterMask.r);

            half edgeOffsetValue = _SinTime.w * _EdgeWaveSpeed;
            edgeOffsetValue = abs(edgeOffsetValue);
            edgeOffsetValue = clamp(edgeOffsetValue, 0, 1);
            _WaterHigh = _WaterHigh + _EdgeWaveOffset * edgeOffsetValue;
            //return _EdgeWaveOffset*edgeOffsetValue;

            /*
            float svalue =_ScreenParams.y/ 1920;
            svalue=floor(svalue);
            svalue=clamp(svalue,1,svalue);
            svalue/=2;
            float2 offsetUv= _WorldSpaceCameraPos.xy*svalue*800/_ScreenParams.xy;
            screenUV+=offsetUv;*/

            //波纹1
            float angle0 = radians(_WaveAngle0); //转换角度为弧度
            float2 waveValue0 = float2(cos(angle0), sin(angle0)) * _WaveSpeed0;

            float2 _WaveT0 = (_TimeParameters.x.xx) * waveValue0;
            float2 _TilingAndOffset0 = fixedScreenUV * WaveScale0 + _WaveT0;
            float4 _WaveCol0 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, _TilingAndOffset0);
            _WaveCol0.rgb = UnpackNormal(_WaveCol0);

            //波纹2
            float angle1 = radians(_WaveAngle1);
            float2 waveValue1 = float2(cos(angle1), sin(angle1)) * _WaveSpeed1;
            float2 _WaveT2 = (_TimeParameters.x.xx) * waveValue1;
            float2 _TilingAndOffset1 = fixedScreenUV * WaveScale1 + _WaveT2;
            float4 _WaveCol1 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, _TilingAndOffset1);
            _WaveCol1.rgb = UnpackNormal(_WaveCol1);


            //波纹叠加
            float3 _endWave = _WaveCol0.xyz + _WaveCol1.xyz;
            //波纹r、g叠加
            float waveBlendCol = _endWave[0] + _endWave[1];
            waveBlendCol = clamp(waveBlendCol, 0, 1);
            waveBlendCol *= waterValue;
            //噪声
            float _waterNoise;
            Unity_SimpleNoise_float(fixedScreenUV.xy, waterNoiseScale, _waterNoise);
            waveBlendCol *= _waterNoise;


            //波纹与边缘混合 
            //映射水面深度
            Unity_Remap_float(_WaterMask.r, float2(0, 1), float2(_WaterZero, _WaterBottom), _WaterMask.r);
            _WaterMask.r = clamp(_WaterMask.r, 0, 1);


            float _WaterMask1 = step(_WaterHigh, _WaterMask.r);
            float _WaterMask2 = step(_WaterHigh + EdgeValue, _WaterMask.r);
            stepMask *= _WaterMask1;

            float _EdgeMaskValue = _WaterMask.r;
            Unity_Remap_float(_EdgeMaskValue, float2(_WaterHigh, _WaterHigh + EdgeValue), float2(0, 1), _EdgeMaskValue);

            float edge = (_WaterMask1 - _WaterMask2) * _EdgeMaskValue;


            //float3 edgeAddColor=edge*float3(0,1,1)*2; 
            float3 endWaveColor = edge * EdgeColor.xyz * waveBlendCol + waveBlendCol * _WaterMask1.rrr;
            endWaveColor = clamp(endWaveColor, 0, 1);

            half sunValue = (_SunColor.x + _SunColor.y + _SunColor.z) / 3;

            //主颜色
            float3 _MainColor = waterColor.xyz * waterColor.a;
            _MainColor += (1 - waterColor.a) * _MainTexColor.xyz;
            _MainColor.xyz *= _WaterMask.r;

            endWaveColor = endWaveColor.xyz * _SunColor.xyz / sunValue;

            float3 outWater = endWaveColor + _MainColor;
            outWater = clamp(outWater, 0, 1);

            outWater = outWater + waterColor.xyz * waterColor.a;

            float water_valueX = outWater.r;
            float water_valueY = outWater.g;
            Unity_Remap_float(water_valueX, float2(0, 1), float2(-0, 0.1), water_valueX);
            Unity_Remap_float(water_valueY, float2(0, 1), float2(-0.02, 0.02), water_valueY);

            mirrorUV.x += water_valueX;
            mirrorUV.y += water_valueY;

            float3 MirrorTexColor = SAMPLE_TEXTURE2D(_MirrorTex, sampler_MirrorTex, mirrorUV).xyz;
            MirrorTexColor.xyz *= _MirrorBlend.xyz;
            float MirrorValue = (MirrorTexColor.x + MirrorTexColor.y + MirrorTexColor.z) / 3;
            //return MirrorTexColor;

            outWater = outWater * (1 - MirrorValue) + MirrorTexColor * MirrorValue;
            outWater = stepMask * outWater + _MainTexColor.xyz * (1 - stepMask);
            return float4(outWater.xyz, stepMask);
        }

        float3 DampColor(float3 col, float2 uv, float2 objUV)
        {
            float r = col.r * col.r;
            float g = col.g * col.g;
            float b = col.b * col.b;
            float3 _col = float3(r, g, b);
            float3 col1 = _col;

            float _DampNoiseValue;

            float svalue = _ScreenParams.y / 1920;
            svalue = floor(svalue);
            svalue = clamp(svalue, 1, svalue);
            svalue /= 2;


            Unity_SimpleNoise_float(uv + _WorldSpaceCameraPos.xy * svalue * 800 / _ScreenParams.xy, _DampNoise,
                                    _DampNoiseValue);
            float3 d = float3(_DampNoiseValue, _DampNoiseValue, _DampNoiseValue);


            float3 water = float3(1 - _DampNoiseValue, 1 - _DampNoiseValue, 1 - _DampNoiseValue);
            float waterValue = clamp((_DampValue - 0.5), 0, 0.5) / 0.5;

            float _HighLightNoiseValue;
            Unity_SimpleNoise_float(uv + _WorldSpaceCameraPos.xy * svalue * 800 / _ScreenParams.xy, _HighLightNoise,
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
            //return water*_WaterColor;

            h *= waterValue;
            h *= step(_HighLighStep, h);
            h *= h;

            h = clamp(h, 0, 1);


            float c = (col.r + col.g + col.b) / 3;

            d *= _DampColor.xyz * c;
            water *= _DampWaterColor.xyz * c;
            h *= _HightLightColor.xyz * c;

            _col += d + water + h;
            half4 normal = _NormalMap.Sample(sampler_MainTex, objUV);
            //half3 normalUnpacked = UnpackNormalRGBNoScale(normal);
            float gv = normal.z;
            //return normal.zzz;
            //float stepGv=step(0.5,gv);
            //gv=stepGv+(1-stepGv)*gv; 
            // return gv.xxx;
            Unity_Remap_float(gv, float2(0, 1), float2(0.2, 1), gv);

            float3 result = lerp(col, _col, clamp(_DampValue / 0.5, 0, 1) * gv);
            //return _DampValue.xxx;
            //result=lerp(col,result,_DampValue); 
            return result * (1 - _Damp) + _col * _Damp;
        }

        float2 MoveUV(float2 uv, float2 screenUV, float SnowMove, out float2 offset)
        {
            float svalue = _ScreenParams.y / 1920;
            svalue = floor(svalue);
            svalue = clamp(svalue, 1, svalue);
            svalue /= 2;
            float2 offsetUv = _WorldSpaceCameraPos.xy * svalue * 800 / _ScreenParams.xy;
            screenUV += offsetUv;
            screenUV = _MoveSelfUV * uv + (1 - _MoveSelfUV) * screenUV;

            float2 panner63 = _WindScroll * 0.3 * _TimeParameters.x + screenUV;
            float2 panner74 = _TimeParameters.x * _WindJitter * 0.5 + screenUV * 2;

            float4 WindNoise0 = SAMPLE_TEXTURE2D(_WindNoiseTexture, sampler_WindNoiseTexture, panner63);
            WindNoise0 = pow(abs(WindNoise0), 2.5);
            float4 WindNoise1 = SAMPLE_TEXTURE2D(_WindNoiseTexture, sampler_WindNoiseTexture, panner74);

            half4 moveValue = _MoveMask.Sample(sampler_MainTex, uv);
            float windValue = lerp(1, 2, abs(_WindValue));
            //return float2(moveValue.x,moveValue.x);
            float value = moveValue.x * _WindNoiseValue * windValue;
            offset = WindNoise0.x * WindNoise1.x * value * SnowMove;
            int stepWind = step(0, _WindValue);
            offset.x = offset.x * stepWind - offset.x * (1 - stepWind);

            return offset + uv;
        }

        float3 BlendSeasonColor(float3 main, float2 uv, float2 worldUV)
        {
            const half4 grassTex = _MoveMask.Sample(sampler_MainTex, uv);
            half _BlendValue = 1 - step(grassTex.g, 0);
            //return _BlendValue.xxx;

            float mainValue = (main.y);

            mainValue = clamp(mainValue, 0, 1);

            float noiseValue = My_SimpleNoise_float(worldUV, _PlantAutumnNoiseScale);
            float noiseValue1 = My_SimpleNoise_float(worldUV, _PlantAutumnNoiseScale * 2);

            int seasonColorBlend = _PlantSpringColor.x + _PlantSpringColor.y + _PlantSpringColor.z;

            //春季颜色
            int springBlend = 1 - step(1, _SeasonValue);
            float w_s = _SeasonValue;
            Unity_Remap_float(w_s, float2(0, 0.15), float2(0, 1), w_s);
            int w_sBlend = 1 - step(0.5, w_s);
            float springValue = _SeasonValue;
            Unity_Remap_float(springValue, float2(0.15, 0.5), float2(0, 1), springValue);
            springValue = clamp(springValue, 0, 1);
            float3 winterColor = (_PlantWinterColor * noiseValue + _PlantWinterColor1 * (1 - noiseValue)) * mainValue;
            float3 springColor0 = (_PlantWinterColor * noiseValue1 + _PlantSpringColor * (1 - noiseValue1)) * mainValue;
            float3 springColor = (_PlantSpringColor * noiseValue + _PlantSpringColor1 * (1 - noiseValue)) * mainValue;
            springColor = (winterColor * (1 - w_s) + springColor0 * w_s) * w_sBlend
                + (1 - w_sBlend) * (springColor0 * (1 - springValue) + springColor * springValue);

            //夏季颜色
            float s_s = _SeasonValue;
            Unity_Remap_float(s_s, float2(1, 1.25), float2(0, 1), s_s);
            s_s = clamp(s_s, 0, 1);
            float3 summerColor = springColor * (1 - s_s) + main.xyz * s_s;

            //秋季颜色
            float s_a = _SeasonValue;
            Unity_Remap_float(s_a, float2(2, 2.25), float2(0, 1), s_a);
            s_a = clamp(s_a, 0, 1);

            float3 AutumnColor0 = (main.xyz * noiseValue1 + _PlantAutumnColor0 * (1 - noiseValue1)) * mainValue;
            AutumnColor0 = AutumnColor0 * s_a + summerColor * (1 - s_a);


            float a_a = _SeasonValue;
            Unity_Remap_float(a_a, float2(2.25, 2.5), float2(0, 1), a_a);
            a_a = clamp(a_a, 0, 1);

            float3 AutumnColor = (_PlantAutumnColor0 * noiseValue + _PlantAutumnColor1 * (1 - noiseValue)) * mainValue;
            AutumnColor = AutumnColor * a_a + AutumnColor0 * (1 - a_a);


            //冬季颜色
            float a_w = _SeasonValue;
            Unity_Remap_float(a_w, float2(2.85, 3.15), float2(0, 1), a_w);
            a_w = clamp(a_w, 0, 1);
            float3 winterColor0 = (_PlantAutumnColor0 * noiseValue1 + _PlantWinterColor1 * (1 - noiseValue1)) *
                mainValue;

            winterColor0 = winterColor0 * a_w + AutumnColor * (1 - a_w);

            float w_w = _SeasonValue;
            Unity_Remap_float(w_w, float2(3.15, 3.35), float2(0, 1), w_w);
            w_w = clamp(w_w, 0, 1);
            winterColor = winterColor * w_w + winterColor0 * (1 - w_w);

            //return winterColor.xyz;


            int seasonStep = 1 - step(seasonColorBlend, 0);
            return main.xyz * (1 - _BlendValue) * seasonStep + winterColor * _BlendValue * seasonStep;
        }


        float4 SnowColor(float4 main, float2 uv, float s_w)
        {
            half4 snow = _SnowTex.Sample(sampler_MainTex, uv);
            //return snow;
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


            float HightValue = step(GrassColorValue, _DepthColor.g) * (1 - step(_DepthColor.g, 0));
            main.a = main.a * HightValue;
            return main;
        }

        float4 ShadowColor(float4 result, float3 lightCol, float2 ScrennUv, float2 uv)
        {
            half _light_value = (lightCol.x + lightCol.y + lightCol.z) / 3;
            float globalValue = (_GlobalColor.x + _GlobalColor.y + _GlobalColor.z) / 3;


            _light_value -= globalValue * _ShadowValue;
            _light_value = clamp(_light_value, 0, 1);
            _light_value = (1 - _light_value * 0.5);


            half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, ScrennUv);
            shadow.xyz *= _light_value;
            half3 shadowColor = GlobalColor.xyz * GlobalColor.a * 0.5;
            float4 moveValue = _MoveMask.Sample(sampler_MainTex, uv);

            int moveAStep = 1 - step(moveValue.a, 0);
            int moveYStep = 1 - step(moveValue.y, 0);
            int moveZStep = 1 - step(moveValue.z, 0);
            int moveXStep = 1 - step(moveValue.x, 0);
            //return moveYStep.xxxx;
            int moveStep = moveYStep + (1 - moveAStep) + moveXStep + moveZStep;
            moveStep = clamp(moveStep, 0, 1);
            moveStep = (1 - _HideNormal) * moveStep + _HideNormal;
            shadow *= moveStep;

            result.xyz = shadowColor * result.xyz * shadow.r + result.xyz * (1 - shadow.r);
            return result;
        }

        float4 ShadowColor(float4 result, float2 ScrennUv, float2 uv)
        {
            float globalValue = (_GlobalColor.x + _GlobalColor.y + _GlobalColor.z) / 3;

            half4 shadow = SAMPLE_TEXTURE2D(_ShadowTex, sampler_ShadowTex, ScrennUv);

            half3 shadowColor = GlobalColor.xyz * GlobalColor.a * 0.5 * shadow.r;

            float4 moveValue = _MoveMask.Sample(sampler_MainTex, uv);

            int moveAStep = 1 - step(moveValue.a, 0);
            int moveYStep = 1 - step(moveValue.y, 0);
            int moveZStep = 1 - step(moveValue.z, 0);
            int moveXStep = 1 - step(moveValue.x, 0);
            //return moveYStep.xxxx;
            int moveStep = moveYStep + (1 - moveAStep) + moveXStep + moveZStep;
            moveStep = clamp(moveStep, 0, 1);
            moveStep = (1 - _HideNormal) * moveStep + _HideNormal;


            result.xyz = shadowColor * result.xyz * moveStep + result.xyz * (1 - shadow.r * moveStep);
            // result.xyz *= globalValue;
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

            struct OutData
            {
                float4 outColor:SV_Target0;
                float4 normalColor:SV_Target1;
                float4 depthColor:SV_Target2;
                float4 waterStepMask:SV_Target3;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            Varyings DefaultVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;

                float3 ObjPos = unity_ObjectToWorld._m03_m13_m23;
                float3 objWroldPos = TransformObjectToWorld(attributes.positionOS);
                float3 worldOS = objWroldPos;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SKINNED_VERTEX_COMPUTE(attributes);

                [branch] if ((_FeatureFlags & FEAT_CHARACTER) != 0u)
                {
                    o.positionCS = TransformObjectToHClip(attributes.positionOS);
                }
                else
                {
                    // worldOS.z += (ObjPos.z + ObjPos.y) * _ZOffset;
                    o.positionCS = TransformWorldToHClip(worldOS);
                }


                [branch] if ((_FeatureFlags & FEAT_SIMPLE) != 0u)
                {
                    o.color = attributes.color * _Color * unity_SpriteColor;
                    o.uv = attributes.uv.xy;
                    return o;
                }

                o.color = attributes.color * unity_SpriteColor;
                float stepPosZ = 1 - step(50, ObjPos.z);

                float3 _objSortPos = ObjPos;
                float offsetPosZ = step(ObjPos.z, -10);
                _objSortPos.y += _objSortPos.z * offsetPosZ;
                float4 worldClip = TransformWorldToHClip(_objSortPos);

                float high = stepPosZ * (objWroldPos.y - ObjPos.y) * 0.5;
                float positionCSY = o.positionCS.y;

                stepPosZ = clamp(stepPosZ, 0, 1);
                worldClip.y = (1 - stepPosZ) * positionCSY + stepPosZ * worldClip.y;
                o.worldScreenPos = ComputeScreenPos(worldClip);
                o.worldScreenPos.z = clamp(high, 0, 1);


                objWroldPos.y = unity_ObjectToWorld._m13;

                half4 worldPosCs = TransformWorldToHClip(objWroldPos.xyz);
                float4 worldScreenPos = ComputeScreenPos(worldPosCs);
                worldScreenPos.xy = worldScreenPos.xy / worldScreenPos.w;
                o.fullWorldPos = float4(objWroldPos.xy, worldScreenPos.xy);


                half3 cameraOffsetPos = _WorldSpaceCameraPos.xyz - unity_ObjectToWorld._m03_m13_m23;
                half3 pos = worldOS.xyz + cameraOffsetPos;

                half4 carmeraPos = TransformWorldToHClip(pos);

                o.fixScreenUV = ComputeScreenPos(carmeraPos);


                o.uv = attributes.uv.xy;


                o.normalWS = -GetViewForwardDir();
                o.tangentWS = attributes.tangent.xyz;


                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                o.lightingUV = ComputeScreenPos(o.positionCS);

                float3 xAxis = unity_ObjectToWorld._m00_m10_m20;
                // 归一化并计算与世界 X 轴的点积
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
                [branch] if ((_FeatureFlags & FEAT_CHARACTER) != 0u)
                {
                    mainTex.xyz = 0;
                }
                else
                {
                    float2 worldScreenPos = i.worldScreenPos.xy / i.worldScreenPos.w;
                    worldScreenPos = UnityStereoTransformScreenSpaceTex(worldScreenPos);
                    // float2 screenUV = i.lightingUV.xy / i.lightingUV.w;
                    // screenUV = UnityStereoTransformScreenSpaceTex(screenUV);

                    float4 DepthTex = _DepthTex.Sample(sampler_MainTex, i.uv.xy);
                    float clipA = 1 - step(DepthTex.a, 0.01);

                    DepthTex.xyz *= clipA;
                    half4 _NormalColor = _NormalMap.Sample(sampler_MainTex, i.uv.xy);

                    half depthStep_R = 1 - step(abs(DepthTex.r - 0.5), 0.01);
                    half depthStep_G = 1 - step(abs(DepthTex.g - 0.5), 0.01);
                    half depthStep_B = 1 - step(abs(DepthTex.b - 0.5), 0.01);
                    half depthStep_ZeroB = 1 - step(DepthTex.b, 0);
                    half stepDepthOne = step(1, DepthTex.b);
                    depthStep_ZeroB *= (1 - stepDepthOne);

                    int clearColor = 1 - step(DepthTex.b, 0) * step(DepthTex.r, 0) * step(DepthTex.g, 0);

                    half otherStep = depthStep_R * depthStep_G + depthStep_B;

                    otherStep = clamp(otherStep, 0, 1) * depthStep_ZeroB;


                    half depthValue = (DepthTex.r - 0.5) * (1 - otherStep) + (DepthTex.r + DepthTex.b - 1) * (1 -
                        stepDepthOne) * otherStep;
                    half offset = depthValue * 512 * 4 / _ScreenParams.y;

                    half depth = worldScreenPos.y + offset * clearColor;
                    half setpHigh = depthStep_G;

                    // return float4(i.worldScreenPos.zzz, 1);
                    half high = i.worldScreenPos.z * (1 - setpHigh) + DepthTex.g * 2 * setpHigh;


                    mainTex.xyz = half3(depth, high, _NormalColor.g * 0.5 + stepDepthOne);

                    mainTex.a = (mainTex.a * (1 - stepDepthOne) + DepthTex.a * stepDepthOne);
                    mainTex.a = clamp(mainTex.a, 0, 1);
                }


                clip(a - 0.5);
                return mainTex;
            }

            half4 DefaultNormal(Varyings i)
            {
                float2 uv = i.uv.xy;
                float2 lightingUV = i.lightingUV.xy / i.lightingUV.w;
                lightingUV = UnityStereoTransformScreenSpaceTex(lightingUV);

                float s_w = 0;
                [branch] if ((_FeatureFlags & FEAT_SNOWBLEND) != 0u)
                {
                    Unity_Remap_float(_SeasonValue, float2(2.95, 3.05), float2(0, 1), s_w);
                    s_w = clamp(s_w, 0, 1);

                    float s_w1 = 0;
                    Unity_Remap_float(_SeasonValue, float2(0.1, 0), float2(0, 1), s_w1);
                    s_w1 = clamp(s_w1, 0, 1);
                    s_w += s_w1;
                }

                [branch] if ((_FeatureFlags & FEAT_MOVE) != 0u)
                {
                    float2 offset;
                    uv = MoveUV(uv, lightingUV, 1 - s_w, offset);
                }


                half4 mainTex = _MainTex.Sample(sampler_MainTex, uv);
                half4 _NormalColor = _NormalMap.Sample(sampler_MainTex, uv);
                // _NormalColor.b=1-_NormalColor.b;
                [branch] if ((_FeatureFlags & FEAT_SNOWBLEND) != 0u)
                {
                    mainTex = SnowColor(mainTex, uv, s_w);
                }

                half3 normalTS = _NormalColor.xyz;
                half4 result = half4(1, 1, 1, 1);
                normalTS = UnpackNormal(_NormalColor);

                //return float4(i.tangentWS.xyz,1);
                result = NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                result.x = unity_SpriteProps.x * result.x + (1 - unity_SpriteProps.x) * (1 - result.x);
                result.z = 0;

                // result.z-=i.positionCS.y; 
                result = result * i.color;
                // normalTS=WaterFragment(i.uv,i.screenUV,normalTS);

                return result;
            }

            half4 DefaultColor(Varyings i, out float waterStepMask)
            {
                float2 uv = i.uv.xy;
                waterStepMask = 0;
                [branch] if ((_FeatureFlags & FEAT_SIMPLE) != 0u)
                {
                    half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                    main.xyz = i.color.xyz;
                    main.a *= i.color.a;

                    return main;
                }


                float2 lightingUV = i.lightingUV.xy / i.lightingUV.w;
                lightingUV = UnityStereoTransformScreenSpaceTex(lightingUV);

                float2 worldScreenPos = i.fullWorldPos.zw;
                worldScreenPos = UnityStereoTransformScreenSpaceTex(worldScreenPos);

                float s_w = 0;
                [branch] if ((_FeatureFlags & FEAT_SNOWBLEND) != 0u)
                {
                    Unity_Remap_float(_SeasonValue, float2(2.95, 3.05), float2(0, 1), s_w);
                    s_w = clamp(s_w, 0, 1);

                    float s_w1 = 0;
                    Unity_Remap_float(_SeasonValue, float2(0.1, 0), float2(0, 1), s_w1);
                    s_w1 = clamp(s_w1, 0, 1);
                    s_w += s_w1;
                }


                [branch] if ((_FeatureFlags & FEAT_MOVE) != 0u)
                {
                    float2 offset;
                    uv = MoveUV(uv, lightingUV, 1 - s_w, offset);
                }

                half4 main = _MainTex.Sample(sampler_MainTex, uv);


                //return half4(uv.xxx,1);

                [branch] if ((_FeatureFlags & FEAT_SEASONCOLORBLEND) != 0u)
                {
                    main.xyz = BlendSeasonColor(main.xyz, uv, i.fullWorldPos.xy);
                }
                [branch] if ((_FeatureFlags & FEAT_SNOWBLEND) != 0u)
                {
                    main = SnowColor(main, uv, s_w);
                }

                [branch] if ((_FeatureFlags & FEAT_FLOWERSTEP) != 0u)
                {
                    half4 flower = _FlowerTex.Sample(sampler_MainTex, uv);
                    flower.xyz = flower.xyz * flower.a * 1.2;
                    half flowerColorValue = (flower.x + flower.y + flower.z) / 3;
                    half flowerRemapValue;
                    half flowerRemapValue0;
                    half flowerRemapValue1;
                    Unity_Remap_float(_SeasonValue, _FlowerRemap.xy, float2(0, _FlowerRemap.w), flowerRemapValue0);
                    Unity_Remap_float(_SeasonValue, _FlowerRemap.yz, float2(_FlowerRemap.w, 0), flowerRemapValue1);
                    half remapStep = step(_FlowerRemap.y, _SeasonValue);
                    flowerRemapValue = flowerRemapValue0 * (1 - remapStep) + flowerRemapValue1 * remapStep;

                    half flowerStep = step(1 - flowerRemapValue, flowerColorValue);
                    flowerColorValue *= flowerStep;
                    main.xyz = main.xyz * (1 - flowerColorValue) + flower.xyz * flowerColorValue;
                }


                half4 result;
                float singleValue = (main.x + main.y + main.z) / 3;
                float3 singleColor = main.xyz * (i.color.a) + singleValue.xxx * (1 - i.color.a);
                float3 waterColor = main.xyz * i.color.xyz;


                waterColor.xyz = waterColor.xyz * (1 - _BlendVertexColor) + singleColor * _BlendVertexColor;
                main.a = main.a * i.color.a * (1 - _BlendVertexColor) + main.a * _BlendVertexColor;
                [branch] if ((_FeatureFlags & FEAT_DAMPBLEND) != 0u)
                {
                    waterColor = DampColor(waterColor, lightingUV, uv);
                }


                main.xyz = waterColor.xyz;
                [branch] if ((_FeatureFlags & FEAT_WATER) != 0u)
                {
                    float2 fixScreenUV = i.fixScreenUV.xy / i.fixScreenUV.w;
                    fixScreenUV = UnityStereoTransformScreenSpaceTex(fixScreenUV);
                    float4 outWaterColor = WaterFragment(uv, fixScreenUV, lightingUV, main);
                    waterColor = outWaterColor.xyz;
                    waterStepMask = outWaterColor.w;
                }

                [branch] if ((_FeatureFlags & FEAT_GRASSBLEND) != 0u)
                {
                    main = GrassColor(main, uv, worldScreenPos.xy);
                }


                //  half4 lightCol = SAMPLE_TEXTURE2D(_LightingTex, sampler_LightingTex, lightingUV);
                //  lightCol.xyz *= 4;


                // result.xyz = waterColor.xyz * lightCol.xyz;
                // result.xyz = _LightBlend * result.xyz + (1 - _LightBlend) * waterColor.xyz;
                // result.a = main.a;

                result.xyz = waterColor.xyz;

                result.a = main.a;


                [branch] if ((_FeatureFlags & FEAT_SHADOWSTEP) != 0u)
                {
                    result = ShadowColor(result, lightingUV, uv);
                }

                result.xyz = result.xyz * (1 - _FixedColor.a) + _FixedColor.xyz * _FixedColor.a;
                clip(main.a - 0.1);
                return result;
            }


            OutData CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                OutData OutData;
                float waterStepMask = 0;
                OutData.outColor = DefaultColor(i, waterStepMask);
                OutData.waterStepMask = float4(waterStepMask.xxx, OutData.outColor.a);
                OutData.normalColor = DefaultNormal(i);
                OutData.depthColor = DefaultObjDepth(i);
                return OutData;
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

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                #if defined(DEBUG_DISPLAY)
                float3 positionWS : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };


            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
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

            float4 UnlitFragment(Varyings i) : SV_Target
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

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldScreenPos : TEXCOORD1;
            };


            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                attributes.positionOS = UnityFlipSprite(attributes.positionOS, unity_SpriteProps.xy);
                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                float3 objWroldPos = TransformObjectToWorld(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = objWroldPos;
                #endif
                o.uv = attributes.uv;

                float3 ObjPos = unity_ObjectToWorld._m03_m13_m23;
                float stepPosZ = step(49, ObjPos.z);

                float3 _objSortPos = ObjPos;
                float4 worldClip = TransformWorldToHClip(_objSortPos);
                float high = (1 - stepPosZ) * (objWroldPos.y - ObjPos.y) * 0.5;
                float positionCSY = o.positionCS.y;
                worldClip.y = stepPosZ * positionCSY + (1 - stepPosZ) * worldClip.y;
                o.worldScreenPos = ComputeScreenPos(worldClip);
                o.worldScreenPos.z = clamp(high, 0, 1);
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
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
                // return float4(otherStep.xxx,mainTex.a);

                half depthValue = (DepthTex.r - 0.5) * (1 - otherStep) + (DepthTex.r + DepthTex.b - 1) * (1 -
                    stepDepthOne) * otherStep;
                half offset = depthValue * 512 * 4 / _ScreenParams.y;


                half depth = worldScreenPos.y + offset;
                half setpHigh = depthStep_G;

                half high = i.worldScreenPos.z * (1 - setpHigh) + DepthTex.g * 2 * setpHigh;


                mainTex.xyz = half3(depth, high, _NormalColor.g * 0.5 + stepDepthOne);
                // mainTex.y+=ObjDepthTex.y;
                mainTex.z += ObjDepthTex.z;
                mainTex.a = mainTex.a * (1 - stepDepthOne) + DepthTex.a * stepDepthOne;

                // mainTex.xyz=half3(0,0,ObjDepthTex.z); 


                // mainTex.xyz=otherStep.xxx;


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
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment


            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 lightingUV : TEXCOORD1;
                float4 worldPos : TEXCOORD4;
                float4 fixScreenUV: TEXCOORD3;
            };


            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
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
                half4 carmeraPos = TransformWorldToHClip(pos);


                o.fixScreenUV = o.lightingUV - ComputeScreenPos(carmeraPos);

                o.color = v.color * unity_SpriteColor;
                return o;
            }

            float3 WaterFragment(float2 uv, float2 screenUV)
            {
                float2 mirrorUV = screenUV;

                float3 _WaterMask = SAMPLE_TEXTURE2D(_WaterMaskTex, sampler_WaterMaskTex, uv.xy).xyz;
                //水域范围
                float stepMask = step(0.06, _WaterMask.r);

                half edgeOffsetValue = _SinTime.w * _EdgeWaveSpeed;
                edgeOffsetValue = abs(edgeOffsetValue);
                edgeOffsetValue = clamp(edgeOffsetValue, 0, 1);
                _WaterHigh = _WaterHigh + _EdgeWaveOffset * edgeOffsetValue;
                //return _EdgeWaveOffset*edgeOffsetValue;


                float svalue = _ScreenParams.y / 1920;
                svalue = floor(svalue);
                svalue = clamp(svalue, 1, svalue);
                svalue /= 2;
                float2 offsetUv = _WorldSpaceCameraPos.xy * svalue * 800 / _ScreenParams.xy;
                screenUV += offsetUv;

                //波纹1
                float angle0 = radians(_WaveAngle0); //转换角度为弧度
                float2 waveValue0 = float2(cos(angle0), sin(angle0)) * _WaveSpeed0;

                float2 _WaveT0 = (_TimeParameters.x.xx) * waveValue0;
                float2 _TilingAndOffset0 = screenUV * WaveScale0 + _WaveT0;
                float4 _WaveCol0 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, _TilingAndOffset0);
                _WaveCol0.rgb = UnpackNormal(_WaveCol0);
                //波纹2
                float angle1 = radians(_WaveAngle1);
                float2 waveValue1 = float2(cos(angle1), sin(angle1)) * _WaveSpeed1;
                float2 _WaveT2 = (_TimeParameters.x.xx) * waveValue1;
                float2 _TilingAndOffset1 = screenUV * WaveScale1 + _WaveT2;
                float4 _WaveCol1 = SAMPLE_TEXTURE2D(_WaterNormalMap, sampler_WaterNormalMap, _TilingAndOffset1);
                _WaveCol1.rgb = UnpackNormal(_WaveCol1);


                //波纹叠加
                float3 _endWave = _WaveCol0.xyz + _WaveCol1.xyz;
                //波纹r、g叠加
                float waveBlendCol = _endWave[0] + _endWave[1];
                waveBlendCol = clamp(waveBlendCol, 0, 1);
                waveBlendCol *= waterValue;
                //噪声
                float _waterNoise;
                Unity_SimpleNoise_float(screenUV.xy, waterNoiseScale, _waterNoise);
                waveBlendCol *= _waterNoise;


                //波纹与边缘混合 
                //映射水面深度
                Unity_Remap_float(_WaterMask.r, float2(0, 1), float2(_WaterZero, _WaterBottom), _WaterMask.r);
                _WaterMask.r = clamp(_WaterMask.r, 0, 1);


                float _WaterMask1 = step(_WaterHigh, _WaterMask.r);
                float _WaterMask2 = step(_WaterHigh + EdgeValue, _WaterMask.r);
                stepMask *= _WaterMask1;
                return stepMask;
            }

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                float2 lightingUV = i.lightingUV.xy / i.lightingUV.w;
                lightingUV = UnityStereoTransformScreenSpaceTex(lightingUV);
                float3 result = WaterFragment(i.uv, lightingUV);
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

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes v)
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
                half4 mainTex = _MainTex.Sample(sampler_MainTex, i.uv);
                float4 moveValue = _MoveMask.Sample(sampler_MainTex, i.uv);
                moveValue.a = mainTex.a;
                //moveValue.xyz=moveValue.ggg;
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

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes v)
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
                half4 mainTex = _MainTex.Sample(sampler_MainTex, i.uv);
                float4 moveValue = _MoveMask.Sample(sampler_MainTex, i.uv);
                int groundStep = 1 - step(moveValue.z, 0);
                moveValue.a *= mainTex.a;

                int grassStep = 1 - step(moveValue.x + moveValue.y, 0);
                int groundIndex = moveValue.z;

                float outValue = grassStep * (moveValue.z + 0.8) + moveValue.z;

                return float4(outValue.xxx, moveValue.a);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
    CustomEditor "FeatureFlagsGUI"
}