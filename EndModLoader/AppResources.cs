using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    public static class AppResources
    {
        // CONSTRUCTOR
        static AppResources()
        {
            LoadLevelPools();
            LoadPiecePools();
            LoadShadersList();
            MainSettings = new SettingsFile("default");
        }

        // MEMBERS

        // Paths for loading resources
        const string LevelPoolPath = "data/levelpools";
        const string PiecePoolPath = "data/piecepools";

        // Pool Categories and Piece Pools are loaded here first
        public static ObservableCollection<LevelPoolCategory> LevelPoolCategories;
        public static ObservableCollection<PiecePool> PiecePools;

        // Settings are loaded here first
        public static SettingsFile MainSettings;

        // A list of all the loaded shaders is stored in this class.
        public static List<Shader> ShadersList;

        // Basic things needed in multiple classes
        public static UInt32 GameSeed;

        // List of area types, used in multiple places
        public static string[] AreaTypes = { "normal", "dark", "cart", "ironcart", "glitch" };

        // METHODS

        // Resource loading functions
        static void LoadShadersList()
        {
            ShadersList = new List<Shader>() { };

            var doc = XDocument.Load($"data/tilesets_pools.xml");    // open levelpool file
            foreach (var element in doc.Root.Element("shaders").Elements())
            {
                var shader = new Shader() { };

                shader.Name = element.Name.ToString();
                shader.Enabled = Convert.ToBoolean(element.Attribute("enabled").Value);
                shader.Content = element.Attribute("content").Value;

                ShadersList.Add(shader);
            }
        }
        public static void SaveShadersList(List<Shader> ShadersList)
        {
            if (ShadersList != null)
            {
                var doc = XDocument.Load($"data/tilesets_pools.xml");    // open levelpool file
                foreach (var shader in ShadersList)
                {
                    doc.Root.Element("shaders").Element(shader.Name).Attribute("enabled").Value = Convert.ToString(shader.Enabled);
                }

                doc.Save($"data/tilesets_pools.xml");
            }
        }
        static void LoadLevelPools()
        {
            LevelPoolCategories = new ObservableCollection<LevelPoolCategory>();
            foreach (var dir in Directory.GetDirectories(LevelPoolPath, "*", SearchOption.TopDirectoryOnly))
            {
                var folder = Path.GetFileNameWithoutExtension(dir);
                var cat = new LevelPoolCategory() { Name = folder, Pools = new ObservableCollection<LevelPool>() { } };
                var tempList = new List<LevelPool>() { };
                string author = null;
                bool enabled = false;
                foreach (var file in Directory.GetFiles($"{LevelPoolPath}/{folder}", "*.xml", SearchOption.TopDirectoryOnly))
                {
                    var pool = LevelPool.LoadPool(Path.GetFileNameWithoutExtension(file), folder);    // pool creation done in Pool constructor
                    if (pool != null)
                    {
                        if (pool.Author != null)        // set category author
                        {
                            if (author == null)         // if author not already set, set it
                                author = pool.Author;
                            else if (author != pool.Author)  // if there is more than one pool author in the category, set to V.A.
                                author = "V.A.";
                        }
                        if (pool.Active == true)
                            enabled = true;
                        tempList.Add(pool);
                    }
                }
                cat.Enabled = enabled;
                cat.Author = author;
                cat.Pools = tempList.OrderBy(p => p.Order);
                LevelPoolCategories.Add(cat);
            }
        }
        static void LoadPiecePools()
        {
            var folder = Path.GetFileNameWithoutExtension(PiecePoolPath);
            PiecePools = new ObservableCollection<PiecePool>();
            foreach (var file in Directory.GetFiles(PiecePoolPath, "*.xml", SearchOption.TopDirectoryOnly))
            {
                var pool = PiecePool.LoadPiecePool(Path.GetFileNameWithoutExtension(file), folder);    // pool creation done in Pool constructor
                if (pool != null)
                    PiecePools.Add(pool);
            }
        }
    }
}
