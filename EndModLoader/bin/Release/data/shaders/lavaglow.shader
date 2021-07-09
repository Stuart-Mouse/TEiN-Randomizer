#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform sampler2D framebuf;
uniform float palette;
uniform vec2 screensize;
uniform float timer;
uniform float waterheight;
uniform float playerglow_strength;
uniform vec2 playerPos;

varying vec2 worldPos;
varying vec2 blurcoords[25];
varying vec2 screencoords;
varying vec2 ratio;

const vec4  light_color = vec4(1.0, 0.33, 0.0, 0.8); // the gradient light that is above (and on top) of the water
const float light_distance = 1.5;                    // less = farther distance
const vec4  glow_color = vec4(1.0, 0.80, 0.0, 0.0);  // the color that white parts glow when near water (alpha is unused here)
const float glow_strength = 24;                      // how strong the glow effect is
const float glow_threshold = 0.00;                   // how bright the green channel needs to be to trigger a glow
const float glow_distance = 0.0015;                  // size of the glow
const float overexpose = 0.25;                       // colors more than full bright bleed into other channels to make stuff whiter


#if COMPILING_VERTEX_PROGRAM

void vert()
{
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r * 15.0 + 0.5) / 16.0, (palette + 0.5) / 64.0)).rgb, outcolor.a);

    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
    worldPos = (gl_ModelViewMatrix * gl_Vertex).xy;

    screencoords = (gl_Position.xy + vec2(1, 1)) * 0.5;

    float scale_x = screensize.x / 1280.0, scale_y = screensize.y / 720.0;
    float scale_min = min(scale_x, scale_y);

    ratio    = vec2(scale_min / scale_x, scale_min / scale_y);
    ratio.y *= (1280.0 / 720.0);

     blurcoords[0] = screencoords.xy + vec2(-2,-2) * ratio * glow_distance;
     blurcoords[1] = screencoords.xy + vec2(-1,-2) * ratio * glow_distance;
     blurcoords[2] = screencoords.xy + vec2( 0,-2) * ratio * glow_distance;
     blurcoords[3] = screencoords.xy + vec2( 1,-2) * ratio * glow_distance;
     blurcoords[4] = screencoords.xy + vec2( 2,-2) * ratio * glow_distance;
     blurcoords[5] = screencoords.xy + vec2(-2,-1) * ratio * glow_distance;
     blurcoords[6] = screencoords.xy + vec2(-1,-1) * ratio * glow_distance;
     blurcoords[7] = screencoords.xy + vec2( 0,-1) * ratio * glow_distance;
     blurcoords[8] = screencoords.xy + vec2( 1,-1) * ratio * glow_distance;
     blurcoords[9] = screencoords.xy + vec2( 2,-1) * ratio * glow_distance;
    blurcoords[10] = screencoords.xy + vec2(-2, 0) * ratio * glow_distance;
    blurcoords[11] = screencoords.xy + vec2(-1, 0) * ratio * glow_distance;
    blurcoords[12] = screencoords.xy + vec2( 0, 0) * ratio * glow_distance;
    blurcoords[13] = screencoords.xy + vec2( 1, 0) * ratio * glow_distance;
    blurcoords[14] = screencoords.xy + vec2( 2, 0) * ratio * glow_distance;
    blurcoords[15] = screencoords.xy + vec2(-2, 1) * ratio * glow_distance;
    blurcoords[16] = screencoords.xy + vec2(-1, 1) * ratio * glow_distance;
    blurcoords[17] = screencoords.xy + vec2( 0, 1) * ratio * glow_distance;
    blurcoords[18] = screencoords.xy + vec2( 1, 1) * ratio * glow_distance;
    blurcoords[19] = screencoords.xy + vec2( 2, 1) * ratio * glow_distance;
    blurcoords[20] = screencoords.xy + vec2(-2, 2) * ratio * glow_distance;
    blurcoords[21] = screencoords.xy + vec2(-1, 2) * ratio * glow_distance;
    blurcoords[22] = screencoords.xy + vec2( 0, 2) * ratio * glow_distance;
    blurcoords[23] = screencoords.xy + vec2( 1, 2) * ratio * glow_distance;
    blurcoords[24] = screencoords.xy + vec2( 2, 2) * ratio * glow_distance;
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

vec4 gblend(vec4 _a, vec4 _b)
{
    return 1.0 - (1.0 - _a) * (1.0 - _b);
}

float overscreen(float _a, float _b)
{
    return 1.0 - (1.0 - clamp(_a, 0.0, 1.0)) * (1.0 - clamp(_b, 0.0, 1.0)) + max(0.0, _a - 1.0) + max(0.0, _b - 1.0);
}

