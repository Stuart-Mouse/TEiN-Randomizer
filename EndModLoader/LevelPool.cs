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
        // MEMBERS

        // Most of these are auto-implemented properties(?) (i think thats what they're called)
        // This enables them to be used as data bindings in the menus
        public string Name { get; set; }
        public List<Level> Levels { get; set; }
        public string NumLevels { get; set; }
        public int Order { get; set; }
        //public string Folder { get; set; }
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

        // Class implements INotifyPropertyChanged
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public int CompareTo(LevelPool other) => Order.CompareTo(other.Order);


        // METHODS

        // Function for reading the level pool gon file
        static void ReadGon(LevelPool pool, string path)
        {
            // This function is called by the LoadPool function.
            // Its purpose is to do the actual gon-reading portion of the pool construction.
            
            // Open LevelPool File
            GonObject gon = GonObject.Load(path);

            string levelsPath;
            {
                // Load Pool Header Info
                GonObject header = gon["header"];
                pool.Order  = header["order"].Int();
                pool.Author = header["author"].String();
                pool.Source = header["source"].String();

                // This is the path for the level files in the pool
                levelsPath = header["path"].String();
            }

            // Load Pool Tilesets Info
            Tileset areaDefault = new Tileset();    // Intialize to empty tileset in case
            Tileset areaNeed    = new Tileset();    // no definition is given in pool file

            // Set area tileset defaults and needs if not null
            {
                if (gon.TryGetChild("tileset", out GonObject gonTileset))
                {
                    if (gon.TryGetChild("default", out GonObject gonDefault))
                        areaDefault = new Tileset(gonDefault);
                    if (gon.TryGetChild("need", out GonObject gonNeed))
                        areaNeed = new Tileset(gonNeed);
                }
            }

            // Iterate over levels and add them to pool
            GonObject gonLevels = gon["levels"];        // not even going to check if this exists bc if it doesn't the pool shouldn't exist

            for (int i = 0; i < gon["levels"].Size(); i++)
            {
                // Get level object from gon file
                GonObject gonLevel = gonLevels[i];
                
                // Create new level file
                Level level     = new Level { };

                // Set basic level info
                level.Path      = levelsPath;
                level.TSDefault = areaDefault;
                level.TSNeed    = areaNeed;
                level.Name      = gonLevel.GetName();

                // Set connections
                if (gonLevel.TryGetChild("connections", out GonObject gonConnections))
                {
                    if (gonConnections.TryGetChild("up", out GonObject gonUp))
                        level.MapConnections.U = GetConnectionType(gonUp.String());
                    else level.MapConnections.U = new ConnectionType();
                    if (gonConnections.TryGetChild("down", out GonObject gonDown))
                        level.MapConnections.D = GetConnectionType(gonDown.String());
                    else level.MapConnections.D = new ConnectionType();
                    if (gonConnections.TryGetChild("left", out GonObject gonLeft))
                        level.MapConnections.L = GetConnectionType(gonLeft.String());
                    else level.MapConnections.L = new ConnectionType();
                    if (gonConnections.TryGetChild("right", out GonObject gonRight))
                        level.MapConnections.R = GetConnectionType(gonRight.String());
                    else level.MapConnections.R = new ConnectionType();
                }

                // Set flags
                if (gonLevel.TryGetChild("flags", out GonObject flags))
                {
                    if (gonConnections.Contains("npc_1"))
                        level.Flags |= Level.LevelFlags.npc_1;
                    if (gonConnections.Contains("npc_2"))
                        level.Flags |= Level.LevelFlags.npc_2;
                    if (gonConnections.Contains("npc_3"))
                        level.Flags |= Level.LevelFlags.npc_3;

                    if (gonConnections.Contains("Key"))
                        level.Flags |= Level.LevelFlags.Key;
                    if (gonConnections.Contains("Lock"))
                        level.Flags |= Level.LevelFlags.Lock;

                    if (gonConnections.Contains("ExitWarp"))
                        level.Flags |= Level.LevelFlags.ExitWarp;
                    if (gonConnections.Contains("WarpPoint"))
                        level.Flags |= Level.LevelFlags.WarpPoint;

                    if (gonConnections.Contains("Collectable"))
                        level.Flags |= Level.LevelFlags.Collectable;
                    if (gonConnections.Contains("MegaTumor"))
                        level.Flags |= Level.LevelFlags.MegaTumor;
                    if (gonConnections.Contains("Cart"))
                        level.Flags |= Level.LevelFlags.Cart;
                }

                // Set level tileset defaults and needs if not null
                if (gon.TryGetChild("tileset", out GonObject gonTileset))
                {
                    if (gonTileset.TryGetChild("default", out GonObject gonDefault))
                        level.TSDefault += new Tileset(gonDefault);
                    if (gonTileset.TryGetChild("need", out GonObject gonNeed))
                        level.TSNeed += new Tileset(gonNeed);
                }
                
                // Add level to pool
                pool.Levels.Add(level);
            }
        }
        public static ConnectionType GetConnectionType(string str)
        {
            // This function's purpose is to convert the string representing a connection type in the data files
            // into the actual formal ConnectionType value that will be used in map generation.
            
            // Create new empty connection type
            ConnectionType connection = new ConnectionType();

            // Search for the characters, representing each flag, in the string
            if (str.Contains("E")) connection |= ConnectionType.entrance;
            if (str.Contains("X")) connection |= ConnectionType.exit;
            if (str.Contains("S")) connection |= ConnectionType.secret;
            if (str.Contains("L")) connection |= ConnectionType.locked;

            // Return the connection type
            return connection;
        }
        public static LevelPool LoadPool(string path)
        {
            // This function is used to load a level pool from a Gon file into the internal LevelPool type.
            // It calls the ReadGon sub-function in order to do that actual gon reading portion of the process,
            // but honestly that could just be inlined and it would make no difference.
            
            // Create new level pool
            LevelPool pool = new LevelPool();

            // Set pool name
            pool.Name = Path.GetFileNameWithoutExtension(path);

            // Create new list of levels
            pool.Levels = new List<Level>();

            // Read the gon file data for the pool
            ReadGon(pool, path);

            // Set the pool to active based on saved settings
            pool.Active = GetIfActive(pool.Name);

            // Set the number of levels (maybe this can be removed)
            pool.NumLevels = pool.Levels.Count.ToString() + " levels";

            // Return the pool that we have just created.
            return pool;
        }
        static bool GetIfActive(string name)
        {
            // The set of active pools is stored as a string array.
            // When loading, we search for the pool's name in the set of active pools.
            // If found, we active the pool.
            // The pool's name is simply saved into this array when saving.

            var settings = AppResources.MainSettings;
            foreach (string str in settings.ActiveLevelPools)
            {
                if (str == name) return true;
            }
            return false;
        }
    }
}