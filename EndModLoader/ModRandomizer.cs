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
                dir = $"{SaveDir}textures";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.Copy("data/palette.png", $"{SaveDir}textures/palette.png", true);

                // set up shaders
                dir = $"{SaveDir}shaders";
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
                dir = SaveDir + "data/particles.txt.append";
                if (!File.Exists(dir))
                    File.Create(dir);
            }
        }
        public static void RandomizeMod(MainWindow mw)
        {
            //ShadersList = mw.ShadersList;

            SaveDir = Settings.GameDirectory;

            PrepModFolders();

            // level corruptions
            string dir = $"{SaveDir}tilemaps";
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
                        LevelManip.CorruptLevel(ref level);
                    LevelManip.Save(level, file);
                }
            }

            // data folder
            dir = $"{SaveDir}data";
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
                            text[i] = "palette " + TilesetManip.GetPalette().ToString();
                        if (text[i].Contains("tile_graphics"))
                            text[i] = "tile_graphics " + TilesetManip.GetTile();
                        if (text[i].Contains("overlay_graphics"))
                            text[i] = "overlay_graphics " + TilesetManip.GetOverlay();
                        if (text[i].Contains("global_particle"))
                        {
                            var split = text[i].Trim().Split(' ');
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
