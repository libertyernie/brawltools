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
        //[Browsable(false)]
        //public MDL0Node Model
        //{
        //    get
        //    {
        //        ArticleNode article = ParentArticle;
        //        if (article != null)
        //        {
        //            if (article._info != null)
        //                return article._info._model;
        //        }
        //        else if (_root != null)
        //            return _root.Model;

        //        return null;
        //    }
        //}

        public SakuraiEntryNode _parent;

        //[Browsable(false)]
        //public ArticleNode ParentArticle
        //{
        //    get
        //    {
        //        MovesetEntryNode n = _parent;
        //        while (!(n is ArticleNode) && n != null)
        //            n = n._parent;
        //        return n as ArticleNode;
        //    }
        //}

        public SakuraiArchiveNode _root;

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
                else
                    throw new Exception("Can't set rebuild address when the file isn't being rebuilt.");
            }
        }
        [Browsable(false)]
        public virtual bool IsDirty { get { return HasChanged; } set { HasChanged = value; } }
        [Browsable(false)]
        public virtual int Index { get { return _index; } }

        public int _initSize = -1;
        public int _calcSize;

        public string _name;
        public int _offset;
        public TableEntryNode _externalEntry = null;
        private bool _changed;
        internal int _index;

        private VoidPtr _rebuildAddress = null;
        public int _entryLength = 0, _childLength = 0;
        public int _lookupCount = 0;
        public List<VoidPtr> _lookupOffsets = new List<VoidPtr>();
        
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
        public int GetSize() { return _calcSize = OnGetSize(); }
        public int Write(VoidPtr address) { OnWrite(address); return RebuildOffset; }

        //Overridable functions
        protected virtual void OnParse(VoidPtr address) { }
        protected virtual void OnWrite(VoidPtr address) { }
        protected virtual int OnGetSize() { return 0; }
        protected virtual void PostProcess(LookupManager lookupOffsets) { }

        public override string ToString() { return String.IsNullOrEmpty(Name) ? base.ToString() : Name; }

        public virtual void PostParse() { }
    }
}
