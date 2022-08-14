using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public class MapArea
    {
        // AreaInfo contains all of the instance information for the map generator. This is being used to create a more functional version of the map generator which should make debugging much easier.

        // Basic map area information
        public string ID;
        public string Name;
        public bool IsStandalone = false;
        public Tileset Tileset = null;
        public string TSRef = null;    // used when one area needs to use the tileset of another

        // The set of levels to be used as the draw pool for gameplay levels in this area
        // By default this is the list of standard levels, but may be replaced with the cart pool or a custom pool
        public List<Level> Levels = Randomizer.StandardLevels;

        // This is used for areas which are loaded from csv
        public string CSVPath;
        public string LevelPath;

        // Used to keep track of how many gameplay and connector levels have been added
        public int LevelCount;
        public int ConnectorCount;

        // The 2D array map of the area
        public GameMap Map;

        // These are the OpenEnds, DeadEntries, and SecretEnds contained within the area
        public Dictionary<Pair, OpenEnd> OpenEnds    = new Dictionary<Pair, OpenEnd>();
        public Dictionary<Pair, OpenEnd> DeadEntries = new Dictionary<Pair, OpenEnd>();
        public Dictionary<Pair, OpenEnd> SecretEnds  = new Dictionary<Pair, OpenEnd>();
        public struct OpenEnd
        {
            public int NumNeighbors;
            public string PathTrace;
            public OpenEnd(int num_neighbors, string pathtrace)
            {
                NumNeighbors = num_neighbors;
                PathTrace = pathtrace;
            }
        }

        // Contains tags which are relevant to generation
        public string[] tags;

        // These will be used for split and loaded areas which can have multiple exits
        public string XUp;
        public string XDown;
        public string XLeft;
        public string XRight;
        public Pair XUpCoords;
        public Pair XDownCoords;
        public Pair XLeftCoords;
        public Pair XRightCoords;

        // The number of gameplay levels that need to be generated in the area
        public int LevelQuota;

        // These are the area IDs of the areas to branch off of this one (that make sense?)
        // We will need to reserve an OpenEnd for each AreaEnd
        //public string[] AreaEnds;
        public string ExitID;

        // These are the maximum dimensions of the area
        // Will only be computed if not equal to zero or null
        // Order is height, width
        public Pair MaxSize;

        // This is used to determine where in the area map to place the entry point
        public Directions Anchor;

        // Map generation will not be allowed to place exits facing in any directions flagged in NoBuild
        public Directions NoBuild;

        // This is the type of generation that will be used in creating the area
        // Some non-standard types will be generated in a separate sub-map that is appended to the main map after the fact
        public GenerationType Type;

        // These are settings that are unique to the specific area
        // These will override global settings
        public SettingsFile AreaSettings;

        // MaxBuilt and MinBuilt area used to track which rows and columns are empty in an area's map.
        // This is used to crop the map later
        public Pair MinBuilt;
        public Pair MaxBuilt;

        // Entrance and exit level coordinates
        public Pair ECoords;
        public Pair XCoords;

        // Entrance direction and exit direction
        public Directions EDir;
        public Directions XDir; // only used by standard type areas

        // Determines the default collectables to be placed in the area's gameplay levels (usually either "t" or "r")
        //Collectables DefaultCollectables;

        // List of all the map screens in the area, used when creating area data files
        public List<MapScreen> ChosenScreens = new List<MapScreen>();

        // Generation settings
        //public bool DoCorruptions;


    }
    public enum GenerationType
    {
        // This is the standard area type used in generating normal areas
        Standard,
        // These exist as slight modifications of the standard generation routine
        // They are effectively a proxy for checking the Tileset.area_type during generation
        Dark,
        Cart,
        Iron,
        Glitch,
        // Used for hub areas, or special layouts. These are loaded from a csv file
        Loaded,
        // Used to create "split" areas
        Split
        // The below types are used in Steven areas, they do not have entrances
    }
}
