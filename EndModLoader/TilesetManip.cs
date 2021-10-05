using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public static class TilesetManip
    {
        static string[] TileGraphicsPool { get; set; }
        static string[] OverlayGraphicsPool { get; set; }
        static string[] BackgroundPool { get; set; }
        static string[] ParticlePool { get; set; }
        static string[] MusicPool { get; set; }
        public static int NumPalettes { get; set; }
        static List<string> ShaderPool { get; set; }
        static string[] AreaTypes { get; set; } = AppResources.AreaTypes;
        static TilesetManip()
        {
            var doc = XDocument.Load("data/tilesets_pools.xml");    // open levelpool file

            NumPalettes = 464;
            NumPalettes = Convert.ToInt32(doc.Root.Element("palettes").Value);
            TileGraphicsPool = Utility.ElementToArray(doc.Root.Element("tile_graphics"));
            OverlayGraphicsPool = Utility.ElementToArray(doc.Root.Element("overlay_graphics"));
            ParticlePool = Utility.ElementToArray(doc.Root.Element("particles"));
            MusicPool = Utility.ElementToArray(doc.Root.Element("music"));
        }
        public static void MakeShaderPool()
        {
            ShaderPool = new List<string> { };

            foreach (var shader in AppResources.ShadersList)
            {
                if (shader.Enabled)
                    ShaderPool.Add(shader.Content);
            }
        }

        public static string GetPalette() { return $"    palette { RNG.random.Next(1, NumPalettes) }"; }
        public static string GetTile() { return $"    tile_graphics { TileGraphicsPool[RNG.random.Next(0, TileGraphicsPool.Count())] }"; }
        public static string GetOverlay() { return $"    overlay_graphics { OverlayGraphicsPool[RNG.random.Next(0, OverlayGraphicsPool.Count())] }"; }

        private static string GetArtAlts(SettingsFile settings)
        {
            string ArtAlts = "";

            try
            {
                var doc = XDocument.Load("data/art_alts.xml");
                if (settings.AltLevel == "Safe")
                {
                    foreach (var art in doc.Root.Element("safe").Elements())
                    {
                        var alts = Utility.ElementToArray(art);
                        ArtAlts += "[" + art.Name + "," + alts[RNG.random.Next(0, alts.Length)].Trim() + "]";
                    }
                }
                else if (settings.AltLevel == "Extended")
                {
                    foreach (var art in doc.Root.Element("extended").Elements())
                    {
                        var alts = Utility.ElementToArray(art);
                        ArtAlts += "[" + art.Name + "," + alts[RNG.random.Next(0, alts.Length)].Trim() + "]";
                    }
                }
                else if (settings.AltLevel == "Crazy")
                {
                    foreach (var set in doc.Root.Element("crazy").Elements())
                    {
                        var alts = Utility.ElementToArray(set);
                        foreach (var alt in alts)
                        {
                            ArtAlts += "[" + alt.Trim() + "," + alts[RNG.random.Next(0, alts.Length)].Trim() + "]";
                        }
                    }
                }
                else if (settings.AltLevel == "Insane")
                {
                    var alts = Utility.ElementToArray(doc.Root.Element("insane"));
                    foreach (var alt in alts)
                    {
                        ArtAlts += "[" + alt.Trim() + "," + alts[RNG.random.Next(0, alts.Length)].Trim() + "]";
                    }
                    //ArtAlts += "[ChainLink, None][ChainLink2, None]";
                }
            }
            catch (Exception)
            {
                MessageBox.Show(
                        "Art Alt Error",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK
                    );
                throw;
            }
            return ArtAlts;
        }
        public static Tileset GetTileset(SettingsFile settings, bool isAreaTS)
        {
            Tileset tileset = new Tileset();
            try
            {
                // Select Tileset Info
                if (isAreaTS)
                {
                    //if (settings.DeadRacer) tileset.AreaType = "glitch";
                    if (settings.RandomizeAreaType)
                        tileset.AreaType = AreaTypes[RNG.random.Next(0, 5)];
                    else tileset.AreaType = settings.AreaType;
                }

                tileset.Tile = ($"    tile_graphics { TileGraphicsPool[RNG.random.Next(0, TileGraphicsPool.Count())] }\n");
                tileset.Overlay = ($"    overlay_graphics { OverlayGraphicsPool[RNG.random.Next(0, OverlayGraphicsPool.Count())] }\n");
                tileset.Palette = ($"    palette { RNG.random.Next(1, NumPalettes) }\n");
                tileset.Music = ($"    music { MusicPool[RNG.random.Next(0, MusicPool.Count())] }\n");

                if (RNG.random.Next(0, 2) == 0)  // set shader
                {
                    tileset.Shader = ($"    { ShaderPool[RNG.random.Next(0, ShaderPool.Count())] }\n");
                }
                tileset.Shader += "shader_param " + ((float)RNG.random.Next(0, 101) / 100);

                // set particles
                if (settings.DoParticles)
                {
                    var loop = RNG.random.Next(1, settings.MaxParticleEffects);
                    if (settings.GenerateCustomParticles)
                    {
                        for (int i = 0; i < loop; i++)
                        {
                            tileset.Particles += ("    global_particle_" + (i + 1).ToString() + $" { ParticleGenerator.GetParticle(settings) }\n");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < loop; i++)
                        {
                            tileset.Particles += ("    global_particle_" + (i + 1).ToString() + $" { ParticlePool[RNG.random.Next(0, ParticlePool.Count())] }\n");
                        }
                    }
                }

                // bgsolid for auto-refresh
                //if (settings.AutoRefresh || settings.LevelMerge)
                //    Extras += "background_graphics bgsolid\n";

                if (settings.LevelMerge)
                    tileset.Extras += "global_particle_1 None\nglobal_particle_2 None\nglobal_particle_3 None tile_particle_1 None tile_particle_2 None tile_particle_3 None tile_particle_4 None tile_particle_5 None\n";

                // generate Art Alts
                if (settings.AltLevel != "None")
                    GetArtAlts(settings);

                // extras and physics
                if (settings.UseAreaTileset == isAreaTS)
                {
                    if (settings.DoNevermoreTilt && RNG.random.Next(0, 6) == 0 /*!(isAreaTS && !settings.UseAreaTileset)*/)
                    {
                        tileset.DoTilt = ($"    do_tilt true\n");
                    }
                    if (settings.DoExodusWobble && RNG.random.Next(0, 6) == 0 /*!(isAreaTS && !settings.UseAreaTileset)*/)
                    {
                        tileset.DoWobble = ($"    do_wobble true\n");
                    }
                    if (settings.DoPhysics)
                    {
                        if (settings.PlatformPhysics)
                            tileset.Extras += "    platform_physics " + Physics.PlatformPhysics() + "\n";
                        if (settings.WaterPhysics)
                            tileset.Extras += "    water_physics " + Physics.WaterPhysics() + "\n";
                        if (settings.PlayerPhysics)
                            tileset.Extras += "    player_physics " + Physics.PlayerPhysics() + "\n";
                        if (settings.LowGravPhysics)
                            tileset.Extras += "    lowgrav_physics " + Physics.LowGravPhysics() + "\n";
                    }
                }

                // create "all", which is just the entire tileset in one string
                tileset.All = $"area_type {tileset.AreaType}\n" + tileset.Tile + "\n" + tileset.Overlay + "\n" + tileset.Particles + "\n" + tileset.Shader + "\n" + tileset.Palette + "\n" + tileset.Music + "\n" + tileset.Extras;
            }
            catch (Exception ex) { MessageBox.Show($"Error creating tileset. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }

            return tileset;
        }
    }
}
