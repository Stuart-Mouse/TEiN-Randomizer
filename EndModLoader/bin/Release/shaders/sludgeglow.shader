#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
ivec2 ipalettesize;
vec2 palettesize;
uniform sampler2D framebuf;
uniform float palette;

uniform vec2 screensize;
uniform float timer;
uniform float waterheight;
uniform float playerglow_strength;
uniform vec2 playerPos;

varying vec2 worldPos;

varying vec2 screencoords;
varying vec2 ratio;

const vec4 light_color = vec4(.88, 1.0, .8, .5); //r, g, b, a. the gradient light that is above (and on top) of the water
const vec4 glow_color = vec4(.88, 1.0, .8, .5); //alpha is unused here. the color that white parts glow when near water
const float glow_strength = 12;                 //how strong the glow effect is
const float glow_threshold = .6;               //how bright the green channel needs to be to trigger a glow
const float glow_distance = .0015;             //size of the glow
const float overexpose = .25;                  //colors more than full bright bleed into other channels to make stuff whiter
const float light_distance = 2;                //less = farther distance

#if COMPILING_VERTEX_PROGRAM

void vert(){
    //gl_FrontColor = gl_Color * color_xform[0] + color_xform[1];
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    ipalettesize = textureSize(palettetex, 0);
    palettesize = vec2(ipalettesize.x, ipalettesize.y);
    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r*15.0 + .5) / palettesize.x, (palette + .5) / palettesize.y)).rgb, outcolor.a);
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
    worldPos = (gl_ModelViewMatrix * gl_Vertex).xy;

    screencoords = (gl_Position.xy + vec2(1, 1))*.5;

    float scale_x = screensize.x / 1280.0;
    float scale_y = screensize.y / 720.0;
    float scale_min = min(scale_x, scale_y);
    ratio = vec2(scale_min / scale_x, scale_min / scale_y);
    ratio.y *= (1280.0 / 720.0);
}

#elif COMPILING_FRAGMENT_PROGRAM

float hash(float x){
    return fract(sin(x)*43758.5453);
}
float noise(float u){
    vec3 x = vec3(u, 0, 0);

    vec3 p = floor(x);
    vec3 f = fract(x);

    f = f*f*(3.0 - 2.0*f);
    float n = p.x + p.y*57.0 + 113.0*p.z;

    return mix(mix(mix(hash(n + 0.0), hash(n + 1.0), f.x),
        mix(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
        mix(mix(hash(n + 113.0), hash(n + 114.0), f.x),
            mix(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
}

vec4 gblend(vec4 a, vec4 b){
    return 1.0 - (1.0 - a)*(1.0 - b);
}

float overscreen(float a, float b){
    return 1.0 - (1.0 - clamp(a, 0.0, 1.0))*(1.0 - clamp(b, 0.0, 1.0)) + max(0.0, a - 1.0) + max(0.0, b - 1.0);
}

void frag(){

    vec4 basecolor = texture(framebuf, screencoords.xy);

    //reflections
    float dy = waterheight - worldPos.y;

    dy += noise(worldPos.x / 32.0 + timer * 2) * 6;
    dy += noise(worldPos.x / 16.0 + timer  * -4.35) * 3;
    dy += noise(worldPos.x / 8.0 + timer * 1)*1.5;

    float glowval = -(dy / 360.0/* * ratio.y*/);
    glowval = glowval*light_distance;
    glowval += 1;
    glowval = clamp(glowval, 0.0, 1.0);
    glowval = glowval*glowval;

    ////////
    vec2 playervec = playerPos - worldPos;
    float dp = length(playervec);

    float angle = dot(vec2(0, -1), playervec) / (dp) * 100;
    float angle2 = dp;

    dp += noise(dp / 32.0 + timer * 2) * 6;
    dp += noise(dp / 16.0 + timer  * -4.35) * 3;
    dp += noise(dp / 8.0 + timer * 1)*1.5;

    dp += noise(angle / 32.0 + timer * 2) * 6;
    dp += noise(angle / 16.0 + timer  * -4.35) * 3;
    dp += noise(angle / 8.0 + timer * 1)*1.5;

    dp = -(dp / 360.0);
    float glowval2 = dp*light_distance / playerglow_strength;
    glowval2 += 1;
    glowval2 = clamp(glowval2, 0.0, 1.0);
    glowval2 = glowval2*glowval2 * playerglow_strength;


    /////////
    float dx = worldPos.x;

    dx += noise(worldPos.y / 32.0 + timer * 2) * 6;
    dx += noise(worldPos.y / 16.0 + timer  * -4.35) * 3;
    dx += noise(worldPos.y / 8.0 + timer * 1)*1.5;

    float glowval3 = -(dx / 360.0/* * ratio.y*/);
    glowval3 = glowval3*light_distance / (playerglow_strength * 4);
    glowval3 += 1;
    glowval3 = clamp(glowval3, 0.0, 1.0);
    glowval3 = glowval3*glowval3 * playerglow_strength*1.5;





    glowval = overscreen(glowval, glowval3);
    glowval = overscreen(glowval, glowval2);


    vec4 outcolor = basecolor;
    /*outcolor.g *= mix(1.0, 1.5, glowval);
    outcolor.g += mix(0.0, .5, glowval);
    outcolor.r *= mix(1.0, .5, glowval);
    outcolor.b *= mix(1.0, .5, glowval);

    float extra_green = max(outcolor.g-1.0, 0.0);
    outcolor.r += extra_green*.5;
    outcolor.b += extra_green*.5;*/

    outcolor = gblend(outcolor, mix(vec4(0.0), light_color * light_color.a, glowval));

    outcolor = outcolor;

    float extra_green = max(outcolor.g - 1.0, 0.0);
    outcolor.r += extra_green*overexpose;
    outcolor.b += extra_green*overexpose;

    float extra_red = max(outcolor.r - 1.0, 0.0);
    outcolor.g += extra_red*overexpose;
    outcolor.b += extra_red*overexpose;

    float extra_blue = max(outcolor.b - 1.0, 0.0);
    outcolor.r += extra_blue*overexpose;
    outcolor.g += extra_blue*overexpose;

    outcolor.a = 1.0;
    gl_FragColor = outcolor;
}

#endif
