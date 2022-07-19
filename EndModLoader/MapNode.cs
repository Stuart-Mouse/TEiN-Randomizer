using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEiNRandomizer
{
    class MapNode
    {
        // This class defines a map screen in a linked-list type representation

        // The MapScreen referenced by this MapNode
        public MapScreen Screen;
        public MapConnections Connections;

        // These are the MapNodes that area linked in each direction
        public MapNodeConnection U;
        public MapNodeConnection D;
        public MapNodeConnection L;
        public MapNodeConnection R;
    }
    struct MapNodeConnection
    {
        // This is a reference to the node at this connection
        public MapNode Node;
        // This is the number of ".." spaces between the two nodes
        public int Distance;
    }
}
