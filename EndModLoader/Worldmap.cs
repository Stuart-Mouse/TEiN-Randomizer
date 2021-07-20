using System;
using System.IO;

namespace TEiNRandomizer
{
    public static class Worldmap
    {
        public static void WriteWorldMap(RandomizerSettings settings)
        {
            string file = File.ReadAllText("Data/worldmap_template.txt");
            if (settings.DeadRacer) DeadRacer(ref file, settings);
            CartLives(ref file, settings);
            File.WriteAllText(Randomizer.saveDir + "data/worldmap.txt", file);
        }
        public static void DeadRacer(ref string file, RandomizerSettings settings)
        {
            string timedCarts = "";
            for (int i = 0; i < settings.NumAreas; i++)
            {
               timedCarts += "v" + Convert.ToString(i + 1) + "-1 ";
            }
            file = file.Replace("TIMEDCARTS", timedCarts);
        }
        public static void CartLives(ref string file, RandomizerSettings settings)
        {
            string cartLives = "";
            for (int i = 0; i < settings.NumAreas; i++)
            {
                cartLives += "[v" + Convert.ToString(i + 1) + "-1 " + settings.CartLives + "] ";
            }
            file = file.Replace("CARTLIVES", cartLives);
        }
    }
}
