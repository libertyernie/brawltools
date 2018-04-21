using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBGC)]
    class TBGCWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBGC; } }

        public TBGCWrapper() { }
    }
}
