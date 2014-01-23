using System;

namespace BrawlLib
{
    public static class FileFilters
    {
        private static string i =
            "Portable Network Graphics (*.png)|*.png|" +
            "Truevision TARGA (*.tga)|*.tga|" +
            "Tagged Image File Format (*.tif, *.tiff)|*.tif;*.tiff|" +
            "Bitmap (*.bmp)|*.bmp|" +
            "Jpeg (*.jpg,*.jpeg)|*.jpg;*.jpeg|" +
            "Gif (*.gif)|*.gif";

        public static string TEX0 =
            "All Image Formats (*.png,*.tga,*.tif,*.tiff,*.bmp,*.jpg,*.jpeg,*.gif,*.tex0)|*.png;*.tga;*.tif;*.tiff;*.bmp;*.jpg;*.jpeg,*.gif;*.tex0|" +
            i + "|" +
            "TEX0 Texture (*.tex0)|*.tex0";

        public static string MDL0Import =
            "All Model Formats (*.mdl0, *.pmd, *.dae)|*.mdl0;*.pmd;*.dae|" +
            "Collada Scene (*.dae)|*.dae|" +
            "MMD Model (*.pmd)|*.pmd|" +
            "MDL0 Model (*.mdl0)|*.mdl0";
        
        public static string MDL0Export =
            "All Model Formats (*.mdl0, *.dae)|*.mdl0;*.dae|" +
            "Collada Scene (*.dae)|*.dae|" +
            "MDL0 Model (*.mdl0)|*.mdl0";

        public static string CHR0 =
            "All Formats (*.chr0, *.anim)|*.chr0;*.anim|" +
            "CHR0 Animation (*.chr0)|*.chr0|" +
            "Maya Animation (*.anim)|*.anim";

        public static string PLT0 =
            "PLT0 Palette (*.plt0)|*.plt0";

        public static string PAT0 =
            "PAT0 Texture Pattern (*.pat0)|*.pat0";

        public static string MSBin =
            "MSBin Message List (*.msbin)|*.msbin";

        public static string BRES =
            "BRResource Pack (*.brres, *.brtex, *.brmdl, *.branm)|*.brres;*.brtex*.brmdl*.branm";

        public static string RSTM =
            "All Audio Formats (*.brstm, *.wav)|*.brstm;*.wav|" +
            "BRSTM Audio (*.brstm)|*.brstm|" + 
            "Uncompressed PCM (*.wav)|*.wav";

        public static string RWSD =
            "Raw Sound Pack (*.brwsd)|*.brwsd";

        public static string RBNK =
            "Raw Sound Bank (*.brbnk)|*.brbnk";

        public static string RSEQ =
            "Raw Sound Requence (*.brseq)|*.brseq";

        public static string CLR0 =
            "Color Sequence (*.clr0)|*.clr0";

        public static string VIS0 =
            "Visibility Sequence (*.vis0)|*.vis0";

        public static string SRT0 =
            "Texture Animation (*.srt0)|*.srt0";

        public static string SCN0 =
            "Scene Settings (*.scn0)|*.scn0";

        public static string SHP0 =
            "Vertex Set Morph (*.shp0)|*.shp0";

        public static string REFF =
            "REFF (*.breff)|*.breff";

        public static string REFT =
            "REFT (*.breft)|*.breft";

        public static string Images =
            "All Image Formats (*.png,*.tga,*.tif,*.tiff,*.bmp,*.jpg,*.jpeg,*.gif)|*.png;*.tga;*.tif;*.tiff;*.bmp;*.jpg;*.jpeg,*.gif|" +
            i;

        public static string EFLS =
            "Effect List (*.efls)|*.efls";

        public static string CollisionDef =
            "Collision Definition (*.coll)|*.coll";

        public static string REL =
            "Relocatable Module (*.rel)|*.rel";

        public static string DOL =
            "Static Module (*.dol)|*.dol";

        public static string Object =
            "Object (*.obj)|*.obj|" +
            "Raw Data File (*.*)|*.*";

        public static string Raw =
            "Raw Data File (*.*)|*.*";

        public static string WAV =
            "Uncompressed PCM (*.wav)|*.wav";

        public static string RSAR = 
            "Sound File Archive (*.brsar)|*.brsar";

        public static string TPL =
            "Texture Archive (*.tpl)|*.tpl";
    }
}
