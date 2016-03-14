using BrawlLib.SSBB.ResourceNodes;
using BrawlLib;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.RSTM)]
    class RSTMWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.RSTM; } }
    }
}
