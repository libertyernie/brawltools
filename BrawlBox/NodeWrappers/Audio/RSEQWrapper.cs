using BrawlLib.SSBB.ResourceNodes;
using BrawlLib;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.RSEQ)]
    class RSEQWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.RSEQ; } }
    }
}
