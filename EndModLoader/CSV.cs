using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Security;
using System.Security.Cryptography;

namespace TEiNRandomizer
{
    class CSV
    {
        //public string[][] file;
        
        static void FlipCSV(string path)
        {
            var arr = File.ReadAllLines(path);
            int length = 0;
            var file = new List<string[]>();
            foreach (var str in arr)
            {
                var line = str.Split(Convert.ToChar(","));
                if (line.Length > length)
                    length = line.Length;
                line = line.Reverse().ToArray();
                file.Add(line);
            }
            var newfile = new string[arr.Length];
            for (int j = 0; j < file.Count; j++)
            {
                string newline = "";
                int offset = length - file[j].Length;
                for (int i = 0; i < offset; i++)
                {
                    newline += ",";
                }
                for (int i = 0; i < file[j].Length; i++)
                {
                    newline += file[j][i] + ",";
                }
                newfile[j] = newline;
            }
            File.WriteAllLines("flip.csv", newfile);
        }

        public static void RotateCSV(string path)
        {
            var arr = File.ReadAllLines(path);
            int length = 0;
            int height = 0;
            var file = new List<string[]>();
            foreach (var str in arr)
            {
                var line = str.Split(Convert.ToChar(","));
                if (line.Length > length)
                    length = line.Length;
                line = line.Reverse().ToArray();
                file.Add(line);
            }
            var oldFile = file.ToArray();
            height = oldFile.Length;
            string[] newFile = new string[length];
            string newLine;
            for (int row = 0; row < height; row++)
            {
                var temp = file[row];
                Array.Resize(ref temp, length);
                file[row] = temp;
            }
            for (int col = 0; col < length; col++)
            {
                newLine = "";
                for (int row = 0; row < height; row++)
                {
                    newLine += file[row][col] + ",";
                }
                newFile[col] = newLine;
            }
            
            File.WriteAllLines("rotate.csv", newFile);
        }

        public static void LevelToCSV(ref LevelFile level, string path)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            //File.Create(path);

            TileID temp;
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int row = 0; row < lh; row++)
                {
                    for (int col = 0; col < lw; col++)
                    {
                        int index = row * lw + col;
                        temp = level.data.back1[index];
                        sw.Write((int)temp + ",");
                    }
                    sw.Write("\n");
                }

                sw.Write("\n\n\n\n");

                for (int row = 0; row < lh; row++)
                {
                    for (int col = 0; col < lw; col++)
                    {
                        int index = row * lw + col;
                        temp = level.data.back2[index];
                        sw.Write((int)temp + ",");
                    }
                    sw.Write("\n");
                }
            }

            //StreamWriter sw = File.AppendText(path);

            //sw.WriteLine("\n\nBack1\n");
            //LevelLayerToCSV(ref level.data.back1, path, lw, lh);
            //sw.WriteLine("\n\nActive\n");
            //LevelLayerToCSV(ref level.data.active, path, lw, lh);
            //sw.WriteLine("\n\nTag\n");
            //LevelLayerToCSV(ref level.data.tag, path, lw, lh);
            //sw.WriteLine("\n\nOverlay\n");
            //LevelLayerToCSV(ref level.data.overlay, path, lw, lh);
            //sw.WriteLine("\n\nBack2\n");
            //LevelLayerToCSV(ref level.data.back2, path, lw, lh);
            
        }

        static void LevelLayerToCSV(ref TileID[] layer, string path, Int32 lw, Int32 lh)
        {
            TileID temp;
            using (StreamWriter sw = File.AppendText(path))
            {
                for (int row = 0; row < lh; row++)
                {
                    for (int col = 0; col < lw; col++)
                    {
                        int index = row * lw + col;

                        temp = layer[index];

                        sw.WriteLine(nameof(temp) + ",");

                    }
                    sw.WriteLine("\n");
                }
            }
        }

    }
}
