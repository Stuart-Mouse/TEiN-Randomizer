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
        public bool UseCommonTileset { get; set; }
        public int CacheRuns { get; set; }
        //public int NumShuffles { get; set; }
        public AreaTypes AreaType { get; set; }
        public int RepeatTolerance { get; set; }
        public AltLevels AltLevel { get; set; }
        public string AttachToTS { get; set; }


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
            //DoArtAlts = false;
            DoNPCs = false;
            UseCommonTileset = false;
            CacheRuns = 0;
            //NumShuffles = 0;
            AreaType = AreaTypes.normal;
            AltLevel = AltLevels.Safe;
            RepeatTolerance = 0;
            AttachToTS = null;

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
                    element.SetElementValue(nameof(UseCommonTileset), UseCommonTileset);
                    element.SetElementValue(nameof(CacheRuns), CacheRuns);
                    //element.SetElementValue(nameof(NumShuffles), NumShuffles);
                    element.SetElementValue(nameof(AreaType), (int)AreaType);
                    element.SetElementValue(nameof(RepeatTolerance), RepeatTolerance);
                    element.SetElementValue(nameof(AltLevel), (int)AltLevel);
                    element.SetElementValue(nameof(AttachToTS), AttachToTS);
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
                    UseCommonTileset = (bool)element.Element(nameof(UseCommonTileset));
                    CacheRuns = (int)element.Element(nameof(CacheRuns));
                    //NumShuffles = (int)element.Element(nameof(NumShuffles));
                    AreaType = (AreaTypes)(int)element.Element(nameof(AreaType));
                    RepeatTolerance = (int)element.Element(nameof(RepeatTolerance));
                    AltLevel = (AltLevels)(int)element.Element(nameof(AltLevel));
                    AttachToTS = (string)element.Element(nameof(AttachToTS));
                }
            }
        }
    }
}
