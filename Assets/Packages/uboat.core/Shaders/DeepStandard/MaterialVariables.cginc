#ifndef UNITY_MATERIAL_VARIABLES_INCLUDED
#define UNITY_MATERIAL_VARIABLES_INCLUDED

CBUFFER_START(UnityPerMaterial)
half4       _Color;
half        _Cutoff;

float4      _MainTex_ST;

float4      _DetailAlbedoMap_ST;

half        _BumpScale;

half        _DetailNormalMapScale;

half        _Metallic;
float       _Glossiness;
float       _GlossMapScale;

half        _OcclusionStrength;

half        _Parallax;
half        _UVSec;

half4       _EmissionColor;

half4 _WetnessBiasScale;

half _DissolveMaskScale;

half _UVWetnessMap;

CBUFFER_END

#endif