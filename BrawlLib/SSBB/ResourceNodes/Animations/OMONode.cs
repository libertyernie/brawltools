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
    public unsafe class OMONode : ResourceNode
    {
        public static VBNNode _skeleton;
        public string Skeleton { get { return _skeleton == null ? "" : _skeleton.Name; } }

        internal OMOHeader* Header { get { return (OMOHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.OMO; } }
        public override Type[] AllowedChildTypes { get { return new Type[] { typeof(OMOEntryNode) }; } }

        public OMONode() { }
        const string _category = "Bone Animation";

        public int _frameCount;
        public int _frameSize;

        [Category(_category)]
        public int FrameCount
        {
            get { return _frameCount; }
            set { _frameCount = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public int FrameSize
        {
            get { return _frameSize; }
            set { _frameSize = value; SignalPropertyChange(); }
        }

        public override bool OnInitialize()
        {
            _frameCount = Header->_frameCount;
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

                UnsafeBuffer fixedData = new UnsafeBuffer(fixedDataSize);
                Memory.Move(fixedData.Address, (VoidPtr)Header + fixedOff + offsetInFixed, (uint)fixedData.Length);

                UnsafeBuffer frameData = new UnsafeBuffer(frameDataSize);
                Memory.Move(frameData.Address, (VoidPtr)Header + frameOff + offsetInFrame, (uint)frameData.Length);

                new OMOEntryNode()
                {
                    _fixedBuffer = fixedData,
                    _frameBuffer = frameData,
                    _fixedDataSize = fixedDataSize,
                    _frameDataSize = frameDataSize,
                }
                .Initialize(this, (VoidPtr)Header + Header->_boneTableOffset + i * 0x10, 0x10);
            }
        }

        internal static ResourceNode TryParse(DataSource source) { return *(BinTag*)source.Address == OMOHeader.Tag ? new OMONode() : null; }
    }

    public unsafe class OMOEntryNode : ResourceNode, IBufferNode
    {
        internal OMOBoneEntry* Header { get { return (OMOBoneEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public uint _boneHash;
        public Bin32 _flags;

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
            set { _flags = value; SignalPropertyChange(); }
        }

        internal int _frameDataSize, _fixedDataSize;
        public int FixedDataLength
        {
            get { return _fixedDataSize; }
        }
        public int FrameDataLength
        {
            get { return _frameDataSize; }
        }
        private static bool _showFrameData;
        public bool ShowFrameData { get { return _showFrameData; } set { _showFrameData = value; UpdateCurrentControl(); } }

        public override bool OnInitialize()
        {
            _boneHash = Header->_boneHash;
            _flags = Header->_flags;

            _name = _boneHash.ToString("X");
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

        [Category("Flags")]
        public bool HasScale
        {
            get { return _flags[26]; }
        }
        [Category("Flags")]
        public bool HasRotation
        {
            get { return _flags[25]; }
        }
        [Category("Flags")]
        public bool HasTranslation
        {
            get { return _flags[24]; }
        }
        [Category("Flags")]
        public bool Flag23
        {
            get { return _flags[23]; }
        }
        [Category("Flags")]
        public bool Flag22
        {
            get { return _flags[22]; }
        }
        [Category("Flags")]
        public bool Flag21
        {
            get { return _flags[21]; }
        }
        [Category("Flags")]
        public bool Flag20
        {
            get { return _flags[20]; }
        }
        [Category("Flags")]
        public bool Flag19
        {
            get { return _flags[19]; }
        }
        [Category("Flags")]
        public bool Flag18
        {
            get { return _flags[18]; }
        }
        [Category("Flags")]
        public bool Flag17
        {
            get { return _flags[17]; }
        }
        [Category("Flags")]
        public bool Flag16
        {
            get { return _flags[16]; }
        }
        [Category("Flags")]
        public bool Flag15
        {
            get { return _flags[15]; }
        }
        [Category("Flags")]
        public bool Flag14
        {
            get { return _flags[14]; }
        }
        [Category("Flags")]
        public bool Flag13
        {
            get { return _flags[13]; }
        }
        [Category("Flags")]
        public bool Flag12
        {
            get { return _flags[12]; }
        }
        [Category("Flags")]
        public bool Flag11
        {
            get { return _flags[11]; }
        }
        [Category("Flags")]
        public bool Flag10
        {
            get { return _flags[10]; }
        }
        [Category("Flags")]
        public bool Flag9
        {
            get { return _flags[9]; }
        }
        [Category("Flags")]
        public bool Flag8
        {
            get { return _flags[8]; }
        }
        [Category("Flags")]
        public bool Flag7
        {
            get { return _flags[7]; }
        }
        [Category("Flags")]
        public bool Flag6
        {
            get { return _flags[6]; }
        }
        [Category("Flags")]
        public bool Flag5
        {
            get { return _flags[5]; }
        }
        [Category("Flags")]
        public bool Flag4
        {
            get { return _flags[4]; }
        }
        [Category("Flags")]
        public bool Flag3
        {
            get { return _flags[3]; }
        }
        [Category("Flags")]
        public bool Flag2
        {
            get { return _flags[2]; }
        }
        [Category("Flags")]
        public bool Flag1
        {
            get { return _flags[1]; }
        }
        [Category("Flags")]
        public bool Flag0
        {
            get { return _flags[0]; }
        }

        internal UnsafeBuffer _fixedBuffer, _frameBuffer;
        private UnsafeBuffer Buffer { get { return ShowFrameData ? _frameBuffer : _fixedBuffer; } }

        public bool IsValid() { return Buffer != null && Buffer.Length > 0; }
        public VoidPtr GetAddress() { return Buffer.Address; }
        public int GetLength() { return Buffer.Length; }
    }
}