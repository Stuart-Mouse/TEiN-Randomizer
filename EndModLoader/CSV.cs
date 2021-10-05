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
        public string[][] file;
        
        /*void FlipH()
        {
            int length = 0;
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

        void RotateCSV(string path)
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
        }*/
    }
}
