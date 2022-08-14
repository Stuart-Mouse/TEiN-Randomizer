using System;

namespace TEiNRandomizer
{
    public struct MapConnections
    {
        // MapConnections struct and ConnectionType enum are used to define a level's connection

        // A MapConnections struct holds the connection type for each direction
        public ConnectionType U;
        public ConnectionType D;
        public ConnectionType L;
        public ConnectionType R;

        public ConnectionType UR;
        public ConnectionType DR;
        public ConnectionType UL;
        public ConnectionType DL;

        public ConnectionType this[Directions key]
        {
            get => GetDirection(key);
            set => SetDirection(key, value);
        }

        // Get ConnectionType by direction
        public ConnectionType GetDirection(Directions dir)
        {
            // Switch to the correct connection based on direction passed in
            switch (dir)
            {
                case Directions.U:
                    return U;
                case Directions.D:
                    return D;
                case Directions.L:
                    return L;
                case Directions.R:
                    return R;
                case Directions.UR:
                    return UR;
                case Directions.DR:
                    return DR;
                case Directions.UL:
                    return UL;
                case Directions.DL:
                    return DL;
                default:
                    throw new Exception("Invalid direction, cannot be used to index ConnectionType.");
            }
        }
        public void SetDirection(Directions dir, ConnectionType con)
        {
            // Switch to the correct connection based on direction passed in
            switch (dir)
            {
                case Directions.U:
                    U = con; 
                    return;
                case Directions.D:
                    D = con; 
                    return;
                case Directions.L:
                    L = con; 
                    return;
                case Directions.R:
                    R = con; 
                    return;
                case Directions.UR:
                    UR = con; 
                    return;
                case Directions.DR:
                    DR = con; 
                    return;
                case Directions.UL:
                    UL = con; 
                    return;
                case Directions.DL:
                    DL = con; 
                    return;
                default:
                    throw new Exception("Invalid direction, cannot be used to index ConnectionType.");
            }
        }
        public Directions Flatten()
        {
            // Basically just an all-or-none converstion from MapConnections to Directions
            // If the MapConnections in any direction is not none, then it is considered to be a connection of some sort
            Directions ret = Directions.None;

            if (U != ConnectionType.none) ret |= Directions.U;
            if (D != ConnectionType.none) ret |= Directions.D;
            if (L != ConnectionType.none) ret |= Directions.L;
            if (R != ConnectionType.none) ret |= Directions.R;
            if (UR != ConnectionType.none) ret |= Directions.UR;
            if (DR != ConnectionType.none) ret |= Directions.DR;
            if (UL != ConnectionType.none) ret |= Directions.UL;
            if (DL != ConnectionType.none) ret |= Directions.DL;

            return ret;
        }

        // Operator overloads for easily comparing Map Connections
        public static MapConnections operator &(MapConnections a, MapConnections b)
        {
            // Overloaded AND operator performs AND on all members

            MapConnections ret;

            ret.U = a.U & b.U;
            ret.D = a.D & b.D;
            ret.L = a.L & b.L;
            ret.R = a.R & b.R;

            ret.UR = a.UR & b.UR;
            ret.DR = a.DR & b.DR;
            ret.UL = a.UL & b.UL;
            ret.DL = a.DL & b.DL;

            return ret;
        }
        public static MapConnections operator |(MapConnections a, MapConnections b)
        {
            // Overloaded OR operator performs OR on all members

            MapConnections ret;

            ret.U = a.U | b.U;
            ret.D = a.D | b.D;
            ret.L = a.L | b.L;
            ret.R = a.R | b.R;

            ret.UR = a.UR | b.UR;
            ret.DR = a.DR | b.DR;
            ret.UL = a.UL | b.UL;
            ret.DL = a.DL | b.DL;

            return ret;
        }
        public static MapConnections operator ~(MapConnections a)
        {
            // Overloaded NOT operator performs NOT on all members

            MapConnections ret;

            ret.U = ~a.U;
            ret.D = ~a.D;
            ret.L = ~a.L;
            ret.R = ~a.R;

            ret.UR = ~a.UR;
            ret.DR = ~a.DR;
            ret.UL = ~a.UL;
            ret.DL = ~a.DL;

            return ret;
        }
        public static bool operator ==(MapConnections a, MapConnections b)
        {
            // Overloaded == operator checks equality of all members

            if (a.U  == b.U
             && a.D  == b.D
             && a.L  == b.L
             && a.R  == b.R
             && a.UR == b.UR
             && a.DR == b.DR
             && a.UL == b.UL
             && a.DL == b.DL)
                return true;
            return false;
        }
        public static bool operator !=(MapConnections a, MapConnections b)
        {
            // Overloaded != operator checks equality of all members, but with reversed return values as compared to == operator
            // This is only included because the language requires it

            if (a.U  == b.U
             && a.D  == b.D
             && a.L  == b.L
             && a.R  == b.R
             && a.UR == b.UR
             && a.DR == b.DR
             && a.UL == b.UL
             && a.DL == b.DL)
                return false;
            return true;
        }
        public string DebugString()
        {
            string str = "";
            if (U  != ConnectionType.none) str += $"U : {U }\n";
            if (D  != ConnectionType.none) str += $"D : {D }\n";
            if (L  != ConnectionType.none) str += $"L : {L }\n";
            if (R  != ConnectionType.none) str += $"R : {R }\n";
            if (UR != ConnectionType.none) str += $"UR: {UR}\n";
            if (DR != ConnectionType.none) str += $"DR: {DR}\n";
            if (UL != ConnectionType.none) str += $"UL: {UL}\n";
            if (DL != ConnectionType.none) str += $"DL: {DL}\n";
            return str;
        }
    }

    [Flags]
    public enum ConnectionType
    {
        // ConnectionType is an enum of flags, so it is easy to use binary operators on the values
        
        none        = 0b0000,
        entrance    = 0b0001,
        exit        = 0b0010,
        secret      = 0b0100,
        locked      = 0b1000,

        both = entrance | exit,
        all  = entrance | exit | secret,

        /*secretExit     = secret | exit,
        secretEntrance = secret | entrance,

        lockedEntrance = locked | entrance,
        lockedExit     = locked | exit,
        lockedBoth     = locked | both,
        lockedSecret   = locked | secret,
        lockedAll      = locked | all*/
    }
}
