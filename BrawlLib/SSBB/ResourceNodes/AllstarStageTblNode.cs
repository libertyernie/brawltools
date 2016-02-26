using BrawlLib.SSBB.Types;
using BrawlLib.SSBBTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class AllstarStageTblNode : ResourceNode
    {
        private AllstarStageTbl _data;

        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        [TypeConverter(typeof(DropDownListStageIDs))]
        public byte Stage1 { get { return (byte)(int)_data._stage1; } set { _data._stage1 = (sbyte)value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public byte Stage2 { get { return (byte)(int)_data._stage2; } set { _data._stage2 = (sbyte)value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public byte Stage3 { get { return (byte)(int)_data._stage3; } set { _data._stage3 = (sbyte)value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public byte Stage4 { get { return (byte)(int)_data._stage4; } set { _data._stage4 = (sbyte)value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public byte Stage5 { get { return (byte)(int)_data._stage5; } set { _data._stage5 = (sbyte)value; SignalPropertyChange(); } }

        [TypeConverter(typeof(DropDownListFighterIDs))]
        public byte Opponent1 { get { return _data._opponent1._fighterID; } set { _data._opponent1._fighterID = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListFighterIDs))]
        public byte Opponent2 { get { return _data._opponent2._fighterID; } set { _data._opponent2._fighterID = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListFighterIDs))]
        public byte Opponent3 { get { return _data._opponent3._fighterID; } set { _data._opponent3._fighterID = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListFighterIDs))]
        public byte Opponent4 { get { return _data._opponent4._fighterID; } set { _data._opponent4._fighterID = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListFighterIDs))]
        public byte Opponent5 { get { return _data._opponent5._fighterID; } set { _data._opponent5._fighterID = value; SignalPropertyChange(); } }

        public byte Opponent1VeryEasyUnknown00 { get { return _data._opponent1._difficulty1._unknown00; } }
        public byte Opponent1VeryEasyUnknown01 { get { return _data._opponent1._difficulty1._unknown01; } }
        public byte Opponent1VeryEasyUnknown02 { get { return _data._opponent1._difficulty1._unknown02; } }
        public byte Opponent1VeryEasyUnknown03 { get { return _data._opponent1._difficulty1._unknown03; } }
        public short Opponent1VeryEasyOffenseRatio { get { return _data._opponent1._difficulty1._offenseRatio; } set { _data._opponent1._difficulty1._offenseRatio = value; SignalPropertyChange(); } }
        public short Opponent1VeryEasyDefenseRatio { get { return _data._opponent1._difficulty1._defenseRatio; } set { _data._opponent1._difficulty1._defenseRatio = value; SignalPropertyChange(); } }
        public byte Opponent1VeryEasyUnknown08 { get { return _data._opponent1._difficulty1._unknown08; } }
        public byte Opponent1VeryEasyColor { get { return _data._opponent1._difficulty1._color; } set { _data._opponent1._difficulty1._color = value; SignalPropertyChange(); } }
        public byte Opponent1VeryEasyStock { get { return _data._opponent1._difficulty1._stock; } set { _data._opponent1._difficulty1._stock = value; SignalPropertyChange(); } }
        public byte Opponent1VeryEasyUnknown0b { get { return _data._opponent1._difficulty1._unknown0b; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            // Copy the data from the address
            AllstarStageTbl* dataPtr = (AllstarStageTbl*)WorkingUncompressed.Address;
            _data = *dataPtr;

            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Copy the data back to the address
            AllstarStageTbl* dataPtr = (AllstarStageTbl*)address;
            *dataPtr = _data;
        }
        public override int OnCalculateSize(bool force)
        {
            return sizeof(AllstarStageTbl);
        }
    }
}
