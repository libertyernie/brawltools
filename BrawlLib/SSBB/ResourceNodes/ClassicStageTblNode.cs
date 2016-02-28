using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BrawlLib.IO;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class ClassicStageTblNode : ResourceNode, IBufferNode
    {
        internal byte* Header { get { return (byte*)WorkingUncompressed.Address; } }

        public UnsafeBuffer _buffer;

        private string GetStageID(int index)
        {
            bint* ptr = (bint*)(_buffer.Address) + 1 + index;
            List<int> stageIDs = new List<int>();
            while (ptr < _buffer.Address + _buffer.Length)
            {
                stageIDs.Add(*ptr);
                ptr += 0x41;
            }
            return string.Join(",", stageIDs);
        }

        public int Size { get { return WorkingUncompressed.Length; } }

        public double SectionsOf260Bytes { get { return Size / 260; } }
        public double ExtraBytes { get { return Size % 260; } }

        public string StageIDs1 { get { return GetStageID(0); } }
        public string StageIDs2 { get { return GetStageID(1); } }
        public string StageIDs3 { get { return GetStageID(2); } }
        public string StageIDs4 { get { return GetStageID(3); } }

        public ClassicStageTblNode() { }
        public ClassicStageTblNode(string name) { _name = name; }

        public override bool OnInitialize()
        {
            _buffer = new UnsafeBuffer(WorkingUncompressed.Length);

            Memory.Move(_buffer.Address, (VoidPtr)Header, (uint)_buffer.Length);

            return false;
        }

        public override int OnCalculateSize(bool force)
        {
            return _buffer.Length;
        }

        public override unsafe void Export(string outPath)
        {
            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.SequentialScan))
            {
                stream.SetLength(_buffer.Length);
                using (FileMap map = FileMap.FromStream(stream))
                    Memory.Move(map.Address, _buffer.Address, (uint)_buffer.Length);
            }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            VoidPtr header = (VoidPtr)address;
            Memory.Move(header, _buffer.Address, (uint)length);
        }

        public VoidPtr GetAddress()
        {
            return _buffer.Address;
        }

        public int GetLength()
        {
            return _buffer.Length;
        }

        public bool IsValid()
        {
            return _buffer != null && _buffer.Length > 0;
        }
    }
}
