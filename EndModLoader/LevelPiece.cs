using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public struct LevelPiece
    {
        // The level file associated with the level piece.
        public LevelFile File;

        // Basic identifying information. Used to find the level file.
        public string Name;
        public string Folder;

        // Whether or not the piece has a ceiling / floor.
        public bool CeilingEn;
        public bool CeilingEx;

        public bool FloorEn;
        public bool FloorEx;

        public bool FullHeight;

        // Determines whether pieces can be placed above or below a certain piece.
        public bool AllowBuildingAbove;
        public bool AllowBuildingBelow;

        // The direction the entrance and exit tiles are facing. 
        // Later on these should be replaced by a list of all entrances and exits to a piece.
        public Direction Entrance;
        public Direction Exit;

        // The margin values tell the generator how many tiles on each side can be ignored when placing camera bounds.
        public Bounds Margin;

        // This is the offset of the exit as compared to the entrance
        //public Pair Shift;

        public LevelPiece(LevelFile file)
        {
            File = file;

            Name = null;
            Folder = null;

            Margin.Left = 0;
            Margin.Right = 0;
            Margin.Top = 0;
            Margin.Bottom = 0;

            CeilingEn = false;
            CeilingEx = false;

            FloorEn = false;
            FloorEx = false;

            FullHeight = false;

            // Determines whether pieces can be placed above or below a certain piece.
            AllowBuildingAbove = true;
            AllowBuildingBelow = true;

            // The direction the entrance and exit tiles are facing. 
            // Later on these should be replaced by a list of all entrances and exits to a piece.
            Entrance = Direction.None;
            Exit = Direction.None;
        }
    }
}
