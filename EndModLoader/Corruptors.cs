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
            overlayTiles = Randomizer.ElementToArray(doc.Root.Element("overlay"), true);
        }
        public static string CorruptLevel(ref LevelFile level)
        {
            string TSAppend = "";

            //RandomCrumbles(ref level);
            //SpikeStrips(ref level);
            //Crushers(ref level);
            //OverlayStuff(ref level);
            if (SmartCorrupt(ref level))
            {
                TSAppend += "fx_shader_mid cloudripples\nmidfx_graphics None\nmidfx_layer 2";
            }
            //if (AddEnemies(ref level, 5))
            //{
            //    TSAppend += "fx_shader_mid cloudripples\nmidfx_graphics None\nmidfx_layer 2";
            //}
            //AddTiles(ref level, 10);
            //TotalChaos(ref level);

            return TSAppend;
        }

        public static bool AddEnemies(ref LevelFile level, int num)
        {
            bool hasGas = false;
            for (int i = 0; i < num; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                if (level.data.active[index] == 0 && level.data.tag[index] == 0)
                {
                    var tile = (TileID)entityTiles[Randomizer.myRNG.rand.Next(0, entityTiles.Length)];
                    level.data.active[index] = tile;
                    if (tile == TileID.Gasper || tile == TileID.GasCloud) hasGas = true;

                }
            }
            return hasGas;
        }
        public static void AddTiles(ref LevelFile level, int num)
        {
            for (int i = 0; i < num; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                if (level.data.active[index] != TileID.Solid && level.data.tag[index] == TileID.Empty)
                    level.data.active[index] = (TileID)activeTiles[Randomizer.myRNG.rand.Next(0, activeTiles.Length)];
            }
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
                    if (level.data.active[index] == TileID.Solid && j % 5 == i % 5)
                        level.data.active[index] = TileID.SpikeU;
                }
            }
        }
        public static void RandomCrumbles(ref LevelFile level)
        {
            for (int i = 0; i < 100; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                if (level.data.active[index] == TileID.Solid)
                    level.data.active[index] = TileID.Crumble;
            }
        }
        public static void TotalChaos(ref LevelFile level)
        {
           
        }

        public static void Crushers(ref LevelFile level)
        {
            for (int i = 0; i < 30; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                if (level.data.active[index] == TileID.Solid && level.data.tag[index] == TileID.Empty)
                {
                    if (Randomizer.myRNG.CoinFlip())
                        level.data.active[index] = TileID.CrusherEye;
                    else level.data.active[index] = TileID.CrusherGear;

                    level.data.tag[index] = TileID.Crusher;
                }
            }
        }

        public static void OverlayStuff(ref LevelFile level)
        {
            for (int i = 0; i < 60; i++)
            {
                int index = Randomizer.myRNG.rand.Next(0, level.data.active.Length);
                //if (level.data.tag[index] == 0)
                //{
                level.data.overlay[index] = (TileID)overlayTiles[Randomizer.myRNG.rand.Next(0, overlayTiles.Length)];
                //}
            }
        }
        public static bool SmartCorrupt(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;
            var doc = XDocument.Load($"data/corruptor_tiles.xml");
            var smart = doc.Root.Element("smart");
            var options = new string[] { };
            bool hasGas = false;
            int corruptLevel = 3;

            // loop over entire level
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    int index = i * lw + j;

                    // active layer
                    var element = smart.Element(Enum.GetName(typeof(TileID), level.data.active[index]));

                    if (element != null)
                        options = Randomizer.ElementToArray(element);   // use index to get enum name, search for corruption options in xml
                    
                    if ((Int32)level.data.active[index] < 1000)
                    {
                        if (Randomizer.myRNG.rand.Next(0, corruptLevel) == 0)
                        {
                            try
                            {
                                string s = options[Randomizer.myRNG.rand.Next(0, options.Length)];
                                if (s != null && s != "")
                                {
                                    int num = Convert.ToInt32(s);
                                    level.data.active[index] = (TileID)num;
                                }
                            }
                            catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                        }
                    }
                    else
                    {
                        try
                        {
                            string s = options[Randomizer.myRNG.rand.Next(0, options.Length)];
                            if (s != null && s != "")
                            {
                                int num = Convert.ToInt32(s);
                                if (num == 40003 || num == 40047) { hasGas = true; }
                                level.data.active[index] = (TileID)num;
                            }
                        }
                        catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                    }

                    // overlay layer
                    element = smart.Element(Enum.GetName(typeof(TileID), level.data.overlay[index]));
                    if (element != null)
                        options = Randomizer.ElementToArray(element);   // use index to get enum name, search for corruption options in xml
                    if (Randomizer.myRNG.CoinFlip())
                    {
                        try
                        {
                            string s = options[Randomizer.myRNG.rand.Next(0, options.Length)];
                            if (s != null && s != "")
                            {
                                int num = Convert.ToInt32(s);
                                level.data.overlay[index] = (TileID)num;
                            }
                        }
                        catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                    }
                }
            }

            // Replace Tile and Enemies by List
            // Make consistent rules for tag replacement
            // Cannons (need targets)
            // special case for musk


            return hasGas;
        }
        

    }
}
