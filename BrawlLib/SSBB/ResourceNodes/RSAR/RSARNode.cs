using System;
using BrawlLib.SSBBTypes;
using System.IO;
using System.Collections.Generic;
using BrawlLib.Wii.Audio;
using System.ComponentModel;
using System.Linq;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSARNode : NW4RNode
    {
        internal RSARHeader* Header { get { return (RSARHeader*)WorkingSource.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RSAR; } }

        [Category("Sound Archive")]
        public ushort SeqSoundCount { get { return ftr._seqSoundCount; } set { ftr._seqSoundCount = value; SignalPropertyChange(); } }
        [Category("Sound Archive")]
        public ushort SeqTrackCount { get { return ftr._seqTrackCount; } set { ftr._seqTrackCount = value; SignalPropertyChange(); } }
        [Category("Sound Archive")]
        public ushort StrmSoundCount { get { return ftr._strmSoundCount; } set { ftr._strmSoundCount = value; SignalPropertyChange(); } }
        [Category("Sound Archive")]
        public ushort StrmTrackCount { get { return ftr._strmTrackCount; } set { ftr._strmTrackCount = value; SignalPropertyChange(); } }
        [Category("Sound Archive")]
        public ushort StrmChannelCount { get { return ftr._strmChannelCount; } set { ftr._strmChannelCount = value; SignalPropertyChange(); } }
        [Category("Sound Archive")]
        public ushort WaveSoundCount { get { return ftr._waveSoundCount; } set { ftr._waveSoundCount = value; SignalPropertyChange(); } }
        [Category("Sound Archive")]
        public ushort WaveTrackCount { get { return ftr._waveTrackCount; } set { ftr._waveTrackCount = value; SignalPropertyChange(); } }

        public List<RSAREntryNode>[] _infoCache = new List<RSAREntryNode>[5];

        internal INFOFooter ftr;

        private List<RSARFileNode> _files;
        [Browsable(false)]
        public List<RSARFileNode> Files { get { return _files; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            if (_name == null)
                if (_origPath != null)
                    _name = Path.GetFileNameWithoutExtension(_origPath);
                else
                    _name = "Sound Archive";

            _files = new List<RSARFileNode>();
            _children = new List<ResourceNode>();

            //Retrieve all files to attach to entries
            GetFiles();
            OnPopulate();
            return true;
        }

        public RSARGroupNode _nullGroup;
        public override void OnPopulate()
        {
            //Enumerate entries, attaching them to the files.
            RSARHeader* rsar = Header;
            SYMBHeader* symb = rsar->SYMBBlock;
            sbyte* offset = (sbyte*)symb + 8;
            buint* stringOffsets = symb->StringOffsets;

            VoidPtr baseAddr = (VoidPtr)rsar->INFOBlock + 8;
            ruint* typeList = (ruint*)baseAddr;

            //Iterate through group types
            for (int i = 0; i < 5; i++)
            {
                _infoCache[i] = new List<RSAREntryNode>();
                Type t = null;

                RuintList* list = (RuintList*)((uint)baseAddr + typeList[i]);
                sbyte* str, end;

                switch (i)
                {
                    case 0: t = typeof(RSARSoundNode); break;
                    case 1: t = typeof(RSARBankNode); break;
                    case 2: t = typeof(RSARPlayerInfoNode); break;
                    case 3: continue; //Files
                    case 4: t = typeof(RSARGroupNode); break; //Last group entry is null
                }

                for (int x = 0; x < list->_numEntries; x++)
                {
                    VoidPtr addr = list->Get(baseAddr, x);

                    ResourceNode parent = this;
                    RSAREntryNode n = Activator.CreateInstance(t) as RSAREntryNode;
                    n._origSource = n._uncompSource = new DataSource(addr, 0);
                    n._infoIndex = x;

                    if (i == 4 && x == list->_numEntries - 1)
                    {
                        n._name = "<null>";
                        n._parent = this;
                        _nullGroup = n as RSARGroupNode;
                    }
                    else
                    {
                        str = offset + stringOffsets[n.StringId];

                        for (end = str; *end != 0; end++) ;
                        while ((--end > str) && (*end != '_')) ;

                        if (end > str)
                        {
                            parent = CreatePath(parent, str, (int)end - (int)str);
                            n._name = new String(end + 1);
                        }
                        else
                            n._name = new String(str);
                    }

                    n.Initialize(parent, addr, 0);
                    _infoCache[i].Add(n);
                }
            }
            ftr = *(INFOFooter*)((uint)baseAddr + typeList[5]);

            foreach (RSARFileNode n in Files)
                if (!(n is RSARExtFileNode))
                    n.GetName();

            _rootIds = new int[4];
            _symbCache = new List<SYMBMaskEntry>[4];
            bint* offsets = (bint*)((VoidPtr)symb + 12);
            for (int i = 0; i < 4; i++)
            {
                _symbCache[i] = new List<SYMBMaskEntry>();
                SYMBMaskHeader* hdr = (SYMBMaskHeader*)((VoidPtr)symb + 8 + offsets[i]);
                //Console.WriteLine("Root Index = " + hdr->_rootId);
                _rootIds[i] = hdr->_rootId;
                for (int x = 0; x < hdr->_numEntries; x++)
                {
                    SYMBMaskEntry* e = &hdr->Entries[x];
                    _symbCache[i].Add(*e);
                    //Console.WriteLine(String.Format("[{5}] {0}, {1}, {2} - {4}", e->_bit != -1 ? e->_bit.ToString().PadLeft(3) : "   ", e->_leftId != -1 ? e->_leftId.ToString().PadLeft(3) : "   ", e->_rightId != -1 ? e->_rightId.ToString().PadLeft(3) : "   ", e->_index != -1 ? e->_index.ToString().PadLeft(3) : "   ", new string(offset + stringOffsets[e->_stringId]), x.ToString().PadLeft(3)));
                }
            }
            //Sort(true);
        }
        
        public int[] _rootIds;
        public List<SYMBMaskEntry>[] _symbCache;

        private void GetFiles()
        {
            INFOFileHeader* fileHeader;
            INFOFileEntry* fileEntry;
            RuintList* entryList;
            INFOGroupHeader* group;
            INFOGroupEntry* gEntry;
            RSARFileNode n;
            DataSource source;
            RSARHeader* rsar = Header;

            //SYMBHeader* symb = rsar->SYMBBlock;
            //sbyte* offset = (sbyte*)symb + 8;
            //buint* stringOffsets = symb->StringOffsets;

            //Get ruint collection from info header
            VoidPtr infoCollection = rsar->INFOBlock->_collection.Address;

            //Info has 5 groups:
            //Sounds 0
            //Banks 1
            //Types 2
            //Files 3
            //Groups 4

            //Convert to ruint buffer
            ruint* groups = (ruint*)infoCollection;

            //Get file ruint list at file offset (groups[3])
            RuintList* fileList = (RuintList*)((uint)groups + groups[3]);

            //Get the info list
            RuintList* groupList = rsar->INFOBlock->Groups;

            //Loop through the ruint offsets to get all files
            for (int x = 0; x < fileList->_numEntries; x++)
            {
                //Get the file header for the file info
                fileHeader = (INFOFileHeader*)(infoCollection + fileList->Entries[x]);
                entryList = fileHeader->GetList(infoCollection);
                if (entryList->_numEntries == 0)
                {
                    //Must be external file.
                    n = new RSARExtFileNode();
                    source = new DataSource(fileHeader, 0);
                }
                else
                {
                    //Use first entry
                    fileEntry = (INFOFileEntry*)entryList->Get(infoCollection, 0);
                    //Find group with matching ID
                    group = (INFOGroupHeader*)groupList->Get(infoCollection, fileEntry->_groupId);
                    //Find group entry with matching index
                    gEntry = (INFOGroupEntry*)group->GetCollection(infoCollection)->Get(infoCollection, fileEntry->_index);

                    //Create node and parse
                    source = new DataSource((int)rsar + group->_headerOffset + gEntry->_headerOffset, gEntry->_headerLength);
                    if ((n = NodeFactory.GetRaw(source) as RSARFileNode) == null)
                        n = new RSARFileNode();
                    n._audioSource = new DataSource((int)rsar + group->_waveDataOffset + gEntry->_dataOffset, gEntry->_dataLength);
                    n._infoHdr = fileHeader;
                }
                n._fileIndex = x;
                n._parent = this; //This is so that the node won't add itself to the child list.
                n.Initialize(this, source);
                _files.Add(n);
            }

            //foreach (ResourceNode r in _files)
            //    r.Populate();
        }

        internal void Sort(bool sortChildren)
        {
            if (_children != null)
            {
                _children.Sort(CompareNodes);
                if (sortChildren)
                    foreach (ResourceNode n in _children)
                        if (n is RSARFolderNode)
                            ((RSARFolderNode)n).Sort(true);
            }
        }

        internal static int CompareNodes(ResourceNode n1, ResourceNode n2)
        {
            bool is1Folder = n1 is RSARFolderNode;
            bool is2Folder = n2 is RSARFolderNode;

            if (is1Folder != is2Folder)
                return is1Folder ? -1 : 1;

            return String.Compare(n1._name, n2._name);
        }

        private ResourceNode CreatePath(ResourceNode parent, sbyte* str, int length)
        {
            ResourceNode current;

            int len;
            char* cPtr;
            sbyte* start, end;
            sbyte* ceil = str + length;
            while (str < ceil)
            {
                for (end = str; ((end < ceil) && (*end != '_')); end++) ;
                len = (int)end - (int)str;

                current = null;
                foreach (ResourceNode n in parent._children)
                {
                    if ((n._name.Length != len) || !(n is RSARFolderNode))
                        continue;

                    fixed (char* p = n._name)
                        for (cPtr = p, start = str; (start < end) && (*start == *cPtr); start++, cPtr++) ;

                    if (start == end)
                    {
                        current = n;
                        break;
                    }
                }
                if (current == null)
                {
                    current = new RSARFolderNode();
                    current._name = new String(str, 0, len);
                    current._parent = parent;
                    parent._children.Add(current);
                }

                str = end + 1;
                parent = current;
            }

            return parent;
        }

        private RSAREntryList _entryList = new RSAREntryList();
        private RSARConverter _converter = new RSARConverter();
        public override int OnCalculateSize(bool force)
        {
            _entryList.Clear();
            _entryList._files = Files;
            _converter = new RSARConverter();

            foreach (ResourceNode n in Children)
                if (n is RSARFolderNode)
                    ((RSARFolderNode)n).GetStrings(null, 0, _entryList);
                else if (n is RSAREntryNode)
                    ((RSAREntryNode)n).GetStrings(null, 0, _entryList);

            _entryList.SortStrings();
            _nullGroup._rebuildIndex = _entryList._groups.Count;

            return _converter.CalculateSize(_entryList, this);
        }

        public string t = null;
        bool l = true;
        public VoidPtr _rebuildBase;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            _rebuildBase = address;

            int symbLen, infoLen, fileLen;

            RSARHeader* rsar = (RSARHeader*)address;
            SYMBHeader* symb = (SYMBHeader*)(address + 0x40);
            INFOHeader* info;
            FILEHeader* data;

            info = (INFOHeader*)((int)symb + (symbLen = _converter.EncodeSYMBBlock(symb, _entryList, this)));
            data = (FILEHeader*)((int)info + (infoLen = _converter.EncodeINFOBlock(info, _entryList, this)));
            fileLen = _converter.EncodeFILEBlock(data, _entryList, this);

            rsar->Set(symbLen, infoLen, fileLen, VersionMinor);

            _entryList.Clear();

            //IsDirty = false;
        }

        internal static ResourceNode TryParse(DataSource source) { return ((RSARHeader*)source.Address)->_header._tag == RSARHeader.Tag ? new RSARNode() : null; }
    }

    public enum PanMode
    {
        Dual,      // Perform position processing for stereo as two monaural channels.
        Balance    // Process the volume balance for the left and right channels.
    }

    public enum PanCurve
    {
        Sqrt,             // Square root curve. The volume will be -3 dB in the center and 0 dB at both ends.
        Sqrt0DB,          // Square root curve. The volume will be 0 dB in the center and +3 dB at both ends.
        Sqrt0DBClamp,     // Square root curve. The volume will be 0 dB in the center and 0 dB at both ends.
        SinCos,           // Trigonometric curve. The volume will be -3 dB in the center and 0 dB at both ends.
        SinCos0DB,        // Trigonometric curve. The volume will be 0 dB in the center and +3 dB at both ends.
        SinCos0DBClamp,   // Trigonometric curve. The volume will be 0 dB in the center and 0 dB at both ends.
        Linear,           // Linear curve. The volume will be -6 dB in the center and 0 dB at both ends.
        Linear0DB,        // Linear curve. The volume will be 0 dB in the center and +6 dB at both ends.
        Linear0DBClamp    // Linear curve. The volume will be 0 dB in the center and 0 dB at both ends.
    }

    public enum DecayCurve
    {
        Logarithmic = 1, // Logarithmic curve
        Linear = 2  // Linear curve
    }

    public enum RegionTableType
    {
        Invalid = 0,
        Direct = 1,
        RangeTable = 2,
        IndexTable = 3,
        Null = 4
    }
}