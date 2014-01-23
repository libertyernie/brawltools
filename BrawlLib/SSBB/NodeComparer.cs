using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Collections.Generic;

namespace BrawlLib.SSBB
{
    public class NodeComparer : IComparer<ResourceNode>
    {
        public static NodeComparer Instance = new NodeComparer();
        public int Compare(ResourceNode x, ResourceNode y) { return String.CompareOrdinal(x.Name, y.Name); }
    }
}
