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
        public static XElement smartTiles { get; set; }

        static Corruptors()
        {
            var doc = XDocument.Load($"data/corruptor_tiles.xml");
            activeTiles = Randomizer.ElementToArray(doc.Root.Element("active"), true);
            entityTiles = Randomizer.ElementToArray(doc.Root.Element("entity"), true);
            overlayTiles = Randomizer.ElementToArray(doc.Root.Element("overlay"), true);
            smartTiles = doc.Root.Element("smart");
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

            
            if (Randomizer.settings.AreaType == AreaTypes.normal) TumorRandomizer(ref level);
            else if (Randomizer.settings.AreaType == AreaTypes.cart) RingRandomizer(ref level);



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

        public static void TumorRandomizer(ref LevelFile level)
        {
            int tumorsPerLevel = 1;
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;

            Bounds bounds = GetCameraBounds(ref level);

            // delete original tumor
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level.data.active[index] == TileID.Tumor)
                        level.data.active[index] = TileID.Empty;
                }
            }
            // place new tumor(s)
            for (int i = 0; i < tumorsPerLevel; i++)
            {
                bool placed = false;
                do {
                    int row = Randomizer.myRNG.rand.Next(bounds.Top, bounds.Bottom);
                    int col = Randomizer.myRNG.rand.Next(bounds.Left, bounds.Right);

                    index = row * lw + col;
                    if (level.data.active[index] == TileID.Empty)
                    {
                        level.data.active[index] = TileID.Tumor;
                        placed = true;
                    }

                } while (!placed);
            }
        }

        public static void RingRandomizer(ref LevelFile level)
        {
            int ringsPerLevel = 10;
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;

            Bounds bounds = GetCameraBounds(ref level);

            // delete original tumor
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level.data.active[index] == TileID.Tumor)
                        level.data.active[index] = TileID.Empty;
                }
            }
            // place new rings(s)
            for (int i = 0; i < ringsPerLevel; i++)
            {
                bool placed = false;
                do
                {
                    int row = Randomizer.myRNG.rand.Next(bounds.Top, bounds.Bottom);
                    int col = Randomizer.myRNG.rand.Next(bounds.Left, bounds.Right);

                    index = row * lw + col;
                    if (level.data.active[index] == TileID.Empty)
                    {
                        level.data.active[index] = TileID.Ring;
                        placed = true;
                    }

                } while (!placed);
            }
        }

        public static Bounds GetCameraBounds(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;
            var bounds = new Bounds { Left = lw, Top = lh, Bottom = 0, Right = 0 };
            bool noCameraFound = false;

            int index = 0;
            for (int row = 0; row < lh; row++)
            {
                for (int col = 0; col < lw; col++)
                {
                    index = row * lw + col;
                    if (level.data.tag[index] == TileID.CameraBounds)
                    {
                        bounds.Top = Math.Min(row, bounds.Top);
                        bounds.Bottom = Math.Max(row, bounds.Bottom);
                        bounds.Left = Math.Min(col, bounds.Left);
                        bounds.Right = Math.Max(col, bounds.Right);
                    }
                }
            }

            // correct for aspect ratio
            double correctAspect = 16 / 9;
            double width = bounds.Right - bounds.Left;
            double height = bounds.Bottom - bounds.Top;
            double aspect = width / height;
            double hCenter = bounds.Left + (width / 2);
            double vCenter = bounds.Top + (height / 2);

            if (aspect < correctAspect) // aspect ratio too tall
            {
                width = height * 16 / 9;
                bounds.Right = (int)(hCenter - width / 2);
                bounds.Left = (int)(hCenter + width / 2);

            }
            if (aspect > correctAspect) // aspect ratio too wide
            {
                height = width * 9 / 16;
                bounds.Top = (int)(vCenter - height / 2);
                bounds.Bottom = (int)(vCenter + height / 2);
            }

            if (bounds.Top < 0) bounds.Top = 0;
            if (bounds.Left < 0) bounds.Left = 0;
            if (bounds.Right > lw) bounds.Top = lw;
            if (bounds.Bottom > lh) bounds.Bottom = lh;

            if (bounds.Top > bounds.Bottom || bounds.Left > bounds.Right)
            {
                Console.WriteLine("Broken Bounds");
            }


            return bounds;
        }

        public static bool IsInCameraBounds(Bounds bounds, int row, int col)
        {
            if (row > bounds.Top && row < bounds.Bottom && col > bounds.Left && col < bounds.Right)
                return true;
            else return false;
        }

        public static bool SmartCorrupt(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;
            
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
                    var element = smartTiles.Element(Enum.GetName(typeof(TileID), level.data.active[index]));

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
                    element = smartTiles.Element(Enum.GetName(typeof(TileID), level.data.overlay[index]));
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
