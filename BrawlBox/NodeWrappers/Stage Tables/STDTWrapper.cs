using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.STDT)]
    class STDTWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.STDT; } }
        
        public STDTWrapper() { }
    }
}
