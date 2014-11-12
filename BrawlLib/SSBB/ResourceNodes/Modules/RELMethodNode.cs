using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using BrawlLib.IO;
using System.Windows.Forms;
using System.PowerPcAssembly;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RELMethodNode : ModuleDataNode
    {
        internal buint* Header { get { return (buint*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RELMethod; } }

        [Browsable(false)]
        public RELObjectNode Object { get { return Parent.Parent as RELObjectNode; } }

        [Browsable(false)]
        public string FullName { get { return (Object != null ? Object.Type.FullName + "." : "") + _name; } }
        [Browsable(false)]
        public string FileOffset { get; set; }

        public string SectionOffset { get { return "0x" + ((int)Header - (int)this.Root.Children[TargetSectionID].WorkingUncompressed.Address).ToString("X"); } }

        public int TargetSectionID;
        
        public override bool OnInitialize()
        {
            //ModuleSectionNode section = Location;
            //if (section == null)
            //    return false;

            //uint relative = (uint)Header - (uint)section.WorkingUncompressed.Address;

            //int x = 0;
            //while (!((PowerPC.Disassemble(*&Header[x++])) is PPCblr)) ;


            //Relocation[] tmp1 = section._relocations.ToArray();
            //_relocations = new List<Relocation>(tmp1);
            //Relocation[] tmp2 = _relocations.ToArray();

            //Array.Copy(tmp1, relative / 4, tmp2, 0, x);
            //_relocations = tmp2.ToList();

            //InitBuffer((uint)x * 4, Header);

            //byte* pOut = (byte*)_dataBuffer.Address;
            //byte* pIn = (byte*)section._dataBuffer.Address + relative;
            //for (int i = 0; i < _dataBuffer.Length; i++)
            //    *pOut++ = *pIn++;

            //if (_relocations != null && _relocations.Count != 0)
            //{
            //    _relocations[0].Tags.Add(FullName + " Start");
            //    _relocations[_relocations.Count - 1].Tags.Add(FullName + " End");
            //}

            return false;
        }

        public override unsafe void Export(string outPath)
        {
        //    using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
        //    {
        //        stream.SetLength(WorkingUncompressed.Length);
        //        using (FileMap map = FileMap.FromStream(stream))
        //        {
        //            VoidPtr addr = Header;

        //            byte* pIn = (byte*)addr;
        //            byte* pOut = (byte*)map.Address;
        //            for (int i = 0; i < _dataBuffer.Length; i++)
        //                *pOut++ = *pIn++;
        //        }
        //    }
        }
    }

    public unsafe class RELExternalMethodNode : ModuleDataNode
    {
        internal buint* Header { get { return (buint*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RELExternalMethod; } }

        public Relocation _rel;

        [Category("External Method")]
        public string TargetModule { get { return RELNode._idNames.ContainsKey((int)_rel.Command._moduleID) ? RELNode._idNames[(int)_rel.Command._moduleID] : ""; } }
    
        [Category("External Method")]
        public string TargetOffset { get { return _rel.Command.TargetOffset; } }
    }
}
