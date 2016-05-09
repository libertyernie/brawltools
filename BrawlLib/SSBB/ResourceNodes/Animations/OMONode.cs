using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using System.Windows.Forms;
using BrawlLib.Modeling;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class OMONode : ResourceNode
    {
        internal OMOHeader* Header { get { return (OMOHeader*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.OMO; } }
        public override Type[] AllowedChildTypes { get { return new Type[] { typeof(OMOEntryNode) }; } }

        public OMONode() { }
        const string _category = "Bone Animation";

        public int _frameCount;

        [Category(_category)]
        public int FrameCount
        {
            get { return _frameCount; }
            set { _frameCount = value; SignalPropertyChange(); }
        }

        public override bool OnInitialize()
        {
            return base.OnInitialize();
        }

        public override void OnPopulate()
        {
            base.OnPopulate();
        }
    }

    public unsafe class OMOEntryNode : ResourceNode
    {
        internal OMOBoneEntry* Header { get { return (OMOBoneEntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public uint _boneID;


        public override bool OnInitialize()
        {
            return base.OnInitialize();
        }
    }
}