using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Imaging;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class CLR0Node : AnimationNode
    {
        internal BRESCommonHeader* Header { get { return (BRESCommonHeader*)WorkingUncompressed.Address; } }
        internal CLR0v3* Header3 { get { return (CLR0v3*)WorkingUncompressed.Address; } }
        internal CLR0v4* Header4 { get { return (CLR0v4*)WorkingUncompressed.Address; } }

        public override ResourceType ResourceType { get { return ResourceType.CLR0; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(CLR0MaterialNode) };
            }
        }

        internal int _numFrames = 1, _origPathOffset, _loop, _version = 3;

        [Category("Color Animation Data")]
        public int Version { get { return _version; } set { _version = value; SignalPropertyChange(); } }
        [Category("Color Animation Data")]
        public override int FrameCount
        {
            get { return _numFrames; }
            set
            {
                _numFrames = value;
                foreach (CLR0MaterialNode n in Children)
                    foreach (CLR0MaterialEntryNode e in n.Children)
                        e.NumEntries = _numFrames + 1;
                SignalPropertyChange();
            }
        }

        [Category("Color Animation Data")]
        public override bool Loop { get { return _loop != 0; } set { _loop = value ? 1 : 0; SignalPropertyChange(); } }

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        [Category("Color Animation Data")]
        public string OriginalPath { get { return _originalPath; } set { _originalPath = value; SignalPropertyChange(); } }
        public string _originalPath;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _version = Header->_version;
            if (_version == 4)
            {
                _numFrames = Header4->_frames;
                _origPathOffset = Header4->_origPathOffset;
                _loop = Header4->_loop;

                if ((_name == null) && (Header4->_stringOffset != 0))
                    _name = Header4->ResourceString;

                if (Header4->_origPathOffset > 0)
                    _originalPath = Header4->OrigPath;

                (_userEntries = new UserDataCollection()).Read(Header4->UserData);

                return Header4->Group->_numEntries > 0;
            }
            else
            {
                _numFrames = Header3->_frames;
                _origPathOffset = Header3->_origPathOffset;
                _loop = Header3->_loop;

                if ((_name == null) && (Header3->_stringOffset != 0))
                    _name = Header3->ResourceString;

                if (Header3->_origPathOffset > 0)
                    _originalPath = Header3->OrigPath;

                return Header3->Group->_numEntries > 0;
            }
        }

        public CLR0MaterialNode CreateEntry()
        {
            CLR0MaterialNode node = new CLR0MaterialNode();
            CLR0MaterialEntryNode entry = new CLR0MaterialEntryNode();
            entry._target = EntryTarget.Color0;
            entry._name = entry._target.ToString();
            entry._numEntries = -1;
            entry.NumEntries = _numFrames;
            entry.Constant = true;
            entry.SolidColor = new ARGBPixel();
            node.Name = this.FindName(null);
            this.AddChild(node);
            node.AddChild(entry);
            return node;
        }

        public override int OnCalculateSize(bool force)
        {
            int size = (_version == 4 ? CLR0v4.Size : CLR0v3.Size) + 0x18 + Children.Count * 0x10;
            foreach (CLR0MaterialNode n in Children)
            {
                size += 8 + n.Children.Count * 8;
                foreach (CLR0MaterialEntryNode e in n.Children)
                    if (e._numEntries != 0)
                        size += (e._colors.Count * 4);
            } 
            if (_version == 4)
                size += _userEntries.GetSize();
            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            int count = Children.Count;

            CLR0Material* pMat = (CLR0Material*)(address + (_version == 4 ? CLR0v4.Size : CLR0v3.Size) + 0x18 + (count * 0x10));

            int offset = Children.Count * 8;
            foreach (CLR0MaterialNode n in Children)
                offset += n.Children.Count * 8;

            ABGRPixel* pData = (ABGRPixel*)((VoidPtr)pMat + offset);

            ResourceGroup* group;
            if (_version == 4)
            {
                CLR0v4* header = (CLR0v4*)address;
                *header = new CLR0v4(length, _numFrames, count, _loop);

                group = header->Group;
            }
            else
            {
                CLR0v3* header = (CLR0v3*)address;
                *header = new CLR0v3(length, _numFrames, count, _loop);

                group = header->Group;
            }
            *group = new ResourceGroup(count);

            ResourceEntry* entry = group->First;
            foreach (CLR0MaterialNode n in Children)
            {
                (entry++)->_dataOffset = (int)pMat - (int)group;

                uint newFlags = 0;

                CLR0MaterialEntry* pMatEntry = (CLR0MaterialEntry*)((VoidPtr)pMat + 8);
                foreach (CLR0MaterialEntryNode e in n.Children)
                {
                    newFlags |= ((uint)((1 + (e._constant ? 2 : 0)) & 3) << ((int)e._target * 2));
                    if (e._numEntries == 0)
                        *pMatEntry = new CLR0MaterialEntry((ABGRPixel)e._colorMask, (ABGRPixel)e._solidColor);
                    else
                    {
                        *pMatEntry = new CLR0MaterialEntry((ABGRPixel)e._colorMask, (int)pData - (int)((VoidPtr)pMatEntry + 4));
                        foreach (ARGBPixel p in e._colors)
                            *pData++ = (ABGRPixel)p;
                    }
                    pMatEntry++;
                    e._changed = false;
                }
                pMat->_flags = newFlags;
                pMat = (CLR0Material*)pMatEntry;
                n._changed = false;
            }

            if (_userEntries.Count > 0 && _version == 4)
            {
                CLR0v4* header = (CLR0v4*)address;
                header->UserData = pData;
                _userEntries.Write(pData);
            }
        }

        public override void OnPopulate()
        {
            ResourceGroup* group = Header3->Group;
            for (int i = 0; i < group->_numEntries; i++)
                new CLR0MaterialNode().Initialize(this, new DataSource(group->First[i].DataAddress, 8));
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);
            foreach (CLR0MaterialNode n in Children)
                table.Add(n.Name);

            if (_version == 4)
                foreach (UserDataClass s in _userEntries)
                {
                    table.Add(s._name);
                    if (s._type == UserValueType.String && s._entries.Count > 0)
                        table.Add(s._entries[0]);
                }

            if (!String.IsNullOrEmpty(_originalPath))
                table.Add(_originalPath);
        }

        protected internal override void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {
            base.PostProcess(bresAddress, dataAddress, dataLength, stringTable);

            CLR0v3* header = (CLR0v3*)dataAddress;
            if (_version == 4)
            {
                ((CLR0v4*)header)->ResourceStringAddress = stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    ((CLR0v4*)dataAddress)->OrigPathAddress = stringTable[_originalPath] + 4;
            }
            else
            {
                header->ResourceStringAddress = stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    header->OrigPathAddress = stringTable[_originalPath] + 4;
            }

            ResourceGroup* group = header->Group;
            group->_first = new ResourceEntry(0xFFFF, 0, 0, 0, 0);
            ResourceEntry* rEntry = group->First;

            int index = 1;
            foreach (CLR0MaterialNode n in Children)
            {
                dataAddress = (VoidPtr)group + (rEntry++)->_dataOffset;
                ResourceEntry.Build(group, index++, dataAddress, (BRESString*)stringTable[n.Name]);
                n.PostProcess(dataAddress, stringTable);
            }

            if (_version == 4) _userEntries.PostProcess(((CLR0v4*)dataAddress)->UserData, stringTable);
        }

        internal static ResourceNode TryParse(DataSource source) { return ((CLR0v3*)source.Address)->_header._tag == CLR0v3.Tag ? new CLR0Node() : null; }
        
        #region Extra Functions
        //public void Append()
        //{
        //    CLR0Node external = null;
        //    OpenFileDialog o = new OpenFileDialog();
        //    o.Filter = "CLR0 Animation (*.clr0)|*.clr0";
        //    o.Title = "Please select an animation to append.";
        //    if (o.ShowDialog() == DialogResult.OK)
        //        if ((external = (CLR0Node)NodeFactory.FromFile(null, o.FileName)) != null)
        //            Append(external);
        //}
        //public void Append(CLR0Node external)
        //{
        //    int origIntCount = FrameCount;
        //    FrameCount += external.FrameCount;

        //    foreach (CLR0MaterialNode mat in external.Children)
        //    {
        //        foreach (CLR0MaterialEntryNode _extEntry in mat.Children)
        //        {
        //            CLR0MaterialEntryNode _intEntry = null;
        //            if ((_intEntry = (CLR0MaterialEntryNode)FindChild(mat.Name + "/" + _extEntry.Name, false)) == null)
        //            {
        //                CLR0MaterialNode wi = null;
        //                if ((wi = (CLR0MaterialNode)FindChild(mat.Name, false)) == null)
        //                    AddChild(wi = new CLR0MaterialNode() { Name = FindName(null) });

        //                CLR0MaterialEntryNode newIntEntry = new CLR0MaterialEntryNode() { Name = _extEntry.Name };

        //                AddChild(newIntEntry);
        //            }
        //            else
        //            {
        //                //for (int x = 0; x < external.FrameCount; x++)
        //            }
        //        }
        //    }
        //}

        #endregion
    }

    public unsafe class CLR0MaterialNode : ResourceNode
    {
        internal CLR0Material* Header { get { return (CLR0Material*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.CLR0Material; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(CLR0MaterialEntryNode) };
            }
        }

        internal CLR0EntryFlags _flags;

        public List<int> _entries;

        public override bool OnInitialize()
        {
            if ((_name == null) && (Header->_stringOffset != 0))
                _name = Header->ResourceString;

            _flags = Header->Flags;
            _entries = new List<int>();

            for (int i = 0; i < 11; i++)
                if ((((uint)_flags >> i * 2) & 1) != 0) 
                    _entries.Add(i);

            return _entries.Count > 0;
        }

        public override void OnPopulate()
        {
            for (int i = 0; i < _entries.Count; i++)
                new CLR0MaterialEntryNode() { _target = (EntryTarget)_entries[i], _constant = ((((uint)_flags >> _entries[i] * 2) & 2) == 2) }.Initialize(this, (VoidPtr)Header + 8 + i * 8, 8);
        }

        protected internal virtual void PostProcess(VoidPtr dataAddress, StringTable stringTable)
        {
            CLR0Material* header = (CLR0Material*)dataAddress;
            header->ResourceStringAddress = stringTable[Name] + 4;
        }

        public void CreateEntry()
        {
            int value = 0; Top:
            foreach (CLR0MaterialEntryNode t in Children)
                if ((int)t._target == value) { value++; goto Top; }
            if (value >= 11)
                return;

            CLR0MaterialEntryNode entry = new CLR0MaterialEntryNode();
            entry._target = (EntryTarget)value;
            entry._name = entry._target.ToString();
            entry._numEntries = -1;
            entry.NumEntries = ((CLR0Node)Parent)._numFrames;
            AddChild(entry);
        }
    }

    public unsafe class CLR0MaterialEntryNode : ResourceNode, IColorSource
    {
        internal CLR0MaterialEntry* Header { get { return (CLR0MaterialEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.CLR0MaterialEntry; } }

        public bool _constant = false;
        [Category("CLR0 Material Entry")]
        public bool Constant 
        {
            get { return _constant; } 
            set 
            {
                if (_constant != value)
                {
                    _constant = value;
                    if (_constant)
                        MakeSolid(_solidColor);
                    else
                        MakeList();
                }
            } 
        }
        internal EntryTarget _target;
        [Category("CLR0 Material Entry")]
        public EntryTarget Target 
        {
            get { return _target; } 
            set 
            {
                foreach (CLR0MaterialEntryNode t in Parent.Children)
                    if (t._target == value) return;

                _target = value;
                Name = _target.ToString();
                SignalPropertyChange(); 
            }
        }

        internal ARGBPixel _colorMask;
        [Browsable(false)]
        public ARGBPixel ColorMask { get { return _colorMask; } set { _colorMask = value; SignalPropertyChange(); } }

        internal List<ARGBPixel> _colors = new List<ARGBPixel>();
        [Browsable(false)]
        public List<ARGBPixel> Colors { get { return _colors; } set { _colors = value; SignalPropertyChange(); } }

        internal ARGBPixel _solidColor = new ARGBPixel();
        [Browsable(false)]
        public ARGBPixel SolidColor { get { return _solidColor; } set { _solidColor = value; SignalPropertyChange(); } }

        internal int _numEntries;
        [Browsable(false)]
        internal int NumEntries
        {
            get { return _numEntries; }
            set
            {
                if (_numEntries == 0)
                    return;

                if (value > _numEntries)
                {
                    ARGBPixel p = _numEntries > 0 ? _colors[_numEntries - 1] : new ARGBPixel(255, 0, 0, 0);
                    for (int i = value - _numEntries; i-- > 0; )
                        _colors.Add(p);
                }
                else if (value < _colors.Count)
                    _colors.RemoveRange(value, _colors.Count - value);

                _numEntries = value;
            }
        }

        public override bool OnInitialize()
        {
            _colorMask = (ARGBPixel)Header->_colorMask;

            _colors.Clear();
            if (_constant)
            {
                _numEntries = 0;
                _solidColor = (ARGBPixel)Header->SolidColor;
            }
            else
            {
                _numEntries = ((CLR0Node)Parent.Parent)._numFrames;
                ABGRPixel* data = Header->Data;
                for (int i = 0; i < _numEntries; i++)
                    _colors.Add((ARGBPixel)(*data++));
            }

            _name = _target.ToString();

            return false;
        }

        public void MakeSolid(ARGBPixel color)
        {
            _numEntries = 0;
            _constant = true;
            _solidColor = color;
            SignalPropertyChange();
        }
        public void MakeList()
        {
            _constant = false;
            int entries = ((CLR0Node)Parent._parent)._numFrames;
            _numEntries = _colors.Count;
            NumEntries = entries;
        }

        protected internal virtual void PostProcess(VoidPtr dataAddress, StringTable stringTable)
        {
            CLR0Material* header = (CLR0Material*)dataAddress;
            header->ResourceStringAddress = stringTable[Name] + 4;
        }

        #region IColorSource Members

        public bool HasPrimary(int id) { return true; }
        public ARGBPixel GetPrimaryColor(int id) { return _colorMask; }
        public void SetPrimaryColor(int id, ARGBPixel color) { _colorMask = color; SignalPropertyChange(); }
        [Browsable(false)]
        public string PrimaryColorName(int id) { return "Mask:"; }
        [Browsable(false)]
        public int TypeCount { get { return 1; } }
        [Browsable(false)]
        public int ColorCount(int id) { return (_numEntries == 0) ? 1 : _numEntries; }
        public ARGBPixel GetColor(int index, int id) { return (_numEntries == 0) ? _solidColor : _colors[index]; }
        public void SetColor(int index, int id, ARGBPixel color)
        {
            if (_numEntries == 0)
                _solidColor = color;
            else
                _colors[index] = color;
            SignalPropertyChange();
        }
        public bool GetClrConstant(int id)
        {
            return Constant;
        }
        public void SetClrConstant(int id, bool constant)
        {
            Constant = constant;
        }

        #endregion
    }
}
