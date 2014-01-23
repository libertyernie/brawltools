using System;
using System.Collections.Generic;
using BrawlLib.Wii.Audio;
using BrawlLib.SSBBTypes;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSARFolderNode : ResourceNode
    {
        internal RSARHeader* Header { get { return (RSARHeader*)WorkingSource.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RSARFolder; } }

        public int _listIndex;

        [Browsable(false)]
        public RSARNode RSARNode
        {
            get
            {
                ResourceNode n = this;
                while (((n = n.Parent) != null) && !(n is RSARNode)) ;
                return n as RSARNode;
            }
        }

        public override void AddChild(ResourceNode child)
        {
            base.AddChild(child);
            //Sort(false);
        }

        internal void Sort(bool sortChildren)
        {
            if (_children != null)
            {
                _children.Sort(RSARNode.CompareNodes);
                if (sortChildren)
                    foreach (ResourceNode n in _children)
                        if (n is RSARFolderNode)
                            ((RSARFolderNode)n).Sort(true);
            }
        }

        //public override bool OnInitialize()
        //{
        //    switch (Index)
        //    {
        //        case 0:
        //            _name = "Types";
        //            _listIndex = 2;
        //            break;
        //        case 1:
        //            _name = "Files";
        //            _listIndex = 3;
        //            break;
        //        case 2:
        //            _name = "Groups";
        //            _listIndex = 4;
        //            break;
        //    }
        //    return true;
        //}

        //public override void OnPopulate()
        //{
        //    RSARHeader* rsar = Header;
        //    INFOHeader* info = Header->INFOBlock;
        //    VoidPtr offset = &info->_collection;
        //    RuintList* list = (RuintList*)info->_collection[_listIndex];
        //    int count = list->_numEntries;

        //    Type t;
        //    switch (_listIndex)
        //    {
        //        case 2:
        //            t = typeof(RSARTypeNode);
        //            break; //Types

        //        case 3: //Files
        //            INFOFileHeader* fileHeader;
        //            INFOFileEntry* fileEntry;
        //            RuintList* entryList;
        //            INFOGroupHeader* group;
        //            INFOGroupEntry* gEntry;
        //            RuintList* groupList = info->Groups;
        //            RSARFileNode n;
        //            DataSource source;

        //            for (int i = 0; i < count; i++)
        //            {
        //                fileHeader = (INFOFileHeader*)list->Get(offset, i);
        //                entryList = fileHeader->GetList(offset);
        //                if (entryList->_numEntries == 0)
        //                {
        //                    //Must be external file.
        //                    n = new RSARExtFileNode();
        //                    n._fileIndex = i;
        //                    n.Initialize(this, fileHeader, 0);
        //                }
        //                else
        //                {
        //                    //Use first entry
        //                    fileEntry = (INFOFileEntry*)entryList->Get(offset, 0);
        //                    //Find group with matching ID
        //                    group = (INFOGroupHeader*)groupList->Get(offset, fileEntry->_groupId);
        //                    //Find group entry with matching index
        //                    gEntry = (INFOGroupEntry*)group->GetCollection(offset)->Get(offset, fileEntry->_index);

        //                    //Create node and parse
        //                    source = new DataSource((int)rsar + group->_headerOffset + gEntry->_headerOffset, gEntry->_headerLength);
        //                    if ((n = NodeFactory.GetRaw(source) as RSARFileNode) == null)
        //                        n = new RSARFileNode();
                            
        //                    n._audioSource = new DataSource((int)rsar + group->_waveDataOffset + gEntry->_dataOffset, gEntry->_dataLength);
        //                    n._fileIndex = i;
        //                    n.Initialize(this, source);
        //                }
        //            }

        //            return;

        //        case 4:
        //            t = typeof(RSARGroupNode);
        //            break; //Groups
        //        default:
        //            return;
        //    }


        //    for (int i = 0; i < count; i++)
        //    {
        //        ResourceNode node = Activator.CreateInstance(t) as ResourceNode;
        //        node.Initialize(this, list->Get(offset, i), 0);
        //    }

        //    base.OnPopulate();
        //}

        internal virtual void GetStrings(sbyte* path, int pathLen, RSAREntryList list)
        {
            int len = _name.Length;
            int i = 0;
            if (len == 0)
                return;

            len += pathLen + ((pathLen != 0) ? 1 : 0);

            sbyte* chars = stackalloc sbyte[len];

            if (pathLen > 0)
            {
                while (i < pathLen)
                    chars[i++] = *path++;
                chars[i++] = (sbyte)'_';
            }

            fixed (char* s = _name)
                for (int x = 0; i < len; )
                    chars[i++] = (sbyte)s[x++];

            foreach (ResourceNode n in Children)
            {
                if (n is RSARFolderNode)
                    ((RSARFolderNode)n).GetStrings(chars, len, list);
                else if (n is RSAREntryNode)
                    ((RSAREntryNode)n).GetStrings(chars, len, list);
            }
        }
    }
}
