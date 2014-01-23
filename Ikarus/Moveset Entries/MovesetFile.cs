using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;
using BrawlLib.Wii.Compression;
using System.Runtime.InteropServices;
using Ikarus;

namespace BrawlLib.SSBB.ResourceNodes
{
    public abstract unsafe class MovesetEntry
    {
        //Properties
        [Browsable(false)]
        public int RebuildOffset { get { return _rebuildAddr == null || BaseAddress == null || _rebuildAddr < BaseAddress ? -1 : (int)_rebuildAddr - (int)BaseAddress; } }
        [Browsable(false)]
        public VoidPtr BaseAddress { get { return _root.BaseAddress; } }
        [Browsable(false)]
        public MDL0Node Model { get { return _root.Model; } }
        public MovesetFile _root;
        [Browsable(false)]
        public bool External { get { return _externalEntry != null; } }
        [Browsable(false)]
        public bool HasChanged
        {
            get { return _changed || (_root != null && _root.ChangedEntries.Contains(this)); }
            set { _changed = value; if (_root != null) _root.ChangedEntries.Remove(this); }
        }
        [Browsable(false)]
        public virtual string Name
        {
            get { return _name; }
        }

        //Variables
        public int _size = -1;
        public string _name;
        public int _offset;
        public ExternalEntry _externalEntry = null;
        public int _offsetID = 0;
        public VoidPtr _rebuildAddr = null;
        public int _entryLength = 0, _childLength = 0;
        public int _lookupCount = 0;
        public List<VoidPtr> _lookupOffsets = new List<VoidPtr>();
        private bool _changed;
        internal int _index;
        public int _calcSize;

        [Browsable(false)]
        public virtual bool IsDirty { get { return HasChanged; } set { HasChanged = value; } }
        [Browsable(false)]
        public virtual int Index { get { return _index; } }

        //Functions
        /// <summary>
        /// Call this when an entry's size changes
        /// </summary>
        public void SignalRebuildChange() { if (_root != null) _root.RebuildNeeded = true; HasChanged = true; }
        /// <summary>
        /// Call this when a property has been changed but the size remains the same
        /// </summary>
        public void SignalPropertyChange() { HasChanged = true; }

        /// <summary>
        /// Returns an offset of the given address relative to the base address.
        /// </summary>
        public int Offset(VoidPtr address) { return _root.Offset(address); }
        /// <summary>
        /// Returns an address of the given offset relative to the base address.
        /// </summary>
        public VoidPtr Address(int offset) { return BaseAddress + offset; }
        /// <summary>
        /// Returns the size of the entry at the given offset.
        /// </summary>
        public int GetSize(int offset) { return _root.GetSize(offset); }

        /// <summary>
        /// Use this to parse a node of a specific type at the given offset.
        /// This will automatically add the node to the entry cache, get its size,
        /// set its offset value, and attach its external entry if it has one.
        /// Be sure to send the proper constructor parameters for the given type
        /// as well, or an error will be thrown.
        /// </summary>
        public T Parse<T>(int offset, params object[] parameters) where T : MovesetEntry
        {
            //The attributes entry is an exception; it's the only entry that has an offset of 0.
            if (parameters.Length > 0 && parameters[0] is string && (string)parameters[0] == "Attributes")
                parameters = new object[0];
            else if (offset <= 0)
                return null;

            T n = Activator.CreateInstance(typeof(T), parameters) as T;
            n.Setup(_root, offset);
            n.Parse(Address(offset));
            return n;
        }
        /// <summary>
        /// Use this to parse a node of a specific type at the given address.
        /// This will automatically add the node to the entry cache, get its size,
        /// set its offset value, and attach its external entry if it has one.
        /// Be sure to send the proper constructor parameters for the given type
        /// as well, or an error will be thrown.
        /// </summary>
        public T Parse<T>(VoidPtr address, params object[] parameters) where T : MovesetEntry
        {
            int offset = Offset(address);
            if (offset <= 0)
                return null;

            T n = Activator.CreateInstance(typeof(T), parameters) as T;
            n.Setup(_root, offset);
            n.Parse(address);
            return n;
        }
        internal void Setup(MovesetFile node, int offset) { Setup(node, offset, null); }
        internal void Setup(MovesetFile node, int offset, string name)
        {
            _name = name;
            _root = node;
            _offset = offset;
            if (_size <= 0)
                _size = _root.GetSize(_offset);
            _root.EntryCache[_offset] = this;
            if ((_externalEntry = _root.TryGetExternal(offset)) != null)
                _externalEntry.References.Add(this);
        }
        public int GetSize() { return _calcSize = OnGetSize(); }
        public int Write(VoidPtr address) { OnWrite(address); return RebuildOffset; }

