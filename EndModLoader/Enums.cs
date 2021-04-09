﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    public enum AreaTypes
    {
        normal,
        dark,
        cart,
        ironcart,
        glitch
    }

    public enum AltLevels
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

    public enum TileID
    {
        // Active
        Empty           = 0,
        Solid           = 1,
        SpikeR          = 2,
        SpikeD          = 3,
        SpikeL          = 4,
        SpikeU          = 5,
        Platform        = 6,
        Hook            = 7,
        Pipe            = 8,
        FakeSolid       = 9,
        Crumble         = 10,
        SmallSpikeR     = 11,
        SmallSpikeD     = 12,
        SmallSpikeL     = 13,
        SmallSpikeU     = 14,
        Drill           = 15,
        Switch1U        = 16,
        Switch2U        = 17,
        Switch3U        = 18,
        Switch4U        = 19,
        Switch1P        = 20,
        Switch2P        = 21,
        Switch3P        = 22,
        Switch4P        = 23,
        Decor1          = 24,
        Decor2          = 25,
        Breakable       = 26,
        ConveySlowR     = 27,
        ConveyFastR     = 28,
        ConveySlowL     = 29,
        ConveyFastL     = 30,
        ConveySlowU     = 31,
        ConveyFastU     = 32,
        Lock            = 33,
        LockBarrier     = 34,
        ParticleEmit1   = 35,
        ParticleEmit2   = 36,
        ParticleEmit3   = 37,
        ParticleEmit4   = 38,
        PipeAlt         = 39,
        Invisible       = 40,

        // Overlay
        Water           = 30000,
        Pollution       = 30001,
        Lava            = 30002,
        WaterUB         = 30003,
        PollutionUB     = 30004,
        LavaUB          = 30005,
        SolidOver       = 30006,
        GraityBeam      = 30007,
        ParticleEmit5   = 30008,
        FakeSolidOver   = 30009,

        // Entity
        HoastL              = 40000,
        HoastR              = 40001,
        Floast              = 40002,
        XFloast             = 40085,
        Toast               = 40004,
        Gasper              = 40003,
        SlagDR              = 40005,
        SlagDL              = 40006,
        SlagUR              = 40007,
        SlagUL              = 40008,
        ParaslagD           = 40009,
        ParaslagU           = 40010,
        Kuko                = 40011,
        KukoBig             = 40072,
        EdemaUR             = 40012,
        EdemaDR             = 40013,
        EdemaDL             = 40014,
        EdemaUL             = 40015,
        AnchorFishSR        = 40016,
        AnchorFishMR        = 40017,
        AnchorFishLR        = 40018,
        AnchorFishSL        = 40019,
        AnchorFishML        = 40020,
        AnchorFishLL        = 40021,
        FissureLA           = 40027,
        FissureMA           = 40045,
        FissureRA           = 40026,
        FissureLB           = 40029,
        FissureMB           = 40046,
        FissureRB           = 40028,
        Musk                = 40030,
        CrunchS             = 40031,
        CrunchM             = 40032,
        CrunchL             = 40033,
        CroastR             = 40034,
        CroastL             = 40035,
        Spring              = 40036,
        Tumor               = 40037,
        MegaTumor           = 40111,
        Cartridge           = 40038,
        Key                 = 40073,
        GasCloud            = 40047,
        Mother              = 40048,
        CannonRA            = 40050,
        CannonGA            = 40052,
        CannonBA            = 40054,
        CannonRB            = 40051,
        CannonGB            = 40053,
        CannonBB            = 40055,
        FireballRA          = 40056,
        FireballGA          = 40058,
        FireballBA          = 40060,
        FireballRB          = 40057,
        FireballGB          = 40059,
        FireballBB          = 40061,
        TargetR             = 40062,
        TargetG             = 40063,
        TargetB             = 40064,
        SingleTurretR       = 40022,
        SingleTurretD       = 40025,
        SingleTurretL       = 40023,
        SingleTurretU       = 40024,
        SightTurret         = 40049,
        BurstTurretR        = 40065,
        BurstTurretD        = 40068,
        BurstTurretL        = 40066,
        BurstTurretU        = 40067,
        SightVTurret        = 40069,
        RapidTurret         = 40070,
        PredictTurret       = 40071,
        GlowMushroom        = 40074,
        GlowMushroom2       = 40099,
        AgonS               = 40075,
        AgonM               = 40077,
        AgonL               = 40113,
        Feral               = 40076,
        Charger             = 40078,
        Drooper             = 40080,
        Gorger              = 40082,
        Retinara            = 40084,
        Thistlefish         = 40086,
        Thistlefish2        = 40087,
        WailerA             = 40089,
        WailerB             = 40090,
        WailerC             = 40088,
        Mine                = 40091,
        ExitWarp            = 40092,
        NPC1                = 40093,
        NPC2                = 40094,
        NPC3                = 40095,
        Decoration1         = 40096,
        Decoration2         = 40097,
        Decoration3         = 40098,
        Ring                = 40110,
        LevelGoal           = 40110,
        FriendPieces1       = 40101,
        FriendPieces2       = 40102,
        FriendPieces3       = 40103,
        FriendStand         = 40104,
        TVConsole           = 40107,
        CrusherGear         = 40108,
        CrusherEye          = 40109,
        Watcher             = 40112,
        Critters1           = 40114,
        Critters2           = 40115,
        Critters3           = 40116,
        Critters4           = 40117,
        Critters5           = 40118,
        Critters6           = 40119,
        Critters7           = 40120,
        FriendOrb           = 40121,
        AcceptanceTrigger   = 40105,

        // Tag
        CameraBounds        = 20000,
        Background          = 20089,
        Foreground          = 20090,
        DebugSpawn          = 20001,
        Savepoint           = 20102,
        GreenTransitionR    = 20001,
        GreenTransitionDR   = 20100,
        GreenTransitionD    = 20001,
        GreenTransitionDL   = 20105,
        GreenTransitionL    = 20003,
        GreenTransitionUL   = 20101,
        GreenTransitionU    = 20004,
        GreenTransitionUR   = 20099,
        YellowTransitionR   = 20054,
        YellowTransitionDR  = 20096,
        YellowTransitionD   = 20057,
        YellowTransitionDL  = 20098,
        YellowTransitionL   = 20055,
        YellowTransitionUL  = 20097,
        YellowTransitionU   = 20056,
        YellowTransitionUR  = 20095,
        CheckPoint          = 20103,
        Stop                = 20058,
        DirectionRight      = 20006,
        DirectionDown       = 20009,
        DirectionLeft       = 20007,
        DirectionUp         = 20008,
        Destroy             = 20059,
        MoveExtension       = 20010,
        FallingSlow         = 20011,
        FallingMed          = 20012,
        FallingFast         = 20013,
        FallTriggerSlow     = 20014,
        FallTriggerMed      = 20015,
        FallTriggerFast     = 20016,
        NoAccelSlowR        = 20017,
        NoAccelSlowD        = 20020,
        NoAccelSlowL        = 20018,
        NoAccelSlowU        = 20019,
        NoAccelMedR         = 20021,
        NoAccelMedD         = 20024,
        NoAccelMedL         = 20022,
        NoAccelMedU         = 20023,
        NoAccelFastR        = 20025,
        NoAccelFastD        = 20028,
        NoAccelFastL        = 20026,
        NoAccelFastU        = 20027,
        MoveWPauseSlowR     = 20029,
        MoveWPauseSlowD     = 20032,
        MoveWPauseSlowL     = 20030,
        MoveWPauseSlowU     = 20031,
        MoveWPauseMedR      = 20033,
        MoveWPauseMedD      = 20036,
        MoveWPauseMedL      = 20034,
        MoveWPauseMedU      = 20035,
        MoveWPauseFastR     = 20037,
        MoveWPauseFastD     = 20040,
        MoveWPauseFastL     = 20038,
        MoveWPauseFastU     = 20039,
        AccelXSlowR         = 20091,
        AccelXSlowD         = 20094,
        AccelXSlowL         = 20092,
        AccelXSlowU         = 20093,
        AccelSlowR          = 20041,
        AccelSlowD          = 20044,
        AccelSlowL          = 20042,
        AccelSlowU          = 20043,
        AccelMedR           = 20045,
        AccelMedD           = 20048,
        AccelMedL           = 20046,
        AccelMedU           = 20047,
        AccelFastR          = 20049,
        AccelFastD          = 20052,
        AccelFastL          = 20050,
        AccelFastU          = 20051,
        NoTile              = 20071,
        NoOver              = 20072,
        NoBack              = 20053,
        Wrap                = 20070,
        AscendSlow          = 20073,
        AscendMed           = 20074,
        AscendFast          = 20075,
        WeightedSlow        = 20076,
        WeightedMed         = 20077,
        WeightedFast        = 20078,
        Sine1               = 20079,
        Sine2               = 20080,
        Sine3               = 20081,
        Spawner1            = 20060,
        Spawner2            = 20061,
        Spawner3            = 20062,
        Spawner4            = 20063,
        Despawner1          = 20064,
        Despawner2          = 20065,
        Despawner3          = 20066,
        Despawner4          = 20067,
        CrusherR            = 20082,
        CrusherD            = 20085,
        CrusherL            = 20083,
        CrusherU            = 20084,
        Crusher             = 20086,
        Warp                = 20069,
        WindR               = 20087,
        WindL               = 20088,

        // Back1
        WholePiece      = 50000,
        LargeSideR      = 50001,
        LargeSideD      = 50002,
        LargeSideU      = 50003,
        LargeSideL      = 50004,
        SmallSideR      = 50005,
        SmallSideD      = 50006,
        SmallSideU      = 50007,
        SmallSideL      = 50008,
        SmallCornerR    = 50009,
        SmallCornerD    = 50010,
        SmallCornerU    = 50011,
        SmallCornerL    = 50012,
        LargeCornerR    = 50013,
        LargeCornerD    = 50014,
        LargeCornerU    = 50015,
        LargeCornerL    = 50016,
        DiagonalR       = 50017,
        DiagonalD       = 50018,
        DiagonalU       = 50019,
        DiagonalL       = 50020,
        DecorationR     = 50021,
        DecorationD     = 50022,
        DecorationU     = 50023,
        DecorationL     = 50024,

        // Back2
        WholePiece2     = 50033,
        LargeSideR2     = 50034,
        LargeSideD2     = 50035,
        LargeSideU2     = 50036,
        LargeSideL2     = 50037,
        SmallSideR2     = 50038,
        SmallSideD2     = 50039,
        SmallSideU2     = 50040,
        SmallSideL2     = 50041,
        SmallCornerR2   = 50042,
        SmallCornerD2   = 50043,
        SmallCornerU2   = 50044,
        SmallCornerL2   = 50045,
        LargeCornerR2   = 50046,
        LargeCornerD2   = 50047,
        LargeCornerU2   = 50048,
        LargeCornerL2   = 50049,
        DiagonalR2      = 50050,
        DiagonalD2      = 50051,
        DiagonalU2      = 50052,
        DiagonalL2      = 50053,
        DecorationR2    = 50054,
        DecorationD2    = 50055,
        DecorationU2    = 50056,
        DecorationL2    = 50057,

        // Unused
        Unused      = 20068,
        Unused1     = 20104,
        Unused2     = 40079,
        Unused3     = 40081,
        Unused4     = 40083,
        Unused5     = 40039,
        Unused6     = 40040,
        Unused7     = 40041,
        Unused8     = 40042,
        Unused9     = 40043,
        Unused10    = 40044,
        Unused11    = 40106,
        Unused12    = 50025,
        Unused13    = 50025,
        Unused14    = 50026,
        Unused15    = 50027,
        Unused16    = 50028,
        Unused17    = 50029,
        Unused18    = 50030,
        Unused19    = 50031,
        Unused20    = 50032,
    }

}
