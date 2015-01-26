using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ikarus.MovesetBuilder
{
    public abstract class BuilderBase
    {
        public BuilderBase() { }

        public int _size;

        public virtual int CalcSize() { return 0; }
        public virtual void Build(VoidPtr address) { }
    }
}
