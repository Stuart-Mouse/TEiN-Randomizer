#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform float palette;
uniform sampler2D framebuf;
uniform vec2 screensize;

const float pixel_density = 256.0; // the pixel density (smaller the number the larger the pixels)

#if COMPILING_VERTEX_PROGRAM

void vert()
{
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    vec2 palette_size = vec2(textureSize(palettetex, 0).xy);

    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r * 15.0 + 0.5) / palette_size.x, (palette + 0.5) / palette_size.y)).rgb, outcolor.a);
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}

#elif COMPILING_FRAGMENT_PROGRAM

void frag()
{
    vec4 screencoords = gl_FragCoord;

    screencoords.x /= screensize.x;
    screencoords.y /= screensize.y;

    float scale_x = screensize.x / 1280.0;
    float scale_y = screensize.y / 720.0;

    float dx = scale_x * (1.0 / pixel_density);
    float dy = scale_y * (1.0 / pixel_density);

    vec2 color_coord = vec2(dx * floor(screencoords.x / dx), dy * floor(screencoords.y / dy));
    vec4 outcolor = texture(framebuf, color_coord.xy);

    outcolor.a = gl_Color.a;
    gl_FragColor = outcolor;
}

#endif
