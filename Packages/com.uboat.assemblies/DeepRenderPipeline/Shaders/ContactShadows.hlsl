RW_TEXTURE2D(uint, _ContactShadowTextureUAV);
//RWTexture2D<uint> _ContactShadowTextureUAV;

CBUFFER_START(DeferredShadowParameters)
float4  _ContactShadowParamsParameters;
float4  _ContactShadowParamsParameters2;
int     _SampleCount;
CBUFFER_END

#define _ContactShadowLength                _ContactShadowParamsParameters.x
#define _ContactShadowDistanceScaleFactor   _ContactShadowParamsParameters.y
#define _ContactShadowFadeEnd               _ContactShadowParamsParameters.z
#define _ContactShadowFadeOneOverRange      _ContactShadowParamsParameters.w
#define _RenderTargetHeight                 _ContactShadowParamsParameters2.x
