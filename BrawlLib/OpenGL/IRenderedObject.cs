using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrawlLib.OpenGL
{
    public interface IRenderedObject
    {
        bool IsRendering { get; set; }
        bool Attached { get; }
        void Attach();
        void Detach();
        void Refesh();
        void Render(params object[] args);
        void GetBox(out Vector3 min, out Vector3 max);
    }
}