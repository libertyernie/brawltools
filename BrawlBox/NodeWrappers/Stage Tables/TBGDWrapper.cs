using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBGD)]
    class TBGDWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBGD; } }

        public TBGDWrapper() { }
    }
}
