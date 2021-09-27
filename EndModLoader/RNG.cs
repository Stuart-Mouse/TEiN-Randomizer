using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TEiNRandomizer
{
    public static class RNG
    {
        public static RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        public static byte[] byteArray = new byte[4];
        public static Random random = new Random();
        //public Int32 CurrentSeed;

        static RNG()
        {
            random = new Random();
        }

        public static void SeedMe(int seed)
        {
            random = new Random(seed);
        }

        public static UInt32 GetUInt32()
        {
            crypto.GetBytes(byteArray);
            return BitConverter.ToUInt32(byteArray, 0);
        }

        public static bool CoinFlip()
        {
            if (random.Next(0,2) == 0)
                return true;
            else return false;
        }

        public static bool Chances(int over, int under)
        {
            if (random.Next(0, under+1) > over)
                return true;
            else return false;
        }

        public static bool Percent(int per)
        {
            if (random.Next(0, 100) < per)
                return true;
            else return false;
        }

        public static int GetFrom(int low, int high)
        {
            int num;
            int bound = high - low;
            //4294967295

            num = (int)(GetUInt32() * bound / 4294967295) + low;

            return num;
        }
    }
}
