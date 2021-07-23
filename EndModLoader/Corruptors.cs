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

            // smart corruptions done first
            if (Randomizer.settings.CRSmart)
            {
                if (SmartCorruptActive(ref level))
                {
                    TSAppend += "\n#added by level corruptor\nfx_shader_mid cloudripples\nmidfx_graphics None\nmidfx_layer 2\n";
                }
            }
            if (Randomizer.settings.CROverlays) SmartCorruptOverlay(ref level);

            // tumor remover second
            if (Randomizer.settings.CRTumors)
            {
                if (Randomizer.settings.AreaType == "normal") TumorRandomizer(ref level);
                else if (Randomizer.settings.AreaType == "cart") RingRandomizer(ref level);
                else if (Randomizer.settings.AreaType == "dark") TumorRemover(ref level);
                else if (Randomizer.settings.AreaType == "glitch") TumorRemover(ref level);
                else if (Randomizer.settings.AreaType == "ironcart") TumorRemover(ref level);
            }

            // add enemies and add tiles is next
            AddTiles(ref level, Randomizer.settings.CRAddTiles);
            if (AddEnemies(ref level, Randomizer.settings.CRAddEnemies))
            {
                TSAppend += "\nfx_shader_mid cloudripples\nmidfx_graphics None\nmidfx_layer 2\n";
            }
            //PlaceTile(ref level, TileID.Feral, 5);
            
            // last priority is the various ones below
            //if (Randomizer.settings.CRChaos) TotalChaos(ref level);
            if (Randomizer.settings.CRCrumbles) RandomCrumbles(ref level);
            if (Randomizer.settings.CRSpikeStrips) SpikeStrips(ref level);
            if (Randomizer.settings.CRCrushers) Crushers(ref level);
            if (Randomizer.settings.CRWaterLevels && RNG.CoinFlip()) WaterLevel(ref level);

            return TSAppend;
        }
        public static bool AddEnemies(ref LevelFile level, int num)
        {
            bool hasGas = false;
            for (int i = 0; i < num; i++)
            {
                int index = RNG.random.Next(0, level.data.active.Length);
                if (level.data.active[index] == 0 && level.data.tag[index] == 0)
                {
                    var tile = (TileID)entityTiles[RNG.random.Next(0, entityTiles.Length)];
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
                int index = RNG.random.Next(0, level.data.active.Length);
                if (level.data.active[index] != TileID.Solid && level.data.tag[index] == TileID.Empty)
                    level.data.active[index] = (TileID)activeTiles[RNG.random.Next(0, activeTiles.Length)];
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
                int index = RNG.random.Next(0, level.data.active.Length);
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
                int index = RNG.random.Next(0, level.data.active.Length);
                if (level.data.active[index] == TileID.Solid && level.data.tag[index] == TileID.Empty)
                {
                    if (RNG.CoinFlip())
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
                int index = RNG.random.Next(0, level.data.active.Length);
                //if (level.data.tag[index] == 0)
                //{
                level.data.overlay[index] = (TileID)overlayTiles[RNG.random.Next(0, overlayTiles.Length)];
                //}
            }
        }
        public static void TumorRandomizer(ref LevelFile level)
        {
            int tumorsPerLevel = 1;
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;

            Bounds bounds = GetCameraBounds(level);
            TumorRemover(ref level);

            // place new tumor(s)
            for (int i = 0; i < tumorsPerLevel; i++)
            {
                bool placed = false;
                do {
                    int row = RNG.random.Next(bounds.Top, bounds.Bottom);
                    int col = RNG.random.Next(bounds.Left, bounds.Right);

                    index = row * lw + col;
                    if (level.data.active[index] == TileID.Empty)
                    {
                        level.data.active[index] = TileID.Tumor;
                        placed = true;
                    }

                } while (!placed);
            }
        }
        public static void WaterLevel(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            Bounds bounds = GetCameraBounds(level);

            for (int j = 0; j < lw; j++)
            {
                level.data.overlay[j] = TileID.WaterUB;
            }
        }
        public static void PlaceTile(ref LevelFile level, TileID toPlace, int numPerLevel, TileID toReplace = TileID.Empty, bool ignoreTags = false, int cushion = 3)
        {
            //int numPerLevel = 10;
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;

            Bounds bounds = GetCameraBounds(level);
            //TumorRemover(ref level);

            // place new tumor(s)
            for (int i = 0; i < numPerLevel; i++)
            {
                bool placed = false;
                do
                {
                    int row = RNG.random.Next(bounds.Top, bounds.Bottom);
                    int col = RNG.random.Next(bounds.Left + cushion, bounds.Right - cushion);

                    index = row * lw + col;
                    if (level.data.active[index] == toReplace)
                    {
                        if (level.data.tag[index] == TileID.Empty || ignoreTags)
                            level.data.active[index] = toPlace;
                        placed = true;
                    }

                } while (!placed);
            }
        }
        public static void TumorRemover(ref LevelFile level)
        {
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;
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
        }
        public static void RingRandomizer(ref LevelFile level)
        {
            int ringsPerLevel = 10;
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;

            Bounds bounds = GetCameraBounds(level);
            TumorRemover(ref level);

            // place new rings(s)
            for (int i = 0; i < ringsPerLevel; i++)
            {
                bool placed = false;
                do
                {
                    int row = RNG.random.Next(bounds.Top, bounds.Bottom);
                    int col = RNG.random.Next(bounds.Left, bounds.Right);

                    index = row * lw + col;
                    if (level.data.active[index] == TileID.Empty)
                    {
                        level.data.active[index] = TileID.Ring;
                        placed = true;
                    }

                } while (!placed);
            }
        }
        public static Bounds GetCameraBounds(LevelFile level)
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
        public static bool SmartCorruptActive(ref LevelFile level)
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
                        if (RNG.random.Next(0, corruptLevel) == 0)
                        {
                            try
                            {
                                string s = options[RNG.random.Next(0, options.Length)];
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
                            string s = options[RNG.random.Next(0, options.Length)];
                            if (s != null && s != "")
                            {
                                int num = Convert.ToInt32(s);
                                if (num == 40003 || num == 40047) { hasGas = true; }
                                level.data.active[index] = (TileID)num;
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
        public static void SmartCorruptOverlay(ref LevelFile level)
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
                    // overlay layer
                    var element = smartTiles.Element(Enum.GetName(typeof(TileID), level.data.overlay[index]));
                    if (element != null)
                        options = Randomizer.ElementToArray(element);   // use index to get enum name, search for corruption options in xml
                    if (RNG.CoinFlip())
                    {
                        try
                        {
                            string s = options[RNG.random.Next(0, options.Length)];
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
        }


        // These need to be updated or removed
        public static LevelFile CombineLevels2(LevelFile level1, LevelFile level2)
        {
            var levelNew = level1;

            if (level1.header.height == 32 && level2.header.height == 32 && level1.header.width == 54 && level2.header.width == 54)
            {
                int index = 0;
                int lw = 54;
                int lh = 32;
                for (int i = 0; i < lh; i++)
                {
                    for (int j = 0; j < lw / 2; j++)
                    {
                        index = i * lw + j;
                        index += 27;
                        //if (j % 4 < 2)
                        //{
                            levelNew.data.active[index] = level2.data.active[index];
                            levelNew.data.back1[index] = level2.data.back1[index];
                            levelNew.data.back2[index] = level2.data.back2[index];
                            levelNew.data.tag[index] = level2.data.tag[index];
                            levelNew.data.overlay[index] = level2.data.overlay[index];
                        //}
                    }
                }

                for (int i = 0; i < lh; i++)
                {
                    for (int j = 25; j < 29; j++)
                    {
                        index = i * lw + j;

                        //levelNew.data.overlay[index] = TileID.GraityBeam;

                        if (RNG.CoinFlip() && RNG.CoinFlip() && RNG.CoinFlip())
                        {
                            switch (RNG.random.Next(0, 3))
                            {
                                case 0:
                                    levelNew.data.active[index] = TileID.Crumble;
                                    break;
                                case 1:
                                    levelNew.data.active[index] = TileID.Platform;
                                    break;
                                case 2:
                                    levelNew.data.active[index] = TileID.Solid;
                                    break;
                            }
                        }
                        else if (levelNew.data.active[index] == TileID.Solid) levelNew.data.active[index] = TileID.Empty;
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    levelNew.data.active[RNG.random.Next(0, lw * lh)] = TileID.Switch1U;
                    levelNew.data.active[RNG.random.Next(0, lw * lh)] = TileID.Switch2U;
                    levelNew.data.active[RNG.random.Next(0, lw * lh)] = TileID.Switch3U;
                    levelNew.data.active[RNG.random.Next(0, lw * lh)] = TileID.Switch4U;
                }

                levelNew.data.tag[0] = TileID.CameraBounds;
                levelNew.data.tag[1727] = TileID.CameraBounds;
                //for (int i = 0; i < lh; i++)
                //{
                //    for (int j = 0; j < 27; j++)
                //    {
                //        index = i * lw + j;
                //        index += 16;
                //        levelNew.data.active[index] = level2.data.active[index];
                //        levelNew.data.back1[index] = level2.data.back1[index];
                //        levelNew.data.back2[index] = level2.data.back2[index];
                //        levelNew.data.tag[index] = level2.data.tag[index];
                //        levelNew.data.overlay[index] = level2.data.overlay[index];
                //    }
                //}
            }

            return levelNew;
        }
        public static LevelFile MergeLevels(LevelFile level1, LevelFile level2)
        {
            // find right edge of first level and actual camera bounds
            // find left edge of second level and actual camera bounds
            // Coordinate pairs have row first, column second
            Pair L1ExitCoord = new Pair(0, 0), L2EntryCoord = new Pair(0, 0);
            Bounds L1Bounds = GetCameraBounds(level1), L2Bounds = GetCameraBounds(level2);

            int index = 0;
            int lw = level1.header.width;
            int lh = level1.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level1.data.tag[index] == TileID.GreenTransitionR)
                    { level1.data.tag[index] = TileID.Empty; L1ExitCoord.First = i; L1ExitCoord.Second = j; }
                    if (level2.data.tag[index] == TileID.GreenTransitionL)
                    { level2.data.tag[index] = TileID.Empty; L2EntryCoord.First = i; L2EntryCoord.Second = j; }
                }
            }
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level1.data.tag[index] == TileID.MergeMarkerR)
                    { L1ExitCoord.First = i; L1ExitCoord.Second = j; }
                    if (level2.data.tag[index] == TileID.MergeMarkerL)
                    { L2EntryCoord.First = i; L2EntryCoord.Second = j; }
                }
            }
            // establish level boundaries and new level size
            Pair L1Origin = new Pair(0, 0); // find vertical offset of L1 (originOffset = L2EntryCoord.height - L1ExitCoord.height)
            Pair L2Origin = new Pair(0, 0); // level 2 origin point = originOffset + L1ExitCoord - L2EntryCoord

            if (L1ExitCoord.First < L2EntryCoord.First)
            {
                L1Origin.First = L2EntryCoord.First - L1ExitCoord.First;
            }
            else
            {
                L1Origin.First = L1ExitCoord.First - L2EntryCoord.First;
            }
            L2Origin = L1Origin + L1ExitCoord - L2EntryCoord;

            Console.WriteLine($"L1ExitCoord: {L1ExitCoord.First}, {L1ExitCoord.Second}");
            Console.WriteLine($"L2EntryCoord: {L2EntryCoord.First}, {L2EntryCoord.Second}");
            Console.WriteLine($"L1Origin: {L1Origin.First}, {L1Origin.Second}");
            Console.WriteLine($"L2Origin: {L2Origin.First}, {L2Origin.Second}");

            // create new level
            // size is doubled in both dimensions
            int width = L1ExitCoord.Second + level2.header.width - L2EntryCoord.Second;
            int height = Math.Max(L1Origin.First + level1.header.height, L2Origin.First + level2.header.height);
            var levelNew = new LevelFile(width, height);

            // copy first level into new level
            // copies from left edge of canvas to L1ExitCoord column
            int copyIndex = 0;
            int pasteIndex = 0;
            int copylw = level1.header.width;
            int copylh = level1.header.height;
            int pastelw = levelNew.header.width;
            int pastelh = levelNew.header.height;
            // copy first level
            for (int i = 0; i < copylh; i++)
            {
                for (int j = 0; j < L1ExitCoord.Second; j++)
                {
                    copyIndex = i * copylw + j;
                    pasteIndex = (i + L1Origin.First) * pastelw + (j + L1Origin.Second);
                    levelNew.data.active[pasteIndex] = level1.data.active[copyIndex];
                    levelNew.data.back1[pasteIndex] = level1.data.back1[copyIndex];
                    levelNew.data.back2[pasteIndex] = level1.data.back2[copyIndex];
                    levelNew.data.tag[pasteIndex] = level1.data.tag[copyIndex];
                    levelNew.data.overlay[pasteIndex] = level1.data.overlay[copyIndex];
                }
            }

            // copy second level into new level
            // align L2EntryCoord to L1ExitCoord
            // copy from L2EntryCoord to right edge of canvas
            copyIndex = 0;
            pasteIndex = 0;
            copylw = level2.header.width;
            copylh = level2.header.height;
            pastelw = levelNew.header.width;
            pastelh = levelNew.header.height;
            for (int i = 0; i < copylh; i++)
            {
                for (int j = L2EntryCoord.Second; j < copylw; j++)
                {
                    copyIndex = i * copylw + j;
                    pasteIndex = (i + L2Origin.First) * pastelw + (j + L2Origin.Second);

                    //Console.WriteLine($"copyIndex: {i},{j}");
                    //Console.WriteLine($"pasteIndex: {i + L2Origin.First},{j + L2Origin.Second}");

                    levelNew.data.active[pasteIndex] = level2.data.active[copyIndex];
                    levelNew.data.back1[pasteIndex] = level2.data.back1[copyIndex];
                    levelNew.data.back2[pasteIndex] = level2.data.back2[copyIndex];
                    levelNew.data.tag[pasteIndex] = level2.data.tag[copyIndex];
                    levelNew.data.overlay[pasteIndex] = level2.data.overlay[copyIndex];
                }
            }

            // place new camera bounds based on original viewable area
            int bottomLeftCamera = (Math.Max(L1Bounds.Bottom, L2Bounds.Bottom)) * levelNew.header.width + (Math.Min(L1Bounds.Left, L2Bounds.Left));
            int topRightCamera = (Math.Min(L1Bounds.Top, L2Bounds.Top)) * levelNew.header.width + (Math.Max(L1Bounds.Right, L2Bounds.Right));
            levelNew.data.tag[bottomLeftCamera] = TileID.CameraBounds;
            levelNew.data.tag[topRightCamera] = TileID.CameraBounds;

            return levelNew;
        }
        public static LevelFile CombineLevels(LevelFile level1, LevelFile level2)
        {
            // find right edge of first level and actual camera bounds
            // find left edge of second level and actual camera bounds
            // Coordinate pairs have row first, column second
            Pair L1ExitCoord = new Pair(0, 0), L2EntryCoord = new Pair(0, 0);
            Bounds L1Bounds = GetCameraBounds(level1), L2Bounds = GetCameraBounds(level2);

            int index = 0;
            int lw = level1.header.width;
            int lh = level1.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level1.data.tag[index] == TileID.GreenTransitionR)
                    { level1.data.tag[index] = TileID.Empty; L1ExitCoord.First = i; L1ExitCoord.Second = j; }
                    if (level2.data.tag[index] == TileID.GreenTransitionL)
                    { level2.data.tag[index] = TileID.Empty; L2EntryCoord.First = i; L2EntryCoord.Second = j; }
                }
            }
            //for (int i = 0; i < lh; i++)
            //{
            //    for (int j = 0; j < lw; j++)
            //    {
            //        index = i * lw + j;
            //        if (level1.data.tag[index] == TileID.MergeMarkerR)
            //        { L1ExitCoord.First = i; L1ExitCoord.Second = j; }
            //        if (level2.data.tag[index] == TileID.MergeMarkerL)
            //        { L2EntryCoord.First = i; L2EntryCoord.Second = j; }
            //    }
            //}
            // establish level boundaries and new level size
            Pair L1Origin = new Pair(0, 0); // find vertical offset of L1 (originOffset = L2EntryCoord.height - L1ExitCoord.height)
            Pair L2Origin = new Pair(0, 0); // level 2 origin point = originOffset + L1ExitCoord - L2EntryCoord

            if (L1ExitCoord.First < L2EntryCoord.First)
            {
                L1Origin.First = L2EntryCoord.First - L1ExitCoord.First;
            }
            else
            {
                L1Origin.First = L1ExitCoord.First - L2EntryCoord.First;
            }
            L2Origin = L1Origin + L1ExitCoord - L2EntryCoord;

            Console.WriteLine($"L1ExitCoord: {L1ExitCoord.First}, {L1ExitCoord.Second}");
            Console.WriteLine($"L2EntryCoord: {L2EntryCoord.First}, {L2EntryCoord.Second}");
            Console.WriteLine($"L1Origin: {L1Origin.First}, {L1Origin.Second}");
            Console.WriteLine($"L2Origin: {L2Origin.First}, {L2Origin.Second}");

            // create new level
            // size is doubled in both dimensions
            int width = L1ExitCoord.Second + level2.header.width - L2EntryCoord.Second;
            int height = Math.Max(L1Origin.First + level1.header.height, L2Origin.First + level2.header.height);
            var levelNew = new LevelFile(width, height);

            // copy first level into new level
            // copies from left edge of canvas to L1ExitCoord column
            int copyIndex = 0;
            int pasteIndex = 0;
            int copylw = level1.header.width;
            int copylh = level1.header.height;
            int pastelw = levelNew.header.width;
            int pastelh = levelNew.header.height;
            // copy first level
            for (int i = 0; i < copylh; i++)
            {
                for (int j = 0; j < L1ExitCoord.Second; j++)
                {
                    copyIndex = i * copylw + j;
                    pasteIndex = (i + L1Origin.First) * pastelw + (j + L1Origin.Second);
                    levelNew.data.active[pasteIndex] = level1.data.active[copyIndex];
                    levelNew.data.back1[pasteIndex] = level1.data.back1[copyIndex];
                    levelNew.data.back2[pasteIndex] = level1.data.back2[copyIndex];
                    levelNew.data.tag[pasteIndex] = level1.data.tag[copyIndex];
                    levelNew.data.overlay[pasteIndex] = level1.data.overlay[copyIndex];
                }
            }

            // copy second level into new level
            // align L2EntryCoord to L1ExitCoord
            // copy from L2EntryCoord to right edge of canvas
            copyIndex = 0;
            pasteIndex = 0;
            copylw = level2.header.width;
            copylh = level2.header.height;
            pastelw = levelNew.header.width;
            pastelh = levelNew.header.height;
            for (int i = 0; i < copylh; i++)
            {
                for (int j = L2EntryCoord.Second; j < copylw; j++)
                {
                    copyIndex = i * copylw + j;
                    pasteIndex = (i + L2Origin.First) * pastelw + (j + L2Origin.Second);

                    //Console.WriteLine($"copyIndex: {i},{j}");
                    //Console.WriteLine($"pasteIndex: {i + L2Origin.First},{j + L2Origin.Second}");

                    levelNew.data.active[pasteIndex]    = level2.data.active[copyIndex];
                    levelNew.data.back1[pasteIndex]     = level2.data.back1[copyIndex];
                    levelNew.data.back2[pasteIndex]     = level2.data.back2[copyIndex];
                    levelNew.data.tag[pasteIndex]       = level2.data.tag[copyIndex];
                    levelNew.data.overlay[pasteIndex]   = level2.data.overlay[copyIndex];
                }
            }

            //// fill in blank at topL1
            //for (int i = 0; i < L1Origin.First; i++)
            //{
            //    for (int j = 0; j < L1ExitCoord.Second; j++)
            //    {
            //        //copyIndex = j;
            //        pasteIndex = i * pastelw + (j + L1Origin.Second);
            //        levelNew.data.active[pasteIndex] = TileID.Decoration1;
            //        //levelNew.data.active[pasteIndex]    = level1.data.active[copyIndex];
            //        //levelNew.data.back1[pasteIndex]     = level1.data.back1[copyIndex];
            //        //levelNew.data.back2[pasteIndex]     = level1.data.back2[copyIndex];
            //        //levelNew.data.tag[pasteIndex]       = level1.data.tag[copyIndex];
            //        //levelNew.data.overlay[pasteIndex]   = level1.data.overlay[copyIndex];
            //    }
            //}
            //// fill in blank at bottomL1
            //for (int i = L1Origin.First + level1.header.height; i < pastelh; i++)
            //{
            //    for (int j = 0; j < L1ExitCoord.Second; j++)
            //    {
            //        //copyIndex = (L1Origin.First + level1.header.height) * copylw + j;
            //        pasteIndex = i * pastelw + (j + L1Origin.Second);
            //        levelNew.data.active[pasteIndex] = TileID.Decoration1;
            //    }
            //}
            //// fill in blank at topL2
            //for (int i = 0; i < L2Origin.First; i++)
            //{
            //    for (int j = L2EntryCoord.Second; j < pastelw; j++)
            //    {
            //        pasteIndex = i * pastelw + (j + L2Origin.Second);
            //        levelNew.data.active[pasteIndex] = TileID.Decoration1;
            //    }
            //}
            //// fill in blank at bottomL2
            //for (int i = L2Origin.First + level2.header.height; i < pastelh; i++)
            //{
            //    for (int j = L2EntryCoord.Second; j < pastelw; j++)
            //    {
            //        pasteIndex = i * pastelw + (j + L2Origin.Second);
            //        levelNew.data.active[pasteIndex] = TileID.Decoration1;
            //    }
            //}

            // place new camera bounds based on original viewable area
            int bottomLeftCamera = (Math.Max(L1Bounds.Bottom, L2Bounds.Bottom)) * levelNew.header.width + (Math.Min(L1Bounds.Left, L2Bounds.Left));
            int topRightCamera = (Math.Min(L1Bounds.Top, L2Bounds.Top)) * levelNew.header.width + (Math.Max(L1Bounds.Right, L2Bounds.Right));
            //levelNew.data.tag[bottomLeftCamera] = TileID.CameraBounds;
            //levelNew.data.tag[topRightCamera] = TileID.CameraBounds;

            int last = levelNew.header.height * levelNew.header.width;
            levelNew.data.tag[0] = TileID.CameraBounds;
            levelNew.data.tag[last - 1] = TileID.CameraBounds;

            // trim level



            return levelNew;
        }

    }
}
