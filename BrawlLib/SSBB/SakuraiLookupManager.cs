using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBBTypes
{
    /// <summary>
    /// When rebuilding, add the addresses of all offset values to this collection
    /// </summary>
    public class LookupManager : IEnumerable<VoidPtr>
    {
        private List<VoidPtr> _values = new List<VoidPtr>();
        public int Count { get { return _values.Count; } }
        public VoidPtr this[int index]
        {
            get
            {
                if (index >= 0 && index < _values.Count)
                    return _values[index];
                return null;
            }
            set
            {
                if (index >= 0 && index < _values.Count)
                    _values[index] = value;
            }
        }

        public void Add(VoidPtr valueAddr)
        {
            if (!_values.Contains(valueAddr))
                _values.Add(valueAddr);
        }
        public void AddRange(params VoidPtr[] valueAddrs)
        {
            foreach (VoidPtr value in valueAddrs)
                if (!_values.Contains(value))
                    _values.Add(value);
        }

        public IEnumerator<VoidPtr> GetEnumerator() { return _values.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        internal void Sort()
        {
            _values.Sort();
        }

        public int Write(VoidPtr address)
        {
            Sort();



            return 0;
        }
        public void Write(ref VoidPtr address)
        {
            address += Write(address);
        }
    }
}
