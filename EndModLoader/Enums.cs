using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using TEiNRandomizer.Properties;

namespace TEiNRandomizer
{
    public enum AreaTypesEnum
    {
        normal,
        dark,
        cart,
        ironcart,
        glitch
    }

    

    public enum AltLevelsEnum
    {
        None,
        Safe,
        Extended,
        Crazy,
        Insane
    }

    public enum Secrets
    {
        Up,
        Down
    }

    public enum Direction
    {
        None = 0,
        Up = 1,
        Down = -1,
        Left = 2,
        Right = -2,
        Any = 3
    }

    public enum TileID
    {
        // Active
        Empty = 0,
        Solid = 1,
        SpikeR = 2,
        SpikeD = 3,
        SpikeL = 4,
        SpikeU = 5,
        Platform = 6,
        Hook = 7,
        Pipe = 8,
        FakeSolid = 9,
        Crumble = 10,
        SmallSpikeR = 11,
        SmallSpikeD = 12,
        SmallSpikeL = 13,
        SmallSpikeU = 14,
        Drill = 15,
        Switch1U = 16,
        Switch2U = 17,
        Switch3U = 18,
        Switch4U = 19,
        Switch1P = 20,
        Switch2P = 21,
        Switch3P = 22,
        Switch4P = 23,
        Decor1 = 24,
        Decor2 = 25,
        Breakable = 26,
        ConveySlowR = 27,
        ConveyFastR = 28,
        ConveySlowL = 29,
        ConveyFastL = 30,
        ConveySlowU = 31,
        ConveyFastU = 32,
        Lock = 33,
        LockBarrier = 34,
        ParticleEmit1 = 35,
        ParticleEmit2 = 36,
        ParticleEmit3 = 37,
        ParticleEmit4 = 38,
        PipeAlt = 39,
        Invisible = 40,

        // Overlay
        Water = 30000,
        Pollution = 30001,
        Lava = 30002,
        WaterUB = 30003,
        PollutionUB = 30004,
        LavaUB = 30005,
        SolidOver = 30006,
        GraityBeam = 30007,
        ParticleEmit5 = 30008,
        FakeSolidOver = 30009,

        // Entity
        HoastL = 40000,
        HoastR = 40001,
        Floast = 40002,
        XFloast = 40085,
        Toast = 40004,
        Gasper = 40003,
        SlagDR = 40005,
        SlagDL = 40006,
        SlagUR = 40007,
        SlagUL = 40008,
        ParaslagD = 40009,
        ParaslagU = 40010,
        Kuko = 40011,
        KukoBig = 40072,
        EdemaUR = 40012,
        EdemaDR = 40013,
        EdemaDL = 40014,
        EdemaUL = 40015,
        AnchorFishSR = 40016,
        AnchorFishMR = 40017,
        AnchorFishLR = 40018,
        AnchorFishSL = 40019,
        AnchorFishML = 40020,
        AnchorFishLL = 40021,
        FissureLA = 40027,
        FissureMA = 40045,
        FissureRA = 40026,
        FissureLB = 40029,
        FissureMB = 40046,
        FissureRB = 40028,
        Musk = 40030,
        CrunchS = 40031,
        CrunchM = 40032,
        CrunchL = 40033,
        CroastR = 40034,
        CroastL = 40035,
        Spring = 40036,
        Tumor = 40037,
        MegaTumor = 40111,
        Cartridge = 40038,
        Key = 40073,
        GasCloud = 40047,
        Mother = 40048,
        CannonRA = 40050,
        CannonGA = 40052,
        CannonBA = 40054,
        CannonRB = 40051,
        CannonGB = 40053,
        CannonBB = 40055,
        FireballRA = 40056,
        FireballGA = 40058,
        FireballBA = 40060,
        FireballRB = 40057,
        FireballGB = 40059,
        FireballBB = 40061,
        TargetR = 40062,
        TargetG = 40063,
        TargetB = 40064,
        SingleTurretR = 40022,
        SingleTurretD = 40025,
        SingleTurretL = 40023,
        SingleTurretU = 40024,
        SightTurret = 40049,
        BurstTurretR = 40065,
        BurstTurretD = 40068,
        BurstTurretL = 40066,
        BurstTurretU = 40067,
        SightVTurret = 40069,
        RapidTurret = 40070,
        PredictTurret = 40071,
        GlowMushroom = 40074,
        GlowMushroom2 = 40099,
        AgonS = 40075,
        AgonM = 40077,
        AgonL = 40113,
        Feral = 40076,
        Charger = 40078,
        Drooper = 40080,
        Gorger = 40082,
        Retinara = 40084,
        Thistlefish = 40086,
        Thistlefish2 = 40087,
        WailerA = 40089,
        WailerB = 40090,
        WailerC = 40088,
        Mine = 40091,
        ExitWarp = 40092,
        NPC1 = 40093,
        NPC2 = 40094,
        NPC3 = 40095,
        Decoration1 = 40096,
        Decoration2 = 40097,
        Decoration3 = 40098,
        Ring = 40100,
        LevelGoal = 40110,
        FriendPieces1 = 40101,
        FriendPieces2 = 40102,
        FriendPieces3 = 40103,
        FriendStand = 40104,
        TVConsole = 40107,
        CrusherGear = 40108,
        CrusherEye = 40109,
        Watcher = 40112,
        Critters1 = 40114,
        Critters2 = 40115,
        Critters3 = 40116,
        Critters4 = 40117,
        Critters5 = 40118,
        Critters6 = 40119,
        Critters7 = 40120,
        FriendOrb = 40121,
        AcceptanceTrigger = 40105,

