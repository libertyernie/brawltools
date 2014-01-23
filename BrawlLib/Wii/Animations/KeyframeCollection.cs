using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using BrawlLib.SSBBTypes;
using BrawlLib.Modeling;

namespace BrawlLib.Wii.Animations
{
    public enum KeyFrameMode
    {
        ScaleX = 0x10,
        ScaleY = 0x11,
        ScaleZ = 0x12,
        RotX = 0x13,
        RotY = 0x14,
        RotZ = 0x15,
        TransX = 0x16,
        TransY = 0x17,
        TransZ = 0x18,
        ScaleXYZ = 0x30,
        RotXYZ = 0x33,
        TransXYZ = 0x36,
        All = 0x90
    }

    public interface IKeyframeHolder
    {
        KeyframeEntry GetKeyframe(KeyFrameMode mode, int index);
        KeyframeEntry SetKeyframe(KeyFrameMode mode, int index, float value);
        void SetKeyframe(int index, AnimationFrame frame);
        void SetKeyframeOnlyTrans(int index, AnimationFrame frame);
        void SetKeyframeOnlyRot(int index, AnimationFrame frame);
        void SetKeyframeOnlyScale(int index, AnimationFrame frame);
        void SetKeyframeOnlyTrans(int index, Vector3 trans);
        void SetKeyframeOnlyRot(int index, Vector3 rot);
        void SetKeyframeOnlyScale(int index, Vector3 scale);
        void RemoveKeyframe(KeyFrameMode mode, int index);
        void RemoveKeyframe(int index);
        void RemoveKeyframeOnlyTrans(int index);
        void RemoveKeyframeOnlyRot(int index);
        void RemoveKeyframeOnlyScale(int index);
        AnimationFrame GetAnimFrame(int index);
        int FrameCount { get; }
        KeyframeCollection Keyframes { get; }
    }

    public interface IKeyframeArrayHolder
    {
        KeyframeEntry GetKeyframe(int index);
        void SetKeyframe(int index, float value);
        void RemoveKeyframe(int index);
        int FrameCount { get; }
        KeyframeArray Keyframes { get; }
    }

    public interface KeyframeArrayGroup
    {
        KeyframeEntry[] KeyRoots { get; set; }
        int[] KeyCounts { get; set; }
        int FrameLimit { get; set; }
        bool LinearRotation { get; set; }

    }

    public unsafe class KeyframeCollection
    {
        public KeyframeEntry[] _keyRoots = new KeyframeEntry[9]{
        //Scale
        new KeyframeEntry(-1, 1.0f),
        new KeyframeEntry(-1, 1.0f),
        new KeyframeEntry(-1, 1.0f),
        //Rotation
        new KeyframeEntry(-1, 0.0f),
        new KeyframeEntry(-1, 0.0f),
        new KeyframeEntry(-1, 0.0f),
        //Translation
        new KeyframeEntry(-1, 0.0f),
        new KeyframeEntry(-1, 0.0f),
        new KeyframeEntry(-1, 0.0f)};

        public int[] _keyCounts = new int[9];

        internal AnimationCode _evalCode;
        internal SRT0Code _texEvalCode;

        public int this[KeyFrameMode mode] { get { return _keyCounts[(int)mode - 0x10]; } }

        internal int _frameCount;
        public int FrameCount
        {
            get { return _frameCount; }
            set
            {
                _frameCount = value;
                for (int i = 0; i < 9; i++)
                {
                    KeyframeEntry root = _keyRoots[i];
                    while (root._prev._index >= value)
                    {
                        root._prev.Remove();
                        _keyCounts[i]--;
                    }
                }
            }
        }
        
        internal bool _linearRot;
        public bool LinearRotation { get { return _linearRot; } set { _linearRot = value; } }

        internal bool _loop;
        public bool Loop { get { return _loop; } set { _loop = value; } }

        public float this[KeyFrameMode mode, int index]
        {
            get { return GetFrameValue(mode, index); }
            set { SetFrameValue(mode, index, value); }
        }

        public KeyframeCollection(int limit) { _frameCount = limit; }

