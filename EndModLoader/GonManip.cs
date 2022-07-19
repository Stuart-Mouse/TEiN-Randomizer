using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public partial class GonObject
    {
        public static class Manip
        {
            
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

                List<string[]> lowArr  = GonTo2DStringArray(low).ToList();
                List<string[]> highArr = GonTo2DStringArray(high).ToList();

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
                    if(!found) ret.InsertChild(FromStringArray(lowArr[i]));
                }
                foreach (var highItem in highArr)
                {
                    ret.InsertChild(FromStringArray(highItem));
                }

                return ret;
            }

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
            public static string[] GonToStringArray(GonObject gon)
            {
                try
                {
                    string[] ret = new string[gon.Size()];
                    for (int i = 0; i < gon.Size(); i++)
                    {
                        ret[i] = gon[i].String();
                    }
                    return ret;
                }
                catch (GonException ge)
                {
                    Console.Write(ge);
                    throw;
                }
            }
            public static string[][] GonTo2DStringArray(GonObject gon)
            {
                try
                {
                    string[][] ret = new string[gon.Size()][];
                    for (int i = 0; i < gon.Size(); i++)
                    {
                        ret[i] = GonToStringArray(gon[i]);
                    }
                    return ret;
                }
                catch (GonException ge)
                {
                    Console.Write(ge);
                    throw;
                }
            }
            public static int[] GonToIntArray(GonObject gon)
            {
                int[] ret = new int[gon.Size()];
                for (int i = 0; i < gon.Size(); i++)
                {
                    ret[i] = gon[i].Int();
                }
                return ret;
            }
        }

    }
}
