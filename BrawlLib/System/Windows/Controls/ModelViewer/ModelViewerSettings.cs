using System;
using System.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Animations;
using System.Collections.Generic;
using System.Drawing;
using BrawlLib.Imaging;

namespace System.Windows.Forms
{
    public class ModelViewerSettingsDialog : Form
    {
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private NumericInputBox ax;
        private NumericInputBox radius;
        private NumericInputBox dx;
        private NumericInputBox sx;
        private NumericInputBox sy;
        private NumericInputBox dy;
        private NumericInputBox azimuth;
        private NumericInputBox ay;
        private NumericInputBox sz;
        private NumericInputBox dz;
        private NumericInputBox elevation;
        private NumericInputBox az;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label label10;
        private Label label9;
        private Label label11;
        private Label label13;
        private Label label12;
        private Label label14;
        private NumericInputBox farZ;
        private NumericInputBox nearZ;
        private NumericInputBox yFov;
        private NumericInputBox zScale;
        private NumericInputBox tScale;
        private NumericInputBox rScale;
        private Label label17;
        private Label label16;
        private GroupBox groupBox3;
        private Label lblLineColor;
        private Label lblLineText;
        private Label label20;
        private Label lblOrbColor;
        private Label lblOrbText;
        private Label label15;
        private GroupBox groupBox4;
        private Label lblCol1Color;
        private Label lblCol1Text;
        private Label label24;
        private NumericInputBox maxUndoCount;
        private Label label18;
        private NumericInputBox ez;
        private NumericInputBox ey;
        private Label label8;
        private NumericInputBox ex;
        private Label label23;
        private Label label22;
        private Label label21;
        private Label label19;
        private IMainWindow _form;

        public ModelViewerSettingsDialog() 
        {
            InitializeComponent(); 
            _dlgColor = new GoodColorDialog(); 
            maxUndoCount._integral = true;
            _boxes[0] = ax;
            _boxes[1] = ay;
            _boxes[2] = az;
            _boxes[3] = ex;
            _boxes[4] = radius;
            _boxes[5] = azimuth;
            _boxes[6] = elevation;
            _boxes[7] = dx;
            _boxes[8] = dy;
            _boxes[9] = dz;
            _boxes[10] = ey;
            _boxes[11] = sx;
            _boxes[12] = sy;
            _boxes[13] = sz;
            _boxes[14] = ez;
            _boxes[15] = tScale;
            _boxes[16] = rScale;
            _boxes[17] = zScale;
            _boxes[18] = yFov;
            _boxes[19] = nearZ;
            _boxes[20] = farZ;
            _boxes[21] = maxUndoCount;

            for (int i = 0; i < 15; i++)
                if (i < 4 || i > 6)
                {
                    _boxes[i]._maxValue = 255;
                    _boxes[i]._minValue = 0;
                }
        }

        private NumericInputBox[] _boxes = new NumericInputBox[22];
        private float[] _origValues = new float[22];

        private Color _origNode, _origBone, _origFloor;

