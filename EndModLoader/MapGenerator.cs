using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TEiNRandomizer.Properties;

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
        static List<Level> Levels;

        // The map created by the map generator
        static List<List<Level>> Map;

        // The list of open ends is used for determining where to place the next level on the map
        static List<Pair> OpenEnds;

        // not currently in use, may be used to determine where to to place secrets/collectables
        static List<Pair> DeadEnds;


        static void LoadTestLevels()
        {
            
        }
        static string[][] MapToArray()
        {
            string[][] arr = new string[Map.Count][];

            for (int i = 0; i < Map.Count; i++)
            {
                arr[i] = new string[Map[i].Count];
                for (int j = 0; j < Map[i].Count; j++)
                {
                    arr[i][j] = Map[i][j].fileName;
                }
            }

            return arr;
        }
        static string[][] GenerateMap()
        {
            // initialize map
            Map = new List<List<Level>>(300);
            for (int i = 0; i < Map.Count; i++)
            {
                Map[i] = new List<Level>(300);
            }

            // place first screen to start from
            var screen = new Level() { connections = Connections.left & Connections.right };
            PlaceScreen(150, 150, screen);

            // start subroutine of placing screens
            for (int i = 0; i < Levels.Count; i++)
            {
                PlaceNext();
            }

            // convert the map to a 2d array
            string[][] arr = MapToArray();

            return arr;
        }
        static void PlaceNext(bool do_remove = true)
        {
            // This function will draw a new map screen from the list of
            // available levels and add it to the map at the position of an open end.
            // If the do_remove parameter is true, then the selected screen
            // will be removed from the 
            
            // get next open end from list
            int next   = RNG.random.Next(0, OpenEnds.Count);
            var coords = OpenEnds[next];

            // get requirements
            Connections reqs, nots;
            CheckNeighbors(coords.First, coords.Second, out reqs, out nots);

            // get options from pool of screens
            var options = GetOptions(reqs, nots);

            // pick which screen to place
            int choice = RNG.random.Next(0, options.Count);
            
            // get the chosen screen from the list of options
            Level level = options[choice];

            // place screen
            PlaceScreen(coords.First, coords.Second, level);

            // check for open ends after placement
            AddEnds(coords.First, coords.Second, level);

            // remove the newly placed screen from the list of screens available
            //RemoveFromScreens(choice);
        }
        static List<Level> GetOptions(Connections reqs, Connections nots)
        {
            List<Level> options = new List<Level>();
            foreach (Level level in Levels)
            {
                if ((level.connections & reqs) == reqs)
                    if ((level.connections & nots) == Connections.empty)
                        options.Add(level);
            }
            return options;
        }
        static void CheckNeighbors(int i, int j, out Connections reqs, out Connections nots)  // returns the type necessary to meet needs of neighbors
        {
            // initialize rqs and nots to empty
            reqs = Connections.empty;
            nots = Connections.empty;

            // map is row major, so i is the row and j is the column
            // get required entrances, and spots where entrances cant be

            // check screen above
            if (i-1 < Map.Count && j < Map[i].Count)    // check that we wont exceed list bounds
            {
                if (Map[i - 1][j] != null)  // if the screen is null, it won't impose any requirements
                {
                    if (Map[i - 1][j].connections.HasFlag(Connections.down))    // check the screen for downwards connection
                        reqs |= Connections.up;     // add connection to requirements
                    else
                        nots |= Connections.down;   // add lack of connection to negative requirements
                }
            }

            // check screen below
            if (i+1 < Map.Count && j < Map[i].Count)
            {
                if (Map[i + 1][j] != null)
                {
                    if (Map[i + 1][j].connections.HasFlag(Connections.up))
                        reqs |= Connections.down;
                    else
                        nots |= Connections.down;
                }
            }

            // check screen left
            if (i < Map.Count && j-1 < Map[i].Count)
            {
                if (Map[i][j - 1] != null)
                {
                    if (Map[i][j - 1].connections.HasFlag(Connections.left))
                        reqs |= Connections.right;
                    else
                        nots |= Connections.right;
                }
            }

            // check screen right
            if (i < Map.Count && j+1 < Map[i].Count)
            {
                if (Map[i][j + 1] != null)
                {
                    if (Map[i][j + 1].connections.HasFlag(Connections.right))
                        reqs |= Connections.left;
                    else
                        nots |= Connections.left;
                }
            }
        }
        static void PlaceScreen(int i, int j, Level level)
        {
            // place the screen to the map
            Map[i][j] = level;
        }
        static void AddEnds(int i, int j, Level level)
        {
            var con = level.connections;

            // if screen has entrances leading to empty screens, add those screens as open ends
            if (con.HasFlag(Connections.down))
                if (Map[i-1][j] == null)
                    OpenEnds.Add(new Pair(i - 1, j));
            if (con.HasFlag(Connections.up))
                if (Map[i+1][j] == null)
                    OpenEnds.Add(new Pair(i + 1, j));
            if (con.HasFlag(Connections.left))
                if (Map[i][j-1] == null)
                    OpenEnds.Add(new Pair(i, j + 1));
            if (con.HasFlag(Connections.right))
                if (Map[i][j+1] == null)
                    OpenEnds.Add(new Pair(i, j + 1));
        }
    }
}
