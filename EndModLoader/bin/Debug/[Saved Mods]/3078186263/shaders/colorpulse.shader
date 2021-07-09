#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform sampler2D framebuf;
uniform float palette;
uniform vec2 screensize;
uniform float timer;

varying vec2 worldPos;

const vec4  pulse_color = vec4(1,0,0,1); // the color to pulse to
const float pulse_speed = 4.0;           // the speed at which to pulse (lower = slower)
const float max_color_intensity = 0.75;  // how close to the desired color the pulse will get

#if COMPILING_VERTEX_PROGRAM

void vert()
{
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r * 15.0 + 0.5) / 16.0, (palette + 0.5) / 64.0)).rgb, outcolor.a);

    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
    worldPos = (gl_ModelViewMatrix * gl_Vertex).xy;
}

#elif COMPILING_FRAGMENT_PROGRAM

float sin_range(float _min, float _max, float _t)
{
    float half_range = (_max - _min) / 2;
    return _min + half_range + sin(_t) * half_range;
}

void frag()
{
    vec4 screencoords = gl_FragCoord;

    screencoords.x /= screensize.x;
    screencoords.y /= screensize.y;

    float time_value = -timer * pulse_speed;
    float mix_value = sin_range(0, max_color_intensity, time_value);

    gl_FragColor = mix(texture(framebuf, screencoords.xy), pulse_color, mix_value);
}

#endif