        public void Show(IMainWindow owner)
        {
            _form = owner;

            _form.RenderLightDisplay = true;

            ax.Value = _form.ModelPanel.Ambient._x * 255.0f;
            ay.Value = _form.ModelPanel.Ambient._y * 255.0f;
            az.Value = _form.ModelPanel.Ambient._z * 255.0f;

            radius.Value = _form.ModelPanel.LightPosition._x;
            azimuth.Value = _form.ModelPanel.LightPosition._y;
            elevation.Value = _form.ModelPanel.LightPosition._z;

            dx.Value = _form.ModelPanel.Diffuse._x * 255.0f;
            dy.Value = _form.ModelPanel.Diffuse._y * 255.0f;
            dz.Value = _form.ModelPanel.Diffuse._z * 255.0f;

            sx.Value = _form.ModelPanel.Specular._x * 255.0f;
            sy.Value = _form.ModelPanel.Specular._y * 255.0f;
            sz.Value = _form.ModelPanel.Specular._z * 255.0f;

            ex.Value = _form.ModelPanel.Emission._x * 255.0f;
            ey.Value = _form.ModelPanel.Emission._y * 255.0f;
            ez.Value = _form.ModelPanel.Emission._z * 255.0f;

            tScale.Value = _form.ModelPanel.TranslationScale;
            rScale.Value = _form.ModelPanel.RotationScale;
            zScale.Value = _form.ModelPanel.ZoomScale;

            yFov.Value = _form.ModelPanel._fovY;
            nearZ.Value = _form.ModelPanel._nearZ;
            farZ.Value = _form.ModelPanel._farZ;

            maxUndoCount.Value = _form.AllowedUndos;

            for (int i = 0; i < 22; i++)
            {
                _origValues[i] = _boxes[i].Value;
                _boxes[i].Tag = i;
            }

            _origBone = MDL0BoneNode.DefaultBoneColor;
            _origNode = MDL0BoneNode.DefaultNodeColor;
            _origFloor = StaticMainWindow._floorHue;

            UpdateOrb();
            UpdateLine();
            UpdateCol1();
            UpdateAmb();
            UpdateDif();
            UpdateSpe();
            UpdateEmi();

            base.Show(owner as IWin32Window);
        }

        private void BoxValueChanged(object sender, EventArgs e)
        {
            _boxes[5].Value = _boxes[5].Value.Clamp180Deg();
            _boxes[6].Value = _boxes[6].Value.Clamp180Deg();

            _form.ModelPanel.Ambient = new Vector4(_boxes[0].Value / 255.0f, _boxes[1].Value / 255.0f, _boxes[2].Value / 255.0f, 1.0f);
            _form.ModelPanel.LightPosition = new Vector4(_boxes[4].Value, _boxes[5].Value, _boxes[6].Value, 1.0f);
            _form.ModelPanel.Diffuse = new Vector4(_boxes[7].Value / 255.0f, _boxes[8].Value / 255.0f, _boxes[9].Value / 255.0f, 1.0f);
            _form.ModelPanel.Specular = new Vector4(_boxes[11].Value / 255.0f, _boxes[12].Value / 255.0f, _boxes[13].Value / 255.0f, 1.0f);
            _form.ModelPanel.Emission = new Vector4(_boxes[3].Value / 255.0f, _boxes[10].Value / 255.0f, _boxes[14].Value / 255.0f, 1.0f);

            _form.ModelPanel.TranslationScale = _boxes[15].Value;
            _form.ModelPanel.RotationScale = _boxes[16].Value;
            _form.ModelPanel.ZoomScale = _boxes[17].Value;

            _form.ModelPanel._fovY = _boxes[18].Value;
            _form.ModelPanel._nearZ = _boxes[19].Value;
            _form.ModelPanel._farZ = _boxes[20].Value;

            _form.AllowedUndos = (uint)Math.Abs(_boxes[21].Value);

            int i = (int)(sender as NumericInputBox).Tag;

            if (i == 3 || i == 10 || i == 14)
                UpdateEmi();
            else if (i < 3)
                UpdateAmb();
            else if (i < 10)
                UpdateDif();
            else
                UpdateSpe();

            _form.ModelPanel._projectionChanged = true;

            _form.ModelPanel.Invalidate();
        }

