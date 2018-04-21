using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBGM)]
    class TBGMWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBGM; } }

        public TBGMWrapper() { }
    }
}