        // Tag
        CameraBounds = 20000,
        Background = 20089,
        Foreground = 20090,
        DebugSpawn = 20001,
        Savepoint = 20102,
        GreenTransitionR = 20002,
        GreenTransitionDR = 20100,
        GreenTransitionD = 20005,
        GreenTransitionDL = 20102,
        GreenTransitionL = 20003,
        GreenTransitionUL = 20101,
        GreenTransitionU = 20004,
        GreenTransitionUR = 20099,
        YellowTransitionR = 20054,
        YellowTransitionDR = 20096,
        YellowTransitionD = 20057,
        YellowTransitionDL = 20098,
        YellowTransitionL = 20055,
        YellowTransitionUL = 20097,
        YellowTransitionU = 20056,
        YellowTransitionUR = 20095,
        CheckPoint = 20103,
        Stop = 20058,
        DirectionRight = 20006,
        DirectionDown = 20009,
        DirectionLeft = 20007,
        DirectionUp = 20008,
        Destroy = 20059,
        MoveExtension = 20010,
        FallingSlow = 20011,
        FallingMed = 20012,
        FallingFast = 20013,
        FallTriggerSlow = 20014,
        FallTriggerMed = 20015,
        FallTriggerFast = 20016,
        NoAccelSlowR = 20017,
        NoAccelSlowD = 20020,
        NoAccelSlowL = 20018,
        NoAccelSlowU = 20019,
        NoAccelMedR = 20021,
        NoAccelMedD = 20024,
        NoAccelMedL = 20022,
        NoAccelMedU = 20023,
        NoAccelFastR = 20025,
        NoAccelFastD = 20028,
        NoAccelFastL = 20026,
        NoAccelFastU = 20027,
        MoveWPauseSlowR = 20029,
        MoveWPauseSlowD = 20032,
        MoveWPauseSlowL = 20030,
        MoveWPauseSlowU = 20031,
        MoveWPauseMedR = 20033,
        MoveWPauseMedD = 20036,
        MoveWPauseMedL = 20034,
        MoveWPauseMedU = 20035,
        MoveWPauseFastR = 20037,
        MoveWPauseFastD = 20040,
        MoveWPauseFastL = 20038,
        MoveWPauseFastU = 20039,
        AccelXSlowR = 20091,
        AccelXSlowD = 20094,
        AccelXSlowL = 20092,
        AccelXSlowU = 20093,
        AccelSlowR = 20041,
        AccelSlowD = 20044,
        AccelSlowL = 20042,
        AccelSlowU = 20043,
        AccelMedR = 20045,
        AccelMedD = 20048,
        AccelMedL = 20046,
        AccelMedU = 20047,
        AccelFastR = 20049,
        AccelFastD = 20052,
        AccelFastL = 20050,
        AccelFastU = 20051,
        NoTile = 20071,
        NoOver = 20072,
        NoBack = 20053,
        Wrap = 20070,
        AscendSlow = 20073,
        AscendMed = 20074,
        AscendFast = 20075,
        WeightedSlow = 20076,
        WeightedMed = 20077,
        WeightedFast = 20078,
        Sine1 = 20079,
        Sine2 = 20080,
        Sine3 = 20081,
        Spawner1 = 20060,
        Spawner2 = 20061,
        Spawner3 = 20062,
        Spawner4 = 20063,
        Despawner1 = 20064,
        Despawner2 = 20065,
        Despawner3 = 20066,
        Despawner4 = 20067,
        CrusherR = 20082,
        CrusherD = 20085,
        CrusherL = 20083,
        CrusherU = 20084,
        Crusher = 20086,
        Warp = 20069,
        WindR = 20087,
        WindL = 20088,

