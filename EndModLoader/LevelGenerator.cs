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
        static LevelFile TLeft = LevelManip.Load("data/levelpieces/Transitions/t-left.lvl");
        static LevelFile TRight = LevelManip.Load("data/levelpieces/Transitions/t-right.lvl");
        static GenInfo genInfo;

        static int UsableWidth = 48;
        static int UsableHeight = 30;
        static int MaxPieces = 8;


        public struct GenInfo
        {
            public int hClearance;
            public int vClearance;
            public Pair currEntryCoord;
            public Pair currExitCoord;
        }

        static void InitGenInfo()
        {
            // initialize vars for usable piece size
            genInfo.hClearance = UsableWidth;
            genInfo.vClearance = UsableHeight;
            //genInfo.currExitCoord = new Pair();
            //genInfo.currEntryCoord = new Pair();
        }

        static LevelFile GetNewLevelFile(int width = 54, int height = 32)
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
                    level.data.tag[index] = TileID.OOBMarker;
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

                    switch (piece.File.data.tag[index])
                    {
                        case TileID.GreenTransitionL:
                            piece.Entrance = Direction.Left;
                            break;
                        case TileID.GreenTransitionD:
                            piece.Entrance = Direction.Down;
                            break;
                        case TileID.YellowTransitionR:
                            piece.Exit = Direction.Right;
                            break;
                        case TileID.YellowTransitionU:
                            piece.Exit = Direction.Up;
                            break;
                    }
                }
            }

            if (piece.Entrance != Direction.None && piece.Exit != Direction.None)
                return true;
            else
            {
                Console.WriteLine($"Piece {piece.Name} is missing an entrance or exit.");
                return false;
            }

        }

        public static LevelPiece GetPiece(Direction entranceDir = Direction.Any, Direction exitDir = Direction.Any, int maxHeight = 100, int maxWidth = 100, string badName = null)
        {
            var pool = new List<LevelPiece>();
            
            foreach (LevelPiece piece in Pieces)
            {
                if (piece.Name != badName)
                    if (entranceDir == Direction.Any || piece.Entrance == entranceDir)  // entrance check
                        if (exitDir == Direction.Any || piece.Exit == exitDir)          // exit check
                            if (piece.File.header.height <= maxHeight && piece.File.header.width <= maxWidth)   // piece size check
                                pool.Add(piece);
            }

            var npiece = pool[Randomizer.myRNG.rand.Next(0, pool.Count())];

            return npiece;   // return a random valid piece
        }

        public static LevelFile CreateLevel()
        {
            // select first piece
            var lastPiece = new LevelPiece();
            var nextPiece = GetPiece(Direction.Left, Direction.Right, UsableHeight, UsableWidth);
            var level = nextPiece.File;

            for (int i = 0; i < MaxPieces; i++)
            {
                lastPiece = nextPiece;
                Direction entranceDir = (Direction)(-(int)lastPiece.Exit); // get the opposite direction of the exit
                nextPiece = GetPiece(entranceDir, Direction.Right, 100, 100, lastPiece.Name);
                
                // attempt to add a piece that does not make the level too big
                for (int attempt = 0; attempt < 5;)
                {
                    var tuple = AppendPiece(level, nextPiece.File);
                    if (tuple.Item1)
                    { level = tuple.Item2; break; }
                    else attempt++;
                }

                // perform size check
                if (level.header.height >= UsableHeight || level.header.width >= UsableWidth)
                    break;
            }

            // Finalizing the level
            // add entrance and exit landing
            level = AppendPiece(TLeft, level, true).Item2;
            level = AppendPiece(level, TRight, true).Item2;
            // fill in top/bottom
            FillEmptySpace(ref level);

            return level;
        }

        static void FillEmptySpace(ref LevelFile level)
        {
            int index = 0;
            int lw = level.header.width;
            int lh = level.header.height;

            TileID activeFillTile   = TileID.Empty;
            TileID back1FillTile    = TileID.Empty;
            TileID back2FillTile    = TileID.Empty;
            TileID tagFillTile      = TileID.OOBMarker;

            // j is on the outside loop here because I need to increment along columns instead of rows
            for (int j = 0; j < lw; j++)
            {
                tagFillTile = TileID.OOBMarker;

                for (int i = 0; i < lh; i++)    // move down the column replacing OOB tiles with whatever the last valid tile was
                {
                    index = i * lw + j;

                    if (level.data.tag[index] == TileID.OOBMarker)
                    {
                        if (tagFillTile != TileID.OOBMarker)
                        {
                            level.data.tag[index]       = TileID.Empty;
                            level.data.active[index]    = activeFillTile;
                            level.data.back1[index]     = back1FillTile;
                            level.data.back2[index]     = back2FillTile;
                            level.data.tag[index]       = tagFillTile;
                        }
                    }
                    else
                    {
                        activeFillTile  = level.data.active[index];
                        back1FillTile   = level.data.back1[index];
                        back2FillTile   = level.data.back2[index];
                        tagFillTile     = level.data.tag[index];
                    }
                }

                tagFillTile = TileID.OOBMarker;

                for (int i = lh - 1; i >= 0; i--)   // just do the same thing as above but going up the column. this is a lazy solution
                {
                    index = i * lw + j;

                    if (level.data.tag[index] == TileID.OOBMarker)
                    {
                        if (tagFillTile != TileID.OOBMarker)
                        {
                            level.data.tag[index] = TileID.Empty;
                            level.data.active[index] = activeFillTile;
                            level.data.back1[index] = back1FillTile;
                            level.data.back2[index] = back2FillTile;
                            level.data.tag[index] = tagFillTile;
                        }
                    }
                    else
                    {
                        activeFillTile = level.data.active[index];
                        back1FillTile = level.data.back1[index];
                        back2FillTile = level.data.back2[index];
                        tagFillTile = level.data.tag[index];
                    }
                }
            }
        }

        public static Tuple<bool,LevelFile> AppendPiece(LevelFile level1, LevelFile level2, bool noExcept = false)
        {
            Pair L1ExitCoord = new Pair(0, 0), L2EntryCoord = new Pair(0, 0);

            int index = 0;
            int lw = level1.header.width;
            int lh = level1.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level1.data.tag[index] == TileID.YellowTransitionR)
                    { L1ExitCoord.First = i; L1ExitCoord.Second = j; }
                }
            }

            lw = level2.header.width;
            lh = level2.header.height;
            for (int i = 0; i < lh; i++)
            {
                for (int j = 0; j < lw; j++)
                {
                    index = i * lw + j;
                    if (level2.data.tag[index] == TileID.GreenTransitionL)
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
                L1Origin.First = 0;    // this is (was) somehow wrong. it is creating levels that are way too tall (setting equal to zero seems to work now?)
            }
            L2Origin = L1Origin + L1ExitCoord - L2EntryCoord;
            L2Origin.Second++;

            //Console.WriteLine($"L1ExitCoord: {L1ExitCoord.First}, {L1ExitCoord.Second}");
            //Console.WriteLine($"L2EntryCoord: {L2EntryCoord.First}, {L2EntryCoord.Second}");
            //Console.WriteLine($"L1Origin: {L1Origin.First}, {L1Origin.Second}");
            //Console.WriteLine($"L2Origin: {L2Origin.First}, {L2Origin.Second}");

            // create new level
            // dimensions calculated based on entrances, exits, level sizes
            int width = L1ExitCoord.Second + level2.header.width - L2EntryCoord.Second + 1;
            int height = Math.Max(L1Origin.First + level1.header.height, L2Origin.First + level2.header.height);

            if (!noExcept && (height > UsableHeight || width > UsableWidth))
            {
                return new Tuple<bool, LevelFile>(false, level1);
            }

            var levelNew = GetNewLevelFile(width, height);

            //Console.WriteLine($"New Level Height: {height}");
            //Console.WriteLine($"New Level Width: {width}");

            // copy first level into new level
            int copyIndex = 0;
            int pasteIndex = 0;
            int copylw = level1.header.width;
            int copylh = level1.header.height;
            int pastelw = levelNew.header.width;
            int pastelh = levelNew.header.height;
            for (int i = 0; i < copylh; i++)
            {
                for (int j = 0; j < L1ExitCoord.Second + 1; j++)
                {
                    copyIndex = i * copylw + j;
                    pasteIndex = (i + L1Origin.First) * pastelw + (j + L1Origin.Second);

                    if (levelNew.data.tag[pasteIndex] != TileID.OOBMarker)
                        throw new LevelCollisionException();

                    levelNew.data.active[pasteIndex] = level1.data.active[copyIndex];
                    levelNew.data.back1[pasteIndex] = level1.data.back1[copyIndex];
                    levelNew.data.back2[pasteIndex] = level1.data.back2[copyIndex];
                    levelNew.data.tag[pasteIndex] = level1.data.tag[copyIndex];
                    levelNew.data.overlay[pasteIndex] = level1.data.overlay[copyIndex];
                }
            }

            // copy second level into new level
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

                    if (levelNew.data.tag[pasteIndex] != TileID.OOBMarker)
                        throw new LevelCollisionException();

                    levelNew.data.active[pasteIndex] = level2.data.active[copyIndex];
                    levelNew.data.back1[pasteIndex] = level2.data.back1[copyIndex];
                    levelNew.data.back2[pasteIndex] = level2.data.back2[copyIndex];
                    levelNew.data.tag[pasteIndex] = level2.data.tag[copyIndex];
                    levelNew.data.overlay[pasteIndex] = level2.data.overlay[copyIndex];
                }
            }

            // get rid of old entrances
            index = (L1Origin.First + L1ExitCoord.First) * pastelw + (L1Origin.Second + L1ExitCoord.Second);
            levelNew.data.tag[index] = TileID.Empty;

            index = (L2Origin.First + L2EntryCoord.First) * pastelw + (L2Origin.Second + L2EntryCoord.Second);
            levelNew.data.tag[index] = TileID.Empty;

            return new Tuple<bool, LevelFile>(true, levelNew);
        }

    }
}
