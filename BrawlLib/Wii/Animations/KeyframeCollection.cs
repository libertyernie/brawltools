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
    public interface IKeyframeSource
    {
        //KeyframeEntry GetKeyframe(int index, int arrayIndex);
        //KeyframeEntry SetKeyframe(int index, float value, params int[] arrays);
        //void RemoveKeyframe(int index, params int[] arrays);
        int FrameCount { get; }
        KeyframeArray[] KeyArrays { get; }
    }

    public class KeyframeCollection
    {
        public KeyframeArray[] _keyArrays;

        public KeyframeArray this[int index]
        {
            get { return _keyArrays[index.Clamp(0, _keyArrays.Length - 1)]; }
        }

        private int _frameLimit;
        public int FrameLimit
        {
            get { return _frameLimit; }
            set
            {
                _frameLimit = value;
                foreach (KeyframeArray r in _keyArrays)
                    r.FrameLimit = _frameLimit;
            }
        }

        public bool LinearRotation { get { return _linearRot; } set { _linearRot = value; } }
        internal bool _linearRot;

        public KeyframeCollection(int arrayCount, int numFrames, params float[] defaultValues)
        {
            _frameLimit = numFrames;
            _keyArrays = new KeyframeArray[arrayCount];
            for (int i = 0; i < arrayCount; i++)
                _keyArrays[i] = new KeyframeArray(numFrames, i < defaultValues.Length ? defaultValues[i] : 0);
        }

        public float this[int index, params int[] arrays]
        {
            get { return GetFrameValue(arrays[0], index); }
            set { foreach (int i in arrays) _keyArrays[i].SetFrameValue(index, value); }
        }

        public KeyframeEntry SetFrameValue(int arrayIndex, int frameIndex, float value, bool parsing = false)
        {
            return _keyArrays[arrayIndex].SetFrameValue(frameIndex, value, parsing);
        }

        public KeyframeEntry GetKeyframe(int arrayIndex, int index)
        {
            return _keyArrays[arrayIndex].GetKeyframe(index);
        }

        public float GetFrameValue(int arrayIndex, float index)
        {
            return _keyArrays[arrayIndex].GetFrameValue(index);
        }

        internal KeyframeEntry Remove(int arrayIndex, int index)
        {
            KeyframeEntry entry = null, root = _keyArrays[arrayIndex]._keyRoot;

            for (entry = root._next; (entry != root) && (entry._index < index); entry = entry._next) ;

            if (entry._index == index)
            {
                entry.Remove();
                _keyArrays[arrayIndex]._keyCount--;
            }
            else
                entry = null;
            
            return entry;
        }

        public void Insert(int index, params int[] arrays)
        {
            KeyframeEntry entry = null, root;
            foreach (int x in arrays)
            {
                root = _keyArrays[x]._keyRoot;
                for (entry = root._prev; (entry != root) && (entry._index >= index); entry = entry._prev)
                    if (++entry._index >= _frameLimit)
                    {
                        entry = entry._next;
                        entry._prev.Remove();
                        _keyArrays[x]._keyCount--;
                    }
            }
        }

        public void Delete(int index, params int[] arrays)
        {
            KeyframeEntry entry = null, root;
            foreach (int x in arrays)
            {
                root = _keyArrays[x]._keyRoot;
                for (entry = root._prev; (entry != root) && (entry._index >= index); entry = entry._prev)
                    if ((entry._index == index) || (--entry._index < 0))
                    {
                        entry = entry._next;
                        entry._prev.Remove();
                        _keyArrays[x]._keyCount--;
                    }
            }
        }

        public int Clean()
        {
            int removed = 0;
            foreach (KeyframeArray arr in _keyArrays)
                removed += arr.Clean();
            return removed;
        }

        public int ArrayCount { get { return _keyArrays.Length; } }
    }

    public class KeyframeEntry
    {
        public int _index;
        public KeyframeEntry _prev, _next;

        public float _value;
        public float _tangent;

        //A second keyframe on the same frame is used for jump cutting values/tangents
        public KeyframeEntry _out;

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

        /// <summary>
        /// Returns an interpolated value between this keyframe and the next. 
        /// You can force linear calculation, but the Wii itself doesn't have anything like that.
        /// The Wii emulates linear interpolation using two keyframes across a range with the same tangent
        /// and then two keyframes on the same frame but with different tangents.
        /// </summary>
        public float Interpolate(float offset, bool forceLinear = false)
        {
            //Return this value if no offset from this keyframe
            if (offset == 0)
                return _value;

            //Get the difference in frames
            int span = _next._index - _index;

            //Return next value if offset is to the next keyframe
            if (offset == span)
                return _next._value;

            //Get the difference in values
            float diff = _next._value - _value;

            //Calculate a percentage from this keyframe to the next
            float time = offset / span; //Normalized, 0 to 1

            if (forceLinear)
                return _value + diff * time;
            
            //Interpolate using a hermite curve
            float inv = time - 1.0f; //-1 to 0
            return _value
                + (offset * inv * ((inv * _tangent) + (time * _next._tangent)))
                + ((time * time) * (3.0f - 2.0f * time) * diff);
        }

        const bool RoundTangent = true;
        const int TangetDecimalPlaces = 3;

        public float GenerateTangent()
        {
            _tangent = 0.0f;

            float weightCount = 0;

            //add the slope (dy/dx) tangent from the prev to current value to tan
            if (_prev._index != -1)
            {
                _tangent += (_value - _prev._value) / (_index - _prev._index);
                weightCount++;
            }

            //add the slope tangent from the current to next value
            if (_next._index != -1)
            {
                _tangent += (_next._value - _value) / (_next._index - _index);
                weightCount++;
            }

            //float deltaX = 2;
            //float ipoVal = _prev.Interpolate(_index - _prev._index - (deltaX / 2f), false,_next);
            //float ipoVal2 = _prev.Interpolate(_index - _prev._index + (deltaX / 2f), false, _next);
            //float curveTan = (ipoVal2 - ipoVal) / deltaX;
           //  tan += curveTan;
           // weightCount++;

            if (weightCount > 0)
                _tangent /= weightCount;

            if (RoundTangent)
                _tangent = (float)Math.Round(_tangent, TangetDecimalPlaces);

            return _tangent;
        }

        public override string ToString()
        {
            return String.Format("Prev={0}, Next={1}, Value={2}", _prev, _next, _value);
        }
    }

    public unsafe class KeyframeArray
    {
        internal KeyframeEntry _keyRoot;
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
        
        public float this[int index]
        {
            get { return GetFrameValue(index); }
            set { SetFrameValue(index, value); }
        }

        public KeyframeArray(int limit, float defaultValue = 0)
        {
            _frameLimit = limit;
            _keyRoot = new KeyframeEntry(-1, defaultValue);
        }

        private const float _cleanDistance = 0.00001f;
        public int Clean()
        {
            int flag, res, removed = 0;
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
                    removed++;
                }
            }

            return removed;
        }

        public KeyframeEntry GetKeyframe(int index)
        {
            KeyframeEntry entry;
            for (entry = _keyRoot._next; (entry != _keyRoot) && (entry._index < index); entry = entry._next) ;
            if (entry._index == index)
                return entry;
            return null;
        }

        public float GetFrameValue(float index)
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
            return entry._prev.Interpolate(index - entry._prev._index);
        }

        public KeyframeEntry SetFrameValue(int index, float value, bool parsing = false)
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
            {
                //There can be up to two keyframes with the same index.
                if (!parsing)
                    entry._value = value; //Do this when editing
                else
                {
                    //And this when parsing
                    _keyCount++;
                    entry.InsertAfter(entry = new KeyframeEntry(index, value));
                }
            }

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
