using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    public static class LevelGenerator
    {
        static List<LevelPiece> Pieces;
        static LevelPiece LTransition = new LevelPiece(LevelManip.Load("data/levelpieces/GEN/t-left.lvl")) { CeilingEx = false, FloorEx = true, CeilingEn = false, FloorEn = true };
        static LevelPiece RTransition = new LevelPiece(LevelManip.Load("data/levelpieces/GEN/t-right.lvl")) { CeilingEx = false, FloorEx = true, CeilingEn = false, FloorEn = true };
        static LevelPiece LTransitionC = new LevelPiece(LevelManip.Load("data/levelpieces/GEN/t-leftc.lvl")) { CeilingEx = true, FloorEx = true, CeilingEn = true, FloorEn = true };
        static LevelPiece RTransitionC = new LevelPiece(LevelManip.Load("data/levelpieces/GEN/t-rightc.lvl")) { CeilingEx = true, FloorEx = true, CeilingEn = true, FloorEn = true };

        // These are basically constants but I might want to change them in the settings
        static int UsableWidth = 48;
        static int UsableHeight = 32;
        static int MaxPieces = 3;

        // The level file for the level being created
        static LevelPiece Canvas    = new LevelPiece();
        static LevelPiece LastPiece = new LevelPiece();
        static LevelPiece NextPiece = new LevelPiece();
        static LevelFile  TempLevel = new LevelFile();

        static Bounds CameraBounds = new Bounds();

        // Level Generation Info
        static Direc EntranceDirection = Direc.None;
        static Direc ExitDirection = Direc.None;
        static int WidthRemaining;

        static void InitGenInfo()
        {
            LastPiece = new LevelPiece();
            NextPiece = new LevelPiece();
            EntranceDirection = Direc.L;
            ExitDirection = Direc.R;
            WidthRemaining = UsableWidth;

            if (RNG.CoinFlip()) { Canvas.FloorEx = true; Canvas.CeilingEx = true; };
            //CameraBounds = { }
        }

        public static LevelFile GetNewLevelFile(int width = 54, int height = 32)
        {
            LevelFile level = new LevelFile(width, height);

            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    level.data[LevelFile.TAG, index] = TileID.OOBMarker;
                }
            }

            return level;
        }
        public static void LoadPieces(MainWindow mw)    // this needs to be run when the program starts up or when the randomization starts
        {
            Pieces = new List<LevelPiece>();

            foreach (var pool in mw.PiecePools)
            {
                if (pool.Active)
                {
                    for (int i = 0; i < pool.Pieces.Count; i++)
                    {
                        var piece = pool.Pieces[i];
                        GetPieceEntrances(ref piece);
                        Pieces.Add(piece);
                    }
                }
            }
        }
        public static bool GetPieceEntrances(ref LevelPiece piece)
        {
            int index = 0;
            int lw = piece.File.header.width;
            int lh = piece.File.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;

                    switch (piece.File.data[LevelFile.TAG, index])
                    {
                        case TileID.GreenTransitionL:
                            piece.Entrance = Direc.L;
                            break;
                        case TileID.GreenTransitionD:
                            piece.Entrance = Direc.D;
                            break;
                        case TileID.YellowTransitionR:
                            piece.Exit = Direc.R;
                            break;
                        case TileID.YellowTransitionU:
                            piece.Exit = Direc.U;
                            break;
                    }
                }
            }

            if (piece.Entrance != Direc.None && piece.Exit != Direc.None)
                return true;
            else
            {
                Console.WriteLine($"Piece {piece.Name} is missing an entrance or exit.");
                return false;
            }

        }
        public static bool GetNextPiece(int maxHeight, int maxWidth)
        {
            var pool = new List<LevelPiece>();

            foreach (LevelPiece piece in Pieces)
            {
                if (piece.Name != LastPiece.Name)
                    if (EntranceDirection == Direc.Any || piece.Entrance == EntranceDirection) // entrance check
                        //if (piece.CeilingEn == Canvas.CeilingEx && piece.FloorEn == Canvas.FloorEx)
                        if (ExitDirection == Direc.Any || piece.Exit == ExitDirection)         // exit check
                            if (piece.File.header.height <= maxHeight && piece.File.header.width <= maxWidth) // piece size check
                                pool.Add(piece);
            }

            if (pool.Count == 0)
                return false;
            else
                NextPiece = pool[RNG.random.Next(0, pool.Count())];
            return true;

        }

        public static bool AttemptAddPiece()
        {
            // attempt to add a piece that does not make the level too big
            for (int a = 0; a < 5; a++) // it gives five attempts to add a piece
            {
                if (!GetNextPiece(100, WidthRemaining)) continue;   // if cannot get new piece, return false and break from adding pieces

                //if (RNG.CoinFlip())    // random chance to place the new piece on either the left or right side
                //{                                   // Since tall pieces are more likely to be picked first, this should keep them more centered, rather than always appearing at the start of a level
                    return AppendPieceH(Canvas, NextPiece);  // return true if piece is added successfully
                //}
                //else return AppendPieceH(NextPiece.File, Canvas);  // return true if piece is added successfully
            }
            return false;
        }

        public static LevelFile CreateLevel(/*Direction EnDir = Direction.Right, Direction ExDir = Direction.Right*/)   // later I will want to be able to specify the entrance and exit direction
        {

            InitGenInfo();  // Initialize the generator info

            /*if (!*/
            GetNextPiece(100, 100);
            //)    // select first piece
            //{ Console.WriteLine("null starting level"); return null; }
            Canvas = NextPiece;    // initializes canvas to first piece selected
            //Canvas.CeilingEn = NextPiece.CeilingEn;
            //Canvas.CeilingEx = NextPiece.CeilingEx;
            //Canvas.FloorEn = NextPiece.FloorEn;
            //Canvas.FloorEx = NextPiece.FloorEx;

            int numPieces = 0;
            while (true)
            {
                LastPiece = NextPiece;                                      // the old new piece becomes the new old piece
                Direc entranceDir = (Direc)(-(int)LastPiece.Exit);  // get the opposite direction of the exit
                WidthRemaining = UsableWidth - Canvas.File.header.width;         // The amount of space left before reaching the usable width limit

                if (!AttemptAddPiece()) break; // try to add a new piece. if this fails, break from the loop
                if (Canvas.File.header.height >= UsableHeight || Canvas.File.header.width >= UsableWidth) break; // perform size check

                numPieces++; // piece number check
                if (numPieces >= MaxPieces) break;
            }

            //int widthToReach = Canvas.header.height * 16 / 9;    // The width required to meet a 16:9 aspect ratio
            //int difference = widthToReach - Canvas.header.width; // If the level is too thin, we want to add more pieces.

            // Finalizing the level, Add entrance and exit landing
            if (Canvas.CeilingEn)
                AppendPieceH(LTransitionC, Canvas, true);
            else AppendPieceH(LTransition, Canvas, true);
            if (Canvas.CeilingEx)
                AppendPieceH(Canvas, RTransitionC, true);
            else AppendPieceH(Canvas, RTransition, true);



            FillEmptySpace(); // fill in top/bottom

            // add camera bounds
            //int vert = level.header.height / 2;
            //int width = level.header.width;
            //int index = vert * width + (width - 2);     // right camera tile
            //level.data[LevelFile.TAG, index] = TileID.CameraBounds;
            //index = vert * width + 1;                   // left camera tile
            //level.data[LevelFile.TAG, index] = TileID.CameraBounds;    
            // vertical camera bounds are not set, they should just naturally work out

            return Canvas.File;
        }
        static void FillEmptySpace()
        {
            int index = 0;
            int lw = Canvas.File.header.width;
            int lh = Canvas.File.header.height;

            TileID activeFillTile = TileID.None;
            TileID back1FillTile = TileID.None;
            TileID back2FillTile = TileID.None;
            TileID tagFillTile = TileID.OOBMarker;

            // j is on the outside loop here because I need to increment along columns instead of rows
            for (int j = 0; j < lw; j++)
            {
                tagFillTile = TileID.OOBMarker;

                for (int i = 0; i < lh; i++)    // move down the column replacing OOB tiles with whatever the last valid tile was
                {
                    index = i * lw + j;

                    if (Canvas.File.data[LevelFile.TAG, index] == TileID.OOBMarker)
                    {
                        if (tagFillTile != TileID.OOBMarker)
                        {
                            //Canvas.File.data[LevelFile.TAG, index]     = TileID.Empty;
                            Canvas.File.data[LevelFile.ACTIVE, index]  = activeFillTile;
                            Canvas.File.data[LevelFile.BACK1, index]   = back1FillTile;
                            Canvas.File.data[LevelFile.BACK2, index]   = back2FillTile;
                            Canvas.File.data[LevelFile.TAG, index]     = tagFillTile;
                            //if (RNG.CoinFlip())
                            //{
                            //    Canvas.File.data[LevelFile.ACTIVE, index] = TileID.Kuko;
                            //    Canvas.File.data[LevelFile.OVERLAY, index] = TileID.FakeSolidOver;
                            //}
                        }
                    }
                    else
                    {
                        activeFillTile = Canvas.File.data[LevelFile.ACTIVE, index];
                        back1FillTile = Canvas.File.data[LevelFile.BACK1, index];
                        back2FillTile = Canvas.File.data[LevelFile.BACK2, index];
                        //if (Canvas.data[LevelFile.TAG, index] != TileID.Empty)
                        tagFillTile = Canvas.File.data[LevelFile.TAG, index];
                    }
                }

                tagFillTile = TileID.OOBMarker;

                for (int i = lh - 1; i >= 0; i--)   // just do the same thing as above but going up the column. this is a lazy solution
                {
                    index = i * lw + j;

                    if (Canvas.File.data[LevelFile.TAG, index] == TileID.OOBMarker)
                    {
                        if (tagFillTile != TileID.OOBMarker)
                        {
                            Canvas.File.data[LevelFile.TAG, index] = TileID.None;
                            Canvas.File.data[LevelFile.ACTIVE, index] = activeFillTile;
                            Canvas.File.data[LevelFile.BACK1, index] = back1FillTile;
                            Canvas.File.data[LevelFile.BACK2, index] = back2FillTile;
                            Canvas.File.data[LevelFile.TAG, index] = tagFillTile;
                            //if (RNG.CoinFlip())
                            //    Canvas.File.data[LevelFile.ACTIVE, index] = TileID.Mother;
                        }
                    }
                    else
                    {
                        activeFillTile = Canvas.File.data[LevelFile.ACTIVE, index];
                        back1FillTile = Canvas.File.data[LevelFile.BACK1, index];
                        back2FillTile = Canvas.File.data[LevelFile.BACK2, index];
                        //if (Canvas.data[LevelFile.TAG, index] != TileID.Empty)
                        tagFillTile = Canvas.File.data[LevelFile.TAG, index];
                    }
                }
            }
        }
        public static Pair GetExitCoord(ref LevelFile level)
        {
            // Find Canvas exit coordinates
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level.data[LevelFile.TAG, index] == TileID.YellowTransitionR)
                        return new Pair(i, j);
                }
            }
            return new Pair();
        }
        public static Pair GetEntryCoord(ref LevelFile level)
        {
            // Find new piece's entrance coordinates
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level.data[LevelFile.TAG, index] == TileID.GreenTransitionL)
                        return new Pair(i, j);
                }
            }
            return new Pair();
        }
        static void GetLevelOrigins(Pair L1ExitCoord, Pair L2EntryCoord, ref Pair L1Origin, ref Pair L2Origin)
        {
            if (L1ExitCoord.First < L2EntryCoord.First) // if the L1 exit is higher up than the L2 entrance then L2 must be moved up to meet the L1 exit
                                                        // L2 will sit higher and thus L1 must start lower
                L1Origin.First = L2EntryCoord.First - L1ExitCoord.First;    // find vertical offset of L1 (originOffset = L2EntryCoord.height - L1ExitCoord.height)

            else                        // if the above wasn't true, then that means L1 is sitting higher than L2, and so it's vertical orgin can be set to zero.
                L1Origin.First = 0;     // I had a more complicated formula here before but it turned out to be unnecessary.

            L2Origin = L1Origin + L1ExitCoord - L2EntryCoord;   // L2 origin point = originOffset + L1ExitCoord - L2EntryCoord
            L2Origin.Second++;  // add 1 so that it is next to the L1 exit but not on top of it
        }

        static LevelPiece CheckCeilingFloor(ref LevelPiece left, ref LevelPiece right)
        {
            Pair L1ExitCoord = GetExitCoord(ref left.File);
            Pair L2EntryCoord = GetEntryCoord(ref right.File);

            int ceilingHeight = 0;
            LevelPiece transition = new LevelPiece(new LevelFile(1,3));

            if (left.CeilingEx != right.CeilingEn) // check ceiling height
            {
                TileID sidePiece = TileID.SmallSideL;
                if (left.CeilingEx)
                    ceilingHeight = GetCeilingHeight(left.File, L1ExitCoord);
                else { ceilingHeight = GetCeilingHeight(right.File, L2EntryCoord); sidePiece = TileID.SmallSideR; }
                transition = new LevelPiece(new LevelFile(1, ceilingHeight + 2));
                for (int i = 1; i < ceilingHeight; i++)
                {
                    if (RNG.CoinFlip()) transition.File.data[LevelFile.BACK1, i] = sidePiece;
                }
            }

            int lh = transition.File.header.height;
            
            if (left.FloorEx || right.FloorEn) // check floor
            {
                if (left.CeilingEx) transition.File.data[LevelFile.BACK1, lh - 2] = TileID.LargeCornerBL;
                else if (right.CeilingEn) transition.File.data[LevelFile.BACK1, lh - 2] = TileID.LargeCornerBR;
                transition.File.data[LevelFile.ACTIVE, lh - 1] = TileID.Solid; // add ground
            }

            //LevelFile tempFile = GetNewLevelFile(left.File.header.width + 1, left.File.header.height);
            //Pair tCoords = new Pair(L1ExitCoord.First - ceilingHeight, left.File.header.width - 1);

            //CopyToCoords(ref left.File, ref tempFile, new Pair(0,0));
            //CopyToCoords(ref transition.File, ref tempFile, new Pair(0,0));

            return transition;
        }

        static int GetCeilingHeight(LevelFile level, Pair coord)
        {
            int ceilingHeight = 0;
            int lw = level.header.width;
            for (ceilingHeight = 1; ceilingHeight < coord.First; ceilingHeight++)
            {
                int index = (coord.First - ceilingHeight) * lw + coord.Second;
                if (level.data[LevelFile.ACTIVE, index] == TileID.Solid) break;
            }
            return ceilingHeight;
        }

        public static bool AppendPieceH(LevelPiece left, LevelPiece right, bool noExcept = false)
        {
            Pair L1ExitCoord  = GetExitCoord(ref left.File);   // Get entry and exit coords
            Pair L2EntryCoord = GetEntryCoord(ref right.File);

            // check for mismatch in level exit/entrance
            //if (left.CeilingEx != right.CeilingEn) FixCeiling(ref left, ref right);
            //if (left.FloorEx   != right.FloorEn)   FixFloor(ref left, ref right);

            // establish level boundaries and new level size
            Pair L1Origin = new Pair(), L2Origin = new Pair(); // initialize
            GetLevelOrigins(L1ExitCoord, L2EntryCoord, ref L1Origin, ref L2Origin); // set the values in this function

            // calculate dimensions for new level
            int width = L1ExitCoord.Second + right.File.header.width - L2EntryCoord.Second + 1;
            int height = Math.Max(L1Origin.First + left.File.header.height, L2Origin.First + right.File.header.height);

            // check for and correct ceiling and floor compatibility
            LevelPiece transition = new LevelPiece(new LevelFile(0,0)); Pair TOrigin = new Pair();
            if (left.CeilingEx != right.CeilingEn || left.FloorEx != right.FloorEn)
            {
                transition = CheckCeilingFloor(ref left, ref right);
                TOrigin.First = L1Origin.First + L1ExitCoord.First - transition.File.header.height + 2;
                TOrigin.Second = L2Origin.Second;
                width++; L2Origin.Second++;
            }

            // noExcept is used to make sure that the entrances and exits
            // can be added without triggering the size limitation
            if (!noExcept && (height > UsableHeight || width > UsableWidth))
                return false;

            TempLevel = GetNewLevelFile(width, height);             // create new level
            CopyToCoords(ref left.File,  ref TempLevel, L1Origin);  // copy left  level into new level
            if (transition.File.header.height != 0)
                CopyToCoords(ref transition.File, ref TempLevel, TOrigin);
            CopyToCoords(ref right.File, ref TempLevel, L2Origin);  // copy right level into new level

            // get rid of old entrances
            int index = (L1Origin.First + L1ExitCoord.First) * TempLevel.header.width + (L1Origin.Second + L1ExitCoord.Second);
            TempLevel.data[LevelFile.TAG, index] = TileID.None;
            index = (L2Origin.First + L2EntryCoord.First) * TempLevel.header.width + (L2Origin.Second + L2EntryCoord.Second);
            TempLevel.data[LevelFile.TAG, index] = TileID.None;

            // set canvas piece info
            Canvas.File = TempLevel;
            Canvas.FloorEn = left.FloorEn;
            Canvas.FloorEx = right.FloorEx;
            Canvas.CeilingEn = left.CeilingEn;
            Canvas.CeilingEx = right.CeilingEx;

            return true;
        }
        public static void CopyToCoords(ref LevelFile copyLevel, ref LevelFile pasteLevel, Pair coords)    // pass in the origin coords of the level to be copied from
        {
            int copyIndex = 0;
            int pasteIndex = 0;
            int copylw = copyLevel.header.width;
            int copylh = copyLevel.header.height;
            int pastelw = pasteLevel.header.width;
            int pastelh = pasteLevel.header.height;
            for (int i = 0; i < copylh; i++)
            {
                for (int j = 0; j < copylw; j++)
                {
                    copyIndex = i * copylw + j;
                    pasteIndex = (i + coords.First) * pastelw + (j + coords.Second);

                    if (pasteLevel.data[LevelFile.TAG, pasteIndex] != TileID.OOBMarker)
                        throw new LevelCollisionException();

                    pasteLevel.data[LevelFile.ACTIVE,  pasteIndex] = copyLevel.data[LevelFile.ACTIVE,  copyIndex];
                    pasteLevel.data[LevelFile.BACK1,   pasteIndex] = copyLevel.data[LevelFile.BACK1,   copyIndex];
                    pasteLevel.data[LevelFile.BACK2,   pasteIndex] = copyLevel.data[LevelFile.BACK2,   copyIndex];
                    pasteLevel.data[LevelFile.TAG,     pasteIndex] = copyLevel.data[LevelFile.TAG,     copyIndex];
                    pasteLevel.data[LevelFile.OVERLAY, pasteIndex] = copyLevel.data[LevelFile.OVERLAY, copyIndex];
                }
            }
        }
    }
}
