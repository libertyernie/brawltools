using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Imaging;
using BrawlLib.Wii.Graphics;
using BrawlLib.Wii.Animations;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class SCN0FogNode : SCN0EntryNode, IColorSource, ISCN0KeyframeHolder
    {
        internal SCN0Fog* Data { get { return (SCN0Fog*)WorkingUncompressed.Address; } }

        private int type = 2;
        public SCN0FogFlags flags = (SCN0FogFlags)0xE0;

        public KeyframeArray _startKeys = new KeyframeArray(0), _endKeys = new KeyframeArray(0);

        #region IColorSource Members

        public bool HasPrimary(int id) { return false; }
        public ARGBPixel GetPrimaryColor(int id) { return new ARGBPixel(); }
        public void SetPrimaryColor(int id, ARGBPixel color) { }
        [Browsable(false)]
        public string PrimaryColorName(int id) { return null; }
        [Browsable(false)]
        public int TypeCount { get { return 1; } }
        [Browsable(false)]
        public int ColorCount(int id) { return (_numEntries == 0) ? 1 : _numEntries; }
        public ARGBPixel GetColor(int index, int id) { return (_numEntries == 0) ? _solidColor : _colors[index]; }
        public void SetColor(int index, int id, ARGBPixel color)
        {
            if (_numEntries == 0)
                _solidColor = color;
            else
                _colors[index] = color;
            SignalPropertyChange();
        }
        public bool GetClrConstant(int id)
        {
            return ConstantColor;
        }
        public void SetClrConstant(int id, bool constant)
        {
            ConstantColor = constant;
        }

        #endregion

        public bool _constant = true;
        [Category("Fog")]
        public bool ConstantColor
        {
            get { return _constant; }
            set
            {
                if (_constant != value)
                {
                    _constant = value;
                    if (_constant)
                        MakeSolid(new ARGBPixel());
                    else
                        MakeList();

                    UpdateCurrentControl();
                }
            }
        }

        #region ISCN0KeyframeHolder Members
        public int KeyArrayCount { get { return 2; } }
        public KeyframeArray GetKeys(int i)
        {
            switch (i)
            {
                case 0: return _startKeys;
                case 1: return _endKeys;
            }
            return null;
        }
        public void SetKeys(int i, KeyframeArray value)
        {
            switch (i)
            {
                case 0: _startKeys = value; break;
                case 1: _endKeys = value; break;
            }
        }
        #endregion

        [Category("Fog")]
        public FogType Type { get { return (FogType)type; } set { type = (int)value; SignalPropertyChange(); } }
        
        [Browsable(false)]
        public int FrameCount
        {
            get { return ((SCN0Node)Parent.Parent).FrameCount; }
            set
            {
                _numEntries = _colors.Count;
                NumEntries = value + 1; 
                if (_constant)
                    _numEntries = 0;
            }
        }

        internal List<ARGBPixel> _colors = new List<ARGBPixel>();
        [Browsable(false)]
        public List<ARGBPixel> Colors { get { return _colors; } set { _colors = value; SignalPropertyChange(); } }

        internal ARGBPixel _solidColor;
        [Browsable(false)]
        public ARGBPixel SolidColor { get { return _solidColor; } set { _solidColor = value; SignalPropertyChange(); } }

        internal int _numEntries;
        [Browsable(false)]
        internal int NumEntries
        {
            get { return _numEntries; }
            set
            {
                //if (_numEntries == 0)
                //    return;

                if (value > _numEntries)
                {
                    ARGBPixel p = _numEntries > 0 ? _colors[_numEntries - 1] : new ARGBPixel(255, 0, 0, 0);
                    for (int i = value - _numEntries; i-- > 0; )
                        _colors.Add(p);
                }
                else if (value < _colors.Count)
                    _colors.RemoveRange(value, _colors.Count - value);

                _numEntries = value;
            }
        }

        public void MakeSolid(ARGBPixel color)
        {
            _numEntries = 0;
            _constant = true;
            _solidColor = color;
            SignalPropertyChange();
        }
        public void MakeList()
        {
            _constant = false;
            int entries = FrameCount + 1;
            _numEntries = _colors.Count;
            NumEntries = entries;
        }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _colors = new List<ARGBPixel>();

            _startKeys = new KeyframeArray(FrameCount + 1);
            _endKeys = new KeyframeArray(FrameCount + 1);

            flags = (SCN0FogFlags)Data->_flags;
            type = Data->_type;
            if (Name != "<null>")
            {
                if (flags.HasFlag(SCN0FogFlags.FixedStart))
                    _startKeys[0] = Data->_start;
                else if (!_replaced)
                    DecodeFrames(_startKeys, Data->startKeyframes);
                
                if (flags.HasFlag(SCN0FogFlags.FixedEnd))
                    _endKeys[0] = Data->_end;
                else if (!_replaced)
                    DecodeFrames(_endKeys, Data->endKeyframes);
                
                if (flags.HasFlag(SCN0FogFlags.FixedColor))
                {
                    _constant = true;
                    _numEntries = 0;
                    _solidColor = (ARGBPixel)Data->_color;
                }
                else
                {
                    _constant = false;
                    _numEntries = FrameCount + 1;
                    RGBAPixel* addr = Data->colorEntries;
                    for (int i = 0; i <= FrameCount; i++)
                        _colors.Add((ARGBPixel)(*addr++));
                }
            }

            return false;
        }

        SCN0FogNode _match;
        public override int OnCalculateSize(bool force)
        {
            _match = null;
            _keyLen = 0;
            _lightLen = 0;
            _visLen = 0;
            if (_name != "<null>")
            {
                if (_startKeys._keyCount > 1)
                    _keyLen += 8 + _startKeys._keyCount * 12;
                if (_endKeys._keyCount > 1)
                    _keyLen += 8 + _endKeys._keyCount * 12;
                if (!_constant)
                {
                    foreach (SCN0FogNode n in Parent.Children)
                    {
                        if (n == this)
                            break;

                        if (!n._constant)
                        {
                            for (int i = 0; i < FrameCount + 1; i++)
                            {
                                if (n._colors[i] != _colors[i])
                                    break;
                                if (i == FrameCount)
                                    _match = n;
                            }
                        }

                        if (_match != null)
                            break;
                    }
                    if (_match == null)
                        _lightLen += 4 * (FrameCount + 1);
                }
            }
            return SCN0Fog.Size;
        }
        VoidPtr _matchAddr;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);

            _matchAddr = null;

            SCN0Fog* header = (SCN0Fog*)address;

            flags = SCN0FogFlags.None;
            if (_colors.Count > 1)
            {
                _matchAddr = lightAddr;
                if (_match == null)
                {
                    *((bint*)header->_color.Address) = (int)lightAddr - (int)header->_color.Address;
                    for (int i = 0; i <= ((SCN0Node)Parent.Parent).FrameCount; i++)
                        if (i < _colors.Count)
                            *lightAddr++ = (RGBAPixel)_colors[i];
                        else
                            *lightAddr++ = new RGBAPixel();
                }
                else
                    *((bint*)header->_color.Address) = (int)_match._matchAddr - (int)header->_color.Address;
            }
            else
            {
                flags |= SCN0FogFlags.FixedColor;
                header->_color = (RGBAPixel)_solidColor;
            }
            if (_startKeys._keyCount > 1)
            {
                *((bint*)header->_start.Address) = (int)keyframeAddr - (int)header->_start.Address;
                EncodeFrames(_startKeys, ref keyframeAddr);
            }
            else
            {
                flags |= SCN0FogFlags.FixedStart;
                if (_startKeys._keyCount == 1)
                    header->_start = _startKeys._keyRoot._next._value;
                else
                    header->_start = 0;
            }
            if (_endKeys._keyCount > 1)
            {
                *((bint*)header->_end.Address) = (int)keyframeAddr - (int)header->_end.Address;
                EncodeFrames(_endKeys, ref keyframeAddr);
            }
            else
            {
                flags |= SCN0FogFlags.FixedEnd;
                if (_endKeys._keyCount == 1)
                    header->_end = _endKeys._keyRoot._next._value;
                else
                    header->_end = 0;
            }

            header->_flags = (byte)flags;
            header->_type = type;
        }

        protected internal override void PostProcess(VoidPtr scn0Address, VoidPtr dataAddress, StringTable stringTable)
        {
            base.PostProcess(scn0Address, dataAddress, stringTable);
        }

        public static bool _generateTangents = true;
        public static bool _linear = true;

        internal FogAnimationFrame GetAnimFrame(int index)
        {
            FogAnimationFrame frame;
            float* dPtr = (float*)&frame;
            for (int x = 0; x < 2; x++)
            {
                KeyframeArray a = GetKeys(x);
                *dPtr++ = a.GetFrameValue(index);
                frame.SetBools(x, a.GetKeyframe(index) != null);
                frame.Index = index;
            }
            return frame;
        }

        internal KeyframeEntry GetKeyframe(int keyFrameMode, int index)
        {
            return GetKeys(keyFrameMode).GetKeyframe(index);
        }

        internal void RemoveKeyframe(int keyFrameMode, int index)
        {
            KeyframeEntry k = GetKeys(keyFrameMode).Remove(index);
            if (k != null && _generateTangents)
            {
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
                SignalPropertyChange();
            }
        }
        
        internal void SetKeyframe(int keyFrameMode, int index, float value)
        {
            KeyframeArray keys = GetKeys(keyFrameMode);
            bool exists = keys.GetKeyframe(index) != null;
            KeyframeEntry k = keys.SetFrameValue(index, value);

            if (!exists && !_generateTangents)
                k.GenerateTangent();

            if (_generateTangents)
            {
                k.GenerateTangent();
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
            }

            SignalPropertyChange();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FogAnimationFrame
    {
        public static readonly FogAnimationFrame Empty = new FogAnimationFrame();

        public float Start;
        public float End;

        public bool hasS;
        public bool hasE;

        public bool HasKeys { get { return hasS || hasE; } }

        public void SetBools(int index, bool val)
        {
            switch (index)
            {
                case 0:
                    hasS = val; break;
                case 1:
                    hasE = val; break;
            }
        }

        public void ResetBools()
        {
            hasS = hasE = false;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Start;
                    case 1: return End;
                    default: return float.NaN;
                }
            }
            set
            {
                switch (index)
                {
                    case 0: Start = value; break;
                    case 1: End = value; break;
                }
            }
        }

        public FogAnimationFrame(float start, float end)
        {
            Start = start;
            End = end;
            Index = 0;
            hasS = hasE = false;
        }

        public int Index;
        const int len = 6;
        static string empty = new String('_', len);
        public override string ToString()
        {
            return String.Format("[{0}] StartZ={1}, EndZ={2}", (Index + 1).ToString().PadLeft(5),
            !hasS ? empty : Start.ToString().TruncateAndFill(len, ' '),
            !hasE ? empty : End.ToString().TruncateAndFill(len, ' '));
        }
    }
}
