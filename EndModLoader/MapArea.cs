using System;
using System.Collections.Generic;

namespace TEiNRandomizer
{
    public class MapArea
    {
        public string  ID;
        public string  Name;
        public Tileset Tileset = new Tileset();

        public AreaType AreaType = AreaType.normal;
        public GenerationType GenType;

        // Map and Map Markers
        public GameMap Map;
        public Dictionary<Pair, OpenEnd> OpenEnds   = new Dictionary<Pair, OpenEnd>();
        public Dictionary<Pair, OpenEnd> DeadEnds   = new Dictionary<Pair, OpenEnd>();
        public Dictionary<Pair, OpenEnd> SecretEnds = new Dictionary<Pair, OpenEnd>();

        // Generation Variables
        public List<Level> Levels = Randomizer.StandardLevels;
        public Collectables   LevelCollectables  = Collectables.None;
        public Collectables   ExitCollectables   = Collectables.None;
        public Collectables[] SecretCollectables;

        // Loaded Areas
        public string CSVPath;
        public string LevelPath;
        // Standard Areas
        public int LevelQuota;
        public int LevelCount;
        public int ConnectorCount;
        public Pair MaxSize;
        public Directions Anchor;
        public Directions NoBuild;
        public List<MapScreen> ChosenScreens = new List<MapScreen>();

        // Linking and generation variables
        public MapArea ChildU;
        public MapArea ChildD;
        public MapArea ChildL;
        public MapArea ChildR;
        public MapArea Child { get => ChildR; set => ChildR = value; } // To save space, single child areas (GenType standard) will use ChildR to store child reference
         
        // Loaded and Split areas
        public Pair XUpCoords;
        public Pair XDownCoords;
        public Pair XLeftCoords;
        public Pair XRightCoords;
        // Standard Areas
        public Pair ECoords;
        public Pair XCoords;
        public Directions EDir;
        public Directions XDir;

        // Only used in standard generation atm
        public _Flags Flags;

        [Flags]
        public enum _Flags
        {
            None        =      0,
            Link        = 1 << 0,
            Concatenate = 1 << 1,
            BackTrack   = 1 << 2,
        }
    }
    public enum GenerationType
    {
        None        = 0,

        Standard    = 1,
        Loaded      = 2,
        Split       = 3,
        Composite   = 4
    }
    public struct OpenEnd
    {
        public Pair   Coords;
        public int    NumNeighbors;
        public string PathTrace;
        public string PlacedBy;
        public int Distance => PathTrace.Length;
        public OpenEnd(Pair coords, string placed_by = "", int num_neighbors = 0, string pathtrace = "")
        {
            Coords       = coords;
            NumNeighbors = num_neighbors;
            PathTrace    = pathtrace;
            PlacedBy     = placed_by;
        }
    }
}
