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
            if (comboObj.SelectedIndex != texCoordRenderer1._objIndex && comboObj.Items.Count != 0)
            {
                _updating = true;
                comboObj.SelectedIndex = texCoordRenderer1._objIndex + 1;
                _updating = false;
            }
        }

        void texCoordRenderer1_UVIndexChanged(object sender, EventArgs e)
        {
            if (comboUVs.SelectedIndex != texCoordRenderer1._uvIndex && comboUVs.Items.Count != 0)
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

        private void btnExport_Click(object sender, EventArgs e)
        {
            
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
