namespace TEiNRandomizer
{
    public struct Tileset
    {
        public string AreaType;
        public string Tile;
        public string Overlay;
        public string Background;
        public string Particles;
        public string Shader;
        public string Palette;
        public string Music;
        public string Extras;
        public string ArtAlts;
        public string DoTilt;
        public string DoWobble;
        public string All;
    }

    //public struct GonTileset
    //{
    //    public string area_name;
    //    public int    area_label_frame;
    //    public string tile_graphics;
    //    public string overlay_graphics;
    //    public string background_graphics;
    //    public string foreground_graphics;
    //    public int    palette;
    //    public string area_type;
    //    public double toxic_timer;

    //    public string tile_particle_1;
    //    public string tile_particle_2;
    //    public string tile_particle_3;
    //    public string tile_particle_4;
    //    public string tile_particle_5;

    //    public string global_particle_1;
    //    public string global_particle_2;
    //    public string global_particle_3;

    //    public string decoration_1;
    //    public string decoration_2;
    //    public string decoration_3;

    //    public string npc_1;
    //    public string npc_2;
    //    public string npc_3;

    //    public string[] music;
    //    public string ambience;
    //    public double ambience_volume;
    //    public string stop_previous_music;

    //    public string[][] art_alts;

    //    public string fx_shader;
    //    public string fx_shader_mid;
    //    public int    midfx_layer;
    //    public string midfx_graphics;
    //    public double shader_param;

    //    public GonTileset(GonObject gon)
    //    {
    //        this = new GonTileset();

    //        if (gon["area_name"] != null)
    //            area_name = gon["area_name"].String();
    //        if (gon["area_label_frame"] != null)
    //            area_label_frame = gon["area_label_frame"].Int();
    //        if (gon["tile_graphics"] != null)
    //            tile_graphics = gon["tile_graphics"].String();
    //        if (gon["overlay_graphics"] != null)
    //            overlay_graphics = gon["overlay_graphics"].String();
    //        if (gon["background_graphics"] != null)
    //            background_graphics = gon["background_graphics"].String();
    //        if (gon["foreground_graphics"] != null)
    //            foreground_graphics = gon["foreground_graphics"].String();
    //        if (gon["palette"] != null)
    //            palette = gon["palette"].Int();
    //        if (gon["area_type"] != null)
    //            area_type = gon["area_type"].String();
    //        if (gon["toxic_timer"] != null)
    //            toxic_timer = gon["toxic_timer"].Number();
    //        if (gon["tile_particle_1"] != null)
    //            tile_particle_1 = gon["tile_particle_1"].String();
    //        if (gon["tile_particle_2"] != null)
    //            tile_particle_2 = gon["tile_particle_2"].String();
    //        if (gon["tile_particle_3"] != null)
    //            tile_particle_3 = gon["tile_particle_3"].String();
    //        if (gon["tile_particle_4"] != null)
    //            tile_particle_4 = gon["tile_particle_4"].String();
    //        if (gon["tile_particle_5"] != null)
    //            tile_particle_5 = gon["tile_particle_5"].String();
    //        if (gon["global_particle_1"] != null)
    //            global_particle_1 = gon["global_particle_1"].String();
    //        if (gon["global_particle_2"] != null)
    //            global_particle_2 = gon["global_particle_2"].String();
    //        if (gon["global_particle_3"] != null)
    //            global_particle_3 = gon["global_particle_3"].String();
    //        if (gon["decoration_1"] != null)
    //            decoration_1 = gon["decoration_1"].String();
    //        if (gon["decoration_2"] != null)
    //            decoration_2 = gon["decoration_2"].String();
    //        if (gon["decoration_3"] != null)
    //            decoration_3 = gon["decoration_3"].String();
    //        if (gon["npc_1"] != null)
    //            npc_1 = gon["npc_1"].String();
    //        if (gon["npc_2"] != null)
    //            npc_2 = gon["npc_2"].String();
    //        if (gon["npc_3"] != null)
    //            npc_3 = gon["npc_3"].String();
    //        if (gon["music"] != null)
    //            music = GonManip.GonToStringArray(gon["music"]);
    //        if (gon["ambience"] != null)
    //            ambience = gon["ambience"].String();
    //        if (gon["ambience_volume"] != null)
    //            ambience_volume = gon["ambience_volume"].Number();
    //        if (gon["stop_previous_music"] != null)
    //            stop_previous_music = gon["stop_previous_music"].String();
    //        if (gon["art_alts"] != null)
    //            art_alts = GonManip.GonTo2DStringArray(gon["art_alts"]);
    //        if (gon["fx_shader"] != null)
    //            fx_shader = gon["fx_shader"].String();
    //        if (gon["fx_shader_mid"] != null)
    //            fx_shader_mid = gon["fx_shader_mid"].String();
    //        if (gon["midfx_layer"] != null)
    //            midfx_layer = gon["midfx_layer"].Int();
    //        if (gon["midfx_graphics"] != null)
    //            midfx_graphics = gon["midfx_graphics"].String();
    //        if (gon["shader_param"] != null)
    //            shader_param = gon["shader_param"].Number();
    //    }

