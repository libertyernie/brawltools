using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.Modeling
{
    public class TextureManager
    {
        internal List<TextureRef> _textures = new List<TextureRef>();

        public int Count { get { return _textures.Count; } }
        public int CountTextures
        {
            get
            {
                int count = 0;
                foreach (TextureRef t in _textures)
                    if (t.IsTexture)
                        count++;
                return count;
            }
        }
        public int CountDecals
        {
            get
            {
                int count = 0;
                foreach (TextureRef t in _textures)
                    if (t.IsDecal)
                        count++;
                return count;
            }
        }

        public TextureRef FindOrCreate(string name)
        {
            foreach (TextureRef t in _textures)
            {
                if (t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return t;
            }

            TextureRef tr = new TextureRef(name);
            _textures.Add(tr);
            return tr;
        }

        public TextureRef[] GetTextures()
        {
            List<TextureRef> l = new List<TextureRef>(_textures.Count);
            foreach (TextureRef t in _textures)
                if (t.IsTexture)
                    l.Add(t);
            return l.ToArray();
        }
        public TextureRef[] GetDecals()
        {
            List<TextureRef> l = new List<TextureRef>(_textures.Count);
            foreach (TextureRef t in _textures)
                if (t.IsDecal)
                    l.Add(t);
            return l.ToArray();
        }

        public void Clean()
        {
            int i = 0;
            while (i < _textures.Count)
            {
                if ((_textures[i]._texRefs.Count == 0) && (_textures[i]._decRefs.Count == 0))
                    _textures.RemoveAt(i);
                else
                    i++;
            }
        }

        public void Sort()
        {
            _textures.Sort(TextureRef.Compare);
        }
    }
}
