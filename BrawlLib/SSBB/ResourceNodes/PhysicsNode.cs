using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class PhysicsNode : ARCEntryNode
    {
        internal PhysicsHeader* Header { get { return (PhysicsHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        [Category("Offsets")]
        public int Unknown2 { get { return Header->_unk2; } }
        [Category("Offsets")]
        public int Unknown3 { get { return Header->_unk3; } }
        [Category("Offsets")]
        public int Unknown4 { get { return Header->_unk4; } }
        [Category("Offsets")]
        public int Unknown5 { get { return Header->_unk5; } }
        [Category("Offsets")]
        public int Unknown6 { get { return Header->_unk6; } }
        [Category("Offsets")]
        public int Unknown7 { get { return Header->_unk7; } }
        [Category("Offsets")]
        public int Unknown8 { get { return Header->_unk8; } }
        [Category("Offsets")]
        public int Unknown9 { get { return Header->_unk9; } }
        [Category("Offsets")]
        public int Unknown10 { get { return Header->_unk10; } }
        [Category("Offsets")]
        public uint Unknown11 { get { return Header->_unk11; } }
        
        public override bool OnInitialize()
        {
            base.OnInitialize();

            _name = Header->Name;

            return true;
        }

        public override void OnPopulate()
        {
            new ClassNamesNode() { Header = Header->ClassNames }.Initialize(this, Header->ClassNamesData, 0);
            new DataNode() { Header = Header->Data }.Initialize(this, Header->DataData, 0);
            new DataNode() { Header = Header->Types }.Initialize(this, Header->TypesData, 0);
        }

        public override int OnCalculateSize(bool force)
        {
            return base.OnCalculateSize(force);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);
        }

        internal static ResourceNode TryParse(DataSource source) { return ((PhysicsHeader*)source.Address)->_tag1 == PhysicsHeader.Tag1 ? new PhysicsNode() : null; }
    }

    public unsafe class ClassNamesNode : ResourceNode
    {
        internal VoidPtr Base { get { return (VoidPtr)WorkingUncompressed.Address; } }
        internal PhysicsOffsetSection Header;

        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public int ChildCount { get { return Children.Count; } }

        [Category("Offsets")]
        public int Unknown { get { return Header._unk0; } }
        [Category("Offsets")]
        public uint MainOffset { get { return Header._dataOffset; } }

        [Category("Offsets")]
        public uint Offset1 { get { return Header._dataOffset1; } }
        [Category("Offsets")]
        public uint Offset2 { get { return Header._dataOffset2; } }
        [Category("Offsets")]
        public uint Offset3 { get { return Header._dataOffset3; } }
        [Category("Offsets")]
        public uint Offset4 { get { return Header._dataOffset4; } }
        [Category("Offsets")]
        public uint Offset5 { get { return Header._dataOffset5; } }
        [Category("Offsets")]
        public uint DataLength { get { return Header._dataLength; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _name = Header.Name;

            SetSizeInternal((int)DataLength);

            return true;
        }

        public override void OnPopulate()
        {
            byte* header = (byte*)Base;
            int size = 0, len = 0;
            while (size <= DataLength && *header != 0xFF)
            {
                len = 5 + ((PhysicsClassName*)header)->_value.Length + 1;
                new ClassNameEntryNode().Initialize(this, header, len);
                header += len;
                size += len;
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return base.OnCalculateSize(force);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);
        }
    }

    public unsafe class DataNode : ResourceNode
    {
        internal VoidPtr Base { get { return (VoidPtr)WorkingUncompressed.Address; } }
        internal PhysicsOffsetSection Header;
        public List<VoidPtr> _indexAddrs;
        public List<uint> _counts;
        
        public int[][] _indices;
        public int[][] Indices { get { return _indices; } }

        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        [Category("Offsets")]
        public int Unknown { get { return Header._unk0; } }
        [Category("Offsets")]
        public uint MainOffset { get { return Header._dataOffset; } }

        [Category("Offsets")]
        public uint Offset1 { get { return Header._dataOffset1; } }
        [Category("Offsets")]
        public uint Offset2 { get { return Header._dataOffset2; } }
        [Category("Offsets")]
        public uint Offset3 { get { return Header._dataOffset3; } }
        [Category("Offsets")]
        public uint Offset4 { get { return Header._dataOffset4; } }
        [Category("Offsets")]
        public uint Offset5 { get { return Header._dataOffset5; } }
        [Category("Offsets")]
        public uint DataLength { get { return Header._dataLength; } }

        public override bool OnInitialize()
        {
            return false;

            base.OnInitialize();

            _name = Header.Name;

            SetSizeInternal((int)DataLength);

            _indexAddrs = new List<VoidPtr>();
            _counts = new List<uint>();
            _indices = new int[0][];

            if (Offset2 - Offset1 != 0)
            {
                _indexAddrs.Add(Base + Offset1);
                _counts.Add((Offset2 - Offset1) / 4);
                Array.Resize(ref _indices, _indices.Length + 1);
                //Array.Resize(ref _indices[_indices.Length - 1], (int)_counts[_counts.Count - 1]);
            }
            if (Offset3 - Offset2 != 0)
            {
                _indexAddrs.Add(Base + Offset2);
                _counts.Add((Offset3 - Offset2) / 4);
                Array.Resize(ref _indices, _indices.Length + 1);
                //Array.Resize(ref _indices[_indices.Length - 1], (int)_counts[_counts.Count - 1]);
            }
            if (Offset4 - Offset3 != 0)
            {
                _indexAddrs.Add(Base + Offset3);
                _counts.Add((Offset4 - Offset3) / 4);
                Array.Resize(ref _indices, _indices.Length + 1);
                //Array.Resize(ref _indices[_indices.Length - 1], (int)_counts[_counts.Count - 1]);
            }
            if (Offset5 - Offset4 != 0)
            {
                _indexAddrs.Add(Base + Offset4);
                _counts.Add((Offset5 - Offset4) / 4);
                Array.Resize(ref _indices, _indices.Length + 1);
                //Array.Resize(ref _indices[_indices.Length - 1], (int)_counts[_counts.Count - 1]);
            }
            if (DataLength - Offset5 != 0)
            {
                _indexAddrs.Add(Base + Offset5);
                _counts.Add((DataLength - Offset5) / 4);
                Array.Resize(ref _indices, _indices.Length + 1);
                //Array.Resize(ref _indices[_indices.Length - 1], (int)_counts[_counts.Count - 1]);
            }
            int z = 0, m = 0;
            foreach (VoidPtr ptr in _indexAddrs)
            {
                m = 0;
                bint* addr = (bint*)ptr;
                for (int x = 0; x < _counts[z]; x++) 
                {
                    if (*addr != -1)
                    {
                        Array.Resize(ref _indices[z], m + 1);
                        _indices[z][m++] = *addr;
                    }
                    addr++; 
                }
                z++;
            }
            //if (Index == 1 && _indices.Length > 0 && _indices[0].Length != Parent.Children[0].Children.Count)
            //    Console.WriteLine();
            if (_indices.Length > 1 && _indices[1].Length % 3 != 0)
                Console.WriteLine();
            if (_indices.Length > 2 && _indices[2].Length % 3 != 0)
                Console.WriteLine();
            return true;
        }

        public override void OnPopulate()
        {
            for (int i = 0; i < _indices[1].Length / 3; i++)
            {
                //if (_indices[2][i * 3] != _indices[1][i * 3 + 2])
                //    Console.WriteLine();
                new RawValueListNode() { _name = "Entry" + i }.Initialize(this, Base + _indices[1][i * 3], 4 * _indices[1][i * 3 + 1]);
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return base.OnCalculateSize(force);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            base.OnRebuild(address, length, force);
        }
    }

    public unsafe class ClassNameEntryNode : ResourceNode
    {
        internal PhysicsClassName* Header { get { return (PhysicsClassName*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        Bin32 _value;
        byte _unk2;
        [TypeConverter(typeof(Bin32StringConverter))]
        public Bin32 Unk1 { get { return _value; } set { _value = value; SignalPropertyChange(); } }
        public byte Unk2 { get { return _unk2; } set { _unk2 = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            _name = Header->_value;
            _value = new Bin32(Header->_unk1);
            _unk2 = Header->_unk2;
            return false;
        }
    }

    public unsafe class PhysicsVectorNode : ResourceNode
    {
        internal PhysicsVector* Header { get { return (PhysicsVector*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        int x, y, z;

        public int X { get { return x; } set { x = value; SignalPropertyChange(); } }
        public int Y { get { return y; } set { y = value; SignalPropertyChange(); } }
        public int Z { get { return z; } set { z = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            return false;
        }
    }

    public unsafe class RawValueListNode : ResourceNode
    {
        internal byte* Header { get { return (byte*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        //public List<AttributeInfo> _info;

        private UnsafeBuffer attributeBuffer;

        [Browsable(false)]
        public UnsafeBuffer AttributeBuffer { get { if (attributeBuffer != null) return attributeBuffer; else return attributeBuffer = new UnsafeBuffer(WorkingUncompressed.Length); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            if (WorkingUncompressed.Length == 0)
                SetSizeInternal(4);

            
            attributeBuffer = new UnsafeBuffer(WorkingUncompressed.Length);
            byte* pOut = (byte*)attributeBuffer.Address;
            byte* pIn = (byte*)Header;

            //_info = new List<AttributeInfo>();
            //for (int i = 0; i < WorkingUncompressed.Length; i++)
            //{
            //    if (i % 4 == 0)
            //    {
            //        AttributeInfo info = new AttributeInfo();

            //        //Guess
            //        if (((((uint)*((buint*)pIn)) >> 24) & 0xFF) != 0 && *((bint*)pIn) != -1 && !float.IsNaN(((float)*((bfloat*)pIn))))
            //            info._type = 0;
            //        else
            //            info._type = 1;

            //        info._name = (info._type == 1 ? "*" : "" + (info._type > 3 ? "+" : "")) + "0x" + i.ToString("X");
            //        info._description = "No Description Available.";
                    
            //        _info.Add(info);
            //    }
            //    *pOut++ = *pIn++;
            //}

            return false;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            byte* pIn = (byte*)attributeBuffer.Address;
            byte* pOut = (byte*)address;
            for (int i = 0; i < attributeBuffer.Length; i++)
                *pOut++ = *pIn++;
        }

        public override int OnCalculateSize(bool force)
        {
            return attributeBuffer.Length;
        }
    }
}
