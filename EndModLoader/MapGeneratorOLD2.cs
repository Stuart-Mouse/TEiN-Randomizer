/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEiNRandomizer
{
    public class MapGenerator
    {
        // MapArea class is used to keep track of map areas as they are created/placed
        // The main map generation routines have been moved from the map generator class into the mapArea class

        // STATIC MEMBERS

        // Gon Object Containing the area definitions
        // The necessary values will be loaded in as they are needed (in theory, these could all be loaded in at once to slightly save on memory, but that is not a major concern atm)
        //private static GonObject AreaDefinitions = GonObject.Load("data/text/area_defs.txt");
        private static Dictionary<string, AreaDef> AreaDefinitions;

        // Reference to main settings
        private static SettingsFile Settings = AppResources.MainSettings;

        // static empty mapConnections instance for comparison purposes
        public static readonly MapConnections NoMapConnections;

        // The area defs to be used in generation
        // When referenced by an AreaEnd, they will be loaded into the queue for building
        //static List<AreaDef> AreaDefs;

        // For keeping track of keys
        static int KeysPlaced;
        static int LocksPlaced;

        // static dots screen
        static readonly MapScreen DotScreen = new MapScreen("..", ScreenType.Dots, null);

        // array used to store tag id for each direction so we can iterate over the directions easily
        // This is initialized every time this function is run, so this is probably not optimal
        static readonly TileID[] transTagIDs = { TileID.GreenTransitionR, TileID.GreenTransitionL, TileID.GreenTransitionD, TileID.GreenTransitionU };

        // INSTANCE AREA DEFINITION VARIABLES

        // The number of gameplay levels that need to be generated in the area
        public int LevelQuota;

        // These are the area IDs of the areas to branch off of this one (that make sense?)
        // We will need to reserve an OpenEnd for each AreaEnd
        public string[] AreaEnds;

        // These are the maximum dimensions of the area
        // Will only be computed if not equal to zero or null
        // Order is height, width
        public Pair Bounds;

        // MaxBuilt and MinBuilt area used to track which rows and columns are empty in an area's map.
        // This is used to crop the map later
        public Pair MinBuilt;
        public Pair MaxBuilt;


        // This is the primary direction of building in the area
        public Directions BuildDirection;
        // This obsoletes the two below members atm, because they are the same value
        // This also matches the way area defs are defined better.
        // The only downside is that it is technically less versatile, but that doesnt matter now because the code is not complex enough to support that anyways

        // This is used to determine where in the area map to place the entry point
        //public Directions Anchor;

        // Map generation will not be allowed to place exits facing in any directions flagged in NoBuild
        //public Directions NoBuild;

        // This is the type of generation that will be used in creating the area
        // Some non-standard types will be generated in a separate sub-map that is appended to the main map after the fact
        public GenerationType Type;

        // These are settings that are unique to the specific area
        // These will override global settings
        public SettingsFile AreaSettings;

        // This is the id of the child area to be generated next
        // this will be used until support for multiple child areas is added
        public string NextAreaID;

        // INSTANCE GENERATION VARIABLES

        // Entrance level coordinates
        public Pair ECoords;
        // Exit level coordinates
        public Pair XCoords;

        // For debug purposes atm
        List<MapScreen> ChosenScreens = new List<MapScreen>();

        // Level pools are shared in common amongst all areas by default
        // These can be overriden though
        public List<Level> Levels       = Randomizer.Levels;
        public List<Level> Connectors   = Randomizer.Connectors;
        public List<Level> Secrets      = Randomizer.Secrets;

        // Basic map area information
        public string ID;
        public string Name;
        public Tileset Tileset;

        // Used to keep track of how many gameplay and connector levels have been added
        public int LevelCount;
        public int ConnectorCount;

        // The 2D array map of the area
        public GameMap Map;

        // These are the OpenEnds, DeadEntries, and SecretEnds contained within the area
        public HashSet<Pair> OpenEnds;
        //public HashSet<Pair> DeadEnds;
        public HashSet<Pair> DeadEntries;
        public HashSet<Pair> SecretEnds;

        // This will be used until support for multiple child areas is added
        public MapGenerator NextArea;

        // This is a list of all areas which branch off of this one
        //public List<MapArea> ChildAreas;

        // The default constructor randomizes and initializes the area name and tileset
        public MapGenerator()
        {
            // Initialize essential information
            ID      = 
            Name    = Randomizer.GetFunnyName();
            Tileset = TilesetManip.GetTileset();
        }
        public MapGenerator(AreaDef def)
        {
            // Initialize essential information
            ID      = def.AreaID;
            Name    = Randomizer.GetFunnyName();
            Tileset = TilesetManip.GetTileset();

            // Load info from area definition
            LevelQuota = def.LevelQuota;
            NextAreaID = def.NextAreaID;
            Bounds = def.Bounds;
            BuildDirection = def.BuildDirection;
            Type = def.Type;
            AreaSettings = def.AreaSettings;
        }
        static Dictionary<string, AreaDef> LoadAreaDefs(string path)
        {
            // Load area definitions
            Dictionary<string, AreaDef> areaDefs = new Dictionary<string, AreaDef>();
            {
                // get the root gon of the area defs file passed in
                GonObject file = GonObject.Load(path);
                
                // Loop over area defs in file
                for (int i = 0; i < file.Size(); i++)
                {
                    // Get area gon from file
                    GonObject gon = file[i];

                    // This function will load all area defs recursively and returns a reference to the starting area
                    AreaDef def;   // declare the current map area

                    // Load area def values
                    def.AreaID        = gon.GetName();
                    def.LevelQuota    = gon["levels"].Int();
                    def.Bounds.First  = gon["bounds"][0].Int();
                    def.Bounds.Second = gon["bounds"][1].Int();

                    // Get the next area id
                    {
                        if (gon.TryGetChild("nextArea", out GonObject nextArea))
                            def.NextAreaID = nextArea.String();
                        else def.NextAreaID = null;
                    }

                    // Set area settings
                    def.AreaSettings = null;

                    // Set Anchor and NoBuild based on direction
                    switch (gon["direction"].String())
                    {
                        case "up":
                            def.BuildDirection = Directions.U;
                            break;
                        case "down":
                            def.BuildDirection = Directions.D;
                            break;
                        case "left":
                            def.BuildDirection = Directions.L;
                            break;
                        case "right":
                            def.BuildDirection = Directions.R;
                            break;
                        default:
                            def.BuildDirection = Directions.D;
                            break;
                    }

                    // Set generation type
                    switch (gon["type"].String())
                    {
                        case "standard":
                            goto default;
                        case "layers":
                            def.Type = GenerationType.Layers;
                            break;
                        case "brackets":
                            def.Type = GenerationType.Brackets;
                            break;
                        case "cube":
                            def.Type = GenerationType.Cube;
                            break;
                        default:
                            def.Type = GenerationType.Standard;
                            break;
                    }

                    // Add the area def to the areaDefs dicitonary
                    areaDefs.Add(def.AreaID, def);
                }
            }

            return areaDefs;
        }
        public static GameMap GenerateGameMap()
        {
            // Begins the map generation process and returns the final game map
            
            // Load the area definitions into the static member
            // These will be accesed as needed during the recursive generation process
            AreaDefinitions = LoadAreaDefs("data/text/area_defs.gon");
            
            // Get start definition
            AreaDef startDef = AreaDefinitions["1a"];

            // Create MapGenerator instance for first area, passing in start area def
            MapGenerator startArea = new MapGenerator(startDef);

            // Begin Generation
            var result = startArea.GenerateArea();

            // Get the final map produced by the generator
            GameMap finalMap = result.Item1;
            
            // We will need extra control structures for non-standard map generation types and discontiguous areas
            


            // Return the final game map
            return finalMap;
        }

        public Tuple<GameMap, Pair, Directions> GenerateArea()
        {
            // PRELIMINARIES
            
            // Initialize Ends
            OpenEnds = new HashSet<Pair>();
            DeadEntries = new HashSet<Pair>();
            SecretEnds = new HashSet<Pair>();

            Map = new GameMap(Bounds.First, Bounds.Second);     // Initialize map to size of bounds in def
            MinBuilt = new Pair(Bounds.First, Bounds.Second);   // Initializes to highest possible value
            MaxBuilt = new Pair(0, 0);                          // Initializes to lowest possible value

            // Place starting level based on anchor point
            {
                // Initialize Entrance level coords
                // This is referenced later when connecting the areas
                ECoords = new Pair();

                // Set initial value to center of map
                ECoords.First = Bounds.First / 2;
                ECoords.Second = Bounds.Second / 2;

                // Move to edges based on build directions
                // For now, you can only anchor to one direction

                // Requirements are based on anchor point as well. 
                // The CheckNeighbors method is not going to give us what we want in this case
                // because we are placing a level on the edge of the map.
                // So, we make our own based on anchors
                MapConnections reqs = NoMapConnections;
                MapConnections nots = NoMapConnections;

                // The position of where to place the first openEnd is also going to be determined here
                // This is initialized to 0,0. In the steps below it will be modified to be an offset from the first placed screen.
                Pair end = new Pair(0, 0);

                if (BuildDirection.HasFlag(Directions.D))
                {
                    ECoords.First = 0;                  // Set entry coord y value to 0 (top edge of map)
                    reqs.U |= ConnectionType.entrance;  // require an upwards entrance
                    reqs.D |= ConnectionType.exit;      // require a downwards exit
                    end.First = 1;                      // Offset the first end y value by +1
                }
                else if (BuildDirection.HasFlag(Directions.U))
                {
                    ECoords.First = Bounds.First - 1;   // Set entry coord y value to bounds y value - 1 (bottom edge of map)
                    reqs.D |= ConnectionType.entrance;  // Require a downwards entrance
                    reqs.U |= ConnectionType.exit;      // Require an upwards exit
                    end.First = -1;                     // Offset the first end y value by -1
                }
                else if (BuildDirection.HasFlag(Directions.R))
                {
                    ECoords.Second = 0;                 // Set entry coord x value to 0 (left edge of map)
                    reqs.L |= ConnectionType.entrance;  // Require a left entrance
                    reqs.R |= ConnectionType.exit;      // Require a right exit
                    end.Second = 1;                     // Offset the first end x value by +1
                }
                else if (BuildDirection.HasFlag(Directions.L))
                {
                    ECoords.Second = Bounds.Second - 1; // Set entry coord x value to bounds x value - 1 (right edge of map)
                    reqs.R |= ConnectionType.entrance;  // Require a right entrance
                    reqs.L |= ConnectionType.exit;      // Require a left exit
                    end.Second = -1;                    // Offset the first end x value by -1
                }

                // Try to get options from Levels
                // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
                List<Level> options;
                bool isGameplay = true;
                options = GetOptionsClosed(reqs, nots, Levels);

                // If there are not any options in Levels
                if (options.Count == 0)
                {
                    options = GetOptionsClosed(reqs, nots, Connectors);    // Try to get options from Connectors
                    isGameplay = false;                                    // Set isGameplay to false since we are now using a connector
                }

                // Get random level from the list of options
                Level level = options[RNG.random.Next(0, options.Count)];

                // Create screen to place
                MapScreen screen;
                if (isGameplay)
                    screen = new MapScreen($"{ID}-{LevelCount + 1}", ScreenType.Level, level);
                else
                    screen = new MapScreen($"{ID}-x{ConnectorCount+1}", ScreenType.Connector, level);

                // increment the correct screen counter
                if (isGameplay)
                    LevelCount++;
                else ConnectorCount++;

                // place new screen
                PlaceScreen(ECoords.First, ECoords.Second, screen);
                ChosenScreens.Add(screen);

                // Open End location is end offset + Ecoord
                end += ECoords;

                // Place open end
                OpenEnds.Add(end);
            }

            // MAIN LOOP

            // add levels until the quota is met ( minus 1 )
            while (LevelCount < LevelQuota-1)
            {
                // Place next level
                PlaceNext();

                // Check that the OpenEnds count has not dropped below 1
                if (OpenEnds.Count == 0)
                {
                    //Console.WriteLine($"Ran out of OpenEnds in area: {ID}");
                    PrintDebugCSV(Map, $"tools/map testing/map_{ID}.csv");
                    throw new Exception($"Ran out of OpenEnds in area: {ID}");
                }
                //while (OpenEnds.Count < Def.AreaEnds.Length)
                //{
                //    // In this order try:
                //    // Replace a connector with one that has more connections
                //    // Search for all connectors in area
                //    // Order options by most empty neighbors
                //    // Select a random option and add the new OpenEnds
                //    // Replace a level with one that has more connections
                //    // Replace a level with a connector that has more connections

                //    // If all those fail, then fuck

                //    // This might not even be necessary if we keep the count >= 1 by necessity
                //    // Still, strange things could happen at the edges, and it might become an important check to make
                //}
            }

            // Select one OpenEnd as the AreaEnd
            // This will be used later as either the connection point to the next area, or as the path end.
            PlaceFinalLevel();
            // If there is no next area, we need to instead place a path end screen, not just the usual final level

            // debug stuff
            Console.WriteLine($"Level Count: {LevelCount}");
            Console.WriteLine($"Connector Count: {OpenEnds.Count}");
            Console.WriteLine($"OpenEnds before capping: {OpenEnds.Count}");

            // Cap all Open Ends
            while (OpenEnds.Count != 0)
            {
                Pair coords = OpenEnds.First();
                CapOpenEnd(coords);
                OpenEnds.Remove(coords);
            }
            // Cap all Dead Ends
            while (DeadEntries.Count != 0)
            {
                Pair coords = DeadEntries.First();
                CapOpenEnd(coords);
                DeadEntries.Remove(coords);
            }

            // Build secret areas

            // Crop the map
            Map = GameMap.CropMap(MaxBuilt.First - MinBuilt.First + 1, MaxBuilt.Second - MinBuilt.Second + 1, Map, MinBuilt.First, MinBuilt.Second);
            // Adjust the ECoords and XCoords
            ECoords -= MinBuilt;
            XCoords -= MinBuilt;

            // Print Debug CSV after generation finishes
            PrintDebugCSV(Map, $"tools/map testing/map_{ID}.csv");

            // Declare the map to return and set default value to the Map
            GameMap returnMap = Map;

            // Generate next area
            if (NextAreaID != null)
            {
                // Get start definition
                AreaDef startDef = AreaDefinitions[NextAreaID];

                // Create MapGenerator instance for first area, passing in start area def
                NextArea = new MapGenerator(startDef);

                // Begin Generation
                var genResult = NextArea.GenerateArea();

                // Combine current area and next area maps
                var comboResult = CombineMaps(Map, genResult.Item1, XCoords, genResult.Item2, BuildDirection, genResult.Item3);

                returnMap = comboResult.Item1;  // Store the result from map combination into returnMap
                ECoords += comboResult.Item2;   // Add the M1Origin from the map combination to the entry coords to adjust them for the new map

                PrintDebugCSV(returnMap, $"tools/map testing/map_{ID}r.csv");
            }
            
            // Create all necessary data files for this area
            GenerateAreaDataFiles();

            // Return the consolidated map and entry coord
            return new Tuple<GameMap, Pair, Directions> ( returnMap, ECoords, BuildDirection );

            *//*
                At the moment, the way the recursive generation works (creation of child MapGenerator instances)
                means that each child area will remain in memory until freed. This is not optimal, and a simple recursive function would be more efficient.
                However, the upside is that it will be easier to debug issues in generation since child areas will remain in memory.
                One way to solve this is to simply not store the reference to NextArea. This will likely be done later when optimizing.
            *//*
        }
        bool PlaceNext()
        {
            // This function will draw a new map screen from the list of
            // available levels and add it to the map at the position of an open end.

            // This tells us whether or not to remove the level from its pool after placing
            bool isGameplay = true;

            // Get next OpenEnd to fill
            Pair coords = SmartSelectOpenEnd();

            // Get level connection requirements
            CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);

            // Get options from pool of levels
            List<Level> options;

            // Try to get open options from Levels
            options = GetOptionsOpen(reqs, nots, Levels);

            // If there are not open options in Levels
            if (options.Count == 0)
            {
                // Try to get open options from Connectors
                options = GetOptionsOpen(reqs, nots, Connectors);
                // set isGameplay to false since we are now using a connector
                isGameplay = false;
            }

            // If there are not open options in Connectors
            // Then try to get any kind of option from Connectors
            if (options.Count == 0)
                options = GetOptions(reqs, nots, Connectors);

            // If there are still no options, we have a problem
            if (options.Count == 0)
            {
                DebugPrintReqs(coords, reqs, nots);
                return false;
            }

            // Get random level from the list of options
            Level level = options[RNG.random.Next(0, options.Count)];

            // Create screen to place
            MapScreen screen;
            if (isGameplay)
                screen = new MapScreen($"{ID}-{LevelCount + 1}", ScreenType.Level, level, directions);
            else
                screen = new MapScreen($"{ID}-x{ConnectorCount + 1}", ScreenType.Connector, level, directions);

            // Place screen
            PlaceScreen(coords.First, coords.Second, screen);
            ChosenScreens.Add(screen);


            // Add new ends after placement
            AddEnds(coords.First, coords.Second, level.MapConnections);

            // Remove the OpenEnd that we are replacing
            OpenEnds.Remove(new Pair(coords.First, coords.Second));

            // increment the correct screen counter
            if (isGameplay)
                LevelCount++;
            else ConnectorCount++;

            // Remove the newly placed screen from the list of screens available
            //if (isGameplay)
            //Levels.Remove(level);

            return true;
        }
        Pair SmartSelectOpenEnd()
        {
            // If there is only one end, use this end
            // If filling this end creates no new open ends, we will need to fix that.
            if (OpenEnds.Count == 1)
                return OpenEnds.First();

            // Iterate over the list of open ends
            for (int i = 0; i < OpenEnds.Count; i++)
            {
                // Count the number of empty neighbors each end has
                // If an end has no empty neighbors
                // Use this end to just get it out of the way
                // This will help us avoid getting stuck with it later
                Pair coords = OpenEnds.ElementAt(i);
                CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);
                if (CountEmptyNeighbors(reqs, nots) == 0)
                {
                    return coords;
                }
            }

            // Declare new openEnd to return
            Pair openEnd = new Pair(-1, -1);
            // Declare distance and set to zero
            int dist = 0;

            // Iterate over the list of open ends again
            for (int i = 0; i < OpenEnds.Count; i++)
            {
                // This time we select the openEnd by greatest distance
                Pair coords = OpenEnds.ElementAt(i);
                CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);
                if (directions.Length > dist)
                {
                    // If the distance is greater than what we had before, make this the openEnd to return
                    openEnd = coords;
                }
            }
            // If our new end has been changed from initial value
            // this probably should not ever be false
            if (openEnd.First != -1)
                return openEnd;

            // Otherwise, pick a random end
            return GetOpenEnd();
        }
        Pair GetOpenEnd()
        {
            // Return a random OpenEnd from the list
            // Will error if there are no OpenEnds
            return OpenEnds.ElementAt(RNG.random.Next(0, OpenEnds.Count));
        }
        void CheckNeighbors(int i, int j, out MapConnections reqs, out MapConnections nots, out string directions)  // returns the type necessary to meet needs of neighbors
        {
            // initialize reqs and nots to empty
            reqs = NoMapConnections;
            nots = NoMapConnections;
            directions = null;

            // map is row major, so i is the row and j is the column
            // get required entrances, and spots where entrances cant be

            // Check Screen Up
            CheckNeighbor(i - 1, j, ref reqs.U, ref nots.U, Directions.D, ref directions);

            // Check Screen Down
            CheckNeighbor(i + 1, j, ref reqs.D, ref nots.D, Directions.U, ref directions);

            // Check Screen Left
            CheckNeighbor(i, j - 1, ref reqs.L, ref nots.L, Directions.R, ref directions);

            // Check Screen Right
            CheckNeighbor(i, j + 1, ref reqs.R, ref nots.R, Directions.L, ref directions);

        }
        void CheckNeighbor(int i, int j, ref ConnectionType reqs, ref ConnectionType nots, Directions dir, ref string directions)
        {
            // Must be inside map bounds and not be a secret end
            if (MapBoundsCheck(Map, i, j) && !SecretEnds.Contains(new Pair(i, j)))
            {
                // Get the neighbor we want to check
                MapScreen neighbor = Map.Data[i, j];
                ConnectionType connection;

                // ENTRANCES AND EXITS
                // If the screen is not null, check the connections
                // If it is null, it will not impose any requirements on entrances or exits
                if (neighbor != null)
                {
                    // Get the desired connection by specifying direction
                    connection = neighbor.Level.MapConnections.GetDirection(dir);

                    // If there is an exit (or both), require an entrance
                    if (connection.HasFlag(ConnectionType.exit))
                    {
                        // Set level requirements
                        reqs |= ConnectionType.entrance;
                        // Get directions to current level
                        // Set directions to new level if shorter than existing path or if no path is set
                        if (directions == null || neighbor.Directions.Length < directions.Length)
                            directions = neighbor.Directions + (DirectionsEnum.Opposite(dir)).ToString();
                    }
                    // If there is only an entrance, require an exit
                    else if (connection.HasFlag(ConnectionType.entrance))
                        reqs |= ConnectionType.exit;
                    // If there is no connection, require a not on both
                    else nots |= ConnectionType.both;

                    // Also, if the screen is not null, then we can't have a secret here
                    nots |= ConnectionType.secret;
                }

                // SECRETS
                // If we have an OpenEnd or DeadEntry in this location, we cannot have a secret here.
                if (OpenEnds.Contains(new Pair(i, j))
                    || DeadEntries.Contains(new Pair(i, j)))
                {
                    nots |= ConnectionType.secret;
                }
            }
            // If the screen we are looking at is invalid, we cannot connect to it at all.
            else nots |= ConnectionType.all;  // all includes both entrances, exits, and secrets
        }
        void AddEnds(int i, int j, MapConnections mCons)
        {
            // This function calls AddEnd for each direction of connection

            // The index given is the screen that is Up, Down, Left, or Right from the screen just placed.
            // The ConnectionType given is the connection type of the transition leading into the given screen.

            // Check Screen Up
            AddEnd(new Pair(i - 1, j), mCons.U);

            // Check Screen Down
            AddEnd(new Pair(i + 1, j), mCons.D);

            // Check Screen Left
            AddEnd(new Pair(i, j - 1), mCons.L);

            // Check Screen Right
            AddEnd(new Pair(i, j + 1), mCons.R);

        }
        void AddEnd(Pair index, ConnectionType con)
        {
            // This function adds the applicable End type for a given screen and connection type

            // OpenEnds    are placed on any exits     which lead to null screens
            // DeadEntries are placed on any entrances which lead to null screens
            // SecretEnds  are placed on any secret entrances (these are pre-checked for compatibility)

            // Exits are checked first because they take precedence over entrances
            if (con.HasFlag(ConnectionType.exit))
            {
                // By the rules in CheckNeighbors, we shouldn't be trying to place an openEnd over an already existing secretEnd
                // So the only other consideration is whether a DeadEntry already exists, which we can overwrite.

                if (Map.Data[index.First, index.Second] == null)
                {
                    // Remove deadEntry if there was one here
                    DeadEntries.Remove(index);

                    // Add OpenEnd
                    OpenEnds.Add(index);
                    UpdateMinMaxCoords(index.First, index.Second);
                }
            }
            else if (con.HasFlag(ConnectionType.entrance))
            {
                // If we have only an entrance here, try to add a deadEntry
                // If there is already an openEnd here, don't add the deadEntry
                if (Map.Data[index.First, index.Second] == null)
                {
                    if (!OpenEnds.Contains(index))
                    {
                        DeadEntries.Add(index);
                        UpdateMinMaxCoords(index.First, index.Second);
                    }
                }
            }
            else if (con.HasFlag(ConnectionType.secret))
            {
                // Add a secret end if applicable
                // Should be no issues with adding this since we pre-check for other types of ends in this spot during CheckNeighbors
                // For SecretEnds, we don't need to check if the screen at index is null
                SecretEnds.Add(index);
                UpdateMinMaxCoords(index.First, index.Second);
            }
        }
        void PlaceFinalLevel()
        {
            // This function selects a random OpenEnd in the area to be the AreaEnd, that is the point at which the next adjacent area will begin building

            // Declare the areaEnd and set to (-1, -1)
            // If it gets returned as this value then there will be an error
            // Under normal operation this should not happen
            Pair areaEnd = new Pair(-1, -1);

            // Set distance to zero
            int dist = 0;

            MapConnections reqs = NoMapConnections;
            MapConnections nots = NoMapConnections;

            // Iterate over the list of open ends
            for (int i = 0; i < OpenEnds.Count; i++)
            {
                // Get the next end to check
                Pair coords = OpenEnds.ElementAt(i);

                // Check that the end is on the edge of the map
                bool onEdge = false;

                switch (BuildDirection) // Switch based on the build direction so we check the correct side
                {
                    case Directions.U:
                        if (coords.First == MinBuilt.First)
                            onEdge = true;
                        break;
                    case Directions.D:
                        if (coords.First == MaxBuilt.First)
                            onEdge = true;
                        break;
                    case Directions.L:
                        if (coords.Second == MinBuilt.Second)
                            onEdge = true;
                        break;
                    case Directions.R:
                        if (coords.Second == MaxBuilt.Second)
                            onEdge = true;
                        break;
                }

                if (onEdge)
                {
                    // Use checkneighbors to get the level requirements and directions of the level
                    CheckNeighbors(coords.First, coords.Second, out MapConnections treqs, out MapConnections tnots, out string directions);

                    // Find the end with the greatest distance
                    if (directions.Length > dist)
                    {
                        areaEnd = coords;           // Set the area end to the coords of the selected end
                        dist = directions.Length;   // Set the highest distance to the distance of the selected end
                        reqs = treqs;               // Set the current requirements and nots
                        nots = tnots;               // These will be used below
                    }
                }
            }

            // Error if the areaEnd is (-1, -1)
            // This can occur if the only OpenEnd is a dead end
            if (areaEnd.First == -1)
                throw new Exception("Area End is invalid.");

            // Set XCoords so we can see where the area end is
            XCoords = areaEnd;

            // Select and place the final screen
            // Manually add the exit requirements based on BuildDirection
            // This will be in addition to the 
            if (BuildDirection.HasFlag(Directions.D))
            {
                reqs.D |= ConnectionType.exit;      // require a downwards exit
                nots.D = ConnectionType.none;       // Negate and down nots from above
            }
            else if (BuildDirection.HasFlag(Directions.U))
            {
                reqs.U |= ConnectionType.exit;      // Require an upwards exit
                nots.U = ConnectionType.none;       // Negate and up nots from above

            }
            else if (BuildDirection.HasFlag(Directions.R))
            {
                reqs.R |= ConnectionType.exit;      // Require a right exit
                nots.R = ConnectionType.none;       // Negate and right nots from above

            }
            else if (BuildDirection.HasFlag(Directions.L))
            {
                reqs.L |= ConnectionType.exit;      // Require a left exit
                nots.L = ConnectionType.none;       // Negate and left nots from above

            }

            // Try to get options from Levels
            // We used closed options in this case because on the first level we don't want to branch, only meet the basic requirements
            List<Level> options;
            bool isGameplay = true;
            options = GetOptionsClosed(reqs, nots, Levels);

            // If there are not any options in Levels
            if (options.Count == 0)
            {
                options = GetOptionsClosed(reqs, nots, Connectors);     // Try to get options from Connectors
                isGameplay = false;                                     // Set isGameplay to false since we are now using a connector
            }

            // Get random level from the list of options
            Level level = options[RNG.random.Next(0, options.Count)];

            // Create screen to place
            MapScreen screen;
            if (isGameplay)
                screen = new MapScreen($"{ID}-{LevelCount + 1}", ScreenType.Level, level);
            else
                screen = new MapScreen($"{ID}-x{ConnectorCount + 1}", ScreenType.Connector, level);

            // increment the correct screen counter
            if (isGameplay)
                LevelCount++;
            else ConnectorCount++;

            // place new screen
            PlaceScreen(XCoords.First, XCoords.Second, screen);
            ChosenScreens.Add(screen);

            // Remove the selected end so that it is not capped in a later step.
            OpenEnds.Remove(areaEnd);
        }

        //bool CheckFromCellToEdge(Pair coords, Pair dir)
        //{
        //    // start at given coords + dir
        //    coords += dir;
        //    // loop while the cell being checked is within the bounds
        //    while (MapBoundsCheck(coords.First, coords.Second))
        //    {
        //        // check if the cell is not null
        //        // if so, return false (we have hit an occupied cell)
        //        if (Map.Data[coords.First, coords.Second] == null)
        //            return false;
        //        // move in direction by adding dir pair to coords being checked
        //        coords += dir;
        //    }
        //    return true;
        //}
        void CapOpenEnd(Pair coords)
        {
            // Check the neighbors to get the requirements
            CheckNeighbors(coords.First, coords.Second, out MapConnections reqs, out MapConnections nots, out string directions);

            // Get options from pool of screens
            List<Level> options = GetOptionsClosed(reqs, nots, Connectors);

            // Get random level from the list of options
            Level level = options[RNG.random.Next(0, options.Count)];

            // Place screen
            MapScreen screen = new MapScreen($"{ID}-x{ConnectorCount+1}", ScreenType.Connector, level, directions);
            ConnectorCount++;
            PlaceScreen(coords.First, coords.Second, screen);
            ChosenScreens.Add(screen);
        }
        static bool MapBoundsCheck(GameMap map, int i, int j)
        {
            if (i >= 0 && j >= 0 && i < map.Height && j < map.Width)
                return true;
            return false;
        }
        bool PlaceScreen(int i, int j, MapScreen screen)
        {
            // Check that the index is within the map's bounds
            if (MapBoundsCheck(Map, i, j))
            {
                // Print debug output to console
                if (Map.Data[i, j] != null)
                    Console.WriteLine($"Overwrote a cell at {i}, {j}");
                
                // Place the screen into the map
                Map.Data[i, j] = screen;

                UpdateMinMaxCoords(i, j);

                return true;
            }
            return false;
        }
        void UpdateMinMaxCoords(int i, int j)
        {
            // Update the minbuilt and maxbuilt coords
            MinBuilt.First  = Math.Min(MinBuilt.First , i);
            MinBuilt.Second = Math.Min(MinBuilt.Second, j);
            MaxBuilt.First  = Math.Max(MaxBuilt.First , i);
            MaxBuilt.Second = Math.Max(MaxBuilt.Second, j);
        }
        List<Level> GetOptions(MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            List<Level> options = new List<Level>();
            foreach (Level level in pool)
            {
                if ((level.MapConnections & reqs) == reqs)
                    if ((level.MapConnections & nots) == NoMapConnections)
                        options.Add(level);
            }
            return options;
        }
        List<Level> GetOptionsOpen(MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            // Gets Options the standard way, but also...
            List<Level> options = GetOptions(reqs, nots, pool);

            // Create new list of options
            List<Level> newOptions = new List<Level>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.MapConnections, reqs);

                // Skip levels which build in disallowed directions
                if ((BuildDirection.HasFlag(Directions.D) && mc.U.HasFlag(ConnectionType.exit))) continue;
                if ((BuildDirection.HasFlag(Directions.U) && mc.D.HasFlag(ConnectionType.exit))) continue;
                if ((BuildDirection.HasFlag(Directions.R) && mc.L.HasFlag(ConnectionType.exit))) continue;
                if ((BuildDirection.HasFlag(Directions.L) && mc.R.HasFlag(ConnectionType.exit))) continue;

                // Spare the level if any new exits are added
                if      (mc.U.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.D.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.L.HasFlag(ConnectionType.exit)) newOptions.Add(level);
                else if (mc.R.HasFlag(ConnectionType.exit)) newOptions.Add(level);
            }
            return newOptions;
        }
        List<Level> GetOptionsClosed(MapConnections reqs, MapConnections nots, List<Level> pool)
        {
            // Gets Options the standard way, but also...
            List<Level> options = GetOptions(reqs, nots, pool);

            // Create new list of options
            List<Level> newOptions = new List<Level>();

            // Checks if they are going to add any new OpenEnds to the map
            foreach (var level in options)
            {
                // Subtract the requirements and see what remains
                MapConnections mc = GetUniqueConnections(level.MapConnections, reqs);

                // Skip the level if any new exits are added
                if (mc.U != 0b0000) continue;
                if (mc.D != 0b0000) continue;
                if (mc.L != 0b0000) continue;
                if (mc.R != 0b0000) continue;

                // Otherwise, add it to the new list
                newOptions.Add(level);
            }
            return newOptions;
        }
        MapConnections GetUniqueConnections(MapConnections cons, MapConnections reqs)
        {
            MapConnections ret = NoMapConnections;

            // If the requirements impose anything on a given side, then that whole side is thrown out
            // If that side is empty, then that side contains unique connections
            if (reqs.U == ConnectionType.none) ret.U = cons.U;
            if (reqs.D == ConnectionType.none) ret.D = cons.D;
            if (reqs.L == ConnectionType.none) ret.L = cons.L;
            if (reqs.R == ConnectionType.none) ret.R = cons.R;

            return ret;
        }
        int CountEmptyNeighbors(MapConnections reqs, MapConnections nots)
        {
            // Set counter to zero
            int val = 0;

            // If there are any directions where no requirements are imposed, then we have no neighbor there.
            // This also means we have the potential for an OpenEnd.
            if ((reqs.U | nots.U) == ConnectionType.none) val++;
            if ((reqs.D | nots.D) == ConnectionType.none) val++;
            if ((reqs.L | nots.L) == ConnectionType.none) val++;
            if ((reqs.R | nots.R) == ConnectionType.none) val++;

            // Return the number of empty neighbors counted.
            return val;
        }

        // Static functions
        public static void DebugPrintReqs(Pair coords, MapConnections reqs, MapConnections nots)
        {
            Console.WriteLine($"Unable to place level at {coords.First}, {coords.Second}");
            Console.WriteLine($"reqs:");
            Console.WriteLine($"\tU: {reqs.U}");
            Console.WriteLine($"\tD: {reqs.D}");
            Console.WriteLine($"\tL: {reqs.L}");
            Console.WriteLine($"\tR: {reqs.R}");
            Console.WriteLine($"nots:");
            Console.WriteLine($"\tU: {nots.U}");
            Console.WriteLine($"\tD: {nots.D}");
            Console.WriteLine($"\tL: {nots.L}");
            Console.WriteLine($"\tR: {nots.R}");
        }
        public Tuple<GameMap, Pair> CombineMaps(GameMap M1, GameMap M2, Pair M1Exit, Pair M2Entry, Directions BD1, Directions BD2)
        {
            // This function will combine the two maps entered as parameters into a single map
            // The given coords are used as the point at which the levels are joined

            // The first coord should be the exit coord of the first map + the offset of where the first screen of the second map should be placed
            // It is important not to forget to add this offset before passing in the first coord parameter
            // The second coord is the location of the first screen (entry coord) of the second map

            // Declare map to be returned
            GameMap newMap;

            // Initialize origin coords for each map
            Pair M1Origin = new Pair(), M2Origin = new Pair();

            // Initialize dimensions for new map
            int height = 0, width = 0;

            // Initialize connection requirements for connecting level
            MapConnections reqs = NoMapConnections;
            MapConnections nots = NoMapConnections;

            // Connect maps with same BuildDirection
            if (BD1 == BD2)
            {
                // This process is basically the same as what is used in the level generator

                // Determine the origin/offset of each map based on direction
                switch (BD1)
                {
                    case Directions.U:
                        if (M1Exit.Second < M2Entry.Second)                   // If M1Exit is higher than M2Entry
                            M1Origin.Second = M2Entry.Second - M1Exit.Second;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.Second = 0;                            // Otherwise, M1Origin y val is 0
                        M1Origin = M2Origin - M1Exit + M2Entry;             // M2Origin = M1Origin + Difference of M1Exit and M2Entry
                        M1Origin.First++;                                  // Shift M2Entry to the right by one
                        width = Math.Max(M1Origin.Second + M1.Width, M2Origin.Second + M2.Width);
                        height = M2Entry.First + M1.Height - M1Exit.First + 1;
                        break;

                    case Directions.D:
                        if (M1Exit.Second < M2Entry.Second)                   // If M1Exit is higher than M2Entry
                            M1Origin.Second = M2Entry.Second - M1Exit.Second;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.Second = 0;                            // Otherwise, M1Origin y val is 0
                        M2Origin = M1Origin + M1Exit - M2Entry;             // M2Origin = M1Origin + Difference of M1Exit and M2Entry
                        M2Origin.First++;                                  // Shift M2Entry to the right by one
                        width = Math.Max(M1Origin.Second + M1.Width, M2Origin.Second + M2.Width);
                        height = M1Exit.First + M2.Height - M2Entry.First + 1;
                        break;

                    case Directions.L:
                        if (M1Exit.First < M2Entry.First)                   // If M1Exit is higher than M2Entry
                            M1Origin.First = M2Entry.First - M1Exit.First;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.First = 0;                            // Otherwise, M1Origin y val is 0
                        M1Origin = M2Origin - M1Exit + M2Entry;             // 
                        M1Origin.Second++;                                  // Shift M1Origin to the left by one
                        height = Math.Max(M1Origin.First + M1.Height, M2Origin.First + M2.Height);
                        width = M2Entry.Second + M1.Width - M1Exit.Second + 1;
                        break;

                    case Directions.R:
                        if (M1Exit.First < M2Entry.First)                   // If M1Exit is higher than M2Entry
                            M1Origin.First = M2Entry.First - M1Exit.First;  // M1Exit is pushed downwards to line up with M2entry
                        else M1Origin.First = 0;                            // Otherwise, M1Origin y val is 0
                        M2Origin = M1Origin + M1Exit - M2Entry;             // M2Origin = M1Origin + Difference of M1Exit and M2Entry
                        M2Origin.Second++;                                  // Shift M2Origin to the right by one
                        height = Math.Max(M1Origin.First + M1.Height, M2Origin.First + M2.Height);
                        width = M1Exit.Second + M2.Width - M2Entry.Second + 1;
                        break;

                    default:
                        throw new Exception("Error combining maps: Invlaid Direction.");
                }

                // Create new map with required size
                newMap = new GameMap(height, width);

                // Combine the maps by pasting into new map
                GameMap.CopyToMapAtCoords(M1, newMap, M1Origin);
                GameMap.CopyToMapAtCoords(M2, newMap, M2Origin);

                // Return the new, combined GameMap and the Coords of the M1Origin
                return new Tuple<GameMap, Pair>(newMap, M1Origin);
            }

            // Connect maps with perpendicular BuildDirections
            Directions bdSum = BD1 | BD2;

            // Check for conflicting directions
            if (bdSum.HasFlag(Directions.R | Directions.L) || bdSum.HasFlag(Directions.U | Directions.D))
                throw new Exception("Error combining maps: Conflicting Directions.");

            // The current method is a bit space inefficient, since it may produce large empty spaces in the connecting corners
            // These spaces will get very large if building in spirals

            Pair ConCoords = new Pair();    // Declare the coords of the connecting screen
            Pair ConVec = new Pair();       // Declare a pair for the directions of the .. fill

            // Determine how maps will be connected based on directions
            // Switch based on the directions being connected
            // The order of these actually matters
            switch (BD1)
            {
                case Directions.R when BD2 == Directions.U:
                    
                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Origin.First + M1Exit.First, M2Origin.Second + M2Entry.Second);

                    // Set the width and height
                    height = M1Origin.First  + M1.Height;
                    width  = M2Origin.Second + M2.Width ;

                    // Set the fill vector
                    ConVec = new Pair(-1, -1);

                    // Set the requirements
                    reqs.L |= ConnectionType.both;
                    reqs.U |= ConnectionType.both;

                    break;

                case Directions.R when BD2 == Directions.D:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Exit.First, M2Origin.Second + M2Entry.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M2Origin.Second + M2.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, -1);

                    // Set the requirements
                    reqs.L |= ConnectionType.both;
                    reqs.D |= ConnectionType.both;

                    break;

                case Directions.L when BD2 == Directions.U:

                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Origin.First + M1Exit.First, M2Entry.Second);

                    // Set the width and height
                    height = M1Origin.First + M1.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(-1, 1);

                    // Set the requirements
                    reqs.R |= ConnectionType.both;
                    reqs.U |= ConnectionType.both;

                    break;

                case Directions.L when BD2 == Directions.D:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M1Exit.First, M2Entry.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, 1);

                    // Set the requirements
                    reqs.R |= ConnectionType.both;
                    reqs.D |= ConnectionType.both;

                    break;

                case Directions.U when BD2 == Directions.R:

                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Entry.First, M1Exit.Second);

                    // Set the width and height
                    height = M1Origin.First + M1.Height;
                    width = M2Origin.Second + M2.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, 1);

                    // Set the requirements
                    reqs.D |= ConnectionType.both;
                    reqs.R |= ConnectionType.both;

                    break;

                case Directions.U when BD2 == Directions.L:

                    // Determine Map Origins
                    M1Origin.First = M2.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Entry.First, M1Origin.Second + M1Exit.Second);

                    // Set the width and height
                    height = M1Origin.First + M1.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(1, -1);

                    // Set the requirements
                    reqs.D |= ConnectionType.both;
                    reqs.L |= ConnectionType.both;

                    break;

                case Directions.D when BD2 == Directions.R:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M2Origin.Second = M1.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Origin.First + M2Entry.First, M1Origin.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M2Origin.Second + M2.Width;

                    // Set the fill vector
                    ConVec = new Pair(-1, 1);

                    // Set the requirements
                    reqs.U |= ConnectionType.both;
                    reqs.R |= ConnectionType.both;

                    break;

                case Directions.D when BD2 == Directions.L:

                    // Determine Map Origins
                    M2Origin.First = M1.Height;
                    M1Origin.Second = M2.Width;

                    // Determine Coords of connecting screen
                    ConCoords = new Pair(M2Origin.First + M2Entry.First, M1Origin.Second + M1Exit.Second);

                    // Set the width and height
                    height = M2Origin.First + M2.Height;
                    width = M1Origin.Second + M1.Width;

                    // Set the fill vector
                    ConVec = new Pair(-1, -1);

                    // Set the requirements
                    reqs.U |= ConnectionType.both;
                    reqs.L |= ConnectionType.both;

                    break;
            }

            // Create new map with required size
            newMap = new GameMap(height, width);

            // Combine the maps by pasting into new map
            GameMap.CopyToMapAtCoords(M1, newMap, M1Origin);
            GameMap.CopyToMapAtCoords(M2, newMap, M2Origin);

            // Fill in the ..s from the connector level to the entrance and exit
            {
                // Vertical Fill
                int i = ConCoords.First + ConVec.First;
                while (true)
                {
                    // Check that the index is not out of range
                    if (!MapBoundsCheck(newMap, i, ConCoords.Second)) break;

                    // Check that the cell is not occupied
                    if (newMap.Data[i, ConCoords.Second] != null) break;

                    // fill the cell with the given mapscreen (this is usually the .. level)
                    newMap.Data[i, ConCoords.Second] = DotScreen;

                    // Move to next screen
                    i += ConVec.First;
                }
                // Horizontal Fill
                int j = ConCoords.Second + ConVec.Second;
                while (true)
                {
                    // Check that the index is not out of range
                    if (!MapBoundsCheck(newMap, ConCoords.First, j)) break;

                    // Check that the cell is not occupied
                    if (newMap.Data[ConCoords.First, j] != null) break;

                    // fill the cell with the given mapscreen (this is usually the .. level)
                    newMap.Data[ConCoords.First, j] = DotScreen;

                    // Move to next screen
                    j += ConVec.Second;
                }
            }

            // Place area connector level
            {
                List<Level> options = GetOptionsClosed(reqs, nots, Connectors);     // Try to get options from Connectors
                Level level = options[RNG.random.Next(0, options.Count)];           // Get random level from the list of options
                
                MapScreen screen = new MapScreen($"AC-{ID}", ScreenType.Connector, level);  // Create screen to place

                ConnectorCount++;   // Increment the connector screen counter

                newMap.Data[ConCoords.First, ConCoords.Second] = screen;   // Place new screen

                ChosenScreens.Add(screen);  // Add the screen to ChosenScreens
            }

            // Return the new, combined GameMap and the Coords of the M1Origin
            return new Tuple<GameMap, Pair>(newMap, M1Origin);
        }
        public void GenerateAreaDataFiles()
        {
            // Open StreamWriters for the data files
            StreamWriter LevelInfo = File.AppendText(Randomizer.saveDir + "data/levelinfo.txt.append");
            StreamWriter Tilesets  = File.AppendText(Randomizer.saveDir + "data/tilesets.txt.append");

            // Set areaTileset to Map Area's tileset
            Tileset areaTileset = Tileset;

            // Write the area's tileset to the file
            Tilesets.WriteLine($"{ID} {{");
            areaTileset.WriteTileset(Tilesets);

            // Iterate over all of the screen in the area
            for (int i = 0; i < ChosenScreens.Count; i++)
            {
                // Get the next screen from the list
                MapScreen screen = ChosenScreens[i];

                // Skip over screens of Dot type
                if (screen.Type == ScreenType.Dots) continue;

                // Load the level file associated with that screen/level
                LevelFile levelFile = LevelManip.Load(screen.Level.InFile);

                // Flip the level horizontally if it supposed to be
                if (screen.Level.FlippedHoriz)
                    LevelManip.FlipLevelH(ref levelFile);

                //if (Settings.DoCorruptions)
                    //level.TSNeed += LevelCorruptors.CorruptLevel(ref levelFile);

                // Save the levelfile
                LevelManip.Save(levelFile, Randomizer.saveDir + $"tilemaps/{screen.ID}.lvl");

                // Write LevelInfo
                if (screen.Type == ScreenType.Level)
                    LevelInfo.WriteLine($"\"{screen.ID}\" {{name=\"{Name} {screen.LevelSuffix}\" id={screen.LevelSuffix}}}");
                else LevelInfo.WriteLine($"\"{screen.ID}\" {{name=\"{Name}\" id=-1}}");

                // Write Level Tileset
                // Calculate the final tileset
                // The tilesets are added in order of priority, from lowest to highest
                Tileset levelTileset = (screen.Level.TSDefault + areaTileset) + screen.Level.TSNeed;

                // Write level tileset to the file
                Tilesets.WriteLine($"{screen.LevelSuffix} {{");
                levelTileset.WriteTileset(Tilesets);
                Tilesets.WriteLine("}");
            }

            // write closing bracket for area tileset
            Tilesets.WriteLine("}\n");

            LevelInfo.Close();
            Tilesets.Close();

            // NPCS
            // Worldmap
        }

        // Entrance Normalization Functions
        *//*public static int[] GetTransitionSizes(ref LevelFile level)
        {


            return 
        }*//*
        public static void CountTransitionTiles(ref LevelFile level, Directions anchorflips)
        {
            // Normalizes all transition tags in a level

            // TODO:
            // need to add way to track when an entrance has already been normalized, otherwise we will basically count every entrance's tags twice.

            int step = level.header.width;

            // Iterates over the four directions
            for (int i = 0; i < 4; i++)
            {
                // Direction is indexed by bit shift
                Directions direction = (Directions)(1 << i);

                // Find transition tags
                int index1 = LevelManip.FindFirstTileByID(ref level, LevelFile.TAG, transTagIDs[i]);

                // Skip side if none found
                if (index1 == -1) continue;

                // Get all transition tags as list of indeces
                if (i >= 2) step = 1;
                List<int> tagList = LevelManip.FindTilesByID(ref level, LevelFile.TAG, TileID.GreenTransitionR, index1, step);

                // Calculate new entry dimension
                //int newEntryDimension = Math.Min(tagList.Count, neighborTagList.Count);

                // Remove extraneous entry tags in left/top level
                bool flip = false;
                if ((anchorflips & direction) != 0) flip = true;    // reverse the set if anchorflips contains direction
                //ReplaceEntryTags(ref level, tagList, newEntryDimension, flip);
            }




        }
        
        public static void ReplaceEntryTags(ref LevelFile level, List<int> set, int newEntryDimension, bool reverse)
        {
            // Replaces tiles in the set, works for horizontal or vertical connections

            // Calculate how many tiles to replace
            int toReplace = set.Count() - newEntryDimension;
            if (toReplace <= 0)
                return;

            // Reverse the list if the anchor is flipped
            if (reverse) set.Reverse();

            // Iterate over set of tiles
            for (int i = 0; i < toReplace; i++)
            {
                int index = set[i];

                // Remove the transition tag
                level.data[LevelFile.TAG, index] = TileID.Empty;

                // Place invisible solid where tag was
                level.data[LevelFile.ACTIVE, index] = TileID.Invisible;
            }
        }
        
        // Deprecated by FindTileByID in LevelManip
        *//*public static List<int> GetEntryTags(ref LevelFile level, TileID id, int index, Directions ConDir)
        {
            // Get basic level info
            int lw = level.header.width;
            int lh = level.header.height;
            int size = lw * lh;

            // Set the value to iterate by (iteration moves in same direction as anchor since first tiles are replaced)
            int it = lw;                                                    // Defaults to iterating by level width (one row)
            if ((ConDir & (Directions.U | Directions.D)) != 0) index = 1;   // If this is a vertical connection, iterate by 1 instead

            // Create the list of adjacent tile indeces
            List<int> adjacents = new List<int>();

            // loop until break
            while (true)
            {
                index += it;
                if (index < size && level.data[LevelFile.ACTIVE, index] == id) // Check the tile is within level bounds and is the same id
                {
                    adjacents.Add(index);
                    continue;
                }
                break;  // Break if index is OOB or id is different
            }
            return adjacents;   // Return the list of transition tiles
        }*//*

        // These functions are currently empty
        static void CreateMoreOpenEnds()
        {
            MapScreen screen = GetScreenToReplace();
            ReplaceScreen(screen);
        }
        static MapScreen GetScreenToReplace()
        {
            // Search the list of MapScreens in CurrentArea's list of screens
            // Look for levels with empty neighbors
            // Pick from levels with the most empty neighbors
            // Prioritize levels with greater distance value
            return null;
        }
        static void ReplaceScreen(MapScreen screen)
        {
            // remove the mapscreen from the map
            // remove the mapscreen from CurrentArea's list of screens
            // if the removed level was a gameplay level
            // place the level back into the pool
            // place a new screen in the now empty space
            // the new requirements must exceed the connections of the old screen
            // we use GetOptionsOpen() so new openEnds are created
        }

        // Debug functions for testing purposes
        public void PrintDebugCSV(GameMap map, string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < map.Height; i++)
                {
                    for (int j = 0; j < map.Width; j++)
                    {
                        
                        if (map.Data[i, j] != null)
                        {
                            if (map.Data[i, j].Type == ScreenType.Dots)
                                sw.Write("..");
                            else
                            {
                                sw.Write($"{DebugGetChar(map.Data[i, j].Level.MapConnections)}");
                                if (map.Data[i, j].Type == ScreenType.Level)
                                    sw.Write($"g");
                                else if (map.Data[i, j].Type == ScreenType.Connector)
                                    sw.Write($"c");
                            }
                        }
                        if (OpenEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"O");
                        }
                        if (DeadEntries.Contains(new Pair(i, j)))
                        {
                            sw.Write($"D");
                        }
                        if (SecretEnds.Contains(new Pair(i, j)))
                        {
                            sw.Write($"S");
                        }
                        if (ECoords.Equals(new Pair(i, j)))
                        {
                            sw.Write($"E");
                        }
                        if (XCoords.Equals(new Pair (i, j)))
                        {
                            sw.Write($"X");
                        }
                        sw.Write(",");
                    }
                    sw.Write('\n');
                }
            }
        }
        public static void PrintCSV(GameMap map, string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < map.Height; i++)
                {
                    for (int j = 0; j < map.Width; j++)
                    {
                        if (map.Data[i, j] != null)
                        {
                            if (map.Data[i, j].Type == ScreenType.Dots)
                                sw.Write("..");
                            else
                            {
                                sw.Write($"{map.Data[i, j].ID}.lvl");
                            }
                        }
                        sw.Write(",");
                    }
                    sw.Write('\n');
                }
            }
        }
        static char DebugGetChar(MapConnections con)
        {
            // convert to simpler representation in form of old connections enum
            Directions con2 = Directions.None;

            if (con.U != ConnectionType.none)
                con2 |= Directions.U;
            if (con.D != ConnectionType.none)
                con2 |= Directions.D;
            if (con.L != ConnectionType.none)
                con2 |= Directions.L;
            if (con.R != ConnectionType.none)
                con2 |= Directions.R;

            // use old switch case to get char
            switch (con2)
            {
                case Directions.R:
                    return '←';
                case Directions.L:
                    return '→';
                case Directions.L | Directions.R:
                    return '─';
                case Directions.U:
                    return '↓';
                case Directions.U | Directions.R:
                    return '└';
                case Directions.U | Directions.L:
                    return '┘';
                case Directions.U | Directions.L | Directions.R:
                    return '┴';
                case Directions.D:
                    return '↑';
                case Directions.D | Directions.R:
                    return '┌';
                case Directions.D | Directions.L:
                    return '┐';
                case Directions.D | Directions.L | Directions.R:
                    return '┬';
                case Directions.D | Directions.U:
                    return '│';
                case Directions.D | Directions.U | Directions.R:
                    return '├';
                case Directions.D | Directions.U | Directions.L:
                    return '┤';
                case Directions.All:
                    return '┼';
                default:
                    return ' ';
            }
        }
    }
}
*/