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
    public unsafe class SndBgmTitleDataNode : ARCEntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.NoEditFolder; } }

        private static byte[] EndString = Encoding.ASCII.GetBytes("sndBgmTitleData\0");

        public override bool OnInitialize()
        {
            base.OnInitialize();

            return true;
        }

        public override void OnPopulate()
        {
            int dataLength = ((SndBgmTitleHeader*)WorkingUncompressed.Address)->_DataLength;
            for (int i = 0; i < dataLength; i += sizeof(SndBgmTitleEntry))
            {
                DataSource source = new DataSource(WorkingUncompressed.Address + sizeof(SndBgmTitleHeader) + i, sizeof(SndBgmTitleEntry));
                new SndBgmTitleEntryNode().Initialize(this, source);
            }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Update base address for children.
            VoidPtr BaseAddress = (VoidPtr)address + sizeof(SndBgmTitleHeader);

            // Calculate data length
            int dataLength = sizeof(SndBgmTitleEntry) * Children.Count;
            VoidPtr stringAddress = BaseAddress + dataLength + 8;
            VoidPtr endAddress = stringAddress + EndString.Length;
            int fileSize = endAddress - address;

            if (fileSize != length)
                throw new Exception("Wrong amount of memory allocated for rebuild of sndBgmTitleData");

            // Create header struct at address
            SndBgmTitleHeader* header = (SndBgmTitleHeader*)address;
            header->_Length = fileSize;
            header->_DataLength = dataLength;
            header->_OffCount = 0;
            header->_DataTable = 1;
            header->_pad0 = header->_pad1 =
            header->_pad2 = header->_pad3 = 0;

            // Rebuild children using new address
            for (int i = 0; i < Children.Count; i++)
                Children[i].Rebuild(BaseAddress + (i * sizeof(SndBgmTitleEntry)), sizeof(SndBgmTitleEntry), true);

            // Finally, write the string
            Marshal.Copy(EndString, 0, stringAddress, EndString.Length);
        }

        public override int OnCalculateSize(bool force)
        {
            int size = sizeof(SndBgmTitleHeader);
            foreach (SndBgmTitleEntryNode node in Children)
                size += node.CalculateSize(true);
            size += 8;
            size += EndString.Length;
            return size;
        }

        internal static ResourceNode TryParse(DataSource source)
        {
            SndBgmTitleHeader* header = (SndBgmTitleHeader*)source.Address;
            return header->_Length == source.Length &&
                header->_DataLength < source.Length &&
                header->Str == "sndBgmTitleData" ? new SndBgmTitleDataNode() : null;
        }
    }

    public unsafe class SndBgmTitleEntryNode : ResourceNode
    {
        [Browsable(false)]
        public SndBgmTitleDataNode Root
        {
            get
            {
                return _parent as SndBgmTitleDataNode;
            }
        }

        [Browsable(true)]
        [Category("Entry")]
        public string Length { get { return sizeof(SndBgmTitleEntry).ToString("x"); } }

        //internal SndBgmTitleEntry* Header { get { return (SndBgmTitleEntry*)WorkingUncompressed.Address; } }
        public SndBgmTitleEntry Data;

        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        [Category("Song")]
        [DisplayName("Song ID")]
        [Description("The ID of the song to show the title for.")]
        public int ID { get { return Data._ID; } set { Data._ID = value;  } }

        [Category("Song")]
        [DisplayName("Song Title Index")]
        [Description("The index of the song title in info.pac (MiscData[140]) and other files.")]
        public int SongTitleIndex { get { return Data._SongTitleIndex; } set { Data._SongTitleIndex = value; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            if (WorkingUncompressed.Length != sizeof(SndBgmTitleEntry))
                throw new Exception("Wrong size for SndBgmTitleEntryNode");

            // Copy the data from the address
            Data = *(SndBgmTitleEntry*)WorkingUncompressed.Address;

            if (_name == null)
                _name = ID.ToString("X4");

            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Copy the data back to the address
            *(SndBgmTitleEntry*)address = Data;
        }
        public override int OnCalculateSize(bool force)
        {
            // Constant size (48 bytes)
            return sizeof(SndBgmTitleEntry);
        }
        public void UpdateName()
        {
            Name = ((int)ID).ToString("X4");
        }
    }
}
