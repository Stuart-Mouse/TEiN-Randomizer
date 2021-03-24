using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public static class Corruptors
    {
        public static Int32[] activeTiles { get; set; }
        public static Int32[] entityTiles { get; set; }
        public static Int32[] overlayTiles { get; set; }
        public static Int32[] tagTiles { get; set; }
        public static Int32[] back1Tiles { get; set; }
        public static Int32[] back2Tiles { get; set; }

        static Corruptors()
        {
            var doc = XDocument.Load($"data/corruptor_tiles.xml");
            activeTiles = Randomizer.ElementToArray(doc.Root.Element("active"), true);
            entityTiles = Randomizer.ElementToArray(doc.Root.Element("entity"), true);
        }
        public static void CorruptLevel(ref LevelFile level)
        {
            //RandomCrumbles(ref level);
            //SpikeStrips(ref level);
            TotalChaos(ref level);
        }
        public static void SpikeStrips(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    int index = i * lw + j;
                    if (level.data.active[index] == 1 && j % 5 == i % 5)
                        level.data.active[index] = 5;
                }
            }
        }
        public static void RandomCrumbles(ref LevelFile level)
        {
            for (int i = 0; i < 100; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                if (level.data.active[index] == 1)
                    level.data.active[index] = 10;
            }
        }
        public static void TotalChaos(ref LevelFile level)
        {
            for (int i = 0; i < 300; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                if (level.data.active[index] == 1 && level.data.tag[index] == 0)
                    level.data.active[index] = activeTiles[Randomizer.myRNG.rand.Next(0, activeTiles.Length)];
            }
            for (int i = 0; i < 10; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                if (level.data.active[index] == 0 && level.data.tag[index] == 0)
                    level.data.active[index] = entityTiles[Randomizer.myRNG.rand.Next(0, entityTiles.Length)];
            }
        }
    }
}
