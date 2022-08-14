using System;
using System.Collections.Generic;

namespace TEiNRandomizer
{
    public class Level
    {
        // Level name used for identification and for loading level file
        public string Name;

        // This identifies the exact path from the executable
        // to the folder of the level pool the level belongs to
        public string Path;

        // Returns the file to load
        public string InFile { get => $"{Path}{Name}.lvl"; }

        // Level tileset defaults and needs
        public Tileset TSDefault;
        public Tileset TSNeed;

        // map connections
        public MapConnections MapConnections;

        // This isn't considered a level flag bc it is not set in the levelpools file.
        // This signifies whether a level has been flipped horizontally
        // (or rather, that we should flip the level upon loading the actual levelFile)
        public bool FlippedHoriz = false;

        // Used to store tileswaps for transition tags
        public Dictionary<TileID, TileID> TileSwaps = new Dictionary<TileID, TileID>();

        // Debug info
        public MapConnections DebugReqs;
        public MapConnections DebugNots;

        // Just gonna store all the level flags in a bit field. I assume it's better than having a dozen bools
        public LevelFlags Flags;

        [Flags]
        public enum LevelFlags
        {
            // Keys and Locks
            Key             = 1 << 0,
            Lock            = 1 << 1,

            // Warps
            ExitWarp        = 1 << 2,
            WarpPoint       = 1 << 3,

            // Collectables
            Collectable     = 1 << 4,   // May be used to signify a generic collectable that can be replaced with any other type
            MegaTumor       = 1 << 5,
            Cart            = 1 << 6,

            // NPC Flags
            npc_1           = 1 << 8,
            npc_2           = 1 << 9,
            npc_3           = 1 << 10
        }

        public Level Clone()
        {
            // This returns a cloned Level object.
            // The tileset will remain a reference to the same tileset as the original level.

            // Create new Level to return
            Level ret = new Level();
            
            // Clone all members
            ret.Name = Name;
            //ret.isGameplay = isGameplay;
            ret.Path = Path;
            ret.TSDefault = TSDefault;
            ret.TSNeed = TSNeed;
            ret.Flags = Flags;
            ret.FlippedHoriz = FlippedHoriz;
            ret.MapConnections = MapConnections;
            ret.TileSwaps = new Dictionary<TileID, TileID>();
            foreach (var item in TileSwaps)
                ret.TileSwaps.Add(item.Key, item.Value);

            // Return the new Level
            return ret;
        }
    }
} 