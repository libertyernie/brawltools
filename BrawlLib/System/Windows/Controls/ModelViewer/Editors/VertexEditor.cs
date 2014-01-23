using System;
using BrawlLib.Wii.Animations;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Modeling;
using System.ComponentModel;
using System.Drawing;
using System.Collections.Generic;
using BrawlLib.SSBBTypes;
using BrawlLib.Wii.Models;
using System.Linq;
using BrawlLib.Imaging;

namespace System.Windows.Forms
{
    public class VertexEditor : UserControl
    {
        #region Designer

        private System.ComponentModel.IContainer components;
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.numPosZ = new System.Windows.Forms.NumericInputBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numPosY = new System.Windows.Forms.NumericInputBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numPosX = new System.Windows.Forms.NumericInputBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numNormZ = new System.Windows.Forms.NumericInputBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numNormX = new System.Windows.Forms.NumericInputBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numNormY = new System.Windows.Forms.NumericInputBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.colorBox = new System.Windows.Forms.Label();
            this.colorIndex = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(6, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "Z: ";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numPosZ
            // 
            this.numPosZ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numPosZ.Location = new System.Drawing.Point(27, 54);
            this.numPosZ.Name = "numPosZ";
            this.numPosZ.Size = new System.Drawing.Size(78, 20);
            this.numPosZ.TabIndex = 6;
            this.numPosZ.Text = "0";
            this.numPosZ.ValueChanged += new System.EventHandler(this.numPosZ_TextChanged);
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(6, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(22, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Y: ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numPosY
            // 
            this.numPosY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numPosY.Location = new System.Drawing.Point(27, 35);
            this.numPosY.Name = "numPosY";
            this.numPosY.Size = new System.Drawing.Size(78, 20);
            this.numPosY.TabIndex = 4;
            this.numPosY.Text = "0";
            this.numPosY.ValueChanged += new System.EventHandler(this.numPosY_TextChanged);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "X: ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numPosX
            // 
            this.numPosX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numPosX.Location = new System.Drawing.Point(27, 16);
            this.numPosX.Name = "numPosX";
            this.numPosX.Size = new System.Drawing.Size(78, 20);
            this.numPosX.TabIndex = 0;
            this.numPosX.Text = "0";
            this.numPosX.ValueChanged += new System.EventHandler(this.numPosX_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.numPosZ);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numPosX);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numPosY);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(111, 82);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Position";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.numNormZ);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.numNormX);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.numNormY);
            this.groupBox2.Location = new System.Drawing.Point(120, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(111, 82);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Normal";
            this.groupBox2.Visible = false;
            // 
            // numNormZ
            // 
            this.numNormZ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numNormZ.Location = new System.Drawing.Point(27, 54);
            this.numNormZ.Name = "numNormZ";
            this.numNormZ.Size = new System.Drawing.Size(78, 20);
            this.numNormZ.TabIndex = 6;
            this.numNormZ.Text = "0";
            this.numNormZ.ValueChanged += new System.EventHandler(this.numNormZ_ValueChanged);
            // 
            // label4
            // 
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.Location = new System.Drawing.Point(6, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Z: ";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numNormX
            // 
            this.numNormX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numNormX.Location = new System.Drawing.Point(27, 16);
            this.numNormX.Name = "numNormX";
            this.numNormX.Size = new System.Drawing.Size(78, 20);
            this.numNormX.TabIndex = 0;
            this.numNormX.Text = "0";
            this.numNormX.ValueChanged += new System.EventHandler(this.numNormX_ValueChanged);
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 20);
            this.label5.TabIndex = 3;
            this.label5.Text = "X: ";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label6.Location = new System.Drawing.Point(6, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(22, 20);
            this.label6.TabIndex = 5;
            this.label6.Text = "Y: ";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numNormY
            // 
            this.numNormY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numNormY.Location = new System.Drawing.Point(27, 35);
            this.numNormY.Name = "numNormY";
            this.numNormY.Size = new System.Drawing.Size(78, 20);
            this.numNormY.TabIndex = 4;
            this.numNormY.Text = "0";
            this.numNormY.ValueChanged += new System.EventHandler(this.numNormY_ValueChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox4.Controls.Add(this.colorBox);
            this.groupBox4.Controls.Add(this.colorIndex);
            this.groupBox4.Location = new System.Drawing.Point(237, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(111, 82);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Color";
            this.groupBox4.Visible = false;
            // 
            // colorBox
            // 
            this.colorBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.colorBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colorBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.colorBox.Location = new System.Drawing.Point(6, 36);
            this.colorBox.Name = "colorBox";
            this.colorBox.Size = new System.Drawing.Size(99, 38);
            this.colorBox.TabIndex = 12;
            this.colorBox.DoubleClick += new System.EventHandler(this.colorBox_Click);
            // 
            // colorIndex
            // 
            this.colorIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorIndex.FormattingEnabled = true;
            this.colorIndex.Items.AddRange(new object[] {
            "Color 0",
            "Color 1"});
            this.colorIndex.Location = new System.Drawing.Point(6, 14);
            this.colorIndex.Name = "colorIndex";
            this.colorIndex.Size = new System.Drawing.Size(99, 21);
            this.colorIndex.TabIndex = 7;
            this.colorIndex.SelectedIndexChanged += new System.EventHandler(this.colorIndex_SelectedIndexChanged);
            // 
            // VertexEditor
            // 
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "VertexEditor";
            this.Size = new System.Drawing.Size(118, 85);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public VertexEditor()
        { 
            InitializeComponent(); 
            //uvIndex.SelectedIndex = 0; 
            colorIndex.SelectedIndex = 0;
            _dlgColor = new GoodColorDialog();
        }

        public IMainWindow _mainWindow;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentFrame
        {
            get { return _mainWindow.CurrentFrame; }
            set { _mainWindow.CurrentFrame = value; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MDL0Node TargetModel
        {
            get { return _mainWindow.TargetModel; }
            set { _mainWindow.TargetModel = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MDL0BoneNode TargetBone { get { return _mainWindow.SelectedBone; } set { _mainWindow.SelectedBone = value; } }

        private Label label3;
        public NumericInputBox numPosZ;
        private Label label2;
        public NumericInputBox numPosY;
        private Label label1;
        public NumericInputBox numPosX;

        private int _uvIndex = 0, _colorIndex = 0;

        private GroupBox groupBox1;
        private GroupBox groupBox2;
        public NumericInputBox numNormZ;
        private Label label4;
        public NumericInputBox numNormX;
        private Label label5;
        private Label label6;
        public NumericInputBox numNormY;
        private GroupBox groupBox4;
        private ComboBox colorIndex;
        private Label colorBox;
        public MDL0BoneNode _targetBone;

        public bool _updating = false;

        private GoodColorDialog _dlgColor;
        private void colorBox_Click(object sender, EventArgs e)
        {
            if (TargetVertex == null)
                return;

            //RGBAPixel p = TargetVertex._colors[_colorIndex];
            //_dlgColor.Color = (Color)(ARGBPixel)p;
            //if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            //{
            //    p = (RGBAPixel)(ARGBPixel)_dlgColor.Color;
            //    colorBox.BackColor = Color.FromArgb(p.A, p.R, p.G, p.B);

            //    TargetVertex._colors[_colorIndex] = p;
            //    TargetVertex.SetColor(_colorIndex);
            //    TargetVertex._object._manager._dirty[_colorIndex + 2] = true;
            //    _mainWindow.UpdateModel();
            //}
        }

        public Vertex3 TargetVertex 
        {
            get 
            {
                if (_targetVertices != null && _targetVertices.Count != 0)
                    return _targetVertices[0];
                return null;
            }
        }

        private void colorIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            _colorIndex = colorIndex.SelectedIndex;
            UpdatePropDisplay();
        }

        //private void uvIndex_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    _uvIndex = uvIndex.SelectedIndex;
        //    UpdatePropDisplay();
        //}

        public void UpdatePropDisplay()
        {
            _updating = true;

            if (_targetVertices != null && _targetVertices.Count != 0)
                Enabled = true;
            else
                Enabled = false;

            Vertex3 vertex = TargetVertex;

            if (vertex == null || _targetVertices.Count > 1)
            {
                numPosX.Value = 0;
                numPosY.Value = 0;
                numPosZ.Value = 0;

                //numNormX.Value = 0;
                //numNormY.Value = 0;
                //numNormZ.Value = 0;

                //numTexX.Value = 0;
                //numTexY.Value = 0;

                //colorBox.BackColor = Color.FromKnownColor(KnownColor.Control);
            }
            else
            {
                Vector3 v3 = vertex.WeightedPosition;
                numPosX.Value = v3._x;
                numPosY.Value = v3._y;
                numPosZ.Value = v3._z;

                //v3 = vertex.WeightedNormal;
                //numNormX.Value = v3._x;
                //numNormY.Value = v3._y;
                //numNormZ.Value = v3._z;

                //Vector2 v2 = vertex._uvs[_uvIndex];
                //numTexX.Value = v2._x;
                //numTexY.Value = v2._y;

                //RGBAPixel p = vertex._colors[_colorIndex];
                //colorBox.BackColor = Color.FromArgb(p.A, p.R, p.G, p.B);
            }

            _updating = false;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<Vertex3> TargetVertices 
        {
            get { return _targetVertices; }
            set { _targetVertices = value; UpdatePropDisplay(); }
        }        
        public List<Vertex3> _targetVertices;

        private void numPosX_TextChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            if (_targetVertices != null)
            {
                if (_targetVertices.Count == 1)
                {
                    TargetVertex._weightedPosition._x = numPosX.Value;
                    TargetVertex.Unweight();
                }
                else
                {
                    foreach (Vertex3 v in _targetVertices)
                    {
                        v._weightedPosition._x += numPosX.Value;
                        v.Unweight();
                    }
                    numPosX.Value = 0;
                }
                _mainWindow.UpdateModel();
            }
        }

        private void numPosY_TextChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            if (_targetVertices != null)
            {
                if (_targetVertices.Count == 1)
                {
                    TargetVertex._weightedPosition._y = numPosY.Value;
                    TargetVertex.Unweight();
                }
                else
                {
                    foreach (Vertex3 v in _targetVertices)
                    {
                        v._weightedPosition._y += numPosY.Value;
                        v.Unweight();
                    }
                    numPosY.Value = 0;
                }
                _mainWindow.UpdateModel();
            }
        }

        private void numPosZ_TextChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            if (_targetVertices != null)
            {
                if (_targetVertices.Count == 1)
                {
                    TargetVertex._weightedPosition._z = numPosZ.Value;
                    TargetVertex.Unweight();
                }
                else
                {
                    foreach (Vertex3 v in _targetVertices)
                    {
                        v._weightedPosition._z += numPosZ.Value;
                        v.Unweight();
                    }
                    numPosZ.Value = 0;
                }
                _mainWindow.UpdateModel();
            }
        }

        private void numNormX_ValueChanged(object sender, EventArgs e)
        {
            //if (_updating)
            //    return;

            //if (TargetVertex != null)
            //{
            //    TargetVertex._weightedNormal._x = numNormX.Value;
            //    TargetVertex.UnweightNormal();
            //    TargetVertex.SetNormal();
            //    _mainWindow.UpdateModel();
            //}
        }

        private void numNormY_ValueChanged(object sender, EventArgs e)
        {
            //if (_updating)
            //    return;

            //if (TargetVertex != null)
            //{
            //    TargetVertex._weightedNormal._y = numNormY.Value;
            //    TargetVertex.UnweightNormal();
            //    TargetVertex.SetNormal();
            //    _mainWindow.UpdateModel();
            //}
        }

        private void numNormZ_ValueChanged(object sender, EventArgs e)
        {
            //if (_updating)
            //    return;

            //if (TargetVertex != null)
            //{
            //    TargetVertex._weightedNormal._z = numNormZ.Value;
            //    TargetVertex.UnweightNormal();
            //    TargetVertex.SetNormal();
            //    _mainWindow.UpdateModel();
            //}
        }

        //private void numTexX_ValueChanged(object sender, EventArgs e)
        //{
        //    if (_updating)
        //        return;

        //    if (TargetVertex != null)
        //    {
        //        TargetVertex._uvs[_uvIndex]._x = numTexX.Value;
        //        TargetVertex.SetUV(_uvIndex);
        //        TargetVertex._object._manager._dirty[_uvIndex + 4] = true;
        //        _mainWindow.UpdateModel();
        //    }
        //}

        //private void numTexY_ValueChanged(object sender, EventArgs e)
        //{
        //    if (_updating)
        //        return;

        //    if (TargetVertex != null)
        //    {
        //        TargetVertex._uvs[_uvIndex]._y = numTexY.Value;
        //        TargetVertex.SetUV(_uvIndex);
        //        TargetVertex._object._manager._dirty[_uvIndex + 4] = true;
        //        _mainWindow.UpdateModel();
        //    }
        //}
    }
}
