using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BrawlLib.IO;

namespace BrawlLib.SSBB.ResourceNodes
{
    unsafe class RawDataNode : ResourceNode
    {
        internal byte* Header { get { return (byte*)WorkingUncompressed.Address; } }
        internal byte[] data;

        public int Size { get { return WorkingUncompressed.Length; } }

        public RawDataNode(string name) { _name = name; }

        public override bool OnInitialize()
        {
            data = new byte[WorkingUncompressed.Length];

            for (int i = 0; i < data.Length; i++)
                data[i] = Header[i];

            return false;
        }

        public override int OnCalculateSize(bool force)
        {
            return data.Length;
        }

        //public override unsafe void Export(string outPath)
        //{
        //    using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.SequentialScan))
        //    {
        //        stream.SetLength(data.Length);
        //        using (FileMap map = FileMap.FromStream(stream))
        //        {
        //            for (int i = 0; i < data.Length; i++)
        //                ((byte*)map.Address)[i] = data[i];
        //        }
        //    }

        //}

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            byte* header = (byte*)address;
            for (int i = 0; i < data.Length; i++)
                header[i] = data[i];
        }
    }
}
