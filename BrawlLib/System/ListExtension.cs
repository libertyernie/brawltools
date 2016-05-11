using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System
{
    public static unsafe class ListExtension
    {
        public static int[] FindAllOccurences(this IList a, object o)
        {
            List<int> l = new List<int>();
            int i = 0;
            foreach (object x in a)
            {
                if (x == o)
                    l.Add(i);
                i++;
            }
            return l.ToArray();
        }
        public static int IndexOf(this byte[] searchList, byte[] pattern)
        {
            Encoding encoding = Encoding.GetEncoding(1252);
            string s1 = encoding.GetString(searchList, 0, searchList.Length);
            string s2 = encoding.GetString(pattern, 0, pattern.Length);
            int result = s1.IndexOf(s2, StringComparison.Ordinal);
            return result;
        }
    }
}
