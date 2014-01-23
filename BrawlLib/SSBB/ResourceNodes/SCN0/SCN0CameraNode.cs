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
    public unsafe class SCN0CameraNode : SCN0EntryNode, ISCN0KeyframeHolder
    {
        internal SCN0Camera* Data { get { return (SCN0Camera*)WorkingUncompressed.Address; } }

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        public SCN0CameraType _type = SCN0CameraType.Aim;
        public ProjectionType _projType;
        public SCN0CameraFlags _flags1 = (SCN0CameraFlags)0xFFFE;
        public ushort _flags2 = 1;

        public KeyframeArray 
            _posX = new KeyframeArray(0), 
            _posY = new KeyframeArray(0), 
            _posZ = new KeyframeArray(0), 
            _rotX = new KeyframeArray(0), 
            _rotY = new KeyframeArray(0), 
            _rotZ = new KeyframeArray(0), 
            _aimX = new KeyframeArray(0), 
            _aimY = new KeyframeArray(0), 
            _aimZ = new KeyframeArray(0), 
            _twist = new KeyframeArray(0), 
            _fovY = new KeyframeArray(0), 
            _height = new KeyframeArray(0), 
            _aspect = new KeyframeArray(0), 
            _nearZ = new KeyframeArray(0), 
            _farZ = new KeyframeArray(0);

        #region ISCN0KeyframeHolder Members

        public int KeyArrayCount { get { return 15; } }
        public KeyframeArray GetKeys(int i)
        {
            switch (i)
            {
                case 0: return _posX;
                case 1: return _posY;
                case 2: return _posZ;
                case 3: return _aspect;
                case 4: return _nearZ;
                case 5: return _farZ;
                case 6: return _rotX;
                case 7: return _rotY;
                case 8: return _rotZ;
                case 9: return _aimX;
                case 10: return _aimY;
                case 11: return _aimZ;
                case 12: return _twist;
                case 13: return _fovY;
                case 14: return _height;
            }
            return null;
        }

        public void SetKeys(int i, KeyframeArray value)
        {
            switch (i)
            {
                case 0: _posX = value; break;
                case 1: _posY = value; break;
                case 2: _posZ = value; break;
                case 3: _aspect = value; break;
                case 4: _nearZ = value; break;
                case 5: _farZ = value; break;
                case 6: _rotX = value; break;
                case 7: _rotY = value; break;
                case 8: _rotZ = value; break;
                case 9: _aimX = value; break;
                case 10: _aimY = value; break;
                case 11: _aimZ = value; break;
                case 12: _twist = value; break;
                case 13: _fovY = value; break;
                case 14: _height = value; break;
            }
        }
        #endregion

        [Category("Camera")]
        public SCN0CameraType Type { get { return _type; } set { _type = value; SignalPropertyChange(); } }
        [Category("Camera")]
        public ProjectionType ProjectionType { get { return _projType; } set { _projType = value; SignalPropertyChange(); } }

        public SCN0CameraFlags[] Ordered = new SCN0CameraFlags[] 
        {
            SCN0CameraFlags.PosXConstant,
            SCN0CameraFlags.PosYConstant,
            SCN0CameraFlags.PosZConstant,
            SCN0CameraFlags.AspectConstant,
            SCN0CameraFlags.NearConstant,
            SCN0CameraFlags.FarConstant,
            SCN0CameraFlags.RotXConstant,
            SCN0CameraFlags.RotYConstant,
            SCN0CameraFlags.RotZConstant,
            SCN0CameraFlags.AimXConstant,
            SCN0CameraFlags.AimYConstant,
            SCN0CameraFlags.AimZConstant,
            SCN0CameraFlags.TwistConstant,
            SCN0CameraFlags.PerspFovYConstant,
            SCN0CameraFlags.OrthoHeightConstant,
        };

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _flags1 = (SCN0CameraFlags)(ushort)Data->_flags1;
            _flags2 = Data->_flags2;
            _type = (SCN0CameraType)((ushort)_flags2 & 1);
            _projType = (ProjectionType)(int)Data->_projType;

            for (int x = 0; x < 15; x++)
                SetKeys(x, new KeyframeArray(FrameCount + 1));

            bint* values = (bint*)&Data->_position;

            if (Name != "<null>")
                for (int i = 0; i < 15; i++)
                {
                    //if (((int)_flags1 & (int)Ordered[i]) == 0)
                    //    SCN0Node.strings[(int)((&values[i] - Parent.Parent.WorkingUncompressed.Address + values[i]))] = "Camera" + Index + " Keys " + Ordered[i].ToString();

                    DecodeFrames(GetKeys(i), &values[i], (int)_flags1, (int)Ordered[i]);
                }

            _posX._linear = true;
            _posY._linear = true;
            _posZ._linear = true;

            _aimX._linear = true;
            _aimY._linear = true;
            _aimZ._linear = true;

            (_userEntries = new UserDataCollection()).Read(Data->UserData);

            return false;
        }

        internal override void GetStrings(StringTable table)
        {
            if (Name != "<null>") 
                table.Add(Name);

            foreach (UserDataClass s in _userEntries)
            {
                table.Add(s._name);
                if (s._type == UserValueType.String && s._entries.Count > 0)
                    table.Add(s._entries[0]);
            }
        }

        public override int OnCalculateSize(bool force)
        {
            _lightLen = 0;
            _keyLen = 0;
            _visLen = 0;
            if (_name != "<null>")
                for (int i = 0; i < 15; i++)
                    if (GetKeys(i)._keyCount > 1)
                        _keyLen += 8 + GetKeys(i)._keyCount * 12;
            return SCN0Camera.Size + _userEntries.GetSize();
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);

            SCN0Camera* header = (SCN0Camera*)address;

            header->_projType = (int)_projType;
            header->_flags2 = (ushort)(2 + (int)_type);
            header->_userDataOffset = 0;

            int newFlags1 = 0;

            bint* values = (bint*)&header->_position;
            for (int i = 0; i < 15; i++)
                EncodeFrames(GetKeys(i), ref keyframeAddr, &values[i], ref newFlags1, (int)Ordered[i]);

            if (_userEntries.Count > 0)
                _userEntries.Write(header->UserData = (VoidPtr)header + SCN0Camera.Size);

            header->_flags1 = (ushort)newFlags1;
        }

        protected internal override void PostProcess(VoidPtr scn0Address, VoidPtr dataAddress, StringTable stringTable)
        {
            base.PostProcess(scn0Address, dataAddress, stringTable);

            _userEntries.PostProcess(((SCN0Camera*)dataAddress)->UserData, stringTable);
        }

        [Browsable(false)]
        public int FrameCount { get { return ((SCN0Node)Parent.Parent).FrameCount; } }

        public static bool _generateTangents = true;
        public static bool _linear = true;

        public CameraAnimationFrame GetAnimFrame(int index)
        {
            CameraAnimationFrame frame;
            float* dPtr = (float*)&frame;
            for (int x = 0; x < 15; x++)
            {
                KeyframeArray a = GetKeys(x);
                *dPtr++ = a.GetFrameValue(index);
                frame.SetBools(x, a.GetKeyframe(index) != null);
                frame.Index = index;
            }
            return frame;
        }
        internal KeyframeEntry GetKeyframe(CameraKeyframeMode keyFrameMode, int index)
        {
            return GetKeys((int)keyFrameMode).GetKeyframe(index);
        }

        public float GetFrameValue(CameraKeyframeMode keyFrameMode, int index)
        {
            return GetKeys((int)keyFrameMode).GetFrameValue(index);
        }

        internal void RemoveKeyframe(CameraKeyframeMode keyFrameMode, int index)
        {
            KeyframeEntry k = GetKeys((int)keyFrameMode).Remove(index);
            if (k != null && _generateTangents)
            {
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
                SignalPropertyChange();
            }
        }
        
        internal void SetKeyframe(CameraKeyframeMode keyFrameMode, int index, float value)
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
    }

    public enum CameraKeyframeMode
    {
        PosX,
        PosY,
        PosZ,
        Aspect,
        NearZ,
        FarZ,
        RotX,
        RotY,
        RotZ,
        AimX,
        AimY,
        AimZ,
        Twist,
        FovY,
        Height,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CameraAnimationFrame
    {
        public static readonly CameraAnimationFrame Empty = new CameraAnimationFrame();

        public Vector3 Pos;
        public float Aspect;
        public float NearZ;
        public float FarZ;
        public Vector3 Rot;
        public Vector3 Aim;
        public float Twist;
        public float FovY;
        public float Height;

        public bool hasPx;
        public bool hasPy;
        public bool hasPz;

        public bool hasRx;
        public bool hasRy;
        public bool hasRz;

        public bool hasAx;
        public bool hasAy;
        public bool hasAz;

        public bool hasT;
        public bool hasF;
        public bool hasH;
        public bool hasA;
        public bool hasNz;
        public bool hasFz;

        public bool HasKeys
        {
            get { return hasPx || hasPy || hasPz || hasRx || hasRy || hasRz || hasAx || hasAy || hasAz || hasT || hasF || hasH || hasA || hasNz || hasFz; }
        }

        public void SetBools(int index, bool val)
        {
            switch (index)
            {
                case 0: hasPx = val; break;
                case 1: hasPy = val; break;
                case 2: hasPz = val; break;
                case 3: hasA = val; break;
                case 4: hasNz = val; break;
                case 5: hasFz = val; break;
                case 6: hasRx = val; break;
                case 7: hasRy = val; break;
                case 8: hasRz = val; break;
                case 9: hasAx = val; break;
                case 10: hasAy = val; break;
                case 11: hasAz = val; break;
                case 12: hasT = val; break;
                case 13: hasF = val; break;
                case 14: hasH = val; break;
            }
        }

        public void ResetBools()
        {
            hasRx = hasRy = hasRz =
            hasPx = hasPy = hasPz =
            hasAx = hasAy = hasAz =
            hasT = hasF = hasH = 
            hasA = hasNz = hasFz = false;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Pos._x;
                    case 1: return Pos._y;
                    case 2: return Pos._z;
                    case 3: return Aspect;
                    case 4: return NearZ;
                    case 5: return FarZ;
                    case 6: return Rot._x;
                    case 7: return Rot._y;
                    case 8: return Rot._z;
                    case 9: return Aim._x;
                    case 10: return Aim._y;
                    case 11: return Aim._z;
                    case 12: return Twist;
                    case 13: return FovY;
                    case 14: return Height;

                    default: return float.NaN;
                }
            }
            set
            {
                switch (index)
                {
                    case 0: Pos._x = value; break;
                    case 1: Pos._y = value; break;
                    case 2: Pos._z = value; break;
                    case 3: Aspect = value; break;
                    case 4: NearZ = value; break;
                    case 5: FarZ = value; break;
                    case 6: Rot._x = value; break;
                    case 7: Rot._y = value; break;
                    case 8: Rot._z = value; break;
                    case 9: Aim._x = value; break;
                    case 10: Aim._y = value; break;
                    case 11: Aim._z = value; break;
                    case 12: Twist = value; break;
                    case 13: FovY = value; break;
                    case 14: Height = value; break;
                }
            }
        }

        public Vector3 GetRotate(int frame, SCN0CameraType type)
        {
            if (type == SCN0CameraType.Rotate)
                return Rot;
            else //Aim - calculate rotation facing the position
            {
                Matrix m = Matrix.ReverseLookat(Aim, Pos, Twist);
                Vector3 a = m.GetAngles();
                return new Vector3(-a._x, -a._y, -a._z);
            }
        }

        public CameraAnimationFrame(Vector3 pos, Vector3 rot, Vector3 aim, float t, float f, float h, float a, float nz, float fz)
        {
            Pos = pos;
            Rot = rot;
            Aim = aim;
            Twist = t;
            FovY = f;
            Height = h;
            Aspect = a;
            NearZ = nz;
            FarZ = fz;
            Index = 0;
            hasRx = hasRy = hasRz =
            hasPx = hasPy = hasPz =
            hasAx = hasAy = hasAz =
            hasT = hasF = hasH =
            hasA = hasNz = hasFz = false;
        }
        public int Index;
        const int len = 6;
        static string empty = new String('_', len);
        public override string ToString()
        {
            return String.Format("[{0}] Pos=({1},{2},{3}), Rot=({4},{5},{6}), Aim=({7},{8},{9}), Twist={10}, FovY={11}, Height={12}, Aspect={13}, NearZ={14}, FarZ={15}", (Index + 1).ToString().PadLeft(5),
            !hasPx ? empty : Pos._x.ToString().TruncateAndFill(len, ' '),
            !hasPy ? empty : Pos._y.ToString().TruncateAndFill(len, ' '),
            !hasPz ? empty : Pos._z.ToString().TruncateAndFill(len, ' '),
            !hasRx ? empty : Rot._x.ToString().TruncateAndFill(len, ' '),
            !hasRy ? empty : Rot._y.ToString().TruncateAndFill(len, ' '),
            !hasRz ? empty : Rot._z.ToString().TruncateAndFill(len, ' '),
            !hasAx ? empty : Aim._x.ToString().TruncateAndFill(len, ' '),
            !hasAy ? empty : Aim._y.ToString().TruncateAndFill(len, ' '),
            !hasAz ? empty : Aim._z.ToString().TruncateAndFill(len, ' '),
            !hasT ? empty : Twist.ToString().TruncateAndFill(len, ' '),
            !hasF ? empty : FovY.ToString().TruncateAndFill(len, ' '),
            !hasH ? empty : Height.ToString().TruncateAndFill(len, ' '),
            !hasA ? empty : Aspect.ToString().TruncateAndFill(len, ' '),
            !hasNz ? empty : NearZ.ToString().TruncateAndFill(len, ' '),
            !hasFz ? empty : FarZ.ToString().TruncateAndFill(len, ' '));
        }
    }
}
