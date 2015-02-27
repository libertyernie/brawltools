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
        public override ResourceType ResourceType
        {
            get
            {
                return WorkingUncompressed.Address ?
                    ResourceType.RELMethod :
                    ResourceType.RELExternalMethod;
            }
        }

        [Browsable(false)]
        public RELObjectNode Object { get { return Parent.Parent as RELObjectNode; } }

        [Browsable(false)]
        public string FullName { get { return (Object != null ? Object.Type.FullName + "." : "") + _name; } }

        public RelCommand _cmd;

        [Category("Method")]
        [DisplayName("Data Size")]
        [Description("The data length of the code in this method")]
        public new string DataSize { get { return base.DataSize; } }

        [Category("Method")]
        [DisplayName("Module ID")]
        [Description("Name of the target module which the assembly code for this method resides")]
        public string TargetModule { get { return RELNode._idNames.ContainsKey(_cmd._moduleID) ? RELNode._idNames[_cmd._moduleID] : ""; } }

        [Category("Method")]
        [DisplayName("Section Index")]
        [Description("The section in which this method's assembly code is located")]
        public uint TargetSection { get { return _cmd.TargetSectionID; } }

        [Category("Method")]
        [DisplayName("Section Offset")]
        [Description("Offset of the method's asssembly code within the target module, relative to the target section")]
        public string TargetOffset { get { return _cmd.TargetOffset; } }

        public List<PPCOpCode> _codes = new List<PPCOpCode>();

        public int _sectionOffset = 0;

        public override bool OnInitialize()
        {
            ModuleSectionNode section = Location;
            if (section == null || !((VoidPtr)Header))
                return false;

            buint* sPtr = Header;
            PPCOpCode code = null;
            do _codes.Add(code = PowerPC.Disassemble(*sPtr++));
            while (!(code is PPCBranch));

            InitBuffer((uint)sPtr - (uint)Header, Header);
            _manager.UseReference(section, _sectionOffset = (int)Header - (int)section.Header);

            if (_dataBuffer.Length > 0)
            {
                _manager.AddTag(0, FullName + " Start");
                _manager.AddTag(_dataBuffer.Length / 4 - 1, FullName + " End");
            }

            return false;
        }

        public override unsafe void Export(string outPath)
        {
            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(_dataBuffer.Length);
                using (FileMap map = FileMap.FromStream(stream))
                    Memory.Move(map.Address, _dataBuffer.Address, (uint)_dataBuffer.Length);
            }
        }
    }
}
