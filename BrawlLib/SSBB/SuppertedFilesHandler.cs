using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB
{
    public static class SuppertedFilesHandler
    {
        public static SupportedFileInfo[] Files = 
        {
            new SupportedFileInfo("PAC File Archive", "pac"),
            new SupportedFileInfo("PCS Compressed File Archive", "pcs"),
            new SupportedFileInfo("NW4R Resource Pack", "brres", "brtex", "brmdl", "branm", "brcha"),
            new SupportedFileInfo("NW4R Palette", "plt0"),
            new SupportedFileInfo("NW4R Texture", "tex0"),
            new SupportedFileInfo("TPL Texture Archive", "tpl"),
            new SupportedFileInfo("NW4R Model", "mdl0"),
            new SupportedFileInfo("NW4R Model Animation", "chr0"),
            new SupportedFileInfo("NW4R Texture Animation", "srt0"),
            new SupportedFileInfo("NW4R Vertex Morph", "shp0"),
            new SupportedFileInfo("NW4R Texture Pattern", "pat0"),
            new SupportedFileInfo("NW4R Visibility Sequence", "vis0"),
            new SupportedFileInfo("NW4R Color Sequence", "clr0"),
            new SupportedFileInfo("NW4R Scene Settings", "scn0"),
            new SupportedFileInfo("Message Pack", "msbin"),
            new SupportedFileInfo("Audio Stream", "brstm"),
            new SupportedFileInfo("Sound Archive", "brsar"),
            new SupportedFileInfo("Sound Stream", "brwsd"),
            new SupportedFileInfo("Sound Bank", "brbnk"),
            new SupportedFileInfo("Sound Sequence", "brseq"),
            new SupportedFileInfo("Effect List", "efls"),
            new SupportedFileInfo("NW4R Effect Parameters", "breff"),
            new SupportedFileInfo("NW4R Effect Textures", "breft"),
            new SupportedFileInfo("ARC File Archive", "arc"),
            //new SupportedFileInfo("RARC File Archive", "rarc"),
            new SupportedFileInfo("MRG Resource Group", "mrg"),
            new SupportedFileInfo("MRG Compressed Resource Group", "mrgc"),
            new SupportedFileInfo("SZS Compressed Archive", "szs"),
            //new SupportedFileInfo("SZP Compressed Archive", "szp"),
            new SupportedFileInfo("Static Module", "dol"),
            new SupportedFileInfo("Relocatable Module", "rel"),
            new SupportedFileInfo("THP Audio/Video", "thp"),
            //new SupportedFileInfo("J3D Model", "bmd", "bdl"),
            //new SupportedFileInfo("Luigi's Mansion GC model", "bin"),
        };

        private static string _allSupportedFilter = null;
        private static string _filterList = null;

        public static string CompleteFilter { get { return GetAllSupportedFilter() + "|" + GetListFilter(); } }

        public static string GetAllSupportedFilter()
        {
            if (_allSupportedFilter != null)
                return _allSupportedFilter;

            //The "all supported formats" string needs (*.*) in it
            //or else the window will display EVERY SINGLE FILTER
            string filter = "All Supported Formats (*.*)|";
            string[] fileTypeExtensions = Files.Select(x => x.ExtensionsFilter).ToArray();
            for (int i = 0; i < fileTypeExtensions.Length; i++)
            {
                string[] extensions = fileTypeExtensions[i].Split(';');
                string n = "";
                for (int x = 0; x < extensions.Length; x++)
                {
                    string ext = extensions[x];
                    string rawExtName = ext.Substring(ext.IndexOf('.') + 1);
                    if (!rawExtName.Contains("*"))
                        n += (x != 0 ? ";" : "") + ext;
                }
                filter += (i != 0 ? ";" : "") + n;
            }
            return _allSupportedFilter = filter;
        }

        public static string GetListFilter()
        {
            if (_filterList != null)
                return _filterList;

            string filter = "";
            string[] fileTypeExtensions = Files.Select(x => x.Filter).ToArray();
            for (int i = 0; i < fileTypeExtensions.Length; i++)
                filter += fileTypeExtensions[i] + (i == fileTypeExtensions.Length - 1 ? "" : "|");
            return _filterList = filter;
        }

        public class SupportedFileInfo
        {
            string _name;
            string[] _extensions;

            public SupportedFileInfo(string name, params string[] extensions)
            {
                _name = name;
                if (extensions == null || extensions.Length == 0)
                    throw new Exception("No extensions for file type \"" + _name + "\".");
                _extensions = extensions;
            }

            public string Filter { get { string s = ExtensionsFilter; return _name + " (" + s.Replace(";", ", ") + ")|" + s; } }
            public string ExtensionsFilter
            {
                get
                {
                    string filter = "";
                    bool first = true;
                    foreach (string ext in _extensions)
                    {
                        if (!first)
                            filter += ";";

                        //In case of a specific file name
                        if (!ext.Contains('.'))
                            filter += "*.";

                        filter += ext;

                        first = false;
                    }
                    return filter;
                }
            }
        }
    }
}
