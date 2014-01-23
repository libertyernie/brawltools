using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class Int64Extension
    {
        public static Int64 Align(this Int64 value, int align)
        {
            if (value < 0) return 0;
            if (align <= 1) return value;
            long temp = value % align;
            if (temp != 0) value += align - temp;
            return value;
        }
        public static Int64 Clamp(this Int64 value, long min, long max)
        {
            if (value <= min) return min;
            if (value >= max) return max;
            return value;
        }
    }
}
