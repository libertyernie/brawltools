using System;
using BrawlLib.OpenGL;

namespace System.Windows.Forms
{
    public class GLTextureWindow : Form
    {
        private GLTexturePanel panel;

        public GLTextureWindow()
        {
            this.Controls.Add(panel = new GLTexturePanel() { Dock = DockStyle.Fill });
            this.FormBorderStyle = Forms.FormBorderStyle.SizableToolWindow;
            this.Text = "Texture Preview";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
        }

        public DialogResult ShowDialog(IWin32Window owner, GLTexture texture)
        {
            panel.Texture = texture;
            this.Width = texture.Width + 16;
            this.Height = texture.Height + 32;
            try { return this.ShowDialog(owner); }
            finally { panel.Texture = null; }
        }
    }
}
