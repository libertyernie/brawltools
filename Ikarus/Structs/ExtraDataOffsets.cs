using BrawlLib.SSBBTypes;
using Ikarus.MovesetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ikarus.MovesetFile
{
    public interface OffsetHolder
    {
        void Parse(DataSection node, VoidPtr address);
        void Write(List<MovesetEntryNode> entries, LookupManager lookup, VoidPtr basePtr, VoidPtr address);
        int Count { get; }
    }
    public unsafe class ExtraDataOffsets
    {
        //How to parse the articles for a character:
        //Copy and paste one of the classes below (ex, Link)
        //Then rename it to the character and uncomment the character in the function below
        //Then using Brawlbox v0.68b, you can view the data offsets
        //by opening the character's moveset file and going to MoveDef_FitChar->Sections->data
        //Go the properties and view the ExtraOffsets collection.
        //Use this to set up the struct for the character accordingly.
        //You can view the int offset of each article in the Articles list under data,
        //So just match those offsets to the list, get the index and use it to parse them.
        public static OffsetHolder GetOffsets(CharName character)
        {
            switch (character)
            {
                //case CharName.CaptainFalcon:
                //    return new CaptainFalcon();
                //case CharName.KingDedede:
                //    return new KingDedede();
                //case CharName.DiddyKong:
                //    return new DiddyKong();
                //case CharName.DonkeyKong:
                //    return new DonkeyKong();
                //case CharName.Falco:
                //    return new Falco();
                //case CharName.Fox:
                //    return new Fox();
                //case CharName.MrGameNWatch:
                //    return new MrGameNWatch();
                //case CharName.Ganondorf:
                //    return new Ganondorf();
                //case CharName.GigaBowser:
                //    return new GigaBowser();
                //case CharName.Ike:
                //    return new Ike();
                //case CharName.Kirby:
                //    return new Kirby();
                //case CharName.Bowser:
                //    return new Bowser();
                case CharName.Link:
                    return new Link();
                //case CharName.Lucario:
                //    return new Lucario();
                //case CharName.Lucas:
                //    return new Lucas();
                //case CharName.Luigi:
                //    return new Luigi();
                case CharName.Mario:
                    return new Mario();
                //case CharName.Marth:
                //    return new Marth();
                //case CharName.Metaknight:
                //    return new Metaknight();
                //case CharName.Ness:
                //    return new Ness();
                case CharName.Peach:
                    return new Peach();
                //case CharName.Pikachu:
                //    return new Pikachu();
                //case CharName.Pikmin:
                //    return new Pikmin();
                case CharName.Pit:
                    return new Pit();
                //case CharName.Ivysaur:
                //    return new Ivysaur();
                //case CharName.Charizard:
                //    return new Charizard();
                //case CharName.PokemonTrainer:
                //    return new PokemonTrainer();
                //case CharName.Squirtle:
                //    return new Squirtle();
                //case CharName.Popo:
                //    return new Popo();
                //case CharName.Jigglypuff:
                //    return new Jigglypuff();
                //case CharName.ROB:
                //    return new ROB();
                //case CharName.Samus:
                //    return new Samus();
                //case CharName.Sheik:
                //    return new Sheik();
                //case CharName.Snake:
                //    return new Snake();
                //case CharName.Sonic:
                //    return new Sonic();
                case CharName.ZeroSuitSamus:
                    return new ZeroSuitSamus();
                //case CharName.ToonLink:
                //    return new ToonLink();
                //case CharName.Wario:
                //    return new Wario();
                //case CharName.WarioMan:
                //    return new WarioMan();
                //case CharName.Wolf:
                //    return new Wolf();
                //case CharName.Yoshi:
                //    return new Yoshi();
                //case CharName.GreenAlloy:
                //    return new GreenAlloy();
                //case CharName.RedAlloy:
                //    return new RedAlloy();
                //case CharName.YellowAlloy:
                //    return new YellowAlloy();
                //case CharName.BlueAlloy:
                //    return new BlueAlloy();
                case CharName.Zelda:
                    return new Zelda();
            }
            return null;
        }

        public class Mario : OffsetHolder
        {
            public int Count { get { return Offsets.Count; } }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public unsafe struct Offsets
            {
                public const int Count = 10;

                public buint _params3;
                public buint _params4;
                public buint _params1;
                public buint _params5;
                public buint _params2;
                public buint _article2;
                public buint _article1;
                public buint _article3;
                public buint _article4;
                public buint _article5;

                public buint* Entries { get { return (buint*)Address; } }
                private VoidPtr Address { get { fixed (void* p = &this)return p; } }
                
                public int Size { get { return Count * 4; } }
            }

            public void Parse(DataSection node, VoidPtr address)
            {
                Offsets* addr = (Offsets*)address;
                for (int i = 0; i < 5; i++)
                    node._extraEntries.Add(node.Parse<RawParamList>((int)addr->Entries[i]));
                for (int i = 5; i < 10; i++)
                    node._articles.Add((int)addr->Entries[i], node.Parse<ArticleNode>((int)addr->Entries[i]));
            }

            public void Write(List<MovesetEntryNode> entries, LookupManager lookup, VoidPtr basePtr, VoidPtr address)
            {
                Offsets* addr = (Offsets*)address;
                int i = 0;
                foreach (MovesetEntryNode e in entries)
                {
                    addr->Entries[i] = (uint)e.RebuildAddress - (uint)basePtr;
                    lookup.Add(&addr->Entries[i++]);
                }
            }
        }
        public class Link : OffsetHolder
        {
            public int Count { get { return Offsets.Count; } }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public unsafe struct Offsets
            {
                public const int Count = 20;

                public buint _params2;
                public buint _params4;
                public buint _params5;
                public buint _params3;
                public buint _params7;
                public buint _params1;
                public buint _params8;
                public sListOffset _hitDataList;
                public uint _count2;
                public buint _count3;
                public buint _article1;
                public buint _article1Count;
                public buint _article2;
                public buint _article2Count;
                public buint _article3;
                public buint _article4;
                public buint _article5;
                public buint _article6;
                public buint _article7;

                public buint* Entries { get { return (buint*)Address; } }
                private VoidPtr Address { get { fixed (void* p = &this)return p; } }

                public int Size { get { return Count * 4; } }
            }

            public void Parse(DataSection node, VoidPtr address)
            {
                Offsets* addr = (Offsets*)address;
                for (int i = 0; i < 7; i++)
                    node._extraEntries.Add(node.Parse<RawParamList>((int)addr->Entries[i]));
                for (int i = 11; i < 20; i++)
                    if (i != 12 && i != 14)
                        node._articles.Add((int)addr->Entries[i], node.Parse<ArticleNode>((int)addr->Entries[i]));
            }

            public void Write(List<MovesetEntryNode> entries, LookupManager lookup, VoidPtr basePtr, VoidPtr address)
            {

            }
        }
        public class ZeroSuitSamus : OffsetHolder
        {
            public int Count { get { return Offsets.Count; } }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public unsafe struct Offsets
            {
                public const int Count = 12;

                public buint _params1;
                public buint _params2;
                public buint _params3;
                public buint _params4;
                public buint _article1;
                public buint _article2;
                public buint _article3;
                public buint _article4;
                public sListOffset _extraOffset8;
                public sListOffset _params5;

                public buint* Entries { get { return (buint*)Address; } }
                private VoidPtr Address { get { fixed (void* p = &this)return p; } }

                public int Size { get { return Count * 4; } }
            }

            public void Parse(DataSection node, VoidPtr address)
            {
                Offsets* addr = (Offsets*)address;
                for (int i = 4; i < 8; i++)
                {
                    int x = (int)addr->Entries[i];
                    node._articles.Add(x, node.Parse<ArticleNode>(x));
                }
            }

            public void Write(List<MovesetEntryNode> entries, LookupManager lookup, VoidPtr basePtr, VoidPtr address)
            {

            }
        }
        public class Pit : OffsetHolder
        {
            public int Count { get { return Offsets.Count; } }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public unsafe struct Offsets
            {
                public const int Count = 13;

                public buint _params1;
                public buint _params2;
                public buint _params4;
                public buint _params5;
                public sListOffset _hitDataList1;
                public uint _unknown;
                public buint _specialHitDataList;
                public buint _params3;
                public buint _article1;
                public buint _article2;
                public buint _article3;
                public buint _article4;

                public buint* Entries { get { return (buint*)Address; } }
                private VoidPtr Address { get { fixed (void* p = &this)return p; } }

                public int Size { get { return Count * 4; } }
            }

            public void Parse(DataSection node, VoidPtr address)
            {
                Offsets* addr = (Offsets*)address;
                for (int i = 9; i < 13; i++)
                {
                    int x = (int)addr->Entries[i];
                    node._articles.Add(x, node.Parse<ArticleNode>(x));
                }
            }

            public void Write(List<MovesetEntryNode> entries, LookupManager lookup, VoidPtr basePtr, VoidPtr address)
            {

            }
        }
        public class Peach : OffsetHolder
        {
            public int Count { get { return Offsets.Count; } }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public unsafe struct Offsets
            {
                public const int Count = 14;
                
                public buint _params1;
                public buint _params2;
                public buint _params3;
                public buint _params4;
                public buint _params5;
                public buint _params6;
                public sListOffset _hitDataList1;
                public sListOffset _hitDataList2;
                public buint _unknown; //0
                public buint _article1;
                public buint _article2;
                public buint _article3;

                public buint* Entries { get { return (buint*)Address; } }
                private VoidPtr Address { get { fixed (void* p = &this)return p; } }

                public int Size { get { return Count * 4; } }
            }

            public void Parse(DataSection node, VoidPtr address)
            {
                Offsets* addr = (Offsets*)address;
                for (int i = 11; i < 14; i++)
                {
                    int x = (int)addr->Entries[i];
                    node._articles.Add(x, node.Parse<ArticleNode>(x));
                }
            }

            public void Write(List<MovesetEntryNode> entries, LookupManager lookup, VoidPtr basePtr, VoidPtr address)
            {

            }
        }

        public class Zelda : OffsetHolder
        {
            public int Count { get { return Offsets.Count; } }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public unsafe struct Offsets
            {
                public const int Count = 10;

                public buint _params1;
                public buint _params2;
                public buint _params3;
                public buint _params4;
                public buint _params5;
                public buint _params6;
                public buint _article1;
                public buint _article2;
                public buint _article3;
                public buint _article4;

                public buint* Entries { get { return (buint*)Address; } }
                private VoidPtr Address { get { fixed (void* p = &this)return p; } }

                public int Size { get { return Count * 4; } }
            }

            public void Parse(DataSection node, VoidPtr address)
            {
                Offsets* addr = (Offsets*)address;
                for (int i = 6; i < 10; i++)
                {
                    int x = (int)addr->Entries[i];
                    node._articles.Add(x, node.Parse<ArticleNode>(x));
                }
            }

            public void Write(List<MovesetEntryNode> entries, LookupManager lookup, VoidPtr basePtr, VoidPtr address)
            {

            }
        }
    }
    //    case CharFolder.ZakoBall:
    //    case CharFolder.ZakoBoy:
    //    case CharFolder.ZakoGirl:
    //    case CharFolder.ZakoChild:
    //        ExtraDataOffsets._count = 1; break;
    //    case CharFolder.Purin:
    //        ExtraDataOffsets._count = 3; break;
    //    case CharFolder.Koopa:
    //    case CharFolder.Metaknight:
    //        ExtraDataOffsets._count = 5; break;
    //    case CharFolder.Ganon:
    //    case CharFolder.GKoopa:
    //    case CharFolder.Marth:
    //        ExtraDataOffsets._count = 6; break;
    //    case CharFolder.PokeFushigisou:
    //        ExtraDataOffsets._count = 7; break;
    //    case CharFolder.Captain:
    //    case CharFolder.Ike:
    //    case CharFolder.Luigi:
    //    case CharFolder.PokeLizardon:
    //    case CharFolder.PokeTrainer:
    //    case CharFolder.PokeZenigame:
    //    case CharFolder.Sonic:
    //        ExtraDataOffsets._count = 8; break;
    //    case CharFolder.Donkey:
    //    case CharFolder.Sheik:
    //    case CharFolder.WarioMan:
    //        ExtraDataOffsets._count = 9; break;
    //    case CharFolder.Mario:
    //    case CharFolder.Wario:
    //    case CharFolder.Zelda:
    //        ExtraDataOffsets._count = 10; break;
    //    case CharFolder.Falco:
    //    case CharFolder.Lucario:
    //    case CharFolder.Pikachu:
    //        ExtraDataOffsets._count = 11; break;
    //    case CharFolder.SZerosuit:
    //        ExtraDataOffsets._count = 12; break;
    //    case CharFolder.Diddy:
    //    case CharFolder.Fox:
    //    case CharFolder.Lucas:
    //    case CharFolder.Pikmin:
    //    case CharFolder.Pit:
    //    case CharFolder.Wolf:
    //    case CharFolder.Yoshi:
    //        ExtraDataOffsets._count = 13; break;
    //    case CharFolder.Ness:
    //    case CharFolder.Peach:
    //    case CharFolder.Robot:
    //        ExtraDataOffsets._count = 14; break;
    //    case CharFolder.Dedede:
    //    case CharFolder.Gamewatch:
    //        ExtraDataOffsets._count = 16; break;
    //    case CharFolder.Popo:
    //        ExtraDataOffsets._count = 18; break;
    //    case CharFolder.Link:
    //    case CharFolder.Snake:
    //    case CharFolder.ToonLink:
    //        ExtraDataOffsets._count = 20; break;
    //    case CharFolder.Samus:
    //        ExtraDataOffsets._count = 22; break;
    //    case CharFolder.Kirby:
    //        ExtraDataOffsets._count = 68; break;
    //    default: //Only works on movesets untouched by PSA
    //        ExtraDataOffsets._count = (Size - 124) / 4; break;
}
