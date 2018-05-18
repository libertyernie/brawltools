using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBCL)]
    class TBCLWrapper : STDTWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBCL; } }

        public override ResourceNode Duplicate()
        {
            if (_resource._parent == null)
            {
                return null;
            }
            _resource.Rebuild();
            TBCLNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length) as TBCLNode;
            _resource._parent.InsertChild(newNode, true, _resource.Index + 1);
            newNode.Populate();
            newNode.FileType = ((TBCLNode)_resource).FileType;
            newNode.FileIndex = ((TBCLNode)_resource).FileIndex;
            newNode.GroupID = ((TBCLNode)_resource).GroupID;
            newNode.RedirectIndex = ((TBCLNode)_resource).RedirectIndex;
            newNode.Name = _resource.Name;
            return newNode;
        }

        public TBCLWrapper() {  }
    }
}
