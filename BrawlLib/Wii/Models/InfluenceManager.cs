using System;
using System.Collections.Generic;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Modeling;
using System.Windows.Forms;

namespace BrawlLib.Wii.Models
{
    /// <summary>
    /// Managed collection of influences. Only influences with references should be used.
    /// It is up to the implementation to properly manage this collection.
    /// </summary>
    public class InfluenceManager
    {
        internal List<Influence> _influences = new List<Influence>();
        public List<Influence> Influences { get { return _influences; } }

        public Influence FindOrCreate(Influence inf, bool increaseRefs)
        {
            //Search for influence in list. If it exists, return it.
            foreach (Influence i in _influences)
                if (i.Equals(inf))
                    return i;

            //Not found, add it to the list.
            _influences.Add(inf);

            if (increaseRefs)
                inf._refCount++;

            return inf;
        }

        public int Count { get { return _influences.Count; } }
        
        public void Remove(Influence inf)
        {
            for (int i = 0; i < Count; i++)
                if (object.ReferenceEquals(_influences[i], inf))
                {
                    if (inf._refCount-- <= 0)
                        _influences.RemoveAt(i);
                    break;
                }
        }

        //Get all weighted influences
        public Influence[] GetWeighted()
        {
            List<Influence> list = new List<Influence>(_influences.Count);
            foreach (Influence i in _influences)
                if (i.IsWeighted)
                    list.Add(i);

            return list.ToArray();
        }

        //Remove all influences without references
        public void Clean()
        {
            int i = 0;
            while (i < Count)
            {
                if (_influences[i].ReferenceCount <= 0)
                    _influences.RemoveAt(i);
                else
                    i++;
            }
        }

        //Sorts influences
        public void Sort() { _influences.Sort(Influence.Compare); }
    }

    public class Influence : IMatrixNode
    {
        public override string ToString() { return ""; }

        internal List<IMatrixNodeUser> _references = new List<IMatrixNodeUser>();
        internal int _refCount;
        internal int _index;
        internal Matrix _matrix;
        internal Matrix? _invMatrix;
        public List<BoneWeight> _weights;

        public List<BoneWeight> Weights { get { return _weights; } }
        public List<IMatrixNodeUser> Users { get { return _references; } set { _references = value; } }

        //Makes sure all weights add up to 1.0f.
        //Does not modify any locked weights.
        public void Normalize() 
        {
            float denom = 0.0f, num = 1.0f;

            foreach (BoneWeight b in Weights)
                if (b.Locked)
                    num -= b.Weight;
                else
                    denom += b.Weight;

            if (denom != 0.0f && num != 0.0f)
                foreach (BoneWeight b in Weights)
                    if (!b.Locked)
                        b.Weight = (float)Math.Round(b.Weight / denom * num, 7);
        }

        public Influence Clone()
        {
            Influence i = new Influence();
            foreach (BoneWeight b in _weights)
                i._weights.Add(new BoneWeight(b.Bone, b.Weight) { Locked = b.Locked });
            
            return i;
        }

        public int ReferenceCount { get { return _refCount; } set { _refCount = value; } }
        public int NodeIndex { get { return _index; } }

        public Matrix Matrix { get { return _matrix; } }
        public Matrix InverseMatrix 
        {
            get
            {
                if (_invMatrix == null)
                {
                    try
                    {
                        _invMatrix = Matrix.Invert(_matrix);
                    }
                    catch
                    {
                        _invMatrix = Matrix.Identity;
                    }
                }

                return (Matrix)_invMatrix;
            }
        }
        
        public bool IsPrimaryNode { get { return false; } }

        public bool IsWeighted { get { return _weights.Count > 1; } }
        public MDL0BoneNode Bone { get { return _weights[0].Bone; } }

        public Influence() { _weights = new List<BoneWeight>(); }
        public Influence(List<BoneWeight> weights) { _weights = weights; }
        public Influence(MDL0BoneNode bone) { _weights = new List<BoneWeight> { new BoneWeight(bone) }; }

        public void CalcMatrix()
        {
            if (IsWeighted)
            {
                _matrix = new Matrix();
                foreach (BoneWeight w in _weights)
                    if (w.Bone != null)
                        _matrix += (w.Bone.Matrix * w.Bone.InverseBindMatrix) * w.Weight;

                //The inverse matrix is only used for unweighting vertices so we don't need to set it now
                _invMatrix = null;
            }
            else if (_weights.Count == 1)
            {
                if (Bone != null)
                {
                    _matrix = Bone.Matrix;
                    _invMatrix = Bone.InverseMatrix;
                }
            }
            else
                _invMatrix = _matrix = Matrix.Identity;
        }
        public static int Compare(Influence i1, Influence i2)
        {
            if (i1._weights.Count < i2._weights.Count)
                return -1;
            if (i1._weights.Count > i2._weights.Count)
                return 1;

            if (i1._refCount > i2._refCount)
                return -1;
            if (i1._refCount < i2._refCount)
                return 1;

            return 0;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Influence)
                return Equals(obj as Influence);
            return false;
        }
        public bool Equals(Influence inf)
        {
            bool found;

            if (object.ReferenceEquals(this, inf))
                return true;

            if (object.ReferenceEquals(inf, null))
                return false;

            if (_weights.Count != inf._weights.Count)
                return false;

            foreach (BoneWeight w1 in _weights)
            {
                found = false;
                foreach (BoneWeight w2 in inf._weights) { if (w1 == w2) { found = true; break; } }
                if (!found)
                    return false;
            }
            return true;
        }
        public static bool operator ==(Influence i1, Influence i2) { return i1.Equals(i2); }
        public static bool operator !=(Influence i1, Influence i2) { return !i1.Equals(i2); }
    }

    public class BoneWeight
    {
        public override string ToString() { return Bone.Name + " - " + Weight * 100.0f + "%"; }

        public MDL0BoneNode Bone;
        public float Weight;

        public bool Locked { get { return Bone._locked; } set { Bone._locked = value; } }

        public BoneWeight() : this(null, 1.0f) { }
        public BoneWeight(MDL0BoneNode bone) : this(bone, 1.0f) { }
        public BoneWeight(MDL0BoneNode bone, float weight) { Bone = bone; Weight = weight; }

        public static bool operator ==(BoneWeight b1, BoneWeight b2) 
        {
            if (System.Object.ReferenceEquals(b1, b2))
                return true;

            return b1.Equals(b2);
        }
        public static bool operator !=(BoneWeight b1, BoneWeight b2) { return !(b1 == b2); }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is BoneWeight)
            {
                if ((Bone == ((BoneWeight)obj).Bone) && (Math.Abs(Weight - ((BoneWeight)obj).Weight) < Collada._importOptions._weightPrecision))
                return true;
            }
            return false;
        }
        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
