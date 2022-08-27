using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;


namespace TEiNRandomizer
{
    public struct Pair
    {
        public Int32 I;
        public Int32 J;

        public Pair(int a, int b)
        { I = a; J = b; }

        public static Pair operator +(Pair a) => a;
        public static Pair operator -(Pair a) => new Pair(-a.I, -a.J);
        public static Pair operator +(Pair a, Pair b) => new Pair(a.I + b.I, a.J + b.J);
        public static Pair operator -(Pair a, Pair b) => new Pair(a.I - b.I, a.J - b.J);
        public static Pair operator *(Pair a, int s) => new Pair(a.I * s, a.J * s);
        public static Pair operator /(Pair a, int s) => new Pair(a.I / s, a.J / s);
        public static Pair operator *(Pair a, Pair b) => new Pair(a.I * b.I, a.J * b.J);
        public static Pair operator /(Pair a, Pair b) => new Pair(a.I / b.I, a.J / b.J);
        public static bool operator ==(Pair a, Pair b) { if (a.I == b.I && a.J == b.J) return true;  else return false; }
        public static bool operator !=(Pair a, Pair b) { if (a.I != b.I && a.J != b.J) return false; else return true;  }
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
        public static Collectables ToCollectables(this string[] strs)
        {
            Collectables ret = Collectables.None;
            if (strs.Contains("tumor"    )) ret |= Collectables.Tumor;
            if (strs.Contains("megatumor")) ret |= Collectables.MegaTumor;
            if (strs.Contains("cart"     )) ret |= Collectables.Cartridge;
            if (strs.Contains("key"      )) ret |= Collectables.Key;
            if (strs.Contains("rings"    )) ret |= Collectables.Rings;
            if (strs.Contains("head"     )) ret |= Collectables.FriendHead;
            if (strs.Contains("body"     )) ret |= Collectables.FriendBody;
            if (strs.Contains("soul"     )) ret |= Collectables.FriendSoul;
            if (strs.Contains("goal"     )) ret |= Collectables.LevelGoal;
            if (strs.Contains("exit"     )) ret |= Collectables.ExitWarp;
            if (strs.Contains("orb"      )) ret |= Collectables.FriendOrb;
            return ret;
        }
        
