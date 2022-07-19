using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEiNRandomizer
{
    public static class MapGenerator
    {
        // MapArea class is used to keep track of map areas as they are created/placed
        // The main map generation routines have been moved from the map generator class into the mapArea class

        // CONSTANTS


        // STATIC MEMBERS

        // Dictionary of the MapAreas where the area name is the key
        private static Dictionary<string, MapArea> MapAreas;

        // Reference to main settings
        private static SettingsFile Settings = AppResources.MainSettings;

        // static empty mapConnections instance for comparison purposes
        public static readonly MapConnections NoMapConnections;

        // The area defs to be used in generation
        // When referenced by an AreaEnd, they will be loaded into the queue for building
        //static List<AreaDef> AreaDefs;

        // For keeping track of keys
        static int KeysPlaced;
        static int LocksPlaced;

        // static dots screen
        static readonly MapScreen DotScreen = new MapScreen("..", ScreenType.Dots, null);

        // array used to store tag id for each direction so we can iterate over the directions easily
        // This is initialized every time this function is run, so this is probably not optimal
        static readonly TileID[] TransitionTagIDs = { TileID.GreenTransitionR, TileID.GreenTransitionL, TileID.GreenTransitionD, TileID.GreenTransitionU };

        // Level pools are shared in common amongst all areas by default
        // These can be overriden though
        public static List<Level> Levels       = Randomizer.Levels;
        public static List<Level> Connectors   = Randomizer.Connectors;
        public static List<Level> Secrets      = Randomizer.Secrets;

        public static GameMap GenerateGameMap()
        {
            MapAreas = new Dictionary<string, MapArea>();
            string start_area_id;  

            // LOAD AREA DEFINITIONS
            {
                string path = "data/text/area_defs.gon";
                GonObject file = GonObject.Load(path);

                // Load meta info
                GonObject meta = file["meta"];
                start_area_id = meta["start_area"].String();
                
                // Load area defintions
                GonObject areas = file["areas"];
                for (int i = 0; i < areas.Size(); i++)
                {
                    // Get area gon from file
                    GonObject area_def = areas[i];

                    // Create the new mapArea
                    MapArea area = new MapArea();


                    // Set level tileset defaults and needs if not null
                    if (area_def.TryGetChild("tileset", out GonObject gonTileset))
                    {
                        if (gonTileset.TryGetChild("default", out GonObject gonDefault))    // load defaults
                            area.Tileset += new Tileset(gonDefault);
                        area.Tileset = TilesetManip.GetTileset();                           // add randomized tileset
                        if (gonTileset.TryGetChild("need", out GonObject gonNeed))          // add needs
                            area.Tileset += new Tileset(gonNeed);
                    }
                    else    // get randomized tsdefault
                        area.Tileset = TilesetManip.GetTileset();

                    // get area id and name
                    area.ID = area_def.GetName();
                    if (area_def.TryGetChild("name", out GonObject name)) area.Name = name.String();

                    // Get the next area id
                    if (area_def.TryGetChild("exit", out GonObject nextArea)) area.NextAreaID = nextArea.String();
                    if (area.NextAreaID == area.ID) throw new Exception("Areas cannot list self as next area id.");

                    // Set area settings
                    area.AreaSettings = null;

                    // set the entrance direction for all areas that are not the starting area
                    if (area.ID != start_area_id)
                    {
                        if (!area_def.Contains("entrance_dir"))
                            throw new Exception($"Must define entrance direction in area {area.ID}");
                        area.EDir = DirectionsEnum.FromString(area_def["entrance_dir"].String());
                    }

                    // Set generation type, do type specific data loading
                    switch (area_def["gen_type"].String())
                    {
                        case "standard":
                            {
                                area.Type = GenerationType.Standard;
                                area.Name = Randomizer.GetFunnyAreaName();
                                area.LevelQuota = area_def["levels"].Int();
                                area.MaxSize.First = area_def["bounds"][0].Int();
                                area.MaxSize.Second = area_def["bounds"][1].Int();
                                if (area_def.TryGetChild("anchor", out GonObject anchor)) area.Anchor = DirectionsEnum.FromString(anchor.String());
                                if (area_def.TryGetChild("no_build", out GonObject no_build)) area.NoBuild = DirectionsEnum.FromString(no_build.String());
                                if (area_def.TryGetChild("exit_dir", out GonObject exit_dir)) area.XDir = DirectionsEnum.FromString(exit_dir.String());
                                break;
                            }
                        case "loaded":
                            {
                                area.Type = GenerationType.Loaded;
                                area.CSVPath = area_def["csvfile"].String();
                                area.LevelPath = area_def["levelpath"].String();
                                if (area_def.TryGetChild("exits", out GonObject exits)) // get the exits
                                {
                                    if (exits.TryGetChild("up"   , out GonObject up   )) area.XUp    = up.String();
                                    if (exits.TryGetChild("down" , out GonObject down )) area.XDown  = down.String();
                                    if (exits.TryGetChild("left" , out GonObject left )) area.XLeft  = left.String();
                                    if (exits.TryGetChild("right", out GonObject right)) area.XRight = right.String();
                                }
                                break;
                            }
                        case "split":
                            {
                                area.Type = GenerationType.Split;
                                if (area_def.TryGetChild("exits", out GonObject exits)) // get the exits
                                {
                                    if (exits.TryGetChild("up"   , out GonObject up   )) area.XUp    = up.String();
                                    if (exits.TryGetChild("down" , out GonObject down )) area.XDown  = down.String();
                                    if (exits.TryGetChild("left" , out GonObject left )) area.XLeft  = left.String();
                                    if (exits.TryGetChild("right", out GonObject right)) area.XRight = right.String();
                                }
                                break;
                            }
                        default:
                            throw new Exception($"Invalid or missing area type in area {area.ID}. Check area defs file.");
                            break;
                    }

                    // Add the area def to the areaDefs dictionary
                    MapAreas.Add(area.ID, area);
                }
            }

            // GENERATE MAP AREAS
            foreach (var item in MapAreas)
            {
                MapArea area = item.Value;
                GenerateArea(area);
            }

            // Get start definition
            MapArea start_area = MapAreas[start_area_id];

            // Link maps together to produce final map
            GameMap final_map = LinkMaps(start_area, out Pair e_coord);

            // REWRITING MAP LINKER
            // generation of area data files needs to be done before linking
            // the linking process will not add any levels to the area or affect the generation of data files whatsoever
            // linking the main maps should still be done recursively
            // carts and steven areas will be added on to the end of the completed map from above step

            // Return the final game map
            return final_map;
        }
        static GameMap LinkMaps(MapArea area, out Pair e_coord)
        {
            // Get entry and exit coords
            Pair new_e_coord = area.ECoords;
            Pair xu_coord = area.XUpCoords,
                 xd_coord = area.XDownCoords,
                 xl_coord = area.XLeftCoords,
                 xr_coord = area.XRightCoords;
            Pair m1_offset = new Pair(0, 0);
            switch (area.Type)
            {
                case GenerationType.Standard:
                    {
                        if (area.NextAreaID == null)
                        {
                            // cap the end with a generic path end

                            // return map and entry coord
                            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                            e_coord = area.ECoords;
                            return area.Map;
                        }
                        else
                        {
                            GameMap ret_map = LinkMap(area.Map, area.NextAreaID, area.XCoords, area.XDir);
                            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                            e_coord = new_e_coord;
                            return ret_map;
                        }
                        break;
                    }
                case GenerationType.Split:
                    goto case GenerationType.Loaded;
                case GenerationType.Loaded:
                    {
                        bool had_child = false;
                        GameMap ret_map = area.Map;
                        DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_pre.csv");
                        
                        if (area.XUp != null)
                        {
                            ret_map = LinkMap(ret_map, area.XUp, xu_coord, Directions.U);
                            if (area.XDown != null)
                            {
                                xd_coord += m1_offset;
                                xd_coord = DotsFromCellToEdge(ret_map, xd_coord, new Pair(1, 0));
                            }
                            if (area.XLeft != null)
                            {
                                xl_coord += m1_offset;
                                xl_coord = DotsFromCellToEdge(ret_map, xl_coord, new Pair(0, -1));
                            }
                            if (area.XRight != null)
                            {
                                xr_coord += m1_offset;
                                xr_coord = DotsFromCellToEdge(ret_map, xr_coord, new Pair(0, 1));
                            }
                            had_child = true;
                        }
                        DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_U.csv");
                        if (area.XDown != null)
                        {
                            ret_map = LinkMap(ret_map, area.XDown, xd_coord, Directions.D);
                            if (area.XLeft != null)
                            {
                                xl_coord += m1_offset;
                                xl_coord = DotsFromCellToEdge(ret_map, xl_coord, new Pair(0, -1));
                            }
                            if (area.XRight != null)
                            {
                                xr_coord += m1_offset;
                                xr_coord = DotsFromCellToEdge(ret_map, xr_coord, new Pair(0, 1));
                            }
                            had_child = true;
                        }
                        DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_D.csv");
                        if (area.XLeft != null)
                        {
                            ret_map = LinkMap(ret_map, area.XLeft, xl_coord, Directions.L);
                            if (area.XRight != null)
                            {
                                xr_coord += m1_offset;
                                xr_coord = DotsFromCellToEdge(ret_map, xr_coord, new Pair(0, 1));
                            }
                            had_child = true;
                        }
                        DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_L.csv");
                        if (area.XRight != null)
                        {
                            ret_map = LinkMap(ret_map, area.XRight, xr_coord, Directions.R);
                            had_child = true;
                        }
                        DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_R.csv");
                        if (had_child)
                        {
                            DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_post.csv");
                            e_coord = new_e_coord;
                            return ret_map;
                        }
                        else
                        {
                            // cap the end with a generic path end

                            // return map and entry coord
                            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                            e_coord = new_e_coord;
                            return area.Map;
                        }
                        break;
                    }
            }
            e_coord = new_e_coord;
            return new GameMap(0,0);    // this should not be reached

            void DebugPrintMap(GameMap map, string path)
            {
                List<Tuple<Pair, string>> features = new List<Tuple<Pair, string>>();
                features.Add(new Tuple<Pair, string>(xu_coord, "Xu"));
                features.Add(new Tuple<Pair, string>(xd_coord, "Xd"));
                features.Add(new Tuple<Pair, string>(xl_coord, "Xl"));
                features.Add(new Tuple<Pair, string>(xr_coord, "Xr"));
                PrintDebugCSV(map, path, features);
            }

            GameMap LinkMap(GameMap curr_map, string next_area_id, Pair x_coord, Directions exit_dir)
            {
                // area has no specified end
                if (next_area_id == null)
                {
                    // cap the end with a generic path end

                    // return map
                    return area.Map;
                }

                // area connects to one or more child areas
                {
                    // get the map (and entry coord) from child
                    if (!MapAreas.TryGetValue(next_area_id, out MapArea child_area)) throw new Exception($"Could not find area def with id {next_area_id}");
                    GameMap child_map = LinkMaps(child_area, out Pair child_entry_coord);

                    // combine the current map with childs map
                    // we can assume that the entrance_dir matches the parent's exit_dir if we pre-check for this when loading the area defs
                    Console.WriteLine($"{area.ID} + {next_area_id}:\n" +
                        $"Dimensions: {area.Map.Height}, {area.Map.Height}\n" +
                        $"Exit_Coord: {x_coord.First}, {x_coord.Second}\n" +
                        $"Direction : {exit_dir}\n");
                    
                    GameMap new_map = CombineMaps(curr_map, child_map, x_coord, child_entry_coord, exit_dir, out Pair new_m1_offset);

                    // set new offset and entry coord
                    m1_offset = new_m1_offset;
                    new_e_coord += m1_offset;
                    // make sure that entrance dots still extends to edge of map
                    Pair e_vec = DirectionsEnum.ToVectorPair(area.EDir);
                    if (e_vec != new Pair (0,0))
                        new_e_coord = DotsFromCellToEdge(new_map, new_e_coord, e_vec);

                    // return the map and the new entry coord to parent
                    return new_map;
                }
            }
        }
        static GameMap CombineMaps(GameMap m1, GameMap m2, Pair m1_exit, Pair m2_entry, Directions m1_exit_dir, out Pair m1_origin)
        {
            // This function will combine the two maps entered as parameters into a single map
            // The two maps passed in MUST have the same build direction, or map linking will not work
            // new entry is the new entry coord for the combined map

            // Declare map to be returned
            GameMap new_map;

            // Initialize origin coords for each map
                 m1_origin = new Pair();
            Pair m2_origin = new Pair();

            // Initialize dimensions for new map
            int height, width;

            // Determine the origin/offset of each map based on direction
            switch (m1_exit_dir)
            {
                case Directions.U:
                    m1_origin.Second = Math.Max(0, m2_entry.Second - m1_exit.Second);   // align exit and entrance horizontally by moving leftmost transition rightwards
                    m2_origin.Second = Math.Max(0, m1_exit.Second - m2_entry.Second);
                    m1_origin.First = m2_origin.First + m2_entry.First + 1;             // align top edge of m1 to bottom edge of m2
                    width = Math.Max(m1_origin.Second + m1.Width, m2_origin.Second + m2.Width); // get rightmost map coord as width
                    height = m1_origin.First + m1.Height;
                    break;

                case Directions.D:
                    m1_origin.Second = Math.Max(0, m2_entry.Second - m1_exit.Second);   // align exit and entrance horizontally by moving leftmost transition rightwards
                    m2_origin.Second = Math.Max(0, m1_exit.Second - m2_entry.Second);
                    m2_origin.First = m1_origin.First + m1_exit.First + 1;             // align top edge of m2 to bottom edge of m1
                    width = Math.Max(m1_origin.Second + m1.Width, m2_origin.Second + m2.Width); // get rightmost map coord as width
                    height = m2_origin.First + m2.Height;
                    break;

                case Directions.L:
                    m1_origin.First = Math.Max(0, m2_entry.First - m1_exit.First);   // align exit and entrance vertically by moving upmost transition downwards
                    m2_origin.First = Math.Max(0, m1_exit.First - m2_entry.First);
                    m1_origin.Second = m2_origin.Second + m2_entry.Second + 1;             // align left edge of m1 to right edge of m2
                    height = Math.Max(m1_origin.First + m1.Height, m2_origin.First + m2.Height); // get rightmost map coord as width
                    width = m1_origin.Second + m1.Width;
                    break;

                case Directions.R:
                    m1_origin.First = Math.Max(0, m2_entry.First - m1_exit.First);   // align exit and entrance vertically by moving upmost transition downwards
                    m2_origin.First = Math.Max(0, m1_exit.First - m2_entry.First);
                    m2_origin.Second = m1_origin.Second + m1_exit.Second + 1;             // align left edge of m2 to right edge of m1
                    height = Math.Max(m1_origin.First + m1.Height, m2_origin.First + m2.Height); // get rightmost map coord as width
                    width = m2_origin.Second + m2.Width;
                    break;

                default:
                    throw new Exception("Error combining maps: Invlaid Direction.");
            }

            // Create new map with required size
            new_map = new GameMap(height, width);

            // Combine the maps by pasting into new map
            GameMap.CopyToMapAtCoords(m1, new_map, m1_origin);
            GameMap.CopyToMapAtCoords(m2, new_map, m2_origin);

            // Set new_entry and return the new combined GameMap
            return new_map;
        }
        static void GenerateArea(MapArea area)
        {
            switch(area.Type)
            {
                case GenerationType.Standard:
                    GenerateStandardArea(area);
                    break;
                case GenerationType.Loaded:
                    LoadAreaFromCSV(area);
                    break;
                case GenerationType.Split:
                    CreateSplitArea(area);
                    break;
            }
            GenerateAreaDataFiles(area);    // Create all necessary data files for this area
        }
        static void CreateSplitArea(MapArea area)
        {
            // Create a single screen map, place all exit coords at (0,0)
            area.Map = new GameMap(1, 1);
            area.XUpCoords    = new Pair(0, 0);
            area.XDownCoords  = new Pair(0, 0);
            area.XLeftCoords  = new Pair(0, 0);
            area.XRightCoords = new Pair(0, 0);

            // get reqs for split level
            MapConnections reqs = new MapConnections();
            MapConnections nots = new MapConnections();
            // entrance
            switch (area.EDir)
            {
                case Directions.U:
                    reqs.U = ConnectionType.entrance;
                    break;
                case Directions.D:
                    reqs.D = ConnectionType.entrance;
                    break;
                case Directions.L:
                    reqs.L = ConnectionType.entrance;
                    break;
                case Directions.R:
                    reqs.R = ConnectionType.entrance;
                    break;
            }
            // exits
            if (area.XUp    != null) reqs.U = ConnectionType.exit;
            if (area.XDown  != null) reqs.D = ConnectionType.exit;
            if (area.XLeft  != null) reqs.L = ConnectionType.exit;
            if (area.XRight != null) reqs.R = ConnectionType.exit;

            List<Level> options = GetOptionsClosed(reqs, nots, Connectors);
            Level level = options[RNG.random.Next(0, options.Count)];
            MapScreen screen = new MapScreen($"{area.ID}-x", ScreenType.Connector, level);
            area.Map.Data[0, 0] = screen;
            area.ChosenScreens.Add(screen);
        }
        static void LoadAreaFromCSV(MapArea area)
        {
            // we will still need to know certain info about this area
            // - entry coords, exit coords, dimensions,
            // this will have to be specified in the area defs and loaded somehow
            // entry and exit could be derived from a csv, as well as screen connection types
            // alternatively, exact levels can be specified

            // need to ba able to specify level number or name


            // FORMAT:
            // levelnumber/tags/filename      (loads level by filename)
            // levelnumber/tags/*UDLR/UDLR           (picks random level with the specified mapconnections, entrances before exits)

            // Random levels need to be in the format "*UDLR"
            // The star indicates the random level and the connections are given explicitly
            // may add a way to indicate pickup types / other info (keys, mega tumors, secret levels, locks)

            // load csv to array
            string[][] arr = Utility.LoadCSV(area.CSVPath, out int length);

            // create GameMap
            area.Map = new GameMap(arr.Length, length);

            // iterate over array to create GameMap
            for (int row = 0; row < arr.Length; row++)
            {
                string[] line = arr[row];
                for (int col = 0; col < line.Length; col++)
                {
                    if (line[col] == "") continue;          // go to next col if cell is empty
                    string[] cell = line[col].Split('/');   // split the cell on '/' to get info

                    // read levelnum and tags
                    string levelnum = cell[0];
                    string tags     = cell[1];
                    Level level = new Level();

                    // randomly chosen levels
                    if (cell[2][0] == '*')  // check if first char is '*'. This signifies a randomized level
                    {
                        // create the mapconnections
                        MapConnections reqs = new MapConnections();
                        if (cell[2].Contains('U')) reqs.U |= ConnectionType.entrance;
                        if (cell[2].Contains('D')) reqs.D |= ConnectionType.entrance;
                        if (cell[2].Contains('L')) reqs.L |= ConnectionType.entrance;
                        if (cell[2].Contains('R')) reqs.R |= ConnectionType.entrance;
                        if (cell[3].Contains('U')) reqs.U |= ConnectionType.exit;
                        if (cell[3].Contains('D')) reqs.D |= ConnectionType.exit;
                        if (cell[3].Contains('L')) reqs.L |= ConnectionType.exit;
                        if (cell[3].Contains('R')) reqs.R |= ConnectionType.exit;

                        // get a random level with only reqs
                        List<Level> options = GetOptionsClosed(reqs, new MapConnections(), Levels);
                        level = options[RNG.random.Next(0, options.Count)];
                    }
                    else
                    {
                        // levels with specified filenames
                        level.Name = $"{cell[2]}";
                        level.Path = $"{area.LevelPath}";
                    }
                    level.TSDefault = new Tileset();
                    level.TSNeed    = new Tileset();

                    // process level tags
                    // perform necessary operations/modifications on level
                    if (tags.Contains("Xu")) area.XUpCoords    = new Pair(row, col);
                    if (tags.Contains("Xd")) area.XDownCoords  = new Pair(row, col);
                    if (tags.Contains("Xl")) area.XLeftCoords  = new Pair(row, col);
                    if (tags.Contains("Xr")) area.XRightCoords = new Pair(row, col);



                    // add the level to map
                    MapScreen screen = new MapScreen($"{area.ID}-{levelnum}", ScreenType.Level, level, "");
                    area.Map.Data[row, col] = screen;
                    area.ChosenScreens.Add(screen);
                }
            }
            if (area.XUp    != null) DotsFromCellToEdge(area.Map, area.XUpCoords   , new Pair(-1,  0));
            if (area.XDown  != null) DotsFromCellToEdge(area.Map, area.XDownCoords , new Pair( 1,  0));
            if (area.XLeft  != null) DotsFromCellToEdge(area.Map, area.XLeftCoords , new Pair( 0, -1));
            if (area.XRight != null) DotsFromCellToEdge(area.Map, area.XRightCoords, new Pair( 0,  1));

        }
        static void GenerateStandardArea(MapArea area)
        {
            // PRELIMINARIES
            // Initialize Ends
            area.OpenEnds    = new HashSet<Pair>();
            area.DeadEntries = new HashSet<Pair>();
            area.SecretEnds  = new HashSet<Pair>();
            // Init Map and associated trackers
            area.Map   = new GameMap(area.MaxSize.First, area.MaxSize.Second);  // Initialize map to size of bounds in def
            area.MinBuilt = new Pair(area.MaxSize.First, area.MaxSize.Second);  // Initializes to highest possible value
            area.MaxBuilt = new Pair(0, 0);                                     // Initializes to lowest possible value

            // PLACE FIRST LEVEL
            {
                // Initialize Entrance coords
                // This is used here as the coords for the first level placed
                // It will be adjusted later to be on the edge of the map
                // This is referenced later when connecting the areas
                area.ECoords = area.MaxSize / 2;

                // Move to edges based on build directions
                // For now, you can only anchor to one direction

                // Requirements are based on anchor point as well. 
                // The CheckNeighbors method is not going to give us what we want in this case
                // because we are placing a level on the edge of the map.
                // So, we make our own based on anchors
                MapConnections reqs = NoMapConnections;
                MapConnections nots = NoMapConnections;

                // Set first level coord based on anchor
                if      (area.Anchor.HasFlag(Directions.U)) { nots.U |= ConnectionType.all; area.ECoords.First  = 0; }
                else if (area.Anchor.HasFlag(Directions.D)) { nots.D |= ConnectionType.all; area.ECoords.First  = area.MaxSize.First  - 1; }
                if      (area.Anchor.HasFlag(Directions.L)) { nots.L |= ConnectionType.all; area.ECoords.Second = 0; }
                else if (area.Anchor.HasFlag(Directions.R)) { nots.R |= ConnectionType.all; area.ECoords.Second = area.MaxSize.Second - 1; }

                // Set first level nots based on no_build
                if      (area.NoBuild.HasFlag(Directions.U)) nots.U |= ConnectionType.all;
                else if (area.NoBuild.HasFlag(Directions.D)) nots.D |= ConnectionType.all;
                if      (area.NoBuild.HasFlag(Directions.L)) nots.L |= ConnectionType.all;
                else if (area.NoBuild.HasFlag(Directions.R)) nots.R |= ConnectionType.all;

                // Set first level reqs based on entrance direction
                switch(area.EDir)
                {
                    case Directions.U:
                        reqs.U |= ConnectionType.entrance;
                        if (nots.U.HasFlag(ConnectionType.entrance)) nots.U -= ConnectionType.entrance;
                        break;
                    case Directions.D:
                        reqs.D |= ConnectionType.entrance;
                        if (nots.D.HasFlag(ConnectionType.entrance)) nots.D -= ConnectionType.entrance;
                        break;
                    case Directions.L:
                        reqs.L |= ConnectionType.entrance;
                        if (nots.L.HasFlag(ConnectionType.entrance)) nots.L -= ConnectionType.entrance;
                        break;
                    case Directions.R:
                        reqs.R |= ConnectionType.entrance;
                        if (nots.R.HasFlag(ConnectionType.entrance)) nots.R -= ConnectionType.entrance;
                        break;
                }

                // Try to get options from Levels
                // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
                List<Level> options;
                bool isGameplay = true;
                options = GetOptionsOpen(area, reqs, nots, Levels);

                // If there are not any options in Levels
                if (options.Count == 0)
                {
                    options = GetOptionsOpen(area, reqs, nots, Connectors);    // Try to get options from Connectors
                    isGameplay = false;                                    // Set isGameplay to false since we are now using a connector
                }

                // Get random level from the list of options
                Level level = options[RNG.random.Next(0, options.Count)];

                // Create screen to place
                MapScreen screen;
                if (isGameplay)
                    screen = new MapScreen($"{area.ID}-{area.LevelCount + 1}", ScreenType.Level, level);
                else
                    screen = new MapScreen($"{area.ID}-x{area.ConnectorCount +1}", ScreenType.Connector, level);

                // increment the correct screen counter
                if (isGameplay)
                    area.LevelCount++;
                else area.ConnectorCount++;

                // place new screen and add the first open end(s)
                PlaceScreen(area, area.ECoords.First, area.ECoords.Second, screen);
                area.ChosenScreens.Add(screen);
                AddEnds(area, area.ECoords.First, area.ECoords.Second, level.MapConnections);

                // Place dot screens from first level to edge of map
                {
                    // This will not update the minbuilt or maxbuilt, so these may be trimmed from the map
                    // but they will prevent the generation from building over these screens
                    Pair dir = DirectionsEnum.ToVectorPair(area.EDir);
                    area.ECoords = DotsFromCellToEdge(area.Map, area.ECoords, dir); // adjusts the entry coords to (hopefully) the edge of the map
                }
            }

            // MAIN GENERATION LOOP
            while (area.LevelCount < area.LevelQuota -1)    // add levels until the quota is met ( minus 1 )
            {
                // Place next level
                {
                    // Draw a new map screen from the list of available levels and add it to the map at the position of an open end.

                    // This tells us whether or not to remove the level from its pool after placing
                    bool isGameplay = true;

                    // Get next OpenEnd to fill
                    Pair coords = SmartSelectOpenEnd(area);

                    // Get level connection requirements
                    CheckNeighbors(area, coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);

                    // Get options from pool of levels
                    List<Level> options;

                    // Try to get open options from Levels
                    options = GetOptionsOpen(area, reqs, nots, Levels);

                    // If there are not open options in Levels
                    if (options.Count == 0)
                    {
                        // Try to get open options from Connectors
                        options = GetOptionsOpen(area, reqs, nots, Connectors);
                        // set isGameplay to false since we are now using a connector
                        isGameplay = false;

                        // If there are not open options in Connectors
                        // Then try to get any kind of option from Connectors
                        if (options.Count == 0)
                        {
                            options = GetOptions(reqs, nots, Connectors);

                            // If there are still no options, we have a problem
                            if (options.Count == 0)
                            {
                                DebugPrintReqs(coords, reqs, nots);
                                throw new Exception("ran out of options during mapArea generation");
                            }
                        }
                    }

                    // Get random level from the list of options
                    Level level = options[RNG.random.Next(0, options.Count)];

                    // Create screen to place
                    MapScreen screen;
                    if (isGameplay)
                        screen = new MapScreen($"{area.ID}-{area.LevelCount + 1}", ScreenType.Level, level, directions);
                    else
                        screen = new MapScreen($"{area.ID}-x{area.ConnectorCount + 1}", ScreenType.Connector, level, directions);

                    // Place screen
                    PlaceScreen(area, coords.First, coords.Second, screen);
                    area.ChosenScreens.Add(screen);

                    // Add new ends after placement
                    AddEnds(area, coords.First, coords.Second, level.MapConnections);

                    // Remove the OpenEnd that we are replacing
                    area.OpenEnds.Remove(new Pair(coords.First, coords.Second));

                    // increment the correct screen counter
                    if (isGameplay)
                        area.LevelCount++;
                    else area.ConnectorCount++;

                    // Remove the newly placed screen from the list of screens available
                    //if (isGameplay)
                    //Levels.Remove(level);
                }

                // Check that the OpenEnds count has not dropped below 1
                if (area.OpenEnds.Count == 0)
                {
                    //Console.WriteLine($"Ran out of OpenEnds in area: {ID}");
                    PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                    throw new Exception($"Ran out of OpenEnds in area: {area.ID}");
                }
            }

            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");

            // PLACE FINAL LEVEL
            {
                // Select one OpenEnd as the AreaEnd
                // This will be used later as either the connection point to the next area, or as the path end.
                // Declare the areaEnd and set to (-1, -1)
                // If it gets returned as this value then there will be an error
                // Under normal operation this should not happen
                Pair areaEnd = new Pair(-1, -1);

                // Set distance to zero
                int dist = 0;

                MapConnections reqs = NoMapConnections;
                MapConnections nots = NoMapConnections;

                // Iterate over the list of open ends
                for (int i = 0; i < area.OpenEnds.Count; i++)
                {
                    // Get the next end to check
                    Pair coords = area.OpenEnds.ElementAt(i);

                    // Check that the end is on the edge of the map
                    bool onEdge = false;

                    switch (area.XDir) // Switch based on the build direction so we check the correct side
                    {
                        case Directions.U:
                            if (coords.First == area.MinBuilt.First)
                                onEdge = true;
                            break;
                        case Directions.D:
                            if (coords.First == area.MaxBuilt.First)
                                onEdge = true;
                            break;
                        case Directions.L:
                            if (coords.Second == area.MinBuilt.Second)
                                onEdge = true;
                            break;
                        case Directions.R:
                            if (coords.Second == area.MaxBuilt.Second)
                                onEdge = true;
                            break;
                    }

                    if (onEdge)
                    {
                        // Use checkneighbors to get the level requirements and directions of the level
                        CheckNeighbors(area, coords.First, coords.Second, out MapConnections treqs, out MapConnections tnots, out string directions);

                        // Find the end with the greatest distance
                        if (directions.Length > dist)
                        {
                            areaEnd = coords;           // Set the area end to the coords of the selected end
                            dist = directions.Length;   // Set the highest distance to the distance of the selected end
                            reqs = treqs;               // Set the current requirements and nots
                            nots = tnots;               // These will be used below
                        }
                    }
                }

                // Error if the areaEnd is (-1, -1)
                // This can occur if the only OpenEnd is a dead end
                if (areaEnd.First == -1)
                    throw new Exception($"Could not place final level in area {area.ID}.");

                // Set XCoords so we can see where the area end is
                area.XCoords = areaEnd;

                // Manually determine the exit requirements based on exit_dir (No multi-direction cases)
                switch(area.XDir)
                {
                    case Directions.D:
                        reqs.D |= ConnectionType.exit;      // require a downwards exit
                        nots.D = ConnectionType.none;       // Negate and down nots from above
                        break;
                    case Directions.U:
                        reqs.U |= ConnectionType.exit;      // require a upwards exit
                        nots.U = ConnectionType.none;       // Negate and up nots from above
                        break;
                    case Directions.R:
                        reqs.R |= ConnectionType.exit;      // require a right exit
                        nots.R = ConnectionType.none;       // Negate and right nots from above
                        break;
                    case Directions.L:
                        reqs.L |= ConnectionType.exit;      // require a left exit
                        nots.L = ConnectionType.none;       // Negate and left nots from above
                        break;
                }

                // Try to get options from Levels
                // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
                List<Level> options;
                bool isGameplay = true;
                options = GetOptionsClosed(reqs, nots, Levels);

                // If there are not any options in Levels
                if (options.Count == 0)
                {
                    options = GetOptionsClosed(reqs, nots, Connectors);     // Try to get options from Connectors
                    isGameplay = false;                                     // Set isGameplay to false since we are now using a connector
                }

                // Get random level from the list of options
                Level level = options[RNG.random.Next(0, options.Count)];

                // Create screen to place
                MapScreen screen;
                if (isGameplay) screen = new MapScreen($"{area.ID}-{area.LevelCount + 1}", ScreenType.Level, level);
                else            screen = new MapScreen($"{area.ID}-x{area.ConnectorCount + 1}", ScreenType.Connector, level);

                // increment the correct screen counter
                if (isGameplay) area.LevelCount++;
                else            area.ConnectorCount++;

                // place new screen
                PlaceScreen(area, area.XCoords.First, area.XCoords.Second, screen);
                area.ChosenScreens.Add(screen);

                // Remove the selected end so that it is not capped in a later step.
                area.OpenEnds.Remove(areaEnd);

                PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");

                // Ensure that the exit screen connects to the edge of the map
                {
                    Pair x_vec = DirectionsEnum.ToVectorPair(area.XDir);
                    area.XCoords = DotsFromCellToEdge(area.Map, area.XCoords, x_vec);
                    if (area.XCoords.First == -1) throw new Exception("Final level unable to connect to edge of map.");
                }
            }

            // If there is no next area, we need to instead place a path end screen, not just the usual final level

            // debug stuff
            Console.WriteLine($"Level Count: {area.LevelCount}");
            Console.WriteLine($"Connector Count: {area.OpenEnds.Count}");
            Console.WriteLine($"OpenEnds before capping: {area.OpenEnds.Count}");

            // Cap all Open Ends
            while (area.OpenEnds.Count != 0)
            {
                Pair coords = area.OpenEnds.First();
                CapOpenEnd(area, coords);
                area.OpenEnds.Remove(coords);
            }
            // Cap all Dead Ends
            while (area.DeadEntries.Count != 0)
            {
                Pair coords = area.DeadEntries.First();
                CapOpenEnd(area, coords);
                area.DeadEntries.Remove(coords);
            }

            // Build secret areas

            // Crop the map
            area.Map = GameMap.CropMap(area.MaxBuilt.First - area.MinBuilt.First + 1, area.MaxBuilt.Second - area.MinBuilt.Second + 1, area.Map, area.MinBuilt.First, area.MinBuilt.Second);

            // Adjust the ECoords and XCoords
            area.ECoords -= area.MinBuilt;
            area.XCoords -= area.MinBuilt;
            area.ECoords.First  = Math.Min(area.ECoords.First , area.MaxBuilt.First );
            area.ECoords.Second = Math.Min(area.ECoords.Second, area.MaxBuilt.Second);
            area.XCoords.First  = Math.Min(area.XCoords.First , area.MaxBuilt.First );
            area.XCoords.Second = Math.Min(area.XCoords.Second, area.MaxBuilt.Second);
            area.ECoords.First  = Math.Max(area.ECoords.First , 0);
            area.ECoords.Second = Math.Max(area.ECoords.Second, 0);
            area.XCoords.First  = Math.Max(area.XCoords.First , 0);
            area.XCoords.Second = Math.Max(area.XCoords.Second, 0);
        }
        static Pair SmartSelectOpenEnd(MapArea area)
        {
            // If there is only one end, use this end
            // If filling this end creates no new open ends, we will need to fix that.
            if (area.OpenEnds.Count == 1)
                return area.OpenEnds.First();

            // Iterate over the list of open ends
            for (int i = 0; i < area.OpenEnds.Count; i++)
            {
                // Count the number of empty neighbors each end has
                // If an end has no empty neighbors
                // Use this end to just get it out of the way
                // This will help us avoid getting stuck with it later
                Pair coords = area.OpenEnds.ElementAt(i);
                CheckNeighbors(area, coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);
                if (CountEmptyNeighbors(reqs, nots) == 0)
                {
                    return coords;
                }
            }

            // Declare new openEnd to return
            Pair openEnd = new Pair(-1, -1);
            // Declare distance and set to zero
            int dist = 0;

            // Iterate over the list of open ends again
            for (int i = 0; i < area.OpenEnds.Count; i++)
            {
                // This time we select the openEnd by greatest distance
                Pair coords = area.OpenEnds.ElementAt(i);
                CheckNeighbors(area, coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);
                if (directions.Length > dist)
                {
                    // If the distance is greater than what we had before, make this the openEnd to return
                    openEnd = coords;
                }
            }
            // If our new end has been changed from initial value
            // this probably should not ever be false
            if (openEnd.First != -1)
                return openEnd;

            // Otherwise, pick a random end
            return GetOpenEnd(area);
        }
        static Pair GetOpenEnd(MapArea area)
        {
            // Return a random OpenEnd from the list
            // Will error if there are no OpenEnds
            return area.OpenEnds.ElementAt(RNG.random.Next(0, area.OpenEnds.Count));
        }
        static void CheckNeighbors(MapArea area, int i, int j, out MapConnections reqs, out MapConnections nots, out string directions)  // returns the type necessary to meet needs of neighbors
        {
            // initialize reqs and nots to empty
            reqs = NoMapConnections;
            nots = NoMapConnections;
            string dirs = null;

            // map is row major, so i is the row and j is the column
            // get required entrances, and spots where entrances cant be

            CheckNeighbor(i - 1, j, ref reqs.U, ref nots.U, Directions.D); // Check Screen Up
            CheckNeighbor(i + 1, j, ref reqs.D, ref nots.D, Directions.U); // Check Screen Down
            CheckNeighbor(i, j - 1, ref reqs.L, ref nots.L, Directions.R); // Check Screen Left
            CheckNeighbor(i, j + 1, ref reqs.R, ref nots.R, Directions.L); // Check Screen Right
            directions = dirs;
            return;

            void CheckNeighbor(int k, int l, ref ConnectionType req, ref ConnectionType not, Directions dir)
            {
                // Must be inside map bounds and not be a secret end
                if (MapBoundsCheck(area.Map, k, l) && !area.SecretEnds.Contains(new Pair(k, l)))
                {
                    // Get the neighbor we want to check
                    MapScreen neighbor = area.Map.Data[k, l];
                    ConnectionType connection;

                    // ENTRANCES AND EXITS
                    // If the screen is not null, check the connections
                    // If it is null, it will not impose any requirements on entrances or exits
                    if (neighbor != null)
                    {
                        // Get the desired connection by specifying direction
                        connection = neighbor.Level.MapConnections.GetDirection(dir);

                        // If there is an exit (or both), require an entrance
                        if (connection.HasFlag(ConnectionType.exit))
                        {
                            // Set level requirements
                            req |= ConnectionType.entrance;
                            // Get directions to current level
                            // Set directions to new level if shorter than existing path or if no path is set
                            if (dirs == null || neighbor.Directions.Length < dirs.Length)
                                dirs = neighbor.Directions + (DirectionsEnum.Opposite(dir)).ToString();
                        }
                        // If there is only an entrance, require an exit
                        else if (connection.HasFlag(ConnectionType.entrance))
                            req |= ConnectionType.exit;
                        // If there is no connection, require a not on both
                        else not |= ConnectionType.both;

                        // Also, if the screen is not null, then we can't have a secret here
                        not |= ConnectionType.secret;
                    }

                    // SECRETS
                    // If we have an OpenEnd or DeadEntry in this location, we cannot have a secret here.
                    if (area.OpenEnds.Contains(new Pair(k, l))
                        || area.DeadEntries.Contains(new Pair(k, l)))
                    {
                        not |= ConnectionType.secret;
                    }
                }
                // If the screen we are looking at is invalid, we cannot connect to it at all.
                else not |= ConnectionType.all;  // all includes both entrances, exits, and secrets
            }
        }
        static void AddEnds(MapArea area, int i, int j, MapConnections mCons)
        {
            // This function calls AddEnd for each direction of connection
            // The index given is the screen that is Up, Down, Left, or Right from the screen just placed.
            // The ConnectionType given is the connection type of the transition leading into the given screen.

            AddEnd(new Pair(i - 1, j), mCons.U); // Check Screen Up
            AddEnd(new Pair(i + 1, j), mCons.D); // Check Screen Down
            AddEnd(new Pair(i, j - 1), mCons.L); // Check Screen Left
            AddEnd(new Pair(i, j + 1), mCons.R); // Check Screen Right
            return;

            void AddEnd(Pair index, ConnectionType con)
            {
                // This function adds the applicable End type for a given screen and connection type

                // OpenEnds    are placed on any exits     which lead to null screens
                // DeadEntries are placed on any entrances which lead to null screens
                // SecretEnds  are placed on any secret entrances (these are pre-checked for compatibility)

                // Exits are checked first because they take precedence over entrances
                if (con.HasFlag(ConnectionType.exit))
                {
                    // By the rules in CheckNeighbors, we shouldn't be trying to place an openEnd over an already existing secretEnd
                    // So the only other consideration is whether a DeadEntry already exists, which we can overwrite.

                    if (area.Map.Data[index.First, index.Second] == null)
                    {
                        // Remove deadEntry if there was one here
                        area.DeadEntries.Remove(index);

                        // Add OpenEnd
                        area.OpenEnds.Add(index);
                        UpdateMinMaxCoords(area, index.First, index.Second);
                    }
                }
                else if (con.HasFlag(ConnectionType.entrance))
                {
                    // If we have only an entrance here, try to add a deadEntry
                    // If there is already an openEnd here, don't add the deadEntry
                    if (MapBoundsCheck(area.Map, index.First, index.Second) && area.Map.Data[index.First, index.Second] == null)
                    {
                        if (!area.OpenEnds.Contains(index))
                        {
                            area.DeadEntries.Add(index);
                            UpdateMinMaxCoords(area, index.First, index.Second);
                        }
                    }
                }
                else if (con.HasFlag(ConnectionType.secret))
                {
                    // Add a secret end if applicable
                    // Should be no issues with adding this since we pre-check for other types of ends in this spot during CheckNeighbors
                    // For SecretEnds, we don't need to check if the screen at index is null
                    area.SecretEnds.Add(index);
                    UpdateMinMaxCoords(area, index.First, index.Second);
                }
            }
        }
        static Pair DotsFromCellToEdge(GameMap map, Pair coords, Pair dir)
        {
            // start at given coords + dir
            coords += dir;
            // loop while the cell being checked is within the bounds
            while (MapBoundsCheck(map, coords.First, coords.Second))
            {
                if (map.Data[coords.First, coords.Second] != null       // check if we have hit an occupied cell
                 && map.Data[coords.First, coords.Second] != DotScreen) // we are ok to overwrite dots with more dots
                    return new Pair(-1, -1);

                map.Data[coords.First, coords.Second] = DotScreen;  // set the mapscreen to be dots (this will prevent building over it, and connect the desired cell to the edge of the map)
                coords += dir;  // move in direction by adding dir pair to coords being checked
            }
            coords -= dir; // get ourselves back on the map before returning coords
            return coords;
        }
        static void CapOpenEnd(MapArea area, Pair coords)
        {
            // Check the neighbors to get the requirements
            CheckNeighbors(area, coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);

            // Get options from pool of screens
            List<Level> options = GetOptionsClosed(reqs, nots, Connectors);

            // Get random level from the list of options
            Level level = options[RNG.random.Next(0, options.Count)];

            // Place screen
            MapScreen screen = new MapScreen($"{area.ID}-x{area.ConnectorCount+1}", ScreenType.Connector, level, directions);
            area.ConnectorCount++;
            PlaceScreen(area, coords.First, coords.Second, screen);
            area.ChosenScreens.Add(screen);
        }
        static bool MapBoundsCheck(GameMap map, int i, int j)
        {
            if (i >= 0 && j >= 0 && i < map.Height && j < map.Width)
                return true;
            return false;
        }
        static bool PlaceScreen(MapArea area, int i, int j, MapScreen screen)
        {
            // Check that the index is within the map's bounds
            if (MapBoundsCheck(area.Map, i, j))
            {
                // Print debug output to console
                if (area.Map.Data[i, j] != null)
                    Console.WriteLine($"Overwrote a cell at {i}, {j}");
                
                // Place the screen into the map
                area.Map.Data[i, j] = screen;

                UpdateMinMaxCoords(area, i, j);

                return true;
            }
            return false;
        }
        static void UpdateMinMaxCoords(MapArea area, int i, int j)
        {
            // Update the minbuilt and maxbuilt coords
            area.MinBuilt.First  = Math.Min(area.MinBuilt.First,  i);
            area.MinBuilt.Second = Math.Min(area.MinBuilt.Second, j);
            area.MaxBuilt.First  = Math.Max(area.MaxBuilt.First,  i);
            area.MaxBuilt.Second = Math.Max(area.MaxBuilt.Second, j);
        }

        static List<Level> GetOptions(MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            List<Level> options = new List<Level>();
            foreach (Level level in pool)
            {
                if ((level.MapConnections & reqs) == reqs)
                    if ((level.MapConnections & nots) == NoMapConnections)
                        options.Add(level);
            }
            return options;
        }
        static List<Level> GetOptionsOpen(MapArea area, MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            // Gets Options the standard way, but also...
            List<Level> options = GetOptions(reqs, nots, pool);

            // Create new list of options
            List<Level> newOptions = new List<Level>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.MapConnections, reqs);

                // Skip levels which build in disallowed directions
                if ((area.NoBuild.HasFlag(Directions.U) && mc.U.HasFlag(ConnectionType.exit))) continue;
                if ((area.NoBuild.HasFlag(Directions.D) && mc.D.HasFlag(ConnectionType.exit))) continue;
                if ((area.NoBuild.HasFlag(Directions.L) && mc.L.HasFlag(ConnectionType.exit))) continue;
                if ((area.NoBuild.HasFlag(Directions.R) && mc.R.HasFlag(ConnectionType.exit))) continue;

                // Spare the level if any new exits are added
                if      (mc.U.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.D.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.L.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.R.HasFlag(ConnectionType.exit)) newOptions.Add(level);
            }
            return newOptions;
        }
        // Static functions
        static List<Level> GetOptionsClosed(MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            // Gets Options the standard way, but also...
            List<Level> options = GetOptions(reqs, nots, pool);

            // Create new list of options
            List<Level> newOptions = new List<Level>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.MapConnections, reqs);

                // Skip the level if any new exits are added
                if (mc.U != 0b0000) continue;
                if (mc.D != 0b0000) continue;
                if (mc.L != 0b0000) continue;
                if (mc.R != 0b0000) continue;

                // Otherwise, add it to the new list
                newOptions.Add(level);
            }
            return newOptions;
        }
        static MapConnections GetUniqueConnections(MapConnections cons, MapConnections reqs)
        {
            MapConnections ret = NoMapConnections;

            // If the requirements impose anything on a given side, then that whole side is thrown out
            // If that side is empty, then that side contains unique connections
            if (reqs.U == ConnectionType.none) ret.U = cons.U;
            if (reqs.D == ConnectionType.none) ret.D = cons.D;
            if (reqs.L == ConnectionType.none) ret.L = cons.L;
            if (reqs.R == ConnectionType.none) ret.R = cons.R;

            return ret;
        }
        static int CountEmptyNeighbors(MapConnections reqs, MapConnections nots)
        {
            // Set counter to zero
            int val = 0;

            // If there are any directions where no requirements are imposed, then we have no neighbor there.
            // This also means we have the potential for an OpenEnd.
            if ((reqs.U | nots.U) == ConnectionType.none) val++;
            if ((reqs.D | nots.D) == ConnectionType.none) val++;
            if ((reqs.L | nots.L) == ConnectionType.none) val++;
            if ((reqs.R | nots.R) == ConnectionType.none) val++;

            // Return the number of empty neighbors counted.
            return val;
        }
        public static void DebugPrintReqs(Pair coords, MapConnections reqs, MapConnections nots)
        {
            Console.WriteLine($"Unable to place level at {coords.First}, {coords.Second}");
            Console.WriteLine($"reqs:");
            Console.WriteLine($"\tU: {reqs.U}");
            Console.WriteLine($"\tD: {reqs.D}");
            Console.WriteLine($"\tL: {reqs.L}");
            Console.WriteLine($"\tR: {reqs.R}");
            Console.WriteLine($"nots:");
            Console.WriteLine($"\tU: {nots.U}");
            Console.WriteLine($"\tD: {nots.D}");
            Console.WriteLine($"\tL: {nots.L}");
            Console.WriteLine($"\tR: {nots.R}");
        }

        public static GameMap CombineMapsOld(MapArea A1, MapArea A2, Pair M1Exit, Pair M2Entry, Directions BD1, Directions BD2, out Pair M1Origin)
        {
            // This function will combine the two maps entered as parameters into a single map
            // The given coords are used as the point at which the levels are joined

            // The first coord should be the exit coord of the first map + the offset of where the first screen of the second map should be placed
            // It is important not to forget to add this offset before passing in the first coord parameter
            // The second coord is the location of the first screen (entry coord) of the second map

            // Get maps directly
            GameMap M1 = A1.Map, 
                    M2 = A2.Map;

            // Declare map to be returned
            GameMap newMap;

            // Initialize origin coords for each map
            // M1Origin should be initialized when being passed in
            M1Origin = new Pair();
            Pair M2Origin = new Pair();

            // Initialize dimensions for new map
            int height = 0, width = 0;

            // Initialize connection requirements for connecting level
            MapConnections reqs = NoMapConnections;
            MapConnections nots = NoMapConnections;

            // Connect maps with same BuildDirection
            if (BD1 == BD2)
            {
                // This process is basically the same as what is used in the level generator

                // Determine the origin/offset of each map based on direction
                switch (BD1)
                {
                    case Directions.U:
                        if (M1Exit.Second < M2Entry.Second)                    // If M1Exit is higher than M2Entry
                            M1Origin.Second = M2Entry.Second - M1Exit.Second;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.Second = 0;                              // Otherwise, M1Origin y val is 0
                        M1Origin = M2Origin - M1Exit + M2Entry;                // M2Origin = M1Origin + Difference of M1Exit and M2Entry
                        M1Origin.First++;                                      // Shift M2Entry to the right by one
                        width = Math.Max(M1Origin.Second + M1.Width, M2Origin.Second + M2.Width);
                        height = M2Entry.First + M1.Height - M1Exit.First + 1;
                        break;

                    case Directions.D:
                        if (M1Exit.Second < M2Entry.Second)                    // If M1Exit is higher than M2Entry
                            M1Origin.Second = M2Entry.Second - M1Exit.Second;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.Second = 0;                              // Otherwise, M1Origin y val is 0
                        M2Origin = M1Origin + M1Exit - M2Entry;                // M2Origin = M1Origin + Difference of M1Exit and M2Entry
                        M2Origin.First++;                                      // Shift M2Entry to the right by one
                        width = Math.Max(M1Origin.Second + M1.Width, M2Origin.Second + M2.Width);
                        height = M1Exit.First + M2.Height - M2Entry.First + 1;
                        break;

                    case Directions.L:
                        if (M1Exit.First < M2Entry.First)                   // If M1Exit is higher than M2Entry
                            M1Origin.First = M2Entry.First - M1Exit.First;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.First = 0;                            // Otherwise, M1Origin y val is 0
                        M1Origin = M2Origin - M1Exit + M2Entry;             // 
                        M1Origin.Second++;                                  // Shift M1Origin to the left by one
                        height = Math.Max(M1Origin.First + M1.Height, M2Origin.First + M2.Height);
                        width = M2Entry.Second + M1.Width - M1Exit.Second + 1;
                        break;

                    case Directions.R:
                        if (M1Exit.First < M2Entry.First)                   // If M1Exit is higher than M2Entry
                            M1Origin.First = M2Entry.First - M1Exit.First;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.First = 0;                            // Otherwise, M1Origin y val is 0
                        M2Origin = M1Origin + M1Exit - M2Entry;             // M2Origin = M1Origin + Difference of M1Exit and M2Entry
                        M2Origin.Second++;                                  // Shift M2Origin to the right by one
                        height = Math.Max(M1Origin.First + M1.Height, M2Origin.First + M2.Height);
                        width = M1Exit.Second + M2.Width - M2Entry.Second + 1;
                        break;

                    default:
                        throw new Exception("Error combining maps: Invlaid Direction.");
                }

                // Create new map with required size
                newMap = new GameMap(height, width);

                // Combine the maps by pasting into new map
                GameMap.CopyToMapAtCoords(M1, newMap, M1Origin);
                GameMap.CopyToMapAtCoords(M2, newMap, M2Origin);

                // Return the new, combined GameMap
                return newMap;
            }

            // Connect maps with perpendicular BuildDirections
            Directions bdSum = BD1 | BD2;

            // Check for conflicting directions
            if (bdSum.HasFlag(Directions.R | Directions.L) || bdSum.HasFlag(Directions.U | Directions.D))
                throw new Exception("Error combining maps: Conflicting Directions.");

            // The current method is a bit space inefficient, since it may produce large empty spaces in the connecting corners
            // These spaces will get very large if building in spirals

            Pair ConCoords = new Pair();    // Declare the coords of the connecting screen
            Pair ConVec = new Pair();       // Declare a pair for the directions of the .. fill

            // Determine how maps will be connected based on directions
            // Switch based on the directions being connected
            // The order of these actually matters
            switch (BD1)
            {
                case Directions.R when BD2 == Directions.U:
                    
                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Origin.First + M1Exit.First, M2Origin.Second + M2Entry.Second);

                    // Set the width and height
                    height = M1Origin.First  + M1.Height;
                    width  = M2Origin.Second + M2.Width ;

                    // Set the fill vector
                    ConVec = new Pair(-1, -1);

                    // Set the requirements
                    reqs.L |= ConnectionType.both;
                    reqs.U |= ConnectionType.both;

                    break;

                case Directions.R when BD2 == Directions.D:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Exit.First, M2Origin.Second + M2Entry.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M2Origin.Second + M2.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, -1);

                    // Set the requirements
                    reqs.L |= ConnectionType.both;
                    reqs.D |= ConnectionType.both;

                    break;

                case Directions.L when BD2 == Directions.U:

                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Origin.First + M1Exit.First, M2Entry.Second);

                    // Set the width and height
                    height = M1Origin.First + M1.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(-1, 1);

                    // Set the requirements
                    reqs.R |= ConnectionType.both;
                    reqs.U |= ConnectionType.both;

                    break;

                case Directions.L when BD2 == Directions.D:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Exit.First, M2Entry.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, 1);

                    // Set the requirements
                    reqs.R |= ConnectionType.both;
                    reqs.D |= ConnectionType.both;

                    break;

                case Directions.U when BD2 == Directions.R:

                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Entry.First, M1Exit.Second);

                    // Set the width and height
                    height = M1Origin.First + M1.Height;
                    width = M2Origin.Second + M2.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, 1);

                    // Set the requirements
                    reqs.D |= ConnectionType.both;
                    reqs.R |= ConnectionType.both;

                    break;

                case Directions.U when BD2 == Directions.L:

                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Entry.First, M1Origin.Second + M1Exit.Second);

                    // Set the width and height
                    height = M1Origin.First + M1.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, -1);

                    // Set the requirements
                    reqs.D |= ConnectionType.both;
                    reqs.L |= ConnectionType.both;

                    break;

                case Directions.D when BD2 == Directions.R:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Origin.First + M2Entry.First, M1Origin.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M2Origin.Second + M2.Width;

                    // Set the fill vector
                    ConVec = new Pair(-1, 1);

                    // Set the requirements
                    reqs.U |= ConnectionType.both;
                    reqs.R |= ConnectionType.both;

                    break;

                case Directions.D when BD2 == Directions.L:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Origin.First + M2Entry.First, M1Origin.Second + M1Exit.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(-1, -1);

                    // Set the requirements
                    reqs.U |= ConnectionType.both;
                    reqs.L |= ConnectionType.both;

                    break;
            }

            // Create new map with required size
            newMap = new GameMap(height, width);

            // Combine the maps by pasting into new map
            GameMap.CopyToMapAtCoords(M1, newMap, M1Origin);
            GameMap.CopyToMapAtCoords(M2, newMap, M2Origin);

            // Fill in the ..s from the connector level to the entrance and exit
            {
                // Vertical Fill
                int i = ConCoords.First + ConVec.First;
                while (true)
                {
                    // Check that the index is not out of range
                    if (!MapBoundsCheck(newMap, i, ConCoords.Second)) break;

                    // Check that the cell is not occupied
                    if (newMap.Data[i, ConCoords.Second] != null) break;

                    // fill the cell with the given mapscreen (this is usually the .. level)
                    newMap.Data[i, ConCoords.Second] = DotScreen;

                    // Move to next screen
                    i += ConVec.First;
                }
                // Horizontal Fill
                int j = ConCoords.Second + ConVec.Second;
                while (true)
                {
                    // Check that the index is not out of range
                    if (!MapBoundsCheck(newMap, ConCoords.First, j)) break;

                    // Check that the cell is not occupied
                    if (newMap.Data[ConCoords.First, j] != null) break;

                    // fill the cell with the given mapscreen (this is usually the .. level)
                    newMap.Data[ConCoords.First, j] = DotScreen;

                    // Move to next screen
                    j += ConVec.Second;
                }
            }

            // Place area connector level
            // The connector screens are added to the areaInfo for A1
            {
                List<Level> options = GetOptionsClosed(reqs, nots, Connectors);     // Try to get options from Connectors
                Level level = options[RNG.random.Next(0, options.Count)];           // Get random level from the list of options
                
                MapScreen screen = new MapScreen($"AC-{A1.ID}", ScreenType.Connector, level);  // Create screen to place

                A1.ConnectorCount++;   // Increment the connector screen counter

                newMap.Data[ConCoords.First, ConCoords.Second] = screen;   // Place new screen

                A1.ChosenScreens.Add(screen);  // Add the screen to ChosenScreens
            }

            // Return the new, combined GameMap
            return newMap;
        }
        public static void GenerateAreaDataFiles(MapArea area)
        {
            // Open StreamWriters for the data files
            StreamWriter levelinfo_file = File.AppendText(Randomizer.saveDir + "data/levelinfo.txt.append");
            StreamWriter tilesets_file  = File.AppendText(Randomizer.saveDir + "data/tilesets.txt.append");

            // Create randomized area tileset
            if (area.Name == null) area.Name = Randomizer.GetFunnyAreaName();

            // Write the area's tileset to the file
            tilesets_file.WriteLine($"{area.ID} {{");
            area.Tileset.WriteTileset(tilesets_file);

            // Iterate over all of the screen in the area
            for (int i = 0; i < area.ChosenScreens.Count; i++)
            {
                // Get the next screen from the list
                MapScreen screen = area.ChosenScreens[i];

                // Skip over screens of Dot type
                if (screen.Type == ScreenType.Dots) continue;

                // Load the level file associated with that screen/level
                LevelFile level_file = LevelManip.Load(screen.Level.InFile);

                // Flip the level horizontally if it supposed to be
                if (screen.Level.FlippedHoriz)
                    LevelManip.FlipLevelH(ref level_file);

                if (Settings.DoCorruptions)
                    screen.Level.TSNeed.extras += LevelCorruptors.CorruptLevel(ref level_file);

                // Save the levelfile
                LevelManip.Save(level_file, Randomizer.saveDir + $"tilemaps/{screen.ID}.lvl");

                // Write LevelInfo
                if (screen.Type == ScreenType.Level)
                    levelinfo_file.WriteLine($"\"{screen.ID}\" {{name=\"{area.Name} {screen.LevelSuffix}\" id={screen.LevelSuffix}}}");
                else levelinfo_file.WriteLine($"\"{screen.ID}\" {{name=\"{area.Name}\" id=-1}}");

                // Write Level Tileset
                // Calculate the final tileset
                // The tilesets are added in order of priority, from lowest to highest
                Tileset level_tilset = (screen.Level.TSDefault + area.Tileset) + screen.Level.TSNeed;

                // Write level tileset to the file
                tilesets_file.WriteLine($"{screen.LevelSuffix} {{");
                tilesets_file.WriteLine($"#filename: {screen.Level.InFile}");
                level_tilset.WriteTileset(tilesets_file);
                tilesets_file.WriteLine("}");
            }

            // write closing bracket for area tileset
            tilesets_file.WriteLine("}\n");

            levelinfo_file.Close();
            tilesets_file.Close();

            // NPCS
            



        }
        public static void GenerateGlobalDataFiles()
        {
            // Worldmap
            {
                StreamWriter worldmap_file = File.AppendText(Randomizer.saveDir + "data/worldmap.txt");
                // write pages info
                worldmap_file.Write("pages {\n" +
                    "lightworld { collectibles 238 }\n" +
                    "darkworld { collectibles 7 }\n" +
                    "console { collectibles 0 }\n}");

                // write meta
                {
                    worldmap_file.Write("meta {\n\n");
                    worldmap_file.Write($"default_node TheEnd"); 
                    worldmap_file.Write("\n}\n");
                }

            }
        }

        // Entrance Normalization Functions
        /*public static int[] GetTransitionSizes(ref LevelFile level)
        {


            return 
        }*/
        public static void CountTransitionTiles(ref LevelFile level, Directions anchorflips)
        {
            // Normalizes all transition tags in a level

            // TODO:
            // need to add way to track when an entrance has already been normalized, otherwise we will basically count every entrance's tags twice.

            int step = level.header.width;

            // Iterates over the four directions
            for (int i = 0; i < 4; i++)
            {
                // Direction is indexed by bit shift
                Directions direction = (Directions)(1 << i);

                // Find transition tags
                int index1 = LevelManip.FindFirstTileByID(ref level, LevelFile.TAG, TransitionTagIDs[i]);

                // Skip side if none found
                if (index1 == -1) continue;

                // Get all transition tags as list of indeces
                if (i >= 2) step = 1;
                List<int> tagList = LevelManip.FindTilesByID(ref level, LevelFile.TAG, TileID.GreenTransitionR, index1, step);

                // Calculate new entry dimension
                //int newEntryDimension = Math.Min(tagList.Count, neighborTagList.Count);

                // Remove extraneous entry tags in left/top level
                bool flip = false;
                if ((anchorflips & direction) != 0) flip = true;    // reverse the set if anchorflips contains direction
                //ReplaceEntryTags(ref level, tagList, newEntryDimension, flip);
            }




        }
        public static void ReplaceEntryTags(ref LevelFile level, List<int> set, int newEntryDimension, bool reverse)
        {
            // Replaces tiles in the set, works for horizontal or vertical connections

            // Calculate how many tiles to replace
            int toReplace = set.Count() - newEntryDimension;
            if (toReplace <= 0)
                return;

            // Reverse the list if the anchor is flipped
            if (reverse) set.Reverse();

            // Iterate over set of tiles
            for (int i = 0; i < toReplace; i++)
            {
                int index = set[i];

                // Remove the transition tag
                level.data[LevelFile.TAG, index] = TileID.Empty;

                // Place invisible solid where tag was
                level.data[LevelFile.ACTIVE, index] = TileID.Invisible;
            }
        }
        
        // Deprecated by FindTileByID in LevelManip
        /*public static List<int> GetEntryTags(ref LevelFile level, TileID id, int index, Directions ConDir)
        {
            // Get basic level info
            int lw = level.header.width;
            int lh = level.header.height;
            int size = lw * lh;

            // Set the value to iterate by (iteration moves in same direction as anchor since first tiles are replaced)
            int it = lw;                                                    // Defaults to iterating by level width (one row)
            if ((ConDir & (Directions.U | Directions.D)) != 0) index = 1;   // If this is a vertical connection, iterate by 1 instead

            // Create the list of adjacent tile indeces
            List<int> adjacents = new List<int>();

            // loop until break
            while (true)
            {
                index += it;
                if (index < size && level.data[LevelFile.ACTIVE, index] == id) // Check the tile is within level bounds and is the same id
                {
                    adjacents.Add(index);
                    continue;
                }
                break;  // Break if index is OOB or id is different
            }
            return adjacents;   // Return the list of transition tiles
        }*/

        // These functions are currently empty
        static void CreateMoreOpenEnds()
        {
            MapScreen screen = GetScreenToReplace();
            ReplaceScreen(screen);
        }
        static MapScreen GetScreenToReplace()
        {
            // Search the list of MapScreens in CurrentArea's list of screens
            // Look for levels with empty neighbors
            // Pick from levels with the most empty neighbors
            // Prioritize levels with greater distance value
            return null;
        }
        static void ReplaceScreen(MapScreen screen)
        {
            // remove the mapscreen from the map
            // remove the mapscreen from CurrentArea's list of screens
            // if the removed level was a gameplay level
            // place the level back into the pool
            // place a new screen in the now empty space
            // the new requirements must exceed the connections of the old screen
            // we use GetOptionsOpen() so new openEnds are created
        }

        // Debug functions for testing purposes
        public static void PrintDebugCSV(MapArea area, string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < area.Map.Height; i++)
                {
                    for (int j = 0; j < area.Map.Width; j++)
                    {
                        
                        if (area.Map.Data[i, j] != null)
                        {
                            if (area.Map.Data[i, j].Type == ScreenType.Dots)
                                sw.Write("..");
                            else
                            {
                                sw.Write($"{DebugGetChar(area.Map.Data[i, j].Level.MapConnections)}");
                                if (area.Map.Data[i, j].Type == ScreenType.Level)
                                    sw.Write($"g");
                                else if (area.Map.Data[i, j].Type == ScreenType.Connector)
                                    sw.Write($"c");
                            }
                        }
                        if (area.OpenEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"O");
                        }
                        if (area.DeadEntries.Contains(new Pair(i, j)))
                        {
                            sw.Write($"D");
                        }
                        if (area.SecretEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"S");
                        }
                        if (area.ECoords.Equals(new Pair(i, j)))
                        {
                            sw.Write($"E");
                        }
                        if (area.XCoords.Equals(new Pair (i, j)))
                        {
                            sw.Write($"X");
                        }
                        sw.Write(",");
                    }
                    sw.Write('\n');
                }
            }
        }
        public static void PrintDebugCSV(GameMap map, string path, List<Tuple<Pair, string>> features = null)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < map.Height; i++)
                {
                    for (int j = 0; j < map.Width; j++)
                    {
                        if (map.Data[i, j] != null)
                        {
                            if (map.Data[i, j].Type == ScreenType.Dots)
                                sw.Write("..");
                            else
                            {
                                sw.Write($"{DebugGetChar(map.Data[i, j].Level.MapConnections)}");
                                if (map.Data[i, j].Type == ScreenType.Level)
                                    sw.Write($"g");
                                else if (map.Data[i, j].Type == ScreenType.Connector)
                                    sw.Write($"c");
                            }
                        }
                        if (features != null)
                        {
                            foreach (var tuple in features)
                            {
                                if (tuple.Item1 == new Pair(i, j))
                                    sw.Write(tuple.Item2);
                            }
                        }
                        sw.Write(",");
                    }
                    sw.Write('\n');
                }
            }
        }
        public static void PrintCSV(GameMap map, string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < map.Height; i++)
                {
                    for (int j = 0; j < map.Width; j++)
                    {
                        if (map.Data[i, j] != null)
                        {
                            if (map.Data[i, j].Type == ScreenType.Dots)
                                sw.Write("..");
                            else
                            {
                                sw.Write($"{map.Data[i, j].ID}.lvl");
                            }
                        }
                        sw.Write(",");
                    }
                    sw.Write('\n');
                }
            }
        }
        static char DebugGetChar(MapConnections con)
        {
            // convert to simpler representation in form of old connections enum
            Directions con2 = Directions.None;

            if (con.U != ConnectionType.none)
                con2 |= Directions.U;
            if (con.D != ConnectionType.none)
                con2 |= Directions.D;
            if (con.L != ConnectionType.none)
                con2 |= Directions.L;
            if (con.R != ConnectionType.none)
                con2 |= Directions.R;

            // use old switch case to get char
            switch (con2)
            {
                case Directions.R:
                    return '←';
                case Directions.L:
                    return '→';
                case Directions.L | Directions.R:
                    return '─';
                case Directions.U:
                    return '↓';
                case Directions.U | Directions.R:
                    return '└';
                case Directions.U | Directions.L:
                    return '┘';
                case Directions.U | Directions.L | Directions.R:
                    return '┴';
                case Directions.D:
                    return '↑';
                case Directions.D | Directions.R:
                    return '┌';
                case Directions.D | Directions.L:
                    return '┐';
                case Directions.D | Directions.L | Directions.R:
                    return '┬';
                case Directions.D | Directions.U:
                    return '│';
                case Directions.D | Directions.U | Directions.R:
                    return '├';
                case Directions.D | Directions.U | Directions.L:
                    return '┤';
                case Directions.All:
                    return '┼';
                default:
                    return ' ';
            }
        }
    }
}