        //Overridable functions
        public virtual void Parse(VoidPtr address) { }
        protected virtual void OnWrite(VoidPtr address) { }
        protected virtual int OnGetSize() { return 0; }
        protected virtual void PostProcess(LookupManager lookupOffsets) { }

        public override string ToString() { return String.IsNullOrEmpty(Name) ? base.ToString() : Name; }
    }

    public unsafe class MovesetFile : ARCEntryNode
    {
        public MovesetFile(CharName character) { _character = character; }

        #region Variables & Properties

        /// <summary>
        /// Returns the name of the character this moveset is for.
        /// This can be directed converted into CharFolder for easy access to necessary files
        /// </summary>
        public CharName Character { get { return _character; } }
        private CharName _character;

        /// <summary>
        /// Returns the address after the moveset header that all offsets use as a base.
        /// This should only be used when parsing or writing.
        /// </summary>
        public VoidPtr BaseAddress { get { return MovesetFile.Builder == null ? WorkingUncompressed.Address + 0x20 : MovesetFile.Builder._baseAddress; } }

        /// <summary>
        /// Returns all entries in the moveset that have had a property changed.
        /// Changed entries do not necessarily mean that a rebuild is needed.
        /// </summary>
        public BindingList<MovesetEntry> ChangedEntries { get { return _changedEntries; } }
        private BindingList<MovesetEntry> _changedEntries = new BindingList<MovesetEntry>();

        /// <summary>
        /// True if the moveset file has had something added or removed and must be rebuilt.
        /// </summary>
        public bool RebuildNeeded { get { return _rebuildNeeded; } set { _rebuildNeeded = value; } }
        private bool _rebuildNeeded = false;

        /// <summary>
        /// List of external subroutines located in Fighter.pac.
        /// </summary>
        public BindingList<ExternalEntry> ReferenceList { get { return _referenceList; } }
        private BindingList<ExternalEntry> _referenceList;
        /// <summary>
        /// List of important entries located in this moveset file.
        /// </summary>
        public BindingList<ExternalEntry> SectionList { get { return _sectionList; } }
        private BindingList<ExternalEntry> _sectionList;

        #region Important Sections
        /// <summary>
        /// The section that contains all information for a character's specific moveset.
        /// </summary>
        public DataSection Data { get { return _data; } }
        private DataSection _data = null;
        /// <summary>
        /// The section that contains all common information located in Fighter.pac
        /// </summary>
        public DataCommonSection DataCommon { get { return _dataCommon; } }
        private DataCommonSection _dataCommon = null;
        /// <summary>
        /// The the first section that contains information for item movesets.
        /// </summary>
        public AnimParamSection AnimParam { get { return _animParam; } }
        private AnimParamSection _animParam = null;
        /// <summary>
        /// The the second section that contains information for item movesets.
        /// </summary>
        //public SubParamSection SubParam { get { return _subParam; } }
        //private SubParamSection _subParam = null;
        #endregion

        /// <summary>
        /// A list of all action scripts referenced only through offset events.
        /// </summary>
        public BindingList<Script> SubRoutines { get { return _subRoutines; } }
        private BindingList<Script> _subRoutines;
        public BindingList<Script> CommonSubRoutines { get { return _commonSubRoutines; } }
        private BindingList<Script> _commonSubRoutines;

        public BindingList<ActionEntry> Actions
        {
            get { return _actions; }
            set
            {
                if ((value == null && _actions != null) || (value != null && _actions == null) || value.Count != _actions.Count)
                    SignalPropertyChange();
                _actions = value;
                SignalPropertyChange();
            }
        }
        private BindingList<ActionEntry> _actions;

        /// <summary>
        /// Provides the size of any entry based on its offset.
        /// </summary>
        public SortedList<int, int> LookupSizes { get { return _lookupSizes; } }
        private SortedList<int, int> _lookupSizes;

