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

namespace System.Windows.Forms
{
    public class WeightEditor : UserControl
    {
        #region Designer

        private System.ComponentModel.IContainer components;
        private void InitializeComponent()
        {
            this.lstBoneWeights = new System.Windows.Forms.RefreshableListBox();
            this.btnSetWeight = new System.Windows.Forms.Button();
            this.numWeight = new System.Windows.Forms.NumericInputBox();
            this.btnBlend = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnSubtract = new System.Windows.Forms.Button();
            this.btnLock = new System.Windows.Forms.Button();
            this.lblBoneName = new System.Windows.Forms.Label();
            this.btnRemove = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstBoneWeights
            // 
            this.lstBoneWeights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstBoneWeights.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstBoneWeights.FormattingEnabled = true;
            this.lstBoneWeights.IntegralHeight = false;
            this.lstBoneWeights.Location = new System.Drawing.Point(0, 3);
            this.lstBoneWeights.Name = "lstBoneWeights";
            this.lstBoneWeights.Size = new System.Drawing.Size(124, 75);
            this.lstBoneWeights.TabIndex = 0;
            this.lstBoneWeights.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstBoneWeights_DrawItem);
            this.lstBoneWeights.SelectedIndexChanged += new System.EventHandler(this.lstBoneWeights_SelectedIndexChanged);
            // 
            // btnSetWeight
            // 
            this.btnSetWeight.Location = new System.Drawing.Point(67, 28);
            this.btnSetWeight.Name = "btnSetWeight";
            this.btnSetWeight.Size = new System.Drawing.Size(79, 22);
            this.btnSetWeight.TabIndex = 2;
            this.btnSetWeight.Text = "Set Weight";
            this.btnSetWeight.UseVisualStyleBackColor = true;
            this.btnSetWeight.Click += new System.EventHandler(this.btnSetWeight_Click);
            // 
            // numWeight
            // 
            this.numWeight.Location = new System.Drawing.Point(3, 29);
            this.numWeight.Name = "numWeight";
            this.numWeight.Size = new System.Drawing.Size(62, 20);
            this.numWeight.TabIndex = 3;
            this.numWeight.Text = "0";
            // 
            // btnBlend
            // 
            this.btnBlend.Location = new System.Drawing.Point(130, 52);
            this.btnBlend.Name = "btnBlend";
            this.btnBlend.Size = new System.Drawing.Size(62, 23);
            this.btnBlend.TabIndex = 4;
            this.btnBlend.Text = "Blend";
            this.btnBlend.UseVisualStyleBackColor = true;
            this.btnBlend.Visible = false;
            this.btnBlend.Click += new System.EventHandler(this.btnBlend_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(3, 52);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(62, 23);
            this.btnCopy.TabIndex = 5;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Visible = false;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.Location = new System.Drawing.Point(67, 52);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(62, 23);
            this.btnPaste.TabIndex = 6;
            this.btnPaste.Text = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Visible = false;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(147, 28);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(22, 22);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "+";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnSubtract
            // 
            this.btnSubtract.Location = new System.Drawing.Point(170, 28);
            this.btnSubtract.Name = "btnSubtract";
            this.btnSubtract.Size = new System.Drawing.Size(22, 22);
            this.btnSubtract.TabIndex = 8;
            this.btnSubtract.Text = "-";
            this.btnSubtract.UseVisualStyleBackColor = true;
            this.btnSubtract.Click += new System.EventHandler(this.btnSubtract_Click);
            // 
            // btnLock
            // 
            this.btnLock.Location = new System.Drawing.Point(3, 3);
            this.btnLock.Name = "btnLock";
            this.btnLock.Size = new System.Drawing.Size(61, 23);
            this.btnLock.TabIndex = 10;
            this.btnLock.Text = "Lock";
            this.btnLock.UseVisualStyleBackColor = true;
            this.btnLock.Click += new System.EventHandler(this.btnLock_Click);
            // 
            // lblBoneName
            // 
            this.lblBoneName.AutoSize = true;
            this.lblBoneName.Location = new System.Drawing.Point(134, 8);
            this.lblBoneName.Name = "lblBoneName";
            this.lblBoneName.Size = new System.Drawing.Size(32, 13);
            this.lblBoneName.TabIndex = 11;
            this.lblBoneName.Text = "Bone";
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(67, 3);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(61, 23);
            this.btnRemove.TabIndex = 12;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemoveBone_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnAdd);
            this.panel1.Controls.Add(this.numWeight);
            this.panel1.Controls.Add(this.btnBlend);
            this.panel1.Controls.Add(this.btnPaste);
            this.panel1.Controls.Add(this.btnSetWeight);
            this.panel1.Controls.Add(this.btnCopy);
            this.panel1.Controls.Add(this.btnSubtract);
            this.panel1.Controls.Add(this.btnLock);
            this.panel1.Controls.Add(this.lblBoneName);
            this.panel1.Controls.Add(this.btnRemove);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(127, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(193, 78);
            this.panel1.TabIndex = 14;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(124, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 78);
            this.splitter1.TabIndex = 15;
            this.splitter1.TabStop = false;
            this.splitter1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitter1_MouseDown);
            this.splitter1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitter1_MouseMove);
            this.splitter1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitter1_MouseUp);
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter2.Location = new System.Drawing.Point(0, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(124, 3);
            this.splitter2.TabIndex = 16;
            this.splitter2.TabStop = false;
            this.splitter2.SplitterMoving += new System.Windows.Forms.SplitterEventHandler(this.splitter2_SplitterMoving);
            this.splitter2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitter2_MouseDown);
            this.splitter2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitter2_MouseMove);
            this.splitter2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitter2_MouseUp);
            // 
            // WeightEditor
            // 
            this.Controls.Add(this.lstBoneWeights);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Name = "WeightEditor";
            this.Size = new System.Drawing.Size(320, 78);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public WeightEditor() { InitializeComponent(); }

        private RefreshableListBox lstBoneWeights;
        private BindingList<BoneWeight> _targetWeights;

        public MDL0BoneNode[] Bones { get { return _targetWeights.Select(x => x.Bone).ToArray(); } }
        public float[] Weights { get { return _targetWeights.Select(x => x.Weight).ToArray(); } }

        public IMainWindow _mainWindow;
        private Button btnSetWeight;
        private NumericInputBox numWeight;
        private Button btnBlend;
        private Button btnCopy;
        private Button btnPaste;
        private Button btnAdd;
        private Label lblBoneName;
        private Button btnSubtract;

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
        public MDL0BoneNode SelectedBone { get { return _mainWindow.SelectedBone; } set { _mainWindow.SelectedBone = value; } }

        private Vertex3 _vertex = null;
        private Button btnLock;

        public List<Vertex3> _targetVertices;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<Vertex3> TargetVertices
        {
            get { return _targetVertices; }
            set { SetVertices(value); }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BoneWeight TargetBoneWeight
        {
            get { return _targetBoneWeight; }
            set 
            {
                if ((_targetBoneWeight = value) != null)
                {
                    _mainWindow.SelectedBone = TargetBoneWeight.Bone;
                    btnAdd.Enabled = _targetBoneWeight.Weight != 1.0f && lstBoneWeights.Items.Count != 1;
                    btnSubtract.Enabled = _targetBoneWeight.Weight != 0.0f && lstBoneWeights.Items.Count != 1;
                    btnRemove.Enabled = true;
                }
                else
                {
                    btnRemove.Enabled = false;
                    btnAdd.Enabled = false;
                    btnSubtract.Enabled = false;
                } 
                _mainWindow.ModelPanel.Invalidate();
            }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MDL0BoneNode TargetBone
        {
            get { return _targetBone; }
            set
            {
                _targetBone = value;

                if (_targetBone != null)
                {
                    lblBoneName.Text = _targetBone.Name;
                    btnLock.Enabled = true;
                    btnLock.Text = _targetBone._locked ? "Unlock" : "Lock";
                    if (_bones != null)
                    {
                        int i = _bones.IndexOf(_targetBone);
                        lstBoneWeights.SelectedIndex = i;
                        numWeight.Value = i != -1 ? ((BoneWeight)lstBoneWeights.Items[i]).Weight * 100.0f : 0;
                    }
                    else
                    {
                        numWeight.Value = 0;
                        lstBoneWeights.SelectedIndex = -1;
                    }
                }
                else
                {
                    btnLock.Enabled = false;
                    btnLock.Text = "Lock";
                    lblBoneName.Text = "(None)";
                    numWeight.Value = 0;
                    lstBoneWeights.SelectedIndex = -1;
                }
            }
        }
        public BoneWeight _targetBoneWeight;
        private Button btnRemove;
        private Panel panel1;
        private Splitter splitter1;
        private Splitter splitter2;
        public MDL0BoneNode _targetBone;
        public void SetVertices(List<Vertex3> vertices)
        {
            //Remove weights with value 0 from the current vertex's influence
            if (_targetVertices != null)
            foreach (Vertex3 v in _targetVertices)
                if (v != null)
                {
                    List<BoneWeight> b = v.GetBoneWeights();
                    for (int i = 0; i < b.Count; i++)
                        if (b[i].Weight == 0.0f)
                            b.RemoveAt(i--);

                    if (b.Count == 1) //Change influence to a bone
                        v.MatrixNode = b[0].Bone;
                }

            _targetVertices = vertices.ToList();

            ResetList();
        }
        public float _weightTotal = 0;
        Dictionary<string, float[]> _totals = new Dictionary<string, float[]>();
        public List<MDL0BoneNode> _bones;
        public void ResetList()
        {
            lstBoneWeights.Items.Clear();

            _bones = new List<MDL0BoneNode>();
            _totals = new Dictionary<string, float[]>();
            _weightTotal = 0;
            foreach (Vertex3 v in _targetVertices)
            {
                List<BoneWeight> array = v.GetBoneWeights();
                foreach (BoneWeight b in array)
                {
                    if (!_bones.Contains(b.Bone))
                    {
                        _bones.Add(b.Bone);
                        _totals.Add(b.Bone.Name, new float[] { b.Weight, 1 });
                    }
                    else
                    {
                        _totals[b.Bone.Name][0] += b.Weight;
                        _totals[b.Bone.Name][1] += 1;
                    }
                    _weightTotal += b.Weight;
                }
            }

            foreach (MDL0BoneNode b in _bones)
                lstBoneWeights.Items.Add(new BoneWeight(b, (_totals[b.Name][0] / _weightTotal)));
            
            if (_bones.Contains(_mainWindow.SelectedBone))
                lstBoneWeights.SelectedIndex = _bones.IndexOf(_mainWindow.SelectedBone);
        }

        public bool _updating = false;

        public void SetWeight(float value) 
        {
            foreach (Vertex3 v in TargetVertices) 
                SetWeight(value, v);
        }
        public void SetWeight(float value, Vertex3 vertex)
        {
            Weight(value, vertex, false);
        }
        public void IncrementWeight(float value)
        {
            foreach (Vertex3 v in TargetVertices) 
                IncrementWeight(value, v); 
        }
        public void IncrementWeight(float value, Vertex3 vertex)
        {
            Weight(value, vertex, true);
        }
        private float RoundValue(float value, float max)
        {
            return (float)Math.Round(value.Clamp(0.0f, max), 7);
        }
        private void Weight(float value, Vertex3 vertex, bool increment)
        {
            //LET'S TANGO

            Influence targetInf = null;
            BoneWeight targetWeight = null;
            float max = 1.0f;
            int selectedIndex = 0;

            IMatrixNode node = vertex.MatrixNode;

            if (node == null)
            {
                vertex._object.ConvertInf();
                node = vertex.MatrixNode;
            }

            List<BoneWeight> weights = node.Weights;

            if (_targetBone == null || _targetBone._locked)
                return;

            MDL0BoneNode origBone = null;
            if (node is MDL0BoneNode)
            {
                origBone = node as MDL0BoneNode;
                node = new Influence(origBone);
            }

            bool refs = node.Users.Count > 1;

            if (refs)
                targetInf = (node as Influence).Clone();
            else
                targetInf = (node as Influence);

            weights = targetInf._weights;

            selectedIndex = vertex.IndexOfBone(TargetBone);
            if (selectedIndex == -1)
            {
                weights.Add(new BoneWeight(TargetBone, 0.0f));
                selectedIndex = weights.Count - 1;
            }

            targetWeight = targetInf._weights[selectedIndex];
            if (targetWeight.Locked) 
                return;

            max = 1.0f;
            foreach (BoneWeight b in weights)
                if (b.Locked)
                    max -= b.Weight;

            value = increment ? RoundValue(targetWeight.Weight + value, max) : RoundValue(value, max);

            if (targetWeight.Weight == value) 
                return;

            List<int> editableWeights = new List<int>();

            int c = 0;
            foreach (BoneWeight b in targetInf._weights)
            {
                if (!b.Locked && c != selectedIndex)
                    editableWeights.Add(c);
                c++;
            }

            if (editableWeights.Count == 0) 
                return;

            float diff = targetWeight.Weight - value;
            targetWeight.Weight = value;

            float val = diff / (editableWeights.Count);
            if (value != max)
                foreach (int i in editableWeights)
                    targetInf._weights[i].Weight = (float)Math.Round((targetInf._weights[i].Weight + val).Clamp(0.0f, 1.0f), 7);
            else
                foreach (int i in editableWeights)
                    targetInf._weights[i].Weight = 0;

            //Don't want the modified value to be normalized
            bool locked = targetWeight.Locked;
            targetWeight.Locked = true;
            targetInf.Normalize();
            targetWeight.Locked = locked;

            vertex.MatrixNode = vertex._object.Model._influences.FindOrCreate(targetInf, false);
            vertex._object.ConvertInf();
            vertex._object.Model.SignalPropertyChange();

            _mainWindow.UpdateModel();
        }
        public void UpdateValues()
        {
            lstBoneWeights.RefreshItems();
            btnAdd.Enabled = _targetBoneWeight.Weight != 1.0f;
            btnSubtract.Enabled = _targetBoneWeight.Weight != 0.0f;
            btnRemove.Enabled = _targetBoneWeight != null;
            if (_targetBoneWeight != null)
                numWeight.Value = _targetBoneWeight.Weight * 100.0f;
        }
        public void BoneChanged()
        {
            TargetBone = SelectedBone;
        }

        private void lstBoneWeights_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstBoneWeights.SelectedIndex >= 0)
                TargetBoneWeight = lstBoneWeights.Items[lstBoneWeights.SelectedIndex] as BoneWeight;
            else
                TargetBoneWeight = null;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {

        }

        private void btnPaste_Click(object sender, EventArgs e)
        {

        }

        private void btnBlend_Click(object sender, EventArgs e)
        {

        }

        private void btnRemoveBone_Click(object sender, EventArgs e)
        {
            if (TargetBoneWeight != null && lstBoneWeights.SelectedIndex != -1)
            {
                //int i = lstBoneWeights.SelectedIndex;
                if (_bones.Count == 2)
                    _bones[0]._locked = false;
                SetWeight(0.0f);
                int x;
                foreach (Vertex3 v in TargetVertices)
                    if (v.MatrixNode != null)
                    {
                        if (v.MatrixNode.Users.Count > 1)
                            if (v.MatrixNode is Influence)
                                _vertex.MatrixNode = (v.MatrixNode as Influence).Clone();
                            else
                                _vertex.MatrixNode = new Influence(v.MatrixNode as MDL0BoneNode);

                        if ((x = v.IndexOfBone(TargetBone)) != -1)
                            v.MatrixNode.Weights.RemoveAt(x);
                    }
                ResetList();
                //lstBoneWeights.SelectedIndex = i.Clamp(0, lstBoneWeights.Items.Count - 1);
            }
        }

        private float _increment = 0.1f;
        public float WeightIncrement { get { return _increment; } set { _increment = value; } }
        
        private void btnSubtract_Click(object sender, EventArgs e)
        {
            IncrementWeight(-_increment);
            ResetList();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            IncrementWeight(_increment);
            ResetList();
        }

        private void btnSetWeight_Click(object sender, EventArgs e)
        {
            SetWeight(numWeight.Value / 100.0f);
            ResetList();
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            int i = lstBoneWeights.SelectedIndex;
            TargetBone._locked = !TargetBone._locked;
            lstBoneWeights.RefreshItems();
            lstBoneWeights.SelectedIndex = i;
            btnLock.Text = _targetBone._locked ? "Unlock" : "Lock";
        }

        private void lstBoneWeights_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            bool selected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);

            int index = e.Index;
            if (index >= 0 && index < lstBoneWeights.Items.Count)
            {
                string text = lstBoneWeights.Items[index].ToString();
                Graphics g = e.Graphics;

                Color color = selected ? Color.FromKnownColor(KnownColor.Highlight) : _bones[index]._locked ? Color.Red : Color.White;
                g.FillRectangle(new SolidBrush(color), e.Bounds);

                g.DrawString(text, e.Font, selected ? Brushes.White : Brushes.Black,
                    lstBoneWeights.GetItemRectangle(index).Location);
            }

            e.DrawFocusRectangle();
        }

        private void splitter2_SplitterMoving(object sender, SplitterEventArgs e)
        {
            int diff = e.Y - e.SplitY;
            _mainWindow.AnimCtrlPnl.Height += diff;
        }

        bool _resizing = false;
        int o = 0;
        private void splitter2_MouseDown(object sender, MouseEventArgs e)
        {
            _resizing = true;
            o = e.Y;
        }

        private void splitter2_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
                _mainWindow.AnimEditors.Height += o - e.Y; 
        }

        private void splitter2_MouseUp(object sender, MouseEventArgs e)
        {
            _resizing = false;
        }

        private void splitter1_MouseDown(object sender, MouseEventArgs e)
        {
            _resizing = true;
            o = e.X;
        }

        private void splitter1_MouseUp(object sender, MouseEventArgs e)
        {
            _resizing = false;
            _mainWindow.CheckDimensions();
        }

        private void splitter1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
                _mainWindow.AnimCtrlPnl.Width += e.X - o;
        }
    }

    public class RefreshableListBox : ListBox
    {
        public new void RefreshItem(int index) { base.RefreshItem(index); }
        public new void RefreshItems() { base.RefreshItems(); }
    }
}
