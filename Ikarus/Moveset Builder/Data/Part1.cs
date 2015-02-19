using BrawlLib.SSBBTypes;
using Ikarus.MovesetFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ikarus.MovesetBuilder
{
    public unsafe partial class DataBuilder : BuilderBase
    {
        private void GetSizePart1()
        {
            GetSize(_data._attributes);
            GetSize(_data._sseAttributes);
            GetSize(_data._commonActionFlags);
            GetSize(_data._unknown7);

            GetSize(_misc._finalSmashAura);

            foreach (SubActionEntry g in _data._subActions)
            {
                //Don't add size if subaction is null
                if (g.Name != "<null>")
                    foreach (Script a in g.GetScriptArray())
                    {
                        if (a.Count > 0 || a._actionRefs.Count > 0 || a._build)
                        {
                            GetSize(a);
                            _lookup++;
                        }
                    }
            }

            if (_misc._soundData != null)
            {

            }
        }

        private void BuildPart1()
        {

        }
    }
}
