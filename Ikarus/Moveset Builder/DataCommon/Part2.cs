using Ikarus.MovesetFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ikarus.MovesetBuilder
{
    public unsafe partial class DataCommonBuilder : BuilderBase
    {
        private void GetSizePart2()
        {
            foreach (CommonAction a in _dataCommon.FlashOverlays)
            {
                if (a.Count > 0 || a.ForceWrite)
                {
                    GetSize(a); //Event + param data
                    AddSize(8); //Offset to data + flags
                }
            }

            if (_dataCommon._ppMul != null)
                foreach (Script s in _dataCommon._ppMul._scripts)
                    foreach (Event e in s)
                        foreach (Parameter p in e)
                            GetSize(p);

            foreach (CommonAction a in _dataCommon.ScreenTints)
                GetSize(a);
        }

        private void BuildPart2()
        {
            foreach (CommonAction a in _dataCommon.FlashOverlays)
                Write(a);

            if (_dataCommon._ppMul != null)
                foreach (Script s in _dataCommon._ppMul._scripts)
                    foreach (Event e in s)
                        foreach (Parameter p in e)
                            Write(p);

            foreach (CommonAction a in _dataCommon.ScreenTints)
                Write(a);
        }
    }
}
