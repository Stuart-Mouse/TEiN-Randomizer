using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;


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

        public static string ErrorNotes = "";    // Will be used for minor errors to be catalogues and printed, without throwing an error that ends randomization.

        public static ObservableCollection<LevelPoolCategory> LevelPoolCategories = AppResources.LevelPoolCategories;
        public static ObservableCollection<PiecePool        > PiecePools          = AppResources.PiecePools;

        static List<List<Level>> ChosenLevels;

        public static string SaveDir;         // The save directory is determined when starting the randomize function and stored. (Will select between game directory and save directory.)
        public static SettingsFile Settings = AppResources.MainSettings;  // a copy of the settings is stored so that all of the functions within this class can read it.

        public static List<string> AreaTypesChosen;

        // The below resources should be moved to the AppResources class

        // NPC Stuff
        public static string[] NPCMovieClips;
        public static string[] NPCSoundIDs;
        public static List<string[]> NPCTexts;

        // Funny Name Stuff
        public static string[] LINouns;
        public static string[] LIAdjectives;
        public static string[] LINames;
        public static string[] LILocations;

        // worldmap.txt
        public static WorldMapFile WorldMap;

        // METHODS

        // resource loading functions
        static void LoadFunnyNames()
        {
            var gon         = GonObject.Load("data/text/area_names.gon");
            LINames         = GonObject.Manip.ToStringArray(gon["name"]);
            LILocations     = GonObject.Manip.ToStringArray(gon["location"]);
            LIAdjectives    = GonObject.Manip.ToStringArray(gon["adjective"]);
            LINouns         = GonObject.Manip.ToStringArray(gon["noun"]);
        }
        static void LoadNPCs()
        {
            var gon = GonObject.Load($"data/text/npcs.gon");    // open npcs file
            NPCMovieClips = GonObject.Manip.ToStringArray(gon["movieclips"]);
            NPCSoundIDs = GonObject.Manip.ToStringArray(gon["movieclips"]);
            NPCTexts = new List<string[]>();

            var text = gon["text"];
            for (int i = 0; i < text.Size(); i++)
            {
                NPCTexts.Add(GonObject.Manip.ToStringArray(text[i]));
            }
        }

        // Randomize() subfunctions (separated out for readability)
        static void NPCs()
        {
            using (StreamWriter sw = File.CreateText(SaveDir + "data/npcs.txt.append"))
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
        public static string GetFunnyAreaName()
        {
            // STRUCTURE:
            // ( NAME's ) ( ADJECTIVE ) LOCATION of ( ADJECTIVE ) NOUN
            // ( NAME's ) ( ADJECTIVE ) NOUN LOCATION

            // create le funny name
            string area_name = "";
            if (RNG.random.Next(0, 5) == 0)
            {
                area_name += LINames[RNG.random.Next(0, LINames.Length)] + "s ";
            }
            if (RNG.CoinFlip())
            {
                if (RNG.CoinFlip())
                    area_name += LIAdjectives[RNG.random.Next(0, LIAdjectives.Length)] + " ";
                area_name += LINouns[RNG.random.Next(0, LINouns.Length)] + " ";
                area_name += LILocations[RNG.random.Next(0, LILocations.Length)];
            }
            else
            {
                area_name += LILocations[RNG.random.Next(0, LILocations.Length)] + " of ";
                if (RNG.CoinFlip())
                    area_name += LIAdjectives[RNG.random.Next(0, LIAdjectives.Length)] + " ";
                area_name += LINouns[RNG.random.Next(0, LINouns.Length)];
            }
            return area_name;
        }
        public static string GetFunnyModName()
        {
            // STRUCTURE:
            // The ( ADJECTIVE ) NOUN is Nigh
            // The End is ADJECTIVE

            // create le funny name
            string area_name = "The ";
            if (RNG.CoinFlip())
            {
                if (RNG.CoinFlip())
                    area_name += LIAdjectives[RNG.random.Next(0, LIAdjectives.Length)] + " ";
                area_name += LINouns[RNG.random.Next(0, LINouns.Length)] + " is Nigh";
            }
            else
            {
                area_name = $"The End is {LIAdjectives[RNG.random.Next(0, LIAdjectives.Length)]}";
            }
            return area_name;
        }
        static void CreateFolders()
        {
            Directory.CreateDirectory(SaveDir + "textures");
            Directory.CreateDirectory(SaveDir + "shaders");
            Directory.CreateDirectory(SaveDir + "swfs");
            Directory.CreateDirectory(SaveDir + "data/platform_physics");
            Directory.CreateDirectory(SaveDir + "data/water_physics");
            Directory.CreateDirectory(SaveDir + "data/lowgrav_physics");
            Directory.CreateDirectory(SaveDir + "data/player_physics");
        }
        static void CopyAssets()
        {
            File.Copy("data/palette.png", SaveDir + "textures/palette.png", true);
            foreach (var file in Directory.GetFiles("data/swfs"))
            {
                File.Copy(file, SaveDir + $"swfs/{Path.GetFileName(file)}", true);
            }
            File.Copy("data/swfs/endnigh.swf", SaveDir + "swfs/endnigh.swf", true); Console.WriteLine("swf");
            foreach (var file in Directory.GetFiles("data/shaders"))
            {
                File.Copy(file, SaveDir + $"shaders/{Path.GetFileName(file)}", true);
            }
        }
        static string SaveRunPrompt()
        {
            string title       = GetFunnyModName();
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
        static void MakeDrawPools()
        {
            StandardLevels = new List<Level>(); // Init all level lists
            CartLevels     = new List<Level>();
            Connectors     = new List<Level>();
            Secrets        = new List<Level>();
            List<Level> list;
            foreach (var cat in AppResources.LevelPoolCategories)
            {
                if (!cat.Enabled) continue;
                foreach (var pool in cat.Pools)
                {
                    if (!pool.Enabled) continue;
                    switch (pool.Type)  // Switch the list being referenced based on the type of pool we are adding
                    {
                        case PoolType.Standard:
                            list = StandardLevels; break;
                        case PoolType.Connector:
                            list = Connectors;     break;
                        case PoolType.Secret:
                            list = Secrets;        break;
                        case PoolType.Cart:
                            list = CartLevels;     break;
                        default:
                            throw new Exception("Tried to load level pool with invalid pool type");
                    }
                    foreach (var level in pool.Levels)
                    {
                        list.Add(level);
                    }
                }
            }
        }
        static void AddFlippedLevels(ref List<Level> pool)
        {
            int size = pool.Count();
            for (int i = 0; i < size; i++)
            {
                Level level = pool[i].Clone();

                // Swap left and right entrances
                ConnectionType temp = level.MapConnections.L;
                level.MapConnections.L = level.MapConnections.R;
                level.MapConnections.R = temp;

                temp = level.MapConnections.UL;
                level.MapConnections.UL = level.MapConnections.UR;
                level.MapConnections.UR = temp;

                temp = level.MapConnections.DL;
                level.MapConnections.DL = level.MapConnections.DR;
                level.MapConnections.DR = temp;

                // Set FlippedHoriz flag to true
                // This notifies the randomizer to flip the level file when copying the level to the output folder
                level.FlippedHoriz = true;

                pool.Add(level);
            }
        }
        public static int Randomize(string args = null)
        {

            SaveDir = Settings.GameDirectory;
            if (args == "savemod") SaveDir = SaveRunPrompt();
            if (SaveDir == null)
            {
                MessageBox.Show($"Error: Save Directory was null.", "Randomizer Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }

            ErrorNotes = "";    // Init ErrorNotes to emtpy string

            TilesetManip.MakeShaderPool();  // Set up the shader pool

            MakeDrawPools();    // Make the draw pool based on which level pools are enabled
            AddFlippedLevels(ref StandardLevels);   // Flip all levels in the drawpool horizontally and add the flipped variants to the pool
            AddFlippedLevels(ref CartLevels);

            // Load the connectors
            Connectors = LevelPool.LoadPool("data/level_pools/.mapgen/TestingConnectors.gon").Levels;
            //CartLevels = Connectors = StandardLevels = LevelPool.LoadPool("data/level_pools/.mapgen/NewConnectors.gon").Levels;
            try { CreateFolders(); } catch (Exception ex) { Console.WriteLine($"Error creating folders. Exception {ex}"); MessageBox.Show($"Error creating folders. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }

            WorldMap.Init();    // Init the worldmap.txt file

            // Map Generation
            try
            {
                GameMap gameMap = GenerateGameMap();
                PrintCSV(gameMap, $"{SaveDir}/data/map.csv");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "Randomizer Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }

            WorldMap.Write();
            CopyAssets();       // Copy the palette and swfs to save dir

            if (ErrorNotes != "") MessageBox.Show($"Minor Errors Encountered:\n{ErrorNotes}", "Error Notes", MessageBoxButton.OK, MessageBoxImage.Warning);

            return 0;
        }

        
    }
}
