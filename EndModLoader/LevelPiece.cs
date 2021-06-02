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
        public bool HasCeiling = true;
        public bool HasFloor = true;

        // Determines whether pieces can be placed above or below a certain piece.
        public bool AllowBuildingAbove = true;
        public bool AllowBuildingBelow = true;

        // The direction the entrance and exit tiles are facing. 
        // Later on these should be replaced by a list of all entrances and exits to a piece.
        public Direction Entrance = Direction.None;
        public Direction Exit = Direction.None;

        // These margin values tell the generator how many tiles on each side can be ignored when placing camera bounds.
        public int MarginTop = 0;
        public int MarginBottom = 0;
        public int MarginLeft = 0;
        public int MarginRight = 0;

        // This is the offset of the exit as compared to the entrance
        //public int hChange = 0;
        //public int vChange = 0;


    }
}
