using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.WiiU.Models
{
    public unsafe delegate void ElementDecoder(ref byte* pIn, ref byte* pOut, float scale);
    public unsafe class ElementCodec
    {
        public enum FVTXAttributeFormat
        {
            XY8 = 0x04,
            XY16 = 0x07,
            XYZW8 = 0x0A,
            XYZ10 = 0x0B,
            XY32 = 0x0D,
            XYZW16 = 0x0F,
            XYZ32 = 0x11,
        }

        public static ElementDecoder[] Decoders = new ElementDecoder[] 
        {
            //Element_Input_Output
            Element_Byte_Float2, //S

        };

        public static void Element_Byte_Float2(ref byte* pIn, ref byte* pOut, float scale)
        {
            ((float*)pOut)[0] = (float)(*pIn++) * scale;
            ((float*)pOut)[1] = 0.0f;
            pOut += 8;
        }
    }
}