        // Back1
        WholePiece = 50000,
        LargeSideL = 50001,
        LargeSideT = 50002,
        LargeSideR = 50003,
        LargeSideB = 50004,
        SmallSideL = 50005,
        SmallSideT = 50006,
        SmallSideR = 50007,
        SmallSideB = 50008,
        SmallCornerBL = 50009,
        SmallCornerTL = 50010,
        SmallCornerTR = 50011,
        SmallCornerBR = 50012,
        LargeCornerBL = 50013,
        LargeCornerTL = 50014,
        LargeCornerTR = 50015,
        LargeCornerBR = 50016,
        DiagonalBL = 50017,
        DiagonalTL = 50018,
        DiagonalTR = 50019,
        DiagonalBR = 50020,
        Back1Deco1 = 50021,
        Back1Deco2 = 50022,
        Back1Deco3 = 50023,
        Back1Deco4 = 50024,

        // Back2
        WholePiece2 = 50033,
        LargeSideL2 = 50034,
        LargeSideT2 = 50035,
        LargeSideR2 = 50036,
        LargeSideB2 = 50037,
        SmallSideL2 = 50038,
        SmallSideT2 = 50039,
        SmallSideR2 = 50040,
        SmallSideB2 = 50041,
        SmallCornerBL2 = 50042,
        SmallCornerTL2 = 50043,
        SmallCornerTR2 = 50044,
        SmallCornerBR2 = 50045,
        LargeCornerBL2 = 50046,
        LargeCornerTL2 = 50047,
        LargeCornerTR2 = 50048,
        LargeCornerBR2 = 50049,
        DiagonalBL2 = 50050,
        DiagonalTL2 = 50051,
        DiagonalTR2 = 50052,
        DiagonalBR2 = 50053,
        Back2Deco1 = 50054,
        Back2Deco2 = 50055,
        Back2Deco3 = 50056,
        Back2Deco4 = 50057,

        // Unused
        Unused = 20068,
        Unused1 = 20104,
        Unused2 = 40079,
        Unused3 = 40081,
        Unused4 = 40083,
        Unused5 = 40039,
        Unused6 = 40040,
        Unused7 = 40041,
        Unused8 = 40042,
        Unused9 = 40043,
        Unused10 = 40044,
        Unused11 = 40106,
        Unused12 = 50025,
        Unused13 = 50025,
        Unused14 = 50026,
        Unused15 = 50027,
        Unused16 = 50028,
        Unused17 = 50029,
        Unused18 = 50030,
        Unused19 = 50031,
        Unused20 = 50032,



