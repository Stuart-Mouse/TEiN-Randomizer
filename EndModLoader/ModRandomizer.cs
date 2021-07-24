using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public static partial class Randomizer
    {
        public static void RandomizeMod(MainWindow mw)
        {
            //ShadersList = mw.ShadersList;
            settings = mw.RSettings;
            mainWindow = mw;

            // level corruptions
            string dir = $"{settings.GameDirectory}tilemaps";
            if (Directory.Exists(dir))
            {
                string[] paths = Directory.GetFiles(dir);
                foreach (var file in paths)
                {
                    LevelFile level = LevelManip.Load(file);
                    if (settings.DoCorruptions)
                        Corruptors.CorruptLevel(ref level);
                    LevelManip.Save(level, file);
                }
            }

            if (settings.DoPalettes)
                File.Copy("data/palette.png", $"{settings.GameDirectory}textures/palette.png", true);


            // data folder
            dir = $"{settings.GameDirectory}data";
            if (Directory.Exists(dir))
            {
                // tilesets.txt
                var file = $"{dir}/tilesets.txt";
                if (File.Exists(file))
                {
                    string[] text = File.ReadAllLines(file);
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (text[i].Contains("palette")) text[i] = $"palette {TilesetManip.GetPalette()}";
                    }
                }
            }




        }
    }
}
