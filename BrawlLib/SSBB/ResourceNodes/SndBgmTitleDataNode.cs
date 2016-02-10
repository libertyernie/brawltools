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
        internal SndBgmTitleHeader* Header { get { return (SndBgmTitleHeader*)WorkingUncompressed.Address; } }

        // Header variables
        bint _unknown1, _unknown2 = 0;

        public VoidPtr BaseAddress;

        private static byte[] EndString = Encoding.ASCII.GetBytes("sndBgmTitleData\0");

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _unknown1 = Header->_unknown1;
            _unknown2 = Header->_unknown2;

            BaseAddress = (VoidPtr)Header + sizeof(SndBgmTitleHeader);

            return true;
        }

        public override void OnPopulate()
        {
            int dataLength = Header->_DataLength;
            for (int i = 0; i < dataLength; i += sizeof(SndBgmTitleEntry))
            {
                DataSource source = new DataSource(BaseAddress + i, sizeof(SndBgmTitleEntry));
                new SndBgmTitleEntryNode().Initialize(this, source);
            }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Update base address for children.
            BaseAddress = (VoidPtr)address + sizeof(SndBgmTitleHeader);

            int dataLength = sizeof(SndBgmTitleEntry) * Children.Count;
            VoidPtr stringAddress = BaseAddress + dataLength + (_unknown1 * 4) + (_unknown2 * 8);
            VoidPtr endAddress = stringAddress + EndString.Length;
            int fileSize = endAddress - address;

            if (fileSize != length)
                throw new Exception("Wrong amount of memory allocated for rebuild of sndBgmTitleData");

            // Initiate header struct
            SndBgmTitleHeader* Header = (SndBgmTitleHeader*)address;
            *Header = new SndBgmTitleHeader();
            Header->_Length = fileSize;
            Header->_unknown1 = _unknown1;
            Header->_unknown2 = _unknown2;
            Header->_DataLength = dataLength;
            Header->_pad0 = Header->_pad1 =
            Header->_pad2 = Header->_pad3 = 0;

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
            size += (_unknown1 * 4) + (_unknown2 * 8);
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

        internal SndBgmTitleEntry* Header { get { return (SndBgmTitleEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        [Category("Song")]
        [DisplayName("Song ID")]
        [Description("The ID of the song to show the title for.")]
        public int ID { get; set; }
        [Browsable(false)]
        public bint Unknown04 { get; set; }
        [Browsable(false)]
        public bint Unknown08 { get; set; }
        [Browsable(false)]
        public bint Unknown0c { get; set; }

        [Category("Song")]
        [DisplayName("Song Title Index")]
        [Description("The index of the song title in info.pac (MiscData[140]) and other files.")]
        public int SongTitleIndex { get; set; }
        [Browsable(false)]
        public bint Unknown14 { get; set; }
        [Browsable(false)]
        public bint Unknown18 { get; set; }
        [Browsable(false)]
        public bint Unknown1c { get; set; }

        [Browsable(false)]
        public bint Unknown20 { get; set; }
        [Browsable(false)]
        public bint Unknown24 { get; set; }
        [Browsable(false)]
        public bint Unknown28 { get; set; }
        [Browsable(false)]
        public bint Unknown2c { get; set; }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            ID = Header->_ID;
            Unknown04 = Header->_unknown04;
            Unknown08 = Header->_unknown08;
            Unknown0c = Header->_unknown0c;
            SongTitleIndex = Header->_SongTitleIndex;
            Unknown14 = Header->_unknown14;
            Unknown18 = Header->_unknown18;
            Unknown1c = Header->_unknown1c;
            Unknown20 = Header->_unknown20;
            Unknown24 = Header->_unknown24;
            Unknown28 = Header->_unknown28;
            Unknown2c = Header->_unknown2c;

            if (_name == null)
                _name = ((int)ID).ToString("X4");

            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            SndBgmTitleEntry* Header = (SndBgmTitleEntry*)address;
            *Header = new SndBgmTitleEntry();
            Header->_ID = ID;
            Header->_unknown04 = Unknown04;
            Header->_unknown08 = Unknown08;
            Header->_unknown0c = Unknown0c;
            Header->_SongTitleIndex = SongTitleIndex;
            Header->_unknown14 = Unknown14;
            Header->_unknown18 = Unknown18;
            Header->_unknown1c = Unknown1c;
            Header->_unknown20 = Unknown20;
            Header->_unknown24 = Unknown24;
            Header->_unknown28 = Unknown28;
            Header->_unknown2c = Unknown2c;
        }
        public override int OnCalculateSize(bool force)
        {
            int size = sizeof(SndBgmTitleEntry);
            return size;
        }
        public void UpdateName()
        {
            Name = ((int)ID).ToString("X4");
        }
    }
}
