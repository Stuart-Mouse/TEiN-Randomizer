using System;
using System.IO;

namespace TEiNRandomizer
{
    public static class Worldmap
    {
        public static string WorldMapFile;

        public static void WriteWorldMap()
        {
            WorldMapFile = File.ReadAllText("data/worldmap_template.txt");
            if (Randomizer.settings.DeadRacer) DeadRacer();
            CartLives();
            File.WriteAllText(Randomizer.saveDir + "data/worldmap.txt", WorldMapFile);
        }
        public static void DeadRacer()
        {
            string timedCarts = "";
            for (int i = 0; i < Randomizer.settings.NumAreas; i++)
            {
                if (Randomizer.AreaTypes[i] == "glitch")
                    timedCarts += "v" + Convert.ToString(i + 1) + "-1 ";
            }
            WorldMapFile = WorldMapFile.Replace("TIMEDCARTS", timedCarts);
        }
        public static void CartLives()
        {
            string cartLives = "";
            for (int i = 0; i < Randomizer.settings.NumAreas; i++)
            {
                if (Randomizer.AreaTypes[i] == "cart")
                    cartLives += "[v" + Convert.ToString(i + 1) + "-1 " + Randomizer.settings.CartLives + "] ";
            }
            WorldMapFile = WorldMapFile.Replace("CARTLIVES", cartLives);
        }
    }
}
