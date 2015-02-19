using BrawlLib.SSBBTypes;
using Ikarus.MovesetFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ikarus.MovesetBuilder
{
    public unsafe partial class DataCommonBuilder : BuilderBase
    {
        private void GetSizePart1()
        {
            GetSize(_dataCommon._unk23);

            if (_dataCommon._unk7 != null)
                foreach (CommonUnk7Entry e in _dataCommon._unk7)
                    foreach (CommonUnk7EntryListEntry e2 in e._children)
                        GetSize(e2);
            
            GetSize(_dataCommon._unk8);
            GetSize(_dataCommon._unk10);
            GetSize(_dataCommon._unk16);
            GetSize(_dataCommon._unk18);
            GetSize(_dataCommon._globalICs);
            GetSize(_dataCommon._ICs);
            GetSize(_dataCommon._unk12);
            GetSize(_dataCommon._unk13);
            GetSize(_dataCommon._unk14);
            GetSize(_dataCommon._unk15);
            GetSize(_dataCommon._globalsseICs);
            GetSize(_dataCommon._sseICs);
        }

        private void BuildPart1()
        {
            Write(_dataCommon._unk23);

            if (_dataCommon._unk7 != null)
                foreach (CommonUnk7Entry e in _dataCommon._unk7)
                    foreach (CommonUnk7EntryListEntry e2 in e._children)
                        Write(e2, 8);

            Write(_dataCommon._unk8);
            Write(_dataCommon._unk10);
            Write(_dataCommon._unk16);
            Write(_dataCommon._unk18);
            Write(_dataCommon._globalICs);
            Write(_dataCommon._ICs);
            Write(_dataCommon._unk12);
            Write(_dataCommon._unk13);
            Write(_dataCommon._unk14);
            Write(_dataCommon._unk15);
            Write(_dataCommon._globalsseICs);
            Write(_dataCommon._sseICs);
        }
    }
}
