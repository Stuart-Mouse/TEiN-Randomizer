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
        public Int32[] back1;
        public Int32[] active;
        public Int32[] tag;
        public Int32[] overlay;
        public Int32[] back2;
    }

    public struct Pair
    {
        public Int32 First;
        public Int32 Second;
    }
}
