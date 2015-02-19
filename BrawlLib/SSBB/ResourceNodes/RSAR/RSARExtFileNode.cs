using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSARExtFileNode : RSARFileNode
    {
        internal INFOFileHeader* Header { get { return (INFOFileHeader*)WorkingUncompressed.Address; } }

        [Category("Data"), Browsable(true)]
        public override string DataOffset { get { return "0"; } }
        [Category("Data"), Browsable(true)]
        public override string InfoHeaderOffset { get { if (RSARNode != null && Header != null) return ((uint)(Header - (VoidPtr)RSARNode.Header)).ToString("X"); else return "0"; } }
        
        public override bool OnInitialize()
        {
            RSARNode parent = RSARNode;
            _extPath = Header->GetPath(&RSARNode.Header->INFOBlock->_collection);
            if (_name == null)
                _name = String.Format("[{0}] {1}", _fileIndex, _extPath);
            _extFileSize = Header->_headerLen;
            return false;
        }
    }
}
