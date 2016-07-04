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
        const string _category = "Bone Animation";

        public int _frameSize;

        [Category(_category)]
        public int FrameSize
        {
            get { return _frameSize; }
            //set { _frameSize = value; SignalPropertyChange(); }
        }

        public override bool OnInitialize()
        {
            _name = _origPath;
            _numFrames = Header->_frameCount;
            _frameSize = Header->_frameSize;
            return Header->_boneCount > 0;
        }

        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_boneCount; ++i)
            {
                OMOBoneEntry* entry = (OMOBoneEntry*)((VoidPtr)Header + Header->_boneTableOffset + i * 0x10);
                uint fixedOff = Header->_fixedDataOffset;
                uint frameOff = Header->_frameDataOffset;
                uint offsetInFixed = entry->_offsetInFixedData;
                uint offsetInFrame = entry->_offsetInFrame;
                uint nextOffsetInFixed, nextOffsetInFrame;
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

                OMORangeAnim s = null, r = null, t = null;
                //OpenTK.Quaternion? _quat = null;

                Vector3[]
                    ScaleInterp = new Vector3[_numFrames],
                    RotInterp = new Vector3[_numFrames],
                    TransInterp = new Vector3[_numFrames];
                Vector3 
                    ScaleMin = Vector3.Zero,
                    ScaleMax = Vector3.Zero,
                    RotMin = Vector3.Zero,
                    RotMax = Vector3.Zero,
                    TransMin = Vector3.Zero,
                    TransMax = Vector3.Zero;

                if (entry->HasTranslation)
                {
                    if (entry->TranslationConstant)
                    {
                        t = new OMORangeAnim(*fixedData++);
                    }
                    else if (entry->TranslationAnimated)
                    {
                        TransMin = *fixedData++;
                        TransMax = *fixedData++;
                        t = new OMORangeAnim(TransMin, TransMax);
                    }
                }
                if (entry->HasRotation)
                {
                    if (entry->RotationFlags.HasFlag(OMORotType.Fixed))
                    {
                        r = new OMORangeAnim((Vector3)(*fixedData++) * Maths._rad2degf);
                    }
                    else if (entry->RotationFlags.HasFlag(OMORotType.Euler))
                    {
                        RotMin = *fixedData++;
                        RotMax = *fixedData++;
                        r = new OMORangeAnim(RotMin, RotMax);
                    }
                    else if (entry->RotationFlags.HasFlag(OMORotType.Quaternion))
                    {
                        //MessageBox.Show("Quat1");
                        //Vector4 q = *(BVec4*)fixedData;
                        fixedData = (BVec3*)((VoidPtr)fixedData + 16);
                        //_quat = new OpenTK.Quaternion(q._x, q._y, q._z, q._w);
                    }
                }
                if (entry->HasScale)
                {
                    if (entry->ScaleConstant)
                    {
                        s = new OMORangeAnim(*fixedData++);
                    }
                    else if (entry->ScaleAnimated)
                    {
                        ScaleMin = *fixedData++;
                        ScaleMax = *fixedData++;
                        s = new OMORangeAnim(ScaleMin, ScaleMax);
                    }
                }

                if ((int)fixedData - (int)fixedDataStart != fixedDataSize)
                    MessageBox.Show("Fixed data length mismatch");

                FrameState[] states = new FrameState[_numFrames];
                for (int x = 0; x < _numFrames; x++)
                {
                    bushort* frameData = (bushort*)(Header->GetFrameAddr(x) + offsetInFrame);
                    VoidPtr frameDataStart = frameData;

                    FrameState state = FrameState.Neutral;
                    if (entry->HasTranslation)
                    {
                        if (entry->TranslationConstant)
                        {
                            state._translate = t.GetValue();
                        }
                        else if (entry->TranslationAnimated)
                        {
                            Vector3 interp = new Vector3(*frameData++, *frameData++, *frameData++);
                            state._translate = t.GetValue(ref interp);
                            TransInterp[x] = interp;
                        }
                        else if (entry->TranslationFrame)
                        {
                            state._translate = *(BVec3*)frameData;
                            frameData += 6;
                        }
                    }
                    //TODO: maybe rotations are in world space, not parent space?
                    if (entry->HasRotation)
                    {
                        if (entry->RotationFlags.HasFlag(OMORotType.Fixed))
                        {
                            state._rotate = r.GetValue();
                        }
                        else if (entry->RotationFlags.HasFlag(OMORotType.Euler))
                        {
                            Vector3 interp = new Vector3(*frameData++, *frameData++, *frameData++);
                            state._rotate = r.GetValue(ref interp);
                            RotInterp[x] = interp;
                        }
                        else if (entry->RotationFlags.HasFlag(OMORotType.Quaternion))
                        {
                            //MessageBox.Show("Quat2");
                            frameData++;
                        }
                        else if (entry->RotationFlags.HasFlag(OMORotType.InFrame))
                        {
                            MessageBox.Show("RotFrame");
                        }
                        state._rotate *= Maths._rad2degf;
                    }
                    if (entry->HasScale)
                    {
                        if (entry->ScaleConstant)
                        {
                            state._scale = s.GetValue();
                        }
                        else if (entry->ScaleAnimated)
                        {
                            Vector3 interp = new Vector3(*frameData++, *frameData++, *frameData++);
                            state._scale = s.GetValue(ref interp);
                            ScaleInterp[x] = interp;
                        }
                        else if (entry->ScaleFrame)
                        {
                            state._scale = *(BVec3*)frameData;
                            frameData += 6;
                        }
                    }
                    state.CalcTransforms();
                    states[x] = state;

                    if ((int)frameData - (int)frameDataStart != frameDataSize)
                        MessageBox.Show("Frame data length mismatch");
                }

                new OMOBoneEntryNode()
                {
                    ScaleMin = ScaleMin,
                    ScaleMax = ScaleMax,
                    ScaleInterp = ScaleInterp,
                    RotMin = RotMin,
                    RotMax = RotMax,
                    RotInterp = RotInterp,
                    TransMin = TransMin,
                    TransMax = TransMax,
                    TransInterp = TransInterp,
                    _frameStates = states
                }
                .Initialize(this, (VoidPtr)Header + Header->_boneTableOffset + i * 0x10, 0x10);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        internal static ResourceNode TryParse(DataSource source) { return *(BinTag*)source.Address == OMOHeader.Tag ? new OMONode() : null; }
    }

    public class OMORangeAnim
    {
        public OMORangeAnim(Vector3 minValue, Vector3 maxValue)
        {
            _minValue = minValue;
            _range = maxValue;
        }
        public OMORangeAnim(Vector3 constantValue)
        {
            _minValue = constantValue;
            _range = Vector3.Zero;
        }

        Vector3 _minValue;
        Vector3 _range;

        public Vector3 GetValue(ref Vector3 interp)
        {
            interp /= 0xFFFF;
            return _minValue + _range * interp;
        }
        public Vector3 GetValue(ushort x = 0, ushort y = 0, ushort z = 0)
        {
            float px = (float)x / (float)0xFFFF;
            float py = (float)y / (float)0xFFFF;
            float pz = (float)z / (float)0xFFFF;
            return new Vector3(
                _minValue._x + _range._x * px,
                _minValue._y + _range._y * py,
                _minValue._z + _range._z * pz);
        }
    }

    public unsafe class OMOBoneEntryNode : ResourceNode
    {
        internal OMOBoneEntry* Header { get { return (OMOBoneEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        internal OMONode ParentOMO { get { return (OMONode)Parent; } }

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

        [Category("Flags")]
        public Vector3 ScaleMin { get; set; }
        [Category("Flags")]
        public Vector3 ScaleMax { get; set; }
        [Category("Flags")]
        public Vector3[] ScaleInterp { get; set; }
        [Category("Flags")]
        public Vector3 RotMin { get; set; }
        [Category("Flags")]
        public Vector3 RotMax { get; set; }
        [Category("Flags")]
        public Vector3[] RotInterp { get; set; }
        [Category("Flags")]
        public Vector3 TransMin { get; set; }
        [Category("Flags")]
        public Vector3 TransMax { get; set; }
        [Category("Flags")]
        public Vector3[] TransInterp { get; set; }

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