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
    public unsafe class SCN0FogNode : SCN0EntryNode, IColorSource, ISCN0KeyframeSource
    {
        internal SCN0Fog* Data { get { return (SCN0Fog*)WorkingUncompressed.Address; } }

        private FogType _type = FogType.PerspectiveLinear;
        public SCN0FogFlags _flags = (SCN0FogFlags)0xE0;
        public bool _constantColor = true;
        public KeyframeArray _startKeys = new KeyframeArray(0), _endKeys = new KeyframeArray(0);
        internal List<RGBAPixel> _colors = new List<RGBAPixel>();
        internal RGBAPixel _solidColor;

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
        public ARGBPixel GetColor(int index, int id) { return (_numEntries != 0 && index < _colors.Count) ? (ARGBPixel)_colors[index] : (ARGBPixel)_solidColor; }
        public void SetColor(int index, int id, ARGBPixel color)
        {
            if (_numEntries == 0)
                _solidColor = (RGBAPixel)color;
            else
                _colors[index] = (RGBAPixel)color;
            SignalPropertyChange();
        }
        public bool GetColorConstant(int id)
        {
            return ConstantColor;
        }
        public void SetColorConstant(int id, bool constant)
        {
            ConstantColor = constant;
        }

        #endregion

        #region ISCN0KeyframeHolder Members
        [Browsable(false)]
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
        public bool ConstantColor
        {
            get { return _constantColor; }
            set
            {
                if (_constantColor != value)
                {
                    _constantColor = value;
                    if (_constantColor)
                        MakeSolid(new RGBAPixel());
                    else
                        MakeList();

                    UpdateCurrentControl();
                }
            }
        }
        [Category("Fog")]
        public FogType Type { get { return _type; } set { _type = value; SignalPropertyChange(); } }

        [Browsable(false)]
        public int FrameCount
        {
            get { return ((SCN0Node)Parent.Parent).FrameCount; }
            set
            {
                _numEntries = _colors.Count;
                NumEntries = value + 1;
                if (_constantColor)
                    _numEntries = 0;
                _startKeys.FrameLimit = value;
                _endKeys.FrameLimit = value;
            }
        }

        [Browsable(false)]
        public List<RGBAPixel> Colors { get { return _colors; } set { _colors = value; SignalPropertyChange(); } }

        [Browsable(false)]
        public RGBAPixel SolidColor { get { return _solidColor; } set { _solidColor = value; SignalPropertyChange(); } }

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
                    RGBAPixel p = _numEntries > 0 ? _colors[_numEntries - 1] : new RGBAPixel(0, 0, 0, 255);
                    for (int i = value - _numEntries; i-- > 0; )
                        _colors.Add(p);
                }
                else if (value < _colors.Count)
                    _colors.RemoveRange(value, _colors.Count - value);

                _numEntries = value;
            }
        }

        public void MakeSolid(RGBAPixel color)
        {
            _numEntries = 0;
            _constantColor = true;
            _solidColor = color;
            SignalPropertyChange();
        }
        public void MakeList()
        {
            _constantColor = false;
            int entries = FrameCount + 1;
            _numEntries = _colors.Count;
            NumEntries = entries;
        }

        public override bool OnInitialize()
        {
            //Read common header
            base.OnInitialize();

            //Set defaults
            _colors = new List<RGBAPixel>();
            _startKeys = new KeyframeArray(FrameCount);
            _endKeys = new KeyframeArray(FrameCount);

            //Read header values
            _flags = (SCN0FogFlags)Data->_flags;
            _type = (FogType)(int)Data->_type;

            //Don't bother reading data if the entry is null
            if (Name == "<null>")
                return false;

            //Read start and end keyframe arrays
            for (int i = 0; i < 2; i++)
                DecodeKeyframes(
                    GetKeys(i),
                    Data->_start.Address + i * 4,
                    (int)_flags,
                    (int)SCN0FogFlags.FixedStart + i * 0x20);

            //Read fog color
            ReadColors(
                (uint)_flags,
                (uint)SCN0FogFlags.FixedColor,
                ref _solidColor,
                ref _colors,
                FrameCount,
                Data->_color.Address,
                ref _constantColor,
                ref _numEntries);

            return false;
        }

        SCN0FogNode _match;
        public override int OnCalculateSize(bool force)
        {
            //If a previous fog node has the same exact color array as this one,
            //both offsets will point to only the first color array.
            _match = null;

            //Reset data lengths
            for (int i = 0; i < 3; i++)
                _dataLengths[i] = 0;

            //Null nodes are only empty headers. No data.
            if (_name == "<null>")
                return SCN0Fog.Size;

            //Add keyframe array sizes
            for (int i = 0; i < 2; i++)
                CalcKeyLen(GetKeys(i));

            _match = FindColorMatch(_constantColor, FrameCount, _match, 0) as SCN0FogNode;
            if (_match == null && !_constantColor)
                _dataLengths[1] += 4 * (FrameCount + 1);

            return SCN0Fog.Size;
        }

        VoidPtr _matchAddr;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);

            _matchAddr = null;

            SCN0Fog* header = (SCN0Fog*)address;

            int flags = 0;

            for (int i = 0; i < 2; i++)
                _dataAddrs[0] += EncodeKeyframes(
                    GetKeys(i),
                    _dataAddrs[0],
                    header->_start.Address + i * 4,
                    ref flags,
                    (int)SCN0FogFlags.FixedStart + i * 0x20);

            _dataAddrs[1] += WriteColors(
                ref flags,
                (int)SCN0FogFlags.FixedColor,
                _solidColor,
                _colors,
                _constantColor,
                FrameCount,
                header->_color.Address,
                ref _matchAddr,
                _match == null ? null : _match._matchAddr,
                (RGBAPixel*)_dataAddrs[1]);

            _flags = (SCN0FogFlags)flags;
            header->_flags = (byte)flags;
            header->_type = (int)_type;
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
                frame.SetBools(x, a.GetKeyframe((int)index) != null);
                frame.Index = index;
            }
            return frame;
        }
        internal FogAnimationFrame GetAnimFrame(int index, bool linear)
        {
            FogAnimationFrame frame;
            float* dPtr = (float*)&frame;
            for (int x = 0; x < 2; x++)
            {
                KeyframeArray a = GetKeys(x);
                *dPtr++ = a.GetFrameValue(index, linear);
                frame.SetBools(x, a.GetKeyframe((int)index) != null);
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
