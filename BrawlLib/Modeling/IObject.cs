using BrawlLib.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.Modeling
{
    public interface IObject : IRenderedObject
    {
        PrimitiveManager PrimitiveManager { get; }
    }
}
