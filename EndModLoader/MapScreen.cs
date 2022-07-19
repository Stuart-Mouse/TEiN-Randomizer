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
        public string ID;

        public string LevelSuffix {
            get => ID.Split('-')[1];
        }
        
        // The type of the MapScreen
        // Can be level, connector, dots, etc.
        public ScreenType Type;
        // The level located at the MapScreen location
        public Level Level;

        // Used for storing information on the size of each transition
        //public int[] TransitionSizes = { 0, 0, 0, 0 };

        // This is a string of the character U, D, L, R that specify the directions to the level
        public string Directions;
        // This is the distance from the start of the area to the MapScreen
        public int Distance { get => Directions.Length; }

        public MapScreen(string id, ScreenType type, Level level, string directions = "")
        {
            ID = id;
            Type = type;
            Level = level;
            Directions = directions;
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
