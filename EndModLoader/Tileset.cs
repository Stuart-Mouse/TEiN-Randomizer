using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public class Tileset
    {
        // MEMBERS

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

        public string npc_1;    // NPCs are handled seperately from regualar tileset generation.
        public string npc_2;    // This is because we only want to generate them when necessary.
        public string npc_3;

        public string music;
        public string ambience;
        public double ambience_volume;
        public string stop_previous_music;

        public List<string[]> art_alts;

        public string fx_shader;

        public string extras;

        // Shader class is included so that fx_shader_mid settings can be easily overwritten as a single thing
        public Shader shaderMid;

        // Constructors
        public Tileset() { }
        public Tileset(GonObject gon)
        {
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
                ambience_volume = gon["ambience_volume"].Number();
            if (gon["stop_previous_music"] != null)
                stop_previous_music = gon["stop_previous_music"].String();
            if (gon["art_alts"] != null)
                art_alts = GonObject.Manip.To2DStringArray(gon["art_alts"]).ToList();
            if (gon["fx_shader"] != null)
                fx_shader = gon["fx_shader"].String();
            if (gon["fx_shader_mid"] != null)
            {
                shaderMid = new Shader();
                shaderMid.fx_shader_mid = gon["fx_shader_mid"].String();
                if (gon["midfx_layer"] != null)
                    shaderMid.midfx_layer = gon["midfx_layer"].Int();
                if (gon["midfx_graphics"] != null)
                    shaderMid.midfx_graphics = gon["midfx_graphics"].String();
                if (gon["shader_param"] != null)
                    shaderMid.shader_param = gon["shader_param"].Number();
            }
        }

        public Tileset Clone()
        {
            // Create new tilset to return
            // Copy all initial values from this
            Tileset tileset = new Tileset();

            tileset.area_name = area_name;
            tileset.area_label_frame = area_label_frame;
            tileset.tile_graphics = tile_graphics;
            tileset.overlay_graphics = overlay_graphics;
            tileset.background_graphics = background_graphics;
            tileset.foreground_graphics = foreground_graphics;
            tileset.palette = palette;
            tileset.area_type = area_type;
            tileset.toxic_timer = toxic_timer;
            tileset.platform_physics = platform_physics;
            tileset.water_physics = water_physics;
            tileset.player_physics = player_physics;
            tileset.lowgrav_physics = lowgrav_physics;
            tileset.tile_particle_1 = tile_particle_1;
            tileset.tile_particle_2 = tile_particle_2;
            tileset.tile_particle_3 = tile_particle_3;
            tileset.tile_particle_4 = tile_particle_4;
            tileset.tile_particle_5 = tile_particle_5;
            tileset.global_particle_1 = global_particle_1;
            tileset.global_particle_2 = global_particle_2;
            tileset.global_particle_3 = global_particle_3;
            tileset.decoration_1 = decoration_1;
            tileset.decoration_2 = decoration_2;
            tileset.decoration_3 = decoration_3;
            tileset.npc_1 = npc_1;
            tileset.npc_2 = npc_2;
            tileset.npc_3 = npc_3;
            tileset.music = music;
            tileset.ambience = ambience;
            tileset.ambience_volume = ambience_volume;
            tileset.stop_previous_music = stop_previous_music;
            tileset.art_alts = art_alts;
            tileset.fx_shader = fx_shader;
            tileset.shaderMid = shaderMid;
            tileset.extras = extras;

            return tileset;
        }


        // OPERATOR OVERLOADS
        // The tileset + operator overwrites each field in the left operand with the value from the right operand if it is not null.
        // Art alts are special and get merged, overwriting only conflicting alts
        public static Tileset operator +(Tileset a, Tileset b)
        {
            if (a == null) return b.Clone();
            
            // Create new tilset to return
            // Copy all initial values from a
            Tileset tileset = a.Clone();

            // Replace all values with those from b if they exist
            if (b.area_name != null)
                tileset.area_name = b.area_name;
            if (b.area_label_frame != 0)
                tileset.area_label_frame = b.area_label_frame;
            if (b.tile_graphics != null)
                tileset.tile_graphics = b.tile_graphics;
            if (b.overlay_graphics != null)
                tileset.overlay_graphics = b.overlay_graphics;
            if (b.background_graphics != null)
                tileset.background_graphics = b.background_graphics;
            if (b.foreground_graphics != null)
                tileset.foreground_graphics = b.foreground_graphics;
            if (b.palette != 0)
                tileset.palette = b.palette;
            if (b.area_type != null)
                tileset.area_type = b.area_type;
            if (b.toxic_timer != 0)
                tileset.toxic_timer = b.toxic_timer;
            if (b.platform_physics != null)
                tileset.platform_physics = b.platform_physics;
            if (b.water_physics != null)
                tileset.water_physics = b.water_physics;
            if (b.player_physics != null)
                tileset.player_physics = b.player_physics;
            if (b.lowgrav_physics != null)
                tileset.lowgrav_physics = b.lowgrav_physics;
            if (b.tile_particle_1 != null)
                tileset.tile_particle_1 = b.tile_particle_1;
            if (b.tile_particle_2 != null)
                tileset.tile_particle_2 = b.tile_particle_2;
            if (b.tile_particle_3 != null)
                tileset.tile_particle_3 = b.tile_particle_3;
            if (b.tile_particle_4 != null)
                tileset.tile_particle_4 = b.tile_particle_4;
            if (b.tile_particle_5 != null)
                tileset.tile_particle_5 = b.tile_particle_5;
            if (b.global_particle_1 != null)
                tileset.global_particle_1 = b.global_particle_1;
            if (b.global_particle_2 != null)
                tileset.global_particle_2 = b.global_particle_2;
            if (b.global_particle_3 != null)
                tileset.global_particle_3 = b.global_particle_3;
            if (b.decoration_1 != null)
                tileset.decoration_1 = b.decoration_1;
            if (b.decoration_2 != null)
                tileset.decoration_2 = b.decoration_2;
            if (b.decoration_3 != null)
                tileset.decoration_3 = b.decoration_3;
            if (b.npc_1 != null)
                tileset.npc_1 = b.npc_1;
            if (b.npc_2 != null)
                tileset.npc_2 = b.npc_2;
            if (b.npc_3 != null)
                tileset.npc_3 = b.npc_3;
            if (b.music != null)
                tileset.music = b.music;
            if (b.ambience != null)
                tileset.ambience = b.ambience;
            if (b.ambience_volume != 0)
                tileset.ambience_volume = b.ambience_volume;
            if (b.stop_previous_music != null)
                tileset.stop_previous_music = b.stop_previous_music;
            if (b.art_alts != null)
                tileset.art_alts = ArtAltsMerge(a.art_alts, b.art_alts);
            if (b.fx_shader != null)
                tileset.fx_shader = b.fx_shader;
            if (b.shaderMid != null)
                tileset.shaderMid = b.shaderMid;
            if (b.extras != null)
                tileset.extras = b.extras;

            return tileset;
        }
        public static List<string[]> ArtAltsMerge(in List<string[]> low, in List<string[]> high)
        {
            // Create a new list to return
            List<string[]> new_list = new List<string[]>();

            // Prevent errors by checking if the lists are null
            if (low == null)
            {
                if (high == null)
                    return new_list;
                else return high;
            }
            else if (high == null) return low;

            // Iterate over low-priority art alts
            for (int i = 0; i < low.Count(); i++)
            {
                // Set found flag to false
                bool found = false;

                // Iterate over high-priority art alts
                // We will do this again, but our goal in the first run through is only
                // to match those art alts that already exist in the low-priority list
                for (int j = 0; j < high.Count(); j++)
                {
                    // If a the low and high priority art alts are for the same art, use the higher priority alt
                    if (low[i][0] == high[j][0])
                    {
                        // add the higher priority alt to the new list of art alts
                        new_list.Add(high[j]);
                        // remove the alt from the high-priority list
                        // (we do this so that it does not reappear on the second run-through)
                        high.Remove(high[j]);
                        // Set the found flag to true
                        found = true;
                        break;
                    }
                }
                // If an alt for the same art was not found, use the low-priority alt
                if (!found) new_list.Add(low[i]);
            }
            // Iterate over high-priroity art alts again
            // This time we only have those that did not appear in the low-priority list
            foreach (var high_item in high)
            {
                // add every entry to the new list
                new_list.Add(high_item);
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
            if (ambience_volume != 0)
                sw.WriteLine($"ambience_volume {ambience_volume}");
            if (stop_previous_music != null)
                sw.WriteLine($"stop_previous_music {stop_previous_music}");
            if (art_alts != null)
            {
                sw.Write($"art_alts [");
                for (int i = 0; i < art_alts.Count(); i++)
                    sw.Write($"[{art_alts[i][0]},{art_alts[i][1]}]");
                sw.Write("]\n");
            }
            if (fx_shader != null)
                sw.WriteLine($"fx_shader {fx_shader}");
            if (shaderMid != null)
            {
                if (shaderMid.fx_shader_mid != null)
                    sw.WriteLine($"fx_shader_mid {shaderMid.fx_shader_mid}");
                if (shaderMid.midfx_graphics != null)
                    sw.WriteLine($"midfx_graphics {shaderMid.midfx_graphics}");
                if (shaderMid.midfx_layer != 0)
                    sw.WriteLine($"midfx_layer {shaderMid.midfx_layer}");
                if (shaderMid.shader_param != 0)
                    sw.WriteLine($"shader_param {shaderMid.shader_param}");
            }
            if (extras != null)
                sw.WriteLine(extras + "\n");
        }
    }
}
