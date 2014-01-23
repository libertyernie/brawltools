using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using Ikarus;
using System.Collections;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class ActionOverrideList : MovesetEntry, IEnumerable<ActionOverrideEntry>, IList
    {
        #region Child Enumeration

        public IEnumerator<ActionOverrideEntry> GetEnumerator() { return _children.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        List<ActionOverrideEntry> _children;

        public int Add(object value)
        {
            _children.Add(value as ActionOverrideEntry);
            SignalRebuildChange();
            return _children.Count - 1;
        }

        void IList.Clear()
        {
            if (Count != 0)
            {
                _children.Clear();
                SignalRebuildChange();
            }
        }

        public bool Contains(object value)
        {
            return _children.IndexOf((ActionOverrideEntry)value) >= 0;
        }

        public int IndexOf(object value)
        {
            return _children.IndexOf((ActionOverrideEntry)value);
        }

        public void Insert(int index, object value)
        {
            if (index >= 0)
            {
                if (index < Count)
                    _children.Insert(index, value as ActionOverrideEntry);
                else
                    _children.Add(value as ActionOverrideEntry);
                SignalRebuildChange();
            }
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            int i = _children.IndexOf((ActionOverrideEntry)value);
            if (i >= 0)
            {
                _children.RemoveAt(i);
                SignalRebuildChange();
            }
        }

        void IList.RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                _children.RemoveAt(index);
                SignalRebuildChange();
            }
        }

        object IList.this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                    return _children[index];
                return null;
            }
            set
            {
                if (index >= 0 && index < Count)
                {
                    SignalPropertyChange();
                    _children[index] = (ActionOverrideEntry)value;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            //_children.CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public int Count { get { return _children.Count; } }

        #endregion

        public override void Parse(VoidPtr address)
        {
            _children = new List<ActionOverrideEntry>();
            sActionOverride* entry = (sActionOverride*)address;
            while (entry->_commandListOffset > 0)
                _children.Add(Parse<ActionOverrideEntry>(entry++));
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            int size = 8;
            foreach (ActionOverrideEntry e in _children)
            {
                size += 8;
                if (e._script != null && e._script.Count > 0)
                    _lookupCount++;
            }
            return _entryLength = size;
        }

        protected override void OnWrite(VoidPtr address)
        {
            _rebuildAddr = address;
            foreach (ActionOverrideEntry e in _children)
            {
                e._rebuildAddr = address;
                sActionOverride* addr = (sActionOverride*)address;
                addr->_actionID = e._actionId;
                MovesetFile.Builder._postProcessNodes.Add(this);
                if (e._script != null && e._script.Count > 0)
                    _lookupOffsets.Add(addr->_commandListOffset.Address);
                address += 8;
            }

            sActionOverride* end = (sActionOverride*)address;
            end->_actionID = -1;
            end->_commandListOffset = 0;
        }

        protected override void PostProcess(LookupManager lookupOffsets)
        {
            foreach (ActionOverrideEntry e in _children)
                if (e._script != null && e._script.Count > 0)
                    ((sActionOverride*)e._rebuildAddr)->_commandListOffset = Offset(e._script._rebuildAddr);
        }
    }

    public unsafe class ActionOverrideEntry : MovesetEntry
    {
        public int _actionId;
        private int _commandListOffset;
        public Script _script;

        [Category("Script Override")]
        public int ActionID { get { return _actionId; } set { _actionId = value; SignalPropertyChange(); } }
        [Category("Script Override")]
        public int CommandListOffset { get { return _commandListOffset; } }

        public override void Parse(VoidPtr address)
        {
            sActionOverride* hdr = (sActionOverride*)address;
            _actionId = hdr->_actionID;
            _commandListOffset = hdr->_commandListOffset;

            if (_commandListOffset > 0)
                _script = Parse<Script>(_commandListOffset);
        }

        public override string ToString()
        {
            return String.Format("[{0}] Action", ActionID.ToString().PadLeft(3));
        }
    }
}
