using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using BrawlLib.IO;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.OpenGL;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Ikarus;
using BrawlLib.Modeling;
using System.Windows.Forms;

namespace Ikarus.MovesetFile
{
    public unsafe class MiscSectionNode : MovesetEntryNode
    {
        sDataMisc _hdr;

        [Category("Misc Offsets")]
        public int UnknownSection1Offset { get { return _hdr.UnknownSection1Offset; } }
        [Category("Misc Offsets")]
        public int FinalSmashAuraOffset { get { return _hdr.FinalSmashAuraOffset; } }
        [Category("Misc Offsets")]
        public int FinalSmashAuraCount { get { return _hdr.FinalSmashAuraCount; } }
        [Category("Misc Offsets")]
        public int HurtBoxOffset { get { return _hdr.HurtBoxOffset; } }
        [Category("Misc Offsets")]
        public int HurtBoxCount { get { return _hdr.HurtBoxCount; } }
        [Category("Misc Offsets")]
        public int LedgegrabOffset { get { return _hdr.LedgegrabOffset; } }
        [Category("Misc Offsets")]
        public int LedgegrabCount { get { return _hdr.LedgegrabCount; } }
        [Category("Misc Offsets")]
        public int UnknownSection2Offset { get { return _hdr.UnknownSection2Offset; } }
        [Category("Misc Offsets")]
        public int UnknownSection2Count { get { return _hdr.UnknownSection2Count; } }
        [Category("Misc Offsets")]
        public int BoneRefOffset { get { return _hdr.BoneRef2Offset; } }
        [Category("Misc Offsets")]
        public int UnknownSection3Offset { get { return _hdr.UnknownSection3Offset; } }
        [Category("Misc Offsets")]
        public int SoundDataOffset { get { return _hdr.SoundDataOffset; } }
        [Category("Misc Offsets")]
        public int UnkSection5Offset { get { return _hdr.UnknownSection5Offset; } }
        [Category("Misc Offsets")]
        public int MultiJumpOffset { get { return _hdr.MultiJumpOffset; } }
        [Category("Misc Offsets")]
        public int GlideOffset { get { return _hdr.GlideOffset; } }
        [Category("Misc Offsets")]
        public int CrawlOffset { get { return _hdr.CrawlOffset; } }
        [Category("Misc Offsets")]
        public int CollisionData { get { return _hdr.CollisionDataOffset; } }
        [Category("Misc Offsets")]
        public int TetherOffset { get { return _hdr.TetherOffset; } }
        [Category("Misc Offsets")]
        public int UnknownSection12Offset { get { return _hdr.UnknownSection12Offset; } }

        public CollisionData _collisionData;
        public EntryListOffset<IndexValue> _unknown18;
        public MiscTether _tether;
        public EntryList<IndexValue> _unknown1;
        public EntryList<MiscFinalSmashAura> _finalSmashAura;
        public EntryList<MiscHurtBox> _hurtBoxes;
        public EntryList<MiscLedgeGrab> _ledgeGrabs;
        public EntryList<MiscUnk7Entry> _unknown7;
        public EntryList<BoneIndexValue> _boneRefs;
        public MiscUnk10 _unknown10;
        public MiscSoundData _soundData;
        public MiscUnk12 _unkSection5;
        public MiscMultiJump _multiJump;
        public MiscGlide _glide;
        public MiscCrawl _crawl;

        protected override void OnParse(VoidPtr address)
        {
            //Get header values
            _hdr = *(sDataMisc*)address;

            //Parse all misc entries.
            //If an offset is 0, the entry will be set to null.
            _unknown1 = Parse<EntryList<IndexValue>>(_hdr[0], 4);
            _finalSmashAura = Parse<EntryList<MiscFinalSmashAura>>(_hdr[1], 0x14, _hdr[2]);
            _hurtBoxes = Parse<EntryList<MiscHurtBox>>(_hdr[3], 0x20, _hdr[4]);
            _ledgeGrabs = Parse<EntryList<MiscLedgeGrab>>(_hdr[5], 0x10, _hdr[6]);
            _unknown7 = Parse<EntryList<MiscUnk7Entry>>(_hdr[7], 0x20, _hdr[8]);
            _boneRefs = Parse<EntryList<BoneIndexValue>>(_hdr[9], 4, 10);
            _unknown10 = Parse<MiscUnk10>(_hdr[10]);
            _soundData = Parse<MiscSoundData>(_hdr[11]);
            _unkSection5 = Parse<MiscUnk12>(_hdr[12]);
            _multiJump = Parse<MiscMultiJump>(_hdr[13]);
            _glide = Parse<MiscGlide>(_hdr[14]);
            _crawl = Parse<MiscCrawl>(_hdr[15]);
            _collisionData = Parse<CollisionData>(_hdr[16]);
            _tether = Parse<MiscTether>(_hdr[17]);
            _unknown18 = Parse<EntryListOffset<IndexValue>>(_hdr[18], 4);
        }
    }
}