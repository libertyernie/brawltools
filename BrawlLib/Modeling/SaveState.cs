using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;

namespace BrawlLib.Modeling
{
    public abstract class SaveState
    {
        public bool _isUndo = true;
    }

    public class CollisionState : SaveState
    {
        public List<CollisionLink> _collisionLinks;
        public List<Vector2> _linkVectors;
        public bool _split;
        public bool _merge;
        public bool _create;
        public bool _delete;

        public CollisionNode _collisionNode;
        public CollisionObject _collisionObject;
        public CollisionPlane _collisionPlane;
    }

    public class VertexState : SaveState
    {
        public List<Vertex3> _vertices = null;
        public FrameState _transform;
        public Vector3 _origin;
        public IModel _targetModel;
    }

    public class BoneState : SaveState
    {
        public int _frameIndex = 0;
        public CHR0Node _animation;
        public IBoneNode _bone;
        public FrameState? _frameState = null;
        public IModel _targetModel;
    }
}
