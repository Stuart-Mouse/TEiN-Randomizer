using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;


namespace TEiNRandomizer
{
    public static partial class Randomizer
    {
        // CONSTRUCTOR
        static Randomizer()
        {
            LoadNPCs();
            LoadFunnyNames();
        }


        // MEMBERS

        public static ObservableCollection<LevelPoolCategory> LevelPoolCategories = AppResources.LevelPoolCategories;
        public static ObservableCollection<PiecePool> PiecePools = AppResources.PiecePools;

        static List<List<Level>> ChosenLevels;
        static List<Level> DrawPool;

        public static string saveDir;         // The save directory is determined when starting the randomize function and stored. (Will select between game directory and save directory.)
        public static SettingsFile settings = AppResources.MainSettings;  // a copy of the settings is stored so that all of the functions within this class can read it.
        public static MainWindow mainWindow;  // The Main Window's info is also stored, so that it can be referenced when needed.

        public static List<string> AreaTypes;

        // NPC Stuff
        public static string[] NPCMovieClips;
        public static string[] NPCSoundIDs;
        public static List<string> NPCTexts;

        // Funny Name Stuff
        public static string[] LINouns;
        public static string[] LIAdjectives;
        public static string[] LINames;
        public static string[] LILocations;


        // METHODS

        // resource loading functions
        static void LoadFunnyNames()
        {
            var doc = XDocument.Load("data/area_names.xml");
            LINames = Utility.ElementToArray(doc.Root.Element("name"));
            LILocations = Utility.ElementToArray(doc.Root.Element("location"));
            LIAdjectives = Utility.ElementToArray(doc.Root.Element("adjective"));
            LINouns = Utility.ElementToArray(doc.Root.Element("noun"));
        }
        static void LoadNPCs()
        {
            var doc = XDocument.Load($"data/npcs.xml");    // open npcs file
            NPCMovieClips = Utility.ElementToArray(doc.Root.Element("movieclips"));
            NPCSoundIDs = Utility.ElementToArray(doc.Root.Element("soundids"));
            NPCTexts = new List<string>();

            foreach (XElement item in doc.Root.Element("text").Elements())
            {
                NPCTexts.Add(item.Value);
            }
        }

