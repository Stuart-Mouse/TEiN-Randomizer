using System;
using System.IO;

namespace TEiNRandomizer
{
    public static partial class Randomizer
    {
        public static void PrepModFolders()
        {
            string dir;
            // set up palette and shaders
            if (Settings.DoPalettes)
            {
                //set up palette
                dir = $"{saveDir}textures";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy("data/palette.png", $"{saveDir}textures/palette.png", true);

                // set up shaders
                dir = $"{saveDir}shaders";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                foreach (var file in Directory.GetFiles("data/shaders"))
                {
                    string dest = dir + "/" + Path.GetFileName(file);
                    if (!File.Exists(dest))
                        File.Copy(file, dest);
                }
            }

            if (Settings.DoParticles)
            {
                dir = saveDir + "data/particles.txt.append";
                if (!File.Exists(dir))
                    File.Create(dir);
            }
        }
        public static void RandomizeMod(MainWindow mw)
        {
            //ShadersList = mw.ShadersList;

            saveDir = Settings.GameDirectory;

            PrepModFolders();

            // level corruptions
            string dir = $"{saveDir}tilemaps";
            if (Directory.Exists(dir))
            {
                string[] paths = Directory.GetFiles(dir);
                foreach (var file in paths)
                {
                    LevelFile level = LevelManip.Load(file);
                    //if(settings.MirrorMode)
                    //{
                    //    LevelManip.FlipLevelH(ref level);
                    //    FlipCSV(saveDir + "data/map.csv");
                    //}
                    if (Settings.DoCorruptions)
                        LevelCorruptors.CorruptLevel(ref level);
                    LevelManip.Save(level, file);
                }
            }

            // data folder
            dir = $"{saveDir}data";
            if (Directory.Exists(dir))
            {
                // tilesets.txt
                var file = $"{dir}/tilesets.txt";
                if (File.Exists(file))
                {
                    string[] text = File.ReadAllLines(file);
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (text[i].Contains("palette"))
                            text[i] = TilesetManip.GetPalette().ToString();
                        if (text[i].Contains("tile_graphics"))
                            text[i] = TilesetManip.GetTile();
                        if (text[i].Contains("overlay_graphics"))
                            text[i] = TilesetManip.GetOverlay();
                        if (text[i].Contains("global_particle"))
                        {
                            var split = text[i].Trim().Split(Convert.ToChar(" "));
                            text[i] = split[0] + " " + ParticleGenerator.GetParticle();
                        }
                    }
                    File.Delete(file);
                    File.WriteAllLines(file, text);
                }


            }




        }
    }
}
