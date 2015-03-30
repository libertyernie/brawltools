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
        DataSection _data;
        MiscSectionNode _misc;

        public DataBuilder(DataSection data)
        {
            _moveset = (_data = data)._root as MovesetNode;
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

        enum ArticleType
        {
            Entry,
            Static,
            Extra,
        }

        private int CalcSize(ArticleNode d, ArticleType type, bool subactions, int index)
        {
            int size = 0;

            switch (type)
            {
                case ArticleType.Static:
                case ArticleType.Entry:
                    if (!subactions)
                    {
                        if (d._actions != null)
                            foreach (MoveDefActionNode a in d._actions)
                                if (a.Children.Count > 0)
                                    size += GetSize(a, ref lookupCount);
                    }
                    else
                    {
                        if (d._subActions != null)
                            foreach (MoveDefSubActionGroupNode grp in d.subActions.Children)
                                if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as MoveDefActionNode)._actionRefs.Count > 0 || (grp.Children[index] as MoveDefActionNode)._build)
                                    size += GetSize((grp.Children[index] as MoveDefActionNode), ref lookupCount);
                    }
                    break;
            }

            return size;
        }

        public int CalcSizeArticleActions(bool subactions, int index)
        {
            int size = 0;
            if (_data._staticArticles != null)
                foreach (ArticleNode d in _data._staticArticles)
                    size += CalcSize(d, ArticleType.Static, subactions, index);

            if (_data._entryArticle != null)
                size += CalcSize(_data._entryArticle, ArticleType.Entry, subactions, index);

            foreach (ArticleNode d in _data._articles)
                if (!subactions)
                {
                    if (d._actions != null)
                    {
                        //if (d._pikmin)
                        //{
                        //    foreach (MoveDefActionGroupNode grp in d.actions.Children)
                        //        foreach (MoveDefActionNode a in grp.Children)
                        //            if (a.Children.Count > 0)
                        //                size += GetSize(a, ref lookupCount);
                        //}
                        //else
                            foreach (ActionEntry a in d._actions)
                                if (a.Children.Count > 0)
                                    size += GetSize(a, ref lookupCount);
                    }
                }
                else
                {
                    if (d._subActions != null)
                    {
                        var e = d._subActions;
                        int populateCount = 1;
                        bool children = false;
                        if (e.Children[0] is MoveDefActionListNode)
                        {
                            populateCount = d.subActions.Children.Count;
                            children = true;
                        }
                        for (int i = 0; i < populateCount; i++)
                        {
                            if (children)
                                e = d.subActions.Children[i] as MoveDefEntryNode;

                            foreach (MoveDefSubActionGroupNode grp in e.Children)
                                if (grp.Children[index].Children.Count > 0 || (grp.Children[index] as MoveDefActionNode)._actionRefs.Count > 0 || (grp.Children[index] as MoveDefActionNode)._build)
                                    size += GetSize((grp.Children[index] as MoveDefActionNode), ref lookupCount);
                        }
                    }
                }

            return size;
        }
    }
}