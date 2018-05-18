using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBRM)]
    class TBRMWrapper : STDTWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBRM; } }

        public override ResourceNode Duplicate()
        {
            if (_resource._parent == null)
            {
                return null;
            }
            _resource.Rebuild();
            TBRMNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length) as TBRMNode;
            _resource._parent.InsertChild(newNode, true, _resource.Index + 1);
            newNode.Populate();
            newNode.FileType = ((TBRMNode)_resource).FileType;
            newNode.FileIndex = ((TBRMNode)_resource).FileIndex;
            newNode.GroupID = ((TBRMNode)_resource).GroupID;
            newNode.RedirectIndex = ((TBRMNode)_resource).RedirectIndex;
            newNode.Name = _resource.Name;
            return newNode;
        }

        public TBRMWrapper() { }
    }
}
