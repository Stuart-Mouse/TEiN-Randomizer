using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    public static class LevelManip
    {
        public static List<Pair> HFlipIndex;
        public static List<Pair> VFlipIndex;
        public static List<Pair> RotationIndex;

        static LevelManip()
        {
            // load flips dictionary in constructor
            HFlipIndex = new List<Pair> { };
            VFlipIndex = new List<Pair> { };
            var gon = GonObject.Load("data/text/tile_flips.gon");
            Pair pair;

            // load horizontal flips
            var child = gon["horz"];
            for (int i = 0; i < child.Size(); i++)
            {
                // We can safely index both the first and second item directly since we know they will always be present
                pair.First  = child[i][0].Int();
                pair.Second = child[i][1].Int();
                HFlipIndex.Add(pair);
            }

            // load vertical flips
            child = gon["vert"];
            for (int i = 0; i < child.Size(); i++)
            {
                pair.First  = child[i][0].Int();
                pair.Second = child[i][1].Int();
                VFlipIndex.Add(pair);
            }

            // load rotations
            gon = GonObject.Load("data/text/tile_rotations.gon");
            RotationIndex = new List<Pair> { };
            child = gon["rotate"];
            for (int i = 0; i < child.Size(); i++)
            {
                pair.First  = child[i][0].Int();
                pair.Second = child[i][1].Int();
                RotationIndex.Add(pair);
            }
        }

        static void LoadLayer(ref byte[] filedata, ref TileID[] layer, ref int offset)
        {
            try
            {
                byte[] tempBytes = new byte[4];
                for (int i = 0; i < layer.Length; i++)
                {
                    Buffer.BlockCopy(filedata, offset, tempBytes, 0, 4);
                    Array.Reverse(tempBytes);
                    layer[i] = (TileID)(BitConverter.ToInt32(tempBytes, 0));
                    offset += 4;
                }
            }
            catch (Exception)
            {
                for (int i = 0; i < layer.Length; i++)
                {
                    layer[i] = 0;
                    offset += 4;
                }
            }
        }

        static void SaveLayer(ref byte[] filedata, ref TileID[] layer, ref int offset)
        {
            byte[] tempBytes = new byte[4];
            for (int i = 0; i < layer.Length; i++)
            {
                tempBytes = BitConverter.GetBytes((Int32)layer[i]);
                Array.Reverse(tempBytes);
                Buffer.BlockCopy(tempBytes, 0, filedata, offset, 4);
                offset += 4;
            }
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
            //Console.WriteLine("file length  (bytes): " + filedata.Length);
            //Console.WriteLine("layer length (bytes): " + layerLength);

            // initialize data layers
            level.data.back1    = new TileID[layerLength];
            level.data.active   = new TileID[layerLength];
            level.data.tag      = new TileID[layerLength];
            level.data.overlay  = new TileID[layerLength];
            level.data.back2    = new TileID[layerLength];

            // load data layers
            int offset = 16;
            LoadLayer(ref filedata, ref level.data.back1, ref offset);
            LoadLayer(ref filedata, ref level.data.active, ref offset);
            LoadLayer(ref filedata, ref level.data.tag, ref offset);
            LoadLayer(ref filedata, ref level.data.overlay, ref offset);
            LoadLayer(ref filedata, ref level.data.back2, ref offset);

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
            SaveLayer(ref filedata, ref level.data.back1, ref offset);
            SaveLayer(ref filedata, ref level.data.active, ref offset);
            SaveLayer(ref filedata, ref level.data.tag, ref offset);
            SaveLayer(ref filedata, ref level.data.overlay, ref offset);
            SaveLayer(ref filedata, ref level.data.back2, ref offset);

            string folder = Path.GetDirectoryName(path);
            if (!File.Exists(folder))
                Directory.CreateDirectory(folder);
            File.WriteAllBytes(path, filedata);     // output copied file

        }

        
        public static void FlipLevelH(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            FlipLayerH(ref level.data.back1, lw, lh);
            FlipLayerH(ref level.data.active, lw, lh);
            FlipLayerH(ref level.data.tag, lw, lh);
            FlipLayerH(ref level.data.overlay, lw, lh);
            FlipLayerH(ref level.data.back2, lw, lh);
        }

        public static void FlipLayerH(ref TileID[] layer, Int32 lw, Int32 lh)
        {
            TileID temp;
            for (int row = 0; row < lh; row++)
            {
                for (int col = 0; col < lw; col++)
                {
                    int index = row * lw + col;
                    int toSwap = row * lw + lw - col - 1;

                    if (index < toSwap)
                    {
                        if (layer[index] != TileID.Background && layer[toSwap] != TileID.Background && layer[index] != TileID.Foreground && layer[toSwap] != TileID.Foreground) // Don't flip BG and FG alignment tiles
                        {
                            // insert tile flip lookup here
                            GetFlipH(ref layer[index]);
                            GetFlipH(ref layer[toSwap]);

                            temp = layer[index];
                            layer[index] = layer[toSwap];
                            layer[toSwap] = temp;
                        }
                    }
                }
            }
        }

        public static void GetFlipH(ref TileID id)
        {
            foreach (var pair in HFlipIndex)
            {
                if (id == (TileID)pair.First)
                    id = (TileID)pair.Second;
                else if (id == (TileID)pair.Second)
                    id = (TileID)pair.First;
            }
            //return id;
        }

        public static void PrintLevelToConsole(LevelFile level)
        {
            var lh = level.header.height;
            var lw = level.header.width;

            Console.WriteLine("============================================");
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    int index = i * lw + j;
                    if (level.data.active[index] == TileID.Solid)
                        Console.Write("▓▓");
                    else if (level.data.back1[index] == TileID.WholePiece)
                        Console.Write("▒▒");
                    else if (level.data.back2[index] == TileID.WholePiece2)
                        Console.Write("░░");
                    else Console.Write("  ");

                }
                Console.Write("\n");
            }
            Console.WriteLine("============================================");
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
                    int pasteIndex = col * lh + (lh-row-1);

                    levelNew.data.active[pasteIndex]  = GetRotation(level.data.active[copyIndex]);
                    levelNew.data.back1[pasteIndex]   = GetRotation(level.data.back1[copyIndex]);
                    levelNew.data.back2[pasteIndex]   = GetRotation(level.data.back2[copyIndex]);
                    levelNew.data.tag[pasteIndex]     = GetRotation(level.data.tag[copyIndex]);
                    levelNew.data.overlay[pasteIndex] = GetRotation(level.data.overlay[copyIndex]);
                }
            }

            return levelNew;
        }

        public static TileID GetRotation(TileID id)
        {
            foreach (var pair in RotationIndex)
            {
                if (id == (TileID)pair.First)
                    return (TileID)pair.Second;
            }
            return id;
        }

        public static LevelFile FixAspect(ref LevelFile levelIn)
        {
            int lh = levelIn.header.height;
            int lw = levelIn.header.width;

            if (lh * 16 > lw * 9)
                lw = lh * 16 / 9;
            else lh = lw * 9 / 16;

            LevelFile levelOut = LevelGenerator.GetNewLevelFile(lw, lh);
            int hOffset = lh - levelIn.header.width;

            LevelGenerator.CopyToCoords(ref levelIn, ref levelOut, new Pair(0, hOffset));

            return levelOut;
        }

        public static void NormalizeEntranceH(ref LevelFile left, ref LevelFile right, bool horiz = false)
        {
            // Find transition tags in left level
            int indexL = FindFirstTag(left, TileID.GreenTransitionR);

            // Throw error if none found
            if (indexL == -1) throw new IndexOutOfRangeException("Could not find given tag in the level.");

            // Get all adjacents
            List<int> adjacentsL = GetEntryTags(ref left, TileID.GreenTransitionR, indexL);

            // Find transition tags in right level
            int indexR = FindFirstTag(right, TileID.GreenTransitionL);

            // Throw error if none found
            if (indexR == -1) throw new IndexOutOfRangeException("Could not find given tag in the level.");

            // Get all adjacents
            List<int> adjacentsR = GetEntryTags(ref right, TileID.GreenTransitionL, indexR);

            // Calculate new entry height
            int newEntryHeight = Math.Min(adjacentsL.Count, adjacentsR.Count);

            // Remove extraneous entry tags in left level
            ReplaceEntryTags(ref left, adjacentsL, newEntryHeight);

            // Remove extraneous entry tags in left level
            ReplaceEntryTags(ref right, adjacentsR, newEntryHeight);
        }

        public static int FindFirstTag(LevelFile level, TileID tag)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            for (int row = 0; row < lh; row++)
            {
                for (int col = 0; col < lw; col++)
                {
                    int index = row * lw + col;

                    if (level.data.tag[index] == tag)
                        return index;
                }
            }
            return -1;
        }

        public static void ReplaceEntryTags(ref LevelFile level, List<int> set, int newEntryHeight)
        {
            // Calculate how many tiles to replace
            int toReplace = set.Count() - newEntryHeight;
            if (toReplace <= 0)
                return;

            // Iterate over set of tiles
            for (int i = 0; i < toReplace; i++)
            {
                int index = set[i];

                // Remove the transition tag
                level.data.tag[index] = TileID.Empty;

                // Place invisible solid where tag was
                level.data.active[index] = TileID.Invisible;
            }
        }

        public static List<int> GetEntryTags(ref LevelFile level, TileID id, int index, bool horiz = false)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            int size = lw * lh;

            List<int> adjacents = new List<int>();

            while (true)
            {
                if (horiz) index += 1;
                else index += lw;

                if (index < size)
                {
                    if (level.data.active[index] == id)
                    {
                        adjacents.Add(index);
                        continue;
                    }
                }
                break;
            }

            return adjacents;
        }
    }
}
