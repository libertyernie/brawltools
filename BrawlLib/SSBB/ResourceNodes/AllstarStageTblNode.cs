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
    /// <summary>
    /// A class that wraps the data represented by the AllstarDifficultyData structure in a class with properties, so it can be modified by PropertyGrid.
    /// </summary>
    public unsafe class AllstarDifficultyDataWrapper
    {
        private ResourceNode parent;
        private AllstarDifficultyData data;

        public byte Unknown00 { get { return data._unknown00; } set { data._unknown00 = value; parent.SignalPropertyChange(); } }
        public byte Unknown01 { get { return data._unknown01; } set { data._unknown01 = value; parent.SignalPropertyChange(); } }
        public byte Unknown02 { get { return data._unknown02; } set { data._unknown02 = value; parent.SignalPropertyChange(); } }
        public byte Unknown03 { get { return data._unknown03; } set { data._unknown03 = value; parent.SignalPropertyChange(); } }

        public short OffenseRatio { get { return data._offenseRatio; } set { data._offenseRatio = value; parent.SignalPropertyChange(); } }
        public short DefenseRatio { get { return data._defenseRatio; } set { data._defenseRatio = value; parent.SignalPropertyChange(); } }

        public byte Unknown08 { get { return data._unknown08; } set { data._unknown08 = value; parent.SignalPropertyChange(); } }
        public byte Color { get { return data._color; } set { data._color = value; parent.SignalPropertyChange(); } }
        public byte Stock { get { return data._stock; } set { data._stock = value; parent.SignalPropertyChange(); } }
        public byte Unknown0b { get { return data._unknown0b; } set { data._unknown0b = value; parent.SignalPropertyChange(); } }

        public bshort Unknown0c { get { return data._unknown0c; } set { data._unknown0c = value; parent.SignalPropertyChange(); } }

        public AllstarDifficultyDataWrapper(ResourceNode parent, AllstarDifficultyData data)
        {
            this.parent = parent;
            this.data = data;
        }

        public static explicit operator AllstarDifficultyData(AllstarDifficultyDataWrapper w)
        {
            return w.data;
        }
    }

    /// <summary>
    /// A class that wraps the data represented by the AllstarFighterData structure in a class with properties, so it can be modified by PropertyGrid.
    /// </summary>
    public unsafe class AllstarFighterDataWrapper
    {
        private ResourceNode parent;

        private byte _fighterID;
        private float _unknown04;

        [TypeConverter(typeof(DropDownListFighterIDs))]
        public byte FighterID { get { return _fighterID; } set { _fighterID = value; parent.SignalPropertyChange(); } }
        public float Unknown04 { get { return _unknown04; } set { _unknown04 = value; parent.SignalPropertyChange(); } }

        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarDifficultyDataWrapper VeryEasy { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarDifficultyDataWrapper Easy { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarDifficultyDataWrapper Normal { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarDifficultyDataWrapper Hard { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarDifficultyDataWrapper VeryHard { get; set; }

        public AllstarFighterDataWrapper(ResourceNode parent, AllstarFighterData data)
        {
            this.parent = parent;

            _fighterID = data._fighterID;
            _unknown04 = data._unknown04;
            VeryEasy = new AllstarDifficultyDataWrapper(parent, data._difficulty1);
            Easy = new AllstarDifficultyDataWrapper(parent, data._difficulty2);
            Normal = new AllstarDifficultyDataWrapper(parent, data._difficulty3);
            Hard = new AllstarDifficultyDataWrapper(parent, data._difficulty4);
            VeryHard = new AllstarDifficultyDataWrapper(parent, data._difficulty5);
        }

        public static explicit operator AllstarFighterData(AllstarFighterDataWrapper w)
        {
            return new AllstarFighterData
            {
                _fighterID = w._fighterID,
                _unknown04 = w._unknown04,
                _difficulty1 = (AllstarDifficultyData)w.VeryEasy,
                _difficulty2 = (AllstarDifficultyData)w.Easy,
                _difficulty3 = (AllstarDifficultyData)w.Normal,
                _difficulty4 = (AllstarDifficultyData)w.Hard,
                _difficulty5 = (AllstarDifficultyData)w.VeryHard
            };
        }

        public override string ToString()
        {
            var fighter = BrawlLib.SSBB.Fighter.Fighters.Where(s => s.ID == FighterID).FirstOrDefault();
            return "All-Star Fighter Data: 0x" + FighterID.ToString("X2") + (fighter == null ? "" : (" - " + fighter.Name));
        }
    }

    public unsafe class AllstarStageTblNode : ResourceNode
    {
        private int _stage1, _stage2, _stage3, _stage4, _stage5;

        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        [TypeConverter(typeof(DropDownListStageIDs))]
        public int Stage1 { get { return _stage1; } set { _stage1 = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public int Stage2 { get { return _stage2; } set { _stage2 = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public int Stage3 { get { return _stage3; } set { _stage3 = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public int Stage4 { get { return _stage4; } set { _stage4 = value; SignalPropertyChange(); } }
        [TypeConverter(typeof(DropDownListStageIDs))]
        public int Stage5 { get { return _stage5; } set { _stage5 = value; SignalPropertyChange(); } }

        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarFighterDataWrapper Opponent1 { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarFighterDataWrapper Opponent2 { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarFighterDataWrapper Opponent3 { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarFighterDataWrapper Opponent4 { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public AllstarFighterDataWrapper Opponent5 { get; set; }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            // Copy the data from the address
            AllstarStageTbl* dataPtr = (AllstarStageTbl*)WorkingUncompressed.Address;
            _stage1 = dataPtr->_stage1;
            _stage2 = dataPtr->_stage2;
            _stage3 = dataPtr->_stage3;
            _stage4 = dataPtr->_stage4;
            _stage5 = dataPtr->_stage5;

            Opponent1 = new AllstarFighterDataWrapper(this, dataPtr->_opponent1);
            Opponent2 = new AllstarFighterDataWrapper(this, dataPtr->_opponent2);
            Opponent3 = new AllstarFighterDataWrapper(this, dataPtr->_opponent3);
            Opponent4 = new AllstarFighterDataWrapper(this, dataPtr->_opponent4);
            Opponent5 = new AllstarFighterDataWrapper(this, dataPtr->_opponent5);

            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Copy the data back to the address
            AllstarStageTbl* dataPtr = (AllstarStageTbl*)address;
            dataPtr->_stage1 = _stage1;
            dataPtr->_stage2 = _stage2;
            dataPtr->_stage3 = _stage3;
            dataPtr->_stage4 = _stage4;
            dataPtr->_stage5 = _stage5;

            dataPtr->_opponent1 = (AllstarFighterData)Opponent1;
            dataPtr->_opponent2 = (AllstarFighterData)Opponent2;
            dataPtr->_opponent3 = (AllstarFighterData)Opponent3;
            dataPtr->_opponent4 = (AllstarFighterData)Opponent4;
            dataPtr->_opponent5 = (AllstarFighterData)Opponent5;
        }
        public override int OnCalculateSize(bool force)
        {
            return sizeof(AllstarStageTbl);
        }
    }
}
