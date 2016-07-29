using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using System.Windows.Forms;
using BrawlLib.Modeling;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class OMONode : NW4RAnimationNode
    {
        public static VBNNode _skeleton;
        public string Skeleton { get { return _skeleton == null ? "" : _skeleton.Name; } }

        internal OMOHeader* Header { get { return (OMOHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.OMO; } }
        public override Type[] AllowedChildTypes { get { return new Type[] { typeof(OMOBoneEntryNode) }; } }

        public OMONode() { }

        public int _frameSize;

        public string Unknown1
        {
            get { return "0x" + _unk1.ToString("X8"); }
            set
            {
                string val = value;
                if (val.StartsWith("0x"))
                    val = val.Substring(2);
                if (uint.TryParse(val, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out _unk1))
                    SignalPropertyChange();
            }
        }
        public ushort Unknown2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }

        uint _unk1;
        ushort _unk2;

        public override bool OnInitialize()
        {
            _name = _origPath;
            _numFrames = Header->_frameCount;
            _frameSize = Header->_frameSize;
            _unk1 = Header->_unk1;
            _unk2 = Header->_unk2;
            return Header->_boneCount > 0;
        }

        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_boneCount; ++i)
            {
                OMOBoneEntry* entry = (OMOBoneEntry*)((VoidPtr)Header + Header->_boneTableOffset + i * 0x10);
                int fixedOff = Header->_fixedDataOffset;
                int frameOff = Header->_frameDataOffset;
                int offsetInFixed = entry->_offsetInFixedData;
                int offsetInFrame = entry->_offsetInFrame;
                int nextOffsetInFixed, nextOffsetInFrame;
                if (i + 1 != Header->_boneCount)
                {
                    OMOBoneEntry* nextEntry = (OMOBoneEntry*)((VoidPtr)entry + 0x10);
                    nextOffsetInFixed = nextEntry->_offsetInFixedData;
                    nextOffsetInFrame = nextEntry->_offsetInFrame;
                }
                else
                {
                    nextOffsetInFixed = frameOff - Header->_fixedDataOffset;
                    nextOffsetInFrame = Header->_frameSize;
                }

                int fixedDataSize = (int)(nextOffsetInFixed - offsetInFixed);
                int frameDataSize = (int)(nextOffsetInFrame - offsetInFrame);

                BVec3* fixedData = (BVec3*)(Header->FixedData + offsetInFixed);
                VoidPtr fixedDataStart = fixedData;

                Vector3 transMin = Vector3.Zero, rotMin = Vector3.Zero, scaleMin = Vector3.One;
                Vector3 transRange = Vector3.Zero, rotRange = Vector3.Zero, scaleRange = Vector3.Zero;
                Vector4 constQuat;

                bool constant = entry->RotationFlags.HasFlag(OMORotType.Fixed);
                bool euler = entry->RotationFlags.HasFlag(OMORotType.Euler);
                bool quat = entry->RotationFlags.HasFlag(OMORotType.Quaternion);
                bool frame = entry->RotationFlags.HasFlag(OMORotType.InFrame);

                #region Fixed data
                if (entry->HasTranslation && (entry->TranslationConstant || entry->TranslationAnimated))
                {
                    transMin = *fixedData++;
                    if (entry->TranslationAnimated && !entry->TranslationConstant)
                        transRange = *fixedData++;
                }
                if (entry->HasRotation)
                {
                    if (constant || euler)
                    {
                        rotMin = *fixedData++;
                        if (euler && !constant)
                            rotRange = *fixedData++;
                    }
                    else if (quat)
                    {
                        //MessageBox.Show("Unknown Rot Type");
                        constQuat = *(BVec4*)fixedData;
                        fixedData = (BVec3*)((VoidPtr)fixedData + 16);
                    }
                }
                if (entry->HasScale && (entry->ScaleConstant || entry->ScaleAnimated))
                {
                    scaleMin = *fixedData++;
                    if (entry->ScaleAnimated && !entry->ScaleConstant)
                        scaleRange = *fixedData++;
                }

                int diff = (int)fixedData - (int)fixedDataStart;
                if (diff != fixedDataSize)
                    MessageBox.Show("Fixed data length mismatch");

                #endregion

                if (entry->HasTranslation && entry->TranslationFrame)
                    MessageBox.Show("Trans Frame");
                if (entry->HasRotation && frame)
                    MessageBox.Show("Rot Frame");
                if (entry->HasScale && entry->ScaleFrame)
                    MessageBox.Show("Scale Frame");

                #region Frame data
                FrameState[] states = new FrameState[_numFrames];
                for (int x = 0; x < _numFrames; x++)
                {
                    bushort* frameData = (bushort*)(Header->GetFrameAddr(x) + offsetInFrame);
                    VoidPtr frameDataStart = frameData;

                    FrameState state = FrameState.Neutral;
                    Vector3 interp = Vector3.Zero;

                    if (entry->HasTranslation)
                    {
                        if (entry->TranslationFrame)
                        {
                            //state._translate = *(BVec3*)frameData;
                            //frameData += 6;
                        }
                        else
                        {
                            if (entry->TranslationAnimated)
                                interp = new Vector3(*frameData++, *frameData++, *frameData++) / 0xFFFF;

                            state._translate = transMin + transRange * interp;
                        }
                    }
                    if (entry->HasRotation)
                    {
                        if (frame)
                        {
                            
                        }
                        else if (constant || euler)
                        {
                            bool isAnimated = euler && !constant;

                            if (isAnimated)
                                interp = new Vector3(*frameData++, *frameData++, *frameData++) / 0xFFFF;

                            Vector3 v = rotMin + rotRange * interp;

                            if (isAnimated)
                            {
                                float w = (float)Math.Sqrt(1.0f - (v._x * v._x + v._y * v._y + v._z * v._z));

                                float n;
                                float xs, ys, zs;
                                float wx, wy, wz;
                                float xx, xy, xz;
                                float yy, yz, zz;

                                n = (v._x * v._x) + (v._y * v._y) + (v._z * v._z) + (w * w);
                                n = (n > 0.0f) ? (2.0f / n) : 0.0f;

                                xs = v._x * n; ys = v._y * n; zs = v._z * n;
                                wx = (float)(w * xs); wy = (float)(w * ys); wz = (float)(w * zs);
                                xx = v._x * xs; xy = v._x * ys; xz = v._x * zs;
                                yy = v._y * ys; yz = v._y * zs; zz = v._z * zs;

                                v._x = (float)Math.Atan2(yz + wx, 1.0f - (xx + yy));
                                v._y = (float)Math.Atan2(-(xz - wy), Math.Sqrt((yz + wx) * (yz + wx) + (1.0f - (xx + yy)) * (1.0f - (xx + yy))));
                                v._z = (float)Math.Atan2((xy + wz), 1.0f - (yy + zz));

                                //v = new Vector4(v, w).ToEuler(Vector4.RotSeq.zyx);
                            }

                            state._rotate = v * Maths._rad2degf;
                        }
                        else if (quat)
                        {
                            Vector4 interpQ = new Vector4((float)(ushort)(*frameData++) / 0xFFFF);

                        }
                    }
                    if (entry->HasScale)
                    {
                        if (entry->ScaleFrame)
                        {
                            //state._scale = *(BVec3*)frameData;
                            //frameData += 6;
                        }
                        else
                        {
                            if (entry->ScaleAnimated)
                                interp = new Vector3(*frameData++, *frameData++, *frameData++) / 0xFFFF;

                            state._scale = scaleMin + scaleRange * interp;
                        }
                    }
                    state.CalcTransforms();
                    states[x] = state;

                    diff = (int)frameData - (int)frameDataStart;
                    if (diff != frameDataSize)
                        MessageBox.Show("Frame data length mismatch");
                }
                #endregion

                new OMOBoneEntryNode() { _frameStates = states }.Initialize(this, (VoidPtr)Header + Header->_boneTableOffset + i * 0x10, 0x10);
            }
        }

        internal override void GetStrings(StringTable strings)
        {
            
        }
        protected internal override void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {

        }

        int _rebuildFrameSize, _rebuildFixedSize;
        public override int OnCalculateSize(bool force)
        {
            _rebuildFrameSize = 0;

            int size = OMOHeader.Size + Children.Count * OMOBoneEntry.Size;
            foreach (OMOBoneEntryNode b in Children)
            {
                b._rebuildScaleMin = new Vector3(float.MaxValue);
                b._rebuildRotMin = new Vector3(float.MaxValue);
                b._rebuildTransMin = new Vector3(float.MaxValue);

                Vector3 scaleMax = new Vector3(float.MinValue);
                Vector3 rotMax = new Vector3(float.MinValue);
                Vector3 transMax = new Vector3(float.MinValue);

                foreach (FrameState s in b.FrameStates)
                {
                    scaleMax.Max(s._scale);
                    rotMax.Max(s._rotate);
                    transMax.Max(s._translate);

                    b._rebuildScaleMin.Min(s._scale);
                    b._rebuildRotMin.Min(s._rotate);
                    b._rebuildTransMin.Min(s._translate);
                }

                b._rebuildScaleRange = scaleMax - b._rebuildScaleMin;
                b._rebuildRotRange = rotMax - b._rebuildRotMin;
                b._rebuildTransRange = transMax - b._rebuildTransMin;

                _rebuildFixedSize += b.GetFixedSize();
                _rebuildFrameSize += b.GetFrameSize();
            }
            return size + _rebuildFixedSize + _rebuildFrameSize * _numFrames;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            OMOHeader* hdr = (OMOHeader*)address;
            hdr->_tag = OMOHeader.Tag;
            hdr->_versionMax = 1;
            hdr->_versionMin = 3;
            hdr->_unk1 = _unk1;
            hdr->_unk2 = _unk2;
            hdr->_frameCount = (ushort)_numFrames;
            hdr->_frameSize = (ushort)_rebuildFrameSize;
            hdr->_boneCount = (ushort)Children.Count;
            hdr->_boneTableOffset = OMOHeader.Size;
            hdr->_fixedDataOffset = OMOHeader.Size + Children.Count * OMOBoneEntry.Size;
            hdr->_frameDataOffset = hdr->_fixedDataOffset + _rebuildFixedSize;

            OMOBoneEntry* entries = (OMOBoneEntry*)(address + hdr->_boneTableOffset);
            BVec3* fixedData = (BVec3*)hdr->FixedData;
            bushort* frameData = (bushort*)hdr->GetFrameAddr(0);
            int frameOffset = 0;

            foreach (OMOBoneEntryNode b in Children)
            {
                entries->_boneHash = b._boneHash;
                entries->_flags = b._flags;
                entries->_offsetInFixedData = fixedData - hdr->FixedData;
                entries->_offsetInFrame = frameOffset;
                entries++;

                frameOffset += b._rebuildFrameSize;

                bool constant = b.RotationFlags.HasFlag(OMORotType.Fixed);
                bool euler = b.RotationFlags.HasFlag(OMORotType.Euler);
                bool quat = b.RotationFlags.HasFlag(OMORotType.Quaternion);
                bool frame = b.RotationFlags.HasFlag(OMORotType.InFrame);

                if (b.HasTranslation)
                {
                    if (b.TranslationFrame)
                    {

                    }
                    else if (b.TranslationConstant || b.TranslationAnimated)
                    {
                        *fixedData++ = b._rebuildTransMin;
                        if (!b.TranslationConstant && b.TranslationAnimated)
                            *fixedData++ = b._rebuildTransRange;
                    }
                }
                if (b.HasRotation)
                {
                    if (frame)
                    {

                    }
                    else if (constant || euler)
                    {
                        *fixedData++ = b._rebuildRotMin;
                        if (euler && !constant)
                            *fixedData++ = b._rebuildRotRange;
                    }
                    else if (quat)
                    {

                    }
                }
                if (b.HasScale)
                {
                    if (b.ScaleFrame)
                    {

                    }
                    else if (b.ScaleConstant || b.ScaleAnimated)
                    {
                        *fixedData++ = b._rebuildScaleMin;
                        if (!b.ScaleConstant && b.ScaleAnimated)
                            *fixedData++ = b._rebuildScaleRange;
                    }
                }

                for (int i = 0; i < _numFrames; ++i)
                {
                    FrameState state = b.FrameStates[i];
                    if (b.HasTranslation)
                    {
                        if (b.TranslationFrame)
                        {
                            *(BVec3*)frameData = state._translate;
                            frameData += 6;
                        }
                        else if (!b.TranslationConstant && b.TranslationAnimated)
                        {
                            Vector3 interp = ((state._translate - b._rebuildTransMin) / b._rebuildTransRange) * 0xFFFF;
                            *frameData++ = (ushort)(int)(interp._x + 0.5f);
                            *frameData++ = (ushort)(int)(interp._y + 0.5f);
                            *frameData++ = (ushort)(int)(interp._z + 0.5f);
                        }
                    }
                    if (b.HasRotation)
                    {
                        if (frame)
                        {

                        }
                        else if (euler && !constant)
                        {
                            Vector4 q = (state._rotate * Maths._deg2radf).ToQuat();
                            Vector3 v = new Vector3(q._x, q._y, q._z);

                            Vector3 interp = ((v - b._rebuildRotMin) / b._rebuildRotRange) * 0xFFFF;
                            *frameData++ = (ushort)(int)(interp._x + 0.5f);
                            *frameData++ = (ushort)(int)(interp._y + 0.5f);
                            *frameData++ = (ushort)(int)(interp._z + 0.5f);
                        }
                        else if (quat)
                        {
                            //Vector4 interpQ = new Vector4((float)(ushort)(*frameData++) / 0xFFFF);

                        }
                    }
                    if (b.HasScale)
                    {
                        if (b.ScaleFrame)
                        {
                            *(BVec3*)frameData = state._scale;
                            frameData += 6;
                        }
                        else if (!b.ScaleConstant && b.ScaleAnimated)
                        {
                            Vector3 interp = ((state._scale - b._rebuildScaleMin) / b._rebuildScaleRange) * 0xFFFF;
                            *frameData++ = (ushort)(int)(interp._x + 0.5f);
                            *frameData++ = (ushort)(int)(interp._y + 0.5f);
                            *frameData++ = (ushort)(int)(interp._z + 0.5f);
                        }
                    }
                }
            }
        }

        public CHR0Node ToCHR0()
        {
            CHR0Node node = new CHR0Node();
            node.Name = Name;
            node.FrameCount = _numFrames;
            foreach (OMOBoneEntryNode b in Children)
            {
                CHR0EntryNode c = new CHR0EntryNode();
                c._keyframes = new KeyframeCollection(9, _numFrames, 1.0f, 1.0f, 1.0f);
                c.Keyframes.FrameLimit = _numFrames;
                c.Name = b.Name;

                bool[] exclude = new bool[9] { true, true, true, true, true, true, true, true, true };
                if (b.HasScale && 
                    !b.ScaleConstant && 
                    b.ScaleAnimated)
                {
                    exclude[0] = false;
                    exclude[1] = false;
                    exclude[2] = false;
                }
                if (b.HasRotation && 
                    !b.RotationFlags.HasFlag(OMORotType.Fixed) &&
                    (b.RotationFlags.HasFlag(OMORotType.InFrame) || 
                    b.RotationFlags.HasFlag(OMORotType.Euler) ||
                    b.RotationFlags.HasFlag(OMORotType.Quaternion)))
                {
                    exclude[3] = false;
                    exclude[4] = false;
                    exclude[5] = false;
                }
                if (b.HasTranslation && 
                    !b.TranslationConstant && 
                    b.ScaleAnimated)
                {
                    exclude[6] = false;
                    exclude[7] = false;
                    exclude[8] = false;
                }

                for (int i = 0; i < _numFrames; ++i)
                    fixed (FrameState* addr = &b.FrameStates[i])
                    {
                        float* values = (float*)addr;
                        for (int x = 0; x < 9; ++x)
                            if (!exclude[x])
                                c.Keyframes.SetFrameValue(x, i, values[x]);
                    }

                fixed (FrameState* addr = &b.FrameStates[0])
                {
                    float* values = (float*)addr;
                    for (int x = 0; x < 9; ++x)
                        if (exclude[x])
                            c.Keyframes.SetFrameValue(x, 0, values[x]);
                }

                node.AddChild(c);
            }
            return node;
        }
        public void FromCHR0(CHR0Node n)
        {
            Name = n.Name;
            _numFrames = n._numFrames;
            while (Children.Count > 0)
                Children[0].Remove();
            foreach (CHR0EntryNode e in n.Children)
            {
                OMOBoneEntryNode b = new OMOBoneEntryNode();
                b._frameStates = new FrameState[_numFrames];
                b.Name = e.Name;
                for (int i = 0; i < _numFrames; ++i)
                {
                    fixed (FrameState* addr = &b.FrameStates[i])
                    {
                        float* values = (float*)addr;
                        for (int x = 0; x < 9; ++x)
                            values[x] = e.Keyframes.GetFrameValue(x, i);
                    }
                }
                AddChild(b);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        internal static ResourceNode TryParse(DataSource source) { return *(BinTag*)source.Address == OMOHeader.Tag ? new OMONode() : null; }
    }

    public unsafe class OMOBoneEntryNode : ResourceNode
    {
        internal OMOBoneEntry* Header { get { return (OMOBoneEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        internal OMONode ParentOMO { get { return (OMONode)Parent; } }

        public Vector3 _rebuildScaleMin, _rebuildRotMin, _rebuildTransMin;
        public Vector3 _rebuildScaleRange, _rebuildRotRange, _rebuildTransRange;

        public uint _boneHash;
        public Bin32 _flags;
        OMOBoneEntry _hdr;

        const string _category = "Bone Entry";

        [Category(_category)]
        public string BoneHash
        {
            get { return _boneHash.ToString("X8"); }
        }
        [Category(_category), TypeConverter(typeof(Bin32StringConverter))]
        public Bin32 Flags
        {
            get { return _flags; }
        }
        [Category("Flags")]
        public bool HasScale
        {
            get { return _hdr.HasScale; }
        }
        [Category("Flags")]
        public bool HasRotation
        {
            get { return _hdr.HasRotation; }
        }
        [Category("Flags")]
        public bool HasTranslation
        {
            get { return _hdr.HasTranslation; }
        }
        [Category("Flags")]
        public bool TranslationConstant
        {
            get { return _hdr.TranslationConstant; }
        }
        [Category("Flags")]
        public bool TranslationAnimated
        {
            get { return _hdr.TranslationAnimated; }
        }
        [Category("Flags")]
        public bool TranslationFrame
        {
            get { return _hdr.TranslationFrame; }
        }
        [Category("Flags")]
        public OMORotType RotationFlags
        {
            get { return _hdr.RotationFlags; }
        }
        [Category("Flags")]
        public bool ScaleConstant
        {
            get { return _hdr.ScaleConstant; }
        }
        [Category("Flags")]
        public bool ScaleAnimated
        {
            get { return _hdr.ScaleAnimated; }
        }
        [Category("Flags")]
        public bool ScaleFrame
        {
            get { return _hdr.ScaleFrame; }
        }
        [Category("Flags")]
        public bool AlwaysOn
        {
            get { return _hdr.AlwaysOn; }
        }

        public override bool OnInitialize()
        {
            _boneHash = Header->_boneHash;
            _flags = Header->_flags;
            _hdr = *Header;

            _name = _boneHash.ToString("X8");
            if (OMONode._skeleton != null)
            {
                foreach (VBNBoneNode b in OMONode._skeleton.BoneCache)
                {
                    if (b._hash == _boneHash)
                    {
                        _name = b.Name;
                        break;
                    }
                }
            }
            
            return false;
        }

        public int _rebuildFrameSize;
        internal int GetFrameSize()
        {
            int size = 0;
            if (HasTranslation)
            {
                if (TranslationFrame)
                    size += 12;
                else if (!TranslationConstant && TranslationAnimated)
                    size += 6;
            }
            if (HasRotation)
            {
                bool constant = RotationFlags.HasFlag(OMORotType.Fixed);
                bool euler = RotationFlags.HasFlag(OMORotType.Euler);
                bool quat = RotationFlags.HasFlag(OMORotType.Quaternion);
                bool frame = RotationFlags.HasFlag(OMORotType.InFrame);

                if (frame)
                {

                }
                else if (!constant)
                {
                    if (euler)
                        size += 6;
                    else if (quat)
                        size += 2;
                }
            }
            if (HasScale)
            {
                if (ScaleFrame)
                    size += 12;
                else if (!ScaleConstant && ScaleAnimated)
                    size += 6;
            }
            return _rebuildFrameSize = size;
        }

        public int GetFixedSize()
        {
            int size = 0;
            if (HasTranslation)
            {
                if (TranslationFrame)
                {

                }
                else if (TranslationConstant || TranslationAnimated)
                {
                    size += 12;
                    if (!TranslationConstant && TranslationAnimated)
                        size += 12;
                }
            }
            if (HasRotation)
            {
                bool constant = RotationFlags.HasFlag(OMORotType.Fixed);
                bool euler = RotationFlags.HasFlag(OMORotType.Euler);
                bool quat = RotationFlags.HasFlag(OMORotType.Quaternion);
                bool frame = RotationFlags.HasFlag(OMORotType.InFrame);

                if (frame)
                {

                }
                else if (constant || euler)
                {
                    size += 12;
                    if (euler && !constant)
                        size += 12;
                }
                else if (quat)
                {
                    //Vector4 interpQ = new Vector4((float)(ushort)(*frameData++) / 0xFFFF);

                }
            }
            if (HasScale)
            {
                if (ScaleFrame)
                {

                }
                else if (ScaleConstant || ScaleAnimated)
                {
                    size += 12;
                    if (!ScaleConstant && ScaleAnimated)
                        size += 12;
                }
            }
            return size;
        }

        public FrameState[] FrameStates { get { return _frameStates; } }
        public FrameState[] _frameStates;
    }

    [Flags]
    public enum OMORotType
    {
        InFrame = 0x8,
        Fixed = 0x7,
        Quaternion = 0x6,
        Euler = 0x5,
    }
}