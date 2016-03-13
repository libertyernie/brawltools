using System;
#if RSTMLIB
#else
using BrawlLib.IO;
#endif
using System.Audio;
using BrawlLib.SSBBTypes;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BrawlLib.Wii.Audio
{
    public static class CSTMConverter
    {
#if RSTMLIB
        public static unsafe byte[] EncodeToByteArray(IAudioStream stream, IProgressTracker progress)
#else
        public static unsafe FileMap Encode(IAudioStream stream, IProgressTracker progress)
#endif
        {
            int tmp;
            bool looped = stream.IsLooping;
            int channels = stream.Channels;
            int samples;
            int blocks;
            int sampleRate = stream.Frequency;
            int lbSamples, lbSize, lbTotal;
            int loopPadding, loopStart, totalSamples;
            short* tPtr;

            if (looped)
            {
                loopStart = stream.LoopStartSample;
                samples = stream.LoopEndSample; //Set sample size to end sample. That way the audio gets cut off when encoding.

                //If loop point doesn't land on a block, pad the stream so that it does.
                if ((tmp = loopStart % 0x3800) != 0)
                {
                    loopPadding = 0x3800 - tmp;
                    loopStart += loopPadding;
                }
                else
                    loopPadding = 0;
                
                totalSamples = loopPadding + samples;
            }
            else
            {
                loopPadding = loopStart = 0;
                totalSamples = samples = stream.Samples;
            }

            if (progress != null)
                progress.Begin(0, totalSamples * channels * 3, 0);

            blocks = (totalSamples + 0x37FF) / 0x3800;

            //Initialize stream info
            if ((tmp = totalSamples % 0x3800) != 0)
            {
                lbSamples = tmp;
                lbSize = (lbSamples + 13) / 14 * 8;
                lbTotal = lbSize.Align(0x20);
            }
            else
            {
                lbSamples = 0x3800;
                lbTotal = lbSize = 0x2000;
            }

            //Get section sizes - these were copied from the RSTM encoder and may not hold up for files with >2 channels
            int rstmSize = 0x40;
            int infoSize = (0x68 + (channels * 0x40)).Align(0x20);
            int seekSize = ((blocks - 1) * 4 * channels + 0x10).Align(0x20);
            int dataSize = ((blocks - 1) * 0x2000 + lbTotal) * channels + 0x20;

#if RSTMLIB
            //Create byte array
            byte[] array = new byte[rstmSize + infoSize + seekSize + dataSize];
            fixed (byte* address = array) {
#else
            //Create file map
            FileMap map = FileMap.FromTempFile(rstmSize + infoSize + seekSize + dataSize);
            VoidPtr address = map.Address;
#endif

            //Get section pointers
            CSTMHeader* rstm = (CSTMHeader*)address;
            CSTMINFOHeader* info = (CSTMINFOHeader*)((byte*)rstm + rstmSize);
            CSTMSEEKHeader* seek = (CSTMSEEKHeader*)((byte*)info + infoSize);
            CSTMDATAHeader* data = (CSTMDATAHeader*)((byte*)seek + seekSize);

            //Initialize sections
            rstm->Set(infoSize, seekSize, dataSize);
            info->Set(infoSize, channels);
            seek->Set(seekSize);
            data->Set(dataSize);

            //Set HEAD data
            info->_dataInfo._format = new AudioFormatInfo(2, (byte)(looped ? 1 : 0), (byte)channels, 0);
            info->_dataInfo._sampleRate = (ushort)sampleRate;
            info->_dataInfo._loopStartSample = loopStart;
            info->_dataInfo._numSamples = totalSamples;
            info->_dataInfo._sampleDataRef._type = CSTMReference.RefType.SampleData;
            info->_dataInfo._sampleDataRef._dataOffset = 0x18;
            info->_dataInfo._numBlocks = blocks;
            info->_dataInfo._blockSize = 0x2000;
            info->_dataInfo._samplesPerBlock = 0x3800;
            info->_dataInfo._lastBlockSize = lbSize;
            info->_dataInfo._lastBlockSamples = lbSamples;
            info->_dataInfo._lastBlockTotal = lbTotal;
            info->_dataInfo._dataInterval = 0x3800;
            info->_dataInfo._bitsPerSample = 4;
            
            //Create one ADPCMInfo for each channel
            int* adpcData = stackalloc int[channels];
            ADPCMInfo_LE** pAdpcm = (ADPCMInfo_LE**)adpcData;
            for (int i = 0; i < channels; i++)
                *(pAdpcm[i] = info->GetChannelInfo(i)) = new ADPCMInfo_LE() { _pad = 0 };

            //Create buffer for each channel
            int* bufferData = stackalloc int[channels];
            short** channelBuffers = (short**)bufferData;
            int bufferSamples = totalSamples + 2; //Add two samples for initial yn values
            for (int i = 0; i < channels; i++)
            {
                channelBuffers[i] = tPtr = (short*)Marshal.AllocHGlobal(bufferSamples * 2); //Two bytes per sample

                //Zero padding samples and initial yn values
                for (int x = 0; x < (loopPadding + 2); x++)
                    *tPtr++ = 0;
            }

            //Fill buffers
            stream.SamplePosition = 0;
            short* sampleBuffer = stackalloc short[channels];

            for (int i = 2; i < bufferSamples; i++)
            {
                if (stream.SamplePosition == stream.LoopEndSample && looped)
                    stream.SamplePosition = stream.LoopStartSample;

                stream.ReadSamples(sampleBuffer, 1);
                for (int x = 0; x < channels; x++)
                    channelBuffers[x][i] = sampleBuffer[x];
            }

            //Calculate coefs
            for (int i = 0; i < channels; i++)
            {
                AudioConverter.CalcCoefs(channelBuffers[i] + 2, totalSamples, (short*)pAdpcm[i], progress);

                //short* from = (short*)pAdpcm[i];
                //bshort* to = (bshort*)from;
                //for (int j=0; j<16; j++) { 
                //    *(to++) = *(from++);
                //}
            }

            //Encode blocks
            byte* dPtr = (byte*)data->Data;
            short* pyn = (short*)seek->Data;
            for (int sIndex = 0, bIndex = 1; sIndex < totalSamples; sIndex += 0x3800, bIndex++)
            {
                int blockSamples = Math.Min(totalSamples - sIndex, 0x3800);
                for (int x = 0; x < channels; x++)
                {
                    short* sPtr = channelBuffers[x] + sIndex;

                    //Set block yn values
                    if (bIndex != blocks)
                    {
                        *pyn++ = sPtr[0x3801];
                        *pyn++ = sPtr[0x3800];
                    }

                    //Encode block (include yn in sPtr)
                    AudioConverter.EncodeBlock(sPtr, blockSamples, dPtr, (short*)pAdpcm[x]);

                    //Set initial ps
                    if (bIndex == 1)
                        pAdpcm[x]->_ps = *dPtr;

                    //Advance output pointer
                    if (bIndex == blocks)
                    {
                        //Fill remaining
                        dPtr += lbSize;
                        for (int i = lbSize; i < lbTotal; i++)
                            *dPtr++ = 0;
                    }
                    else
                        dPtr += 0x2000;
                }

                if (progress != null)
                {
                    if ((sIndex % 0x3800) == 0)
                        progress.Update(progress.CurrentValue + (0x7000 * channels));
                }
            }

            //Reverse coefs
            //for (int i = 0; i < channels; i++)
            //{
            //    short* p = pAdpcm[i]->_coefs;
            //    for (int x = 0; x < 16; x++, p++)
            //        *p = p->Reverse();
            //}

            //Write loop states
            if (looped)
            {
                //Can't we just use block states?
                int loopBlock = loopStart / 0x3800;
                int loopChunk = (loopStart - (loopBlock * 0x3800)) / 14;
                dPtr = (byte*)data->Data + (loopBlock * 0x2000 * channels) + (loopChunk * 8);
                tmp = (loopBlock == blocks - 1) ? lbTotal : 0x2000;

                for (int i = 0; i < channels; i++, dPtr += tmp)
                {
                    //Use adjusted samples for yn values
                    tPtr = channelBuffers[i] + loopStart;
                    pAdpcm[i]->_lps = *dPtr;
                    pAdpcm[i]->_lyn2 = *tPtr++;
                    pAdpcm[i]->_lyn1 = *tPtr;
                }
            }

            //Free memory
            for (int i = 0; i < channels; i++)
                Marshal.FreeHGlobal((IntPtr)channelBuffers[i]);

            if (progress != null)
                progress.Finish();

#if RSTMLIB
            }
            return array;
#else
            return map;
#endif
        }
    }
}
