using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class PAT0Node : AnimationNode
    {
        internal BRESCommonHeader* Header { get { return (BRESCommonHeader*)WorkingUncompressed.Address; } }
        internal PAT0v3* Header3 { get { return (PAT0v3*)WorkingUncompressed.Address; } }
        internal PAT0v4* Header4 { get { return (PAT0v4*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.PAT0; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(PAT0EntryNode) };
            }
        }

        internal List<string> _textureFiles = new List<string>();
        internal List<string> _paletteFiles = new List<string>();

        internal int _loop, _version = 3;
        internal int _frameCount = 1;

        public bool texChanged = false, pltChanged = false;

        [Category("Texture Pattern Data")]
        public int Version { get { return _version; } set { _version = value; SignalPropertyChange(); } }
        [Category("Texture Pattern Data")]
        public string[] Textures
        {
            get
            {
                if (texChanged)
                {
                    _textureFiles.Clear();
                    foreach (PAT0EntryNode n in Children)
                        foreach (PAT0TextureNode t in n.Children)
                            foreach (PAT0TextureEntryNode e in t.Children)
                                if (t._hasTex && !String.IsNullOrEmpty(e._tex) && !_textureFiles.Contains(e._tex))
                                    _textureFiles.Add(e._tex);
                    _textureFiles.Sort();
                    texChanged = false;
                }
                return _textureFiles.ToArray();
            }
        }
        [Category("Texture Pattern Data")]
        public string[] Palettes
        {
            get
            {
                if (pltChanged)
                {
                    _paletteFiles.Clear();
                    foreach (PAT0EntryNode n in Children)
                        foreach (PAT0TextureNode t in n.Children)
                            foreach (PAT0TextureEntryNode e in t.Children)
                                if (t._hasPlt && !String.IsNullOrEmpty(e._plt) && !_paletteFiles.Contains(e._plt))
                                    _paletteFiles.Add(e._plt);
                    _paletteFiles.Sort();
                    pltChanged = false;
                }
                return _paletteFiles.ToArray();
            }
        }
        [Category("Texture Pattern Data")]
        public override int FrameCount
        {
            get { return _frameCount; }
            set
            {
                if ((_frameCount == value) || (value < 1))
                    return; 

                _frameCount = value;
                SignalPropertyChange();
            }
        }
        [Category("Texture Pattern Data")]
        public override bool Loop { get { return _loop != 0; } set { _loop = value ? 1 : 0; SignalPropertyChange(); } }

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        [Category("Texture Pattern Data")]
        public string OriginalPath { get { return _originalPath; } set { _originalPath = value; SignalPropertyChange(); } }
        public string _originalPath;

        public override bool OnInitialize()
        {
            base.OnInitialize();

            _textureFiles.Clear();
            _paletteFiles.Clear();

            _version = Header->_version;

            int texPtr, pltPtr;
            if (_version == 4)
            {
                PAT0v4* header = Header4;
                _frameCount = header->_numFrames;
                _loop = header->_loop;
                texPtr = header->_numTexPtr;
                pltPtr = header->_numPltPtr;
                if ((_name == null) && (header->_stringOffset != 0))
                    _name = header->ResourceString;

                if (Header4->_origPathOffset > 0)
                    _originalPath = Header4->OrigPath;

                (_userEntries = new UserDataCollection()).Read(Header4->UserData);
            }
            else
            {
                PAT0v3* header = Header3;
                _frameCount = header->_numFrames;
                _loop = header->_loop;
                texPtr = header->_numTexPtr;
                pltPtr = header->_numPltPtr;

                if ((_name == null) && (header->_stringOffset != 0))
                    _name = header->ResourceString;

                if (Header3->_origPathOffset > 0)
                    _originalPath = Header3->OrigPath;
            }

            //Get texture strings
            for (int i = 0; i < texPtr; i++)
                _textureFiles.Add(Header3->GetTexStringEntry(i));

            //Get palette strings
            for (int i = 0; i < pltPtr; i++)
                _paletteFiles.Add(Header3->GetPltStringEntry(i));

            //Link all entries
            Populate();

            return Header3->Group->_numEntries > 0;
        }

        public override void OnPopulate()
        {
            ResourceGroup* group = Header3->Group;
            for (int i = 0; i < group->_numEntries; i++)
                new PAT0EntryNode().Initialize(this, new DataSource(group->First[i].DataAddress, PAT0Pattern.Size));
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);

            foreach (PAT0EntryNode n in Children)
            {
                table.Add(n.Name);
                foreach (PAT0TextureNode t in n.Children)
                    foreach (PAT0TextureEntryNode e in t.Children)
                        table.Add(e.Name);
            }

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

        public override int OnCalculateSize(bool force)
        {
            _textureFiles.Clear();
            _paletteFiles.Clear();
            foreach (PAT0EntryNode n in Children)
                foreach (PAT0TextureNode t in n.Children)
                    foreach (PAT0TextureEntryNode e in t.Children)
                    {
                        if (t._hasTex && !String.IsNullOrEmpty(e._tex) && !_textureFiles.Contains(e._tex))
                            _textureFiles.Add(e._tex);
                        if (t._hasPlt && !String.IsNullOrEmpty(e._plt) && !_paletteFiles.Contains(e._plt))
                            _paletteFiles.Add(e._plt);
                    }

            _textureFiles.Sort();
            _paletteFiles.Sort();

            int size = PAT0v3.Size + 0x18 + Children.Count * 0x10;
            size += (_textureFiles.Count + _paletteFiles.Count) * 8;
            foreach (PAT0EntryNode n in Children)
                size += n.CalculateSize(true);

            if (_version == 4)
                size += _userEntries.GetSize();

            return size;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            //Set header values
            if (_version == 4)
            {
                PAT0v4* header = (PAT0v4*)address;
                header->_header._tag = PAT0v4.Tag;
                header->_header._version = 4;
                header->_dataOffset = PAT0v4.Size;
                header->_userDataOffset = header->_origPathOffset = 0;
                header->_numFrames = (ushort)_frameCount;
                header->_numEntries = (ushort)Children.Count;
                header->_numTexPtr = (ushort)_textureFiles.Count;
                header->_numPltPtr = (ushort)_paletteFiles.Count;
                header->_loop = _loop;
            }
            else
            {
                PAT0v3* header = (PAT0v3*)address;
                header->_header._tag = PAT0v3.Tag;
                header->_header._version = 3;
                header->_dataOffset = PAT0v3.Size;
                header->_origPathOffset = 0;
                header->_numFrames = (ushort)_frameCount;
                header->_numEntries = (ushort)Children.Count;
                header->_numTexPtr = (ushort)_textureFiles.Count;
                header->_numPltPtr = (ushort)_paletteFiles.Count;
                header->_loop = _loop;
            }

            PAT0v3* commonHeader = (PAT0v3*)address;

            //Now set header values that are in the same spot between versions

            //Set offsets
            commonHeader->_texTableOffset = length - (_textureFiles.Count + _paletteFiles.Count) * 8;
            commonHeader->_pltTableOffset = commonHeader->_texTableOffset + _textureFiles.Count * 4;

            //Set pointer offsets
            int offset = length - _textureFiles.Count * 4 - _paletteFiles.Count * 4;
            commonHeader->_texPtrTableOffset = offset;
            commonHeader->_pltPtrTableOffset = offset + _textureFiles.Count * 4;

            //Set pointers
            bint* ptr = (bint*)(commonHeader->Address + commonHeader->_texPtrTableOffset);
            for (int i = 0; i < _textureFiles.Count; i++)
                *ptr++ = 0;
            ptr = (bint*)(commonHeader->Address + commonHeader->_pltPtrTableOffset);
            for (int i = 0; i < _paletteFiles.Count; i++)
                *ptr++ = 0;

            ResourceGroup* group = commonHeader->Group;
            *group = new ResourceGroup(Children.Count);

            VoidPtr entryAddress = group->EndAddress;
            VoidPtr dataAddress = entryAddress;
            ResourceEntry* rEntry = group->First;

            foreach (PAT0EntryNode n in Children)
                dataAddress += n._entryLen;
            foreach (PAT0EntryNode n in Children)
            foreach (PAT0TextureNode t in n.Children)
            {
                n._dataAddrs[t.Index] = dataAddress;
                if (n._dataLens[t.Index] != -1)
                    dataAddress += n._dataLens[t.Index];
            }

            foreach (PAT0EntryNode n in Children)
            {
                (rEntry++)->_dataOffset = (int)entryAddress - (int)group;

                n.Rebuild(entryAddress, n._entryLen, true);
                entryAddress += n._entryLen;
            }

            if (_userEntries.Count > 0 && _version == 4)
            {
                PAT0v4* header = (PAT0v4*)address;
                header->UserData = dataAddress;
                _userEntries.Write(dataAddress);
            }
        }

        protected internal override void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {
            base.PostProcess(bresAddress, dataAddress, dataLength, stringTable);

            PAT0v3* header = (PAT0v3*)dataAddress;
            if (_version == 4)
            {
                ((PAT0v4*)dataAddress)->ResourceStringAddress = stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    ((PAT0v4*)dataAddress)->OrigPathAddress = stringTable[_originalPath] + 4;
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
            foreach (PAT0EntryNode n in Children)
            {
                dataAddress = (VoidPtr)group + (rEntry++)->_dataOffset;
                ResourceEntry.Build(group, index++, dataAddress, (BRESString*)stringTable[n.Name]);
                n.PostProcess(dataAddress, stringTable);
            }

            int i = 0;
            bint* strings = header->TexFile;

            for (i = 0; i < _textureFiles.Count; i++)
                if (!String.IsNullOrEmpty(_textureFiles[i]))
                    strings[i] = (int)stringTable[_textureFiles[i]] + 4 - (int)strings;

            strings = header->PltFile;

            for (i = 0; i < _paletteFiles.Count; i++)
                if (!String.IsNullOrEmpty(_paletteFiles[i]))
                    strings[i] = (int)stringTable[_paletteFiles[i]] + 4 - (int)strings;

            if (_version == 4) _userEntries.PostProcess(((PAT0v4*)dataAddress)->UserData, stringTable);
        }

        internal static ResourceNode TryParse(DataSource source) { return ((PAT0v3*)source.Address)->_header._tag == PAT0v3.Tag ? new PAT0Node() : null; }

        public void CreateEntry()
        {
            PAT0EntryNode n = new PAT0EntryNode();
            n.Name = FindName(null);
            AddChild(n);
            n.CreateEntry();
        }

        #region Extra Functions
        /// <summary>
        /// Stretches or compresses all frames of the animation to fit a new frame count specified by the user.
        /// </summary>
        public void Resize()
        {
            FrameCountChanger f = new FrameCountChanger();
            if (f.ShowDialog(FrameCount) == DialogResult.OK)
                Resize(f.NewValue);
        }
        /// <summary>
        /// Stretches or compresses all frames of the animation to fit a new frame count.
        /// </summary>
        public void Resize(int newFrameCount)
        {
            float ratio = (float)newFrameCount / (float)FrameCount;
            foreach (PAT0EntryNode e in Children)
                foreach (PAT0TextureNode t in e.Children)
                {
                    foreach (PAT0TextureEntryNode x in t.Children)
                        x._frame *= ratio;
                    //t.SortChildren();
                }

            FrameCount = newFrameCount;
        }
        /// <summary>
        /// Adds an animation opened by the user to the end of this one
        /// </summary>
        public void Append()
        {
            PAT0Node external = null;
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "PAT0 Animation (*.pat0)|*.pat0";
            o.Title = "Please select an animation to append.";
            if (o.ShowDialog() == DialogResult.OK)
                if ((external = (PAT0Node)NodeFactory.FromFile(null, o.FileName)) != null)
                    Append(external);
        }
        /// <summary>
        /// Adds an animation to the end of this one
        /// </summary>
        public void Append(PAT0Node external)
        {
            int origIntCount = FrameCount;
            FrameCount += external.FrameCount;

            foreach (PAT0EntryNode w in external.Children)
                foreach (PAT0TextureNode _extEntry in w.Children)
                {
                    PAT0TextureNode _intEntry = null;
                    if ((_intEntry = (PAT0TextureNode)FindChild(w.Name + "/" + _extEntry.Name, false)) == null)
                    {
                        PAT0EntryNode wi = null;
                        if ((wi = (PAT0EntryNode)FindChild(w.Name, false)) == null)
                            AddChild(wi = new PAT0EntryNode() { Name = FindName(null) });

                        PAT0TextureNode newIntEntry = new PAT0TextureNode(_extEntry._texFlags, _extEntry.TextureIndex);
                        
                        wi.AddChild(newIntEntry);
                        foreach (PAT0TextureEntryNode e in _extEntry.Children)
                        {
                            PAT0TextureEntryNode q = new PAT0TextureEntryNode() { _frame = e._frame + origIntCount };
                            newIntEntry.AddChild(q);

                            q.Texture = e.Texture;
                            q.Palette = e.Palette;
                        }
                    }
                    else
                    {
                        foreach (PAT0TextureEntryNode e in _extEntry.Children)
                        {
                            PAT0TextureEntryNode q = new PAT0TextureEntryNode() { _frame = e._frame + origIntCount };
                            _intEntry.AddChild(q);

                            q.Texture = e.Texture;
                            q.Palette = e.Palette;
                        }
                    }
                }
        }
        #endregion
    }

    public unsafe class PAT0EntryNode : ResourceNode
    {
        internal PAT0Pattern* Header { get { return (PAT0Pattern*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.PAT0Entry; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(PAT0TextureNode) };
            }
        }

        internal PAT0Flags[] texFlags = new PAT0Flags[8];

        public override bool OnInitialize()
        {
            if ((_name == null) && (Header->_stringOffset != 0))
                _name = Header->ResourceString;

            uint flags = Header->_flags;
            for (int i = 0; i < 8; i++)
                texFlags[i] = (PAT0Flags)((flags >> (i * 4)) & 0xF);

            return true;
        }

        public override void OnPopulate()
        {
            int count = 0, index = 0;
            foreach (PAT0Flags p in texFlags)
            {
                if (p.HasFlag(PAT0Flags.Enabled))
                {
                    if (!p.HasFlag(PAT0Flags.FixedTexture))
                        new PAT0TextureNode(p, index).Initialize(this, new DataSource(Header->GetTexTable(count), PAT0Texture.Size));
                    else
                    {
                        PAT0TextureNode t = new PAT0TextureNode(p, index) { textureCount = 1 };
                        t._parent = this;
                        _children.Add(t);
                        PAT0TextureEntryNode entry = new PAT0TextureEntryNode();
                        entry._frame = 0;
                        entry._texFileIndex = Header->GetIndex(count, false);
                        entry._pltFileIndex = Header->GetIndex(count, true);
                        entry._parent = t;
                        t._children.Add(entry);
                        entry.GetStrings();
                    }
                    count++;
                }
                index++;
            }
        }

        public override int OnCalculateSize(bool force)
        {
            _dataLens = new int[Children.Count];
            _dataAddrs = new VoidPtr[Children.Count];

            _entryLen = PAT0Pattern.Size + Children.Count * 4;

            foreach (PAT0TextureNode table in Children)
                _dataLens[table.Index] = table.CalculateSize(true);

            //Check to see if any children can be remapped.
            foreach (PAT0TextureNode table in Children)
                table.CompareToAll();

            int size = 0;
            foreach (int i in _dataLens)
                if (i != -1)
                    size += i;

            return size + _entryLen; 
        }

        public VoidPtr[] _dataAddrs;
        public int _entryLen;
        public int[] _dataLens;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            PAT0Pattern* header = (PAT0Pattern*)address;

            int x = 0;
            foreach (int i in _dataLens)
            {
                if (i == -1)
                    _dataAddrs[x] = ((PAT0EntryNode)Parent.Children[((PAT0TextureNode)Children[x])._matIndex])._dataAddrs[((PAT0TextureNode)Children[x])._texIndex];
                x++;
            }
            uint flags = 0;
            foreach (PAT0TextureNode table in Children)
            {
                table._texFlags |= PAT0Flags.Enabled;
                if (table.Children.Count > 1)
                    table._texFlags &= ~PAT0Flags.FixedTexture;
                else
                    table._texFlags |= PAT0Flags.FixedTexture;

                bool hasTex = false, hasPlt = false;

                //foreach (PAT0TextureEntryNode e in table.Children)
                //{
                //    if (e.Texture != null)
                //        hasTex = true;
                //    if (e.Palette != null)
                //        hasPlt = true;
                //}

                hasTex = table._hasTex;
                hasPlt = table._hasPlt;

                if (!hasTex)
                    table._texFlags &= ~PAT0Flags.HasTexture;
                else
                    table._texFlags |= PAT0Flags.HasTexture;
                if (!hasPlt)
                    table._texFlags &= ~PAT0Flags.HasPalette;
                else
                    table._texFlags |= PAT0Flags.HasPalette;

                if (table.Children.Count > 1)
                {
                    header->SetTexTableOffset(table.Index, _dataAddrs[table.Index]);
                    if (table._rebuild)
                        table.Rebuild(_dataAddrs[table.Index], PAT0TextureTable.Size + PAT0Texture.Size * table.Children.Count, true);
                }
                else
                {
                    PAT0TextureEntryNode entry = (PAT0TextureEntryNode)table.Children[0];
                    PAT0Node node = (PAT0Node)Parent;

                    short i = 0;
                    if (table._hasTex && !String.IsNullOrEmpty(entry.Texture))
                        i = (short)node._textureFiles.IndexOf(entry.Texture);

                    if (i < 0)
                        entry._texFileIndex = 0;
                    else
                        entry._texFileIndex = (ushort)i;

                    i = 0;
                    if (table._hasPlt && !String.IsNullOrEmpty(entry.Palette))
                        i = (short)node._paletteFiles.IndexOf(entry.Palette);

                    if (i < 0)
                        entry._pltFileIndex = 0;
                    else
                        entry._pltFileIndex = (ushort)i;

                    header->SetIndex(table.Index, entry._texFileIndex, false);
                    header->SetIndex(table.Index, entry._pltFileIndex, true);
                }

                flags = flags & ~((uint)0xF << (table._textureIndex * 4)) | ((uint)table._texFlags << (table._textureIndex * 4)); 
            }

            header->_flags = flags;
        }

        protected internal virtual void PostProcess(VoidPtr dataAddress, StringTable stringTable)
        {
            PAT0Pattern* header = (PAT0Pattern*)dataAddress;
            header->ResourceStringAddress = stringTable[Name] + 4;
        }

        public void CreateEntry()
        {
            int value = 0;
            foreach (PAT0TextureNode t in Children)
                if (t._textureIndex == value)
                    value++;

            if (value == 8)
                return;

            PAT0TextureNode node = new PAT0TextureNode((PAT0Flags)7, value);
            AddChild(node);
            node.CreateEntry();
        }
    }

    public unsafe class PAT0TextureNode : ResourceNode
    {
        internal PAT0TextureTable* Header { get { return (PAT0TextureTable*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.PAT0Texture; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(PAT0TextureEntryNode) };
            }
        }

        public PAT0Flags _texFlags;
        public int _textureIndex, textureCount;
        public ushort _texNameIndex, _pltNameIndex;
        public bool _hasPlt, _hasTex;
        public float _frameScale;

        public bool _rebuild = true;

        //[Category("PAT0 Texture")]
        //public PAT0Flags TextureFlags { get { return texFlags; } }//set { texFlags = value; SignalPropertyChange(); } }
        //[Category("PAT0 Texture")]
        //public float FrameScale { get { return frameScale; } }
        //[Category("PAT0 Texture")]
        //public int TextureCount { get { return textureCount; } }
        [Category("PAT0 Texture")]
        public bool HasTexture { get { return _hasTex; } set { _hasTex = value; SignalPropertyChange(); } }
        [Category("PAT0 Texture")]
        public bool HasPalette { get { return _hasPlt; } set { _hasPlt = value; SignalPropertyChange(); } }
        [Category("PAT0 Texture")]
        public int TextureIndex 
        { 
            get { return _textureIndex; } 
            set 
            {
                foreach (PAT0TextureNode t in Parent.Children)
                    if (t.Index != Index && t._textureIndex == (value > 7 ? 7 : value < 0 ? 0 : value))
                        return;

                _textureIndex = value > 7 ? 7 : value < 0 ? 0 : value;

                Name = "Texture" + _textureIndex;

                CheckNext();
                CheckPrev();
            } 
        }

        public void CheckNext()
        {
            if (Index == Parent.Children.Count - 1)
                return;

            int index = Index;
            if (_textureIndex > ((PAT0TextureNode)Parent.Children[Index + 1])._textureIndex)
            {
                DoMoveDown();
                if (index != Index)
                    CheckNext();
            }
        }

        public void CheckPrev()
        {
            if (Index == 0)
                return;

            int index = Index;
            if (_textureIndex < ((PAT0TextureNode)Parent.Children[Index - 1])._textureIndex)
            {
                DoMoveUp();
                if (index != Index)
                    CheckPrev();
            }
        }

        /// <summary>
        /// Gets the last applied texture entry before or at the index.
        /// </summary>
        public PAT0TextureEntryNode GetPrevious(int index)
        {
            PAT0TextureEntryNode prev = null;
            foreach (PAT0TextureEntryNode next in Children)
            {
                if (next.Index == 0)
                {
                    prev = next;
                    continue;
                }
                if (prev._frame <= index && next._frame > index)
                    break;
                prev = next;
            }
            return prev;
        }

        /// <summary>
        /// Gets the texture entry at the index, if there is one.
        /// </summary>
        public PAT0TextureEntryNode GetEntry(int index)
        {
            PAT0TextureEntryNode prev = null;
            if (Children.Count == 0)
                return null;
            foreach (PAT0TextureEntryNode next in Children)
            {
                if (next.Index == 0)
                {
                    prev = next;
                    continue;
                }
                if (prev._frame <= index && next._frame > index)
                    break;
                prev = next;
            }
            if ((int)prev._frame == index)
                return prev;
            else
                return null;
        }

        /// <summary>
        /// Gets the applied texture at the index and outputs if the value is a keyframe.
        /// </summary>
        public string GetTexture(int index, out bool kf)
        {
            PAT0TextureEntryNode prev = null;
            if (Children.Count == 0)
            {
                kf = false;
                return null;
            }
            foreach (PAT0TextureEntryNode next in Children)
            {
                if (next.Index == 0)
                {
                    prev = next;
                    continue;
                }
                if (prev._frame <= index && next._frame > index)
                    break;
                prev = next;
            }
            if ((int)prev._frame == index)
                kf = true;
            else
                kf = false;
            return prev.Texture;
        }

        /// <summary>
        /// Gets the applied palette at the index and outputs if the value is a keyframe.
        /// </summary>
        public string GetPalette(int index, out bool kf)
        {
            PAT0TextureEntryNode prev = null;
            if (Children.Count == 0)
            {
                kf = false;
                return null;
            }
            foreach (PAT0TextureEntryNode next in Children)
            {
                if (next.Index == 0)
                {
                    prev = next;
                    continue;
                }
                if (prev._frame <= index && next._frame > index)
                    break;
                prev = next;
            }
            if ((int)prev._frame == index)
                kf = true;
            else
                kf = false;
            return prev.Palette;
        }

        public PAT0TextureNode(PAT0Flags flags, int index)
        {
            _texFlags = flags;
            _hasTex = _texFlags.HasFlag(PAT0Flags.HasTexture);
            _hasPlt = _texFlags.HasFlag(PAT0Flags.HasPalette);
            _textureIndex = index;
            _name = "Texture" + _textureIndex;
        }

        public override bool OnInitialize()
        {
            _frameScale = Header->_frameScale;
            textureCount = Header->_textureCount;

            return textureCount > 0;
        }

        public override void OnPopulate()
        {
            if (!_texFlags.HasFlag(PAT0Flags.FixedTexture))
            {
                PAT0Texture* current = Header->Textures;
                for (int i = 0; i < textureCount; i++, current++)
                    new PAT0TextureEntryNode().Initialize(this, new DataSource(current, PAT0Texture.Size));
            }
        }

        public override int OnCalculateSize(bool force)
        {
            return Children.Count > 1 ? PAT0TextureTable.Size + PAT0Texture.Size * Children.Count : 0;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            if (Children.Count > 1)
            {
                PAT0TextureTable* table = (PAT0TextureTable*)address;
                table->_textureCount = (short)Children.Count;
                float f = (Children[Children.Count - 1] as PAT0TextureEntryNode)._frame;
                table->_frameScale = f == 0 ? 0 : 1.0f / f;
                table->_pad = 0;

                PAT0Texture* entry = table->Textures;
                foreach (PAT0TextureEntryNode n in Children)
                    n.Rebuild(entry++, PAT0Texture.Size, true);
            }
        }

        internal int _matIndex, _texIndex;
        internal void CompareToAll()
        {
            _rebuild = true;
            foreach (PAT0EntryNode e in Parent.Parent.Children)
            foreach (PAT0TextureNode table in e.Children)
            {
                if (table == this)
                    return;

                if (table != this && table.Children.Count == Children.Count)
                {
                    bool same = true;
                    for (int l = 0; l < Children.Count; l++)
                    {
                        PAT0TextureEntryNode exte = (PAT0TextureEntryNode)table.Children[l];
                        PAT0TextureEntryNode inte = (PAT0TextureEntryNode)Children[l];

                        if (exte._frame != inte._frame || exte.Texture != inte.Texture || exte.Palette != inte.Palette)
                        {
                            same = false;
                            break;
                        }
                    }
                    if (same)
                    {
                        _rebuild = false;
                        _matIndex = e.Index;
                        _texIndex = table.Index;
                        ((PAT0EntryNode)Parent)._dataLens[Index] = -1;
                        return;
                    }
                }
            }
        }

        public void CreateEntry()
        {
            float value = Children.Count > 0 ? ((PAT0TextureEntryNode)Children[Children.Count - 1])._frame + 1 : 0;
            PAT0TextureEntryNode node = new PAT0TextureEntryNode() { _frame = value };
            AddChild(node);
            node.Texture = "NewTexture";
        }

        /// <summary>
        /// Sorts the order of texture entries by their frame index.
        /// </summary>
        public void SortChildren()
        {
            Top:
            for (int i = 0; i < Children.Count; i++)
            {
                PAT0TextureEntryNode t = Children[i] as PAT0TextureEntryNode;
                int x = t.Index;
                t.CheckNext();
                t.CheckPrev();
                if (t.Index != x)
                    goto Top;
            }
        }
    }
    
    public unsafe class PAT0TextureEntryNode : ResourceNode
    {
        internal PAT0Texture* Header { get { return (PAT0Texture*)WorkingUncompressed.Address; } }
        public float _frame;
        public ushort _texFileIndex, _pltFileIndex;

        public string _tex = null, _plt = null;

        public override ResourceType ResourceType { get { return ResourceType.PAT0TextureEntry; } }
        public override bool AllowDuplicateNames { get { return true; } }

        [Category("PAT0 Texture Entry")]
        public float FrameIndex 
        {
            get { return _frame; }
            set
            {
                if (Index == 0)
                {
                    if (Index == Children.Count - 1)
                        _frame = 0;
                    else if (value >= ((PAT0TextureEntryNode)Parent.Children[Index + 1])._frame)
                    {
                        ((PAT0TextureEntryNode)Parent.Children[Index + 1])._frame = 0;
                        Parent.Children[Index + 1].SignalPropertyChange();

                        _frame = value;
                        CheckNext();
                    }
                    SignalPropertyChange();
                    return;
                }

                _frame = value;
                CheckPrev();
                CheckNext();
                
                SignalPropertyChange();
            }
        }

        public void CheckNext()
        {
            if (Index == Parent.Children.Count - 1)
                return;

            int index = Index;
            if (_frame > ((PAT0TextureEntryNode)Parent.Children[Index + 1])._frame)
            {
                DoMoveDown();
                if (index != Index)
                    CheckNext();
            }
        }

        public void CheckPrev()
        {
            if (Index == 0)
                return;

            int index = Index;
            if (_frame < ((PAT0TextureEntryNode)Parent.Children[Index - 1])._frame)
            {
                DoMoveUp();
                if (index != Index)
                    CheckPrev();
            }
        }

        [Category("PAT0 Texture Entry"), Browsable(true), TypeConverter(typeof(DropDownListPAT0Textures))]
        public string Texture
        {
            get { return _tex; }
            set 
            {
                if (!(Parent as PAT0TextureNode)._hasTex || value == _tex)
                    return;

                if (!String.IsNullOrEmpty(value))
                    Name = value;
                
                ((PAT0Node)Parent.Parent.Parent).texChanged = true;
            }
        }
        [Category("PAT0 Texture Entry"), Browsable(true), TypeConverter(typeof(DropDownListPAT0Palettes))]
        public string Palette
        {
            get { return _plt; }
            set 
            {
                if (!(Parent as PAT0TextureNode)._hasPlt || value == _plt)
                    return;

                if (!String.IsNullOrEmpty(value))
                {
                    _plt = value;
                    SignalPropertyChange();
                }
                
                ((PAT0Node)Parent.Parent.Parent).pltChanged = true;
            }
        }

        //[Category("PAT0 Texture")]
        //public ushort TextureFileIndex { get { return texFileIndex; } set { texFileIndex = value; GetName(); SignalPropertyChange(); } }
        //[Category("PAT0 Texture")]
        //public ushort PaletteFileIndex { get { return pltFileIndex; } set { pltFileIndex = value; SignalPropertyChange(); } }

        [Browsable(false)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; _tex = value;/*checkTexture(value, true);*/ }
        }

        public override bool OnInitialize()
        {
            _frame = Header->_key;
            _texFileIndex = Header->_texFileIndex;
            _pltFileIndex = Header->_pltFileIndex;

            GetStrings();

            return false;
        }

        public void GetStrings()
        {
            if (Parent == null)
            {
                _name = "<null>";
                return;
            }

            PAT0Node node = (PAT0Node)Parent.Parent.Parent;

            if (((PAT0TextureNode)Parent)._hasPlt && _pltFileIndex < node._paletteFiles.Count)
                _plt = node._paletteFiles[_pltFileIndex];

            if (((PAT0TextureNode)Parent)._hasTex && _texFileIndex < node._textureFiles.Count)
                _name = _tex = node._textureFiles[_texFileIndex];

            if (_name == null && _plt != null)
                _name = _plt;

            if (_name == null)
                _name = "<null>";
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            PAT0Node node = (PAT0Node)Parent.Parent.Parent;

            PAT0Texture* header = (PAT0Texture*)address;

            header->_key = _frame;

            short i = 0;
            if (((PAT0TextureNode)Parent)._hasTex && !String.IsNullOrEmpty(Texture))
                i = (short)node._textureFiles.IndexOf(Texture);

            if (i < 0)
                _texFileIndex = 0;
            else
                _texFileIndex = (ushort)i;

            header->_texFileIndex = _texFileIndex;

            i = 0;
            if (((PAT0TextureNode)Parent)._hasPlt && !String.IsNullOrEmpty(Palette))
                i = (short)node._paletteFiles.IndexOf(Palette);

            if (i < 0)
                _pltFileIndex = 0;
            else
                _pltFileIndex = (ushort)i;

            header->_pltFileIndex = _pltFileIndex;
        }

        public override int OnCalculateSize(bool force)
        {
            return PAT0Texture.Size;
        }
    }
}
