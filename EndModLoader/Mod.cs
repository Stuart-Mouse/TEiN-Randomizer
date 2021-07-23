using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public class Mod : IComparable<Mod>
    {
        public string Title { get; private set; }       // Title of Saved Runs is set by the user
        public string Description { get; private set; } // Used for the seed of Saved Runs
        public string Author { get; private set; }      // 
        public string Version { get; private set; }     // Used for the date and time of Saved Runs
        public string ModPath { get; private set; }
        public bool Active { get; set; } = false;

        public List<int> ExeOffset { get; private set; }
        public List<byte> ExeByteReplacement { get; private set; }

        public Boolean HasExePatch { get; private set; }

        public Boolean HasSWFMod { get; private set; }

        public static readonly string MetaFile = "meta.xml";
        public static readonly string MetaFileNotFound = $"no {MetaFile} found";

        public static readonly string ExePatchFile = "exepatch.xml";

        public static readonly string SWFFile = "swfs/endnigh.swf";

        public override string ToString() => $"{Title} {Author} {Version}";

        public int CompareTo(Mod other) => Title.CompareTo(other.Title);

        //public static void ExtractToFolder(string path)
        //{
        //    ZipFile.ExtractToDirectory();
        //}

        public static Mod FromZip(string path)
        {
            // Various wacky Changed, Renamed, Removed events eventually lead to this.
            // C# Optionals when.
            if (!File.Exists(path)) return null;

            var mod = new Mod { ModPath = path };
            try
            {
                using (var zip = ZipFile.Open(path, ZipArchiveMode.Read))
                {
                    var swf = zip.GetEntry(SWFFile);

                    if(swf != null)
                    {
                        mod.HasSWFMod = true;
                    }

                    var meta = zip.GetEntry(MetaFile);

                    if (meta == null)
                    {
                        mod.Title = Path.GetFileNameWithoutExtension(path);
                        return mod;
                    }

                    try
                    {
                        var stream = meta.Open();
                        var doc = XDocument.Load(stream);
                        foreach (var element in doc.Root.Elements())
                        {
                            if (element.Name == "title") mod.Title = element.Value;
                            else if (element.Name == "description" || element.Name == "desc") mod.Description = element.Value;
                            else if(element.Name == "author") mod.Author = element.Value;
                            else if (element.Name == "version") mod.Version = element.Value;
                        }
                    }
                    catch (FileNotFoundException) { }

                    var exePatch = zip.GetEntry(ExePatchFile);

                    if (exePatch == null)
                    {
                        mod.HasExePatch = false;
                        return mod;
                    } else mod.HasExePatch = true;

                    try
                    {
                        mod.ExeOffset = new List<int>();
                        mod.ExeByteReplacement = new List<byte>();

                        var stream = exePatch.Open();
                        var doc = XDocument.Load(stream);
                        foreach (var element in doc.Root.Elements())
                        {
                            //if(element.Name == "entry" || element.Name == "Entry") It can be whatever for now but make it entry to futureproof
                            //{
                                String[] toAdd = element.Value.Split('~');

                                mod.ExeOffset.Add(int.Parse(toAdd[0], System.Globalization.NumberStyles.HexNumber));
                                mod.ExeByteReplacement.Add(toAdd[1].HexToByteArray()[0]);
                            //}  
                        }
                    }
                    catch (FileNotFoundException) { }
                }
            }
            // On the weird off chance that you are currently still copying the folder
            // to the mods directory, it will be "opened by another process" and crash
            // the program horribly, to the point of not even responding to Task Manager.
            catch (IOException)
            {
                for (int i = 0; i < 10; ++i)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        var again = FromZip(path);
                        return again;
                    }
                    catch (IOException) { } 
                }
                // Fuck it, give up on this file.
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                throw e;
            }

            return mod;
        }
    }
} 