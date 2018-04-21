using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBRM)]
    class TBRMWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBRM; } }

        public TBRMWrapper() { }
    }
}
