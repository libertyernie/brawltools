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
using System.Globalization;

namespace BrawlLib.SSBB.ResourceNodes
{
    //Credit to PhantomWings for researching RELs and coding Module Editors 1, 2 & 3
    public unsafe class RELNode : ARCEntryNode, ModuleNode
    {
        internal RELHeader* Header { get { return (RELHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.REL; } }

        public static SortedList<int, string> _idNames = new SortedList<int, string>()
        {
            {0, "main.dol"},
            {1, "sora_scene"},
            {2, "sora_menu_main"},
            {3, "sora_menu_tour"},
            {4, "sora_menu_qm"},
            {5, "sora_menu_edit"},
            {6, "sora_menu_collect_viewer"},
            {7, "sora_menu_replay"},
            {8, "sora_menu_snap_shot"},
            {9, "sora_menu_event"},
            {10, "sora_menu_sel_char"},
            {11, "sora_menu_sel_stage"},
            {12, "sora_menu_game_over"},
            {13, "sora_menu_intro"},
            {14, "sora_menu_friend_list"},
            {15, "sora_menu_watch"},
            {16, "sora_menu_name"},
            {17, "sora_menu_sel_char_access"},
            {18, "sora_menu_rule"},
            {19, "sora_menu_simple_ending"},
            {20, "sora_minigame"},
            {21, "sora_menu_time_result"},
            {22, "sora_menu_boot"},
            {23, "sora_menu_challenger"},
            {24, "sora_menu_title"},
            {25, "sora_menu_title_sunset"},
            {26, "sora_menu_fig_get_demo"},
            {27, "sora_melee"},
            {28, "sora_adv_menu_name"},
            {29, "sora_adv_menu_visual"},
            {30, "sora_adv_menu_sel_char"},
            {31, "sora_adv_menu_sel_map"},
            {32, "sora_adv_menu_difficulty"},
            {33, "sora_adv_menu_game_over"},
            {34, "sora_adv_menu_result"},
            {35, "sora_adv_menu_save_load"},
            {36, "sora_adv_menu_seal"},
            {37, "sora_adv_menu_ending"},
            {38, "sora_adv_menu_telop"},
            {39, "sora_adv_menu_save_point"},
            {40, "sora_adv_stage"},
            {41, "sora_enemy"},
            {42, "st_battles"},
            {43, "st_battle"},
            {44, "st_config"},
            {45, "st_final"},
            {46, "st_dolpic"},
            {47, "st_mansion"},
            {48, "st_mariopast"},
            {49, "st_kart"},
            {50, "st_donkey"},
            {51, "st_jungle"},
            {52, "st_pirates"},
            {53, "st_oldin"},
            {54, "st_norfair"},
            {55, "st_orpheon"},
            {56, "st_crayon"},
            {57, "st_halberd"},
            {58, "st_starfox"},
            {59, "st_stadium"},
            {60, "st_tengan"},
            {61, "st_fzero"},
            {62, "st_ice"},
            {63, "st_gw"},
            {64, "st_emblem"},
            {65, "st_madein"},
            {66, "st_earth"},
            {67, "st_palutena"},
            {68, "st_famicom"},
            {69, "st_newpork"},
            {70, "st_village"},
            {71, "st_metalgear"},
            {72, "st_greenhill"},
            {73, "st_pictchat"},
            {74, "st_plankton"},
            {75, "st_dxshrine"},
            {76, "st_dxyorster"},
            {77, "st_dxgarden"},
            {78, "st_dxonett"},
            {79, "st_dxgreens"},
            {80, "st_dxrcruise"},
            {81, "st_dxbigblue"},
            {82, "st_dxcorneria"},
            {83, "st_dxpstadium"},
            {84, "st_dxzebes"},
            {85, "st_stageedit"},
            {86, "st_otrain"},
            {87, "st_heal"},
            {88, "st_homerun"},
            {89, "st_tbreak"},
            {90, "st_croll"},
            {91, "ft_mario"},
            {92, "ft_donkey"},
            {93, "ft_link"},
            {94, "ft_samus"},
            {95, "ft_yoshi"},
            {96, "ft_kirby"},
            {97, "ft_fox"},
            {98, "ft_pikachu"},
            {99, "ft_luigi"},
            {100, "ft_captain"},
            {101, "ft_ness"},
            {102, "ft_koopa"},
            {103, "ft_peach"},
            {104, "ft_zelda"},
            {105, "ft_iceclimber"},
            {106, "ft_marth"},
            {107, "ft_gamewatch"},
            {108, "ft_falco"},
            {109, "ft_ganon"},
            {110, "ft_wario"},
            {111, "ft_metaknight"},
            {112, "ft_pit"},
            {113, "ft_pikmin"},
            {114, "ft_lucas"},
            {115, "ft_diddy"},
            {116, "ft_poke"},
            {117, "ft_dedede"},
            {118, "ft_lucario"},
            {119, "ft_ike"},
            {120, "ft_robot"},
            {121, "ft_toonlink"},
            {122, "ft_snake"},
            {123, "ft_sonic"},
            {124, "ft_purin"},
            {125, "ft_wolf"},
            {126, "ft_zako"},
        };

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

        public byte? _stageID; // null if it's not a stage .rel
        public byte[] _itemIDs; // null if it's not an online training room .rel

        [Category("Relocatable Module")]
        public uint ModuleID { get { return ID; } set { if (value > 0) { ID = value; SignalPropertyChange(); } } }
        [Browsable(false)]
        public new uint ID { get { return _id; } set { _id = value; } }

        [Category("Relocatable Module")]
        public string ModuleName { get { return _idNames.ContainsKey((int)ID) ? _idNames[(int)ID] : ""; } }
        
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

        #region Stage module conversion - designer properties
        [Category("Relocatable Module")]
        [TypeConverter(typeof(DropDownListStageIDs))]
        public string StageID {
            get {
                if (_stageID == null) return "N/A";
                Stage stage = Stage.Stages.Where(s => s.ID == _stageID).FirstOrDefault();
                return _stageID.Value.ToString("X2") + (stage == null ? "" : (" - " + stage.Name));
            }
            set {
                // Don't try to set the stage ID if it's not a stage module
                if (_stageID == null) return;
				if (value.Length < 2) return;
                _stageID = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
                SignalPropertyChange();
            }
        }

        [Category("Relocatable Module")]
        [TypeConverter(typeof(DropDownListItemIDs))]
        public string ItemID {
            get {
                if (_itemIDs == null) return "N/A";
                if (_itemIDs.Distinct().Count() > 1) {
                    // If the four IDs are different (not sure why they would be)
                    return "Mismatched (" + string.Join(",", _itemIDs.Select(b => b.ToString("X2"))) + ")";
                }
                string item = Items.Where(s => s.StartsWith(_itemIDs[0].ToString("X2"))).FirstOrDefault();
                return item ?? _itemIDs[0].ToString("X2");
            }
            set {
                // Don't try to set the item ID if it's not an Online Training Room module
                if (_itemIDs == null) return;
				if (value.Length < 2) return;
				if (value.Contains("(")) value = value.Substring(value.IndexOf("(")+1);
				if (value.Contains(")")) value = value.Substring(0, value.IndexOf(")"));
				string[] split = value.Split(',');
				if (split.Length == 4) {
					for (int i = 0; i < 4; i++) _itemIDs[i] = byte.Parse(split[i], NumberStyles.HexNumber);
					SignalPropertyChange();
				} else {
					byte b = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
					for (int i = 0; i < 4; i++) _itemIDs[i] = b;
					SignalPropertyChange();
				}
            }
        }
        #endregion

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

            // Stage module conversion
            byte* bptr = (byte*)WorkingUncompressed.Address;
            int offset = findStageIDOffset();
            _stageID = offset < 0 ? (byte?)null : bptr[offset];

            if (nodeContainsString("stOnlineTrainning")) {
                // File must be online training room .rel file
                _itemIDs = new byte[OTrainItemOffsets.Length];
                for (int i = 0; i < OTrainItemOffsets.Length; i++) {
                    _itemIDs[i] = bptr[OTrainItemOffsets[i]];
                }
            }
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

            // Stage module conversion
            byte* bptr = (byte*)address;
            if (_stageID != null) bptr[findStageIDOffset()] = _stageID.Value;

            if (_itemIDs != null) {
                // File must be online training room .rel file
                for (int i = 0; i < _itemIDs.Length; i++) {
                    bptr[OTrainItemOffsets[i]] = _itemIDs[i];
                }
            }
        }

