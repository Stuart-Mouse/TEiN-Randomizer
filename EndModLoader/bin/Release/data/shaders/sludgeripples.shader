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

varying vec2 worldPos;

#if COMPILING_VERTEX_PROGRAM

    void vert(){
        //gl_FrontColor = gl_Color * color_xform[0] + color_xform[1];
        vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
        ipalettesize = textureSize(palettetex, 0);
	palettesize = vec2(ipalettesize.x, ipalettesize.y);
        gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r*15.0+.5)/palettesize.x,(palette+.5)/palettesize.y)).rgb, outcolor.a);
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

        vec2 rippleval = vec2(sin(-timer*1.9*.5+worldPos.y / 9.0) * .0007*3.0 * ratio.x, 0.0);
        rippleval += vec2(sin(-timer*1.7*.5+worldPos.y / 16.0) * .0005*3.0 * ratio.x, 0.0);
        rippleval += vec2(sin(-timer*1.4*.5+worldPos.x / 32.0) * .0006*3.0 * ratio.x, 0.0);
        
        vec4 basecolor = texture(framebuf, screencoords.xy + rippleval);
        

        

        //reflections
        float dy = waterheight - worldPos.y;

        vec2 reflectionval = vec2(0.0, dy / 360.0) * ratio;
       // reflectionval += vec2(sin(-timer*20+worldPos.y / 1.0) * .001 * ratio.x, 0.0); //ripple distortion

        vec4 reflection = texture(framebuf, screencoords.xy - reflectionval);

        float ripple_alpha = .25;
        float ripple_fadedown = 2.0;

        float ripple_a = min(1.0, max(0.0, reflectionval.y * -ripple_fadedown/ratio.y));
        ripple_a = ripple_a*ripple_alpha+1.0-ripple_alpha;

        vec4 outcolor = basecolor*ripple_a + reflection*(1.0-ripple_a);


        outcolor = outcolor*(1.0-gl_Color.a) + gl_Color*gl_Color.a;

        outcolor.a = 1.0;
        gl_FragColor = outcolor;
    }
    
#endif
