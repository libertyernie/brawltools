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
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    [TypeConverter(typeof(ExpandableObjectCustomConverter))]
    public unsafe class RelCommand
    {
        ModuleSectionNode[] Sections { get { return (_parentRelocation._section.Root as ModuleNode).Sections; } }

        [Category("Relocation Command"), Browsable(false)]
        public bool IsBranchSet { get { return (_command >= RELCommandType.SetBranchDestination && _command <= RELCommandType.SetBranchConditionDestination3); } }
        [Category("Relocation Command"), Browsable(false)]
        public bool IsHalf { get { return (_command >= RELCommandType.WriteLowerHalf1 && _command <= RELCommandType.WriteUpperHalfandBit1); } }

        [Category("Relocation Command"), Description("The offset relative to the start of the target section.")]
        public string TargetOffset 
        {
            get { return "0x" + _addend.ToString("X"); }
            set
            {
                string s = (value.StartsWith("0x") ? value.Substring(2, Math.Min(value.Length - 2, 8)) : value.Substring(0, Math.Min(value.Length, 8)));
                uint offset;
                if (uint.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out offset))
                {
                    if ((_parentRelocation._section.Root as ModuleNode).ID == _moduleID)
                    {
                        ModuleSectionNode section = Sections[TargetSectionID];
                        int x = (section.Relocations.Count * 4) - 2;
                        offset = offset.Clamp(0, (uint)(x < 0 ? 0 : x)); 
                    }
                    _addend = offset;
                    _parentRelocation._section.SignalPropertyChange();
                }
            }
        }
        [Category("Relocation Command"), Description("Determines how the offset should be written.")]
        public RELCommandType Command { get { return _command; } set { _command = value; _parentRelocation._section.SignalPropertyChange(); } }

        [Category("Relocation Command"), Description("The index of the section to offset into.")]
        public uint TargetSectionID 
        {
            get { return _targetSectionId; } 
            set 
            {
                if ((_parentRelocation._section.Root as ModuleNode).ID == _moduleID)
                {
                    _targetSectionId = value.Clamp(0, (uint)Sections.Length - 1);
                    ModuleSectionNode section = Sections[TargetSectionID];
                    int x = (section.Relocations.Count * 4) - 2;
                    _addend = _addend.Clamp(0, (uint)(x < 0 ? 0 : x));
                }
                else
                    _targetSectionId = value;
                _parentRelocation._section.SignalPropertyChange(); 
            } 
        }
        [Category("Relocation Command"), Description("The ID of the target module."), TypeConverter(typeof(DropDownListRELModuleIDs))]
        public string TargetModuleID
        {
            get { return RELNode._idNames.ContainsKey((int)_moduleID) ? RELNode._idNames[(int)_moduleID] : _moduleID.ToString(); }
            set
            {
                uint id = 0;
                if (!uint.TryParse(value, out id) && RELNode._idNames.ContainsValue(value))
                    id = (uint)RELNode._idNames.Keys[RELNode._idNames.IndexOfValue(value)];

                _moduleID = id;
                _parentRelocation._section.SignalPropertyChange();
            }
        }

        public RELCommandType _command;
        public int _modifiedSectionId;
        public uint _targetSectionId;
        public uint _moduleID;

        //Added is an offset relative to the start of the section
        public uint _addend;
        public bool _initialized = false;

        [Category("Relocation Command"), Browsable(false)]
        public RelCommand Next { get { return _next; } }
        [Category("Relocation Command"), Browsable(false)]
        public RelCommand Previous { get { return _prev; } }

        public RelCommand _next = null;
        public RelCommand _prev = null;

        public Relocation _parentRelocation;

        public void Remove()
        {
            if (_next != null)
                _next._prev = _prev;
            if (_prev != null)
                _prev._next = _next;
            _next = _prev = null;
        }

        public void InsertAfter(RelCommand cmd)
        {
            _prev = cmd;
            _next = cmd._next;
            cmd._next = this;
        }

        public void InsertBefore(RelCommand cmd)
        {
            _next = cmd;
            _prev = cmd._prev;
            cmd._prev = this;
        }

        public RelCommand(uint fileId, int section, RELLink link)
        {
            _moduleID = fileId;
            _modifiedSectionId = section;
            _targetSectionId = link._section;
            _command = (RELCommandType)(int)link._type;
            _addend = link._value;
        }

        public Relocation GetTargetRelocation()
        {
            if (_parentRelocation == null)
                return null;
            RELNode r = _parentRelocation._section.Root as RELNode;
            if (r != null && _targetSectionId > 0 && _targetSectionId < r.Sections.Length && r.ModuleID == _moduleID)
                return r.Sections[_targetSectionId].GetRelocationAtOffset((int)_addend);
            return null;
        }

        public void SetTargetRelocation(Relocation e)
        {
            if (e != null)
                _addend = (uint)e._index * 4;
        }

        public uint Apply(bool absolute)
        {
            uint newValue = _parentRelocation.RawValue;
            uint addend = _addend + (absolute ? _parentRelocation._section._offset : 0);

            switch (_command)
            {
                case RELCommandType.WriteWord: //0x1
                    newValue = addend;
                    break;

                case RELCommandType.SetBranchOffset: //0x2
                    newValue &= 0xFC000003;
                    newValue |= (addend & 0x03FFFFFC);
                    break;

                case RELCommandType.WriteLowerHalf1: //0x3
                case RELCommandType.WriteLowerHalf2: //0x4
                    newValue &= 0xFFFF0000;
                    newValue |= (ushort)(addend & 0xFFFF);
                    break;

                case RELCommandType.WriteUpperHalf: //0x5
                    newValue &= 0xFFFF0000;
                    newValue |= (ushort)(addend >> 16);
                    break;

                case RELCommandType.WriteUpperHalfandBit1: //0x6
                    newValue &= 0xFFFF0000;
                    newValue |= (ushort)((addend >> 16) | (addend & 0x1));
                    break;

                case RELCommandType.SetBranchConditionOffset1: //0x7
                case RELCommandType.SetBranchConditionOffset2: //0x8
                case RELCommandType.SetBranchConditionOffset3: //0x9
                    newValue &= 0xFFFF0003;
                    newValue |= (addend & 0xFFFC);
                    break;

                case RELCommandType.SetBranchDestination: //0xA
                    Console.WriteLine();
                    break;

                case RELCommandType.SetBranchConditionDestination1: //0xB
                case RELCommandType.SetBranchConditionDestination2: //0xC
                case RELCommandType.SetBranchConditionDestination3: //0xD
                    Console.WriteLine();
                    break;

                default:
                    throw new Exception("Unknown Relocation Command.");
            }
            return newValue;
        }
    }

    public enum RELCommandType : byte
    {
        Nop = 0x0,
        WriteWord = 0x1,
        SetBranchOffset = 0x2,
        WriteLowerHalf1 = 0x3,
        WriteLowerHalf2 = 0x4,
        WriteUpperHalf = 0x5,
        WriteUpperHalfandBit1 = 0x6,
        SetBranchConditionOffset1 = 0x7,
        SetBranchConditionOffset2 = 0x8,
        SetBranchConditionOffset3 = 0x9,
        SetBranchDestination = 0xA,
        SetBranchConditionDestination1 = 0xB,
        SetBranchConditionDestination2 = 0xC,
        SetBranchConditionDestination3 = 0xD,
    }
}
