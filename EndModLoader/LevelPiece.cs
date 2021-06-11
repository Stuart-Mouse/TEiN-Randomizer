using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public class LevelPiece
    {
        // The level file associated with the level piece.
        public LevelFile File;

        // Basic identifying information. Used to find the level file.
        public string Name;
        public string Folder;

        // Whether or not the piece has a ceiling / floor.
        public bool CeilingEn = false;
        public bool CeilingEx = false;

        public bool FloorEn = false;
        public bool FloorEx = false;

        public bool FullHeight = false;

        // Determines whether pieces can be placed above or below a certain piece.
        public bool AllowBuildingAbove = true;
        public bool AllowBuildingBelow = true;

        // The direction the entrance and exit tiles are facing. 
        // Later on these should be replaced by a list of all entrances and exits to a piece.
        public Direction Entrance = Direction.None;
        public Direction Exit     = Direction.None;

        // The margin values tell the generator how many tiles on each side can be ignored when placing camera bounds.
        public Bounds Margin;

        // This is the offset of the exit as compared to the entrance
        //public Pair Shift;
        
        public LevelPiece()
        {
            File = new LevelFile();

            Margin.Left = 0;
            Margin.Right = 0;
            Margin.Top = 0;
            Margin.Bottom = 0;
        }

        public LevelPiece(LevelFile file)
        {
            File = file;

            Margin.Left = 0;
            Margin.Right = 0;
            Margin.Top = 0;
            Margin.Bottom = 0;
        }
    }
}
