using System;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Compression;
using System.Windows;
using System.Collections.Specialized;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RARCNode : RARCEntryNode
    {
        internal RARC* Header { get { return (RARC*)WorkingUncompressed.Address; } }
        
        public override ResourceType ResourceType { get { return ResourceType.RARC; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(RARCEntryNode) };
            }
        }

        [Browsable(true), TypeConverter(typeof(DropDownListCompression))]
        public override string Compression
        {
            get { return base.Compression; }
            set { base.Compression = value; }
        }

        public override void OnPopulate()
        {
            RARCFolderNode.ParseDirectory(Header, *Header->FolderEntries, this);
        }

        public override bool OnInitialize()
        {
            _name = Path.GetFileNameWithoutExtension(_origPath);
            return Header->_fileDataSize0 > 0 || Header->_folderCount > 0;
        }

        public override int OnCalculateSize(bool force)
        {
            int size = RARC.Size;
            size += RecursiveGetSize(this);
            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            RARC* header = (RARC*)address;
            header->_tag = RARC.Tag;
            header->_totalSize = (uint)length;
            header->_headerOffset = 0x20;
            header->_fileDataOffset = 0;

            header->_fileDataSize0 =
            header->_fileDataSize1 = 0;
            header->_unknown0 = 0;
            header->_unknown1 = 0;
        }

        private int RecursiveGetSize(ResourceNode parent)
        {
            return 0;
        }

        public void ExportCompressed(string outPath)
        {
            if (_compression != CompressionType.None)
                base.Export(outPath);
            else
            {
                using (FileStream inStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 0x8, FileOptions.SequentialScan | FileOptions.DeleteOnClose))
                using (FileStream outStream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.SequentialScan))
                {
                    Compressor.Compact(CompressionType.RunLengthYAZ0, WorkingUncompressed.Address, WorkingUncompressed.Length, inStream, this);
                    outStream.SetLength(inStream.Length);
                    using (FileMap map = FileMap.FromStream(inStream))
                    using (FileMap outMap = FileMap.FromStream(outStream))
                        Memory.Move(outMap.Address, map.Address, (uint)map.Length);
                }
            }
        }

        internal static ResourceNode TryParse(DataSource source) 
        {
            return ((RARC*)source.Address)->_tag == RARC.Tag ? new RARCNode() : null;
        }
    }

    public unsafe class RARCEntryNode : ResourceNode
    {
        public override ResourceType ResourceType { get { return ResourceType.RARCEntry; } }

        internal RARCFileEntry _entry;
        
        [Category("RARC File Entry")]
        public ushort Unknown0 { get { return _entry._unknown0; } }
        [Category("RARC File Entry")]
        public ushort Unknown1 { get { return _entry._unknown1; } }
        [Category("RARC File Entry")]
        public uint Unknown2 { get { return _entry._unknown2; } }
        [Category("RARC File Entry")]
        public ushort ID { get { return _entry._id; } }
        [Category("RARC File Entry")]
        public uint DataOffset { get { return _entry._dataOffset; } }
        [Category("RARC File Entry")]
        public uint DataSize { get { return _entry._dataSize; } }
        [Category("RARC File Entry")]
        public ushort StringOffset { get { return _entry._stringOffset; } }

        [Browsable(false)]
        public RARCNode RARCNode
        {
            get
            {
                ResourceNode n = _parent;
                while (!(n is RARCNode) && (n != null))
                    n = n._parent;
                return n as RARCNode;
            }
        }
    }
    
    public unsafe class RARCFolderNode : RARCEntryNode
    {
        internal RARCFolderEntry* Header { get { return (RARCFolderEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RARCFolder; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(RARCEntryNode) };
            }
        }
        RARC* RARCHeader;
        
        [Category("RARC Folder")]
        public string Tag { get { return Header->_tag; } }
        [Category("RARC Folder")]
        public ushort Unknown { get { return Header->_unknown; } }
        [Category("RARC Folder")]
        public ushort EntryCount { get { return Header->_numFileEntries; } }
        [Category("RARC Folder")]
        public uint FirstEntryIndex { get { return Header->_firstFileEntryIndex; } }
        [Category("RARC Folder")]
        public uint FolderStringOffset { get { return Header->_stringOffset; } }

        public override bool OnInitialize()
        {
            if (_name == null && !_replaced)
                _name = RARCHeader->GetString(Header->_stringOffset);

            return Header->_numFileEntries > 0;
        }

        public override void OnPopulate()
        {
            ParseDirectory(RARCHeader, *Header, this);
        }

        public override int OnCalculateSize(bool force)
        {
            return 0;
        }

        public T CreateResource<T>(string name) where T : RARCEntryNode
        {
            T n = Activator.CreateInstance<T>();
            n.Name = FindName(name);
            AddChild(n);

            return n;
        }

        public static void ParseDirectory(RARC* header, RARCFolderEntry node, ResourceNode parent)
        {
            for (int x = 0; x < node._numFileEntries; x++)
            {
                RARCFileEntry entry = header->FileEntries[node._firstFileEntryIndex + x];
                RARCEntryNode entryNode = null;

                if (entry._stringOffset > 2)
                    if (entry._id == 0xFFFF)
                    {
                        //Folder
                        entryNode = new RARCFolderNode() { RARCHeader = header };
                        entryNode.Initialize(parent, new DataSource(&header->FolderEntries[entry._dataOffset], 0x14));
                    }
                    else
                    {
                        //File
                        DataSource source = new DataSource((VoidPtr)header + header->_headerOffset + header->_fileDataOffset + entry._dataOffset, (int)(uint)entry._dataSize);
                        if ((entryNode = NodeFactory.FromSource(parent, source) as RARCEntryNode) == null)
                        {
                            entryNode = new RARCEntryNode();
                            entryNode.Initialize(parent, source);
                        }
                    }
                else
                    entryNode = new RARCEntryNode();

                entryNode._entry = entry;
                entryNode._name = header->GetString(entry._stringOffset);
            }
        }
    }
}