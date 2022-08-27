using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    public static partial class LevelManip
    {
        public static List<Pair> HFlipIndex;
        public static List<Pair> VFlipIndex;
        public static List<Pair> RotationIndex;

        static LevelManip()
        {
            // load flips dictionary in constructor
            var gon = GonObject.Load("data/text/tile_flips.gon");
            Pair pair;

            // load horizontal flips
            var child = gon["horz"];
            HFlipIndex = new List<Pair>(child.Size());
            for (int i = 0; i < child.Size(); i++)
            {
                // We can safely index both the first and second item directly since we know they will always be present
                pair.I = child[i][0].Int();
                pair.J = child[i][1].Int();
                HFlipIndex.Add(pair);
            }

            // load vertical flips
            child = gon["vert"];
            VFlipIndex = new List<Pair>(child.Size());
            for (int i = 0; i < child.Size(); i++)
            {
                pair.I = child[i][0].Int();
                pair.J = child[i][1].Int();
                VFlipIndex.Add(pair);
            }

            // load rotations
            gon = GonObject.Load("data/text/tile_rotations.gon");
            child = gon["rotate"];
            RotationIndex = new List<Pair>(child.Size());
            for (int i = 0; i < child.Size(); i++)
            {
                pair.I = child[i][0].Int();
                pair.J = child[i][1].Int();
                RotationIndex.Add(pair);
            }

            // load level corruptors stuff
            gon = GonObject.Load($"data/text/corruptor_tiles.gon");
            ActiveTiles  = gon["active" ].ToIntArray();
            EntityTiles  = gon["entity" ].ToIntArray();
            OverlayTiles = gon["overlay"].ToIntArray();
            SmartTiles = LoadDictionary(gon["smart"]);
            ColorTiles = LoadDictionary(gon["color"]);
        }
        public static LevelFile Load(string path)
        {
            LevelFile level = new LevelFile() { };
            byte[] filedata = File.ReadAllBytes(path);

            // load level header
            byte[] tempBytes = new byte[4];

            Buffer.BlockCopy(filedata, 0, tempBytes, 0, 4);
            Array.Reverse(tempBytes);
            level.header.version = BitConverter.ToInt32(tempBytes, 0);
            Buffer.BlockCopy(filedata, 4, tempBytes, 0, 4);
            Array.Reverse(tempBytes);
            level.header.width = BitConverter.ToInt32(tempBytes, 0);
            Buffer.BlockCopy(filedata, 8, tempBytes, 0, 4);
            Array.Reverse(tempBytes);
            level.header.height = BitConverter.ToInt32(tempBytes, 0);
            Buffer.BlockCopy(filedata, 12, tempBytes, 0, 4);
            Array.Reverse(tempBytes);
            level.header.layers = BitConverter.ToInt32(tempBytes, 0);

            // get data layer length
            int layerLength = level.header.width * level.header.height;

            // initialize data layers
            level.data = new TileID[level.header.layers, layerLength];

            // load data layers
            int offset = 16;
            for (int i = 0; i < level.header.layers; i++) // Iterate over the layers
            {
                tempBytes = new byte[4];
                for (int j = 0; j < layerLength; j++)
                {
                    Buffer.BlockCopy(filedata, offset, tempBytes, 0, 4);
                    Array.Reverse(tempBytes);
                    level.data[i,j] = (TileID)(BitConverter.ToInt32(tempBytes, 0));
                    offset += 4;
                }
            }

            return level;
        }
        public static void Save(LevelFile level, string path)
        {
            // get data layer length
            int layerLength = level.header.width * level.header.height;
            int fileLength = layerLength * 5 * 4 + 16;
            //Console.WriteLine("file length  (bytes): " + fileLength);
            //Console.WriteLine("layer length (bytes): " + layerLength);

            byte[] filedata = new byte[fileLength];
            byte[] tempBytes = new byte[4];
            tempBytes = BitConverter.GetBytes(level.header.version);
            Array.Reverse(tempBytes);
            Buffer.BlockCopy(tempBytes, 0, filedata, 0, 4);
            tempBytes = BitConverter.GetBytes(level.header.width);
            Array.Reverse(tempBytes);
            Buffer.BlockCopy(tempBytes, 0, filedata, 4, 4);
            tempBytes = BitConverter.GetBytes(level.header.height);
            Array.Reverse(tempBytes);
            Buffer.BlockCopy(tempBytes, 0, filedata, 8, 4);
            tempBytes = BitConverter.GetBytes(level.header.layers);
            Array.Reverse(tempBytes);
            Buffer.BlockCopy(tempBytes, 0, filedata, 12, 4);

            // save data layers
            int offset = 16;
            for (int i = 0; i < level.header.layers; i++)
            {
                tempBytes = new byte[4];
                for (int j = 0; j < layerLength; j ++)
                {
                    tempBytes = BitConverter.GetBytes((Int32)level.data[i,j]);
                    Array.Reverse(tempBytes);
                    Buffer.BlockCopy(tempBytes, 0, filedata, offset, 4);
                    offset += 4;
                }
            }

            string folder = Path.GetDirectoryName(path);
            if (!File.Exists(folder))
                Directory.CreateDirectory(folder);
            File.WriteAllBytes(path, filedata);     // output copied file

        }

        // Basic Manipulations ( Flip / Rotation / Resize )
        public static void FlipLevelH(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            TileID temp;

            for (int layer = 0; layer < level.header.layers; layer++)   // iterate over layers
            {
                for (int row = 0; row < lh; row++)                      // iterate over rows
                {
                    for (int col = 0; col < lw; col++)                  // iterate over columns
                    {
                        int index = row * lw + col;                     // get tile index
                        int toSwap = (row+1) * lw - col - 1;            // get swap index

                        if (index >= toSwap) break; // Only iterate over half of level
                        
                        if (   level.data[layer, index ] != TileID.Background 
                            && level.data[layer, toSwap] != TileID.Background 
                            && level.data[layer, index ] != TileID.Foreground 
                            && level.data[layer, toSwap] != TileID.Foreground) // Don't flip BG and FG alignment tiles
                        {
                            // Flip the TileIDs in place
                            FlipTileH(ref level.data[layer, index ]);
                            FlipTileH(ref level.data[layer, toSwap]);

                            // Swap the locations of the now flipped tiles
                            temp                      = level.data[layer, index];
                            level.data[layer, index]  = level.data[layer, toSwap];
                            level.data[layer, toSwap] = temp;
                        }
                    }
                }
            }

            void FlipTileH(ref TileID id)
            {
                foreach (var pair in HFlipIndex)
                {
                    if (id == (TileID)pair.I)
                        id = (TileID)pair.J;
                    else if (id == (TileID)pair.J)
                        id = (TileID)pair.I;
                }
            }
        }
        public static LevelFile RotateLevel(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            LevelFile levelNew = new LevelFile(lh, lw);

            for (int row = 0; row < lh; row++)
            {
                for (int col = 0; col < lw; col++)
                {
                    int copyIndex = row * lw + col;
                    int pasteIndex = col * lh + (lh - row - 1);

                    for (int layer = 0; layer < levelNew.header.layers; layer++)
                        levelNew.data[layer, pasteIndex] = GetRotation(level.data[layer, copyIndex]);
                }
            }

            return levelNew;

            TileID GetRotation(TileID id)
            {
                foreach (var pair in RotationIndex)
                {
                    if (id == (TileID)pair.I)
                        return (TileID)pair.J;
                }
                return id;
            }
        }
        
        public static LevelFile FixAspect(ref LevelFile level_in)
        {
            // This function resizes the level to a 16:9 aspect ratio.
            int lh = level_in.header.height;
            int lw = level_in.header.width;

            if (lh * 16 > lw * 9)
                lw = lh * 16 / 9;
            else lh = lw * 9 / 16;

            LevelFile level_out = new LevelFile(lw, lh);
            int hOffset = lh - level_in.header.width;

            LevelManip.CopyToCoords(ref level_in, ref level_out, new Pair(0, hOffset));

            return level_out;
        }

        // Search Functions ( Find / Replace )
        public static int GetTileCount(ref LevelFile level, int layer, TileID id)
        {
            int lsize = level.header.width * level.header.height;

            int count = 0;

            for (int i = 0; i < lsize; i++)
                if (level.data[layer, i] == id) count++;

            return count;
        }
        public static void ReplaceTilesByID(ref LevelFile level, int layer, TileID id, TileID rep)
        {
            // Replaces all tiles of a specified tileID on a specified layer
            int lsize = level.header.width * level.header.height;

            for (int i = 0; i < lsize; i++)
                if (level.data[layer, i] == id) level.data[layer, i] = rep;
        }
        public static List<int> FindTilesByID(ref LevelFile level, int layer, TileID id, int startIndex = 0, int step = 1)
        {
            // Finds all tiles of a specified tileID on a specified layer and returns their indices as a list of integers
            int lsize = level.header.width * level.header.height;

            List<int> list = new List<int>();

            for (int i = startIndex; i < lsize; i += step)
                if (level.data[layer, i] == id) list.Add(i);

            return list;
        }
        public static int FindFirstTileByID(ref LevelFile level, int layer, TileID id)
        {
            int lsize = level.header.width * level.header.height;

            for (int i = 0; i < lsize; i++)
                if (level.data[layer, i] == id) return i;

            return -1;  // return -1 if none found
        }

        // Copy & Paste Operations
        public static void CopyLevelClip(ref LevelFile copyLevel, ref LevelFile pasteLevel, Pair coords, Pair size)    // pass in the origin coords of the level to be copied from
        {
            // This function will copy a specified portion of the copylevel into the pastelevel at the origin
            int copyIndex = 0;
            int pasteIndex = 0;

            int copylw = copyLevel.header.width;
            int copylh = copyLevel.header.height;

            int pastelw = pasteLevel.header.width;
            int pastelh = pasteLevel.header.height;

            for (int i = 0; i < size.I; i++)
            {
                for (int j = 0; j < size.J; j++)
                {
                    pasteIndex =  i                 * pastelw +  j                 ;
                    copyIndex  = (i + coords.I) * copylw  + (j + coords.J);

                    for (int layer = 0; layer < pasteLevel.header.layers; layer++)
                        pasteLevel.data[layer, pasteIndex]  = copyLevel.data[layer, copyIndex];
                }
            }
        }

    }
}
