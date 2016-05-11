namespace System
{
    public static class Int16Extension
    {
        public static BrawlLib.SSBBTypes.Endian Endian = BrawlLib.SSBBTypes.Endian.Big;
        public static Int16 Reverse(this Int16 value)
        {
            if (Endian == BrawlLib.SSBBTypes.Endian.Little)
                return value;
            return (short)(((value >> 8) & 0xFF) | (value << 8));
        }
    }
}
