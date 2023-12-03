// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef UNITYACTION_CG_INCLUDED
    #define UNITYACTION_CG_INCLUDED

    // Built-in renderer (CG) to SRP (HLSL) bindings
    #define UnityObjectToClipPos TransformObjectToHClip
    #define _WorldSpaceLightPos0 _MainLightPosition
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
 

    inline float2 GetScreenUV(float2 clipPos)
    {
        float4x4 mvpMatrix = mul(unity_MatrixVP, unity_ObjectToWorld);
        float4 screenSpaceObjPos = float4(mvpMatrix[0][3],mvpMatrix[1][3],mvpMatrix[2][3],mvpMatrix[3][3]);
        float2 screenUV = clipPos.xy;
        screenUV.xy -= screenSpaceObjPos.xy / screenSpaceObjPos.ww;
        float ratio = _ScreenParams.x/_ScreenParams.y;
        screenUV.x *= ratio;
        screenUV *= screenSpaceObjPos.w;
        screenUV.x *= sign(UNITY_MATRIX_P[1].y); // 1 for Game View, -1 for Scene View
        return screenUV / UNITY_MATRIX_P._m11; // scale with the Camera FoV
    }

    float2 Unity_Flipbook_InvertY_float (float2 UV, float Width, float Height, float Tile, float2 Invert)
    {
        Tile = floor(fmod(Tile + 0.000001, Width*Height));
        float2 tileCount = float2(1.0, 1.0) / float2(Width, Height);
        float tileX = (Tile - Width * floor(Tile * tileCount.x));
        float tileY = (Invert.y * Height - (floor(Tile * tileCount.x) + Invert.y * 1));
        return (UV + float2(tileX, tileY)) * tileCount;
    }
    float2 Posterize_float2(float2 In, float2 Steps)
    {
        return floor(In / (1 / Steps)) * (1 / Steps);
    }

    float2 Unity_GradientNoise_Dir_float(float2 p)
    {
        // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
        p = p % 289;
        // need full precision, otherwise half overflows when p > 1
        float x = float(34 * p.x + 1) * p.x % 289 + p.y;
        x = (34 * x + 1) * x % 289;
        x = frac(x / 41) * 2 - 1;
        return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
    }
    float Unity_GradientNoise_float(float2 UV, float Scale)
    { 
        float2 p = UV * Scale;
        float2 ip = floor(p);
        float2 fp = frac(p);
        float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
        float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
        float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
        float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
        fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
        return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        
    }
    void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
    {
        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
    }
    void Unity_Remap_float2(float2 In, float2 InMinMax, float2 OutMinMax, out float2 Out)
    {
        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
    }
    void Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax, out float4 Out)
    {
        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
    }
    void Unity_Remap_float3(float3 In, float2 InMinMax, float2 OutMinMax, out float3 Out)
    {
        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
    }

    
    float3 Unity_Rotate_About_Axis_Radians_float(float3 In, float3 Axis, float Rotation)
    {
        float s = sin(Rotation);
        float c = cos(Rotation);
        float one_minus_c = 1.0 - c;

        Axis = normalize(Axis);

        float3x3 rot_mat = { one_minus_c * Axis.x * Axis.x + c,            one_minus_c * Axis.x * Axis.y - Axis.z * s,     one_minus_c * Axis.z * Axis.x + Axis.y * s,
            one_minus_c * Axis.x * Axis.y + Axis.z * s,   one_minus_c * Axis.y * Axis.y + c,              one_minus_c * Axis.y * Axis.z - Axis.x * s,
            one_minus_c * Axis.z * Axis.x - Axis.y * s,   one_minus_c * Axis.y * Axis.z + Axis.x * s,     one_minus_c * Axis.z * Axis.z + c
        };

        return mul(rot_mat,  In);
    }


    float2 Unity_Rotate_Radians_float(float2 UV, float2 Center, float Rotation)
    {
        //rotation matrix
        UV -= Center;
        float s = sin(Rotation);
        float c = cos(Rotation);
        
        //center rotation matrix
        float2x2 rMatrix = float2x2(c, -s, s, c);
        rMatrix *= 0.5;
        rMatrix += 0.5;
        rMatrix = rMatrix*2 - 1;
        
        //multiply the UVs by the rotation matrix
        UV.xy = mul(UV.xy, rMatrix);
        UV += Center;
        
        return UV;
    }

    float4 NoiseSineWave_float4(float4 In, float2 MinMax)
    {
        float sinIn = sin(In.x);
        float sinInOffset = sin(In.x + 1.0);
        float randomno =  frac(sin((sinIn - sinInOffset) * (12.9898 + 78.233))*43758.5453);
        float noise = lerp(MinMax.x, MinMax.y, randomno);
        return sinIn + noise;
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
    

    float3 StandardColorRevise(float3 baseColor,float3 standardColor,float colorThresHold){

        float3 color = baseColor-standardColor;
        float thresHold0 = pow(color.r*color.r+color.g*color.g+color.b*color.b,0.5);
        //float thresHold0=abs(baseColor.r-standardColor.r)+abs(baseColor.g-standardColor.g)+abs(baseColor.b-standardColor.b);
        thresHold0=step(colorThresHold,thresHold0);

        return  1-thresHold0;
    }
 
 
#endif