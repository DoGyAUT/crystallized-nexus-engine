#version {VERSION}
#ifdef GL_ES
precision mediump float;
#endif

uniform sampler2D SourceTexture;
uniform vec2 ReflectionOffset;
uniform vec3 Tint;
uniform float Alpha;
uniform float Distortion;
uniform float WaveScale;
uniform float WaveSpeed;
uniform float Shimmer;
uniform float Time;

in vec2 vTexCoord;
out vec4 fragColor;

void main()
{
	// UV sentinel (2, 2): cliff-block passthrough — restore original terrain pixel unchanged.
	if (vTexCoord.x > 1.5)
	{
		fragColor = texelFetch(SourceTexture, ivec2(gl_FragCoord.xy), 0);
		return;
	}

	float diamond = abs(vTexCoord.x) + abs(vTexCoord.y);
	if (diamond > 1.0)
		discard;

	float edge = 1.0;
	float phase = Time * WaveSpeed + gl_FragCoord.x * WaveScale + gl_FragCoord.y * WaveScale * 0.63;
	vec2 wave = vec2(
		sin(phase) + 0.5 * sin(phase * 1.73 + 2.1),
		0.35 * cos(phase * 1.37));

	vec2 samplePos = gl_FragCoord.xy + ReflectionOffset + wave * Distortion;
	vec4 base = texelFetch(SourceTexture, ivec2(gl_FragCoord.xy), 0);
	vec4 reflected = texelFetch(SourceTexture, ivec2(samplePos), 0);

	float shimmer = Shimmer * edge * (0.5 + 0.5 * sin(phase * 2.3));
	float blend = Alpha * edge;
	vec3 reflectionColor = reflected.rgb * Tint + shimmer;
	fragColor = vec4(mix(base.rgb, reflectionColor, blend), base.a);
}
