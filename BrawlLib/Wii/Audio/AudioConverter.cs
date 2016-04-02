using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BrawlLib.Wii.Audio
{
    public unsafe class AudioConverter
    {
        /* Temporal Vector
         * A contiguous history of 3 samples starting with
         * 'current' and going 2 backwards
         */
        public unsafe struct tvec
        {
            fixed double values[3];

            public unsafe double this[int index]
            {
                get
                {
                    fixed (double* ptr = values) return ptr[index];
                }
                set
                {
                    fixed (double* ptr = values) ptr[index] = value;
                }
            }
        }

        public static void CalcCoefs(short* source, int samples, short* coefsOut, IProgressTracker progress)
        {
            int numFrames = (samples + 13) / 14;
            int frameSamples;

            short* blockBuffer = stackalloc short[0x3800];
            short* pcmHistBuffer0 = stackalloc short[14];
            Memory.Fill(pcmHistBuffer0, (uint)(sizeof(tvec) * 14), 0);
            short* pcmHistBuffer1 = stackalloc short[14];
            Memory.Fill(pcmHistBuffer1, (uint)(sizeof(tvec) * 14), 0);

            tvec vec1;
            tvec vec2;

            tvec* mtx = stackalloc tvec[3];
            int* vecIdxs = stackalloc int[3];

            tvec* records = (tvec*)Marshal.AllocHGlobal(sizeof(tvec) * numFrames * 2);
            Memory.Fill(records, (uint)(sizeof(tvec) * numFrames * 2), 0);
            int recordCount = 0;

            tvec* vecBest = stackalloc tvec[8];

            float initValue = 0;
            int lastUpdate = 0;
            if (progress != null)
                initValue = progress.CurrentValue;

            /* Iterate though 1024-block frames */
            for (int x = samples; x>0 ;)
            {
                if (x > 0x3800) /* Full 1024-block frame */
                {
                    frameSamples = 0x3800;
                    x -= 0x3800;
                }
                else /* Partial frame */
                {
                    /* Zero lingering block samples */
                    frameSamples = x;
                    for (int z = 0; z< 14 && z+frameSamples<0x3800 ; z++)
                        blockBuffer[frameSamples + z] = 0;
                    x = 0;
                }

                /* Copy (potentially non-frame-aligned PCM samples into aligned buffer) */
                Memory.Move(blockBuffer, source, (uint)(frameSamples * sizeof(short)));
                source += frameSamples;


                for (int i = 0; i<frameSamples ;)
                {
                    for (int z = 0; z < 14 ; z++)
                        pcmHistBuffer0[z] = pcmHistBuffer1[z];
                    for (int z = 0; z < 14 ; z++)
                        pcmHistBuffer1[z] = blockBuffer[i++];

                    InnerProductMerge(vec1, pcmHistBuffer1);
                    if (Math.Abs(vec1[0]) > 10.0)
                    {
                        OuterProductMerge(mtx, pcmHistBuffer1);
                        if (!AnalyzeRanges(mtx, vecIdxs))
                        {
                            BidirectionalFilter(mtx, vecIdxs, vec1);
                            if (!QuadraticMerge(vec1))
                            {
                                FinishRecord(vec1, records[recordCount]);
                                recordCount++;
                            }
                        }
                    }
                }
                if (progress != null)
                {
                    lastUpdate += frameSamples;
                    if ((lastUpdate % 0x3800) == 0)
                        progress.Update(progress.CurrentValue + 0x3800);
                }
            }

            if (progress != null)
                progress.Update(initValue + samples);

            vec1[0] = 1.0;
            vec1[1] = 0.0;
            vec1[2] = 0.0;

            for (int z = 0; z<recordCount ; z++)
            {
                MatrixFilter(records[z], vecBest[0]);
                for (int y = 1; y <= 2 ; y++)
                    vec1[y] += vecBest[0][y];
            }
            for (int y = 1; y <= 2 ; y++)
                vec1[y] /= recordCount;

            MergeFinishRecord(vec1, vecBest[0]);


            int exp = 1;
            for (int w = 0; w<3 ;)
            {
                vec2[0] = 0.0;
                vec2[1] = -1.0;
                vec2[2] = 0.0;
                for (int i = 0; i<exp ; i++)
                    for (int y = 0; y <= 2 ; y++)
                        vecBest[exp + i][y] = (0.01 * vec2[y]) + vecBest[i][y];
                ++w;
                exp = 1 << w;
                FilterRecords(vecBest, exp, records, recordCount);
            }

            // Write output
            for (int z = 0; z<8 ; z++)
            {
                double d;
                d = -vecBest[z][1] * 2048.0;
                if (d > 0.0)
                    coefsOut[z * 2] = (d > 32767.0) ? (short)32767 : (short)Math.Round(d, MidpointRounding.AwayFromZero);
                else
                    coefsOut[z * 2] = (d< -32768.0) ? (short)-32768 : (short)Math.Round(d, MidpointRounding.AwayFromZero);

                d = -vecBest[z][2] * 2048.0;
                if (d > 0.0)
                    coefsOut[z * 2 + 1] = (d > 32767.0) ? (short)32767 : (short)Math.Round(d, MidpointRounding.AwayFromZero);
                else
                    coefsOut[z * 2 + 1] = (d< -32768.0) ? (short)-32768 : (short)Math.Round(d, MidpointRounding.AwayFromZero);
            }

            //Free memory
            Marshal.FreeHGlobal((IntPtr)records);

        }

        public static unsafe void EncodeBlock(short* source, int samples, byte* dest, short* coefs)
        {
            for (int i = 0; i < samples; i += 14, source += 14, dest += 8)
                DSPEncodeFrame(source, Math.Min(samples - i, 14), dest, coefs);
        }

        /* Make sure source includes the yn values (16 samples total) */
        public static void DSPEncodeFrame(short* pcmInOut, int sampleCount, byte* adpcmOut, short* coefsIn)
        {
            int* buffer1 = stackalloc int[128];
            int* buffer2 = stackalloc int[112];

            int bestDistance = int.MaxValue;
            int distAccum;
            int bestIndex = 0;
            int bestScale = 0;

            int distance, index, scale;

            int* p1;
            int* p2;
            int* t1;
            int* t2;
            short* sPtr;
            int v1, v2, v3;

            //Iterate through each coef set, finding the set with the smallest error */
            p1 = buffer1;
            p2 = buffer2;
            for (int i = 0; i < 8; i++, p1 += 16, p2 += 14, coefsIn += 2)
            {
                //Set yn values
                t1 = p1;
                *t1++ = pcmInOut[0];
                *t1++ = pcmInOut[1];

                //Round and clamp samples for this coef set
                distance = 0;
                sPtr = pcmInOut;
                for (int y = 0; y < sampleCount; y++)
                {
                    //Multiply previous samples by coefs
                    *t1++ = v1 = ((sPtr[0] * coefsIn[1]) + (sPtr[1] * coefsIn[0])) >> 11;
                    //Subtract from current sample
                    v2 = sPtr[2] - v1;
                    //Clamp
                    v3 = (v2 >= 32767) ? 32767 : (v2 <= -32768) ? -32768 : v2;
                    //Compare distance
                    if (Math.Abs(v3) > Math.Abs(distance))
                        distance = v3;

                    sPtr += 1;
                }

                //Set initial scale
                for (scale = 0; (scale <= 12) && ((distance > 7) || (distance < -8)); scale++, distance >>= 1) ;
                    scale = (scale <= 1) ? -1 : scale - 2;

                do
                {
                    scale++;
                    distAccum = 0;
                    index = 0;

                    t1 = p1;
                    t2 = p2;
                    sPtr = pcmInOut + 2;
                    for (int y = 0; y < sampleCount; y++)
                    {
                        //Multiply previous
                        v1 = ((t1[0] * coefsIn[1]) + (t1[1] * coefsIn[0]));
                        //Evaluate from real sample
                        v2 = ((*sPtr << 11) - v1) / 2048;
                        //Round to nearest sample
                        v3 = (v2 > 0) ? (int)((double)v2 / (1 << scale) + 0.4999999f) : (int)((double)v2 / (1 << scale) - 0.4999999f);

                        //Clamp sample and set index
                        if (v3 < -8)
                        {
                            if (index < (v3 = -8 - v3))
                                index = v3;
                            v3 = -8;
                        }
                        else if (v3 > 7)
                        {
                            if (index < (v3 -= 7))
                                index = v3;
                            v3 = 7;
                        }

                        //Store result
                        *t2++ = v3;

                        //Round and expand
                        v1 = (v1 + ((v3 * (1 << scale)) << 11) + 1024) >> 11;
                        //Clamp and store
                        t1[2] = v2 = (v1 >= 32767) ? 32767 : (v1 <= -32768) ? -32768 : v1;
                        //Accumulate distance
                        v3 = *sPtr++ - v2;
                        distAccum += v3 * v3;

                        t1 += 1;

                        //Break if we're higher than a previous search
                        if (distAccum >= bestDistance)
                            break;
                    }

                    for (int x = index + 8; x > 256; x >>= 1)
                        if (++scale >= 12)
                            scale = 11;

                } while ((scale < 12) && (index > 1));

                if (distAccum < bestDistance)
                {
                    bestDistance = distAccum;
                    bestIndex = i;
                    bestScale = scale;
                }
            }

            p1 = buffer1 + (bestIndex * 16) + 2;
            p2 = buffer2 + (bestIndex * 14);

            //Write converted samples
            sPtr = pcmInOut + 2;
            for (int i = 0; i < sampleCount; i++)
                *sPtr++ = (short)*p1++;

            //Write ps
            *adpcmOut++ = (byte)((bestIndex << 4) | (bestScale & 0xF));

            //Zero remaining samples
            for (int i = sampleCount; i < 14; i++)
                p2[i] = 0;

            //Write output samples
            for (int y = 0; y++ < 7;)
            {
                *adpcmOut++ = (byte)((p2[0] << 4) | (p2[1] & 0xF));
                p2 += 2;
            }
        }

        static void FilterRecords(tvec* vecBest, int exp, tvec* records, int recordCount)
        {
            tvec* bufferList = stackalloc tvec[8];

            int* buffer1 = stackalloc int[8];
            tvec buffer2;

            int index;
            double value, tempVal = 0;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < exp; y++)
                {
                    buffer1[y] = 0;
                    for (int i = 0; i <= 2; i++)
                        bufferList[y][i] = 0.0;
                }
                for (int z = 0; z < recordCount; z++)
                {
                    index = 0;
                    value = 1.0e30;
                    for (int i = 0; i < exp; i++)
                    {
                        tempVal = ContrastVectors(vecBest[i], records[z]);
                        if (tempVal < value)
                        {
                            value = tempVal;
                            index = i;
                        }
                    }
                    buffer1[index]++;
                    MatrixFilter(records[z], buffer2);
                    for (int i = 0; i <= 2; i++)
                        bufferList[index][i] += buffer2[i];
                }

                for (int i = 0; i < exp; i++)
                    if (buffer1[i] > 0)
                        for (int y = 0; y <= 2; y++)
                            bufferList[i][y] /= buffer1[i];

                for (int i = 0; i < exp; i++)
                    MergeFinishRecord(bufferList[i], vecBest[i]);
            }
        }

        static double ContrastVectors(tvec source1, tvec source2)
        {
            double val = (source2[2] * source2[1] + -source2[1]) / (1.0 - source2[2] * source2[2]);
            double val1 = (source1[0] * source1[0]) + (source1[1] * source1[1]) + (source1[2] * source1[2]);
            double val2 = (source1[0] * source1[1]) + (source1[1] * source1[2]);
            double val3 = source1[0] * source1[2];
            return val1 + (2.0 * val * val2) + (2.0 * (-source2[1] * val + -source2[2]) * val3);
        }

        static void FinishRecord(tvec inp, tvec outp)
        {
            for (int z = 1; z <= 2; z++)
            {
                if (inp[z] >= 1.0)
                    inp[z] = 0.9999999999;
                else if (inp[z] <= -1.0)
                    inp[z] = -0.9999999999;
            }
            outp[0] = 1.0;
            outp[1] = (inp[2] * inp[1]) + inp[1];
            outp[2] = inp[2];
        }

        static void MergeFinishRecord(tvec src, tvec dst)
        {
            tvec tmp;
            double val = src[0];

            dst[0] = 1.0;
            for (int i = 1; i <= 2; i++)
            {
                double v2 = 0.0;
                for (int y = 1; y < i; y++)
                    v2 += dst[y] * src[i - y];

                if (val > 0.0)
                    dst[i] = -(v2 + src[i]) / val;
                else
                    dst[i] = 0.0;

                tmp[i] = dst[i];

                for (int y = 1; y < i; y++)
                    dst[y] += dst[i] * dst[i - y];

                val *= 1.0 - (dst[i] * dst[i]);
            }

            FinishRecord(tmp, dst);
        }

        static void MatrixFilter(tvec src, tvec dst)
        {
            tvec* mtx = stackalloc tvec[3];

            mtx[2][0] = 1.0;
            for (int i = 1; i <= 2; i++)
                mtx[2][i] = -src[i];

            for (int i = 2; i > 0; i--)
            {
                double val = 1.0 - (mtx[i][i] * mtx[i][i]);
                for (int y = 1; y <= i; y++)
                    mtx[i - 1][y] = ((mtx[i][i] * mtx[i][y]) + mtx[i][y]) / val;
            }

            dst[0] = 1.0;
            for (int i = 1; i <= 2; i++)
            {
                dst[i] = 0.0;
                for (int y = 1; y <= i; y++)
                    dst[i] += mtx[i][y] * dst[i - y];
            }
        }

        static void InnerProductMerge(tvec vecOut, short* pcmBuf)
        {
            for (int i = 0; i <= 2; i++)
            {
                vecOut[i] = 0.0f;
                for (int x = 0; x < 14; x++)
                    vecOut[i] -= pcmBuf[x - i] * pcmBuf[x];
            }
        }

        static void OuterProductMerge(tvec* mtxOut, short* pcmBuf)
        {
            for (int x = 1; x <= 2; x++)
                for (int y = 1; y <= 2; y++)
                {
                    mtxOut[x][y] = 0.0;
                    for (int z = 0; z < 14; z++)
                        mtxOut[x][y] += pcmBuf[z - x] * pcmBuf[z - y];
                }
        }

        static bool AnalyzeRanges(tvec* mtx, int* vecIdxsOut)
        {
            double* recips = stackalloc double[3];
            double val, tmp, min, max;

            /* Get greatest distance from zero */
            for (int x = 1; x <= 2; x++)
            {
                val = Math.Max(Math.Abs(mtx[x][1]), Math.Abs(mtx[x][2]));
                if (val < Double.Epsilon)
                    return true;

                recips[x] = 1.0 / val;
            }

            int maxIndex = 0;
            for (int i = 1; i <= 2; i++)
            {
                for (int x = 1; x < i; x++)
                {
                    tmp = mtx[x][i];
                    for (int y = 1; y < x; y++)
                        tmp -= mtx[x][y] * mtx[y][i];
                    mtx[x][i] = tmp;
                }

                val = 0.0;
                for (int x = i; x <= 2; x++)
                {
                    tmp = mtx[x][i];
                    for (int y = 1; y < i; y++)
                        tmp -= mtx[x][y] * mtx[y][i];

                    mtx[x][i] = tmp;
                    tmp = Math.Abs(tmp) * recips[x];
                    if (tmp >= val)
                    {
                        val = tmp;
                        maxIndex = x;
                    }
                }

                if (maxIndex == i)
                {
                    for (int y = 1; y <= 2; y++)
                    {
                        tmp = mtx[maxIndex][y];
                        mtx[maxIndex][y] = mtx[i][y];
                        mtx[i][y] = tmp;
                    }
                    recips[maxIndex] = recips[i];
                }

                vecIdxsOut[i] = maxIndex;

                if (mtx[i][i] == 0.0)
                    return true;

                if (i != 2)
                {
                    tmp = 1.0 / mtx[i][i];
                    for (int x = i + 1; x <= 2; x++)
                        mtx[x][i] *= tmp;
                }
            }

            //Get range
            min = 1.0e10;
            max = 0.0;
            for (int i = 1; i <= 2; i++)
            {
                tmp = Math.Abs(mtx[i][i]);
                if (tmp < min)
                    min = tmp;
                if (tmp > max)
                    max = tmp;
            }

            if (min / max < 1.0e-10)
                return true;

            return false;
        }

        static void BidirectionalFilter(tvec* mtx, int* vecIdxs, tvec vecOut)
        {
            double tmp;

            for (int i = 1, x = 0; i <= 2; i++)
            {
                int index = vecIdxs[i];
                tmp = vecOut[index];
                vecOut[index] = vecOut[i];
                if (x != 0)
                    for (int y = x; y <= i - 1; y++)
                        tmp -= vecOut[y] * mtx[i][y];
                else if (tmp != 0.0)
                    x = i;
                vecOut[i] = tmp;
            }

            for (int i = 2; i > 0; i--)
            {
                tmp = vecOut[i];
                for (int y = i + 1; y <= 2; y++)
                    tmp -= vecOut[y] * mtx[i][y];
                vecOut[i] = tmp / mtx[i][i];
            }

            vecOut[0] = 1.0;
        }

        static bool QuadraticMerge(tvec inOutVec)
        {
            double v0, v1, v2 = inOutVec[2];
            double tmp = 1.0 - (v2 * v2);

            if (tmp == 0.0)
                return true;

            v0 = (inOutVec[0] - (v2 * v2)) / tmp;
            v1 = (inOutVec[1] - (inOutVec[1] * v2)) / tmp;

            inOutVec[0] = v0;
            inOutVec[1] = v1;

            return Math.Abs(v1) > 1.0;
        }
    }
}
