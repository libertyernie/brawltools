using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.RWSD)]
    class RWSDWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.RWSD; } }
    }
}
