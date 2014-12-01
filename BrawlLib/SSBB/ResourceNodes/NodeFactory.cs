using System;
using System.Reflection;
using System.Collections.Generic;
using BrawlLib.IO;
using BrawlLib.Wii.Compression;
using System.IO;
using System.Windows.Forms;
using BrawlLib.SSBBTypes;

namespace BrawlLib.SSBB.ResourceNodes
{
    internal delegate ResourceNode ResourceParser(DataSource source);

    //Factory is for initializing root node, and unknown child nodes.
    public static class NodeFactory
    {
        private const bool UseRawDataNode = true;
        
        private static List<ResourceParser> _parsers = new List<ResourceParser>();
        static NodeFactory()
        {
            Delegate del;
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                if (t.IsSubclassOf(typeof(ResourceNode)))
                    if ((del = Delegate.CreateDelegate(typeof(ResourceParser), t, "TryParse", false, false)) != null)
                        _parsers.Add(del as ResourceParser);
        }

        //Parser commands must initialize the node before returning.
        public unsafe static ResourceNode FromFile(ResourceNode parent, string path)
        {
            ResourceNode node = null;
            FileMap map = FileMap.FromFile(path, FileMapProtect.Read);
            try
            {
                DataSource source = new DataSource(map);
                if (String.Equals(Path.GetExtension(path), ".mrg", StringComparison.OrdinalIgnoreCase) || String.Equals(Path.GetExtension(path), ".mrgc", StringComparison.OrdinalIgnoreCase))
                {
                    node = new MRGNode();
                    if (Compressor.IsDataCompressed(source.Address, source.Length))
                    {
                        CompressionHeader* cmpr = (CompressionHeader*)source.Address;
                        source.Compression = cmpr->Algorithm;
                        if (Compressor.Supports(cmpr->Algorithm))
                        {
                            try
                            {
                                //Expand the whole resource and initialize
                                FileMap uncompMap = FileMap.FromTempFile((int)cmpr->ExpandedSize);
                                Compressor.Expand(cmpr, uncompMap.Address, uncompMap.Length);
                                node.Initialize(parent, source, new DataSource(uncompMap));
                            }
                            catch (InvalidCompressionException e) { MessageBox.Show(e.ToString()); }
                        }
                        else
                            node.Initialize(parent, source);
                    }
                    else
                        node.Initialize(parent, source);
                }
                else if (String.Equals(Path.GetExtension(path), ".rel", StringComparison.OrdinalIgnoreCase))
                {
                    node = new RELNode();
                    node.Initialize(parent, map);
                }
                else if (String.Equals(Path.GetExtension(path), ".dol", StringComparison.OrdinalIgnoreCase))
                {
                    node = new DOLNode();
                    node.Initialize(parent, map);
                }
                else if ((node = FromSource(parent, source, UseRawDataNode)) is RawDataNode)
                    node._name = Path.GetFileNameWithoutExtension(path);
            }
            finally { if (node == null) map.Dispose(); }
            return node;
        }
        public static ResourceNode FromAddress(ResourceNode parent, VoidPtr address, int length)
        {
            return FromSource(parent, new DataSource(address, length));
        }

        public static unsafe ResourceNode FromSource(ResourceNode parent, DataSource source) { return FromSource(parent, source, false); }
        public static unsafe ResourceNode FromSource(ResourceNode parent, DataSource source, bool returnRaw)
        {
            ResourceNode n = null;
            if ((n = GetRaw(source)) != null)
                n.Initialize(parent, source);
            else
                n = Compressor.TryExpand(source, parent, returnRaw);

            return n;
        }
        public static ResourceNode GetRaw(VoidPtr address, int length)
        {
            return GetRaw(new DataSource(address, length));
        }

        public static ResourceNode GetRaw(DataSource source)
        {
            ResourceNode n = null;
            foreach (ResourceParser d in _parsers)
                if ((n = d(source)) != null)
                    break;
            return n;
        }
    }
}
