using System;

namespace BrawlLib.SSBB
{
    public class Stage {
        /// <summary>
        /// The stage ID, as used by the module files and the Custom SSS code.
        /// </summary>
        public byte ID { get; private set; }
        /// <summary>
        /// The stage name (e.g. "Flat Zone 2").
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The .rel filename (e.g. "st_gw.rel").
        /// </summary>
        public string RelName { get; private set; }
        /// <summary>
        /// The name of the .pac file, minus "STG" and anything    
        /// after an underscore (e.g. "gw" or "emblem").
        /// </summary>
        public string PacBasename { get; private set; }

        public bool ContainsPac(string filename) {
            int i = filename.IndexOfAny(new char[] { '.', '_' });
            if (filename.Length < 3 || i < 0) return false;

            string input_basename = filename.Substring(3, i - 3);
            return String.Equals(input_basename.ToLower(), PacBasename.ToLower(), StringComparison.InvariantCultureIgnoreCase);
        }

        public Stage(byte id, string name, string relname, string pac_basename) {
            this.ID = id;
            this.Name = name;
            this.RelName = relname;
            this.PacBasename = pac_basename;
        }

        public override string ToString() { return Name; }

        public string[] PacNames {
            get {
                string s = PacBasename;
                return s == "starfox" ? new string[] { "STGSTARFOX_GDIFF.pac" } :
                        s == "emblem" ? new string[] {
                                "STGEMBLEM_00.pac",
                                "STGEMBLEM_01.pac",
                                "STGEMBLEM_02.pac" } :
                        s == "mariopast" ? new string[] {
                                "STGMARIOPAST_00.pac",
                                "STGMARIOPAST_01.pac" } :
                        s == "metalgear" ? new string[] {
                                "STGMETALGEAR_00.pac",
                                "STGMETALGEAR_01.pac",
                                "STGMETALGEAR_02.pac" } :
                        s == "tengan" ? new string[] {
                                "STGTENGAN_1.pac",
                                "STGTENGAN_2.pac",
                                "STGTENGAN_3.pac" } :
                        s == "village" ? new string[] {
                                "STGVILLAGE_00.pac",
                                "STGVILLAGE_01.pac",
                                "STGVILLAGE_02.pac",
                                "STGVILLAGE_03.pac",
                                "STGVILLAGE_04.pac" } :
                        s == "custom" ? new string[0] :
                        new string[] { "STG" + s.ToUpper() + ".pac" };
            }
        }