    //    // The tileset + operator overwrites each field in the left operand with the value from the right operand if it is not null.
    //    public static GonTileset operator +(GonTileset a, GonTileset b)
    //    {
    //        GonTileset tileset = new GonTileset();
    //        tileset = a;

    //        if (b.area_name != null)
    //            tileset.area_name = b.area_name;
    //        if (b.area_label_frame != 0)
    //            tileset.area_label_frame = b.area_label_frame;
    //        if (b.tile_graphics != null)
    //            tileset.tile_graphics = b.tile_graphics;
    //        if (b.overlay_graphics != null)
    //            tileset.overlay_graphics = b.overlay_graphics;
    //        if (b.background_graphics != null)
    //            tileset.background_graphics = b.background_graphics;
    //        if (b.foreground_graphics != null)
    //            tileset.foreground_graphics = b.foreground_graphics;
    //        if (b.palette != 0)
    //            tileset.palette = b.palette;
    //        if (b.area_type != null)
    //            tileset.area_type = b.area_type;
    //        if (b.toxic_timer != 0)
    //            tileset.toxic_timer = b.toxic_timer;
    //        if (b.tile_particle_1 != null)
    //            tileset.tile_particle_1 = b.tile_particle_1;
    //        if (b.tile_particle_2 != null)
    //            tileset.tile_particle_2 = b.tile_particle_2;
    //        if (b.tile_particle_3 != null)
    //            tileset.tile_particle_3 = b.tile_particle_3;
    //        if (b.tile_particle_4 != null)
    //            tileset.tile_particle_4 = b.tile_particle_4;
    //        if (b.tile_particle_5 != null)
    //            tileset.tile_particle_5 = b.tile_particle_5;
    //        if (b.global_particle_1 != null)
    //            tileset.global_particle_1 = b.global_particle_1;
    //        if (b.global_particle_2 != null)
    //            tileset.global_particle_2 = b.global_particle_2;
    //        if (b.global_particle_3 != null)
    //            tileset.global_particle_3 = b.global_particle_3;
    //        if (b.decoration_1 != null)
    //            tileset.decoration_1 = b.decoration_1;
    //        if (b.decoration_2 != null)
    //            tileset.decoration_2 = b.decoration_2;
    //        if (b.decoration_3 != null)
    //            tileset.decoration_3 = b.decoration_3;
    //        if (b.npc_1 != null)
    //            tileset.npc_1 = b.npc_1;
    //        if (b.npc_2 != null)
    //            tileset.npc_2 = b.npc_2;
    //        if (b.npc_3 != null)
    //            tileset.npc_3 = b.npc_3;
    //        if (b.music != null)
    //            tileset.music = b.music;
    //        if (b.ambience != null)
    //            tileset.ambience = b.ambience;
    //        if (b.ambience_volume != 0)
    //            tileset.ambience_volume = b.ambience_volume;
    //        if (b.stop_previous_music != null)
    //            tileset.stop_previous_music = b.stop_previous_music;
    //        if (b.art_alts != null)
    //            tileset.art_alts = b.art_alts;
    //        if (b.fx_shader != null)
    //            tileset.fx_shader = b.fx_shader;
    //        if (b.fx_shader_mid != null)
    //            tileset.fx_shader_mid = b.fx_shader_mid;
    //        if (b.midfx_layer != 0)
    //            tileset.midfx_layer = b.midfx_layer;
    //        if (b.midfx_graphics != null)
    //            tileset.midfx_graphics = b.midfx_graphics;
    //        if (b.shader_param != 0)
    //            tileset.shader_param = b.shader_param;

    //        return tileset;
    //    }

    //}
}
