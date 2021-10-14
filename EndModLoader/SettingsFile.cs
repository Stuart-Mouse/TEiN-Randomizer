using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{

    public class SettingsFile
    {
        public string UserName { get; set; }
        public int NumLevels { get; set; }
        public int NumAreas { get; set; }
        public bool DoMusic { get; set; }
        public bool DoPalettes { get; set; }
        //public bool PalettePerLevel { get; set; }
        public bool MusicPerLevel { get; set; }
        public bool DoShaders { get; set; }
        public bool DoParticles { get; set; }
        public bool DoOverlays { get; set; }
        public bool DoTileGraphics { get; set; }
        public bool DoNevermoreTilt { get; set; }
        public bool DoExodusWobble { get; set; }
        //public bool DoArtAlts { get; set; }
        public bool DoNPCs { get; set; }
        public bool UseAreaTileset { get; set; }
        public int CacheRuns { get; set; }  // This is no longer used, but it is still present in one place, so I haven't removed it yet.
        //public int NumShuffles { get; set; }
        public string AreaType { get; set; }
        public int RepeatTolerance { get; set; } // This is no longer used, but it is still present in one place, so I haven't removed it yet.
        public string AltLevel { get; set; }
        //public bool AutoRefresh { get; set; }
        public bool GenerateCustomParticles { get; set; }
        public int MaxParticles { get; set; }
        public string GameDirectory { get; set; }
        //public string ModSaveDirectory { get; set; }
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

        // Mod Randomization Settings

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
            //AutoRefresh = false;
            //DoArtAlts = false;
            DoNPCs = false;
            UseAreaTileset = true;
            CacheRuns = 0;
            //NumShuffles = 0;
            AreaType = "normal";
            AltLevel = "Safe";
            RepeatTolerance = 0;
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
            //ModSaveDirectory = "saved runs/";
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
            catch (Exception ex)
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

        public void SaveXML(string preset)
        {
            var doc = XDocument.Load("data/RandomizerSettings.xml");    // open levelpool file
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == preset)
                {
                    element.RemoveAll();
                    element.SetElementValue(nameof(NumLevels), NumLevels);
                    element.SetElementValue(nameof(NumAreas), NumAreas);
                    element.SetElementValue(nameof(DoMusic), DoMusic);
                    element.SetElementValue(nameof(DoPalettes), DoPalettes);
                    element.SetElementValue(nameof(MusicPerLevel), MusicPerLevel);
                    //element.SetElementValue(nameof(PalettePerLevel), PalettePerLevel);
                    element.SetElementValue(nameof(DoShaders), DoShaders);
                    element.SetElementValue(nameof(DoParticles), DoParticles);
                    element.SetElementValue(nameof(DoOverlays), DoOverlays);
                    element.SetElementValue(nameof(DoTileGraphics), DoTileGraphics);
                    element.SetElementValue(nameof(DoNevermoreTilt), DoNevermoreTilt);
                    element.SetElementValue(nameof(DoExodusWobble), DoExodusWobble);
                    //element.SetElementValue(nameof(DoArtAlts), DoArtAlts);
                    element.SetElementValue(nameof(DoNPCs), DoNPCs);
                    element.SetElementValue(nameof(UseAreaTileset), UseAreaTileset);
                    element.SetElementValue(nameof(CacheRuns), CacheRuns);
                    //element.SetElementValue(nameof(NumShuffles), NumShuffles);
                    //element.SetElementValue(nameof(AutoRefresh), AutoRefresh);
                    element.SetElementValue(nameof(AreaType), (string)AreaType);
                    element.SetElementValue(nameof(RepeatTolerance), RepeatTolerance);
                    element.SetElementValue(nameof(AltLevel), (string)AltLevel);
                    //element.SetElementValue(nameof(AttachToTS), AttachToTS);
                    element.SetElementValue(nameof(MaxParticles), MaxParticles);
                    element.SetElementValue(nameof(GenerateCustomParticles), GenerateCustomParticles);
                    element.SetElementValue(nameof(GameDirectory), GameDirectory);
                    //element.SetElementValue(nameof(ModSaveDirectory), ModSaveDirectory);
                    element.SetElementValue(nameof(MaxParticleEffects), MaxParticleEffects);
                    element.SetElementValue(nameof(ManualLoad), ManualLoad);
                    element.SetElementValue(nameof(DoCorruptions), DoCorruptions);
                    element.SetElementValue(nameof(MirrorMode), MirrorMode);
                    element.SetElementValue(nameof(DeadRacer), DeadRacer);
                    element.SetElementValue(nameof(CartLives), CartLives);
                    element.SetElementValue(nameof(DoPhysics), DoPhysics);
                    element.SetElementValue(nameof(PlatformPhysics), PlatformPhysics);
                    element.SetElementValue(nameof(PlayerPhysics), PlayerPhysics);
                    element.SetElementValue(nameof(WaterPhysics), WaterPhysics);
                    element.SetElementValue(nameof(LowGravPhysics), LowGravPhysics);
                    element.SetElementValue(nameof(LevelMerge), LevelMerge);
                    element.SetElementValue(nameof(ToolsInDirectory), ToolsInDirectory);
                    element.SetElementValue(nameof(ToolsOutDirectory), ToolsOutDirectory);
                    element.SetElementValue(nameof(RandomizeAreaType), RandomizeAreaType);
                    element.SetElementValue(nameof(CRSmart), CRSmart);
                    element.SetElementValue(nameof(CROverlays), CROverlays);
                    element.SetElementValue(nameof(CRTumors), CRTumors);
                    element.SetElementValue(nameof(CRAddTiles), CRAddTiles);
                    element.SetElementValue(nameof(CRAddEnemies), CRAddEnemies);
                    element.SetElementValue(nameof(CRSpikeStrips), CRSpikeStrips);
                    element.SetElementValue(nameof(CRCrumbles), CRCrumbles);
                    element.SetElementValue(nameof(CRCrushers), CRCrushers);
                    element.SetElementValue(nameof(CRChaos), CRChaos);
                    element.SetElementValue(nameof(CRWaterLevels), CRWaterLevels);
                    element.SetElementValue(nameof(UserName), UserName);
                }
            }
            doc.Save("data/RandomizerSettings.xml");
        }
        public void Save()
        {
            var gon = new GonObject();

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
            gon.InsertChild(nameof(CacheRuns), GonObject.Manip.FromInt(CacheRuns));
            gon.InsertChild(nameof(AreaType), GonObject.Manip.FromString(AreaType));
            gon.InsertChild(nameof(RepeatTolerance), GonObject.Manip.FromInt(RepeatTolerance));
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

            gon.Save("data/text/settings.gon");
        }

        public void LoadXML(string preset)
        {
            var doc = XDocument.Load("data/RandomizerSettings.xml");    // open levelpool file
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == preset)
                {
                    NumLevels = (int)element.Element(nameof(NumLevels));
                    NumAreas = (int)element.Element(nameof(NumAreas));
                    DoMusic = (bool)element.Element(nameof(DoMusic));
                    DoPalettes = (bool)element.Element(nameof(DoPalettes));
                    MusicPerLevel = (bool)element.Element(nameof(MusicPerLevel));
                    //PalettePerLevel = (bool)element.Element(nameof(PalettePerLevel));
                    DoShaders = (bool)element.Element(nameof(DoShaders));
                    DoParticles = (bool)element.Element(nameof(DoParticles));
                    DoOverlays = (bool)element.Element(nameof(DoOverlays));
                    DoTileGraphics = (bool)element.Element(nameof(DoTileGraphics));
                    DoNevermoreTilt = (bool)element.Element(nameof(DoNevermoreTilt));
                    DoExodusWobble = (bool)element.Element(nameof(DoExodusWobble));
                    //DoArtAlts = (bool)element.Element(nameof(DoArtAlts));
                    DoNPCs = (bool)element.Element(nameof(DoNPCs));
                    UseAreaTileset = (bool)element.Element(nameof(UseAreaTileset));
                    CacheRuns = (int)element.Element(nameof(CacheRuns));
                    //NumShuffles = (int)element.Element(nameof(NumShuffles));
                    AreaType = (string)element.Element(nameof(AreaType));
                    RepeatTolerance = (int)element.Element(nameof(RepeatTolerance));
                    AltLevel = (string)element.Element(nameof(AltLevel));
                    //AttachToTS = (string)element.Element(nameof(AttachToTS));
                    //AutoRefresh = (bool)element.Element(nameof(AutoRefresh));
                    GenerateCustomParticles = (bool)element.Element(nameof(GenerateCustomParticles));
                    MaxParticles = (int)element.Element(nameof(MaxParticles));
                    GameDirectory = (string)element.Element(nameof(GameDirectory));
                    //ModSaveDirectory = (string)element.Element(nameof(ModSaveDirectory));
                    MaxParticleEffects = (int)element.Element(nameof(MaxParticleEffects));
                    ManualLoad = (bool)element.Element(nameof(ManualLoad));
                    DoCorruptions = (bool)element.Element(nameof(DoCorruptions));
                    MirrorMode = (bool)element.Element(nameof(MirrorMode));
                    DeadRacer = (bool)element.Element(nameof(DeadRacer));
                    CartLives = (int)element.Element(nameof(CartLives));
                    DoPhysics = (bool)element.Element(nameof(DoPhysics));
                    PlatformPhysics = (bool)element.Element(nameof(PlatformPhysics));
                    PlayerPhysics = (bool)element.Element(nameof(PlayerPhysics));
                    WaterPhysics = (bool)element.Element(nameof(WaterPhysics));
                    LowGravPhysics = (bool)element.Element(nameof(LowGravPhysics));
                    LevelMerge = (bool)element.Element(nameof(LevelMerge));
                    ToolsInDirectory = (string)element.Element(nameof(ToolsInDirectory));
                    ToolsOutDirectory = (string)element.Element(nameof(ToolsOutDirectory));
                    RandomizeAreaType = (bool)element.Element(nameof(RandomizeAreaType));
                    CRSmart = (bool)element.Element(nameof(CRSmart));
                    CROverlays = (bool)element.Element(nameof(CROverlays));
                    CRTumors = (bool)element.Element(nameof(CRTumors));
                    CRAddTiles = (int)element.Element(nameof(CRAddTiles));
                    CRAddEnemies = (int)element.Element(nameof(CRAddEnemies));
                    CRSpikeStrips = (bool)element.Element(nameof(CRSpikeStrips));
                    CRCrumbles = (bool)element.Element(nameof(CRCrumbles));
                    CRCrushers = (bool)element.Element(nameof(CRCrushers));
                    CRChaos = (bool)element.Element(nameof(CRChaos));
                    CRWaterLevels = (bool)element.Element(nameof(CRWaterLevels));
                    UserName = (string)element.Element(nameof(UserName));

                }
            }
        }

        public void Load()
        {
            var gon = GonObject.Load("data/text/settings.gon");    // open levelpool file

            NumLevels = gon[nameof(NumLevels)].Int();
            NumAreas = gon[nameof(NumAreas)].Int();
            DoMusic = gon[nameof(DoMusic)].Bool();
            DoPalettes = gon[nameof(DoPalettes)].Bool();
            MusicPerLevel = gon[nameof(MusicPerLevel)].Bool();
            //PalettePerLevel = gon[nameof(PalettePerLevel));
            DoShaders = gon[nameof(DoShaders)].Bool();
            DoParticles = gon[nameof(DoParticles)].Bool();
            DoOverlays = gon[nameof(DoOverlays)].Bool();
            DoTileGraphics = gon[nameof(DoTileGraphics)].Bool();
            DoNevermoreTilt = gon[nameof(DoNevermoreTilt)].Bool();
            DoExodusWobble = gon[nameof(DoExodusWobble)].Bool();
            //DoArtAlts = gon[nameof(DoArtAlts));
            DoNPCs = gon[nameof(DoNPCs)].Bool();
            UseAreaTileset = gon[nameof(UseAreaTileset)].Bool();
            CacheRuns = gon[nameof(CacheRuns)].Int();
            //NumShuffles = gon[nameof(NumShuffles));
            AreaType = gon[nameof(AreaType)].String();
            RepeatTolerance = gon[nameof(RepeatTolerance)].Int();
            AltLevel = gon[nameof(AltLevel)].String();
            //AttachToTS = gon[nameof(AttachToTS));
            //AutoRefresh = gon[nameof(AutoRefresh));
            GenerateCustomParticles = gon[nameof(GenerateCustomParticles)].Bool();
            MaxParticles = gon[nameof(MaxParticles)].Int();
            GameDirectory = gon[nameof(GameDirectory)].String();
            //ModSaveDirectory = gon[nameof(ModSaveDirectory));
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
        }
        public void WriteNewSaveFunc()
        {
            Type SettingsType = this.GetType();
            System.Reflection.MemberInfo[] SettingsMembers = SettingsType.GetMembers();
            using (StreamWriter sw = File.CreateText("data/newsavefunc.txt"))
            {
                sw.WriteLine("Members List");
                for (int i = 0; i < SettingsMembers.Length; i++)
                {
                    var Member = SettingsMembers[i];
                    sw.WriteLine(Member.Name);
                }

                sw.WriteLine("Save Function");
                for (int i = 0; i < SettingsMembers.Length; i++)
                {
                    var Member = SettingsMembers[i];
                    sw.WriteLine($"element.SetElementValue(nameof({Member.Name}), {Member.Name});");
                }

                sw.WriteLine("Load Function");
                for (int i = 0; i < SettingsMembers.Length; i++)
                {
                    var Member = SettingsMembers[i];
                    //var thing = Member.CustomAttributes;
                    //var thingtwo = thing.
                    sw.WriteLine($"{Member.Name} = (bool)element.Element(nameof({Member.Name}));");
                }
            }
        }
    }
}