        private unsafe void btnOkay_Click(object sender, EventArgs e)
        {
            if (Math.Abs(_boxes[5].Value) == Math.Abs(_boxes[6].Value) &&
                _boxes[5].Value % 180.0f == 0 &&
                _boxes[6].Value % 180.0f == 0)
            {
                _boxes[5].Value = 0;
                _boxes[6].Value = 0;
            }

            _form.ModelPanel.Ambient = new Vector4(ax.Value / 255.0f, ay.Value / 255.0f, az.Value / 255.0f, 1.0f);
            _form.ModelPanel.LightPosition = new Vector4(radius.Value, azimuth.Value, elevation.Value, 1.0f);
            _form.ModelPanel.Diffuse = new Vector4(dx.Value / 255.0f, dy.Value / 255.0f, dz.Value / 255.0f, 1.0f);
            _form.ModelPanel.Specular = new Vector4(sx.Value / 255.0f, sy.Value / 255.0f, sz.Value / 255.0f, 1.0f);
            _form.ModelPanel.Emission = new Vector4(_boxes[3].Value / 255.0f, _boxes[10].Value / 255.0f, _boxes[14].Value / 255.0f, 1.0f);

            _form.ModelPanel.TranslationScale = tScale.Value;
            _form.ModelPanel.RotationScale = rScale.Value;
            _form.ModelPanel.ZoomScale = zScale.Value;

            _form.ModelPanel._fovY = yFov.Value;
            _form.ModelPanel._nearZ = nearZ.Value;
            _form.ModelPanel._farZ = farZ.Value;

            _form.AllowedUndos = (uint)Math.Abs(maxUndoCount.Value);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) 
        {
            _form.ModelPanel.Ambient = new Vector4(_origValues[0] / 255.0f, _origValues[1] / 255.0f, _origValues[2] / 255.0f, 1.0f);
            _form.ModelPanel.LightPosition = new Vector4(_origValues[4], _origValues[5], _origValues[6], 1.0f);
            _form.ModelPanel.Diffuse = new Vector4(_origValues[7] / 255.0f, _origValues[8] / 255.0f, _origValues[9] / 255.0f, 1.0f);
            _form.ModelPanel.Specular = new Vector4(_origValues[11] / 255.0f, _origValues[12] / 255.0f, _origValues[13] / 255.0f, 1.0f);
            _form.ModelPanel.Emission = new Vector4(_origValues[3] / 255.0f, _origValues[10] / 255.0f, _origValues[14] / 255.0f, 1.0f);

            _form.ModelPanel.TranslationScale = _origValues[15];
            _form.ModelPanel.RotationScale = _origValues[16];
            _form.ModelPanel.ZoomScale = _origValues[17];

            _form.ModelPanel._fovY = _origValues[18];
            _form.ModelPanel._nearZ = _origValues[19];
            _form.ModelPanel._farZ = _origValues[20];

            _form.AllowedUndos = (uint)Math.Abs(_origValues[21]);

            StaticMainWindow._floorHue = _origFloor;
            MDL0BoneNode.DefaultBoneColor = _origBone;
            MDL0BoneNode.DefaultNodeColor = _origNode;

            DialogResult = DialogResult.Cancel; 
            Close(); 
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            _form.ModelPanel._projectionChanged = true;
            _form.RenderLightDisplay = false;

            _form.ModelPanel.Invalidate();
        }

        #region Designer

        private Button btnCancel;
        private Button btnOkay;

