using BrawlLib.Modeling;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : ModelEditorBase
    {
        public class CollisionState : SaveState
        {
            public CollisionNode _collisionNode;
            public CollisionObject _collisionObject;
            public CollisionPlane _collisionPlane;
            public List<CollisionLink> _collisionLinks;
            public List<Vector2> _linkVectors;
            public MDL0BoneNode _bone;
            public enum Type
            {
                Split,
                Merge,
                Create,
                Delete,
                Move,
                Select,
                BoneChange,
                ParamChange
            }
            public Type _changeType;
        }
        public void LinkMoveChange()
        {
            AddState(new CollisionState()
            {
                _changeType = CollisionState.Type.Move,
                _collisionNode = TargetCollisionNode,
                _collisionObject = TargetCollisionObject,
                _collisionLinks = _selectedLinks,
                _linkVectors = _selectedLinks.Select(x => x._rawValue).ToList(),
            });
        }
        public void LinkMergeChange()
        {

        }
        public void LinkSplitChange()
        {

        }
        public void LinkBoneChange()
        {
            AddState(new CollisionState()
            {
                _changeType = CollisionState.Type.BoneChange,
                _collisionNode = TargetCollisionNode,
                _collisionObject = TargetCollisionObject,
                _collisionLinks = _selectedLinks,
                _linkVectors = _selectedLinks.Select(x => x._rawValue).ToList(),
            });
        }
        private void ApplyCollisionState(CollisionState state)
        {
            switch (state._changeType)
            {
                case CollisionState.Type.Move:
                    TargetCollisionNode = state._collisionNode;
                    TargetCollisionObject = state._collisionObject;
                    _selectedLinks = state._collisionLinks;
                    for (int i = 0; i < _selectedLinks.Count; ++i)
                        _selectedLinks[i]._rawValue = state._linkVectors[i];
                    break;
            }
            ModelPanel.Invalidate();
        }
        protected override void ApplySaveState(SaveState state)
        {
            if (state is CollisionState)
                ApplyCollisionState((CollisionState)state);
            else
                base.ApplySaveState(state);
        }
    }
}
