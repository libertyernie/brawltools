using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Wii.Animations;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class SHP0Node : AnimationNode
    {
        internal BRESCommonHeader* Header { get { return (BRESCommonHeader*)WorkingUncompressed.Address; } }
        internal SHP0v3* Header3 { get { return (SHP0v3*)WorkingUncompressed.Address; } }
        internal SHP0v4* Header4 { get { return (SHP0v4*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.SHP0; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(SHP0EntryNode) };
            }
        }

        int _version = 3, _numFrames = 1, _loop;

        public int ConversionBias = 0;
        public int startUpVersion = 3;

        [Category("Vertex Morph Data")]
        public int Version
        {
            get { return _version; }
            set
            {
                if (_version == value)
                    return;

                if (value == startUpVersion)
                    ConversionBias = 0;
                else if (startUpVersion == 3 && value == 4)
                    ConversionBias = 1;
                else if (startUpVersion == 4 && value == 3)
                    ConversionBias = -1;

                _version = value;
                SignalPropertyChange();
            }
        }
        [Category("Vertex Morph Data")]
        public override int FrameCount
        {
            get { return _numFrames + (startUpVersion == 4 ? 1 : 0); }
            set
            {
                int bias = (startUpVersion == 4 ? 1 : 0);

                if ((_numFrames == value - bias) || (value - bias < (1 - bias)))
                    return;

                _numFrames = value - bias;

                foreach (SHP0EntryNode n in Children)
                    foreach (SHP0VertexSetNode s in n.Children)
                        s.SetSize(FrameCount);

                SignalPropertyChange();
            }
        }
        [Category("Vertex Morph Data")]
        public override bool Loop { get { return _loop != 0; } set { _loop = value ? 1 : 0; SignalPropertyChange(); } }

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        [Category("Vertex Morph Data")]
        public string OriginalPath { get { return _originalPath; } set { _originalPath = value; SignalPropertyChange(); } }
        public string _originalPath;

        public void InsertKeyframe(int index)
        {
            FrameCount++;
            foreach (SHP0EntryNode e in Children)
                foreach (SHP0VertexSetNode c in e.Children)
                    c.Keyframes.Insert(index);
        }
        public void DeleteKeyframe(int index)
        {
            foreach (SHP0EntryNode e in Children)
                foreach (SHP0VertexSetNode c in e.Children)
                    c.Keyframes.Delete(index);
            FrameCount--;
        }

        //public string[] StringEntries { get { return _strings.ToArray(); } }
        internal List<string> _strings = new List<string>();

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);

            foreach (string s in _strings)
                table.Add(s);

            foreach (SHP0EntryNode n in Children)
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

        public override bool OnInitialize()
        {
            base.OnInitialize();

            startUpVersion = _version = Header->_version;
            _strings.Clear();

            if (_version == 4)
            {
                if ((_name == null) && (Header4->_stringOffset != 0))
                    _name = Header4->ResourceString;

                _numFrames = Header4->_numFrames;
                _loop = Header4->_loop;

                bint* stringOffset = Header4->StringEntries;
                for (int i = 0; i < Header4->_numEntries; i++)
                    _strings.Add(new String((sbyte*)stringOffset + stringOffset[i]));

                if (Header4->_origPathOffset > 0)
                    _originalPath = Header4->OrigPath;

                (_userEntries = new UserDataCollection()).Read(Header4->UserData);

                return Header4->Group->_numEntries > 0;
            }
            else
            {
                if ((_name == null) && (Header3->_stringOffset != 0))
                    _name = Header3->ResourceString;

                _numFrames = Header3->_numFrames;
                _loop = Header3->_loop;

                bint* stringOffset = Header3->StringEntries;
                for (int i = 0; i < Header3->_numEntries; i++)
                    _strings.Add(new String((sbyte*)stringOffset + stringOffset[i]));

                if (Header3->_origPathOffset > 0)
                    _originalPath = Header3->OrigPath;

                return Header3->Group->_numEntries > 0;
            }
        }

        public override void OnPopulate()
        {
            ResourceGroup* group = Header4->Group;
            for (int i = 0; i < group->_numEntries; i++)
                new SHP0EntryNode().Initialize(this, new DataSource(group->First[i].DataAddress, 0));
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            ResourceGroup* group;
            if (_version == 4)
            {
                SHP0v4* header = (SHP0v4*)address;
                *header = new SHP0v4(_loop, (ushort)(_numFrames - ConversionBias), (ushort)_strings.Count);
                group = header->Group;
            }
            else
            {
                SHP0v3* header = (SHP0v3*)address;
                *header = new SHP0v3(_loop, (ushort)(_numFrames - ConversionBias), (ushort)_strings.Count);
                group = header->Group;
            }

            *group = new ResourceGroup(Children.Count);

            VoidPtr entryAddress = group->EndAddress;
            VoidPtr dataAddress = entryAddress;

            foreach (SHP0EntryNode n in Children)
                dataAddress += n._entryLen;

            ResourceEntry* rEntry = group->First;
            foreach (SHP0EntryNode n in Children)
            {
                (rEntry++)->_dataOffset = (int)entryAddress - (int)group;

                n._dataAddr = dataAddress;
                n.Rebuild(entryAddress, n._entryLen, true);
                entryAddress += n._entryLen;
                dataAddress += n._dataLen;
            }

            ((SHP0v3*)address)->_stringListOffset = (int)dataAddress - (int)address;

            if (_userEntries.Count > 0 && _version == 4)
            {
                SHP0v4* header = (SHP0v4*)address;
                header->UserData = dataAddress;
                _userEntries.Write(dataAddress);
            }
        }

        public override int OnCalculateSize(bool force)
        {
            _strings.Clear();
            int size = (Version == 4 ? SHP0v4.Size : SHP0v3.Size) + 0x18 + Children.Count * 0x10;
            foreach (SHP0EntryNode entry in Children)
            {
                _strings.Add(entry.Name);
                foreach (SHP0VertexSetNode n in entry.Children)
                    _strings.Add(n.Name);

                size += entry.CalculateSize(true);
            }
            size += _strings.Count * 4;
            if (_version == 4)
            size += _userEntries.GetSize();
            return size;
        }

        protected internal override void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {
            base.PostProcess(bresAddress, dataAddress, dataLength, stringTable);

            SHP0v3* header = (SHP0v3*)dataAddress;
            if (_version == 4)
            {
                ((SHP0v4*)dataAddress)->ResourceStringAddress = stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    ((SHP0v4*)dataAddress)->OrigPathAddress = stringTable[_originalPath] + 4;
            }
            else
            {
                header->ResourceStringAddress = stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    header->OrigPathAddress = stringTable[_originalPath] + 4;
            }

            bint* stringPtr = header->StringEntries;
            for (int i = 0; i < header->_numEntries; i++)
                stringPtr[i] = ((int)stringTable[_strings[i]] + 4) - (int)stringPtr;

            ResourceGroup* group = header->Group;
            group->_first = new ResourceEntry(0xFFFF, 0, 0, 0, 0);

            ResourceEntry* rEntry = group->First;

            int index = 1;
            foreach (SHP0EntryNode n in Children)
            {
                dataAddress = (VoidPtr)group + (rEntry++)->_dataOffset;
                ResourceEntry.Build(group, index++, dataAddress, (BRESString*)stringTable[n.Name]);
                n.PostProcess(dataAddress, stringTable);
            }

            if (_version == 4) _userEntries.PostProcess(((SHP0v4*)dataAddress)->UserData, stringTable);
        }

        internal static ResourceNode TryParse(DataSource source) { return ((BRESCommonHeader*)source.Address)->_tag == SHP0v3.Tag ? new SHP0Node() : null; }

        public unsafe SHP0EntryNode FindOrCreateEntry(string name)
        {
            foreach (SHP0EntryNode t in Children)
                if (t.Name == name)
                    return t;

            SHP0EntryNode entry = new SHP0EntryNode();
            entry.Name = name;
            entry._flags = 3;
            AddChild(entry);
            entry.AddChild(new SHP0VertexSetNode(FindName("NewMorphTarget")));
            return entry;
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
            KeyframeEntry kfe = null;
            float ratio = (float)newFrameCount / (float)FrameCount;
            foreach (SHP0EntryNode s in Children)
                foreach (SHP0VertexSetNode e in s.Children)
                {
                    KeyframeArray newArr = new KeyframeArray(newFrameCount);
                    for (int x = 0; x < FrameCount; x++)
                        if ((kfe = e.GetKeyframe(x)) != null)
                        {
                            int newFrame = (int)((float)x * ratio + 0.5f);
                            float frameRatio = newFrame == 0 ? 0 : (float)x / (float)newFrame;
                            newArr.SetFrameValue(newFrame, kfe._value)._tangent = kfe._tangent * (float.IsNaN(frameRatio) ? 1 : frameRatio);
                        }
                    e._keyframes = newArr;
                }
            FrameCount = newFrameCount;
        }
        /// <summary>
        /// Adds an animation opened by the user to the end of this one
        /// </summary>
        public void Append()
        {
            SHP0Node external = null;
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "SHP0 Animation (*.shp0)|*.shp0";
            o.Title = "Please select an animation to append.";
            if (o.ShowDialog() == DialogResult.OK)
                if ((external = (SHP0Node)NodeFactory.FromFile(null, o.FileName)) != null)
                    Append(external);
        }
        /// <summary>
        /// Adds an animation to the end of this one
        /// </summary>
        public void Append(SHP0Node external)
        {
            KeyframeEntry kfe;

            int origIntCount = FrameCount;
            FrameCount += external.FrameCount;

            foreach (SHP0EntryNode w in external.Children)
                foreach (SHP0VertexSetNode _extEntry in w.Children)
                {
                    SHP0VertexSetNode _intEntry = null;
                    if ((_intEntry = (SHP0VertexSetNode)FindChild(w.Name + "/" + _extEntry.Name, false)) == null)
                    {
                        SHP0EntryNode wi = null;
                        if ((wi = (SHP0EntryNode)FindChild(w.Name, false)) == null)
                            AddChild(wi = new SHP0EntryNode() { Name = FindName(null), _flags = w._flags });

                        SHP0VertexSetNode newIntEntry = new SHP0VertexSetNode(_extEntry.Name);
                        newIntEntry._numFrames = _extEntry.FrameCount + origIntCount;
                        for (int x = 0; x < _extEntry.FrameCount; x++)
                                if ((kfe = _extEntry.GetKeyframe(x)) != null)
                                    newIntEntry.Keyframes.SetFrameValue(x + origIntCount, kfe._value)._tangent = kfe._tangent;
                        wi.AddChild(newIntEntry);
                    }
                    else
                        for (int x = 0; x < _extEntry.FrameCount; x++)
                            if ((kfe = _extEntry.GetKeyframe(x)) != null)
                                _intEntry.Keyframes.SetFrameValue(x + origIntCount, kfe._value)._tangent = kfe._tangent;
                }
        }
        public void AverageKeys()
        {
            foreach (SHP0EntryNode u in Children)
                foreach (SHP0VertexSetNode w in u.Children)
                    if (w.Keyframes._keyCount > 1)
                    {
                        KeyframeEntry root = w.Keyframes._keyRoot;
                        if (root._next != root && root._prev != root && root._prev != root._next)
                        {
                            float tan = (root._next._tangent + root._prev._tangent) / 2.0f;
                            float val = (root._next._value + root._prev._value) / 2.0f;

                            root._next._tangent = tan;
                            root._prev._tangent = tan;

                            root._next._value = val;
                            root._prev._value = val;
                        }
                    }

            SignalPropertyChange();
        }

        public void AverageKeys(string baseName, string morphName)
        {
            SHP0EntryNode w = FindChild(baseName, false) as SHP0EntryNode;
            if (w == null)
                return;

            SHP0VertexSetNode t = w.FindChild(morphName, false) as SHP0VertexSetNode;
            if (t == null)
                return;

            if (t.Keyframes._keyCount > 1)
            {
                KeyframeEntry root = t.Keyframes._keyRoot;
                if (root._next != root && root._prev != root && root._prev != root._next)
                {
                    float tan = (root._next._tangent + root._prev._tangent) / 2.0f;
                    float val = (root._next._value + root._prev._value) / 2.0f;

                    root._next._tangent = tan;
                    root._prev._tangent = tan;

                    root._next._value = val;
                    root._prev._value = val;
                }
            }

            SignalPropertyChange();
        }
        #endregion
    }

    public unsafe class SHP0EntryNode : ResourceNode
    {
        internal SHP0Entry* Header { get { return (SHP0Entry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.SHP0Entry; } }
        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(SHP0VertexSetNode) };
            }
        }

        List<short> _indices;
        public Bin32 _flags = 3;
        public int _indexCount, _fixedFlags;

        [Flags]
        public enum SHP0EntryFlags
        {
            Enabled = 1,
            UpdatePosition = 2,
            UpdateNormals = 4,
            UpdateColors = 8,
        }

        [Category("Vertex Morph Entry")]
        public bool Enabled { get { return _flags[0]; } set { _flags[0] = value; SignalPropertyChange(); } }
        [Category("Vertex Morph Entry")]
        public bool UpdatePosition { get { return _flags[1]; } set { _flags[1] = value; SignalPropertyChange(); } }
        [Category("Vertex Morph Entry")]
        public bool UpdateNormals { get { return _flags[2]; } set { _flags[2] = value; SignalPropertyChange(); } }
        [Category("Vertex Morph Entry")]
        public bool UpdateColors { get { return _flags[3]; } set { _flags[3] = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            if ((_name == null) && (Header->_stringOffset != 0))
                _name = Header->ResourceString;

            _indices = new List<short>();
            for (int i = 0; i < Header->_numIndices; i++)
                _indices.Add((short)(Header->Indicies[i]));

            _flags = (uint)(int)Header->_flags;
            _indexCount = Header->_numIndices;
            _fixedFlags = Header->_fixedFlags;

            return Header->_flags > 0;
        }

        public override void OnPopulate()
        {
            for (int i = 0; i < _indexCount; i++)
                if ((_fixedFlags >> i & 1) == 0)
                    new SHP0VertexSetNode(_indices[i] < ((SHP0Node)Parent)._strings.Count ? ((SHP0Node)Parent)._strings[_indices[i]] : "Unknown").Initialize(this, new DataSource(Header->GetEntry(i), 0x14 + Header->_numIndices * 6));
                else
                {
                    SHP0VertexSetNode n = new SHP0VertexSetNode(_indices[i] < ((SHP0Node)Parent)._strings.Count ? ((SHP0Node)Parent)._strings[_indices[i]] : "Unknown") { _isFixed = true };
                    n.Keyframes[0] = ((bfloat*)Header->EntryOffset)[i];
                    n._numFrames = ((SHP0Node)Parent).FrameCount;
                    _children.Add(n);
                    n._parent = this;
                }
        }

        public VoidPtr _dataAddr;
        public int _dataLen, _entryLen;

        protected internal virtual void PostProcess(VoidPtr dataAddress, StringTable stringTable)
        {
            SHP0Entry* header = (SHP0Entry*)dataAddress;
            header->ResourceStringAddress = stringTable[Name] + 4;
        }

        public override int OnCalculateSize(bool force)
        {
            _entryLen = (0x14 + Children.Count * 6).Align(4);
            _dataLen = 0;

            foreach (SHP0VertexSetNode p in Children)
                _dataLen += p.CalculateSize(true);

            return _entryLen + _dataLen;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            SHP0Entry* header = (SHP0Entry*)address;
            VoidPtr addr = _dataAddr;
            header->_numIndices = (short)Children.Count;
            header->_nameIndex = (short)((SHP0Node)Parent)._strings.IndexOf(Name);
            header->_flags = (int)(uint)_flags;
            header->_indiciesOffset = 0x14 + Children.Count * 4;
            uint fixedflags = 0;
            foreach (SHP0VertexSetNode p in Children)
            {
                p._dataAddr = addr;
                header->Indicies[p.Index] = (short)((SHP0Node)Parent)._strings.IndexOf(p.Name);
                if (p._isFixed)
                {
                    KeyframeEntry kf; float value = 0;
                    if ((kf = p.Keyframes.GetKeyframe(0)) != null)
                        value = kf._value;
                    ((bfloat*)header->EntryOffset)[p.Index] = value;
                    fixedflags = (uint)(fixedflags & ((uint)0xFFFFFFFF - (uint)(1 << p.Index))) | (uint)(1 << p.Index);
                }
                else
                {
                    header->EntryOffset[p.Index] = (int)(p._dataAddr - &header->EntryOffset[p.Index]);
                    p.Rebuild(p._dataAddr, p._calcSize, true);
                }
                addr += p._dataLen;
            }
            header->_fixedFlags = (int)fixedflags;
        }

        public unsafe SHP0VertexSetNode FindOrCreateEntry(string name)
        {
            foreach (SHP0VertexSetNode t in Children)
                if (t.Name == name)
                    return t;

            SHP0VertexSetNode entry = new SHP0VertexSetNode(name) { _numFrames = ((SHP0Node)Parent).FrameCount };
            AddChild(entry);
            return entry;
        }

        public void CreateEntry()
        {
            SHP0VertexSetNode morph = new SHP0VertexSetNode(FindName("NewMorphTarget"));
            AddChild(morph);
        }
    }

    public unsafe class SHP0VertexSetNode : ResourceNode, IKeyframeArrayHolder
    {
        internal SHP0KeyframeEntries* Header { get { return (SHP0KeyframeEntries*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.SHP0VertexSet; } }
        
        public int _dataLen;
        public VoidPtr _dataAddr;
        internal void SetSize(int count)
        {
            if (_keyframes != null)
                Keyframes.FrameLimit = count;

            _numFrames = count;
            SignalPropertyChange();
        }

        internal int _numFrames;
        [Browsable(false)]
        public int FrameCount { get { return _numFrames; } }

        internal KeyframeArray _keyframes;
        [Browsable(false)]
        public KeyframeArray Keyframes
        {
            get
            {
                if (_keyframes == null)
                {
                    _keyframes = new KeyframeArray(_numFrames) { _linear = true };
                    if (Header != null)
                    {
                        int fCount = Header->_numEntries;
                        BVec3* entry = Header->Entries;
                        for (int i = 0; i < fCount; i++, entry++)
                            Keyframes.SetFrameValue((int)entry->_y, entry->_z)._tangent = entry->_x;
                    }
                }
                return _keyframes;
            }
        }

        public bool _isFixed = false;
        public float _fixedValue;

        public SHP0VertexSetNode(string name) { _name = name; }

        public override bool OnInitialize()
        {
            if (_parent is SHP0EntryNode && _parent._parent is SHP0Node)
                _numFrames = ((SHP0Node)_parent._parent).FrameCount;

            return false;
        }

        public override int OnCalculateSize(bool force)
        {
            return _dataLen = ((_isFixed = Keyframes._keyCount <= 1) ? 0 : Keyframes._keyCount * 12 + 8);
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            SHP0KeyframeEntries* header = (SHP0KeyframeEntries*)address;
            header->_numEntries = (short)Keyframes._keyCount;
            header->_unk1 = 0;

            BVec3* entry = header->Entries;
            KeyframeEntry frame, root = Keyframes._keyRoot;
            for (frame = root._next; frame._index != -1; frame = frame._next)
                *entry++ = new Vector3(frame._tangent, frame._index, frame._value);
        }

        #region Keyframe Management

        public static bool _generateTangents = true;
        public static bool _linear = true;

        public KeyframeEntry GetKeyframe(int index) { return Keyframes.GetKeyframe(index); }
        public void SetKeyframe(int index, float value)
        {
            bool exists = Keyframes.GetKeyframe(index) != null;
            KeyframeEntry k = Keyframes.SetFrameValue(index, value);

            if (!exists && !_generateTangents)
                k.GenerateTangent();

            if (_generateTangents)
            {
                k.GenerateTangent();
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
            }

            SignalPropertyChange();
        }
        public void RemoveKeyframe(int index)
        {
            KeyframeEntry k = Keyframes.Remove(index);
            if (k != null && _generateTangents)
            {
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
                SignalPropertyChange();
            }
        }

        #endregion
    }

    public class SHP0Keyframe
    {
        public float _index, _percentage, _tangent;

        public static implicit operator SHP0Keyframe(Vector3 v) { return new SHP0Keyframe(v._x, v._y, v._z); }
        public static implicit operator Vector3(SHP0Keyframe v) { return new Vector3(v._tangent, v._index, v._percentage); }
        
        public SHP0Keyframe(float tan, float index, float percent) { _index = index; _percentage = percent; _tangent = tan; }
        public SHP0Keyframe() { }
        
        public float Index { get { return _index; } set { _index = value; } }
        public float Percentage { get { return _percentage; } set { _percentage = value; } }
        public float Tangent { get { return _tangent; } set { _tangent = value; } }

        public override string ToString()
        {
            return String.Format("Index={0}, Percentage={1}, Tangent={2}", _index, String.Format("{0}%", _percentage * 100.0f), _tangent);
        }
    }
}