        // Randomize() subfunctions (separated out for readability)
        static void NPCs()
        {
            using (StreamWriter sw = File.CreateText(saveDir + "data/npcs.txt.append"))
            {
                for (int j = 0; j < settings.NumAreas; j++) // area loop
                {
                    for (int i = 0; i < settings.NumLevels; i++) // level loop
                    {
                        sw.WriteLine($"NPC{j}-{i} {{");
                        sw.WriteLine($"\tmovieclip {NPCMovieClips[RNG.random.Next(0, NPCMovieClips.Length)]}");
                        sw.WriteLine($"\tsound_id {NPCSoundIDs[RNG.random.Next(0, NPCSoundIDs.Length)]}");
                        sw.WriteLine($"\ttext [");
                        sw.WriteLine(NPCTexts[RNG.random.Next(0, NPCTexts.Count())]);
                        sw.WriteLine("\t]\n}");
                    }
                    sw.WriteLine($"NPCv{j + 1} {{");
                    sw.WriteLine($"\tmovieclip {NPCMovieClips[RNG.random.Next(0, NPCMovieClips.Length)]}");
                    sw.WriteLine($"\tsound_id {NPCSoundIDs[RNG.random.Next(0, NPCSoundIDs.Length)]}");
                    sw.WriteLine($"\ttext [");
                    sw.WriteLine(NPCTexts[RNG.random.Next(0, NPCTexts.Count())].Replace("PLAYERNAME", settings.UserName));
                    sw.WriteLine("\t]\n}");
                }
            }
        }
        public static string GetFunnyName()
        {
            //STRUCTURE
            //NAME's ( ADJECTIVE ) LOCATION ( of ( ADJECTIVE ) NOUN )
            //NAME's ( ADJECTIVE ) NOUN LOCATION

            // create le funny name
            string areaname = "";
            if (RNG.random.Next(0, 5) == 0)
            {
                areaname += LINames[RNG.random.Next(0, LINames.Length)] + "s ";
            }
            if (RNG.CoinFlip())
            {
                if (RNG.CoinFlip())
                    areaname += LIAdjectives[RNG.random.Next(0, LIAdjectives.Length)] + " ";
                if (RNG.CoinFlip())
                    areaname += LINouns[RNG.random.Next(0, LINouns.Length)] + " ";
                areaname += LILocations[RNG.random.Next(0, LILocations.Length)];
            }
            else
            {
                areaname += LILocations[RNG.random.Next(0, LILocations.Length)] + " of ";
                if (RNG.CoinFlip())
                    areaname += LIAdjectives[RNG.random.Next(0, LIAdjectives.Length)] + " ";
                areaname += LINouns[RNG.random.Next(0, LINouns.Length)];
            }
            return areaname;
        }
        static void Tilesets()
        {
            using (StreamWriter sw = File.CreateText(saveDir + "data/tilesets.txt.append"))
            {
                sw.WriteLine("v { area_name \"Void\" area_label_frame 0 tile_graphics Tilehell overlay_graphics Overlaysairship background_graphics neverbg foreground_graphics none palette 0 do_tilt true fx_shader ripples fx_shader_mid heatwave2 midfx_graphics SolidBox global_particle_1 bgrain global_particle_2 embers global_particle_3 leaves decoration_1 CreepingMass decoration_2 OrbBlob2 decoration_3 OrbLarge2 ambience flesh.ogg art_alts[[OrbSmall, OrbBlob2][OrbLarge, OrbLarge2][ChainLink, None][ChainLink2, None]]");
                for (int j = 0; j < settings.NumAreas; j++) // npc levels
                {
                    sw.WriteLine($"npc{j + 1} {{ npc_1 NPCv{j + 1} }}");
                }
                sw.WriteLine("}");

                for (int j = 0; j < settings.NumAreas; j++) // area loop
                {
                    var areatileset = TilesetManip.GetTileset(settings, true);

                    AreaTypes.Add(areatileset.AreaType);

                    sw.WriteLine("v" + (j + 1).ToString() + $" {{\n    area_name \"TEiN Randomizer\"\n    area_label_frame 0\n    background_graphics neverbg\n    area_type {areatileset.AreaType}\n");
                    if (settings.UseAreaTileset || (settings.DoShaders && settings.DoParticles))
                        sw.WriteLine(areatileset.All + "art_alts[" + areatileset.ArtAlts + "]");

                    for (int i = 0; i < settings.NumLevels; i++) // level loop
                    {
                        var tileset = TilesetManip.GetTileset(settings, false);
                        sw.WriteLine("    " + Convert.ToString(i + 1) + " {");
                        sw.WriteLine("    " + ChosenLevels[j][i].TSDefault);

                        sw.WriteLine($"# Level Name: {ChosenLevels[j][i].Name}");

                        // Write Tileset info for level
                        if (settings.UseAreaTileset)
                            tileset = areatileset;
                        if (settings.DoMusic)
                            sw.WriteLine(areatileset.Music);
                        if (settings.DoPalettes)
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

                        sw.WriteLine(tileset.DoTilt);
                        sw.WriteLine(tileset.DoWobble);
                        sw.WriteLine(tileset.Extras);

                        // Art alts
                        sw.Write("     art_alts[" + ChosenLevels[j][i].Art);
                        sw.Write(tileset.ArtAlts);
                        //if (settings.UseCommonTileset)
                        //    sw.Write(areatileset.ArtAlts);
                        sw.WriteLine("     ]");

                        sw.WriteLine("     # TS Needs");
                        sw.WriteLine("    " + ChosenLevels[j][i].TSNeed);

                        sw.WriteLine(settings.AttachToTS);

                        // NPCs
                        sw.WriteLine($"     npc_1 NPC{j}-{i}\n");
                        sw.WriteLine("    }\n");
                    }
                    sw.WriteLine("}");
                }
            }
            File.Copy(saveDir + "data/tilesets.txt.append", "data/debug/last_tilesets.txt", true);
        }
        static void CleanFolders()
        {

            Console.WriteLine("del data");
            try
            {
                foreach (var file in Directory.GetFiles(saveDir + "data"))
                {
                    File.Delete(file);
                }
            }
            catch (DirectoryNotFoundException) { }
            Directory.CreateDirectory(saveDir + "data");
            Console.WriteLine("del tilemaps");
            try
            {
                foreach (var file in Directory.GetFiles(saveDir + "tilemaps"))
                {
                    File.Delete(file);
                }
            }
            catch (DirectoryNotFoundException) { }
            Directory.CreateDirectory(saveDir + "tilemaps"); //Console.WriteLine("tilemaps");
            Directory.CreateDirectory(saveDir + "textures"); //Console.WriteLine("textures");
            File.Copy("data/palette.png", saveDir + "textures/palette.png", true); //Console.WriteLine("palette");
            Directory.CreateDirectory(saveDir + "shaders"); //Console.WriteLine("shaders");
            Directory.CreateDirectory(saveDir + "swfs"); //Console.WriteLine("swfs");
            Directory.CreateDirectory(saveDir + "data/platform_physics");
            Directory.CreateDirectory(saveDir + "data/water_physics");
            Directory.CreateDirectory(saveDir + "data/lowgrav_physics");
            Directory.CreateDirectory(saveDir + "data/player_physics");
            foreach (var file in Directory.GetFiles("data/swfs"))
            {
                File.Copy(file, saveDir + $"swfs/{Path.GetFileName(file)}", true);
            }
            File.Copy("data/swfs/endnigh.swf", saveDir + "swfs/endnigh.swf", true); Console.WriteLine("swf");
            foreach (var file in Directory.GetFiles("data/shaders"))
            {
                File.Copy(file, saveDir + $"shaders/{Path.GetFileName(file)}", true);
            }
            //Console.WriteLine("shaders");
        }
        static void MapCSV()
        {
            System.IO.File.Copy("data/map_CLEAN.csv", saveDir + "data/map.csv", true);
            using (StreamWriter sw = File.AppendText(saveDir + "data/map.csv"))
            {
                for (int j = 0; j < settings.NumAreas; j++)
                {
                    for (int i = 0; i < settings.NumLevels; i++)
                    {
                        sw.Write($"v{j + 1}-{i + 1}.lvl,");
                    }
                    if (j != settings.NumAreas-1) sw.Write($"v-npc{j + 1}.lvl,");
                }
                sw.Write("v-end.lvl");
            }
            /*if (settings.MirrorMode)
                FlipCSV(saveDir + "data/map.csv");*/

        }
        static void LevelInfo()
        {
            for (int i = 0; i < settings.NumAreas; i++)
            {
                string areaname = GetFunnyName();

                using (StreamWriter sw = File.AppendText(saveDir + "data/levelinfo.txt.append"))
                {
                    for (int j = 0; j < settings.NumLevels; j++)
                    {
                        sw.WriteLine("\"v" + Convert.ToString(i + 1) + "-" + Convert.ToString(j + 1) + "\" {name=\"" + areaname + " " + Convert.ToString(j + 1) + "\" id=-1}");
                    }
                }
            }
        }
        static void TileMaps()
        {
            string[] baseLevels = { "1-1", "1-1x", "v-connect", "v-start", "v-end" };
            var npclevel = LevelManip.Load($"data/tilemaps/The End is Nigh/v-npc.lvl");

            foreach (var level in baseLevels)
            {
                //File.Copy($"data/vtilemaps/The End is Nigh/{level}.lvl", saveDir + $"tilemaps/{level}.lvl", true);
                var levelFile = LevelManip.Load($"data/tilemaps/The End is Nigh/{level}.lvl");

                if (settings.MirrorMode)
                    LevelManip.FlipLevelH(ref levelFile);

                LevelManip.Save(levelFile, saveDir + $"tilemaps/{level}.lvl");
            }

            for (int j = 0; j < settings.NumAreas; j++)
            {
                for (int i = 0; i < settings.NumLevels; i++)
                {
                    var level = ChosenLevels[j][i];
                    var levelFile = LevelManip.Load($"data/tilemaps/{level.Folder}/{level.Name}.lvl");

                    if (/*level.CanReverse && RNG.CoinFlip() ||*/ settings.MirrorMode)
                        LevelManip.FlipLevelH(ref levelFile);

                    if (settings.DoCorruptions)
                        level.TSNeed += LevelCorruptors.CorruptLevel(ref levelFile);

                    LevelManip.Save(levelFile, saveDir + $"tilemaps/v{j + 1}-{i + 1}.lvl");
                }
                LevelManip.Save(npclevel, saveDir + $"tilemaps/v-npc{j + 1}.lvl");
            }
        }
        static string SaveRunPrompt()
        {
            string title       = GetFunnyName();
            string author      = $"Seed: {mainWindow.GameSeed.ToString()}";
            string description = "A randomized world!";
            string version     = System.DateTime.Now.ToString();

            TextWindow inputWindow = new TextWindow("Give this world a name: ", title);
            if (inputWindow.ShowDialog() == true)
                title = inputWindow.Result;

            string dir = mainWindow.SavedRunsPath + $"/{title}";

            inputWindow = new TextWindow("Give this world a short description: ", description);
            if (inputWindow.ShowDialog() == true)
                description = inputWindow.Result;

            if (!File.Exists(dir + ".zip"))
            {
                Directory.CreateDirectory(dir);
                using (StreamWriter sw = File.CreateText($"{dir}/meta.xml"))
                {
                    sw.Write("<mod>\n  <title>");
                    sw.Write(title);
                    sw.Write("</title>\n  <author>");
                    sw.Write(author);
                    sw.Write("</author>\n  <description>");
                    sw.Write(description);
                    sw.Write("</description>\n  <version>");
                    sw.Write(version);
                    sw.Write("</version>\n</mod>");
                }
                return dir + "/";
            }
            else { MessageBox.Show($"A saved run with this name already exists. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return null; }
        }
        static List<Level> MakeDrawPool()
        {
            DrawPool = new List<Level> { };     // make drawpool
            try
            {
                foreach (var cat in mainWindow.PoolCats)
                {
                    if (cat.Enabled)
                    {
                        foreach (var pool in cat.Pools) // push levels in all active level pools into drawpool vector
                        {
                            if (pool.Active)
                            {
                                foreach (var level in pool.Levels)
                                {
                                    DrawPool.Add(level);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { MessageBox.Show("Error creating drawpool.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            return DrawPool;
        }
        static void ChooseLevels()
        {
            ChosenLevels = new List<List<Level>> { };   // initialize ChosenLevels
            try
            {
                for (int j = 0; j < settings.NumAreas; j++)     // select levels
                {
                    var levels = new List<Level> { };
                    for (int i = 0; i < settings.NumLevels; i++)
                    {
                        int selection = RNG.random.Next(0, DrawPool.Count());
                        levels.Add(DrawPool[selection]);
                        DrawPool.RemoveAt(selection);
                    }
                    ChosenLevels.Add(levels);
                }
                //if (settings.CacheRuns != 0) SaveRecents();      // add chosenlevels to cache
            }
            catch (Exception) { Console.WriteLine("Error selecting levels or saving cache."); MessageBox.Show("Error selecting levels or saving cache.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
        }

        public static void Randomize(MainWindow mw, string args = null)
        {
            //ShadersList = mw.ShadersList;
            settings = mw.RSettings;
            mainWindow = mw;

            AreaTypes = new List<string>();

            saveDir = settings.GameDirectory;
            if (args == "savemod") saveDir = SaveRunPrompt();
            if (saveDir == null) return;

            TilesetManip.MakeShaderPool();

            // make the draw pool based on which level pools are enabled
            MakeDrawPool();
            // make randomized selections from drawpool
            ChooseLevels();
            
            // The rest of the randomization process is delegated to the functions below.
            try { CleanFolders(); }   catch (Exception ex) { Console.WriteLine($"Error cleaning folders or printing debug. Exception {ex}");  MessageBox.Show($"Error cleaning folders or printing debug. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try { LevelInfo(); }      catch (Exception ex) { Console.WriteLine($"Error creating levelinfo. Exception {ex}");                  MessageBox.Show($"Error creating levelinfo. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try { MapCSV(); }         catch (Exception ex) { Console.WriteLine($"Error creating map. Exception {ex}");                        MessageBox.Show($"Error creating map. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try { TileMaps(); }       catch (Exception ex) { Console.WriteLine($"Error copying tilemaps. Exception {ex}");                    MessageBox.Show($"Error copying tilemaps. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try { Tilesets(); }       catch (Exception ex) { Console.WriteLine($"Error creating tilesets. Exception {ex}");                   MessageBox.Show($"Error creating tilesets. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try { NPCs(); }           catch (Exception ex) { Console.WriteLine($"Error creating npcs. Exception {ex}");                       MessageBox.Show($"Error creating tilesets. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            try { Worldmap.WriteWorldMap(); } catch (Exception ex) { Console.WriteLine($"Error creating worldmap. Exception {ex}");           MessageBox.Show($"Error creating worldmap. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }

            Console.WriteLine(mw.GameSeed);

            //if (args == "savemod")    // this is commented out bc mods will not save to zip for now
            //{
            //    saveDir = saveDir.Remove(saveDir.Length - 1);
            //    ZipFile.CreateFromDirectory(saveDir, saveDir + ".zip");
            //    Directory.Delete(saveDir, true);
            //}

        }
        
    }
}
