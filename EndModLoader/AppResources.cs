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
            AttachToTS = new ObservableCollection<string>(File.ReadAllLines("data/text/AttachToTS.txt"));
        }

        // MEMBERS

        // Paths for saving and loading mods
        public const string ModPath = "mods/";
        public const string SavedRunsPath = "saved runs/";

        // Paths for loading resources
        public const string LevelPoolPath = "data/level pools/";
        public const string PiecePoolPath = "data/piecepools/";

        // Pool Categories and Piece Pools are loaded here first
        public static ObservableCollection<LevelPoolCategory> LevelPoolCategories;
        public static ObservableCollection<PiecePool> PiecePools;

        // Settings are loaded here first
        public static SettingsFile MainSettings = new SettingsFile();

        // A list of all the loaded shaders is stored in this class.
        public static List<Shader> ShadersList = new List<Shader>();

        // Basic things needed in multiple classes
        public static UInt32 GameSeed = RNG.GetUInt32();

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
                Shader shader = new Shader() { };
                GonObject item = gon[i];

                shader.Name = item.GetName();
                //shader.Enabled = item["enabled"].Bool();

                // Load shader content
                if (item["fx_shader_mid"] != null)
                    shader.fx_shader_mid = item["fx_shader_mid"].String();
                if (item["midfx_graphics"] != null)
                    shader.midfx_graphics = item["midfx_graphics"].String();
                if (item["midfx_layer"] != null)
                    shader.midfx_layer = item["midfx_layer"].Int();
                if (item["shader_param"] != null)
                    shader.shader_param = item["shader_param"].Number();

                // The set of active shaders is stored as a string array.
                // When loading, we search for the shader's name in the set of active shaders.
                // If found, we active the shader.
                // The shader's name is simply saved into this array when saving.
                var settings = AppResources.MainSettings;
                foreach (string str in settings.ActiveShaders)
                {
                    if (str == shader.Name) shader.Enabled = true;
                }

                ShadersList.Add(shader);
            }

            

            

        }
        static void LoadLevelPoolCategories()
        {
            // This function loads all of the playable level pools into the randomizer.
            // This is performed during the boot-up process of the program.
            // If these pools are unable to load, then the program will not run correctly (or at all).
            
            // Create new list of level pool categories
            LevelPoolCategories = new ObservableCollection<LevelPoolCategory>();
            
            // Iterate over directories in level pools folder
            foreach (var dir in Directory.GetDirectories(LevelPoolPath, "*", SearchOption.TopDirectoryOnly))
            {
                // Get the directory name of the pool
                string folder = Path.GetFileName(dir);

                // If the folder begins with '.' then do not process it.
                // Folders beginning with '.' are used for templated levels and other stuff the randomizer needs.
                // These are not intended to be your typical playable leves.
                if (folder[0] == '.') continue;

                // Create new level pool category
                LevelPoolCategory cat = new LevelPoolCategory() { Name = folder, Pools = new ObservableCollection<LevelPool>() { } };
                
                // Create temporary list of levels
                List<LevelPool> tempList = new List<LevelPool>() { };

                // Initialize category.enabled to false
                cat.Enabled = false;

                // Iterate over all level pools in the category directory
                foreach (var file in Directory.GetFiles($"{LevelPoolPath}/{folder}", "*.gon", SearchOption.TopDirectoryOnly))
                {
                    // Create new level pool from file
                    LevelPool pool = LevelPool.LoadPool(file);
                    
                    // If the pool is not null, set category info and add to temp list
                    if (pool != null)
                    {
                        if (pool.Author != null)                // set category author
                        {
                            if (cat.Author == null)             // if author not already set, set it
                                cat.Author = pool.Author;
                            else if (cat.Author != pool.Author) // if there is more than one pool author in the category, set to V.A.
                                cat.Author = "V.A.";
                        }
                        if (pool.Active == true)                // if the pool is enabled, enabled the category
                            cat.Enabled = true;
                        tempList.Add(pool);
                    }
                }

                // Reorder temporary list and save to pool category
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
                var pool = PiecePool.LoadPiecePool(Path.GetFileNameWithoutExtension(file), folder);
                if (pool != null)
                    PiecePools.Add(pool);
            }
        }
    }
}
