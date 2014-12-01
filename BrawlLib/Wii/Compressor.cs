using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BrawlLib.SSBBTypes;
using System.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.IO;

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
        private const int CompressBufferLen = 0x60;
        private const string RawDataName = "UnknownData";

        internal static CompressionType[] _supportedCompressionTypes = 
        {
            CompressionType.None,
            CompressionType.LZ77,
            CompressionType.ExtendedLZ77,
            CompressionType.RunLength,
            CompressionType.RunLengthYAZ0,
            CompressionType.RunLengthYAY0,
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
            BinTag compTag = *(BinTag*)addr;
            if (compTag == YAZ0.Tag || compTag == YAY0.Tag)
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

        public static ResourceNode TryExpand(DataSource source, ResourceNode parent, bool returnRaw)
        {
            ResourceNode n = null;
            if (IsDataCompressed(source))
            {
                try
                {
                    uint len = 0;
                    CompressionType algorithm = CompressionType.None;

                    FileMap map = null;
                    UnsafeBuffer testBuffer = null;

                    BinTag tag = *(BinTag*)source.Address;
                    int type = tag == YAZ0.Tag ? 0 : tag == YAY0.Tag ? 1 : 2;
                    switch (type)
                    {
                        case 0:
                            algorithm = CompressionType.RunLengthYAZ0;
                            len = *(buint*)(source.Address + 4);
                            if (returnRaw)
                            {
                                map = FileMap.FromTempFile((int)len);
                                Expand((YAZ0*)source.Address, map.Address, map.Length);
                            }
                            else
                            {
                                testBuffer = new UnsafeBuffer(CompressBufferLen);
                                Expand((YAZ0*)source.Address, testBuffer.Address, testBuffer.Length);
                            }
                            break;
                        case 1:
                            algorithm = CompressionType.RunLengthYAY0;
                            len = *(buint*)(source.Address + 4);
                            if (returnRaw)
                            {
                                map = FileMap.FromTempFile((int)len);
                                Expand((YAY0*)source.Address, map.Address, map.Length);
                            }
                            else
                            {
                                testBuffer = new UnsafeBuffer(CompressBufferLen);
                                Expand((YAY0*)source.Address, testBuffer.Address, testBuffer.Length);
                            }
                            break;
                        case 2:
                            CompressionHeader* hdr = (CompressionHeader*)source.Address;
                            algorithm = hdr->Algorithm;
                            len = hdr->ExpandedSize;

                            if (!Supports(algorithm))
                            {
                                source.Compression = algorithm;
                                goto NotDecompressable;
                            }

                            if (returnRaw)
                            {
                                map = FileMap.FromTempFile((int)len);
                                Expand(hdr, map.Address, map.Length);
                            }
                            else
                            {
                                testBuffer = new UnsafeBuffer(CompressBufferLen);
                                Expand(hdr, testBuffer.Address, testBuffer.Length);
                            }
                            break;
                    }

                    source.Compression = algorithm;

                    if (returnRaw)
                    {
                        if ((n = NodeFactory.GetRaw(map.Address, map.Length)) == null)
                            n = new RawDataNode(RawDataName);
                        n.Initialize(parent, source, new DataSource(map));
                    }
                    else
                    {
                        if ((n = NodeFactory.GetRaw(new DataSource(testBuffer.Address, testBuffer.Length))) != null)
                        {
                            map = FileMap.FromTempFile((int)len);
                            switch (type)
                            {
                                case 0:
                                    Compressor.Expand((YAZ0*)source.Address, map.Address, map.Length);
                                    break;
                                case 1:
                                    Compressor.Expand((YAY0*)source.Address, map.Address, map.Length);
                                    break;
                                case 2:
                                    Compressor.Expand((CompressionHeader*)source.Address, map.Address, map.Length);
                                    break;
                            }
                            n.Initialize(parent, source, new DataSource(map));
                        }
                    }
                    return n;
                }
                catch (InvalidCompressionException e) { MessageBox.Show(e.ToString()); }
            }

            NotDecompressable:
            if (returnRaw)
                (n = new RawDataNode(RawDataName)).Initialize(parent, source);

            return n;
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
        public static void Expand(YAY0* header, VoidPtr dstAddr, int dstLen) { RunLength.ExpandYAY0(header, dstAddr, dstLen); }
        public static unsafe void Compact(CompressionType type, VoidPtr srcAddr, int srcLen, Stream outStream, ResourceNode r)
        {
            switch (type)
            {
                case CompressionType.LZ77: { LZ77.Compact(srcAddr, srcLen, outStream, r, false); break; }
                case CompressionType.ExtendedLZ77: { LZ77.Compact(srcAddr, srcLen, outStream, r, true); break; }
                case CompressionType.RunLength: { RunLength.Compact(srcAddr, srcLen, outStream, r); break; }
                case CompressionType.RunLengthYAZ0: { RunLength.CompactYAZ0(srcAddr, srcLen, outStream, r); break; }
                case CompressionType.RunLengthYAY0: { RunLength.CompactYAY0(srcAddr, srcLen, outStream, r); break; }
            }
        }
    }
}
