using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBCL)]
    class TBCLWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBCL; } }
        
        public TBCLWrapper() { }
    }
}
