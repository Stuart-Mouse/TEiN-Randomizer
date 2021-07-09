#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
ivec2 ipalettesize;
vec2 palettesize;
uniform sampler2D framebuf;
uniform float palette;

uniform vec2 screensize;
uniform float timer;

varying vec2 worldPos;

#if COMPILING_VERTEX_PROGRAM

    void vert(){
        //gl_FrontColor = gl_Color * color_xform[0] + color_xform[1];
        vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
  	ipalettesize = textureSize(palettetex, 0);
	palettesize = vec2(ipalettesize.x, ipalettesize.y);
        gl_FrontColor = outcolor;//vec4(texture(palettetex, vec2((outcolor.r*15.0+.5)/palettesize.x,(palette+.5)/palettesize.y)).rgb, outcolor.a);
        gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
        worldPos = (gl_ModelViewMatrix * gl_Vertex).xy;
    }
    
#elif COMPILING_FRAGMENT_PROGRAM

    void frag(){
        vec4 screencoords = gl_FragCoord;

        screencoords.x /= screensize.x;
        screencoords.y /= screensize.y;

        
        float scale_x = screensize.x / 1280.0;
        float scale_y = screensize.y / 720.0;
        float scale_min = min(scale_x, scale_y);

        vec2 ratio = vec2(scale_min / scale_x, scale_min / scale_y);

        vec2 rippleval = vec2((gl_Color.rg-vec2(.5,.5))*.01 * ratio);
        
        vec4 outcolor = texture(framebuf, screencoords.xy + rippleval);

        outcolor.a = 1.0;
        gl_FragColor = outcolor;
    }
    
#endif
