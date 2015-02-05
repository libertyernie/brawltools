using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct FSAR
    {
        public const int Size = 0x40;
        public const string Tag = "FSAR";

        public NW4FCommonHeader _header;
        public buint _unk1;
        public buint _unk2;
        public buint _strgOffset;
        public buint _strgLength;
        public buint _unk3;
        public buint _infoOffset;
        public buint _infoLength;
        public buint _unk4;
        public buint _fileOffset;
        public buint _fileLength;
        public buint _pad1;
        public buint _pad2;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct FSARSTRGHeader
    {
        public const int Size = 0x10;
        public const string Tag = "STRG";

        public SSBBEntryHeader _header;
        public ruint _dataOffset;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct FSARINFOHeader
    {
        public const int Size = 0x40;
        public const string Tag = "INFO";

        public SSBBEntryHeader _header;
        public ruint _dataOffset;
        public ruint _type4Offset;
        public ruint _type1Offset;
        public ruint _type3Offset;
        public ruint _type5Offset;
        public ruint _type2Offset;
        public ruint _type6Offset;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct FSARFILEHeader
    {
        public const int Size = 0x20;
        public const string Tag = "FILE";

        public SSBBEntryHeader _header;
        public fixed uint _pad[6];

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}
