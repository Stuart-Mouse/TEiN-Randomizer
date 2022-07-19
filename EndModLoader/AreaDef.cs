using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public struct AreaDef
    {
        // AreaDefs are used to define the requirements and rules for an area being generated
        // A list of AreaDefs should provide all of the necessary information for the Map Generator to build all areas in a world to spec

        // The name given to the area in the area_defs file
        // This is used as the filename for levels in this area
        public string AreaID;

        // The number of gameplay levels that need to be generated in the area
        public int LevelQuota;

        // These are the area IDs of the areas to branch off of this one (that make sense?)
        // We will need to reserve an OpenEnd for each AreaEnd
        //public string[] AreaEnds;
        public string NextAreaID;

        // These are the maximum dimensions of the area
        // Will only be computed if not equal to zero or null
        // Order is height, width
        public Pair Bounds;

        // This is the primary direction of building in the area
        public Directions BuildDirection;
        // This obsoletes the two below members atm, because they are the same value
        // This also matches the way area defs are defined better.
        // The only downside is that it is technically less versatile, but that doesnt matter now because the code is not complex enough to support that anyways

        // This is used to determine where in the area map to place the entry point
        //public Directions Anchor;

        // Map generation will not be allowed to place exits facing in any directions flagged in NoBuild
        //public Directions NoBuild;

        // This is the type of generation that will be used in creating the area
        // Some non-standard types will be generated in a separate sub-map that is appended to the main map after the fact
        public GenerationType Type;

        // These are settings that are unique to the specific area
        // These will override global settings
        public SettingsFile AreaSettings;
    }
}
