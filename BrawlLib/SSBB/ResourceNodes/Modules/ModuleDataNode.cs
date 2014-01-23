using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.IO;
using System.ComponentModel;
using System.PowerPcAssembly;
using System.Drawing;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{   
    public unsafe class ModuleDataNode : RELEntryNode
    {
        [Browsable(false)]
        public virtual bool HasCode { get { return true; } }
        [Browsable(false)]
        public RelCommand First { get { return _firstCommand; } }
        [Browsable(false)]
        public virtual uint ASMOffset { get { return _offset; } }

        public SectionEditor _linkedEditor = null;
        internal UnsafeBuffer _dataBuffer;
        public List<string>[] _tags;
        public RelCommand _firstCommand = null;

        public override void Dispose()
        {
            if (_dataBuffer != null)
            {
                _dataBuffer.Dispose();
                _dataBuffer = null;
            }
            base.Dispose();
        }

        public List<Relocation> _relocations;
        [Browsable(false)]
        public List<Relocation> Relocations { get { return _linkedEditor != null ? _linkedEditor._relocations : _relocations; } set { _relocations = value; } }
        
        [Browsable(false)]
        public buint* BufferAddress { get { return (buint*)_dataBuffer.Address; } }

        public Relocation this[int index] 
        {
            get
            {
                List<Relocation> l = _linkedEditor != null ? _linkedEditor._relocations : _relocations;

                if (index < l.Count && index >= 0)
                    return l[index]; 
                return null;
            }
            set
            {
                List<Relocation> l = _linkedEditor != null ? _linkedEditor._relocations : _relocations;

                if (index < l.Count && index >= 0)
                    l[index] = value;
            }
        }

        /// <summary>
        /// Fills the data buffer with the specified amount of data from an address.
        /// </summary>
        public void InitBuffer(uint size, VoidPtr address)
        {
            _dataBuffer = new UnsafeBuffer((int)size.RoundUp(4));
            _relocations = new List<Relocation>();

            for (int x = 0; x < _dataBuffer.Length / 4; x++)
                _relocations.Add(new Relocation(this, x));

            Memory.Move(_dataBuffer.Address, address, size);
        }
        /// <summary>
        /// Fills the data buffer with the specified amount of zerobytes.
        /// </summary>
        public void InitBuffer(uint size)
        {
            _dataBuffer = new UnsafeBuffer((int)size.RoundUp(4));
            _relocations = new List<Relocation>();

            for (int x = 0; x < _dataBuffer.Length / 4; x++)
                _relocations.Add(new Relocation(this, x));

            Memory.Fill(_dataBuffer.Address, size, 0);
        }

        public byte[] GetInitializedBuffer()
        {
            List<byte> bytes = new List<byte>();
            uint value;
            foreach (Relocation loc in Relocations)
            {
                value = loc.SectionOffset;
                bytes.Add((byte)((value >> 24) & 0xFF));
                bytes.Add((byte)((value >> 16) & 0xFF));
                bytes.Add((byte)((value >> 08) & 0xFF));
                bytes.Add((byte)((value >> 00) & 0xFF));
            }
            return bytes.ToArray();
        }

        //private void ApplyTags()
        //{
        //    if (HasCode)
        //    {
        //        int i = 0;
        //        foreach (Relocation r in Relocations)
        //        {
        //            PPCOpCode op = r.Code;
        //            if (op is BranchOpcode)
        //            {
        //                BranchOpcode b = op as BranchOpcode;
        //                if (!b.Absolute)
        //                {
        //                    int offset = b.Offset;
        //                    int iOff = offset.RoundDown(4) / 4;
        //                    int index = i + iOff;
        //                    if (index >= 0 && index < _relocations.Count)
        //                        Relocations[index].Tags.Add(String.Format("Sub 0x{0}", PPCFormat.Hex(ASMOffset + (uint)i * 4)));
        //                }
        //            }
        //            i++;
        //        }
        //    }
        //}

        //public void RemoveAtIndex(int index)
        //{
        //    if (_dataBuffer.Length < 4)
        //        return;

        //    UnsafeBuffer newBuffer = new UnsafeBuffer(_dataBuffer.Length - 4);
        //    _relocations.RemoveAt(index);

        //    for (int i = index; i < _relocations.Count; i++)
        //    {
        //        Relocation r = _relocations[i];
        //        foreach (Relocation l in r.Linked)
        //            if (l.Command != null && l.Command.TargetRelocation._section == this)
        //                l.Command._addend -= 4;
        //        r._index--;
        //    }

        //    int offset = index * 4;

        //    //Move memory before the removed value
        //    if (offset > 0)
        //        Memory.Move(newBuffer.Address, _dataBuffer.Address, (uint)offset);

        //    //Move memory after the removed value
        //    if (offset + 4 < _dataBuffer.Length)
        //        Memory.Move(newBuffer.Address + offset, _dataBuffer.Address + offset + 4, (uint)_dataBuffer.Length - (uint)(offset + 4));

        //    _dataBuffer.Dispose();
        //    _dataBuffer = newBuffer;
        //}

        //public void InsertAtIndex(int index, Relocation r)
        //{
        //    UnsafeBuffer newBuffer = new UnsafeBuffer(_dataBuffer.Length + 4);
        //    _relocations.Insert(index, r);
        //    r._index = index;

        //    for (int i = index + 1; i < _relocations.Count; i++)
        //    {
        //        Relocation e = _relocations[i];
        //        foreach (Relocation l in e.Linked)
        //            if (l.Command != null && l.Command.TargetRelocation._section == this)
        //                l.Command._addend += 4;
        //        e._index++;
        //    }

        //    int offset = index * 4;

        //    //Move memory before the inserted value
        //    if (offset > 0)
        //        Memory.Move(newBuffer.Address, _dataBuffer.Address, (uint)offset);

        //    //Move memory after the inserted value
        //    if (offset + 4 < _dataBuffer.Length)
        //        Memory.Move(newBuffer.Address + offset + 4, _dataBuffer.Address + offset + 4, (uint)_dataBuffer.Length - (uint)(offset + 4));

        //    //Clear the new value
        //    *(uint*)(newBuffer.Address + offset) = 0;

        //    _dataBuffer.Dispose();
        //    _dataBuffer = newBuffer;
        //}

        public void Resize(int newSize)
        {
            int diff = (newSize.RoundDown(4) - _dataBuffer.Length) / 4;
            if (diff == 0)
                return;
            if (diff > 0)
                for (int i = 0; i < diff; i++)
                    _relocations.Add(new Relocation(this, _relocations.Count));
            else if (diff < 0)
                _relocations.RemoveRange(_relocations.Count + diff, -diff);
            
            UnsafeBuffer newBuffer = new UnsafeBuffer(newSize);
            int max = Math.Min(_dataBuffer.Length, newBuffer.Length);
            if (max > 0)
                Memory.Move(newBuffer.Address, _dataBuffer.Address, (uint)max);

            _dataBuffer.Dispose();
            _dataBuffer = newBuffer;
        }

        #region Command Functions

        public void ClearCommands()
        {
            RelCommand c = _firstCommand;
            while (c != null)
            {
                c._parentRelocation.Command = null;
                c = c._next;
            }
        }

        public void GetFirstCommand() { _firstCommand = GetCommandAfter(-1); }

        public RelCommand GetCommandAfter(int startIndex)
        {
            int i = GetIndexOfCommandAfter(startIndex);
            if (i >= 0 && i < Relocations.Count)
                return Relocations[i].Command;
            return null;
        }

        public RelCommand GetCommandBefore(int startIndex)
        {
            int i = GetIndexOfCommandBefore(startIndex);
            if (i >= 0 && i < Relocations.Count)
                return Relocations[i].Command;
            return null;
        }

        public int GetIndexOfCommandBefore(int startIndex)
        {
            if (startIndex < 0)
                return -1;

            if (startIndex > Relocations.Count)
                startIndex = Relocations.Count;

            for (int i = startIndex - 1; i >= 0; i--)
                if (i < Relocations.Count && Relocations[i].Command != null)
                    return i;

            return -1;
        }

        public int GetIndexOfCommandAfter(int startIndex)
        {
            if (startIndex >= Relocations.Count || startIndex < -1)
                return -1;

            for (int i = startIndex + 1; i < Relocations.Count - 1; i++)
                if (i < Relocations.Count && Relocations[i].Command != null)
                    return i;

            return -1;
        }

        public RelCommand GetCommandFromOffset(int offset) { return GetCommandFromIndex(offset.RoundDown(4) / 4); }
        public RelCommand GetCommandFromIndex(int index) { if (index < Relocations.Count && index >= 0) return _relocations[index].Command; return null; }

        public void SetCommandAtOffset(int offset, RelCommand cmd) { SetCommandAtIndex(offset.RoundDown(4) / 4, cmd); }
        public void SetCommandAtIndex(int index, RelCommand cmd)
        {
            if (index >= Relocations.Count || index < 0)
                return;

            if (Relocations[index].Command != null)
                Relocations[index].Command.Remove();

            Relocations[index].Command = cmd;

            RelCommand c = GetCommandBefore(index);
            if (c != null)
                cmd.InsertAfter(c);
            else
            {
                c = GetCommandAfter(index);
                if (c != null)
                    cmd.InsertBefore(c);
            }
            GetFirstCommand();
        }

        #endregion

        #region Relocation Functions

        public Relocation GetRelocationAtOffset(int offset) { return this[offset.RoundDown(4) / 4]; }
        public Relocation GetRelocationAtIndex(int index) { return this[index]; }

        public void SetRelocationAtOffset(int offset, Relocation value) { this[offset.RoundDown(4) / 4] = value; }
        public void SetRelocationAtIndex(int index, Relocation value) { this[index] = value; }

        public static Color clrNotRelocated = Color.FromArgb(255, 255, 255);
        public static Color clrRelocated = Color.FromArgb(200, 255, 200);
        public static Color clrBadRelocate = Color.FromArgb(255, 200, 200);
        public static Color clrBlr = Color.FromArgb(255, 255, 100);

        public Color GetStatusColorFromOffset(int offset) { return GetStatusColorFromIndex(offset.RoundDown(4) / 4); }
        public Color GetStatusColorFromIndex(int index) { return GetStatusColor(this[index]); }
        public Color GetStatusColor(Relocation c)
        {
            if (c.Code is PPCblr)
                return clrBlr;
            if (c.Command == null)
                return clrNotRelocated;
            return clrRelocated;
        }

        #endregion
    }
}
