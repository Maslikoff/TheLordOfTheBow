#ifndef FLAT_KIT_FOG_INCLUDED
#define FLAT_KIT_FOG_INCLUDED

TEXTURE2D_X (_EffectTexture);
SAMPLER (sampler_EffectTexture);

float Linear01Depth(float z)
{
    const float isOrtho = unity_OrthoParams.w;
    const float isPers = 1.0 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}

float LinearEyeDepth(float z)
{
    return rcp(_ZBufferParams.z * z + _ZBufferParams.w);
}

float4 SampleCameraColor(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_EffectTexture, sampler_EffectTexture, UnityStereoTransformScreenSpaceTex(uv));
}

void Fog_float(float2 UV, out float4 Out)
{
    float4 original = SampleCameraColor(UV);
    float fogBlend = _DistanceHeightBlend;
    float depthPacked = SampleSceneDepth(UV);
    const float depthCameraPlanes = Linear01Depth(depthPacked);
    // Skybox / clear depth sits at the far plane.
    const float isSky = smoothstep(0.992, 1.0, depthCameraPlanes);

    float depthPackedWs = depthPacked;
    #if !UNITY_REVERSED_Z
        depthPackedWs = lerp(UNITY_NEAR_CLIP_VALUE, 1, depthPacked);
    #endif
    const float3 worldPos = ComputeWorldSpacePosition(UV, depthPackedWs, UNITY_MATRIX_I_VP);

    // Reconstruct a far-plane ray for elevation. Do not use UNITY_MATRIX_I_P here:
    // fullscreen ShaderGraph passes do not always populate it.
    #if UNITY_REVERSED_Z
        const float farDepth = 0.0;
    #else
        const float farDepth = 1.0;
    #endif
    const float3 farPos = ComputeWorldSpacePosition(UV, farDepth, UNITY_MATRIX_I_VP);
    const float3 viewDir = normalize(farPos - _WorldSpaceCameraPos);

    // Only the true horizon: keep mid-ground trees and sky zenith unfogged.
    const float horizonSky = 1.0 - smoothstep(0.0, 0.18, viewDir.y);
    const float fogMask = lerp(horizonSky, 1.0, 1.0 - isSky);

    #if defined(USE_DISTANCE_FOG)
        const float depthAbsolute = _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * depthCameraPlanes;
        float depthFogPlanes = saturate((depthAbsolute - _Near) / (_Far - _Near));
        // Sky has camera-far depth; always use the far LUT color so the horizon fill is opaque.
        depthFogPlanes = lerp(1.0, depthFogPlanes, 1.0 - isSky);
        float4 distanceFog = SAMPLE_TEXTURE2D_X(_DistanceLUT, sampler_DistanceLUT, float2(depthFogPlanes, 0.5));
        distanceFog.a *= _DistanceFogIntensity * fogMask;
    #else
        float4 distanceFog = float4(0, 0, 0, 0);
        fogBlend = 1.0;
    #endif

    #if defined(USE_HEIGHT_FOG)
        #if defined(FOG_CAMERA_RELATIVE)
            const float3 heightWorldPos = float3(UV, depthPacked) * LinearEyeDepth(depthPacked) + _WorldSpaceCameraPos;
        #else
            const float3 heightWorldPos = worldPos;
        #endif
    
        const float heightUV = saturate((heightWorldPos.y - _LowWorldY) / (_HighWorldY - _LowWorldY));
        float4 heightFog = SAMPLE_TEXTURE2D_X(_HeightLUT, sampler_HeightLUT, float2(heightUV, 0.5));
        heightFog.a *= _HeightFogIntensity * (1.0 - isSky);
    #else
        float4 heightFog = float4(0, 0, 0, 0);
        fogBlend = 0.0;
    #endif

    const float4 fog = lerp(distanceFog, heightFog, fogBlend);
    float4 final = lerp(original, fog, fog.a);
    final.a = original.a;

    Out = final;
}
#endif  // FLAT_KIT_FOG_INCLUDED
