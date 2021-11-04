using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEiNRandomizer
{
    public class GameMap
    {
        // This is basically a wrapper for a 2D array which keeps record of the width and height and provides methods for copying the map's contents to a new map.

        // Data is just the 2D array of all map cells in the CSV-to-be
        public MapScreen[,] Data;

        // Width and height are kept for easy bounds checking
        public int Width { get; private set; }
        public int Height { get; private set; }

        // The default constructor initializes the width, height, and creates and empty array of the proper size.
        public GameMap(int height, int width)
        {
            Height = height;
            Width = width;
            Data = new MapScreen[height, width];
        }
        public GameMap CopyToNew(int height, int width, GameMap old, int offsetX, int offsetY)
        {
            // Create new GameMap to return 
            GameMap ret = new GameMap(height, width);
            
            // Check that the old map will fit into the new map
            if (ret.Width < old.Width + offsetX || ret.Height < old.Height + offsetY)
                throw new IndexOutOfRangeException("Cannot create new GameMap from larger original.");

            // Iterate over old map
            for (int i = 0; i < old.Height; i++)
            {
                for (int j = 0; j < old.Width; j++)
                {
                    // Copy map data to new map at offset
                    Data[i + offsetY, j + offsetX] = Data[i, j];
                }
            }

            return ret;
        }
    }

    public class MapArea
    {
        // MapArea class is used to keep track of map areas as they are created/placed

        // Basic map area information
        public string ID;
        public string Name;
        public List<Level> Levels;
        public Tileset Tileset;
        public HashSet<Pair> OpenEnds;
        //public HashSet<Pair> DeadEnds;
        public HashSet<Pair> DeadEntries;
        public HashSet<Pair> SecretEnds;

        // The default constructor randomizes and initializes the area name and tileset
        public MapArea()
        {
            // Initialize essential information
            ID = MapGenerator.Areas.Count.ToString();
            Name = Randomizer.GetFunnyName();
            Tileset = TilesetManip.GetTileset();
            Levels = new List<Level>();

            // Initialize Ends
            OpenEnds   = new HashSet<Pair>();
            DeadEntries   = new HashSet<Pair>();
            SecretEnds = new HashSet<Pair>();
        }
    }
    public enum ScreenType
    {
        Level,
        Connector,
        Secret,
        Dots
    }
    public class MapScreen
    {
        // The type of the MapScreen
        // Can be level, connector, dots, etc.
        public ScreenType Type;
        // The level located at the MapScreen location
        public Level Level;

        // This is a string of the character U, D, L, R that specify the directions to the level
        public string Directions;
        // This is the distance from the start of the area to the MapScreen
        public int Distance { get => Directions.Length; }

        public MapScreen(ScreenType type, Level level, string directions)
        {
            Type = type;
            Level = level;
            Directions = directions;
        }
    }
    public static class MapGenerator
    {
        // Reference to main settings
        private static SettingsFile Settings = AppResources.MainSettings;

        // The list of levels available to the level generator
        // This may be passed in the main randomize() function later
        public static List<Level> Levels;

        // These levels are reusable connectors that use
        // color tiles and corruptors to reduce repetition
        public static List<Level> Connectors;

        // These levels are placed at secret entrances
        // They will not be reused unless necessary
        public static List<Level> Secrets;

        // Hub levels are placed at the beginning of generation
        // They are always placed in the same configuration
        public static List<Level> HubLevels;

        // These levels are placed at the edges of the map
        // They can be reused if necessary.
        public static List<Level> WorldEdges;

        // static empty mapConnections instance for comparison purposes
        public static readonly MapConnections NoMapConnections;

        // The dimensions of the map.
        // These need to be stored so that map bounds
        // can be check when placing levels.
        const int MAP_WIDTH  = 200;
        const int MAP_HEIGHT = 200;

        // The map created by the map generator
        static GameMap Map;
        // The list of Areas created while generating the map
        public static List<MapArea> Areas;
        // The map area currently being built
        static MapArea CurrentArea;
        // Used to signify the start of a new area
        // Although the chances of these being on the same space is astronomically low, we use a hash set anyways for safety
        static HashSet<Pair> AreaEnds;
        // Used for counting the number of gameplay levels added to the current area
        static int MainLevelsCount;


        // The list of open ends is used for determining where to place the next level on the map
        // This is replaced with each area maintaining an internal list of OpenEnds
        //static Dictionary<Pair, OpenEnd> OpenEnds = new Dictionary<Pair, OpenEnd>();

        // For keeping track of keys
        static int KeysPlaced;
        static int LocksPlaced;

        public static void LoadSpecial()
        {
            // This function is responsible for loading the connector pieces, hub levels, and world edges

            HubLevels = LevelPool.LoadPool("data/level pools/.mapgen/HubLevels.gon").Levels;
            Connectors = LevelPool.LoadPool("data/level pools/.mapgen/Connectors.gon").Levels;
            // WorldEdges = LevelPool.LoadPool("data/level pools/.mapgen/WorldEdges.gon").Levels;
        }

        static void PlaceHub()
        {
            // This function will be responsible for placing all the starting hub levels before generation begins
            
            Level startLevel = Utility.FindLevelInListByName(Levels, "___R");
            MapScreen screen = new MapScreen(ScreenType.Level, startLevel, "");
            int x = MAP_WIDTH/2, y = MAP_HEIGHT/2;
            PlaceScreen(y, x, screen);
            AreaEnds.Add(new Pair(y, x + 1));
        }

        static void InitValues()
        {
            // Initialize map
            Map = new GameMap(MAP_HEIGHT, MAP_WIDTH);

            // Initialize map areas
            Areas = new List<MapArea>();
            AreaEnds = new HashSet<Pair>();

            // Initialize locks and keys
            KeysPlaced = 0;
            LocksPlaced = 0;
        }

        public static GameMap GenerateMap()
        {
            // Initialize map generation values
            InitValues();
            
            // Place hub screens
            PlaceHub();

            // Start subroutine of placing areas
            for (int i = 0; i < AppResources.MainSettings.NumAreas; i++)
            {
                // We build the next area starting with the oldest AreaEnd (first in the list)
                Pair index = AreaEnds.First();
                BuildArea(index);
                AreaEnds.Remove(index);
            }

            // Print the CSV
            PrintCSV("C:/Program Files (x86)/Steam/steamapps/common/theendisnigh/data");
            PrintDebugCSV("tools/map testing");

            // Return the map that we just created
            return Map;
        }
        static Pair GetOpenEnd()
        {
            // Return a random OpenEnd from the list
            // Will error if there are no OpenEnds
            return CurrentArea.OpenEnds.ElementAt(RNG.random.Next(0, CurrentArea.OpenEnds.Count));
        }
        static void SelectAreaEnd()
        {
            // This function selects a random OpenEnd in the area to be the AreaEnd, that is the point at which the next adjacent area will begin building

            // Declare the areaEnd and set to (-1, -1)
            // If it gets returned as this value then there will be an error
            // Under normal operation this should not happen
            Pair areaEnd = new Pair (-1, -1);
            // Set distance to zero
            int dist = 0;

            // Get a random OpenEnd that is not closed
            List<Pair> options = new List<Pair>();
            // Iterate over the list of open ends
            for (int i = 0; i < CurrentArea.OpenEnds.Count; i++)
            {
                // Get the next end to check
                Pair coords = CurrentArea.OpenEnds.ElementAt(i);

                // Count the number of empty neighbors each end has
                // If an end has empty neighbors, we can use it as an option for the areaEnd
                // We just want to weed out any remaining areaEnds which may be closed to further building
                CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);
                if (CountEmptyNeighbors(reqs, nots) != 0)
                {
                    // Find the end with the greatest distance
                    if (directions.Length > dist)
                        areaEnd = coords;
                }
            }

            // Pick a random choice from the available options
            //Pair areaEnd = options[RNG.random.Next(0, options.Count)];

            // Add the end to the areaEnds set
            AreaEnds.Add(areaEnd);

            // Remove the selected end so that it is not capped in a later step.
            CurrentArea.OpenEnds.Remove(areaEnd);
        }
        static void CapEnds()
        {
            // Cap all Open Ends
            while (CurrentArea.OpenEnds.Count != 0)
            {
                Pair coords = CurrentArea.OpenEnds.First();
                CapOpenEnd(coords);
                CurrentArea.OpenEnds.Remove(coords);
            }
            // Cap all Dead Ends
            while (CurrentArea.DeadEntries.Count != 0)
            {
                Pair coords = CurrentArea.DeadEntries.First();
                CapOpenEnd(coords);
                CurrentArea.DeadEntries.Remove(coords);
            }
        }
        static void CapOpenEnd(Pair coords)
        {
            // Check the neighbors to get the requirements
            CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);

            // Get options from pool of screens
            List<Level> options = GetOptionsClosed(reqs, nots, Connectors);

            // Get random level from the list of options
            Level level = options[RNG.random.Next(0, options.Count)];

            // Place screen
            ScreenType type = ScreenType.Level;
            MapScreen screen = new MapScreen(type, level, directions);
            PlaceScreen(coords.First, coords.Second, screen);
        }
        static void BuildSecrets()
        {

        }
        static void BuildArea(Pair startIndex)
        {
            // Create new area to build in and set as current area
            CurrentArea = new MapArea();

            // Add current area to list of areas
            Areas.Add(CurrentArea);

            // Set main levels count to zero
            MainLevelsCount = 0;

            // Set first OpenEnd to startIndex
            CurrentArea.OpenEnds.Add(startIndex);

            // add levels until the quota is met
            // create method for handling when we run out of openEnds before quota met
            while ( MainLevelsCount < Settings.NumLevels )
            {
                // Place next level
                PlaceNext();

                // Check that the OpenEnds count has not dropped below 1
                if (CurrentArea.OpenEnds.Count < 1)
                {
                    //Console.WriteLine($"Ran out of OpenEnds in area: {CurrentArea.ID}");
                    PrintDebugCSV("tools/map testing");
                    throw new Exception($"Ran out of OpenEnds in area: {CurrentArea.ID}");
                }
                //while (CurrentArea.OpenEnds.Count < 1)
                //{
                //    // In this order try:
                //    // Replace a connector with one that has more connections
                //        // Search for all connectors in area    
                //        // Order options by most empty neighbors
                //        // Select a random option and add the new OpenEnds
                //    // Replace a level with one that has more connections
                //    // Replace a level with a connector that has more connections

                //    // If all those fail, then fuck

                //    // This might not even be necessary if we keep the count >= 1 by necessity
                //    // Still, strange things could happen at the edges, and it might become an important check to make

                //}
            }

            // Select one OpenEnd as the AreaEnd
            SelectAreaEnd();

            // Cap all other OpenEnds
            CapEnds();

            // Build secret areas
            BuildSecrets();
        }
        static Pair SmartSelectOpenEnd()
        {

            // If there is only one end, use this end
            // If filling this end creates no new open ends, we will need to fix that.
            if (CurrentArea.OpenEnds.Count == 1)
                return CurrentArea.OpenEnds.First();

            // Iterate over the list of open ends
            for (int i = 0; i < CurrentArea.OpenEnds.Count; i++)
            {
                // Count the number of empty neighbors each end has
                // If an end has no empty neighbors
                // Use this end to just get it out of the way
                // This will help us avoid getting stuck with it later
                Pair coords = CurrentArea.OpenEnds.ElementAt(i);
                CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);
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
            for (int i = 0; i < CurrentArea.OpenEnds.Count; i++)
            {
                // This time we select the openEnd by greatest distance
                Pair coords = CurrentArea.OpenEnds.ElementAt(i);
                CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);
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
            return GetOpenEnd();
        }
        static bool PlaceNext()
        {
            // This function will draw a new map screen from the list of
            // available levels and add it to the map at the position of an open end.

            // This tells us whether or not to remove the level from its pool after placing
            bool isGameplay = true;

            // Get next OpenEnd to fill
            Pair coords = SmartSelectOpenEnd();

            // Get level connection requirements
            CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);

            // Get options from pool of levels
            List<Level> options;

            // Try to get open options from Levels
            options = GetOptionsOpen(reqs, nots, Levels);

            // If there are not open options in Levels
            if (options.Count == 0)
            {
                // Try to get open options from Connectors
                options = GetOptionsOpen(reqs, nots, Connectors);
                // set isGameplay to false since we are now using a connector
                isGameplay = false;
            }

            // If there are not open options in Connectors
            // Then try to get any kind of option from Connectors
            if (options.Count == 0)
                options = GetOptions(reqs, nots, Connectors);

            //// If there are not open (produces new OpenEnds) options in Connectors
            //if (options.Count == 0)
            //{
            //    // Check Levels again, but allowing closed (not producing new OpenEnds) options
            //    options = GetOptions(reqs, nots, Levels);

            //    // set isGameplay to true since we are now using a level again
            //    isGameplay = true;
            //}

            //// If there are not closed options in Levels
            //if (options.Count == 0)
            //{
            //    // Check Connectors again, but allowing closed (not producing new OpenEnds) options
            //    options = GetOptions(reqs, nots, Connectors);

            //    // set isGameplay to false since we are now using a connector
            //    isGameplay = false;
            //}

            // If there are still no options, we have a problem
            if (options.Count == 0)
            {
                DebugPrintReqs(coords, reqs, nots);
                return false;
            }

            // Get random level from the list of options
            Level level = options[RNG.random.Next(0, options.Count)];

            // Place screen
            ScreenType type = ScreenType.Level;
            if (!isGameplay)
                type = ScreenType.Connector;
            MapScreen screen = new MapScreen(type, level, directions);
            PlaceScreen(coords.First, coords.Second, screen);

            // Add new ends after placement
            AddEnds(coords.First, coords.Second, level.MapConnections);

            // Remove the OpenEnd that we are replacing
            CurrentArea.OpenEnds.Remove(new Pair(coords.First, coords.Second));

            // Add the level to the area's level list
            CurrentArea.Levels.Add(level);

            // We only want to increment the level counter when placing a gameplay level
            if (isGameplay)
                MainLevelsCount++;

            // Remove the newly placed screen from the list of screens available
            //if (isGameplay)
                //Levels.Remove(level);

            return true;
        }
        static void DebugPrintReqs(Pair coords, MapConnections reqs, MapConnections nots)
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
        static List<Level> GetOptionsOpen(MapConnections reqs, MapConnections nots, List<Level> pool)
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

                // Spare the level if any new exits are added
                if (mc.U.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.D.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.L.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.R.HasFlag(ConnectionType.exit)) newOptions.Add(level);
            }
            return newOptions;
        }
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
            if ((reqs.U & nots.U) == ConnectionType.none) val++;
            if ((reqs.D & nots.D) == ConnectionType.none) val++;
            if ((reqs.L & nots.L) == ConnectionType.none) val++;
            if ((reqs.R & nots.R) == ConnectionType.none) val++;

            // Return the number of empty neighbors counted.
            return val;
        }
        static bool MapBoundsCheck(int i, int j)
        {
            if (i >= 0 && j >= 0 && i < Map.Height && j < Map.Width)
                return true;
            return false;
        }
        static bool PlaceScreen(int i, int j, MapScreen screen)
        {
            // Check that the index is within the map's bounds
            if (MapBoundsCheck(i, j))
            {
                // Print debug output to console
                if (Map.Data[i, j] != null)
                    Console.WriteLine($"Overwrote a cell at {i}, {j}");
                // Place the screen into the map
                Map.Data[i, j] = screen;
                return true;
            }
            return false;
        }
        static void CheckNeighbors(int i, int j, out MapConnections reqs, out MapConnections nots, out string directions)  // returns the type necessary to meet needs of neighbors
        {
            // initialize reqs and nots to empty
            reqs = NoMapConnections;
            nots = NoMapConnections;
            directions = null;

            // map is row major, so i is the row and j is the column
            // get required entrances, and spots where entrances cant be

            // Check Screen Up
            CheckNeighbor(i - 1, j, ref reqs.U, ref nots.U, Direction.U, ref directions);

            // Check Screen Down
            CheckNeighbor(i + 1, j, ref reqs.D, ref nots.D, Direction.D, ref directions);

            // Check Screen Left
            CheckNeighbor(i, j - 1, ref reqs.L, ref nots.L, Direction.L, ref directions);

            // Check Screen Right
            CheckNeighbor(i, j + 1, ref reqs.R, ref nots.R, Direction.R, ref directions);

        }
        static void CheckNeighbor(int i, int j, ref ConnectionType reqs, ref ConnectionType nots, Direction dir, ref string directions)
        {
            // Must be inside map bounds and not be a secret end
            if (MapBoundsCheck(i, j) && !CurrentArea.SecretEnds.Contains(new Pair(i, j)))
            {
                // Get the neighbor we want to check
                MapScreen neighbor = Map.Data[i, j];
                ConnectionType connection;

                // ENTRANCES AND EXITS
                // If the screen is not null, check the connections
                // If it is null, it will not impose any requirements on entrances or exits
                if (neighbor != null)
                {
                    // Switch to the correct connection based on direction passed in
                    // I don't like having to do this. I would rather inline the whole function and be explicit about all this, but it is messy to look at and edit that way.
                    switch (dir)
                    {
                        case Direction.U:
                            connection = neighbor.Level.MapConnections.D;
                            break;
                        case Direction.D:
                            connection = neighbor.Level.MapConnections.U;
                            break;
                        case Direction.L:
                            connection = neighbor.Level.MapConnections.R;
                            break;
                        case Direction.R:
                            connection = neighbor.Level.MapConnections.L;
                            break;
                        default:
                            connection = neighbor.Level.MapConnections.D;
                            break;
                    }

                    // If there is an exit (or both), require an entrance
                    if (connection.HasFlag(ConnectionType.exit))
                    {
                        // Set level requirements
                        reqs |= ConnectionType.entrance;
                        // Get directions to current level
                        // Set directions to new level if shorter than existing path or if no path is set
                        if (directions == null || neighbor.Directions.Length < directions.Length)
                            directions = neighbor.Directions + (((Direction)(-(int)dir))).ToString();
                    }
                    // If there is only an entrance, require an exit
                    else if (connection.HasFlag(ConnectionType.entrance))
                        reqs |= ConnectionType.exit;
                    // If there is no connection, require a not on both
                    else nots |= ConnectionType.both;

                    // Also, if the screen is not null, then we can't have a secret here
                    nots |= ConnectionType.secret;
                }

                // SECRETS
                // If we have an OpenEnd or DeadEntry in this location, we cannot have a secret here.
                if (CurrentArea.OpenEnds.Contains(new Pair(i, j))
                    || CurrentArea.DeadEntries.Contains(new Pair(i, j)))
                {
                    nots |= ConnectionType.secret;
                }
            }
            // If the screen we are looking at is invalid, we cannot connect to it at all.
            else nots |= ConnectionType.all;  // all includes both entrances, exits, and secrets
        }
        static void AddEnds(int i, int j, MapConnections mCons)
        {
            // This function calls AddEnd for each direction of connection

            // The index given is the screen that is Up, Down, Left, or Right from the screen just placed.
            // The ConnectionType given is the connection type of the transition leading into the given screen.

            // Check Screen Up
            AddEnd(new Pair(i - 1, j), mCons.U);

            // Check Screen Down
            AddEnd(new Pair(i + 1, j), mCons.D);

            // Check Screen Left
            AddEnd(new Pair(i, j - 1), mCons.L);

            // Check Screen Right
            AddEnd(new Pair(i, j + 1), mCons.R);

        }
        static void AddEnd(Pair index, ConnectionType con)
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
                    
                if (Map.Data[index.First, index.Second] == null)
                {
                    // Remove deadEntry if there was one here
                    CurrentArea.DeadEntries.Remove(index);

                    // Add OpenEnd
                    CurrentArea.OpenEnds.Add(index);
                }
            }
            else if (con.HasFlag(ConnectionType.entrance))
            {
                // If we have only an entrance here, try to add a deadEntry
                // If there is already an openEnd here, don't add the deadEntry
                if (Map.Data[index.First, index.Second] == null)
                    if (!CurrentArea.OpenEnds.Contains(index))
                        CurrentArea.DeadEntries.Add(index);
            }
            else if (con.HasFlag(ConnectionType.secret))
            {
                // Add a secret end if applicable
                // Should be no issues with adding this since we pre-check for other types of ends in this spot during CheckNeighbors
                // For SecretEnds, we don't need to check if the screen at index is null
                CurrentArea.SecretEnds.Add(index);
            }
        }
        static void PrintCSV(string path)
        {
            using (StreamWriter sw = File.CreateText($"{path}/map.csv"))
            {
                for (int i = 0; i < Map.Height; i++)
                {
                    for (int j = 0; j < Map.Width; j++)
                    {
                        if (Map.Data[i, j] != null)
                        {
                            string name = $"{i}-{j}";

                            LevelFile file = LevelManip.Load(Map.Data[i, j].Level.InFile);
                            if (Map.Data[i, j].Level.FlippedHoriz)
                                LevelManip.FlipLevelH(ref file);
                            LevelManip.Save(file, $"C:\\Program Files (x86)\\Steam\\steamapps\\common\\theendisnigh\\tilemaps\\{name}.lvl");

                            sw.Write($"{name},");
                        }
                        else if (CurrentArea.OpenEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"O,");
                        }
                        else if (CurrentArea.DeadEntries.Contains(new Pair(i, j)))
                        {
                            sw.Write($"D,");
                        }
                        else if (CurrentArea.SecretEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"S,");
                        }
                        else if (AreaEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"A,");
                        }
                        else sw.Write("a,");
                    }
                    sw.Write('\n');
                }
            }
        }
        static void PrintDebugCSV(string path)
        {
            using (StreamWriter sw = File.CreateText($"{path}/debug.csv"))
            {
                for (int i = 0; i < MAP_HEIGHT; i++)
                {
                    for (int j = 0; j < MAP_WIDTH; j++)
                    {
                        if (Map.Data[i, j] != null)
                        {
                            sw.Write($"{DebugGetChar(Map.Data[i,j].Level.MapConnections)},");
                        }
                        else if (CurrentArea.OpenEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"O,");
                        }
                        else if (CurrentArea.DeadEntries.Contains(new Pair(i, j)))
                        {
                            sw.Write($"D,");
                        }
                        else if (CurrentArea.SecretEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"S,");
                        }
                        else if (AreaEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"A,");
                        }
                        else sw.Write(" ,");
                    }
                    sw.Write('\n');
                }
            }
        }
        static char DebugGetChar(MapConnections con)
        {

            // convert to simpler representation in form of old connections enum
            Connections con2 = Connections.empty;

            if (con.U != ConnectionType.none)
                con2 |= Connections.up;
            if (con.D != ConnectionType.none)
                con2 |= Connections.down;
            if (con.L != ConnectionType.none)
                con2 |= Connections.left;
            if (con.R != ConnectionType.none)
                con2 |= Connections.right;

            // use old switch case to get char
            switch (con2)
            {
                case Connections.right:
                    return '←';
                case Connections.left:
                    return '→';
                case Connections.left | Connections.right:
                    return '─';
                case Connections.up:
                    return '↓';
                case Connections.up | Connections.right:
                    return '└';
                case Connections.up | Connections.left:
                    return '┘';
                case Connections.up | Connections.left | Connections.right:
                    return '┴';
                case Connections.down:
                    return '↑';
                case Connections.down | Connections.right:
                    return '┌';
                case Connections.down | Connections.left:
                    return '┐';
                case Connections.down | Connections.left | Connections.right:
                    return '┬';
                case Connections.down | Connections.up:
                    return '│';
                case Connections.down | Connections.up | Connections.right:
                    return '├';
                case Connections.down | Connections.up | Connections.left:
                    return '┤';
                case Connections.all:
                    return '┼';
                default:
                    return ' ';

            }
        }
    }
}
