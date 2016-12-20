using System;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using System.ComponentModel;
using BrawlLib;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.SCLA)]
    public class SCLAWrapper : GenericWrapper
    {
        public override string ExportFilter { get { return FileFilters.SCLA; } }
    }
}
