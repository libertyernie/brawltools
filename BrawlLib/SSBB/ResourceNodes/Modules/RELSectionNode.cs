using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using BrawlLib.IO;
using System.PowerPcAssembly;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class ModuleSectionNode : ModuleDataNode
    {
        internal VoidPtr Header { get { return WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.RELSection; } }
        
        [Browsable(false)]
        public override uint ASMOffset { get { return (uint)_dataOffset; } }

        public bool _isCodeSection = false;
        public bool _isBSSSection = false;
        public int _dataOffset = 0;
        public uint _dataSize;

        [Category("REL Section")]
        public bool HasCommands { get { return First != null; } }
        [Category("REL Section"), Browsable(true)]
        public override bool HasCode { get { return _isCodeSection; } }
        [Category("REL Section")]
        public bool IsBSS { get { return _isBSSSection; } }
        //[Category("REL Section")]
        //public int Offset { get { return _dataOffset; } }
        //[Category("REL Section")]
        //public uint Size { get { return _dataSize; } }

        public ModuleSectionNode() { }
        public ModuleSectionNode(uint size) { InitBuffer(size); }

        public override bool OnInitialize()
        {
            if (_name == null)
                _name = String.Format("Section[{0}] ", Index);

            if (_dataOffset == 0 && WorkingUncompressed.Length != 0)
            {
                _isBSSSection = true;
                InitBuffer(_dataSize);
            }
            else
            {
                _isBSSSection = false;
                InitBuffer(_dataSize, Header);
            }

            return false;
        }

        public override int OnCalculateSize(bool force)
        {
            return _dataBuffer.Length;
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            Memory.Move(address, _dataBuffer.Address, (uint)length);
        }

        public override void Dispose()
        {
            if (_dataBuffer != null)
                _dataBuffer.Dispose();

            base.Dispose();
        }

        public unsafe void ExportInitialized(string outPath)
        {
            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(_dataBuffer.Length);
                using (FileMap map = FileMap.FromStream(stream))
                {
                    buint* addr = (buint*)map.Address;
                    foreach (Relocation loc in Relocations)
                        *addr++ = loc.SectionOffset;
                }
            }
        }

        public override unsafe void Export(string outPath)
        {
            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(_dataBuffer.Length);
                using (FileMap map = FileMap.FromStream(stream))
                    Memory.Move(map.Address, _dataBuffer.Address, (uint)_dataBuffer.Length);
            }
        }
    }

    //public class RELObjectSectionNode : ModuleSectionNode
    //{
    //    ObjectParser _objectParser;

    //    public void ParseObjects()
    //    {
    //        (_objectParser = new ObjectParser(this)).Parse();
    //    }

    //    public override bool OnInitialize()
    //    {
    //        base.OnInitialize();
    //        return _objectParser._objects.Count > 0;
    //    }

    //    public override void OnPopulate()
    //    {
    //        _objectParser.Populate();
    //    }
    //}

    //public unsafe class RELConstantsSectionNode : ModuleSectionNode
    //{
    //    float[] _values;
    //    public float[] Values { get { return _values; } set { _values = value; } }

    //    public override bool OnInitialize()
    //    {
    //        base.OnInitialize();
    //        _values = new float[_dataBuffer.Length / 4];
    //        bfloat* values = (bfloat*)_dataBuffer.Address;
    //        for (int i = 0; i < _values.Length; i++)
    //            _values[i] = values[i];
    //        return false;
    //    }
    //    public override void OnPopulate()
    //    {
    //        _values = new float[_dataBuffer.Length / 4];
    //        bfloat* values = (bfloat*)_dataBuffer.Address;
    //        for (int i = 0; i < _values.Length; i++)
    //            _values[i] = values[i];
    //    }

        //public override int OnCalculateSize(bool force)
        //{
        //    return _values.Length * 4;
        //}

        //public override void OnRebuild(VoidPtr address, int length, bool force)
        //{
        //    bfloat* values = (bfloat*)address;
        //    for (int i = 0; i < _values.Length; i++)
        //        values[i] = _values[i];
        //}
    //}

    //public class RELStructorSectionNode : ModuleSectionNode
    //{
    //    public bool _destruct;
    //    public override bool OnInitialize()
    //    {
    //        base.OnInitialize();
    //        for (int i = 0; i < _relocations.Count; i++)
    //            if (_relocations[i].RelOffset > 0)
    //                return true;
    //        return false;
    //    }
    //    public override void OnPopulate()
    //    {
    //        for (int i = 0; i < _relocations.Count; i++)
    //            if (_relocations[i].RelOffset > 0)
    //                new RELDeConStructorNode() { _destruct = _destruct, _index = i }.Initialize(this, (VoidPtr)BaseAddress + _relocations[i].RelOffset, 0);
    //    }
    //}
}
