using System.Collections.Generic;
using System.IO;

namespace TEiNRandomizer
{
    public struct Tileset
    {
        public string area_name;
        public int    area_label_frame;
        public string tile_graphics;
        public string overlay_graphics;
        public string background_graphics;
        public string foreground_graphics;
        public int    palette;
        public string area_type;
        public double toxic_timer;

        public string platform_physics;
        public string water_physics;
        public string player_physics;
        public string lowgrav_physics;

        public bool   do_tilt;
        public bool   do_wobble;

        public string tile_particle_1;
        public string tile_particle_2;
        public string tile_particle_3;
        public string tile_particle_4;
        public string tile_particle_5;

        public string global_particle_1;
        public string global_particle_2;
        public string global_particle_3;

        public string decoration_1;
        public string decoration_2;
        public string decoration_3;

        public string npc_1;
        public string npc_2;
        public string npc_3;

        public string music;
        public string ambience;
        public string ambience_volume;
        public string stop_previous_music;

        public Dictionary<string, string> art_alts;

        public string fx_shader;
        public string fx_shader_mid;
        public string midfx_graphics;
        public int    midfx_layer;
        public string shader_param;

        public string extras;


        public Tileset(GonObject gon)
        {
            this = new Tileset();
            
            if (gon["area_name"] != null)
                area_name = gon["area_name"].String();
            if (gon["area_label_frame"] != null)
                area_label_frame = gon["area_label_frame"].Int();
            if (gon["tile_graphics"] != null)
                tile_graphics = gon["tile_graphics"].String();
            if (gon["overlay_graphics"] != null)
                overlay_graphics = gon["overlay_graphics"].String();
            if (gon["background_graphics"] != null)
                background_graphics = gon["background_graphics"].String();
            if (gon["foreground_graphics"] != null)
                foreground_graphics = gon["foreground_graphics"].String();
            if (gon["palette"] != null)
                palette = gon["palette"].Int();
            if (gon["area_type"] != null)
                area_type = gon["area_type"].String();
            if (gon["toxic_timer"] != null)
                toxic_timer = gon["toxic_timer"].Number();
            if (gon["platform_physics"] != null)
                platform_physics = gon["platform_physics"].String();
            if (gon["water_physics"] != null)
                water_physics = gon["water_physics"].String();
            if (gon["player_physics"] != null)
                player_physics = gon["player_physics"].String();
            if (gon["lowgrav_physics"] != null)
                lowgrav_physics = gon["_physics"].String();
            if (gon["tile_particle_1"] != null)
                tile_particle_1 = gon["tile_particle_1"].String();
            if (gon["tile_particle_2"] != null)
                tile_particle_2 = gon["tile_particle_2"].String();
            if (gon["tile_particle_3"] != null)
                tile_particle_3 = gon["tile_particle_3"].String();
            if (gon["tile_particle_4"] != null)
                tile_particle_4 = gon["tile_particle_4"].String();
            if (gon["tile_particle_5"] != null)
                tile_particle_5 = gon["tile_particle_5"].String();
            if (gon["global_particle_1"] != null)
                global_particle_1 = gon["global_particle_1"].String();
            if (gon["global_particle_2"] != null)
                global_particle_2 = gon["global_particle_2"].String();
            if (gon["global_particle_3"] != null)
                global_particle_3 = gon["global_particle_3"].String();
            if (gon["decoration_1"] != null)
                decoration_1 = gon["decoration_1"].String();
            if (gon["decoration_2"] != null)
                decoration_2 = gon["decoration_2"].String();
            if (gon["decoration_3"] != null)
                decoration_3 = gon["decoration_3"].String();
            if (gon["npc_1"] != null)
                npc_1 = gon["npc_1"].String();
            if (gon["npc_2"] != null)
                npc_2 = gon["npc_2"].String();
            if (gon["npc_3"] != null)
                npc_3 = gon["npc_3"].String();
            if (gon["music"] != null)
                music = gon["music"].GetOutStr().Trim();
            if (gon["ambience"] != null)
                ambience = gon["ambience"].String();
            if (gon["ambience_volume"] != null)
                ambience_volume = gon["ambience_volume"].String();
            if (gon["stop_previous_music"] != null)
                stop_previous_music = gon["stop_previous_music"].String();
            if (gon["art_alts"] != null)
                art_alts = gon["art_alts"].ToDictionary();
            if (gon["fx_shader"] != null)
                fx_shader = gon["fx_shader"].String();
            if (gon["fx_shader_mid"] != null)
                fx_shader_mid = gon["fx_shader_mid"].String();
            if (gon["midfx_layer"] != null)
                midfx_layer = gon["midfx_layer"].Int();
            if (gon["midfx_graphics"] != null)
                midfx_graphics = gon["midfx_graphics"].String();
            if (gon["shader_param"] != null)
                shader_param = gon["shader_param"].String();
        }

