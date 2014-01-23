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
    public unsafe class RELMethodNode : ModuleDataNode
    {
        internal buint* Header { get { return (buint*)WorkingUncompressed.Address; } }

        [Browsable(false)]
        public RELObjectNode Object { get { return Parent.Parent as RELObjectNode; } }

        [Browsable(false)]
        public string FullName { get { return (Object != null ? Object.Type.FullName + "." : "") + _name; } }
        
        public override bool OnInitialize()
        {
            //ModuleSectionNode section = Location;
            //if (section == null)
            //    return false;

            //uint relative = (uint)Header - (uint)section.WorkingUncompressed.Address;

            //int x = 0;
            //while (!((PPCOpCode.Disassemble(&Header[x++])) is OpBlr)) ;

            //_relocations = new List<Relocation>();
            //Array.Copy(section._relocations, relative / 4, _relocations, 0, x);

            //_dataBuffer = new UnsafeBuffer(x * 4);

            //byte* pOut = (byte*)_dataBuffer.Address;
            //byte* pIn = (byte*)section._dataBuffer.Address + relative;
            //for (int i = 0; i < _dataBuffer.Length; i++)
            //    *pOut++ = *pIn++;

            //if (_relocations != null && _relocations.Count != 0)
            //{
            //    _relocations[0].Tags.Add(FullName + " Start");
            //    _relocations[_relocations.Count - 1].Tags.Add(FullName + " End");
            //}

            return false;
        }

        public override unsafe void Export(string outPath)
        {
            using (FileStream stream = new FileStream(outPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.RandomAccess))
            {
                stream.SetLength(_dataBuffer.Length);
                using (FileMap map = FileMap.FromStream(stream))
                {
                    VoidPtr addr = _dataBuffer.Address;

                    byte* pIn = (byte*)addr;
                    byte* pOut = (byte*)map.Address;
                    for (int i = 0; i < _dataBuffer.Length; i++)
                        *pOut++ = *pIn++;
                }
            }
        }
    }
}
