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
        static readonly MapScreen DotScreen = new MapScreen { ScreenID = "..", Type = ScreenType.Dots };
        public static readonly MapConnections NoMapConnections;

        static int KeysPlaced;
        static int LocksPlaced;

        public static List<Level> StandardLevels = new List<Level>();
        public static List<Level> CartLevels     = new List<Level>();
        public static List<Level> Connectors     = new List<Level>();
        public static List<Level> Secrets        = new List<Level>();
        public static List<Level> FriendOrb      = LevelPool.LoadPool("data/level_pools/.mapgen/FriendOrb.gon").Levels;

        public static (GameMap, Pair) GenerateAndLink(MapArea area)
        {
            switch (area.GenType)
            {
                case GenerationType.Standard:
                    return Standard();
                case GenerationType.Loaded:
                    return Loaded();
                case GenerationType.Split:
                    return Split();
                default:
                    return (new GameMap(0, 0), new Pair(0, 0));
            }

            (GameMap, Pair) Standard()
            {
                // PRELIMINARIES
                // Initialize Ends
                area.OpenEnds = new Dictionary<Pair, OpenEnd>();
                area.DeadEnds = new Dictionary<Pair, OpenEnd>();
                area.SecretEnds = new Dictionary<Pair, OpenEnd>();
                // Init Map and associated trackers
                area.Map = new GameMap(area.MaxSize.I, area.MaxSize.J);   // Initialize map to size of bounds in def

                Pair min_built = new Pair(area.MaxSize.I, area.MaxSize.J); // Initializes to highest possible value
                Pair max_built = new Pair(0, 0);                                    // Initializes to lowest possible value

                bool force_backtrack = area.Flags.HasFlag(MapArea._Flags.BackTrack);

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

                    // directions to ignore when we do add_ends
                    // these area all the directions which are contacting the edge of the map
                    Directions ends_ignore = Directions.None;

                    // Set first level coord and nots based on anchor
                    switch (area.Anchor)
                    {
                        // Cardinals
                        case Directions.U:
                            ends_ignore = Directions.UL | Directions.U | Directions.UR;
                            area.ECoords.I = 0;
                            break;
                        case Directions.D:
                            ends_ignore = Directions.DR | Directions.D | Directions.DL;
                            area.ECoords.I = area.MaxSize.I - 1;
                            break;
                        case Directions.L:
                            ends_ignore = Directions.UL | Directions.L | Directions.DL;
                            area.ECoords.J = 0;
                            break;
                        case Directions.R:
                            ends_ignore = Directions.DR | Directions.R | Directions.UR;
                            area.ECoords.J = area.MaxSize.J - 1;
                            break;
                        // Diagonals
                        case Directions.UR:
                            ends_ignore = Directions.UL | Directions.U | Directions.UR | Directions.R | Directions.DR;
                            area.ECoords = new Pair(0, area.MaxSize.J - 1);
                            break;
                        case Directions.DR:
                            ends_ignore = Directions.UR | Directions.R | Directions.DR | Directions.D | Directions.DL;
                            area.ECoords = new Pair(area.MaxSize.I - 1, area.MaxSize.J - 1);
                            break;
                        case Directions.UL:
                            ends_ignore = Directions.DL | Directions.L | Directions.UL | Directions.U | Directions.UR;
                            area.ECoords = new Pair(0, 0);
                            break;
                        case Directions.DL:
                            ends_ignore = Directions.DR | Directions.D | Directions.DL | Directions.L | Directions.UL;
                            area.ECoords = new Pair(area.MaxSize.I - 1, 0);
                            break;
                    }
                    // We set all the necessary nots using set multiple, based on the ends_ignore directions
                    nots.SetMultiple(ends_ignore, ConnectionType.All);

                    // Set first level nots based on no_build
                    if (area.NoBuild.HasFlag(Directions.U)) nots.U |= ConnectionType.All;
                    if (area.NoBuild.HasFlag(Directions.D)) nots.D |= ConnectionType.All;
                    if (area.NoBuild.HasFlag(Directions.L)) nots.L |= ConnectionType.All;
                    if (area.NoBuild.HasFlag(Directions.R)) nots.R |= ConnectionType.All;
                    if (area.NoBuild.HasFlag(Directions.UR)) nots.UR |= ConnectionType.All;
                    if (area.NoBuild.HasFlag(Directions.DR)) nots.DR |= ConnectionType.All;
                    if (area.NoBuild.HasFlag(Directions.UL)) nots.UL |= ConnectionType.All;
                    if (area.NoBuild.HasFlag(Directions.DL)) nots.DL |= ConnectionType.All;

                    // Set first level reqs based on entrance direction (strictly uni-directional)
                    switch (area.EDir)
                    {
                        case Directions.U:
                            reqs.U |= ConnectionType.Entrance;
                            if (nots.U.HasFlag(ConnectionType.Entrance)) nots.U -= ConnectionType.Entrance;
                            if (nots.U.HasFlag(ConnectionType.Exit)) nots.U -= ConnectionType.Exit;
                            break;
                        case Directions.D:
                            reqs.D |= ConnectionType.Entrance;
                            if (nots.D.HasFlag(ConnectionType.Entrance)) nots.D -= ConnectionType.Entrance;
                            if (nots.D.HasFlag(ConnectionType.Exit)) nots.D -= ConnectionType.Exit;
                            break;
                        case Directions.L:
                            reqs.L |= ConnectionType.Entrance;
                            if (nots.L.HasFlag(ConnectionType.Entrance)) nots.L -= ConnectionType.Entrance;
                            if (nots.L.HasFlag(ConnectionType.Exit)) nots.L -= ConnectionType.Exit;
                            break;
                        case Directions.R:
                            reqs.R |= ConnectionType.Entrance;
                            if (nots.R.HasFlag(ConnectionType.Entrance)) nots.R -= ConnectionType.Entrance;
                            if (nots.R.HasFlag(ConnectionType.Exit)) nots.R -= ConnectionType.Exit;
                            break;
                    }

                    // Try to get options from Levels
                    // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
                    List<(Level, MapConnections, Dictionary<TileID, TileID>)> options;
                    (Level level, MapConnections mc, Dictionary<TileID, TileID> dict) choice;
                    MapScreen screen;
                    string screen_id;
                    options = GetOptionsComplex(Directions.None, reqs, nots, area.Levels, force_backtrack);
                    options = FilterOptionsOpen(reqs, options);

                    if (options.Count != 0)
                    {
                        choice = options[RNG.random.Next(0, options.Count)];
                        screen_id = $"{++area.LevelCount}";
                        screen = new MapScreen(area.ID, screen_id, ScreenType.Level, choice.level, choice.mc, choice.dict, $"{(char)area.EDir.Opposite()}", area.LevelCollectables);
                    }
                    else
                    {
                        options = GetOptionsComplex(Directions.None, reqs, nots, Connectors);
                        options = FilterOptionsOpen(reqs, options);

                        choice = options[RNG.random.Next(0, options.Count)];
                        screen_id = $"x{++area.ConnectorCount}";
                        screen = new MapScreen(area.ID, screen_id, ScreenType.Connector, choice.Item1, choice.Item2, choice.Item3, $"{(char)area.EDir.Opposite()}");
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

                    if (area.AreaType == AreaType.dark)
                        WorldMap.game_over_checkpoints += $"[{area.ID}, {screen_id}] ";
                    if (area.AreaType == AreaType.ironcart)
                        WorldMap.iron_cart_entrypoints += $"{screen_id} ";
                    if (area.AreaType == AreaType.cart)
                        screen.BlockEntrances = area.EDir;

                    // place new screen and add the first open end(s)
                    PlaceScreen(area.ECoords.I, area.ECoords.J, screen);
                    area.ChosenScreens.Add(screen);
                    AddEnds(area.OpenEnds, area.DeadEnds, area.SecretEnds, screen, area.ECoords, ends_ignore);

                    // Place dot screens from first level to edge of map
                    if (area.EDir != Directions.None)
                    {
                        // This will not update the minbuilt or maxbuilt, so these may be trimmed from the map
                        // but they will prevent the generation from building over these screens
                        Pair dir = area.EDir.ToVectorPair();
                        area.ECoords = DotsFromCellToEdge(area.Map, area.ECoords, dir); // adjusts the entry coords to (hopefully) the edge of the map
                        if (area.ECoords.I == -1)
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
                    OpenEnd end;
                    MapConnections reqs = NoMapConnections;
                    MapConnections nots = NoMapConnections;

                    while (area.LevelCount < area.LevelQuota)
                    {
                        end = GetFarthestEnd(area.OpenEnds, 3);

                        CheckNeighbors(area, end.Coords, out reqs, out nots);

                        (Level level, MapConnections mc, Dictionary<TileID, TileID> dict) choice;
                        string screen_id;

                        List<(Level, MapConnections, Dictionary<TileID, TileID>)> options;
                        options = GetOptionsComplex(area.NoBuild, reqs, nots, area.Levels, force_backtrack);
                        options = FilterOptionsOpen(reqs, options);
                        Collectables collectables = area.LevelCollectables;

                        if (options.Count == 0)
                        {
                            options = GetOptionsComplex(area.NoBuild, reqs, nots, Connectors);
                            List<(Level, MapConnections, Dictionary<TileID, TileID>)> options_open = FilterOptionsOpen(reqs, options);
                            options = (options_open.Count != 0) ? options_open : options;

                            if (options.Count == 0)
                                throw new Exception("Ran out of options during MapArea generation.");

                            screen_id = $"{++area.ConnectorCount}";
                            collectables = Collectables.None;
                        }
                        else screen_id = $"{++area.LevelCount}";

                        choice = options[RNG.random.Next(0, options.Count)];
                        screen = new MapScreen(area.ID, screen_id, ScreenType.Connector, choice.level, choice.mc, choice.dict, end.PathTrace, collectables);

                        PlaceScreen(end.Coords.I, end.Coords.J, screen);
                        area.ChosenScreens.Add(screen);
                        AddEnds(area.OpenEnds, area.DeadEnds, area.SecretEnds, screen, end.Coords);
                        area.OpenEnds.Remove(end.Coords);

                        // TODO: Remove the newly placed screen from the list of screens available
                        // TODO: Remove the newly placed screen from the list of screens available
                        // TODO: Remove the newly placed screen from the list of screens available

                        if (area.OpenEnds.Count == 0)
                        {
                            PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                            throw new Exception($"Ran out of OpenEnds in area: {area.ID}");
                        }
                    }

                    // Area terminates with a cart end
                    if (area.AreaType == AreaType.cart)
                    {
                        // Get the last screen added by the main generation loop and set the block entrances directions based on last nots
                        screen.BlockEntrances = reqs.Flatten().Opposite();
                        screen.Collectables |= Collectables.LevelGoal;
                    }
                }

                // If the area is a cart, skip placing the end level and secret areas.
                if (area.AreaType == AreaType.cart)
                    goto EndBuildSecretAreas;

                // Place Final Level
                if (area.Child != null)
                {
                    // Select the open end with the greatest distance to be the area end
                    OpenEnd area_end = new OpenEnd(new Pair(-1, -1));
                    int dist = 0;

                    foreach (var end in area.OpenEnds)
                    {
                        Pair coords = end.Key;

                        if (!CheckFromCellToEdge(area.Map, coords, area.XDir.ToVectorPair()))
                            continue;

                        // Find the end with the greatest distance
                        if (end.Value.PathTrace.Length > dist)
                            area_end = end.Value;
                    }

                    // Error if the areaEnd is (-1, -1)
                    // This can occur if the only OpenEnd is a dead end
                    if (area_end.Coords.I == -1)
                    {
                        PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                        throw new Exception($"Could not select area end in area {area.ID}.");
                    }

                    MapConnections reqs;
                    MapConnections nots;
                    Directions ends_ignore = Directions.None;

                    CheckNeighbors(area, area_end.Coords, out reqs, out nots);

                    // Manually alter the exit requirements based on exit_dir (No multi-direction cases)
                    // Nots are cleared in exit direction so that MapBounds do not prevent an exit touching the map's edge
                    switch (area.XDir)
                    {
                        case Directions.U:
                            reqs.U |= ConnectionType.Exit;      // require an upwards exit
                            nots.U = ConnectionType.None;       // clear nots.U
                            ends_ignore |= Directions.U;
                            break;
                        case Directions.D:
                            reqs.D |= ConnectionType.Exit;      // require a downwards exit
                            nots.D = ConnectionType.None;       // clear nots.D
                            ends_ignore |= Directions.D;
                            break;
                        case Directions.L:
                            reqs.L |= ConnectionType.Exit;      // require a left exit
                            nots.L = ConnectionType.None;       // clear nots.R
                            ends_ignore |= Directions.L;
                            break;
                        case Directions.R:
                            reqs.R |= ConnectionType.Exit;      // require a right exit
                            nots.R = ConnectionType.None;       // clear nots.R
                            ends_ignore |= Directions.R;
                            break;
                    }

                    // Try to get options from Levels
                    // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
                    Level level;
                    MapScreen screen;
                    string screen_id;

                    List<Level> options;
                    options = GetOptions(area.NoBuild, reqs, nots, area.Levels);

                    if (options.Count != 0)
                    {
                        level = options[RNG.random.Next(0, options.Count)];
                        screen_id = $"{++area.LevelCount}";
                        screen = new MapScreen(area.ID, screen_id, ScreenType.Level, level, level.MapConnections, null, area_end.PathTrace, area.LevelCollectables);
                    }
                    else
                    {
                        options = GetOptions(area.NoBuild, reqs, nots, Connectors);

                        level = options[RNG.random.Next(0, options.Count)];
                        screen_id = $"x{++area.ConnectorCount}";
                        screen = new MapScreen(area.ID, screen_id, ScreenType.Connector, level, level.MapConnections, null, area_end.PathTrace);
                    }

                    // add exit screen to worldmap screenwipes
                    switch (area.XDir)
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

                    // place new screen
                    PlaceScreen(area_end.Coords.I, area_end.Coords.J, screen);
                    AddEnds(area.OpenEnds, area.DeadEnds, area.SecretEnds, screen, area_end.Coords, ends_ignore);
                    area.ChosenScreens.Add(screen);

                    // Remove the selected end so that it is not capped in a later step.
                    area.OpenEnds.Remove(area_end.Coords);

                    // Ensure that the exit screen connects to the edge of the map
                    Pair x_vec = area.XDir.ToVectorPair();
                    area.XCoords = DotsFromCellToEdge(area.Map, area_end.Coords, x_vec);
                    if (area.ECoords.I == -1)
                    {
                        PrintDebugCSV(area, $"tools/map testing/map_{area.ID}_exitdots.csv");
                        throw new Exception($"Area {area.ID} unable to connect final level to edge of map.");
                    }
                }
                else
                {
                    // Select the area end
                    OpenEnd area_end = GetFarthestEnd(area.OpenEnds);

                    // Error if the areaEnd is (-1, -1)
                    // This can occur if the only OpenEnd is a dead end
                    if (area_end.Coords.I == -1)
                    {
                        PrintDebugCSV(area, $"tools/map testing/map_{area.ID}_exitdots.csv");
                        throw new Exception($"Could not select area end in area {area.ID}.");
                    }

                    MapConnections reqs;
                    MapConnections nots;
                    CheckNeighbors(area, area_end.Coords, out reqs, out nots);

                    List<(Level, MapConnections, Dictionary<TileID, TileID>)> options;
                    if (area.ExitCollectables.HasFlag(Collectables.FriendOrb))
                        options = GetOptionsComplex(area.NoBuild, reqs, nots, FriendOrb, force_backtrack);     // Try to get options from FriendOrb levels
                    else
                        options = GetOptionsComplex(area.NoBuild, reqs, nots, Connectors, force_backtrack);     // Try to get options from Connectors
                    options = FilterOptionsClosed(reqs, options);

                    // Get random level from the list of options
                    (Level level, MapConnections mc, Dictionary<TileID, TileID> dict) choice = options[RNG.random.Next(0, options.Count)];

                    // Create screen to place
                    MapScreen screen;
                    string screen_id;
                    {
                        screen_id = $"x{++area.ConnectorCount}";
                        screen = new MapScreen(area.ID, screen_id, ScreenType.Connector, choice.level, choice.mc, choice.dict, area_end.PathTrace, area.ExitCollectables);
                        Console.WriteLine($"{area.ID}: {area.ExitCollectables}");
                        Console.WriteLine($"{screen.FullID}: {screen.Collectables}");
                    }

                    // place new screen
                    PlaceScreen(area_end.Coords.I, area_end.Coords.J, screen);
                    //AddEnds(area, screen, area_end.Coords);
                    area.ChosenScreens.Add(screen);

                    // Remove the selected end so that it is not capped in a later step.
                    area.OpenEnds.Remove(area_end.Coords);
                }

                // Build Secret Areas
                if (area.AreaType != AreaType.cart)
                {
                    // If there are no SecretEnds, use OpenEnds instead
                    if (area.SecretEnds.Count == 0)
                    {
                        area.SecretEnds = area.OpenEnds;
                        Console.WriteLine($"Subbed OpenEnds for SecretEnds in area {area.ID}.");
                    }
                    if (area.SecretEnds.Count == 0)
                    {
                        // If OpenEnds is empty as well, we still need to place the area's cart somewhere ( in a random level I suppose. )
                        // Place the necessary collectables in a random level i guess
                        MapScreen choice = area.ChosenScreens[RNG.random.Next(0, area.ChosenScreens.Count)];
                        choice.Collectables = Collectables.Cartridge;
                        goto EndBuildSecretAreas;
                    }

                    int total_level_count = 0;

                    // Use first available end to build cart secret
                    {
                        OpenEnd end = area.SecretEnds.ElementAt(RNG.random.Next(0, area.SecretEnds.Count)).Value;
                        BuildSecret(end, Collectables.Cartridge);
                        area.SecretEnds.Remove(end.Coords);
                    }

                    // Build remaining secret areas (mega tumors)
                    while (area.SecretEnds.Values.Count != 0)
                        BuildSecret(area.SecretEnds.Values.First(), Collectables.MegaTumor);

                    area.SecretEnds = new Dictionary<Pair, OpenEnd>();

                    void BuildSecret(OpenEnd initial_end, Collectables col_needed)
                    {
                        int internal_level_count = 0;
                        Dictionary<Pair, OpenEnd> internal_ends = new Dictionary<Pair, OpenEnd>
                        {
                            { initial_end.Coords, initial_end }
                        };
                        area.SecretEnds.Remove(initial_end.Coords);    // Remove this coord from secretEnds ahead of using it, since this will prevent connecting to the level in checkneighbors.

                        Loop:

                        OpenEnd curr_end = internal_ends.ElementAt(RNG.random.Next(0, internal_ends.Count)).Value;

                        CheckNeighbors(area, curr_end.Coords, out MapConnections reqs, out MapConnections nots);

                        List<(Level, MapConnections, Dictionary<TileID, TileID>)> options = GetOptionsComplex(Directions.None, reqs, nots, Secrets);
                        if (options.Count == 0)
                            throw new Exception($"Ran out of options while placing secrets in area {area.ID}.");

                        // if there are no open options, make sure the level placed contains all necessary collectables and a backwarp
                        List<(Level, MapConnections, Dictionary<TileID, TileID>)> options_open = FilterOptionsOpen(reqs, options, 1);
                        if (options_open.Count == 0 || internal_level_count == 2 || RNG.CoinFlip())
                        {
                            // try to use explicitly closed options
                            List<(Level, MapConnections, Dictionary<TileID, TileID>)> options_closed = FilterOptionsClosed(reqs, options);
                            if (options_closed.Count != 0)
                                options = options_closed;

                            (Level level, MapConnections mc, Dictionary<TileID, TileID> dict) choice = options[RNG.random.Next(0, options.Count)];
                            MapScreen screen = new MapScreen(area.ID, $"s{++total_level_count}", ScreenType.Secret, choice.level, choice.mc, choice.dict, curr_end.PathTrace, col_needed | Collectables.ExitWarp);
                            screen.DebugNotes += $"Placed on OpenEnd created by {curr_end.PlacedBy}.";
                            PlaceScreen(curr_end.Coords.I, curr_end.Coords.J, screen);
                            area.ChosenScreens.Add(screen);
                            internal_ends.Remove(curr_end.Coords);
                            AddEnds(internal_ends, area.DeadEnds, area.SecretEnds, screen, curr_end.Coords);
                            goto After;
                        }

                        // If we have open options, use those
                        {
                            (Level level, MapConnections mc, Dictionary<TileID, TileID> dict) choice = options_open[RNG.random.Next(0, options_open.Count)];
                            MapScreen screen = new MapScreen(area.ID, $"s{++total_level_count}", ScreenType.Secret, choice.level, choice.mc, choice.dict, curr_end.PathTrace, Collectables.None);
                            screen.DebugNotes += $"Placed on OpenEnd created by {curr_end.PlacedBy}.";
                            PlaceScreen(curr_end.Coords.I, curr_end.Coords.J, screen);
                            area.ChosenScreens.Add(screen);
                            internal_ends.Remove(curr_end.Coords);
                            AddEnds(internal_ends, area.DeadEnds, area.SecretEnds, screen, curr_end.Coords);
                            internal_level_count++;
                        }

                        goto Loop;

                        After:
                        foreach (var internal_end in internal_ends.Values)
                            CapOpenEnd(internal_end);
                    }
                }
                EndBuildSecretAreas:

                // Cap all Open Ends
                foreach (var end in area.OpenEnds)
                    CapOpenEnd(end.Value);

                // Cap all Dead Ends
                foreach (var end in area.DeadEnds)
                    CapOpenEnd(end.Value);

                // No *real* need to worry about removing the ends as we go, since we will not be using the lists anymore after this.
                // They area all re-initialized here only so that the debug output displays ends properly
                area.OpenEnds = area.DeadEnds = area.SecretEnds = new Dictionary<Pair, OpenEnd>();

                // Crop the map
                area.Map = GameMap.CropMap(max_built.I - min_built.I + 1, max_built.J - min_built.J + 1, area.Map, min_built.I, min_built.J);
                PrintDebugCSV(area, $"tools/map testing/{area.ID}_aftercrop.csv");

                // Adjust the ECoords and XCoords
                area.ECoords -= min_built;
                area.XCoords -= min_built;
                area.ECoords.I  = Math.Min(area.ECoords.I , area.Map.Height - 1);
                area.ECoords.J = Math.Min(area.ECoords.J, area.Map.Width  - 1);
                area.XCoords.I  = Math.Min(area.XCoords.I , area.Map.Height - 1);
                area.XCoords.J = Math.Min(area.XCoords.J, area.Map.Width  - 1);
                area.ECoords.I  = Math.Max(area.ECoords.I , 0);
                area.ECoords.J = Math.Max(area.ECoords.J, 0);
                area.XCoords.I  = Math.Max(area.XCoords.I , 0);
                area.XCoords.J = Math.Max(area.XCoords.J, 0);

                GenerateDataFiles();

                // Link area to child
                if (area.Child == null)
                    return (area.Map, area.ECoords);

                // Get the result of generaing and linking the child area
                (GameMap child_map, Pair child_e_coord) = GenerateAndLink(area.Child);

                // Combine the current area map and child map
                (GameMap new_map, Pair m1_offset) = CombineMaps(area.Map, child_map, area.XCoords, child_e_coord, area.XDir);

                // Get the new entry coord and add dots to map
                Pair new_e_coord = area.ECoords + m1_offset;
                Pair e_vec = area.EDir.ToVectorPair();
                if (e_vec != new Pair(0, 0))
                    new_e_coord = DotsFromCellToEdge(new_map, new_e_coord, e_vec);

                return (new_map, new_e_coord);

                bool PlaceScreen(int i, int j, MapScreen screen)
                {
                    if (MapBoundsCheck(area.Map, i, j))
                    {
                        if (area.Map.Data[i, j] != null)
                            Console.WriteLine($"Overwrote a cell at {i}, {j}:\n\t{area.Map.Data[i, j].FullID} -> {screen.FullID}");

                        area.Map.Data[i, j] = screen;
                        UpdateMinMaxCoords(i, j);

                        return true;
                    }
                    return false;
                }
                void UpdateMinMaxCoords(int i, int j)
                {
                    // Update the minbuilt and maxbuilt coords
                    min_built.I = Math.Min(min_built.I, i);
                    min_built.J = Math.Min(min_built.J, j);
                    max_built.I = Math.Max(max_built.I, i);
                    max_built.J = Math.Max(max_built.J, j);
                }
                void AddEnds(Dictionary<Pair, OpenEnd> open_ends, Dictionary<Pair, OpenEnd> dead_ends, Dictionary<Pair, OpenEnd> secret_ends, MapScreen screen, Pair coords, Directions ignore = Directions.None)
                {
                    // This function calls AddEnd for each direction of connection
                    // The index given is the screen that is Up, Down, Left, or Right from the screen just placed.
                    // The ConnectionType given is the connection type of the transition leading into the given screen.

                    MapConnections cons = screen.MapConnections;

                    // Only check direction if a connection exists and we are not set to ignore it
                    if (cons.U != ConnectionType.None && !ignore.HasFlag(Directions.U))
                        AddEnd(new Pair(-1, 0), Directions.U, cons.U);
                    if (cons.D != ConnectionType.None && !ignore.HasFlag(Directions.D))
                        AddEnd(new Pair(1, 0), Directions.D, cons.D);
                    if (cons.L != ConnectionType.None && !ignore.HasFlag(Directions.L))
                        AddEnd(new Pair(0, -1), Directions.L, cons.L);
                    if (cons.R != ConnectionType.None && !ignore.HasFlag(Directions.R))
                        AddEnd(new Pair(0, 1), Directions.R, cons.R);

                    if (cons.UR != ConnectionType.None && !ignore.HasFlag(Directions.UR))
                        AddEnd(new Pair(-1, 1), Directions.UR, cons.UR);
                    if (cons.DR != ConnectionType.None && !ignore.HasFlag(Directions.DR))
                        AddEnd(new Pair(1, 1), Directions.DR, cons.DR);
                    if (cons.UL != ConnectionType.None && !ignore.HasFlag(Directions.UL))
                        AddEnd(new Pair(-1, -1), Directions.UL, cons.UL);
                    if (cons.DL != ConnectionType.None && !ignore.HasFlag(Directions.DL))
                        AddEnd(new Pair(1, -1), Directions.DL, cons.DL);

                    return;

                    void AddEnd(Pair vec, Directions dir, ConnectionType con)
                    {
                        Pair index = coords + vec;

                    Loop:

                        if (!MapBoundsCheck(area.Map, index.I, index.J))
                        {
                            Console.WriteLine($"Tried to place an end off of the map:\n\t{screen.FullID} {screen.FullID} {dir} {con}");
                            return;
                        }

                        MapScreen neighbor = area.Map.Data[index.I, index.J];

                        if (neighbor != null)
                        {
                            if (neighbor.Type == ScreenType.Dots)
                            {
                                index += vec;
                                goto Loop;
                            }

                            if (con.HasFlag(ConnectionType.Exit))
                            {
                                string new_pathtrace = screen.PathTrace + (char)dir;
                                neighbor.PathTrace = (new_pathtrace.Length < neighbor.PathTrace.Length) ? new_pathtrace : neighbor.PathTrace;
                            }
                            else if (con.HasFlag(ConnectionType.Secret))
                                screen.BlockEntrances |= dir;

                            return;
                        }

                        // neighbor is null

                        if (con.HasFlag(ConnectionType.Exit))
                        {
                            string new_pathtrace = screen.PathTrace + (char)dir;

                            if (dead_ends.TryGetValue(index, out OpenEnd d_end))
                            {
                                area.DeadEnds.Remove(index);
                                open_ends.Add(index, new OpenEnd(index, screen.FullID, d_end.NumNeighbors + 1, new_pathtrace));
                            }
                            else if (open_ends.TryGetValue(index, out OpenEnd o_end))
                            {
                                o_end.PathTrace = (new_pathtrace.Length < o_end.PathTrace.Length) ? new_pathtrace : o_end.PathTrace;
                                o_end.NumNeighbors++;
                            }
                            else open_ends.Add(index, new OpenEnd(index, screen.FullID, 1, new_pathtrace));

                            UpdateMinMaxCoords(index.I, index.J);
                            return;
                        }

                        if (con.HasFlag(ConnectionType.Entrance))
                        {
                            if (open_ends.TryGetValue(index, out OpenEnd o_end))
                                o_end.NumNeighbors++;
                            else area.DeadEnds.Add(index, new OpenEnd(index, screen.FullID, 1));

                            UpdateMinMaxCoords(index.I, index.J);
                            return;
                        }

                        if (con.HasFlag(ConnectionType.Secret) && !open_ends.ContainsKey(index) && !area.DeadEnds.ContainsKey(index) && !area.SecretEnds.ContainsKey(index))
                        {
                            secret_ends.Add(index, new OpenEnd(index, screen.FullID, 1, screen.PathTrace + (char)dir));
                            screen.MapConnections[dir] = ConnectionType.Exit; // Replace secret connection with exit
                            UpdateMinMaxCoords(index.I, index.J);
                            return;
                        }
                    }
                }
                void CapOpenEnd(OpenEnd end, Collectables collectables = Collectables.None)
                {
                    // Check the neighbors to get the requirements
                    CheckNeighbors(area, end.Coords, out MapConnections reqs, out MapConnections nots);

                    // Get options from pool of screens
                    List<(Level, MapConnections, Dictionary<TileID, TileID>)> options = GetOptionsComplex(area.NoBuild, reqs, nots, Connectors);
                    options = FilterOptionsClosed(reqs, options);

                    // Get random level from the list of options
                    (Level, MapConnections, Dictionary<TileID, TileID>) choice = options[RNG.random.Next(0, options.Count)];

                    // Place screen
                    MapScreen screen = new MapScreen(area.ID, $"x{++area.ConnectorCount}", ScreenType.Connector, choice.Item1, choice.Item2, choice.Item3, end.PathTrace, collectables);
                    PlaceScreen(end.Coords.I, end.Coords.J, screen);
                    area.ChosenScreens.Add(screen);
                }
            }
            (GameMap, Pair) Loaded()
            {
                string[][] arr = Utility.LoadCSV(area.CSVPath, out int length);
                area.Map = new GameMap(arr.Length, length);
                for (int row = 0; row < arr.Length; row++)
                {
                    string[] line = arr[row];
                    for (int col = 0; col < line.Length; col++)
                    {
                        if (line[col] == "") continue;          // go to next col if cell is empty
                        string[] cell = line[col].Split('/');   // split the cell on '/' to get info

                        // read levelnum and tags
                        string screen_num = cell[0];
                        string tags = cell[1];
                        (Level level, MapConnections mc, Dictionary<TileID, TileID> dict) choice = (null, NoMapConnections, null);

                        // randomly chosen levels
                        if (cell[2][0] == '*')  // check if first char is '*'. This signifies a randomized level
                        {
                            // create the mapconnections
                            MapConnections reqs = new MapConnections();
                            if (cell[2].Contains('U')) reqs.U |= ConnectionType.Entrance;
                            if (cell[2].Contains('D')) reqs.D |= ConnectionType.Entrance;
                            if (cell[2].Contains('L')) reqs.L |= ConnectionType.Entrance;
                            if (cell[2].Contains('R')) reqs.R |= ConnectionType.Entrance;
                            if (cell[3].Contains('U')) reqs.U |= ConnectionType.Exit;
                            if (cell[3].Contains('D')) reqs.D |= ConnectionType.Exit;
                            if (cell[3].Contains('L')) reqs.L |= ConnectionType.Exit;
                            if (cell[3].Contains('R')) reqs.R |= ConnectionType.Exit;

                            // get a random level with only reqs
                            List<(Level, MapConnections, Dictionary<TileID, TileID>)> options = GetOptionsComplex(area.NoBuild, reqs, NoMapConnections, area.Levels);
                            options = FilterOptionsClosed(reqs, options);
                            choice = options[RNG.random.Next(0, options.Count)];
                        }
                        else
                        {
                            // levels with specified filenames
                            choice.level = new Level();
                            choice.level.Name = $"{cell[2]}";
                            choice.level.Path = $"{area.LevelPath}";
                            choice.level.TSDefault = new Tileset();
                            choice.level.TSNeed = new Tileset();
                        }

                        // process level tags
                        // perform necessary operations/modifications on level
                        if (tags.Contains("Xu"))
                        {
                            area.XUpCoords = new Pair(row, col);
                            WorldMap.upwipes += $"{area.ID}-{screen_num} ";
                        }
                        if (tags.Contains("Xd"))
                        {
                            area.XDownCoords = new Pair(row, col);
                            WorldMap.downwipes += $"{area.ID}-{screen_num} ";
                        }
                        if (tags.Contains("Xl"))
                        {
                            area.XLeftCoords = new Pair(row, col);
                            WorldMap.leftwipes += $"{area.ID}-{screen_num} ";
                        }
                        if (tags.Contains("Xr"))
                        {
                            area.XRightCoords = new Pair(row, col);
                            WorldMap.rightwipes += $"{area.ID}-{screen_num} ";
                        }

                        // add the level to map
                        MapScreen screen = new MapScreen(area.ID, screen_num, ScreenType.Level, choice.level, choice.mc, choice.dict, "", Collectables.None);
                        area.Map.Data[row, col] = screen;
                        area.ChosenScreens.Add(screen);
                    }
                }
                if (area.ChildU != null) DotsFromCellToEdge(area.Map, area.XUpCoords   , new Pair(-1,  0));
                if (area.ChildD != null) DotsFromCellToEdge(area.Map, area.XDownCoords , new Pair( 1,  0));
                if (area.ChildL != null) DotsFromCellToEdge(area.Map, area.XLeftCoords , new Pair( 0, -1));
                if (area.ChildR != null) DotsFromCellToEdge(area.Map, area.XRightCoords, new Pair( 0,  1));

                GenerateDataFiles();
                return LinkMultiple();
            }
            (GameMap, Pair) Split()
            {
                // Create a single screen map, place all exit coords at (0,0)
                area.Map = new GameMap(1, 1);
                area.XUpCoords = new Pair(0, 0);
                area.XDownCoords = new Pair(0, 0);
                area.XLeftCoords = new Pair(0, 0);
                area.XRightCoords = new Pair(0, 0);

                // just add wipes in every direction
                WorldMap.upwipes += $"{area.ID}-x{area.ConnectorCount} ";
                WorldMap.downwipes += $"{area.ID}-x{area.ConnectorCount} ";
                WorldMap.leftwipes += $"{area.ID}-x{area.ConnectorCount} ";
                WorldMap.rightwipes += $"{area.ID}-x{area.ConnectorCount} ";

                // get reqs for split level
                MapConnections reqs = new MapConnections();
                MapConnections nots = new MapConnections();
                // entrance
                switch (area.EDir)
                {
                    case Directions.U:
                        reqs.U = ConnectionType.Entrance;
                        break;
                    case Directions.D:
                        reqs.D = ConnectionType.Entrance;
                        break;
                    case Directions.L:
                        reqs.L = ConnectionType.Entrance;
                        break;
                    case Directions.R:
                        reqs.R = ConnectionType.Entrance;
                        break;
                }
                // exits
                if (area.ChildU != null) reqs.U = ConnectionType.Exit;
                if (area.ChildD != null) reqs.D = ConnectionType.Exit;
                if (area.ChildL != null) reqs.L = ConnectionType.Exit;
                if (area.ChildR != null) reqs.R = ConnectionType.Exit;

                List<(Level, MapConnections, Dictionary<TileID, TileID>)> options = GetOptionsComplex(area.NoBuild, reqs, NoMapConnections, Connectors);
                options = FilterOptionsClosed(reqs, options);
                (Level, MapConnections, Dictionary<TileID, TileID>) choice = options[RNG.random.Next(0, options.Count)];
                MapScreen screen = new MapScreen(area.ID, $"x{area.ConnectorCount}", ScreenType.Connector, choice.Item1, choice.Item2, choice.Item3, "", Collectables.None);
                area.Map.Data[0, 0] = screen;
                area.ChosenScreens.Add(screen);

                GenerateDataFiles();
                return LinkMultiple();
            }

            // Does the linking for both split and loaded areas
            (GameMap, Pair) LinkMultiple()
            {
                Pair xd_coord = area.XDownCoords,
                     xl_coord = area.XLeftCoords,
                     xr_coord = area.XRightCoords,
                     new_e_coord = new Pair(0, 0);

                bool had_child = false;
                GameMap ret_map = area.Map;

                if (area.ChildU != null)
                {
                    (GameMap child_map, Pair child_e_coord) = GenerateAndLink(area.ChildU);
                    (GameMap new_map  , Pair m1_offset    ) = CombineMaps(ret_map, child_map, area.XUpCoords, child_e_coord, Directions.U);
                    ret_map = new_map;
                    if (area.ChildD != null || area.EDir == Directions.D)
                    {
                        xd_coord += m1_offset;
                        xd_coord = DotsFromCellToEdge(ret_map, xd_coord, new Pair(1, 0));
                    }
                    if (area.ChildL != null || area.EDir == Directions.L)
                    {
                        xl_coord += m1_offset;
                        xl_coord = DotsFromCellToEdge(ret_map, xl_coord, new Pair(0, -1));
                    }
                    if (area.ChildR != null || area.EDir == Directions.R)
                    {
                        xr_coord += m1_offset;
                        xr_coord = DotsFromCellToEdge(ret_map, xr_coord, new Pair(0, 1));
                    }
                    new_e_coord += m1_offset;
                    had_child = true;
                }
                if (area.ChildD != null)
                {
                    (GameMap child_map, Pair child_e_coord) = GenerateAndLink(area.ChildD);
                    (GameMap new_map  , Pair m1_offset    ) = CombineMaps(ret_map, child_map, xd_coord, child_e_coord, Directions.D);
                    ret_map = new_map;
                    if (area.ChildL != null || area.EDir == Directions.L)
                    {
                        xl_coord += m1_offset;
                        xl_coord = DotsFromCellToEdge(ret_map, xl_coord, new Pair(0, -1));
                    }
                    if (area.ChildR != null || area.EDir == Directions.R)
                    {
                        xr_coord += m1_offset;
                        xr_coord = DotsFromCellToEdge(ret_map, xr_coord, new Pair(0, 1));
                    }
                    new_e_coord += m1_offset;
                    had_child = true;
                }
                if (area.ChildL != null)
                {
                    (GameMap child_map, Pair child_e_coord) = GenerateAndLink(area.ChildL);
                    (GameMap new_map  , Pair m1_offset    ) = CombineMaps(ret_map, child_map, xl_coord, child_e_coord, Directions.L);
                    ret_map = new_map;
                    if (area.ChildR != null || area.EDir == Directions.R)
                    {
                        xr_coord += m1_offset;
                        xr_coord = DotsFromCellToEdge(ret_map, xr_coord, new Pair(0, 1));
                    }
                    new_e_coord += m1_offset;
                    had_child = true;
                }
                if (area.ChildR != null)
                {
                    (GameMap child_map, Pair child_e_coord) = GenerateAndLink(area.ChildR);
                    (GameMap new_map  , Pair m1_offset    ) = CombineMaps(ret_map, child_map, xr_coord, child_e_coord, Directions.R);
                    ret_map = new_map;
                    new_e_coord += m1_offset;
                    had_child = true;
                }
                if (had_child)
                {
                    PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                    return (ret_map, new_e_coord);
                }
                else
                {
                    PrintDebugCSV(area, $"tools/map testing/map_{area.ID}.csv");
                    return (area.Map, area.ECoords);
                }
            }

            void GenerateDataFiles()
            {
                // Open StreamWriters for the data files
                StreamWriter levelinfo_file = File.AppendText(SaveDir + "data/levelinfo.txt.append");
                StreamWriter tilesets_file  = File.AppendText(SaveDir + "data/tilesets.txt.append");
                StreamWriter debug_file     = File.AppendText("tools/debug.md");

                // Create randomized area tileset
                if (area.Name == null) area.Name = GetFunnyAreaName();

                area.Tileset.area_type = area.AreaType.ToString();

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
                    if (screen.Type == ScreenType.Dots)
                        continue;

                    debug_file.WriteLine($"\n### Level {area.ID}-{screen.ScreenID}");
                    debug_file.WriteLine($"InFile: {screen.Level.InFile}");

                    // Load the level file associated with that screen/level
                    LevelFile level_file = LevelManip.Load(screen.Level.InFile);

                    if (screen.Level.FlipH)
                        LevelManip.FlipLevelH(ref level_file);

                    // Clean Levels (set collectables and de-colorize)
                    {
                        int ret_code = level_file.CleanLevel(screen.Collectables, screen.BlockEntrances);
                        if (ret_code != 0) ErrorNotes += $"Failed to place collectables on level {area.ID}-{screen.ScreenID}, infile {screen.Level.Name} , code {ret_code}\n";

                        level_file.ReplaceColorTiles();

                        if (screen.TileSwaps != null && screen.TileSwaps.Count != 0)
                        {
                            debug_file.WriteLine($"TileSwaps: ");
                            level_file.SwapTiles(screen.TileSwaps);      // do tile swaps
                            foreach (var swap in screen.TileSwaps)
                                debug_file.WriteLine($"{swap.Key} -> {swap.Value}");
                        }
                    }

                    debug_file.WriteLine("MapConnections: ");
                    debug_file.Write(screen.MapConnections.DebugString());
                    /*debug_file.WriteLine("Reqs: ");
                    debug_file.Write(screen.Level.DebugReqs.DebugString());
                    debug_file.WriteLine("Nots: ");
                    debug_file.Write(screen.Level.DebugNots.DebugString());*/
                    debug_file.WriteLine("Collectables: ");
                    debug_file.WriteLine(screen.Collectables);
                    debug_file.WriteLine("Path Trace: ");
                    foreach (char c in screen.PathTrace)
                    {
                        debug_file.Write($"{(Directions)c}, ");
                    }
                    debug_file.WriteLine();
                    debug_file.WriteLine(screen.DebugNotes);
                    debug_file.WriteLine();

                    if (Settings.DoCorruptions)
                        screen.Level.TSNeed.extras += LevelManip.CorruptLevel(ref level_file);

                    LevelManip.Save(level_file, SaveDir + $"tilemaps/{area.ID}-{screen.ScreenID}.lvl");

                    // Write LevelInfo
                    if (screen.Type == ScreenType.Level)
                        levelinfo_file.WriteLine($"\"{area.ID}-{screen.ScreenID}\" {{name=\"{area.Name} {screen.ScreenID}\" id={screen.ScreenID}}}");
                    else levelinfo_file.WriteLine($"\"{area.ID}-{screen.ScreenID}\" {{name=\"{area.Name}\" id=-1}}");

                    // Write Level Tileset
                    Tileset level_tileset = Tileset.PriorityMerge(screen.Level.TSDefault, area.Tileset, screen.Level.TSNeed);
                    //level_tileset = Tileset.PriorityMerge(level_tileset, screen.Level.TSNeed);
                    level_tileset = Tileset.GetDifference(area.Tileset, level_tileset);

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

        // Map Linking Functions
        static (GameMap, Pair) CombineMaps(GameMap m1, GameMap m2, Pair m1_exit, Pair m2_entry, Directions m1_exit_dir)
        {
            // This function will combine the two maps entered as parameters into a single map
            // The two maps passed in MUST have the same build direction, or map linking will not work
            // new entry is the new entry coord for the combined map

            // Declare map to be returned
            GameMap new_map;

            // Initialize origin coords for each map
            Pair m1_origin = new Pair();
            Pair m2_origin = new Pair();

            // Initialize dimensions for new map
            int height, width;

            // Determine the origin/offset of each map based on direction
            switch (m1_exit_dir)
            {
                case Directions.U:
                    m1_origin.J = Math.Max(0, m2_entry.J - m1_exit.J);           // align exit and entrance horizontally by moving leftmost transition rightwards
                    m2_origin.J = Math.Max(0, m1_exit.J - m2_entry.J);
                    m1_origin.I = m2_origin.I + m2_entry.I + 1;                     // align top edge of m1 to bottom edge of m2
                    width = Math.Max(m1_origin.J + m1.Width, m2_origin.J + m2.Width); // get rightmost map coord as width
                    height = m1_origin.I + m1.Height;
                    break;

                case Directions.D:
                    m1_origin.J = Math.Max(0, m2_entry.J - m1_exit.J);           // align exit and entrance horizontally by moving leftmost transition rightwards
                    m2_origin.J = Math.Max(0, m1_exit.J - m2_entry.J);
                    m2_origin.I = m1_origin.I + m1_exit.I + 1;                      // align top edge of m2 to bottom edge of m1
                    width = Math.Max(m1_origin.J + m1.Width, m2_origin.J + m2.Width); // get rightmost map coord as width
                    height = m2_origin.I + m2.Height;
                    break;

                case Directions.L:
                    m1_origin.I = Math.Max(0, m2_entry.I - m1_exit.I);               // align exit and entrance vertically by moving upmost transition downwards
                    m2_origin.I = Math.Max(0, m1_exit.I - m2_entry.I);
                    m1_origin.J = m2_origin.J + m2_entry.J + 1;                   // align left edge of m1 to right edge of m2
                    height = Math.Max(m1_origin.I + m1.Height, m2_origin.I + m2.Height); // get lowest map coord as height
                    width = m1_origin.J + m1.Width;
                    break;

                case Directions.R:
                    m1_origin.I = Math.Max(0, m2_entry.I - m1_exit.I);               // align exit and entrance vertically by moving upmost transition downwards
                    m2_origin.I = Math.Max(0, m1_exit.I - m2_entry.I);
                    m2_origin.J = m1_origin.J + m1_exit.J + 1;                    // align left edge of m2 to right edge of m1
                    height = Math.Max(m1_origin.I + m1.Height, m2_origin.I + m2.Height); // get lowest map coord as height
                    width = m2_origin.J + m2.Width;
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
            return (new_map, m1_origin);
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

        // Functions For Getting OpenEnds
        static OpenEnd GetFarthestEnd(Dictionary<Pair, OpenEnd> ends)
        {
            OpenEnd open_end = new OpenEnd(new Pair(-1, -1));
            int dist = 0;
            foreach (var end in ends)
            {
                if (end.Value.PathTrace.Length > dist)
                    open_end = end.Value;
            }
            if (open_end.Coords.I == -1)
                throw new Exception("Failed to select an OpenEnd.");
            return open_end;
        }
        static OpenEnd GetFarthestEnd(Dictionary<Pair, OpenEnd> ends, int max_diff)
        {
            // Returns a random end from the top options ordered by distance.
            // Leniency controls how far down the list the options can be selected.
            if (ends.Count == 1)
                return ends.First().Value;

            List<OpenEnd> options = ends.Values.OrderByDescending(k => k.PathTrace.Length).ToList();

            int max_dist   = options.First().Distance;
            int min_dist   = max_dist - max_diff;
            int max_index = 1;

            while (max_index < options.Count && options[max_index].Distance >= min_dist)
                max_index++;

            return options.ElementAt(RNG.random.Next(0, max_index));
        }
        static OpenEnd GetOpenEnd(Dictionary<Pair, OpenEnd> ends)
        {
            // Return a random OpenEnd from the list
            // Will error if there are no OpenEnds
            return ends.ElementAt(RNG.random.Next(0, ends.Count)).Value;
        }

        // Functions for Checking the GameMap
        static Pair DotsFromCellToEdge(GameMap map, Pair coords, Pair dir)
        {
            // start at given coords + dir
            coords += dir;
            // loop while the cell being checked is within the bounds
            while (MapBoundsCheck(map, coords.I, coords.J))
            {
                if (map.Data[coords.I, coords.J] != null       // check if we have hit an occupied cell
                 && map.Data[coords.I, coords.J] != DotScreen) // we are ok to overwrite dots with more dots
                    return new Pair(-1, -1);

                map.Data[coords.I, coords.J] = DotScreen;  // set the mapscreen to be dots (this will prevent building over it, and connect the desired cell to the edge of the map)
                coords += dir;  // move in direction by adding dir pair to coords being checked
            }
            coords -= dir; // get ourselves back on the map before returning coords
            return coords;
        }
        static bool CheckFromCellToEdge(GameMap map, Pair coords, Pair dir)
        {
            // start at given coords + dir
            coords += dir;
            // loop while the cell being checked is within the bounds
            while (MapBoundsCheck(map, coords.I, coords.J))
            {
                if (map.Data[coords.I, coords.J] != null       // check if we have hit an occupied cell
                 && map.Data[coords.I, coords.J] != DotScreen) // we are ok to overwrite dots with more dots
                    return false;
                coords += dir;  // move in direction by adding dir pair to coords being checked
            }
            return true;
        }
        static bool MapBoundsCheck(GameMap map, int i, int j)
        {
            if (i >= 0 && j >= 0 && i < map.Height && j < map.Width)
                return true;
            return false;
        }
        static void CheckNeighbors(MapArea area, Pair coords, out MapConnections reqs, out MapConnections nots)  // returns the type necessary to meet needs of neighbors
        {
            // initialize reqs and nots to empty
            reqs = NoMapConnections;
            nots = NoMapConnections;

            // Cardinal Directions
            CheckNeighbor(new Pair(-1,  0), ref reqs.U, ref nots.U, Directions.D); // Check Screen Up
            CheckNeighbor(new Pair( 1,  0), ref reqs.D, ref nots.D, Directions.U); // Check Screen Down
            CheckNeighbor(new Pair( 0, -1), ref reqs.L, ref nots.L, Directions.R); // Check Screen Left
            CheckNeighbor(new Pair( 0,  1), ref reqs.R, ref nots.R, Directions.L); // Check Screen Right

            // Diagonals
            CheckNeighbor(new Pair(-1,  1), ref reqs.UR, ref nots.UR, Directions.DL); // Check Screen UpRight
            CheckNeighbor(new Pair( 1,  1), ref reqs.DR, ref nots.DR, Directions.UL); // Check Screen DownRight
            CheckNeighbor(new Pair(-1, -1), ref reqs.UL, ref nots.UL, Directions.DR); // Check Screen UpLeft
            CheckNeighbor(new Pair( 1, -1), ref reqs.DL, ref nots.DL, Directions.UR); // Check Screen DownLeft

            return;

            void CheckNeighbor(Pair vec, ref ConnectionType req, ref ConnectionType not, Directions dir)
            {
                Pair index = coords + vec;

                // Program will loop back here if a dot screen is encountered
                Loop:

                // If coords are outside map bounds or contained in secretEnds
                if (!MapBoundsCheck(area.Map, index.I, index.J) || area.SecretEnds.ContainsKey(new Pair(index.I, index.J)))
                {
                    // Return a not on all connections
                    not |= ConnectionType.All;
                    return;
                }

                // Get the neighbor we want to check
                MapScreen neighbor = area.Map.Data[index.I, index.J];
                ConnectionType connection;

                // If it is null, it will not impose any requirements on entrances or exits
                if (neighbor == null)
                    return;

                // If it is dots, loop back and check next screen in direction
                if (neighbor.Type == ScreenType.Dots)
                {
                    index += vec;
                    goto Loop;
                }

                // Get the desired connection by specifying direction
                connection = neighbor.MapConnections[dir];

                // If there is an exit (or both), require an entrance
                if (connection.HasFlag(ConnectionType.Exit))
                {
                    // Set level requirements
                    req |= ConnectionType.Entrance;
                }

                // If there is only an entrance, require an exit
                else if (connection.HasFlag(ConnectionType.Entrance))
                    req |= ConnectionType.Exit;

                // If there is no connection, require a not on both
                else not |= ConnectionType.Both;
            }
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
                if ((no_build.HasFlag(Directions.U) && umc.U.HasFlag(ConnectionType.Exit))) continue;
                if ((no_build.HasFlag(Directions.D) && umc.D.HasFlag(ConnectionType.Exit))) continue;
                if ((no_build.HasFlag(Directions.L) && umc.L.HasFlag(ConnectionType.Exit))) continue;
                if ((no_build.HasFlag(Directions.R) && umc.R.HasFlag(ConnectionType.Exit))) continue;
                if ((no_build.HasFlag(Directions.UR) && umc.UR.HasFlag(ConnectionType.Exit))) continue;
                if ((no_build.HasFlag(Directions.DR) && umc.DR.HasFlag(ConnectionType.Exit))) continue;
                if ((no_build.HasFlag(Directions.UL) && umc.UL.HasFlag(ConnectionType.Exit))) continue;
                if ((no_build.HasFlag(Directions.DL) && umc.DL.HasFlag(ConnectionType.Exit))) continue;

                // Add a clone instead of the original reference
                options.Add(level.Clone());
            }
            return options;
        }
        public static List<(Level, MapConnections, Dictionary<TileID, TileID>)> GetOptionsComplex
            (Directions no_build, MapConnections reqs, MapConnections nots, List<Level> pool, bool force_backtrack = false)
        {
            List<(Level, MapConnections, Dictionary<TileID, TileID>)> options = new List<(Level, MapConnections, Dictionary<TileID, TileID>)>();

            foreach (Level base_level in pool)
            {
                // Skip base_levels which build (add exits) in disallowed directions
                {
                    // Subtract the requirements and see what remains
                    MapConnections umc = GetUniqueConnections(base_level.MapConnections, reqs);
                    if ((no_build.HasFlag(Directions.U) && umc.U.HasFlag(ConnectionType.Exit))) continue;
                    if ((no_build.HasFlag(Directions.D) && umc.D.HasFlag(ConnectionType.Exit))) continue;
                    if ((no_build.HasFlag(Directions.L) && umc.L.HasFlag(ConnectionType.Exit))) continue;
                    if ((no_build.HasFlag(Directions.R) && umc.R.HasFlag(ConnectionType.Exit))) continue;
                    if ((no_build.HasFlag(Directions.UR) && umc.UR.HasFlag(ConnectionType.Exit))) continue;
                    if ((no_build.HasFlag(Directions.DR) && umc.DR.HasFlag(ConnectionType.Exit))) continue;
                    if ((no_build.HasFlag(Directions.UL) && umc.UL.HasFlag(ConnectionType.Exit))) continue;
                    if ((no_build.HasFlag(Directions.DL) && umc.DL.HasFlag(ConnectionType.Exit))) continue;
                }

                if (force_backtrack)
                {
                    if (base_level.MapConnections.U.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.U.HasFlag(ConnectionType.Exit)) continue;
                    if (base_level.MapConnections.D.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.D.HasFlag(ConnectionType.Exit)) continue;
                    if (base_level.MapConnections.L.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.L.HasFlag(ConnectionType.Exit)) continue;
                    if (base_level.MapConnections.R.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.R.HasFlag(ConnectionType.Exit)) continue;
                    if (base_level.MapConnections.UR.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.UR.HasFlag(ConnectionType.Exit)) continue;
                    if (base_level.MapConnections.DR.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.DR.HasFlag(ConnectionType.Exit)) continue;
                    if (base_level.MapConnections.UL.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.UL.HasFlag(ConnectionType.Exit)) continue;
                    if (base_level.MapConnections.DL.HasFlag(ConnectionType.Entrance) != base_level.MapConnections.DL.HasFlag(ConnectionType.Exit)) continue;
                }

                // Check vertical connections first
                if ((base_level.MapConnections.U & reqs.U) != reqs.U) continue;
                if ((base_level.MapConnections.D & reqs.D) != reqs.D) continue;
                if ((base_level.MapConnections.U & nots.U) != ConnectionType.None) continue;
                if ((base_level.MapConnections.D & nots.D) != ConnectionType.None) continue;

                int[,] opt = new int[3, 4];
                Directions taken_dirs = Directions.None;
                int[] assignments = new int[3];
                MapConnections map_connections = base_level.MapConnections;
                Dictionary<TileID, TileID> tile_swaps = new Dictionary<TileID, TileID>();

                // right side
                TileID[] tiles = { TileID.GreenTransitionUR, TileID.GreenTransitionR, TileID.GreenTransitionDR };
                Directions[] dirs = { Directions.UR, Directions.R, Directions.DR };
                ConnectionType[] r = { reqs.UR, reqs.R, reqs.DR };
                ConnectionType[] n = { nots.UR, nots.R, nots.DR };
                ConnectionType[] mc = { base_level.MapConnections.UR, base_level.MapConnections.R, base_level.MapConnections.DR };
                if (!CheckSide()) continue;

                // left side
                tiles = new TileID[] { TileID.GreenTransitionUL, TileID.GreenTransitionL, TileID.GreenTransitionDL };
                dirs = new Directions[] { Directions.UL, Directions.L, Directions.DL };
                r = new ConnectionType[] { reqs.UL, reqs.L, reqs.DL };
                n = new ConnectionType[] { nots.UL, nots.L, nots.DL };
                mc = new ConnectionType[] { base_level.MapConnections.UL, base_level.MapConnections.L, base_level.MapConnections.DL };
                assignments = new int[3];
                opt = new int[3, 4];
                if (!CheckSide()) continue;

                options.Add((base_level, map_connections, tile_swaps));

                bool CheckSide()
                {
                    // Get options for assignments
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (taken_dirs.HasFlag(dirs[j])
                            || (r[i] & mc[j]) != r[i]
                            || (n[i] & mc[j]) != ConnectionType.None)
                                continue;
                            opt[i, opt[i, 3]++] = j;
                        }
                        if (opt[i, 3] == 0)
                        {
                            return false;
                        }
                        if (opt[i, 3] == 1)
                        {
                            assignments[i] = opt[i, 0];
                            taken_dirs |= dirs[opt[i, 0]];
                        }
                    }

                    // Make assignments
                    for (int i = 0; i < 3; i++)
                    {
                        if (opt[i, 3] < 2)
                            continue;

                        int j = 0;
                        while (j < opt[i, 3])
                        {
                            int choice = opt[i, j];
                            if (!taken_dirs.HasFlag(dirs[choice]))
                            {
                                assignments[i] = choice;
                                taken_dirs |= dirs[choice];
                                goto Assignment_Succes;
                            }
                            j++;
                        }

                        return false;
                        Assignment_Succes:;
                        // Just used to skip over the return false without having to check a bool.
                    }

                    // Use assignments to make new base_level data
                    for (int i = 0; i < 3; i++)
                    {
                        int j = assignments[i]; // get assigned j of i
                        // j will give us the direction and tileid for connection i
                        // mc[j] is the connectiontype of the connection assigned to dir[i]
                        // this is stored to the new_mc in the new direction
                        map_connections[dirs[i]] = mc[j];
                        // the transition tile orginally corresponding to j will be replaced with the tile corresponding to i
                        if (i != j) tile_swaps.Add(tiles[j], tiles[i]);
                    }
                    return true;
                }
            }
            return options;
        }
        static List<(Level, MapConnections, Dictionary<TileID, TileID>)> FilterOptionsOpen(MapConnections reqs, List<(Level, MapConnections, Dictionary<TileID, TileID>)> options, int max = 9)
        {
            // Use GetOptions or GetOptionsComplex and pass the result in to narrow down options further

            // Create new list of options
            List<(Level, MapConnections, Dictionary<TileID, TileID>)> new_options = new List<(Level, MapConnections, Dictionary<TileID, TileID>)>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.Item2, reqs);

                // Count the number of new exits added
                int count = 0;
                if (mc.U.HasFlag(ConnectionType.Exit)) count++;
                if (mc.D.HasFlag(ConnectionType.Exit)) count++;
                if (mc.L.HasFlag(ConnectionType.Exit)) count++;
                if (mc.R.HasFlag(ConnectionType.Exit)) count++;
                if (mc.UR.HasFlag(ConnectionType.Exit)) count++;
                if (mc.DR.HasFlag(ConnectionType.Exit)) count++;
                if (mc.UL.HasFlag(ConnectionType.Exit)) count++;
                if (mc.DL.HasFlag(ConnectionType.Exit)) count++;

                // Spare the level if any new exits are added (up to our max count)
                if (count > 0 && count <= max) new_options.Add(level);
            }
            return new_options;
        }
        static List<(Level, MapConnections, Dictionary<TileID, TileID>)> FilterOptionsClosed(MapConnections reqs, List<(Level, MapConnections, Dictionary<TileID, TileID>)> options)
        {
            // Use GetOptions or GetOptionsComplex and pass the result in to narrow down options further

            // Create new list of options
            List<(Level, MapConnections, Dictionary<TileID, TileID>)> newOptions = new List<(Level, MapConnections, Dictionary<TileID, TileID>)>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.Item2, reqs);

                // Skip the level if any new exits are added
                if (mc.U  != 0) continue;
                if (mc.D  != 0) continue;
                if (mc.L  != 0) continue;
                if (mc.R  != 0) continue;
                if (mc.UR != 0) continue;
                if (mc.DR != 0) continue;
                if (mc.UL != 0) continue;
                if (mc.DL != 0) continue;

                // Otherwise, add it to the new list
                newOptions.Add(level);
            }
            return newOptions;
        }
        static MapConnections GetUniqueConnections(MapConnections cons, MapConnections reqs)
        {
            MapConnections ret = NoMapConnections;

            // If the requirements impose anything on a given side, then that whole side is thrown out
            // If that side's reqs is empty, then that side contains unique connections
            if (reqs.U  == 0) ret.U  = cons.U ;
            if (reqs.D  == 0) ret.D  = cons.D ;
            if (reqs.L  == 0) ret.L  = cons.L ;
            if (reqs.R  == 0) ret.R  = cons.R ;
            if (reqs.UR == 0) ret.UR = cons.UR;
            if (reqs.DR == 0) ret.DR = cons.DR;
            if (reqs.UL == 0) ret.UL = cons.UL;
            if (reqs.DL == 0) ret.DL = cons.DL;

            return ret;
        }

        // Various Debug Functions
        public static void DebugPrintReqs(Pair coords, MapConnections reqs, MapConnections nots)
        {
            Console.WriteLine($"Unable to place level at {coords.I}, {coords.J}");
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
        public static void CountTransitionTiles(ref LevelFile level, Directions anchorflips)
        {
            // Normalizes all transition tags in a level

            // TODO:
            // need to add way to track when an entrance has already been normalized, otherwise we will basically count every entrance's tags twice.

            TileID[] TransitionTagIDs = { TileID.GreenTransitionR, TileID.GreenTransitionL, TileID.GreenTransitionD, TileID.GreenTransitionU };

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

        // Map Printing Functions
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
                                sw.Write($"{map.Data[i, j].FullID}.lvl");
                        }
                        sw.Write(",");
                    }
                    sw.Write('\n');
                }
            }
        }
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
                        if (area.DeadEnds.ContainsKey(new Pair(i, j)))
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
        static char DebugGetChar(MapConnections con)
        {
            // convert to simpler representation in form of old connections enum
            Directions con2 = Directions.None;

            if (con.U != ConnectionType.None)
                con2 |= Directions.U;
            if (con.D != ConnectionType.None)
                con2 |= Directions.D;
            if (con.L != ConnectionType.None)
                con2 |= Directions.L;
            if (con.R != ConnectionType.None)
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

            if (con.U != ConnectionType.None)
                con2 |= Directions.U;
            if (con.D != ConnectionType.None)
                con2 |= Directions.D;
            if (con.L != ConnectionType.None)
                con2 |= Directions.L;
            if (con.R != ConnectionType.None)
                con2 |= Directions.R;
            if (con.UR != ConnectionType.None)
                con2 |= Directions.UR;
            if (con.DR != ConnectionType.None)
                con2 |= Directions.DR;
            if (con.UL != ConnectionType.None)
                con2 |= Directions.UL;
            if (con.DL != ConnectionType.None)
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
