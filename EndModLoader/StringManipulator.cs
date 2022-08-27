using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public static class StringManipulator
    {
        public static byte[] HexToByteArray(this string hex)
        {
            int num_chars = hex.Length;
            byte[] bytes = new byte[num_chars / 2];

            for (int i = 0; i < num_chars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }
    }
}
