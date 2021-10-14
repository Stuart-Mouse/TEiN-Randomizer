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
            LoadLevelPoolCategories();
            LoadPiecePools();
            LoadShadersList();
            MainSettings = new SettingsFile();
            AttachToTS = new ObservableCollection<string>(File.ReadAllLines("data/text/AttachToTS.txt"));
        }

        // MEMBERS

        // Paths for saving and loading mods
        public const string ModPath = "mods/";
        public const string SavedRunsPath = "saved runs/";

        // Paths for loading resources
        public const string LevelPoolPath = "data/levelpools/";
        public const string PiecePoolPath = "data/piecepools/";

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

        // AttachToTS stored as a List<string> so that it is a refence type and doesn't need to be passed around
        public static ObservableCollection<string> AttachToTS;

        // METHODS

        // Resource loading functions
        static void LoadShadersList()
        {
            ShadersList = new List<Shader>() { };

            var gon = GonObject.Load($"data/text/shaders.gon");    // open levelpool file
            for (int i = 0; i < gon.Size(); i++)
            {
                var shader = new Shader() { };
                var item = gon[i];

                shader.Name = item.GetName();
                shader.Enabled = item["enabled"].Bool();
                shader.Content = item["content"].String();

                ShadersList.Add(shader);
            }
        }
        public static void SaveShadersList(List<Shader> ShadersList)
        {
            if (ShadersList != null)
            {
                var gon = new GonObject();
                foreach (var shader in ShadersList)
                {
                    var item = new GonObject();
                    item.InsertChild(GonObject.Manip.FromBool(shader.Enabled, "enabled"));
                    item.InsertChild(GonObject.Manip.FromString(shader.Content, "content"));
                    gon.InsertChild(shader.Name, item);
                }

                gon.Save($"data/text/shaders.gon");
            }
        }
        static void LoadLevelPoolCategories()
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
                    var pool = LevelPool.LoadPool(file);    // pool creation done in Pool constructor
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
