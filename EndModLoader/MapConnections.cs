using System;

namespace TEiNRandomizer
{
    //public partial class Randomizer
    //{
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
        ConnectionType GetDirection(Directions dir)
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
        void SetDirection(Directions dir, ConnectionType con)
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

            if (U != ConnectionType.None) ret |= Directions.U;
            if (D != ConnectionType.None) ret |= Directions.D;
            if (L != ConnectionType.None) ret |= Directions.L;
            if (R != ConnectionType.None) ret |= Directions.R;
            if (UR != ConnectionType.None) ret |= Directions.UR;
            if (DR != ConnectionType.None) ret |= Directions.DR;
            if (UL != ConnectionType.None) ret |= Directions.UL;
            if (DL != ConnectionType.None) ret |= Directions.DL;

            return ret;
        }

        public void SetMultiple(Directions dir, ConnectionType con)
        {
            if (dir.HasFlag(Directions.U)) U = con;
            if (dir.HasFlag(Directions.D)) D = con;
            if (dir.HasFlag(Directions.L)) L = con;
            if (dir.HasFlag(Directions.R)) R = con;

            if (dir.HasFlag(Directions.UR)) UR = con;
            if (dir.HasFlag(Directions.DR)) DR = con;
            if (dir.HasFlag(Directions.UL)) UL = con;
            if (dir.HasFlag(Directions.DL)) DL = con;
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

            if (a.U == b.U
                && a.D == b.D
                && a.L == b.L
                && a.R == b.R
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

            if (a.U == b.U
                && a.D == b.D
                && a.L == b.L
                && a.R == b.R
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
            if (U != ConnectionType.None) str += $"U : {U }\n";
            if (D != ConnectionType.None) str += $"D : {D }\n";
            if (L != ConnectionType.None) str += $"L : {L }\n";
            if (R != ConnectionType.None) str += $"R : {R }\n";
            if (UR != ConnectionType.None) str += $"UR: {UR}\n";
            if (DR != ConnectionType.None) str += $"DR: {DR}\n";
            if (UL != ConnectionType.None) str += $"UL: {UL}\n";
            if (DL != ConnectionType.None) str += $"DL: {DL}\n";
            return str;
        }
    }

    [Flags]
    public enum ConnectionType
    {
        None     = 0b0000,
        Entrance = 0b0001,
        Exit     = 0b0010,
        Secret   = 0b0100,
        Lockable = 0b1000,  

        Both = Entrance | Exit,
        All  = Entrance | Exit | Secret,
    }
}
//}
