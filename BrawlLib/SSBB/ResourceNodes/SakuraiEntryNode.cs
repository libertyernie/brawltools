using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    public abstract unsafe class SakuraiEntryNode
    {
        public string DataOffset { get { return "0x" + _offset.ToString("X"); } }
        public string DataSize { get { return "0x" + _initSize.ToString("X"); } }
        
        [Browsable(false)]
        public int RebuildOffset
        {
            get
            {
                return RebuildAddress == null 
                    || BaseAddress == null 
                    || RebuildAddress < BaseAddress ? 
                    -1 : (int)RebuildAddress - (int)BaseAddress;
            }
        }
        [Browsable(false)]
        public VoidPtr BaseAddress { get { return _root.BaseAddress; } }
        [Browsable(false)]
        public bool External { get { return _externalEntry != null; } }
        [Browsable(false)]
        public bool HasChanged
        {
            get { return _changed || (_root != null && _root.ChangedEntries.Contains(this)); }
            set { _changed = value; if (_root != null) _root.ChangedEntries.Remove(this); }
        }
        [Browsable(false)]
        public virtual string Name { get { return _name; } }
        /// <summary>
        /// This is where the data for this node was written during the last rebuild.
        /// Don't forget to set this when rebuilding a node!
        /// </summary>
        [Browsable(false)]
        public VoidPtr RebuildAddress
        {
            get { return _rebuildAddress; }
            set
            {
                if (_root.IsRebuilding)
                    _rebuildAddress = value;
                else //DEBUG
                    throw new Exception("Can't set rebuild address when the file isn't being rebuilt.");
            }
        }
        [Browsable(false)]
        public virtual bool IsDirty { get { return HasChanged; } set { HasChanged = value; } }
        [Browsable(false)]
        public virtual int Index { get { return _index; } }
        [Browsable(false)]
        public int TotalSize { get { return _entryLength + _childLength; } }

        public string _name;
        private bool _changed;
        public SakuraiEntryNode _parent;
        public SakuraiArchiveNode _root;
        public int
            _offset, //The initial offset of this entry when first parsed
            _index, //The entry's child index when first parsed
            _initSize = -1, //The size of this entry when first parsed.
            _calcSize; //This size of this entry after GetSize() has been called.

        //Sometimes a section will reference an entry contained in another section.
        //This keeps track of that
        public TableEntryNode _externalEntry = null;

        private VoidPtr _rebuildAddress = null;
        public int _entryLength = 0, _childLength = 0;

        [Browsable(false)]
        public int LookupCount { get { return _lookupCount; } }
        private int _lookupCount = 0;

        public List<VoidPtr> _lookupOffsets;
        
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
        public T Parse<T>(int offset, params object[] parameters) where T : SakuraiEntryNode
        {
            return CommonInit<T>(_root, this, Address(offset), parameters);
        }
        /// <summary>
        /// Use this to parse a node of a specific type at the given address.
        /// This will automatically add the node to the entry cache, get its size,
        /// set its offset value, and attach its external entry if it has one.
        /// Be sure to send the proper constructor parameters for the given type
        /// as well, or an error will be thrown.
        /// </summary>
        public T Parse<T>(VoidPtr address, params object[] parameters) where T : SakuraiEntryNode
        {
            return CommonInit<T>(_root, this, address, parameters);
        }
        /// <summary>
        /// Use this to parse a node of a specific type at the given address.
        /// This will automatically add the node to the entry cache, get its size,
        /// set its offset value, and attach its external entry if it has one.
        /// Be sure to send the proper constructor parameters for the given type
        /// as well, or an error will be thrown.
        /// </summary>
        public static T Parse<T>(SakuraiArchiveNode root, SakuraiEntryNode parent, VoidPtr address, params object[] parameters) where T : SakuraiEntryNode
        {
            return CommonInit<T>(root, parent, address, parameters);
        }
        /// <summary>
        /// Don't call this outside of the Parse functions. 
        /// This is here to eliminate redundant code.
        /// </summary>
        private static T CommonInit<T>(SakuraiArchiveNode root, SakuraiEntryNode parent, VoidPtr addr, params object[] parameters) where T : SakuraiEntryNode
        {
            int offset = root.Offset(addr);
            bool attributes = parameters.Contains("Attributes");
            if (offset <= 0 && !attributes)
                return null;

            if (attributes)
                parameters = new object[0];

            T n = Activator.CreateInstance(typeof(T), parameters) as T;
            n.Setup(root, parent, offset);
            n.OnParse(addr);
            return n;
        }

        public void ParseSelf(SakuraiArchiveNode root, SakuraiEntryNode parent, int offset)
        {
            Setup(root, parent, offset);
            OnParse(Address(offset));
        }
        public void ParseSelf(SakuraiArchiveNode root, SakuraiEntryNode parent, VoidPtr address)
        {
            Setup(root, parent, Offset(address));
            OnParse(address);
        }

        private void Setup(SakuraiArchiveNode node, SakuraiEntryNode parent, int offset) { Setup(node, parent, offset, null); }
        private void Setup(SakuraiArchiveNode node, SakuraiEntryNode parent, int offset, string name)
        {
            _name = name;
            _root = node;
            _offset = offset;
            _parent = parent;
            if (_initSize <= 0)
                _initSize = _root.GetSize(_offset);
            _root.EntryCache[_offset] = this;
            if ((_externalEntry = _root.TryGetExternal(offset)) != null)
                _externalEntry.References.Add(this);
        }
        public int GetSize()
        {
            _entryLength = 0;
            _childLength = 0;
            return _calcSize = OnGetSize();
        }

        /// <summary>
        /// Writes this node's data at the given address.
        /// Because most entries write their children before their header,
        /// this returns the offset of the header.
        /// Also resets the lookup count for the next rebuild.
        /// </summary>
        public int Write(VoidPtr address)
        {
            //Reset list of lookup offsets
            //Addresses will be added in OnWrite.
            _lookupOffsets = new List<VoidPtr>();
            
            //Write this node's data to the address.
            //Sets RebuildAddress to the location of the header.
            //The header is often not the first thing written to the given address.
            //Children are always written first.
            OnWrite(address);

            if (_lookupOffsets.Count != _lookupCount) //DEBUG
                throw new Exception("Number of actual lookup offsets does not match the calculated count.");

            //Reset for next calc size
            _lookupCount = 0;

            if (!RebuildAddress) //DEBUG
                throw new Exception("RebuildAddress was not set.");

            //Return the offset to the header
            return RebuildOffset;
        }

        public int GetLookupCount()
        {
            if (_lookupCount == 0)
                _lookupCount = OnGetLookupCount();
            return _lookupCount;
        }

        //Call this function on the addresses of all offsets.
        //DO NOT send the offset itself as the address!
        protected void Lookup(VoidPtr address)
        {
            _lookupOffsets.Add(Offset(address));
        }

        //Overridable functions
        protected virtual void OnParse(VoidPtr address) { }
        protected virtual void OnWrite(VoidPtr address) { }
        protected virtual int OnGetSize() { return 0; }
        protected virtual int OnGetLookupCount() { return 0; }
        protected virtual void PostProcess(LookupManager lookupOffsets) { }

        public override string ToString() { return String.IsNullOrEmpty(Name) ? base.ToString() : Name; }

        public virtual void PostParse() { }
    }
}
