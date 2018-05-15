using System;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CollisionHeader
    {
        public const int Size = 0x28;

        public bshort _numPoints;
        public bshort _numPlanes;
        public bshort _numObjects;
        public bshort _unk1;
        public bint _pointOffset;
        public bint _planeOffset;
        public bint _objectOffset;
        internal fixed int _pad[5];

        public CollisionHeader(int numPoints, int numPlanes, int numObjects, int unk1)
        {
            _numPoints = (short)numPoints;
            _numPlanes = (short)numPlanes;
            _numObjects = (short)numObjects;
            _unk1 = (short)unk1;
            _pointOffset = 0x28;
            _planeOffset = 0x28 + (numPoints * 8);
            _objectOffset = 0x28 + (numPoints * 8) + (numPlanes * ColPlane.Size);

            fixed (int* p = _pad)
                for (int i = 0; i < 5; i++)
                    p[i] = 0;
        }

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }

        public BVec2* Points { get { return (BVec2*)(Address + _pointOffset); } }
        public ColPlane* Planes { get { return (ColPlane*)(Address + _planeOffset); } }
        public ColObject* Objects { get { return (ColObject*)(Address + _objectOffset); } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ColPlane
    {
        public const int Size = 0x10;

        public bshort _point1;
        public bshort _point2;
        public bshort _link1;
        public bshort _link2;
        public bint _magic; //-1
        public bushort _type;
        public CollisionPlaneFlags _flags;
        public CollisionPlaneMaterial _material;

        public ColPlane(int pInd1, int pInd2, int pLink1, int pLink2, CollisionPlaneType type, CollisionPlaneFlags2 flags2, CollisionPlaneFlags flags, CollisionPlaneMaterial material)
        {
            _point1 = (short)pInd1;
            _point2 = (short)pInd2;
            _link1 = (short)pLink1;
            _link2 = (short)pLink2;
            _magic = -1;
            _type = (ushort)((int)flags2 | (int)type);
            _flags = flags;
            _material = material;
        }

        public CollisionPlaneType Type { get { return (CollisionPlaneType)(_type & 0xF); } set { _type = (ushort)(_type & 0xFFF0 | (int)value); } }
        public CollisionPlaneFlags2 Flags2 { get { return (CollisionPlaneFlags2)(_type & 0xFFF0); } set { _type = (ushort)(_type & 0x000F | (int)value); } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ColObject
    {
        public const int Size = 0x6C;

        public bshort _planeIndex;
        public bshort _planeCount;
        public bint _unk1; //0
        public bint _unk2; //0
        public bint _unk3; //0
        public bushort _flags; //2
        public bshort _unk5; //0
        public BVec2 _boxMin;
        public BVec2 _boxMax;
        public bshort _pointOffset;
        public bshort _pointCount;
        public bshort _unk6; //0
        public bshort _boneIndex;
        public fixed byte _modelName[32];
        public fixed byte _boneName[32];

        public ColObject(int planeIndex, int planeCount, int pointOffset, int pointCount, Vector2 boxMin, Vector2 boxMax, string modelName, string boneName,
            int unk1, int unk2, int unk3, int flags, int unk5, int unk6, int boneIndex)
        {
            _planeIndex = (short)planeIndex;
            _planeCount = (short)planeCount;
            _unk1 = unk1;
            _unk2 = unk2;
            _unk3 = unk3;
            _flags = (ushort)flags;
            _unk5 = (short)unk5;
            _boxMin = boxMin;
            _boxMax = boxMax;
            _pointOffset = (short)pointOffset;
            _pointCount = (short)pointCount;
            _unk6 = (short)unk6;
            _boneIndex = (short)boneIndex;

            fixed (byte* p = _modelName)
                SetStr(p, modelName);

            fixed (byte* p = _boneName)
                SetStr(p, boneName);
        }

        public void Set(int planeIndex, int planeCount, Vector2 boxMin, Vector2 boxMax, string modelName, string boneName)
        {
            _planeIndex = (short)planeIndex;
            _planeCount = (short)planeCount;
            _unk1 = 0;
            _unk2 = 0;
            _unk3 = 0;
            _flags = 0;
            _boxMin = boxMin;
            _boxMax = boxMax;
            _unk5 = 0;
            _unk6 = 0;

            ModelName = modelName;
            BoneName = boneName;
        }

        private VoidPtr Address { get { fixed (void* p = &this)return p; } }

        public string ModelName
        {
            get { return new String((sbyte*)Address + 0x2C); }
            set { SetStr((byte*)Address + 0x2C, value); }
        }
        public string BoneName
        {
            get { return new String((sbyte*)Address + 0x4C); }
            set { SetStr((byte*)Address + 0x4C, value); }
        }

        private static void SetStr(byte* dPtr, string str)
        {
            int index = 0;
            if (str != null)
            {
                //Fill string
                int len = Math.Min(str.Length, 31);
                fixed (char* p = str)
                    while (index < len)
                        *dPtr++ = (byte)p[index++];
            }

            //Fill remaining
            while (index++ < 32)
                *dPtr++ = 0;
        }
    }

    public enum CollisionPlaneMaterial : byte
    {
        Basic = 0,                      // Used for many different objects
        Rock = 1,                       // Used for Spear Pillar lower floor, PS1 Mountain
        Grass = 2,                      // Used for grass or leaves
        Soil = 3,                       // Used for PS2 mountain
        Wood = 4,                       // Used for trees (PS1 Fire) and logs/planks (Jungle Japes)
        LightMetal = 5,                 // Used for thin metal platforms
        HeavyMetal = 6,                 // Used for thick metal platforms
        Carpet = 7,                     // Used by Rainbow Cruise
        Alien = 8,                      // Only used for Brinstar side platforms
        Bulborb = 9,                    // Used for Bulborb collision in Distant Planet
        Water = 0x0A,                   // Used for splash effects (Summit when sunk)
        Rubber = 0x0B,                  // Used for the Trowlon subspace enemy
        Slippery = 0x0C,                // Unknown where this is used, but has ice traction
        Snow = 0x0D,                    // Used for snowy surfaces that aren't slippery (SSE)
        SnowIce = 0x0E,                 // Used for Summit and PS2 Ice Transformation
        GameWatch = 0x0F,               // Used for all Flat Zone platforms
        SubspaceIce = 0x10,             // Used some places in Subspace (Purple floor where the door to Tabuu is)
        Checkered = 0x11,               // Used for Green Greens's checkerboard platforms and the present skin of rolling crates
        SpikesTargetTestOnly = 0x12,    // Used for Spike Hazards in Target Test levels and collision hazard #1 for SSE stages. Crashes or has no effect on stages not using a target test module
        Hazard2SSEOnly = 0x13,          // Used for hitboxes on certain SSE levels (180002). Crashes or has no effect on versus stages.
        Hazard3SSEOnly = 0x14,          // Used for hitboxes on certain SSE levels. Crashes or has no effect on versus stages.
        Electroplankton = 0x15,         // Used for Hanenbow leaves
        Cloud = 0x16,                   // Used for clouds on Summit and Skyworld
        Subspace = 0x17,                // Used for Subspace levels, Tabuu's Residence
        Stone = 0x18,                   // Used for Spear Pillar upper level
        UnknownDustless = 0x19,         // Unknown, doesn't generate dust clouds when landing
        MarioBros = 0x1A,               // Used for Mario Bros.
        Grate = 0x1B,                   // Used for Delfino Plaza's main platform
        Sand = 0x1C,                    // Used for sand (Unknown where used)
        Homerun = 0x1D,                 // Used for Home Run Contest, makes Olimar only spawn Purple Pikmin
        WaterNoSplash = 0x1E,           // Used for Distant Planet slope during rain
        Unknown0x1F = 0x1F,             // 
        CollEx32 = 0x20,                // Expanded collisions, require SCLA edits or they won't work properly
        CollEx33 = 0x21,
        CollEx34 = 0x22,
        CollEx35 = 0x23,
        CollEx36 = 0x24,
        CollEx37 = 0x25,
        CollEx38 = 0x26,
        CollEx39 = 0x27,
        CollEx40 = 0x28,
        CollEx41 = 0x29,
        CollEx42 = 0x2A,
        CollEx43 = 0x2B,
        CollEx44 = 0x2C,
        CollEx45 = 0x2D,
        CollEx46 = 0x2E,
        CollEx47 = 0x2F,
        CollEx48 = 0x30,
        CollEx49 = 0x31,
        CollEx50 = 0x32,
        CollEx51 = 0x33,
        CollEx52 = 0x34,
        CollEx53 = 0x35,
        CollEx54 = 0x36,
        CollEx55 = 0x37,
        CollEx56 = 0x38,
        CollEx57 = 0x39,
        CollEx58 = 0x3A,
        CollEx59 = 0x3B,
        CollEx60 = 0x3C,
        CollEx61 = 0x3D,
        CollEx62 = 0x3E,
        CollEx63 = 0x3F,
        CollEx64 = 0x40,
        CollEx65 = 0x41,
        CollEx66 = 0x42,
        CollEx67 = 0x43,
        CollEx68 = 0x44,
        CollEx69 = 0x45,
        CollEx70 = 0x46,
        CollEx71 = 0x47,
        CollEx72 = 0x48,
        CollEx73 = 0x49,
        CollEx74 = 0x4A,
        CollEx75 = 0x4B,
        CollEx76 = 0x4C,
        CollEx77 = 0x4D,
        CollEx78 = 0x4E,
        CollEx79 = 0x4F,
        CollEx80 = 0x50,
        CollEx81 = 0x51,
        CollEx82 = 0x52,
        CollEx83 = 0x53,
        CollEx84 = 0x54,
        CollEx85 = 0x55,
        CollEx86 = 0x56,
        CollEx87 = 0x57,
        CollEx88 = 0x58,
        CollEx89 = 0x59,
        CollEx90 = 0x5A,
        CollEx91 = 0x5B,
        CollEx92 = 0x5C,
        CollEx93 = 0x5D,
        CollEx94 = 0x5E,
        CollEx95 = 0x5F,
        CollEx96 = 0x60,
        CollEx97 = 0x61,
        CollEx98 = 0x62,
        CollEx99 = 0x63,
        CollEx100 = 0x64,
        CollEx101 = 0x65,
        CollEx102 = 0x66,
        CollEx103 = 0x67,
        CollEx104 = 0x68,
        CollEx105 = 0x69,
        CollEx106 = 0x6A,
        CollEx107 = 0x6B,
        CollEx108 = 0x6C,
        CollEx109 = 0x6D,
        CollEx110 = 0x6E,
        CollEx111 = 0x6F,
        CollEx112 = 0x70,
        CollEx113 = 0x71,
        CollEx114 = 0x72,
        CollEx115 = 0x73,
        CollEx116 = 0x74,
        CollEx117 = 0x75,
        CollEx118 = 0x76,
        CollEx119 = 0x77,
        CollEx120 = 0x78,
        CollEx121 = 0x79,
        CollEx122 = 0x7A,
        CollEx123 = 0x7B,
        CollEx124 = 0x7C,
        CollEx125 = 0x7D,
        CollEx126 = 0x7E,
        CollEx127 = 0x7F,
        CollEx128 = 0x80,
        CollEx129 = 0x81,
        CollEx130 = 0x82,
        CollEx131 = 0x83,
        CollEx132 = 0x84,
        CollEx133 = 0x85,
        CollEx134 = 0x86,
        CollEx135 = 0x87,
        CollEx136 = 0x88,
        CollEx137 = 0x89,
        CollEx138 = 0x8A,
        CollEx139 = 0x8B,
        CollEx140 = 0x8C,
        CollEx141 = 0x8D,
        CollEx142 = 0x8E,
        CollEx143 = 0x8F,
        CollEx144 = 0x90,
        CollEx145 = 0x91,
        CollEx146 = 0x92,
        CollEx147 = 0x93,
        CollEx148 = 0x94,
        CollEx149 = 0x95,
        CollEx150 = 0x96,
        CollEx151 = 0x97,
        CollEx152 = 0x98,
        CollEx153 = 0x99,
        CollEx154 = 0x9A,
        CollEx155 = 0x9B,
        CollEx156 = 0x9C,
        CollEx157 = 0x9D,
        CollEx158 = 0x9E,
        CollEx159 = 0x9F,
        CollEx160 = 0xA0,
        CollEx161 = 0xA1,
        CollEx162 = 0xA2,
        CollEx163 = 0xA3,
        CollEx164 = 0xA4,
        CollEx165 = 0xA5,
        CollEx166 = 0xA6,
        CollEx167 = 0xA7,
        CollEx168 = 0xA8,
        CollEx169 = 0xA9,
        CollEx170 = 0xAA,
        CollEx171 = 0xAB,
        CollEx172 = 0xAC,
        CollEx173 = 0xAD,
        CollEx174 = 0xAE,
        CollEx175 = 0xAF,
        CollEx176 = 0xB0,
        CollEx177 = 0xB1,
        CollEx178 = 0xB2,
        CollEx179 = 0xB3,
        CollEx180 = 0xB4,
        CollEx181 = 0xB5,
        CollEx182 = 0xB6,
        CollEx183 = 0xB7,
        CollEx184 = 0xB8,
        CollEx185 = 0xB9,
        CollEx186 = 0xBA,
        CollEx187 = 0xBB,
        CollEx188 = 0xBC,
        CollEx189 = 0xBD,
        CollEx190 = 0xBE,
        CollEx191 = 0xBF,
        CollEx192 = 0xC0,
        CollEx193 = 0xC1,
        CollEx194 = 0xC2,
        CollEx195 = 0xC3,
        CollEx196 = 0xC4,
        CollEx197 = 0xC5,
        CollEx198 = 0xC6,
        CollEx199 = 0xC7,
        CollEx200 = 0xC8,
        CollEx201 = 0xC9,
        CollEx202 = 0xCA,
        CollEx203 = 0xCB,
        CollEx204 = 0xCC,
        CollEx205 = 0xCD,
        CollEx206 = 0xCE,
        CollEx207 = 0xCF,
        CollEx208 = 0xD0,
        CollEx209 = 0xD1,
        CollEx210 = 0xD2,
        CollEx211 = 0xD3,
        CollEx212 = 0xD4,
        CollEx213 = 0xD5,
        CollEx214 = 0xD6,
        CollEx215 = 0xD7,
        CollEx216 = 0xD8,
        CollEx217 = 0xD9,
        CollEx218 = 0xDA,
        CollEx219 = 0xDB,
        CollEx220 = 0xDC,
        CollEx221 = 0xDD,
        CollEx222 = 0xDE,
        CollEx223 = 0xDF,
        CollEx224 = 0xE0,
        CollEx225 = 0xE1,
        CollEx226 = 0xE2,
        CollEx227 = 0xE3,
        CollEx228 = 0xE4,
        CollEx229 = 0xE5,
        CollEx230 = 0xE6,
        CollEx231 = 0xE7,
        CollEx232 = 0xE8,
        CollEx233 = 0xE9,
        CollEx234 = 0xEA,
        CollEx235 = 0xEB,
        CollEx236 = 0xEC,
        CollEx237 = 0xED,
        CollEx238 = 0xEE,
        CollEx239 = 0xEF,
        CollEx240 = 0xF0,
        CollEx241 = 0xF1,
        CollEx242 = 0xF2,
        CollEx243 = 0xF3,
        CollEx244 = 0xF4,
        CollEx245 = 0xF5,
        CollEx246 = 0xF6,
        CollEx247 = 0xF7,
        CollEx248 = 0xF8,
        CollEx249 = 0xF9,
        CollEx250 = 0xFA,
        CollEx251 = 0xFB,
        CollEx252 = 0xFC,
        CollEx253 = 0xFD,
        CollEx254 = 0xFE,
        CollEx255 = 0xFF
    }

    public enum CollisionPlaneMaterialUnexpanded : byte
    {
        Basic = 0,                      // Used for many different objects
        Rock = 1,                       // Used for Spear Pillar lower floor, PS1 Mountain
        Grass = 2,                      // Used for grass or leaves
        Soil = 3,                       // Used for PS2 mountain
        Wood = 4,                       // Used for trees (PS1 Fire) and logs/planks (Jungle Japes)
        LightMetal = 5,                 // Used for thin metal platforms
        HeavyMetal = 6,                 // Used for thick metal platforms
        Carpet = 7,                     // Used by Rainbow Cruise
        Alien = 8,                      // Only used for Brinstar side platforms
        Bulborb = 9,                    // Used for Bulborb collision in Distant Planet
        Water = 0x0A,                   // Used for splash effects (Summit when sunk)
        Rubber = 0x0B,                  // Used for the Trowlon subspace enemy
        Slippery = 0x0C,                // Unknown where this is used, but has ice traction
        Snow = 0x0D,                    // Used for snowy surfaces that aren't slippery (SSE)
        SnowIce = 0x0E,                 // Used for Summit and PS2 Ice Transformation
        GameWatch = 0x0F,               // Used for all Flat Zone platforms
        SubspaceIce = 0x10,             // Used some places in Subspace (Purple floor where the door to Tabuu is)
        Checkered = 0x11,               // Used for Green Greens's checkerboard platforms and the present skin of rolling crates
        SpikesTargetTestOnly = 0x12,    // Used for Spike Hazards in Target Test levels and collision hazard #1 for SSE stages. Crashes or has no effect on stages not using a target test module
        Hazard2SSEOnly = 0x13,          // Used for hitboxes on certain SSE levels (180002). Crashes or has no effect on versus stages.
        Hazard3SSEOnly = 0x14,          // Used for hitboxes on certain SSE levels. Crashes or has no effect on versus stages.
        Electroplankton = 0x15,         // Used for Hanenbow leaves
        Cloud = 0x16,                   // Used for clouds on Summit and Skyworld
        Subspace = 0x17,                // Used for Subspace levels, Tabuu's Residence
        Stone = 0x18,                   // Used for Spear Pillar upper level
        UnknownDustless = 0x19,         // Unknown, doesn't generate dust clouds when landing
        MarioBros = 0x1A,               // Used for Mario Bros.
        Grate = 0x1B,                   // Used for Delfino Plaza's main platform
        Sand = 0x1C,                    // Used for sand (Unknown where used)
        Homerun = 0x1D,                 // Used for Home Run Contest, makes Olimar only spawn Purple Pikmin
        WaterNoSplash = 0x1E,           // Used for Distant Planet slope during rain
        Unknown0x1F = 0x1F              // 
    }

    public enum CollisionPlaneType
    {
        None = 0x0000,          // 0000
        Floor = 0x0001,         // 0001
        Ceiling = 0x0002,       // 0010
        RightWall = 0x0004,     // 0100
        LeftWall = 0x0008       // 1000
    }

    [Flags]
    public enum CollisionPlaneFlags2
    {
        None = 0x0000,
        Characters = 0x0010,        // Characters (Also allows Items and PT to interact)
        Items = 0x0020,             // Items
        PokemonTrainer = 0x0040,    // Pokemon Trainer
        UnknownStageBox = 0x0080    // Unknown, used in the SSE
    }

    [Flags]
    public enum CollisionPlaneFlags : byte
    {
        None = 0x00,
        DropThrough = 0x01,         // Can fall through a floor by pressing down
        Unknown1 = 0x02,            // 
        Rotating = 0x04,            // Automatically changes between floor/wall/ceiling based on angle
        Unknown3 = 0x08,            // 
        Unknown4 = 0x10,            //
        LeftLedge = 0x20,           // Can grab ledge from the left
        RightLedge = 0x40,          // Can grab ledge from the right
        NoWalljump = 0x80           // Cannot walljump off when set
    }
}
