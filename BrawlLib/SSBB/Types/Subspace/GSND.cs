using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct GSND
    {
        public const uint Tag = 0x444E5347;
        public const int Size = 0x50;
        public uint _tag;
        public bint _count;

        public VoidPtr this[int index] { get { return (VoidPtr)((byte*)Address + Offsets(index)); } }
        public uint Offsets(int index) { return *(buint*)((byte*)Address + 0x08 + (index * 4)); }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public GSND(int count)
        {
            _tag = Tag;
            _count = count;
        }
    }
    public unsafe struct GSNDEntry
    {
        public bint _InfoIndex;
        public const uint unk0 = 0x00000001;
        public int _pad0;
        public int _pad1;
        public int _pad2;
        public bfloat _unkFloat0;
        public bfloat _unkFloat1;
        public int _pad3;
        fixed sbyte _name[32];
        public sbyte _Trigger;
        fixed int _pad4[16];

        public GSNDEntry(bint InfoIndex, bfloat UnkFloat0, bfloat UnkFloat1, sbyte Trigger,string name)
        {
            _InfoIndex = InfoIndex;
            _pad0 = _pad1 = _pad2 =_pad3 = 0;
            _unkFloat0 = UnkFloat0;
            _unkFloat1 = UnkFloat1;
            _Trigger = Trigger;
            Name = name;
        }

        public string Name
        {
            get { return new String((sbyte*)Address + 0x1C); }
            set
            {
                if (value == null)
                    value = "";

                fixed (sbyte* ptr = _name)
                {
                    int i = 0;
                    while ((i < 31) && (i < value.Length))
                        ptr[i] = (sbyte)value[i++];

                    while (i < 32) ptr[i++] = 0;
                }
            }
        }
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}