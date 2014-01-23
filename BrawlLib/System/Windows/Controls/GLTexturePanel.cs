using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.OpenGL;
using BrawlLib.Imaging;
using BrawlLib.SSBB.ResourceNodes;
using OpenTK.Graphics.OpenGL;

namespace System.Windows.Forms
{
    public class GLTexturePanel : GLPanel
    {
        private GLTexture _currentTexture;
        public GLTexture Texture
        {
            get { return _currentTexture; }
            set 
            {
                if (_currentTexture == value)
                    return;

                //if (((
                _currentTexture = value;
                //) != null))
                //    _currentTexture._context.Share(_context);
            }
        }

        unsafe internal override void OnInit(TKContext ctx)
        {
            //Share lists with original context
            //if (_currentTexture != null)
            //    _currentTexture._context.Share(_context);

            //Set caps
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            OnResized();
        }

        protected internal unsafe override void OnRender(TKContext ctx, PaintEventArgs e)
        {
            GLTexture _bgTex = ctx.FindOrCreate<GLTexture>("TexBG", CreateBG);
            _bgTex.Bind();

            //Draw BG
            float s = (float)Width / (float)_bgTex.Width, t = (float)Height / (float)_bgTex.Height;

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(0.0f, 0.0f);
            GL.TexCoord2(s, 0.0f);
            GL.Vertex2(1.0, 0.0f);
            GL.TexCoord2(s, t);
            GL.Vertex2(1.0, 1.0);
            GL.TexCoord2(0, t);
            GL.Vertex2(0.0f, 1.0);

            GL.End();

            //Draw texture
            if ((_currentTexture != null) && (_currentTexture._texId != 0))
            {
                float tAspect = (float)_currentTexture.Width / _currentTexture.Height;
                float wAspect = (float)Width / Height;
                float* points = stackalloc float[8];

                if (tAspect > wAspect) //Texture is wider, use horizontal fit
                {
                    points[0] = points[6] = 0.0f;
                    points[2] = points[4] = 1.0f;

                    points[1] = points[3] = ((Height - ((float)Width / _currentTexture.Width * _currentTexture.Height))) / Height / 2.0f;
                    points[5] = points[7] = 1.0f - points[1];
                }
                else
                {
                    points[1] = points[3] = 0.0f;
                    points[5] = points[7] = 1.0f;

                    points[0] = points[6] = (Width - ((float)Height / _currentTexture.Height * _currentTexture.Width)) / Width / 2.0f;
                    points[2] = points[4] = 1.0f - points[0];
                }

                GL.BindTexture(TextureTarget.Texture2D, _currentTexture._texId);

                GL.Begin(BeginMode.Quads);

                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex2(&points[0]);
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex2(&points[2]);
                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex2(&points[4]);
                GL.TexCoord2(0.0f, 1.0f);
                GL.Vertex2(&points[6]);

                GL.End();
            }
        }

        public override void OnResized()
        {
            //Set up orthographic projection

            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0f, 1.0f, 1.0f, 0.0f, -0.1f, 1.0f);
        }

        public static RGBAPixel _left = new RGBAPixel(192, 192, 192, 255), _right = new RGBAPixel(240, 240, 240, 255);
        public static unsafe GLTexture CreateBG(TKContext ctx)
        {
            GLTexture tex = new GLTexture(16, 16);
            tex._texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex._texId);

            int* pixelData = stackalloc int[16 * 16];
            RGBAPixel* p = (RGBAPixel*)pixelData;

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    *p++ = ((x & 8) == (y & 8)) ? _left : _right;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, 16, 16, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (VoidPtr)pixelData);

            return tex;
        }
    }
}
