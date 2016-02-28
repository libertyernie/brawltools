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
    public unsafe class EventMatchFighterNode : ResourceNode
    {
        public enum StatusEnum : byte
        {
            Normal = 0,
            Metal = 1,
            Invisible = 2
        }

        private EventMatchFighterData data;

        // Hack to maintain ordering from replacement file upon replace
        private int _origIndex;
        public EventMatchFighterNode(int index) : base() { _origIndex = index; }

        [TypeConverter(typeof(DropDownListFighterIDs))]
        public byte FighterID { get { return data._fighterID; } set { data._fighterID = value; SignalPropertyChange(); } }

        public StatusEnum Status { get { return (StatusEnum)data._status; } set { data._status = (byte)value; SignalPropertyChange(); } }
        //public byte Unknown02  { get { return data._unknown02; }          set { data._unknown02 = value;    SignalPropertyChange(); } }
        //public byte Unknown03  { get { return data._unknown03; }          set { data._unknown03 = value;    SignalPropertyChange(); } }
        public float Scale       { get { return data._scale; }              set { data._scale = value;        SignalPropertyChange(); } }
        public byte Team         { get { return data._team; }               set { data._team = value;         SignalPropertyChange(); } }
        //public byte Unknown09  { get { return data._unknown09; }          set { data._unknown09 = value;    SignalPropertyChange(); } }
        //public byte Unknown0a  { get { return data._unknown0a; }          set { data._unknown0a = value;    SignalPropertyChange(); } }
        //public byte Unknown0b  { get { return data._unknown0b; }          set { data._unknown0b = value;    SignalPropertyChange(); } }

        public byte EasyCpuLevel          { get { return data._easy._cpuLevel; }         set { data._easy._cpuLevel = value;         SignalPropertyChange(); } }
        //public byte EasyUnknown01       { get { return data._easy._unknown01; }        set { data._easy._unknown01 = value;        SignalPropertyChange(); } }
        public ushort EasyOffenseRatio    { get { return data._easy._offenseRatio; }     set { data._easy._offenseRatio = value;     SignalPropertyChange(); } }
        public ushort EasyDefenseRatio    { get { return data._easy._defenseRatio; }     set { data._easy._defenseRatio = value;     SignalPropertyChange(); } }
        public byte EasyAiType            { get { return data._easy._aiType; }           set { data._easy._aiType = value;           SignalPropertyChange(); } }
        public byte EasyCostume           { get { return data._easy._costume; }          set { data._easy._costume = value;          SignalPropertyChange(); } }
        public byte EasyStockCount        { get { return data._easy._stockCount; }       set { data._easy._stockCount = value;       SignalPropertyChange(); } }
        //public byte EasyUnknown09       { get { return data._easy._unknown09; }        set { data._easy._unknown09 = value;        SignalPropertyChange(); } }
        public short EasyInitialHitPoints { get { return data._easy._initialHitPoints; } set { data._easy._initialHitPoints = value; SignalPropertyChange(); } }
        public short EasyStartingDamage   { get { return data._easy._startingDamage; }   set { data._easy._startingDamage = value;   SignalPropertyChange(); } }

        public byte NormalCpuLevel          { get { return data._normal._cpuLevel; }         set { data._normal._cpuLevel = value;         SignalPropertyChange(); } }
        //public byte NormalUnknown01       { get { return data._normal._unknown01; }        set { data._normal._unknown01 = value;        SignalPropertyChange(); } }
        public ushort NormalOffenseRatio    { get { return data._normal._offenseRatio; }     set { data._normal._offenseRatio = value;     SignalPropertyChange(); } }
        public ushort NormalDefenseRatio    { get { return data._normal._defenseRatio; }     set { data._normal._defenseRatio = value;     SignalPropertyChange(); } }
        public byte NormalAiType            { get { return data._normal._aiType; }           set { data._normal._aiType = value;           SignalPropertyChange(); } }
        public byte NormalCostume           { get { return data._normal._costume; }          set { data._normal._costume = value;          SignalPropertyChange(); } }
        public byte NormalStockCount        { get { return data._normal._stockCount; }       set { data._normal._stockCount = value;       SignalPropertyChange(); } }
        //public byte NormalUnknown09       { get { return data._normal._unknown09; }        set { data._normal._unknown09 = value;        SignalPropertyChange(); } }
        public short NormalInitialHitPoints { get { return data._normal._initialHitPoints; } set { data._normal._initialHitPoints = value; SignalPropertyChange(); } }
        public short NormalStartingDamage   { get { return data._normal._startingDamage; }   set { data._normal._startingDamage = value;   SignalPropertyChange(); } }

        public byte HardCpuLevel          { get { return data._hard._cpuLevel; }         set { data._hard._cpuLevel = value;         SignalPropertyChange(); } }
        //public byte HardUnknown01       { get { return data._hard._unknown01; }        set { data._hard._unknown01 = value;        SignalPropertyChange(); } }
        public ushort HardOffenseRatio    { get { return data._hard._offenseRatio; }     set { data._hard._offenseRatio = value;     SignalPropertyChange(); } }
        public ushort HardDefenseRatio    { get { return data._hard._defenseRatio; }     set { data._hard._defenseRatio = value;     SignalPropertyChange(); } }
        public byte HardAiType            { get { return data._hard._aiType; }           set { data._hard._aiType =  value;          SignalPropertyChange(); } }
        public byte HardCostume           { get { return data._hard._costume; }          set { data._hard._costume = value;          SignalPropertyChange(); } }
        public byte HardStockCount        { get { return data._hard._stockCount; }       set { data._hard._stockCount = value;       SignalPropertyChange(); } }
        //public byte HardUnknown09       { get { return data._hard._unknown09; }        set { data._hard._unknown09 = value;        SignalPropertyChange(); } }
        public short HardInitialHitPoints { get { return data._hard._initialHitPoints; } set { data._hard._initialHitPoints = value; SignalPropertyChange(); } }
        public short HardStartingDamage   { get { return data._hard._startingDamage; }   set { data._hard._startingDamage = value;   SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            if (WorkingUncompressed.Length != sizeof(EventMatchFighterData))
                throw new Exception("Wrong size for EventMatchFighterNode");

            // Copy the data from the address
            data = *(EventMatchFighterData*)WorkingUncompressed.Address;

            if (_name == null)
            {
                bool changed = HasChanged;
                UpdateName();
                HasChanged = changed;
            }

            return false;
        }
        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            // Copy the data back to the address
            *(EventMatchFighterData*)address = data;
        }
        public override int OnCalculateSize(bool force)
        {
            // Constant size (48 bytes)
            return sizeof(EventMatchFighterData);
        }
        public void UpdateName()
        {
            var fighter = BrawlLib.SSBB.Fighter.Fighters.Where(s => s.ID == FighterID).FirstOrDefault();
            Name = "Fighter: 0x" + FighterID.ToString("X2") + (fighter == null ? "" : (" - " + fighter.Name));
            // Hack to maintain ordering from replacement file upon replace
            Name = "#" + _origIndex + " " + Name;
        }
    }

    public unsafe class EventMatchNode : ResourceNode
    {
        public override ResourceType ResourceType { get { return ResourceType.Container; } }

        public enum ItemLevelEnum : short
        {
            Off = 0,
            Low = 1,
            Medium = 2,
            High = 3,
            Raining = 4
        }

        public enum MatchTypeEnum : byte
        {
            Time = 0,
            Stock = 1,
            Coin = 2
        }

        private EventMatchTblHeader _header;

        [DisplayName("Event Extension")]
        public bint EventExtension { get { return _header._eventExtension; } }

        //public bint Unknown04 { get { return _header._unknown04; } set { _header._unknown04 = value; } }

        [DisplayName("Match Type")]
        public MatchTypeEnum MatchType { get { return (MatchTypeEnum)_header._matchType; } set { _header._matchType = (byte)value; SignalPropertyChange(); } }

        //public byte Unknown09 { get { return _header._unknown09; } set { _header._unknown09 = value; SignalPropertyChange(); } }
        //public byte Unknown0a { get { return _header._unknown0a; } set { _header._unknown0a = value; SignalPropertyChange(); } }
        //public byte Unknown0b { get { return _header._unknown0b; } set { _header._unknown0b = value; SignalPropertyChange(); } }

        [DisplayName("Time Limit")]
        public int TimeLimit { get { return _header._timeLimit; } set { _header._timeLimit = value; SignalPropertyChange(); } }

        [DisplayName("Timer Visible")]
        public bool TimerVisible
        {
            get
            {
                return (_header._flags10 & 0x20000000) != 0;
            }
            set
            {
                SignalPropertyChange();
                if (value)
                    _header._flags10 |= 0x20000000;
                else
                    _header._flags10 &= ~0x20000000;
            }
        }

        [DisplayName("Hide Countdown")]
        public bool HideCountdown
        {
            get
            {
                return (_header._flags10 & 0x40000000) != 0;
            }
            set
            {
                SignalPropertyChange();
                if (value)
                    _header._flags10 |= 0x40000000;
                else
                    _header._flags10 &= ~0x40000000;
            }
        }

        //public bool UnknownFlag_10_80000000
        //{
        //    get
        //    {
        //        return (_header._flags10 & 0x80000000) != 0;
        //    }
        //    set
        //    {
        //        SignalPropertyChange();
        //        if (value)
        //            _header._flags10 |= unchecked((int)0x80000000);
        //        else
        //            _header._flags10 &= ~unchecked((int)0x80000000);
        //    }
        //}

        //public float Unknown14 { get { return _header._unknown14; } set { _header._unknown14 = value; SignalPropertyChange(); } }

        [DisplayName("Hide Damage Values")]
        public bool HideDamageValues
        {
            get
            {
                return (_header._flags18 & 0x80) != 0;
            }
            set
            {
                SignalPropertyChange();
                if (value)
                    _header._flags18 |= 0x80;
                else
                    _header._flags18 &= unchecked((byte)~0x80);
            }
        }

        [DisplayName("Team Match")]
        public bool IsTeamGame
        {
            get
            {
                return _header._isTeamGame != 0;
            }
            set
            {
                SignalPropertyChange();
                _header._isTeamGame = (byte)(value ? 1 : 0);
            }
        }

        [DisplayName("Item Level")]
        public ItemLevelEnum ItemLevel { get { return (ItemLevelEnum)(short)_header._itemLevel; } set { _header._itemLevel = (short)value; SignalPropertyChange(); } }

        //public byte Unknown1c { get { return _header._unknown1c; } set { _header._unknown1c = value; SignalPropertyChange(); } }
        //public byte Unknown1d { get { return _header._unknown1d; } set { _header._unknown1d = value; SignalPropertyChange(); } }

        [DisplayName("Stage")]
        [TypeConverter(typeof(DropDownListStageIDs))]
        public int StageID
        {
            get
            {
                return (ushort)_header._stageID;
            }
            set
            {
                _header._stageID = (ushort)value;
                SignalPropertyChange();
            }
        }

        [DisplayName("Players On Screen")]
        public int PlayersOnScreen
        {
            get
            {
                return (_header._flags20 >> 21) & 7;
            }
            set
            {
                SignalPropertyChange();
                _header._flags20 &= ~0xE00000;
                _header._flags20 |= (int)((value & 7) << 21);
            }
        }

        //public bool UnknownFlag_20_10000000
        //{
        //    get
        //    {
        //        return (_header._flags20 & 0x10000000) != 0;
        //    }
        //    set
        //    {
        //        SignalPropertyChange();
        //        if (value)
        //            _header._flags20 |= unchecked((int)0x10000000);
        //        else
        //            _header._flags20 &= ~unchecked((int)0x10000000);
        //    }
        //}

        //public int Unknown24 { get { return _header._unknown24; } set { _header._unknown24 = value; SignalPropertyChange(); } }
        //public int Unknown28 { get { return _header._unknown28; } set { _header._unknown28 = value; SignalPropertyChange(); } }
        //public int Unknown2c { get { return _header._unknown2c; } set { _header._unknown2c = value; SignalPropertyChange(); } }
        //public int Unknown30 { get { return _header._unknown30; } set { _header._unknown30 = value; SignalPropertyChange(); } }
        //public int Unknown34 { get { return _header._unknown34; } set { _header._unknown34 = value; SignalPropertyChange(); } }

        [DisplayName("Game Speed")]
        public float GameSpeed { get { return _header._gameSpeed; } set { _header._gameSpeed = value; SignalPropertyChange(); } }

        [DisplayName("Camera Shake Control")]
        public float CameraShakeControl { get { return _header._cameraShakeControl; } set { _header._cameraShakeControl = value; SignalPropertyChange(); } }

        //public bool UnknownFlag_40_80000000
        //{
        //    get
        //    {
        //        return (_header._flags40 & 0x80000000) != 0;
        //    }
        //    set
        //    {
        //        SignalPropertyChange();
        //        if (value)
        //            _header._flags40 |= unchecked((int)0x80000000);
        //        else
        //            _header._flags40 &= ~unchecked((int)0x80000000);
        //    }
        //}

        [DisplayName("Song ID")]
        public int SongID { get { return _header._songID; } set { _header._songID = value; SignalPropertyChange(); } }

        [DisplayName("Global Offense Ratio")]
        public short GlobalOffenseRatio { get { return _header._globalOffenseRatio; } set { _header._globalOffenseRatio = value; SignalPropertyChange(); } }

        [DisplayName("Global Defense Ratio")]
        public short GlobalDefenseRatio { get { return _header._globalDefenseRatio; } set { _header._globalDefenseRatio = value; SignalPropertyChange(); } }

        //[DisplayName("Unknown")]
        //public bint Unknown4c { get { return _header._unknown4c; } set { _header._unknown4c = value; SignalPropertyChange(); } }

        public override bool OnInitialize()
        {
            base.OnInitialize();

            // Copy the data from the address
            EventMatchTblHeader* dataPtr = (EventMatchTblHeader*)WorkingUncompressed.Address;
            _header = *dataPtr;

            return true;
        }

        public override void OnPopulate()
        {
            int numFighters =
                _header._eventExtension == 0 ? 4
                : _header._eventExtension == 1 ? 9
                : _header._eventExtension == 2 ? 38
                : 0;

            VoidPtr ptr = (VoidPtr)(WorkingUncompressed.Address + sizeof(EventMatchTblHeader));
            for (int i = 0; i < numFighters; i++)
            {
                DataSource source = new DataSource(ptr, sizeof(EventMatchFighterData));
                new EventMatchFighterNode(i).Initialize(this, source);
                ptr += sizeof(EventMatchFighterData);
            }
        }

        public override void OnRebuild(VoidPtr address, int length, bool force)
        {
            switch (Children.Count)
            {
                case 4:
                    _header._eventExtension = 0;
                    break;
                case 9:
                    _header._eventExtension = 1;
                    break;
                case 38:
                    _header._eventExtension = 2;
                    break;
                default:
                    throw new Exception("Invalid number of children for EventMatchNode (must be 4, 9, or 38)");
            }

            // Copy the data back to the address
            EventMatchTblHeader* dataPtr = (EventMatchTblHeader*)address;
            *dataPtr = _header;

            // Rebuild children using new address
            VoidPtr ptr = (VoidPtr)(address + sizeof(EventMatchTblHeader));
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Rebuild(ptr, sizeof(EventMatchFighterData), true);
                ptr += sizeof(EventMatchFighterData);
            }
        }

        public override int OnCalculateSize(bool force)
        {
            int size = sizeof(EventMatchTblHeader);
            foreach (ResourceNode node in Children)
                size += node.CalculateSize(true);
            return size;
        }
    }
}
