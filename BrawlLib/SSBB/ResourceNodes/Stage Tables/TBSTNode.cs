using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BrawlLib.SSBBTypes;
using System.Linq;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class TBSTNode : ARCEntryNode, IAttributeList
    {
        public override ResourceType ResourceType { get { return ResourceType.TBST; } }
        internal TBST* Header { get { return (TBST*)WorkingUncompressed.Address; } }
        internal int unk0, unk1, unk2;

        // Internal buffer for editing - changes written back to WorkingUncompressed on rebuild
        internal UnsafeBuffer entries;

        [Category("TBST")]
        public int NumEntries { get { return entries.Length / 4; } }
        [Category("TBST")]
        public int Unk0 { get { return unk0; } set { unk0 = value; SignalPropertyChange(); } }
        [Category("TBST")]
        public int Unk1 { get { return unk1; } set { unk1 = value; SignalPropertyChange(); } }
        [Category("TBST")]
        public int Unk2 { get { return unk2; } set { unk2 = value; SignalPropertyChange(); } }

        public TBSTNode() { }

        public TBSTNode(VoidPtr address, int numEntries)
        {
            unk0 = 19;              // Default set in Flat Zone 2
            unk1 = 0;
            unk2 = 0;
            entries = new UnsafeBuffer((numEntries * 4));
            if (address == null)
            {
                byte* pOut = (byte*)entries.Address;
                for (int i = 0; i < (numEntries * 4); i++)
                    *pOut++ = 0;
            }
            else
            {
                byte* pIn = (byte*)address;
                byte* pOut = (byte*)entries.Address;
                for (int i = 0; i < (numEntries * 4); i++)
                    *pOut++ = *pIn++;
            }
        }
        ~TBSTNode() { entries.Dispose(); }


        public override bool OnInitialize()
        {
            unk0 = Header->_unk0;
            unk1 = Header->_unk1;
            unk2 = Header->_unk2;
            entries = new UnsafeBuffer(WorkingUncompressed.Length - 0x10);
            Memory.Move(entries.Address, Header->Entries, (uint)entries.Length);
            return false;
        }

        protected override string GetName()
        {
            return base.GetName("TBST");
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            TBST* header = (TBST*)address;
            header->_tag = TBST.Tag;
            header->_unk0 = unk0;
            header->_unk1 = unk1;
            header->_unk2 = unk2;
            Memory.Move(header->Entries, entries.Address, (uint)entries.Length);
        }

        public override int OnCalculateSize(bool force)
        {
            return 0x10 + entries.Length;
        }

        internal static ResourceNode TryParse(DataSource source) { return ((TBST*)source.Address)->_tag == TBST.Tag ? new TBSTNode() : null; }
        [Browsable(false)]
        public VoidPtr AttributeAddress
        {
            get
            {
                return entries.Address;
            }
        }
        public void SetFloat(int index, float value)
        {
            if (((bfloat*)AttributeAddress)[index] != value)
            {
                ((bfloat*)AttributeAddress)[index] = value;
                SignalPropertyChange();
            }
        }
        public float GetFloat(int index)
        {
            return ((bfloat*)AttributeAddress)[index];
        }
        public void SetInt(int index, int value)
        {
            if (((bint*)AttributeAddress)[index] != value)
            {
                ((bint*)AttributeAddress)[index] = value;
                SignalPropertyChange();
            }
        }
        public int GetInt(int index)
        {
            return ((bint*)AttributeAddress)[index];
        }

        public IEnumerable<AttributeInterpretation> GetPossibleInterpretations()
        {
            ReadConfig();
            ResourceNode root = this;
            while (root.Parent != null) root = root.Parent;
            var q = from f in TBSTFormats
                    where 0x10 + f.NumEntries * 4 == WorkingUncompressed.Length
                    select f;

            bool any_match_name = q.Any(f => String.Equals(
                Path.GetFileNameWithoutExtension(f.Filename),
                root.Name.Replace("STG", ""),
                StringComparison.InvariantCultureIgnoreCase));
            if (!any_match_name) q = q.Concat(new AttributeInterpretation[] { GenerateDefaultInterpretation() });

            q = q.OrderBy(f => !String.Equals(
                Path.GetFileNameWithoutExtension(f.Filename),
                root.Name.Replace("STG", ""),
                StringComparison.InvariantCultureIgnoreCase));

            return q;
        }

        private AttributeInterpretation GenerateDefaultInterpretation()
        {
            AttributeInfo[] arr = new AttributeInfo[NumEntries];
            buint* pIn = (buint*)AttributeAddress;
            int index = 0x10;
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new AttributeInfo()
                {
                    _name = "0x" + index.ToString("X3")
                };
                //Guess if the value is a an integer or float
                uint u = (uint)*((buint*)pIn);
                float f = (float)*((bfloat*)pIn);
                if (*pIn == 0)
                {
                    arr[i]._type = 0;
                    arr[i]._description = "Default: 0 (could be int or float - be careful)";
                }
                else if (((u >> 24) & 0xFF) != 0 && *((bint*)pIn) != -1 && !float.IsNaN(f))
                {
                    float abs = Math.Abs(f);
                    if (abs > 0.0000001 && abs < 10000000)
                    {
                        arr[i]._type = 0;
                        arr[i]._description = "Default (float): " + f + " (" + u.ToString("X8") + ")";
                    }
                    else
                    {
                        arr[i]._type = 1;
                        arr[i]._description = "Default (unknown type): " + u + " (" + u.ToString("X8") + ")";
                        arr[i]._name = "~" + arr[i]._name;
                    }
                }
                else
                {
                    arr[i]._type = 1;
                    arr[i]._description = "Default (int): " + u + " (" + u.ToString("X8") + ")";
                    arr[i]._name = "*" + arr[i]._name;
                }
                index += 4;
                pIn++;
            }

            ResourceNode root = this;
            while (root.Parent != null) root = root.Parent;
            string filename = "TBST/" + root.Name.Replace("STG", "") + ".txt";
            return new AttributeInterpretation(arr, filename);
        }

        private static List<AttributeInterpretation> TBSTFormats = new List<AttributeInterpretation>();
        private static HashSet<string> configpaths_read = new HashSet<string>();

        private static void ReadConfig()
        {
            if (Directory.Exists("TBST"))
            {
                foreach (string path in Directory.EnumerateFiles("TBST", "*.txt"))
                {
                    if (configpaths_read.Contains(path)) continue;
                    configpaths_read.Add(path);
                    try
                    {
                        TBSTFormats.Add(new AttributeInterpretation(path));
                    }
                    catch (FormatException ex)
                    {
                        if (Properties.Settings.Default.HideMDL0Errors)
                        {
                            Console.Error.WriteLine(ex.Message);
                        }
                        else
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
        }
    }
}
