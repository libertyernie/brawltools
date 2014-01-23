using System;
namespace System.Audio
{
    public interface IAudioStream: IDisposable
    {
        WaveFormatTag Format { get; }
        int BitsPerSample { get; }
        int Samples { get; }
        int Channels { get; }
        int Frequency { get; }
        bool IsLooping { get; set; }
        int LoopStartSample { get; set; }
        int LoopEndSample { get; set; }
        int SamplePosition { get; set; }

        //Reads numSamples audio samples into the address specified by destAddr.
        //Returns the actual number of samples read.
        //Cannot loop automatically, because sample offsets would then be incorrect.
        int ReadSamples(VoidPtr destAddr, int numSamples);

        //Wraps the stream to the loop context.
        //Must be used manually in order to track stream state. (Just good coding practice)
        void Wrap();
    }
}
