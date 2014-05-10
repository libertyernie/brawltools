using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.Runtime.InteropServices;
using BrawlLib.Imaging;
using BrawlLib.Wii;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class GDORNode : ResourceNode
    {
        internal GDOR* Header { get { return (GDOR*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.GDOR; } }
        [Category("GDOR")]
        public int Doors { get { return Header->_count; } }

        public override void OnPopulate()
        {
            for (int i = 0; i < Header->_count; i++)
            {
                DataSource source;
                if (i == Header->_count - 1)
                { source = new DataSource((*Header)[i], WorkingUncompressed.Address + WorkingUncompressed.Length - (*Header)[i]); }
                else { source = new DataSource((*Header)[i], (*Header)[i + 1] - (*Header)[i]); }
                new GDOREntryNode().Initialize(this, source);

            }
        }
        public override bool OnInitialize()
        {
            base.OnInitialize();
            if (_name == null)
                _name = "GDOR";
            return Header->_count > 0;
        }

        internal static ResourceNode TryParse(DataSource source) { return ((GDOR*)source.Address)->_tag == GDOR.Tag ? new GDORNode() : null; }
    }
    public unsafe class GDOREntryNode : ResourceNode
    {
        internal GDOREntry* Header { get { return (GDOREntry*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }
        
        [Category("Door Info")]
        [DisplayName("Stage ID (decimal)")]
        public string FileID { get { return getStage(); } }

        [Category("Door Info")]
        [DisplayName("Door Index")]
        public string DoorID { get { int i = *(byte*)(Header + 0x33); return i.ToString("x"); } }

        public string getStage()
        {
            string s1="";
            for (int i = 0; i < 3; i++)
            {
                int i1 = *(byte*)(Header + 0x30+i);
                if (i1 < 10) { s1 += i1.ToString("x").PadLeft(2, '0'); } else { s1 = i1.ToString("x"); }
            }
            return s1;
        }

        public override bool OnInitialize()
        {
            base.OnInitialize();
            getStage();
            if (_name == null)
                _name = "Door["+Index+']';

            return false;
        }
    }
}