        private const float _cleanDistance = 0.00001f;
        public int Clean()
        {
            int flag, res, removed = 0;
            KeyframeEntry entry, root;
            for (int i = 0; i < 9; i++)
            {
                root = _keyRoots[i];

                //Eliminate redundant values
                for (entry = root._next._next; entry != root; entry = entry._next)
                {
                    flag = res = 0;

                    if (entry._prev == root)
                    {
                        if (entry._next != root)
                            flag = 1;
                    }
                    else if (entry._next == root)
                        flag = 2;
                    else
                        flag = 3;

                    if((flag & 1) != 0)
                        res |= (Math.Abs(entry._next._value - entry._value) <= _cleanDistance) ? 1 : 0;
                    
                    if((flag & 2) != 0)
                        res |= (Math.Abs(entry._prev._value - entry._value) <= _cleanDistance) ? 2 : 0;

                    if ((flag == res) && (res != 0))
                    {
                        entry = entry._prev;
                        entry._next.Remove();
                        _keyCounts[i]--;
                        removed++;
                    }
                }
            }
            return removed;
        }

        public KeyframeEntry GetKeyframe(KeyFrameMode mode, int index)
        {
            KeyframeEntry entry, root = _keyRoots[(int)mode & 0xF];
            for (entry = root._next; (entry != root) && (entry._index < index); entry = entry._next) ;
            if (entry._index == index)
                return entry;
            return null;
        }
        public float GetFrameValue(KeyFrameMode mode, int index)
        {
            return GetFrameValue(mode, index, false, false);
        }
        public float GetFrameValue(KeyFrameMode mode, int index, bool linear, bool loop)
        {
            KeyframeEntry entry, root = _keyRoots[(int)mode & 0xF];

            if (index >= root._prev._index)
                //if (!loop || root._prev == root._next)
                    return root._prev._value;
                //else
                //    return root._prev.Interpolate2(_frameCount - index + root._next._index, _linearRot || linear, _frameCount);
            if (index <= root._next._index)
                //if (!loop || root._prev == root._next)
                    return root._next._value;
                //else
                //    return root._prev.Interpolate2(_frameCount - root._prev._index + index, _linearRot || linear, _frameCount);

            for (entry = root._next;
                (entry != root) &&
                (entry._index < index); 
                entry = entry._next)
                if (entry._index == index)
                    return entry._value;
            
            return entry._prev.Interpolate(index - entry._prev._index, _linearRot || linear);
        }
        public KeyframeEntry SetFrameValue(KeyFrameMode mode, int index, float value)
        {
            KeyframeEntry entry = null, root;
            for (int x = (int)mode & 0xF, y = x + ((int)mode >> 4); x < y; x++)
            {
                root = _keyRoots[x];

                if ((root._prev == root) || (root._prev._index < index))
                    entry = root;
                else
                    for (entry = root._next; (entry != root) && (entry._index <= index); entry = entry._next) ;

                entry = entry._prev;
                if (entry._index != index)
                {
                    _keyCounts[x]++;
                    entry.InsertAfter(entry = new KeyframeEntry(index, value));
                }
                else
                    entry._value = value;
            }
            return entry;
        }

        public AnimationFrame GetFullFrame(int index)
        {
            return GetFullFrame(index, false);
        }
        public AnimationFrame GetFullFrame(int index, bool linear)
        {
            AnimationFrame frame = new AnimationFrame() { Index = index };
            float* dPtr = (float*)&frame;
            for (int x = 0x10; x < 0x19; x++)
            {
                frame.SetBool(x - 0x10, GetKeyframe((KeyFrameMode)x, index) != null);
                *dPtr++ = GetFrameValue((KeyFrameMode)x, index, linear, false);
            }
            return frame;
        }

        public KeyframeEntry Remove(KeyFrameMode mode, int index)
        {
            KeyframeEntry entry = null, root;
            for (int x = (int)mode & 0xF, y = x + ((int)mode >> 4); x < y; x++)
            {
                root = _keyRoots[x];

                for (entry = root._next; (entry != root) && (entry._index < index); entry = entry._next) ;

                if (entry._index == index)
                {
                    entry.Remove();
                    _keyCounts[x]--;
                }
                else
                    entry = null;
            }
            return entry;
        }

        public void Insert(KeyFrameMode mode, int index)
        {
            KeyframeEntry entry = null, root;
            for (int x = (int)mode & 0xF, y = x + ((int)mode >> 4); x < y; x++)
            {
                root = _keyRoots[x];
                for (entry = root._prev; (entry != root) && (entry._index >= index); entry = entry._prev)
                    if (++entry._index >= _frameCount)
                    {
                        entry = entry._next;
                        entry._prev.Remove();
                        _keyCounts[x]--;
                    }
            }
        }

        public void Delete(KeyFrameMode mode, int index)
        {
            KeyframeEntry entry = null, root;
            for (int x = (int)mode & 0xF, y = x + ((int)mode >> 4); x < y; x++)
            {
                root = _keyRoots[x];
                for (entry = root._prev; (entry != root) && (entry._index >= index); entry = entry._prev)
                    if ((entry._index == index) || (--entry._index < 0))
                    {
                        entry = entry._next;
                        entry._prev.Remove();
                        _keyCounts[x]--;
                    }
            }
        }
    }

