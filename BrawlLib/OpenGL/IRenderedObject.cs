using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrawlLib.OpenGL
{
    public interface IRenderedObject
    {
        void Attach(TKContext ctx);
        void Detach();
        void Refesh();
        void Render(TKContext ctx, ModelPanel mainWindow);
        void GetBox(out Vector3 min, out Vector3 max);
    }
}
