using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using OpenTK.Graphics.OpenGL;
using BrawlLib.OpenGL;
using System.Drawing.Imaging;

namespace System.Windows.Forms
{
    public partial class TexCoordControl : UserControl
    {
        public TexCoordControl()
        {
            InitializeComponent();

            texCoordRenderer1.UVIndexChanged += texCoordRenderer1_UVIndexChanged;
            texCoordRenderer1.ObjIndexChanged += texCoordRenderer1_ObjIndexChanged;
        }

        bool _updating;
        void texCoordRenderer1_ObjIndexChanged(object sender, EventArgs e)
        {
            if (comboObj.Items.Count == 1)
                return;

            if (comboObj.SelectedIndex != texCoordRenderer1._objIndex + 1 && comboObj.Items.Count != 0)
            {
                _updating = true;
                comboObj.SelectedIndex = texCoordRenderer1._objIndex + 1;
                _updating = false;
            }
        }

        void texCoordRenderer1_UVIndexChanged(object sender, EventArgs e)
        {
            if (comboUVs.Items.Count == 1)
                return;

            if (comboUVs.SelectedIndex != texCoordRenderer1._uvIndex + 1 && comboUVs.Items.Count != 0)
            {
                _updating = true;
                comboUVs.SelectedIndex = texCoordRenderer1._uvIndex + 1;
                _updating = false;
            }
        }

        public MDL0MaterialRefNode TargetNode
        {
            get { return texCoordRenderer1.TargetNode; }
            set
            {
                if ((texCoordRenderer1.TargetNode = value) != null)
                {
                    comboObj.DataSource = texCoordRenderer1.ObjectNames;
                    comboUVs.DataSource = texCoordRenderer1.UVSetNames;

                    texCoordRenderer1_ObjIndexChanged(null, null);
                    texCoordRenderer1_UVIndexChanged(null, null);
                }
            }
        }

        private unsafe void btnExport_Click(object sender, EventArgs e)
        {
            int height = texCoordRenderer1._yScale * 1024;
            int width = texCoordRenderer1._xScale * 1024;

            uint bufferHandle;
            uint colorTex;

            texCoordRenderer1.Capture();

            GL.GenTextures(1, out colorTex);
            GL.BindTexture(TextureTarget.Texture2D, colorTex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, height, width, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Ext.GenFramebuffers(1, out bufferHandle);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, bufferHandle);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, colorTex, 0);

            GL.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0Ext);

            GL.PushAttrib(AttribMask.ViewportBit);
            {
                GL.Viewport(0, 0, height, width);
                GLCamera cam = new GLCamera() { _ortho = true, _nearZ = -1.0f, _farZ = 1.0f };
                texCoordRenderer1.ResizeData(width, height);
                cam.SetDimensions(width, height);
                cam.Reset();
                texCoordRenderer1.Render(cam, true, false, false);

                Bitmap bmp = new Bitmap(width, height);
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                ModelEditorBase.SaveBitmap(bmp, Application.StartupPath + "\\UVs\\test.png", this);
            }
            GL.PopAttrib();

            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.DrawBuffer(DrawBufferMode.Back);

            if (bufferHandle != 0)
                GL.Ext.DeleteFramebuffers(1, ref bufferHandle);

            texCoordRenderer1.ResizeData(texCoordRenderer1.Width, texCoordRenderer1.Height);
        }

        private void comboObj_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updating)
                texCoordRenderer1.SetObjectIndex(comboObj.SelectedIndex - 1);
        }

        private void comboUVs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updating)
                texCoordRenderer1.SetUVIndex(comboUVs.SelectedIndex - 1);
        }
    }
}