void frag()
{
    float blurcolor = 0;

    blurcolor += max(0, texture(framebuf,  blurcoords[1]).g-glow_threshold) * 4.0  / 252.0;
    blurcolor += max(0, texture(framebuf,  blurcoords[2]).g-glow_threshold) * 6.0  / 252.0;
    blurcolor += max(0, texture(framebuf,  blurcoords[3]).g-glow_threshold) * 4.0  / 252.0;
    blurcolor += max(0, texture(framebuf,  blurcoords[5]).g-glow_threshold) * 4.0  / 252.0;
    blurcolor += max(0, texture(framebuf,  blurcoords[6]).g-glow_threshold) * 16.0 / 252.0;
    blurcolor += max(0, texture(framebuf,  blurcoords[7]).g-glow_threshold) * 24.0 / 252.0;
    blurcolor += max(0, texture(framebuf,  blurcoords[8]).g-glow_threshold) * 16.0 / 252.0;
    blurcolor += max(0, texture(framebuf,  blurcoords[9]).g-glow_threshold) * 4.0  / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[10]).g-glow_threshold) * 6.0  / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[11]).g-glow_threshold) * 24.0 / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[12]).g-glow_threshold) * 36.0 / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[13]).g-glow_threshold) * 24.0 / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[14]).g-glow_threshold) * 6.0  / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[15]).g-glow_threshold) * 4.0  / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[16]).g-glow_threshold) * 16.0 / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[17]).g-glow_threshold) * 24.0 / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[18]).g-glow_threshold) * 16.0 / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[19]).g-glow_threshold) * 4.0  / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[21]).g-glow_threshold) * 4.0  / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[22]).g-glow_threshold) * 6.0  / 252.0;
    blurcolor += max(0, texture(framebuf, blurcoords[23]).g-glow_threshold) * 4.0  / 252.0;

    vec4 basecolor = texture(framebuf, screencoords.xy);

    float dy = waterheight - worldPos.y;

    dy += noise(worldPos.x / 32.0 + timer *  2.00) * 6.0;
    dy += noise(worldPos.x / 16.0 + timer * -4.35) * 3.0;
    dy += noise(worldPos.x /  8.0 + timer *  1.00) * 1.5;

    float glowval  = -(dy / 360.0);
          glowval  = glowval * light_distance;
          glowval += 1;
          glowval  = clamp(glowval, 0.0, 1.0);
          glowval  = glowval * glowval;

    vec2 playervec = playerPos - worldPos;
    float dp = length(playervec);

    float angle = dot(vec2(0, -1), playervec) / (dp) * 100;
    float angle2 = dp;

    dp += noise(dp / 32.0 + timer *  2.00) * 6.0;
    dp += noise(dp / 16.0 + timer * -4.35) * 3.0;
    dp += noise(dp /  8.0 + timer *  1.00) * 1.5;

    dp += noise(angle / 32.0 + timer *  2.00) * 6.0;
    dp += noise(angle / 16.0 + timer * -4.35) * 3.0;
    dp += noise(angle /  8.0 + timer *  1.00) * 1.5;

    dp = -(dp / 360.0);
    float glowval2  = dp * light_distance / playerglow_strength;
          glowval2 += 1;
          glowval2  = clamp(glowval2, 0.0, 1.0);
          glowval2  = glowval2 * glowval2 * playerglow_strength;

    float dx = worldPos.x;

    dx += noise(worldPos.y / 32.0 + timer *  2.00) * 6.0;
    dx += noise(worldPos.y / 16.0 + timer * -4.35) * 3.0;
    dx += noise(worldPos.y /  8.0 + timer *  1.00) * 1.5;

    float glowval3  = -(dx / 360.0);
          glowval3  = glowval3 * light_distance / (playerglow_strength * 4);
          glowval3 += 1;
          glowval3  = clamp(glowval3, 0.0, 1.0);
          glowval3  = glowval3 * glowval3 * playerglow_strength * 1.5;

    glowval = overscreen(glowval, glowval3);
    glowval = overscreen(glowval, glowval2);

    vec4 outcolor = basecolor;
    outcolor = gblend(outcolor, mix(vec4(0.0), light_color * light_color.a, glowval));

    vec4 glowcolor = (glow_color * glowval * glow_strength) * blurcolor;
    outcolor = outcolor + glowcolor;

    float extra_green = max(outcolor.g - 1.0, 0.0);
    outcolor.r += extra_green * overexpose;
    outcolor.b += extra_green * overexpose;

    float extra_red = max(outcolor.r - 1.0, 0.0);
    outcolor.g += extra_red * overexpose;
    outcolor.b += extra_red * overexpose;

    float extra_blue = max(outcolor.b - 1.0, 0.0);
    outcolor.r += extra_blue * overexpose;
    outcolor.g += extra_blue * overexpose;

    outcolor.a = gl_Color.a;
    gl_FragColor = outcolor;
}

#endif
