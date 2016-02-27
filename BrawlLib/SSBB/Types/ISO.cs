using System;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ISO
    {
        public const uint Tag = 0xA39E1C5D;

        public char _console;
        public char _title0;
        public char _title1;
        public char _region;
        public bushort _publisher;
        public bushort _unk1;
        public fixed byte _pad1[0x10];
        public uint _tag;
        public uint _pad2;
        public fixed byte _name[0x60];

        public string GameID
        {
            get { return *(BinTag*)Address; }
            set { *(BinTag*)Address = value; }
        }
        public bool IsWii
        {
            get
            {
                return 
                    _console == 'R' || 
                    _console == '_' ||
                    _console == 'H' ||
                    _console == '0' ||
                    _console == '4';
            }
        }
        public bool IsGC
        {
            get
            {
                return
                  _console == 'G' ||
                  _console == 'D' ||
                  _console == 'P' ||
                  _console == 'U';
            }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }
}
