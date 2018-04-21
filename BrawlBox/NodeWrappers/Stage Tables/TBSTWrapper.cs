using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBST)]
    class TBSTWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBST; } }

        public TBSTWrapper() { }
    }
}