        public static List<RELNode> _files = new List<RELNode>();

        #region Stage module conversion
        private unsafe static int arrayIndexOf(void* haystack, int length, byte[] needle) {
            byte?[] b = new byte?[needle.Length];
            for (int i = 0; i < b.Length; i++) {
                b[i] = needle[i];
            }
            return arrayIndexOf(haystack, length, b);
        }

        private unsafe static int arrayIndexOf(void* haystack, int length, byte?[] needle) {
            byte* ptr = (byte*)haystack;
            int indexToCheck = 0;
            for (int i = 0; i < length; i++) {
                byte? b = needle[indexToCheck];
                if ((b ?? ptr[i]) == ptr[i]) {
                    indexToCheck++;
                    if (indexToCheck == needle.Length) return i + 1 - needle.Length;
                } else {
                    indexToCheck = 0;
                }
            }
            return -1;
        }

        private unsafe int findStageIDOffset() {
            byte?[] searchFor = { 0x38, null, 0x00, null,
                                  0x38, 0xA5, 0x00, 0x00,
                                  0x38, 0x80, 0x00 };
            int index = arrayIndexOf(WorkingUncompressed.Address, WorkingUncompressed.Length, searchFor);
            return index < 0
                ? -1
                : index + 11;
        }

        private unsafe bool nodeContainsString(string s) {
            return arrayIndexOf(WorkingUncompressed.Address,
                WorkingUncompressed.Length,
                Encoding.UTF8.GetBytes(s)) > 0;
        }

