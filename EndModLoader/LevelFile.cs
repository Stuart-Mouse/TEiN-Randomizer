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