    public class KeyframeEntry
    {
        public int _index;
        public KeyframeEntry _prev, _next;

        public float _value;
        public float _tangent;

        public KeyframeEntry(int index, float value)
        {
            _index = index;
            _prev = _next = this;
            _value = value;
        }

        public void InsertBefore(KeyframeEntry entry)
        {
            _prev._next = entry;
            entry._prev = _prev;
            entry._next = this;
            _prev = entry;
        }
        public void InsertAfter(KeyframeEntry entry)
        {
            _next._prev = entry;
            entry._next = _next;
            entry._prev = this;
            _next = entry;
        }
        public void Remove()
        {
            _next._prev = _prev;
            _prev._next = _next;
        }

        public float Interpolate2(float offset, bool linear, int frameCount)
        {
            if (offset == 0) return _value;
            int span = frameCount - _index + _next._index;
            if (offset == span) return _next._value;

            float diff = _next._value - _value;

            if (linear) return _value + (diff / span * offset);

            float time = (float)offset / span;
            float inv = time - 1.0f;

            return _value
                + (offset * inv * ((inv * _tangent) + (time * _next._tangent)))
                + ((time * time) * (3.0f - 2.0f * time) * diff);
        }

        public float Interpolate(float offset, bool linear)
        {
            if (offset == 0) return _value;
            int span = _next._index - _index;
            if (offset == span) return _next._value;

            float diff = _next._value - _value;
            
            if (linear) return _value + (diff / span * offset);

            float time = (float)offset / span;
            float inv = time - 1.0f;

            return _value 
                + (offset * inv * ((inv * _tangent) + (time * _next._tangent)))
                + ((time * time) * (3.0f - 2.0f * time) * diff);
        }

        // Arguments:    p1: Value at point 1.
        //               t1: Slope at point 1.
        //               p2: Value at point 2.
        //               t2: Slope at point 2.
        //               s:  Interpolation target position. (Point 1: 0.0~1.0 :Point 2)
        float Hermite(float p1, float t1,
                    float p2, float t2,
                    float s)
        {
            float SS = s * s;
            float SSmS = s * s - s;
            float b1 = SSmS * s - SSmS;
            float b2 = SSmS * s;
            float a2 = SS - 2.0f * b2;
            //    f32 a1 = 1.f - a2;

            return p1 - a2 * p1 + a2 * p2 + b1 * t1 + b2 * t2;
        }

        // Arguments:    p1: Value at point 1.
        //               p2: Control value at point 1.
        //               p3: Control value at point 2.
        //               p4: Value at point 2.
        //               s:  Interpolation target position. (Point 1: 0.0~1.0 :Point 2)
        float Bezier(float p1, float p2, float p3, float p4, float s)
        {
            float t = 1.0f - s;
            float tt = t * t;
            float ss = s * s;

            float a1 = tt * t;
            float a2 = tt * s * 3.0f;
            float a3 = ss * t * 3.0f;
            float a4 = ss * s;

            return a1 * p1 + a2 * p2 + a3 * p3 + a4 * p4;
        }

        // Arguments:   p0: Control value at point 1.
        //              p1: Value at point 1.
        //              p2: Value at point 2.
        //              p3: Control value at point 2.
        //              s:  Interpolation target position. (Point 1: 0.0~1.0 :Point 2)
        public float CatmullRom(float p0, float p1, float p2, float p3, float s)
        {
            return Hermite(p1, 0.5f * (p0 + p2),
                           p2, 0.5f * (p1 + p3),
                           s);
        }

        public float Hermite(float t)
        {
            float h1 = (float)(2 * (t * t * t) - 3 * (t * t) + 1);
            float h2 = (float)(-2 * (t * t * t) + 3 * (t * t));
            float h3 = (float)((t * t * t) - 2 * (t * t) + t);
            float h4 = (float)((t * t * t) - (t * t));

            return
                h1 * _value +
                h2 * _next._value +
                h3 * _tangent +
                h4 * _next._tangent;
        }

        public float GenerateTangent()
        {
            float tan = 0.0f;
            if (_prev._index != -1)
                tan += (_value - _prev._value) / (_index - _prev._index);
            if (_next._index != -1)
            {
                tan += (_next._value - _value) / (_next._index - _index);
                if (_prev._index != -1)
                    tan *= 0.5f;
            }

            return _tangent = tan;
        }

        public override string ToString()
        {
            return String.Format("Prev={0}, Next={1}, Value={2}", _prev, _next, _value);
        }
    }

