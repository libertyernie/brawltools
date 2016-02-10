using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Common2TblHeader
    {
        public const int Size = 0x20;

        public bint _Length;
        public bint _DataLength;
        public bint _OffCount;
        public bint _DataTable;
        public int _pad0;
        public int _pad1;
        public int _pad2;
        public int _pad3;


        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
        public string Str { get { return new string((sbyte*)Address + _DataLength + (_OffCount * 4) + 0x20 + (_DataTable*8)); } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EventStageTblEntry
    {
        public const int Size = 304;

		public bint _unknown00;
		public bint _unknown04;
		public bint _MatchType;
		public bint _TimeLimit;
		public bfloat _unknown10; // Timer visibility?
		public bfloat _unknown14;
		public bshort _ItemFrequency;
		public bshort _unknown1a;
		public byte _unknown1c;
		public byte _unknown1d;
		public byte _unknown1e;
		public byte _StageID;
		public byte _unknown20;
		public byte _unknown21;
		public byte _unknown22;
		public byte _unknown23;
		public bint _unknown24;
		public bint _unknown28;
		public bint _unknown2c;
		public bint _unknown30;
		public bint _unknown34;
		public bfloat _GameSpeed;
		public bfloat _CameraShakeControl;
		public byte _unknown40;
		public byte _unknown41;
		public byte _unknown42;
		public byte _unknown43;
		public bint _SongID;
		public bshort _GlobalOffenseRatio;
		public bshort _GlobalDefenseRatio;
		public bint _unknown4c;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
    }
}