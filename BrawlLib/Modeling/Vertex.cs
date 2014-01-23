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
        public IMatrixNode _matrixNode;
        public MDL0ObjectNode _object;

        public int _index = 0;
        public List<int> _faceDataIndices = new List<int>();

        //Contains all the facepoints with the same position and influence.
        //Note that the normal, uv and color indices may differ per facepoint
        public List<Facepoint> _facepoints = new List<Facepoint>();
        public Facepoint[] Facepoints { get { return _facepoints.ToArray(); } }

        public Matrix GetMatrix()
        {
            if (_object != null && _object.MatrixNode != null)
                return _object.MatrixNode.Matrix;
            else if (MatrixNode != null)
                return MatrixNode.Matrix;

            return Matrix.Identity;
        }
        public Matrix GetInvMatrix()
        {
            if (_object != null && _object.MatrixNode != null)
                return _object.MatrixNode.InverseMatrix;
            else if (MatrixNode != null)
                return MatrixNode.InverseMatrix;

            return Matrix.Identity;
        }

        public Vector3 UnweightPos(Vector3 pos) { return GetInvMatrix() * pos; }
        public Vector3 WeightPos(Vector3 pos) { return GetMatrix() * pos; }

        public List<BoneWeight> GetBoneWeights() { return MatrixNode == null ? _object.MatrixNode.Weights : MatrixNode.Weights; }
        public MDL0BoneNode[] GetBones()
        {
            List<BoneWeight> b = GetBoneWeights();
            return b == null ? null : b.Select(x => x.Bone).ToArray();
        }
        public float[] GetWeightValues()
        {
            List<BoneWeight> b = GetBoneWeights();
            return b == null ? null : b.Select(x => x.Weight).ToArray();
        }
        public int IndexOfBone(MDL0BoneNode b) { return Array.IndexOf(GetBones(), b); }
        public BoneWeight WeightForBone(MDL0BoneNode b) { int i = IndexOfBone(b); if (i == -1) return null; return GetBoneWeights()[i]; }

        [Browsable(true)]
        public string Influence
        {
            get { return MatrixNode == null ? "(none)" : MatrixNode.IsPrimaryNode ? ((MDL0BoneNode)MatrixNode).Name : "(multiple)"; }
        }

        [Browsable(false)]
        public IMatrixNode MatrixNode
        {
            get { return _matrixNode; }
            set
            {
                if (_matrixNode == value)
                    return;

                if (value is MDL0BoneNode && _matrixNode is Influence)
                {
                    _position *= ((MDL0BoneNode)value).InverseMatrix;
                    SetPosition(_object._vertexNode, _position);
                }
                else if (value is Influence && _matrixNode is MDL0BoneNode)
                {
                    _position *= ((MDL0BoneNode)_matrixNode).Matrix;
                    SetPosition(_object._vertexNode, _position);
                }

                if (_matrixNode != null)
                {
                    _matrixNode.ReferenceCount--;
                    _matrixNode.Users.Remove(this);
                }

                if ((_matrixNode = value) != null)
                {
                    _matrixNode.ReferenceCount++;
                    _matrixNode.Users.Add(this);

                    //if (_object != null)
                    //    _object.SignalPropertyChange();
                }
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

            weights = null;
            nodes = null;
        }

        public void Unweight()
        {
            if (weights == null || nodes == null)
                _position = GetInvMatrix() * WeightedPosition;
            else
            {
                Vector3 trans = _weightedPosition - bCenter; 
                for (int i = 0; i < nodes.Length; i++)
                {
                    MDL0VertexNode set = nodes[i];
                    SetPosition(set, GetInvMatrix() * ((GetMatrix() * set.Vertices[_facepoints[0]._vertexIndex]) + trans));
                }

                _position = GetInvMatrix() * ((GetMatrix() * _object._vertexNode.Vertices[_facepoints[0]._vertexIndex]) + trans);
            }

            SetPosition(_object._vertexNode, _position);
        }

        internal float baseWeight = 0;
        internal float[] weights = null;
        internal MDL0VertexNode[] nodes = null;
        internal Vector3 bCenter = new Vector3();

        public void SetPosition(MDL0VertexNode node, Vector3 pos)
        {
            node.Vertices[_facepoints[0]._vertexIndex] = pos;
            node.ForceRebuild = true;
            if (node.Format == WiiVertexComponentType.Float)
                node.ForceFloat = true;
        }

        //Call only after weighting
        public void Morph(Vector3 dest, float percent) { _weightedPosition.Morph(dest, percent); }

        public Color GetWeightColor(MDL0BoneNode targetBone)
        {
            float weight = -1;
            if (_matrixNode == null || targetBone == null) 
                return Color.Transparent;
            if (_matrixNode is MDL0BoneNode)
                if (_matrixNode == targetBone)
                    weight = 1.0f;
                else
                    return Color.Transparent;
            else 
                foreach (BoneWeight b in ((Influence)_matrixNode)._weights)
                    if (b.Bone == targetBone)
                    {
                        weight = b.Weight;
                        break;
                    }
            if (weight == -1)
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
    }
}