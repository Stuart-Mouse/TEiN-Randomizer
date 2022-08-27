using System;
using System.Windows;
using System.Collections.Generic;

namespace TEiNRandomizer
{
    public class SettingsFile
    {
        // Mod Randomization Settings
        public string UserName { get; set; }
        public int NumLevels { get; set; }
        public int NumAreas { get; set; }
        public bool DoMusic { get; set; }
        public bool DoPalettes { get; set; }
        public bool MusicPerLevel { get; set; }
        public bool DoShaders { get; set; }
        public bool DoParticles { get; set; }
        public bool DoOverlays { get; set; }
        public bool DoTileGraphics { get; set; }
        public bool DoNevermoreTilt { get; set; }
        public bool DoExodusWobble { get; set; }
        public bool DoNPCs { get; set; }
        public bool UseAreaTileset { get; set; }
        public string AreaType { get; set; }
        public string AltLevel { get; set; }
        public bool GenerateCustomParticles { get; set; }
        public int MaxParticles { get; set; }
        public string GameDirectory { get; set; }
        public string ToolsInDirectory { get; set; }
        public string ToolsOutDirectory { get; set; }
        public int MaxParticleEffects { get; set; }
        public bool MirrorMode { get; set; }
        public bool DoCorruptions { get; set; }
        public bool ManualLoad { get; set; }
        public bool DeadRacer { get; set; }
        public int CartLives { get; set; }
        public bool DoPhysics { get; set; }
        public bool PlatformPhysics { get; set; }
        public bool PlayerPhysics { get; set; }
        public bool WaterPhysics { get; set; }
        public bool LowGravPhysics { get; set; }
        public bool LevelMerge { get; set; }
        public bool RandomizeAreaType { get; set; }

        [Flags]
        enum CorruptorSettings
        {
            None         = 0,
            SmartCorrupt = 1 << 0,
            Overlays     = 1 << 0,
            SpikeStrips  = 1 << 0,
            Crumbles     = 1 << 0,
            Crushers     = 1 << 0,
        }

        public bool CRSmart { get; set; }
        public bool CROverlays { get; set; }
        public bool CRTumors { get; set; }
        public int CRAddTiles { get; set; }
        public int CRAddEnemies { get; set; }
        public bool CRSpikeStrips { get; set; }
        public bool CRCrumbles { get; set; }
        public bool CRCrushers { get; set; }
        public bool CRChaos { get; set; }
        public bool CRWaterLevels { get; set; }

        // Level Pool Settings
        public string[] ActiveLevelPools { get; set; }
        public string[] ActiveShaders { get; set; }