        /*
         * Numbering scheme for tiles
         * 
         * Base Game:
         * 
         *     x - active layer tiles, use the lowest numbers (0 - 40)
         * 2xxxx - tag layer tiles
         * 3xxxx - overlay layer tiles
         * 4xxxx - entity tiles (use the active layer)
         * 5xxxx - back1 and back2 layer tiles
         * 
         * 
         * Randomizer:
         * 
         * 9xxxx - root number for all randomizer tiles
         * 
         * 90xxx - colored tiles in active layer
         * 900xx - number tags
         * 901xx - blue   tiles
         * 902xx - yellow tiles
         * 903xx - red    tiles
         * 904xx - green  tiles
         * 
         * 9140xxx - blue   entities
         * 9240xxx - yellow entities
         * 9340xxx - red    entities
         * 9440xxx - green  entities
         *
         * 92xxx - used for markers (tag layer)
         * 920xx - tag markers   (not sure what to use for)
         * 921xx - merge markers (probably going to remove)
         * 
         * 922xx - custom movement tags
         * 
         * 
         * 94xxx - generalized enemy placers
         * 
         * -----
         * 
         * Randomizer Colored Tile Variants:
         * 
         * NUMBER TAGS
         *  used to indicate the X value for the colored tiles below
         *  must be contiguous with the colored tiles
         *  will adopt the tile id of the first tile in the contiguous group
         * 
         * BLUE     "alt me"
         *  the randomizer will randomly convert the tile into one of its variants
         *  this tile conversion will be applied even if level corruptions are not
         *  e.g. blue ceiling spikes have a chance to turn into ceiling decor, small spikes, or be deleted
         * 
         * YELLOW   "place X tiles in range"
         *  operates on a set of contiguous tiles
         *  places X number of tiles in the given contiguous range
         *  (((the remaining tiles wilt be alted, if applicable))) no they wont but maybe for another color
         * 
         * RED      "no more than X in a row"
         *  operates on a set of contiguous tiles (best to be used horizontally xor vertically, not both)
         *  will flip a coin on whether to place a tile, keeping count of how many have been placed in a row
         *  once the length of tiles placed in a row = X, the next tile must be empty
         *  e.g. create a row of spikes in random locations, ensuring that player will always be able to make the jump
         * 
         * GREEN    "no more than X blank between"
         *  operates on a set of contiguous tiles (best to be used horizontally xor vertically, not both)
         *  similar to red tiles, but the opposite
         *  may not be implemented, only real use would be hooks (and I may just make red hooks act this way)
         * 
         * How are contiguous tiles found?
         * 
         * level is iterated upon by column within row iteration
         * upon finding first colored tile or number tile, create a list of contiguous tiles and add the first tile to it (probably best to use a hash set, since we do not want any repeats)
         * increment along the list
         * check all neighboring tiles, if they are of the same type, add them to the list
         *
         * -----
         * 
         * Custom Movement Tags:
         * 
         * green directional tags    (operate on a contiguous set of tiles)
         *  bi-directional arrow tag (gets replaced with a random speed movement tag going either direction)
         *  four-direction arrow tag (gets replaced by a random movement tag going any direction)
         *  no-accel variants of above tags (similar to the above tags, but it makes sure they do not select an accelerating movement type)
         * 
         * 
         * 
         * 
        */

        // Entity Placers
        /*EP_GroundType    = 94000,
        EP_BouncyType    = 94000,
        EP_FloaterType   = 94000,
        EP_BouncyFloater = 94000,
        EP_CannonType    = 94000,

        EP_DecoType      = 94000,
        EP_PickupType    = 94000,
        EP_NPC           = 94000,

        EP_Edema = 94000,



        EP_GunTypeR = 94101,
        EP_GunTypeD = 94102,
        EP_GunTypeL = 94103,
        EP_GunTypeU = 94104,

        EP_MaybeTumor = 94000,*/



        // Markers
        OOBMarker = 92000,      //this marker is essential for the level generator,
                                //it prevents the generator from overwriting parts of a level that it has already written to
                                //it marks areas outside the generated bounds so that it can later fill them in
        TagMarker1  = 92001,
        TagMarker2  = 92002,
        TagMarker3  = 92003,
        TagMarker4  = 92004,

        // Merge Markers
        MergeMarkerR = 92101,
        MergeMarkerD = 92102,
        MergeMarkerL = 92103,
        MergeMarkerU = 92104,

        // Number tags (active layer)
        ANumber1 = 91,
        ANumber2 = 92,
        ANumber3 = 93,
        ANumber4 = 94,
        ANumber5 = 95,
        ANumber6 = 96,
        ANumber7 = 97,
        ANumber8 = 98,
        ANumber9 = 99,

        // Colored Entities
        KukoYellow = 9240011,
        KukoRed = 9240011,
        KukoGreen = 9240011,

        // Blue Tiles
        BlueSolid = 90101,
        BlueSpikeR = 90102,
        BlueSpikeD = 90103,
        BlueSpikeL = 90104,
        BlueSpikeU = 90105,
        BlueSmallSpikeR = 90111,
        BlueSmallSpikeD = 90112,
        BlueSmallSpikeL = 90113,
        BlueSmallSpikeU = 90114,

        // Yellow Tiles
        YellowSolid = 90201,
        YellowSpikeR = 90202,
        YellowSpikeD = 90203,
        YellowSpikeL = 90204,
        YellowSpikeU = 90205,
        YellowPlatform = 90206,
        YellowCrumble = 90210,
        YellowSmallSpikeR = 90211,
        YellowSmallSpikeD = 90212,
        YellowSmallSpikeL = 90213,
        YellowSmallSpikeU = 90214,
        YellowSwitch1U = 90216,
        YellowSwitch2U = 90217,
        YellowSwitch3U = 90218,
        YellowSwitch4U = 90219,
        YellowSwitch1P = 90220,
        YellowSwitch2P = 90221,
        YellowSwitch3P = 90222,
        YellowSwitch4P = 90223,

