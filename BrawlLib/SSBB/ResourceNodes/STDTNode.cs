using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using BrawlLib.IO;
using BrawlLib.SSBBTypes;
using System.Drawing.Design;

namespace BrawlLib.SSBB.ResourceNodes {
	public unsafe class STDTNode : ARCEntryNode, IAttributeList {
        public override ResourceType ResourceType { get { return ResourceType.STDT; } }
		internal STDT* Header { get { return (STDT*)WorkingUncompressed.Address; } }
		internal int version, unk1, unk2;

		// Internal buffer for editing - changes written back to WorkingUncompressed on rebuild
		internal UnsafeBuffer entries;

		[Category("Stage Trap Data Table")]
		public int Unk1 { get { return unk1; } set { unk1 = value; SignalPropertyChange(); } }
		[Category("Stage Trap Data Table")]
		public int Unk2 { get { return unk2; } set { unk2 = value; SignalPropertyChange(); } }
		[Category("Stage Trap Data Table")]
		//[Editor(typeof(System.Windows.Forms.FourByteTypeEditor), typeof(UITypeEditor))]
		//public List<BUnion4> Entries { get { return entries; } set { entries = value; SignalPropertyChange(); } }

		public override bool OnInitialize() {
			_name = "STDT";
			version = Header->_version;
			unk1 = Header->_unk1;
			unk2 = Header->_unk2;

			entries = new UnsafeBuffer(WorkingUncompressed.Length - 0x14);
			Memory.Move(entries.Address, Header->Entries, (uint)entries.Length);
			return false;
		}

		public override void OnRebuild(VoidPtr address, int length, bool force) {
			STDT* header = (STDT*)address;
			header->_tag = STDT.Tag;
			header->_unk1 = unk1;
			header->_unk2 = unk2;
			header->_version = version;
			header->_entryOffset = 0x14;
			Memory.Move(Header->Entries, entries.Address, (uint)entries.Length);
		}

		public override int OnCalculateSize(bool force) {
			return WorkingUncompressed.Length;
		}

		internal static ResourceNode TryParse(DataSource source) { return ((STDT*)source.Address)->_tag == STDT.Tag ? new STDTNode() : null; }

		public VoidPtr AttributeAddress {
			get {
				return entries.Address;
			}
		}
		public int NumEntries {
			get {
				return entries.Length / 4;
			}
		}

		public void SetFloat(int index, float value) {
			if (((bfloat*)AttributeAddress)[index] != value) {
				((bfloat*)AttributeAddress)[index] = value;
				SignalPropertyChange();
			}
		}
		public float GetFloat(int index) {
			return ((bfloat*)AttributeAddress)[index];
		}
		public void SetInt(int index, int value) {
			if (((bint*)AttributeAddress)[index] != value) {
				((bint*)AttributeAddress)[index] = value;
				SignalPropertyChange();
			}
		}
		public int GetInt(int index) {
			return ((bint*)AttributeAddress)[index];
		}

		/// <summary>
		/// Creates an array of AttributeInfo objects to use with AttributeGrid,
		/// reading from the given text file if it exists.
		/// </summary>
		public AttributeInfo[] BuildAttributeArray(string loc) {
			AttributeInfo[] arr = new AttributeInfo[NumEntries];
			int index = 0x14;
			for (int i = 0; i < arr.Length; i++) {
				arr[i] = new AttributeInfo() {
					_name = "0x" + index.ToString("X3") + " ???",
					_description = "Unknown",
					_type = 0,
				};
				index += 4;
			}
			// txt override
			if (loc != null && File.Exists(loc)) {
				using (var sr = new StreamReader(loc)) {
					for (int i = 0; !sr.EndOfStream && i < 185; i++) {
						arr[i]._name = sr.ReadLine();
						arr[i]._description = sr.ReadLine();
						arr[i]._type = int.Parse(sr.ReadLine());

						if (arr[i]._description == "") arr[i]._description = "No Description Available.";

						sr.ReadLine();
					}
				}
			}
			return arr;
		}
	}
}
