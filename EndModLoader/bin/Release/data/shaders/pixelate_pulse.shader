#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform float palette;
uniform sampler2D framebuf;
uniform vec2 screensize;
uniform float timer;

varying vec2 worldPos;

const float min_pixel_density = 100.0; // the minimum pixel density (smaller the number the larger the pixels)
const float max_pixel_density = 512.0; // the maximum pixel density (larger the number the smaller the pixels)

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
    vec4 outcolor = texture(framebuf, color_coord.xy);

    outcolor.a = gl_Color.a;
    gl_FragColor = outcolor;
}

#endif
