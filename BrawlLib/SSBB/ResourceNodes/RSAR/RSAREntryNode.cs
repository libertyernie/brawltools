using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.Wii.Audio;
using System.ComponentModel;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RSAREntryNode : ResourceNode
    {
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
        internal virtual int StringId { get { return 0; } }

        public int InfoIndex { get { return _infoIndex; } }
        public int _infoIndex;
        internal VoidPtr Data { get { return (VoidPtr)WorkingUncompressed.Address; } }

        [Category("Data"), Browsable(true)]
        public string DataOffset { get { if (RSARNode != null) return ((uint)(Data - (VoidPtr)RSARNode.Header)).ToString("X"); else return null; } }
        
        public VoidPtr _rebuildBase;
        public int _rebuildIndex, _rebuildStringId;

        public override bool OnInitialize()
        {
            if (_name == null)
            {
                RSARNode p = RSARNode;
                if (p != null)
                    _name = p.Header->SYMBBlock->GetStringEntry(StringId);
                else
                    _name = String.Format("Entry{0}", StringId);
            }

            return false;
        }

        internal string _fullPath;
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

            if (len != 0)
                _fullPath = new String(chars, 0, len);
            else
                _fullPath = "";

            list.AddEntry(_fullPath, this);
        }
    }
}
