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
		internal STDT* Header { get { return (STDT*)WorkingUncompressed.Address; } }
		internal int version, unk1, unk2;
		internal List<float> entries = new List<float>();

		[Category("Stage Trap Data Table")]
		public int Unk1 { get { return unk1; } set { unk1 = value; SignalPropertyChange(); } }
		[Category("Stage Trap Data Table")]
		public int Unk2 { get { return unk2; } set { unk2 = value; SignalPropertyChange(); } }
		[Category("Stage Trap Data Table")]
		[Editor(typeof(System.Windows.Forms.FourByteTypeEditor), typeof(UITypeEditor))]
		public List<float> Entries { get { return entries; } set { entries = value; SignalPropertyChange(); } }

		public override bool OnInitialize() {
			_name = "STDT";
			version = Header->_version;
			unk1 = Header->_unk1;
			unk2 = Header->_unk2;
			bfloat* Entries = Header->Entries;
			for (int i = 0; i * 0x4 + 0x14 < WorkingUncompressed.Length; i++) {
				entries.Add(*Entries); Entries++;
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
			for (int i = 0; i < entries.Count; i++) { Entry[i] = entries[i]; }
		}

		public override int OnCalculateSize(bool force) {
			return 0x14 + entries.Count * 0x4;
		}

		internal static ResourceNode TryParse(DataSource source) { return ((STDT*)source.Address)->_tag == STDT.Tag ? new STDTNode() : null; }

	}
}