        public static Directions Opposite(this Directions dir)
        {
            Directions ret = Directions.None;

            if (dir.HasFlag(Directions.R)) ret |= Directions.L;
            if (dir.HasFlag(Directions.L)) ret |= Directions.R;
            if (dir.HasFlag(Directions.D)) ret |= Directions.U;
            if (dir.HasFlag(Directions.U)) ret |= Directions.D;

            if (dir.HasFlag(Directions.UR)) ret |= Directions.DL;
            if (dir.HasFlag(Directions.DR)) ret |= Directions.UL;
            if (dir.HasFlag(Directions.UL)) ret |= Directions.DR;
            if (dir.HasFlag(Directions.DL)) ret |= Directions.UR;

            return ret;
        }
        public static Directions ToDirections(this string dir)
        {
            switch (dir)
            {
                case "u": return Directions.U;
                case "d": return Directions.D;
                case "l": return Directions.L;
                case "r": return Directions.R;
                case "ur": return Directions.UR;
                case "dr": return Directions.DR;
                case "ul": return Directions.UL;
                case "dl": return Directions.DL;
            }
            return Directions.None;
        }
        public static Directions ToDirections(this string[] dir)
        {
            Directions ret = Directions.None;

            if (dir.Contains("u")) ret |= Directions.U;
            if (dir.Contains("d")) ret |= Directions.D;
            if (dir.Contains("l")) ret |= Directions.L;
            if (dir.Contains("r")) ret |= Directions.R;
            if (dir.Contains("ur")) ret |= Directions.UR;
            if (dir.Contains("dr")) ret |= Directions.DR;
            if (dir.Contains("ul")) ret |= Directions.UL;
            if (dir.Contains("dl")) ret |= Directions.DL;

            return ret;
        }
        public static string SingleToString(this Directions dir)
        {
            switch (dir)
            {
                case Directions.U: return "up";
                case Directions.D: return "down";
                case Directions.L: return "left";
                case Directions.R: return "right";
                case Directions.UR: return "upright";
                case Directions.DR: return "downright";
                case Directions.UL: return "upleft";
                case Directions.DL: return "downleft";
            }
            return null;
        }
        public static void ActOnEach(this Directions dir, Action<Directions> act)
        {
            if (dir.HasFlag(Directions.U)) act(Directions.U);
            if (dir.HasFlag(Directions.D)) act(Directions.D);
            if (dir.HasFlag(Directions.L)) act(Directions.L);
            if (dir.HasFlag(Directions.R)) act(Directions.R);
            if (dir.HasFlag(Directions.UR)) act(Directions.UR);
            if (dir.HasFlag(Directions.DR)) act(Directions.DR);
            if (dir.HasFlag(Directions.UL)) act(Directions.UL);
            if (dir.HasFlag(Directions.DL)) act(Directions.DL);
        }
        public static Pair ToVectorPair(this Directions dir)
        {
            switch (dir)
            {
                case Directions.U: return new Pair(-1, 0);
                case Directions.D: return new Pair(1, 0);
                case Directions.L: return new Pair(0, -1);
                case Directions.R: return new Pair(0, 1);
                case Directions.UR: return new Pair(-1, 1);
                case Directions.DR: return new Pair(1, 1);
                case Directions.UL: return new Pair(-1, -1);
                case Directions.DL: return new Pair(1, -1);
            }
            return new Pair(0, 0);
        }
        public static int CountSetBits(int a)
        {
            int count = 0;
            while (a != 0)
            {
                count++;
                a &= a - 1;
            }
            return count;
        }
        public static Collectables CollectablesFromString(string str)
        {
            Collectables ret = Collectables.None;

            if (str.Contains("t")) ret |= Collectables.Tumor;
            if (str.Contains("m")) ret |= Collectables.MegaTumor;
            if (str.Contains("c")) ret |= Collectables.Cartridge;
            if (str.Contains("r")) ret |= Collectables.Rings;
            if (str.Contains("h")) ret |= Collectables.FriendHead;
            if (str.Contains("b")) ret |= Collectables.FriendBody;
            if (str.Contains("s")) ret |= Collectables.FriendSoul;

            return ret;
        }
        public static int FindHighestBit(byte bite)
        {
            // gets the highest active bit in the byte.
            // The intent is to basically get what is the bit shift of 1 that this nummber equates to
            // if bite == 0 then -1 is returned

            int hiBit = -1;
            for (int i = 0; i < 8; i++)
                hiBit = ((bite & (1 << i)) != 0) ? i : hiBit;
            return hiBit;
        }
        public static void XML2GON(string path)
        {
            string outFile = "";
            string fileName = Path.GetFileNameWithoutExtension(path);

            outFile += $"# {fileName}\n";
            
            var doc = XDocument.Load(path);    // open levelpool file

            // Reformat header
            outFile += "header {\n";
            if (Convert.ToBoolean(doc.Root.Attribute("enabled").Value == "True"))
                outFile += "enabled true\n";
            else outFile += "enabled false\n";
            outFile += "order " + Convert.ToInt32(doc.Root.Attribute("order").Value) + "\n";
            outFile += "author \"" + doc.Root.Attribute("author").Value + "\"\n";
            outFile += "source \"" + doc.Root.Attribute("source").Value + "\"\n";
            outFile += "path   \"data/level_pools/" + doc.Root.Attribute("source").Value + "/\"\n";
            outFile += "}\n\n";

            // Reformat levels / tileset
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == "lvl" || element.Name == "level")
                {
                    outFile += $"  \"{element.Attribute("name").Value}\" {{\n";
                    //outFile += "    name \"" + element.Attribute("name").Value + "\"\n";
                    //outFile += "    secret " + Convert.ToBoolean(element.Attribute("secret").Value) + "\n";

                    outFile += "    connections {\n";
                    outFile += "      up n\n";
                    outFile += "      down n\n";
                    outFile += "      left E\n";
                    outFile += "      right X\n";
                    outFile += "    }\n";

                    foreach (var element2 in element.Elements())
                    {
                        if (element2.Name == "tileset")
                        {
                            outFile += "    tileset {\n";
                            foreach (var element3 in element2.Elements())
                            {
                                if (element3.Name == "default")
                                {
                                    outFile += "      default\n";
                                    outFile += "      {" + element3.Value + "}\n";
                                }
                                else if (element3.Name == "need")
                                {
                                    outFile += "      need\n";
                                    outFile += "      {" + element3.Value + "}\n";
                                }
                                else if (element3.Name == "art")
                                {
                                    outFile += "      art\n";
                                    outFile += "      {" + element3.Value + "}\n";
                                }
                            }
                            outFile += "    }\n";
                        }
                    }
                    outFile += "  }\n";
                }
                else if (element.Name == "tileset")
                {
                    outFile += "tileset {\n";
                    foreach (var element2 in element.Elements())
                    {
                        if (element2.Name == "default")
                        {
                            outFile += "  default\n";
                            outFile += "  {" + element2.Value + "}\n";
                        }
                        else if (element2.Name == "need")
                        {
                            outFile += "  need\n";
                            outFile += "  {" + element2.Value + "}\n";
                        }
                        else if (element2.Name == "art")
                        {
                            outFile += "  art\n";
                            outFile += "  {" + element2.Value + "}\n";
                        }
                    }
                    outFile += "}\n\n";
                }
            }

            File.WriteAllText($"data/level_pools/The End is Nigh/{fileName}.gon", outFile);
        }
        static void FlipCSV(string path)
        {
            var arr = File.ReadAllLines(path);
            int length = 0;
            var file = new List<string[]>();
            foreach (var str in arr)
            {
                var line = str.Split(',');
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
        public static string[][] LoadCSV(string path, out int length)
        {
            // loads a csv file into a jagged 2d string array
            // the length of the longest line is sent back as an out parameter
            var file = File.ReadAllLines(path);
            length = 0;
            var arr = new List<string[]>();
            foreach (var str in file)
            {
                var line = str.Split(',');
                    length = line.Length;
                arr.Add(line);
            }
            return arr.ToArray();
        }
    }
}
