﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    class LevelCollisionException : Exception
    {
        public LevelCollisionException()
        {

        }

        public LevelCollisionException(string name)
            : base(String.Format("Collision Detected in Level Generation:", name))
        {

        }
    }
}
