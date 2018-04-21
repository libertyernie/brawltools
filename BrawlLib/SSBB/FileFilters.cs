using BrawlLib.SSBB;

namespace BrawlLib
{
    public static class FileFilters
    {
        public static string BRES =
            SupportedFilesHandler.GetCompleteFilter("brres", "branm", "brmdl", "brtex", "brplt", "brcha", "brtsa", "brsha", "brvia", "brtpa", "brcla", "brsca");
        public static string MDL0Import = 
            SupportedFilesHandler.GetCompleteFilter("mdl0", "pmd", "dae");
        public static string MDL0Export = 
            SupportedFilesHandler.GetCompleteFilter("mdl0", "dae");
        public static string TEX0 =
            SupportedFilesHandler.GetCompleteFilter("png", "tga", "tif", "bmp", "jpg", "gif", "tex0");
        public static string PLT0 =
            SupportedFilesHandler.GetCompleteFilter("plt0");
        public static string CHR0Import = 
            SupportedFilesHandler.GetCompleteFilter("chr0", "anim", "txt", "json");
        public static string CHR0Export =
            SupportedFilesHandler.GetCompleteFilter("chr0", "anim");
        public static string CLR0 =
            SupportedFilesHandler.GetCompleteFilter("clr0");
        public static string PAT0 = 
            SupportedFilesHandler.GetCompleteFilter("pat0");
        public static string VIS0 =
            SupportedFilesHandler.GetCompleteFilter("vis0");
        public static string SRT0 =
            SupportedFilesHandler.GetCompleteFilter("srt0");
        public static string SCN0 =
            SupportedFilesHandler.GetCompleteFilter("scn0");
        public static string SHP0 =
            SupportedFilesHandler.GetCompleteFilter("shp0");
        public static string MSBin = 
            SupportedFilesHandler.GetCompleteFilter("msbin", "txt");
        public static string RSTM = 
            SupportedFilesHandler.GetCompleteFilter("brstm", "bcstm", "bfstm", "wav");
        public static string RWSD =
            SupportedFilesHandler.GetCompleteFilter("brwsd");
        public static string RBNK =
            SupportedFilesHandler.GetCompleteFilter("brbnk");
        public static string RSEQ =
            SupportedFilesHandler.GetCompleteFilter("brseq");
        public static string REFF =
            SupportedFilesHandler.GetCompleteFilter("breff");
        public static string REFT =
            SupportedFilesHandler.GetCompleteFilter("breft");
        public static string Images =
            SupportedFilesHandler.GetCompleteFilter("png", "tga", "tif", "bmp", "jpg", "gif");
        public static string EFLS =
            SupportedFilesHandler.GetCompleteFilter("efls");
        public static string CollisionDef =
            SupportedFilesHandler.GetCompleteFilter("coll");
        public static string REL =
            SupportedFilesHandler.GetCompleteFilter("rel");
        public static string DOL =
            SupportedFilesHandler.GetCompleteFilter("dol");
        public static string RSAR =
            SupportedFilesHandler.GetCompleteFilter("brsar");
        public static string TPL =
            SupportedFilesHandler.GetCompleteFilter("tpl");
        public static string Object =
            SupportedFilesHandler.GetCompleteFilter("obj", "*");
        public static string WAV =
            SupportedFilesHandler.GetCompleteFilter("wav");
        public static string BLOC =
            "BLOC Adventure Archive (*.BLOC)|*.bloc";
        public static string FMDL =
            SupportedFilesHandler.GetCompleteFilter("fmdl");
            
        // Brawl Stage Files
        public static string STPM =
            SupportedFilesHandler.GetCompleteFilter("stpm");
        public static string STDT =
            SupportedFilesHandler.GetCompleteFilter("stdt");
        public static string SCLA =
            SupportedFilesHandler.GetCompleteFilter("scla");
        public static string TBGC =
            SupportedFilesHandler.GetCompleteFilter("tb");
        public static string TBGD =
            SupportedFilesHandler.GetCompleteFilter("tb");
        public static string TBGM =
            SupportedFilesHandler.GetCompleteFilter("tb");
        public static string TBLV =
            SupportedFilesHandler.GetCompleteFilter("tb");
        public static string TBRM =
            SupportedFilesHandler.GetCompleteFilter("tb");
        public static string TBST =
            SupportedFilesHandler.GetCompleteFilter("tb");
            
        public static string Raw =
            SupportedFilesHandler.GetCompleteFilter("*");
            //Some files already have an extension in their name,
            //or sometimes the user will want to add the extension themselves.
            //Not only that, but '.dat' might be assigned to something else on their computer.
            //It's possible to assign a program (like a hex editor) to open files without extensions.

        public static string Havok =
            SupportedFilesHandler.GetCompleteFilter("hkx", "xml");
    }
}
