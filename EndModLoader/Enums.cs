using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEiNRandomizer
{
    public enum AppState
    {
        IncorrectPath,
        NoModsFound,
        NoModSelected,
        InMenus,
        InGame
    }
    public enum PoolType
    {
        None      =      0,
        Standard  = 1 << 0,
        Connector = 1 << 1,
        Secret    = 1 << 2,
        Cart      = 1 << 3
    }

    public enum AreaType
    {
        None     = 0,
        normal   = 1,
        dark     = 2,
        cart     = 3,
        ironcart = 4,
        glitch   = 5
    }

    [Flags]
    public enum Collectables
    {
        None       =      0,
        Tumor      = 1 << 0,
        MegaTumor  = 1 << 1,
        Cartridge  = 1 << 2,
        Key        = 1 << 3,
        Rings      = 1 << 4,
        FriendHead = 1 << 5,
        FriendBody = 1 << 6,
        FriendSoul = 1 << 7,
        LevelGoal  = 1 << 8,
        ExitWarp   = 1 << 9,
        FriendOrb  = 1 << 10
    }

    [Flags]
    public enum Directions
    {
        None = 0,

        R  = 1 << 0,
        L  = 1 << 1,
        D  = 1 << 2,
        U  = 1 << 3,
        UR = 1 << 4,
        DR = 1 << 5,
        UL = 1 << 6,
        DL = 1 << 7,

        All = 0b_1111_1111
    }
}
