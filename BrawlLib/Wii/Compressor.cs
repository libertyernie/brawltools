using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BrawlLib.SSBBTypes;
using System.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.Wii.Compression
{
    [global::System.Serializable]
    public class InvalidCompressionException : Exception
    {
        public InvalidCompressionException() { }
        public InvalidCompressionException(string message) : base(message) { }
        public InvalidCompressionException(string message, Exception inner) : base(message, inner) { }
        protected InvalidCompressionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public static unsafe class Compressor
    {
        internal static CompressionType[] _supportedCompressionTypes = 
        {
            CompressionType.None,
            CompressionType.LZ77,
            CompressionType.ExtendedLZ77,
            CompressionType.RunLength,
        };

        public static bool Supports(VoidPtr addr) { return Supports(((CompressionHeader*)addr)->Algorithm); }
        public static bool Supports(CompressionType type)
        {
            return Array.IndexOf(_supportedCompressionTypes, type) != -1;
        }
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static bool IsDataCompressed(DataSource source) { return IsDataCompressed(source.Address, source.Length); }
        public static bool IsDataCompressed(VoidPtr addr, int length)
        {
            if (*(uint*)addr == YAZ0.Tag)
                return true;
            else
            {
                CompressionHeader* cmpr = (CompressionHeader*)addr;

                if (cmpr->ExpandedSize < length)
                    return false;

                if (!cmpr->HasLegitCompression())
                    return false;

                char[] chars = characters.ToCharArray();

                //Check to make sure we're not reading a tag
                byte* c = (byte*)addr;
                byte[] tag = { c[0], c[1], c[2], c[3] };
                if ((Array.IndexOf(chars, (char)tag[0]) >= 0) &&
                    (Array.IndexOf(chars, (char)tag[1]) >= 0) &&
                    (Array.IndexOf(chars, (char)tag[2]) >= 0) &&
                    (Array.IndexOf(chars, (char)tag[3]) >= 0))
                    return false;

                return true;
            }
        }
        public static void Expand(CompressionHeader* header, VoidPtr dstAddr, int dstLen)
        {
            switch (header->Algorithm)
            {
                case CompressionType.LZ77:
                case CompressionType.ExtendedLZ77: { LZ77.Expand(header, dstAddr, dstLen); break; }
                case CompressionType.RunLength: { RunLength.Expand(header, dstAddr, dstLen); break; }
                //case CompressionType.Huffman: { Huffman.Expand(header, dstAddr, dstLen); break; }
                //case CompressionType.Differential: { Differential.Expand(header, dstAddr, dstLen); break; }
                default:
                    throw new InvalidCompressionException("Unknown compression type.");
            }
        }
        public static void Expand(YAZ0* header, VoidPtr dstAddr, int dstLen) { RunLength.ExpandYAZ0(header, dstAddr, dstLen); }
        internal static unsafe void CompactYAZ0(VoidPtr srcAddr, int srcLen, Stream outStream, ResourceNode r) { RunLength.CompactYAZ0(srcAddr, srcLen, outStream, r); }
        public static unsafe void Compact(CompressionType type, VoidPtr srcAddr, int srcLen, Stream outStream, ResourceNode r)
        {
            switch (type)
            {
                case CompressionType.LZ77: { LZ77.Compact(srcAddr, srcLen, outStream, r, false); break; }
                case CompressionType.ExtendedLZ77: { LZ77.Compact(srcAddr, srcLen, outStream, r, true); break; }
                case CompressionType.RunLength: { RunLength.Compact(srcAddr, srcLen, outStream, r); break; }
            }
        }
    }
}