        /// <summary>
        /// Provides easy access to any entry in the moveset using its original offset.
        /// Use only when parsing.
        /// </summary>
        internal SortedDictionary<int, MovesetEntry> EntryCache { get { return _entryCache; } }
        private SortedDictionary<int, MovesetEntry> _entryCache;

        /// <summary>
        /// Provides easy access to the model that this moveset will affect.
        /// </summary>
        public MDL0Node Model { get { return MainForm.Instance._mainControl.TargetModel; } }

        internal List<MovesetEntry> _postProcessEntries;
        internal List<List<int>>[] _scriptOffsets;

        #endregion

        #region Parsing

        /// <summary>
        /// Returns a node of the given type at the offset in the moveset file.
        /// </summary>
        public T Parse<T>(int offset) where T : MovesetEntry
        {
            T n = Activator.CreateInstance(typeof(T)) as T;
            n.Setup(this, offset);
            n.Parse(BaseAddress + offset);
            return n;
        }

        internal int _dataSize;
        internal bool _initializing = false;
        public override bool OnInitialize()
        {
            //Start initializing. 
            //This enables some functions for use.
            _initializing = true;

            //Get structs
            MovesetHeader* hdr = (MovesetHeader*)WorkingUncompressed.Address;
            sStringTable* stringTable = hdr->StringTable;
            _dataSize = hdr->_fileSize;

            //Create lists
            _subRoutines = new BindingList<Script>();
            _commonSubRoutines = new BindingList<Script>();
            _referenceList = new BindingList<ExternalEntry>();
            _sectionList = new BindingList<ExternalEntry>();
            _lookupSizes = new SortedList<int, int>();
            _actions = new BindingList<ActionEntry>();
            _entryCache = new SortedDictionary<int, MovesetEntry>();
            _postProcessEntries = new List<MovesetEntry>();
            _scriptOffsets = new List<List<int>>[5];
            for (int i = 0; i < 5; i++)
            {
                _scriptOffsets[i] = new List<List<int>>();
                for (int x = 0; x < (i == 0 ? 2 : i == 1 ? 4 : 1); x++)
                    _scriptOffsets[i].Add(new List<int>());
            }

            //Read lookup offsets first and use them to get entry sizes at each offset.
            bint* lookup = hdr->LookupEntries;
            //First add each offset to the dictionary with size of 0.
            //The dictionary will sort the offsets automatically, in case they aren't already.
            for (int i = 0; i < hdr->_lookupEntryCount; i++)
            {
                int w = *(bint*)Address(lookup[i]);
                if (!_lookupSizes.ContainsKey(w))
                    _lookupSizes.Add(w, 0);
            }
            //Now go through each offset and calculate the size with the offset difference.
            int prev = 0; bool first = true;
            int[] t = _lookupSizes.Keys.ToArray();
            for (int i = 0; i < t.Length; i++)
            {
                int off = t[i];
                if (first)
                    first = false;
                else
                    _lookupSizes[prev] = off - prev;
                prev = off;
            }
            //The last entry in the moveset file goes right up to the lookup offsets.
            _lookupSizes[prev] = Offset(lookup) - prev;

            //Parse references
            int numRefs = hdr->_externalSubRoutineCount;
            if (numRefs > 0)
            {
                sStringEntry* entries = (sStringEntry*)hdr->ExternalSubRoutines;
                for (int i = 0; i < numRefs; i++)
                {
                    ExternalEntry e = Parse<ExternalEntry>(entries[i]._dataOffset);
                    e._name = stringTable->GetString(entries[i]._stringOffset);
                    _referenceList.Add(e);
                }
            }

            //Parse sections
            int numSections = hdr->_dataTableEntryCount;
            if (numSections > 0)
            {
                int dataIndex = -1;
                sStringEntry* entries = (sStringEntry*)hdr->DataTable;
                for (int i = 0; i < numSections; i++)
                {
                    ExternalEntry e = null;
                    int off = entries[i]._dataOffset;
                    string name = stringTable->GetString(entries[i]._stringOffset);
                    switch (name)
                    {
                        //Don't parse data sections until the very end
                        case "data":
                        case "dataCommon":
                            dataIndex = i;
                            _sectionList.Add(null);
                            continue;
                        //case "animParam":
                        //    e = _animParam = Parse<AnimParamSection>(off);
                        //    break;
                        //case "subParam":
                        //    e = _subParam = Parse<SubParamSection>(off);
                        //    break;
                        default:
                            if (name.Contains("AnimCmd"))
                            {
                                e = Parse<Script>(off);
                                _commonSubRoutines.Add(e as Script);
                            }
                            else
                                e = Parse<RawData>(off);
                            break;
                    }
                    e.DataOffsets.Add(off);
                    e._name = name;
                    _sectionList.Add(e);
                }

                if (dataIndex >= 0)
                {
                    int off = entries[dataIndex]._dataOffset;
                    string name = stringTable->GetString(entries[dataIndex]._stringOffset);
                    switch (name)
                    {
                        case "data":
                            _data = Parse<DataSection>(off);
                            _data.DataOffsets.Add(off);
                            _data._name = name;
                            _sectionList[dataIndex] = _data;
                            break;
                        case "dataCommon":
                            _dataCommon = Parse<DataCommonSection>(off);
                            _dataCommon.DataOffsets.Add(off);
                            _dataCommon._name = name;
                            _sectionList[dataIndex] = _dataCommon;
                            break;
                    }
                }
            }

            while (_postProcessEntries.Count > 0)
            {
                //Make a copy of the post process nodes
                MovesetEntry[] arr = _postProcessEntries.ToArray();
                //Clear the original array so it can be repopulated
                _postProcessEntries.Clear();
                //Parse subroutines - may add more entries to post process
                foreach (MovesetEntry e in arr)
                    if (e is EventOffset)
                        ((EventOffset)e).LinkScript();
            }

            //Sort subroutines by offset
            _subRoutines = new BindingList<Script>(_subRoutines.OrderBy(x => x._offset).ToList());
            //Add the proper information to the sorted subroutines
            int q = 0;
            foreach (Script s in _subRoutines)
            {
                s._name = String.Format("[{0}] SubRoutine", s._index = q++);
                foreach (EventOffset e in s.ActionRefs)
                    e._offsetInfo = new ScriptOffsetInfo(ListValue.SubRoutines, TypeValue.None, s.Index);
            }

            return _initializing = false;
        }

