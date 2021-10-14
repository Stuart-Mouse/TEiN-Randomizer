using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public class LevelPool : IComparable<LevelPool>, INotifyPropertyChanged
    {
        public string Name { get; set; }
        public List<Level> Levels { get; set; }
        public string NumLevels { get; set; }
        public short Order { get; set; }
        public string Folder { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }
        private bool _active { get; set; }
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged(nameof(Active));
            }
        }

        // class implements INotifyPropertyChanged
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public int CompareTo(LevelPool other) => Order.CompareTo(other.Order);

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
        static void ReadXDoc(LevelPool pool, string path)
        {
            string tiledefault = "";
            string tileneed = "";
            string tileart = "";

            var doc = XDocument.Load(path);    // open levelpool file
            pool.Active = Convert.ToBoolean(doc.Root.Attribute("enabled").Value == "True");
            pool.Order = Convert.ToInt16(doc.Root.Attribute("order").Value);
            pool.Author = (doc.Root.Attribute("author").Value);
            pool.Source = doc.Root.Attribute("source").Value;
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == "lvl" || element.Name == "level")
                {
                    var level = new Level { };
                    level.Folder = pool.Folder;
                    level.TSDefault += tiledefault;
                    level.TSNeed = tileneed;
                    level.Art += tileart;
                    level.Name = element.Attribute("name").Value;
                    //level.HasSecret = Convert.ToBoolean(element.Attribute("secret").Value);
                    //level.CanReverse = Convert.ToBoolean(element.Attribute("reverse").Value);

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
                    //Console.WriteLine($"{level.Name} {level.TSNeed}");
                    pool.Levels.Add(level);
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
        }
        public static Connections GetConnections(string str)
        {
            var connections = Connections.empty;

            if (str.Contains("U")) connections |= Connections.up;
            if (str.Contains("D")) connections |= Connections.down;
            if (str.Contains("L")) connections |= Connections.left;
            if (str.Contains("R")) connections |= Connections.right;

            return connections;
        }
        public static LevelPool LoadPool(string path)
        {
            LevelPool pool = new LevelPool();
            string folder = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path)); // derive the folder name here instead of passing it in

            pool.Name = Path.GetFileNameWithoutExtension(path);
            pool.Folder = folder;
            pool.Levels = new List<Level>();

            ReadXDoc(pool, path);

            pool.NumLevels = pool.Levels.Count.ToString() + " levels";

            return pool;
        }
        public static LevelPool LoadPoolMapTest()
        {
            LevelPool pool = new LevelPool();

            pool.Name = "maptest";
            pool.Folder = "maptest";
            pool.Levels = new List<Level>();
            string tiledefault = "";
            string tileneed = "";
            string tileart = "";

            var doc = XDocument.Load($"tools/map testing/maptest.xml");    // open levelpool file
            pool.Active = Convert.ToBoolean(doc.Root.Attribute("enabled").Value == "True");
            pool.Order = Convert.ToInt16(doc.Root.Attribute("order").Value);
            pool.Author = (doc.Root.Attribute("author").Value);
            pool.Source = doc.Root.Attribute("source").Value;
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == "lvl" || element.Name == "level")
                {
                    var level = new Level { };
                    level.Folder = pool.Folder;
                    level.TSDefault += tiledefault;
                    level.TSNeed = tileneed;
                    level.Art += tileart;
                    level.Name = element.Attribute("name").Value;
                    //level.HasSecret = Convert.ToBoolean(element.Attribute("secret").Value);
                    //level.CanReverse = Convert.ToBoolean(element.Attribute("reverse").Value);
                    level.MapConnections = GetConnections(element.Attribute("connections").Value);

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
                    //Console.WriteLine($"{level.Name} {level.TSNeed}");
                    pool.Levels.Add(level);
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
            pool.NumLevels = pool.Levels.Count.ToString() + " levels";

            return pool;
        }
    }
}