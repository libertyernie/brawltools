using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBB.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ClassicStageBlock
    {
        public const int Size = 260;

        public bint _unknown00;
        public bint _stageID1;
        public bint _stageID2;
        public bint _stageID3;
        public bint _stageID4;
        public bint _unknown14;
        public bint _unknown18;
        public bint _unknown1c;
        public bint _unknown20;
        public bint _unknown24;
        public bint _unknown28;
        public bint _unknown2c;
        public bint _unknown30;
        public bint _unknown34;
        public bint _unknown38;
        public bint _unknown3c;
        public bint _unknown40;
        public bint _unknown44;
        public bint _unknown48;
        public bint _unknown4c;
        public bint _unknown50;
        public bint _unknown54;
        public bint _unknown58;
        public bint _unknown5c;
        public bint _unknown60;
        public bint _unknown64;
        public bint _unknown68;
        public bint _unknown6c;
        public bint _unknown70;
        public bint _unknown74;
        public bint _unknown78;
        public bint _unknown7c;
        public bint _unknown80;
        public bint _unknown84;
        public bint _unknown88;
        public bint _unknown8c;
        public bint _unknown90;
        public bint _unknown94;
        public bint _unknown98;
        public bint _unknown9c;
        public bint _unknowna0;
        public bint _unknowna4;
        public bint _unknowna8;
        public bint _unknownac;
        public bint _unknownb0;
        public bint _unknownb4;
        public bint _unknownb8;
        public bint _unknownbc;
        public bint _unknownc0;
        public bint _unknownc4;
        public bint _unknownc8;
        public bint _unknowncc;
        public bint _unknownd0;
        public bint _unknownd4;
        public bint _unknownd8;
        public bint _unknowndc;
        public bint _unknowne0;
        public bint _unknowne4;
        public bint _unknowne8;
        public bint _unknownec;
        public bint _unknownf0;
        public bint _unknownf4;
        public bint _unknownf8;
        public bint _unknownfc;
        public bint _unknown100;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}
