using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using BrawlLib.SSBB.ResourceNodes;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace BrawlLib.OpenGL
{
    public class GLTexture
    {
        public string _name;
        public int _texId;

        private bool _remake = true;
        private Bitmap[] _textures;

        public unsafe int Initialize()
        {
            if (_remake && _textures != null)
            {
                ClearTexture();

                _texId = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, _texId);

                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, _textures.Length - 1);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);

                for (int i = 0; i < _textures.Length; i++)
                {
                    Bitmap bmp = _textures[i];
                    if (bmp != null)
                    {
                        BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        GL.TexImage2D(TextureTarget.Texture2D, i, PixelInternalFormat.Four, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)data.Scan0);
                        bmp.UnlockBits(data);
                    }
                }

                _remake = false;
                ClearImages();
            }
            return _texId;
        }

        private void ClearImages()
        {
            if (_textures != null)
            {
                foreach (Bitmap bmp in _textures)
                    if (bmp != null)
                        bmp.Dispose();
                _textures = null;
            }
        }
        private unsafe void ClearTexture()
        {
            if (_texId != 0)
            {
                int id = _texId;
                GL.DeleteTexture(id);
                _texId = 0;
            }
        }

        public unsafe void Attach(TEX0Node tex)
        {
            ClearImages();

            _textures = new Bitmap[tex.LevelOfDetail];
            for (int i = 0; i < tex.LevelOfDetail; i++)
                _textures[i] = tex.GetImage(i);

            if (_textures.Length != 0 && _textures[0] != null)
            {
                _width = _textures[0].Width;
                _height = _textures[0].Height;
            }

            _remake = true;
            Initialize();
        }

        public unsafe void Attach(TEX0Node tex, PLT0Node plt)
        {
            ClearImages();

            _textures = new Bitmap[tex.LevelOfDetail];
            for (int i = 0; i < tex.LevelOfDetail; i++)
                _textures[i] = tex.GetImage(i, plt);

            if (_textures.Length != 0)
            {
                _width = _textures[0].Width;
                _height = _textures[0].Height;
            }

            _remake = true;
            Initialize();
        }

        public unsafe void Attach(Bitmap bmp)
        {
            ClearImages();

            _textures = new Bitmap[] { bmp };

            if (_textures.Length != 0)
            {
                _width = _textures[0].Width;
                _height = _textures[0].Height;
            }

            _remake = true;
            Initialize();
        }
        
        internal int _width, _height;
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public GLTexture() { }
        public unsafe GLTexture(int width, int height)
        {
            _width = width;
            _height = height;
        }
        public unsafe GLTexture(Bitmap b)
        {
            _width = b.Width;
            _height = b.Height;
            ClearImages();
            ClearTexture();
            _textures = new Bitmap[] { b };
            _remake = true;
        }

        public void Bind() { Bind(-1, -1, null); }
        public void Bind(int index, int program, TKContext ctx)
        {
            if (program != -1 && ctx != null && ctx._shadersEnabled)
            {
                index = index.Clamp(0, 7);
                GL.ActiveTexture(TextureUnit.Texture0 + index);
                GL.BindTexture(TextureTarget.Texture2D, Initialize());
                GL.Uniform1(GL.GetUniformLocation(program, "Texture" + index), index);
            }
            else
                GL.BindTexture(TextureTarget.Texture2D, Initialize());
        }

        public unsafe void Delete()
        {
            ClearImages();
            ClearTexture();
        }
    }
}
