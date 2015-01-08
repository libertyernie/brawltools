using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Ikarus.MovesetBuilder;
using BrawlLib.SSBB.ResourceNodes;
using Ikarus.ModelViewer;
using Ikarus.UI;

namespace Ikarus.MovesetFile
{
    public unsafe class MovesetNode : ARCEntryNode
    {
        public MovesetNode(CharName character) { _character = character; }

        #region Variables & Properties

        /// <summary>
        /// 
        /// </summary>
        [Browsable(true), Category("Moveset Node")]
        public int DataSize { get { return _dataSize; } }
        internal int _dataSize;

        [Browsable(false)]
        public bool Initializing { get { return _initializing; } }
        internal bool _initializing = false;

        /// <summary>
        /// Returns the name of the character this moveset is for.
        /// This can be directed converted into CharFolder for easy access to necessary files
        /// </summary>
        [Browsable(false)]
        public CharName Character { get { return _character; } }
        private CharName _character;

        /// <summary>
        /// Returns the address after the moveset header that all offsets use as a base.
        /// This should only be used when parsing or writing.
        /// </summary>
        [Browsable(false)]
        public VoidPtr BaseAddress { get { return MovesetNode.Builder == null ? WorkingUncompressed.Address + MovesetHeader.Size : MovesetNode.Builder._baseAddress; } }

        /// <summary>
        /// Returns all entries in the moveset that have had a property changed.
        /// Changed entries do not necessarily mean that a rebuild is needed.
        /// </summary>
        [Browsable(false)]
        public BindingList<MovesetEntryNode> ChangedEntries { get { return _changedEntries; } }
        private BindingList<MovesetEntryNode> _changedEntries = new BindingList<MovesetEntryNode>();

        /// <summary>
        /// True if the moveset file has had something added or removed and must be rebuilt.
        /// </summary>
        [Browsable(false)]
        public bool RebuildNeeded { get { return _rebuildNeeded; } set { _rebuildNeeded = value; } }
        private bool _rebuildNeeded = false;

        /// <summary>
        /// List of external subroutines located in Fighter.pac.
        /// </summary>
        [Browsable(false)]
        public BindingList<ExternalEntryNode> ReferenceList { get { return _referenceList; } }
        private BindingList<ExternalEntryNode> _referenceList;

        /// <summary>
        /// List of important entries located in this moveset file.
        /// </summary>
        [Browsable(false)]
        public BindingList<ExternalEntryNode> SectionList { get { return _sectionList; } }
        private BindingList<ExternalEntryNode> _sectionList;

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
        [Browsable(false)]
        public BindingList<Script> SubRoutines { get { return _subRoutines; } }
        private BindingList<Script> _subRoutines;

        /// <summary>
        /// A list of all action scripts referenced in the section list.
        /// </summary>
        [Browsable(false)]
        public BindingList<Script> CommonSubRoutines { get { return _commonSubRoutines; } }
        private BindingList<Script> _commonSubRoutines;

        [Browsable(false)]
        public BindingList<ActionEntry> Actions { get { return _actions; } }
        internal BindingList<ActionEntry> _actions;

        /// <summary>
        /// Provides the size of any entry based on its offset.
        /// </summary>
        [Browsable(false)]
        public SortedList<int, int> LookupSizes { get { return _lookupSizes; } }
        private SortedList<int, int> _lookupSizes;

        /// <summary>
        /// Provides easy access to any entry in the moveset using its original offset.
        /// Use only when parsing.
        /// </summary>
        internal SortedDictionary<int, MovesetEntryNode> EntryCache { get { return _entryCache; } }
        private SortedDictionary<int, MovesetEntryNode> _entryCache;

        /// <summary>
        /// Provides easy access to the model that this moveset will affect.
        /// </summary>
        public MDL0Node Model { get { return MainForm.Instance._mainControl.TargetModel as MDL0Node; } }

        internal List<MovesetEntryNode> _postProcessEntries;
        internal List<List<int>>[] _scriptOffsets;

