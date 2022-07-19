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
        public static Pair operator *(Pair a, int s) => new Pair(a.First * s, a.Second * s);
        public static Pair operator /(Pair a, int s) => new Pair(a.First / s, a.Second / s);
        public static Pair operator *(Pair a, Pair b) => new Pair(a.First * b.First, a.Second * b.Second);
        public static Pair operator /(Pair a, Pair b) => new Pair(a.First / b.First, a.Second / b.Second);
        public static bool operator ==(Pair a, Pair b) { if (a.First == b.First && a.Second == b.Second) return true; else return false; }
        public static bool operator !=(Pair a, Pair b) { if (a.First != b.First && a.Second != b.Second) return false; else return true; }
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
        public static int FindHighestBit(byte bite)
        {
            // gets the highest active bit in the byte.
            // The intent is to basically get what is the bit shift of 1 that this nummber equates to
            // if bite == 0 then -1 is returned

            int hiBit = -1;

            for (int i = 0; i < 8; i++)
                if ((bite & (1 << i)) != 0) hiBit = i;

            return hiBit;
        }
        
        public static Level FindLevelInListByName(List<Level> list, string name)
        {
            foreach (var level in list)
            {
                if (level.Name == name)
                    return level;
            }
            return null;
        }
        public static void TilesetTest()
        {

            // prevent crashing
            TilesetManip.MakeShaderPool();

            // create a drawpool for testing
            var drawPool = new List<Level>();
            foreach (var cat in AppResources.LevelPoolCategories)
            {
                if (cat.Enabled)
                {
                    foreach (var pool in cat.Pools) // push levels in all active level pools into drawpool vector
                    {
                        if (pool.Active)
                        {
                            foreach (var level in pool.Levels)
                            {
                                drawPool.Add(level);
                            }
                        }
                    }
                }
            }

            // Create a few MapAreas for testing purposes
            /*List<MapGenerator> MapAreasList = new List<MapGenerator>();
            for (int i = 0; i < 10; i++)
            {
                // Create new MapArea
                // the map area is created with a randmized name and tileset already initialized
                MapGenerator mapArea = new MapGenerator();

                // select some random levels
                for (int j = 0; j < 10; j++)
                {
                    int selection = RNG.random.Next(0, drawPool.Count());
                    mapArea.Levels.Add(drawPool[selection]);
                }

                MapAreasList.Add(mapArea);
            }

            // Create a StreamWriter object for writing to the tilesets.txt file
            using (StreamWriter sw = File.CreateText("data/tilesets.txt.append"))
            {
                // Loop over map areas
                for (int i = 0; i < MapAreasList.Count(); i++)
                {
                    // Get reference to MapArea #i in list 
                    MapGenerator mapArea = MapAreasList[i];
                     
                    // Set areaTileset to Map Area's tileset
                    Tileset areaTileset = mapArea.Tileset;

                    // Write the area's tileset to the file
                    sw.WriteLine($"{mapArea.Name} {{");
                    areaTileset.WriteTileset(sw);

                    // Loop over levels in area
                    for (int j = 0; j < mapArea.Levels.Count(); j++)
                    {
                        // Get reference to Level #i in area 
                        Level level = mapArea.Levels[j];

                        // Calculate the final tileset
                        // The tilesets are added in order of priority, from lowest to highest
                        Tileset levelTileset = (level.TSDefault + areaTileset) + level.TSNeed;
                       
                        // Write level tileset to the file
                        sw.WriteLine($"{level.Name} {{");
                        levelTileset.WriteTileset(sw);
                        sw.WriteLine("}");
                    }

                    // write closing bracket for area tileset
                    sw.WriteLine("}\n");
                }
            }*/
        }
        public static void WriteTilesetFunction()
        {
            string outFile = "";

            string[] names = {"area_name", "area_label_frame", "tile_graphics", "overlay_graphics", "background_graphics", "foreground_graphics", "palette", "area_type", "toxic_timer", "tile_particle_1", "tile_particle_2", "tile_particle_3", "tile_particle_4", "tile_particle_5", "global_particle_1", "global_particle_2", "global_particle_3", "decoration_1", "decoration_2", "decoration_3", "npc_1", "npc_2", "npc_3", "music", "ambience", "ambience_volume", "stop_previous_music", "art_alts", "fx_shader", "fx_shader_mid", "midfx_layer", "midfx_graphics", "shader_param" };

            foreach (string str in names)
            {
                outFile += $"if (gon[\"{str}\"] != null)\n";
                outFile += $"\t{str} = gon[\"{str}\"].String();\n\n";
            }

            outFile += "\n\n\n";

            foreach (string str in names)
            {
                outFile += $"if (b.{str} != null)\n";
                outFile += $"\ta.{str} = b.{str};\n";
            }

            File.WriteAllText("testout.txt", outFile);
        }

        public static void FixAltsFile()
        {
            var gon = GonObject.Load("data/text/art_alts.gon");

            string outFile = "";

            for (int i = 0; i < gon["safe"].Size(); i++)
            {
                var art = gon["safe"][i];
                outFile += $"{art.GetName()} [ ";

                for (int j = 0; j < art.Size(); j++)
                {
                    outFile += $"\"{art[j].String()}\", ";
                }
                outFile = outFile.TrimEnd(' ', ',');
                outFile += "]\n";
            }

            for (int i = 0; i < gon["extended"].Size(); i++)
            {
                var art = gon["extended"][i];
                outFile += $"{art.GetName()} [ ";

                for (int j = 0; j < art.Size(); j++)
                {
                    outFile += $"\"{art[j].GetName()}\", ";
                }
                outFile = outFile.TrimEnd(' ', ',');
                outFile += "]\n";
            }

            for (int i = 0; i < gon["crazy"].Size(); i++)
            {
                var art = gon["crazy"][i];
                outFile += $"{art.GetName()} [ ";

                for (int j = 0; j < art.Size(); j++)
                {
                    outFile += $"\"{art[j].GetName()}\", ";
                }
                outFile = outFile.TrimEnd(' ', ',');
                outFile += "]\n";
            }

            for (int i = 0; i < gon["insane"].Size(); i++)
            {
                string name = gon["insane"][i].GetName();
                outFile += ($"\"{name}\", ");
            }
            outFile = outFile.TrimEnd(' ', ',');
            outFile += "]\n";

            File.WriteAllText("art_alts.txt", outFile);
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
            outFile += "path   \"data/level pools/" + doc.Root.Attribute("source").Value + "/\"\n";
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

            File.WriteAllText($"data/level pools/The End is Nigh/{fileName}.gon", outFile);
        }

        
        public static string[] XElementToArray(XElement element)         // converts xml element value to a string array
        {
            string t_string = Convert.ToString(element.Value).Trim();
            var myArray = t_string.Split('\n');
            for (int i = 0; i < myArray.Count(); i++)
            {
                myArray[i] = myArray[i].Trim('\t', ' ');
            }

            return myArray;
        }
        public static Int32[] XElementToArray(XElement element, bool y)  // converts xml element value to a string array
        {
            string t_string = Convert.ToString(element.Value).Trim();
            var myArray = t_string.Split('\n');
            int[] intArray = new int[myArray.Count()];
            for (int i = 0; i < myArray.Count(); i++)
            {
                intArray[i] = Convert.ToInt32(myArray[i].Trim('\t'), ' ');
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
