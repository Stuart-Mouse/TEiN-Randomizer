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
        public PoolType Type { get; set; }
        private bool _enabled { get; set; }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
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

            string levels_path;
            {
                // Load Pool Header Info
                GonObject header = gon["header"];
                pool.Order  = header["order"].Int();
                pool.Author = header["author"].String();
                pool.Source = header["source"].String();

                // This is the path for the level files in the pool
                {
                    if (header.TryGetChild("path", out GonObject alt_path))
                    {
                        levels_path = alt_path.String();
                    }
                    else levels_path = path + "/tilemaps/";
                }
            }

            // Load Pool Tilesets Info
            Tileset pool_default = new Tileset();    // Intialize to empty tileset in case
            Tileset pool_need    = new Tileset();    // no definition is given in pool file

            // Set area tileset defaults and needs if not null
            {
                if (gon.TryGetChild("tileset", out GonObject gon_tileset))
                {
                    if (gon_tileset.TryGetChild("default", out GonObject gon_default))
                        pool_default = new Tileset(gon_default);
                    if (gon_tileset.TryGetChild("need", out GonObject gon_need))
                        pool_need = new Tileset(gon_need);
                }
            }

            // Iterate over levels and add them to pool
            GonObject gon_levels = gon["levels"];        // not even going to check if this exists bc if it doesn't the pool shouldn't exist

            for (int i = 0; i < gon["levels"].Size(); i++)
            {
                // Get level object from gon file
                GonObject gon_level = gon_levels[i];
                
                // Create new level file
                Level level     = new Level();

                // Set basic level info
                level.Path      = levels_path;
                level.TSDefault = pool_default;
                level.TSNeed    = pool_need;
                level.Name      = gon_level.GetName();

                // Set connections
                if (gon_level.TryGetChild("connections", out GonObject gonConnections))
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

                // Set level tags
                /*if (gon_level.TryGetChild("tags", out GonObject flags))
                {
                    if (flags.Contains("npc_1"))
                        level.Flags |= Level.LevelFlags.npc_1;
                    if (flags.Contains("npc_2"))
                        level.Flags |= Level.LevelFlags.npc_2;
                    if (flags.Contains("npc_3"))
                        level.Flags |= Level.LevelFlags.npc_3;

                    if (flags.Contains("Key"))
                        level.Flags |= Level.LevelFlags.Key;
                    if (flags.Contains("Lock"))
                        level.Flags |= Level.LevelFlags.Lock;

                    if (flags.Contains("ExitWarp"))
                        level.Flags |= Level.LevelFlags.ExitWarp;
                    if (flags.Contains("WarpPoint"))
                        level.Flags |= Level.LevelFlags.WarpPoint;

                    if (flags.Contains("Collectable"))
                        level.Flags |= Level.LevelFlags.Collectable;
                    if (flags.Contains("MegaTumor"))
                        level.Flags |= Level.LevelFlags.MegaTumor;
                    if (flags.Contains("Cart"))
                        level.Flags |= Level.LevelFlags.Cart;
                }*/

                // Set level tileset defaults and needs if not null
                if (gon_level.TryGetChild("tileset", out GonObject gon_tileset))
                {
                    if (gon_tileset.TryGetChild("default", out GonObject gon_default))
                        level.TSDefault += new Tileset(gon_default);
                    if (gon_tileset.TryGetChild("need", out GonObject gon_need))
                        level.TSNeed += new Tileset(gon_need);
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

            // Create new level pool
            LevelPool pool = new LevelPool();

            // Set pool name
            pool.Name = Path.GetFileNameWithoutExtension(path);

            // Create new list of levels
            pool.Levels = new List<Level>();

            // Read the gon file data for the pool
            // Open LevelPool File
            GonObject gon = GonObject.Load(path);

            string levels_path;
            {
                // Load Pool Header Info
                GonObject header = gon["header"];
                pool.Order  = header["order"].Int();
                pool.Author = header["author"].String();
                pool.Source = header["source"].String();
                {
                    if (header.TryGetChild("type", out GonObject type))
                    {
                        switch (type.String())
                        {
                            case "standard":
                                pool.Type = PoolType.Standard; break;
                            case "connector":
                                pool.Type = PoolType.Connector; break;
                            case "secret":
                                pool.Type = PoolType.Secret; break;
                            case "cart":
                                pool.Type = PoolType.Cart; break;
                            default:
                                throw new Exception("Tried to load level pool with invalid pool type");
                        }
                    } else pool.Type = PoolType.Standard;
                }
                // This is the path for the level files in the pool
                {
                    if (header.TryGetChild("path", out GonObject alt_path))
                    {
                        levels_path = alt_path.String();
                    }
                    else levels_path = Path.GetDirectoryName(path) + "/tilemaps/";
                }
            }

            // Load Pool Tilesets Info
            Tileset pool_default = new Tileset();    // Intialize to empty tileset in case
            Tileset pool_need    = new Tileset();    // no definition is given in pool file

            // Set area tileset defaults and needs if not null
            {
                if (gon.TryGetChild("tileset", out GonObject gon_tileset))
                {
                    if (gon_tileset.TryGetChild("default", out GonObject gon_default))
                        pool_default = new Tileset(gon_default);
                    if (gon_tileset.TryGetChild("need", out GonObject gon_need))
                        pool_need = new Tileset(gon_need);
                }
            }

            // Iterate over levels and add them to pool
            GonObject gon_levels = gon["levels"];        // not even going to check if this exists bc if it doesn't the pool shouldn't exist

            for (int i = 0; i < gon["levels"].Size(); i++)
            {
                // Get level object from gon file
                GonObject gon_level = gon_levels[i];

                // Create new level file
                Level level = new Level();

                // Set basic level info
                level.Path = levels_path;
                level.TSDefault = pool_default;
                level.TSNeed = pool_need;
                level.Name = gon_level.GetName();

                // Set connections
                if (gon_level.TryGetChild("connections", out GonObject gonConnections))
                {
                    // Cardinals
                    if (gonConnections.TryGetChild("up", out GonObject gonDir))
                        level.MapConnections.U = GetConnectionType(gonDir.String());
                    else level.MapConnections.U = new ConnectionType();
                    if (gonConnections.TryGetChild("down", out gonDir))
                        level.MapConnections.D = GetConnectionType(gonDir.String());
                    else level.MapConnections.D = new ConnectionType();
                    if (gonConnections.TryGetChild("left", out gonDir))
                        level.MapConnections.L = GetConnectionType(gonDir.String());
                    else level.MapConnections.L = new ConnectionType();
                    if (gonConnections.TryGetChild("right", out gonDir))
                        level.MapConnections.R = GetConnectionType(gonDir.String());
                    else level.MapConnections.R = new ConnectionType();
                    // Diagonals
                    if (gonConnections.TryGetChild("upright", out gonDir))
                        level.MapConnections.UR = GetConnectionType(gonDir.String());
                    else level.MapConnections.UR = new ConnectionType();
                    if (gonConnections.TryGetChild("downright", out gonDir))
                        level.MapConnections.DR = GetConnectionType(gonDir.String());
                    else level.MapConnections.DR = new ConnectionType();
                    if (gonConnections.TryGetChild("upleft", out gonDir))
                        level.MapConnections.UL = GetConnectionType(gonDir.String());
                    else level.MapConnections.UL = new ConnectionType();
                    if (gonConnections.TryGetChild("downleft", out gonDir))
                        level.MapConnections.DL = GetConnectionType(gonDir.String());
                    else level.MapConnections.DL = new ConnectionType();
                }

                // Set level tileset defaults and needs if not null
                if (gon_level.TryGetChild("tileset", out GonObject gon_tileset))
                {
                    if (gon_tileset.TryGetChild("default", out GonObject gon_default))
                        level.TSDefault += new Tileset(gon_default);
                    if (gon_tileset.TryGetChild("need", out GonObject gon_need))
                        level.TSNeed += new Tileset(gon_need);
                }

                // Add level to pool
                pool.Levels.Add(level);
            }

            // Set the pool to active based on saved settings
            pool.Enabled = false;
            foreach (string str in AppResources.MainSettings.ActiveLevelPools)
            {
                if (str == pool.Name) pool.Enabled = true;
            }

            // Set the number of levels (maybe this can be removed)c
            pool.NumLevels = pool.Levels.Count.ToString() + " levels";

            // Return the pool that we have just created.
            return pool;
        }
    }
}