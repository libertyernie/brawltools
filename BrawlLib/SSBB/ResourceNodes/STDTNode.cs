using System;
using System.Collections.Generic;
using BrawlLib.Wii;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class STDTNode : ARCEntryNode
    {
		public override ResourceType ResourceType { get { return ResourceType.STDT; } }
		public Tuple<bfloat, buint>[] Values { get; private set; }

        public override bool OnInitialize()
        {
			base.OnInitialize();

			if (_name == null)
				_name = "STDT";

            byte* floor = (byte*)WorkingUncompressed.Address + 0x14;
            byte* ceiling = (byte*)WorkingUncompressed.Address + WorkingUncompressed.Length;

			List<Tuple<bfloat, buint>> values = new List<Tuple<bfloat, buint>>();
			for (bfloat* ptr = (bfloat*)floor; ptr != (bfloat*)ceiling; ptr++) {
				values.Add(new Tuple<bfloat, buint>(*ptr, *((buint*)ptr)));
			}
			this.Values = values.ToArray();
            return false;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Won't implement unless I add editing support
        }
		
        internal static ResourceNode TryParse(DataSource source) { return ((uint*)source.Address)[0] == 0x54445453 ? new STDTNode() : null; }
    }
}