        #endregion

        #region Parsing

        public override bool OnInitialize()
        {
            //Start initializing. 
            //This enables some functions for use.
            _initializing = true;

            MovesetHeader* hdr = (MovesetHeader*)WorkingUncompressed.Address;

            InitData(hdr);
            GetLookupSizes(hdr);
            ParseExternals(hdr);

            PostParse();
            CleanUp();

            return _initializing = false;
        }
        /// <summary>
        /// Initializes all variables.
        /// </summary>
        private void InitData(MovesetHeader* hdr)
        {
            //Get header values
            _dataSize = hdr->_fileSize;

            //Debug
            for (int i = 0; i < 3; i++)
            {
                int value = (&hdr->_pad1)[i];
                if (value != 0)
                    Console.WriteLine("MovesetNode InitData " + i);
            }

            //Create lists
            _subRoutines = new BindingList<Script>();
            _commonSubRoutines = new BindingList<Script>();
            _referenceList = new BindingList<ExternalEntryNode>();
            _sectionList = new BindingList<ExternalEntryNode>();
            _lookupSizes = new SortedList<int, int>();
            _actions = new BindingList<ActionEntry>();
            _entryCache = new SortedDictionary<int, MovesetEntryNode>();
            _postProcessEntries = new List<MovesetEntryNode>();
            _scriptOffsets = new List<List<int>>[5];
            for (int i = 0; i < 5; i++)
            {
                _scriptOffsets[i] = new List<List<int>>();
                for (int x = 0; x < (i == 0 ? 2 : i == 1 ? 4 : 1); x++)
                    _scriptOffsets[i].Add(new List<int>());
            }
        }
        /// <summary>
        /// Creates a table of offsets with a corresponding data size at each offset.
        /// </summary>
        private void GetLookupSizes(MovesetHeader* hdr)
        {
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
        }
        /// <summary>
        /// Parses references and section data.
        /// </summary>
        private void ParseExternals(MovesetHeader* hdr)
        {
            sStringTable* stringTable = hdr->StringTable;

            //Parse references
            int numRefs = hdr->_externalSubRoutineCount;
            if (numRefs > 0)
            {
                sStringEntry* entries = (sStringEntry*)hdr->ExternalSubRoutines;
                for (int i = 0; i < numRefs; i++)
                {
                    ExternalEntryNode e = Parse<ExternalEntryNode>(entries[i]._dataOffset);
                    e._name = stringTable->GetString(entries[i]._stringOffset);
                    _referenceList.Add(e);
                }
            }

            //Parse sections
            int numSections = hdr->_sectionCount;
            if (numSections > 0)
            {
                int dataIndex = -1;
                sStringEntry* entries = (sStringEntry*)hdr->Sections;
                for (int i = 0; i < numSections; i++)
                {
                    ExternalEntryNode entry = null;
                    int offset = entries[i]._dataOffset;
                    string name = stringTable->GetString(entries[i]._stringOffset);
                    switch (name)
                    {
                        //Don't parse data sections until the very end
                        case "data":
                        case "dataCommon":
                            dataIndex = i;
                            _sectionList.Add(null);
                            continue;
                        case "animParam":
                            entry = _animParam = Parse<AnimParamSection>(offset);
                            break;
                        //case "subParam":
                        //    entry = _subParam = Parse<SubParamSection>(offset);
                        //    break;
                        default:
                            if (name.Contains("AnimCmd"))
                            {
                                entry = Parse<Script>(offset);
                                _commonSubRoutines.Add(entry as Script);
                            }
                            else
                                entry = Parse<RawDataNode>(offset);
                            break;
                    }
                    entry.DataOffsets.Add(offset);
                    entry._name = name;
                    _sectionList.Add(entry);
                }

                //Now parse the data section. This contains all the main moveset information.
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
        }
        private void PostParse()
        {
            while (_postProcessEntries.Count > 0)
            {
                //Make a copy of the post process nodes
                MovesetEntryNode[] arr = _postProcessEntries.ToArray();
                //Clear the original array so it can be repopulated
                _postProcessEntries.Clear();
                //Parse subroutines - may add more entries to post process
                foreach (MovesetEntryNode e in arr)
                    if (e is EventOffset)
                        ((EventOffset)e).LinkScript();
            }
        }
        /// <summary>
        /// Makes all final changes before the parsed data is ready to be used.
        /// </summary>
        private void CleanUp()
        {
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
        }
        class Temp
        {
            public int _offset;
            public int _size;
            public Temp(int offset, int size)
            {
                _offset = offset;
                _size = size;
            }
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

        /// <summary>
        /// Returns a node of the given type at an offset in the moveset file.
        /// </summary>
        public T Parse<T>(int offset) where T : MovesetEntryNode
        {
            return MovesetEntryNode.Parse<T>(this, Address(offset));
        }
        /// <summary>
        /// Returns a node of the given type at an address in the moveset file.
        /// </summary>
        public T Parse<T>(VoidPtr address) where T : MovesetEntryNode
        {
            return MovesetEntryNode.Parse<T>(this, address);
        }

        #endregion

        #region Saving

        /// <summary>
        /// Returns the moveset builder of the moveset currently being written.
        /// This can only be used while calculating the size or rebuilding a moveset.
        /// </summary>
        public static NewMovesetBuilder Builder { get { return _currentlyBuilding == null ? null : _currentlyBuilding._builder; } }
        public static MovesetNode _currentlyBuilding = null;

        public bool IsRebuilding { get { return _builder != null && _builder.Rebuilding; } }
        public bool IsCalculatingSize { get { return _builder != null && _builder.CalculatingSize; } }

        internal NewMovesetBuilder _builder;
        public override int OnCalculateSize(bool force)
        {
            _currentlyBuilding = this;
            int size = (_builder = new NewMovesetBuilder(this)).GetSize();
            _currentlyBuilding = null;
            return size;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            _currentlyBuilding = this;
            _builder.Write(this, address, length);
            _currentlyBuilding = null;
            _builder = null;
        }
        #endregion

        #region Parse functions
        /// <summary>
        /// Use this only when parsing or writing.
        /// </summary>
        public int Offset(VoidPtr address) 
        { 
            if (!_initializing && _currentlyBuilding != this) 
                throw new Exception("Not initializing or rebuilding."); 
            return (int)(address - BaseAddress);
        }
        /// <summary>
        /// Use this only when parsing or writing.
        /// </summary>
        public VoidPtr Address(int offset)
        {
            if (!_initializing && _currentlyBuilding != this)
                throw new Exception("Not initializing or rebuilding.");
            return BaseAddress + offset;
        }
        /// <summary>
        /// Use this only when parsing.
        /// </summary>
        public int GetSize(int offset)
        {
            if (!_initializing)
                throw new Exception("Not initializing.");
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
        public ExternalEntryNode TryGetExternal(int offset)
        {
            if (!_initializing)
                throw new Exception("Not initializing."); 
            foreach (ExternalEntryNode e in _referenceList)
                foreach (int i in e.DataOffsets)
                    if (i == offset)
                        return e;
            foreach (ExternalEntryNode e in _sectionList)
                if (e!= null && e.DataOffsets.Count > 0 && e.DataOffsets[0] == offset)
                    return e;
            return null;
        }
        /// <summary>
        /// Use this only when parsing.
        /// </summary>
        public MovesetEntryNode GetEntry(int offset)
        {
            if (!_initializing)
                throw new Exception("Not initializing."); 
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
                throw new Exception("Not initializing."); 

            if (offset < 0)
                return null;

            MovesetEntryNode e = GetEntry(offset);

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
                throw new Exception("Not initializing."); 

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
            MovesetEntryNode e = GetEntry(offset);
            if (e is ExternalEntryNode && e != null)
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

        #region Misc Functions

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

        #endregion
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
}