using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;


namespace TEiNRandomizer
{
    public struct Pair
    {
        public Int32 First;
        public Int32 Second;

        public Pair(int a, int b)
        { First = a; Second = b; }

        public static Pair operator +(Pair a) => a;
        public static Pair operator -(Pair a) => new Pair(-a.First, -a.Second);
        public static Pair operator +(Pair a, Pair b) => new Pair(a.First + b.First, a.Second + b.Second);
        public static Pair operator -(Pair a, Pair b) => new Pair(a.First - b.First, a.Second - b.Second);
    }

    public struct Bounds
    {
        public Int32 Left;
        public Int32 Right;
        public Int32 Top;
        public Int32 Bottom;
    }

    public static class Utility
    {
        public static string[] ElementToArray(XElement element)         // converts xml element value to a string array
        {
            string t_string = Convert.ToString(element.Value).Trim();
            var myArray = t_string.Split(Convert.ToChar("\n"));
            for (int i = 0; i < myArray.Count(); i++)
            {
                myArray[i] = myArray[i].Trim(Convert.ToChar("\t"), Convert.ToChar(" "));
            }

            return myArray;
        }
        public static Int32[] ElementToArray(XElement element, bool y)  // converts xml element value to a string array
        {
            string t_string = Convert.ToString(element.Value).Trim();
            var myArray = t_string.Split(Convert.ToChar("\n"));
            int[] intArray = new int[myArray.Count()];
            for (int i = 0; i < myArray.Count(); i++)
            {
                intArray[i] = Convert.ToInt32(myArray[i].Trim(Convert.ToChar("\t"), Convert.ToChar(" ")));
            }

            return intArray;
        }
        static void FlipCSV(string path)
        {
            var arr = File.ReadAllLines(path);
            int length = 0;
            var file = new List<string[]>();
            foreach (var str in arr)
            {
                var line = str.Split(Convert.ToChar(","));
                if (line.Length > length)
                    length = line.Length;
                line = line.Reverse().ToArray();
                file.Add(line);
            }
            var newfile = new string[arr.Length];
            for (int j = 0; j < file.Count; j++)
            {
                string newline = "";
                int offset = length - file[j].Length;
                for (int i = 0; i < offset; i++)
                {
                    newline += ",";
                }
                for (int i = 0; i < file[j].Length; i++)
                {
                    newline += file[j][i] + ",";
                }
                newfile[j] = newline;
            }
            File.WriteAllLines(path, newfile);
        }
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RNG.random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