        // Red Tiles
        RedSolid = 90301,
        RedSpikeR = 90302,
        RedSpikeD = 90303,
        RedSpikeL = 90304,
        RedSpikeU = 90305,
        RedPlatform = 90306,
        RedCrumble = 90310,
        RedSmallSpikeR = 90311,
        RedSmallSpikeD = 90312,
        RedSmallSpikeL = 90313,
        RedSmallSpikeU = 90314,

        // Green Tiles
        GreenSolid = 90401,
        GreenSpikeR = 90402,
        GreenSpikeD = 90403,
        GreenSpikeL = 90404,
        GreenSpikeU = 90405,
        GreenPlatform = 90406,
        GreenHook = 90407,
        GreenCrumble = 90410,
        GreenSmallSpikeR = 90411,
        GreenSmallSpikeD = 90412,
        GreenSmallSpikeL = 90413,
        GreenSmallSpikeU = 90414,


        // Blue Entity
        // HoastL = 9140000,
        // HoastR = 9140001,
        // Floast = 9140002,
        // XFloast = 9140085,
        // Toast = 9140004,
        // Gasper = 9140003,
        // SlagDR = 9140005,
        // SlagDL = 9140006,
        // SlagUR = 9140007,
        // SlagUL = 9140008,
        // ParaslagD = 9140009,
        // ParaslagU = 9140010,
        // Kuko = 9140011,
        // KukoBig = 9140072,
        // EdemaUR = 9140012,
        // EdemaDR = 9140013,
        // EdemaDL = 9140014,
        // EdemaUL = 9140015,
        // AnchorFishSR = 9140016,
        // AnchorFishMR = 9140017,
        // AnchorFishLR = 9140018,
        // AnchorFishSL = 9140019,
        // AnchorFishML = 9140020,
        // AnchorFishLL = 9140021,
        // FissureLA = 9140027,
        // FissureMA = 9140045,
        // FissureRA = 9140026,
        // FissureLB = 9140029,
        // FissureMB = 9140046,
        // FissureRB = 9140028,
        // Musk = 9140030,
        // CrunchS = 9140031,
        // CrunchM = 9140032,
        // CrunchL = 9140033,
        // CroastR = 9140034,
        // CroastL = 9140035,
        // Spring = 9140036,
        // Tumor = 9140037,
        // MegaTumor = 9140111,
        // Cartridge = 9140038,
        // Key = 9140073,
        // GasCloud = 9140047,
        // Mother = 9140048,
        // CannonRA = 9140050,
        // CannonGA = 9140052,
        // CannonBA = 9140054,
        // CannonRB = 9140051,
        // CannonGB = 9140053,
        // CannonBB = 9140055,
        // FireballRA = 9140056,
        // FireballGA = 9140058,
        // FireballBA = 9140060,
        // FireballRB = 9140057,
        // FireballGB = 9140059,
        // FireballBB = 9140061,
        // TargetR = 9140062,
        // TargetG = 9140063,
        // TargetB = 9140064,
        // SingleTurretR = 9140022,
        // SingleTurretD = 9140025,
        // SingleTurretL = 9140023,
        // SingleTurretU = 9140024,
        // SightTurret = 9140049,
        // BurstTurretR = 9140065,
        // BurstTurretD = 9140068,
        // BurstTurretL = 9140066,
        // BurstTurretU = 9140067,
        // SightVTurret = 9140069,
        // RapidTurret = 9140070,
        // PredictTurret = 9140071,
        // GlowMushroom = 9140074,
        // GlowMushroom2 = 9140099,
        // AgonS = 9140075,
        // AgonM = 9140077,
        // AgonL = 9140113,
        // Feral = 9140076,
        // Charger = 9140078,
        // Drooper = 9140080,
        // Gorger = 9140082,
        // Retinara = 9140084,
        // Thistlefish = 9140086,
        // Thistlefish2 = 9140087,
        // WailerA = 9140089,
        // WailerB = 9140090,
        // WailerC = 9140088,
        // Mine = 9140091,
        // ExitWarp = 9140092,
        // NPC1 = 9140093,
        // NPC2 = 9140094,
        // NPC3 = 9140095,
        // Decoration1 = 9140096,
        // Decoration2 = 9140097,
        // Decoration3 = 9140098,
        // Ring = 9140100,
        // LevelGoal = 9140110,
        // FriendPieces1 = 9140101,
        // FriendPieces2 = 9140102,
        // FriendPieces3 = 9140103,
        // FriendStand = 9140104,
        // TVConsole = 9140107,
        // CrusherGear = 9140108,
        // CrusherEye = 9140109,
        // Watcher = 9140112,
        // Critters1 = 9140114,
        // Critters2 = 9140115,
        // Critters3 = 9140116,
        // Critters4 = 9140117,
        // Critters5 = 9140118,
        // Critters6 = 9140119,
        // Critters7 = 9140120,
        // FriendOrb = 9140121,
        // AcceptanceTrigger = 9140105,

