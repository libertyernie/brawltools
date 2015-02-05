using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Collections.Generic;
using BrawlLib.Wii.Compression;
using System.IO;
using System.Drawing;
using BrawlLib.IO;
using System.Windows.Forms;
using BrawlLib.Wii.Textures;
using Gif.Components;
using System.Linq;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class BFRESNode : SARCEntryNode
    {
        internal BFRESHeader* Header { get { return (BFRESHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.BFRES; } }
        public override Type[] AllowedChildTypes { get { return new Type[] { typeof(BFRESGroupNode) }; } }

        public byte _versionMajor, _versionMinor;
        public ushort _flags;

        [Category("Wii U Resource Package")]
        public byte VersionMajor { get { return _versionMajor; } set { _versionMinor = value; SignalPropertyChange(); } }
        [Category("Wii U Resource Package")]
        public byte VersionMinor { get { return _versionMinor; } set { _versionMinor = value; SignalPropertyChange(); } }
        [Category("Wii U Resource Package")]
        public ushort Flags { get { return _flags; } set { _flags = value; SignalPropertyChange(); } }

        public override void OnPopulate()
        {
            buint* offset = Header->Offsets;
            bushort* count = Header->Counts;
            for (int x = 0; x < 12; x++, offset++, count++)
                if (*offset > 0 && *count > 0)
                    new BFRESGroupNode((BFRESGroupNode.BFRESGroupType)x).Initialize(this, offset->OffsetAddress, 0);
        }
        public override bool OnInitialize()
        {
            if (_name == null && !String.IsNullOrEmpty(_origPath))
                _name = Path.GetFileNameWithoutExtension(_origPath);

            _versionMajor = Header->_versionMajor;
            _versionMinor = Header->_versionMinor;
            _flags = Header->_flags;

            bool ok = false;
            for (int i = 0; i < 12; i++)
                if (Header->Counts[i] > 0 && Header->Offsets[i] > 0)
                {
                    ok = true;
                    break;
                }

            return ok;
        }

        public BFRESGroupNode GetOrCreateFolder<T>() where T : BFRESEntryNode
        {
            BFRESGroupNode.BFRESGroupType type;
            if (typeof(T) == typeof(FMDLNode))
            {
                type = BFRESGroupNode.BFRESGroupType.Models;
            }
            else if (typeof(T) == typeof(FTEXNode))
            {
                type = BFRESGroupNode.BFRESGroupType.Textures;
            }
            //else if (typeof(T) == typeof(FSKANode))
            //
            //    type = BFRESGroupNode.BFRESGroupType.SKA;
            //
            //else if (typeof(T) == typeof(FSHU1Node))
            //
            //    type = BFRESGroupNode.BFRESGroupType.SHU1;
            //
            //else if (typeof(T) == typeof(FSHU2Node))
            //
            //    type = BFRESGroupNode.BFRESGroupType.SHU2;
            //
            //else if (typeof(T) == typeof(FSHU3Node))
            //
            //    type = BFRESGroupNode.BFRESGroupType.SHU3;
            //
            //else if (typeof(T) == typeof(FTXPNode))
            //
            //    type = BFRESGroupNode.BFRESGroupType.TXP;
            //
            //else if (typeof(T) == typeof(UNK8Node))
            //
            //    type = BFRESGroupNode.BFRESGroupType.UNK8;
            //
            //else if (typeof(T) == typeof(FVISNode))
            //
            //    type = BFRESGroupNode.BFRESGroupType.VIS;
            //
            //else if (typeof(T) == typeof(FSHANode))
            //
            //    type = BFRESGroupNode.BFRESGroupType.SHA;
            //
            //else if (typeof(T) == typeof(FSCNNode))
            //
            //    type = BFRESGroupNode.BFRESGroupType.Scenes;
            //
            //else if (typeof(T) == typeof(EmbeddedNode))
            //
            //    type = BFRESGroupNode.BFRESGroupType.Embedded;
            //
            else
                return null;

            BFRESGroupNode group = null;
            foreach (BFRESGroupNode node in Children)
                if (node.Type == type)
                { group = node; break; }

            if (group == null)
                AddChild(group = new BFRESGroupNode(type));

            return group;
        }
        public T CreateResource<T>(string name) where T : BFRESEntryNode
        {
            BFRESGroupNode group = GetOrCreateFolder<T>();
            if (group == null)
                return null;

            T n = Activator.CreateInstance<T>();
            n.Name = group.FindName(name);
            group.AddChild(n);

            return n;
        }

        public void ExportToFolder(string outFolder) { ExportToFolder(outFolder, ".tex0"); }
        public void ExportToFolder(string outFolder, string imageExtension)
        {
            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            string ext = "";
            foreach (BFRESGroupNode group in Children)
            {
                if (group.Type == BFRESGroupNode.BFRESGroupType.Textures)
                    ext = imageExtension;
                else if (group.Type == BFRESGroupNode.BFRESGroupType.Models)
                    ext = ".fmdl";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.SKA)
                    ext = ".fska";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.TXP)
                    ext = ".ftxp";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.VIS)
                    ext = ".fvis";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.UNK8)
                    ext = ".*";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.SHU1)
                    ext = ".fshu";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.SHU2)
                    ext = ".fshu";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.SHU3)
                    ext = ".fshu";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.Embedded)
                    ext = ".*";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.SHA)
                    ext = ".fsha";
                else if (group.Type == BFRESGroupNode.BFRESGroupType.Scenes)
                    ext = ".fscn";
                foreach (BFRESGroupNode entry in group.Children)
                    entry.Export(Path.Combine(outFolder, entry.Name + ext));
            }
        }

        internal static ResourceNode TryParse(DataSource source) { return ((BFRESHeader*)source.Address)->_tag == BFRESHeader.Tag ? new BFRESNode() : null; }
    }

    public unsafe class BFRESGroupNode : ResourceNode
    {
        internal ResourceGroup* Group { get { return (ResourceGroup*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.BFRESGroup; } }

        [Browsable(false)]
        public BFRESGroupType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public BFRESGroupNode() : base() { }
        public BFRESGroupNode(BFRESGroupType type) : base() { _name = (Type = type).ToString(); }

        public enum BFRESGroupType
        {
            Models,
            Textures,
            SKA,
            SHU1,
            SHU2,
            SHU3,
            TXP,
            UNK8,
            VIS,
            SHA,
            Scenes,
            Embedded,
            None
        }

        public BFRESGroupType _type = BFRESGroupType.None;

        public override void RemoveChild(ResourceNode child)
        {
            if ((Children.Count == 1) && (Children.Contains(child)))
                Parent.RemoveChild(this);
            else
                base.RemoveChild(child);
        }

        public override bool OnInitialize()
        {
            if (_name == null)
                _name = Path.GetFileNameWithoutExtension(_origPath);
            return Group->_numEntries > 0;
        }

        public override void OnPopulate()
        {
            ResourceGroup* group = Group;
            for (int i = 0; i < group->_numEntries; i++)
            {
                VoidPtr data = group->First[i].DataAddressRelative;
                if (NodeFactory.FromAddress(this, data, 0) == null)
                    new BFRESEntryNode().Initialize(this, data, 0);
            }
        }
    }

    public unsafe class BFRESEntryNode : ResourceNode
    {
        [Browsable(false)]
        public BFRESNode BFRESNode { get { return ((_parent != null) && (Parent.Parent is BFRESNode)) ? Parent.Parent as BFRESNode : null; } }
        
        public override bool OnInitialize()
        {
            return false;
        }
    }
}
