using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Imaging;
using BrawlLib.Wii.Graphics;
using System.Runtime.InteropServices;
using BrawlLib.Wii.Animations;

namespace BrawlLib.SSBB.ResourceNodes
{
    public interface ISCN0KeyframeSource
    {
        int KeyArrayCount { get; }
        KeyframeArray GetKeys(int i);
        void SetKeys(int i, KeyframeArray value);
    }

    public unsafe class SCN0LightNode : SCN0EntryNode, IBoolArraySource, IColorSource, ISCN0KeyframeSource
    {
        internal SCN0Light* Data { get { return (SCN0Light*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        #region Variables

        //Header variables
        internal int _nonSpecLightId, _distFunc, _spotFunc;
        internal FixedFlags _fixedFlags = (FixedFlags)0xFFF8;
        internal Bin16 _typeUsageFlags = 0x35;

        //Visibility array
        internal byte[] _data = new byte[0];
        internal int _entryCount;

        //Color arrays
        public int[] _numEntries = new int[] { 0, 0 };
        public RGBAPixel[] _solidColors = new RGBAPixel[2];
        private List<RGBAPixel>
            _lightColor = new List<RGBAPixel>(),
            _specColor = new List<RGBAPixel>();
        public bool[] _constants = new bool[] { true, true };

        //Keyframes
        public KeyframeArray
            _startX = new KeyframeArray(0),
            _startY = new KeyframeArray(0),
            _startZ = new KeyframeArray(0),
            _endX = new KeyframeArray(0),
            _endY = new KeyframeArray(0),
            _endZ = new KeyframeArray(0),
            _spotCut = new KeyframeArray(0),
            _spotBright = new KeyframeArray(0),
            _refDist = new KeyframeArray(0),
            _refBright = new KeyframeArray(0);

        #endregion

        #region User Editable Variables
        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();
        [Category("SCN0 Entry")]
        public int NonSpecularLightID
        {
            get
            {
                if (!SpecularEnabled)
                    return 0;

                int i = 0;
                foreach (SCN0LightNode n in Parent.Children)
                {
                    if (n.Index == Index)
                        return Parent.Children.Count + i;
                    if (n.SpecularEnabled && n.Index != Index)
                        i++;
                }
                return 0;
            }
        }
        [Category("Light")]
        public LightType LightType { get { return (LightType)_typeUsageFlags[0, 2]; } set { _typeUsageFlags[0, 2] = (ushort)value; SignalPropertyChange(); } }
        [Category("Light")]
        public bool ColorEnabled
        {
            get { return UsageFlags.HasFlag(UsageFlags.ColorEnabled); }
            set
            {
                if (value)
                    UsageFlags |= UsageFlags.ColorEnabled;
                else
                    UsageFlags &= ~UsageFlags.ColorEnabled;
                SignalPropertyChange();
            }
        }
        [Category("Light")]
        public bool AlphaEnabled
        {
            get { return UsageFlags.HasFlag(UsageFlags.AlphaEnabled); }
            set
            {
                if (value)
                    UsageFlags |= UsageFlags.AlphaEnabled;
                else
                    UsageFlags &= ~UsageFlags.AlphaEnabled;
                SignalPropertyChange();
            }
        }
        [Category("Light")]
        public bool SpecularEnabled
        {
            get { return UsageFlags.HasFlag(UsageFlags.SpecularEnabled); }
            set
            {
                if (value)
                    UsageFlags |= UsageFlags.SpecularEnabled;
                else
                    UsageFlags &= ~UsageFlags.SpecularEnabled;
                SignalPropertyChange();
            }
        }
        [Category("Source Light")]
        public DistAttnFn DistanceFunction { get { return (DistAttnFn)_distFunc; } set { _distFunc = (int)value; SignalPropertyChange(); } }
        [Category("Spotlight")]
        public SpotFn SpotFunction { get { return (SpotFn)_spotFunc; } set { _spotFunc = (int)value; SignalPropertyChange(); } }
        [Category("Light Colors")]
        public bool ConstantColor
        {
            get { return _constants[0]; }
            set
            {
                if (_constants[0] != value)
                {
                    _constants[0] = value;
                    if (_constants[0])
                        MakeSolid(new ARGBPixel(), 0);
                    else
                        MakeList(0);

                    UpdateCurrentControl();
                }
            }
        }
        [Category("Light Colors")]
        public bool ConstantSpecular
        {
            get { return _constants[1]; }
            set
            {
                if (_constants[1] != value)
                {
                    _constants[1] = value;
                    if (_constants[1])
                        MakeSolid(new ARGBPixel(), 1);
                    else
                        MakeList(1);

                    UpdateCurrentControl();
                }
            }
        }
        [Category("Light Enable")]
        public bool Constant
        {
            get { return ConstantVisibility; }
            set
            {
                if (value != ConstantVisibility)
                {
                    if (value)
                        MakeConstant(true);
                    else
                        MakeAnimated();

                    UpdateCurrentControl();
                }
            }
        }
        [Category("Light Enable")]
        public bool Enabled
        {
            get { return VisibilityEnabled; }
            set { VisibilityEnabled = value; SignalPropertyChange(); }
        }
        #endregion

        #region IColorSource Members

        public bool HasPrimary(int id) { return false; }
        public ARGBPixel GetPrimaryColor(int id) { return new ARGBPixel(); }
        public void SetPrimaryColor(int id, ARGBPixel color) { }
        [Browsable(false)]
        public string PrimaryColorName(int id) { return null; }
        [Browsable(false)]
        public int TypeCount { get { return _numEntries.Length; } }
        [Browsable(false)]
        public int ColorCount(int id) { return (_numEntries[id] == 0) ? 1 : _numEntries[id]; }
        public ARGBPixel GetColor(int index, int id) { return (_numEntries[id] == 0) ? (ARGBPixel)_solidColors[id] : (ARGBPixel)GetColors(id)[index]; }
        public void SetColor(int index, int id, ARGBPixel color)
        {
            if (_numEntries[id] == 0)
                _solidColors[id] = (RGBAPixel)color;
            else
                GetColors(id)[index] = (RGBAPixel)color;
            SignalPropertyChange();
        }
        public bool GetColorConstant(int id)
        {
            switch (id)
            {
                case 0: return ConstantColor;
                case 1: return ConstantSpecular;
            }
            return false;
        }
        public void SetColorConstant(int id, bool constant)
        {
            switch (id)
            {
                case 0: ConstantColor = constant; break;
                case 1: ConstantSpecular = constant; break;
            }
        }

        #endregion

        #region IBoolArrayNode Members

        [Browsable(false)]
        public int EntryCount
        {
            get { return _entryCount; }
            set
            {
                if (_entryCount == 0)
                    return;

                _entryCount = value;
                int len = value.Align(32) / 8;

                if (_data.Length < len)
                {
                    byte[] newArr = new byte[len];
                    Array.Copy(_data, newArr, _data.Length);
                    _data = newArr;
                }
                SignalPropertyChange();
            }
        }

        public bool GetEntry(int index)
        {
            int i = index >> 3;
            int bit = 1 << (7 - (index & 0x7));
            return (_data[i] & bit) != 0;
        }
        public void SetEntry(int index, bool value)
        {
            int i = index >> 3;
            int bit = 1 << (7 - (index & 0x7));
            int mask = ~bit;
            _data[i] = (byte)((_data[i] & mask) | (value ? bit : 0));
            SignalPropertyChange();
        }

        public void MakeConstant(bool value)
        {
            ConstantVisibility = true;
            VisibilityEnabled = value;
            _entryCount = 0;

            SignalPropertyChange();
        }
        public void MakeAnimated()
        {
            bool enabled = VisibilityEnabled;

            ConstantVisibility = false;
            VisibilityEnabled = false;

            _entryCount = -1;
            EntryCount = FrameCount + 1;

            if (enabled)
                for (int i = 0; i < _entryCount; i++)
                    SetEntry(i, true);

            SignalPropertyChange();
        }

        #endregion

        #region ISCN0KeyframeHolder Members
        [Browsable(false)]
        public int KeyArrayCount { get { return 10; } }
        public KeyframeArray GetKeys(int i)
        {
            switch (i)
            {
                case 0: return _startX;
                case 1: return _startY;
                case 2: return _startZ;
                case 3: return _endX;
                case 4: return _endY;
                case 5: return _endZ;
                case 6: return _refDist;
                case 7: return _refBright;
                case 8: return _spotCut;
                case 9: return _spotBright;
            }
            return null;
        }
        public void SetKeys(int i, KeyframeArray value)
        {
            switch (i)
            {
                case 0: _startX = value; break;
                case 1: _startY = value; break;
                case 2: _startZ = value; break;
                case 3: _endX = value; break;
                case 4: _endY = value; break;
                case 5: _endZ = value; break;
                case 6: _refDist = value; break;
                case 7: _refBright = value; break;
                case 8: _spotCut = value; break;
                case 9: _spotBright = value; break;
            }
        }
        #endregion

        #region Flags
        [Browsable(false)]
        public UsageFlags UsageFlags { get { return (UsageFlags)_typeUsageFlags[2, 4]; } set { _typeUsageFlags[2, 4] = (ushort)value; SignalPropertyChange(); } }
        [Browsable(false)]
        public bool ConstantVisibility
        {
            get { return _fixedFlags.HasFlag(FixedFlags.EnabledConstant); }
            set
            {
                if (value)
                    _fixedFlags |= FixedFlags.EnabledConstant;
                else
                    _fixedFlags &= ~FixedFlags.EnabledConstant;
            }
        }
        [Browsable(false)]
        public bool VisibilityEnabled
        {
            get { return UsageFlags.HasFlag(UsageFlags.Enabled); }
            set
            {
                if (value)
                    UsageFlags |= UsageFlags.Enabled;
                else
                    UsageFlags &= ~UsageFlags.Enabled;
            }
        }
        private static readonly FixedFlags[] _ordered = new FixedFlags[] 
        {
            FixedFlags.StartXConstant,
            FixedFlags.StartYConstant,
            FixedFlags.StartZConstant,
            FixedFlags.EndXConstant,
            FixedFlags.EndYConstant,
            FixedFlags.EndZConstant,
            FixedFlags.RefDistanceConstant,
            FixedFlags.RefBrightnessConstant,
            FixedFlags.CutoffConstant,
            FixedFlags.ShininessConstant,
        };
        [Flags]
        public enum FixedFlags : ushort
        {
            StartXConstant = 0x8,
            StartYConstant = 0x10,
            StartZConstant = 0x20,
            ColorConstant = 0x40,
            EnabledConstant = 0x80, //Refer to Enabled in UsageFlags if constant
            EndXConstant = 0x100,
            EndYConstant = 0x200,
            EndZConstant = 0x400,
            CutoffConstant = 0x800,
            RefDistanceConstant = 0x1000,
            RefBrightnessConstant = 0x2000,
            SpecColorConstant = 0x4000,
            ShininessConstant = 0x8000
        }
        #endregion

        #region Light Color
        public List<RGBAPixel> GetColors(int i)
        {
            switch (i)
            {
                case 0: return _lightColor;
                case 1: return _specColor;
            }
            return null;
        }
        public void SetColors(int i, List<RGBAPixel> value)
        {
            switch (i)
            {
                case 0: _lightColor = value; break;
                case 1: _specColor = value; break;
            }
        }

        public void MakeSolid(ARGBPixel color, int id)
        {
            _numEntries[id] = 0;
            _constants[id] = true;
            _solidColors[id] = (RGBAPixel)color;
            SignalPropertyChange();
        }
        public void MakeList(int id)
        {
            _constants[id] = false;
            int entries = ((SCN0Node)Parent._parent).FrameCount + 1;
            _numEntries[id] = GetColors(id).Count;
            SetNumEntries(id, entries);
        }

        [Browsable(false)]
        internal void SetNumEntries(int id, int value)
        {
            //if (_numEntries[id] == 0)
            //    return;

            if (value > _numEntries[id])
            {
                ARGBPixel p = _numEntries[id] > 0 ? (ARGBPixel)GetColors(id)[_numEntries[id] - 1] : new ARGBPixel(255, 0, 0, 0);
                for (int i = value - _numEntries[id]; i-- > 0; )
                    GetColors(id).Add((RGBAPixel)p);
            }
            else if (value < GetColors(id).Count)
                GetColors(id).RemoveRange(value, GetColors(id).Count - value);

            _numEntries[id] = value;
        }


        #endregion

        [Browsable(false)]
        internal int FrameCount
        {
            get { return ((SCN0Node)Parent.Parent).FrameCount; }
            set
            {
                int x = value + 1;

                _numEntries[0] = GetColors(0).Count;
                _numEntries[1] = GetColors(1).Count;

                SetNumEntries(0, x);
                SetNumEntries(1, x);

                if (_constants[0])
                    _numEntries[0] = 0;
                if (_constants[1])
                    _numEntries[1] = 0;

                for (int i = 0; i < 10; i++)
                    GetKeys(i).FrameLimit = value;

                _entryCount = FrameCount + 1;
                int numBytes = Math.Min(_entryCount.Align(32) / 8, _data.Length);
                Array.Resize(ref _data, numBytes);
            }
        }

        public override bool OnInitialize()
        {
            //Read common header
            base.OnInitialize();

            //Initialize defaults
            _numEntries = new int[] { 0, 0 };
            _solidColors = new RGBAPixel[2];
            _constants = new bool[] { true, true };
            for (int x = 0; x < 10; x++)
                SetKeys(x, new KeyframeArray(FrameCount));

            //Read header values
            _nonSpecLightId = Data->_nonSpecLightId;
            _fixedFlags = (FixedFlags)(ushort)Data->_fixedFlags;
            _typeUsageFlags = (ushort)Data->_usageFlags;
            _distFunc = Data->_distFunc;
            _spotFunc = Data->_spotFunc;

            //Read user data
            //(_userEntries = new UserDataCollection()).Read(Data->UserData);

            //Don't bother reading data if the entry is null
            if (Name == "<null>")
                return false;

            //Read keyframe data
            int index = 0;
            for (int i = 0; i < 14; i++)
                if (!(i == 3 || i == 7 || i == 9 || i == 11))
                    DecodeKeyframes(
                        GetKeys(index),
                        Data->_startPoint._x.Address + i * 4,
                        (int)_fixedFlags,
                        (int)_ordered[index++]);

            //Read light visibility array
            if (!_fixedFlags.HasFlag(FixedFlags.EnabledConstant) && !_replaced)
            {
                _entryCount = FrameCount + 1;
                int numBytes = _entryCount.Align(32) / 8;

                _data = new byte[numBytes];
                Marshal.Copy((IntPtr)Data->VisBitEntries, _data, 0, numBytes);
            }
            else
            {
                _entryCount = 0;
                _data = new byte[0];
            }

            //Read light color
            ReadColors(
                (uint)_fixedFlags,
                (uint)FixedFlags.ColorConstant,
                ref _solidColors[0],
                ref _lightColor,
                FrameCount,
                Data->_lightColor.Address,
                ref _constants[0],
                ref _numEntries[0]);

            if (!SpecularEnabled)
                return false;

            //Read light specular color
            ReadColors(
                (uint)_fixedFlags,
                (uint)FixedFlags.SpecColorConstant,
                ref _solidColors[1],
                ref _specColor,
                FrameCount,
                Data->_specularColor.Address,
                ref _constants[1],
                ref _numEntries[1]);

            return false;
        }

        SCN0LightNode[] _matches = { null, null };
        public override int OnCalculateSize(bool force)
        {
            _matches[0] = null;
            _matches[1] = null;

            for (int i = 0; i < 3; i++)
                _dataLengths[i] = 0;

            if (_name == "<null>")
                return SCN0Light.Size;

            for (int i = 0; i < KeyArrayCount; i++)
                CalcKeyLen(GetKeys(i));

            for (int i = 0; i < 2; i++)
            {
                _matches[i] = FindColorMatch(_constants[i], FrameCount, _matches[i], i) as SCN0LightNode;
                if (_matches[i] == null && !_constants[i])
                    _dataLengths[1] += 4 * (FrameCount + 1);
            }

            if (!ConstantVisibility)
                _dataLengths[2] += _entryCount.Align(32) / 8;

            //If this light uses specular lighting, 
            //increment SCN0 specular light count
            if (UsageFlags.HasFlag(UsageFlags.SpecularEnabled))
                ((SCN0Node)Parent.Parent)._specLights++;

            return SCN0Light.Size;
        }

        VoidPtr _lightAddress, _specularAddress;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            //Build common header
            base.OnRebuild(address, length, force);

            //Reset addresses
            _lightAddress = null;
            _specularAddress = null;

            //Don't write anything if this node is null
            if (_name == "<null>")
                return;

            //Write header information
            SCN0Light* header = (SCN0Light*)address;
            header->_nonSpecLightId = NonSpecularLightID;
            header->_userDataOffset = 0;
            header->_distFunc = _distFunc;
            header->_spotFunc = _spotFunc;

            int newFlags = 0;

            //Encode keyframe data
            int index = 0;
            for (int i = 0; i < 14; i++)
                if (!(i == 3 || i == 7 || i == 9 || i == 11))
                    _dataAddrs[0] += EncodeKeyframes(
                        GetKeys(index),
                        _dataAddrs[0],
                        header->_startPoint._x.Address + i * 4,
                        ref newFlags,
                        (int)_ordered[index++]);

            _dataAddrs[1] += WriteColors(
                ref newFlags,
                (int)FixedFlags.ColorConstant,
                _solidColors[0],
                _lightColor,
                _constants[0],
                FrameCount,
                header->_lightColor.Address,
                ref _lightAddress,
                _matches[0] == null ? null : _matches[0]._lightAddress,
                (RGBAPixel*)_dataAddrs[1]);

            if (SpecularEnabled)
                _dataAddrs[1] += WriteColors(
                    ref newFlags,
                    (int)FixedFlags.SpecColorConstant,
                    _solidColors[1],
                    _specColor,
                    _constants[1],
                    FrameCount,
                    header->_specularColor.Address,
                    ref _specularAddress,
                    _matches[1] == null ? null : _matches[1]._specularAddress,
                    (RGBAPixel*)_dataAddrs[1]);

            if (!ConstantVisibility && _entryCount != 0)
            {
                header->_visOffset = (int)_dataAddrs[2] - (int)header->_visOffset.Address;
                Marshal.Copy(_data, 0, (IntPtr)_dataAddrs[2], _data.Length);
                _dataAddrs[2] = ((VoidPtr)_dataAddrs[2] + EntryCount.Align(32) / 8);
            }
            else
                newFlags |= (int)FixedFlags.EnabledConstant;

            //Set newly calculated flags
            header->_fixedFlags = (ushort)(_fixedFlags = (FixedFlags)newFlags);
            header->_usageFlags = _typeUsageFlags._data;
        }

        protected internal override void PostProcess(VoidPtr scn0Address, VoidPtr dataAddress, StringTable stringTable)
        {
            base.PostProcess(scn0Address, dataAddress, stringTable);
        }

        internal LightAnimationFrame GetAnimFrame(int index)
        {
            LightAnimationFrame frame;
            float* dPtr = (float*)&frame;
            for (int x = 0; x < 10; x++)
            {
                KeyframeArray a = GetKeys(x);
                *dPtr++ = a.GetFrameValue(index);
                frame.SetBools(x, a.GetKeyframe((int)index) != null);
                frame.Index = index;
            }

            //if (((FixedFlags)_flags1).HasFlag(FixedFlags.EnabledConstant))
            //    frame.Enabled = UsageFlags.HasFlag(UsageFlags.Enabled);
            //else
            //    frame.Enabled = index < _enabled.Count ? _enabled[index] : false;

            return frame;
        }
        internal LightAnimationFrame GetAnimFrame(int index, bool linear)
        {
            LightAnimationFrame frame;
            float* dPtr = (float*)&frame;
            for (int x = 0; x < 10; x++)
            {
                KeyframeArray a = GetKeys(x);
                *dPtr++ = a.GetFrameValue(index, linear);
                frame.SetBools(x, a.GetKeyframe((int)index) != null);
                frame.Index = index;
            }

            //if (((FixedFlags)_flags1).HasFlag(FixedFlags.EnabledConstant))
            //    frame.Enabled = UsageFlags.HasFlag(UsageFlags.Enabled);
            //else
            //    frame.Enabled = index < _enabled.Count ? _enabled[index] : false;

            return frame;
        }

        public static bool _generateTangents = true;
        public static bool _linear = true;

        public float GetFrameValue(LightKeyframeMode keyFrameMode, float index)
        {
            return GetKeys((int)keyFrameMode).GetFrameValue(index);
        }

        internal KeyframeEntry GetKeyframe(LightKeyframeMode keyFrameMode, int index)
        {
            return GetKeys((int)keyFrameMode).GetKeyframe(index);
        }

        internal void RemoveKeyframe(LightKeyframeMode keyFrameMode, int index)
        {
            KeyframeEntry k = GetKeys((int)keyFrameMode).Remove(index);
            if (k != null && _generateTangents)
            {
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
                SignalPropertyChange();
            }
        }

        internal void SetKeyframe(LightKeyframeMode keyFrameMode, int index, float value)
        {
            KeyframeArray keys = GetKeys((int)keyFrameMode);
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

        public Vector3 GetStart(int frame)
        {
            return new Vector3(
                _startX.GetFrameValue(frame),
                _startY.GetFrameValue(frame),
                _startZ.GetFrameValue(frame));
        }

        public Vector3 GetEnd(int frame)
        {
            return new Vector3(
                _endX.GetFrameValue(frame),
                _endY.GetFrameValue(frame),
                _endZ.GetFrameValue(frame));
        }

        public Vector3 GetLightSpot(int frame)
        {
            float a0, a1, a2, r, d, cr;

            SpotFn spot_func = SpotFunction;
            float cutoff = _spotCut.GetFrameValue(frame);

            if (cutoff <= 0.0f || cutoff > 90.0f)
                spot_func = SpotFn.Off;

            r = cutoff * Maths._deg2radf;
            cr = (float)Math.Cos(r);

            switch (spot_func)
            {
                case SpotFn.Flat:
                    a0 = -1000.0f * cr;
                    a1 = 1000.0f;
                    a2 = 0.0f;
                    break;
                case SpotFn.Cos:
                    a0 = -cr / (1.0f - cr);
                    a1 = 1.0f / (1.0f - cr);
                    a2 = 0.0f;
                    break;
                case SpotFn.Cos2:
                    a0 = 0.0f;
                    a1 = -cr / (1.0f - cr);
                    a2 = 1.0f / (1.0f - cr);
                    break;
                case SpotFn.Sharp:
                    d = (1.0f - cr) * (1.0f - cr);
                    a0 = cr * (cr - 2.0f) / d;
                    a1 = 2.0f / d;
                    a2 = -1.0f / d;
                    break;
                case SpotFn.Ring:
                    d = (1.0f - cr) * (1.0f - cr);
                    a0 = -4.0f * cr / d;
                    a1 = 4.0f * (1.0f + cr) / d;
                    a2 = -4.0f / d;
                    break;
                case SpotFn.Ring2:
                    d = (1.0f - cr) * (1.0f - cr);
                    a0 = 1.0f - 2.0f * cr * cr / d;
                    a1 = 4.0f * cr / d;
                    a2 = -2.0f / d;
                    break;
                case SpotFn.Off:
                default:
                    a0 = 1.0f;
                    a1 = 0.0f;
                    a2 = 0.0f;
                    break;
            }

            return new Vector3(a0, a1, a2);
        }

        public Vector3 GetLightDistAttn(int frame)
        {
            float k0, k1, k2;

            float ref_dist = _refDist.GetFrameValue(frame);
            float ref_br = _refBright.GetFrameValue(frame);
            DistAttnFn dist_func = DistanceFunction;

            if (ref_dist < 0.0F || ref_br <= 0.0F || ref_br >= 1.0F)
                dist_func = DistAttnFn.Off;

            switch (dist_func)
            {
                case DistAttnFn.Gentle:
                    k0 = 1.0F;
                    k1 = (1.0F - ref_br) / (ref_br * ref_dist);
                    k2 = 0.0F;
                    break;
                case DistAttnFn.Medium:
                    k0 = 1.0F;
                    k1 = 0.5F * (1.0f - ref_br) / (ref_br * ref_dist);
                    k2 = 0.5F * (1.0f - ref_br) / (ref_br * ref_dist * ref_dist);
                    break;
                case DistAttnFn.Steep:
                    k0 = 1.0F;
                    k1 = 0.0F;
                    k2 = (1.0F - ref_br) / (ref_br * ref_dist * ref_dist);
                    break;
                case DistAttnFn.Off:
                default:
                    k0 = 1.0F;
                    k1 = 0.0F;
                    k2 = 0.0F;
                    break;
            }

            return new Vector3(k0, k1, k2);
        }
    }

    public enum LightKeyframeMode
    {
        StartX,
        StartY,
        StartZ,
        EndX,
        EndY,
        EndZ,
        SpotCut,
        SpotBright,
        RefDist,
        RefBright,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LightAnimationFrame
    {
        public static readonly LightAnimationFrame Empty = new LightAnimationFrame();

        public Vector3 Start;
        public Vector3 End;
        public float RefDist;
        public float RefBright;
        public float SpotCutoff;
        public float SpotBright;

        public bool Enabled;

        public bool hasSx;
        public bool hasSy;
        public bool hasSz;

        public bool hasEx;
        public bool hasEy;
        public bool hasEz;

        public bool hasSC;
        public bool hasSB;
        public bool hasRD;
        public bool hasRB;

        public bool HasKeys
        {
            get { return hasSx || hasSy || hasSz || hasEx || hasEy || hasEz || hasSC || hasSB || hasRD || hasRB; }
        }

        public void SetBools(int index, bool val)
        {
            switch (index)
            {
                case 0: hasSx = val; break;
                case 1: hasSy = val; break;
                case 2: hasSz = val; break;
                case 3: hasEx = val; break;
                case 4: hasEy = val; break;
                case 5: hasEz = val; break;
                case 6: hasRD = val; break;
                case 7: hasRB = val; break;
                case 8: hasSC = val; break;
                case 9: hasSB = val; break;
            }
        }

        public void ResetBools()
        {
            hasEx = hasEy = hasEz =
            hasSx = hasSy = hasSz =
            hasSC = hasSB = hasRD = hasRB = false;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Start._x;
                    case 1: return Start._y;
                    case 2: return Start._z;
                    case 3: return End._x;
                    case 4: return End._y;
                    case 5: return End._z;
                    case 6: return RefDist;
                    case 7: return RefBright;
                    case 8: return SpotCutoff;
                    case 9: return SpotBright;
                    default: return float.NaN;
                }
            }
            set
            {
                switch (index)
                {
                    case 0: Start._x = value; break;
                    case 1: Start._y = value; break;
                    case 2: Start._z = value; break;
                    case 3: End._x = value; break;
                    case 4: End._y = value; break;
                    case 5: End._z = value; break;
                    case 6: RefDist = value; break;
                    case 7: RefBright = value; break;
                    case 8: SpotCutoff = value; break;
                    case 9: SpotBright = value; break;
                }
            }
        }

        public LightAnimationFrame(Vector3 start, Vector3 end, float sc, float sb, float rd, float rb, bool enabled)
        {
            Start = start;
            End = end;
            SpotCutoff = sc;
            SpotBright = sb;
            RefDist = rd;
            RefBright = rb;
            Enabled = enabled;
            Index = 0;
            hasSx = hasSy = hasSz = hasEx = hasEy = hasEz = hasSC = hasSB = hasRD = hasRB = false;
        }
        public int Index;
        const int len = 6;
        static string empty = new String('_', len);
        public override string ToString()
        {
            return String.Format("[{0}] Start=({1},{2},{3}), End=({4},{5},{6}), SC={7}, SB={8} RD={9}, RB={10}", (Index + 1).ToString().PadLeft(5),
            !hasSx ? empty : Start._x.ToString().TruncateAndFill(len, ' '),
            !hasSy ? empty : Start._y.ToString().TruncateAndFill(len, ' '),
            !hasSz ? empty : Start._z.ToString().TruncateAndFill(len, ' '),
            !hasEx ? empty : End._x.ToString().TruncateAndFill(len, ' '),
            !hasEy ? empty : End._y.ToString().TruncateAndFill(len, ' '),
            !hasEz ? empty : End._z.ToString().TruncateAndFill(len, ' '),
            !hasSC ? empty : SpotCutoff.ToString().TruncateAndFill(len, ' '),
            !hasSB ? empty : SpotBright.ToString().TruncateAndFill(len, ' '),
            !hasRD ? empty : RefDist.ToString().TruncateAndFill(len, ' '),
            !hasRB ? empty : RefBright.ToString().TruncateAndFill(len, ' '));
        }
    }
}
