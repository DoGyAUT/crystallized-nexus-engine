#version {VERSION}
#ifdef GL_ES
precision mediump float;
#endif

uniform sampler2D SourceTexture;
uniform vec2 WorldScroll;
uniform float Time;
uniform float Intensity;

uniform vec3 ClearTint;
uniform float ClearAlpha;
uniform float ClearShimmer;
uniform float ClearDistortion;
uniform float ClearWaveScale;
uniform float ClearWaveSpeed;

uniform vec3 StormTint;
uniform float StormAlpha;
uniform float StormShimmer;
uniform float StormDistortion;
uniform float StormPulse;
uniform float StormWaveScale;
uniform float StormWaveSpeed;

in vec2 vTexCoord;
out vec4 fragColor;

void main()
{
	// Always write fragColor first so all control-flow paths satisfy strict drivers.
	vec4 src = texelFetch(SourceTexture, ivec2(gl_FragCoord.xy), 0);
	fragColor = src;

	// UV sentinel (2, 2): cliff-block passthrough - return with the pixel we already wrote.
	if (vTexCoord.x > 1.5)
		return;

	float diamond = abs(vTexCoord.x) + abs(vTexCoord.y);
	if (diamond > 1.0)
		discard;

	// World-space coords anchor the pattern to the map so scrolling doesn't shift the waves.
	// Independent phases per axis break the diagonal band pattern.
	vec2 world = gl_FragCoord.xy + WorldScroll;
	float cx = world.x * ClearWaveScale;
	float cy = world.y * ClearWaveScale;
	float ct = Time * ClearWaveSpeed;
	float clearPhaseX = ct * 1.30 + cx * 1.00 + cy * 0.47;
	float clearPhaseY = ct * 0.91 + cx * 0.63 + cy * 1.11 + 1.71;
	vec2 clearWave = vec2(
		sin(clearPhaseX)        + 0.38 * sin(clearPhaseX * 1.87 + 2.34) + 0.17 * sin(clearPhaseX * 2.71 + 0.91),
		0.32 * cos(clearPhaseY) + 0.22 * cos(clearPhaseY * 1.41 + 3.67) + 0.12 * cos(clearPhaseY * 2.13 + 1.23));

	float sx = world.x * StormWaveScale;
	float sy = world.y * StormWaveScale;
	float st = Time * StormWaveSpeed;
	float stormPhaseX = st * 1.20 + sx * 1.00 + sy * 0.53 + 0.80;
	float stormPhaseY = st * 0.83 + sx * 0.71 + sy * 1.17 + 2.50;
	vec2 stormWave = vec2(
		sin(stormPhaseX)        + 0.40 * sin(stormPhaseX * 2.09 + 1.31) + 0.22 * sin(stormPhaseX * 0.73 + 3.72),
		0.43 * cos(stormPhaseY) + 0.21 * cos(stormPhaseY * 1.53 + 0.83) + 0.15 * cos(stormPhaseY * 2.67));

	vec2 wave = mix(clearWave, stormWave, Intensity);
	float distortion = mix(ClearDistortion, StormDistortion, Intensity);

	vec4 base = texelFetch(SourceTexture, ivec2(gl_FragCoord.xy + wave * distortion), 0);

	// Clear shimmer + tint
	float clearShimmer = ClearShimmer * (0.5 + 0.5 * sin(clearPhaseX * 2.3 + clearPhaseY * 1.1));
	vec3 clearOverlay = base.rgb * ClearTint + clearShimmer;

	// Storm ionized pulse + shimmer
	float pulse = StormPulse * (0.5 + 0.5 * sin(stormPhaseX * 3.1 + 1.4)) * (0.5 + 0.5 * sin(stormPhaseY * 1.7));
	float stormShimmer = StormShimmer * (0.5 + 0.5 * sin(stormPhaseX * 2.9 + stormPhaseY * 0.7));
	vec3 stormOverlay = base.rgb * StormTint + pulse + stormShimmer;

	vec3 overlay = mix(clearOverlay, stormOverlay, Intensity);
	float alpha = mix(ClearAlpha, StormAlpha, Intensity);

	fragColor = vec4(mix(src.rgb, overlay, alpha), src.a);
}
