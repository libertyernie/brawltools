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

        /// <summary>
        /// Clamps the value to the rotational range of [-180, 180).
        /// </summary>
        public static Single Clamp180Deg(this Single value)
        {
            //Figure out how many multiples of 360 there are.
            //Dividing the value by 360 and cutting off the decimal places
            //will return the number of multiples of whole 360's in the value.
            //Then those multiples need to be subtracted out.
            value -= 360.0f * (int)(value / 360.0f);

            //Now the value is in the range of +360 to -360.
            //The range needs to be from +180 to -180.
            value += value > 180.0f ? -360.0f : value < -180.0f ? 360.0f : 0;

            //180 and -180 represent the same rotation.
            //When it comes to signed numbers, negative is highest. 
            //For example, -128 (0xFF) vs 127 (0x7F)
            if (value == 180.0f)
                value = -180.0f;

            return value;
        }
    }
}
