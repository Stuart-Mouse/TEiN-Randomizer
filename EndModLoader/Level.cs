using System;
using System.Collections.Generic;

namespace TEiNRandomizer
{
    public class Level
    {
        public string Name;
        public string Path;
        public string InFile { get => $"{Path}{Name}.lvl"; }

        public Tileset TSDefault;
        public Tileset TSNeed;
        public MapConnections MapConnections;
        public bool FlipH = false;

        public Level Clone()
        {
            return (Level)MemberwiseClone();
        }
    }
} 