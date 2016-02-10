using BrawlLib.SSBB.Types;
using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe interface ICommon2MiscDataEntry
    {
        VoidPtr Address { get; }
        int Length { get; }
    }

    public unsafe class Common2MiscDataEntry : ICommon2MiscDataEntry, IDisposable
    {
        public VoidPtr Address { get; private set; }
        public int Length { get; private set; }

        public Common2MiscDataEntry(DataSource source)
        {
            Console.WriteLine(source.Length);
            Address = Marshal.AllocHGlobal(source.Length);
            Memory.Move(Address, source.Address, (uint)source.Length);
            Length = source.Length;
        }

        public void Dispose()
        {
            if (Address != null)
            {
                Marshal.FreeHGlobal(Address);
                Address = null;
            }
        }
    }

    public unsafe class Common2MiscDataNode : ARCEntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.NoEditFolder; } }
        internal Common2TblHeader* Header { get { return (Common2TblHeader*)WorkingUncompressed.Address; } }

        // Header variables
        bint _offCount = 0;

        public VoidPtr BaseAddress;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _offCount = Header->_OffCount;

            BaseAddress = (VoidPtr)Header + sizeof(Common2TblHeader);

            return true;
        }

        private class OffsetPair
        {
            public int dataOffset;
            public int nameOffset;
            public int dataEnd;

            public override string ToString()
            {
                return dataOffset + "-" + dataEnd + ", " + nameOffset;
            }
        }

        public override void OnPopulate()
        {
            int dataLength = Header->_DataLength;
            VoidPtr offsetTable = BaseAddress + dataLength;
            VoidPtr stringList = offsetTable + Header->_DataTable * 8;
            List<OffsetPair> offsets = new List<OffsetPair>();

            bint* ptr = (bint*)offsetTable;
            for (int i = 0; i < Header->_DataTable; i++)
            {
                OffsetPair o = new OffsetPair();
                o.dataOffset = *(ptr++);
                o.nameOffset = *(ptr++);
                offsets.Add(o);
                Console.WriteLine(o.dataOffset.ToString("X4") + " " + o.nameOffset.ToString("X4"));
            }

            offsets = offsets.OrderBy(o => o.dataOffset).ToList();
            for (int i = 1; i < offsets.Count; i++)
            {
                offsets[i - 1].dataEnd = offsets[i].dataOffset;
            }
            offsets[offsets.Count - 1].dataEnd = dataLength;

            foreach (OffsetPair o in offsets)
            {
                if (o.dataEnd <= o.dataOffset)
                    throw new Exception("Invalid data length (less than data offset) in common2 data");

                DataSource source = new DataSource(BaseAddress + o.dataOffset, o.dataEnd - o.dataOffset);
                var node = new Common2TblEntryNode();
                node.Initialize(this, source);
                node.Name = new string((sbyte*)stringList + o.nameOffset);
            }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Update base address for children.
            BaseAddress = (VoidPtr)address + sizeof(Common2TblHeader);

            // Initiate header struct
            Common2TblHeader* Header = (Common2TblHeader*)address;
            *Header = new Common2TblHeader();
            Header->_OffCount = _offCount;
            Header->_DataTable = Children.Count;
            Header->_pad0 = Header->_pad1 =
            Header->_pad2 = Header->_pad3 = 0;

            Dictionary<ResourceNode, VoidPtr> dataLocations = new Dictionary<ResourceNode, VoidPtr>();

            VoidPtr ptr = BaseAddress;
            foreach (var child in Children)
            {
                int size = child.CalculateSize(false);
                dataLocations.Add(child, ptr);
                child.Rebuild(ptr, size, false);
                ptr += size;
            }
            Header->_DataLength = (int)(ptr - BaseAddress);

            bint* dataPointers = (bint*)ptr;
            bint* stringPointers = dataPointers + 1;
            byte* strings = (byte*)(dataPointers + Children.Count + Children.Count);
            byte* currentString = strings;

            foreach (var child in Children)
            {
                *dataPointers = (int)(dataLocations[child] - BaseAddress);
                dataPointers += 2;
                *stringPointers = (int)(currentString - strings);
                stringPointers += 2;

                byte[] text = Encoding.UTF8.GetBytes(child.Name);
                fixed (byte* from = text)
                {
                    Memory.Move(currentString, from, (uint)text.Length);
                    currentString += text.Length;
                    *currentString = 0;
                    currentString++;
                }
            }

            Header->_Length = (int)(currentString - address);

            if (Header->_Length != length)
            {
                using (System.IO.FileStream fs = new System.IO.FileStream("C:/Users/Owner/Desktop/dump.dat", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    for (int i = 0; i < Math.Max(Header->_Length, length); i++)
                    {
                        fs.WriteByte(((byte*)address)[i]);
                    }
                    fs.Flush();
                }
                throw new Exception("Wrong amount of memory allocated for rebuild of common2 data");
            }
        }

        public override int OnCalculateSize(bool force)
        {
            int size = sizeof(Common2TblHeader);
            foreach (ResourceNode node in Children)
            {
                size += node.CalculateSize(true);
                size += Encoding.UTF8.GetByteCount(node.Name) + 1;
            }
            size += (_offCount * 4) + (Children.Count * 8);
            return size;
        }

        internal static ResourceNode TryParse(DataSource source)
        {
            Common2TblHeader* header = (Common2TblHeader*)source.Address;
            return header->_Length == source.Length &&
                header->_DataLength < source.Length ? new Common2MiscDataNode() : null;
        }
    }

    public unsafe class Common2TblEntryNode : ResourceNode
    {
        [Browsable(false)]
        public Common2MiscDataNode Root
        {
            get
            {
                return _parent as Common2MiscDataNode;
            }
        }
        [Browsable(false)]
        public VoidPtr Data { get { return (VoidPtr)WorkingUncompressed.Address; } }
        [Browsable(false)]
        public VoidPtr BaseAddress
        {
            get
            {
                if (Root == null)
                    return 0;
                return Root.BaseAddress;
            }
        }
        [Browsable(false)]
        public int _FileOffset { get { if (Data != null) return (int)Data - (int)BaseAddress; else return 0; } }

        [Browsable(true)]
        [Category("Entry")]
        public string FileOffset { get { return _FileOffset.ToString("x"); } }

        [Browsable(true)]
        [Category("Entry")]
        public string Length { get { return WorkingUncompressed.Length.ToString("x"); } }

        //internal SndBgmTitleEntry* Header { get { return (SndBgmTitleEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        ICommon2MiscDataEntry _data;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _data = new Common2MiscDataEntry(WorkingUncompressed);

            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            if (length != _data.Length) throw new Exception("Wrong size for common2 data rebuild");
            Memory.Move(address, _data.Address, (uint)length);
        }
        public override int OnCalculateSize(bool force)
        {
            return _data.Length;
        }
    }
}
