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
            LINames         = gon["name"     ].ToStringArray();
            LILocations     = gon["location" ].ToStringArray();
            LIAdjectives    = gon["adjective"].ToStringArray();
            LINouns         = gon["noun"     ].ToStringArray();
        }
        static void LoadNPCs()
        {
            var gon = GonObject.Load($"data/text/npcs.gon");    // open npcs file
            NPCMovieClips = gon["movieclips"].ToStringArray();
            NPCSoundIDs   = gon["soundids"  ].ToStringArray();
            NPCTexts = new List<string[]>();

            var text = gon["text"];
            for (int i = 0; i < text.Size(); i++)
            {
                NPCTexts.Add(text[i].ToStringArray());
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
                ConnectionType temp    = level.MapConnections.L;
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
                level.FlipH = true;

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

            // Delete existing debug.md file
            if (File.Exists("tools/debug.md"))
                File.Delete("tools/debug.md");
            // Init ErrorNotes to emtpy string
            ErrorNotes = "";

            // Set up the shader pool
            TilesetManip.MakeShaderPool();

            MakeDrawPools();

            Connectors = LevelPool.LoadPool("data/level_pools/.mapgen/NewConnectors.gon").Levels;
            Secrets    = LevelPool.LoadPool("data/level_pools/Secrets.gon").Levels;

            AddFlippedLevels(ref StandardLevels);
            AddFlippedLevels(ref CartLevels);
            AddFlippedLevels(ref Secrets);

            //CartLevels = Connectors = StandardLevels = LevelPool.LoadPool("data/level_pools/.mapgen/NewConnectors.gon").Levels;
            try { CreateFolders(); } catch (Exception ex) { Console.WriteLine($"Error creating folders. Exception {ex}"); MessageBox.Show($"Error creating folders. Exception {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); throw; }

            WorldMap.Init();    // Init the worldmap.txt file

            // Load Area Definitions
            List<MapArea> head_areas = new List<MapArea>();
            List<MapArea> cart_areas = new List<MapArea>();
            {
                // Load Meta Info
                string path = "data/text/area_defs/standard.gon";
                GonObject file = GonObject.Load(path);

                GonObject meta = file["meta"];
                if (meta.TryGetChild("save_initiallevel", out GonObject initiallevel))
                    WorldMap.save_initiallevel = initiallevel.String();
                if (meta.TryGetChild("save_lightspawn", out GonObject lightspawn))
                    WorldMap.save_lightspawn = lightspawn.String();
                if (meta.TryGetChild("save_lightspawnlabel", out GonObject lightspawnlabel))
                    WorldMap.save_lightspawnlabel = lightspawnlabel.String();
                if (meta.TryGetChild("save_darkspawn", out GonObject darkspawn))
                    WorldMap.save_darkspawn = darkspawn.String();
                if (meta.TryGetChild("save_darkspawnlabel", out GonObject darkspawnlabel))
                    WorldMap.save_darkspawnlabel = darkspawnlabel.String();

                // Load Map Areas
                List<GonObject> loaded_gons = new List<GonObject>();
                if (!file.TryGetChild("areas", out GonObject gon_areas))
                    throw new Exception("Error: No areas found in area defs file.");
                if (!meta.TryGetChild("link", out GonObject gon_link))
                    throw new Exception("Error: No link info found in area defs meta.");

                string[] link_ids = gon_link.ToStringArray();
                for (int i = 0; i < link_ids.Length; i++)
                {
                    if (!gon_areas.TryGetChild(link_ids[i], out GonObject def))
                        throw new Exception($"Error: Could not find area {link_ids[i]} in areas");
                    head_areas.Add(LoadAreaDef(def));
                }

                if (file.TryGetChild("carts", out GonObject gon_carts))
                    for (int i = 0; i < gon_carts.Size(); i++)
                    {
                        GonObject gon_area = gon_carts[i];
                        string id = gon_area.GetName();
                        cart_areas.Add(LoadAreaDef(gon_area));
                        WorldMap.cartworlds += $"{id} ";
                    }

                MapArea LoadAreaDef(GonObject area_def)
                {
                    if (loaded_gons.Contains(area_def)) 
                        throw new Exception($"Error: Area def {area_def.GetName()} has already been loaded. Areas cannot be multiply referenced.");
                    loaded_gons.Add(area_def);

                    MapArea area = new MapArea();
                    area.ID = area_def.GetName();

                    if (area_def.TryGetChild("name", out GonObject name))
                        area.Name = name.String();
                    else area.Name = GetFunnyAreaName();

                    if (area_def.TryGetChild("tileset", out GonObject gon_tileset))
                    {
                        if (gon_tileset.TryGetChild("default", out GonObject gon_default))
                            area.Tileset = Tileset.PriorityMerge(area.Tileset, new Tileset(gon_default));
                        area.Tileset = Tileset.PriorityMerge(area.Tileset, TilesetManip.GetTileset());
                        if (gon_tileset.TryGetChild("need", out GonObject gon_need))
                            area.Tileset = Tileset.PriorityMerge(area.Tileset, new Tileset(gon_need));
                    }
                    else area.Tileset = TilesetManip.GetTileset();

                    if (area_def.TryGetChild("area_type", out GonObject areatype_gon))
                    {
                        switch (areatype_gon.String())
                        {
                            case "dark":
                                area.AreaType = AreaType.dark;
                                break;

                            case "cart":
                                area.AreaType = AreaType.cart;
                                area.LevelCollectables = Collectables.Rings;
                                goto case null;
                            case "ironcart":
                                area.AreaType = AreaType.ironcart;
                                goto case null;
                            case "glitch":
                                area.AreaType = AreaType.glitch;
                                goto case null;

                            // This case cannot occur naturally, so we use it for the common attributes of cart cases
                            case null:
                                WorldMap.cartworlds += $"{area.ID} ";
                                area.Flags |= MapArea._Flags.Concatenate;
                                area.Levels = CartLevels;
                                break;
                        }
                    }
                    else area.LevelCollectables = Collectables.Tumor;

                    if (area_def.TryGetChild("tags", out GonObject tags_gon))
                    {
                        string[] tags = tags_gon.ToStringArray();

                        if (tags.Contains("backtrack"))
                            area.Flags |= MapArea._Flags.BackTrack;
                        if (tags.Contains("mainworlds"))
                            WorldMap.mainworlds += $"{area.ID} ";
                    }

                    if (area_def.Contains("entrance_dir"))
                        area.EDir = area_def["entrance_dir"].String().ToDirections();

                    // GenType-specific loading below
                    string gen_type = area_def["gen_type"].String();

                    if (gen_type == "standard")
                    {
                        area.GenType = GenerationType.Standard;
                        area.LevelQuota = area_def["levels"].Int();
                        area.MaxSize.I = area_def["bounds"][0].Int();
                        area.MaxSize.J = area_def["bounds"][1].Int();
                        if (area_def.TryGetChild("exit_dir", out GonObject exit_dir))
                            area.XDir = exit_dir.String().ToDirections();
                        if (area_def.TryGetChild("anchor", out GonObject anchor))
                            area.Anchor = anchor.String().ToDirections();
                        if (area_def.TryGetChild("no_build", out GonObject no_build))
                            area.NoBuild = no_build.ToStringArray().ToDirections();


                        if (area_def.TryGetChild("exit", out GonObject exit))
                        {
                            GonObject.FieldType type = exit.GetFieldType();
                            if (type == GonObject.FieldType.ARRAY)
                                area.ExitCollectables = exit.ToStringArray().ToCollectables();
                            else
                            {
                                string child_id = exit.String();
                                if (child_id == area.ID)
                                    throw new Exception("Areas cannot list self as next area id.");
                                if (!gon_areas.TryGetChild(child_id, out GonObject child))
                                    throw new Exception($"Could not find area definition with matching id {child_id}.");
                                area.Child = LoadAreaDef(child);
                            }
                        }

                        return area;
                    }

                    if (gen_type == "loaded")
                    {
                        area.GenType = GenerationType.Loaded;
                        area.CSVPath = area_def["csvfile"].String();
                        area.LevelPath = area_def["levelpath"].String();
                    }
                    else if (gen_type == "split")
                        area.GenType = GenerationType.Split;

                    if (area_def.TryGetChild("exits", out GonObject exits))
                    {
                        area.ChildU = AddChild("up");
                        area.ChildD = AddChild("down");
                        area.ChildL = AddChild("left");
                        area.ChildR = AddChild("right");

                        MapArea AddChild(string exit_id)
                        {
                            if (exits.TryGetChild(exit_id, out GonObject exit))
                            {
                                string child_id = exit.String();
                                if (child_id == area.ID)
                                    throw new Exception("Areas cannot list self as next area id.");
                                if (!gon_areas.TryGetChild(child_id, out GonObject child))
                                    throw new Exception($"Could not find area definition with matching id {child_id}.");
                                return LoadAreaDef(child);
                            }
                            return null;
                        }
                    }

                    return area;

                    
                }
            }

            GameMap final_map = new GameMap(0, 0);
            for (int i = 0; i < head_areas.Count; i++)
            {
                (GameMap map, Pair e_coord) = GenerateAndLink(head_areas[i]);
                final_map = ConcatenateMaps(final_map, map);
            }
            for (int i = 0; i < cart_areas.Count; i++)
            {
                (GameMap map, Pair e_coord) = GenerateAndLink(cart_areas[i]);
                final_map = ConcatenateMaps(final_map, map);
            }

            PrintCSV(final_map, $"{SaveDir}/data/map.csv");
            WorldMap.Write();
            CopyAssets();

            if (ErrorNotes != "") MessageBox.Show($"Minor Errors Encountered:\n{ErrorNotes}", "Error Notes", MessageBoxButton.OK, MessageBoxImage.Warning);

            return 0;
        }
    }
}
