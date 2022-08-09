using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public static class LevelCorruptors
    {
        public static Int32[] ActiveTiles { get; set; }
        public static Int32[] EntityTiles { get; set; }
        public static Int32[] OverlayTiles { get; set; }
        public static Int32[] TagTiles { get; set; }
        public static Int32[] Back1Tiles { get; set; }
        public static Int32[] Back2Tiles { get; set; }
        public static Dictionary<int, int[]> SmartTiles { get; set; }
        public static Dictionary<int, int[]> ColorTiles { get; set; }

        public const int BLUE_OFFSET    = 100000;
        public const int YELLOW_OFFSET  = 200000;
        public const int RED_OFFSET     = 300000;
        public const int GREEN_OFFSET   = 400000;
        
        static LevelCorruptors()
        {
            var gon      = GonObject.Load($"data/text/corruptor_tiles.gon");
            ActiveTiles  = GonObject.Manip.ToIntArray(gon["active"]);
            EntityTiles  = GonObject.Manip.ToIntArray(gon["entity"]);
            OverlayTiles = GonObject.Manip.ToIntArray(gon["overlay"]);
            SmartTiles = LoadDictionary(gon["smart"]);
            ColorTiles = LoadDictionary(gon["color"]);
        }
        static Dictionary<int, int[]> LoadDictionary(GonObject gon)
        {
            var dict = new Dictionary<int, int[]>();
            for (int i = 0; i < gon.Size(); i++)
            {
                var item = gon[i];
                int id = item["id"].Int();
                int[] alts = GonObject.Manip.ToIntArray(item["alts"]);

                dict.Add(id, alts);
            }
            return dict;
        }

        // Clean Level and Do Collectables
        public static int CleanLevels(ref LevelFile level, Collectables collectables, Directions block_directions)
        {
            // This function "cleans" the level file before saving. 
            // This entails placing all of the required collectables,
            // creating locks where necessary, and de-coloring the level.

            // Return codes let the calling function know if minor errors are encountered,
            // which we do not want to throw as exceptions.
            
            // WARNING: This function will throw an index out of range exception if the level
            // contains fewer positions for collectables than the number of collectables to be placed.
            // This can also occur if a cart level has fewer than 10 rings, and rings are requested.

            List<int> tumors = new List<int>(); // indices where tumors, megatumors, and carts may be placed
            List<int> rings  = new List<int>(); // indices where rings may be placed

            for (int i = 0; i < level.header.width * level.header.height; i++)
            {
                if (level.data[LevelFile.ACTIVE, i] == TileID.Tumor)
                {
                    level.data[LevelFile.ACTIVE, i] = TileID.None;
                    tumors.Add(i);
                }
                if (level.data[LevelFile.ACTIVE, i] == TileID.Ring)
                {
                    level.data[LevelFile.ACTIVE, i] = TileID.None;
                    rings.Add(i);
                }
                if (level.data[LevelFile.ACTIVE , i] == TileID.LevelGoal && !collectables.HasFlag(Collectables.LevelGoal))
                    level.data[LevelFile.ACTIVE , i] = TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockU)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.U) ? TileID.SolidOver : TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockD)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.D) ? TileID.SolidOver : TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockL)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.L) ? TileID.SolidOver : TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockR)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.R) ? TileID.SolidOver : TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockUR)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.UR) ? TileID.SolidOver : TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockDR)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.DR) ? TileID.SolidOver : TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockUL)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.UL) ? TileID.SolidOver : TileID.None;
                if (level.data[LevelFile.OVERLAY, i] == TileID.EBlockDL)
                    level.data[LevelFile.OVERLAY, i] = block_directions.HasFlag(Directions.DL) ? TileID.SolidOver : TileID.None;
            }

            if (collectables.HasFlag(Collectables.Tumor))
            {
                if (tumors.Count == 0) return 1;
                int i = RNG.random.Next(0, tumors.Count);
                level.data[LevelFile.ACTIVE, tumors[i]] = TileID.Tumor;
                tumors.RemoveAt(i);
            }
            if (collectables.HasFlag(Collectables.MegaTumor))
            {
                if (tumors.Count == 0) return 2;
                int i = RNG.random.Next(0, tumors.Count);
                level.data[LevelFile.ACTIVE, tumors[i]] = TileID.MegaTumor;
                tumors.RemoveAt(i);
            }
            if (collectables.HasFlag(Collectables.Cartridge))
            {
                if (tumors.Count == 0) return 3;
                int i = RNG.random.Next(0, tumors.Count);
                level.data[LevelFile.ACTIVE, tumors[i]] = TileID.Cartridge;
                tumors.RemoveAt(i);
            }
            if (collectables.HasFlag(Collectables.FriendHead))
            {
                if (tumors.Count == 0) return 4;
                int i = RNG.random.Next(0, tumors.Count);
                level.data[LevelFile.ACTIVE, tumors[i]] = TileID.FriendPieces1;
                tumors.RemoveAt(i);
            }
            if (collectables.HasFlag(Collectables.FriendBody))
            {
                if (tumors.Count == 0) return 5;
                int i = RNG.random.Next(0, tumors.Count);
                level.data[LevelFile.ACTIVE, tumors[i]] = TileID.FriendPieces2;
                tumors.RemoveAt(i);
            }
            if (collectables.HasFlag(Collectables.FriendSoul))
            {
                if (tumors.Count == 0) return 6;
                int i = RNG.random.Next(0, tumors.Count);
                level.data[LevelFile.ACTIVE, tumors[i]] = TileID.FriendPieces3;
                tumors.RemoveAt(i);
            }
            /*if (args.HasFlag(Collectables.LevelGoal))
            {
                if (tumors.Count == 0) return 7;
                int i = RNG.random.Next(0, tumors.Count);
                level.data[LevelFile.ACTIVE, tumors[i]] = TileID.LevelGoal;
                tumors.RemoveAt(i);
            }*/
            if (collectables.HasFlag(Collectables.Rings))
            {
                if (rings.Count < 10) return 8;
                for (int r = 0; r < 10; r++)
                {
                    int i = RNG.random.Next(0, rings.Count);
                    level.data[LevelFile.ACTIVE, rings[i]] = TileID.Ring;
                    rings.RemoveAt(i);
                }
            }

            return 0;
        }


        public static string CorruptLevel(ref LevelFile level)
        {
            string TSAppend = "";

            // smart corruptions done first
            if (Randomizer.Settings.CRSmart)
            {
                if (SmartCorruptActive(ref level))
                {
                    TSAppend += "\n#added by level corruptor\nfx_shader_mid cloudripples\nmidfx_graphics None\nmidfx_layer 2\n";
                }
            }
            if (Randomizer.Settings.CROverlays) SmartCorruptOverlay(ref level);

            // tumor remover second
            if (Randomizer.Settings.CRTumors)
            {
                if (Randomizer.Settings.AreaType == "normal") TumorRandomizer(ref level);
                else if (Randomizer.Settings.AreaType == "cart") RingRandomizer(ref level);
                else if (Randomizer.Settings.AreaType == "dark") TumorRemover(ref level);
                else if (Randomizer.Settings.AreaType == "glitch") TumorRemover(ref level);
                else if (Randomizer.Settings.AreaType == "ironcart") TumorRemover(ref level);
            }

            // add enemies and add tiles is next
            AddTiles(ref level, Randomizer.Settings.CRAddTiles);
            if (AddEnemies(ref level, Randomizer.Settings.CRAddEnemies))
            {
                TSAppend += "\nfx_shader_mid cloudripples\nmidfx_graphics None\nmidfx_layer 2\n";
            }
            //PlaceTile(ref level, TileID.Feral, 5);
            
            // last priority is the various ones below
            //if (Randomizer.settings.CRChaos) TotalChaos(ref level);
            if (Randomizer.Settings.CRCrumbles) RandomCrumbles(ref level);
            if (Randomizer.Settings.CRSpikeStrips) SpikeStrips(ref level);
            if (Randomizer.Settings.CRCrushers) Crushers(ref level);
            if (Randomizer.Settings.CRWaterLevels && RNG.CoinFlip()) WaterLevel(ref level);

            return TSAppend;
        }
        public static bool AddEnemies(ref LevelFile level, int num)
        {
            bool hasGas = false;
            for (int i = 0; i < num; i++)
            {
                int index = RNG.random.Next(0, level.header.height * level.header.width);
                if (level.data[LevelFile.ACTIVE, index] == 0 && level.data[LevelFile.TAG, index] == 0)
                {
                    var tile = (TileID)EntityTiles[RNG.random.Next(0, EntityTiles.Length)];
                    level.data[LevelFile.ACTIVE, index] = tile;
                    if (tile == TileID.Gasper || tile == TileID.GasCloud) hasGas = true;

                }
            }
            return hasGas;
        }
        public static void AddTiles(ref LevelFile level, int num)
        {
            for (int i = 0; i < num; i++)
            {
                int index = RNG.random.Next(0, level.header.height * level.header.width);
                if (level.data[LevelFile.ACTIVE, index] != TileID.Solid && level.data[LevelFile.TAG, index] == TileID.None)
                    level.data[LevelFile.ACTIVE, index] = (TileID)ActiveTiles[RNG.random.Next(0, ActiveTiles.Length)];
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
                    if (level.data[LevelFile.ACTIVE, index] == TileID.Solid && j % 5 == i % 5)
                        level.data[LevelFile.ACTIVE, index] = TileID.SpikeU;
                }
            }
        }
        public static void RandomCrumbles(ref LevelFile level)
        {
            for (int i = 0; i < 100; i++)
            {
                int index = RNG.random.Next(0, level.header.height * level.header.width);
                if (level.data[LevelFile.ACTIVE, index] == TileID.Solid)
                    level.data[LevelFile.ACTIVE, index] = TileID.Crumble;
            }
        }
        public static void Crushers(ref LevelFile level)
        {
            for (int i = 0; i < 30; i++)
            {
                int index = RNG.random.Next(0, level.header.height * level.header.width);
                if (level.data[LevelFile.ACTIVE, index] == TileID.Solid && level.data[LevelFile.TAG, index] == TileID.None)
                {
                    if (RNG.CoinFlip())
                        level.data[LevelFile.ACTIVE, index] = TileID.CrusherEye;
                    else level.data[LevelFile.ACTIVE, index] = TileID.CrusherGear;

                    level.data[LevelFile.TAG, index] = TileID.Crusher;
                }
            }
        }
        public static void OverlayStuff(ref LevelFile level)
        {
            for (int i = 0; i < 60; i++)
            {
                int index = RNG.random.Next(0, level.header.height * level.header.width);
                //if (level.data[LevelFile.TAG, index] == 0)
                //{
                level.data[LevelFile.OVERLAY, index] = (TileID)OverlayTiles[RNG.random.Next(0, OverlayTiles.Length)];
                //}
            }
        }
        public static void TumorRandomizer(ref LevelFile level)
        {
            int tumorsPerLevel = 1;
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;
            int attempts = 0;

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
                    if (level.data[LevelFile.ACTIVE, index] == TileID.None)
                    {
                        level.data[LevelFile.ACTIVE, index] = TileID.Tumor;
                        placed = true;
                    }
                    if (attempts > 5) placed = true;
                    attempts++;
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
                level.data[LevelFile.OVERLAY, j] = TileID.WaterUB;
            }
        }
        public static void PlaceTile(ref LevelFile level, TileID toPlace, int numPerLevel, TileID toReplace = TileID.None, bool ignoreTags = false, int cushion = 3)
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
                    if (level.data[LevelFile.ACTIVE, index] == toReplace)
                    {
                        if (level.data[LevelFile.TAG, index] == TileID.None || ignoreTags)
                            level.data[LevelFile.ACTIVE, index] = toPlace;
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
                    if (level.data[LevelFile.ACTIVE, index] == TileID.Tumor)
                        level.data[LevelFile.ACTIVE, index] = TileID.None;
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
                    if (level.data[LevelFile.ACTIVE, index] == TileID.None)
                    {
                        level.data[LevelFile.ACTIVE, index] = TileID.Ring;
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
            bool CameraFound = false;

            int index = 0;
            for (int row = 0; row < lh; row++)
            {
                for (int col = 0; col < lw; col++)
                {
                    index = row * lw + col;
                    if (level.data[LevelFile.TAG, index] == TileID.CameraBounds)
                    {
                        bounds.Top = Math.Min(row, bounds.Top);
                        bounds.Bottom = Math.Max(row, bounds.Bottom);
                        bounds.Left = Math.Min(col, bounds.Left);
                        bounds.Right = Math.Max(col, bounds.Right);
                        CameraFound = true;
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

            if (bounds.Top > bounds.Bottom || bounds.Left > bounds.Right || !CameraFound)
            {
                Console.WriteLine("Broken Bounds");
                bounds.Top = 0;
                bounds.Left = 0;
                bounds.Bottom = lh;
                bounds.Right = lw;
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

                    if (SmartTiles.TryGetValue((int)level.data[LevelFile.ACTIVE, index], out int[] alts) && alts.Length > 0)
                    {
                        if ((int)level.data[LevelFile.ACTIVE, index] < 1000)
                        {
                            if (RNG.random.Next(0, corruptLevel) == 0)
                            {
                                try
                                {
                                    int num = alts[RNG.random.Next(0, alts.Length)];
                                    level.data[LevelFile.ACTIVE, index] = (TileID)num;
                                }
                                catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                            }
                        }
                        else
                        {
                            try
                            {
                                int num = alts[RNG.random.Next(0, options.Length)];
                                if (num == 40003 || num == 40047) { hasGas = true; }
                                level.data[LevelFile.ACTIVE, index] = (TileID)num;
                            }
                            catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                        }
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

            // loop over entire level
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    int index = i * lw + j;

                    if(SmartTiles.TryGetValue((int)level.data[LevelFile.ACTIVE, index], out int[] alts) && alts.Length > 0)
                    {
                        if (RNG.CoinFlip())
                        {
                            try
                            {
                                int num = alts[RNG.random.Next(0, options.Length)];
                                level.data[LevelFile.OVERLAY, index] = (TileID)num;
                            }
                            catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                        }
                    }
                }
            }
        }

        // Colored tile replacement routine
        public static void ReplaceColorTiles(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            // loop over entire level
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    int index = i * lw + j;

                    int tile = (int)level.data[LevelFile.ACTIVE, index];

                    //// SPECIAL RULES (these gotta go first so it all works right)

                    // BLUE Collectables



                    // BLUE CONVEYORS
                    if (tile == 100027 || tile == 100028)
                    {
                        if (ColorTiles.TryGetValue(tile, out int[] alts))
                        {
                            int num = alts[RNG.random.Next(0, alts.Length)]; // select one option for all tiles

                            var adjacents = new HashSet<int>();
                            adjacents.Add(index);

                            int numTile = 1, numTileX = 1;
                            GetAdjacentTiles(ref level, level.data[LevelFile.ACTIVE, index], BLUE_OFFSET, index, ref adjacents, ref numTile, ref numTileX);

                            int[] contiguous = new int[adjacents.Count];

                            adjacents.CopyTo(contiguous);

                            try
                            {
                                for (int k = 0; k < contiguous.Length; k++)
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = (TileID)num;
                            }
                            catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                        }
                    }

                    // BLUE TILES
                    else if (tile > BLUE_OFFSET && tile < YELLOW_OFFSET)
                    {
                        if (ColorTiles.TryGetValue(tile, out int[] alts))
                        {
                            try
                            {
                                int num = alts[RNG.random.Next(0, alts.Length)];
                                level.data[LevelFile.ACTIVE, index] = (TileID)num;
                            }
                            catch (Exception) { Console.WriteLine("Exception on TileID\n"); }
                        }
                    }

                    // YELLOW TILES
                    else if (tile > YELLOW_OFFSET && tile < YELLOW_OFFSET + 50)
                    {
                        // create hash set and get adjacent tiles
                        var adjacents = new HashSet<int>(); // create hash set for adjacents
                        adjacents.Add(index);               // add first tile to adjacents
                        
                        int numTile = 1, numTileX = 1;
                        GetAdjacentTiles(ref level, level.data[LevelFile.ACTIVE, index], YELLOW_OFFSET, index, ref adjacents, ref numTile, ref numTileX);

                        // create array from adjacents
                        int[] contiguous = new int[adjacents.Count];
                        adjacents.CopyTo(contiguous);

                        // get tile to place in "empty" spots
                        //string s = colorTiles.Element(Enum.GetName(typeof(TileID), level.data[LevelFile.ACTIVE, index])).ToString();
                        TileID noTile = TileID.None;   // default is empty tile
                        if (tile - YELLOW_OFFSET == 7)  // empty tile is solid when tile value is a hook
                            noTile = TileID.Solid;
                        //if (s != null && s != "")
                        //    noTile = (TileID)Convert.ToInt32(s);

                        // pick out the ones to make into actual tiles
                        for (int k = 0; k < numTile; )
                        {
                            int n = RNG.random.Next(0, contiguous.Length);  // get random tile from contiguous
                            if (adjacents.Remove(contiguous[n]))            // if it can be removed (i.e. hasn't been selected yet)
                            {
                                level.data[LevelFile.ACTIVE, contiguous[n]] = (TileID)(tile - YELLOW_OFFSET);  // place the tile
                                k++;                                                                // increment loop counter
                            }
                        }

                        // replace the rest with empty tiles
                        int[] alltherest = new int[adjacents.Count];
                        adjacents.CopyTo(alltherest);
                        for (int k = 0; k < alltherest.Length; k++)
                        {
                            int n = alltherest[k];
                            level.data[LevelFile.ACTIVE, n] = noTile;
                        }
                    }

                    // RED TILES
                    else if (tile > RED_OFFSET && tile < RED_OFFSET + 50)
                    {
                        var adjacents = new HashSet<int>();
                        adjacents.Add(index);

                        int numTile = 1, numTileX = 1;
                        GetAdjacentTiles(ref level, level.data[LevelFile.ACTIVE, index], RED_OFFSET, index, ref adjacents, ref numTile, ref numTileX);

                        int[] contiguous = new int[adjacents.Count];
                        adjacents.CopyTo(contiguous);

                        TileID noTile = TileID.None;   // default is empty tile
                        if (tile - RED_OFFSET == 7)  // empty tile is solid when tile value is a hook
                            noTile = TileID.Solid;

                        bool placeTile = RNG.CoinFlip();    // true = placing blanks, false = placing tiles
                        for (int k = 0; k < contiguous.Length;)
                        {
                            if (placeTile)
                            {
                                int loop = RNG.random.Next(0, numTile) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = (TileID)(tile - RED_OFFSET);  // place the next tile
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                            else
                            {
                                int loop = RNG.random.Next(0, numTileX) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = noTile;
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                        }
                    }

                    // GREEN TILES
                    else if (tile > GREEN_OFFSET && tile < GREEN_OFFSET + 50)
                    {
                        var adjacents = new HashSet<int>();
                        adjacents.Add(index);

                        int numTile = 1, numTileX = 1;
                        GetAdjacentColor(ref level, level.data[LevelFile.ACTIVE, index], GREEN_OFFSET, index, ref adjacents, ref numTile, ref numTileX);

                        int[] contiguous = new int[adjacents.Count];
                        adjacents.CopyTo(contiguous);

                        /*TileID noTile = TileID.Empty;   // default is empty tile
                        if (tile - GREEN_OFFSET == 7)   // empty tile is solid when tile value is a hook
                            noTile = TileID.Solid;*/

                        bool placeTile = RNG.CoinFlip();    // true = placing blanks, false = placing tiles
                        for (int k = 0; k < contiguous.Length;)
                        {
                            if (placeTile)
                            {
                                int loop = RNG.random.Next(0, numTile) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = (TileID)((int)level.data[LevelFile.ACTIVE, contiguous[k]] - GREEN_OFFSET);  // place the next tile
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                            else
                            {
                                int loop = RNG.random.Next(0, numTileX) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = TileID.None;
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                        }
                    }

                    // COLOR ENTITIES
                    // YELLOW
                    else if (tile >= YELLOW_OFFSET + 40000 && tile < YELLOW_OFFSET + 50000)
                    {
                        var adjacents = new HashSet<int>();
                        adjacents.Add(index);

                        int numTile = 1, numTileX = 1;
                        GetAdjacentEntity(ref level, level.data[LevelFile.ACTIVE, index], YELLOW_OFFSET, index, ref adjacents, ref numTile, ref numTileX);

                        int[] contiguous = new int[adjacents.Count];
                        adjacents.CopyTo(contiguous);
                        
                        // pick out the ones to make into actual tiles
                        for (int k = 0; k < numTile;)
                        {
                            int n = RNG.random.Next(0, contiguous.Length);  // get random tile from contiguous
                            if (adjacents.Remove(contiguous[n]))            // if it can be removed (i.e. hasn't been selected yet)
                            {
                                level.data[LevelFile.ACTIVE, contiguous[n]] = (TileID)((int)level.data[LevelFile.ACTIVE, contiguous[n]] - YELLOW_OFFSET);  // place the tile
                                k++;                                                                // increment loop counter
                            }
                        }
                        // replace the rest with empty tiles
                        int[] alltherest = new int[adjacents.Count];
                        adjacents.CopyTo(alltherest);
                        for (int k = 0; k < alltherest.Length; k++)
                        {
                            int n = alltherest[k];
                            level.data[LevelFile.ACTIVE, n] = TileID.None;
                        }
                    }
                    // RED
                    else if (tile >= RED_OFFSET + 40000 && tile < RED_OFFSET + 50000)
                    {
                        var adjacents = new HashSet<int>();
                        adjacents.Add(index);

                        int numTile = 1, numTileX = 1;
                        GetAdjacentEntity(ref level, level.data[LevelFile.ACTIVE, index], RED_OFFSET, index, ref adjacents, ref numTile, ref numTileX);
                        
                        int[] contiguous = new int[adjacents.Count];
                        adjacents.CopyTo(contiguous);

                        bool placeTile = RNG.CoinFlip();    // true = placing blanks, false = placing tiles
                        for (int k = 0; k < contiguous.Length;)
                        {
                            if (placeTile)
                            {
                                int loop = RNG.random.Next(0, numTile) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = (TileID)((int)level.data[LevelFile.ACTIVE, contiguous[k]] - RED_OFFSET);  // place the next tile
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                            else
                            {
                                int loop = RNG.random.Next(0, numTileX) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = TileID.None;
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                        }
                    }
                    // GREEN
                    else if (tile >= GREEN_OFFSET + 40000 && tile < GREEN_OFFSET + 50000)
                    {
                        var adjacents = new HashSet<int>();
                        adjacents.Add(index);
                        int numTile = 1, numTileX = 1;
                        GetAdjacentEntity(ref level, level.data[LevelFile.ACTIVE, index], GREEN_OFFSET, index, ref adjacents, ref numTile, ref numTileX);
                        int[] contiguous = new int[adjacents.Count];
                        adjacents.CopyTo(contiguous);

                        bool placeTile = RNG.CoinFlip();    // true = placing blanks, false = placing tiles
                        for (int k = 0; k < contiguous.Length;)
                        {
                            if (placeTile)
                            {
                                int loop = RNG.random.Next(0, numTile) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = (TileID)((int)level.data[LevelFile.ACTIVE, contiguous[k]] - GREEN_OFFSET);  // place the next tile
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                            else
                            {
                                int loop = RNG.random.Next(0, numTileX) + 1;
                                for (int p = 0; p < loop; p++)
                                {
                                    if (k + p >= contiguous.Length) break;
                                    level.data[LevelFile.ACTIVE, contiguous[k]] = TileID.None;
                                    k++;
                                }
                                placeTile = !placeTile;
                            }
                        }
                    }

                }
            }
        }
        public static void GetAdjacentTiles(ref LevelFile level, TileID id, int color_offset, int index, ref HashSet<int> adjacents, ref int numTile, ref int numTileX)
        {
            int lw = level.header.width;
            int lh = level.header.height;
            
            int size = lw * lh;

            // put all neighbors in array.
            int[] a = { index + 1, index - 1, index + lw, index - lw };

            // loop through neighbors
            for (int k = 0; k < 4; k++)
            {
                if ( a[k] > 0 && a[k] < size )  // check if in bounds
                {
                    int tileValue = (int)level.data[LevelFile.ACTIVE, a[k]] - color_offset;

                    if (level.data[LevelFile.ACTIVE, a[k]] == id)  // check if tile id matches
                    {
                        if (adjacents.Add(a[k]))            // add to adjacents
                            GetAdjacentTiles(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                    else if (tileValue > 90 && tileValue < 100)
                    {
                        if (adjacents.Add(a[k]))
                        {
                            numTile = Math.Max(numTile, tileValue - 90);
                            GetAdjacentTiles(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);   //look for adjecents to next set of tiles
                        }
                    }
                    else if (tileValue > 80 && tileValue < 90)  // X Number tiles
                    {
                        if (adjacents.Add(a[k]))            // add to adjacents
                        {
                            numTileX = Math.Max(numTileX, tileValue - 80);
                            GetAdjacentTiles(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);   //look for adjecents to next set of tiles
                        }
                    }
                    else if (tileValue == 0)    // null tile
                    {
                        level.data[LevelFile.ACTIVE, a[k]] = TileID.None;
                        GetAdjacentTiles(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);   //look for adjecents to next set of tiles
                    }
                }
            }
        }
        public static void GetAdjacentEntity(ref LevelFile level, TileID id, int color_offset, int index, ref HashSet<int> adjacents, ref int numTile, ref int numTileX)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            int size = lw * lh;

            // put all neighbors in array.
            int[] a = { index + 1, index - 1, index + lw, index - lw };

            // loop through neighbors
            for (int k = 0; k < 4; k++)
            {
                if (a[k] > 0 && a[k] < size)  // check if in bounds
                {
                    int tileValue = (int)level.data[LevelFile.ACTIVE, a[k]] - color_offset;

                    if (tileValue >= 40000 && tileValue < 50000)  // check if tile value is in correct range
                    {
                        if (adjacents.Add(a[k]))            // add to adjacents
                            GetAdjacentEntity(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                    else if (tileValue > 90 && tileValue < 100) // Number tiles
                    {
                        //if (adjacents.Add(a[k]))            // add to adjacents
                        level.data[LevelFile.ACTIVE, a[k]] = TileID.None;
                        numTile = Math.Max(numTile, tileValue - 90);
                        GetAdjacentEntity(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                    else if (tileValue > 80 && tileValue < 90)  // X Number tiles
                    {
                        //if (adjacents.Add(a[k]))            // add to adjacents
                        level.data[LevelFile.ACTIVE, a[k]] = TileID.None;
                        numTileX = Math.Max(numTileX, tileValue - 80);
                        GetAdjacentEntity(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                    else if (tileValue == 0)    // null tile
                    {
                        level.data[LevelFile.ACTIVE, a[k]] = TileID.None;
                        GetAdjacentEntity(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                }
            }
        }
        public static void GetAdjacentColor(ref LevelFile level, TileID id, int color_offset, int index, ref HashSet<int> adjacents, ref int numTile, ref int numTileX)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            int size = lw * lh;

            // put all neighbors in array.
            int[] a = { index + 1, index - 1, index + lw, index - lw };

            // loop through neighbors
            for (int k = 0; k < 4; k++)
            {
                if (a[k] > 0 && a[k] < size)  // check if in bounds
                {
                    int tileValue = (int)level.data[LevelFile.ACTIVE, a[k]] - color_offset;

                    if (tileValue > 0 && tileValue < 50000)  // check if tile value is in correct range
                    {
                        if (adjacents.Add(a[k]))            // add to adjacents
                            GetAdjacentColor(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                    else if (tileValue > 90 && tileValue < 100) // Number tiles
                    {
                        //if (adjacents.Add(a[k]))            // add to adjacents
                        numTile = Math.Max(numTile, tileValue - 90);
                        level.data[LevelFile.ACTIVE, a[k]] = TileID.None;
                        GetAdjacentColor(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                    else if (tileValue > 80 && tileValue < 90)  // X Number tiles
                    {
                        //if (adjacents.Add(a[k]))            // add to adjacents
                        numTile = Math.Max(numTileX, tileValue - 80);
                        level.data[LevelFile.ACTIVE, a[k]] = TileID.None;
                        GetAdjacentColor(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                    else if (tileValue == 0)    // null tile
                    {
                        level.data[LevelFile.ACTIVE, a[k]] = TileID.None;
                        GetAdjacentColor(ref level, id, color_offset, a[k], ref adjacents, ref numTile, ref numTileX);
                    }
                }
            }
        }

        

    }
}
