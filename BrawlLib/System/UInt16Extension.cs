namespace System
{
    public static class UInt16Extension
    {
        public static BrawlLib.SSBBTypes.Endian Endian = BrawlLib.SSBBTypes.Endian.Big;
        public static UInt16 Reverse(this UInt16 value)
        {
            if (Endian == BrawlLib.SSBBTypes.Endian.Little)
                return value;
            return (ushort)((value >> 8) | (value << 8));
        }
    }
}
