using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BrawlLib.SSBB.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EventMatchTblHeader
    {
        public bint _unknown00;
        public bint _unknown04;
        public byte _matchType;
        public byte _unknown09;
        public byte _unknown0a;
        public byte _unknown0b;
        public bint _timeLimit;
        // _unknown10 = timer visibility?
        public byte _unknown10;
        public byte _unknown11;
        public byte _unknown12;
        public byte _unknown13;
        public bfloat _unknown14;
        public bshort _itemFrequency;
        public bshort _unknown1a;
        public byte _unknown1c;
        public byte _unknown1d;
        public byte _unknown1e;
        public byte _stageID;
        public byte _unknown20;
        public byte _unknown21;
        public byte _unknown22;
        public byte _unknown23;
        public bint _unknown24;
        public bint _unknown28;
        public bint _unknown2c;
        public bint _unknown30;
        public bint _unknown34;
        public bfloat _gameSpeed;
        public bfloat _cameraShakeControl;
        public byte _unknown40;
        public byte _unknown41;
        public byte _unknown42;
        public byte _unknown43;
        public byte _unknown44;
        public byte _unknown45;
        public bshort _songID;
        public bshort _globalOffenseRatio;
        public bshort _globalDefenseRatio;
        public bint _unknown4c;

        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public EventMatchFighterData* FighterDataPtr
        {
            get
            {
                fixed (EventMatchTblHeader* ptr = &this)
                {
                    return (EventMatchFighterData*)(ptr + 1);
                }
            }
        }
    }

    public unsafe struct EventMatchFighterData
    {
        public byte _fighterID;
        public byte _status; // normal, metal, invisible
        public byte _unknown02;
        public byte _unknown03;
        public bfloat _scale;
        public byte _team;
        public byte _unknown09;
        public byte _unknown0a;
        public byte _unknown0b;
        public EventMatchDifficultyData _easy;
        public EventMatchDifficultyData _normal;
        public EventMatchDifficultyData _hard;
    }

    public unsafe struct EventMatchDifficultyData
    {
        public byte _cpuLevel;
        public byte _unknown01;
        public byte _unknown02;
        public byte _offenseRatio;
        public byte _unknown04;
        public byte _defenseRatio;
        public byte _unknown06;
        public byte _unknown07;
        public byte _stockCount;
        public byte _unknown09;
        public byte _unknown0a;
        public byte _unknown0b;
        public bshort _startingDamage;
    }
}
