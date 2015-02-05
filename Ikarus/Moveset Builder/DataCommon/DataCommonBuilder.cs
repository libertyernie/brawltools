using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.OpenGL;
using Ikarus;
using Ikarus.MovesetFile;

namespace Ikarus.MovesetBuilder
{
    public unsafe partial class DataCommonBuilder : BuilderBase
    {
        SakuraiEntryNode[] _orderedDataCommon;
        DataCommonSection _dataCommon;

        public DataCommonBuilder(DataCommonSection dataCommon)
        {
            _moveset = (_dataCommon = dataCommon)._root as MovesetNode;
            _orderedDataCommon = new SakuraiEntryNode[]
            {
                
            };
            _getPartSize = new Action[]
            {
                GetSizePart1,
                GetSizePart2,
                GetSizePart3,
            };
            _buildPart = new Action[]
            {
                BuildPart1,
                BuildPart2,
                BuildPart3,
            };
        }
    }
}