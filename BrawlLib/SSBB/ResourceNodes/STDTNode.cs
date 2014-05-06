using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using BrawlLib.IO;
using BrawlLib.SSBBTypes;
using System.Drawing.Design;

namespace BrawlLib.SSBB.ResourceNodes {
	public unsafe class STDTNode : ARCEntryNode {
        public override ResourceType ResourceType { get { return ResourceType.STDT; } }
		internal STDT* Header { get { return (STDT*)WorkingUncompressed.Address; } }
		internal int version, unk1, unk2;
		internal List<BUnion4> entries = new List<BUnion4>();

		[Category("Stage Trap Data Table")]
		public int Unk1 { get { return unk1; } set { unk1 = value; SignalPropertyChange(); } }
		[Category("Stage Trap Data Table")]
		public int Unk2 { get { return unk2; } set { unk2 = value; SignalPropertyChange(); } }
		[Category("Stage Trap Data Table")]
		//[Editor(typeof(System.Windows.Forms.FourByteTypeEditor), typeof(UITypeEditor))]
		public List<BUnion4> Entries { get { return entries; } set { entries = value; SignalPropertyChange(); } }

		public override bool OnInitialize() {
			_name = "STDT";
			version = Header->_version;
			unk1 = Header->_unk1;
			unk2 = Header->_unk2;
			bfloat* Entries = Header->Entries;
			for (int i = 0; i * 0x4 + 0x14 < WorkingUncompressed.Length; i++) {
				entries.Add(new BUnion4(*Entries, this)); Entries++;
			}
			return false;
		}

		public override void OnRebuild(VoidPtr address, int length, bool force) {
			STDT* header = (STDT*)address;
			header->_tag = STDT.Tag;
			header->_unk1 = unk1;
			header->_unk2 = unk2;
			header->_version = version;
			header->_entryOffset = 0x14;
			bfloat* Entry = header->Entries;
			for (int i = 0; i < entries.Count; i++) { Entry[i] = entries[i].v; }
		}

		public override int OnCalculateSize(bool force) {
			return 0x14 + entries.Count * 0x4;
		}

		internal static ResourceNode TryParse(DataSource source) { return ((STDT*)source.Address)->_tag == STDT.Tag ? new STDTNode() : null; }

	}

	public class BUnion4 {
		public bfloat v;
		private STDTNode parent;

		public BUnion4(bfloat v, STDTNode parent) {
			this.v = v;
			this.parent = parent;
		}

		public string Float {
			get {
				return v.ToString();
			}
			set {
				v = Single.Parse(value);
				parent.SignalPropertyChange();
			}
		}
		public unsafe string Int {
			get {
				fixed (bfloat* ptr = &v) return ((bint*)ptr)->ToString();
			}
			set {
				fixed (bfloat* ptr = &v) *((bint*)ptr) = Int32.Parse(value);
				parent.SignalPropertyChange();
			}
		}
		public unsafe string Bytes {
			get {
				fixed (bfloat* ptr = &v) return ((buint*)ptr)->Value.ToString("X8");
			}
		}
	}
}