        public SettingsFile()
        {
            NumLevels = 10;
            NumAreas = 3;
            DoMusic = false;
            DoPalettes = false;
            DoShaders = true;
            DoParticles = true;
            DoOverlays = true;
            DoTileGraphics = true;
            DoNevermoreTilt = true;
            DoExodusWobble = true;
            DoNPCs = false;
            UseAreaTileset = true;
            AreaType = "normal";
            AltLevel = "Safe";
            GenerateCustomParticles = false;
            MaxParticles = 1000;
            MaxParticleEffects = 2;
            ManualLoad = false;
            DoCorruptions = false;
            MirrorMode = false;
            DeadRacer = false;
            CartLives = 19;
            DoPhysics = false;
            PlatformPhysics = false;
            PlayerPhysics = false;
            WaterPhysics = false;
            LowGravPhysics = false;
            LevelMerge = false;
            GameDirectory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\theendisnigh\\";
            ToolsInDirectory = "tools/input/";
            ToolsOutDirectory = "tools/output/";
            RandomizeAreaType = false;
            CRSmart = true;
            CROverlays = false;
            CRTumors = true;
            CRAddTiles = 0;
            CRAddEnemies = 0;
            CRSpikeStrips = false;
            CRCrumbles = false;
            CRCrushers = false;
            CRChaos = false;
            CRWaterLevels = false;
            UserName = "Ash";

            try
            {
                Load();
            }
            catch (Exception)
            {
                try
                {
                    MessageBox.Show(
                        "Something was wrong with your settings file.\nAttempting to make a new one...",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK
                    );
                    Save();
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "Your RandomSettings file was somehow fucked up.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK
                    );
                    throw;
                }
            }
        }
        public void Save()
        {
            // Create new gon object
            GonObject gon = new GonObject();

            // Save randomization settings
            gon.InsertChild(nameof(NumLevels), GonObject.FromInt(NumLevels));
            gon.InsertChild(nameof(NumAreas), GonObject.FromInt(NumAreas));
            gon.InsertChild(nameof(DoMusic), GonObject.FromBool(DoMusic));
            gon.InsertChild(nameof(DoPalettes), GonObject.FromBool(DoPalettes));
            gon.InsertChild(nameof(MusicPerLevel), GonObject.FromBool(MusicPerLevel));
            gon.InsertChild(nameof(DoShaders), GonObject.FromBool(DoShaders));
            gon.InsertChild(nameof(DoParticles), GonObject.FromBool(DoParticles));
            gon.InsertChild(nameof(DoOverlays), GonObject.FromBool(DoOverlays));
            gon.InsertChild(nameof(DoTileGraphics), GonObject.FromBool(DoTileGraphics));
            gon.InsertChild(nameof(DoNevermoreTilt), GonObject.FromBool(DoNevermoreTilt));
            gon.InsertChild(nameof(DoExodusWobble), GonObject.FromBool(DoExodusWobble));
            gon.InsertChild(nameof(DoNPCs), GonObject.FromBool(DoNPCs));
            gon.InsertChild(nameof(UseAreaTileset), GonObject.FromBool(UseAreaTileset));
            gon.InsertChild(nameof(AreaType), GonObject.FromString(AreaType));
            gon.InsertChild(nameof(AltLevel), GonObject.FromString(AltLevel));
            gon.InsertChild(nameof(GenerateCustomParticles), GonObject.FromBool(GenerateCustomParticles));
            gon.InsertChild(nameof(MaxParticles), GonObject.FromInt(MaxParticles));
            gon.InsertChild(nameof(GameDirectory), GonObject.FromString(GameDirectory));
            gon.InsertChild(nameof(MaxParticleEffects), GonObject.FromInt(MaxParticleEffects));
            gon.InsertChild(nameof(ManualLoad), GonObject.FromBool(ManualLoad));
            gon.InsertChild(nameof(DoCorruptions), GonObject.FromBool(DoCorruptions));
            gon.InsertChild(nameof(MirrorMode), GonObject.FromBool(MirrorMode));
            gon.InsertChild(nameof(DeadRacer), GonObject.FromBool(DeadRacer));
            gon.InsertChild(nameof(CartLives), GonObject.FromInt(CartLives));
            gon.InsertChild(nameof(DoPhysics), GonObject.FromBool(DoPhysics));
            gon.InsertChild(nameof(PlatformPhysics), GonObject.FromBool(PlatformPhysics));
            gon.InsertChild(nameof(PlayerPhysics), GonObject.FromBool(PlayerPhysics));
            gon.InsertChild(nameof(WaterPhysics), GonObject.FromBool(WaterPhysics));
            gon.InsertChild(nameof(LowGravPhysics), GonObject.FromBool(LowGravPhysics));
            gon.InsertChild(nameof(LevelMerge), GonObject.FromBool(LevelMerge));
            gon.InsertChild(nameof(ToolsInDirectory), GonObject.FromString(ToolsInDirectory));
            gon.InsertChild(nameof(ToolsOutDirectory), GonObject.FromString(ToolsOutDirectory));
            gon.InsertChild(nameof(RandomizeAreaType), GonObject.FromBool(RandomizeAreaType));

            gon.InsertChild(nameof(CRSmart), GonObject.FromBool(CRSmart));
            gon.InsertChild(nameof(CROverlays), GonObject.FromBool(CROverlays));
            gon.InsertChild(nameof(CRTumors), GonObject.FromBool(CRTumors));
            gon.InsertChild(nameof(CRAddTiles), GonObject.FromInt(CRAddTiles));
            gon.InsertChild(nameof(CRAddEnemies), GonObject.FromInt(CRAddEnemies));
            gon.InsertChild(nameof(CRSpikeStrips), GonObject.FromBool(CRSpikeStrips));
            gon.InsertChild(nameof(CRCrumbles), GonObject.FromBool(CRCrumbles));
            gon.InsertChild(nameof(CRCrushers), GonObject.FromBool(CRCrushers));
            gon.InsertChild(nameof(CRChaos), GonObject.FromBool(CRChaos));
            gon.InsertChild(nameof(CRWaterLevels), GonObject.FromBool(CRWaterLevels));

            gon.InsertChild(nameof(UserName), GonObject.FromString(UserName));

            // Save pool settings
            gon.InsertChild(nameof(ActiveLevelPools), GonObject.FromStringArray(GetActivePools()));
            gon.InsertChild(nameof(ActiveShaders), GonObject.FromStringArray(GetActiveShaders()));

            gon.Save("data/text/settings.gon");
        }
        public string[] GetActivePools()
        {
            List<string> strs = new List<string>();
            foreach (var cat in AppResources.LevelPoolCategories)
            {
                if (cat.Enabled)
                {
                    foreach (var pool in cat.Pools)
                    {
                        if (pool.Enabled)
                            strs.Add(pool.Name);
                    }
                }
            }
            return strs.ToArray();
        }
        public string[] GetActiveShaders()
        {
            List<string> strs = new List<string>();
            foreach (var shader in AppResources.ShadersList)
            {
                if (shader.Enabled)
                    strs.Add(shader.Name);
            }
            return strs.ToArray();
        }
        public void Load()
        {
            // Open levelpool file
            GonObject gon = GonObject.Load("data/text/settings.gon");

            // Load randomization settings
            NumLevels = gon[nameof(NumLevels)].Int();
            NumAreas = gon[nameof(NumAreas)].Int();
            DoMusic = gon[nameof(DoMusic)].Bool();
            DoPalettes = gon[nameof(DoPalettes)].Bool();
            MusicPerLevel = gon[nameof(MusicPerLevel)].Bool();
            DoShaders = gon[nameof(DoShaders)].Bool();
            DoParticles = gon[nameof(DoParticles)].Bool();
            DoOverlays = gon[nameof(DoOverlays)].Bool();
            DoTileGraphics = gon[nameof(DoTileGraphics)].Bool();
            DoNevermoreTilt = gon[nameof(DoNevermoreTilt)].Bool();
            DoExodusWobble = gon[nameof(DoExodusWobble)].Bool();
            DoNPCs = gon[nameof(DoNPCs)].Bool();
            UseAreaTileset = gon[nameof(UseAreaTileset)].Bool();
            AreaType = gon[nameof(AreaType)].String();
            AltLevel = gon[nameof(AltLevel)].String();
            GenerateCustomParticles = gon[nameof(GenerateCustomParticles)].Bool();
            MaxParticles = gon[nameof(MaxParticles)].Int();
            GameDirectory = gon[nameof(GameDirectory)].String();
            MaxParticleEffects = gon[nameof(MaxParticleEffects)].Int();
            ManualLoad = gon[nameof(ManualLoad)].Bool();
            DoCorruptions = gon[nameof(DoCorruptions)].Bool();
            MirrorMode = gon[nameof(MirrorMode)].Bool();
            DeadRacer = gon[nameof(DeadRacer)].Bool();
            CartLives = gon[nameof(CartLives)].Int();
            DoPhysics = gon[nameof(DoPhysics)].Bool();
            PlatformPhysics = gon[nameof(PlatformPhysics)].Bool();
            PlayerPhysics = gon[nameof(PlayerPhysics)].Bool();
            WaterPhysics = gon[nameof(WaterPhysics)].Bool();
            LowGravPhysics = gon[nameof(LowGravPhysics)].Bool();
            LevelMerge = gon[nameof(LevelMerge)].Bool();
            ToolsInDirectory = gon[nameof(ToolsInDirectory)].String();
            ToolsOutDirectory = gon[nameof(ToolsOutDirectory)].String();
            RandomizeAreaType = gon[nameof(RandomizeAreaType)].Bool();

            CRSmart = gon[nameof(CRSmart)].Bool();
            CROverlays = gon[nameof(CROverlays)].Bool();
            CRTumors = gon[nameof(CRTumors)].Bool();
            CRAddTiles = gon[nameof(CRAddTiles)].Int();
            CRAddEnemies = gon[nameof(CRAddEnemies)].Int();
            CRSpikeStrips = gon[nameof(CRSpikeStrips)].Bool();
            CRCrumbles = gon[nameof(CRCrumbles)].Bool();
            CRCrushers = gon[nameof(CRCrushers)].Bool();
            CRChaos = gon[nameof(CRChaos)].Bool();
            CRWaterLevels = gon[nameof(CRWaterLevels)].Bool();

            UserName = gon[nameof(UserName)].String();

            // Load pool settings
            ActiveLevelPools = gon[nameof(ActiveLevelPools)].ToStringArray();
            ActiveShaders    = gon[nameof(ActiveShaders)   ].ToStringArray();

        }

