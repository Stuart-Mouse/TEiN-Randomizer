using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    public static class Randomizer
    {

        public static MyRNG myRNG = new MyRNG();
        public static List<List<Level>> ChosenLevels;
        public static RandomizerSettings settings;
        public static List<Shader> ShadersList;
        //static string gameDir;
        //static int prevRuns = 0;

        static void FlipCSV(string path)
        {
            var arr = File.ReadAllLines(path);
            int length = 0;
            var file = new List<string[]>();
            foreach (var str in arr)
            {
                var line = str.Split(Convert.ToChar(","));
                if (line.Length > length)
                    length = line.Length;
                line = line.Reverse().ToArray();
                file.Add(line);
            }
            var newfile = new string[arr.Length];
            for (int j = 0; j < file.Count; j++)
            {
                string newline = "";
                int offset = length - file[j].Length;
                for (int i = 0; i < offset; i++)
                {
                    newline += ",";
                }
                for (int i = 0; i < file[j].Length; i++)
                {
                    newline += file[j][i] + ",";
                }
                newfile[j] = newline;
            }
            File.WriteAllLines(path, newfile);
        }
        public static List<Shader> GetShadersList()
        {
            ShadersList = new List<Shader>() { };

            var doc = XDocument.Load($"data/tilesets_pools.xml");    // open levelpool file
            foreach (var element in doc.Root.Element("shaders").Elements())
            {
                var shader = new Shader() { };

                shader.Name = element.Name.ToString();
                shader.Enabled = Convert.ToBoolean(element.Attribute("enabled").Value);
                shader.Content = element.Attribute("content").Value;

                ShadersList.Add(shader);
            }

            return ShadersList;
        }
        public static void SaveShadersList(List<Shader> ShadersList)
        {
            if (ShadersList != null)
            {
                var doc = XDocument.Load($"data/tilesets_pools.xml");    // open levelpool file
                foreach (var shader in ShadersList)
                {
                    doc.Root.Element("shaders").Element(shader.Name).Attribute("enabled").Value = Convert.ToString(shader.Enabled);
                }

                doc.Save($"data/tilesets_pools.xml");
            }
        }
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
        public static Int32[] ElementToArray(XElement element, bool y) // converts xml element value to a string array
        {
            string t_string = Convert.ToString(element.Value).Trim();
            var myArray = t_string.Split(Convert.ToChar("\n"));
            int[] intArray = new int[myArray.Count()];
            for (int i = 0; i < myArray.Count(); i++)
            {
                intArray[i] = Convert.ToInt32(myArray[i].Trim(Convert.ToChar("\t"), Convert.ToChar(" ")));
            }

            return intArray;
        }
        public static IEnumerable<PoolCategory> PoolLoader(string path)
        {
            var poolCats = new ObservableCollection<PoolCategory>();
            foreach (var dir in Directory.GetDirectories(path, "*",SearchOption.TopDirectoryOnly))
            {
                var folder = Path.GetFileNameWithoutExtension(dir);
                var cat = new PoolCategory() { Name = folder, Pools = new ObservableCollection<Pool>() { } };
                var tempList = new List<Pool>() { };
                string author = null;
                bool enabled = false;
                foreach (var file in Directory.GetFiles($"{path}/{folder}", "*.xml", SearchOption.TopDirectoryOnly))
                {
                    var pool = new Pool(Path.GetFileNameWithoutExtension(file), folder);    // pool creation done in Pool constructor
                    if (pool != null)
                    {
                        if (pool.Author != null)        // set category author
                        {
                            if (author == null)         // if author not already set, set it
                                author = pool.Author;
                            else if (author != pool.Author)  // if there is more than one pool author in the category, set to V.A.
                                author = "V.A.";
                        }
                        if (pool.Active == true)
                            enabled = true;
                        tempList.Add(pool);
                    }
                }
                cat.Enabled = enabled;
                cat.Author = author;
                cat.Pools = tempList.OrderBy(p => p.Order);
                poolCats.Add(cat);
            }
            return poolCats;
        }
        static IEnumerable<string> LoadRecents()
        {
            var recents = new List<string> { };
            var unCache = new List<XElement> { };
            if (!File.Exists("data/cache.xml"))
            {
                var doc = new XDocument { };
                doc.Add(new XElement("cache"));
                doc.Save("data/cache.xml");
            }

            var cachedoc = XDocument.Load("data/cache.xml");    // open cache file
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
            cachedoc.Save("data/cache.xml");

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
            cachedoc.Save("data/cache.xml");
        }
        static void Tilesets()
        {
            using (StreamWriter sw = File.CreateText(settings.GameDirectory + "data/tilesets.txt.append"))
            {
                sw.WriteLine("v { area_name \"Void\" area_label_frame 0 tile_graphics Tilehell overlay_graphics Overlaysairship background_graphics neverbg foreground_graphics none palette 0 do_tilt true fx_shader ripples fx_shader_mid heatwave2 midfx_graphics SolidBox global_particle_1 bgrain global_particle_2 embers global_particle_3 leaves decoration_1 CreepingMass decoration_2 OrbBlob2 decoration_3 OrbLarge2 ambience flesh.ogg art_alts[[OrbSmall, OrbBlob2][OrbLarge, OrbLarge2][ChainLink, None][ChainLink2, None]]}");
                
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

                        // Write Tileset info for level
                        if (settings.UseCommonTileset)
                        {
                            tileset = areatileset;
                            if (!settings.UseDefaultMusic)
                                sw.WriteLine(tileset.Music);
                        }
                        if (!settings.UseDefaultPalettes)
                            sw.WriteLine(tileset.Palette);
                        if (settings.DoTileGraphics)
                            sw.WriteLine(tileset.Tile);
                        if (settings.DoOverlays)
                            sw.WriteLine(tileset.Overlay);
                        if (settings.DoShaders)
                            sw.WriteLine(tileset.Shader);
                        if (settings.DoParticles)
                        {
                            sw.WriteLine("global_particle_1 None\nglobal_particle_2 None\nglobal_particle_3 None\n");
                            sw.WriteLine(tileset.Particles);
                        }
                            
                        sw.WriteLine(tileset.Extras);

                        // Art alts
                        sw.Write("art_alts[" + ChosenLevels[j][i].Art);
                        sw.Write(tileset.ArtAlts);
                        if (settings.UseCommonTileset)
                            sw.Write(areatileset.ArtAlts);
                        sw.WriteLine("]");

                        sw.WriteLine("    " + ChosenLevels[j][i].TSNeed);
                        sw.WriteLine(settings.AttachToTS);
                        sw.WriteLine("    }\n");
                    }
                    sw.WriteLine("}");
                }
            }
            File.Copy(settings.GameDirectory + "data/tilesets.txt.append", "data/debug/last_tilesets.txt", true);
        }
        static void CleanFolders()
        {
            try
            {
                foreach (var file in Directory.GetFiles(settings.GameDirectory + "data"))
                {
                    File.Delete(file);
                }
            }
            catch (DirectoryNotFoundException) { }
            Directory.CreateDirectory(settings.GameDirectory + "data");
            try
            {
                foreach (var file in Directory.GetFiles(settings.GameDirectory + "tilemaps"))
                {
                    File.Delete(file);
                }
            }
            catch (DirectoryNotFoundException) { }
            Directory.CreateDirectory(settings.GameDirectory + "tilemaps");
            
            Directory.CreateDirectory(settings.GameDirectory + "textures");
            File.Copy("data/palette.png", settings.GameDirectory + "textures/palette.png", true);
            Directory.CreateDirectory(settings.GameDirectory + "shaders");
            Directory.CreateDirectory(settings.GameDirectory + "swfs");        
            File.Copy("data/endnigh.swf", settings.GameDirectory + "swfs/endnigh.swf", true);    // copy swf
            foreach (var file in Directory.GetFiles("data/shaders"))
            {
                File.Copy(file, settings.GameDirectory + $"shaders/{Path.GetFileName(file)}", true);
            }
        }
        static void MapCSV()
        {
            System.IO.File.Copy("data/map_CLEAN.csv", settings.GameDirectory + "data/map.csv", true);
            using (StreamWriter sw = File.AppendText(settings.GameDirectory + "data/map.csv"))
            {
                for (int j = 0; j < settings.NumAreas; j++)
                {
                    for (int i = 0; i < settings.NumLevels; i++)
                    {
                        sw.Write($"v{j + 1}-{i + 1}.lvl,");
                    }
                }
                sw.Write("v-end.lvl");
            }
            if (settings.MirrorMode)
                FlipCSV(settings.GameDirectory + "data/map.csv");

        }
        static void LevelInfo()
        {
            for (int i = 0; i < settings.NumAreas; i++)
            {
                string areaname = null;

                //STRUCTURE
                //NAME's ( ADJECTIVE ) LOCATION ( of ( ADJECTIVE ) NOUN )
                //NAME's ( ADJECTIVE ) NOUN LOCATION

                var doc = XDocument.Load("data/area_names.xml");
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
                    if (myRNG.CoinFlip())
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

                using (StreamWriter sw = File.AppendText(settings.GameDirectory + "data/levelinfo.txt.append"))
                {
                    for (int j = 0; j < settings.NumLevels; j++)
                    {
                        sw.WriteLine("\"v" + Convert.ToString(i + 1) + "-" + Convert.ToString(j + 1) + "\" {name=\"" + areaname + Convert.ToString(j + 1) + "\" id=-1}");
                    }
                }
            }
        }
        static void TileMaps()
        {
            string[] baseLevels = { "1-1", "1-1x", "v-connect", "v-start", "v-end" };

            foreach (var level in baseLevels)
            {
                //File.Copy($"data/vtilemaps/The End is Nigh/{level}.lvl", settings.GameDirectory + $"tilemaps/{level}.lvl", true);
                var levelFile = LevelManip.Load($"data/vtilemaps/The End is Nigh/{level}.lvl");

                if (settings.MirrorMode)
                    LevelManip.FlipLevelH(ref levelFile);

                LevelManip.Save(levelFile, settings.GameDirectory + $"tilemaps/{level}.lvl");
            }


            for (int j = 0; j < settings.NumAreas; j++)
            {
                for (int i = 0; i < settings.NumLevels; i++)
                {
                    
                    var levelFile = LevelManip.Load($"data/vtilemaps/{ChosenLevels[j][i].Folder}/{ChosenLevels[j][i].Name}.lvl");
                    
                    if (ChosenLevels[j][i].CanReverse && myRNG.CoinFlip() || settings.MirrorMode)
                        LevelManip.FlipLevelH(ref levelFile);

                    if (settings.DoCorruptions)
                        ChosenLevels[j][i].TSNeed += Corruptors.CorruptLevel(ref levelFile);

                    LevelManip.Save(levelFile, settings.GameDirectory + $"tilemaps/v{j + 1}-{i + 1}.lvl");
                    
                    //File.Copy($"data/vtilemaps/{ChosenLevels[j][i].Folder}/{ChosenLevels[j][i].Name}.lvl", settings.GameDirectory + $"tilemaps/v{j + 1}-{i + 1}.lvl", true);
                }
            }
        }
        static void WriteDebug()
        {
            using (StreamWriter sw = File.CreateText("data/debug/last_levelnames.txt"))
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

        public static void Randomize(MainWindow mw)
        {

            //var allpools = mw.Pools;            // get pools from main window
            //gameDir = mw.EndIsNighPath;         // set game path (no longer used)
            ShadersList = mw.ShadersList;
            settings = mw.RSettings;
            //prevRuns = mw.PrevRuns;

            // This is an odd spot for this but I don't know where else to put it right now (sets areatype to glitch if dead racer mode is turned on)
            if (settings.DeadRacer) settings.AreaType = AreaTypes.glitch;


            var drawpool = new List<Level> { };     // make drawpool
            try
            {
                foreach (var cat in mw.PoolCats)
                {
                    if (cat.Enabled)
                    {
                        foreach (var pool in cat.Pools) // push levels in all active level pools into drawpool vector
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
                }
            }
            catch (Exception){MessageBox.Show( "Error creating drawpool.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw;}

            // cache loading no longer used
            //try
            //{
            //    if (settings.CacheRuns != 0)
            //    {
            //        var toRemove = LoadRecents();       // load cached levels
            //        foreach (var item in toRemove)      // actually remove them
            //        {
            //            for (int i = 0; i < drawpool.Count(); i++)
            //            {
            //                if (drawpool[i].Name == item)
            //                    drawpool.RemoveAt(i);
            //            }
            //        }
            //    }
            //}
            //catch (Exception){MessageBox.Show("Error loading cache.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw;}

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
                //if (settings.CacheRuns != 0) SaveRecents();      // add chosenlevels to cache
            }
            catch (Exception){MessageBox.Show("Error selecting levels or saving cache.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);throw;} 

            try{
                WriteDebug();   //debugging output chosen levels
                CleanFolders(); // clean up folders
            }
            catch (Exception ex) { MessageBox.Show("Error cleaning folders or printing debug. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try{
                LevelInfo();    // create levelinfo.txt
            }
            catch (Exception ex) { MessageBox.Show("Error creating levelinfo. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try
            {
                Worldmap.WriteWorldMap(settings);   // create worldmap.txt
            }
            catch (Exception ex) { MessageBox.Show("Error creating worldmap. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try
            {
                MapCSV();       // create map.csv
            }
            catch (Exception ex) { MessageBox.Show("Error creating map. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try{
                TileMaps();     // copy tilemaps to game folder
            }
            catch (Exception ex) { MessageBox.Show("Error copying tilemaps. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try
            {
                Tilesets();             // create tilesets.txt
            }
            catch (Exception ex) { MessageBox.Show("Error creating tilesets. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
        }
    }
}
