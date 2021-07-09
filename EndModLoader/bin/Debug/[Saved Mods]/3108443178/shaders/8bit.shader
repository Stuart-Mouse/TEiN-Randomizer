#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform sampler2D framebuf;
uniform float palette;
uniform vec2 screensize;

const float resolution = 10.0;                // the resolution of the selected colors (lower = more limited)
const float saturation = 1.5;                 // how saturated the resulting colors should be
const vec2  mosaic_size = vec2(400.0, 400.0); // the smaller the number the larger the on-screen pixels

#if COMPILING_VERTEX_PROGRAM

void vert()
{
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r * 15.0 + 0.5) / 16.0, (palette + 0.5) / 64.0)).rgb, outcolor.a);

    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}

#elif COMPILING_FRAGMENT_PROGRAM

void frag()
{
    vec4 screencoords = gl_FragCoord;

    screencoords.x /= screensize.x;
    screencoords.y /= screensize.y;

    vec2 screen_fract_coords = fract(screencoords.xy * mosaic_size) / mosaic_size;
    vec4 outcolor = texture(framebuf, screencoords.xy - screen_fract_coords);

    vec3 fract_pixel = outcolor.rgb - fract(outcolor.rgb * resolution) / resolution;

    float luma = dot(fract_pixel, vec3(0.3, 0.59, 0.11));
    vec3 chroma = (fract_pixel - luma) * saturation;

    outcolor.rgb = luma + chroma;
    outcolor.a = 1.0;

    gl_FragColor = outcolor;
}

#endif
