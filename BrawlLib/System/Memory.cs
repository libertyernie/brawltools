using System.IO;

namespace System
{
    public unsafe static class Memory
    {
        public static unsafe void Move(VoidPtr dst, VoidPtr src, uint size)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    Win32.MoveMemory(dst, src, size);
                    break;
                default:
                    byte* bsrc = (byte*)src;
                    byte* bdst = (byte*)dst;
                    for (uint i = 0; i < size; i++) {
                        bdst[i] = bsrc[i];
                    }
                    break;
            }
        }

        internal static unsafe void Fill(VoidPtr dest, uint length, byte value)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT: { 
                    Win32.FillMemory(dest, length, value); 
                    break; 
                }
                default:
                    byte* ptr = (byte*)dest;
                    for (int i = 0; i < length; i++) {
                        ptr[i] = value;
                    }
                    break;
            }
        }
    }
}
