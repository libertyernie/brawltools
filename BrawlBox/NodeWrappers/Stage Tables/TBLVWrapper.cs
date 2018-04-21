using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBLV)]
    class TBLVWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBLV; } }

        public TBLVWrapper() { }
    }
}
