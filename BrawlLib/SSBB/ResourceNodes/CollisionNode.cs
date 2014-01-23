using System;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using BrawlLib.OpenGL;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class CollisionNode : ARCEntryNode, IRenderedObject
    {
        internal CollisionHeader* Header { get { return (CollisionHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.CollisionDef; } }

        public List<CollisionObject> _objects = new List<CollisionObject>();

        internal int _unk1;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _objects.Clear();
            ColObject* obj = Header->Objects;
            for (int i = Header->_numObjects; i-- > 0; )
                _objects.Add(new CollisionObject(this, obj++));

            _unk1 = Header->_unk1;

            return false;
        }

        private int _pointCount, _planeCount;
        public override int OnCalculateSize(bool force)
        {
            _pointCount = _planeCount = 0;
            foreach (CollisionObject obj in _objects)
            {
                _pointCount += obj._points.Count;
                _planeCount += obj._planes.Count;
            }

            return CollisionHeader.Size + (_pointCount * 8) + (_planeCount * ColPlane.Size) + (_objects.Count * ColObject.Size);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            CollisionHeader* header = (CollisionHeader*)address;
            *header = new CollisionHeader(_pointCount, _planeCount, _objects.Count, _unk1);

            BVec2* pPoint = header->Points;
            ColPlane* pPlane = header->Planes;
            ColObject* pObj = header->Objects;

            int iPoint = 0, iPlane = 0;
            int lind, rind, llink, rlink, tmp;
            int cPoint, cPlane;

            CollisionPlane current, next;
            CollisionLink link;
            foreach (CollisionObject obj in _objects)
            {
                //Sets bounds and entry indices
                obj.Prepare();

                cPoint = iPoint;
                cPlane = iPlane;
                foreach (CollisionPlane plane in obj._planes)
                {
                    if (plane._encodeIndex != -1)
                        continue;
                    current = next = plane;

                Top:
                    //Update variables, moving to next plane and links
                    llink = current._encodeIndex;
                    current = next;
                    next = null;
                    rlink = -1;

                    //Get left point index, and encode where necessary
                    if ((link = current._linkLeft)._encodeIndex == -1)
                        pPoint[link._encodeIndex = lind = iPoint++] = link._rawValue;
                    else
                        lind = link._encodeIndex;

                    //Get right point index and encode. 
                    if ((link = current._linkRight)._encodeIndex == -1)
                        pPoint[link._encodeIndex = rind = iPoint++] = link._rawValue;
                    else
                        rind = link._encodeIndex;

                    //Right-link planes by finding next available
                    if (link != null)
                        foreach (CollisionPlane p in link._members)
                        {
                            if ((p == current) || (p._linkLeft != link))
                                continue; //We only want to go left-to-right!

                            //Determine if entry has been encoded yet
                            if ((tmp = p._encodeIndex) != -1)
                                if (pPlane[tmp]._link1 != -1)
                                    continue; //Already linked, try again
                                else
                                    pPlane[rlink = tmp]._link1 = (short)iPlane; //Left link, which means the end!
                            else
                            {
                                next = p;
                                rlink = iPlane + 1;
                            }

                            break;
                        }

                    //Create entry
                    pPlane[current._encodeIndex = iPlane++] = new ColPlane(lind, rind, llink, rlink, current._type, current._flags2, current._flags, current._material);

                    //Traverse
                    if (next != null)
                        goto Top;
                }

                *pObj++ = new ColObject(cPlane, iPlane - cPlane, cPoint, iPoint - cPoint, obj._boxMin, obj._boxMax, obj._modelName, obj._boneName,
                    obj._unk1, obj._unk2, obj._unk3, obj._flags, obj._unk5, obj._unk6, obj._boneIndex);
            }
        }

        #region IRenderedObject Members

        public void Attach(TKContext ctx) { }
        public void Detach() { }
        public void Refesh() { }
        public void Render(TKContext ctx, ModelPanel mainWindow)
        {
            GL.Disable(EnableCap.Lighting);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            foreach (CollisionObject obj in _objects)
                obj.Render(ctx);
        }
        public void GetBox(out Vector3 min, out Vector3 max)
        {
            min = new Vector3();
            max = new Vector3();
        }
        #endregion

        internal static ResourceNode TryParse(DataSource source)
        {
            CollisionHeader* header = (CollisionHeader*)source.Address;

            if ((header->_numPoints < 0) || (header->_numPlanes < 0) || (header->_numObjects < 0) || (header->_unk1 < 0))
                return null;

            if ((header->_pointOffset != 0x28) ||
                (header->_planeOffset >= source.Length) || (header->_planeOffset <= header->_pointOffset) ||
                (header->_objectOffset >= source.Length) || (header->_objectOffset <= header->_planeOffset))
                return null;

            int* sPtr = header->_pad;
            for (int i = 0; i < 5; i++)
                if (sPtr[i] != 0)
                    return null;

            return new CollisionNode();
        }
    }

    public unsafe class CollisionObject
    {
        public MDL0BoneNode LinkedBone 
        {
            get { return _linkedBone; }
            set 
            {
                if ((_linkedBone = value) != null)
                {
                    _boneIndex = _linkedBone.BoneIndex;
                    _boneName = _linkedBone.Name;
                    _modelName = _linkedBone.Model.Name;
                    _flags[1] = false;
                }
                else
                {
                    _boneIndex = -1;
                    _boneName = "";
                    _modelName = "";
                    _flags[1] = true;
                }
            }
        }

        public MDL0BoneNode _linkedBone = null;

        public Vector2 _boxMin, _boxMax;
        public int _unk1, _unk2, _unk3, _unk5, _unk6, _boneIndex;
        public Bin16 _flags;

        public List<CollisionPlane> _planes = new List<CollisionPlane>();
        public bool _render = true;
        public string _modelName = "", _boneName = "";

        public enum Flags
        {
            None = 0,
            Unknown = 1,
            Independent = 2,
            ModuleControlled = 4,
            SSEUnknown = 8,
        }

        public List<CollisionLink> _points = new List<CollisionLink>();
        public CollisionObject() { }
        public CollisionObject(CollisionNode parent, ColObject* entry)
        {
            _modelName = entry->ModelName;
            _boneName = entry->BoneName;
            _unk1 = entry->_unk1;
            _unk2 = entry->_unk2;
            _unk3 = entry->_unk3;
            _flags = (ushort)entry->_flags;
            _unk5 = entry->_unk5;
            _unk6 = entry->_unk6;
            _boneIndex = entry->_boneIndex;

            int pointCount = entry->_pointCount;
            int pointOffset = entry->_pointOffset;
            int planeCount = entry->_planeCount;
            int planeOffset = entry->_planeIndex;

            ColPlane* pPlane = &parent.Header->Planes[planeOffset];

            //Decode points
            BVec2* pPtr = &parent.Header->Points[pointOffset];
            for (int i = 0; i < pointCount; i++)
                new CollisionLink(this, *pPtr++);

            //CollisionPlane plane;
            for (int i = 0; i < planeCount; i++)
                if (pPlane->_point1 != pPlane->_point2)
                    new CollisionPlane(this, pPlane++, pointOffset);
        }

        //Calculate bounds, and reset indices
        internal void Prepare()
        {
            Vector2 left, right;
            _boxMin = new Vector2(0);
            _boxMax = new Vector2(0);

            foreach (CollisionPlane plane in _planes)
            {
                //Normalize plane direction


                //Get bounds
                left = plane.PointLeft;
                right = plane.PointRight;

                _boxMin.Min(left);
                _boxMin.Min(right);
                _boxMax.Max(left);
                _boxMax.Max(right);

                //Reset encode indices
                plane._encodeIndex = -1;
                if (plane._linkLeft != null)
                    plane._linkLeft._encodeIndex = -1;
                if (plane._linkRight != null)
                    plane._linkRight._encodeIndex = -1;
            }
        }

        internal unsafe void Render(TKContext ctx)
        {
            if (!_render)
                return;

            //bool t = !_flags[1] && _linkedBone != null;
            //if (t)
            //{
            //    //Apply bone transform
            //    GL.PushMatrix();
            //    fixed (Matrix* m = &_linkedBone._frameMatrix)
            //        GL.MultMatrix((float*)m);
            //}

            foreach (CollisionPlane p in _planes)
                p.DrawPlanes();
            foreach (CollisionLink l in _points)
                l.Render(ctx);

            //if (t)
            //    GL.PopMatrix();
        }

        public override string ToString()
        {
            return "Collision Object";
        }
    }

    public unsafe class CollisionLink
    {
        private const float BoxRadius = 0.15f;
        private const float LineWidth = 11.0f;

        public CollisionObject _parent;
        public int _encodeIndex;
        public bool _highlight;

        public Vector2 _rawValue;
        public Vector2 Value 
        {
            get
            {
                if (_parent == null || _parent.LinkedBone == null)
                    return _rawValue;

                Vector3 point = _parent.LinkedBone.Matrix.GetPoint();
                Vector3 scale = _parent.LinkedBone.Matrix.GetScale();
                return _rawValue * new Vector2(scale._x, scale._y) + new Vector2(point._x, point._y);
            }
            set
            {
                if (_parent == null || _parent.LinkedBone == null)
                {
                    _rawValue = value;
                    return;
                }

                //Vector3 point = _parent.LinkedBone.InverseMatrix.GetPoint();
                //Vector3 scale = _parent.LinkedBone.InverseMatrix.GetScale();
                //_rawValue = value * new Vector2(scale._x, scale._y) + new Vector2(point._x, point._y);

                Vector3 point = _parent.LinkedBone.Matrix.GetPoint();
                Vector3 scale = _parent.LinkedBone.Matrix.GetScale();
                _rawValue = (value - new Vector2(point._x, point._y)) / new Vector2(scale._x, scale._y);
            }
        }
        
        public List<CollisionPlane> _members = new List<CollisionPlane>();

        public CollisionLink() { }
        public CollisionLink(CollisionObject parent, Vector2 value)
        {
            _parent = parent;
            _rawValue = value;
            _parent._points.Add(this);
        }

        public CollisionLink Clone() { return new CollisionLink(_parent, _rawValue); }

        //internal CollisionLink Splinter()
        //{
        //    if (_members.Count <= 1)
        //        return null;

        //    CollisionPlane plane = _members[1];
        //    CollisionLink link = new CollisionLink(_parent, _value);

        //    if (plane._linkLeft == this)
        //        plane.LinkLeft = link;
        //    else
        //        plane.LinkRight = link;
            

        //    return link;
        //}

        public CollisionLink[] Split()
        {
            int count = _members.Count - 1;
            CollisionLink[] links = new CollisionLink[count];
            for (int i = 0; i < count; i++)
                if (_members[0]._linkLeft == this)
                    _members[0].LinkLeft = links[i] = Clone();
                else
                    _members[0].LinkRight = links[i] = Clone();

            return links;
        }

        public bool Merge(CollisionLink link)
        {
            if (_parent != link._parent)
                return false;

            CollisionPlane plane;
            for (int i = link._members.Count; --i >= 0; )
            {
                plane = link._members[0];

                if (plane._linkLeft == link)
                {
                    if (plane._linkRight == this)
                        plane.Delete();
                    else
                        plane.LinkLeft = this;
                }
                else
                {
                    if (plane._linkLeft == this)
                        plane.Delete();
                    else
                        plane.LinkRight = this;
                }
            }

            //_value = (_value + link._value) / 2.0f;

            return true;
        }

        //Connects two points together to create a plane
        public unsafe CollisionPlane Connect(CollisionLink p)
        {
            //Don't connect if not on same object
            if ((p == this) || (this._parent != p._parent))
                return null;

            //Don't connect if plane already exists
            foreach (CollisionPlane plane in _members)
                if (p._members.Contains(plane))
                    return null;

            return new CollisionPlane(_parent, this, p);
        }
        //Create new point/plane extending to target
        public CollisionLink Branch(Vector2 point)
        {
            CollisionLink link = new CollisionLink(_parent, point);
            CollisionPlane plane = new CollisionPlane(_parent, this, link);
            return link;
        }
        //Deletes link and all planes connected
        public void Delete()
        {
            while (_members.Count != 0)
                _members[0].Delete();
        }
        //Deletes link but re-links existing planes
        public void Pop()
        {
            CollisionLink link1, link2;
            CollisionPlane plane1, plane2;
            while (_members.Count != 0)
            {
                plane1 = _members[0];
                if (_members.Count > 1)
                {
                    plane2 = _members[1];
                    link1 = plane1._linkLeft == this ? plane1._linkRight : plane1._linkLeft;
                    link2 = plane2._linkLeft == this ? plane2._linkRight : plane2._linkLeft;

                    link1.Connect(link2);
                    plane1.Delete();
                    plane2.Delete();
                }
                else
                    plane1.Delete();
            }
        }

        public void RemoveMember(CollisionPlane plane)
        {
            _members.Remove(plane);
            if (_members.Count == 0)
                _parent._points.Remove(this);
        }

        public void Render(TKContext ctx)
        {
            if (_highlight)
                GL.Color4(1.0f, 1.0f, 0.0f, 1.0f);
            else
                GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);

            Vector2 v = Value;

            ctx.DrawBox(
                new Vector3(v._x - BoxRadius, v._y - BoxRadius, LineWidth),
                new Vector3(v._x + BoxRadius, v._y + BoxRadius, -LineWidth));
        }
    }

    public unsafe class CollisionPlane
    {
        public int _encodeIndex;

        public CollisionLink _linkLeft, _linkRight;

        public CollisionPlaneMaterial _material;
        public CollisionPlaneFlags _flags;
        public CollisionPlaneType _type;
        public CollisionPlaneFlags2 _flags2;

        public bool _render = true;

        public CollisionObject _parent;

        public CollisionPlaneType Type
        {
            get { return _type; }
            set
            {
                switch (_type = value)
                {
                    case CollisionPlaneType.Floor:
                        if (PointLeft._x > PointRight._x)
                            SwapLinks();
                        break;

                    case CollisionPlaneType.Ceiling:
                        if (PointLeft._x < PointRight._x)
                            SwapLinks();
                        break;

                    case CollisionPlaneType.RightWall:
                        if (PointLeft._y < PointRight._y)
                            SwapLinks();
                        break;

                    case CollisionPlaneType.LeftWall:
                        if (PointLeft._y > PointRight._y)
                            SwapLinks();
                        break;
                }
            }
        }

        public bool IsType1
        {
            get { return (_flags2 & CollisionPlaneFlags2.Unk1) != 0; }
            set { _flags2 = (_flags2 & ~CollisionPlaneFlags2.Unk1) | (value ? CollisionPlaneFlags2.Unk1 : 0); }
        }
        public bool IsType2
        {
            get { return (_flags2 & CollisionPlaneFlags2.Unk2) != 0; }
            set { _flags2 = (_flags2 & ~CollisionPlaneFlags2.Unk2) | (value ? CollisionPlaneFlags2.Unk2 : 0); }
        }
        public bool IsFallThrough
        {
            get { return (_flags & CollisionPlaneFlags.DropThrough) != 0; }
            set { _flags = (_flags & ~CollisionPlaneFlags.DropThrough) | (value ? CollisionPlaneFlags.DropThrough : 0); }
        }
        public bool IsRightLedge
        {
            get { return (_flags & CollisionPlaneFlags.RightLedge) != 0; }
            set { _flags = (_flags & ~CollisionPlaneFlags.RightLedge) | (value ? CollisionPlaneFlags.RightLedge : 0); }
        }
        public bool IsLeftLedge
        {
            get { return (_flags & CollisionPlaneFlags.LeftLedge) != 0; }
            set { _flags = (_flags & ~CollisionPlaneFlags.LeftLedge) | (value ? CollisionPlaneFlags.LeftLedge : 0); }
        }
        public bool IsNoWalljump
        {
            get { return (_flags & CollisionPlaneFlags.NoWalljump) != 0; }
            set { _flags = (_flags & ~CollisionPlaneFlags.NoWalljump) | (value ? CollisionPlaneFlags.NoWalljump : 0); }
        }

        public Vector2 PointLeft
        {
            get { return _linkLeft.Value; }
            //set { _linkLeft.Value = value; }
        }
        public Vector2 PointRight
        {
            get { return _linkRight.Value; }
            //set { _linkRight.Value = value; }
        }
        public CollisionLink LinkLeft
        {
            get { return _linkLeft; }
            set
            {
                if (_linkLeft != null)
                    _linkLeft.RemoveMember(this);

                if ((_linkLeft = value) != null)
                    if (_linkLeft != _linkRight)
                        _linkLeft._members.Add(this);
                    else
                        _linkLeft = null;
            }
        }
        public CollisionLink LinkRight
        {
            get { return _linkRight; }
            set
            {
                if (_linkRight != null)
                    _linkRight.RemoveMember(this);

                if ((_linkRight = value) != null)
                    if (_linkRight != _linkLeft)
                        _linkRight._members.Add(this);
                    else
                        _linkRight = null;
            }
        }

        public CollisionPlane(CollisionObject parent, CollisionLink left, CollisionLink right)
        {
            _parent = parent;
            _parent._planes.Add(this);

            LinkLeft = left;
            LinkRight = right;
        }
        public CollisionPlane(CollisionObject parent, ColPlane* entry, int offset)
            : this(parent, parent._points[entry->_point1 - offset], parent._points[entry->_point2 - offset])
        {
            _material = entry->_material;
            _flags = entry->_flags;
            _type = entry->Type;
            _flags2 = entry->Flags2;
        }

        public CollisionLink Split(Vector2 point)
        {
            CollisionLink link = new CollisionLink(_parent, point);
            CollisionPlane plane = new CollisionPlane(_parent, link, _linkRight);
            LinkRight = link;
            return link;
        }

        private void SwapLinks()
        {
            CollisionLink l = _linkLeft; 
            _linkLeft = _linkRight; 
            _linkRight = l;
        }

        public void Delete()
        {
            LinkLeft = null;
            LinkRight = null;
            _parent._planes.Remove(this);
        }

        internal unsafe void DrawPlanes()
        {
            if (!_render)
                return;

            Vector2 l = _linkLeft.Value;
            Vector2 r = _linkRight.Value;

            int lev = 0;
            if (_linkLeft._highlight)
                lev++;
            if (_linkRight._highlight)
                lev++;

            if (lev == 0)
                GL.Color4(0.0f, 0.9f, 0.9f, 0.8f);
            else if (lev == 1)
                GL.Color4(1.0f, 0.5f, 0.5f, 0.8f);
            else
                GL.Color4(0.9f, 0.0f, 0.9f, 0.8f);

            GL.Begin(BeginMode.Quads);

            GL.Vertex3(l._x, l._y, 10.0f);
            GL.Vertex3(l._x, l._y, -10.0f);
            GL.Vertex3(r._x, r._y, -10.0f);
            GL.Vertex3(r._x, r._y, 10.0f);

            GL.End();

            if (lev == 0)
                GL.Color4(0.0f, 0.6f, 0.6f, 0.8f);
            else if (lev == 1)
                GL.Color4(0.7f, 0.2f, 0.2f, 0.8f);
            else
                GL.Color4(0.6f, 0.0f, 0.6f, 0.8f);

            GL.Begin(BeginMode.Lines);

            GL.Vertex3(l._x, l._y, 10.0f);
            GL.Vertex3(r._x, r._y, 10.0f);
            GL.Vertex3(l._x, l._y, -10.0f);
            GL.Vertex3(r._x, r._y, -10.0f);

            GL.End();
        }
    }
}
