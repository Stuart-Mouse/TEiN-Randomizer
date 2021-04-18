using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Security;
using System.Windows;

namespace TEiNRandomizer
{
    public class Tileset
    {
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

        private void GetArtAlts(RandomizerSettings settings)
        {
            ArtAlts = "";

            try
            {
                var doc = XDocument.Load("data/art_alts.xml");
                if (settings.AltLevel == AltLevels.Safe)
                {
                    foreach (var art in doc.Root.Element("safe").Elements())
                    {
                        var alts = Randomizer.ElementToArray(art);
                        ArtAlts += "[" + art.Name + "," + alts[Randomizer.myRNG.rand.Next(0, alts.Length)].Trim() + "]";
                    }
                }
                else if (settings.AltLevel == AltLevels.Extended)
                {
                    foreach (var art in doc.Root.Element("extended").Elements())
                    {
                        var alts = Randomizer.ElementToArray(art);
                        ArtAlts += "[" + art.Name + "," + alts[Randomizer.myRNG.rand.Next(0, alts.Length)].Trim() + "]";
                    }
                }
                else if (settings.AltLevel == AltLevels.Crazy)
                {
                    foreach (var set in doc.Root.Element("crazy").Elements())
                    {
                        var alts = Randomizer.ElementToArray(set);
                        foreach (var alt in alts)
                        {
                            ArtAlts += "[" + alt.Trim() + "," + alts[Randomizer.myRNG.rand.Next(0, alts.Length)].Trim() + "]";
                        }
                    }
                }
                else if (settings.AltLevel == AltLevels.Insane)
                {
                    var alts = Randomizer.ElementToArray(doc.Root.Element("insane"));
                    foreach (var alt in alts)
                    {
                        ArtAlts += "[" + alt.Trim() + "," + alts[Randomizer.myRNG.rand.Next(0, alts.Length)].Trim() + "]";
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
            return;
        }

        public Tileset(RandomizerSettings settings, bool isAreaTS)
        {
            try
            {
                var tile_graphicsPool = new string[] { };
                var overlay_graphicsPool = new string[] { };
                var background_graphicsPool = new string[] { };
                var particlePool = new string[] { };
                var shaderPool = new List<string> { };
                int numPalettes = 464;
                var musicPool = new string[] { };

                var doc = XDocument.Load("data/tilesets_pools.xml");    // open levelpool file

                numPalettes = Convert.ToInt32(doc.Root.Element("palettes").Value);
                tile_graphicsPool = Randomizer.ElementToArray(doc.Root.Element("tile_graphics"));
                overlay_graphicsPool = Randomizer.ElementToArray(doc.Root.Element("overlay_graphics"));
                particlePool = Randomizer.ElementToArray(doc.Root.Element("particles"));
                musicPool = Randomizer.ElementToArray(doc.Root.Element("music"));

                foreach (var shader in Randomizer.ShadersList)
                {
                    if (shader.Enabled)
                        shaderPool.Add(shader.Content);
                }

                Tile = ($"    tile_graphics { tile_graphicsPool[Randomizer.myRNG.rand.Next(0, tile_graphicsPool.Count())] }\n");
                Overlay = ($"    overlay_graphics { overlay_graphicsPool[Randomizer.myRNG.rand.Next(0, overlay_graphicsPool.Count())] }\n");
                Palette = ($"    palette { Randomizer.myRNG.rand.Next(1, numPalettes) }\n");
                Music = ($"    music { musicPool[Randomizer.myRNG.rand.Next(0, musicPool.Count())] }\n");

                if (Randomizer.myRNG.rand.Next(0, 2) == 0)  // set shader
                {
                    Shader = ($"    { shaderPool[Randomizer.myRNG.rand.Next(0, shaderPool.Count())] }\n");
                }
                Shader += "shader_param " + Randomizer.myRNG.rand.NextDouble();

                // set particles
                if(settings.DoParticles)
                {
                    var loop = Randomizer.myRNG.rand.Next(1, settings.MaxParticleEffects);
                    if (settings.GenerateCustomParticles)
                    {
                        for (int i = 0; i < loop; i++)
                        {
                            Particles += ("    global_particle_" + (i + 1).ToString() + $" { ParticleGenerator.GetParticle(settings) }\n");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < loop; i++)
                        {
                            Particles += ("    global_particle_" + (i + 1).ToString() + $" { particlePool[Randomizer.myRNG.rand.Next(0, particlePool.Count())] }\n");
                        }
                    }
                }

                // bgsolid for auto-refresh
                if (settings.AutoRefresh)
                    Extras += "background_graphics bgsolid\n";

                // generate Art Alts
                if (settings.AltLevel != AltLevels.None)
                    GetArtAlts(settings);

                // extras and physics
                if (settings.UseAreaTileset == isAreaTS)
                {
                    if (settings.DoNevermoreTilt && Randomizer.myRNG.rand.Next(0, 6) == 0 /*!(isAreaTS && !settings.UseAreaTileset)*/)
                    {
                        Extras += ($"    do_tilt true\n");
                    }
                    if (settings.DoExodusWobble && Randomizer.myRNG.rand.Next(0, 6) == 0 /*!(isAreaTS && !settings.UseAreaTileset)*/)
                    {
                        Extras += ($"    do_wobble true\n");
                    }
                    if (settings.DoPhysics)
                    {
                        if (settings.PlatformPhysics)
                            Extras += "    platform_physics " + Physics.PlatformPhysics() + "\n";
                        if (settings.WaterPhysics)
                            Extras += "    water_physics " + Physics.WaterPhysics() + "\n";
                        if (settings.PlayerPhysics)
                            Extras += "    player_physics " + Physics.PlayerPhysics() + "\n";
                        if (settings.LowGravPhysics)
                            Extras += "    lowgrav_physics " + Physics.LowGravPhysics() + "\n";
                    }
                }

                // create "all", which is just the entire tileset in one string
                All = Tile + "\n" + Overlay + "\n" + Particles + "\n" + Shader + "\n" + Palette + "\n" + Music + "\n" + Extras;
            }
            catch (Exception ex) { MessageBox.Show($"Error creating tileset. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
        }
    }
}
