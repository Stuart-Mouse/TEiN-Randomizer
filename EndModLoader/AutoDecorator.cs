using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public static class AutoDecorator
    {
        public static void DecorateMachine(ref LevelFile level)
        {
            int lw = level.header.width;
            int lh = level.header.height;

            //TileID[] brokenWindows  = { TileID.DiagonalBL, TileID.DiagonalBR, TileID.SmallSideB, TileID.LargeSideB };
            //TileID[] brokenWindows2 = {  };

            // loop over entire level
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    int index = i * lw + j;
                    int choice = 0;

                    // BACKGROUND 1 TILES
                    {
                        TileID tile = level.data.back1[index];
                        TileID toPlace;
                        // decorate windows
                        if (tile == TileID.Back1Deco2 && (level.data.back1[index - lw] == TileID.WholePiece || level.data.back1[index - lw] == TileID.Back1Deco2)) // this if statement could cause a vector error if you place a window on the first row. So just don't do that!
                        {
                            choice = RNG.random.Next(0, 10); // make a random number selection to decide how to modify the window
                            switch (choice)
                            {
                                case 0:
                                    level.data.back1[index] = TileID.Back1Deco4;
                                    break;

                                case 1:
                                    level.data.back1[index] = (TileID)RNG.random.Next(50001, 50021);
                                    break;
                                case 2:
                                case 3:
                                    level.data.back2[index] = (TileID)RNG.random.Next(50034, 50054);
                                    break;
                            }
                        }
                        // decorate remaining solid tiles
                        if (tile == TileID.WholePiece)
                        {
                            choice = RNG.random.Next(0, 20);
                            if (choice == 0)
                                level.data.back1[index] = TileID.Back1Deco1;
                            else if (choice < 4)
                                level.data.back1[index] = TileID.Back1Deco3;
                        }
                        // decorate side pieces
                        if (tile == TileID.LargeSideL)
                        {
                            choice = RNG.random.Next(0, 6); // make a random number selection to decide how to modify the window
                            switch (choice)
                            {
                                case 0:
                                    break;
                                case 1:
                                case 2:
                                    level.data.back1[index] = TileID.SmallSideL;
                                    break;
                                case 3:
                                    level.data.back1[index - 1] = (TileID)RNG.random.Next(50013, 50017);
                                    goto default;
                                default:
                                    level.data.back1[index] = TileID.Empty;
                                    break;
                            }
                        }
                        if (tile == TileID.LargeSideR)
                        {
                            choice = RNG.random.Next(0, 6); // make a random number selection to decide how to modify the window
                            switch (choice)
                            {
                                case 0:
                                    break;
                                case 1:
                                case 2:
                                    level.data.back1[index] = TileID.SmallSideR;
                                    break;
                                /*case 3:
                                    level.data.back1[index + 1] = (TileID)RNG.random.Next(50013, 50017);
                                    goto default;*/
                                default:
                                    level.data.back1[index] = TileID.Empty;
                                    break;
                            }
                        }
                        if (tile == TileID.LargeSideB)
                        {
                            choice = RNG.random.Next(0, 6); // make a random number selection to decide how to modify the window
                            switch (choice)
                            {
                                case 0:
                                    break;
                                case 1:
                                case 2:
                                    level.data.back1[index] = TileID.SmallSideB;
                                    break;
                                /*case 3:
                                    level.data.back1[index + lw] = (TileID)RNG.random.Next(50013, 50017);
                                    goto default;*/
                                default:
                                    level.data.back1[index] = TileID.Empty;
                                    break;
                            }
                        }
                        if (tile == TileID.LargeSideT)
                        {
                            choice = RNG.random.Next(0, 6); // make a random number selection to decide how to modify the window
                            switch (choice)
                            {
                                case 0:
                                    break;
                                case 1:
                                case 2:
                                    level.data.back1[index] = TileID.SmallSideT;
                                    break;
                                case 3:
                                    level.data.back1[index] = TileID.Back1Deco4;
                                    break;
                                /*case 4:
                                    level.data.back1[index - lw] = (TileID)RNG.random.Next(50013, 50017);
                                    goto default;*/
                                default:
                                    level.data.back1[index] = TileID.Empty;
                                    break;
                            }
                        }

                    }

                    // BACKGROUND 2 TILES
                    {
                        TileID tile = level.data.back2[index];
                        // decorate windows
                        if (tile == TileID.Back2Deco2 && (level.data.back2[index - lw] == TileID.WholePiece2 || level.data.back2[index - lw] == TileID.Back2Deco2)) // this if statement could cause a vector error if you place a window on the first row. So just don't do that!
                        {
                            choice = RNG.random.Next(0, 10);
                            switch (choice)
                            {
                                case 0:
                                    level.data.back2[index] = TileID.Back2Deco4;
                                    break;

                                case 1:
                                    level.data.back2[index] = (TileID)RNG.random.Next(50034, 50054);
                                    break;
                            }
                        }
                        // decorate remaining solid tiles
                        if (tile == TileID.WholePiece2)
                        {
                            choice = RNG.random.Next(0, 20);
                            if (choice == 0)
                                level.data.back2[index] = TileID.Back2Deco1;
                            else if (choice < 4)
                                level.data.back2[index] = TileID.Back2Deco3;
                        }
                    }


                }
            }
            
        }
    }
}
