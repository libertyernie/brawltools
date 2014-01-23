using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class SingleExtension
    {
        public static unsafe Single Reverse(this Single value)
        {
            *(uint*)(&value) = ((uint*)&value)->Reverse();
            return value;
        }

        //private static double _double2fixmagic = 68719476736.0f * 1.5f;
        //public static unsafe Int32 ToInt32(this Single value)
        //{
        //    double v = value + _double2fixmagic;
        //    return *((int*)&v) >> 16; 
        //}

        public static Single Clamp(this Single value, Single min, Single max)
        {
            return value <= min ? min : value >= max ? max : value;
        }

        public static Single Clamp180Deg(this Single value)
        {
            float e = value;

            float d = (int)(e / 360.0f);
            e -= 360.0f * d;

            float l = e / 180.0f;
            if (l > 1)
                e -= 360.0f;
            else if (l < -1)
                e += 360.0f;

            return e;
        }
    }
}
