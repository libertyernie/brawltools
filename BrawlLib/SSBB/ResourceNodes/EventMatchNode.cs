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
    public unsafe class EventMatchFighterDataWrapper
    {
        private ResourceNode parent;
        private EventMatchFighterData data;

        public EventMatchFighterDataWrapper(ResourceNode parent, EventMatchFighterData data)
        {
            this.parent = parent;
            this.data = data;
        }

        public byte FighterID { get { return data._fighterID; } set { data._fighterID = value; parent.SignalPropertyChange(); } }
        public byte Status { get { return data._status; } set { data._status = value; parent.SignalPropertyChange(); } }
        public byte Unknown02 { get { return data._unknown02; } set { data._unknown02 = value; parent.SignalPropertyChange(); } }
        public byte Unknown03 { get { return data._unknown03; } set { data._unknown03 = value; parent.SignalPropertyChange(); } }
        public float Scale { get { return data._scale; } set { data._scale = value; parent.SignalPropertyChange(); } }
        public byte Team { get { return data._team; } set { data._team = value; parent.SignalPropertyChange(); } }
        public byte Unknown09 { get { return data._unknown09; } set { data._unknown09 = value; parent.SignalPropertyChange(); } }
        public byte Unknown0a { get { return data._unknown0a; } set { data._unknown0a = value; parent.SignalPropertyChange(); } }
        public byte Unknown0b { get { return data._unknown0b; } set { data._unknown0b = value; parent.SignalPropertyChange(); } }

        public static explicit operator EventMatchFighterData(EventMatchFighterDataWrapper w)
        {
            return w.data;
        }

        public override string ToString()
        {
            return "Event Match Fighter Data" + (FighterID == 0x3E ? " - Disabled" : "");
        }
    }
    public unsafe class EventMatchFighterDataSelfWrapper : EventMatchFighterDataWrapper
    {
        public EventMatchFighterDataSelfWrapper(ResourceNode parent, EventMatchFighterData data) : base(parent, data) { }

        public override string ToString()
        {
            return "Event Match Fighter Data" + (FighterID == 0x3E ? " - Choose" : "");
        }
    }

    public unsafe class EventMatchNode : ResourceNode
    {
        public static ResourceNode Create(int length)
        {
            length -= sizeof(EventMatchTblHeader);
            if (length % sizeof(EventMatchFighterData) != 0)
                throw new Exception("Cannot create EventMatchNode with size " + length);
            length /= sizeof(EventMatchFighterData);

            switch (length)
            {
                case 4:
                case 9:
                case 38:
                    return new EventMatchNode();
                default:
                    throw new Exception("Cannot create EventMatchNode with size " + length);
            }
        }

        private EventMatchTblHeader _header;

        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public EventMatchFighterDataWrapper FighterData0 { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public EventMatchFighterDataWrapper FighterData1 { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public EventMatchFighterDataWrapper FighterData2 { get; set; }
        [Category("Fighters"), TypeConverter(typeof(ExpandableObjectConverter))]
        public EventMatchFighterDataWrapper FighterData3 { get; set; }

        public bint Unknown00 { get { return _header._unknown00; } set { _header._unknown00 = value; } }
        public bint Unknown04 { get { return _header._unknown04; } set { _header._unknown04 = value; } }

        [Category("Match")]
        [DisplayName("Match Type")]
        public byte MatchType { get { return _header._matchType; } set { _header._matchType = value; SignalPropertyChange(); } }

        public byte Unknown09 { get { return _header._unknown09; } set { _header._unknown09 = value; } }
        public byte Unknown0a { get { return _header._unknown0a; } set { _header._unknown0a = value; } }
        public byte Unknown0b { get { return _header._unknown0b; } set { _header._unknown0b = value; } }

        [Category("Match")]
        [DisplayName("Time Limit")]
        public int TimeLimit { get { return _header._timeLimit; } set { _header._timeLimit = value; SignalPropertyChange(); } }

        public byte Unknown10 { get { return _header._unknown10; } set { _header._unknown10 = value; } }
        public byte Unknown11 { get { return _header._unknown10; } set { _header._unknown10 = value; } }
        public byte Unknown12 { get { return _header._unknown10; } set { _header._unknown10 = value; } }
        public byte Unknown13 { get { return _header._unknown10; } set { _header._unknown10 = value; } }
        public float Unknown14 { get { return _header._unknown14; } set { _header._unknown14 = value; } }

        [Category("Match")]
        [DisplayName("Item Frequency")]
        public short ItemFrequency { get { return _header._itemFrequency; } set { _header._itemFrequency = value; SignalPropertyChange(); } }

        public short Unknown1a { get { return _header._unknown1a; } set { _header._unknown1a = value; } }
        public byte Unknown1c { get { return _header._unknown1c; } set { _header._unknown1c = value; } }
        public byte Unknown1d { get { return _header._unknown1d; } set { _header._unknown1d = value; } }
        public byte Unknown1e { get { return _header._unknown1e; } set { _header._unknown1e = value; } }

        //[Category("Match")]
        //[DisplayName("Stage ID")]
        //public byte StageID { get { return _header._stageID; } set { _header._stageID = value; SignalPropertyChange(); } }

        [Category("Match")]
        [TypeConverter(typeof(DropDownListStageIDs))]
        public string StageName
        {
            get
            {
                Stage stage = Stage.Stages.Where(s => s.ID == _header._stageID).FirstOrDefault();
                return _header._stageID.ToString("X2") + (stage == null ? "" : (" - " + stage.Name));
            }
            set
            {
                // Don't try to set the stage ID if it's not a stage module
                if (value.Length < 2) return;
                _header._stageID = byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber);
                SignalPropertyChange();
            }
        }

        public byte Unknown20 { get { return _header._unknown20; } set { _header._unknown20 = value; } }
        public byte Unknown21 { get { return _header._unknown21; } set { _header._unknown21 = value; } }
        public byte Unknown22 { get { return _header._unknown22; } set { _header._unknown22 = value; } }
        public byte Unknown23 { get { return _header._unknown23; } set { _header._unknown23 = value; } }
        public int Unknown24 { get { return _header._unknown24; } set { _header._unknown24 = value; } }
        public int Unknown28 { get { return _header._unknown28; } set { _header._unknown28 = value; } }
        public int Unknown2c { get { return _header._unknown2c; } set { _header._unknown2c = value; } }
        public int Unknown30 { get { return _header._unknown30; } set { _header._unknown30 = value; } }
        public int Unknown34 { get { return _header._unknown34; } set { _header._unknown34 = value; } }

        [Category("Match")]
        [DisplayName("Game Speed")]
        public float GameSpeed { get { return _header._gameSpeed; } set { _header._gameSpeed = value; SignalPropertyChange(); } }

        [Category("Match")]
        [DisplayName("Camera Shake Control")]
        public float CameraShakeControl { get { return _header._cameraShakeControl; } set { _header._cameraShakeControl = value; SignalPropertyChange(); } }

        public byte Unknown40 { get { return _header._unknown40; } set { _header._unknown40 = value; } }
        public byte Unknown41 { get { return _header._unknown41; } set { _header._unknown41 = value; } }
        public byte Unknown42 { get { return _header._unknown42; } set { _header._unknown42 = value; } }
        public byte Unknown43 { get { return _header._unknown43; } set { _header._unknown43 = value; } }
        public byte Unknown44 { get { return _header._unknown44; } set { _header._unknown44 = value; } }
        public byte Unknown45 { get { return _header._unknown45; } set { _header._unknown45 = value; } }

        [Category("Match")]
        [DisplayName("Song ID")]
        public short SongID { get { return _header._songID; } set { _header._songID = value; SignalPropertyChange(); } }

        [Category("Match")]
        [DisplayName("Global Offense Ratio")]
        public short GlobalOffenseRatio { get { return _header._globalOffenseRatio; } set { _header._globalOffenseRatio = value; SignalPropertyChange(); } }

        [Category("Match")]
        [DisplayName("Global Defense Ratio")]
        public short GlobalDefenseRatio { get { return _header._globalDefenseRatio; } set { _header._globalDefenseRatio = value; SignalPropertyChange(); } }

        public bint Unknown4c { get { return _header._unknown4c; } set { _header._unknown4c = value; } }

        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            // Copy the data from the address
            EventMatchTblHeader* dataPtr = (EventMatchTblHeader*)WorkingUncompressed.Address;
            _header = *dataPtr;

            FighterData0 = new EventMatchFighterDataSelfWrapper(this, dataPtr->FighterDataPtr[0]);
            FighterData1 = new EventMatchFighterDataWrapper(this, dataPtr->FighterDataPtr[1]);
            FighterData2 = new EventMatchFighterDataWrapper(this, dataPtr->FighterDataPtr[2]);
            FighterData3 = new EventMatchFighterDataWrapper(this, dataPtr->FighterDataPtr[3]);

            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Copy the data back to the address
            EventMatchTblHeader* dataPtr = (EventMatchTblHeader*)address;
            *dataPtr = _header;
            dataPtr->FighterDataPtr[0] = (EventMatchFighterData)FighterData0;
            dataPtr->FighterDataPtr[1] = (EventMatchFighterData)FighterData1;
            dataPtr->FighterDataPtr[2] = (EventMatchFighterData)FighterData2;
            dataPtr->FighterDataPtr[3] = (EventMatchFighterData)FighterData3;
        }
        public override int OnCalculateSize(bool force)
        {
            return sizeof(EventMatchTblHeader) + 4 * sizeof(EventMatchFighterData);
        }
    }
}
