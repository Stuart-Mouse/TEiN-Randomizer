#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform float palette;
uniform sampler2D framebuf;
uniform vec2 screensize;

const vec2 intensity = vec2(0.005, 0.005); // change intensity of the chromatic abberation in the x and y directions

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

    gl_FragColor = vec4
    (
        texture(framebuf, screencoords.xy - vec2( intensity.x,          0.0)).r,
        texture(framebuf, screencoords.xy - vec2(         0.0, -intensity.y)).g,
        texture(framebuf, screencoords.xy - vec2(-intensity.x,          0.0)).b,
        gl_Color.a
    );
}

#endif