        //This calculate data entry sizes.
        //One array will be initialized with each offset,
        //then another will be created and sorted using the same temp entries.
        //This will allow for sorted offsets and easy indexing of the same entries.
        internal static int[] CalculateSizes(int end, bint* hdr, int count, bool data, params int[] ignore)
        {
            Temp[] t = new Temp[count];
            for (int i = 0; i < count; i++)
                if (Array.IndexOf(ignore, i) < 0)
                    t[i] = new Temp((int)hdr[i], 0);
                else
                    t[i] = null;

            if (data) t[2]._offset = 1;
            Temp[] sorted = t.Where(x => x != null).OrderBy(x => x._offset).ToArray();
            if (data) t[2]._offset -= 1;

            for (int i = 0; i < sorted.Length; i++)
                sorted[i]._size = ((i < sorted.Length - 1) ? sorted[i + 1]._offset : end) - sorted[i]._offset;
            return t.Select(x => x._size).ToArray();
        }

        #endregion

        #region Saving

        /// <summary>
        /// Returns the moveset builder of the moveset currently being written.
        /// Use only after calling CalculateSize or Rebuild.
        /// </summary>
        public static NewMovesetBuilder Builder { get { return _currentlyBuilding == null ? null : _currentlyBuilding._builder; } }
        public static MovesetFile _currentlyBuilding = null;

        internal NewMovesetBuilder _builder;
        public override int OnCalculateSize(bool force)
        {
            _currentlyBuilding = this;
            return (_builder = new NewMovesetBuilder()).CalcSize(this);
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            _currentlyBuilding = this;
            _builder.Write(this, address, length);
            _currentlyBuilding = null;
        }
        #endregion

