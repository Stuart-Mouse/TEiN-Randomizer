using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEiNRandomizer
{
    public static partial class Randomizer
    {
        // MapArea class is used to keep track of map areas as they are created/placed
        // The main map generation routines have been moved from the map generator class into the mapArea class

        // CONSTANTS
        static readonly MapScreen DotScreen = new MapScreen(null, "..", ScreenType.Dots, null);
        public static readonly MapConnections NoMapConnections;

        // STATIC MEMBERS

        // Dictionary of the MapAreas where the area name is the key
        private static Dictionary<string, MapArea> MapAreas;

        // The area defs to be used in generation
        // When referenced by an AreaEnd, they will be loaded into the queue for building
        //static List<AreaDef> AreaDefs;

        // For keeping track of keys
        static int KeysPlaced;
        static int LocksPlaced;

        // array used to store tag id for each direction so we can iterate over the directions easily
        // This is initialized every time this function is run, so this is probably not optimal
        static readonly TileID[] TransitionTagIDs = { TileID.GreenTransitionR, TileID.GreenTransitionL, TileID.GreenTransitionD, TileID.GreenTransitionU };

        // Level pools are shared in common amongst all areas by default
        // These can be overriden though
        public static List<Level> StandardLevels = new List<Level>();
        public static List<Level> CartLevels     = new List<Level>();
        public static List<Level> Connectors     = new List<Level>();
        public static List<Level> Secrets        = new List<Level>();
        // This is a special pool containing friend orb levels for ruin escape areas
        public static List<Level> FriendOrb      = LevelPool.LoadPool("data/level_pools/.mapgen/FriendOrb.gon").Levels;

        public static GameMap GenerateGameMap()
        {
            // Load Area Definitions
            MapAreas = new Dictionary<string, MapArea>();
            string lightspawn_id = null;
            string darkspawn_id  = null;
            {
                string path = "data/text/area_defs/test.gon";
                GonObject file = GonObject.Load(path);

                // Load area defintions
                GonObject areas = file["areas"];
                for (int i = 0; i < areas.Size(); i++)
                {
                    // Get area gon from file
                    GonObject area_def = areas[i];

                    // Create the new mapArea
                    MapArea area = new MapArea();

                    area.ID = area_def.GetName();   // get area id
                    if (area_def.TryGetChild("name", out GonObject name)) area.Name = name.String();            // get area name
                    if (area_def.TryGetChild("use_ts_of", out GonObject ts_ref)) area.TSRef = ts_ref.String();  // get area tileset ref

                    // Set level tileset defaults and needs if not null
                    if (area_def.TryGetChild("tileset", out GonObject gonTileset))
                    {
                        if (gonTileset.TryGetChild("default", out GonObject gonDefault))    // add defaults
                            area.Tileset += new Tileset(gonDefault);
                        if (area.TSRef == null)                                             // add randomized tileset if no tsref (preserves bg for loaded/split areas)
                            area.Tileset += TilesetManip.GetTileset();
                        if (gonTileset.TryGetChild("need", out GonObject gonNeed))          // add needs
                            area.Tileset += new Tileset(gonNeed);
                    }
                    else area.Tileset = TilesetManip.GetTileset();                          // get randomized tsdefault

                    // Get tags
                    if (area_def.TryGetChild("tags", out GonObject tags_gon))
                    {
                        area.tags = GonObject.Manip.ToStringArray(tags_gon);

                        if (area.tags.Contains("mainworlds"))
                            WorldMap.mainworlds += $"{area.ID} ";
                        if (area.tags.Contains("lightspawn"))
                            lightspawn_id = area.ID;
                        if (area.tags.Contains("darkspawn"))
                        {
                            darkspawn_id = area.ID;
                            area.Tileset.area_type = "dark";
                        }
                        if (area.tags.Contains("dark"))
                        {
                            area.Tileset.area_type = "dark";
                        }
                        if (area.tags.Contains("cart"))
                        {
                            area.Tileset.area_type = "cart";
                            WorldMap.cartworlds += $"{area.ID} ";
                            area.IsStandalone = true;
                            area.Levels = CartLevels;
                        }
                        if (area.tags.Contains("ironcart"))
                        {
                            area.Tileset.area_type = "ironcart";
                            WorldMap.cartworlds += $"{area.ID} ";
                            area.IsStandalone = true;
                        }
                        if (area.tags.Contains("glitch"))
                        {
                            area.Tileset.area_type = "glitch";
                            WorldMap.cartworlds += $"{area.ID} ";
                            area.IsStandalone = true;
                        }
                        if (area.tags.Contains("steven"))
                        {
                            area.IsStandalone = true;
                        }
                    }

                    // Get area exit
                    if (area_def.TryGetChild("exit", out GonObject exit)) area.ExitID = exit.String();
                    if (area.ExitID == area.ID) throw new Exception("Areas cannot list self as next area id.");

                    // Set area settings
                    area.AreaSettings = null;

                    // set the entrance direction if present
                    if (area_def.Contains("entrance_dir"))
                        area.EDir = DirectionsEnum.FromString(area_def["entrance_dir"].String());

                    // Set generation type, do type specific data loading
                    switch (area_def["gen_type"].String())
                    {
                        case "standard":
                            {
                                area.Type = GenerationType.Standard;
                                area.Name = GetFunnyAreaName();
                                area.LevelQuota = area_def["levels"].Int();
                                area.MaxSize.First = area_def["bounds"][0].Int();
                                area.MaxSize.Second = area_def["bounds"][1].Int();
                                if (area_def.TryGetChild("anchor"  , out GonObject anchor  )) area.Anchor  = DirectionsEnum.FromString(  anchor.String());
                                if (area_def.TryGetChild("no_build", out GonObject no_build)) area.NoBuild = DirectionsEnum.FromStringArray(GonObject.Manip.ToStringArray(no_build));
                                if (area_def.TryGetChild("exit_dir", out GonObject exit_dir)) area.XDir    = DirectionsEnum.FromString(exit_dir.String());
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

                // Second pass loading and pre-check area connections. We don't want to waste time generating areas if the connections will fail later
                foreach(var item in MapAreas)
                {
                    MapArea area = item.Value;

                    if (area.TSRef != null)
                    {
                        if (!MapAreas.TryGetValue(area.TSRef, out MapArea ref_area))
                            throw new Exception($"Area {area.ID} references tileset of area {area.TSRef} which could not be found.");
                        area.Tileset += ref_area.Tileset;
                    }

                    switch (area.Type)
                    {
                        case GenerationType.Standard:
                            if (area.XDir == Directions.None) break;    // This will be the case if the ExitID is a special case and not another area id
                            CheckConnection(area.ExitID, area.XDir);
                            break;
                        case GenerationType.Loaded:
                        case GenerationType.Split:
                            CheckConnection(area.XUp   , Directions.U);
                            CheckConnection(area.XDown , Directions.D);
                            CheckConnection(area.XLeft , Directions.L);
                            CheckConnection(area.XRight, Directions.R);
                            break;
                    }

                    void CheckConnection(string next_area_id, Directions dir)
                    {
                        if (next_area_id != null)
                        {
                            if (!MapAreas.TryGetValue(next_area_id, out MapArea next_area))
                                throw new Exception($"Area {area.ID} exits to area {next_area_id} which could not be found.");
                            if (dir != DirectionsEnum.Opposite(next_area.EDir))
                                throw new Exception($"Area {area.ID} exit direction does not match area {next_area_id} entrance direction.");
                        }
                    }
                }
            }

            // Generate All Areas
            foreach (var item in MapAreas)
            {
                MapArea area = item.Value;
                // Switch on area generation type
                switch (area.Type)
                {
                    case GenerationType.Standard:
                        {
                            // PRELIMINARIES
                            // Initialize Ends
                            area.OpenEnds    = new Dictionary<Pair, MapArea.OpenEnd>();
                            area.DeadEntries = new Dictionary<Pair, MapArea.OpenEnd>();
                            area.SecretEnds  = new Dictionary<Pair, MapArea.OpenEnd>();
                            // Init Map and associated trackers
                            area.Map = new GameMap(area.MaxSize.First, area.MaxSize.Second);  // Initialize map to size of bounds in def
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

                                // Set first level coord and nots based on anchor
                                switch (area.Anchor)
                                {
                                    // Cardinals
                                    case Directions.U:
                                        nots.UL |= ConnectionType.all;
                                        nots.U  |= ConnectionType.all;
                                        nots.UR |= ConnectionType.all;
                                        area.ECoords.First = 0;
                                        break;
                                    case Directions.D:
                                        nots.DR |= ConnectionType.all;
                                        nots.D  |= ConnectionType.all;
                                        nots.DL |= ConnectionType.all;
                                        area.ECoords.First = area.MaxSize.First - 1;
                                        break;
                                    case Directions.L:
                                        nots.UL |= ConnectionType.all;
                                        nots.L  |= ConnectionType.all;
                                        nots.DL |= ConnectionType.all;
                                        area.ECoords.Second = 0;
                                        break;
                                    case Directions.R:
                                        nots.UR |= ConnectionType.all;
                                        nots.R  |= ConnectionType.all;
                                        nots.DR |= ConnectionType.all;
                                        area.ECoords.Second = area.MaxSize.Second - 1;
                                        break;
                                    // Diagonals
                                    case Directions.UR:
                                        nots.UL |= ConnectionType.all;
                                        nots.U  |= ConnectionType.all;
                                        nots.UR |= ConnectionType.all;
                                        nots.R  |= ConnectionType.all;
                                        nots.DR |= ConnectionType.all;
                                        area.ECoords = new Pair(0, area.MaxSize.Second - 1);
                                        break;
                                    case Directions.DR:
                                        nots.UR |= ConnectionType.all;
                                        nots.R  |= ConnectionType.all;
                                        nots.DR |= ConnectionType.all;
                                        nots.D  |= ConnectionType.all;
                                        nots.DL |= ConnectionType.all;
                                        area.ECoords = new Pair(area.MaxSize.First - 1, area.MaxSize.Second - 1);
                                        break;
                                    case Directions.UL:
                                        nots.DL |= ConnectionType.all;
                                        nots.L  |= ConnectionType.all;
                                        nots.UL |= ConnectionType.all;
                                        nots.U  |= ConnectionType.all;
                                        nots.UR |= ConnectionType.all;
                                        area.ECoords = new Pair(0, 0);
                                        break;
                                    case Directions.DL:
                                        nots.DR |= ConnectionType.all;
                                        nots.D  |= ConnectionType.all;
                                        nots.DL |= ConnectionType.all;
                                        nots.L  |= ConnectionType.all;
                                        nots.UL |= ConnectionType.all;
                                        area.ECoords = new Pair(area.MaxSize.First - 1, 0);
                                        break;
                                }

                                // Set first level nots based on no_build
                                if (area.NoBuild.HasFlag(Directions.U)) nots.U |= ConnectionType.all;
                                if (area.NoBuild.HasFlag(Directions.D)) nots.D |= ConnectionType.all;
                                if (area.NoBuild.HasFlag(Directions.L)) nots.L |= ConnectionType.all;
                                if (area.NoBuild.HasFlag(Directions.R)) nots.R |= ConnectionType.all;
                                if (area.NoBuild.HasFlag(Directions.UR)) nots.UR |= ConnectionType.all;
                                if (area.NoBuild.HasFlag(Directions.DR)) nots.DR |= ConnectionType.all;
                                if (area.NoBuild.HasFlag(Directions.UL)) nots.UL |= ConnectionType.all;
                                if (area.NoBuild.HasFlag(Directions.DL)) nots.DL |= ConnectionType.all;

                                // Set first level reqs based on entrance direction (strictly uni-directional)
                                switch (area.EDir)
                                {
                                    case Directions.U:
                                        reqs.U |= ConnectionType.entrance;
                                        if (nots.U.HasFlag(ConnectionType.entrance)) nots.U -= ConnectionType.entrance;
                                        if (nots.U.HasFlag(ConnectionType.exit)) nots.U -= ConnectionType.exit;
                                        break;
                                    case Directions.D:
                                        reqs.D |= ConnectionType.entrance;
                                        if (nots.D.HasFlag(ConnectionType.entrance)) nots.D -= ConnectionType.entrance;
                                        if (nots.D.HasFlag(ConnectionType.exit)) nots.D -= ConnectionType.exit;
                                        break;
                                    case Directions.L:
                                        reqs.L |= ConnectionType.entrance;
                                        if (nots.L.HasFlag(ConnectionType.entrance)) nots.L -= ConnectionType.entrance;
                                        if (nots.L.HasFlag(ConnectionType.exit)) nots.L -= ConnectionType.exit;
                                        break;
                                    case Directions.R:
                                        reqs.R |= ConnectionType.entrance;
                                        if (nots.R.HasFlag(ConnectionType.entrance)) nots.R -= ConnectionType.entrance;
                                        if (nots.R.HasFlag(ConnectionType.exit)) nots.R -= ConnectionType.exit;
                                        break;
                                }

                                // Try to get options from Levels
                                // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
                                List<Level> options;
                                Level level;
                                MapScreen screen;
                                string screen_id;
                                options = GetOptionsComplex(Directions.None, reqs, nots, area.Levels);
                                options = FilterOptionsOpen(reqs, options);

                                if (options.Count != 0)
                                {
                                    level     = options[RNG.random.Next(0, options.Count)];
                                    screen_id = $"{++area.LevelCount}";
                                    screen    = new MapScreen(area.ID, screen_id, ScreenType.Level, level);
                                }
                                // If we get no options from area.Levels, we try to get options from the Connectors pool
                                else
                                {
                                    options = GetOptionsComplex(Directions.None, reqs, nots, Connectors);
                                    options = FilterOptionsOpen(reqs, options);

                                    level     = options[RNG.random.Next(0, options.Count)];
                                    screen_id = $"x{++area.ConnectorCount}";
                                    screen    = new MapScreen(area.ID, screen_id, ScreenType.Connector, level);
                                }

                                // add entrance screen to worldmap screenwipes
                                switch (area.EDir)
                                {
                                    case Directions.U:
                                        WorldMap.upwipes += $"{screen.FullID} ";
                                        break;
                                    case Directions.D:
                                        WorldMap.downwipes += $"{screen.FullID} ";
                                        break;
                                    case Directions.L:
                                        WorldMap.leftwipes += $"{screen.FullID} ";
                                        break;
                                    case Directions.R:
                                        WorldMap.rightwipes += $"{screen.FullID} ";
                                        break;
                                }

                                if (area.tags != null)
                                {
                                    if (area.tags.Contains("dark") || area.tags.Contains("darkspawn"))
                                        WorldMap.game_over_checkpoints += $"[{area.ID}, {screen_id}] ";
                                    if (area.tags.Contains("ironcart"))
                                        WorldMap.iron_cart_entrypoints += $"{screen_id} ";
                                    if (area.tags.Contains("cart"))
                                        screen.BlockEntrances = area.EDir;
                                }

                                // place new screen and add the first open end(s)
                                PlaceScreen(area, area.ECoords.First, area.ECoords.Second, screen);
                                area.ChosenScreens.Add(screen);
                                {
                                    Directions dir_ret = AddEnds(area, area.ECoords.First, area.ECoords.Second, level.MapConnections, screen.PathTrace);
                                    //if (dir_ret != Directions.None && dir_ret != area.EDir) 
                                    //    throw new Exception("Error, tried to add invalid end.");
                                }

                                // Place dot screens from first level to edge of map
                                if (area.EDir != Directions.None)
                                {
                                    // This will not update the minbuilt or maxbuilt, so these may be trimmed from the map
                                    // but they will prevent the generation from building over these screens
                                    Pair dir = DirectionsEnum.ToVectorPair(area.EDir);
                                    area.ECoords = DotsFromCellToEdge(area.Map, area.ECoords, dir); // adjusts the entry coords to (hopefully) the edge of the map
                                    if (area.ECoords.First == -1)
                                    {
                                        PrintDebugCSV(area, $"tools/map testing/map_{area.ID}_entrydots.csv");
                                        throw new Exception($"Area {area.ID} unable to connect entry level to edge of map.");
                                    }
                                }
                            }

                            // If the area exits into another area, subtract one from the level quota
                            // We do this to account for placing the final level
                            if (area.XDir != Directions.None) area.LevelQuota--;

                            // MAIN GENERATION LOOP
                            {
                                // These are declared in enclosing scope so that we can see the final values when loop breaks
                                MapScreen screen = null;
                                Pair coords = new Pair();
                                MapConnections reqs = NoMapConnections;
                                MapConnections nots = NoMapConnections;

                                while (area.LevelCount < area.LevelQuota)    // add levels until the quota is met
                                {
                                    // Get next OpenEnd to fill
                                    coords = SmartSelectOpenEnd(area);

                                    // Get level connection requirements
                                    CheckNeighbors(area, coords.First, coords.Second, out reqs, out nots, out string pathtrace);

                                    // Try to get open options from Levels
                                    Level level;
                                    string screen_id;

                                    List<Level> options;
                                    options = GetOptionsComplex(area.NoBuild, reqs, nots, area.Levels);
                                    options = FilterOptionsOpen(reqs, options);

                                    // If there are not open options in Levels
                                    if (options.Count != 0)
                                    {
                                        level = options[RNG.random.Next(0, options.Count)];
                                        screen_id = $"{++area.LevelCount}";
                                        screen = new MapScreen(area.ID, screen_id, ScreenType.Level, level, pathtrace);
                                    }
                                    // If we get no options from area.Levels, we try to get options from the Connectors pool
                                    else
                                    {
                                        // Try to get open options from Connectors
                                        options = GetOptionsComplex(area.NoBuild, reqs, nots, Connectors);
                                        options = FilterOptionsOpen(reqs, options);

                                        // If there are not open options in Connectors
                                        // Then try to get any kind of option from Connectors
                                        if (options.Count == 0)
                                        {
                                            options = GetOptionsComplex(area.NoBuild, reqs, nots, Connectors);

                                            // If there are still no options, we have a problem
                                            if (options.Count == 0)
                                            {
                                                DebugPrintReqs(coords, reqs, nots);
                                                throw new Exception("ran out of options during mapArea generation");
                                            }

                                            level = options[RNG.random.Next(0, options.Count)];
                                        }

                                        level = options[RNG.random.Next(0, options.Count)];
                                        screen = new MapScreen(area.ID, $"x{++area.ConnectorCount}", ScreenType.Connector, level, pathtrace);
                                    }

                                    // Place screen
                                    PlaceScreen(area, coords.First, coords.Second, screen);
                                    area.ChosenScreens.Add(screen);

                                    // Add new ends after placement
                                    {
                                        Directions dir_ret = AddEnds(area, coords.First, coords.Second, level.MapConnections, screen.PathTrace);
                                        if (dir_ret != Directions.None) throw new Exception("Error, tried to add invalid end.");
                                    }

                                    // Remove the OpenEnd that we are replacing
                                    area.OpenEnds.Remove(new Pair(coords.First, coords.Second));

                                    // Remove the newly placed screen from the list of screens available
                                    //if (isGameplay)
                                    //Levels.Remove(level);

                                    // Check that the OpenEnds count has not dropped below 1
                                    if (area.OpenEnds.Count == 0)
                                    {
                                        PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                                        throw new Exception($"Ran out of OpenEnds in area: {area.ID}");
                                    }
                                }

                                // Area terminates with a cart end
                                if (area.tags != null && area.tags.Contains("cart"))
                                {
                                    // Get the last screen added by the main generation loop and set the block entrances directions based on last nots
                                    screen.BlockEntrances = DirectionsEnum.Opposite(reqs.Flatten());
                                    screen.Collectables |= Collectables.LevelGoal;
                                }
                            }

                            // PLACE FINAL LEVEL
                            if (area.tags == null || !area.tags.Contains("cart"))
                            {
                                // Area exits into another area
                                if (area.XDir != Directions.None)
                                {
                                    // Select the open end with the greatest distance to be the area end
                                    Pair area_end = new Pair(-1, -1);
                                    int dist = 0;
                                    MapConnections reqs = NoMapConnections;
                                    MapConnections nots = NoMapConnections;

                                    foreach (var end in area.OpenEnds)
                                    {
                                        Pair coords = end.Key;   // Get the next end to check

                                        switch (area.XDir) // Switch based on the build direction so we check the correct side
                                        {
                                            case Directions.U:
                                                if (coords.First  != area.MinBuilt.First ) continue;
                                                break;
                                            case Directions.D:
                                                if (coords.First  != area.MaxBuilt.First ) continue;
                                                break;
                                            case Directions.L:
                                                if (coords.Second != area.MinBuilt.Second) continue;
                                                break;
                                            case Directions.R:
                                                if (coords.Second != area.MaxBuilt.Second) continue;
                                                break;
                                        }

                                        // Find the end with the greatest distance
                                        if (end.Value.PathTrace.Length > dist)
                                            area_end = end.Key;
                                    }

                                    // Error if the areaEnd is (-1, -1)
                                    // This can occur if the only OpenEnd is a dead end
                                    if (area_end.First == -1)
                                    {
                                        PrintDebugCSV(area, $"tools/map testing/map_{area.ID}_exitdots.csv");
                                        throw new Exception($"Could not select area end in area {area.ID}.");
                                    }

                                    // Set XCoords so we can see where the area end is
                                    area.XCoords = area_end;

                                    // Manually determine the exit requirements based on exit_dir (No multi-direction cases)
                                    // Nots are cleared in exit direction so that MapBounds do not prevent an exit touching the map's edge
                                    switch (area.XDir)
                                    {
                                        case Directions.D:
                                            reqs.D |= ConnectionType.exit;      // require a downwards exit
                                            nots.D = ConnectionType.none;       // clear nots.D
                                            break;
                                        case Directions.U:
                                            reqs.U |= ConnectionType.exit;      // require an upwards exit
                                            nots.U = ConnectionType.none;       // clear nots.U
                                            break;
                                        case Directions.R:
                                            reqs.R |= ConnectionType.exit;      // require a right exit
                                            nots.R = ConnectionType.none;       // clear nots.R
                                            break;
                                        case Directions.L:
                                            reqs.L |= ConnectionType.exit;      // require a left exit
                                            nots.L = ConnectionType.none;       // clear nots.R
                                            break;
                                    }

                                    // Try to get options from Levels
                                    // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
                                    Level level;
                                    MapScreen screen;
                                    string screen_id;

                                    List<Level> options;
                                    options = GetOptions(area.NoBuild, reqs, nots, area.Levels);
                                    options = FilterOptionsOpen(reqs, options);

                                    if (options.Count != 0)
                                    {
                                        level = options[RNG.random.Next(0, options.Count)];
                                        screen_id = $"{++area.LevelCount}";
                                        screen = new MapScreen(area.ID, screen_id, ScreenType.Level, level);
                                    }
                                    // If we get no options from area.Levels, we try to get options from the Connectors pool
                                    else
                                    {
                                        options = GetOptions(area.NoBuild, reqs, nots, Connectors);
                                        options = FilterOptionsOpen(reqs, options);

                                        level = options[RNG.random.Next(0, options.Count)];
                                        screen_id = $"x{++area.ConnectorCount}";
                                        screen = new MapScreen(area.ID, screen_id, ScreenType.Connector, level);
                                    }

                                    // add exit screen to worldmap screenwipes
                                    switch (area.XDir)
                                    {
                                        case Directions.U:
                                            WorldMap.upwipes    += $"{screen.FullID} ";
                                            break;
                                        case Directions.D:
                                            WorldMap.downwipes  += $"{screen.FullID} ";
                                            break;
                                        case Directions.L:
                                            WorldMap.leftwipes  += $"{screen.FullID} ";
                                            break;
                                        case Directions.R:
                                            WorldMap.rightwipes += $"{screen.FullID} ";
                                            break;
                                    }

                                    // place new screen
                                    PlaceScreen(area, area.XCoords.First, area.XCoords.Second, screen);
                                    area.ChosenScreens.Add(screen);

                                    // Remove the selected end so that it is not capped in a later step.
                                    area.OpenEnds.Remove(area_end);

                                    // Ensure that the exit screen connects to the edge of the map
                                    Pair x_vec = DirectionsEnum.ToVectorPair(area.XDir);
                                    area.XCoords = DotsFromCellToEdge(area.Map, area.XCoords, x_vec);
                                    if (area.ECoords.First == -1)
                                    {
                                        PrintDebugCSV(area, $"tools/map testing/map_{area.ID}_exitdots.csv");
                                        throw new Exception($"Area {area.ID} unable to connect final level to edge of map.");
                                    }
                                }

                                // Area terminates with a path end (and is not a cart)
                                else
                                {
                                    // Select the area end
                                    Pair area_end = new Pair(-1, -1);
                                    int dist = 0;
                                    MapConnections reqs = NoMapConnections;
                                    MapConnections nots = NoMapConnections;

                                    {
                                        // Find furthest end
                                        foreach (var end in area.OpenEnds)
                                        {
                                            if (end.Value.PathTrace.Length > dist)
                                                area_end = end.Key;
                                        }

                                        // Error if the areaEnd is (-1, -1)
                                        // This can occur if the only OpenEnd is a dead end
                                        if (area_end.First == -1)
                                        {
                                            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}_exitdots.csv");
                                            throw new Exception($"Could not select area end in area {area.ID}.");
                                        }

                                        CheckNeighbors(area, area_end.First, area_end.Second, out reqs, out nots, out string pathtrace);
                                    }

                                    // Set XCoords so we can see where the area end is
                                    area.XCoords = area_end;

                                    List<Level> options;
                                    if (area.ExitID == "friend_orb") options = GetOptionsComplex(area.NoBuild, reqs, nots, FriendOrb );     // Try to get options from FriendOrb levels
                                    else                             options = GetOptionsComplex(area.NoBuild, reqs, nots, Connectors);     // Try to get options from Connectors
                                    options = FilterOptionsClosed(reqs, options);

                                    // Get random level from the list of options
                                    Level level = options[RNG.random.Next(0, options.Count)];

                                    // Create screen to place
                                    MapScreen screen;
                                    string screen_id;
                                    {
                                        screen_id = $"x{++area.ConnectorCount}";
                                        screen = new MapScreen(area.ID, screen_id, ScreenType.Connector, level);
                                    }

                                    // Switch on ExitID. These are all path end options. If there is not a match, an error will be thrown because a XDir of none was used with an invalid path end id
                                    switch (area.ExitID)
                                    {
                                        case "friend_head":
                                            screen.Collectables |= Collectables.FriendHead; break;
                                        case "friend_body":
                                            screen.Collectables |= Collectables.FriendBody; break;
                                        case "friend_soul":
                                            screen.Collectables |= Collectables.FriendSoul; break;
                                        case "level_goal":
                                            screen.Collectables |= Collectables.LevelGoal; break;
                                        case "friend_orb":
                                            break; // This case is handled above in level selection.
                                        default:
                                            throw new Exception($"Invalid path end exit id in area {area.ID}.\nEither use a proper path end or set the exit direction to exit into another area.");
                                    }

                                    // place new screen
                                    PlaceScreen(area, area.XCoords.First, area.XCoords.Second, screen);
                                    area.ChosenScreens.Add(screen);

                                    // Remove the selected end so that it is not capped in a later step.
                                    area.OpenEnds.Remove(area_end);
                                }
                            }

                            // Build secret areas

                            // Cap all Open Ends
                            foreach (var end in area.OpenEnds)
                                CapOpenEnd(area, end.Key);
                            // Cap all Dead Ends
                            foreach (var end in area.DeadEntries)
                                CapOpenEnd(area, end.Key);

                            // Crop the map
                            //PrintDebugCSV(area, $"tools/map testing/{area.ID}_beforecrop.csv");
                            area.Map = GameMap.CropMap(area.MaxBuilt.First - area.MinBuilt.First + 1, area.MaxBuilt.Second - area.MinBuilt.Second + 1, area.Map, area.MinBuilt.First, area.MinBuilt.Second);
                            //PrintDebugCSV(area, $"tools/map testing/{area.ID}_aftercrop.csv");

                            // Adjust the ECoords and XCoords
                            area.ECoords -= area.MinBuilt;
                            area.XCoords -= area.MinBuilt;
                            area.ECoords.First  = Math.Min(area.ECoords.First , area.Map.Height - 1);
                            area.ECoords.Second = Math.Min(area.ECoords.Second, area.Map.Width  - 1);
                            area.XCoords.First  = Math.Min(area.XCoords.First , area.Map.Height - 1);
                            area.XCoords.Second = Math.Min(area.XCoords.Second, area.Map.Width  - 1);
                            area.ECoords.First  = Math.Max(area.ECoords.First , 0);
                            area.ECoords.Second = Math.Max(area.ECoords.Second, 0);
                            area.XCoords.First  = Math.Max(area.XCoords.First , 0);
                            area.XCoords.Second = Math.Max(area.XCoords.Second, 0);
                        }
                        break;
                    case GenerationType.Loaded:
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
                                    string tags = cell[1];
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
                                        List<Level> options = GetOptionsComplex(area.NoBuild, reqs, NoMapConnections, area.Levels);
                                        options = FilterOptionsClosed(reqs, options);
                                        level = options[RNG.random.Next(0, options.Count)];
                                    }
                                    else
                                    {
                                        // levels with specified filenames
                                        level.Name = $"{cell[2]}";
                                        level.Path = $"{area.LevelPath}";
                                    }
                                    level.TSDefault = new Tileset();
                                    level.TSNeed = new Tileset();

                                    // process level tags
                                    // perform necessary operations/modifications on level
                                    if (tags.Contains("Xu"))
                                    {
                                        area.XUpCoords = new Pair(row, col);
                                        WorldMap.upwipes += $"{area.ID}-{levelnum} ";
                                    }
                                    if (tags.Contains("Xd"))
                                    {
                                        area.XDownCoords = new Pair(row, col);
                                        WorldMap.downwipes += $"{area.ID}-{levelnum} ";
                                    }
                                    if (tags.Contains("Xl"))
                                    {
                                        area.XLeftCoords = new Pair(row, col);
                                        WorldMap.leftwipes += $"{area.ID}-{levelnum} ";
                                    }
                                    if (tags.Contains("Xr"))
                                    {
                                        area.XRightCoords = new Pair(row, col);
                                        WorldMap.rightwipes += $"{area.ID}-{levelnum} ";
                                    }

                                    // add the level to map
                                    MapScreen screen = new MapScreen(area.ID, levelnum, ScreenType.Level, level, "");
                                    area.Map.Data[row, col] = screen;
                                    area.ChosenScreens.Add(screen);
                                }
                            }
                            if (area.XUp != null) DotsFromCellToEdge(area.Map, area.XUpCoords, new Pair(-1, 0));
                            if (area.XDown != null) DotsFromCellToEdge(area.Map, area.XDownCoords, new Pair(1, 0));
                            if (area.XLeft != null) DotsFromCellToEdge(area.Map, area.XLeftCoords, new Pair(0, -1));
                            if (area.XRight != null) DotsFromCellToEdge(area.Map, area.XRightCoords, new Pair(0, 1));

                        }
                        break;
                    case GenerationType.Split:
                        {
                            // Create a single screen map, place all exit coords at (0,0)
                            area.Map = new GameMap(1, 1);
                            area.XUpCoords = new Pair(0, 0);
                            area.XDownCoords = new Pair(0, 0);
                            area.XLeftCoords = new Pair(0, 0);
                            area.XRightCoords = new Pair(0, 0);

                            // just add wipes in every direction
                            WorldMap.upwipes += $"{area.ID}-x ";
                            WorldMap.downwipes += $"{area.ID}-x ";
                            WorldMap.leftwipes += $"{area.ID}-x ";
                            WorldMap.rightwipes += $"{area.ID}-x ";

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
                            if (area.XUp != null) reqs.U = ConnectionType.exit;
                            if (area.XDown != null) reqs.D = ConnectionType.exit;
                            if (area.XLeft != null) reqs.L = ConnectionType.exit;
                            if (area.XRight != null) reqs.R = ConnectionType.exit;

                            List<Level> options = options = GetOptionsComplex(area.NoBuild, reqs, NoMapConnections, Connectors);
                            options = FilterOptionsClosed(reqs, Connectors);
                            Level level = options[RNG.random.Next(0, options.Count)];
                            MapScreen screen = new MapScreen(area.ID, "x", ScreenType.Connector, level);
                            area.Map.Data[0, 0] = screen;
                            area.ChosenScreens.Add(screen);
                        }
                        break;
                }
                // Generate area data files
                {
                    // Open StreamWriters for the data files
                    StreamWriter levelinfo_file = File.AppendText(SaveDir + "data/levelinfo.txt.append");
                    StreamWriter tilesets_file = File.AppendText(SaveDir + "data/tilesets.txt.append");
                    StreamWriter debug_file = File.CreateText("tools/debug.md");

                    // Create randomized area tileset
                    if (area.Name == null) area.Name = GetFunnyAreaName();

                    // Write the area's tileset to the file
                    tilesets_file.WriteLine($"{area.ID} {{");
                    debug_file.WriteLine($"## Area {area.ID}");
                    area.Tileset.WriteTileset(tilesets_file);

                    // Iterate over all of the screen in the area
                    for (int i = 0; i < area.ChosenScreens.Count; i++)
                    {
                        // Get the next screen from the list
                        MapScreen screen = area.ChosenScreens[i];

                        // Skip over screens of Dot type
                        if (screen.Type == ScreenType.Dots) continue;

                        debug_file.WriteLine($"\n### Level {screen.ScreenID}");
                        debug_file.WriteLine($"InFile: {screen.Level.InFile}");
                        
                        // Load the level file associated with that screen/level
                        LevelFile level_file = LevelManip.Load(screen.Level.InFile);

                        // Clean Levels (set collectables and de-colorize)
                        {
                            if (screen.Type == ScreenType.Level)
                            {
                                if (area.Tileset.area_type == null
                                 || area.Tileset.area_type == "normal")          // normal levels have tumors
                                    screen.Collectables |= Collectables.Tumor;
                                else if (area.Tileset.area_type == "cart")       // add rings to cart levels
                                    screen.Collectables |= Collectables.Rings;
                            }
                            int ret_code = LevelCorruptors.CleanLevel(ref level_file, screen.Collectables, screen.BlockEntrances);
                            if (ret_code != 0) ErrorNotes += $"Failed to place collectables on level {area.ID}-{screen.ScreenID}, code {ret_code}\n";
                            
                            if (screen.Level.TileSwaps != null && screen.Level.TileSwaps.Count != 0)
                            {
                                debug_file.WriteLine($"TileSwaps: ");
                                LevelCorruptors.SwapTiles(ref level_file, screen.Level.TileSwaps);      // do tile swaps
                                foreach (var swap in screen.Level.TileSwaps)
                                    debug_file.WriteLine($"{swap.Key} -> {swap.Value}");
                            }
                        }

                        debug_file.WriteLine("MapConnections: ");
                        debug_file.Write(screen.Level.MapConnections.DebugString());
                        debug_file.WriteLine("Reqs: ");
                        debug_file.Write(screen.Level.DebugReqs.DebugString());
                        debug_file.WriteLine("Nots: ");
                        debug_file.Write(screen.Level.DebugNots.DebugString());
                        debug_file.WriteLine("Collectables: ");
                        debug_file.WriteLine(screen.Collectables);

                        if (screen.Level.FlippedHoriz)
                            LevelManip.FlipLevelH(ref level_file);

                        if (Settings.DoCorruptions)
                            screen.Level.TSNeed.extras += LevelCorruptors.CorruptLevel(ref level_file);

                        LevelManip.Save(level_file, SaveDir + $"tilemaps/{area.ID}-{screen.ScreenID}.lvl");

                        // Write LevelInfo
                        if (screen.Type == ScreenType.Level)
                            levelinfo_file.WriteLine($"\"{area.ID}-{screen.ScreenID}\" {{name=\"{area.Name} {screen.ScreenID}\" id={screen.ScreenID}}}");
                        else levelinfo_file.WriteLine($"\"{area.ID}-{screen.ScreenID}\" {{name=\"{area.Name}\" id=-1}}");

                        // Write Level Tileset
                        // Calculate the final tileset
                        // The tilesets are added in order of priority, from lowest to highest
                        Tileset level_tileset = (screen.Level.TSDefault + area.Tileset) + screen.Level.TSNeed;
                        if (level_tileset.area_type == "cart")
                        {
                            Console.WriteLine($"{area.ID}-{screen.ScreenID}");
                        }

                        // Write level tileset to the file
                        //debug_file.WriteLine($"Tileset: {screen.Level.InFile}");
                        //level_tileset.WriteTileset(debug_file);
                        tilesets_file.WriteLine($"{screen.ScreenID} {{");
                        tilesets_file.WriteLine($"#filename: {screen.Level.InFile}");
                        level_tileset.WriteTileset(tilesets_file);
                        tilesets_file.WriteLine("}");
                    }

                    // write closing bracket for area tileset
                    tilesets_file.WriteLine("}\n");

                    // close filestreams
                    levelinfo_file.Close();
                    tilesets_file.Close();
                    debug_file.Close();

                }
            }

            // Link map areas
            GameMap final_map;
            {
                // Get light and dark spawn areas
                // Link maps to produce final map
                Pair e_coord;       // We don't actually use this once it's returned, but it needs to be declared

                if (lightspawn_id == null)
                    throw new Exception("Light world spawn not assigned to any area.");
                MapArea lightspawn_area = MapAreas[lightspawn_id];
                final_map = LinkMaps(lightspawn_area, out e_coord);

                if (darkspawn_id != null)
                {
                    MapArea darkspawn_area = MapAreas[darkspawn_id];
                    GameMap dark_map = LinkMaps(darkspawn_area, out e_coord);
                    final_map = ConcatenateMaps(final_map, dark_map);
                }

                // Concatenate final map with all standalone areas' maps
                // This includes carts, stevens.
                foreach (var item in MapAreas)
                {
                    MapArea area = item.Value;
                    if (area.IsStandalone)
                    {
                        final_map = ConcatenateMaps(final_map, area.Map);
                    }
                }
            }

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
                        if (area.ExitID == null || area.XDir == Directions.None)
                        {
                            // return map and entry coord
                            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                            e_coord = area.ECoords;
                            return area.Map;
                        }
                        else
                        {
                            GameMap ret_map = LinkMap(area.Map, area.ExitID, area.XCoords, area.XDir);
                            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                            e_coord = new_e_coord;
                            return ret_map;
                        }
                    }
                case GenerationType.Split:
                case GenerationType.Loaded:
                    {
                        bool had_child = false;
                        GameMap ret_map = area.Map;
                        //DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_pre.csv");

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
                        //DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_U.csv");
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
                        //DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_D.csv");
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
                        //DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_L.csv");
                        if (area.XRight != null)
                        {
                            ret_map = LinkMap(ret_map, area.XRight, xr_coord, Directions.R);
                            had_child = true;
                        }
                        //DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_R.csv");
                        if (had_child)
                        {
                            //DebugPrintMap(ret_map, $"tools/map testing/map_{area.ID}_post.csv");
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
                    }
            }
            e_coord = new_e_coord;
            return new GameMap(0, 0);    // this should not be reached

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
                // get the map (and entry coord) from child
                if (!MapAreas.TryGetValue(next_area_id, out MapArea child_area)) throw new Exception($"Could not find area def with id {next_area_id}");
                GameMap child_map = LinkMaps(child_area, out Pair child_entry_coord);

                // combine the current map with childs map
                GameMap new_map = CombineMaps(curr_map, child_map, x_coord, child_entry_coord, exit_dir, out Pair new_m1_offset);

                // set new offset and entry coord
                m1_offset = new_m1_offset;
                new_e_coord += m1_offset;

                // make sure that entrance dots still extends to edge of map
                Pair e_vec = DirectionsEnum.ToVectorPair(area.EDir);
                if (e_vec != new Pair(0, 0))
                    new_e_coord = DotsFromCellToEdge(new_map, new_e_coord, e_vec);

                // return the map and the new entry coord to parent
                return new_map;
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
                    m1_origin.Second = Math.Max(0, m2_entry.Second - m1_exit.Second);           // align exit and entrance horizontally by moving leftmost transition rightwards
                    m2_origin.Second = Math.Max(0, m1_exit.Second - m2_entry.Second);
                    m1_origin.First = m2_origin.First + m2_entry.First + 1;                     // align top edge of m1 to bottom edge of m2
                    width = Math.Max(m1_origin.Second + m1.Width, m2_origin.Second + m2.Width); // get rightmost map coord as width
                    height = m1_origin.First + m1.Height;
                    break;

                case Directions.D:
                    m1_origin.Second = Math.Max(0, m2_entry.Second - m1_exit.Second);           // align exit and entrance horizontally by moving leftmost transition rightwards
                    m2_origin.Second = Math.Max(0, m1_exit.Second - m2_entry.Second);
                    m2_origin.First = m1_origin.First + m1_exit.First + 1;                      // align top edge of m2 to bottom edge of m1
                    width = Math.Max(m1_origin.Second + m1.Width, m2_origin.Second + m2.Width); // get rightmost map coord as width
                    height = m2_origin.First + m2.Height;
                    break;

                case Directions.L:
                    m1_origin.First = Math.Max(0, m2_entry.First - m1_exit.First);               // align exit and entrance vertically by moving upmost transition downwards
                    m2_origin.First = Math.Max(0, m1_exit.First - m2_entry.First);
                    m1_origin.Second = m2_origin.Second + m2_entry.Second + 1;                   // align left edge of m1 to right edge of m2
                    height = Math.Max(m1_origin.First + m1.Height, m2_origin.First + m2.Height); // get lowest map coord as height
                    width = m1_origin.Second + m1.Width;
                    break;

                case Directions.R:
                    m1_origin.First = Math.Max(0, m2_entry.First - m1_exit.First);               // align exit and entrance vertically by moving upmost transition downwards
                    m2_origin.First = Math.Max(0, m1_exit.First - m2_entry.First);
                    m2_origin.Second = m1_origin.Second + m1_exit.Second + 1;                    // align left edge of m2 to right edge of m1
                    height = Math.Max(m1_origin.First + m1.Height, m2_origin.First + m2.Height); // get lowest map coord as height
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
        static GameMap ConcatenateMaps(GameMap m1, GameMap m2)
        {
            int height = m1.Height + m2.Height + 1;
            int width = Math.Max(m1.Width, m2.Width);

            Pair m1_origin = new Pair(0, 0);
            Pair m2_origin = new Pair(m1.Height + 1, 0);

            GameMap new_map = new GameMap(height, width);

            GameMap.CopyToMapAtCoords(m1, new_map, m1_origin);
            GameMap.CopyToMapAtCoords(m2, new_map, m2_origin);

            return new_map;
        }
        static Pair SmartSelectOpenEnd(MapArea area)
        {
            // If there is only one end, use this end
            // If filling this end creates no new open ends, we will need to fix that.
            if (area.OpenEnds.Count == 1)
                return area.OpenEnds.First().Key;

            Pair open_end = new Pair(-1, -1);
            int dist = 0;

            // find furthest end
            foreach (var end in area.OpenEnds)
            {
                
                // Code to use up ends with many neighbors, so they are less likely to get boxed in later on
                /*if (end.Value.NumNeighbors == 7)
                    return end.Key;*/
                if (end.Value.PathTrace.Length > dist)
                    open_end = end.Key;
            }

            if (open_end.First != -1)
                return open_end;

            // Otherwise, pick a random end
            return area.OpenEnds.ElementAt(RNG.random.Next(0, area.OpenEnds.Count)).Key;
        }
        static Pair GetOpenEnd(MapArea area)
        {
            // Return a random OpenEnd from the list
            // Will error if there are no OpenEnds
            return area.OpenEnds.ElementAt(RNG.random.Next(0, area.OpenEnds.Count)).Key;
        }
        static void CheckNeighbors(MapArea area, int i, int j, out MapConnections reqs, out MapConnections nots, out string pathtrace)  // returns the type necessary to meet needs of neighbors
        {
            // initialize reqs and nots to empty
            reqs = NoMapConnections;
            nots = NoMapConnections;
            string dirs = null;

            // map is row major, so i is the row and j is the column
            // get required entrances, and spots where entrances cant be
            // Cardinal Directions
            CheckNeighbor(i - 1, j, ref reqs.U, ref nots.U, Directions.D); // Check Screen Up
            CheckNeighbor(i + 1, j, ref reqs.D, ref nots.D, Directions.U); // Check Screen Down
            CheckNeighbor(i, j - 1, ref reqs.L, ref nots.L, Directions.R); // Check Screen Left
            CheckNeighbor(i, j + 1, ref reqs.R, ref nots.R, Directions.L); // Check Screen Right
            // Diagonals
            CheckNeighbor(i - 1, j + 1, ref reqs.UR, ref nots.UR, Directions.DL); // Check Screen UpRight
            CheckNeighbor(i + 1, j + 1, ref reqs.DR, ref nots.DR, Directions.UL); // Check Screen DownRight
            CheckNeighbor(i - 1, j - 1, ref reqs.UL, ref nots.UL, Directions.DR); // Check Screen UpLeft
            CheckNeighbor(i + 1, j - 1, ref reqs.DL, ref nots.DL, Directions.UR); // Check Screen DownLeft

            if (dirs == null) dirs = "";
            pathtrace = dirs;
            return;

            void CheckNeighbor(int k, int l, ref ConnectionType req, ref ConnectionType not, Directions dir)
            {
                // Must be inside map bounds and not be a secret end
                if (MapBoundsCheck(area.Map, k, l) && !area.SecretEnds.ContainsKey(new Pair(k, l)))
                {
                    // Get the neighbor we want to check
                    MapScreen neighbor = area.Map.Data[k, l];
                    ConnectionType connection;

                    // ENTRANCES AND EXITS
                    // If the screen is not null, check the connections
                    // If it is null, it will not impose any requirements on entrances or exits
                    if (neighbor != null)
                    {
                        if (neighbor.Type == ScreenType.Dots)
                        {
                            not |= ConnectionType.both;
                            return;
                        }

                        // Get the desired connection by specifying direction
                        connection = neighbor.Level.MapConnections.GetDirection(dir);

                        // If there is an exit (or both), require an entrance
                        if (connection.HasFlag(ConnectionType.exit))
                        {
                            // Set level requirements
                            req |= ConnectionType.entrance;
                            
                            // Set pathtrace to new level if shorter than existing path or if no path is set
                            if (dirs == null || neighbor.PathTrace.Length < dirs.Length)
                                dirs = neighbor.PathTrace + (char)(DirectionsEnum.Opposite(dir));
                        }
                        // If there is only an entrance, require an exit
                        else if (connection.HasFlag(ConnectionType.entrance))
                            req |= ConnectionType.exit;
                        // If there is no connection, require a not on both (secret can be blocked off later)
                        else not |= ConnectionType.both;

                        // Also, if the screen is not null, then we can't have a secret here
                        not |= ConnectionType.secret;
                    }

                    // SECRETS
                    // If we have an OpenEnd or DeadEntry in this location, we cannot have a secret here.
                    if (area.OpenEnds.ContainsKey(new Pair(k, l))
                     || area.DeadEntries.ContainsKey(new Pair(k, l)))
                    {
                        not |= ConnectionType.secret;
                    }
                }
                // If the screen we are looking at is invalid, we cannot connect to it at all.
                else not |= ConnectionType.all;  // all includes both entrances, exits, and secrets
            }
        }
        static Directions AddEnds(MapArea area, int i, int j, MapConnections mCons, string pathtrace)
        {
            // This function calls AddEnd for each direction of connection
            // The index given is the screen that is Up, Down, Left, or Right from the screen just placed.
            // The ConnectionType given is the connection type of the transition leading into the given screen.

            Directions ret = Directions.None;

            ret |= AddEnd(Directions.U, mCons.U);
            ret |= AddEnd(Directions.D, mCons.D);
            ret |= AddEnd(Directions.L, mCons.L);
            ret |= AddEnd(Directions.R, mCons.R);

            ret |= AddEnd(Directions.UR, mCons.UR);
            ret |= AddEnd(Directions.DR, mCons.DR);
            ret |= AddEnd(Directions.UL, mCons.UL);
            ret |= AddEnd(Directions.DL, mCons.DL);

            return ret;

            Directions AddEnd(Directions dir, ConnectionType con)
            {
                // This function adds the applicable End type for a given screen and connection type

                // OpenEnds    are placed on any exits     which lead to null screens
                // DeadEntries are placed on any entrances which lead to null screens
                // SecretEnds  are placed on any secret entrances (these are pre-checked for compatibility)
                Pair vec = DirectionsEnum.ToVectorPair(dir);
                Pair index = new Pair(i + vec.First, j + vec.Second);

                // Exits are checked first because they take precedence over entrances
                if (con.HasFlag(ConnectionType.exit))
                {
                    // By the rules in CheckNeighbors, we shouldn't be trying to place an openEnd over an already existing secretEnd
                    // So the only other consideration is whether a DeadEntry already exists, which we can overwrite.
                    if (!MapBoundsCheck(area.Map, index.First, index.Second))
                        return dir;
                    if (area.Map.Data[index.First, index.Second] == null)
                    {
                        // Remove deadEntry if there was one here
                        area.DeadEntries.Remove(index);

                        // Add or Update OpenEnd
                        if (area.OpenEnds.TryGetValue(index, out MapArea.OpenEnd end))
                        {
                            if (pathtrace.Length > end.PathTrace.Length)
                                end.PathTrace = pathtrace + (char)DirectionsEnum.Opposite(dir);
                            end.NumNeighbors++;
                        }
                        else area.OpenEnds.Add(index, new MapArea.OpenEnd(1, pathtrace + (char)DirectionsEnum.Opposite(dir)));
                        UpdateMinMaxCoords(area, index.First, index.Second);
                    }
                }
                else if (con.HasFlag(ConnectionType.entrance))
                {
                    // If we have only an entrance here, try to add a deadEntry
                    // If there is already an openEnd here, don't add the deadEntry
                    if (!MapBoundsCheck(area.Map, index.First, index.Second))
                        return dir;
                    if (area.Map.Data[index.First, index.Second] == null)
                    {
                        if (!area.OpenEnds.ContainsKey(index))
                        {
                            area.DeadEntries.Add(index, new MapArea.OpenEnd(1, pathtrace + (char)DirectionsEnum.Opposite(dir)));
                            UpdateMinMaxCoords(area, index.First, index.Second);
                        }
                    }
                }
                else if (con.HasFlag(ConnectionType.secret))
                {
                    // Add a secret end if applicable
                    // Should be no issues with adding this since we pre-check for other types of ends in this spot during CheckNeighbors
                    // For SecretEnds, we don't need to check if the screen at index is null
                    if (!MapBoundsCheck(area.Map, index.First, index.Second))
                        return dir;
                    area.SecretEnds.Add(index, new MapArea.OpenEnd(1, pathtrace + (char)DirectionsEnum.Opposite(dir)));
                    UpdateMinMaxCoords(area, index.First, index.Second);
                }
                return Directions.None;
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
            CheckNeighbors(area, coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string pathtrace);

            // Get options from pool of screens
            List<Level> options = GetOptionsComplex(area.NoBuild, reqs, nots, Connectors);
            options = FilterOptionsClosed(reqs, options);

            // Get random level from the list of options
            Level level = options[RNG.random.Next(0, options.Count)];

            // Place screen
            MapScreen screen = new MapScreen(area.ID, $"x{++area.ConnectorCount}", ScreenType.Connector, level, pathtrace);
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
        // Get and Filter Level Options
        static List<Level> GetOptions(Directions no_build, MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            List<Level> options = new List<Level>();
            foreach (Level level in pool)
            {
                // Check connection requirements
                if ((level.MapConnections & reqs) != reqs) continue;
                if ((level.MapConnections & nots) != NoMapConnections) continue;

                // Subtract the requirements and see what remains
                MapConnections umc = GetUniqueConnections(level.MapConnections, reqs);
                // Skip levels which build (add exits) in disallowed directions
                if ((no_build.HasFlag(Directions.U) && umc.U.HasFlag(ConnectionType.exit))) continue;
                if ((no_build.HasFlag(Directions.D) && umc.D.HasFlag(ConnectionType.exit))) continue;
                if ((no_build.HasFlag(Directions.L) && umc.L.HasFlag(ConnectionType.exit))) continue;
                if ((no_build.HasFlag(Directions.R) && umc.R.HasFlag(ConnectionType.exit))) continue;
                if ((no_build.HasFlag(Directions.UR) && umc.UR.HasFlag(ConnectionType.exit))) continue;
                if ((no_build.HasFlag(Directions.DR) && umc.DR.HasFlag(ConnectionType.exit))) continue;
                if ((no_build.HasFlag(Directions.UL) && umc.UL.HasFlag(ConnectionType.exit))) continue;
                if ((no_build.HasFlag(Directions.DL) && umc.DL.HasFlag(ConnectionType.exit))) continue;

                options.Add(level);
            }
            return options;
        }
        static List<Level> GetOptionsComplex(Directions no_build, MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            List<Level> options = new List<Level>();

            foreach (Level level in pool)
            {
                Level copy_level = level.Clone();

                // Skip levels which build (add exits) in disallowed directions
                {
                    // Subtract the requirements and see what remains
                    MapConnections umc = GetUniqueConnections(level.MapConnections, reqs);
                    if ((no_build.HasFlag(Directions.U) && umc.U.HasFlag(ConnectionType.exit))) continue;
                    if ((no_build.HasFlag(Directions.D) && umc.D.HasFlag(ConnectionType.exit))) continue;
                    if ((no_build.HasFlag(Directions.L) && umc.L.HasFlag(ConnectionType.exit))) continue;
                    if ((no_build.HasFlag(Directions.R) && umc.R.HasFlag(ConnectionType.exit))) continue;
                    if ((no_build.HasFlag(Directions.UR) && umc.UR.HasFlag(ConnectionType.exit))) continue;
                    if ((no_build.HasFlag(Directions.DR) && umc.DR.HasFlag(ConnectionType.exit))) continue;
                    if ((no_build.HasFlag(Directions.UL) && umc.UL.HasFlag(ConnectionType.exit))) continue;
                    if ((no_build.HasFlag(Directions.DL) && umc.DL.HasFlag(ConnectionType.exit))) continue;
                }

                // Continue will skip over the level without adding it to the return list
                if ((level.MapConnections.U & reqs.U) != reqs.U) continue;
                if ((level.MapConnections.D & reqs.D) != reqs.D) continue;
                if ((level.MapConnections.U & nots.U) != ConnectionType.none) continue;
                if ((level.MapConnections.D & nots.D) != ConnectionType.none) continue;

                List<int>[] opt = new List<int>[3];
                for (int i = 0; i < 3; i++) opt[i] = new List<int>(3);
                Directions taken_dirs = Directions.None;
                int[] assignments = new int[3];

                //Console.Write("-----\n");

                // right side
                TileID[] tiles = { TileID.GreenTransitionUR , TileID.GreenTransitionR , TileID.GreenTransitionDR };
                Directions[] dirs = { Directions.UR, Directions.R, Directions.DR };
                ConnectionType[] r  = { reqs.UR, reqs.R, reqs.DR };
                ConnectionType[] n  = { nots.UR, nots.R, nots.DR };
                ConnectionType[] mc = { level.MapConnections.UR, level.MapConnections.R, level.MapConnections.DR };
                if (!CheckSide()) continue;

                //Console.Write("--\n");

                // left side
                tiles = new TileID[] { TileID.GreenTransitionUL, TileID.GreenTransitionL, TileID.GreenTransitionDL };
                dirs = new Directions[] { Directions.UL, Directions.L, Directions.DL };
                r  = new ConnectionType[] { reqs.UL, reqs.L, reqs.DL };
                n  = new ConnectionType[] { nots.UL, nots.L, nots.DL };
                mc = new ConnectionType[] { level.MapConnections.UL, level.MapConnections.L, level.MapConnections.DL };
                for (int i = 0; i < 3; i++) opt[i] = new List<int>(3);
                assignments = new int[3];
                if (!CheckSide()) continue;

                copy_level.DebugReqs = reqs;
                copy_level.DebugNots = nots;

                options.Add(copy_level);

                bool CheckSide()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (taken_dirs.HasFlag(dirs[j])
                            || (r[i] & mc[j]) != r[i]
                            || (n[i] & mc[j]) != ConnectionType.none)
                                continue;
                            opt[i].Add(j);
                        }
                        if (opt[i].Count == 0)
                        {
                            //Console.Write($"P1: returned false on i{i}\n");
                            return false;
                        }
                        if (opt[i].Count == 1)
                        {
                            assignments[i] = opt[i][0];
                            taken_dirs |= dirs[opt[i][0]];
                            //Console.Write($"P1: i{i} = j{opt[i][0]}\n");
                        }
                    }
                    //if (level.Name == "2" && )
                    //    Console.WriteLine("break");
                    // loop again, make assignments for directions with multiple options
                    for (int i = 0; i < 3; i++)
                    {
                        if (opt[i].Count < 2)
                        {
                            //Console.Write($"P2: broke on i{i}\n");
                            continue;
                        }
                        for (int j = 0; j < opt[i].Count; j++)
                        {
                            int choice = opt[i][j];
                            if (!taken_dirs.HasFlag(dirs[choice]))   // check again that it is not taken
                            {
                                assignments[i] = choice;
                                taken_dirs |= dirs[choice];
                                //Console.Write($"P2: i{i} = j{opt[i][j]}\n");
                                break;
                            }
                            opt[i].RemoveAt(j--);     // remove option if it was already taken.
                        }
                        if (opt[i].Count == 0)  // if we exit the loop with no options, the level failed
                        {
                            //Console.Write($"P2: returned false on i{i}\n");
                            return false;
                        }
                    }
                    // use assignments to make new level data
                    for (int i = 0; i < 3; i++)
                    {
                        int j = assignments[i]; // get assigned j of i
                        // j will give us the direction and tileid for connection i
                        // mc[j] is the connectiontype of the connection assigned to dir[i]
                        // this is stored to the new_mc in the new direction
                        copy_level.MapConnections[dirs[i]] = mc[j];
                        // the transition tile orginally corresponding to j will be replaced with the tile corresponding to i
                        if (i != j) copy_level.TileSwaps.Add(tiles[j], tiles[i]);
                    }
                    return true;
                }
            }
            return options;
        }
        static List<Level> FilterOptionsOpen(MapConnections reqs, List<Level> options)
        {
            // Use GetOptions or GetOptionsComplex and pass the result in to narrow down options further

            // Create new list of options
            List<Level> new_options = new List<Level>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.MapConnections, reqs);

                // Spare the level if any new exits are added
                if      (mc.U.HasFlag(ConnectionType.exit)) new_options.Add(level);
                else if (mc.D.HasFlag(ConnectionType.exit)) new_options.Add(level);
                else if (mc.L.HasFlag(ConnectionType.exit)) new_options.Add(level);
                else if (mc.R.HasFlag(ConnectionType.exit)) new_options.Add(level);
                else if (mc.UR.HasFlag(ConnectionType.exit)) new_options.Add(level);
                else if (mc.DR.HasFlag(ConnectionType.exit)) new_options.Add(level);
                else if (mc.UL.HasFlag(ConnectionType.exit)) new_options.Add(level);
                else if (mc.DL.HasFlag(ConnectionType.exit)) new_options.Add(level);
            }
            return new_options;
        }
        static List<Level> FilterOptionsClosed(MapConnections reqs, List<Level> options)
        {
            // Use GetOptions or GetOptionsComplex and pass the result in to narrow down options further

            // Create new list of options
            List<Level> newOptions = new List<Level>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.MapConnections, reqs);

                // Skip the level if any new exits are added
                if (mc.U  != 0b0000) continue;
                if (mc.D  != 0b0000) continue;
                if (mc.L  != 0b0000) continue;
                if (mc.R  != 0b0000) continue;
                if (mc.UR != 0b0000) continue;
                if (mc.DR != 0b0000) continue;
                if (mc.UL != 0b0000) continue;
                if (mc.DL != 0b0000) continue;

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
            if (reqs.U  == ConnectionType.none) ret.U  = cons.U ;
            if (reqs.D  == ConnectionType.none) ret.D  = cons.D ;
            if (reqs.L  == ConnectionType.none) ret.L  = cons.L ;
            if (reqs.R  == ConnectionType.none) ret.R  = cons.R ;
            if (reqs.UR == ConnectionType.none) ret.UR = cons.UR;
            if (reqs.DR == ConnectionType.none) ret.DR = cons.DR;
            if (reqs.UL == ConnectionType.none) ret.UL = cons.UL;
            if (reqs.DL == ConnectionType.none) ret.DL = cons.DL;

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
                level.data[LevelFile.TAG, index] = TileID.None;

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
                                sw.Write($"{DebugGetStr(area.Map.Data[i, j].Level.MapConnections)}");
                                /*if (area.Map.Data[i, j].Type == ScreenType.Level)
                                    sw.Write($"g");
                                else if (area.Map.Data[i, j].Type == ScreenType.Connector)
                                    sw.Write($"c");*/
                            }
                        }
                        if (area.OpenEnds.ContainsKey(new Pair(i, j)))
                        {
                            sw.Write($"O");
                        }
                        if (area.DeadEntries.ContainsKey(new Pair(i, j)))
                        {
                            sw.Write($"D");
                        }
                        if (area.SecretEnds.ContainsKey(new Pair(i, j)))
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
                                sw.Write($"{DebugGetStr(map.Data[i, j].Level.MapConnections)}");
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
                                sw.Write($"{map.Data[i, j].FullID}.lvl");
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
        static string DebugGetStr(MapConnections con)
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
            if (con.UR != ConnectionType.none)
                con2 |= Directions.UR;
            if (con.DR != ConnectionType.none)
                con2 |= Directions.DR;
            if (con.UL != ConnectionType.none)
                con2 |= Directions.UL;
            if (con.DL != ConnectionType.none)
                con2 |= Directions.DL;

            string str = "";

            if (con2.HasFlag(Directions.UL | Directions.DL)) str += ":";
            else if (con2.HasFlag(Directions.UL)) str += "'";
            else if (con2.HasFlag(Directions.DL)) str += ".";
            else str += " ";

            // use old switch case to get char

            if      (con2.HasFlag(Directions.D | Directions.U | Directions.L | Directions.R)) str += "┼";
            else if (con2.HasFlag(Directions.D | Directions.U | Directions.L)) str += "┤";
            else if (con2.HasFlag(Directions.D | Directions.U | Directions.R)) str += "├";
            else if (con2.HasFlag(Directions.D | Directions.L | Directions.R)) str += "┬";
            else if (con2.HasFlag(Directions.U | Directions.L | Directions.R)) str += "┴";
            else if (con2.HasFlag(Directions.U | Directions.R)) str += "└";
            else if (con2.HasFlag(Directions.D | Directions.R)) str += "┌";
            else if (con2.HasFlag(Directions.U | Directions.L)) str += "┘";
            else if (con2.HasFlag(Directions.D | Directions.L)) str += "┐";
            else if (con2.HasFlag(Directions.D | Directions.U)) str += "│";
            else if (con2.HasFlag(Directions.L | Directions.R)) str += "─";
            else if (con2.HasFlag(Directions.U)) str += "↓";
            else if (con2.HasFlag(Directions.D)) str += "↑";
            else if (con2.HasFlag(Directions.L)) str += "→";
            else if (con2.HasFlag(Directions.R)) str += "←";
            else str += " ";

            if (con2.HasFlag(Directions.UR | Directions.DR)) str += ":";
            else if (con2.HasFlag(Directions.UR)) str += "'";
            else if (con2.HasFlag(Directions.DR)) str += ".";
            else str += " ";

            return str;
        }
    }
}
