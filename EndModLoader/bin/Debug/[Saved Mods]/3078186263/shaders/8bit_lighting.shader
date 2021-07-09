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

const vec4  light_color = vec4(1, 1, 1, 0.5);    // r, g, b, a. the gradient light that is above (and on top) of the water
const float light_distance = 4.0;                // less = farther distance
const vec2  light_resolution = vec2(0.25, 0.25); // the resolution of lighting (smaller = larger on-screen pixels)
const float glow_strength = 0.5;                 // the strength of glowing objects
const float resolution = 10.0;                   // the resolution of the selected colors (lower = more limited)
const float saturation = 1.5;                    // how saturated the resulting colors should be
const vec2  mosaic_size = vec2(400.0, 400.0);    // the smaller the number the larger the on-screen pixels

#if COMPILING_VERTEX_PROGRAM

void vert()
{
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r * 15.0 + 0.5) / 16.0, (palette + 0.5) / 64.0)).rgb, outcolor.a);

    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
    worldPos = (gl_ModelViewMatrix * gl_Vertex).xy;
}

#elif COMPILING_FRAGMENT_PROGRAM

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

    vec2 screen_fract_coords = fract(screencoords.xy * mosaic_size) / mosaic_size;
    vec2 world_fract_coords = fract(worldPos * light_resolution) / light_resolution;

    vec4 outcolor = texture(framebuf, screencoords.xy - screen_fract_coords);
    vec2 world_pos = worldPos - world_fract_coords;

    // LIGHTING ////////////////////////////////////////////////////////////////

    float d = length(playerPos - world_pos);

    float ambient_light = level_param * level_param;
    float l_p = min(1.0, 60.0 / (d + 50.0));
    float l_0 = min(1.0, light0.z / (length(light0.xy - world_pos) + light0.z * 0.75));
    float l_1 = min(1.0, light1.z / (length(light1.xy - world_pos) + light1.z * 0.75));
    float l_2 = min(1.0, light2.z / (length(light2.xy - world_pos) + light2.z * 0.75));
    float l_3 = min(1.0, light3.z / (length(light3.xy - world_pos) + light3.z * 0.75));
    float l_4 = min(1.0, light4.z / (length(light4.xy - world_pos) + light4.z * 0.75));
    float l_5 = min(1.0, light5.z / (length(light5.xy - world_pos) + light5.z * 0.75));
    float l_6 = min(1.0, light6.z / (length(light6.xy - world_pos) + light6.z * 0.75));
    float l_7 = min(1.0, light7.z / (length(light7.xy - world_pos) + light7.z * 0.75));

    l_p = l_p * l_p;
    l_0 = l_0 * l_0 * 2;
    l_1 = l_1 * l_1 * 2;
    l_2 = l_2 * l_2 * 2;
    l_3 = l_3 * l_3 * 2;
    l_4 = l_4 * l_4 * 2;
    l_5 = l_5 * l_5 * 2;
    l_6 = l_6 * l_6 * 2;
    l_7 = l_7 * l_7 * 2;

    float dy = waterheight - world_pos.y;

    dy += noise(world_pos.x / 32.0 + timer *  2.00) * 6.0;
    dy += noise(world_pos.x / 16.0 + timer * -4.35) * 3.0;
    dy += noise(world_pos.x /  8.0 + timer *  1.00) * 1.5;

    float glowval = -(dy / 360.0);
    glowval  = glowval * light_distance;
    glowval += 1;
    glowval  = clamp(glowval, 0.0, 1.0);
    glowval  = glowval * glowval * glow_strength;

    float lightval = l_p + l_0 + l_1 + l_2 + l_3 + l_4 + l_5 + l_6 + l_7 + glowval;
    lightval = 1.0 - (1.0 - lightval) * (1.0 - ambient_light);

    outcolor *= min(1.5, lightval);

    ////////////////////////////////////////////////////////////////////////////

    vec3 fract_pixel = outcolor.rgb - fract(outcolor.rgb * resolution) / resolution;

    float luma = dot(fract_pixel, vec3(0.3, 0.59, 0.11));
    vec3 chroma = (fract_pixel - luma) * saturation;

    outcolor.rgb = luma + chroma;
    outcolor.a = 1.0;

    gl_FragColor = outcolor;
}

#endif