        #region Parse functions
        /// <summary>
        /// Use this only when parsing or writing.
        /// </summary>
        public int Offset(VoidPtr address) 
        { 
            if (!_initializing && _currentlyBuilding != this) 
                throw new Exception("Error: Not initializing or rebuilding."); 
            return (int)(address - BaseAddress);
        }
        /// <summary>
        /// Use this only when parsing or writing.
        /// </summary>
        public VoidPtr Address(int offset)
        {
            if (!_initializing && _currentlyBuilding != this)
                throw new Exception("Error: Not initializing or rebuilding.");
            return BaseAddress + offset;
        }
        /// <summary>
        /// Use this only when parsing.
        /// </summary>
        public int GetSize(int offset)
        {
            if (!_initializing)
                throw new Exception("Error: Not initializing.");
            if (_lookupSizes.ContainsKey(offset))
            {
                int size = _lookupSizes[offset];
                _lookupSizes.Remove(offset);
                return size;
            }
            return -1;
        }
        /// <summary>
        /// Use this only when parsing.
        /// </summary>
        public ExternalEntry TryGetExternal(int offset)
        {
            if (!_initializing)
                throw new Exception("Error: Not initializing."); 
            foreach (ExternalEntry e in _referenceList)
                foreach (int i in e.DataOffsets)
                    if (i == offset)
                        return e;
            foreach (ExternalEntry e in _sectionList)
                if (e!= null && e.DataOffsets.Count > 0 && e.DataOffsets[0] == offset)
                    return e;
            return null;
        }
        /// <summary>
        /// Use this only when parsing.
        /// </summary>
        public MovesetEntry GetEntry(int offset)
        {
            if (!_initializing)
                throw new Exception("Error: Not initializing."); 
            if (_entryCache.ContainsKey(offset))
                return _entryCache[offset];
            return null;
        }
        /// <summary>
        /// Use this only when parsing.
        /// </summary>
        public Script GetScript(int offset)
        {
            if (!_initializing)
                throw new Exception("Error: Not initializing."); 

            if (offset < 0)
                return null;

            MovesetEntry e = GetEntry(offset);

            if (e is Script)
                return e as Script;
            else if (e is Event)
                return ((Event)e)._script;

            return null;
        }
        /// <summary>
        /// Use this only when parsing.
        /// </summary>
        internal ScriptOffsetInfo GetScriptLocation(int offset)
        {
            if (!_initializing)
                throw new Exception("Error: Not initializing."); 

            //Create new offset info
            ScriptOffsetInfo info = new ScriptOffsetInfo();

            //Check if the offset is legit
            if (offset <= 0)
                return info;

            info.list = ListValue.Actions;

            //Search action offsets
            for (info.type = 0; (int)info.type < 2; info.type++)
                if ((info.index = _scriptOffsets[(int)info.list][(int)info.type].IndexOf(offset)) != -1)
                    return info;

            info.list++;

            //Search subaction offsets
            for (info.type = 0; (int)info.type < 4; info.type++)
                if ((info.index = _scriptOffsets[(int)info.list][(int)info.type].IndexOf(offset)) != -1)
                    return info;

            info.type = TypeValue.None;
            info.list++;

            //Search subroutine offsets
            if ((info.index = _scriptOffsets[(int)info.list][0].IndexOf(offset)) != -1)
                return info;

            info.list++;

            //Search reference entry offsets
            MovesetEntry e = GetEntry(offset);
            if (e is ExternalEntry && e != null)
            {
                info.index = e.Index;
                return info;
            }

            //Set values to null
            info.list++;
            info.type = TypeValue.None;
            info.index = -1;

            //Continue searching dataCommon
            if (_dataCommon != null)
            {
                info.list++;

                //Search screen tint offsets
                if ((info.index = _scriptOffsets[3][0].IndexOf(offset)) != -1)
                    return info;

                info.list++;

                //Search flash overlay offsets
                if ((info.index = _scriptOffsets[4][0].IndexOf(offset)) != -1)
                    return info;

                info.list = ListValue.Null;
            }
            return info;
        }
#endregion

        /// <summary>
        /// Returns the script at the given location.
        /// </summary>
        public Script GetScript(ScriptOffsetInfo info)
        {
            ListValue list = info.list;
            TypeValue type = info.type;
            int index = info.index;

            if ((list > ListValue.References && _dataCommon == null) || list == ListValue.Null || index == -1)
                return null;

            switch (list)
            {
                case ListValue.Actions:
                    if ((type == TypeValue.Entry || type == TypeValue.Exit) && index >= 0 && index < Actions.Count)
                        return (Script)Actions[index].GetWithType((int)type);
                    break;
                case ListValue.SubActions:
                    if (_data != null && index >= 0 && index < _data.SubActions.Count)
                        return (Script)_data.SubActions[index].GetWithType((int)type);
                    break;
                case ListValue.SubRoutines:
                    if (index >= 0 && index < _subRoutines.Count)
                        return (Script)_subRoutines[index];
                    break;
                case ListValue.FlashOverlays:
                    if (_dataCommon != null && index >= 0 && index < _dataCommon.FlashOverlays.Count)
                        return (Script)_dataCommon.FlashOverlays[index];
                    break;
                case ListValue.ScreenTints:
                    if (_dataCommon != null && index >= 0 && index < _dataCommon.ScreenTints.Count)
                        return (Script)_dataCommon.ScreenTints[index];
                    break;
            }
            return null;
        }

