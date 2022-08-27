using System;
using System.Collections.Generic;
using System.Linq;

namespace TEiNRandomizer
{
    public partial class GonObject
    {
        // Custom Gon Object Constructors)
        public static GonObject FromBool(bool val, string name = "")
        {
            GonObject gon = new GonObject();
            gon.Name = name;
            gon.Type = FieldType.BOOL;

            gon.Bool_Data = val;

            return gon;
        }
        public static GonObject FromInt(int val, string name = "")
        {
            GonObject gon = new GonObject();
            gon.Name = name;
            gon.Type = FieldType.NUMBER;

            gon.Int_Data = val;
            gon.Float_Data = val;

            return gon;
        }
        public static GonObject FromDouble(double val, string name = "")
        {
            GonObject gon = new GonObject();
            gon.Name = name;
            gon.Type = FieldType.NUMBER;

            gon.Float_Data = val;

            return gon;
        }
        public static GonObject FromString(string val, string name = "")
        {
            GonObject gon = new GonObject();
            gon.Name = name;
            gon.Type = FieldType.STRING;

            gon.String_Data = val;

            return gon;
        }
        public static GonObject FromIntArray(int[] arr, string name = "")
        {
            GonObject gon = new GonObject();
            gon.Name = name;
            gon.Type = FieldType.ARRAY;

            foreach (int i in arr)
            {
                gon.InsertChild(FromInt(i));
            }

            return gon;
        }
        public static GonObject FromStringArray(string[] arr, string name = "")
        {
            GonObject gon = new GonObject();
            gon.Name = name;
            gon.Type = FieldType.ARRAY;

            foreach (string s in arr)
            {
                gon.InsertChild(FromString(s));
            }

            return gon;
        }

        // Functions for easily reading Gon Arrays
        public string[] ToStringArray()
        {
            string[] ret = new string[this.Size()];
            for (int i = 0; i < this.Size(); i++)
            {
                ret[i] = this[i].String();
            }
            return ret;
        }
        public string[][] To2DStringArray()
        {
            string[][] ret = new string[this.Size()][];
            for (int i = 0; i < this.Size(); i++)
            {
                ret[i] = this[i].ToStringArray();
            }
            return ret;
        }
        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>(this.Size());
            for (int i = 0; i < this.Size(); i++)
            {
                if (ret.ContainsKey(this[i][0].String()))
                    Console.WriteLine(this[i][0].String());
                else
                    ret.Add(this[i][0].String(), this[i][1].String());
            }
            return ret;
        }
        public int[] ToIntArray()
        {
            int[] ret = new int[this.Size()];
            for (int i = 0; i < this.Size(); i++)
            {
                ret[i] = this[i].Int();
            }
            return ret;
        }

        public static GonObject PriorityMerge(GonObject low, GonObject high)
        {
            // create new gonObject to return
            GonObject ret = new GonObject();

            // iterate over the low-priority object's children
            for (int i = 0; i < low.Size(); i++)
            {
                string childName = low[i].Name;
                var lowChild = low[childName];

                // step into if the same child exists in high priority object
                if (high[childName] != null)
                {
                    var highChild = high[childName];

                    // special case for art_alts
                    if (childName == "art_alts")
                        ret.InsertChild("art_alts", ArtAltsMerge(lowChild, highChild));

                    // recurse if type is object
                    else if (lowChild.Type == FieldType.OBJECT && highChild.Type == FieldType.OBJECT)
                        ret.InsertChild(PriorityMerge(lowChild, highChild));

                    // otherwise just use the high-priority child
                    else ret.InsertChild(highChild);
                }
                else ret.InsertChild(lowChild); // if no high-priority child available, use low-priority child
            }

            // iterate over high-priority object's children
            // copy those that do not already exist in low-priority object to return object
            for (int i = 0; i < high.Size(); i++)
            {
                string childName = high[i].Name;

                if (low[childName] == null)
                    ret.InsertChild(high[childName]);
            }
            return ret;
        }
        public static GonObject ArtAltsMerge(GonObject low, GonObject high)
        {
            // create new gonObject to return
            GonObject ret = new GonObject();

            ret.Type = FieldType.ARRAY;

            List<string[]> lowArr  = low.To2DStringArray().ToList();
            List<string[]> highArr = high.To2DStringArray().ToList();

            for (int i = 0; i < lowArr.Count(); i++)
            {
                bool found = false;
                for (int j = 0; j < highArr.Count(); j++)
                {
                    if (lowArr[i][0] == highArr[j][0])
                    {
                        ret.InsertChild(FromStringArray(highArr[j]));
                        highArr.Remove(highArr[j]);
                        found = true;
                        break;
                    }
                }
                if (!found) ret.InsertChild(FromStringArray(lowArr[i]));
            }
            foreach (var highItem in highArr)
            {
                ret.InsertChild(FromStringArray(highItem));
            }

            return ret;
        }
    }
}
