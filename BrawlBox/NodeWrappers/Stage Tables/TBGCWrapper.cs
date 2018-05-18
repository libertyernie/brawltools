using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBGC)]
    class TBGCWrapper : STDTWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBGC; } }

        public override ResourceNode Duplicate()
        {
            if (_resource._parent == null)
            {
                return null;
            }
            _resource.Rebuild();
            TBGCNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length) as TBGCNode;
            _resource._parent.InsertChild(newNode, true, _resource.Index + 1);
            newNode.Populate();
            newNode.FileType = ((TBGCNode)_resource).FileType;
            newNode.FileIndex = ((TBGCNode)_resource).FileIndex;
            newNode.GroupID = ((TBGCNode)_resource).GroupID;
            newNode.RedirectIndex = ((TBGCNode)_resource).RedirectIndex;
            newNode.Name = _resource.Name;
            return newNode;
        }

        public TBGCWrapper() { }
    }
}
