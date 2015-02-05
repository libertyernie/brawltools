using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Ikarus.MovesetFile;
using BrawlLib;
using BrawlLib.SSBBTypes;

namespace Ikarus.MovesetBuilder
{
    //TODO: program something that will compare all read entries of each moveset,
    //compare their offsets, determine where each one needs to be written,
    //and write them out in order, 
    //not including repetitive child entries that already appear before their parent

    public unsafe partial class DataBuilder : BuilderBase
    {
        SakuraiEntryNode[] _orderedData;
        SakuraiEntryNode[] _orderedMisc;

        DataSection _data;
        MiscSectionNode _misc;

        public DataBuilder(DataSection data)
        {
            _moveset = (_data = data)._root as MovesetNode;
            _orderedData = new SakuraiEntryNode[]
            {
                _data._attributes,
                _data._sseAttributes,
                _data._commonActionFlags,
                _data._unknown7,
            };
            if (_data._misc != null)
            {
                _misc = _data._misc;
                _orderedMisc = new SakuraiEntryNode[]
                {
                    _misc._finalSmashAura,
                    
                };
            }
            _getPartSize = new Action[]
            {
                GetSizePart1,
                GetSizePart2,
                GetSizePart3,
                GetSizePart4,
                GetSizePart5,
                GetSizePart6,
                GetSizePart7,
            };
            _buildPart = new Action[]
            {
                BuildPart1,
                BuildPart2,
                BuildPart3,
                BuildPart4,
                BuildPart5,
                BuildPart6,
                BuildPart7,
            };
        }
    }
}