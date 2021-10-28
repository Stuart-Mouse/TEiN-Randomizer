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

        // Operator overloads for easily comparing Map Connections
        public static MapConnections operator &(MapConnections a, MapConnections b)
        {
            // Overloaded & operator performs & on all members

            MapConnections ret;

            ret.U = a.U & b.U;
            ret.D = a.D & b.D;
            ret.L = a.L & b.L;
            ret.R = a.R & b.R;

            return ret;
        }
        public static MapConnections operator ~(MapConnections a)
        {
            // Overloaded & operator performs & on all members

            MapConnections ret;

            ret.U = ~a.U;
            ret.D = ~a.D;
            ret.L = ~a.L;
            ret.R = ~a.R;

            return ret;
        }
        public static bool operator ==(MapConnections a, MapConnections b)
        {
            // Overloaded == operator checks equality of all members

            if (a.U == b.U)
                if (a.D == b.D)
                    if (a.L == b.L)
                        if (a.R == b.R)
                            return true;
            return false;
        }
        public static bool operator !=(MapConnections a, MapConnections b)
        {
            // Overloaded != operator checks equality of all members, but with reversed return values as compared to == operator
            // This is only included because the language requires it

            if (a.U == b.U)
                if (a.D == b.D)
                    if (a.L == b.L)
                        if (a.R == b.R)
                            return false;
            return true;
        }
    }
    [Flags]
    public enum ConnectionType
    {
        // ConnectionType is an enum of flags, so it is easy to use binary operators on the values
        
        none = 0b0000,
        entrance = 0b0001,
        exit = 0b0010,
        both = 0b0011,
        secret = 0b0100,
        locked = 0b1000
    }
}
