using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace TEiNRandomizer
{
    public class Pool : IComparable<Pool>
    {
        public string Name { get; set; }
        public List<Level> Levels { get; set; }
        public bool Active { get; set; }
        public string NumLevels { get; set; }
        public short Order { get; set; }
        public string Folder { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }

        public int CompareTo(Pool other) => Order.CompareTo(other.Order);

        public void Save()
        {
            try
            {
                var doc = XDocument.Load($"data/levelpools/{Folder}/{Name}.xml");    // open levelpool file
                doc.Root.Attribute("enabled").Value = Active.ToString();
                doc.Save($"data/levelpools/{Folder}/{Name}.xml");
            }
            catch (Exception)
            {
                MessageBox.Show(
                        "Error saving the pool bool.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK
                    );
                throw;
            }
            
        }


        public Pool(string fileName, string folder)
        {
            this.Name = fileName;
            this.Folder = folder;
            this.Levels = new List<Level>();
            string tiledefault = "";
            string tileneed = "";
            string tileart = "";

            var doc = XDocument.Load($"data/levelpools/{Folder}/{Name}.xml");    // open levelpool file
            this.Active = Convert.ToBoolean(doc.Root.Attribute("enabled").Value == "True");
            this.Order = Convert.ToInt16(doc.Root.Attribute("order").Value);
            this.Author = (doc.Root.Attribute("author").Value);
            this.Source = doc.Root.Attribute("source").Value;
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == "lvl" || element.Name == "level")
                {
                    var level = new Level { };
                    level.Folder = this.Folder;
                    level.TSDefault += tiledefault;
                    level.TSNeed = tileneed;
                    level.Art += tileart;
                    level.Name = element.Attribute("name").Value;
                    level.HasSecret = Convert.ToBoolean(element.Attribute("secret").Value);
                    level.CanReverse = Convert.ToBoolean(element.Attribute("reverse").Value);
                    
                    foreach (var element2 in element.Elements())
                    {
                        //if (element2.Name == "name") level.Name = element2.Value;
                        if (element2.Name == "tileset")
                        {
                            foreach (var element3 in element2.Elements())
                            {
                                if (element3.Name == "default") level.TSDefault += element3.Value.Replace("\t", null);
                                else if (element3.Name == "need") level.TSNeed += " " + element3.Value.Replace("\t", null);
                                else if (element3.Name == "art") level.Art += element3.Value.Replace("\t", null);
                            }
                        }
                    }
                    Console.WriteLine($"{level.Name} {level.TSNeed}");
                    this.Levels.Add(level);
                }
                else if (element.Name == "tileset")
                {
                    foreach (var element2 in element.Elements())
                    {
                        if (element2.Name == "default") tiledefault = element2.Value.Replace("\t", null);
                        else if (element2.Name == "need") tileneed = element2.Value.Replace("\t", null);
                        else if (element2.Name == "art") tileart = element2.Value.Replace("\t", null);
                    }
                }
            }
            NumLevels = this.Levels.Count.ToString() + " levels";
        }
    }
}