using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using System.Windows.Forms;
using BrawlLib.Modeling;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class CHR0Node : AnimationNode
    {
        internal BRESCommonHeader* Header { get { return (BRESCommonHeader*)WorkingUncompressed.Address; } }
        internal CHR0v4_3* Header4_3 { get { return (CHR0v4_3*)WorkingUncompressed.Address; } }
        internal CHR0v5* Header5 { get { return (CHR0v5*)WorkingUncompressed.Address; } }

        public override ResourceType ResourceType { get { return ResourceType.CHR0; } }

        public override Type[] AllowedChildTypes
        {
            get
            {
                return new Type[] { typeof(CHR0EntryNode) };
            }
        }

        internal int _numFrames = 1;
        internal int _stringoffset, _dataoffset, _loop;
        internal int _version = 4;

        public int _conversionBias = 0;
        public int _startUpVersion = 4;

        [Category("Animation Data")]
        public int Version
        {
            get { return _version; }
            set
            {
                if (_version == value)
                    return;

                if (value == _startUpVersion)
                    _conversionBias = 0;
                else if (_startUpVersion == 4 && value == 5)
                    _conversionBias = 1;
                else if (_startUpVersion == 5 && value == 4)
                    _conversionBias = -1;

                _version = value;
                SignalPropertyChange();
            }
        }
        [Category("Animation Data")]
        public override int FrameCount
        {
            get { return _numFrames + (_startUpVersion == 5 ? 1 : 0); }
            set
            {
                int bias = (_startUpVersion == 5 ? 1 : 0);
                if ((_numFrames == value - bias) || (value - bias < (1 - bias)))
                    return;

                _numFrames = value - bias;
                
                foreach (CHR0EntryNode n in Children)
                    n.SetSize(FrameCount);

                SignalPropertyChange();
            }
        }
        [Category("Animation Data")]
        public override bool Loop { get { return _loop != 0; } set { _loop = (ushort)(value ? 1 : 0); SignalPropertyChange(); } }

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        [Category("Animation Data")]
        public string OriginalPath { get { return _originalPath; } set { _originalPath = value; SignalPropertyChange(); } }
        public string _originalPath;

        public CHR0EntryNode CreateEntry() { return CreateEntry(null); }
        public CHR0EntryNode CreateEntry(string name)
        {
            CHR0EntryNode n = new CHR0EntryNode();
            n._numFrames = _numFrames;
            n._name = this.FindName(name);
            AddChild(n);
            return n;
        }

        public void InsertKeyframe(int index)
        {
            FrameCount++;
            foreach (CHR0EntryNode c in Children)
                c.Keyframes.Insert(KeyFrameMode.All, index);
        }
        public void DeleteKeyframe(int index)
        {
            foreach (CHR0EntryNode c in Children)
                c.Keyframes.Delete(KeyFrameMode.All, index);
            FrameCount--;
        }
        public int num;
        public override bool OnInitialize()
        {
            base.OnInitialize();

            _startUpVersion = _version = Header->_version;

            if (_version == 5)
            {
                CHR0v5* header = Header5;
                _numFrames = header->_numFrames;
                _loop = header->_loop;

                _dataoffset = header->_dataOffset;
                _stringoffset = header->_stringOffset;

                if (_name == null) 
                    if (Header5->ResourceString != null)
                        _name = Header5->ResourceString;
                    else
                        _name = "anim" + Index;

                if (Header5->_origPathOffset > 0)
                    _originalPath = Header5->OrigPath;

                (_userEntries = new UserDataCollection()).Read(Header5->UserData);

                return Header5->Group->_numEntries > 0;
            }
            else
            {
                CHR0v4_3* header = Header4_3;
                _numFrames = header->_numFrames;
                _loop = header->_loop;
                _dataoffset = header->_dataOffset;
                _stringoffset = header->_stringOffset;

                if (_name == null)
                    if (Header4_3->ResourceString != null)
                        _name = Header4_3->ResourceString;
                    else
                        _name = "anim" + Index;

                if (Header4_3->_origPathOffset > 0)
                    _originalPath = Header4_3->OrigPath;

                return Header4_3->Group->_numEntries > 0;
            }
        }

        public override void OnPopulate()
        {
            ResourceGroup* group = Header4_3->Group;
            for (int i = 0; i < group->_numEntries; i++)
                new CHR0EntryNode().Initialize(this, new DataSource(group->First[i].DataAddress, 0));
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);
            foreach (CHR0EntryNode n in Children)
                table.Add(n.Name);

            if (_version == 5)
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
            int size = (_version == 5 ? CHR0v5.Size : CHR0v4_3.Size) + 0x18 + (Children.Count * 0x10);
            foreach (CHR0EntryNode n in Children)
                size += n.CalculateSize(true);
            if (_version == 5)
                size += _userEntries.GetSize();
            return size;
        }

        public override unsafe void Export(string outPath)
        {
            if (outPath.EndsWith(".dae", StringComparison.OrdinalIgnoreCase))
                Collada.Serialize(new CHR0Node[] { this }, 60.0f, false, outPath);
            else if (outPath.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
                AnimFormat.Serialize(this, false, outPath);
            else
                base.Export(outPath);
        }

        public static CHR0Node FromFile(string path)
        {
            //string ext = Path.GetExtension(path);
            if (path.EndsWith(".chr0", StringComparison.OrdinalIgnoreCase))
                return NodeFactory.FromFile(null, path) as CHR0Node;
            if (path.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                return CHR0TextImporter.Convert(path);
            if (path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
                return AnimFormat.Read(path);
            //if (path.EndsWith(".bvh", StringComparison.OrdinalIgnoreCase))
            //    return BVH.Import(path);
            //if (path.EndsWith(".vmd", StringComparison.OrdinalIgnoreCase))
            //    return PMDModel.ImportVMD(path);

            throw new NotSupportedException("The file extension specified is not of a supported animation type.");
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            ResourceGroup* group;
            if (_version == 5)
            {
                CHR0v5* header = (CHR0v5*)address;
                *header = new CHR0v5(_version, length, _numFrames - _conversionBias, Children.Count, _loop);
                group = header->Group;
            }
            else
            {
                CHR0v4_3* header = (CHR0v4_3*)address;
                *header = new CHR0v4_3(_version, length, _numFrames - _conversionBias, Children.Count, _loop);
                group = header->Group;
            }

            *group = new ResourceGroup(Children.Count);

            VoidPtr entryAddress = group->EndAddress;
            VoidPtr dataAddress = entryAddress;

            foreach (CHR0EntryNode n in Children)
                dataAddress += n._entryLen;

            ResourceEntry* rEntry = group->First;
            foreach (CHR0EntryNode n in Children)
            {
                (rEntry++)->_dataOffset = (int)entryAddress - (int)group;

                n._dataAddr = dataAddress;
                n.Rebuild(entryAddress, n._entryLen, true);
                entryAddress += n._entryLen;
                dataAddress += n._dataLen;
            }

            if (_userEntries.Count > 0 && _version == 5)
            {
                CHR0v5* header = (CHR0v5*)address;
                header->UserData = dataAddress;
                _userEntries.Write(dataAddress);
            }
        }

        protected internal override void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {
            base.PostProcess(bresAddress, dataAddress, dataLength, stringTable);

            ResourceGroup* group;
            if (_version == 5)
            {
                CHR0v5* header = (CHR0v5*)dataAddress;
                header->ResourceStringAddress = (int)stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    header->OrigPathAddress = stringTable[_originalPath] + 4;
                group = header->Group;
            }
            else
            {
                CHR0v4_3* header = (CHR0v4_3*)dataAddress;
                header->ResourceStringAddress = (int)stringTable[Name] + 4;
                if (!String.IsNullOrEmpty(_originalPath))
                    header->OrigPathAddress = stringTable[_originalPath] + 4;
                group = header->Group;
            }

            group->_first = new ResourceEntry(0xFFFF, 0, 0, 0, 0);
            ResourceEntry* rEntry = group->First;

            int index = 1;
            foreach (CHR0EntryNode n in Children)
            {
                dataAddress = (VoidPtr)group + (rEntry++)->_dataOffset;
                ResourceEntry.Build(group, index++, dataAddress, (BRESString*)stringTable[n.Name]);
                n.PostProcess(dataAddress, stringTable);
            }

            if (_version == 5) _userEntries.PostProcess(((CHR0v5*)dataAddress)->UserData, stringTable);
        }

        internal static ResourceNode TryParse(DataSource source) { return ((BRESCommonHeader*)source.Address)->_tag == CHR0v4_3.Tag ? new CHR0Node() : null; }

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
            foreach (CHR0EntryNode e in Children)
            {
                KeyframeCollection newCollection = new KeyframeCollection(newFrameCount);
                for (int x = 0; x < FrameCount; x++)
                {
                    int newFrame = (int)((float)x * ratio + 0.5f);
                    float frameRatio = newFrame == 0 ? 0 : (float)x / (float)newFrame;
                    for (int i = 0x10; i < 0x19; i++)
                        if ((kfe = e.GetKeyframe((KeyFrameMode)i, x)) != null)
                            newCollection.SetFrameValue((KeyFrameMode)i, newFrame, kfe._value)._tangent = kfe._tangent * (float.IsNaN(frameRatio) ? 1 : frameRatio);
                }
                e._keyframes = newCollection;
            }
            FrameCount = newFrameCount;
        }

        public void MergeWith()
        {
            CHR0Node external = null;
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "CHR0 Animation (*.chr0)|*.chr0";
            o.Title = "Please select an animation to merge with.";
            if (o.ShowDialog() == DialogResult.OK)
                if ((external = (CHR0Node)NodeFactory.FromFile(null, o.FileName)) != null)
                    MergeWith(external);
        }

        public void MergeWith(CHR0Node external)
        {
            if (external.FrameCount != FrameCount && MessageBox.Show(null, "Frame counts are not equal; the shorter animation will end early. Do you still wish to continue?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            if (external.FrameCount > FrameCount)
                FrameCount = external.FrameCount;

            foreach (CHR0EntryNode _extTarget in external.Children)
            {
                CHR0EntryNode node = null;
                KeyframeEntry kfe = null;

                CHR0EntryNode entry = new CHR0EntryNode() { Name = _extTarget.Name };
                entry._numFrames = _extTarget.FrameCount;

                //Apply all external keyframes to current entry.
                for (int x = 0; x < _extTarget.FrameCount; x++)
                    for (int i = 0x10; i < 0x19; i++)
                        if ((kfe = _extTarget.GetKeyframe((KeyFrameMode)i, x)) != null)
                            entry.Keyframes.SetFrameValue((KeyFrameMode)i, x, kfe._value)._tangent = kfe._tangent;

                if ((node = FindChild(_extTarget.Name, false) as CHR0EntryNode) == null)
                    AddChild(entry, true);
                else
                {
                    DialogResult result = MessageBox.Show(null, "A bone entry with the name " + _extTarget.Name + " already exists.\nDo you want to rename this entry?\nOtherwise, you will have the option to merge the keyframes.", "Rename Entry?", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                    Top:
                        RenameDialog d = new RenameDialog();
                        if (d.ShowDialog(null, entry) == DialogResult.OK)
                        {
                            if (entry.Name != _extTarget.Name)
                                AddChild(entry, true);
                            else
                            {
                                MessageBox.Show("The name wasn't changed!");
                                goto Top;
                            }
                        }
                    }
                    else if (result == DialogResult.No)
                    {
                        result = MessageBox.Show(null, "Do you want to merge the keyframes of the entries?", "Merge Keyframes?", MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            KeyframeEntry kfe2 = null;

                            if (_extTarget.FrameCount > node.FrameCount)
                                node._numFrames = _extTarget.FrameCount;

                            //Merge all external keyframes with the current entry.
                            for (int x = 0; x < _extTarget.FrameCount; x++)
                                for (int i = 0x10; i < 0x19; i++)
                                    if ((kfe = _extTarget.GetKeyframe((KeyFrameMode)i, x)) != null)
                                        if ((kfe2 = node.GetKeyframe((KeyFrameMode)i, x)) == null)
                                            node.SetKeyframe((KeyFrameMode)i, x, kfe._value);
                                        else
                                        {
                                            result = MessageBox.Show(null, "A keyframe at frame " + x + " already exists.\nOld value: " + kfe2._value + "\nNew value:" + kfe._value + "\nReplace the old value with the new one?", "Replace Keyframe?", MessageBoxButtons.YesNoCancel);
                                            if (result == DialogResult.Yes)
                                                node.SetKeyframe((KeyFrameMode)i, x, kfe._value);
                                            else if (result == DialogResult.Cancel)
                                            {
                                                Restore();
                                                return;
                                            }
                                        }
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            Restore();
                            return;
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        Restore();
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Adds an animation opened by the user to the end of this one
        /// </summary>
        public void Append()
        {
            CHR0Node external = null;
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "CHR0 Animation (*.chr0)|*.chr0";
            o.Title = "Please select an animation to append.";
            if (o.ShowDialog() == DialogResult.OK)
                if ((external = (CHR0Node)NodeFactory.FromFile(null, o.FileName)) != null)
                    Append(external);
        }
        /// <summary>
        /// Adds an animation to the end of this one
        /// </summary>
        public void Append(CHR0Node external)
        {
            KeyframeEntry kfe;

            int origIntCount = FrameCount;
            int extCount = external.FrameCount;
            FrameCount += extCount;

            foreach (CHR0EntryNode _extEntry in external.Children)
            {
                CHR0EntryNode _intEntry = null;
                if ((_intEntry = (CHR0EntryNode)FindChild(_extEntry.Name, false)) == null)
                {
                    CHR0EntryNode newIntEntry = new CHR0EntryNode() { Name = _extEntry.Name };
                    newIntEntry._numFrames = _extEntry.FrameCount + origIntCount;
                    for (int x = 0; x < _extEntry.FrameCount; x++)
                        for (int i = 0x10; i < 0x19; i++)
                            if ((kfe = _extEntry.GetKeyframe((KeyFrameMode)i, x)) != null)
                                newIntEntry.Keyframes.SetFrameValue((KeyFrameMode)i, x + origIntCount, kfe._value)._tangent = kfe._tangent;
                    AddChild(newIntEntry);
                }
                else
                    for (int x = 0; x < _extEntry.FrameCount; x++)
                        for (int i = 0x10; i < 0x19; i++)
                            if ((kfe = _extEntry.GetKeyframe((KeyFrameMode)i, x)) != null)
                                _intEntry.Keyframes.SetFrameValue((KeyFrameMode)i, x + origIntCount, kfe._value)._tangent = kfe._tangent;
            }
        }

        public void Port(MDL0Node baseModel)
        {
            MDL0Node model;

            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.Filter = "MDL0 Model (*.mdl0)|*.mdl0";
            dlgOpen.Title = "Select the model this animation is for...";

            if (dlgOpen.ShowDialog() == DialogResult.OK)
                if ((model = (MDL0Node)NodeFactory.FromFile(null, dlgOpen.FileName)) != null)
                    Port(baseModel, model);
        }

        public void Port(MDL0Node _targetModel, MDL0Node _extModel)
        {
            MDL0BoneNode extBone;
            MDL0BoneNode bone;
            KeyframeEntry kfe;
            float difference = 0;
            foreach (CHR0EntryNode _target in Children)
            {
                extBone = (MDL0BoneNode)_extModel.FindChild(_target.Name, true); //Get external model bone
                bone = (MDL0BoneNode)_targetModel.FindChild(_target.Name, true); //Get target model bone

                for (int x = 0; x < _target.FrameCount; x++)
                    for (int i = 0x13; i < 0x19; i++)
                        if ((kfe = _target.GetKeyframe((KeyFrameMode)i, x)) != null) //Check for a keyframe
                        {
                            if (bone != null && extBone != null)
                                switch (i)
                                {
                                    //Translations
                                    case 0x16: //Trans X
                                        if (Math.Round(kfe._value, 4) == Math.Round(extBone._bindState.Translate._x, 4))
                                            kfe._value = bone._bindState.Translate._x;
                                        else if (bone._bindState.Translate._x < extBone._bindState.Translate._x)
                                            kfe._value -= extBone._bindState.Translate._x - bone._bindState.Translate._x;
                                        else if (bone._bindState.Translate._x > extBone._bindState.Translate._x)
                                            kfe._value += bone._bindState.Translate._x - extBone._bindState.Translate._x;
                                        break;
                                    case 0x17: //Trans Y
                                        if (Math.Round(kfe._value, 4) == Math.Round(extBone._bindState.Translate._y, 4))
                                            kfe._value = bone._bindState.Translate._y;
                                        else if (bone._bindState.Translate._y < extBone._bindState.Translate._y)
                                            kfe._value -= extBone._bindState.Translate._y - bone._bindState.Translate._y;
                                        else if (bone._bindState.Translate._y > extBone._bindState.Translate._y)
                                            kfe._value += bone._bindState.Translate._y - extBone._bindState.Translate._y;
                                        break;
                                    case 0x18: //Trans Z
                                        if (Math.Round(kfe._value, 4) == Math.Round(extBone._bindState.Translate._z, 4))
                                            kfe._value = bone._bindState.Translate._z;
                                        else if (bone._bindState.Translate._z < extBone._bindState.Translate._z)
                                            kfe._value -= extBone._bindState.Translate._z - bone._bindState.Translate._z;
                                        else if (bone._bindState.Translate._z > extBone._bindState.Translate._z)
                                            kfe._value += bone._bindState.Translate._z - extBone._bindState.Translate._z;
                                        break;

                                    //Rotations
                                    //case 0x13: //Rot X
                                    //    difference = bone._bindState.Rotate._x - extBone._bindState.Rotate._x;
                                    //    kfe._value += difference;
                                    //    //if (difference != 0)
                                    //    //    FixChildren(bone, 0);
                                    //    break;
                                    //case 0x14: //Rot Y
                                    //    difference = bone._bindState.Rotate._y - extBone._bindState.Rotate._y;
                                    //    kfe._value += difference;
                                    //    //if (difference != 0)
                                    //    //    FixChildren(bone, 1);
                                    //    break;
                                    //case 0x15: //Rot Z
                                    //    difference = bone._bindState.Rotate._z - extBone._bindState.Rotate._z;
                                    //    kfe._value += difference;
                                    //    //if (difference != 0)
                                    //    //    FixChildren(bone, 2);
                                    //    break;
                                }
                            if (kfe._value == float.NaN || kfe._value == float.PositiveInfinity || kfe._value == float.NegativeInfinity)
                            {
                                kfe.Remove();
                                _target.Keyframes._keyCounts[i]--;
                            }
                        }
            }
            _changed = true;
        }

        private void FixChildren(MDL0BoneNode node, int axis)
        {
            KeyframeEntry kfe;
            foreach (MDL0BoneNode b in node.Children)
            {
                CHR0EntryNode _target = (CHR0EntryNode)FindChild(b.Name, true);
                if (_target != null)
                    switch (axis)
                    {
                        case 0: //X, correct Y and Z
                            for (int l = 0; l < _target.FrameCount; l++)
                                for (int g = 0x13; g < 0x16; g++)
                                    if (g != 0x13)
                                        if ((kfe = _target.GetKeyframe((KeyFrameMode)g, l)) != null)
                                            kfe._value *= -1;
                            break;
                        case 1: //Y, correct X and Z
                            for (int l = 0; l < _target.FrameCount; l++)
                                for (int g = 0x13; g < 0x16; g++)
                                    if (g != 0x14)
                                        if ((kfe = _target.GetKeyframe((KeyFrameMode)g, l)) != null)
                                            kfe._value *= -1;
                            break;
                        case 2: //Z, correct X and Y
                            for (int l = 0; l < _target.FrameCount; l++)
                                for (int g = 0x13; g < 0x16; g++)
                                    if (g != 0x15)
                                        if ((kfe = _target.GetKeyframe((KeyFrameMode)g, l)) != null)
                                            kfe._value *= -1;
                            break;
                    }
                FixChildren(b, axis);
            }
        }

        public void AverageKeys()
        {
            foreach (CHR0EntryNode w in Children)
                for (int i = 0; i < 9; i++)
                    if (w.Keyframes._keyCounts[i] > 1)
                    {
                        KeyframeEntry root = w.Keyframes._keyRoots[i];
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

        public void AverageKeys(string boneName)
        {
            CHR0EntryNode w = FindChild(boneName, false) as CHR0EntryNode;
            if (w == null)
                return;

            for (int i = 0; i < 9; i++)
                if (w.Keyframes._keyCounts[i] > 1)
                {
                    KeyframeEntry root = w.Keyframes._keyRoots[i];
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

    public unsafe class CHR0EntryNode : ResourceNode, IKeyframeHolder
    {
        internal CHR0Entry* Header { get { return (CHR0Entry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.CHR0Entry; } }

        public int _numFrames;
        [Browsable(false)]
        public int FrameCount { get { return _numFrames; } }

        internal KeyframeCollection _keyframes = null;
        [Browsable(false)]
        public KeyframeCollection Keyframes 
        {
            get 
            {
                if (_keyframes == null)
                {
                    if (Header != null)
                        _keyframes = AnimationConverter.DecodeCHR0Keyframes(Header, FrameCount);
                    else
                        _keyframes = new KeyframeCollection(FrameCount);
                }
                return _keyframes;
            } 
        }

        bool _useModelScale, _useModelRotate, _useModelTranslate, _scaleCompApply, _scaleCompParent, _classicScaleOff;
        
        //public bool UseModelScale { get { return _useModelScale; } set { _useModelScale = value; SignalPropertyChange(); } }
        //public bool UseModelRotate { get { return _useModelRotate; } set { _useModelRotate = value; SignalPropertyChange(); } }
        //public bool UseModelTranslate { get { return _useModelTranslate; } set { _useModelTranslate = value; SignalPropertyChange(); } }

        //public bool ScaleCompensateApply { get { return _scaleCompApply; } set { _scaleCompApply = value; SignalPropertyChange(); } }
        //public bool ScaleCompensateParent { get { return _scaleCompParent; } set { _scaleCompParent = value; SignalPropertyChange(); } }
        //public bool ClassicScaleOff { get { return _classicScaleOff; } set { _classicScaleOff = value; SignalPropertyChange(); } }
        
        [Browsable(false)]
        public AnimationCode Code { get { if (Header != null) return Header->Code; else return 0; } }

        internal int _dataLen;
        internal int _entryLen;
        internal VoidPtr _dataAddr;
        public override int OnCalculateSize(bool force)
        {
            _dataLen = AnimationConverter.CalculateCHR0Size(Keyframes, out _entryLen);
            return _dataLen + _entryLen;
        }

        public override bool OnInitialize()
        {
            _keyframes = null;

            if (_parent is CHR0Node)
                _numFrames = ((CHR0Node)_parent).FrameCount;

            if ((_name == null) && (Header->_stringOffset != 0))
                _name = Header->ResourceString;

            _useModelScale = Header->Code.UseModelScale;
            _useModelRotate = Header->Code.UseModelRot;
            _useModelTranslate = Header->Code.UseModelTrans;

            _scaleCompApply = Header->Code.ScaleCompApply;
            _scaleCompParent = Header->Code.ScaleCompParent;
            _classicScaleOff = Header->Code.ClassicScaleOff;

            return false;
        }

        public override unsafe void Export(string outPath)
        {
            StringTable table = new StringTable();
            table.Add(_name);

            int dataLen = OnCalculateSize(true);
            int totalLen = dataLen + table.GetTotalSize();

            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(totalLen);
                using (FileMap map = FileMap.FromStream(stream))
                {
                    AnimationConverter.EncodeCHR0Keyframes(Keyframes, map.Address, map.Address + _entryLen);
                    table.WriteTable(map.Address + dataLen);
                    PostProcess(map.Address, table);
                }
            }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            //_keyframes._evalCode.UseModelScale = _useModelScale;
            //_keyframes._evalCode.UseModelRot = _useModelRotate;
            //_keyframes._evalCode.UseModelTrans = _useModelTranslate;

            //_keyframes._evalCode.ScaleCompApply = _scaleCompApply;
            //_keyframes._evalCode.ScaleCompParent = _scaleCompParent;
            //_keyframes._evalCode.ClassicScaleOff = _classicScaleOff;

            AnimationConverter.EncodeCHR0Keyframes(_keyframes, address, _dataAddr);
        }

        protected internal virtual void PostProcess(VoidPtr dataAddress, StringTable stringTable)
        {
            CHR0Entry* header = (CHR0Entry*)dataAddress;
            header->ResourceStringAddress = stringTable[Name] + 4;
        }

        internal void SetSize(int count)
        {
            if (_keyframes != null)
                Keyframes.FrameCount = count;

            _numFrames = count;
            SignalPropertyChange();
        }

        #region Keyframe Management

        public static bool _generateTangents = true;
        public static bool _linear = false;

        public float GetFrameValue(KeyFrameMode mode, int index) { return Keyframes.GetFrameValue(mode, index); }
        public float GetFrameValue(KeyFrameMode mode, int index, bool linear, bool loop) { return Keyframes.GetFrameValue(mode, index, linear, loop); }

        public KeyframeEntry GetKeyframe(KeyFrameMode mode, int index) { return Keyframes.GetKeyframe(mode, index); }
        public KeyframeEntry SetKeyframe(KeyFrameMode mode, int index, float value)
        {
            bool exists = Keyframes.GetKeyframe(mode, index) != null;
            KeyframeEntry k = Keyframes.SetFrameValue(mode, index, value);

            if (!exists && !_generateTangents)
                k.GenerateTangent();

            if (_generateTangents)
            {
                k.GenerateTangent();
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
            }

            SignalPropertyChange(); 
            return k;
        }
        public KeyframeEntry SetKeyframe(KeyFrameMode mode, int index, float value, bool forceNoGenTans)
        {
            KeyframeEntry k = Keyframes.SetFrameValue(mode, index, value);
            if (_generateTangents && !forceNoGenTans)
            {
                k.GenerateTangent();
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
            }

            SignalPropertyChange();
            return k;
        }
        public void SetKeyframe(int index, AnimationFrame frame)
        {
            float* v = (float*)&frame;
            for (int i = 0x10; i < 0x19; i++)
                SetKeyframe((KeyFrameMode)i, index, *v++);
        }

        public void SetKeyframeOnlyTrans(int index, AnimationFrame frame)
        {
            float* v = (float*)&frame.Translation;
            for (int i = 0x16; i < 0x19; i++)
                SetKeyframe((KeyFrameMode)i, index, *v++);
        }

        public void SetKeyframeOnlyRot(int index, AnimationFrame frame)
        {
            float* v = (float*)&frame.Rotation;
            for (int i = 0x13; i < 0x16; i++)
                SetKeyframe((KeyFrameMode)i, index, *v++);
        }
        
        public void SetKeyframeOnlyScale(int index, AnimationFrame frame)
        {
            float* v = (float*)&frame.Scale;
            for (int i = 0x10; i < 0x13; i++)
                SetKeyframe((KeyFrameMode)i, index, *v++);
        }

        public void SetKeyframeOnlyTrans(int index, Vector3 trans)
        {
            float* v = (float*)&trans;
            for (int i = 0x16; i < 0x19; i++)
                SetKeyframe((KeyFrameMode)i, index, *v++);
        }

        public void SetKeyframeOnlyRot(int index, Vector3 rot)
        {
            float* v = (float*)&rot;
            for (int i = 0x13; i < 0x16; i++)
                SetKeyframe((KeyFrameMode)i, index, *v++);
        }

        public void SetKeyframeOnlyScale(int index, Vector3 scale)
        {
            float* v = (float*)&scale;
            for (int i = 0x10; i < 0x13; i++)
                SetKeyframe((KeyFrameMode)i, index, *v++);
        }

        public void RemoveKeyframe(KeyFrameMode mode, int index)
        {
            KeyframeEntry k = Keyframes.Remove(mode, index);
            if (k != null && _generateTangents)
            {
                k._prev.GenerateTangent();
                k._next.GenerateTangent();
                SignalPropertyChange();
            }
        }

        public void RemoveKeyframe(int index)
        {
            for (int i = 0x10; i < 0x19; i++)
                RemoveKeyframe((KeyFrameMode)i, index);
        }

        public void RemoveKeyframeOnlyTrans(int index)
        {
            for (int i = 0x16; i < 0x19; i++)
                RemoveKeyframe((KeyFrameMode)i, index);
        }

        public void RemoveKeyframeOnlyRot(int index)
        {
            for (int i = 0x13; i < 0x16; i++)
                RemoveKeyframe((KeyFrameMode)i, index);
        }

        public void RemoveKeyframeOnlyScale(int index)
        {
            for (int i = 0x10; i < 0x13; i++)
                RemoveKeyframe((KeyFrameMode)i, index);
        }

        public AnimationFrame GetAnimFrame(int index)
        {
            AnimationFrame a = Keyframes.GetFullFrame(index);
            a.forKeyframeCHR = true;
            return a;
        }
        public AnimationFrame GetAnimFrame(int index, bool linear)
        {
            AnimationFrame a = Keyframes.GetFullFrame(index, linear);
            a.forKeyframeCHR = true;
            return a;
        }

        #endregion
    }
}
