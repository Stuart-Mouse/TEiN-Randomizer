using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public struct LevelFile
    {
        public LevelHeader header;
        public TileID[,] data;

        public LevelFile(int width = 54, int height = 32)
        {
            header.version = 1;
            header.width   = width;
            header.height  = height;
            header.layers  = 5;

            // get data layer length
            int layerLength = header.width * header.height;

            // initialize data layers
            data = new TileID[5,layerLength];
        }

        // const int values for indexing level layers
        public const int BACK1   = 0;
        public const int ACTIVE  = 1;
        public const int TAG     = 2;
        public const int OVERLAY = 3;
        public const int BACK2   = 4;
    }
    public struct LevelHeader
    {
        public Int32 version;
        public Int32 width;
        public Int32 height;
        public Int32 layers;
    }
}
