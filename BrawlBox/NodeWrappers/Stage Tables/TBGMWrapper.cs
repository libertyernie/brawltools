using BrawlLib;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlBox.NodeWrappers
{
    [NodeWrapper(ResourceType.TBGM)]
    class TBGMWrapper : STDTWrapper
    {
        public override string ExportFilter { get { return FileFilters.TBGM; } }

        public override ResourceNode Duplicate()
        {
            if (_resource._parent == null)
            {
                return null;
            }
            _resource.Rebuild();
            TBGMNode newNode = NodeFactory.FromAddress(null, _resource.WorkingUncompressed.Address, _resource.WorkingUncompressed.Length) as TBGMNode;
            _resource._parent.InsertChild(newNode, true, _resource.Index + 1);
            newNode.Populate();
            newNode.FileType = ((TBGMNode)_resource).FileType;
            newNode.FileIndex = ((TBGMNode)_resource).FileIndex;
            newNode.GroupID = ((TBGMNode)_resource).GroupID;
            newNode.RedirectIndex = ((TBGMNode)_resource).RedirectIndex;
            newNode.Name = _resource.Name;
            return newNode;
        }

        public TBGMWrapper() { }
    }
}
