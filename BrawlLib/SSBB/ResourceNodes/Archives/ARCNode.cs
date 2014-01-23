using System;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Compression;
using System.Windows;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class ARCNode : ARCEntryNode
    {
        internal ARCHeader* Header { get { return (ARCHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.ARC; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(ARCEntryNode) };
            }
        }

        [Browsable(false)]
        public bool IsPair { get { return _isPair; } set { _isPair = value; } }
        private bool _isPair;

        public override void OnPopulate()
        {
            ARCFileHeader* entry = Header->First;
            for (int i = 0; i < Header->_numFiles; i++, entry = entry->Next)
            {
                DataSource source = new DataSource(entry->Data, entry->Length);
                if ((entry->Length == 0) || (NodeFactory.FromSource(this, source) == null))
                {
                    //CompressionHeader* cmpr = (CompressionHeader*)source.Address;
                    //if (Compressor.IsDataCompressed(source))
                    //{
                    //    source.Compression = cmpr->Algorithm;
                    //    if (cmpr->ExpandedSize >= entry->Length && Compressor.Supports(cmpr->Algorithm))
                    //    {
                    //        //Expand the whole resource and initialize
                    //        FileMap uncompMap = FileMap.FromTempFile(cmpr->ExpandedSize);
                    //        Compressor.Expand(cmpr, uncompMap.Address, uncompMap.Length);
                    //        new ARCEntryNode().Initialize(this, source, new DataSource(uncompMap));
                    //    }
                    //    else
                    //        new ARCEntryNode().Initialize(this, source);
                    //}
                    //else
                        new ARCEntryNode().Initialize(this, source);
                }
            }
        }

        public override void Initialize(ResourceNode parent, DataSource origSource, DataSource uncompSource)
        {
            base.Initialize(parent, origSource, uncompSource);
            if (_origPath != null)
            {
                string path = Path.Combine(Path.GetDirectoryName(_origPath), Path.GetFileNameWithoutExtension(_origPath));
                _isPair = File.Exists(path + ".pac") && File.Exists(path + ".pcs");
            }
        }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            _name = Header->Name;
            return Header->_numFiles > 0;
        }

        public void ExtractToFolder(string outFolder)
        {
            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            foreach (ARCEntryNode entry in Children)
                if (entry is ARCNode)
                    ((ARCNode)entry).ExtractToFolder(Path.Combine(outFolder, entry.Name));
                else if (entry is BRESNode)
                    ((BRESNode)entry).ExportToFolder(outFolder);
        }

        public void ReplaceFromFolder(string inFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(inFolder);
            DirectoryInfo[] dirs;
            foreach (ARCEntryNode entry in Children)
            {
                if (entry is ARCNode)
                {
                    dirs = dir.GetDirectories(entry.Name);
                    if (dirs.Length > 0)
                    {
                        ((ARCNode)entry).ReplaceFromFolder(dirs[0].FullName);
                        continue;
                    }
                }
                else if (entry is BRESNode)
                {
                    ((BRESNode)entry).ReplaceFromFolder(inFolder);
                    continue;
                }
            }
        }

        public override int OnCalculateSize(bool force)
        {
            int size = ARCHeader.Size + (Children.Count * 0x20);
            foreach (ResourceNode node in Children)
                size += node.CalculateSize(force).Align(0x20);
            return size;
        }

        public override void OnRebuild(VoidPtr address, int size, bool force)
        {
            ARCHeader* header = (ARCHeader*)address;
            *header = new ARCHeader((ushort)Children.Count, Name);

            ARCFileHeader* entry = header->First;
            foreach (ARCEntryNode node in Children)
            {
                *entry = new ARCFileHeader(node.FileType, node.FileIndex, node._calcSize, node.GroupID, node._redirectIndex);
                if (node.IsCompressed)
                    node.MoveRaw(entry->Data, entry->Length);
                else
                    node.Rebuild(entry->Data, entry->Length, force);
                entry = entry->Next;
            }
        }

        public override unsafe void Export(string outPath)
        {
            if (outPath.EndsWith(".pair", StringComparison.OrdinalIgnoreCase))
                ExportPair(outPath);
            else if (outPath.EndsWith(".mrg", StringComparison.OrdinalIgnoreCase))
                ExportAsMRG(outPath);
            else if (outPath.EndsWith(".pcs", StringComparison.OrdinalIgnoreCase))
                ExportPCS(outPath);
            //else if (outPath.EndsWith(".pac", StringComparison.OrdinalIgnoreCase))
            //    ExportPAC(outPath);
            else
                base.Export(outPath);
        }

        public void ExportAsMRG(string path)
        {
            MRGNode node = new MRGNode();
            node._children = Children;
            node._changed = true;
            node.Export(path);
        }

        public void ExportPair(string path)
        {
            if (Path.HasExtension(path))
                path = path.Substring(0, path.LastIndexOf('.'));

            ExportPAC(path + ".pac");
            ExportPCS(path + ".pcs");
        }
        public void ExportPAC(string outPath)
        {
            Rebuild();
            ExportUncompressed(outPath);
        }
        public void ExportPCS(string outPath)
        {
            Rebuild();
            if (_compression != CompressionType.None)
                base.Export(outPath);
            else
            {
                using (FileStream inStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 0x8, FileOptions.SequentialScan | FileOptions.DeleteOnClose))
                using (FileStream outStream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.SequentialScan))
                {
                    Compressor.Compact(CompressionType.LZ77, WorkingUncompressed.Address, WorkingUncompressed.Length, inStream, this);
                    outStream.SetLength(inStream.Length);
                    using (FileMap map = FileMap.FromStream(inStream))
                    using (FileMap outMap = FileMap.FromStream(outStream))
                        Memory.Move(outMap.Address, map.Address, (uint)map.Length);
                }
            }
        }

        internal static ResourceNode TryParse(DataSource source) { return ((ARCHeader*)source.Address)->_tag == ARCHeader.Tag ? new ARCNode() : null; }
    }

    public unsafe class ARCEntryGroup : ResourceNode
    {
        internal byte _group;
        [Category("ARC Group")]
        public byte GroupID { get { return _group; } set { _group = value; SignalPropertyChange(); UpdateName(); } }

        public ARCEntryGroup(byte group)
        {
            _group = group;
            UpdateName();
        }

        protected void UpdateName()
        {
            Name = String.Format("[{0}]Group", _group);
        }
    }

    public unsafe class ARCEntryNode : U8EntryNode
    {
        public override ResourceType ResourceType { get { return ResourceType.ARCEntry; } }

        [Browsable(true), TypeConverter(typeof(DropDownListCompression))]
        public override string Compression
        {
            get { return base.Compression; }
            set { base.Compression = value; }
        }

        internal ARCFileType _fileType;
        [Category("ARC Entry")]
        public ARCFileType FileType { get { return _fileType; } set { _fileType = value; SignalPropertyChange(); UpdateName(); } }

        internal short _fileIndex;
        [Category("ARC Entry")]
        public short FileIndex { get { return _fileIndex; } set { _fileIndex = value; SignalPropertyChange(); UpdateName(); } }
        
        internal byte _group;
        [Category("ARC Entry")]
        public byte GroupID { get { return _group; } set { _group = value; SignalPropertyChange(); UpdateName(); } }

        [Category("ARC Entry"), Browsable(true)]
        public int AbsoluteIndex { get { return base.Index; } }
        
        internal short _redirectIndex = -1;
        [Category("ARC Entry"), TypeConverter(typeof(DropDownListARCEntry))]
        public short RedirectIndex
        {
            get { return _redirectIndex; }
            set
            {
                if (value == Index || value == _redirectIndex)
                    return;

                _redirectIndex = (short)((int)value).Clamp(0, Children.Count - 1);

                SignalPropertyChange();
            } 
        }

        private string GetName()
        {
            return String.Format("{0}[{1}]", _fileType, _fileIndex);
        }

        protected void UpdateName()
        {
            if (!(this is ARCNode))
                Name = GetName();
        }

        public override void Initialize(ResourceNode parent, DataSource origSource, DataSource uncompSource)
        {
            base.Initialize(parent, origSource, uncompSource);

            if (parent != null && (parent is MRGNode || RootNode is U8Node))
            {
                _fileType = 0;
                _fileIndex = (short)Parent._children.IndexOf(this);
                _group = 0;
                //_unk = 0;
                _redirectIndex = 0;
                if (_name == null)
                    _name = GetName();
            }
            else if (parent != null && !(parent is FileScanNode))
            {
                ARCFileHeader* header = (ARCFileHeader*)(origSource.Address - 0x20);
                _fileType = header->FileType;
                _fileIndex = header->_index;
                _group = header->_groupIndex;
                //_unk = header->_padding;
                _redirectIndex = header->_redirectIndex;
                if (_name == null)
                    _name = GetName();
            }
            else if (_name == null)
                _name = Path.GetFileName(_origPath);
        }

        //public override unsafe void Export(string outPath)
        //{
        //    ExportUncompressed(outPath);
        //}
    }
}
