using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using BrawlLib.Wii.Models;
using BrawlLib.OpenGL;
using System.Drawing;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Imaging;

namespace BrawlLib.Modeling
{
    public class Vertex3 : IMatrixNodeUser
    {
        public Vector3 _position;
        public Vector3 _weightedPosition;
        private IMatrixNode _matrixNode;
        
        //normals, colors and uvs aren't stored in this class
        //because this stores a unique weighted point in space.
        //Multiple vertices may have different normals etc but the same weighted position

        [Browsable(false)]
        public IMatrixNodeUser Parent { get { return _parent; } set { _parent = value; } }
        private IMatrixNodeUser _parent;

        public List<int> _faceDataIndices = new List<int>();

        //Contains all the facepoints with the same position and influence.
        //Note that the normal, etc indices may differ per facepoint
        public List<Facepoint> _facepoints = new List<Facepoint>();
        public Facepoint[] Facepoints { get { return _facepoints.ToArray(); } }

        public IMatrixNode GetMatrixNode()
        {
            if (_parent != null && _parent.MatrixNode != null)
                return _parent.MatrixNode;
            else if (MatrixNode != null)
                return MatrixNode;
            return null;
        }
        public Matrix GetMatrix()
        {
            IMatrixNode node = GetMatrixNode();
            if (node != null)
                return node.Matrix;
            return Matrix.Identity;
        }
        public Matrix GetInvMatrix()
        {
            IMatrixNode node = GetMatrixNode();
            if (node != null)
                return node.InverseMatrix;
            return Matrix.Identity;
        }

        public List<BoneWeight> GetBoneWeights() { return MatrixNode == null ? _parent.MatrixNode.Weights : MatrixNode.Weights; }
        public IBoneNode[] GetBones()
        {
            List<BoneWeight> b = GetBoneWeights();
            return b == null ? null : b.Select(x => x.Bone).ToArray();
        }
        public float[] GetWeightValues()
        {
            List<BoneWeight> b = GetBoneWeights();
            return b == null ? null : b.Select(x => x.Weight).ToArray();
        }
        public int IndexOfBone(IBoneNode b) { return Array.IndexOf(GetBones(), b); }
        public BoneWeight WeightForBone(IBoneNode b) { int i = IndexOfBone(b); if (i == -1) return null; return GetBoneWeights()[i]; }

        [Browsable(true)]
        public string Influence
        {
            get { return MatrixNode == null ? "(none)" : MatrixNode.IsPrimaryNode ? ((ResourceNode)MatrixNode).Name : "(multiple)"; }
        }

        public void ChangeInfluence(IMatrixNode newMatrixNode)
        {
            if (_parent == null)
                return;

            IMatrixNode oldMatrixNode = GetMatrixNode();
            if (newMatrixNode is IBoneNode && (oldMatrixNode is Influence || oldMatrixNode == null))
            {
                //Move to local
                _position *= ((IBoneNode)newMatrixNode).InverseMatrix;

                SetPosition(((MDL0ObjectNode)_parent)._vertexNode, _position);
            }
            else if ((newMatrixNode is Influence || newMatrixNode == null) && oldMatrixNode is IBoneNode)
            {
                //Move to world
                _position *= ((IBoneNode)oldMatrixNode).Matrix;

                SetPosition(((MDL0ObjectNode)_parent)._vertexNode, _position);
            }
            else if (newMatrixNode is IBoneNode && oldMatrixNode is IBoneNode)
            {
                //Update local position
                _position *= ((IBoneNode)oldMatrixNode).Matrix;
                _position *= ((IBoneNode)newMatrixNode).InverseMatrix;

                SetPosition(((MDL0ObjectNode)_parent)._vertexNode, _position);
            }

            //If both are null or an influence, they're already in world position
        }

