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
    public unsafe class VBNNode : ResourceNode
    {
        internal VBNHeader* Header { get { return (VBNHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.OMO; } }
        public override Type[] AllowedChildTypes { get { return new Type[] { typeof(OMOEntryNode) }; } }

        public Endian _endian = Endian.Little;

        public VBNNode() { }
        const string _category = "Skeleton";

        public ushort _versionMin, _versionMax;
        public uint _unk;

        uint _unk1;
        uint _unk2;
        uint _unk3;

        [Category(_category)]
        public ushort VersionMin
        {
            get { return _versionMin; }
            set { _versionMin = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public ushort VersionMax
        {
            get { return _versionMax; }
            set { _versionMax = value; SignalPropertyChange(); }
        }
        [Category(_category)]
        public uint Unknown
        {
            get { return _unk; }
            set { _unk = value; SignalPropertyChange(); }
        }
        [Category("Frame Header")]
        public uint Unk1
        {
            get { return _unk1; }
            set { _unk1 = value; SignalPropertyChange(); }
        }
        [Category("Frame Header")]
        public uint Unk2
        {
            get { return _unk2; }
            set { _unk2 = value; SignalPropertyChange(); }
        }
        [Category("Frame Header")]
        public uint Unk3
        {
            get { return _unk3; }
            set { _unk3 = value; SignalPropertyChange(); }
        }

        public override bool OnInitialize()
        {
            _name = "Skeleton";
            _endian = *(byte*)Header->_tag.Address == 0x20 ? Endian.Little : Endian.Big;
            EndianMode.SetEndian(_endian);
            _unk = Header->_boneCount2Unk;
            _versionMin = Header->_versionMin;
            _versionMax = Header->_versionMax;
            uint boneCount = Header->_boneCount;
            VBNFrameHeader* frameHdr = Header->FrameData;
            _unk1 = frameHdr->_unk1;
            _unk2 = frameHdr->_unk2;
            _unk3 = frameHdr->_unk3;
            EndianMode.SetEndian(Endian.Big);
            return boneCount > 0;
        }

        public override void OnPopulate()
        {
            EndianMode.SetEndian(_endian);
            uint count = Header->_boneCount;
            for (int i = 0; i < count; ++i)
                new VBNBoneNode().Initialize(this, (VoidPtr)Header + 0x10 + i * 0x4C, 0x4C);
            EndianMode.SetEndian(Endian.Big);
            ResourceNode[] bones = _children.ToArray();
            //_children.Clear();
            bones[0].Parent = this;
            //_children.Add(bones[0]);
            foreach (VBNBoneNode bone in bones)
            {
                uint id = bone._parentID;
                ResourceNode node;
                if (id == 0x0FFFFFFF)
                    node = bones[0];
                else
                    node = bones[(int)id + 1];
                if (node != bone)
                {
                    bone._parent._children.Remove(bone);
                    bone._parent = node;
                    node.Children.Add(bone);
                }
            }
        }

        internal static ResourceNode TryParse(DataSource source)
        {
            string tag = *(BinTag*)source.Address;
            if (tag == VBNHeader.Tag)
                return new VBNNode() { _endian = Endian.Big };
            if (tag.Reverse() == VBNHeader.Tag)
                return new VBNNode() { _endian = Endian.Little };
            return null;
        }
    }

    public unsafe class VBNBoneNode : ResourceNode
    {
        internal VBNBone* Header { get { return (VBNBone*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public uint _parentID;
        public uint _hash;
        
        const string _category = "Bone";

        [Category(_category)]
        public uint HashValue
        {
            get { return _hash; }
            set { _hash = value; SignalPropertyChange(); }
        }
        [Category(_category), TypeConverter(typeof(Bin32StringConverter))]
        public uint ParentID
        {
            get { return _parentID; }
            set { _parentID = value; SignalPropertyChange(); }
        }

        public override bool OnInitialize()
        {
            _parentID = Header->_parentIndex;
            _hash = Header->_hash;
            _name = Header->GetString();

            return false;
        }
    }
}