        // Yellow Entity
        // HoastL = 9240000,
        // HoastR = 9240001,
        // Floast = 9240002,
        // XFloast = 9240085,
        // Toast = 9240004,
        // Gasper = 9240003,
        // SlagDR = 9240005,
        // SlagDL = 9240006,
        // SlagUR = 9240007,
        // SlagUL = 9240008,
        // ParaslagD = 9240009,
        // ParaslagU = 9240010,
        // Kuko = 9240011,
        // KukoBig = 9240072,
        // EdemaUR = 9240012,
        // EdemaDR = 9240013,
        // EdemaDL = 9240014,
        // EdemaUL = 9240015,
        // AnchorFishSR = 9240016,
        // AnchorFishMR = 9240017,
        // AnchorFishLR = 9240018,
        // AnchorFishSL = 9240019,
        // AnchorFishML = 9240020,
        // AnchorFishLL = 9240021,
        // FissureLA = 9240027,
        // FissureMA = 9240045,
        // FissureRA = 9240026,
        // FissureLB = 9240029,
        // FissureMB = 9240046,
        // FissureRB = 9240028,
        // Musk = 9240030,
        // CrunchS = 9240031,
        // CrunchM = 9240032,
        // CrunchL = 9240033,
        // CroastR = 9240034,
        // CroastL = 9240035,
        // Spring = 9240036,
        // Tumor = 9240037,
        // MegaTumor = 9240111,
        // Cartridge = 9240038,
        // Key = 9240073,
        // GasCloud = 9240047,
        // Mother = 9240048,
        // CannonRA = 9240050,
        // CannonGA = 9240052,
        // CannonBA = 9240054,
        // CannonRB = 9240051,
        // CannonGB = 9240053,
        // CannonBB = 9240055,
        // FireballRA = 9240056,
        // FireballGA = 9240058,
        // FireballBA = 9240060,
        // FireballRB = 9240057,
        // FireballGB = 9240059,
        // FireballBB = 9240061,
        // TargetR = 9240062,
        // TargetG = 9240063,
        // TargetB = 9240064,
        // SingleTurretR = 9240022,
        // SingleTurretD = 9240025,
        // SingleTurretL = 9240023,
        // SingleTurretU = 9240024,
        // SightTurret = 9240049,
        // BurstTurretR = 9240065,
        // BurstTurretD = 9240068,
        // BurstTurretL = 9240066,
        // BurstTurretU = 9240067,
        // SightVTurret = 9240069,
        // RapidTurret = 9240070,
        // PredictTurret = 9240071,
        // GlowMushroom = 9240074,
        // GlowMushroom2 = 9240099,
        // AgonS = 9240075,
        // AgonM = 9240077,
        // AgonL = 9240113,
        // Feral = 9240076,
        // Charger = 9240078,
        // Drooper = 9240080,
        // Gorger = 9240082,
        // Retinara = 9240084,
        // Thistlefish = 9240086,
        // Thistlefish2 = 9240087,
        // WailerA = 9240089,
        // WailerB = 9240090,
        // WailerC = 9240088,
        // Mine = 9240091,
        // ExitWarp = 9240092,
        // NPC1 = 9240093,
        // NPC2 = 9240094,
        // NPC3 = 9240095,
        // Decoration1 = 9240096,
        // Decoration2 = 9240097,
        // Decoration3 = 9240098,
        // Ring = 9240100,
        // LevelGoal = 9240110,
        // FriendPieces1 = 9240101,
        // FriendPieces2 = 9240102,
        // FriendPieces3 = 9240103,
        // FriendStand = 9240104,
        // TVConsole = 9240107,
        // CrusherGear = 9240108,
        // CrusherEye = 9240109,
        // Watcher = 9240112,
        // Critters1 = 9240114,
        // Critters2 = 9240115,
        // Critters3 = 9240116,
        // Critters4 = 9240117,
        // Critters5 = 9240118,
        // Critters6 = 9240119,
        // Critters7 = 9240120,
        // FriendOrb = 9240121,
        // AcceptanceTrigger = 9240105,



    }

}
