using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using Ikarus.ModelViewer;
using BrawlLib.SSBBTypes;

namespace Ikarus.MovesetFile
{
    public class SubActionEntry : MovesetEntryNode
    {
        [Category("Sub Action")]
        public AnimationFlags Flags { get { return _flags; } set { _flags = value; SignalPropertyChange(); } }
        [Category("Sub Action")]
        public byte InTranslationTime { get { return _inTransTime; } set { _inTransTime = value; SignalPropertyChange(); } }
        [Category("Sub Action")]
        public int ID { get { return _index; } }

        public SubActionEntry(sSubActionFlags flags, string name, int i)
        {
            _flags = flags._flags;
            _inTransTime = flags._inTranslationTime;
            _stringOffset = flags._stringOffset;
            _animationName = name;
            _index = i;
        }

        public AnimationFlags _flags;
        public byte _inTransTime;
        public string _animationName;
        public int _stringOffset;

        public Script _main;
        public Script _sfx;
        public Script _gfx;
        public Script _other;

        public Script GetWithType(int type)
        {
            switch (type)
            {
                case 0: return _main;
                case 1: return _sfx;
                case 2: return _gfx;
                case 3: return _other;
            }
            return null;
        }
        public void SetWithType(int type, Script s)
        {
            switch (type)
            {
                case 0: _main = s; break;
                case 1: _sfx = s; break;
                case 2: _gfx = s; break;
                case 3: _other = s; break;
            }
        }

        public Script[] GetScriptArray()
        {
            return new Script[] { _main, _sfx, _gfx, _other };
        }

        public override string Name
        {
            get
            {
                return _animationName;
            }
        }

        public override string ToString() { return String.Format("[{0}] {1}", _index.ToString().PadLeft(3), _animationName); }
    }
    public class ActionEntry : MovesetEntryNode
    {
        [Category("Action")]
        public int ID { get { return _id; } }
        [Category("Action")]
        public int Flags1 { get { return _flags1; } }
        [Category("Action")]
        public int Flags2 { get { return _flags2; } }
        [Category("Action")]
        public int Flags3 { get { return _flags3; } }
        [Category("Action")]
        public int Flags4 { get { return _flags4; } }
        
        int _id, _flags1, _flags2, _flags3, _flags4;

        public ActionEntry(sActionFlags flags, int i, int id)
        {
            _flags1 = flags._flags1;
            _flags2 = flags._flags2;
            _flags3 = flags._flags3;
            _flags4 = flags._flags4;
            _index = i;
            _id = id;
        }

        public Script _entry;
        public Script _exit;

        public Script GetWithType(int type)
        {
            switch (type)
            {
                case 0: return _entry;
                case 1: return _exit;
            }
            return null;
        }
        public void SetWithType(int type, Script s)
        {
            switch (type)
            {
                case 0: _entry = s; break;
                case 1: _exit = s; break;
            }
        }

        public override string ToString() { return String.Format("[{0}] Action", ID.ToString().PadLeft(3)); }
    }
    public unsafe class Script : TableEntryNode, IEnumerable<Event>
    {
        #region Child Enumeration

        public IEnumerator<Event> GetEnumerator() { return _children.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        private List<Event> _children = new List<Event>();
        public int Count { get { return _children.Count; } }
        public Event this[int i]
        {
            get
            {
                if (i >= 0 && i < Count)
                    return _children[i];
                return null;
            }
            set
            {
                if (i >= 0 && i < Count)
                {
                    value._root = _root;
                    value._script = this;
                    _children[i] = value;
                    SignalPropertyChange();
                    
                }
            }
        }
        public void Insert(int i, Event e)
        {
            if (i >= 0)
            {
                e._root = _root;
                e._script = this;
                if (i < Count)
                    _children.Insert(i, e);
                else
                    _children.Add(e);
                SignalRebuildChange();
            }
        }
        public void Add(Event e)
        {
            e._root = _root;
            e._script = this;
            _children.Add(e);
            SignalRebuildChange();
        }
        public void RemoveAt(int i)
        {
            if (i >= 0 && i < Count)
            {
                _children.RemoveAt(i);
                SignalRebuildChange();
            }
        }
        public void Clear()
        {
            if (Count != 0)
            {
                _children.Clear();
                SignalRebuildChange();
            }
        }

        #endregion

        public bool _build = false;
        [Category("Script")]
        public bool ForceWrite { get { return _build; } set { _build = value; } }

        public List<MovesetEntryNode> _actionRefs = new List<MovesetEntryNode>();
        public MovesetEntryNode[] ActionRefs { get { return _actionRefs.ToArray(); } }

        public Script(ArticleNode article)
        {
            _build = false;
            _scriptor = new Scriptor(this);
        }
        public Script()
        {
            _build = false;
            _scriptor = new Scriptor(this);
        }

        protected override void OnParse(VoidPtr address)
        {
            _build = true;
            sEvent* ev = (sEvent*)address;
            while (ev->_nameSpace != 0 || ev->_id != 0)
            {
                Event e = Parse<Event>(ev++);
                e._script = this;
                _children.Add(e);
            }
        }

        protected override int OnGetSize()
        {
            _lookupCount = 0;
            int size = 8; //Terminator event size
            foreach (Event e in _children)
            {
                if (e.EventID == 0xFADEF00D || e.EventID == 0xFADE0D8A) continue;
                size += e.GetSize();
                _lookupCount += e._lookupCount;
            }
            return size;
        }

        protected override void OnWrite(VoidPtr address)
        {
            int off = 0;
            foreach (Event e in _children)
                off += e.Count * 8;

            sParameter* paramAddr = (sParameter*)address;
            sEvent* eventAddr = (sEvent*)(address + off);

            RebuildAddress = eventAddr;

            foreach (Event e in _children)
            {
                if (e._name == "FADEF00D" || e._name == "FADE0D8A") continue;
                e.RebuildAddress = eventAddr;
                *eventAddr = new sEvent() { _id = e.ID, _nameSpace = e.NameSpace, _numArguments = (byte)e.Count, _unk1 = e._unknown };
                if (e.Count > 0)
                {
                    eventAddr->_argumentOffset = (uint)Offset(paramAddr);
                    _lookupOffsets.Add(&eventAddr->_argumentOffset);
                }
                else
                    eventAddr->_argumentOffset = 0;
                eventAddr++;
                foreach (Parameter p in e)
                {
                    p.RebuildAddress = paramAddr;
                    if (p.ParamType != ParamType.Offset)
                        *paramAddr = new sParameter() { _type = (int)p.ParamType, _data = p.Data };
                    else
                        SakuraiArchiveNode.Builder._postProcessNodes.Add(p);
                    paramAddr++;
                }
            }

            eventAddr++; //Terminate
        }

        public string MakeScript()
        {
            int tabs = 0;
            string str = "";
            foreach (Event e in this)
            {
                string t = "";

                tabs -= Util.TabDownEvents(e.EventID);
                for (int i = 0; i < tabs; i++) t += "    ";
                tabs += Util.TabUpEvents(e.EventID);

                string f = e.GetFormattedSyntax();
                if (!String.IsNullOrEmpty(f))
                    str += t + f.Replace(Environment.NewLine, Environment.NewLine + t) + Environment.NewLine;
            }
            return str;
        }

        public Scriptor _scriptor;
        internal void FrameAdvance() { _scriptor.FrameAdvance(); }
        public void SetFrame(int index) { _scriptor.SetFrame(index); }
        public void Reset() { _scriptor.Reset(); }

        public override string ToString() { return Name != null ? Name : String.Format("[{0}] Script", Index); }
    }
}
