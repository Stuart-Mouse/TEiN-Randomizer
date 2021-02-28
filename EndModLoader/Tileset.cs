﻿using System;
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
                foreach (var item in Randomizer.ElementToArray(doc.Root.Element("safe")))
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
                    ArtAlts += "[ChainLink, None][ChainLink2, None]";
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


        public Tileset(RandomizerSettings settings, bool isMainTS)
        {
            var tile_graphicsPool = new string[] { };
            var overlay_graphicsPool = new string[] { };
            var background_graphicsPool = new string[] { };
            var particlePool = new string[] { };
            var shaderPool = new List<string> { };
            var palettePool = new string[] { };
            var musicPool = new string[] { };

            var doc = XDocument.Load("data/tilesets_pools.xml");    // open levelpool file
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == "tile_graphics")
                {
                    tile_graphicsPool = Randomizer.ElementToArray(element);
                }
                else if (element.Name == "overlay_graphics")
                {
                    overlay_graphicsPool = Randomizer.ElementToArray(element);
                }
                //else if (element.Name == "background_graphics")
                //{
                //    foreach (var element2 in element.Elements())
                //    {
                //        if (element.Name == bgType)
                //            background_graphicsPool = ElementToArray(element);
                //    }
                //}
                else if (element.Name == "particles")
                {
                    particlePool = Randomizer.ElementToArray(element);
                }
                else if (element.Name == "shaders")
                {
                    foreach (var element2 in element.Elements())
                    {
                        shaderPool.Add(element2.Attribute("content").Value);
                    }
                }
                //else if (element.Name == "palette")
                //{
                //    palettePool = Randomizer.ElementToArray(element);
                //}
                else if (element.Name == "music")
                {
                    musicPool = Randomizer.ElementToArray(element);
                }
            }

            // shuffle pools
            //for (int i = 0; i < settings.NumShuffles; i++)
            //{
            //    Randomizer.Shuffle(tile_graphicsPool);
            //    Randomizer.Shuffle(overlay_graphicsPool);
            //    //Shuffle(background_graphicsPool);
            //    Randomizer.Shuffle(particlePool);
            //    Randomizer.Shuffle(shaderPool);
            //    Randomizer.Shuffle(palettePool);
            //    Randomizer.Shuffle(musicPool);
            //}

            Tile = ($"    tile_graphics { tile_graphicsPool[Randomizer.myRNG.rand.Next(0, tile_graphicsPool.Count())] }\n");
            Overlay = ($"    overlay_graphics { overlay_graphicsPool[Randomizer.myRNG.rand.Next(0, overlay_graphicsPool.Count())] }\n");
            //Palette = ($"    palette { palettePool[Randomizer.myRNG.rand.Next(0, palettePool.Count())] }\n");
            Palette = ($"    palette { Randomizer.myRNG.rand.Next(1, 464) }\n");
            Music = ($"    music { musicPool[Randomizer.myRNG.rand.Next(0, musicPool.Count())] }\n");
            //tileset += ($"    background_graphics { background_graphicsPool[rng.Next(0, tile_graphicsPool.Count())] }\n");

            if (Randomizer.myRNG.rand.Next(0, 2) == 0)  // set shader
            {
                Shader = ($"    { shaderPool[Randomizer.myRNG.rand.Next(0, shaderPool.Count())] }\n");
            }
            Shader += "shader_param 0." + Randomizer.myRNG.rand.Next(0, 99);

            var loop = Randomizer.myRNG.rand.Next(1, 4);    // set particles
            for (int i = 0; i < loop; i++)
            {
                Particles += ("    global_particle_" + (i + 1).ToString() + $" { particlePool[Randomizer.myRNG.rand.Next(0, particlePool.Count())] }\n");
            }

            if (settings.DoNevermoreTilt && Randomizer.myRNG.rand.Next(0, 6) == 0 && !(isMainTS && !settings.UseCommonTileset))
            {
                Extras += ($"    do_tilt true\n");
            }
            if (settings.DoExodusWobble && Randomizer.myRNG.rand.Next(0, 6) == 0 && !(isMainTS && !settings.UseCommonTileset))
            {
                Extras += ($"    do_wobble true\n");
            }

            // bgsolid for auto-refresh
            if (settings.AutoRefresh)
                Extras += "background_graphics bgsolid\n";

            // generate Art Alts
            if (settings.AltLevel != AltLevels.None)
                GetArtAlts(settings);

            // create "all", which is just the entire tileset in one string
            All = Tile + "\n" + Overlay + "\n" + Particles + "\n" + Shader + "\n" + Palette + "\n" + Music + "\n" + Extras;


        }
    }
}
