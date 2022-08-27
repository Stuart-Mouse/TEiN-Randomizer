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
        public static void CopyToMapAtCoords(GameMap source, GameMap dest, Pair coords)
        {
            // Iterate over source map, pasting into destination map
            for (int i = 0; i < source.Height; i++)
            {
                for (int j = 0; j < source.Width; j++)
                {
                    // Copy map data to new map at offset
                    dest.Data[i + coords.I, j + coords.J] = source.Data[i, j];
                }
            }
        }
        public static GameMap CropMap(int height, int width, GameMap old, int offsetY, int offsetX)
        {
            // This function will copy an existing map's data to a new map.
            // The origin data can be given an offset, and the width and height will determine the size of the new map
            // The purpose of this is to be able to crop a map and place the data into a smaller map
            
            // Create new GameMap to return 
            GameMap ret = new GameMap(height, width);

            // Iterate over old map
            for (int i = 0; i < ret.Height; i++)
            {
                for (int j = 0; j < ret.Width; j++)
                {
                    // Copy map data to new map at offset
                    ret.Data[i, j] = old.Data[i + offsetY, j + offsetX];
                }
            }

            // Return the newly cropped map
            return ret;
        }
    }
}
