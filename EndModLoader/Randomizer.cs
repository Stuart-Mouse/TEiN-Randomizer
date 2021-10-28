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
        public static SettingsFile Settings = AppResources.MainSettings;  // a copy of the settings is stored so that all of the functions within this class can read it.

        public static List<string> AreaTypesChosen;

        public static List<MapArea> MapAreasList;

        // NPC Stuff
        public static string[] NPCMovieClips;
        public static string[] NPCSoundIDs;
        public static List<string[]> NPCTexts;

        // Funny Name Stuff
        public static string[] LINouns;
        public static string[] LIAdjectives;
        public static string[] LINames;
        public static string[] LILocations;


        // METHODS

        // resource loading functions
        static void LoadFunnyNames()
        {
            var gon         = GonObject.Load("data/text/area_names.gon");
            LINames         = GonObject.Manip.GonToStringArray(gon["name"]);
            LILocations     = GonObject.Manip.GonToStringArray(gon["location"]);
            LIAdjectives    = GonObject.Manip.GonToStringArray(gon["adjective"]);
            LINouns         = GonObject.Manip.GonToStringArray(gon["noun"]);
        }
        static void LoadNPCs()
        {
            var gon = GonObject.Load($"data/text/npcs.gon");    // open npcs file
            NPCMovieClips = GonObject.Manip.GonToStringArray(gon["movieclips"]);
            NPCSoundIDs = GonObject.Manip.GonToStringArray(gon["movieclips"]);
            NPCTexts = new List<string[]>();

            var text = gon["text"];
            for (int i = 0; i < text.Size(); i++)
            {
                NPCTexts.Add(GonObject.Manip.GonToStringArray(text[i]));
            }
        }

        // Randomize() subfunctions (separated out for readability)
        static void NPCs()
        {
            using (StreamWriter sw = File.CreateText(saveDir + "data/npcs.txt.append"))
            {
                for (int j = 0; j < Settings.NumAreas; j++) // area loop
                {
                    // Write npc data for every level
                    for (int i = 0; i < Settings.NumLevels; i++) // level loop
                    {
                        sw.WriteLine($"NPC{j}-{i} {{");
                        sw.WriteLine($"\tmovieclip {NPCMovieClips[RNG.random.Next(0, NPCMovieClips.Length)]}");
                        sw.WriteLine($"\tsound_id {NPCSoundIDs[RNG.random.Next(0, NPCSoundIDs.Length)]}");
                        sw.WriteLine($"\ttext [");
                        var lines = NPCTexts[RNG.random.Next(0, NPCTexts.Count())];
                        for (int k = 0; k < lines.Count(); k++)
                        {
                            sw.WriteLine($"\"{lines[k].Replace("PLAYERNAME", Settings.UserName)}\"");
                        }
                        sw.WriteLine("\t]\n}");
                    }
                    // write npc data for transition levels
                    {
                        sw.WriteLine($"NPCv{j + 1} {{");
                        sw.WriteLine($"\tmovieclip {NPCMovieClips[RNG.random.Next(0, NPCMovieClips.Length)]}");
                        sw.WriteLine($"\tsound_id {NPCSoundIDs[RNG.random.Next(0, NPCSoundIDs.Length)]}");
                        sw.WriteLine($"\ttext [");
                        var lines = NPCTexts[RNG.random.Next(0, NPCTexts.Count())];
                        for (int i = 0; i < lines.Count(); i++)
                        {
                            sw.WriteLine($"\"{lines[i].Replace("PLAYERNAME", Settings.UserName)}\"");
                        }
                        sw.WriteLine("\t]\n}");
                    }
                }
            }
        }
        public static string GetFunnyName()
        {
            // STRUCTURE:
            // NAME's ( ADJECTIVE ) LOCATION ( of ( ADJECTIVE ) NOUN )
            // NAME's ( ADJECTIVE ) NOUN LOCATION

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
            // Create a StreamWriter object for writing to the tilesets.txt file
            using (StreamWriter sw = File.CreateText("data/tilesets.txt.append"))
            {
                // Loop over map areas
                for (int i = 0; i < MapAreasList.Count(); i++)
                {
                    // Get reference to MapArea #i in list 
                    MapArea mapArea = MapAreasList[i];

                    // Set areaTileset to Map Area's tileset
                    Tileset areaTileset = mapArea.Tileset;

                    // Write the area's tileset to the file
                    sw.WriteLine($"{mapArea.Name} {{");
                    areaTileset.WriteTileset(sw);

                    // Loop over levels in area
                    for (int j = 0; j < mapArea.Levels.Count(); j++)
                    {
                        // Get reference to Level #i in area 
                        Level level = mapArea.Levels[j];

                        // Calculate the final tileset
                        // The tilesets are added in order of priority, from lowest to highest
                        Tileset levelTileset = (level.TSDefault + areaTileset) + level.TSNeed;

                        // Write level tileset to the file
                        sw.WriteLine($"{level.Name} {{");
                        levelTileset.WriteTileset(sw);
                        sw.WriteLine("}");
                    }

                    // write closing bracket for area tileset
                    sw.WriteLine("}\n");
                }
            }
        }
        static void CreateFolders()
        {
            Directory.CreateDirectory(saveDir + "textures");
            Directory.CreateDirectory(saveDir + "shaders");
            Directory.CreateDirectory(saveDir + "swfs");
            Directory.CreateDirectory(saveDir + "data/platform_physics");
            Directory.CreateDirectory(saveDir + "data/water_physics");
            Directory.CreateDirectory(saveDir + "data/lowgrav_physics");
            Directory.CreateDirectory(saveDir + "data/player_physics");
            
        }
        static void CopyPreliminaries()
        {
            File.Copy("data/palette.png", saveDir + "textures/palette.png", true);
            foreach (var file in Directory.GetFiles("data/swfs"))
            {
                File.Copy(file, saveDir + $"swfs/{Path.GetFileName(file)}", true);
            }
            File.Copy("data/swfs/endnigh.swf", saveDir + "swfs/endnigh.swf", true); Console.WriteLine("swf");
            foreach (var file in Directory.GetFiles("data/shaders"))
            {
                File.Copy(file, saveDir + $"shaders/{Path.GetFileName(file)}", true);
            }
        }
        static void MapCSV()
        {
            System.IO.File.Copy("data/map_CLEAN.csv", saveDir + "data/map.csv", true);
            using (StreamWriter sw = File.AppendText(saveDir + "data/map.csv"))
            {
                for (int j = 0; j < Settings.NumAreas; j++)
                {
                    for (int i = 0; i < Settings.NumLevels; i++)
                    {
                        sw.Write($"v{j + 1}-{i + 1}.lvl,");
                    }
                    if (j != Settings.NumAreas-1) sw.Write($"v-npc{j + 1}.lvl,");
                }
                sw.Write("v-end.lvl");
            }
            /*if (settings.MirrorMode)
                FlipCSV(saveDir + "data/map.csv");*/

        }
        static void LevelInfo()
        {
            for (int i = 0; i < Settings.NumAreas; i++)
            {
                string areaname = GetFunnyName();

                using (StreamWriter sw = File.AppendText(saveDir + "data/levelinfo.txt.append"))
                {
                    for (int j = 0; j < Settings.NumLevels; j++)
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

                if (Settings.MirrorMode)
                    LevelManip.FlipLevelH(ref levelFile);

                LevelManip.Save(levelFile, saveDir + $"tilemaps/{level}.lvl");
            }

            for (int j = 0; j < Settings.NumAreas; j++)
            {
                for (int i = 0; i < Settings.NumLevels; i++)
                {
                    var level = ChosenLevels[j][i];
                    var levelFile = LevelManip.Load(level.InFile);

                    if (/*level.CanReverse && RNG.CoinFlip() ||*/ Settings.MirrorMode)
                        LevelManip.FlipLevelH(ref levelFile);

                    if (Settings.DoCorruptions)
                        //level.TSNeed += LevelCorruptors.CorruptLevel(ref levelFile);

                    LevelManip.Save(levelFile, saveDir + $"tilemaps/v{j + 1}-{i + 1}.lvl");
                }
                LevelManip.Save(npclevel, saveDir + $"tilemaps/v-npc{j + 1}.lvl");
            }
        }
        static string SaveRunPrompt()
        {
            string title       = GetFunnyName();
            string author      = $"Seed: {AppResources.GameSeed}";
            string description = "A randomized world!";
            string version     = DateTime.Now.ToString();

            TextWindow inputWindow = new TextWindow("Give this world a name: ", title);
            if (inputWindow.ShowDialog() == true)
                title = inputWindow.Result;

            string dir = AppResources.SavedRunsPath + title;

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
                foreach (var cat in AppResources.LevelPoolCategories)
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
                for (int j = 0; j < Settings.NumAreas; j++)     // select levels
                {
                    var levels = new List<Level> { };
                    for (int i = 0; i < Settings.NumLevels; i++)
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
        static void AddFlippedLevels(ref List<Level> DrawPool)
        {
            int size = DrawPool.Count();
            for (int i = 0; i < size; i++)
            {
                Level flipLevel = DrawPool[i].Clone();
                FlipLevelHoriz(ref flipLevel);
                DrawPool.Add(flipLevel);
            }
        }
        static void FlipLevelHoriz(ref Level level)
        {
            // This function flips the level (not level file) horizontally

            // Swap left and right entrances
            ConnectionType temp = level.MapConnections.L;
            level.MapConnections.L = level.MapConnections.R;
            level.MapConnections.R = temp;

            // Set FlippedHoriz flag to true
            // This notifies the randomizer to flip the level file when copying the level to the output folder
            level.FlippedHoriz = true;
        }

        public static void Randomize(string args = null)
        {

            saveDir = Settings.GameDirectory;
            if (args == "savemod") saveDir = SaveRunPrompt();
            if (saveDir == null) return;

            TilesetManip.MakeShaderPool();

            // Make the draw pool based on which level pools are enabled
            // MakeDrawPool();

            // Flip all levels in the drawpool horizontally and add the flipped variants to the pool
            // AddFlippedLevels(ref DrawPool);

            // Pass drawpool to map generator
            // MapGenerator.Levels = DrawPool;
            var pool = LevelPool.LoadPool("data/level pools/.mapgen/TestingConnectors.gon");
            MapGenerator.Connectors = pool.Levels;
            pool = LevelPool.LoadPool("data/level pools/.mapgen/TestingGameplay.gon");
            MapGenerator.Levels = pool.Levels;

            try { CreateFolders(); } catch (Exception ex) { Console.WriteLine($"Error creating folders. Exception {ex}"); MessageBox.Show($"Error creating folders. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            
            MapGenerator.GenerateMap();
            
            // The rest of the randomization process is delegated to the functions below.
            //try { LevelInfo(); }      catch (Exception ex) { Console.WriteLine($"Error creating levelinfo. Exception {ex}");                  MessageBox.Show($"Error creating levelinfo. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            //try { MapCSV(); }         catch (Exception ex) { Console.WriteLine($"Error creating map. Exception {ex}");                        MessageBox.Show($"Error creating map. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            //try { TileMaps(); }       catch (Exception ex) { Console.WriteLine($"Error copying tilemaps. Exception {ex}");                    MessageBox.Show($"Error copying tilemaps. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            //try { Tilesets(); }       catch (Exception ex) { Console.WriteLine($"Error creating tilesets. Exception {ex}");                   MessageBox.Show($"Error creating tilesets. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            //try { NPCs(); }           catch (Exception ex) { Console.WriteLine($"Error creating npcs. Exception {ex}");                       MessageBox.Show($"Error creating tilesets. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
            //try { Worldmap.WriteWorldMap(); } catch (Exception ex) { Console.WriteLine($"Error creating worldmap. Exception {ex}");           MessageBox.Show($"Error creating worldmap. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }
        }
    }
}
