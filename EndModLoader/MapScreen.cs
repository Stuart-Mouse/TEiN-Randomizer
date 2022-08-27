using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public class MapScreen
    {
        public string FullID => AreaID != null ? $"{AreaID}-{ScreenID}" : ScreenID;
        public string AreaID;
        public string ScreenID;

        public Level          Level;
        public ScreenType     Type;
        public MapConnections MapConnections;
        public Collectables   Collectables   = Collectables.None;
        public Directions     BlockEntrances = Directions.None;
        public Dictionary<TileID, TileID> TileSwaps = new Dictionary<TileID, TileID>();

        public int    Distance { get => PathTrace.Length; }
        public string PathTrace;

        public string DebugNotes = "";

        public MapScreen() { }
        public MapScreen(string area_id, string screen_id, ScreenType type, Level level, MapConnections map_connections, Dictionary<TileID, TileID> tile_swaps = null, string pathtrace = "", Collectables collectables = Collectables.None)
        {
            AreaID         = area_id;
            ScreenID       = screen_id;
            Type           = type;
            Level          = level;
            PathTrace      = pathtrace;
            MapConnections = map_connections;
            TileSwaps      = tile_swaps;
            Collectables   = collectables;
        }
    }
    public enum ScreenType
    {
        None      = 0,

        Level     = 1,
        Connector = 2,
        Secret    = 3,
        Dots      = 4
    }
}
