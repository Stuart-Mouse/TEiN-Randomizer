#version 130

uniform mat2x4 color_xform;
uniform sampler2D palettetex;
uniform float palette;
uniform vec2 screensize;
uniform vec2 playerPos;

#if COMPILING_VERTEX_PROGRAM

void vert()
{
    vec4 outcolor = gl_Color * color_xform[0] + color_xform[1];
    gl_FrontColor = vec4(texture(palettetex, vec2((outcolor.r * 15.0 + 0.5) / 16.0, (palette + 0.5) / 64.0)).rgb, outcolor.a);

    vec4 vert_position = (gl_ModelViewProjectionMatrix * gl_Vertex);
    vec2 player_offset = playerPos / screensize; // the player offset from 0-1 relative to bottom-left corner

    // doing this centers the screen on the bottom-left corner of the map
    vert_position.x += 1.0;

    // offset by the player's position to center the player on screen
    vert_position.x -= (player_offset.x * 2);
    vert_position.y += ((1 - (2 * player_offset.y)) * -1);

    gl_Position = vert_position;
}

#elif COMPILING_FRAGMENT_PROGRAM

void frag()
{
    gl_FragColor = gl_Color;
}

#endif
