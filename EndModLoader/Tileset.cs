using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public struct Tileset
    {
        public string AreaType { get; set; }
        public string Tile { get; set; }
        public string Overlay { get; set; }
        public string Background { get; set; }
        public string Particles { get; set; }
        public string Shader { get; set; }
        public string Palette { get; set; }
        public string Music { get; set; }
        public string All { get; set; }
        public string Extras { get; set; }
        public string ArtAlts { get; set; }
        public string DoTilt { get; set; }
        public string DoWobble { get; set; }
    }

    class TilesetManip
    {
        static string[] TileGraphicsPool { get; set; }
        static string[] OverlayGraphicsPool { get; set; }
        static string[] BackgroundPool { get; set; }
        static string[] ParticlePool { get; set; }
        static string[] MusicPool { get; set; }
        static int NumPalettes { get; set; }
        static List<string> ShaderPool { get; set; }
        static ObservableCollection<string> AreaTypes { get; set; } = Randomizer.mainWindow.AreaTypes;

        static TilesetManip()
        {
            var doc = XDocument.Load("data/tilesets_pools.xml");    // open levelpool file

            NumPalettes         = 464;
            NumPalettes         = Convert.ToInt32(doc.Root.Element("palettes").Value);
            TileGraphicsPool    = Randomizer.ElementToArray(doc.Root.Element("tile_graphics"));
            OverlayGraphicsPool = Randomizer.ElementToArray(doc.Root.Element("overlay_graphics"));
            ParticlePool        = Randomizer.ElementToArray(doc.Root.Element("particles"));
            MusicPool           = Randomizer.ElementToArray(doc.Root.Element("music"));
            ShaderPool          = new List<string> { };

            foreach (var shader in Randomizer.ShadersList)
            {
                if (shader.Enabled)
                    ShaderPool.Add(shader.Content);
            }
        }
        private static string GetArtAlts(RandomizerSettings settings)
        {
            string ArtAlts = "";

            try
            {
                var doc = XDocument.Load("data/art_alts.xml");
                if (settings.AltLevel == "Safe")
                {
                    foreach (var art in doc.Root.Element("safe").Elements())
                    {
                        var alts = Randomizer.ElementToArray(art);
                        ArtAlts += "[" + art.Name + "," + alts[RNG.random.Next(0, alts.Length)].Trim() + "]";
                    }
                }
                else if (settings.AltLevel == "Extended")
                {
                    foreach (var art in doc.Root.Element("extended").Elements())
                    {
                        var alts = Randomizer.ElementToArray(art);
                        ArtAlts += "[" + art.Name + "," + alts[RNG.random.Next(0, alts.Length)].Trim() + "]";
                    }
                }
                else if (settings.AltLevel == "Crazy")
                {
                    foreach (var set in doc.Root.Element("crazy").Elements())
                    {
                        var alts = Randomizer.ElementToArray(set);
                        foreach (var alt in alts)
                        {
                            ArtAlts += "[" + alt.Trim() + "," + alts[RNG.random.Next(0, alts.Length)].Trim() + "]";
                        }
                    }
                }
                else if (settings.AltLevel == "Insane")
                {
                    var alts = Randomizer.ElementToArray(doc.Root.Element("insane"));
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
        public static Tileset GetTileset(RandomizerSettings settings, bool isAreaTS)
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
                }

                tileset.Tile    = ($"    tile_graphics { TileGraphicsPool[RNG.random.Next(0, TileGraphicsPool.Count())] }\n");
                tileset.Overlay = ($"    overlay_graphics { OverlayGraphicsPool[RNG.random.Next(0, OverlayGraphicsPool.Count())] }\n");
                tileset.Palette = ($"    palette { RNG.random.Next(1, NumPalettes) }\n");
                tileset.Music   = ($"    music { MusicPool[RNG.random.Next(0, MusicPool.Count())] }\n");

                if (RNG.random.Next(0, 2) == 0)  // set shader
                {
                    tileset.Shader = ($"    { ShaderPool[RNG.random.Next(0, ShaderPool.Count())] }\n");
                }
                tileset.Shader += "shader_param " + (RNG.random.Next(0, 101) / 10);

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
                tileset.All = tileset.AreaType + "\n" + tileset.Tile + "\n" + tileset.Overlay + "\n" + tileset.Particles + "\n" + tileset.Shader + "\n" + tileset.Palette + "\n" + tileset.Music + "\n" + tileset.Extras;
            }
            catch (Exception ex) { MessageBox.Show($"Error creating tileset. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }

            return tileset;
        }
    }

}
