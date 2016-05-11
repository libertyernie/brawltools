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
                new OMOEntryNode().Initialize(this, (VoidPtr)Header + Header->_boneTableOffset + i * 0x10, 0x10);
        }

        internal static ResourceNode TryParse(DataSource source) { return *(BinTag*)source.Address == OMOHeader.Tag ? new OMONode() : null; }
    }

    public unsafe class OMOEntryNode : ResourceNode
    {
        internal OMOBoneEntry* Header { get { return (OMOBoneEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public uint _boneHash;
        public Bin32 _flags;

        const string _category = "Bone Entry";

        [Category(_category)]
        public uint BoneHash
        {
            get { return _boneHash; }
            set { _boneHash = value; SignalPropertyChange(); }
        }
        [Category(_category), TypeConverter(typeof(Bin32StringConverter))]
        public Bin32 Flags
        {
            get { return _flags; }
            set { _flags = value; SignalPropertyChange(); }
        }

        public override bool OnInitialize()
        {
            _boneHash = Header->_boneHash;
            _flags = Header->_flags;

            _name = _boneHash.ToString("X");
            return false;
        }
    }
}