#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform float palette;
uniform sampler2D framebuf;
uniform vec2 screensize;
uniform float timer;
uniform vec2 playerPos;

varying vec2 worldPos;

const float min_pixel_density = 100.0; // the minimum pixel density (smaller the number the larger the pixels)
const float max_pixel_density = 512.0; // the maximum pixel density (larger the number the smaller the pixels)

const vec2 chromatic_intensity = vec2(0.005, 0.005); // change intensity of the chromatic abberation in the x and y directions

#if COMPILING_VERTEX_PROGRAM

void vert()
{
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    vec2 palette_size = vec2(textureSize(palettetex, 0).xy);

    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r * 15.0 + 0.5) / palette_size.x, (palette + 0.5) / palette_size.y)).rgb, outcolor.a);
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}

#elif COMPILING_FRAGMENT_PROGRAM

float sin_range(float min, float max, float t)
{
	float half_range = (max - min) / 2;
	return min + half_range + sin(t) * half_range;
}

float rand(vec2 coord)
{
    return fract(sin(dot(coord.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

void frag()
{
    vec4 screencoords = gl_FragCoord;

    screencoords.x /= screensize.x;
    screencoords.y /= screensize.y;

    float scale_x = screensize.x / 1280.0;
    float scale_y = screensize.y / 720.0;

    float scale_min = min(scale_x, scale_y);
    vec2 ratio = vec2(scale_min / scale_x, scale_min / scale_y);

    float time_value = -timer * 8 + worldPos.y / 15.0;
    float pixelate_value = sin_range(min_pixel_density, max_pixel_density, time_value);

    float dx = scale_x * (1.0 / pixelate_value);
    float dy = scale_y * (1.0 / pixelate_value);

    vec2 color_coord = vec2(dx * floor(screencoords.x / dx), dy * floor(screencoords.y / dy));

    vec2 pulse = vec2(sin_range(chromatic_intensity.x, 0.0, time_value), sin_range(chromatic_intensity.y, 0.0, time_value));

    vec2 r_offset = vec2( pulse.x,      0.0);
    vec2 g_offset = vec2(     0.0, -pulse.y);
    vec2 b_offset = vec2(-pulse.x,      0.0);

    vec4 outcolor;

    outcolor.r = mix(texture(framebuf, color_coord.xy - r_offset).r, rand(color_coord.xy) * 0.1, 0.5);
    outcolor.g = mix(texture(framebuf, color_coord.xy - g_offset).g, rand(color_coord.yx) * 0.1, 0.5);
    outcolor.b = mix(texture(framebuf, color_coord.xy - b_offset).b, rand(color_coord.xx) * 0.1, 0.5);

    outcolor.a = gl_Color.a;
    gl_FragColor = outcolor;
}

#endif
