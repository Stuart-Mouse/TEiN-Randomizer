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
        ratio.y *= (1280.0/720.0);

        vec2 off1 = vec2(0,  1)*ratio*.015;
        vec2 off2 = vec2(0, -1)*ratio*.015;
        vec2 off3 = vec2(1,  0)*ratio*.015;
        vec2 off4 = vec2(-1, 0)*ratio*.015;
        vec2 off5 = vec2(1,  1)*ratio*.015* 0.7071;
        vec2 off6 = vec2(-1, 1)*ratio*.015* 0.7071;
        vec2 off7 = vec2(1, -1)*ratio*.015* 0.7071;
        vec2 off8 = vec2(-1,-1)*ratio*.015* 0.7071;
        
        vec4 outcolor_main = texture(framebuf, screencoords.xy);

        float t_a = fract(timer*.5);
        float t_b = fract(timer*.5+.5);

        vec4 outcolora_1 = texture(framebuf, screencoords.xy + off1*t_a);
        vec4 outcolora_2 = texture(framebuf, screencoords.xy + off2*t_a);
        vec4 outcolora_3 = texture(framebuf, screencoords.xy + off3*t_a);
        vec4 outcolora_4 = texture(framebuf, screencoords.xy + off4*t_a);
        vec4 outcolora_5 = texture(framebuf, screencoords.xy + off5*t_a);
        vec4 outcolora_6 = texture(framebuf, screencoords.xy + off6*t_a);
        vec4 outcolora_7 = texture(framebuf, screencoords.xy + off7*t_a);
        vec4 outcolora_8 = texture(framebuf, screencoords.xy + off8*t_a);

        vec4 outcolorb_1 = texture(framebuf, screencoords.xy + off1*t_b);
        vec4 outcolorb_2 = texture(framebuf, screencoords.xy + off2*t_b);
        vec4 outcolorb_3 = texture(framebuf, screencoords.xy + off3*t_b);
        vec4 outcolorb_4 = texture(framebuf, screencoords.xy + off4*t_b);
        vec4 outcolorb_5 = texture(framebuf, screencoords.xy + off5*t_b);
        vec4 outcolorb_6 = texture(framebuf, screencoords.xy + off6*t_b);
        vec4 outcolorb_7 = texture(framebuf, screencoords.xy + off7*t_b);
        vec4 outcolorb_8 = texture(framebuf, screencoords.xy + off8*t_b);

        vec4 outcolora_mix = 
            min(
                min(
                    min(outcolora_1, outcolora_2), 
                    min(outcolora_3, outcolora_4)
                ),
                min(
                    min(outcolora_5, outcolora_6), 
                    min(outcolora_7, outcolora_8)
                )
            );

        vec4 outcolorb_mix = 
            min(
                min(
                    min(outcolorb_1, outcolorb_2), 
                    min(outcolorb_3, outcolorb_4)
                ),
                min(
                    min(outcolorb_5, outcolorb_6), 
                    min(outcolorb_7, outcolorb_8)
                )
            );


        outcolora_mix = mix(outcolor_main, outcolora_mix, (1.0-t_a)*.5);
        outcolorb_mix = mix(outcolor_main, outcolorb_mix, (1.0-t_b)*.5);
        vec4 outcolor = mix(outcolora_mix, outcolorb_mix, .5);

        outcolor.a = 1.0;
        gl_FragColor = outcolor;
    }
    
#endif
