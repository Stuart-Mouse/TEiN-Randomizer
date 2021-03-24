﻿using System;
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

        static LevelManip()
        {
            // load flips dictionary in constructor
            HFlipIndex = new List<Pair> { };
            var hflips = File.ReadAllLines("data/hflips.txt");
            Pair pair;
            string[] temp;
            foreach (var line in hflips)
            {
                temp = line.Split(Convert.ToChar(" "));
                pair.First = Convert.ToInt32(temp[0]);
                pair.Second = Convert.ToInt32(temp[1]);
                HFlipIndex.Add(pair);
            }
        }

        public static void LoadLayer(ref byte[] filedata, ref Int32[] layer, ref int offset)
        {
            try
            {
                byte[] tempBytes = new byte[4];
                for (int i = 0; i < layer.Length; i++)
                {
                    Buffer.BlockCopy(filedata, offset, tempBytes, 0, 4);
                    Array.Reverse(tempBytes);
                    layer[i] = (BitConverter.ToInt32(tempBytes, 0));
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

        public static void SaveLayer(ref byte[] filedata, ref Int32[] layer, ref int offset)
        {
            byte[] tempBytes = new byte[4];
            for (int i = 0; i < layer.Length; i++)
            {
                tempBytes = BitConverter.GetBytes(layer[i]);
                Array.Reverse(tempBytes);
                Buffer.BlockCopy(tempBytes, 0, filedata, offset, 4);
                offset += 4;
            }
        }

        public static LevelFile Load(string path)
        {
            Console.WriteLine("load");

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
            Console.WriteLine("file length  (bytes): " + filedata.Length);
            Console.WriteLine("layer length (bytes): " + layerLength);

            // initialize data layers
            level.data.back1 = new Int32[layerLength];
            level.data.active = new Int32[layerLength];
            level.data.tag = new Int32[layerLength];
            level.data.overlay = new Int32[layerLength];
            level.data.back2 = new Int32[layerLength];

            // load data layers
            int offset = 16;
            LoadLayer(ref filedata, ref level.data.back1, ref offset);
            LoadLayer(ref filedata, ref level.data.active, ref offset);
            LoadLayer(ref filedata, ref level.data.tag, ref offset);
            LoadLayer(ref filedata, ref level.data.overlay, ref offset);
            LoadLayer(ref filedata, ref level.data.back2, ref offset);

            File.WriteAllBytes("output_original.lvl", filedata);    // output original file

            return level;
        }

        public static void Save(LevelFile level, string path)
        {
            Console.WriteLine("save");

            // get data layer length
            int layerLength = level.header.width * level.header.height;
            int fileLength = layerLength * 5 * 4 + 16;
            Console.WriteLine("file length  (bytes): " + fileLength);
            Console.WriteLine("layer length (bytes): " + layerLength);

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

        public static void FlipLayerH(ref Int32[] layer, Int32 lw, Int32 lh)
        {
            int temp;
            for (int row = 0; row < lh; row++)
            {
                for (int col = 0; col < lw; col++)
                {
                    int index = row * lw + col;
                    int toSwap = row * lw + lw - col - 1;

                    if (index < toSwap)
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

        public static Int32 GetFlipH(ref Int32 id)
        {
            foreach (var pair in HFlipIndex)
            {
                if (id == pair.First)
                    id = pair.Second;
                else if (id == pair.Second)
                    id = pair.First;
            }
            return id;
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
                    if (level.data.tag[index] == 20070)
                        Console.Write("*");
                    else if (level.data.overlay[index] == 30009)
                        Console.Write("■");
                    else if (level.data.active[index] == 1)
                        Console.Write("▓");
                    else if (level.data.back1[index] == 50000)
                        Console.Write("▒");
                    else if (level.data.back2[index] == 50033)
                        Console.Write("░");
                    else Console.Write(" ");

                }
                Console.Write("\n");
            }
            Console.WriteLine("============================================");
        }

    }
}