        [Browsable(false)]
        public IMatrixNode MatrixNode
        {
            get { return _matrixNode; }
            set
            {
                if (_matrixNode == value)
                    return;

                ChangeInfluence(value);

                if (_matrixNode != null && _matrixNode.Users.Contains(this))
                    _matrixNode.Users.Remove(this);

                if ((_matrixNode = value) != null && !_matrixNode.Users.Contains(this))
                    _matrixNode.Users.Add(this);
            }
        }
        [Browsable(true), Category("Vertex"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 WeightedPosition
        {
            get { return _weightedPosition; }
            set { _weightedPosition = value; Unweight(); }
        }
        [Browsable(false), Category("Vertex"), TypeConverter(typeof(Vector3StringConverter))]
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vertex3() { }
        public Vertex3(Vector3 position)
        {
            Position = position;
        }
        public Vertex3(Vector3 position, IMatrixNode influence)
        {
            Position = position;
            MatrixNode = influence;
        }

        //Pre-multiply vertex using influence.
        //Influences must have already been calculated.
        public void Weight()
        {
            _weightedPosition = GetMatrix() * Position;

            _weights = null;
            _nodes = null;
        }

        public void Unweight()
        {
            if (_weights == null || _nodes == null)
                _position = GetInvMatrix() * WeightedPosition;
            else
            {
                Vector3 trans = _weightedPosition - _bCenter;
                for (int i = 0; i < _nodes.Length; i++)
                {
                    MDL0VertexNode set = _nodes[i];
                    SetPosition(set, GetInvMatrix() * ((GetMatrix() * set.Vertices[_facepoints[0]._vertexIndex]) + trans));
                }

                _position = GetInvMatrix() * ((GetMatrix() * ((MDL0ObjectNode)_parent)._vertexNode.Vertices[_facepoints[0]._vertexIndex]) + trans);
            }

            SetPosition(((MDL0ObjectNode)_parent)._vertexNode, _position);
        }

        internal float _baseWeight = 0;
        internal float[] _weights = null;
        internal MDL0VertexNode[] _nodes = null;
        internal Vector3 _bCenter = new Vector3();

        public void SetPosition(MDL0VertexNode node, Vector3 pos)
        {
            node.Vertices[_facepoints[0]._vertexIndex] = pos;
            node.ForceRebuild = true;
            if (node.Format == WiiVertexComponentType.Float)
                node.ForceFloat = true;
        }
        public void SetPosition(MDL0VertexNode node)
        {
            node.Vertices[_facepoints[0]._vertexIndex] = Position;
            node.ForceRebuild = true;
            if (node.Format == WiiVertexComponentType.Float)
                node.ForceFloat = true;
        }
        public unsafe void SetNormal(MDL0ObjectNode o)
        {
            MDL0NormalNode node = o._normalNode;
            if (node == null)
                return;

            if (o._manager._faceData[1] != null)
            {
                Vector3* pData = (Vector3*)o._manager._faceData[1].Address;
                int x = 0;
                foreach (int i in _faceDataIndices)
                {
                    if (x >= _facepoints.Count)
                        continue;
                    
                    int n = _facepoints[x++]._normalIndex;
                    if (n < 0 || n >= node.Normals.Length)
                        node.Normals[n] = pData[i];
                }
            }

            node.ForceRebuild = true;
            if (node.Format == WiiVertexComponentType.Float)
                node.ForceFloat = true;
        }

        //Call only after weighting
        public void Morph(Vector3 dest, float percent) { _weightedPosition.Lerp(dest, percent); }

        public Color GetWeightColor(IBoneNode targetBone)
        {
            float weight = -1.0f;
            if (_matrixNode == null || targetBone == null) 
                return Color.Transparent;
            if (_matrixNode is MDL0BoneNode)
                if (_matrixNode == targetBone)
                    weight = 1.0f;
                else
                    return Color.Transparent;
            else
                foreach (BoneWeight b in ((Influence)_matrixNode).Weights)
                    if (b.Bone == targetBone)
                    {
                        weight = b.Weight;
                        break;
                    }
            if (weight < 0.0f || weight > 1.0f)
                return Color.Transparent;
            int r = ((int)(weight * 255.0f)).Clamp(0, 0xFF);
            return Color.FromArgb(r, 0, 0xFF - r);
        }

        public bool Equals(Vertex3 v)
        {
            if (object.ReferenceEquals(this, v))
                return true;

            return (Position == v.Position) && (_matrixNode == v._matrixNode);
        }

        public Color _highlightColor = Color.Transparent;
        public bool _selected = false;
        [Browsable(false)]
        public bool Selected
        { 
            get { return _selected; } 
            set { _highlightColor = (_selected = value) ? Color.Orange : Color.Transparent; }
        }
    }
}