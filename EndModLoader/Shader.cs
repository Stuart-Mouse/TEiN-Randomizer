using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public class Shader
    {
        public bool   Enabled { get; set; }
        public string Name { get; set; }
        public string fx_shader_mid { get; set; }
        public string midfx_graphics { get; set; }
        public int    midfx_layer { get; set; }
        public double shader_param { get; set; }
    }
}
