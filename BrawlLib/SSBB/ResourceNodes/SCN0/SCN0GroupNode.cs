using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Imaging;
using BrawlLib.Wii.Animations;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class SCN0GroupNode : ResourceNode
    {
        internal ResourceGroup* Group { get { return (ResourceGroup*)WorkingUncompressed.Address; } }

        public override ResourceType ResourceType { get { return ResourceType.MDL0Group; } }

        public SCN0GroupNode() : base() { }
        public SCN0GroupNode(string name) : base() { _name = name; }

        internal void GetStrings(StringTable table)
        {
            table.Add(Name);
            foreach (SCN0EntryNode n in Children)
                n.GetStrings(table);
        }

        public override void RemoveChild(ResourceNode child)
        {
            if ((_children != null) && (_children.Count == 1) && (_children.Contains(child)))
                _parent.RemoveChild(this);
            else
                base.RemoveChild(child);
        }

        public override bool OnInitialize()
        {
            return Group->_numEntries > 0;
        }

        public int _groupLen, _entryLen, keyLen, lightLen, visLen;
        public override int OnCalculateSize(bool force)
        {
            _groupLen = 0x18 + UsedChildren.Count * 0x10;
            _entryLen = 0;
            foreach (SCN0EntryNode n in Children)
            {
                _entryLen += n.CalculateSize(true);
                keyLen += n._keyLen;
                lightLen += n._lightLen;
                visLen += n._visLen;
            }
            return _entryLen + _groupLen + keyLen + lightLen + visLen;
        }
        public VoidPtr _dataAddr, keyframeAddress, lightArrayAddress, visibilityAddress;
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            ResourceGroup* group = (ResourceGroup*)address;
            *group = new ResourceGroup(UsedChildren.Count);

            int nodeIndex = 0;

            ResourceEntry* entry = group->First;
            foreach (SCN0EntryNode n in Children)
            {
                if (n.Name != "<null>")
                {
                    (entry++)->_dataOffset = (int)_dataAddr - (int)group;
                    n._nodeIndex = nodeIndex++;
                    n._realIndex = n.Index;
                }
                else
                    n._nodeIndex = n._realIndex = -1;

                n.keyframeAddr = keyframeAddress;
                n.lightAddr = (RGBAPixel*)lightArrayAddress;
                n.visAddr = visibilityAddress;

                n.Rebuild(_dataAddr, n._calcSize, true);

                _dataAddr += n._calcSize;
                keyframeAddress += n._keyLen;
                lightArrayAddress += n._lightLen;
                visibilityAddress += n._visLen;
            }
        }

        protected internal virtual void PostProcess(VoidPtr scn0Address, VoidPtr dataAddress, StringTable stringTable)
        {
            ResourceGroup* group = (ResourceGroup*)dataAddress;
            group->_first = new ResourceEntry(0xFFFF, 0, 0, 0, 0);

            ResourceEntry* rEntry = group->First;

            int index = 1;
            foreach (SCN0EntryNode n in UsedChildren)
            {
                dataAddress = (VoidPtr)group + (rEntry++)->_dataOffset;
                ResourceEntry.Build(group, index++, dataAddress, (BRESString*)stringTable[n.Name]);
                n.PostProcess(scn0Address, dataAddress, stringTable);
            }
        }

        [Browsable(false)]
        public List<ResourceNode> UsedChildren
        {
            get
            {
                List<ResourceNode> l = new List<ResourceNode>();
                foreach (SCN0EntryNode n in Children)
                    if (n.Name != "<null>")
                        l.Add(n);
                return l;
            }
        }
    }

    public unsafe class SCN0EntryNode : ResourceNode
    {
        internal SCN0CommonHeader* Header { get { return (SCN0CommonHeader*)WorkingUncompressed.Address; } }
        public override bool AllowNullNames { get { return true; } }

        public VoidPtr keyframeAddr, visAddr;
        public RGBAPixel* lightAddr;

        public int _keyLen, _lightLen, _visLen;

        public int _length, _scn0Offset, _stringOffset, _nodeIndex, _realIndex;

        [Category("SCN0 Entry"), Browsable(false)]
        public int Length 
        {
            get 
            {
                if (this is SCN0LightSetNode)
                    return SCN0LightSet.Size;
                else if (this is SCN0LightNode)
                    return SCN0Light.Size;
                else if (this is SCN0AmbientLightNode)
                    return SCN0AmbientLight.Size;
                else if (this is SCN0CameraNode)
                    return SCN0Camera.Size;
                else if (this is SCN0FogNode)
                    return SCN0Fog.Size;
                return 0;
            }
        }
        //[Category("SCN0 Entry")]
        //public int SCN0Offset { get { return _scn0Offset; } }
        [Category("SCN0 Entry")]
        public int NodeIndex { get { return ((SCN0GroupNode)Parent).UsedChildren.IndexOf(this); } }
        [Category("SCN0 Entry")]
        public int RealIndex { get { return Name != "<null>" ? Index : -1; } }

        internal virtual void GetStrings(StringTable table) { if (Name != "<null>") table.Add(Name); }

        public override bool OnInitialize()
        {
            if (!_replaced)
            if ((_name == null) && (Header->_stringOffset != 0))
                _name = Header->ResourceString;
            else
                _name = "<null>";

            SetSizeInternal(Header->_length);

            _length = Header->_length;
            _scn0Offset = Header->_scn0Offset;
            _stringOffset = Header->_stringOffset;
            _nodeIndex = Header->_nodeIndex;
            _realIndex = Header->_realIndex;

            return false;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            SCN0CommonHeader* header = (SCN0CommonHeader*)address;
            if (Name == "<null>")
                header->_nodeIndex = header->_realIndex = -1;
            header->_length = length;
            header->_scn0Offset = (int)((SCN0Node)Parent.Parent)._header - (int)address;
        }

        protected internal virtual void PostProcess(VoidPtr scn0Address, VoidPtr dataAddress, StringTable stringTable)
        {
            SCN0CommonHeader* header = (SCN0CommonHeader*)dataAddress;
            header->_scn0Offset = (int)scn0Address - (int)dataAddress;
            if (Name != "<null>")
            {
                header->ResourceStringAddress = stringTable[Name] + 4;
                header->_nodeIndex = ((SCN0GroupNode)Parent).UsedChildren.IndexOf(this);
                header->_realIndex = Index;
            }
            else
            {
                header->_nodeIndex = header->_realIndex = -1;
                header->_stringOffset = 0;
            }
        }

        public static void DecodeFrames(KeyframeArray kf, VoidPtr offset, int flags, int fixedBit)
        {
            if ((flags & fixedBit) != 0)
                kf[0] = *(bfloat*)offset;
            else
                DecodeFrames(kf, offset + *(bint*)offset);
        }
        public static void DecodeFrames(KeyframeArray kf, VoidPtr dataAddr)
        {
            SCN0KeyframesHeader* header = (SCN0KeyframesHeader*)dataAddr;
            SCN0KeyframeStruct* entry = header->Data;
            for (int i = 0; i < header->_numFrames; i++, entry++)
                kf.SetFrameValue((int)entry->_index, entry->_value)._tangent = entry->_tangent;
        }

        public static void EncodeFrames(KeyframeArray kf, ref VoidPtr dataAddr, VoidPtr offset, ref int flags, int fixedBit)
        {
            if (kf._keyCount > 1)
            {
                flags &= ~fixedBit;
                EncodeFrames(kf, ref dataAddr, offset);
            }
            else
            {
                flags |= fixedBit;
                *(bfloat*)offset = kf._keyRoot._next._value;
            }
        }
        public static void EncodeFrames(KeyframeArray kf, ref VoidPtr dataAddr, VoidPtr offset)
        {
            *(bint*)offset = (int)dataAddr - (int)offset;
            EncodeFrames(kf, ref dataAddr);
        }
        public static void EncodeFrames(KeyframeArray kf, ref VoidPtr dataAddr)
        {
            SCN0KeyframesHeader* header = (SCN0KeyframesHeader*)dataAddr;
            *header = new SCN0KeyframesHeader(kf._keyCount);
            KeyframeEntry frame, root = kf._keyRoot;

            SCN0KeyframeStruct* entry = header->Data;
            for (frame = root._next; frame._index != -1; frame = frame._next)
                *entry++ = new SCN0KeyframeStruct(frame._tangent, frame._index, frame._value);
            *(bint*)entry = 0;
            dataAddr = ((VoidPtr)entry) + 4;
        }
    }
}
