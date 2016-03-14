using BrawlLib.SSBB.ResourceNodes;
using BrawlLib;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.MSBin)]
    class MSBinWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.MSBin; } }
    }
}
