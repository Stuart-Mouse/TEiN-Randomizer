using System;
using System.Windows;
using System.Xml.Linq;

namespace TEiNRandomizer
{
    
    public class RandomizerSettings
    {
        public int NumLevels { get; set; }
        public int NumAreas { get; set; }
        public bool UseDefaultMusic { get; set; }
        public bool UseDefaultPalettes { get; set; }
        public bool PalettePerLevel { get; set; }
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
        public int CacheRuns { get; set; }
        //public int NumShuffles { get; set; }
        public AreaTypes AreaType { get; set; }
        public int RepeatTolerance { get; set; }
        public AltLevels AltLevel { get; set; }
        public string AttachToTS { get; set; }
        public bool AutoRefresh { get; set; }
        public bool GenerateCustomParticles { get; set; }
        public int MaxParticles { get; set; }
        public string GameDirectory { get; set; }
        public int MaxParticleEffects { get; set; }
        public bool MirrorMode { get; set; }
        public bool DoCorruptions { get; set; }
        public bool ManualLoad { get; set; }
        public bool DeadRacer { get; set; }
        public int CartLives { get; set; }
        public bool PlatformPhysics { get; set; }

        public RandomizerSettings()
        {
            // intialize true defaults in case settings file is fucked
            NumLevels = 10;
            NumAreas = 3;
            UseDefaultMusic = false;
            UseDefaultPalettes = false;
            MusicPerLevel = false;
            PalettePerLevel = true;
            DoShaders = true;
            DoParticles = true;
            DoOverlays = true;
            DoTileGraphics = true;
            DoNevermoreTilt = true;
            DoExodusWobble = true;
            AutoRefresh = false;
            //DoArtAlts = false;
            DoNPCs = false;
            UseAreaTileset = true;
            CacheRuns = 0;
            //NumShuffles = 0;
            AreaType = AreaTypes.normal;
            AltLevel = AltLevels.Safe;
            RepeatTolerance = 0;
            AttachToTS = null;
            GenerateCustomParticles = false;
            MaxParticles = 1000;
            MaxParticleEffects = 2;
            ManualLoad = false;
            DoCorruptions = false;
            MirrorMode = false;
            DeadRacer = false;
            CartLives = 19;
            PlatformPhysics = false;
            GameDirectory = "C:/Program Files(x86)/Steam/steamapps/common/theendisnigh/";

            try
            {
                Load("default");
            }
            catch (Exception)
            {
                try
                {
                    Save("default");

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

        public void Save(string preset)
        {
            var doc = XDocument.Load("data/RandomizerSettings.xml");    // open levelpool file
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == preset)
                {
                    element.RemoveAll();
                    element.SetElementValue(nameof(NumLevels), NumLevels);
                    element.SetElementValue(nameof(NumAreas), NumAreas);
                    element.SetElementValue(nameof(UseDefaultMusic), UseDefaultMusic);
                    element.SetElementValue(nameof(UseDefaultPalettes), UseDefaultPalettes);
                    element.SetElementValue(nameof(MusicPerLevel), MusicPerLevel);
                    element.SetElementValue(nameof(PalettePerLevel), PalettePerLevel);
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
                    element.SetElementValue(nameof(AutoRefresh), AutoRefresh);
                    element.SetElementValue(nameof(AreaType), (int)AreaType);
                    element.SetElementValue(nameof(RepeatTolerance), RepeatTolerance);
                    element.SetElementValue(nameof(AltLevel), (int)AltLevel);
                    element.SetElementValue(nameof(AttachToTS), AttachToTS);
                    element.SetElementValue(nameof(MaxParticles), MaxParticles);
                    element.SetElementValue(nameof(GenerateCustomParticles), GenerateCustomParticles);
                    element.SetElementValue(nameof(GameDirectory), GameDirectory);
                    element.SetElementValue(nameof(MaxParticleEffects), MaxParticleEffects);
                    element.SetElementValue(nameof(ManualLoad), ManualLoad);
                    element.SetElementValue(nameof(DoCorruptions), DoCorruptions);
                    element.SetElementValue(nameof(MirrorMode), MirrorMode);
                    element.SetElementValue(nameof(DeadRacer), DeadRacer);
                    element.SetElementValue(nameof(CartLives), CartLives);
                    element.SetElementValue(nameof(PlatformPhysics), PlatformPhysics);
                }
            }
            doc.Save("data/RandomizerSettings.xml");
        }

        public void Load(string preset)
        {
            var doc = XDocument.Load("data/RandomizerSettings.xml");    // open levelpool file
            foreach (var element in doc.Root.Elements())
            {
                if (element.Name == preset)
                {
                    NumLevels = (int)element.Element(nameof(NumLevels));
                    NumAreas = (int)element.Element(nameof(NumAreas));
                    UseDefaultMusic = (bool)element.Element(nameof(UseDefaultMusic));
                    UseDefaultPalettes = (bool)element.Element(nameof(UseDefaultPalettes));
                    MusicPerLevel = (bool)element.Element(nameof(MusicPerLevel));
                    PalettePerLevel = (bool)element.Element(nameof(PalettePerLevel));
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
                    AreaType = (AreaTypes)(int)element.Element(nameof(AreaType));
                    RepeatTolerance = (int)element.Element(nameof(RepeatTolerance));
                    AltLevel = (AltLevels)(int)element.Element(nameof(AltLevel));
                    AutoRefresh = (bool)element.Element(nameof(AutoRefresh));
                    AttachToTS = (string)element.Element(nameof(AttachToTS));
                    GenerateCustomParticles = (bool)element.Element(nameof(GenerateCustomParticles));
                    MaxParticles = (int)element.Element(nameof(MaxParticles));
                    GameDirectory = (string)element.Element(nameof(GameDirectory));
                    MaxParticleEffects = (int)element.Element(nameof(MaxParticleEffects));
                    ManualLoad = (bool)element.Element(nameof(ManualLoad));
                    DoCorruptions = (bool)element.Element(nameof(DoCorruptions));
                    MirrorMode = (bool)element.Element(nameof(MirrorMode));
                    DeadRacer = (bool)element.Element(nameof(DeadRacer));
                    CartLives = (int)element.Element(nameof(CartLives));
                    PlatformPhysics = (bool)element.Element(nameof(PlatformPhysics));
                }
            }
        }
    }
}
