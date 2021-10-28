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

        // Corruptor Settings
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
            // intialize true defaults in case settings file is fucked
            NumLevels = 10;
            NumAreas = 3;
            DoMusic = false;
            DoPalettes = false;
            //MusicPerLevel = false;
            //PalettePerLevel = true;
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
            gon.InsertChild(nameof(NumLevels), GonObject.Manip.FromInt(NumLevels));
            gon.InsertChild(nameof(NumAreas), GonObject.Manip.FromInt(NumAreas));
            gon.InsertChild(nameof(DoMusic), GonObject.Manip.FromBool(DoMusic));
            gon.InsertChild(nameof(DoPalettes), GonObject.Manip.FromBool(DoPalettes));
            gon.InsertChild(nameof(MusicPerLevel), GonObject.Manip.FromBool(MusicPerLevel));
            gon.InsertChild(nameof(DoShaders), GonObject.Manip.FromBool(DoShaders));
            gon.InsertChild(nameof(DoParticles), GonObject.Manip.FromBool(DoParticles));
            gon.InsertChild(nameof(DoOverlays), GonObject.Manip.FromBool(DoOverlays));
            gon.InsertChild(nameof(DoTileGraphics), GonObject.Manip.FromBool(DoTileGraphics));
            gon.InsertChild(nameof(DoNevermoreTilt), GonObject.Manip.FromBool(DoNevermoreTilt));
            gon.InsertChild(nameof(DoExodusWobble), GonObject.Manip.FromBool(DoExodusWobble));
            gon.InsertChild(nameof(DoNPCs), GonObject.Manip.FromBool(DoNPCs));
            gon.InsertChild(nameof(UseAreaTileset), GonObject.Manip.FromBool(UseAreaTileset));
            gon.InsertChild(nameof(AreaType), GonObject.Manip.FromString(AreaType));
            gon.InsertChild(nameof(AltLevel), GonObject.Manip.FromString(AltLevel));
            gon.InsertChild(nameof(GenerateCustomParticles), GonObject.Manip.FromBool(GenerateCustomParticles));
            gon.InsertChild(nameof(MaxParticles), GonObject.Manip.FromInt(MaxParticles));
            gon.InsertChild(nameof(GameDirectory), GonObject.Manip.FromString(GameDirectory));
            gon.InsertChild(nameof(MaxParticleEffects), GonObject.Manip.FromInt(MaxParticleEffects));
            gon.InsertChild(nameof(ManualLoad), GonObject.Manip.FromBool(ManualLoad));
            gon.InsertChild(nameof(DoCorruptions), GonObject.Manip.FromBool(DoCorruptions));
            gon.InsertChild(nameof(MirrorMode), GonObject.Manip.FromBool(MirrorMode));
            gon.InsertChild(nameof(DeadRacer), GonObject.Manip.FromBool(DeadRacer));
            gon.InsertChild(nameof(CartLives), GonObject.Manip.FromInt(CartLives));
            gon.InsertChild(nameof(DoPhysics), GonObject.Manip.FromBool(DoPhysics));
            gon.InsertChild(nameof(PlatformPhysics), GonObject.Manip.FromBool(PlatformPhysics));
            gon.InsertChild(nameof(PlayerPhysics), GonObject.Manip.FromBool(PlayerPhysics));
            gon.InsertChild(nameof(WaterPhysics), GonObject.Manip.FromBool(WaterPhysics));
            gon.InsertChild(nameof(LowGravPhysics), GonObject.Manip.FromBool(LowGravPhysics));
            gon.InsertChild(nameof(LevelMerge), GonObject.Manip.FromBool(LevelMerge));
            gon.InsertChild(nameof(ToolsInDirectory), GonObject.Manip.FromString(ToolsInDirectory));
            gon.InsertChild(nameof(ToolsOutDirectory), GonObject.Manip.FromString(ToolsOutDirectory));
            gon.InsertChild(nameof(RandomizeAreaType), GonObject.Manip.FromBool(RandomizeAreaType));

            gon.InsertChild(nameof(CRSmart), GonObject.Manip.FromBool(CRSmart));
            gon.InsertChild(nameof(CROverlays), GonObject.Manip.FromBool(CROverlays));
            gon.InsertChild(nameof(CRTumors), GonObject.Manip.FromBool(CRTumors));
            gon.InsertChild(nameof(CRAddTiles), GonObject.Manip.FromInt(CRAddTiles));
            gon.InsertChild(nameof(CRAddEnemies), GonObject.Manip.FromInt(CRAddEnemies));
            gon.InsertChild(nameof(CRSpikeStrips), GonObject.Manip.FromBool(CRSpikeStrips));
            gon.InsertChild(nameof(CRCrumbles), GonObject.Manip.FromBool(CRCrumbles));
            gon.InsertChild(nameof(CRCrushers), GonObject.Manip.FromBool(CRCrushers));
            gon.InsertChild(nameof(CRChaos), GonObject.Manip.FromBool(CRChaos));
            gon.InsertChild(nameof(CRWaterLevels), GonObject.Manip.FromBool(CRWaterLevels));

            gon.InsertChild(nameof(UserName), GonObject.Manip.FromString(UserName));

            // Save pool settings
            gon.InsertChild(nameof(ActiveLevelPools), GonObject.Manip.FromStringArray(GetActivePools()));
            gon.InsertChild(nameof(ActiveShaders), GonObject.Manip.FromStringArray(GetActiveShaders()));

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
                        if (pool.Active)
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
            ActiveLevelPools = GonObject.Manip.GonToStringArray(gon[nameof(ActiveLevelPools)]);
            ActiveShaders    = GonObject.Manip.GonToStringArray(gon[nameof(ActiveShaders)]);

        }
    }
}
