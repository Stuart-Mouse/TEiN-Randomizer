using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public static class Randomizer
    {

        public static MyRNG myRNG = new MyRNG();
        public static List<List<Level>> ChosenLevels;
        static RandomizerSettings settings;
        static string gameDir;
        static int prevRuns = 0;

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = myRNG.rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static string[] ElementToArray(XElement element) // converts xml element value to a string array
        {
            string t_string = Convert.ToString(element.Value).Trim();
            var myArray = t_string.Split(Convert.ToChar("\n"));
            for (int i = 0; i < myArray.Count(); i++)
            {
                myArray[i] = myArray[i].Trim(Convert.ToChar("\t"), Convert.ToChar(" "));
            }

            return myArray;
        }
        public static IEnumerable<Pool> PoolLoader(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.xml", SearchOption.TopDirectoryOnly))
            {
                var pool = new Pool(Path.GetFileNameWithoutExtension(file));    // pool creation done in Pool constructor
                if (pool != null) yield return pool;
            }
        }
        static IEnumerable<string> LoadRecents()
        {
            var recents = new List<string> { };
            var unCache = new List<XElement> { };
            if (!File.Exists("cache.xml"))
            {
                var doc = new XDocument { };
                doc.Add(new XElement("cache"));
                doc.Save("cache.xml");
            }

            var cachedoc = XDocument.Load("cache.xml");    // open cache file
            foreach (var element in cachedoc.Root.Elements())
            {
                var t_num = int.Parse(element.Attribute("num").Value.ToString());
                recents.AddRange(ElementToArray(element));
                t_num++;
                element.Attribute("num").Value = (t_num).ToString();
                if (t_num > settings.CacheRuns)         // removing from element.Elements() does a yeild break, so the elements are added to a list to be removed later instead
                    unCache.Add(element);
            }
            foreach (var deleteMe in unCache)   // remove excess runs from cache
            {
                foreach (var element in cachedoc.Root.Elements())   // this may be a weird solution, but it seems to work so I'm going with it.
                {
                    if (element.Equals(deleteMe))   // since the only need here is to delete the one matching element, breaking from the foreach doesn't hurt
                        element.Remove();
                }
            }
            cachedoc.Save("cache.xml");

            // remove recently played levels
            var toRemove = new List<string> { };
            if (settings.RepeatTolerance == 0)
                toRemove = recents;
            else
            {
                for (int i = 0; i < recents.Count(); i++)   // build toRemove based on recents and repeatTolerance
                {
                    int matches = 0;
                    for (int j = 0; j < recents.Count(); j++)
                    {
                        if (recents[i] == recents[j] && i != j)
                        {
                            matches++;
                            recents.RemoveAt(j);
                            j--;
                        }
                    }
                    if (matches > settings.RepeatTolerance)
                        toRemove.Add(recents[i]);
                }
            }
            return toRemove;
        }
        static void SaveRecents()
        {
            var cachedoc = XDocument.Load("cache.xml");
            var newelement = new XElement("run");
            for (int j = 0; j < settings.NumAreas; j++)
            {
                for (int i = 0; i < settings.NumLevels; i++)
                {
                    newelement.SetAttributeValue("num", 1);
                    newelement.Value += "\n\t\t" + ChosenLevels[j][i].Name;
                }
            }
            newelement.Value += "\n  ";
            cachedoc.Root.Add(newelement);
            cachedoc.Save("cache.xml");
        }
        static void Tilesets()
        {
            using (StreamWriter sw = File.CreateText(gameDir + "data/tilesets.txt.append"))
            {
                sw.WriteLine("v { area_name \"Void\" area_label_frame 0 tile_graphics Tilehell overlay_graphics Overlaysairship background_graphics neverbg foreground_graphics none palette 2 do_tilt true fx_shader ripples fx_shader_mid heatwave2 midfx_graphics SolidBox global_particle_1 bgrain global_particle_2 embers global_particle_3 leaves decoration_1 CreepingMass ambience flesh.ogg art_alts [[OrbSmall, OrbBlob2][OrbLarge, OrbLarge2][ChainLink, None][ChainLink2, None]]}");
                
                for (int j = 0; j < settings.NumAreas; j++) // area loop
                {
                    var areatileset = new Tileset(settings, true) { };

                    sw.WriteLine("v" + (j + 1).ToString() + " {\n    area_name \"TEiN Randomizer\"\n    area_label_frame 0\n    background_graphics neverbg\n    area_type " + settings.AreaType.ToString() + "\n");
                    if (settings.UseCommonTileset || (settings.DoShaders && settings.DoParticles))
                        sw.WriteLine(areatileset.All + "art_alts[" + areatileset.ArtAlts + "]");

                    for (int i = 0; i < settings.NumLevels; i++) // level loop
                    {
                        var tileset = new Tileset(settings, false) { };

                        sw.WriteLine("    " + Convert.ToString(i + 1) + " {");
                        sw.WriteLine("    " + ChosenLevels[j][i].TSDefault);
                        if (!settings.UseCommonTileset)
                        {
                            if (!settings.UseDefaultMusic)
                            {
                                if (settings.MusicPerLevel)
                                    sw.WriteLine(tileset.Music);
                                else sw.WriteLine(areatileset.Music);
                            }
                            if (!settings.UseDefaultPalettes)
                            {
                                if (settings.PalettePerLevel)
                                    sw.WriteLine(tileset.Palette);
                                else sw.WriteLine(areatileset.Palette);
                            }
                            if (settings.DoTileGraphics)
                                sw.WriteLine(tileset.Tile);
                            if (settings.DoOverlays)
                                sw.WriteLine(tileset.Overlay);
                            if (settings.DoShaders)
                                sw.WriteLine(tileset.Shader);
                            if (settings.DoParticles)
                                sw.WriteLine(tileset.Particles);

                            // Art alts
                            sw.Write("art_alts[" + ChosenLevels[j][i].Art);
                            if (settings.UseCommonTileset)
                                sw.Write(areatileset.ArtAlts);
                            else
                                sw.Write(tileset.ArtAlts);
                            sw.WriteLine("]");

                            sw.WriteLine(tileset.Extras);
                        }
                        else sw.WriteLine(areatileset.All);

                        sw.WriteLine("    " + ChosenLevels[j][i].TSNeed);
                        sw.WriteLine(settings.AttachToTS);
                        sw.WriteLine("    }\n");
                    }
                    sw.WriteLine("}");
                }
            }
        }
        static void CleanFolders()
        {
            try
            {
                foreach (var file in Directory.GetFiles(gameDir + "data"))
                {
                    File.Delete(file);
                }
            }
            catch (DirectoryNotFoundException) { }
            Directory.CreateDirectory(gameDir + "data");
            try
            {
                foreach (var file in Directory.GetFiles(gameDir + "tilemaps"))
                {
                    File.Delete(file);
                }
            }
            catch (DirectoryNotFoundException) { }
            Directory.CreateDirectory(gameDir + "tilemaps");
            
            Directory.CreateDirectory(gameDir + "textures");
            File.Copy("palette.png", gameDir + "textures/palette.png", true);
            Directory.CreateDirectory(gameDir + "shaders");
            Directory.CreateDirectory(gameDir + "swfs");        
            File.Copy("endnigh.swf", gameDir + "swfs/endnigh.swf", true);    // copy swf
            foreach (var file in Directory.GetFiles("shaders"))
            {
                File.Copy(file, gameDir + $"shaders/{Path.GetFileName(file)}", true);
            }
        }
        static void MapCSV()
        {
            System.IO.File.Copy("map_CLEAN.csv", gameDir + "data/map.csv", true);
            using (StreamWriter sw = File.AppendText(gameDir + "data/map.csv"))
            {
                for (int j = 0; j < settings.NumAreas; j++)
                {
                    for (int i = 0; i < settings.NumLevels; i++)
                    {
                        sw.Write($"v{prevRuns + j + 1}-{i + 1}.lvl,");
                    }
                }
                sw.Write("v-end.lvl");
            }
        }
        static void LevelInfo()
        {
            for (int i = 0; i < settings.NumAreas; i++)
            {
                string areaname = null;

                //STRUCTURE
                //NAME's ( ADJECTIVE ) LOCATION ( of ( ADJECTIVE ) NOUN )
                //NAME's ( ADJECTIVE ) NOUN LOCATION

                var doc = XDocument.Load("area_names.xml");
                var name = Randomizer.ElementToArray(doc.Root.Element("name"));
                var location = Randomizer.ElementToArray(doc.Root.Element("location"));
                var adjective = Randomizer.ElementToArray(doc.Root.Element("adjective"));
                var noun = Randomizer.ElementToArray(doc.Root.Element("noun"));

                // create le funny name
                if (myRNG.rand.Next(0, 5) == 0)
                {
                    areaname += name[myRNG.rand.Next(0, name.Length)] + "s ";
                }
                if (myRNG.CoinFlip())
                {
                    if (myRNG.CoinFlip())
                        areaname += adjective[myRNG.rand.Next(0, adjective.Length)] + " ";
                    areaname += noun[myRNG.rand.Next(0, noun.Length)] + " ";
                    areaname += location[myRNG.rand.Next(0, location.Length)] + " ";
                }
                else
                {
                    areaname += location[myRNG.rand.Next(0, location.Length)] + " of ";
                    if (myRNG.CoinFlip())
                        areaname += adjective[myRNG.rand.Next(0, adjective.Length)] + " ";
                    areaname += noun[myRNG.rand.Next(0, noun.Length)] + " ";
                }

                using (StreamWriter sw = File.AppendText(gameDir + "data/levelinfo.txt.append"))
                {
                    for (int j = 0; j < settings.NumLevels; j++)
                    {
                        sw.WriteLine("\"v" + Convert.ToString(prevRuns + i + 1) + "-" + Convert.ToString(j + 1) + "\" {name=\"" + areaname + Convert.ToString(j + 1) + "\" id=-1}");
                    }
                }
            }
        }
        static void TileMaps()
        {
            File.Copy("vtilemaps/1-1.lvl", gameDir + "tilemaps/1-1.lvl", true);
            File.Copy("vtilemaps/1-1x.lvl", gameDir + "tilemaps/1-2.lvl", true);
            File.Copy("vtilemaps/v-connect.lvl", gameDir + "tilemaps/v-connect.lvl", true);
            File.Copy("vtilemaps/v-start.lvl", gameDir + "tilemaps/v-start.lvl", true);
            File.Copy("vtilemaps/v-end.lvl", gameDir + "tilemaps/v-end.lvl", true);

            for (int j = 0; j < settings.NumAreas; j++)
            {
                for (int i = 0; i < settings.NumLevels; i++)
                {
                    File.Copy($"vtilemaps/{ChosenLevels[j][i].Name}.lvl", gameDir + $"tilemaps/v{j + 1}-{i + 1}.lvl", true);
                }
            }
        }
        static void WriteDebug()
        {
            using (StreamWriter sw = File.CreateText("debug.txt"))
            {
                sw.WriteLine("Chosen Levels");
                for (int j = 0; j < settings.NumAreas; j++)
                {
                    for (int i = 0; i < settings.NumLevels; i++)
                    {
                        sw.WriteLine((j+1).ToString() + "-" + (i + 1).ToString() + ": " + ChosenLevels[j][i].Name);
                    }
                }
            }
        }

        public static void Randomize(MainWindow mw, bool analyzeMe)
        {
            
            var allpools = mw.Pools;            // get pools from main window
            gameDir = mw.EndIsNighPath;         // set game path
            settings = mw.RSettings;
            prevRuns = mw.PrevRuns;

            var drawpool = new List<Level> { };     // make drawpool
            try
            {
                foreach (var pool in allpools)          // push levels in all active level pools into drawpool vector
                {
                    if (pool.Active)
                    {
                        foreach (var level in pool.Levels)
                        {
                            drawpool.Add(level);
                        }
                    }
                }
            }
            catch (Exception){MessageBox.Show( "Error creating drawpool.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw;}

            try
            {
                if (settings.CacheRuns != 0)
                {
                    var toRemove = LoadRecents();       // load cached levels
                    foreach (var item in toRemove)      // actually remove them
                    {
                        for (int i = 0; i < drawpool.Count(); i++)
                        {
                            if (drawpool[i].Name == item)
                                drawpool.RemoveAt(i);
                        }
                    }
                }
            }
            catch (Exception){MessageBox.Show("Error loading cache.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw;}

            //for (int i = 0; i < mw.RSettings.NumShuffles; i++)  // shuffle drawpool
            //    Shuffle(drawpool);

            ChosenLevels = new List<List<Level>> { };           // initialize ChosenLevels

            try
            {
                for (int j = 0; j < mw.RSettings.NumAreas; j++)     // select levels
                {
                    var levels = new List<Level> { };
                    for (int i = 0; i < mw.RSettings.NumLevels; i++)
                    {
                        int selection = myRNG.rand.Next(0, drawpool.Count());
                        levels.Add(drawpool[selection]);
                        drawpool.RemoveAt(selection);
                    }
                    ChosenLevels.Add(levels);
                }
                if (settings.CacheRuns != 0) SaveRecents();      // add chosenlevels to cache
            }
            catch (Exception){MessageBox.Show("Error selecting levels or saving cache.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);throw;}

            if (analyzeMe)      // provide info to analyzer
            {
                foreach (var area in ChosenLevels)
                {
                    foreach (var level in area)
                    {
                        mw.AnalysisLevelList.Add(level);
                    }
                }
            }   

            if (!analyzeMe)     // skip this when analyzing to save time
            {
                try{
                    WriteDebug();   //debugging output chosen levels
                    CleanFolders(); // clean up folders
                }
                catch (Exception) { MessageBox.Show("Error cleaning folders or printing debug.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
                try{
                    LevelInfo();    // create levelinfo.txt
                }
                catch (Exception) { MessageBox.Show("Error creating levelinfo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
                try{
                    MapCSV();       // create map.csv
                }
                catch (Exception) { MessageBox.Show("Error creating map.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
                try{
                    TileMaps();     // copy tilemaps to game folder
                }
                catch (Exception){MessageBox.Show("Error copying tilemaps.","Error",MessageBoxButton.OK,MessageBoxImage.Error); throw;}
                
            }
            Tilesets();         // create tilesets.txt
        }
    }
}
