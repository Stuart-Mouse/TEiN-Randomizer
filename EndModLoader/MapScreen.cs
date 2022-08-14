using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public class MapScreen
    {
        // The ID of the MapScreen
        // This will be used as the level filename when saving the level
        public string AreaID;
        public string ScreenID;

        public string FullID => AreaID != null ? $"{AreaID}-{ScreenID}" : ScreenID;

        // The type of the MapScreen
        // Can be level, connector, dots, etc.
        public ScreenType Type;
        // The level located at the MapScreen location
        public Level Level;

        // Used for storing information on the size of each transition
        //public int[] TransitionSizes = { 0, 0, 0, 0 };

        // Used in level cleaning / decolorization
        public Collectables Collectables   = Collectables.None; // The collectables to place
        public Directions   BlockEntrances = Directions.None;   // The entrances to block off

        // can be used to replace any tileid with another tileid when cleaning level
        // created for replacing horizontal transitions with vertical ones where necessary
        public Dictionary<TileID, TileID> TileSwaps = new Dictionary<TileID, TileID>();

        // This is a string of the character U, D, L, R that specify the directions to the level
        public string PathTrace;
        // This is the distance from the start of the area to the MapScreen
        public int Distance { get => PathTrace.Length; }

        public MapScreen(string area_id, string screen_id, ScreenType type, Level level, string pathtrace = "", Collectables collectables = Collectables.None)
        {
            AreaID = area_id;
            ScreenID = screen_id;
            Type = type;
            Level = level;
            PathTrace = pathtrace;
            Collectables = collectables;
            if (Level != null)
                TileSwaps = Level.TileSwaps;
        }
    }
    public enum ScreenType
    {
        // Level Types
        Level,
        Connector,
        Secret,
        Dots,
    }
}