        /// <summary>
        /// Characters like kirby and wario need to swap bone indices. 
        /// Use this function to get the proper index.
        /// </summary>
        public void GetBoneIndex(ref int boneIndex)
        {
            //if (Character == CharName.Wario || Character == CharName.Kirby)
            //{
            //    if (_data != null)
            //        if (_data.warioParams8 != null)
            //        {
            //            RawParamList p1 = _data.warioParams8.Children[0] as RawParamList;
            //            RawParamList p2 = _data.warioParams8.Children[1] as RawParamList;
            //            bint* values = (bint*)p2.AttributeBuffer.Address;
            //            int i = 0;
            //            for (; i < p2.AttributeBuffer.Length / 4; i++)
            //                if (values[i] == boneIndex)
            //                    break;
            //            if (p1.AttributeBuffer.Length / 4 > i)
            //            {
            //                int value = -1;
            //                if ((value = (int)(((bint*)p1.AttributeBuffer.Address)[i])) >= 0)
            //                {
            //                    boneIndex = value;
            //                    return;
            //                }
            //                else
            //                    boneIndex -= 400;
            //            }
            //        }
            //}
        }
        /// <summary>
        /// Characters like kirby and wario need to swap bone indices. 
        /// Use this function to set the proper index.
        /// </summary>
        public void SetBoneIndex(ref int boneIndex)
        {
            //if (Character == CharName.Wario || Character == CharName.Kirby)
            //{
            //    if (_data != null)
            //        if (_data.warioParams8 != null)
            //        {
            //            RawParamList p1 = _data.warioParams8.Children[0] as RawParamList;
            //            RawParamList p2 = _data.warioParams8.Children[1] as RawParamList;
            //            bint* values = (bint*)p2.AttributeBuffer.Address;
            //            int i = 0;
            //            for (; i < p1.AttributeBuffer.Length / 4; i++)
            //                if (values[i] == boneIndex)
            //                    break;
            //            if (p2.AttributeBuffer.Length / 4 > i)
            //            {
            //                int value = -1;
            //                if ((value = ((bint*)p2.AttributeBuffer.Address)[i]) >= 0)
            //                {
            //                    boneIndex = value;
            //                    return;
            //                }
            //            }
            //        }
            //}
        }
    }

    public enum ListValue
    {
        Actions = 0,
        SubActions = 1,
        SubRoutines = 2,
        References = 3,
        Null = 4,
        FlashOverlays = 5,
        ScreenTints = 6
    }

    public enum TypeValue
    {
        None = -1,
        Main = 0,
        GFX = 1,
        SFX = 2,
        Other = 3,
        Entry = 0,
        Exit = 1,
    }

    public unsafe class OffsetValue : MovesetEntry
    {
        [Category("Offset Entry")]
        public int DataOffset { get { return _dataOffset; } }
        private int _dataOffset = 0;

        public override void Parse(VoidPtr address) { _dataOffset = *(bint*)address; }
    }

    public unsafe class ExternalEntry : MovesetEntry
    {
        [Browsable(false)]
        public List<int> DataOffsets { get { return _dataOffsets; } set { _dataOffsets = value; } }
        private List<int> _dataOffsets = new List<int>();

        [Browsable(false)]
        public List<MovesetEntry> References { get { return _references; } set { _references = value; } }
        private List<MovesetEntry> _references = new List<MovesetEntry>();

        public override void Parse(VoidPtr address)
        {
            _dataOffsets = new List<int>();
            _dataOffsets.Add(_offset);
            int offset = *(bint*)address;
            while (offset > 0)
            {
                _dataOffsets.Add(offset);
                offset = *(bint*)(BaseAddress + offset);

                //Infinite loops are NO GOOD
                if (_dataOffsets.Contains(offset))
                    break;
            }
        }
    }

    internal class Temp
    {
        public int _offset;
        public int _size;
        public Temp(int offset, int size)
        {
            _offset = offset;
            _size = size;
        }
    }
}