using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BrawlLib.Wii.Models
{
    [StructLayout( LayoutKind.Sequential)]
    public unsafe struct bMatrix43
    {
        fixed float _data[12];

        public bfloat* Data { get { fixed (float* ptr = _data)return (bfloat*)ptr; } }

        public float this[int x, int y]
        {
            get { return Data[(y << 2) + x]; }
            set { Data[(y << 2) + x] = value; }
        }
        public float this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
        }

        public override string ToString()
        {
            return String.Format("({0},{1},{2},{3})({4},{5},{6},{7})({8},{9},{10},{11})", this[0], this[1], this[2], this[3], this[4], this[5], this[6], this[7], this[8], this[9], this[10], this[11]);
        }

        public static implicit operator Matrix(bMatrix43 bm)
        {
            Matrix m;

            bfloat* sPtr = (bfloat*)&bm;
            float* dPtr = (float*)&m;

            dPtr[0] = sPtr[0];
            dPtr[1] = sPtr[4];
            dPtr[2] = sPtr[8];
            dPtr[3] = 0.0f;
            dPtr[4] = sPtr[1];
            dPtr[5] = sPtr[5];
            dPtr[6] = sPtr[9];
            dPtr[7] = 0.0f;
            dPtr[8] = sPtr[2];
            dPtr[9] = sPtr[6];
            dPtr[10] = sPtr[10];
            dPtr[11] = 0.0f;
            dPtr[12] = sPtr[3];
            dPtr[13] = sPtr[7];
            dPtr[14] = sPtr[11];
            dPtr[15] = 1.0f;

            return m;
        }

        public static implicit operator bMatrix43(Matrix m)
        {
            bMatrix43 bm;

            bfloat* dPtr = (bfloat*)&bm;
            float* sPtr = (float*)&m;

            dPtr[0] = sPtr[0];
            dPtr[1] = sPtr[4];
            dPtr[2] = sPtr[8];
            dPtr[3] = sPtr[12];
            dPtr[4] = sPtr[1];
            dPtr[5] = sPtr[5];
            dPtr[6] = sPtr[9];
            dPtr[7] = sPtr[13];
            dPtr[8] = sPtr[2];
            dPtr[9] = sPtr[6];
            dPtr[10] = sPtr[10];
            dPtr[11] = sPtr[14];

            return bm;
        }

        public static implicit operator Matrix43(bMatrix43 bm)
        {
            Matrix43 m = new Matrix43();
            float* dPtr = (float*)&m;
            bfloat* sPtr = (bfloat*)&bm;
            for (int i = 0; i < 12; i++)
                dPtr[i] = sPtr[i];
            return m;
        }

        public static implicit operator bMatrix43(Matrix43 m)
        {
            bMatrix43 bm = new bMatrix43();
            bfloat* dPtr = (bfloat*)&bm;
            float* sPtr = (float*)&m;
            for (int i = 0; i < 12; i++)
                dPtr[i] = sPtr[i];
            return bm;
        }
    }
}
