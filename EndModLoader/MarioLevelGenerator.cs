using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TEiNRandomizer
{
    public static class MarioLevelGenerator
    {
        public static void SaveToCSV(LevelFile level, string path)
        {
            // Open streamwriter for output file
            using (StreamWriter sw = File.CreateText(path))
            {
                int index;
                int lw = level.header.width;
                int lh = level.header.height;

                // Iterate over tiles in level
                for (int row = 0; row < level.header.height; row++)
                {
                    for (int col = 0; col < level.header.width; col++)
                    {
                        index = (row * lw) + col;
                        sw.Write((int)level.data[LevelFile.ACTIVE, index]);        // Print the integer value of every tile to the csv
                    }
                    sw.Write("\n");
                }
            }
        }
        public static LevelFile LoadFromCSV()
        {
            LevelFile level = new LevelFile();

            return level;
        }

        public static void ChopAndSave()
        {
            LevelFile level = CreateLevel(2048, 16);

            int lw = level.header.width;
            int lh = level.header.height;

            // Save a new 16x16 level made from every 8th block index in long level
            for (int col = 0; col < lw-32; col += 8)
            {
                // Create clip level
                LevelFile clip = new LevelFile(16, 16);

                // Copy from indices to clip level
                Pair coords = new Pair(0, col);
                Pair size   = new Pair(16, 16);
                LevelManip.CopyLevelClip(ref level, ref clip, coords, size);

                // Save levels with randomized name
                LevelManip.Save(clip, "C:/Users/Noah/Sync/Artificial Intelligence/Mario Level GAN/Training Data/LVL/" + RNG.GetUInt32().ToString() + ".lvl");
                LevelManip.Save(level, "C:/Users/Noah/Sync/Artificial Intelligence/Mario Level GAN/" + "mario.lvl");
            }


        }
        public static LevelFile CreateLevel(int lw, int lh)
        {
            // Create the new level
            LevelFile level = new LevelFile(lw, lh);

            // Create level column iterator
            int col = 0;

            // place ground across entire level
            {
                for (int i = 0; i < lw; i++)              // Iterate over columns
                {

                    int index = (lh - 1) * lw + i;        // Set index to col, bottom row
                    level.data[LevelFile.ACTIVE, index] = (TileID)1;   // Place ground at index
                    index = (lh - 2) * lw + i;            // Set index to col, second-to-bottom row
                    level.data[LevelFile.ACTIVE, index] = (TileID)1;   // Place ground at index
                }
            }


            // Iterate over the columns in the level (until we are close to the end, the last few columns will probably be unused) 
            while (col < lw-24)
            {
                // Structures to use in generation will be based on structures used in the original games

                // Select random choice from the structures below
                int choice = RNG.random.Next(0, 11);

                int index;

                // leave a little room between all elements
                col += RNG.random.Next(1, 7);

                // PLACE LEVEL ELEMENTS

                // Pits  (1-7 blocks)
                if (RNG.Percent(20))
                {
                    int size = RNG.random.Next(1, 8);           // Get length of pit
                    for (int i = 0; i < size; i++)              // Iterate over columns
                    {

                        index = (lh - 1) * lw + col;            // Set index to col, bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index
                        index = (lh - 2) * lw + col;            // Set index to col, second-to-bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index

                        if (i != 0 && i != size-1)
                        {
                            index = 10 * lw + col;                  // Set index
                            level.data[LevelFile.ACTIVE, index] = (TileID)11;   // Place coin at index
                        }
                        
                        col++;
                    }
                    continue;
                }

                // Pipes (2-5 blocks tall, or 3 blocks tall and 3-5 blocks above the ground)
                if (RNG.Percent(40))
                {
                    // get pipe height 2-5 (subtract 1 bc of lid)
                    int height = RNG.random.Next(1, 5);

                    // loop for height of pipe
                    for (int i = 0; i < height; i++)
                    {
                        // Place ground blocks below pipe
                        index = (lh - 3 - i) * lw + col;        // left pipe
                        level.data[LevelFile.ACTIVE, index] = (TileID)7;   // 
                        index = (lh - 3 - i) * lw + (col + 1);  // right pipe
                        level.data[LevelFile.ACTIVE, index] = (TileID)8;   // 
                    }

                    // place pipe lid
                    index = (lh - 3 - height) * lw + col;       // left pipe
                    level.data[LevelFile.ACTIVE, index] = (TileID)9;       // 
                    index = (lh - 3 - height) * lw + (col + 1); // right pipe
                    level.data[LevelFile.ACTIVE, index] = (TileID)10;      // 

                    // Increment column iterator by 2
                    col += 2;
                    continue;

                }

                // Floating blocks
                if (RNG.Percent(50))
                {
                    // Random lines of 1-10 brick blocks (chance to be a ? block instead)

                    // get brick length
                    int length = RNG.random.Next(1, 11);

                    // get if multiple rows
                    bool topRow = RNG.Percent(20);

                    for (int i = 0; i < length; i++)
                    {
                        // Place block
                        int block = 3;                  // brick block
                        if (RNG.Percent(20)) block = 4; // change some to question mark block
                        index = (lh - 6) * lw + col;    // set index
                        level.data[LevelFile.ACTIVE, index] = (TileID)block;   // place block
                        if (topRow)
                        {
                            block = 3;                  // brick block
                            if (RNG.Percent(20)) block = 4; // change some to question mark block
                            index = (lh - 10) * lw + col;    // set index
                            level.data[LevelFile.ACTIVE, index] = (TileID)block;   // place block
                        }
                        col++;
                    }

                    // More complex shapes could be drawn from files
                    continue;

                }

                // Solid Block Walls
                //if (RNG.Percent(50))
                //{
                //    // get height of wall (can be 1 to 4 tall)
                //    int height = RNG.random.Next(3, 7);

                //    for (int i = height; i > 2; i--)
                //    {
                //        index = (lh - i) * lw + col;            // set index
                //        level.data[LevelFile.ACTIVE, index] = (TileID)2;   // place block
                //    }
                //    col++;
                //    continue;
                //}

                // Platforms
                if (RNG.Percent(60))
                {
                    // get height of platform
                    int height = RNG.random.Next(10, 14);
                    int width  = RNG.random.Next(3, 8);

                    // place small pit and left edge
                    {
                        int size = RNG.random.Next(1, 4);
                        for (int i = 0; i < size; i++)              // Iterate over columns
                        {

                            index = (lh - 1) * lw + col;            // Set index to col, bottom row
                            level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index
                            index = (lh - 2) * lw + col;            // Set index to col, second-to-bottom row
                            level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index
                            col++;
                        }
                        index = (lh - 1) * lw + col;                // Set index to col, bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)0;       // Place pit at index
                        index = (lh - 2) * lw + col;                // Set index to col, second-to-bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)0;       // Place pit at index
                        index = height * lw + col;                  // Set index to col, bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)5;       // Place platform at index
                        col++;
                    }

                    // place tiles for width of platform
                    {
                        for (int i = 0; i < width; i++)                 // Iterate over columns
                        {
                            index = height * lw + col;                  // Set index to col, bottom row
                            level.data[LevelFile.ACTIVE, index] = (TileID)5;       // Place platform at index
                            for (int k = height + 1; k < 16; k++)
                            {
                                index = k * lw + col;                   // Set index to col, bottom row
                                level.data[LevelFile.ACTIVE, index] = (TileID)6;   // Place bg at index
                            }

                            // chance to place block above platform
                            if (RNG.Percent(10))
                            {
                                index = (height-4) * lw + col;          // Set index to col, bottom row
                                level.data[LevelFile.ACTIVE, index] = (TileID)RNG.random.Next(3,5);   // Place bg at index
                            }

                            col++;
                        }
                    }

                    // place right edge and small pit
                    {
                        index = (lh - 1) * lw + col;                // Set index to col, bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)0;       // Place pit at index
                        index = (lh - 2) * lw + col;                // Set index to col, second-to-bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)0;       // Place pit at index
                        index = height * lw + col;                  // Set index to col, bottom row
                        level.data[LevelFile.ACTIVE, index] = (TileID)5;       // Place platform at index
                        col++;
                        int size = RNG.random.Next(1, 4);
                        for (int i = 0; i < size; i++)              // Iterate over columns
                        {

                            index = (lh - 1) * lw + col;            // Set index to col, bottom row
                            level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index
                            index = (lh - 2) * lw + col;            // Set index to col, second-to-bottom row
                            level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index
                            col++;
                        }
                    }
                    continue;
                }

                // Solid Block Stairs
                if (RNG.Percent(70))
                {
                    // leave a little extra room for stairs
                    col += RNG.random.Next(2, 5);

                    // get height of stairs
                    int height = 3;

                    // ascending stairs
                    int incr = 1;
                    int j = 3;

                    for (int i = 0; i < height; i++)
                    {
                        for (int k = j; k > 2; k--)     // loop from block height (j) to ground (2)
                        {
                            index = (lh - k) * lw + col;            // set index
                            level.data[LevelFile.ACTIVE, index] = (TileID)2;   // place block
                        }
                        j += incr;
                        col++;
                    }
                            
                    // place gap between stairs
                    {
                        int size = RNG.random.Next(2,6);
                        for (int i = 0; i < size; i++)              // Iterate over columns
                        {

                            index = (lh - 1) * lw + col;            // Set index to col, bottom row
                            level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index
                            index = (lh - 2) * lw + col;            // Set index to col, second-to-bottom row
                            level.data[LevelFile.ACTIVE, index] = (TileID)0;   // Place pit at index
                            col++;
                        }
                    }
                            
                    // descending stairs
                    incr = -1;
                    j = height + 2;

                    for (int i = 0; i < height; i++)
                    {
                        for (int k = j; k > 2; k--)     // loop from block height (j) to ground (2)
                        {
                            index = (lh - k) * lw + col;            // set index
                            level.data[LevelFile.ACTIVE, index] = (TileID)2;   // place block
                        }
                        j += incr;
                        col++;
                    }

                    // leave room again
                    col += RNG.random.Next(2, 5);

                    continue;
                }

            } // End main while


            return level;
        }

    }
}
