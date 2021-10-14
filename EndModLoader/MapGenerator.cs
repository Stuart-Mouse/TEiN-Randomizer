using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    /*public class MapScreen
    {
        public Connections connections;
        public Level level;
        //public MapArea parentArea;
        //public int levelNumber;
        public string levelName;
    }*/
    public class MapArea
    {
        public string areaName;
        public int numScreens;
        public Tileset tileset;

        public MapArea()
        {
            areaName = Randomizer.GetFunnyName();
            numScreens = 0;
            tileset = new Tileset();
        }
    }
    public static class MapGenerator
    {
        // The list of levels available to the level generator
        // This may be passed in the main randomize() function later
        public static List<Level> Levels;

        // These levels are reusable connectors that use
        // color tiles and corruptors to reduce repetition.
        public static List<Level> Connectors;

        // These levels are placed at dead ends.
        // They cannot be reused.
        public static List<Level> Secrets;

        // Hub levels are placed at the beginning of generation.
        // They are always placed in the same configuration.
        public static List<Level> HubLevels;

        // These levels are placed at the edges of the map.
        // They can be reused if necessary.
        public static List<Level> WorldEdges;

        // The dimensions of the map.
        // These need to be stored so that map bounds
        // can be check when placing levels.
        const int MAP_WIDTH  = 50;
        const int MAP_HEIGHT = 50;

        // The map created by the map generator
        static Level[,] Map;

        // The list of open ends is used for determining where to place the next level on the map
        static HashSet<Pair> OpenEnds = new HashSet<Pair>();
        static HashSet<Pair> SecretEnds = new HashSet<Pair>();

        // not currently in use, may be used to determine where to to place secrets/collectables
        //static List<Pair> DeadEnds;

        public static Level[,] GenerateMap()
        {
            // load levels to internal list
            LevelPool pool = LevelPool.LoadPoolMapTest();
            Levels = pool.Levels;

            // initialize map
            Map = new Level[MAP_HEIGHT, MAP_WIDTH];

            // place first screen to start from
            var startLevel = Utility.FindLevelInListByName(HubLevels, "start");
            PlaceLevel(MAP_HEIGHT/2, MAP_WIDTH / 2, startLevel);
            AddEnds(MAP_HEIGHT / 2, MAP_WIDTH / 2, startLevel);
            Levels.Remove(startLevel);

            // start subroutine of placing screens
            while (OpenEnds.Count != 0)
            {
                /*if (OpenEnds.Count != 0)
                    break;*/
                PlaceNext();
                PrintDebugCSV();
            }

            // convert the map to a 2d array
            //string[][] arr = MapToArray();

            PrintCSV();

            return Map;
        }
        static void PlaceNext(bool do_remove = true)
        {
            // This function will draw a new map screen from the list of
            // available levels and add it to the map at the position of an open end.
            // If the do_remove parameter is true, then the selected screen
            // will be removed from the level list
            
            // get next open end from list
            int end   = RNG.random.Next(0, OpenEnds.Count);
            var coords = OpenEnds.ElementAt(end);

            // get requirements
            CheckNeighbors(coords.First, coords.Second, out Connections reqs, out Connections nots);

            // get options from pool of screens
            var options = GetOptions(reqs, nots);

            if (options.Count == 0)
            {
                Console.WriteLine($"Unable to place level at {coords.First}, {coords.Second}");
                Console.WriteLine($"\treqs: {reqs}");
                Console.WriteLine($"\tnots: {nots}");
                return;
            }

            // pick which screen to place
            int choice = RNG.random.Next(0, options.Count);
            
            // get the chosen screen from the list of options
            Level level = options[choice];

            // place screen
            if (PlaceLevel(coords.First, coords.Second, level))
            {
                OpenEnds.Remove(new Pair(coords.First, coords.Second));
            }
            else Console.WriteLine("Error placing level");

            // check for open ends after placement
            AddEnds(coords.First, coords.Second, level);

            // remove the newly placed screen from the list of screens available
            if (do_remove)
                Levels.Remove(level);
        }
        static List<Level> GetOptions(Connections reqs, Connections nots)
        {
            List<Level> options = new List<Level>();
            foreach (Level level in Levels)
            {
                if ((level.MapConnections & reqs) == reqs)
                    if ((level.MapConnections & nots) == Connections.empty)
                        options.Add(level);
            }
            return options;
        }
        static bool MapBoundsCheck(int i, int j)
        {
            if (i >= 0 && j >= 0 && i < MAP_HEIGHT && j < MAP_WIDTH)
                return true;
            return false;
        }

        static void CheckNeighbors(int i, int j, out Connections reqs, out Connections nots)  // returns the type necessary to meet needs of neighbors
        {
            // initialize reqs and nots to empty
            reqs = Connections.empty;
            nots = Connections.empty;

            // map is row major, so i is the row and j is the column
            // get required entrances, and spots where entrances cant be

            // check screen above
            if (MapBoundsCheck(i-1, j))    // check that we wont exceed list bounds
            {
                if (Map[i - 1,j] != null)  // if the screen is null, it won't impose any requirements
                {
                    if (Map[i - 1,j].MapConnections.HasFlag(Connections.down))    // check the screen for downwards connection
                        reqs |= Connections.up;     // add connection to requirements
                    else nots |= Connections.up;   // add lack of connection to negative requirements
                }
            }
            else nots |= Connections.up;   // add lack of connection to negative requirements

            // check screen below
            if (MapBoundsCheck(i+1, j))
            {
                if (Map[i + 1,j] != null)
                {
                    if (Map[i + 1,j].MapConnections.HasFlag(Connections.up))
                        reqs |= Connections.down;
                    else nots |= Connections.down;
                }
            }
            else nots |= Connections.down;


            // check screen left
            if (MapBoundsCheck(i, j-1))
            {
                if (Map[i,j - 1] != null)
                {
                    if (Map[i,j - 1].MapConnections.HasFlag(Connections.right))
                        reqs |= Connections.left;
                    else nots |= Connections.left;
                }
            }
            else nots |= Connections.left;


            // check screen right
            if (MapBoundsCheck(i, j+1))
            {
                if (Map[i,j + 1] != null)
                {
                    if (Map[i,j + 1].MapConnections.HasFlag(Connections.left))
                        reqs |= Connections.right;
                    else nots |= Connections.right;
                }
            }
            else nots |= Connections.right;

        }
        static bool PlaceLevel(int i, int j, Level level)
        {
            // place the screen to the map
            if (MapBoundsCheck(i,j))
            {
                if (Map[i, j] != null) Console.WriteLine($"Overwrote a cell at {i}, {j}");
                Map[i,j] = level;
                return true;
            }
            return false;
        }
        static void AddEnds(int i, int j, Level level)
        {
            var con = level.MapConnections;

            // if screen has entrances leading to empty screens, add those screens as open ends
            if (con.HasFlag(Connections.up))
                if (Map[i-1,j] == null)
                    OpenEnds.Add(new Pair(i - 1, j));
            if (con.HasFlag(Connections.down))
                if (Map[i+1,j] == null)
                    OpenEnds.Add(new Pair(i + 1, j));
            if (con.HasFlag(Connections.left))
                if (Map[i,j-1] == null)
                    OpenEnds.Add(new Pair(i, j - 1));
            if (con.HasFlag(Connections.right))
                if (Map[i,j+1] == null)
                    OpenEnds.Add(new Pair(i, j + 1));
        }
        static void PrintCSV()
        {
            using (StreamWriter sw = File.CreateText("C:\\Program Files (x86)\\Steam\\steamapps\\common\\theendisnigh\\data\\map.csv"))
            {
                for (int i = 0; i < MAP_HEIGHT; i++)
                {
                    for (int j = 0; j < MAP_WIDTH; j++)
                    {
                        if (Map[i, j] != null)
                        {
                            string name = RNG.GetUInt32().ToString();

                            if (Map[i, j].Name == "start") name = "start";

                            var file = LevelManip.Load($"tools/map testing/maptest/{Map[i, j].Name}.lvl");
                            LevelManip.Save(file, $"C:\\Program Files (x86)\\Steam\\steamapps\\common\\theendisnigh\\tilemaps\\{name}.lvl");

                            sw.Write($"{name}.lvl,");
                        }
                        else sw.Write("a.lvl,");
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
                        if (Map[i, j] != null)
                        {
                            if (Map[i, j].Name == "start")
                                sw.Write($"{Map[i, j].Name},");
                            else
                            {
                                sw.Write($"{DebugGetChar(Map[i,j].MapConnections)},");
                            }
                        }
                        else if (OpenEnds.Contains(new Pair(i,j)))
                        {
                            sw.Write($"OE,");
                        }
                        else sw.Write(" ,");
                    }
                    sw.Write('\n');
                }
            }
        }
        static char DebugGetChar(Connections con)
        {
            switch (con)
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
