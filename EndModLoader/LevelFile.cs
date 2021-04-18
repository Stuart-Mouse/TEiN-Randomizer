using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public class LevelFile
    {
        public LevelHeader header;
        public LevelData data;

        public LevelFile()
        {
            header.version = 1;
            header.width = 54;
            header.height = 32;
            header.layers = 5;

            // get data layer length
            int layerLength = header.width * header.height;

            // initialize data layers
            data.back1      = new TileID[layerLength];
            data.active     = new TileID[layerLength];
            data.tag        = new TileID[layerLength];
            data.overlay    = new TileID[layerLength];
            data.back2      = new TileID[layerLength];
        }

        public LevelFile(int width, int height)
        {
            header.version = 1;
            header.width = width;
            header.height = height;
            header.layers = 5;

            // get data layer length
            int layerLength = header.width * header.height;

            // initialize data layers
            data.back1 = new TileID[layerLength];
            data.active = new TileID[layerLength];
            data.tag = new TileID[layerLength];
            data.overlay = new TileID[layerLength];
            data.back2 = new TileID[layerLength];
        }
    }
    public struct LevelHeader
    {
        public Int32 version;
        public Int32 width;
        public Int32 height;
        public Int32 layers;
    }

    public struct LevelData
    {
        public TileID[] back1;
        public TileID[] active;
        public TileID[] tag;
        public TileID[] overlay;
        public TileID[] back2;
    }

    public struct Pair
    {
        public Int32 First;
        public Int32 Second;
    }

    public struct Bounds
    {
        public Int32 Left;
        public Int32 Right;
        public Int32 Top;
        public Int32 Bottom;
    }

}
