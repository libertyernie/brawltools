using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Imaging;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class SCN0Node : AnimationNode
    {
        internal BRESCommonHeader* Header { get { return (BRESCommonHeader*)WorkingUncompressed.Address; } }
        internal SCN0v4* Header4 { get { return (SCN0v4*)WorkingUncompressed.Address; } }
        internal SCN0v5* Header5 { get { return (SCN0v5*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.SCN0; } }
        
        //public static SortedDictionary<int, string> strings = new SortedDictionary<int, string>();

        public int _version = 4, _origPathOffset, _frameCount = 1, _specLights, _loop, _lightset, _amblights, _lights, _fog, _camera;

        [Category("Scene Data")]
        public int Version { get { return _version; } set { _version = value; SignalPropertyChange(); } }
        [Category("Scene Data")]
        public override int FrameCount 
        {
            get { return _frameCount; }
            set 
            {
                _frameCount = value;
                SCN0GroupNode grp = GetFolder<SCN0LightNode>();
                if (grp != null)
                    foreach (SCN0LightNode l in grp.Children)
                        l.FrameCount = _frameCount;
                grp = GetFolder<SCN0FogNode>();
                if (grp != null)
                    foreach (SCN0FogNode l in grp.Children)
                        l.FrameCount = _frameCount;
                grp = GetFolder<SCN0AmbientLightNode>();
                if (grp != null)
                    foreach (SCN0AmbientLightNode l in grp.Children)
                        l.FrameCount = _frameCount;
                SignalPropertyChange(); 
            }
        }
        //[Category("Scene Data")]
        //public int SpecularLightCount { get { return _specLights; } set { _specLights = value; SignalPropertyChange(); } }
        [Category("Scene Data")]
        public override bool Loop { get { return _loop != 0; } set { _loop = value ? 1 : 0; SignalPropertyChange(); } }

        [Category("User Data"), TypeConverter(typeof(ExpandableObjectCustomConverter))]
        public UserDataCollection UserEntries { get { return _userEntries; } set { _userEntries = value; SignalPropertyChange(); } }
        internal UserDataCollection _userEntries = new UserDataCollection();

        public override bool OnInitialize()
        {
            base.OnInitialize();
            //strings.Clear();
            _version = Header->_version;
            if (_version == 5)
            {
                if ((_name == null) && (Header5->_stringOffset != 0))
                    _name = Header5->ResourceString;

                _origPathOffset = Header5->_origPathOffset;
                _frameCount = Header5->_frameCount;
                _specLights = Header5->_specLightCount;
                _loop = Header5->_loop;
                _lightset = Header5->_lightSetCount;
                _amblights = Header5->_ambientCount;
                _lights = Header5->_lightCount;
                _fog = Header5->_fogCount;
                _camera = Header5->_cameraCount;

                (_userEntries = new UserDataCollection()).Read(Header5->UserData);

                return Header5->Group->_numEntries > 0;
            }
            else
            {
                if ((_name == null) && (Header4->_stringOffset != 0))
                    _name = Header4->ResourceString;

                _origPathOffset = Header4->_origPathOffset;
                _frameCount = Header4->_frameCount;
                _specLights = Header4->_specLightCount;
                _loop = Header4->_loop;
                _lightset = Header4->_lightSetCount;
                _amblights = Header4->_ambientCount;
                _lights = Header4->_lightCount;
                _fog = Header4->_fogCount;
                _camera = Header4->_cameraCount;

                return Header4->Group->_numEntries > 0;
            }
        }

        public override void OnPopulate()
        {
            if (Header->_version == 5)
            {
                ResourceGroup* group = Header5->Group;
                SCN0GroupNode g;
                for (int i = 0; i < group->_numEntries; i++)
                {
                    string name = group->First[i].GetName();
                    (g = new SCN0GroupNode(name)).Initialize(this, new DataSource(group->First[i].DataAddress, 0));
                    if (name == "LightSet(NW4R)")
                        for (int x = 0; x < Header5->_lightSetCount; x++)
                            new SCN0LightSetNode().Initialize(g, new DataSource(&Header5->LightSets[x], SCN0LightSet.Size));
                    else if (name == "AmbLights(NW4R)")
                        for (int x = 0; x < Header5->_ambientCount; x++)
                            new SCN0AmbientLightNode().Initialize(g, new DataSource(&Header5->AmbientLights[x], SCN0AmbientLight.Size));
                    else if (name == "Lights(NW4R)")
                        for (int x = 0; x < Header5->_lightCount; x++)
                            new SCN0LightNode().Initialize(g, new DataSource(&Header5->Lights[x], SCN0Light.Size));
                    else if (name == "Fogs(NW4R)")
                        for (int x = 0; x < Header5->_fogCount; x++)
                            new SCN0FogNode().Initialize(g, new DataSource(&Header5->Fogs[x], SCN0Fog.Size));
                    else if (name == "Cameras(NW4R)")
                        for (int x = 0; x < Header5->_cameraCount; x++)
                            new SCN0CameraNode().Initialize(g, new DataSource(&Header5->Cameras[x], SCN0Camera.Size));
                }
            }
            else
            {
                ResourceGroup* group = Header4->Group;
                SCN0GroupNode g, lightsets = null;
                for (int i = 0; i < group->_numEntries; i++)
                {
                    string name = group->First[i].GetName();
                    (g = new SCN0GroupNode(name)).Initialize(this, new DataSource(group->First[i].DataAddress, 0));
                    if (name == "LightSet(NW4R)")
                        for (int x = 0; x < Header4->_lightSetCount; x++)
                            new SCN0LightSetNode().Initialize(lightsets = g, new DataSource(&Header4->LightSets[x], SCN0LightSet.Size));
                    else if (name == "AmbLights(NW4R)")
                        for (int x = 0; x < Header4->_ambientCount; x++)
                            new SCN0AmbientLightNode().Initialize(g, new DataSource(&Header4->AmbientLights[x], SCN0AmbientLight.Size));
                    else if (name == "Lights(NW4R)")
                        for (int x = 0; x < Header4->_lightCount; x++)
                            new SCN0LightNode().Initialize(g, new DataSource(&Header4->Lights[x], SCN0Light.Size));
                    else if (name == "Fogs(NW4R)")
                        for (int x = 0; x < Header4->_fogCount; x++)
                            new SCN0FogNode().Initialize(g, new DataSource(&Header4->Fogs[x], SCN0Fog.Size));
                    else if (name == "Cameras(NW4R)")
                        for (int x = 0; x < Header4->_cameraCount; x++)
                            new SCN0CameraNode().Initialize(g, new DataSource(&Header4->Cameras[x], SCN0Camera.Size));
                }
                foreach (SCN0LightSetNode t in lightsets.Children)
                    t.AttachNodes();
            }

            //for (int i = 0; i < strings.Count; i++)
            //    Console.WriteLine(strings.Keys.ElementAt(i) + " " + strings.Values.ElementAt(i));
        }

        public SCN0GroupNode GetOrCreateFolder<T>() where T : SCN0EntryNode
        {
            string groupName;
            if (typeof(T) == typeof(SCN0LightSetNode))
                groupName = "LightSet(NW4R)";
            else if (typeof(T) == typeof(SCN0AmbientLightNode))
                groupName = "AmbLights(NW4R)";
            else if (typeof(T) == typeof(SCN0LightNode))
                groupName = "Lights(NW4R)";
            else if (typeof(T) == typeof(SCN0FogNode))
                groupName = "Fogs(NW4R)";
            else if (typeof(T) == typeof(SCN0CameraNode))
                groupName = "Cameras(NW4R)";
            else
                return null;

            SCN0GroupNode group = null;
            foreach (SCN0GroupNode node in Children)
                if (node.Name == groupName) { group = node; break; }

            if (group == null)
                AddChild(group = new SCN0GroupNode(groupName));

            return group;
        }

        public SCN0GroupNode GetFolder<T>() where T : SCN0EntryNode
        {
            string groupName;
            if (typeof(T) == typeof(SCN0LightSetNode))
                groupName = "LightSet(NW4R)";
            else if (typeof(T) == typeof(SCN0AmbientLightNode))
                groupName = "AmbLights(NW4R)";
            else if (typeof(T) == typeof(SCN0LightNode))
                groupName = "Lights(NW4R)";
            else if (typeof(T) == typeof(SCN0FogNode))
                groupName = "Fogs(NW4R)";
            else if (typeof(T) == typeof(SCN0CameraNode))
                groupName = "Cameras(NW4R)";
            else
                return null;

            SCN0GroupNode group = null;
            foreach (SCN0GroupNode node in Children)
                if (node.Name == groupName) { group = node; break; }

            return group;
        }

        internal override void GetStrings(StringTable table)
        {
            table.Add(Name);
            foreach (SCN0GroupNode n in Children)
                n.GetStrings(table);

            foreach (UserDataClass s in _userEntries)
            {
                table.Add(s._name);
                if (s._type == UserValueType.String && s._entries.Count > 0)
                    table.Add(s._entries[0]);
            }
        }

        public override int OnCalculateSize(bool force)
        {
            _specLights = 0;
            int size = SCN0v4.Size + 0x18 + Children.Count * 0x10;
            foreach (SCN0GroupNode n in Children)
                size += n.CalculateSize(true);
            size += _userEntries.GetSize();
            return size;
        }

        internal VoidPtr _header;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            int GroupLen = 0, LightSetLen = 0, AmbLightSetLen = 0, LightLen = 0, FogLen = 0, CameraLen = 0;

            _header = address;

            ResourceGroup* group;
            if (_version == 5)
            {
                SCN0v5* header = (SCN0v5*)address;

                header->_origPathOffset = _origPathOffset;
                header->_frameCount = (short)_frameCount;
                header->_specLightCount = (short)_specLights;
                header->_loop = _loop;
                header->_pad = 0;
                header->_dataOffset = SCN0v5.Size;

                group = header->Group;
            }
            else
            {
                SCN0v4* header = (SCN0v4*)address;

                header->_origPathOffset = _origPathOffset;
                header->_frameCount = (short)_frameCount;
                header->_specLightCount = (short)_specLights;
                header->_loop = _loop;
                header->_pad = 0;
                header->_dataOffset = SCN0v4.Size;

                group = header->Group;
            }

            *group = new ResourceGroup(Children.Count);
            GroupLen = group->_totalSize;

            ResourceEntry* entry = group->First;
            VoidPtr groupAddress = group->EndAddress;
            VoidPtr entryAddress = groupAddress;

            foreach (SCN0GroupNode g in Children)
                entryAddress += g._groupLen;

            VoidPtr keyframeAddress = entryAddress;
            foreach (SCN0GroupNode g in Children)
                foreach (SCN0EntryNode e in g.Children)
                    keyframeAddress += e._length;

            VoidPtr lightArrayAddress = keyframeAddress;
            foreach (SCN0GroupNode g in Children)
                foreach (SCN0EntryNode e in g.Children)
                    lightArrayAddress += e._keyLen;

            VoidPtr visibilityAddress = lightArrayAddress;
            foreach (SCN0GroupNode g in Children)
                foreach (SCN0EntryNode e in g.Children)
                    visibilityAddress += e._lightLen;

            short _lightSetCount = 0, _ambientCount = 0, _lightCount = 0, _fogCount = 0, _cameraCount = 0;

            int[] indices = new int[] { -1, -1, -1, -1, -1 };
            foreach (SCN0GroupNode g in Children)
            {
                if (g._name == "LightSet(NW4R)")
                {
                    indices[0] = g.Index;
                    LightSetLen = g._entryLen;
                    _lightSetCount = (short)g.Children.Count;
                }
                else if (g._name == "AmbLights(NW4R)")
                {
                    indices[1] = g.Index;
                    AmbLightSetLen = g._entryLen;
                    _ambientCount = (short)g.Children.Count;
                }
                else if (g._name == "Lights(NW4R)")
                {
                    indices[2] = g.Index;
                    LightLen = g._entryLen;
                    _lightCount = (short)g.Children.Count;
                }
                else if (g._name == "Fogs(NW4R)")
                {
                    indices[3] = g.Index;
                    FogLen = g._entryLen;
                    _fogCount = (short)g.Children.Count;
                }
                else if (g._name == "Cameras(NW4R)")
                {
                    indices[4] = g.Index;
                    CameraLen = g._entryLen;
                    _cameraCount = (short)g.Children.Count;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                SCN0GroupNode g = indices[i] >= 0 ? Children[indices[i]] as SCN0GroupNode : null;
                if (g != null)
                {
                    (entry++)->_dataOffset = (int)groupAddress - (int)group;

                    g._dataAddr = entryAddress;
                    g.keyframeAddress = keyframeAddress;
                    g.lightArrayAddress = lightArrayAddress;
                    g.visibilityAddress = visibilityAddress;

                    g.Rebuild(groupAddress, g._groupLen, true);

                    groupAddress += g._groupLen;
                    GroupLen += g._groupLen;
                    entryAddress += g._entryLen;
                    keyframeAddress += g.keyLen;
                    lightArrayAddress += g.lightLen;
                    visibilityAddress += g.visLen;
                }
            }
            if (_version == 5)
            {
                SCN0v5* header = (SCN0v5*)address;
                header->_lightSetCount = _lightSetCount;
                header->_ambientCount = _ambientCount;
                header->_lightCount = _lightCount;
                header->_fogCount = _fogCount;
                header->_cameraCount = _cameraCount;
                header->Set(GroupLen, LightSetLen, AmbLightSetLen, LightLen, FogLen, CameraLen);

                if (_userEntries.Count > 0)
                    _userEntries.Write(header->UserData = lightArrayAddress);
            }
            else
            {
                SCN0v4* header = (SCN0v4*)address;
                header->_lightSetCount = _lightSetCount;
                header->_ambientCount = _ambientCount;
                header->_lightCount = _lightCount;
                header->_fogCount = _fogCount;
                header->_cameraCount = _cameraCount;
                header->Set(GroupLen, LightSetLen, AmbLightSetLen, LightLen, FogLen, CameraLen);
            }
        }

        protected internal override void PostProcess(VoidPtr bresAddress, VoidPtr dataAddress, int dataLength, StringTable stringTable)
        {
            base.PostProcess(bresAddress, dataAddress, dataLength, stringTable);

            SCN0v4* header = (SCN0v4*)dataAddress;
            header->ResourceStringAddress = stringTable[Name] + 4;

            ResourceGroup* group = header->Group;
            group->_first = new ResourceEntry(0xFFFF, 0, 0, 0, 0);

            ResourceEntry* rEntry = group->First;

            int index = 1;
            int[] indices = new int[] { -1, -1, -1, -1, -1 };
            foreach (SCN0GroupNode g in Children)
            {
                if (g._name == "LightSet(NW4R)")
                    indices[0] = g.Index;
                else if (g._name == "AmbLights(NW4R)")
                    indices[1] = g.Index;
                else if (g._name == "Lights(NW4R)")
                    indices[2] = g.Index;
                else if (g._name == "Fogs(NW4R)")
                    indices[3] = g.Index;
                else if (g._name == "Cameras(NW4R)")
                    indices[4] = g.Index;
            }

            for (int i = 0; i < 5; i++)
            {
                SCN0GroupNode n = indices[i] >= 0 ? Children[indices[i]] as SCN0GroupNode : null;
                if (n != null)
                {
                    dataAddress = (VoidPtr)group + (rEntry++)->_dataOffset;
                    ResourceEntry.Build(group, index++, dataAddress, (BRESString*)stringTable[n.Name]);
                    n.PostProcess(header, dataAddress, stringTable);
                }
            }

            if (_version == 5) 
                _userEntries.PostProcess(((SCN0v5*)dataAddress)->UserData, stringTable);
        }

        internal static ResourceNode TryParse(DataSource source) { return ((SCN0v4*)source.Address)->_header._tag == SCN0v4.Tag ? new SCN0Node() : null; }

        public T CreateResource<T>(string name) where T : SCN0EntryNode
        {
            SCN0GroupNode group = GetOrCreateFolder<T>();
            if (group == null)
                return null;

            T n = Activator.CreateInstance<T>();
            n.Name = group.FindName(name);
            group.AddChild(n);

            n._realIndex = n.Index;
            n._nodeIndex = group.UsedChildren.IndexOf(n);

            return n;
        }
    }
}