        /* These are absolute offsets - land within section 1.
         * When BrawlBox rebuilds st_otrain.rel, it cuts out 16 bytes from 0xA50-0xA60,
         * but those come after these, so we should be ok. */
        private readonly static int[] OTrainItemOffsets = {
            // Changing some values but not others has strange effects
            1223,
            1347, // this appears to be some sort of "if" condition
            1371,
            1627,
        };

        public readonly static string[] Items = {
            "00 - Assist Trophy",
            "01 - Franklin Badge",
            "02 - Banana Peel",
            "03 - Barrel",
            "04 - Beam Sword",
            "05 - Bill (coin mode)",
            "06 - Bob-Omb",
            "07 - Crate",
            "08 - Bumper",
            "09 - Capsule",
            "0A - Rolling Crate",
            "0B - CD",
            "0C - Gooey Bomb",
            "0D - Cracker Launcher",
            "0E - Cracker Launcher Shot",
            "0F - Coin",
            "10 - Superspicy Curry",
            "11 - Superspice Curry Shot",
            "12 - Deku Nut",
            "13 - Mr. Saturn",
            "14 - Dragoon Part",
            "15 - Dragoon Set",
            "16 - Dragoon Sight",
            "17 - Trophy",
            "18 - Fire Flower",
            "19 - Fire Flower Shot",
            "1A - Freezie",
            "1B - Golden Hammer",
            "1C - Green Shell",
            "1D - Hammer",
            "1E - Hammer Head",
            "1F - Fan",
            "20 - Heart Container",
            "21 - Homerun Bat",
            "22 - Party Ball",
            "23 - Manaphy Heart",
            "24 - Maxim Tomato",
            "25 - Poison Mushroom",
            "26 - Super Mushroom",
            "27 - Metal Box",
            "28 - Hothead",
            "29 - Pitfall",
            "2A - Pokéball",
            "2B - Blast Box",
            "2C - Ray Gun",
            "2D - Ray Gun Shot",
            "2E - Lipstick",
            "2F - Lipstick Flower",
            "30 - Lipstick Shot",
            "31 - Sandbag",
            "32 - Screw Attack",
            "33 - Sticker",
            "34 - Motion-Sensor Bomb",
            "35 - Timer",
            "36 - Smart Bomb",
            "37 - Smash Ball",
            "38 - Smoke Screen",
            "39 - Spring",
            "3A - Star Rod",
            "3B - Star Rod Shot",
            "3C - Soccer Ball",
            "3D - Super Scope",
            "3E - Super Scope shot",
            "3F - Star",
            "40 - Food",
            "41 - Team Healer",
            "42 - Lightning",
            "43 - Unira",
            "44 - Bunny Hood",
            "45 - Warpstar"};
        #endregion
    }
}