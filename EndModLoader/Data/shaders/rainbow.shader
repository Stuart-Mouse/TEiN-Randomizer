#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform sampler2D framebuf;
uniform float palette;

uniform vec2 screensize;
uniform float timer;
uniform float level_param;
varying vec2 worldPos;
uniform vec2 playerPos;

#if COMPILING_VERTEX_PROGRAM

    void vert(){
        //gl_FrontColor = gl_Color * color_xform[0] + color_xform[1];
        vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
        gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r*15.0+.5)/16.0,(palette+.5)/64.0)).rgb, outcolor.a);
        gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
		worldPos = (gl_ModelViewMatrix * gl_Vertex).xy;
    }
    
#elif COMPILING_FRAGMENT_PROGRAM

	float hash(float x){
        return fract(sin(x)*43758.5453);
    }
    float noise( float u ){
        vec3 x = vec3(u, 0, 0);

        vec3 p = floor(x);
        vec3 f = fract(x);

        f       = f*f*(3.0-2.0*f);
        float n = p.x + p.y*57.0 + 113.0*p.z;

        return mix(mix(mix( hash(n+0.0), hash(n+1.0),f.x),
            mix( hash(n+57.0), hash(n+58.0),f.x),f.y),
            mix(mix( hash(n+113.0), hash(n+114.0),f.x),
                mix( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
    }
	
	vec4 gblend(vec4 a, vec4 b){
        return 1.0-(1.0-a)*(1.0-b);
    }

    void frag(){

	    vec4 screencoords = gl_FragCoord;

        screencoords.x /= screensize.x;
        screencoords.y /= screensize.y;
		
		
		float scale_x = screensize.x / 1280.0;
        float scale_y = screensize.y / 720.0;
        float scale_min = min(scale_x, scale_y);
	
		vec2 ratio = vec2(scale_min / scale_x, scale_min / scale_y);
		
		vec4 outcolor = texture(framebuf, screencoords.xy);
		
        if((outcolor.r == 0.0) && (outcolor.g == 0.0) && (outcolor.b == 0.0)) {
			
			outcolor.r = noise(sin(worldPos.x/160.0) + sin(worldPos.y/90.0) + 25.0 + timer * level_param * 0.125);
			outcolor.g = noise(cos(worldPos.x/160.0) + cos(worldPos.y/90.0) + 15.0 + timer * level_param * 0.125);
			outcolor.b = noise(sin(worldPos.x/160.0) + cos(worldPos.y/90.0) + 35.0 + timer * level_param * 0.125);
			
			while((outcolor.r + outcolor.g + outcolor.b) < 1.8) {
				outcolor *= 1.01;
			}
			while((outcolor.r + outcolor.g + outcolor.b) > 1.8) {
				outcolor *= 0.99;
			}
			
			outcolor.r *= noise(sin(worldPos.x/320.0) + sin(worldPos.y/180.0) + 25.0 + timer * level_param * 0.25);
			outcolor.g *= noise(cos(worldPos.x/320.0) + cos(worldPos.y/180.0) + 15.0 + timer * level_param * 0.25);
			outcolor.b *= noise(sin(worldPos.x/320.0) + cos(worldPos.y/180.0) + 35.0 + timer * level_param * 0.25);
			
			while((outcolor.r + outcolor.g + outcolor.b) < 1.8) {
				outcolor *= 1.01;
			}
			while((outcolor.r + outcolor.g + outcolor.b) > 1.8) {
				outcolor *= 0.99;
			}
			
			outcolor.r *= noise(sin(worldPos.x/640.0) + sin(worldPos.y/360.0) + 25.0 + timer * level_param * 0.5);
			outcolor.g *= noise(cos(worldPos.x/640.0) + cos(worldPos.y/360.0) + 15.0 + timer * level_param * 0.5);
			outcolor.b *= noise(sin(worldPos.x/640.0) + cos(worldPos.y/360.0) + 35.0 + timer * level_param * 0.5);
			
			while((outcolor.r + outcolor.g + outcolor.b) < 1.8) {
				outcolor *= 1.01;
			}
			while((outcolor.r + outcolor.g + outcolor.b) > 1.8) {
				outcolor *= 0.99;
			}
		} else if((outcolor.r == 31.0/255.0) && (outcolor.g == 31.0/255.0) && (outcolor.b == 31.0/255.0)) {
			
			outcolor.r = 1-noise(sin(worldPos.x/160.0) + sin(worldPos.y/90.0) + 25.0 + timer * level_param * 0.125);
			outcolor.g = 1-noise(cos(worldPos.x/160.0) + cos(worldPos.y/90.0) + 15.0 + timer * level_param * 0.125);
			outcolor.b = 1-noise(sin(worldPos.x/160.0) + cos(worldPos.y/90.0) + 35.0 + timer * level_param * 0.125);
			
			while((outcolor.r + outcolor.g + outcolor.b) < 1.2) {
				outcolor *= 1.01;
			}
			while((outcolor.r + outcolor.g + outcolor.b) > 1.2) {
				outcolor *= 0.99;
			}
			
			outcolor.r *= 1-noise(sin(worldPos.x/320.0) + sin(worldPos.y/180.0) + 25.0 + timer * level_param * 0.25);
			outcolor.g *= 1-noise(cos(worldPos.x/320.0) + cos(worldPos.y/180.0) + 15.0 + timer * level_param * 0.25);
			outcolor.b *= 1-noise(sin(worldPos.x/320.0) + cos(worldPos.y/180.0) + 35.0 + timer * level_param * 0.25);
		
			while((outcolor.r + outcolor.g + outcolor.b) < 1.2) {
				outcolor *= 1.01;
			}
			while((outcolor.r + outcolor.g + outcolor.b) > 1.2) {
				outcolor *= 0.99;
			}
		
			outcolor.r *= 1-noise(sin(worldPos.x/640.0) + sin(worldPos.y/360.0) + 25.0 + timer * level_param * 0.5);
			outcolor.g *= 1-noise(cos(worldPos.x/640.0) + cos(worldPos.y/360.0) + 15.0 + timer * level_param * 0.5);
			outcolor.b *= 1-noise(sin(worldPos.x/640.0) + cos(worldPos.y/360.0) + 35.0 + timer * level_param * 0.5);
			
			while((outcolor.r + outcolor.g + outcolor.b) < 1.2) {
				outcolor *= 1.01;
			}
			while((outcolor.r + outcolor.g + outcolor.b) > 1.2) {
				outcolor *= 0.99;
			}
		} else if((outcolor.r == 63.0/255.0) && (outcolor.g == 63.0/255.0) && (outcolor.b == 63.0/255.0)) {
			
			outcolor.r = 2-noise(sin(worldPos.x/160.0) + sin(worldPos.y/90.0) + 25.0 + timer * level_param * 0.125);
			outcolor.g = 2-noise(cos(worldPos.x/160.0) + cos(worldPos.y/90.0) + 15.0 + timer * level_param * 0.125);
			outcolor.b = 2-noise(sin(worldPos.x/160.0) + cos(worldPos.y/90.0) + 35.0 + timer * level_param * 0.125);
			
			while((outcolor.r + outcolor.g + outcolor.b) > 0.6) {
				outcolor *= 0.99;
			}
			while((outcolor.r + outcolor.g + outcolor.b) < 0.6) {
				outcolor *= 1.01;
			}
			
			outcolor.r *= 1-noise(sin(worldPos.x/320.0) + sin(worldPos.y/180.0) + 25.0 + timer * level_param * 0.25);
			outcolor.g *= 1-noise(cos(worldPos.x/320.0) + cos(worldPos.y/180.0) + 15.0 + timer * level_param * 0.25);
			outcolor.b *= 1-noise(sin(worldPos.x/320.0) + cos(worldPos.y/180.0) + 35.0 + timer * level_param * 0.25);
		
			while((outcolor.r + outcolor.g + outcolor.b) > 0.6) {
				outcolor *= 0.99;
			}
			while((outcolor.r + outcolor.g + outcolor.b) < 0.6) {
				outcolor *= 1.01;
			}
		
			outcolor.r *= 1-noise(sin(worldPos.x/640.0) + sin(worldPos.y/360.0) + 25.0 + timer * level_param * 0.5);
			outcolor.g *= 1-noise(cos(worldPos.x/640.0) + cos(worldPos.y/360.0) + 15.0 + timer * level_param * 0.5);
			outcolor.b *= 1-noise(sin(worldPos.x/640.0) + cos(worldPos.y/360.0) + 35.0 + timer * level_param * 0.5);
			
			while((outcolor.r + outcolor.g + outcolor.b) > 0.6) {
				outcolor *= 0.99;
			}
			while((outcolor.r + outcolor.g + outcolor.b) < 0.6) {
				outcolor *= 1.01;
			}
		} else if ((outcolor.r == 51.0/255.0) && (outcolor.g == 51.0/255.0) && (outcolor.b == 51.0/255.0)) {
			outcolor.r = 0;
			outcolor.g = 15.0/255.0;
			outcolor.b = 15.0/255.0;
			if (0.003 > noise(worldPos.x * worldPos.y)) {
				outcolor.r = 1.0;
				outcolor.g = 1.0;
				outcolor.b = 1.0;
			}
		}
		
        outcolor.a = 1.0;
			
        gl_FragColor = outcolor;
    }
    
#endif