    public unsafe class KeyframeArray
    {
        internal KeyframeEntry _keyRoot = new KeyframeEntry(-1, 0.0f);
        internal int _keyCount = 0;

        internal int _frameLimit;
        public int FrameLimit
        {
            get { return _frameLimit; }
            set
            {
                _frameLimit = value;
                while (_keyRoot._prev._index >= value)
                {
                    _keyRoot._prev.Remove();
                    _keyCount--;
                }

            }
        }
        
        internal bool _linear;
        public bool LinearInterpolation { get { return _linear; } set { _linear = value; } }

        public float this[int index]
        {
            get { return GetFrameValue(index); }
            set { SetFrameValue(index, value); }
        }

        public KeyframeArray(int limit) { _frameLimit = limit; }

        private const float _cleanDistance = 0.00001f;
        public void Clean()
        {
            int flag, res;
            KeyframeEntry entry;

            //Eliminate redundant values
            for (entry = _keyRoot._next._next; entry != _keyRoot; entry = entry._next)
            {
                flag = res = 0;

                if (entry._prev == _keyRoot)
                {
                    if (entry._next != _keyRoot)
                        flag = 1;
                }
                else if (entry._next == _keyRoot)
                    flag = 2;
                else
                    flag = 3;

                if ((flag & 1) != 0)
                    res |= (Math.Abs(entry._next._value - entry._value) <= _cleanDistance) ? 1 : 0;
                
                if ((flag & 2) != 0)
                    res |= (Math.Abs(entry._prev._value - entry._value) <= _cleanDistance) ? 2 : 0;

                if ((flag == res) && (res != 0))
                {
                    entry = entry._prev;
                    entry._next.Remove();

                    entry.GenerateTangent();
                    entry._next.GenerateTangent();
                    entry._prev.GenerateTangent();

                    _keyCount--;
                }
            }
        }

        public KeyframeEntry GetKeyframe(int index)
        {
            KeyframeEntry entry;
            for (entry = _keyRoot._next; (entry != _keyRoot) && (entry._index < index); entry = entry._next) ;
            if (entry._index == index)
                return entry;
            return null;
        }

        public float GetFrameValue(int index)
        {
            return GetFrameValue(index, false);
        }
        public float GetFrameValue(int index, bool linear)
        {
            KeyframeEntry entry;

            if (index >= _keyRoot._prev._index)
                return _keyRoot._prev._value;
            if (index <= _keyRoot._next._index)
                return _keyRoot._next._value;

            //Find the entry just before the specified index
            for (entry = _keyRoot._next; //Get the first entry
                (entry != _keyRoot) && //Make sure it's not the root
                (entry._index < index);  //Its index must be less than the current index
                entry = entry._next) //Get the next entry
                if (entry._index == index) //The index is a keyframe
                    return entry._value; //Return the value of the keyframe.

            //There was no keyframe... interpolate!
            return entry._prev.Interpolate(index - entry._prev._index, _linear || linear);
        }

        public KeyframeEntry SetFrameValue(int index, float value)
        {
            KeyframeEntry entry = null;
            if ((_keyRoot._prev == _keyRoot) || (_keyRoot._prev._index < index))
                entry = _keyRoot;
            else
                for (entry = _keyRoot._next; (entry != _keyRoot) && (entry._index <= index); entry = entry._next) ;

            entry = entry._prev;
            if (entry._index != index)
            {
                _keyCount++;
                entry.InsertAfter(entry = new KeyframeEntry(index, value));
            }
            else
                entry._value = value;

            return entry;
        }

        public KeyframeEntry Remove(int index)
        {
            KeyframeEntry entry = null;
            for (entry = _keyRoot._next; (entry != _keyRoot) && (entry._index < index); entry = entry._next) ;

            if (entry._index == index)
            {
                entry.Remove();
                _keyCount--;
            }
            else
                entry = null;

            return entry;
        }

        public void Insert(int index)
        {
            KeyframeEntry entry = null;
            for (entry = _keyRoot._prev; (entry != _keyRoot) && (entry._index >= index); entry = entry._prev)
                if (++entry._index >= _frameLimit)
                {
                    entry = entry._next;
                    entry._prev.Remove();
                    _keyCount--;
                }
        }

        public void Delete(int index)
        {
            KeyframeEntry entry = null;
            for (entry = _keyRoot._prev; (entry != _keyRoot) && (entry._index >= index); entry = entry._prev)
                if ((entry._index == index) || (--entry._index < 0))
                {
                    entry = entry._next;
                    entry._prev.Remove();
                    _keyCount--;
                }
        }
    }
}
