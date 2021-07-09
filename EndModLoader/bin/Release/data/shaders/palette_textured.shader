#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
ivec2 ipalettesize;
vec2 palettesize;
uniform sampler2D tex;
uniform float palette;
varying vec4 texcoord;

#if COMPILING_VERTEX_PROGRAM

    void vert(){
        gl_FrontColor = gl_Color * color_xform[0] + color_xform[1];
        gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
        texcoord = gl_MultiTexCoord0;
    }
    
#elif COMPILING_FRAGMENT_PROGRAM

    void frag(){
        vec4 outcolor = gl_Color * texture(tex, texcoord.xy);
        ipalettesize = textureSize(palettetex, 0);
	palettesize = vec2(ipalettesize.x, ipalettesize.y);
        gl_FragColor = vec4(texture(palettetex, vec2((outcolor.r*15.0+.5)/palettesize.x,(palette+.5)/palettesize.y)).rgb, outcolor.a);
    }
    
#endif
