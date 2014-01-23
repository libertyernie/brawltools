using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.Modeling
{
    public class SaveState
    {
        public string _action;

        public int _frameIndex = 0;
        public CHR0Node _animation;
        public MDL0BoneNode _bone;
        public FrameState? _frameState = null;

        public List<CollisionLink> _collisionLinks;
        public List<Vector2> _linkVectors;
        public bool _split;
        public bool _merge;
        public bool _create;
        public bool _delete;

        public CollisionNode _collisionNode;
        public CollisionObject _collisionObject;
        public CollisionPlane _collisionPlane;
        
        public List<Vertex3> _vertices = null;
        public Vector3 _translation;
        public MDL0Node _targetModel;
    }
}