        public void Copy()
        {
            // Open levelpool file
            GonObject gon = GonObject.Load("data/text/settings.gon");

            // Load randomization settings
            NumLevels = gon[nameof(NumLevels)].Int();
            NumAreas = gon[nameof(NumAreas)].Int();
            DoMusic = gon[nameof(DoMusic)].Bool();
            DoPalettes = gon[nameof(DoPalettes)].Bool();
            MusicPerLevel = gon[nameof(MusicPerLevel)].Bool();
            DoShaders = gon[nameof(DoShaders)].Bool();
            DoParticles = gon[nameof(DoParticles)].Bool();
            DoOverlays = gon[nameof(DoOverlays)].Bool();
            DoTileGraphics = gon[nameof(DoTileGraphics)].Bool();
            DoNevermoreTilt = gon[nameof(DoNevermoreTilt)].Bool();
            DoExodusWobble = gon[nameof(DoExodusWobble)].Bool();
            DoNPCs = gon[nameof(DoNPCs)].Bool();
            UseAreaTileset = gon[nameof(UseAreaTileset)].Bool();
            AreaType = gon[nameof(AreaType)].String();
            AltLevel = gon[nameof(AltLevel)].String();
            GenerateCustomParticles = gon[nameof(GenerateCustomParticles)].Bool();
            MaxParticles = gon[nameof(MaxParticles)].Int();
            GameDirectory = gon[nameof(GameDirectory)].String();
            MaxParticleEffects = gon[nameof(MaxParticleEffects)].Int();
            ManualLoad = gon[nameof(ManualLoad)].Bool();
            DoCorruptions = gon[nameof(DoCorruptions)].Bool();
            MirrorMode = gon[nameof(MirrorMode)].Bool();
            DeadRacer = gon[nameof(DeadRacer)].Bool();
            CartLives = gon[nameof(CartLives)].Int();
            DoPhysics = gon[nameof(DoPhysics)].Bool();
            PlatformPhysics = gon[nameof(PlatformPhysics)].Bool();
            PlayerPhysics = gon[nameof(PlayerPhysics)].Bool();
            WaterPhysics = gon[nameof(WaterPhysics)].Bool();
            LowGravPhysics = gon[nameof(LowGravPhysics)].Bool();
            LevelMerge = gon[nameof(LevelMerge)].Bool();
            ToolsInDirectory = gon[nameof(ToolsInDirectory)].String();
            ToolsOutDirectory = gon[nameof(ToolsOutDirectory)].String();
            RandomizeAreaType = gon[nameof(RandomizeAreaType)].Bool();

            CRSmart = gon[nameof(CRSmart)].Bool();
            CROverlays = gon[nameof(CROverlays)].Bool();
            CRTumors = gon[nameof(CRTumors)].Bool();
            CRAddTiles = gon[nameof(CRAddTiles)].Int();
            CRAddEnemies = gon[nameof(CRAddEnemies)].Int();
            CRSpikeStrips = gon[nameof(CRSpikeStrips)].Bool();
            CRCrumbles = gon[nameof(CRCrumbles)].Bool();
            CRCrushers = gon[nameof(CRCrushers)].Bool();
            CRChaos = gon[nameof(CRChaos)].Bool();
            CRWaterLevels = gon[nameof(CRWaterLevels)].Bool();

            UserName = gon[nameof(UserName)].String();

            // Load pool settings
            ActiveLevelPools = gon[nameof(ActiveLevelPools)].ToStringArray();
            ActiveShaders    = gon[nameof(ActiveShaders)   ].ToStringArray();
        }
    }
}