        public readonly static Stage[] Stages = new Stage[] {
            //        ID    Display Name                .rel filename           Name without STG
            new Stage(0x00, "STGCUSTOM##.pac",          "st_custom##.rel",      "custom"),
            new Stage(0x01, "Battlefield",              "st_battle.rel",        "battlefield"),
            new Stage(0x02, "Final Destination",        "st_final.rel",         "final"),
            new Stage(0x03, "Delfino Plaza",            "st_dolpic.rel",        "dolpic"),
            new Stage(0x04, "Luigi's Mansion",          "st_mansion.rel",       "mansion"),
            new Stage(0x05, "Mushroomy Kingdom",        "st_mariopast.rel",     "mariopast"),
            new Stage(0x06, "Mario Circuit",            "st_kart.rel",          "kart"),
            new Stage(0x07, "75 m",                     "st_donkey.rel",        "donkey"),
            new Stage(0x08, "Rumble Falls",             "st_jungle.rel",        "jungle"),
            new Stage(0x09, "Pirate Ship",              "st_pirates.rel",       "pirates"),
            new Stage(0x0B, "Norfair",                  "st_norfair.rel",       "norfair"),
            new Stage(0x0C, "Frigate Orpheon",          "st_orpheon.rel",       "orpheon"),
            new Stage(0x0D, "Yoshi's Island (Brawl)",   "st_crayon.rel",        "crayon"),
            new Stage(0x0E, "Halberd",                  "st_halberd.rel",       "halberd"),
            new Stage(0x13, "Lylat Cruise",             "st_starfox.rel",       "starfox"),
            new Stage(0x14, "Pokemon Stadium 2",        "st_stadium.rel",       "stadium"),
            new Stage(0x15, "Spear Pillar",             "st_tengan.rel",        "tengan"),
            new Stage(0x16, "Port Town Aero Dive",      "st_fzero.rel",         "fzero"),
            new Stage(0x17, "Summit",                   "st_ice.rel",           "ice"),
            new Stage(0x18, "Flat Zone 2",              "st_gw.rel",            "gw"),
            new Stage(0x19, "Castle Siege",             "st_emblem.rel",        "emblem"),
            new Stage(0x1C, "WarioWare Inc.",           "st_madein.rel",        "madein"),
            new Stage(0x1D, "Distant Planet",           "st_earth.rel",         "earth"),
            new Stage(0x1E, "Skyworld",                 "st_palutena.rel",      "palutena"),
            new Stage(0x1F, "Mario Bros.",              "st_famicom.rel",       "famicom"),
            new Stage(0x20, "New Pork City",            "st_newpork.rel",       "newpork"),
            new Stage(0x21, "Smashville",               "st_village.rel",       "village"),
            new Stage(0x22, "Shadow Moses Island",      "st_metalgear.rel",     "metalgear"),
            new Stage(0x23, "Green Hill Zone",          "st_greenhill.rel",     "greenhill"),
            new Stage(0x24, "PictoChat",                "st_pictchat.rel",      "pictchat"),
            new Stage(0x25, "Hanenbow",                 "st_plankton.rel",      "plankton"),
            new Stage(0x26, "ConfigTest",               "st_config.rel",        "configtest"),
            new Stage(0x29, "Temple",                   "st_dxshrine.rel",      "dxshrine"),
            new Stage(0x2A, "Yoshi's Island (Melee)",   "st_dxyorster.rel",     "dxyorster"),
            new Stage(0x2B, "Jungle Japes",             "st_dxgarden.rel",      "dxgarden"),
            new Stage(0x2C, "Onett",                    "st_dxonett.rel",       "dxonett"),
            new Stage(0x2D, "Green Greens",             "st_dxgreens.rel",      "dxgreens"),
            new Stage(0x2E, "Pokemon Stadium",          "st_dxpstadium.rel",    "dxpstadium"),
            new Stage(0x2F, "Rainbow Cruise",           "st_dxrcruise.rel",     "dxrcruise"),
            new Stage(0x30, "Corneria",                 "st_dxcorneria.rel",    "dxcorneria"),
            new Stage(0x31, "Big Blue",                 "st_dxbigblue.rel",     "dxbigblue"),
            new Stage(0x32, "Brinstar",                 "st_dxzebes.rel",       "dxzebes"),
            new Stage(0x33, "Bridge of Eldin",          "st_oldin.rel",         "oldin"),
            new Stage(0x34, "Homerun",                  "st_homerun.rel",       "homerun"),
            new Stage(0x35, "Edit",                     "st_stageedit.rel",     "edit"),
            new Stage(0x36, "Heal",                     "st_heal.rel",          "heal"),
            new Stage(0x37, "Online Training",          "st_otrain.rel",        "onlinetraining"),
            new Stage(0x38, "TargetBreak",              "st_tbreak.rel",        "targetlv"),
            new Stage(0x39, "Classic mode credits",     "st_croll.rel",         "chararoll"),
            new Stage(0x40, "Custom 01",                "st_custom01.rel",      "custom01"),
            new Stage(0x41, "Custom 02",                "st_custom02.rel",      "custom02"),
            new Stage(0x42, "Custom 03",                "st_custom03.rel",      "custom03"),
            new Stage(0x43, "Custom 04",                "st_custom04.rel",      "custom04"),
            new Stage(0x44, "Custom 05",                "st_custom05.rel",      "custom05"),
            new Stage(0x45, "Custom 06",                "st_custom06.rel",      "custom06"),
            new Stage(0x46, "Custom 07",                "st_custom07.rel",      "custom07"),
            new Stage(0x47, "Custom 08",                "st_custom08.rel",      "custom08"),
            new Stage(0x48, "Custom 09",                "st_custom09.rel",      "custom09"),
            new Stage(0x49, "Custom0 A",                "st_custom0A.rel",      "custom0A"),
            new Stage(0x4A, "Custom 0B",                "st_custom0B.rel",      "custom0B"),
            new Stage(0x4B, "Custom 0C",                "st_custom0C.rel",      "custom0C"),
            new Stage(0x4C, "Custom 0D",                "st_custom0D.rel",      "custom0D"),
            new Stage(0x4D, "Custom 0E",                "st_custom0E.rel",      "custom0E"),
            new Stage(0x4E, "Custom 0F",                "st_custom0F.rel",      "custom0F"),
            new Stage(0x4F, "Custom 10",                "st_custom10.rel",      "custom10"),
            new Stage(0x50, "Custom 11",                "st_custom11.rel",      "custom11"),
            new Stage(0x51, "Custom 12",                "st_custom12.rel",      "custom12"),
            new Stage(0x52, "Custom 13",                "st_custom13.rel",      "custom13"),
            new Stage(0x53, "Custom 14",                "st_custom14.rel",      "custom14"),
            new Stage(0x54, "Custom 15",                "st_custom15.rel",      "custom15"),
            new Stage(0x55, "Custom 16",                "st_custom16.rel",      "custom16"),
            new Stage(0x56, "Custom 17",                "st_custom17.rel",      "custom17"),
            new Stage(0x57, "Custom 18",                "st_custom18.rel",      "custom18"),
            new Stage(0x58, "Custom 19",                "st_custom19.rel",      "custom19"),
            new Stage(0x59, "Custom 1A",                "st_custom1A.rel",      "custom1A"),
            new Stage(0x5A, "Custom 1B",                "st_custom1B.rel",      "custom1B"),
            new Stage(0x5B, "Custom 1C",                "st_custom1C.rel",      "custom1C"),
            new Stage(0x5C, "Custom 1D",                "st_custom1D.rel",      "custom1D"),
            new Stage(0x5D, "Custom 1E",                "st_custom1E.rel",      "custom1E"),
            new Stage(0x5E, "Custom 1F",                "st_custom1F.rel",      "custom1F"),
            new Stage(0x5F, "Custom 20",                "st_custom20.rel",      "custom20"),
            new Stage(0x60, "Custom 21",                "st_custom21.rel",      "custom21"),
            new Stage(0x61, "Custom 22",                "st_custom22.rel",      "custom22"),
            new Stage(0x62, "Custom 23",                "st_custom23.rel",      "custom23"),
            new Stage(0x63, "Custom 24",                "st_custom24.rel",      "custom24"),
            new Stage(0x64, "Custom 25",                "st_custom25.rel",      "custom25"),
            new Stage(0x65, "Custom 26",                "st_custom26.rel",      "custom26"),
            new Stage(0x66, "Custom 27",                "st_custom27.rel",      "custom27"),
            new Stage(0x67, "Custom 28",                "st_custom28.rel",      "custom28"),
            new Stage(0x68, "Custom 29",                "st_custom29.rel",      "custom29"),
            new Stage(0x69, "Custom 2A",                "st_custom2A.rel",      "custom2A"),
            new Stage(0x6A, "Custom 2B",                "st_custom2B.rel",      "custom2B"),
            new Stage(0x6B, "Custom 2C",                "st_custom2C.rel",      "custom2C"),
            new Stage(0x6C, "Custom 2D",                "st_custom2D.rel",      "custom2D"),
            new Stage(0x6D, "Custom 2E",                "st_custom2E.rel",      "custom2E"),
            new Stage(0x6E, "Custom 2F",                "st_custom2F.rel",      "custom2F"),
            new Stage(0x6F, "Custom 30",                "st_custom30.rel",      "custom30"),
            new Stage(0x70, "Custom 31",                "st_custom31.rel",      "custom31"),
            new Stage(0x71, "Custom 32",                "st_custom32.rel",      "custom32"),
            new Stage(0x72, "Custom 33",                "st_custom33.rel",      "custom33"),
            new Stage(0x73, "Custom 34",                "st_custom34.rel",      "custom34"),
            new Stage(0x74, "Custom 35",                "st_custom35.rel",      "custom35"),
            new Stage(0x75, "Custom 36",                "st_custom36.rel",      "custom36"),
            new Stage(0x76, "Custom 37",                "st_custom37.rel",      "custom37"),
            new Stage(0x77, "Custom 38",                "st_custom38.rel",      "custom38"),
            new Stage(0x78, "Custom 39",                "st_custom39.rel",      "custom39"),
            new Stage(0x79, "Custom 3A",                "st_custom3A.rel",      "custom3A"),
            new Stage(0x7A, "Custom 3B",                "st_custom3B.rel",      "custom3B"),
            new Stage(0x7B, "Custom 3C",                "st_custom3C.rel",      "custom3C"),
            new Stage(0x7C, "Custom 3D",                "st_custom3D.rel",      "custom3D"),
            new Stage(0x7D, "Custom 3E",                "st_custom3E.rel",      "custom3E"),
            new Stage(0x7E, "Custom 3F",                "st_custom3F.rel",      "custom3F"),
            new Stage(0x7F, "Custom 40",                "st_custom40.rel",      "custom40"),
            new Stage(0x80, "Custom 41",                "st_custom41.rel",      "custom41"),
            new Stage(0x81, "Custom 42",                "st_custom42.rel",      "custom42"),
            new Stage(0x82, "Custom 43",                "st_custom43.rel",      "custom43"),
            new Stage(0x83, "Custom 44",                "st_custom44.rel",      "custom44"),
            new Stage(0x84, "Custom 45",                "st_custom45.rel",      "custom45"),
            new Stage(0x85, "Custom 46",                "st_custom46.rel",      "custom46"),
            new Stage(0x86, "Custom 47",                "st_custom47.rel",      "custom47"),
            new Stage(0x87, "Custom 48",                "st_custom48.rel",      "custom48"),
            new Stage(0x88, "Custom 49",                "st_custom49.rel",      "custom49"),
            new Stage(0x89, "Custom 4A",                "st_custom4A.rel",      "custom4A"),
            new Stage(0x8A, "Custom 4B",                "st_custom4B.rel",      "custom4B"),
            new Stage(0x8B, "Custom 4C",                "st_custom4C.rel",      "custom4C"),
            new Stage(0x8C, "Custom 4D",                "st_custom4D.rel",      "custom4D"),
            new Stage(0x8D, "Custom 4E",                "st_custom4E.rel",      "custom4E"),
            new Stage(0x8E, "Custom 4F",                "st_custom4F.rel",      "custom4F"),
            new Stage(0x8F, "Custom 50",                "st_custom50.rel",      "custom50"),
            new Stage(0x90, "Custom 51",                "st_custom51.rel",      "custom51"),
            new Stage(0x91, "Custom 52",                "st_custom52.rel",      "custom52"),
            new Stage(0x92, "Custom 53",                "st_custom53.rel",      "custom53"),
            new Stage(0x93, "Custom 54",                "st_custom54.rel",      "custom54"),
            new Stage(0x94, "Custom 55",                "st_custom55.rel",      "custom55"),
            new Stage(0x95, "Custom 56",                "st_custom56.rel",      "custom56"),
            new Stage(0x96, "Custom 57",                "st_custom57.rel",      "custom57"),
            new Stage(0x97, "Custom 58",                "st_custom58.rel",      "custom58"),
            new Stage(0x98, "Custom 59",                "st_custom59.rel",      "custom59"),
            new Stage(0x99, "Custom 5A",                "st_custom5A.rel",      "custom5A"),
            new Stage(0x9A, "Custom 5B",                "st_custom5B.rel",      "custom5B"),
            new Stage(0x9B, "Custom 5C",                "st_custom5C.rel",      "custom5C"),
            new Stage(0x9C, "Custom 5D",                "st_custom5D.rel",      "custom5D"),
            new Stage(0x9D, "Custom 5E",                "st_custom5E.rel",      "custom5E"),
            new Stage(0x9E, "Custom 5F",                "st_custom5F.rel",      "custom5F"),
            new Stage(0x9F, "Custom 60",                "st_custom60.rel",      "custom60"),
            new Stage(0xA0, "Custom 61",                "st_custom61.rel",      "custom61"),
            new Stage(0xA1, "Custom 62",                "st_custom62.rel",      "custom62"),
            new Stage(0xA2, "Custom 63",                "st_custom63.rel",      "custom63"),
            new Stage(0xA3, "Custom 64",                "st_custom64.rel",      "custom64"),
            new Stage(0xA4, "Custom 65",                "st_custom65.rel",      "custom65"),
            new Stage(0xA5, "Custom 66",                "st_custom66.rel",      "custom66"),
            new Stage(0xA6, "Custom 67",                "st_custom67.rel",      "custom67"),
            new Stage(0xA7, "Custom 68",                "st_custom68.rel",      "custom68"),
            new Stage(0xA8, "Custom 69",                "st_custom69.rel",      "custom69"),
            new Stage(0xA9, "Custom 6A",                "st_custom6A.rel",      "custom6A"),
            new Stage(0xAA, "Custom 6B",                "st_custom6B.rel",      "custom6B"),
            new Stage(0xAB, "Custom 6C",                "st_custom6C.rel",      "custom6C"),
            new Stage(0xAC, "Custom 6D",                "st_custom6D.rel",      "custom6D"),
            new Stage(0xAD, "Custom 6E",                "st_custom6E.rel",      "custom6E"),
            new Stage(0xAE, "Custom 6F",                "st_custom6F.rel",      "custom6F"),
            new Stage(0xAF, "Custom 70",                "st_custom70.rel",      "custom70"),
            new Stage(0xB0, "Custom 71",                "st_custom71.rel",      "custom71"),
            new Stage(0xB1, "Custom 72",                "st_custom72.rel",      "custom72"),
            new Stage(0xB2, "Custom 73",                "st_custom73.rel",      "custom73"),
            new Stage(0xB3, "Custom 74",                "st_custom74.rel",      "custom74"),
            new Stage(0xB4, "Custom 75",                "st_custom75.rel",      "custom75"),
            new Stage(0xB5, "Custom 76",                "st_custom76.rel",      "custom76"),
            new Stage(0xB6, "Custom 77",                "st_custom77.rel",      "custom77"),
            new Stage(0xB7, "Custom 78",                "st_custom78.rel",      "custom78"),
            new Stage(0xB8, "Custom 79",                "st_custom79.rel",      "custom79"),
            new Stage(0xB9, "Custom 7A",                "st_custom7A.rel",      "custom7A"),
            new Stage(0xBA, "Custom 7B",                "st_custom7B.rel",      "custom7B"),
            new Stage(0xBB, "Custom 7C",                "st_custom7C.rel",      "custom7C"),
            new Stage(0xBC, "Custom 7D",                "st_custom7D.rel",      "custom7D"),
            new Stage(0xBD, "Custom 7E",                "st_custom7E.rel",      "custom7E"),
            new Stage(0xBE, "Custom 7F",                "st_custom7F.rel",      "custom7F"),
            new Stage(0xBF, "Custom 80",                "st_custom80.rel",      "custom80"),
            new Stage(0xC0, "Custom 81",                "st_custom81.rel",      "custom81"),
            new Stage(0xC1, "Custom 82",                "st_custom82.rel",      "custom82"),
            new Stage(0xC2, "Custom 83",                "st_custom83.rel",      "custom83"),
            new Stage(0xC3, "Custom 84",                "st_custom84.rel",      "custom84"),
            new Stage(0xC4, "Custom 85",                "st_custom85.rel",      "custom85"),
            new Stage(0xC5, "Custom 86",                "st_custom86.rel",      "custom86"),
            new Stage(0xC6, "Custom 87",                "st_custom87.rel",      "custom87"),
            new Stage(0xC7, "Custom 88",                "st_custom88.rel",      "custom88"),
            new Stage(0xC8, "Custom 89",                "st_custom89.rel",      "custom89"),
            new Stage(0xC9, "Custom 8A",                "st_custom8A.rel",      "custom8A"),
            new Stage(0xCA, "Custom 8B",                "st_custom8B.rel",      "custom8B"),
            new Stage(0xCB, "Custom 8C",                "st_custom8C.rel",      "custom8C"),
            new Stage(0xCC, "Custom 8D",                "st_custom8D.rel",      "custom8D"),
            new Stage(0xCD, "Custom 8E",                "st_custom8E.rel",      "custom8E"),
            new Stage(0xCE, "Custom 8F",                "st_custom8F.rel",      "custom8F"),
            new Stage(0xCF, "Custom 90",                "st_custom90.rel",      "custom90"),
            new Stage(0xD0, "Custom 91",                "st_custom91.rel",      "custom91"),
            new Stage(0xD1, "Custom 92",                "st_custom92.rel",      "custom92"),
            new Stage(0xD2, "Custom 93",                "st_custom93.rel",      "custom93"),
            new Stage(0xD3, "Custom 94",                "st_custom94.rel",      "custom94"),
            new Stage(0xD4, "Custom 95",                "st_custom95.rel",      "custom95"),
            new Stage(0xD5, "Custom 96",                "st_custom96.rel",      "custom96"),
            new Stage(0xD6, "Custom 97",                "st_custom97.rel",      "custom97"),
            new Stage(0xD7, "Custom 98",                "st_custom98.rel",      "custom98"),
            new Stage(0xD8, "Custom 99",                "st_custom99.rel",      "custom99"),
            new Stage(0xD9, "Custom 9A",                "st_custom9A.rel",      "custom9A"),
            new Stage(0xDA, "Custom 9B",                "st_custom9B.rel",      "custom9B"),
            new Stage(0xDB, "Custom 9C",                "st_custom9C.rel",      "custom9C"),
            new Stage(0xDC, "Custom 9D",                "st_custom9D.rel",      "custom9D"),
            new Stage(0xDD, "Custom 9E",                "st_custom9E.rel",      "custom9E"),
            new Stage(0xDE, "Custom 9F",                "st_custom9F.rel",      "custom9F"),
            new Stage(0xDF, "Custom A0",                "st_customA0.rel",      "customA0"),
            new Stage(0xE0, "Custom A1",                "st_customA1.rel",      "customA1"),
            new Stage(0xE1, "Custom A2",                "st_customA2.rel",      "customA2"),
            new Stage(0xE2, "Custom A3",                "st_customA3.rel",      "customA3"),
            new Stage(0xE3, "Custom A4",                "st_customA4.rel",      "customA4"),
            new Stage(0xE4, "Custom A5",                "st_customA5.rel",      "customA5"),
            new Stage(0xE5, "Custom A6",                "st_customA6.rel",      "customA6"),
            new Stage(0xE6, "Custom A7",                "st_customA7.rel",      "customA7"),
            new Stage(0xE7, "Custom A8",                "st_customA8.rel",      "customA8"),
            new Stage(0xE8, "Custom A9",                "st_customA9.rel",      "customA9"),
            new Stage(0xE9, "Custom AA",                "st_customAA.rel",      "customAA"),
            new Stage(0xEA, "Custom AB",                "st_customAB.rel",      "customAB"),
            new Stage(0xEB, "Custom AC",                "st_customAC.rel",      "customAC"),
            new Stage(0xEC, "Custom AD",                "st_customAD.rel",      "customAD"),
            new Stage(0xED, "Custom AE",                "st_customAE.rel",      "customAE"),
            new Stage(0xEE, "Custom AF",                "st_customAF.rel",      "customAF"),
            new Stage(0xEF, "Custom B0",                "st_customB0.rel",      "customB0"),
            new Stage(0xF0, "Custom B1",                "st_customB1.rel",      "customB1"),
            new Stage(0xF1, "Custom B2",                "st_customB2.rel",      "customB2"),
            new Stage(0xF2, "Custom B3",                "st_customB3.rel",      "customB3"),
            new Stage(0xF3, "Custom B4",                "st_customB4.rel",      "customB4"),
            new Stage(0xF4, "Custom B5",                "st_customB5.rel",      "customB5"),
            new Stage(0xF5, "Custom B6",                "st_customB6.rel",      "customB6"),
            new Stage(0xF6, "Custom B7",                "st_customB7.rel",      "customB7"),
            new Stage(0xF7, "Custom B8",                "st_customB8.rel",      "customB8"),
            new Stage(0xF8, "Custom B9",                "st_customB9.rel",      "customB9"),
            new Stage(0xF9, "Custom BA",                "st_customBA.rel",      "customBA"),
            new Stage(0xFA, "Custom BB",                "st_customBB.rel",      "customBB"),
            new Stage(0xFB, "Custom BC",                "st_customBC.rel",      "customBC"),
            new Stage(0xFC, "Custom BD",                "st_customBD.rel",      "customBD"),
            new Stage(0xFD, "Custom BE",                "st_customBE.rel",      "customBE"),
            new Stage(0xFE, "Custom BF",                "st_customBF.rel",      "customBF"),
            new Stage(0xFF, "Custom C0",                "st_customC0.rel",      "customC0"),
        };
    }
}
