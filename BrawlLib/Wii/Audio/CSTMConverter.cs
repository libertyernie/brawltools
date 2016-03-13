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
        internal static unsafe byte[] FromRSTM(RSTMHeader* rstm)
        {
            StrmDataInfo strmDataInfo = *rstm->HEADData->Part1;
            int channels = strmDataInfo._format._channels;

            // Get section sizes from the BRSTM - BCSTM is such a similar format that we can assume the sizes will match.
            int rstmSize = 0x40;
            int infoSize = rstm->_headLength;
            int seekSize = rstm->_adpcLength;
            int dataSize = rstm->_dataLength;

            //Create byte array
            byte[] array = new byte[rstmSize + infoSize + seekSize + dataSize];

            fixed (byte* address = array) {
                //Get section pointers
                CSTMHeader* cstm = (CSTMHeader*)address;
                CSTMINFOHeader* info = (CSTMINFOHeader*)((byte*)cstm + rstmSize);
                CSTMSEEKHeader* seek = (CSTMSEEKHeader*)((byte*)info + infoSize);
                CSTMDATAHeader* data = (CSTMDATAHeader*)((byte*)seek + seekSize);

                //Initialize sections
                cstm->Set(infoSize, seekSize, dataSize);
                info->Set(infoSize, channels);
                seek->Set(seekSize);
                data->Set(rstm->DATAData->_length);

                //Set HEAD data
                info->_dataInfo = new CSTMDataInfo(strmDataInfo);

                //Create one ADPCMInfo for each channel
                IntPtr* adpcData = stackalloc IntPtr[channels];
                ADPCMInfo_LE** pAdpcm = (ADPCMInfo_LE**)adpcData;
                for (int i = 0; i < channels; i++)
                    *(pAdpcm[i] = info->GetChannelInfo(i)) = new ADPCMInfo_LE(*rstm->HEADData->GetChannelInfo(i));

                bshort* seekFrom = (bshort*)rstm->ADPCData->Data;
                short* seekTo = (short*)seek->Data;
                for (int i = 0; i < seek->_length / 2 - 8; i++)
                {
                    *(seekTo++) = *(seekFrom++);
                }

                VoidPtr dataFrom = rstm->DATAData->Data;
                VoidPtr dataTo = data->Data;
                Memory.Move(dataTo, dataFrom, (uint)data->_length - 8);
            }
            return array;
        }

        internal static unsafe byte[] ToRSTM(CSTMHeader* cstm)
        {
            throw new NotImplementedException();
        }

#if RSTMLIB
#else
        public static unsafe FileMap Encode(IAudioStream stream, IProgressTracker progress)
        {
            using (FileMap rstmMap = RSTMConverter.Encode(stream, progress))
            {
                byte[] cstmArray = FromRSTM((RSTMHeader*)rstmMap.Address);
                FileMap newMap = FileMap.FromTempFile(cstmArray.Length);
                Marshal.Copy(cstmArray, 0, newMap.Address, cstmArray.Length);
                return newMap;
            }
        }
    }
#endif
}
