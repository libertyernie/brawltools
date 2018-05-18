using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBLV)]
    class TBLVWrapper : STDTWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBLV; } }

        public override ResourceNode Duplicate()
        {
            if (_resource._parent == null)
            {
                return null;
            }
            _resource.Rebuild();
            TBLVNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length) as TBLVNode;
            _resource._parent.InsertChild(newNode, true, _resource.Index + 1);
            newNode.Populate();
            newNode.FileType = ((TBLVNode)_resource).FileType;
            newNode.FileIndex = ((TBLVNode)_resource).FileIndex;
            newNode.GroupID = ((TBLVNode)_resource).GroupID;
            newNode.RedirectIndex = ((TBLVNode)_resource).RedirectIndex;
            newNode.Name = _resource.Name;
            return newNode;
        }

        public TBLVWrapper() { }
    }
}
