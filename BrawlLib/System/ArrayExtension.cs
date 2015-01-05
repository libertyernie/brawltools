using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static unsafe class ArrayExtension
    {
        public static int[] FindAllOccurences(this Array a, object o)
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
        public static int[] Append(this Array a, int[] array)
        {
            List<int> values = new List<int>();
            foreach (int i in a)
                values.Add(i);
            foreach (int i in array)
                values.Add(i);
            return values.ToArray();
        }
        public static int IndexOf(this Array a, object value)
        {
            return Array.IndexOf(a, value);
        }
        public static bool Contains(this Array a, object value)
        {
            return Array.IndexOf(a, value) >= 0;
        }
    }
}
