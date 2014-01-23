using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Collections.Generic;
using BrawlLib.Wii.Compression;
using System.IO;
using System.Drawing;
using BrawlLib.IO;
using System.Windows.Forms;
using BrawlLib.Wii.Textures;
using Gif.Components;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class BRESNode : ARCEntryNode
    {
        internal BRESHeader* Header { get { return (BRESHeader*)WorkingUncompressed.Address; } }

        internal ROOTHeader* RootHeader { get { return Header->First; } }
        internal ResourceGroup* Group { get { return &RootHeader->_master; } }

        public override ResourceType ResourceType { get { return ResourceType.BRES; } }

        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(BRESGroupNode) };
            }
        }

        public override void OnPopulate()
        {
            ResourceGroup* group = Group;
            for (int i = 0; i < group->_numEntries; i++)
                new BRESGroupNode(new String((sbyte*)group + group->First[i]._stringOffset)).Initialize(this, (VoidPtr)group + group->First[i]._dataOffset, 0);
        }
        public override bool OnInitialize()
        {
            base.OnInitialize();

            return Group->_numEntries > 0;
        }

        public BRESGroupNode GetOrCreateFolder<T>() where T : BRESEntryNode
        {
            string groupName; BRESGroupNode.BRESGroupType type;
            if (typeof(T) == typeof(TEX0Node))
            {
                type = BRESGroupNode.BRESGroupType.Textures;
                groupName = "Textures(NW4R)";
            }
            else if (typeof(T) == typeof(PLT0Node))
            {
                type = BRESGroupNode.BRESGroupType.Palettes;
                groupName = "Palettes(NW4R)";
            }
            else if (typeof(T) == typeof(MDL0Node))
            {
                type = BRESGroupNode.BRESGroupType.Models;
                groupName = "3DModels(NW4R)";
            }
            else if (typeof(T) == typeof(CHR0Node))
            {
                type = BRESGroupNode.BRESGroupType.CHR0;
                groupName = "AnmChr(NW4R)";
            }
            else if (typeof(T) == typeof(CLR0Node))
            {
                type = BRESGroupNode.BRESGroupType.CLR0;
                groupName = "AnmClr(NW4R)";
            }
            else if (typeof(T) == typeof(SRT0Node))
            {
                type = BRESGroupNode.BRESGroupType.SRT0;
                groupName = "AnmTexSrt(NW4R)";
            }
            else if (typeof(T) == typeof(PAT0Node))
            {
                type = BRESGroupNode.BRESGroupType.PAT0;
                groupName = "AnmTexPat(NW4R)";
            }
            else if (typeof(T) == typeof(SHP0Node))
            {
                type = BRESGroupNode.BRESGroupType.SHP0;
                groupName = "AnmShp(NW4R)";
            }
            else if (typeof(T) == typeof(VIS0Node))
            {
                type = BRESGroupNode.BRESGroupType.VIS0;
                groupName = "AnmVis(NW4R)";
            }
            else if (typeof(T) == typeof(SCN0Node))
            {
                type = BRESGroupNode.BRESGroupType.SCN0;
                groupName = "AnmScn(NW4R)";
            }
            else if (typeof(T) == typeof(RASDNode))
            {
                type = BRESGroupNode.BRESGroupType.External;
                groupName = "External";
            }
            else
                return null;

            BRESGroupNode group = null;
            foreach (BRESGroupNode node in Children)
                if (node.Type == type)
                { group = node; break; }

            if (group == null)
                AddChild(group = new BRESGroupNode(groupName, type));

            return group;
        }
        public T CreateResource<T>(string name) where T : BRESEntryNode
        {
            BRESGroupNode group = GetOrCreateFolder<T>();
            if (group == null)
                return null;

            T n = Activator.CreateInstance<T>();
            n.Name = group.FindName(name);
            group.AddChild(n);

            return n;
        }
        public void ExportToFolder(string outFolder) { ExportToFolder(outFolder, ".tex0"); }
        public void ExportToFolder(string outFolder, string imageExtension)
        {
            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            string ext = "";
            foreach (BRESGroupNode group in Children)
            {
                if (group.Type == BRESGroupNode.BRESGroupType.Textures)
                    ext = imageExtension;
                else if (group.Type == BRESGroupNode.BRESGroupType.Models)
                    ext = ".mdl0";
                else if (group.Type == BRESGroupNode.BRESGroupType.CHR0)
                    ext = ".chr0";
                else if (group.Type == BRESGroupNode.BRESGroupType.CLR0)
                    ext = ".clr0";
                else if (group.Type == BRESGroupNode.BRESGroupType.SRT0)
                    ext = ".srt0";
                else if (group.Type == BRESGroupNode.BRESGroupType.SHP0)
                    ext = ".shp0";
                else if (group.Type == BRESGroupNode.BRESGroupType.PAT0)
                    ext = ".pat0";
                else if (group.Type == BRESGroupNode.BRESGroupType.VIS0)
                    ext = ".vis0";
                else if (group.Type == BRESGroupNode.BRESGroupType.SCN0)
                    ext = ".scn0";
                else if (group.Type == BRESGroupNode.BRESGroupType.Palettes)
                    ext = ".plt0";
                foreach (BRESEntryNode entry in group.Children)
                    entry.Export(Path.Combine(outFolder, entry.Name + ext));
            }
        }
        public void ReplaceFromFolder(string inFolder) { ReplaceFromFolder(inFolder, ".tex0"); }
        public void ReplaceFromFolder(string inFolder, string imageExtension)
        {
            DirectoryInfo dir = new DirectoryInfo(inFolder);
            FileInfo[] files = dir.GetFiles();
            string ext = "*";
            foreach (BRESGroupNode group in Children)
            {
                if (group.Type == BRESGroupNode.BRESGroupType.Textures)
                    ext = imageExtension;
                else if (group.Type == BRESGroupNode.BRESGroupType.Palettes)
                    ext = ".plt0";
                else if (group.Type == BRESGroupNode.BRESGroupType.Models)
                    ext = ".mdl0";
                else if (group.Type == BRESGroupNode.BRESGroupType.CHR0)
                    ext = ".chr0";
                else if (group.Type == BRESGroupNode.BRESGroupType.CLR0)
                    ext = ".clr0";
                else if (group.Type == BRESGroupNode.BRESGroupType.SRT0)
                    ext = ".srt0";
                else if (group.Type == BRESGroupNode.BRESGroupType.SHP0)
                    ext = ".shp0";
                else if (group.Type == BRESGroupNode.BRESGroupType.PAT0)
                    ext = ".pat0";
                else if (group.Type == BRESGroupNode.BRESGroupType.VIS0)
                    ext = ".vis0";
                else if (group.Type == BRESGroupNode.BRESGroupType.SCN0)
                    ext = ".scn0";
                foreach (BRESEntryNode entry in group.Children)
                {
                    //Find file name for entry
                    foreach (FileInfo info in files)
                    {
                        if (info.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase) && info.Name.Equals(entry.Name + ext, StringComparison.OrdinalIgnoreCase))
                        {
                            entry.Replace(info.FullName);
                            break;
                        }
                    }
                }
            }
        }

        public void ImportFolder(string inFolder)
        {
            DirectoryInfo dir = new DirectoryInfo(inFolder);
            FileInfo[] files;
            
            files = dir.GetFiles();
            foreach (FileInfo info in files)
            {
                string ext = Path.GetExtension(info.FullName).ToUpper();
                if (ext == ".PNG" || ext == ".TGA" || ext == ".BMP" || ext == ".JPG" || ext == ".JPEG" || ext == ".GIF" || ext == ".TIF" || ext == ".TIFF")
                {
                    using (TextureConverterDialog dlg = new TextureConverterDialog())
                    {
                        dlg.ImageSource = info.FullName;
                        dlg.ShowDialog(null, this);
                    }
                }
                else if (ext == ".TEX0")
                {
                    TEX0Node node = NodeFactory.FromFile(null, info.FullName) as TEX0Node;
                    GetOrCreateFolder<TEX0Node>().AddChild(node);
                }
                else if (ext == ".MDL0")
                {
                    MDL0Node node = NodeFactory.FromFile(null, info.FullName) as MDL0Node;
                    GetOrCreateFolder<MDL0Node>().AddChild(node);
                }
                else if (ext == ".CHR0")
                {
                    CHR0Node node = NodeFactory.FromFile(null, info.FullName) as CHR0Node;
                    GetOrCreateFolder<CHR0Node>().AddChild(node);
                }
                else if (ext == ".CLR0")
                {
                    CLR0Node node = NodeFactory.FromFile(null, info.FullName) as CLR0Node;
                    GetOrCreateFolder<CLR0Node>().AddChild(node);
                }
                else if (ext == ".SRT0")
                {
                    SRT0Node node = NodeFactory.FromFile(null, info.FullName) as SRT0Node;
                    GetOrCreateFolder<SRT0Node>().AddChild(node);
                }
                else if (ext == ".SHP0")
                {
                    SHP0Node node = NodeFactory.FromFile(null, info.FullName) as SHP0Node;
                    GetOrCreateFolder<SHP0Node>().AddChild(node);
                }
                else if (ext == ".PAT0")
                {
                    PAT0Node node = NodeFactory.FromFile(null, info.FullName) as PAT0Node;
                    GetOrCreateFolder<PAT0Node>().AddChild(node);
                }
                else if (ext == ".VIS0")
                {
                    VIS0Node node = NodeFactory.FromFile(null, info.FullName) as VIS0Node;
                    GetOrCreateFolder<VIS0Node>().AddChild(node);
                }
            }            
        }

        private int _numEntries, _strOffset, _rootSize;
        StringTable _stringTable = new StringTable();
        public override int OnCalculateSize(bool force)
        {
            int size = BRESHeader.Size;
            _rootSize = 0x20 + (Children.Count * 0x10);

            //Get entry count and data start
            _numEntries = 0;
            //Children.Sort(NodeComparer.Instance);
            foreach (BRESGroupNode n in Children)
            {
                //n.Children.Sort(NodeComparer.Instance);
                _rootSize += (n.Children.Count * 0x10) + 0x18;
                _numEntries += n.Children.Count;
            }
            size += _rootSize;

            //Get strings and advance entry offset
            _stringTable.Clear();
            foreach (BRESGroupNode n in Children)
            {
                _stringTable.Add(n.Name);
                foreach (BRESEntryNode c in n.Children)
                {
                    size = size.Align(c.DataAlign) + c.CalculateSize(force);
                    c.GetStrings(_stringTable);
                }
            }
            _strOffset = size = size.Align(4);

            size += _stringTable.GetTotalSize();

            return size.Align(0x80);
        }

        public override void OnRebuild(VoidPtr address, int size, bool force)
        {
            BRESHeader* header = (BRESHeader*)address;
            *header = new BRESHeader(size, _numEntries + 1);

            ROOTHeader* rootHeader = header->First;
            *rootHeader = new ROOTHeader(_rootSize, Children.Count);

            ResourceGroup* pMaster = &rootHeader->_master;
            ResourceGroup* rGroup = (ResourceGroup*)pMaster->EndAddress;

            //Write string table
            _stringTable.WriteTable(address + _strOffset);

            VoidPtr dataAddr = (VoidPtr)rootHeader + _rootSize;

            int gIndex = 1;
            foreach (BRESGroupNode g in Children)
            {
                ResourceEntry.Build(pMaster, gIndex++, rGroup, (BRESString*)_stringTable[g.Name]);

                *rGroup = new ResourceGroup(g.Children.Count);
                ResourceEntry* nEntry = rGroup->First;

                int rIndex = 1;
                foreach (BRESEntryNode n in g.Children)
                {
                    //Align data
                    dataAddr = ((int)dataAddr).Align(n.DataAlign);

                    ResourceEntry.Build(rGroup, rIndex++, dataAddr, (BRESString*)_stringTable[n.Name]);

                    //Rebuild entry
                    int len = n._calcSize;
                    n.Rebuild(dataAddr, len, force);
                    n.PostProcess(address, dataAddr, len, _stringTable);
                    dataAddr += len;
                }
                g._changed = false;

                //Advance to next group
                rGroup = (ResourceGroup*)rGroup->EndAddress;
            }
            _stringTable.Clear();
        }

        public static BRESNode FromGIF(string file)
        {
            string s = Path.GetFileNameWithoutExtension(file);
            BRESNode b = new BRESNode() { _name = s };
            PAT0Node p = new PAT0Node() { _name = s };
            p.CreateEntry();

            PAT0TextureNode t = p.Children[0].Children[0] as PAT0TextureNode;

            GifDecoder d = new GifDecoder();
            d.Read(file);

            int f = d.GetFrameCount();
            using (TextureConverterDialog dlg = new TextureConverterDialog())
            {
                dlg.Source = (Bitmap)d.GetFrame(0);
                if (dlg.ShowDialog(null, b) == DialogResult.OK)
                {
                    for (int i = 1; i < f; i++)
                    {
                        dlg.Source = (Bitmap)d.GetFrame(i);
                        dlg.EncodeSource();
                    }
                }
            }

            return b;
        }

        internal static ResourceNode TryParse(DataSource source) { return ((BRESHeader*)source.Address)->_tag == BRESHeader.Tag ? new BRESNode() : null; }
    }

    public unsafe class BRESGroupNode : ResourceNode
    {
        internal ResourceGroup* Group { get { return (ResourceGroup*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.BRESGroup; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                switch (Type)
                {
                    case BRESGroupType.Textures:
                        return new Type[] { typeof(TEX0Node) };
                    case BRESGroupType.Palettes:
                        return new Type[] { typeof(PLT0Node) };
                    case BRESGroupType.Models:
                        return new Type[] { typeof(MDL0Node) };
                    case BRESGroupType.CHR0:
                        return new Type[] { typeof(CHR0Node) };
                    case BRESGroupType.CLR0:
                        return new Type[] { typeof(CLR0Node) };
                    case BRESGroupType.SRT0:
                        return new Type[] { typeof(SRT0Node) };
                    case BRESGroupType.SHP0:
                        return new Type[] { typeof(SHP0Node) };
                    case BRESGroupType.VIS0:
                        return new Type[] { typeof(VIS0Node) };
                    case BRESGroupType.SCN0:
                        return new Type[] { typeof(SCN0Node) };
                    case BRESGroupType.PAT0:
                        return new Type[] { typeof(PAT0Node) };
                    default:
                        return new Type[] { };
                }
                
            }
        }

        [Browsable(false)]
        public BRESGroupType Type
        {
            get { if (_type == BRESGroupType.None) GetFileType(); return _type; }
            set { _type = value; }
        }

        public BRESGroupNode() : base() { }
        public BRESGroupNode(string name) : base() { _name = name; }
        public BRESGroupNode(string name, BRESGroupType type) : base() { _name = name; Type = type; }

        public enum BRESGroupType
        {
            Textures,
            Palettes,
            Models,
            CHR0,
            CLR0,
            SRT0,
            SHP0,
            VIS0,
            SCN0,
            PAT0,
            External,
            None
        }

        public BRESGroupType _type = BRESGroupType.None;

        public override void RemoveChild(ResourceNode child)
        {
            if ((Children.Count == 1) && (Children.Contains(child)))
                Parent.RemoveChild(this);
            else
                base.RemoveChild(child);
        }

        public override bool OnInitialize()
        {
            return Group->_numEntries > 0;
        }

        public void GetFileType()
        {
            if (_name == "Textures(NW4R)" || Children[0] is TEX0Node)
                Type = BRESGroupType.Textures;
            if (_name == "Palettes(NW4R)" || Children[0] is PLT0Node)
                Type = BRESGroupType.Palettes;
            if (_name == "3DModels(NW4R)" || Children[0] is MDL0Node)
                Type = BRESGroupType.Models;
            if (_name == "AnmChr(NW4R)" || Children[0] is CHR0Node)
                Type = BRESGroupType.CHR0;
            if (_name == "AnmClr(NW4R)" || Children[0] is CLR0Node)
                Type = BRESGroupType.CLR0;
            if (_name == "AnmTexSrt(NW4R)" || Children[0] is SRT0Node)
                Type = BRESGroupType.SRT0;
            if (_name == "AnmShp(NW4R)" || Children[0] is SHP0Node)
                Type = BRESGroupType.SHP0;
            if (_name == "AnmVis(NW4R)" || Children[0] is VIS0Node)
                Type = BRESGroupType.VIS0;
            if (_name == "AnmScn(NW4R)" || Children[0] is SCN0Node)
                Type = BRESGroupType.SCN0;
            if (_name == "AnmPat(NW4R)" || Children[0] is PAT0Node)
                Type = BRESGroupType.PAT0;
            if (_name == "External" || Children[0] is RASDNode)
                Type = BRESGroupType.External;
        }

        public override void OnPopulate()
        {
            ResourceGroup* group = Group;
            for (int i = 0; i < group->_numEntries; i++)
            {
                BRESCommonHeader* hdr = (BRESCommonHeader*)group->First[i].DataAddress;
                if (NodeFactory.FromAddress(this, hdr, hdr->_size) == null)
                  new BRESEntryNode().Initialize(this, hdr, hdr->_size);
            }
            if (Type == BRESGroupType.None) GetFileType();
        }
    }

    public class AnimationNode : BRESEntryNode
    {
        [Browsable(true)]
        public virtual int FrameCount { get { return 0; } set { } }
        [Browsable(true)]
        public virtual bool Loop { get { return false; } set { } }
    }

    public unsafe class BRESEntryNode : ResourceNode
    {
        internal BRESCommonHeader* CommonHeader { get { return (BRESCommonHeader*)WorkingSource.Address; } }

        [Browsable(false)]
        public virtual int DataAlign { get { return 4; } }

        [Browsable(false)]
        public BRESNode BRESNode { get { return ((_parent != null) && (Parent.Parent is BRESNode)) ? Parent.Parent as BRESNode : null; } }

        public override bool OnInitialize()
        {
            SetSizeInternal(CommonHeader->_size);
            return false;
        }

        internal virtual void GetStrings(StringTable strings)
        {
            strings.Add(Name);
        }

        public override unsafe void Export(string outPath)
        {
            Rebuild();

            StringTable table = new StringTable();
            GetStrings(table);

            int dataLen = WorkingUncompressed.Length.Align(4);
            int size = dataLen + table.GetTotalSize();

            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(size);
                using (FileMap map = FileMap.FromStream(stream))
                {
                    System.Memory.Move(map.Address, WorkingUncompressed.Address, (uint)WorkingUncompressed.Length);
                    table.WriteTable(map.Address + dataLen);
                    PostProcess(null, map.Address, WorkingUncompressed.Length, table);
                }
            }
            table.Clear();
        }

        internal protected virtual void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {
            BRESCommonHeader* header = (BRESCommonHeader*)dataAddress;

            if (bresAddress)
                header->_bresOffset = (int)bresAddress - (int)header;

            header->_size = dataLength;
        }
    }
}
