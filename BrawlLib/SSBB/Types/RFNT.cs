using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct RFNT
    {
        NW4RCommonHeader _header;
    }
}