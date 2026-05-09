#ifndef FX_COMMON_INCLUDED
#define FX_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

struct FXAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct FXVaryings
{
    float4 positionCS : SV_POSITION;
    float4 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float3 positionWS : TEXCOORD2;
    half3 normalWS : TEXCOORD3;
    half4 color : COLOR;
    float4 screenPos : TEXCOORD4;
    half fogFactor : TEXCOORD5;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

FXVaryings FXVertex(FXAttributes input)
{
    FXVaryings output = (FXVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.positionCS = positionInputs.positionCS;
    output.positionWS = positionInputs.positionWS;
    output.normalWS = normalize(normalInputs.normalWS);
    output.uv0 = input.uv0;
    output.uv1 = input.uv1;
    output.color = input.color;
    output.screenPos = ComputeScreenPos(positionInputs.positionCS);
    output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
    return output;
}

float FXSoftParticleFade(float4 screenPos, float enabled, float nearFade, float farFade)
{
    if (enabled < 0.5)
        return 1.0;

    float2 screenUV = GetNormalizedScreenSpaceUV(screenPos);
    float sceneRawDepth = SampleSceneDepth(screenUV);
    float sceneEyeDepth = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
    float particleEyeDepth = LinearEyeDepth(screenPos.z / max(screenPos.w, 1e-5), _ZBufferParams);
    float distanceToScene = max(sceneEyeDepth - particleEyeDepth, 0.0);
    float fadeRange = max(farFade - nearFade, 1e-4);
    return saturate((distanceToScene - nearFade) / fadeRange);
}

float2 FXScroll(float2 uv, float2 speed)
{
    return uv + speed * _Time.y;
}

float FXCenterMask(float2 uv, float power)
{
    float2 centered = uv * 2.0 - 1.0;
    float radial = saturate(1.0 - length(centered));
    return pow(radial, max(power, 1e-4));
}

float FXRingMask(float2 uv, float innerRadius, float outerRadius, float softness)
{
    float2 centered = uv * 2.0 - 1.0;
    float radial = length(centered);
    float outer = 1.0 - smoothstep(max(outerRadius - softness, 0.0), outerRadius, radial);
    float inner = smoothstep(innerRadius, innerRadius + softness, radial);
    return saturate(outer * inner);
}

float FXLineMask(float2 uv, float width, float softness)
{
    float distanceToCenter = abs(uv.y - 0.5) * 2.0;
    return 1.0 - smoothstep(width, width + max(softness, 1e-4), distanceToCenter);
}

float FXGradient(float u, float rangeValue, float power)
{
    return saturate(pow(saturate(u * max(rangeValue, 1e-4)), max(power, 1e-4)));
}

float FXViewFresnel(float3 normalWS, float3 positionWS, float power)
{
    float3 viewDirWS = SafeNormalize(GetWorldSpaceViewDir(positionWS));
    float ndv = saturate(abs(dot(SafeNormalize(normalWS), viewDirWS)));
    return pow(1.0 - ndv, max(power, 1e-4));
}

half3 FXApplyFog(half3 color, half fogFactor)
{
    return MixFog(color, fogFactor);
}

#endif
