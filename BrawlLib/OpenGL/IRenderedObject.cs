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
        void Refresh();
        void Render(params object[] args);
        Box GetBox();
    }
}