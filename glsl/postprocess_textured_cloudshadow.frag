#version {VERSION}
#ifdef GL_ES
precision mediump float;
#endif

uniform sampler2D SourceTexture;
uniform vec2 WorldScroll;
uniform float Time;
uniform float Intensity;

uniform float CloudScale;
uniform vec2 WindVelocity;
uniform float ShadowAlpha;
uniform float CloudCoverage;
uniform float CloudEdge;

uniform float StormShadowAlpha;
uniform float StormCloudCoverage;
uniform float StormWindSpeed;

in vec2 vTexCoord;
out vec4 fragColor;

// Multi-octave trig noise — independent X/Y phases break diagonal banding.
float cloudNoise(vec2 p)
{
	float n = 0.0;

	float a1 = p.x * 1.00 + p.y * 0.62;
	float a2 = p.x * 0.71 + p.y * 1.00 + 1.83;
	n += 0.50 * (sin(a1) * 0.5 + 0.5) * (sin(a2) * 0.5 + 0.5);

	float b1 = p.x * 2.31 + p.y * 1.73 + 3.14;
	float b2 = p.x * 1.87 + p.y * 2.43 + 1.57;
	n += 0.30 * (sin(b1) * 0.5 + 0.5) * (sin(b2) * 0.5 + 0.5);

	float c1 = p.x * 4.11 + p.y * 3.17 + 2.18;
	float c2 = p.x * 3.73 + p.y * 4.33 + 4.37;
	n += 0.20 * (sin(c1) * 0.5 + 0.5) * (cos(c2) * 0.5 + 0.5);

	return n;
}

void main()
{
	vec4 base = texelFetch(SourceTexture, ivec2(gl_FragCoord.xy), 0);

	vec2 world = (gl_FragCoord.xy + WorldScroll) * CloudScale;

	float clearCloud = cloudNoise(world + Time * WindVelocity);
	float clearShadow = smoothstep(CloudCoverage, CloudCoverage + CloudEdge, clearCloud) * ShadowAlpha;

	vec2 stormWorld = world * 1.3 + Time * WindVelocity * (1.0 + StormWindSpeed * 3.0);
	float stormCloud = cloudNoise(stormWorld);
	float stormShadow = smoothstep(StormCloudCoverage, StormCloudCoverage + CloudEdge, stormCloud) * StormShadowAlpha;

	float shadow = mix(clearShadow, stormShadow, Intensity);

	fragColor = vec4(base.rgb * (1.0 - shadow), base.a);
}
