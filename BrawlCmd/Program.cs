using System;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;

namespace BrawlCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            //Print info/copyright
            Console.WriteLine("SmashCmd v0.1 - Copyright 2009 SmashTools Project\n");

            //print help
            if ((args.Length == 0) || (args[0] == "/?"))
            {
                Console.WriteLine("Help goes here");
                return;
            }

            //Open file
            using (ResourceNode node = NodeFactory.FromFile(null, args[0]))
            {
                //list file contents
                if ((args.Length == 1) || (args[1].Equals("/list", StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("Listing contents of file \"{0}\":\n", Path.GetFileName(args[0]));
                    Console.WriteLine("{0}| {1}| {2}", "Name".PadRight(48), "Type".PadRight(10), "Children");
                    PrintPath(node);
                    return;
                }
            }
        }

        public static void PrintPath(ResourceNode n)
        {
            if (n.HasChildren)
            {
                Console.WriteLine("{0}| {1}| {2}", (string.Join("...", new string[n.Level + 1]) + n.Name).PadRight(48), n.ResourceType.ToString().PadRight(10), n.Children.Count);
                Console.WriteLine(string.Join("...", new string[n.Level + 2]) + "| " + string.Join("-", new string[64 - (n.Level * 3)]));
                foreach (ResourceNode c in n.Children)
                    PrintPath(c);
            }
            else
                Console.WriteLine("{0}| {1}|", (string.Join("...", new string[n.Level + 1]) + n.Name).PadRight(48),  n.ResourceType.ToString().PadRight(10));
        }
    }
}
