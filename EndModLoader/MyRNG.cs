using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TEiNRandomizer
{
    public class MyRNG
    {
        public RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        public byte[] byteArray = new byte[4];
        public Random rand = new Random();

        public MyRNG()
        {
            rand = new Random();
        }

        public void SeedMe(int seed)
        {
            rand = new Random(seed);
        }

        public UInt32 GetUInt32()
        {
            crypto.GetBytes(byteArray);
            return BitConverter.ToUInt32(byteArray, 0);
        }

        public bool CoinFlip()
        {
            if (rand.Next(0,2) == 0)
                return true;
            else return false;
        }

        public int GetFrom(int low, int high)
        {
            int num;
            int bound = high - low;
            //4294967295

            num = (int)(GetUInt32() * bound / 4294967295) + low;

            return num;
        }
    }
}
