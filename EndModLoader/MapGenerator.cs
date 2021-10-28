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
        public Level[,] Data;

        // Width and height are kept for easy bounds checking
        public int Width { get; private set; }
        public int Height { get; private set; }

        // The default constructor initializes the width, height, and creates and empty array of the proper size.
        public GameMap(int height, int width)
        {
            Height = height;
            Width = width;
            Data = new Level[height, width];
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
        public Dictionary<Pair, LevelType> OpenEnds;

        // The default constructor randomizes and initializes the area name and tileset
        public MapArea()
        {
            // Initialize essential information
            ID = MapGenerator.Areas.Count.ToString();
            Name = Randomizer.GetFunnyName();
            Tileset = TilesetManip.GetTileset();
            Levels = new List<Level>();
            OpenEnds = new Dictionary<Pair, LevelType>();
        }
    }
    // Used in conjunction with OpenEnds dictionary to track where to place levels
    public struct OpenEnd
    {
        // The area that the OpenEnd belongs to
        //public int Area;
        // The type of the level to be placed
        public LevelType Type;
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
        const int MAP_WIDTH  = 50;
        const int MAP_HEIGHT = 50;

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
            
            var startLevel = Utility.FindLevelInListByName(Levels, "UDLR");
            int x = 5, y = 10;
            PlaceLevel(y, x, startLevel);
            AddOpenEnds(y, x, startLevel);
        }

        static void InitValues()
        {
            // Initialize map
            Map = new GameMap(MAP_HEIGHT, MAP_WIDTH);

            // Initialize map areas
            Areas = new List<MapArea>();

            // Initialize locks and keys
            KeysPlaced = 0;
            LocksPlaced = 0;
        }

        public static GameMap GenerateMap()
        {
            // Place hub screens
            PlaceHub();

            // Start subroutine of placing areas
            for (int i = 0; i < AppResources.MainSettings.NumAreas; i++)
            {
                BuildArea();
            }

            // Print the CSV
            PrintCSV();
            PrintDebugCSV();

            return Map;
        }

        static void SelectAreaEnd()
        {
            // This function selects a random OpenEnd in the area to be the AreaEnd, that is the point at which the next adjacent area will begin building
            
            // Get a random OpenEnd using the GetOpenEnd function
            Pair areaEnd = GetOpenEnd(LevelType.Standard);

            // Check that we got a valid end
            if (areaEnd.First != -1)
            {
                // Add the end to the areaEnds set
                AreaEnds.Add(areaEnd);

                // Remove the selected end so that it is not capped in a later step.
                CurrentArea.OpenEnds.Remove(areaEnd);
            }
        }
        static void CapEnds()
        {
            // Fill the list with all ends of the same type
            for (int i = 0; i < CurrentArea.OpenEnds.Count; i++)
            {
                Pair key = CurrentArea.OpenEnds.ElementAt(i).Key;
                if (CurrentArea.OpenEnds[key] == LevelType.Standard)
                {
                    CapOpenEnd(key);
                    CurrentArea.OpenEnds.Remove(key);
                }
                if (CurrentArea.OpenEnds[key] == LevelType.DeadEnd)
                {
                    CapOpenEnd(key);
                    CurrentArea.OpenEnds.Remove(key);
                }
            }
        }
        static void CapOpenEnd(Pair key)
        {
            CheckNeighbors(key.First, key.Second, out MapConnections reqs, out MapConnections nots);

            // Get special nots
            // In this case we want only the requirements, nothing more.
            nots = ~reqs;


            
        }
        static void BuildSecrets()
        {

        }
        static void BuildArea()
        {
            // Create new area to build in and set as current area
            CurrentArea = new MapArea();

            // Set main levels count to zero
            MainLevelsCount = 0;

            // add levels until the quota is met
            // create method for handling when we run out of openEnds before quota met
            while ( MainLevelsCount < Settings.NumLevels )
            {
                // Place next level
                // Will attempt to place a standard type level first
                // If that is not possible, will place a connector
                int ret = PlaceNext();

                // Return code 2 means we need to replace a screen with a connector 
                if (ret == 2)
                {
                    // look through all levels in area, picking out connectors first.
                    // try replacing a connector with a new one
                    // if we cannot do this, try replacing a gameplay level with a connector.
                }


                // We need at least one remaining OpenEnd when finishing placing levels
            }


            // once quota is met:
            // select one OpenEnd as the AreaEnd
            SelectAreaEnd();

            // cap all other OpenEnds
            CapEnds();

            // build secret areas
            BuildSecrets();

        }

        static Pair GetOpenEnd(LevelType getType)
        {
            // Check if OpenEnds is empty
            if (CurrentArea.OpenEnds.Count == 0)
            {
                Console.WriteLine("Ran out of OpenEnds");
                return new Pair(-1, 0);
            }

            // Create a list of options to return
            List<Pair> options = new List<Pair>();

            // Fill the list with all ends of the same type
            for (int i = 0; i < CurrentArea.OpenEnds.Count; i++)
            {
                if (CurrentArea.OpenEnds.ElementAt(i).Value == getType)
                {
                    options.Add(CurrentArea.OpenEnds.ElementAt(i).Key);
                }
            }

            // Need to return special value if none found
            if (options.Count == 0)
                return new Pair(-1, 0);

            // return a random option from the list
            return options[RNG.random.Next(0, options.Count)];
        }

        static int PlaceNext()
        {
            // This function will draw a new map screen from the list of
            // available levels and add it to the map at the position of an open end.
            // If the do_remove parameter is true, then the selected screen
            // will be removed from the level list

            // Return values:
            // 0 - no issues, level added successfully
            // 1 - unable to place level
            // 2 - ran out of OpenEnds (can either mean there are no openEnds whatsoever, or none of the desired type)

            // Get next OpenEnd to fill
            Pair coords = GetOpenEnd(LevelType.Standard);

            // If we have no openEnds of desired type, return 2
            if (coords.First == -1)
            {
                Console.WriteLine("No OpenEnds of standard type.");
                return 2;
            }

            // Get level connection requirements
            CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots);

            // Get options from pool of screens
            List<Level> options = GetOptions(reqs, nots, Levels);

            // If the there are no available options, try using a connector
            if (options.Count == 0)
            {
                // Retry getting connections with connectors
                options = GetOptions(reqs, nots, Connectors);
            }

            // Debug output if the number of options returns zero
            if (options.Count == 0)
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
                return 1;
            }

            // Pick which screen to place
            int choice = RNG.random.Next(0, options.Count);
            // Get the chosen screen from the list of options
            Level level = options[choice];

            // Place screen
            if (PlaceLevel(coords.First, coords.Second, level))
            {
                CurrentArea.OpenEnds.Remove(new Pair(coords.First, coords.Second));
            }
            else Console.WriteLine("Error placing level");

            // Check for open ends after placement
            AddOpenEnds(coords.First, coords.Second, level);

            // Add the level to the area's level list
            CurrentArea.Levels.Add(level);

            // We only want to increment the level counter when placing a gameplay (standard) level
            if (level.Type == LevelType.Standard)
                MainLevelsCount++;

            // Remove the newly placed screen from the list of screens available
            Levels.Remove(level);

            return 0;
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
        static bool MapBoundsCheck(int i, int j)
        {
            if (i >= 0 && j >= 0 && i < Map.Height && j < Map.Width)
                return true;
            return false;
        }

        static void CheckNeighbors(int i, int j, out MapConnections reqs, out MapConnections nots)  // returns the type necessary to meet needs of neighbors
        {
            // initialize reqs and nots to empty
            reqs = NoMapConnections;
            nots = NoMapConnections;

            // map is row major, so i is the row and j is the column
            // get required entrances, and spots where entrances cant be

            // check screen above
            if (MapBoundsCheck(i - 1, j))
            {
                // Get reference to screen in question
                Level screen = Map.Data[i - 1, j];

                // If the screen is not null, check the connections
                // If it is null, it will not impose any requirements
                if (screen != null)
                {
                    if (screen.MapConnections.D.HasFlag(ConnectionType.exit))
                        reqs.U |= ConnectionType.entrance;
                    else if (screen.MapConnections.D.HasFlag(ConnectionType.entrance))
                        reqs.U |= ConnectionType.exit;
                    else nots.U |= ConnectionType.both;
                }
            }
            // If the screen we are looking at is invalid, we cannot connect to it at all.
            else nots.U |= ConnectionType.both;

            // check screen below
            if (MapBoundsCheck(i + 1, j))
            {
                // Get reference to screen in question
                Level screen = Map.Data[i + 1, j];

                // If the screen is not null, check the connections
                // If it is null, it will not impose any requirements
                if (screen != null)
                {
                    if (screen.MapConnections.U.HasFlag(ConnectionType.exit))
                        reqs.D |= ConnectionType.entrance;
                    else if (screen.MapConnections.U.HasFlag(ConnectionType.entrance))
                        reqs.D |= ConnectionType.exit;
                    else nots.D |= ConnectionType.both;
                }
            }
            // If the screen we are looking at is invalid, we cannot connect to it at all.
            else nots.D |= ConnectionType.both;

            // check screen left
            if (MapBoundsCheck(i, j - 1))
            {
                // Get reference to screen in question
                Level screen = Map.Data[i, j - 1];

                // If the screen is not null, check the connections
                // If it is null, it will not impose any requirements
                if (screen != null)
                {
                    if (screen.MapConnections.R.HasFlag(ConnectionType.exit))
                        reqs.L |= ConnectionType.entrance;
                    else if (screen.MapConnections.R.HasFlag(ConnectionType.entrance))
                        reqs.L |= ConnectionType.exit;
                    else nots.L |= ConnectionType.both;
                }
            }
            // If the screen we are looking at is invalid, we cannot connect to it at all.
            else nots.L |= ConnectionType.both;

            // Check screen to right
            if (MapBoundsCheck(i, j + 1))
            {
                // Get reference to screen in question
                Level screen = Map.Data[i, j + 1];

                // If the screen is not null, check the connections
                // If it is null, it will not impose any requirements
                if (screen != null)
                {
                    if (screen.MapConnections.L.HasFlag(ConnectionType.exit))
                        reqs.R |= ConnectionType.entrance;
                    else if (screen.MapConnections.L.HasFlag(ConnectionType.entrance))
                        reqs.R |= ConnectionType.exit;
                    else nots.R |= ConnectionType.both;
                }
            }
            // If the screen we are looking at is invalid, we cannot connect to it at all.
            else nots.R |= ConnectionType.both;

        }
        static bool PlaceLevel(int i, int j, Level level)
        {
            // Check that the index is within the map's bounds
            if (MapBoundsCheck(i,j))
            {
                // Print debug output to console
                if (Map.Data[i, j] != null)
                    Console.WriteLine($"Overwrote a cell at {i}, {j}");
                // Place the level into the map
                Map.Data[i,j] = level;
                return true;
            }
            return false;
        }
        static void AddOpenEnds(int i, int j, Level level)
        {
            // Get reference to level.MapConnections for comparing below
            MapConnections con = level.MapConnections;

            // if screen has exits leading to empty screens, add those screens as open ends
                // or if the exits lead towards deadends, then those can be overwritten with deadends
            // else if entrances leading to empty screens, place a deadEnd
            // else if check for secret connections
            if (con.U.HasFlag(ConnectionType.exit))
            {
                if (Map.Data[i - 1, j] == null)
                {
                    if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i - 1, j), out LevelType type))
                        CurrentArea.OpenEnds.Add(new Pair(i - 1, j), LevelType.Standard);
                    else if (type ==  LevelType.DeadEnd)
                        CurrentArea.OpenEnds.Add(new Pair(i - 1, j), LevelType.Standard);
                }
            }
            else if (con.U.HasFlag(ConnectionType.entrance))
            {
                if (Map.Data[i - 1, j] == null)
                    if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i - 1, j), out LevelType type))
                        CurrentArea.OpenEnds.Add(new Pair(i - 1, j), LevelType.DeadEnd);
            }
            else if (con.U.HasFlag(ConnectionType.secret))
            {
                if (Map.Data[i - 1, j] == null)
                    if (!CurrentArea.OpenEnds.ContainsKey(new Pair(i - 1, j)))
                        CurrentArea.OpenEnds.Add(new Pair(i - 1, j), LevelType.Secret);
            }

            if (con.D.HasFlag(ConnectionType.exit) || con.D.HasFlag(ConnectionType.entrance))
            {
                if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i + 1, j), out LevelType type))
                    CurrentArea.OpenEnds.Add(new Pair(i + 1, j), LevelType.Standard);
                else if (type == LevelType.DeadEnd)
                    CurrentArea.OpenEnds.Add(new Pair(i + 1, j), LevelType.Standard);
            }
            else if (con.D.HasFlag(ConnectionType.entrance))
            {
                if (Map.Data[i + 1, j] == null)
                    if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i + 1, j), out LevelType type))
                        CurrentArea.OpenEnds.Add(new Pair(i + 1, j), LevelType.DeadEnd);
            }
            else if (con.D.HasFlag(ConnectionType.secret))
            {
                if (Map.Data[i + 1, j] == null)
                    if (!CurrentArea.OpenEnds.ContainsKey(new Pair(i + 1, j)))
                        CurrentArea.OpenEnds.Add(new Pair(i + 1, j), LevelType.Secret);
            }

            if (con.L.HasFlag(ConnectionType.exit) || con.L.HasFlag(ConnectionType.entrance))
            {
                if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i, j - 1), out LevelType type))
                    CurrentArea.OpenEnds.Add(new Pair(i, j - 1), LevelType.Standard);
                else if (type == LevelType.DeadEnd)
                    CurrentArea.OpenEnds.Add(new Pair(i, j - 1), LevelType.Standard);
            }
            else if (con.L.HasFlag(ConnectionType.entrance))
            {
                if (Map.Data[i, j - 1] == null)
                    if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i, j - 1), out LevelType type))
                        CurrentArea.OpenEnds.Add(new Pair(i, j - 1), LevelType.DeadEnd);
            }
            else if (con.L.HasFlag(ConnectionType.secret))
            {
                if (Map.Data[i, j - 1] == null)
                    if (!CurrentArea.OpenEnds.ContainsKey(new Pair(i, j - 1)))
                        CurrentArea.OpenEnds.Add(new Pair(i, j - 1), LevelType.Secret);
            }

            if (con.R.HasFlag(ConnectionType.exit) || con.R.HasFlag(ConnectionType.entrance))
            {
                if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i, j + 1), out LevelType type))
                    CurrentArea.OpenEnds.Add(new Pair(i, j + 1), LevelType.Standard);
                else if (type == LevelType.DeadEnd)
                    CurrentArea.OpenEnds.Add(new Pair(i, j + 1), LevelType.Standard);
            }
            else if (con.R.HasFlag(ConnectionType.entrance))
            {
                if (Map.Data[i, j + 1] == null)
                    if (!CurrentArea.OpenEnds.TryGetValue(new Pair(i, j + 1), out LevelType type))
                        CurrentArea.OpenEnds.Add(new Pair(i, j + 1), LevelType.DeadEnd);
            }
            else if (con.R.HasFlag(ConnectionType.secret))
            {
                if (Map.Data[i, j + 1] == null)
                    if (!CurrentArea.OpenEnds.ContainsKey(new Pair(i, j + 1)))
                        CurrentArea.OpenEnds.Add(new Pair(i, j + 1), LevelType.Standard);
            }
        }
        static void PrintCSV()
        {
            using (StreamWriter sw = File.CreateText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\theendisnigh\\data\\map.csv"))
            {
                for (int i = 0; i < Map.Height; i++)
                {
                    for (int j = 0; j < Map.Width; j++)
                    {
                        if (Map.Data[i, j] != null)
                        {
                            string name = $"{i}-{j}";

                            if (Map.Data[i, j].Name == "start") name = "start";

                            LevelFile file = LevelManip.Load(Map.Data[i, j].InFile);
                            if (Map.Data[i, j].FlippedHoriz)
                                LevelManip.FlipLevelH(ref file);
                            LevelManip.Save(file, $"C:\\Program Files (x86)\\Steam\\steamapps\\common\\theendisnigh\\tilemaps\\{name}.lvl");

                            sw.Write($"{name},");
                        }
                        else if (CurrentArea.OpenEnds.ContainsKey(new Pair(i, j)))
                        {
                            sw.Write($"OE,");
                        }
                        else sw.Write("a,");
                    }
                    sw.Write('\n');
                }
            }
        }
        static void PrintDebugCSV()
        {
            using (StreamWriter sw = File.CreateText($"tools/map testing/debug.csv"))
            {
                for (int i = 0; i < MAP_HEIGHT; i++)
                {
                    for (int j = 0; j < MAP_WIDTH; j++)
                    {
                        if (Map.Data[i, j] != null)
                        {
                            sw.Write($"{DebugGetChar(Map.Data[i,j].MapConnections)},");
                        }
                        else if (CurrentArea.OpenEnds.ContainsKey(new Pair(i, j)))
                        {
                            sw.Write($"O,");
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
