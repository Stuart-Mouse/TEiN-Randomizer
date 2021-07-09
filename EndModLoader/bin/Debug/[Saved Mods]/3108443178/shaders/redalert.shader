#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform sampler2D framebuf;
uniform float palette;
uniform vec2 screensize;
uniform float timer;
uniform float level_param;
uniform vec2 playerPos;
uniform float waterheight;

uniform vec4 light0;
uniform vec4 light1;
uniform vec4 light2;
uniform vec4 light3;
uniform vec4 light4;
uniform vec4 light5;
uniform vec4 light6;
uniform vec4 light7;

varying vec2 worldPos;

const vec4  light_color = vec4(1,1,1,0.5);            // the gradient light that is above (and on top) of the water
const float light_distance = 4;                       // less = farther distance
const float glow_strength = 0.5;                      // how strong the glow effect is
const vec4  pulse_color = vec4(1,0,0,1);              // the color to pulse to
const float pulse_speed = 4.0;                        // the speed at which to pulse (lower = slower)
const float max_color_intensity = 0.75;               // how close to the desired color the pulse will get
const vec2  chromatic_intensity = vec2(0.003, 0.003); // the intensity of the chromatic abberation in the x and y directions
const float shake_speed = 20;                         // the speed at which to shake the screen (lower = slower)
const vec2  shake_intensity = vec2(0.05, 0.05);       // the intensity of the screen shake in the x and y directions

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

float hash(float _x)
{
    return fract(sin(_x) * 43758.5453);
}

float noise(float _u)
{
    vec3 x = vec3(_u, 0, 0);

    vec3 p = floor(x);
    vec3 f = fract(x);

    f = f * f * (3.0 - 2.0 * f);
    float n = p.x + p.y * 57.0 + 113.0 * p.z;

    return mix(mix(mix(hash(n + 0.0), hash(n + 1.0),   f.x),
           mix(hash(n + 57.0), hash(n + 58.0), f.x),   f.y),
           mix(mix(hash(n + 113.0), hash(n + 114.0),   f.x),
           mix(hash(n + 170.0), hash(n + 171.0), f.x), f.y),
           f.z);
}

void frag()
{
    vec4 screencoords = gl_FragCoord;

    screencoords.x /= screensize.x;
    screencoords.y /= screensize.y;

    float scale_x = screensize.x / 1280.0, scale_y = screensize.y / 720.0;
    float scale_min = min(scale_x, scale_y);

    vec2 ratio = vec2(scale_min / scale_x, scale_min / scale_y);

    float shake_time_value = -timer * shake_speed;
    float pulse_time_value = -timer * pulse_speed;

    // shake the screen randomly in various directions
    vec2 screen_offset = vec2(sin_range(0, shake_intensity.x, shake_time_value), sin_range(0, shake_intensity.y, shake_time_value)) * ratio * 0.015;
    vec2 shaken_screencoords = screencoords.xy + screen_offset;

    // pulse between the standard palette color and the color specifed by pulse_color
    float mix_value = sin_range(0, max_color_intensity, pulse_time_value);
    vec4 outcolor = mix(texture(framebuf, shaken_screencoords), pulse_color, mix_value);

    // pulse the chromatic abberation effect in and out
    vec2 pulse = vec2(sin_range(0, chromatic_intensity.x, pulse_time_value), sin_range(0, chromatic_intensity.y, pulse_time_value));

    outcolor.rgb += vec3
    (
        texture(framebuf, shaken_screencoords.xy - vec2( pulse.x,      0.0)).r,
        texture(framebuf, shaken_screencoords.xy - vec2(     0.0, -pulse.y)).g,
        texture(framebuf, shaken_screencoords.xy - vec2(-pulse.x,      0.0)).b,
    );

    // a copy and paste of the default lighting shader
    float d = length(playerPos - worldPos);

    float ambient_light = level_param * level_param;
    float l_p = min(1.0, 60.0 / (d + 50.0));
    float l_0 = min(1.0, light0.z / (length(light0.xy - worldPos) + light0.z * 0.75));
    float l_1 = min(1.0, light1.z / (length(light1.xy - worldPos) + light1.z * 0.75));
    float l_2 = min(1.0, light2.z / (length(light2.xy - worldPos) + light2.z * 0.75));
    float l_3 = min(1.0, light3.z / (length(light3.xy - worldPos) + light3.z * 0.75));
    float l_4 = min(1.0, light4.z / (length(light4.xy - worldPos) + light4.z * 0.75));
    float l_5 = min(1.0, light5.z / (length(light5.xy - worldPos) + light5.z * 0.75));
    float l_6 = min(1.0, light6.z / (length(light6.xy - worldPos) + light6.z * 0.75));
    float l_7 = min(1.0, light7.z / (length(light7.xy - worldPos) + light7.z * 0.75));

    l_p = l_p * l_p;
    l_0 = l_0 * l_0 * 2;
    l_1 = l_1 * l_1 * 2;
    l_2 = l_2 * l_2 * 2;
    l_3 = l_3 * l_3 * 2;
    l_4 = l_4 * l_4 * 2;
    l_5 = l_5 * l_5 * 2;
    l_6 = l_6 * l_6 * 2;
    l_7 = l_7 * l_7 * 2;

    float dy = waterheight - worldPos.y;

    dy += noise(worldPos.x / 32.0 + timer *  2.00) * 6.0;
    dy += noise(worldPos.x / 16.0 + timer * -4.35) * 3.0;
    dy += noise(worldPos.x /  8.0 + timer *  1.00) * 1.5;

    float glowval  = -(dy / 360.0);
          glowval  = glowval * light_distance;
          glowval += 1;
          glowval  = clamp(glowval, 0.0, 1.0);
          glowval  = glowval * glowval * glow_strength;

    float lightval = l_p + l_0 + l_1 + l_2 + l_3 + l_4 + l_5 + l_6 + l_7 + glowval;
    lightval = 1.0 - (1.0 - lightval) * (1.0 - ambient_light);

    outcolor *= min(1.5, lightval);
    outcolor.a = gl_Color.a;

    gl_FragColor = outcolor;
}

#endif