        public static Tileset PriorityMerge(Tileset a, Tileset b)
        {
            Tileset t;

            t.area_name             = b.area_name           != null ? b.area_name           : a.area_name;
            t.area_label_frame      = b.area_label_frame    != 0    ? b.area_label_frame    : a.area_label_frame;
            t.tile_graphics         = b.tile_graphics       != null ? b.tile_graphics       : a.tile_graphics;
            t.overlay_graphics      = b.overlay_graphics    != null ? b.overlay_graphics    : a.overlay_graphics;
            t.background_graphics   = b.background_graphics != null ? b.background_graphics : a.background_graphics;
            t.foreground_graphics   = b.foreground_graphics != null ? b.foreground_graphics : a.foreground_graphics;
            t.palette               = b.palette             != 0    ? b.palette             : a.palette;
            t.area_type             = b.area_type           != null ? b.area_type           : a.area_type;
            t.toxic_timer           = b.toxic_timer         != 0    ? b.toxic_timer         : a.toxic_timer;

            t.do_tilt   = b.do_tilt;
            t.do_wobble = b.do_wobble;

            t.platform_physics      = b.platform_physics    != null ? b.platform_physics    : a.platform_physics;
            t.water_physics         = b.water_physics       != null ? b.water_physics       : a.water_physics;
            t.player_physics        = b.player_physics      != null ? b.player_physics      : a.player_physics;
            t.lowgrav_physics       = b.lowgrav_physics     != null ? b.lowgrav_physics     : a.lowgrav_physics;

            t.tile_particle_1       = b.tile_particle_1     != null ? b.tile_particle_1     : a.tile_particle_1;
            t.tile_particle_2       = b.tile_particle_2     != null ? b.tile_particle_2     : a.tile_particle_2;
            t.tile_particle_3       = b.tile_particle_3     != null ? b.tile_particle_3     : a.tile_particle_3;
            t.tile_particle_4       = b.tile_particle_4     != null ? b.tile_particle_4     : a.tile_particle_4;
            t.tile_particle_5       = b.tile_particle_5     != null ? b.tile_particle_5     : a.tile_particle_5;

            t.global_particle_1     = b.global_particle_1   != null ? b.global_particle_1   : a.global_particle_1;
            t.global_particle_2     = b.global_particle_2   != null ? b.global_particle_2   : a.global_particle_2;
            t.global_particle_3     = b.global_particle_3   != null ? b.global_particle_3   : a.global_particle_3;

            t.decoration_1          = b.decoration_1        != null ? b.decoration_1        : a.decoration_1;
            t.decoration_2          = b.decoration_2        != null ? b.decoration_2        : a.decoration_2;
            t.decoration_3          = b.decoration_3        != null ? b.decoration_3        : a.decoration_3;

            t.npc_1                 = b.npc_1               != null ? b.npc_1               : a.npc_1;
            t.npc_2                 = b.npc_2               != null ? b.npc_2               : a.npc_2;
            t.npc_3                 = b.npc_3               != null ? b.npc_3               : a.npc_3;

            t.music                 = b.music               != null ? b.music               : a.music;
            t.ambience              = b.ambience            != null ? b.ambience            : a.ambience;
            t.ambience_volume       = b.ambience_volume     != null ? b.ambience_volume     : a.ambience_volume;
            t.stop_previous_music   = b.stop_previous_music != null ? b.stop_previous_music : a.stop_previous_music;

            t.fx_shader             = b.fx_shader           != null ? b.fx_shader           : a.fx_shader;
            t.extras                = b.extras              != null ? b.extras              : a.extras;

            // All of these area dependent on the fx_shader_mid
            t.fx_shader_mid  = b.fx_shader_mid != null ? b.fx_shader_mid  : a.fx_shader_mid;
            t.midfx_graphics = b.fx_shader_mid != null ? b.midfx_graphics : a.midfx_graphics;
            t.midfx_layer    = b.fx_shader_mid != null ? b.midfx_layer    : a.midfx_layer;
            t.shader_param   = b.fx_shader_mid != null ? b.shader_param   : a.shader_param;

            t.art_alts = ArtAltsMerge(a.art_alts, b.art_alts);

            return t;
        }
        public static Tileset PriorityMerge(params Tileset[] list)
        {
            Tileset ret = new Tileset();
            foreach (Tileset ts in list)
                ret = PriorityMerge(ret, ts);
            return ret;
        }
        public static Tileset GetDifference(Tileset a, Tileset b)
        {
            Tileset t = new Tileset();

            t.area_name           = b.area_name           != a.area_name           ? b.area_name           : null;
            t.area_label_frame    = b.area_label_frame    != a.area_label_frame    ? b.area_label_frame    : 0;
            t.tile_graphics       = b.tile_graphics       != a.tile_graphics       ? b.tile_graphics       : null;
            t.overlay_graphics    = b.overlay_graphics    != a.overlay_graphics    ? b.overlay_graphics    : null;
            t.background_graphics = b.background_graphics != a.background_graphics ? b.background_graphics : null;
            t.foreground_graphics = b.foreground_graphics != a.foreground_graphics ? b.foreground_graphics : null;
            t.palette             = b.palette             != a.palette             ? b.palette             : 0;
            t.area_type           = b.area_type           != a.area_type           ? b.area_type           : null;
            t.toxic_timer         = b.toxic_timer         != a.toxic_timer         ? b.toxic_timer         : 0;

            t.do_tilt   = b.do_tilt;
            t.do_wobble = b.do_wobble;

            t.platform_physics = b.platform_physics != a.platform_physics ? b.platform_physics : null;
            t.water_physics    = b.water_physics    != a.water_physics    ? b.water_physics    : null;
            t.player_physics   = b.player_physics   != a.player_physics   ? b.player_physics   : null;
            t.lowgrav_physics  = b.lowgrav_physics  != a.lowgrav_physics  ? b.lowgrav_physics  : null;

            t.tile_particle_1 = b.tile_particle_1 != a.tile_particle_1 ? b.tile_particle_1 : null;
            t.tile_particle_2 = b.tile_particle_2 != a.tile_particle_2 ? b.tile_particle_2 : null;
            t.tile_particle_3 = b.tile_particle_3 != a.tile_particle_3 ? b.tile_particle_3 : null;
            t.tile_particle_4 = b.tile_particle_4 != a.tile_particle_4 ? b.tile_particle_4 : null;
            t.tile_particle_5 = b.tile_particle_5 != a.tile_particle_5 ? b.tile_particle_5 : null;

            t.global_particle_1 = b.global_particle_1 != a.global_particle_1 ? b.global_particle_1 : null;
            t.global_particle_2 = b.global_particle_2 != a.global_particle_2 ? b.global_particle_2 : null;
            t.global_particle_3 = b.global_particle_3 != a.global_particle_3 ? b.global_particle_3 : null;

            t.decoration_1 = b.decoration_1 != a.decoration_1 ? b.decoration_1 : null;
            t.decoration_2 = b.decoration_2 != a.decoration_2 ? b.decoration_2 : null;
            t.decoration_3 = b.decoration_3 != a.decoration_3 ? b.decoration_3 : null;

            t.npc_1 = b.npc_1 != a.npc_1 ? b.npc_1 : null;
            t.npc_2 = b.npc_2 != a.npc_2 ? b.npc_2 : null;
            t.npc_3 = b.npc_3 != a.npc_3 ? b.npc_3 : null;

            t.music               = b.music               != a.music               ? b.music               : null;
            t.ambience            = b.ambience            != a.ambience            ? b.ambience            : null;
            t.ambience_volume     = b.ambience_volume     != a.ambience_volume     ? b.ambience_volume     : null;
            t.stop_previous_music = b.stop_previous_music != a.stop_previous_music ? b.stop_previous_music : null;

            t.fx_shader = b.fx_shader != a.fx_shader ? b.fx_shader : null;
            t.extras    = b.extras    != a.extras    ? b.extras    : null;

            // All of these area dependent on the fx_shader_mid
            t.fx_shader_mid  = b.fx_shader_mid != a.fx_shader_mid ? b.fx_shader_mid  : null;
            t.midfx_graphics = b.fx_shader_mid != a.fx_shader_mid ? b.midfx_graphics : null;
            t.midfx_layer    = b.fx_shader_mid != a.fx_shader_mid ? b.midfx_layer    : 0;
            t.shader_param   = b.fx_shader_mid != a.fx_shader_mid ? b.shader_param   : null;

            if (a.art_alts != b.art_alts)
                t.art_alts = ArtAltsMerge(a.art_alts, b.art_alts);

            return t;
        }
        public static Dictionary<string, string> ArtAltsMerge(in Dictionary<string, string> low, in Dictionary<string, string> high)
        {
            Dictionary<string, string> new_list = new Dictionary<string, string>();

            // Prevent errors by checking if the lists are null
            if (low == null)
            {
                if (high == null)
                    return new_list;
                else return high;
            }
            else if (high == null) return low;

            // add all high priority items
            new_list = high;

            // fill in with low priority items
            foreach (var item in low)
            {
                if (!new_list.ContainsKey(item.Key))
                    new_list.Add(item.Key, item.Value);
            }

            // return the newly created list
            return new_list;
        }
        public void WriteTileset(StreamWriter sw)
        {
            // This function takes in a streamWriter which it uses to write all of its data to a file

            if (area_name != null)
                sw.WriteLine($"area_name {area_name}");
            if (area_label_frame != 0)
                sw.WriteLine($"area_label_frame {area_label_frame}");
            if (tile_graphics != null)
                sw.WriteLine($"tile_graphics {tile_graphics}");
            if (overlay_graphics != null)
                sw.WriteLine($"overlay_graphicsy {overlay_graphics}");
            if (background_graphics != null)
                sw.WriteLine($"background_graphics {background_graphics}");
            if (foreground_graphics != null)
                sw.WriteLine($"foreground_graphics {foreground_graphics}");
            if (palette != 0)
                sw.WriteLine($"palette {palette}");
            if (area_type != null)
                sw.WriteLine($"area_type {area_type}");
            if (toxic_timer != 0)
                sw.WriteLine($"toxic_timer {toxic_timer}");
            if (platform_physics != null)
                sw.WriteLine($"platform_physics {platform_physics}");
            if (water_physics != null)
                sw.WriteLine($"water_physics {water_physics}");
            if (player_physics != null)
                sw.WriteLine($"player_physics {player_physics}");
            if (lowgrav_physics != null)
                sw.WriteLine($"lowgrav_physics {lowgrav_physics}");
            if (tile_particle_1 != null)
                sw.WriteLine($"tile_particle_1 {tile_particle_1}");
            if (tile_particle_2 != null)
                sw.WriteLine($"tile_particle_2 {tile_particle_2}");
            if (tile_particle_3 != null)
                sw.WriteLine($"tile_particle_3 {tile_particle_3}");
            if (tile_particle_4 != null)
                sw.WriteLine($"tile_particle_4 {tile_particle_4}");
            if (tile_particle_5 != null)
                sw.WriteLine($"tile_particle_5 {tile_particle_5}");
            if (global_particle_1 != null)
                sw.WriteLine($"global_particle_1 {global_particle_1}");
            if (global_particle_2 != null)
                sw.WriteLine($"global_particle_2 {global_particle_2}");
            if (global_particle_3 != null)
                sw.WriteLine($"global_particle_3 {global_particle_3}");
            if (decoration_1 != null)
                sw.WriteLine($"decoration_1 {decoration_1}");
            if (decoration_2 != null)
                sw.WriteLine($"decoration_2 {decoration_2}");
            if (decoration_3 != null)
                sw.WriteLine($"decoration_3 {decoration_3}");
            if (npc_1 != null)
                sw.WriteLine($"npc_1 {npc_1}");
            if (npc_2 != null)
                sw.WriteLine($"npc_2 {npc_2}");
            if (npc_3 != null)
                sw.WriteLine($"npc_3 {npc_3}");
            if (music != null)
                sw.WriteLine($"music {music}");
            if (ambience != null)
                sw.WriteLine($"ambience {ambience}");
            if (ambience_volume != null)
                sw.WriteLine($"ambience_volume {ambience_volume}");
            if (stop_previous_music != null)
                sw.WriteLine($"stop_previous_music {stop_previous_music}");
            if (art_alts != null)
            {
                sw.Write($"art_alts [");
                foreach (var item in art_alts)
                    sw.Write($"[{item.Key},{item.Value}]");
                sw.Write("]\n");
            }
            if (fx_shader != null)
                sw.WriteLine($"fx_shader {fx_shader}");
            if (fx_shader_mid != null)
            {
                sw.WriteLine($"fx_shader_mid {fx_shader_mid}");
                if (midfx_graphics != null)
                    sw.WriteLine($"midfx_graphics {midfx_graphics}");
                if (midfx_layer != 0)
                    sw.WriteLine($"midfx_layer {midfx_layer}");
                if (shader_param != null)
                    sw.WriteLine($"shader_param {shader_param}");
            }
            if (extras != null)
                sw.WriteLine(extras + "\n");
        }
    }
}
