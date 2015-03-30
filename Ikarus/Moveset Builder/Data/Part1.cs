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

            //Add subaction data size (not including offset arrays to the data)
            foreach (SubActionEntry g in _data._subActions)
                if (g.Name != "<null>")
                    foreach (Script c in g.GetScriptArray())
                        GetScriptSize(c);

            CalcSizeArticleActions(true, 0); //Main
            CalcSizeArticleActions(true, 1); //GFX
            CalcSizeArticleActions(true, 2); //SFX

            GetSize(_misc._finalSmashAura);

            if (_misc._soundData != null)
                foreach (var r in _misc._soundData._entries)
                    AddSize(r._entries.Count * 4);

            foreach (ActionEntry a in _moveset._actions)
            {
                GetScriptSize(a._entry);
                GetScriptSize(a._exit);
            }
        }

        private void BuildPart1()
        {

        }
    }
}
