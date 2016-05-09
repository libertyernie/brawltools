using BrawlLib.SSBB.ResourceNodes;
using BrawlLib;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.RSAR)]
    class RSARWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.RSAR; } }
    }
}
