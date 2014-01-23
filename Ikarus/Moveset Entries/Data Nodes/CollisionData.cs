using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using Ikarus;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class CollisionData : ListOffset
    {
        public List<CollisionDataEntry> _entries;
        public override void Parse(VoidPtr address)
        {
            base.Parse(address);
            _entries = new List<CollisionDataEntry>();
            if (DataOffset > 0)
            {
                bint* addr = (bint*)(BaseAddress + DataOffset);
                for (int i = 0; i < Count; i++)
                    if (addr[i] > 0)
                        _entries.Add(Parse<CollisionDataEntry>(addr[i]));
            }
        }
        //protected override void Write(VoidPtr address)
        //{
        //    bint* offsets = (bint*)address;
        //    VoidPtr dataAddr = address;
        //    if (Children.Count > 0)
        //    {
        //        foreach (MoveDefOffsetNode o in Children)
        //            if (o.Children.Count > 0 && !(o.Children[0] as MovesetEntry).External)
        //            {
        //                o.Children[0].Rebuild(dataAddr, o.Children[0]._calcSize, true);
        //                _lookupOffsets.AddRange((o.Children[0] as MovesetEntry)._lookupOffsets);
        //                dataAddr += o.Children[0]._calcSize;
        //            }
        //        offsets = (bint*)dataAddr;
        //        foreach (MoveDefOffsetNode o in Children)
        //        {
        //            if (o.Children.Count > 0)
        //            {
        //                *offsets = (int)(o.Children[0] as MovesetEntry)._rebuildAddr - (int)RebuildBase;
        //                _lookupOffsets.Add(offsets); //offset to child
        //            }
        //            offsets++;
        //        }
        //    }

        //    _rebuildAddr = offsets;
        //    FDefListOffset* header = (FDefListOffset*)offsets;

        //    header->_listCount = Children.Count;
        //    if (Children.Count > 0)
        //    {
        //        header->_startOffset = (int)dataAddr - (int)RebuildBase;
        //        _lookupOffsets.Add(header->_startOffset.Address);
        //    }
        //}
        //public override int GetSize()
        //{
        //    _lookupCount = 0;
        //    _entryLength = 8;
        //    _childLength = 0;
        //    if (Children.Count > 0)
        //    {
        //        _lookupCount++; //offset to children
        //        foreach (MoveDefOffsetNode o in Children)
        //        {
        //            _childLength += 4;
        //            if (o.Children.Count > 0)
        //            {
        //                _lookupCount++; //offset to child
        //                if (!(o.Children[0] as MovesetEntry).External)
        //                {
        //                    _childLength += o.Children[0].CalculateSize(true);
        //                    _lookupCount += (o.Children[0] as MovesetEntry)._lookupCount;
        //                }
        //            }
        //        }
        //    }
        //    return _childLength + _entryLength;
        //}
    }

    public enum CollisionType : int
    {
        Type0,
        Type1,
        Type2
    }

    public unsafe class CollisionDataEntry : MovesetEntry
    {
        public List<BoneIndexValue> _bones;
        public int _dataOffset, _count, _flags;
        public CollisionType _type;
        public float _length, _width, _height, _unknown;

        [Category("Collision Data")]
        public CollisionType Type { get { return _type; } }
        [Category("Collision Data")]
        public float Length { get { return _length; } set { _length = value; SignalPropertyChange(); } }
        [Category("Collision Data")]
        public float Width { get { return _width; } set { _width = value; SignalPropertyChange(); } }
        [Category("Collision Data")]
        public float Height { get { return _height; } set { _height = value; SignalPropertyChange(); } }
        [Category("Collision Data")]
        public int Flags { get { return _flags; } set { _flags = value; SignalPropertyChange(); } }

        public override void Parse(VoidPtr address)
        {
            _bones = new List<BoneIndexValue>();
            _type = *(CollisionType*)address;
            switch (_type)
            {
                case CollisionType.Type0:

                    sCollData0* hdr1 = (sCollData0*)address;
                    _dataOffset = hdr1->_list._startOffset;
                    _count = hdr1->_list._listCount;
                    _length = hdr1->unk1;
                    _width = hdr1->unk2;
                    _height = hdr1->unk3;

                    for (int i = 0; i < _count; i++)
                        _bones.Add(Parse<BoneIndexValue>(_dataOffset + i * 4));

                    break;

                case CollisionType.Type1:

                    sCollData1* hdr2 = (sCollData1*)address;
                    _length = hdr2->unk1;
                    _width = hdr2->unk2;
                    _height = hdr2->unk3;

                    break;

                case CollisionType.Type2:

                    sCollData2* hdr3 = (sCollData2*)address;
                    _flags = hdr3->flags;
                    _length = hdr3->unk1;
                    _width = hdr3->unk2;
                    _height = hdr3->unk3;

                    if ((_flags & 2) == 2)
                        _unknown = hdr3->unk4;

                    if (_size != 24 && _size != 20)
                        throw new Exception("Incorrect size");

                    break;
            }
        }

        protected override int OnGetSize()
        {
            switch (_type)
            {
                case CollisionType.Type0:
                    _lookupCount = (_bones.Count > 0 ? 1 : 0);
                    return 24 + _bones.Count * 4;
                case CollisionType.Type1:
                    _lookupCount = 0;
                    return 16;
                case CollisionType.Type2:
                    _lookupCount = 0;
                    return ((_flags & 2) == 2 ? 24 : 20);
            }
            throw new Exception("Unsupported collision type");
        }

        protected override void OnWrite(VoidPtr address)
        {
            *(CollisionType*)address = _type;
            switch (_type)
            {
                case CollisionType.Type0:

                    bint* addr = (bint*)address;
                    foreach (BoneIndexValue b in _bones)
                    {
                        b._rebuildAddr = addr;
                        *addr++ = b.boneIndex;
                    }

                    sCollData0* data1 = (sCollData0*)addr;
                    data1->unk1 = _length;
                    data1->unk2 = _width;
                    data1->unk3 = _height;

                    if (_bones.Count > 0)
                    {
                        data1->_list._startOffset = Offset(address);
                        _lookupOffsets.Add(&data1->_list._startOffset);
                    }
                    data1->_list._listCount = _bones.Count;
                    _rebuildAddr = addr;

                    break;

                case CollisionType.Type1:

                    sCollData1* data2 = (sCollData1*)address;
                    data2->unk1 = _length;
                    data2->unk2 = _width;
                    data2->unk3 = _height;
                    _rebuildAddr = address;

                    break;

                case CollisionType.Type2:

                    sCollData2* data3 = (sCollData2*)address;
                    data3->flags = _flags;
                    data3->unk1 = _length;
                    data3->unk2 = _width;
                    data3->unk3 = _height;

                    if ((_flags & 2) == 2)
                        data3->unk4 = _unknown;

                    _rebuildAddr = address;

                    break;
            }
        }
    }
}
