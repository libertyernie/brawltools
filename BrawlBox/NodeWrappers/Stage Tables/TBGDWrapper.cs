using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBGD)]
    class TBGDWrapper : STDTWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBGD; } }

        public override ResourceNode Duplicate()
        {
            if (_resource._parent == null)
            {
                return null;
            }
            _resource.Rebuild();
            TBGDNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length) as TBGDNode;
            _resource._parent.InsertChild(newNode, true, _resource.Index + 1);
            newNode.Populate();
            newNode.FileType = ((TBGDNode)_resource).FileType;
            newNode.FileIndex = ((TBGDNode)_resource).FileIndex;
            newNode.GroupID = ((TBGDNode)_resource).GroupID;
            newNode.RedirectIndex = ((TBGDNode)_resource).RedirectIndex;
            newNode.Name = _resource.Name;
            return newNode;
        }

        public TBGDWrapper() { }
    }
}
