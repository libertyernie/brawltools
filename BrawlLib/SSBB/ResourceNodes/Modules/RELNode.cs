using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using BrawlLib.IO;
using System.PowerPcAssembly;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    //Credit to PhantomWings for researching RELs and coding Module Editors 1, 2 & 3
    public unsafe class RELNode : ARCEntryNode, ModuleNode
    {
        internal RELHeader* Header { get { return (RELHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.REL; } }

        public static SortedList<int, string> _idNames = new SortedList<int, string>();

        static RELNode()
        {
            string loc = Application.StartupPath + "/REL ID List.txt";
            if (File.Exists(loc))
                using (StreamReader sr = new StreamReader(loc))
                    for (int i = 0; !sr.EndOfStream; i++)
                    {
                        string s = sr.ReadLine();
                        string[] sp = s.Split(' ');
                        if (sp.Length < 2 || String.IsNullOrEmpty(sp[1]))
                            continue;
                        int x;
                        if (int.TryParse(sp[0], out x))
                            _idNames[x] = sp[1];
                    }
        }

        [Browsable(false)]
        public ModuleSectionNode[] Sections { get { return _sections; } }
        public ModuleSectionNode[] _sections;

        public uint _id;
        public int _linkNext; //0
        public int _linkPrev; //0
        public uint _numSections;

        public uint _infoOffset;
        public uint _nameOffset;
        public uint _nameSize;
        public uint _version;

        public uint _bssSize;
        public uint _relOffset;
        public uint _impOffset;
        public uint _impSize;

        public byte _prologSection;
        public byte _epilogSection;
        public byte _unresolvedSection;
        public byte _bssSection;

        public uint _prologOffset;
        public uint _epilogOffset;
        public uint _unresolvedOffset;

        public uint _moduleAlign = 32;
        public uint _bssAlign = 8;
        public uint _fixSize;

        [Category("Relocatable Module")]
        public uint ModuleID { get { return ID; } set { if (value > 0) { ID = value; SignalPropertyChange(); } } }
        [Browsable(false)]
        public new uint ID { get { return _id; } set { _id = value; } }
        
        //[Category("REL")]
        //public int NextLink { get { return _linkNext; } }
        //[Category("REL")]
        //public int PrevLink { get { return _linkPrev; } }
        //[Category("REL")]
        //public uint SectionCount { get { return _numSections; } }
        
        //[Category("REL")]
        //public uint SectionInfoOffset { get { return _infoOffset; } }
        [Category("Relocatable Module")]
        public uint NameOffset { get { return _nameOffset; } set { _nameOffset = value; SignalPropertyChange(); } }
        [Category("Relocatable Module")]
        public uint NameSize { get { return _nameSize; } set { _nameSize = value; SignalPropertyChange(); } }
        [Category("Relocatable Module")]
        public uint Version { get { return _version; } }

        //[Category("REL")]
        //public uint BSSSize { get { return _bssSize; } }
        //[Category("REL")]
        //public uint RelOffset { get { return _relOffset; } }
        //[Category("REL")]
        //public uint ImpOffset { get { return _impOffset; } }
        //[Category("REL")]
        //public uint ImpSize { get { return _impSize; } }

        //[Category("REL")]
        //public uint PrologSection { get { return _prologSection; } }
        //[Category("REL")]
        //public uint EpilogSection { get { return _epilogSection; } }
        //[Category("REL")]
        //public uint UnresolvedSection { get { return _unresolvedSection; } }
        //[Category("REL")]
        //public uint BSSSection { get { return _bssSection; } }

        //[Category("REL")]
        //public uint PrologOffset { get { return _prologOffset; } }
        //[Category("REL")]
        //public uint EpilogOffset { get { return _epilogOffset; } }
        //[Category("REL")]
        //public uint UnresolvedOffset { get { return _unresolvedOffset; } }

        //[Category("REL")]
        //public uint ModuleAlign { get { return _moduleAlign; } }
        //[Category("REL")]
        //public uint BSSAlign { get { return _bssAlign; } }
        //[Category("REL")]
        //public uint FixSize { get { return _fixSize; } }

        public Relocation
            _prologReloc = null,
            _epilogReloc = null,
            _unresReloc = null;

        public override bool OnInitialize()
        {
            _files.Add(this);

            _name = Path.GetFileName(_origPath);

            _id = Header->_info._id;
            _linkNext = Header->_info._link._linkNext; //0
            _linkPrev = Header->_info._link._linkPrev; //0
            _numSections = Header->_info._numSections;
            _infoOffset = Header->_info._sectionInfoOffset;
            _nameOffset = Header->_info._nameOffset;
            _nameSize = Header->_info._nameSize;
            _version = Header->_info._version;

            _bssSize = Header->_bssSize;
            _relOffset = Header->_relOffset;
            _impOffset = Header->_impOffset;
            _impSize = Header->_impSize;
            _prologSection = Header->_prologSection;
            _epilogSection = Header->_epilogSection;
            _unresolvedSection = Header->_unresolvedSection;
            _bssSection = Header->_bssSection;
            _prologOffset = Header->_prologOffset;
            _epilogOffset = Header->_epilogOffset;
            _unresolvedOffset = Header->_unresolvedOffset;

            _moduleAlign = Header->_moduleAlign;
            _bssAlign = Header->_bssAlign;
            _fixSize = Header->_commandOffset;

            _imports = new SortedDictionary<uint, List<RELLink>>();
            for (int i = 0; i < Header->ImportListCount; i++)
            {
                RELImportEntry* entry = (RELImportEntry*)&Header->Imports[i];
                uint id = (uint)entry->_moduleId;
                _imports.Add(id, new List<RELLink>());

                RELLink* link = (RELLink*)(WorkingUncompressed.Address + (uint)entry->_offset);
                do { _imports[id].Add(*link); }
                while ((link++)->_type != RELLinkType.End);
            }

            return true;
        }

        public SortedDictionary<uint, List<RELLink>> _imports = new SortedDictionary<uint,List<RELLink>>();
        public override void OnPopulate()
        {
            _sections = new ModuleSectionNode[_numSections];
            for (int i = 0; i < _numSections; i++)
            {
                RELSectionEntry entry = Header->SectionInfo[i];
                ModuleSectionNode section = _sections[i] = new ModuleSectionNode();

                section._isCodeSection = entry.IsCodeSection;
                section._dataOffset = entry.Offset;
                section._dataSize = entry._size;

                section.Initialize(this, WorkingUncompressed.Address + Header->SectionInfo[i].Offset, (int)Header->SectionInfo[i]._size);
            }

            ApplyRelocations();
        }

        public void ApplyRelocations()
        {
            foreach (ModuleSectionNode r in Sections)
                r.ClearCommands();

            int offset = 0;
            int i = 0;
            foreach (uint x in _imports.Keys)
            {
                List<RELLink> cmds = _imports[x];
                ModuleSectionNode section = null;
                foreach (RELLink link in cmds)
                    if (link._type == RELLinkType.Section)
                    {
                        offset = 0;
                        section = Sections[link._section];
                    }
                    else
                    {
                        offset += (int)(ushort)link._prevOffset;

                        if (link._type == RELLinkType.End || link._type == RELLinkType.IncrementOffset) 
                            continue;

                        if (link._type == RELLinkType.MrkRef)
                        {
                            Console.WriteLine("Mark Ref");
                            continue;
                        }

                        if (section != null)
                            section.SetCommandAtOffset(offset, new RelCommand(x, section.Index, link));
                    }
                i++;
            }

            ModuleDataNode s;
            if (_prologReloc == null)
            {
                s = _sections[Header->_prologSection];
                offset = (int)Header->_prologOffset - (int)s._offset;
            }
            else
            {
                s = _prologReloc._section;
                offset = _prologReloc._index * 4;
            }
            _prologReloc = s.GetRelocationAtOffset(offset);
            if (_prologReloc != null)
                _prologReloc._prolog = true;

            if (_epilogReloc == null)
            {
                s = _sections[Header->_epilogSection];
                offset = (int)Header->_epilogOffset - (int)s._offset;
            }
            else
            {
                s = _epilogReloc._section;
                offset = _epilogReloc._index * 4;
            }
            _epilogReloc = s.GetRelocationAtOffset(offset);
            if (_epilogReloc != null)
                _epilogReloc._epilog = true;

            if (_unresReloc == null)
            {
                s = _sections[Header->_unresolvedSection];
                offset = (int)Header->_unresolvedOffset - (int)s._offset;
            }
            else
            {
                s = _unresReloc._section;
                offset = _unresReloc._index * 4;
            }
            _unresReloc = s.GetRelocationAtOffset(offset);
            if (_unresReloc != null)
                _unresReloc._unresolved = true;
        }

        class ImportData
        {
            public bool _first = true;
            public uint _lastOffset = 0;
        }

        public void GenerateImports()
        {
            _imports.Clear();
            Dictionary<uint, ImportData> data = new Dictionary<uint, ImportData>();
            foreach (ModuleSectionNode s in _sections)
            {
                foreach (ImportData e in data.Values)
                {
                    e._first = true;
                    e._lastOffset = 0;
                }
                uint i = 0;
                uint offset = 0;
                List<RELLink> cmds;
                foreach (Relocation loc in s._relocations)
                {
                    if (loc.Command != null)
                    {
                        RelCommand cmd = loc.Command;
                        ImportData d;
                        uint id = cmd._moduleID;

                        if (_imports.ContainsKey(id))
                        {
                            cmds = _imports[id];
                            d = data[id];
                        }
                        else
                        {
                            _imports.Add(id, cmds = new List<RELLink>());
                            data.Add(id, d = new ImportData() { _first = true, _lastOffset = 0 });
                        }

                        if (d._first)
                        {
                            cmds.Add(new RELLink() { _type = RELLinkType.Section, _section = (byte)s.Index });
                            d._first = false;
                        }

                        offset = i * 4 + (cmd.IsHalf ? 2u : 0);
                        uint diff = offset - d._lastOffset;
                        while (offset - d._lastOffset > 0xFFFF)
                        {
                            d._lastOffset += 0xFFFF;
                            cmds.Add(new RELLink() { _type = RELLinkType.IncrementOffset, _section = 0, _value = 0, _prevOffset = 0xFFFF });
                        }

                        byte targetSection = (byte)cmd._targetSectionId;
                        RELLinkType type = (RELLinkType)cmd._command;
                        uint val = cmd._addend;

                        cmds.Add(new RELLink() { _type = type, _section = targetSection, _value = val, _prevOffset = (ushort)diff });

                        d._lastOffset = offset;
                    }
                    i++;
                }
            }

            foreach (List<RELLink> cmds in _imports.Values)
                cmds.Add(new RELLink() { _type = RELLinkType.End });
        }

        public override int OnCalculateSize(bool force)
        {
            GenerateImports();

            int size = RELHeader.Size + Children.Count * RELSectionEntry.Size + _imports.Keys.Count * RELImportEntry.Size;
            foreach (ModuleSectionNode s in Children)
            {
                if (s.Index > 3)
                    size = size.Align(8);
                int r = s.CalculateSize(true);
                if (!s._isBSSSection)
                    size += r;
            }
            foreach (List<RELLink> s in _imports.Values)
                size += s.Count * RELLink.Size;
            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            RELHeader* header = (RELHeader*)address;

            header->_info._id = _id;
            header->_info._link._linkNext = 0;
            header->_info._link._linkPrev = 0;
            header->_info._numSections = (uint)Children.Count;
            header->_info._sectionInfoOffset = RELHeader.Size;
            header->_info._nameOffset = _nameOffset;
            header->_info._nameSize = _nameSize;
            header->_info._version = _version;

            header->_moduleAlign = 0x20;
            header->_bssAlign = 0x8;
            header->_commandOffset = 0;

            bool bssFound = false;
            RELSectionEntry* sections = (RELSectionEntry*)(address + RELHeader.Size);
            VoidPtr dataAddr = address + RELHeader.Size + Children.Count * RELSectionEntry.Size;
            foreach (ModuleSectionNode s in Children)
                if (s._dataBuffer.Length != 0)
                {
                    int i = s.Index;

                    if (i > 3)
                    {
                        int off = (int)(dataAddr - address);
                        int aligned = off.Align(8);
                        int diff = aligned - off;
                        dataAddr += diff;
                    }

                    if (!s._isBSSSection)
                    {
                        sections[i]._offset = (uint)(dataAddr - address);
                        sections[i].IsCodeSection = s.HasCode;

                        s.Rebuild(dataAddr, s._calcSize, true);
                        dataAddr += s._calcSize;
                    }
                    else
                    {
                        bssFound = true;
                        sections[i]._offset = 0;
                        header->_bssSize = (uint)s._calcSize;

                        //This is always 0 it seems
                        header->_bssSection = 0;
                    }
                    sections[i]._size = (uint)s._calcSize;
                }

            if (!bssFound)
            {
                header->_bssSize = 0;
                header->_bssSection = 0;
            }

            if (_prologReloc != null)
            {
                header->_prologSection = (byte)_prologReloc._section.Index;
                header->_prologOffset = (uint)sections[_prologReloc._section.Index].Offset + (uint)_prologReloc._index * 4;
            }
            else
            {
                header->_prologOffset = 0;
                header->_prologSection = 0;
            }

            if (_epilogReloc != null)
            {
                header->_epilogSection = (byte)_epilogReloc._section.Index;
                header->_epilogOffset = (uint)sections[_epilogReloc._section.Index].Offset + (uint)_epilogReloc._index * 4;
            }
            else
            {
                header->_epilogSection = 0;
                header->_epilogOffset = 0;
            }

            if (_unresReloc != null)
            {
                header->_unresolvedSection = (byte)_unresReloc._section.Index;
                header->_unresolvedOffset = (uint)sections[_unresReloc._section.Index].Offset + (uint)_unresReloc._index * 4;
            }
            else
            {
                header->_unresolvedSection = 0;
                header->_unresolvedOffset = 0;
            }
            
            RELImportEntry* imports = (RELImportEntry*)dataAddr;
            header->_impOffset = (uint)(dataAddr - address);
            dataAddr = (VoidPtr)imports + (header->_impSize = (uint)_imports.Keys.Count * RELImportEntry.Size);
            header->_relOffset = (uint)(dataAddr - address);

            List<uint> k = new List<uint>();
            foreach (uint s in _imports.Keys)
                if (s != ModuleID && s != 0)
                    k.Add(s);

            k.Sort();

            foreach (uint s in _imports.Keys)
                if (s == ModuleID)
                {
                    k.Add(s);
                    break;
                }

            foreach (uint s in _imports.Keys)
                if (s == 0)
                {
                    k.Add(s);
                    break;
                }

            for (int i = 0; i < k.Count; i++)
            {
                uint id = k[i];
                uint offset = (uint)(dataAddr - address);
                if (id == ModuleID)
                    header->_commandOffset = offset;

                imports[i]._moduleId = id;
                imports[i]._offset = offset;

                RELLink* link = (RELLink*)dataAddr;
                foreach (RELLink n in _imports[k[i]])
                    *link++ = n;
                dataAddr = link;
            }
        }

        public static List<RELNode> _files = new List<RELNode>();
    }
}