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
    public unsafe class SCN0CameraNode : SCN0EntryNode, IKeyframeSource
    {
        internal SCN0Camera* Data { get { return (SCN0Camera*)WorkingUncompressed.Address; } }

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        public SCN0CameraType _type = SCN0CameraType.Aim;
        public ProjectionType _projType;
        public SCN0CameraFlags _flags1 = (SCN0CameraFlags)0xFFFE;
        public ushort _flags2 = 1;

        [Browsable(false)]
        public int FrameCount { get { return Keyframes.FrameLimit; } }

        [Category("Camera")]
        public SCN0CameraType Type { get { return _type; } set { _type = value; SignalPropertyChange(); } }
        [Category("Camera")]
        public ProjectionType ProjectionType { get { return _projType; } set { _projType = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            //Read common header
            base.OnInitialize();

            if (_name == "<null>")
                return false;

            //Read header data
            _flags1 = (SCN0CameraFlags)(ushort)Data->_flags1;
            _flags2 = Data->_flags2;
            _type = (SCN0CameraType)((ushort)_flags2 & 1);
            _projType = (ProjectionType)(int)Data->_projectionType;

            //Read user data
            (_userEntries = new UserDataCollection()).Read(Data->UserData);

            return false;
        }

        internal override void GetStrings(StringTable table)
        {
            if (Name == "<null>")
                return;

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
            //Reset data lengths
            for (int i = 0; i < 3; i++)
                _dataLengths[i] = 0;

            int size = SCN0Camera.Size;

            if (_name != "<null>")
            {
                //Get the total data size of all keyframes
                for (int i = 0; i < 15; i++)
                    CalcKeyLen(Keyframes[i]);

                //Add the size of the user entries
                size += _userEntries.GetSize();
            }

            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);

            if (_name == "<null>")
                return;

            SCN0Camera* header = (SCN0Camera*)address;

            header->_projectionType = (int)_projType;
            header->_flags2 = (ushort)(2 + (int)_type);
            header->_userDataOffset = 0;

            int newFlags1 = 0;

            for (int i = 0; i < 15; i++)
                _dataAddrs[0] += EncodeKeyframes(
                    Keyframes[i],
                    _dataAddrs[0],
                    header->_position._x.Address + i * 4,
                    ref newFlags1,
                    (int)Ordered[i]);

            header->_flags1 = (ushort)newFlags1;

            if (_userEntries.Count > 0)
                _userEntries.Write(header->UserData = (VoidPtr)header + SCN0Camera.Size);
        }

        protected internal override void PostProcess(VoidPtr scn0Address, VoidPtr dataAddress, StringTable stringTable)
        {
            base.PostProcess(scn0Address, dataAddress, stringTable);

            if (_name != "<null>")
                _userEntries.PostProcess(((SCN0Camera*)dataAddress)->UserData, stringTable);
        }

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

        public static bool _generateTangents = true;

        public CameraAnimationFrame GetAnimFrame(int index)
        {
            CameraAnimationFrame frame;
            float* dPtr = (float*)&frame;
            for (int x = 0; x < 15; x++)
            {
                KeyframeArray a = Keyframes[x];
                *dPtr++ = a.GetFrameValue(index);
                frame.SetBools(x, a.GetKeyframe((int)index) != null);
                frame.Index = index;
            }
            return frame;
        }

        internal KeyframeEntry GetKeyframe(CameraKeyframeMode keyFrameMode, int index)
        {
            return Keyframes[(int)keyFrameMode].GetKeyframe(index);
        }

        public float GetFrameValue(CameraKeyframeMode keyFrameMode, float index)
        {
            return Keyframes[(int)keyFrameMode].GetFrameValue(index);
        }

        internal void RemoveKeyframe(CameraKeyframeMode keyFrameMode, int index)
        {
            KeyframeEntry k = Keyframes[(int)keyFrameMode].Remove(index);
            if (k != null && _generateTangents)
            {
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
                SignalPropertyChange();
            }
        }

        internal void SetKeyframe(CameraKeyframeMode keyFrameMode, int index, float value)
        {
            KeyframeArray keys = Keyframes[(int)keyFrameMode];
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

        [Browsable(false)]
        public KeyframeArray Posx { get { return Keyframes[0]; } }
        [Browsable(false)]
        public KeyframeArray PosY { get { return Keyframes[1]; } }
        [Browsable(false)]
        public KeyframeArray PosZ { get { return Keyframes[2]; } }
        [Browsable(false)]
        public KeyframeArray RotX { get { return Keyframes[3]; } }
        [Browsable(false)]
        public KeyframeArray RotY { get { return Keyframes[4]; } }
        [Browsable(false)]
        public KeyframeArray RotZ { get { return Keyframes[5]; } }
        [Browsable(false)]
        public KeyframeArray AimX { get { return Keyframes[6]; } }
        [Browsable(false)]
        public KeyframeArray AimY { get { return Keyframes[7]; } }
        [Browsable(false)]
        public KeyframeArray AimZ { get { return Keyframes[8]; } }
        [Browsable(false)]
        public KeyframeArray Twist { get { return Keyframes[9]; } }
        [Browsable(false)]
        public KeyframeArray FovY { get { return Keyframes[10]; } }
        [Browsable(false)]
        public KeyframeArray Height { get { return Keyframes[11]; } }
        [Browsable(false)]
        public KeyframeArray Aspect { get { return Keyframes[12]; } }
        [Browsable(false)]
        public KeyframeArray NearZ { get { return Keyframes[13]; } }
        [Browsable(false)]
        public KeyframeArray FarZ { get { return Keyframes[14]; } }

        private KeyframeCollection _keyframes = null;
        [Browsable(false)]
        public KeyframeCollection Keyframes
        {
            get
            {
                if (_keyframes == null)
                {
                    _keyframes = new KeyframeCollection(15, Scene.FrameCount + (Scene.Loop ? 1 : 0));
                    if (Data != null && Name != "<null>")
                        for (int i = 0; i < 15; i++)
                            DecodeKeyframes(
                                Keyframes[i],
                                Data->_position._x.Address + i * 4,
                                (int)_flags1,
                                (int)Ordered[i]);
                }
                return _keyframes;
            }
        }

        [Browsable(false)]
        public KeyframeArray[] KeyArrays { get { return Keyframes._keyArrays; } }

        internal void SetSize(int numFrames, bool looped)
        {
            Keyframes.FrameLimit = numFrames + (looped ? 1 : 0);
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
}
