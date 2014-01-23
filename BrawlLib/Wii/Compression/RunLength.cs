using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Compression;
using BrawlLib.IO;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.Wii.Compression
{
    public unsafe class RunLength
    {
        //Credit goes to Chadderz for this compressor included in CTools.
        //http://wiki.tockdom.com/wiki/User:Chadderz
        private struct Contraction
        {
            public int Location;
            public int Size;
            public int Offset;

            public Contraction(int loc, int sz, int off)
            {
                Location = loc;
                Size = sz;
                Offset = off;
            }
        }

        private static int _lookBackCache = 63;
        private const int _threadChunk = 0x10000;
        private List<Contraction>[] _contractions;
        private int _sourceLen;
        private byte* _pSrc;

        private void FindContractions(int chunk)
        {
            int from, to, run, bestRun, bestOffset;
            Contraction contraction;

            _contractions[chunk] = new List<Contraction>();

            from = chunk * _threadChunk;
            to = Math.Min(from + _threadChunk, _sourceLen);

            for (int i = from; i < to; )
            {
                bestRun = 0;
                bestOffset = 0;

                for (int j = i - 1; j > 0 && j >= i - _lookBackCache; j--)
                {
                    run = 0;

                    while (i + run < _sourceLen && run < 0x111 && _pSrc[j + run] == _pSrc[i + run])
                        run++;

                    if (run > bestRun)
                    {
                        bestRun = run;
                        bestOffset = i - j - 1;

                        if (run == 0x111) break;
                    }
                }

                if (bestRun >= 3)
                {
                    contraction = new Contraction(i, bestRun, bestOffset);
                    _contractions[chunk].Add(contraction);
                    i += bestRun;
                }
                else
                    i++;
            }
        }

        public int Compress(VoidPtr srcAddr, int srcLen, Stream outStream, IProgressTracker progress, bool YAZ0)
        {
            _pSrc = (byte*)srcAddr;
            _sourceLen = srcLen;

            int chunkCount = (int)Math.Ceiling((double)srcLen / _threadChunk);

            if (progress != null)
                progress.Begin(0, srcLen, 0);

            _contractions = new List<Contraction>[chunkCount];

            if (YAZ0)
            {
                outStream.WriteByte(0x59);
                outStream.WriteByte(0x61);
                outStream.WriteByte(0x7A);
                outStream.WriteByte(0x30);

                outStream.WriteByte((byte)((_sourceLen >> 24) & 0xFF));
                outStream.WriteByte((byte)((_sourceLen >> 16) & 0xFF));
                outStream.WriteByte((byte)((_sourceLen >> 08) & 0xFF));
                outStream.WriteByte((byte)((_sourceLen >> 00) & 0xFF));

                for (int i = 0; i < 8; i++)
                    outStream.WriteByte(0);
            }
            else
            {
                CompressionHeader header = new CompressionHeader();
                header.Algorithm = CompressionType.RunLength;
                header.ExpandedSize = (int)_sourceLen;
                outStream.Write(&header, 4 + (header.LargeSize ? 4 : 0));
            }

            ParallelLoopResult result = Parallel.For(0, chunkCount, FindContractions);

            while (!result.IsCompleted)
                Thread.Sleep(100);

            List<Contraction> fullContractions;
            int codeBits, tempLoc, current;
            byte codeByte;
            byte[] temp;
            int lastUpdate = srcLen;

            fullContractions = new List<Contraction>();
            for (int i = 0; i < _contractions.Length; i++)
            {
                fullContractions.AddRange(_contractions[i]);
                _contractions[i].Clear();
                _contractions[i] = null;
            }

            _contractions = null;
            temp = new byte[3 * 8];
            codeBits = 0;
            codeByte = 0;
            tempLoc = 0;
            current = 0;

            for (int i = 0; i < srcLen; )
            {
                if (codeBits == 8)
                {
                    outStream.WriteByte(codeByte);
                    outStream.Write(temp, 0, tempLoc);
                    codeBits = 0;
                    codeByte = 0;
                    tempLoc = 0;
                }

                if (current < fullContractions.Count && fullContractions[current].Location == i)
                {
                    if (fullContractions[current].Size >= 0x12)
                    {
                        temp[tempLoc++] = (byte)(fullContractions[current].Offset >> 8);
                        temp[tempLoc++] = (byte)(fullContractions[current].Offset & 0xFF);
                        temp[tempLoc++] = (byte)(fullContractions[current].Size - 0x12);
                    }
                    else
                    {
                        temp[tempLoc++] = (byte)((fullContractions[current].Offset >> 8) | ((fullContractions[current].Size - 2) << 4));
                        temp[tempLoc++] = (byte)(fullContractions[current].Offset & 0xFF);
                    }

                    i += fullContractions[current++].Size;

                    while (current < fullContractions.Count && fullContractions[current].Location < i)
                        current++;
                }
                else
                {
                    codeByte |= (byte)(1 << (7 - codeBits));
                    temp[tempLoc++] = _pSrc[i++];
                }

                codeBits++;

                if (progress != null)
                    if (i % 0x4000 == 0)
                        progress.Update(i);
            }

            outStream.WriteByte(codeByte);
            outStream.Write(temp, 0, tempLoc);

            outStream.Flush();

            if (progress != null)
                progress.Finish();

            return (int)outStream.Length;
        }

        public static int CompactYAZ0(VoidPtr srcAddr, int srcLen, Stream outStream, ResourceNode r)
        {
            using (ProgressWindow prog = new ProgressWindow(r.RootNode._mainForm, "RunLength", String.Format("Compressing {0}, please wait...", r.Name), false))
                return new RunLength().Compress(srcAddr, srcLen, outStream, prog, true);
        }

        public static int Compact(VoidPtr srcAddr, int srcLen, Stream outStream, ResourceNode r)
        {
            using (ProgressWindow prog = new ProgressWindow(r.RootNode._mainForm, "RunLength", String.Format("Compressing {0}, please wait...", r.Name), false))
                return new RunLength().Compress(srcAddr, srcLen, outStream, prog, false);
        }

        public static void ExpandYAZ0(YAZ0* header, VoidPtr dstAddress, int dstLen) { Expand(header->Data, dstAddress, dstLen); }
        public static void Expand(CompressionHeader* header, VoidPtr dstAddress, int dstLen) { Expand(header->Data, dstAddress, dstLen); }
        public static void Expand(VoidPtr srcAddress, VoidPtr dstAddress, int dstLen)
        {
            for (byte* srcPtr = (byte*)srcAddress, dstPtr = (byte*)dstAddress, ceiling = dstPtr + dstLen; dstPtr < ceiling; )
                for (byte control = *srcPtr++, bit = 8; (bit-- != 0) && (dstPtr != ceiling); )
                    if ((control & (1 << bit)) != 0)
                        *dstPtr++ = *srcPtr++;
                    else
                        for (int b1 = *srcPtr++, b2 = *srcPtr++, offset = ((b1 & 0xF) << 8 | b2) + 2, temp = (b1 >> 4) & 0xF, num = temp == 0 ? *srcPtr++ + 0x12 : temp + 2; num-- > 0 && dstPtr != ceiling; *dstPtr++ = dstPtr[-offset]) ;
        }
    }
}