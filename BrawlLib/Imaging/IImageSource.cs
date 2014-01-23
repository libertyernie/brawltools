using System;
using System.Drawing;

namespace BrawlLib.Imaging
{
    public interface IImageSource
    {
        int ImageCount { get; }
        Bitmap GetImage(int index);
    }
}