        private void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOkay = new System.Windows.Forms.Button();
            this.ax = new System.Windows.Forms.NumericInputBox();
            this.radius = new System.Windows.Forms.NumericInputBox();
            this.dx = new System.Windows.Forms.NumericInputBox();
            this.sx = new System.Windows.Forms.NumericInputBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.sy = new System.Windows.Forms.NumericInputBox();
            this.dy = new System.Windows.Forms.NumericInputBox();
            this.azimuth = new System.Windows.Forms.NumericInputBox();
            this.ay = new System.Windows.Forms.NumericInputBox();
            this.sz = new System.Windows.Forms.NumericInputBox();
            this.dz = new System.Windows.Forms.NumericInputBox();
            this.elevation = new System.Windows.Forms.NumericInputBox();
            this.az = new System.Windows.Forms.NumericInputBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.ez = new System.Windows.Forms.NumericInputBox();
            this.ey = new System.Windows.Forms.NumericInputBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ex = new System.Windows.Forms.NumericInputBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.farZ = new System.Windows.Forms.NumericInputBox();
            this.nearZ = new System.Windows.Forms.NumericInputBox();
            this.yFov = new System.Windows.Forms.NumericInputBox();
            this.zScale = new System.Windows.Forms.NumericInputBox();
            this.tScale = new System.Windows.Forms.NumericInputBox();
            this.rScale = new System.Windows.Forms.NumericInputBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblLineColor = new System.Windows.Forms.Label();
            this.lblLineText = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.lblOrbColor = new System.Windows.Forms.Label();
            this.lblOrbText = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblCol1Color = new System.Windows.Forms.Label();
            this.lblCol1Text = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.maxUndoCount = new System.Windows.Forms.NumericInputBox();
            this.label18 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(231, 400);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOkay
            // 
            this.btnOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkay.Location = new System.Drawing.Point(150, 400);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 1;
            this.btnOkay.Text = "&Okay";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // ax
            // 
            this.ax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ax.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ax.Location = new System.Drawing.Point(88, 81);
            this.ax.Name = "ax";
            this.ax.Size = new System.Drawing.Size(50, 20);
            this.ax.TabIndex = 3;
            this.ax.Text = "0";
            this.ax.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // radius
            // 
            this.radius.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.radius.Location = new System.Drawing.Point(60, 35);
            this.radius.Name = "radius";
            this.radius.Size = new System.Drawing.Size(66, 20);
            this.radius.TabIndex = 4;
            this.radius.Text = "0";
            this.radius.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // dx
            // 
            this.dx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dx.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dx.Location = new System.Drawing.Point(88, 100);
            this.dx.Name = "dx";
            this.dx.Size = new System.Drawing.Size(50, 20);
            this.dx.TabIndex = 5;
            this.dx.Text = "0";
            this.dx.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // sx
            // 
            this.sx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.sx.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sx.Location = new System.Drawing.Point(88, 119);
            this.sx.Name = "sx";
            this.sx.Size = new System.Drawing.Size(50, 20);
            this.sx.TabIndex = 6;
            this.sx.Text = "0";
            this.sx.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(33, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Ambient:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Location = new System.Drawing.Point(60, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Radius";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(33, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "Diffuse:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.Location = new System.Drawing.Point(33, 119);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "Specular:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label5.Location = new System.Drawing.Point(88, 62);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 20);
            this.label5.TabIndex = 19;
            this.label5.Text = "R";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label6.Location = new System.Drawing.Point(137, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 20);
            this.label6.TabIndex = 20;
            this.label6.Text = "G";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label7.Location = new System.Drawing.Point(186, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 20);
            this.label7.TabIndex = 21;
            this.label7.Text = "B";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sy
            // 
            this.sy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.sy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sy.Location = new System.Drawing.Point(137, 119);
            this.sy.Name = "sy";
            this.sy.Size = new System.Drawing.Size(50, 20);
            this.sy.TabIndex = 26;
            this.sy.Text = "0";
            this.sy.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // dy
            // 
            this.dy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dy.Location = new System.Drawing.Point(137, 100);
            this.dy.Name = "dy";
            this.dy.Size = new System.Drawing.Size(50, 20);
            this.dy.TabIndex = 25;
            this.dy.Text = "0";
            this.dy.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // azimuth
            // 
            this.azimuth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.azimuth.Location = new System.Drawing.Point(129, 35);
            this.azimuth.Name = "azimuth";
            this.azimuth.Size = new System.Drawing.Size(66, 20);
            this.azimuth.TabIndex = 24;
            this.azimuth.Text = "0";
            this.azimuth.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // ay
            // 
            this.ay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ay.Location = new System.Drawing.Point(137, 81);
            this.ay.Name = "ay";
            this.ay.Size = new System.Drawing.Size(50, 20);
            this.ay.TabIndex = 23;
            this.ay.Text = "0";
            this.ay.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // sz
            // 
            this.sz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.sz.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sz.Location = new System.Drawing.Point(186, 119);
            this.sz.Name = "sz";
            this.sz.Size = new System.Drawing.Size(50, 20);
            this.sz.TabIndex = 30;
            this.sz.Text = "0";
            this.sz.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // dz
            // 
            this.dz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dz.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dz.Location = new System.Drawing.Point(186, 100);
            this.dz.Name = "dz";
            this.dz.Size = new System.Drawing.Size(50, 20);
            this.dz.TabIndex = 29;
            this.dz.Text = "0";
            this.dz.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // elevation
            // 
            this.elevation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.elevation.Location = new System.Drawing.Point(197, 35);
            this.elevation.Name = "elevation";
            this.elevation.Size = new System.Drawing.Size(66, 20);
            this.elevation.TabIndex = 28;
            this.elevation.Text = "0";
            this.elevation.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // az
            // 
            this.az.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.az.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.az.Location = new System.Drawing.Point(186, 81);
            this.az.Name = "az";
            this.az.Size = new System.Drawing.Size(50, 20);
            this.az.TabIndex = 27;
            this.az.Text = "0";
            this.az.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label23);
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.label21);
            this.groupBox1.Controls.Add(this.label19);
            this.groupBox1.Controls.Add(this.ez);
            this.groupBox1.Controls.Add(this.ey);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.ex);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.sz);
            this.groupBox1.Controls.Add(this.dz);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.radius);
            this.groupBox1.Controls.Add(this.az);
            this.groupBox1.Controls.Add(this.elevation);
            this.groupBox1.Controls.Add(this.sy);
            this.groupBox1.Controls.Add(this.azimuth);
            this.groupBox1.Controls.Add(this.dy);
            this.groupBox1.Controls.Add(this.ay);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.sx);
            this.groupBox1.Controls.Add(this.dx);
            this.groupBox1.Controls.Add(this.ax);
            this.groupBox1.Location = new System.Drawing.Point(0, 91);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(315, 167);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Lighting";
            // 
            // label23
            // 
            this.label23.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label23.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label23.Location = new System.Drawing.Point(235, 138);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(40, 20);
            this.label23.TabIndex = 43;
            this.label23.Click += new System.EventHandler(this.label23_Click);
            // 
            // label22
            // 
            this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label22.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label22.Location = new System.Drawing.Point(235, 119);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(40, 20);
            this.label22.TabIndex = 42;
            this.label22.Click += new System.EventHandler(this.label22_Click);
            // 
            // label21
            // 
            this.label21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label21.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label21.Location = new System.Drawing.Point(235, 100);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(40, 20);
            this.label21.TabIndex = 41;
            this.label21.Click += new System.EventHandler(this.label21_Click);
            // 
            // label19
            // 
            this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label19.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label19.Location = new System.Drawing.Point(235, 81);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(40, 20);
            this.label19.TabIndex = 11;
            this.label19.Click += new System.EventHandler(this.label19_Click);
            // 
            // ez
            // 
            this.ez.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ez.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ez.Location = new System.Drawing.Point(186, 138);
            this.ez.Name = "ez";
            this.ez.Size = new System.Drawing.Size(50, 20);
            this.ez.TabIndex = 40;
            this.ez.Text = "0";
            // 
            // ey
            // 
            this.ey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ey.Location = new System.Drawing.Point(137, 138);
            this.ey.Name = "ey";
            this.ey.Size = new System.Drawing.Size(50, 20);
            this.ey.TabIndex = 39;
            this.ey.Text = "0";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label8.Location = new System.Drawing.Point(33, 138);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 20);
            this.label8.TabIndex = 38;
            this.label8.Text = "Emission:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ex
            // 
            this.ex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ex.Location = new System.Drawing.Point(88, 138);
            this.ex.Name = "ex";
            this.ex.Size = new System.Drawing.Size(50, 20);
            this.ex.TabIndex = 37;
            this.ex.Text = "0";
            // 
            // label17
            // 
            this.label17.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label17.Location = new System.Drawing.Point(197, 16);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(66, 20);
            this.label17.TabIndex = 36;
            this.label17.Text = "Elevation";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label16
            // 
            this.label16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label16.Location = new System.Drawing.Point(129, 16);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(66, 20);
            this.label16.TabIndex = 35;
            this.label16.Text = "Azimuth";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.farZ);
            this.groupBox2.Controls.Add(this.nearZ);
            this.groupBox2.Controls.Add(this.yFov);
            this.groupBox2.Controls.Add(this.zScale);
            this.groupBox2.Controls.Add(this.tScale);
            this.groupBox2.Controls.Add(this.rScale);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(315, 91);
            this.groupBox2.TabIndex = 36;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Projection";
            // 
            // farZ
            // 
            this.farZ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.farZ.Location = new System.Drawing.Point(247, 57);
            this.farZ.Name = "farZ";
            this.farZ.Size = new System.Drawing.Size(60, 20);
            this.farZ.TabIndex = 11;
            this.farZ.Text = "0";
            this.farZ.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // nearZ
            // 
            this.nearZ.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nearZ.Location = new System.Drawing.Point(247, 38);
            this.nearZ.Name = "nearZ";
            this.nearZ.Size = new System.Drawing.Size(60, 20);
            this.nearZ.TabIndex = 10;
            this.nearZ.Text = "0";
            this.nearZ.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // yFov
            // 
            this.yFov.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.yFov.Location = new System.Drawing.Point(247, 19);
            this.yFov.Name = "yFov";
            this.yFov.Size = new System.Drawing.Size(60, 20);
            this.yFov.TabIndex = 9;
            this.yFov.Text = "0";
            this.yFov.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // zScale
            // 
            this.zScale.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.zScale.Location = new System.Drawing.Point(109, 57);
            this.zScale.Name = "zScale";
            this.zScale.Size = new System.Drawing.Size(50, 20);
            this.zScale.TabIndex = 8;
            this.zScale.Text = "0";
            this.zScale.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // tScale
            // 
            this.tScale.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tScale.Location = new System.Drawing.Point(109, 38);
            this.tScale.Name = "tScale";
            this.tScale.Size = new System.Drawing.Size(50, 20);
            this.tScale.TabIndex = 7;
            this.tScale.Text = "0";
            this.tScale.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // rScale
            // 
            this.rScale.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rScale.Location = new System.Drawing.Point(109, 19);
            this.rScale.Name = "rScale";
            this.rScale.Size = new System.Drawing.Size(50, 20);
            this.rScale.TabIndex = 6;
            this.rScale.Text = "0";
            this.rScale.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // label14
            // 
            this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label14.Location = new System.Drawing.Point(158, 57);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(90, 20);
            this.label14.TabIndex = 5;
            this.label14.Text = "Far Z: ";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label13.Location = new System.Drawing.Point(158, 38);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(90, 20);
            this.label13.TabIndex = 4;
            this.label13.Text = "Near Z: ";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label12.Location = new System.Drawing.Point(158, 19);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(90, 20);
            this.label12.TabIndex = 3;
            this.label12.Text = "Y Field Of View: ";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label11.Location = new System.Drawing.Point(10, 57);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 20);
            this.label11.TabIndex = 2;
            this.label11.Text = "Zoom Scale: ";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label10.Location = new System.Drawing.Point(10, 38);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(100, 20);
            this.label10.TabIndex = 1;
            this.label10.Text = "Translation Scale: ";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label9.Location = new System.Drawing.Point(10, 19);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 20);
            this.label9.TabIndex = 0;
            this.label9.Text = "Rotation Scale: ";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.lblLineColor);
            this.groupBox3.Controls.Add(this.lblLineText);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.lblOrbColor);
            this.groupBox3.Controls.Add(this.lblOrbText);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Location = new System.Drawing.Point(0, 258);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(315, 62);
            this.groupBox3.TabIndex = 37;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Bones";
            // 
            // lblLineColor
            // 
            this.lblLineColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLineColor.Location = new System.Drawing.Point(263, 35);
            this.lblLineColor.Name = "lblLineColor";
            this.lblLineColor.Size = new System.Drawing.Size(40, 20);
            this.lblLineColor.TabIndex = 8;
            this.lblLineColor.Click += new System.EventHandler(this.lblLineColor_Click);
            // 
            // lblLineText
            // 
            this.lblLineText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLineText.BackColor = System.Drawing.Color.White;
            this.lblLineText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLineText.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLineText.Location = new System.Drawing.Point(76, 35);
            this.lblLineText.Name = "lblLineText";
            this.lblLineText.Size = new System.Drawing.Size(188, 20);
            this.lblLineText.TabIndex = 10;
            this.lblLineText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label20
            // 
            this.label20.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label20.Location = new System.Drawing.Point(6, 35);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(71, 20);
            this.label20.TabIndex = 9;
            this.label20.Text = "Line Color:";
            this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOrbColor
            // 
            this.lblOrbColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOrbColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblOrbColor.Location = new System.Drawing.Point(263, 16);
            this.lblOrbColor.Name = "lblOrbColor";
            this.lblOrbColor.Size = new System.Drawing.Size(40, 20);
            this.lblOrbColor.TabIndex = 5;
            this.lblOrbColor.Click += new System.EventHandler(this.lblOrbColor_Click);
            // 
            // lblOrbText
            // 
            this.lblOrbText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOrbText.BackColor = System.Drawing.Color.White;
            this.lblOrbText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblOrbText.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOrbText.Location = new System.Drawing.Point(76, 16);
            this.lblOrbText.Name = "lblOrbText";
            this.lblOrbText.Size = new System.Drawing.Size(188, 20);
            this.lblOrbText.TabIndex = 7;
            this.lblOrbText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label15
            // 
            this.label15.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label15.Location = new System.Drawing.Point(6, 16);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(71, 20);
            this.label15.TabIndex = 6;
            this.label15.Text = "Orb Color:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.lblCol1Color);
            this.groupBox4.Controls.Add(this.lblCol1Text);
            this.groupBox4.Controls.Add(this.label24);
            this.groupBox4.Location = new System.Drawing.Point(0, 326);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(315, 42);
            this.groupBox4.TabIndex = 38;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Floor";
            // 
            // lblCol1Color
            // 
            this.lblCol1Color.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCol1Color.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCol1Color.Location = new System.Drawing.Point(263, 16);
            this.lblCol1Color.Name = "lblCol1Color";
            this.lblCol1Color.Size = new System.Drawing.Size(40, 20);
            this.lblCol1Color.TabIndex = 5;
            this.lblCol1Color.Click += new System.EventHandler(this.lblCol1Color_Click);
            // 
            // lblCol1Text
            // 
            this.lblCol1Text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCol1Text.BackColor = System.Drawing.Color.White;
            this.lblCol1Text.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCol1Text.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCol1Text.Location = new System.Drawing.Point(76, 16);
            this.lblCol1Text.Name = "lblCol1Text";
            this.lblCol1Text.Size = new System.Drawing.Size(188, 20);
            this.lblCol1Text.TabIndex = 7;
            this.lblCol1Text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label24
            // 
            this.label24.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label24.Location = new System.Drawing.Point(6, 16);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(71, 20);
            this.label24.TabIndex = 6;
            this.label24.Text = "Color:";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // maxUndoCount
            // 
            this.maxUndoCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.maxUndoCount.Location = new System.Drawing.Point(240, 374);
            this.maxUndoCount.Name = "maxUndoCount";
            this.maxUndoCount.Size = new System.Drawing.Size(66, 20);
            this.maxUndoCount.TabIndex = 37;
            this.maxUndoCount.Text = "0";
            this.maxUndoCount.ValueChanged += new System.EventHandler(this.BoxValueChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(120, 376);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(114, 13);
            this.label18.TabIndex = 39;
            this.label18.Text = "Undo Buffer Maximum:";
            // 
            // ModelViewerSettingsDialog
            // 
            this.AcceptButton = this.btnOkay;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(315, 432);
            this.Controls.Add(this.maxUndoCount);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnOkay);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ModelViewerSettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Viewer Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private GoodColorDialog _dlgColor;
        private void lblOrbColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = MDL0BoneNode.DefaultNodeColor;
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                MDL0BoneNode.DefaultNodeColor = _dlgColor.Color;
                UpdateOrb();
            }
        }

        private void lblLineColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = MDL0BoneNode.DefaultBoneColor;
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                MDL0BoneNode.DefaultBoneColor = _dlgColor.Color;
                UpdateLine();
            }
        }

        private void lblCol1Color_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = StaticMainWindow._floorHue;
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                StaticMainWindow._floorHue = _dlgColor.Color;
                UpdateCol1();
            }
        }

        private void UpdateOrb()
        {
            lblOrbText.Text = ((ARGBPixel)MDL0BoneNode.DefaultNodeColor).ToString();
            lblOrbColor.BackColor = Color.FromArgb(MDL0BoneNode.DefaultNodeColor.R, MDL0BoneNode.DefaultNodeColor.G, MDL0BoneNode.DefaultNodeColor.B);
            _form.ModelPanel.Invalidate();
        }
        private void UpdateLine()
        {
            lblLineText.Text = ((ARGBPixel)MDL0BoneNode.DefaultBoneColor).ToString();
            lblLineColor.BackColor = Color.FromArgb(MDL0BoneNode.DefaultBoneColor.R, MDL0BoneNode.DefaultBoneColor.G, MDL0BoneNode.DefaultBoneColor.B);
            _form.ModelPanel.Invalidate();
        }
        private void UpdateCol1()
        {
            lblCol1Text.Text = ((ARGBPixel)StaticMainWindow._floorHue).ToString();
            lblCol1Color.BackColor = Color.FromArgb(StaticMainWindow._floorHue.R, StaticMainWindow._floorHue.G, StaticMainWindow._floorHue.B);
            _form.ModelPanel.Invalidate();
        }
        private void UpdateAmb()
        {
            label19.BackColor = Color.FromArgb(255, (int)(ax.Value), (int)(ay.Value), (int)(az.Value));
            _form.ModelPanel.Ambient = new Vector4(ax.Value / 255.0f, ay.Value / 255.0f, az.Value / 255.0f, 1.0f);
        }
        private void UpdateDif()
        {
            label21.BackColor = Color.FromArgb(255, (int)(dx.Value), (int)(dy.Value), (int)(dz.Value));
            _form.ModelPanel.Diffuse = new Vector4(dx.Value / 255.0f, dy.Value / 255.0f, dz.Value / 255.0f, 1.0f);
        }
        private void UpdateSpe()
        {
            label22.BackColor = Color.FromArgb(255, (int)(sx.Value), (int)(sy.Value), (int)(sz.Value));
            _form.ModelPanel.Specular = new Vector4(sx.Value / 255.0f, sy.Value / 255.0f, sz.Value / 255.0f, 1.0f);
        }
        private void UpdateEmi()
        {
            label23.BackColor = Color.FromArgb(255, (int)(ex.Value), (int)(ey.Value), (int)(ez.Value));
            _form.ModelPanel.Emission = new Vector4(ex.Value / 255.0f, ey.Value / 255.0f, ez.Value / 255.0f, 1.0f);
        }
        public bool _updating = false;
        private void label19_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = Color.FromArgb(255, (int)(ax.Value), (int)(ay.Value), (int)(az.Value));
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                _updating = true;
                ax.Value = (float)_dlgColor.Color.R;
                ay.Value = (float)_dlgColor.Color.G;
                az.Value = (float)_dlgColor.Color.B;
                _updating = false;
                UpdateAmb();
            }
        }

        private void label21_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = Color.FromArgb(255, (int)(dx.Value), (int)(dy.Value), (int)(dz.Value));
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                _updating = true;
                dx.Value = (float)_dlgColor.Color.R;
                dy.Value = (float)_dlgColor.Color.G;
                dz.Value = (float)_dlgColor.Color.B;
                _updating = false;
                UpdateDif();
            }
        }

        private void label22_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = Color.FromArgb(255, (int)(sx.Value), (int)(sy.Value), (int)(sz.Value));
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                _updating = true;
                sx.Value = (float)_dlgColor.Color.R;
                sy.Value = (float)_dlgColor.Color.G;
                sz.Value = (float)_dlgColor.Color.B;
                _updating = false;
                UpdateSpe();
            }
        }

        private void label23_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = Color.FromArgb(255, (int)(ex.Value), (int)(ey.Value), (int)(ez.Value));
            if (_dlgColor.ShowDialog(this) == DialogResult.OK)
            {
                _updating = true;
                ex.Value = (float)_dlgColor.Color.R;
                ey.Value = (float)_dlgColor.Color.G;
                ez.Value = (float)_dlgColor.Color.B;
                _updating = false;
                UpdateEmi();
            }
        }
    }
}
