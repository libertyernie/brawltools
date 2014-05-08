using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using BrawlLib.IO;
using BrawlLib.SSBBTypes;
using System.Drawing.Design;
using System.Linq;

namespace BrawlLib.SSBB.ResourceNodes {
    public unsafe class STDTNode : ARCEntryNode, IAttributeList {
        public override ResourceType ResourceType { get { return ResourceType.STDT; } }
        internal STDT* Header { get { return (STDT*)WorkingUncompressed.Address; } }
        internal int version, unk1, unk2;

        // Internal buffer for editing - changes written back to WorkingUncompressed on rebuild
        internal UnsafeBuffer entries;

        [Category("Stage Trap Data Table")]
        public int NumEntries{get{return entries.Length / 4;}}
        [Category("Stage Trap Data Table")]
        public int Unk1 { get { return unk1; } set { unk1 = value; SignalPropertyChange(); } }
        [Category("Stage Trap Data Table")]
        public int Unk2 { get { return unk2; } set { unk2 = value; SignalPropertyChange(); } }

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
        [Browsable(false)]
        public VoidPtr AttributeAddress {
            get {
                return entries.Address;
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

		public IEnumerable<AttributeInfo[]> GetPossibleInterpretations() {
			var q= from f in STDTFormat.Formats
				   where f.Size == WorkingUncompressed.Length
				   select f.AttributeArray;
			Console.WriteLine(q.Count());
			return q;
		}

        /*/// <summary>
        /// Creates an array of AttributeInfo objects to use with AttributeGrid,
        /// reading from the given text file if it exists. If the file is null,
        /// default names will be used and the program will guess if values are
        /// ints or floats.
        /// </summary>
        public void BuildAttributeArray(string rootName) {
            var formats = from f in STDTFormat.Formats
                          where f.Size == WorkingUncompressed.Length
                          select f;
            if (formats.Any()) {
                _attributeArray = formats.First().AttributeArray;
            } else {
                AttributeInfo[] arr = new AttributeInfo[NumEntries];
                buint* pIn = (buint*)AttributeAddress;
                int index = 0x14;
                for (int i = 0; i < arr.Length; i++) {
                    arr[i] = new AttributeInfo() {
                        _name = "0x" + index.ToString("X3")
                    };
                    //Guess if the value is a an integer or float
                    if (*pIn == 0) {
                        arr[i]._type = 0;
                        arr[i]._description = "Unknown (value of 0 could be either - guessed float)";
                    } else if (((((uint)*((buint*)pIn)) >> 24) & 0xFF) != 0 && *((bint*)pIn) != -1 && !float.IsNaN(((float)*((bfloat*)pIn)))) {
                        float abs = Math.Abs((float)*((bfloat*)pIn));
                        if (abs > 0.0000001 && abs < 10000000) {
                            arr[i]._type = 0;
                            arr[i]._description = "Unknown (probably float)";
                        } else {
                            arr[i]._type = 0;
                            arr[i]._description = "Unknown";
                        }
                    } else {
                        arr[i]._type = 1;
                        arr[i]._description = "Unknown (probably int)";
                        arr[i]._name = "*" + arr[i]._name;
                    }
                    index += 4;
                    pIn++;
                }
                _attributeArray = arr;
            }
        }*/
    }

    public class STDTFormat {
        public STDTFormat(string name, int size) {
            StageBase = name;
            Size = size;
        }

        public string StageBase { get; private set; }
        public int Size { get; private set; }

        public int NumEntries {
            get {
                return (Size - 0x14) / 4;
            }
        }

        private AttributeInfo[] _attributeArray;
        public AttributeInfo[] AttributeArray {
            get {
                if (_attributeArray == null) BuildAttributeArray("STDT/" + StageBase + ".txt");
                return _attributeArray;
            }
        }

        private void BuildAttributeArray(string filename) {
			List<AttributeInfo> list = new List<AttributeInfo>();
			int index = 0x14;
            if (filename != null && File.Exists(filename)) {
                using (var sr = new StreamReader(filename)) {
                    for (int i = 0; !sr.EndOfStream && i < NumEntries; i++) {
                        AttributeInfo attr = new AttributeInfo();
                        attr._name = sr.ReadLine();
                        attr._description = sr.ReadLine();
                        attr._type = int.Parse(sr.ReadLine());

                        if (attr._description == "") attr._description = "No Description Available.";

                        list.Add(attr);
                        sr.ReadLine();
                        index++;
                    }
                }
			}
			while (list.Count < NumEntries) {
				list.Add(new AttributeInfo() {
					_name = "0x" + index.ToString("X3"),
					_description = "Unknown",
					_type = 0,
				});
			}
            _attributeArray = list.ToArray();
        }

        public override string ToString() {
            return "STDT format: " + StageBase + " (" + Size + " bytes)";
        }

        public static readonly STDTFormat[] Formats = {
            new STDTFormat("DXBIGBLUE", 344),
            new STDTFormat("TENGAN", 300),
            new STDTFormat("PLANKTON", 264),
            new STDTFormat("EARTH", 240),
            new STDTFormat("ICE", 236),
            new STDTFormat("HALBERD", 196),
            new STDTFormat("METALGEAR", 180),
            new STDTFormat("PIRATES", 172),
            new STDTFormat("GW", 160),
            new STDTFormat("PALUTENA", 152),
            new STDTFormat("DXGREENS", 140),
            new STDTFormat("JUNGLE", 132),
            new STDTFormat("STADIUM", 132),
            new STDTFormat("DONKEY", 128),
            new STDTFormat("MANSION", 128),
            new STDTFormat("NORFAIR", 128),
            new STDTFormat("FAMICOM", 120),
            new STDTFormat("FZERO", 112),
            new STDTFormat("GREENHILL", 100),
            new STDTFormat("DXONETT", 96),
            new STDTFormat("KART", 96),
            new STDTFormat("VILLAGE", 92),
            new STDTFormat("STARFOX", 76), // STGSTARFOX_GDIFF.PAC
            new STDTFormat("CRAYON", 60),
            new STDTFormat("MARIOPAST", 60),
            new STDTFormat("DOLPIC", 56),
            new STDTFormat("DXGARDEN", 48),
            new STDTFormat("PICTCHAT", 40),
            new STDTFormat("HOMERUN_en", 28), // two values
            new STDTFormat("HOMERUN", 20), // empty file??
        };
    }
}
