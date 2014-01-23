using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public partial class InterpolationEditor : UserControl
    {
        public IMainWindow _mainWindow;

        public InterpolationEditor() { _mainWindow = null; }
        public InterpolationEditor(IMainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            numericInputBox3._integral = true;
            comboBox1.DataSource = _modes;
            if (_mainWindow != null)
                chkLinear.Checked = _mainWindow.LinearInterpolation;

            interpolationViewer.SelectedKeyframeChanged += interpolationViewer1_SelectedKeyframeChanged;
            interpolationViewer.FrameChanged += interpolationViewer1_FrameChanged;
            interpolationViewer.SignalChange += interpolationViewer_SignalChange;

            numTanLen.Value = interpolationViewer.TangentLength;
        }

        void interpolationViewer_SignalChange(object sender, EventArgs e)
        {
            _targetNode.SignalPropertyChange();
        }

        void interpolationViewer1_UpdateProps(object sender, EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.UpdatePropDisplay();
                _mainWindow.UpdateModel();
            }
        }

        void interpolationViewer1_FrameChanged(object sender, EventArgs e)
        {
            if (_mainWindow != null && _mainWindow.CurrentFrame - 1 != interpolationViewer.FrameIndex)
                _mainWindow.SetFrame((interpolationViewer.FrameIndex + 1).Clamp(1, _mainWindow.MaxFrame));
        }

        public BindingList<string> _modes = new BindingList<string>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public KeyframeEntry SelectedKeyframe
        {
            get { return interpolationViewer.SelectedKeyframe; }
            set { interpolationViewer.SelectedKeyframe = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedMode { get { if (comboBox1.SelectedIndex == -1 || _modes.Count == 0) return -1; return comboBox1.SelectedIndex; } set { comboBox1.SelectedIndex = value.Clamp(comboBox1.Items.Count == 0 ? -1 : 0, comboBox1.Items.Count - 1); } }

        public void KeyframeChanged()
        {
            interpolationViewer.Invalidate();
        }

        public void RootChanged()
        {
            ResourceNode node = _targetNode;
            if (node is ISCN0KeyframeHolder)
            {
                if (node is SCN0LightNode)
                {
                    SCN0LightNode n = node as SCN0LightNode;

                    interpolationViewer.FrameLimit = n.FrameCount;
                    interpolationViewer.KeyRoot = n.GetKeys(SelectedMode)._keyRoot;
                }
                else if (node is SCN0FogNode)
                {
                    SCN0FogNode n = node as SCN0FogNode;

                    interpolationViewer.FrameLimit = n.FrameCount;
                    interpolationViewer.KeyRoot = n.GetKeys(SelectedMode)._keyRoot;
                }
                else if (node is SCN0CameraNode)
                {
                    SCN0CameraNode n = node as SCN0CameraNode;

                    interpolationViewer.FrameLimit = n.FrameCount;
                    interpolationViewer.KeyRoot = n.GetKeys(SelectedMode)._keyRoot;
                }
            }
            else if (node is IKeyframeHolder)
            {
                if (node is CHR0EntryNode)
                {
                    CHR0EntryNode n = node as CHR0EntryNode;

                    interpolationViewer.FrameLimit = n._keyframes._frameCount;
                    interpolationViewer.KeyRoot = n._keyframes._keyRoots[SelectedMode];
                }
                else if (node is SRT0TextureNode)
                {
                    SRT0TextureNode n = node as SRT0TextureNode;

                    interpolationViewer.FrameLimit = n._keyframes._frameCount;
                    int i = SelectedMode;
                    if (i == 2) i = 3;
                    else if (i > 2) i += 3;
                    interpolationViewer.KeyRoot = n._keyframes._keyRoots[i];
                }
            }
            else if (node is IKeyframeArrayHolder)
            {
                comboBox1.Enabled = false;

                if (node is SHP0VertexSetNode)
                {
                    SHP0VertexSetNode n = node as SHP0VertexSetNode;

                    interpolationViewer.FrameLimit = n._keyframes._frameLimit;
                    interpolationViewer.KeyRoot = n._keyframes._keyRoot;
                }
            }
        }

        public ResourceNode _targetNode;
        public void SetTarget(ResourceNode node)
        {
            if ((_targetNode = node) != null)
            {
                panel1.Enabled = true;
                if (node is ISCN0KeyframeHolder)
                {
                    if (node is SCN0LightNode)
                    {
                        //chkGenTans.Checked = SCN0LightNode._generateTangents;
                        chkLinear.Checked = SCN0LightNode._linear;

                        if (_modes.Count != 10)
                        {
                            _updating = true;
                            _modes.Clear();
                            _modes.Add("Start X");
                            _modes.Add("Start Y");
                            _modes.Add("Start Z");
                            _modes.Add("End X");
                            _modes.Add("End Y");
                            _modes.Add("End Z");
                            _modes.Add("Spotlight Cutoff");
                            _modes.Add("Spotlight Brightness");
                            _modes.Add("Reference Distance");
                            _modes.Add("Reference Brightness");

                            comboBox1.SelectedIndex = 0;
                            _updating = false;
                        }
                    }
                    else if (node is SCN0FogNode)
                    {
                        //chkGenTans.Checked = SCN0FogNode._generateTangents;
                        chkLinear.Checked = SCN0FogNode._linear;

                        if (_modes.Count != 2)
                        {
                            _updating = true;
                            _modes.Clear();
                            _modes.Add("Start Z");
                            _modes.Add("End Z");

                            comboBox1.SelectedIndex = 0;
                            _updating = false;
                        }
                    }
                    else if (node is SCN0CameraNode)
                    {
                        //chkGenTans.Checked = SCN0CameraNode._generateTangents;   
                        chkLinear.Checked = SCN0CameraNode._linear;

                        if (_modes.Count != 15)
                        {
                            _updating = true;
                            _modes.Clear();
                            _modes.Add("Position X");
                            _modes.Add("Position Y");
                            _modes.Add("Position Z");
                            _modes.Add("Aspect");
                            _modes.Add("Near Z");
                            _modes.Add("Far Z");
                            _modes.Add("Rotation X");
                            _modes.Add("Rotation Y");
                            _modes.Add("Rotation Z");
                            _modes.Add("Aim X");
                            _modes.Add("Aim Y");
                            _modes.Add("Aim Z");
                            _modes.Add("Twist");
                            _modes.Add("Vertical Field of View");
                            _modes.Add("Height");

                            comboBox1.SelectedIndex = 0;
                            _updating = false;
                        }
                    }
                }
                else if (node is IKeyframeHolder)
                {
                    if (node is CHR0EntryNode)
                    {
                        //chkGenTans.Checked = CHR0EntryNode._generateTangents;
                        chkLinear.Checked = CHR0EntryNode._linear;

                        if (_modes.Count != 9)
                        {
                            _updating = true;
                            _modes.Clear();
                            _modes.Add("Scale X");
                            _modes.Add("Scale Y");
                            _modes.Add("Scale Z");
                            _modes.Add("Rotation X");
                            _modes.Add("Rotation Y");
                            _modes.Add("Rotation Z");
                            _modes.Add("Translation X");
                            _modes.Add("Translation Y");
                            _modes.Add("Translation Z");

                            comboBox1.SelectedIndex = 0;
                            _updating = false;
                        }
                    }
                    else if (node is SRT0TextureNode)
                    {
                        //chkGenTans.Checked = SRT0TextureNode._generateTangents;
                        chkLinear.Checked = SRT0TextureNode._linear;

                        if (_modes.Count != 5)
                        {
                            _updating = true;
                            _modes.Clear();
                            _modes.Add("Scale X");
                            _modes.Add("Scale Y");
                            _modes.Add("Rotation");
                            _modes.Add("Translation X");
                            _modes.Add("Translation Y");

                            comboBox1.SelectedIndex = 0;
                            _updating = false;
                        }
                    }
                }
                else if (node is IKeyframeArrayHolder)
                {
                    //chkGenTans.Checked = SHP0VertexSetNode._generateTangents;
                    chkLinear.Checked = SHP0VertexSetNode._linear;

                    comboBox1.Enabled = false;
                    if (_modes.Count != 1)
                    {
                        _updating = true;

                        _modes.Clear();
                        _modes.Add("Percentage");

                        comboBox1.SelectedIndex = 0;
                        _updating = false;
                    }
                }
                RootChanged();
            }
            else
            {
                interpolationViewer.KeyRoot = null;
                panel1.Enabled = false;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Frame
        {
            get { return interpolationViewer.FrameIndex + 1; }
            set
            {
                if (interpolationViewer.FrameIndex == value - 1)
                    return;

                interpolationViewer._updating = true;
                interpolationViewer.FrameIndex = value - 1;
                interpolationViewer._updating = false;
            }
        }

        bool _updating = false;
        void interpolationViewer1_SelectedKeyframeChanged(object sender, EventArgs e)
        {
            _updating = true;
            if (interpolationViewer._selKey != null)
            {
                bool indexChanged = numericInputBox3.Value != interpolationViewer._selKey._index + 1;

                groupBox1.Enabled = true;
                numericInputBox1.Value = interpolationViewer._selKey._tangent;
                numericInputBox2.Value = interpolationViewer._selKey._value;
                numericInputBox3.Value = interpolationViewer._selKey._index + 1;

                if (chkSetFrame.Checked)
                    interpolationViewer1_FrameChanged(this, null);
                
                if (_mainWindow != null)
                    if (indexChanged)
                        _mainWindow.KeyframePanel.UpdateKeyframes();
                    else
                        _mainWindow.KeyframePanel.UpdateKeyframe(interpolationViewer._selKey._index);

                //_mainWindow.UpdatePropDisplay();
                //_mainWindow.UpdateModel();
            }
            else
            {
                groupBox1.Enabled = false;
                numericInputBox1.Value = 0;
                numericInputBox2.Value = 0;
                numericInputBox3.Value = 0;
            }
            _updating = false;
        }

        private void numericInputBox1_ValueChanged(object sender, EventArgs e)
        {
            if (_updating || interpolationViewer._selKey == null)
                return;

            interpolationViewer._selKey._tangent = numericInputBox1.Value;
            interpolationViewer.Invalidate();
            _targetNode.SignalPropertyChange();

            if (chkSyncStartEnd.Checked)
            {
                if (SelectedKeyframe._prev._index == -1 && SelectedKeyframe._prev._prev != SelectedKeyframe)
                {
                    SelectedKeyframe._prev._prev._tangent = SelectedKeyframe._tangent;
                    SelectedKeyframe._prev._prev._value = SelectedKeyframe._value;
                }

                if (SelectedKeyframe._next._index == -1 && SelectedKeyframe._next._next != SelectedKeyframe)
                {
                    SelectedKeyframe._next._next._tangent = SelectedKeyframe._tangent;
                    SelectedKeyframe._next._next._value = SelectedKeyframe._value;
                }
            }
        }

        private void numericInputBox3_ValueChanged(object sender, EventArgs e)
        {
            if (_updating || interpolationViewer._selKey == null)
                return;

            KeyframeEntry w = interpolationViewer._selKey;
            int prev = w._prev._index + 1;
            if (prev < 0) prev = 0;
            int next = w._next._index - 1;
            if (_mainWindow != null)
                if (next < 0) next = _mainWindow.MaxFrame - 1;

            int index = ((int)numericInputBox3.Value - 1).Clamp(prev, next);

            interpolationViewer._selKey._index = index;
            interpolationViewer.Invalidate();
            _targetNode.SignalPropertyChange();
            if (_mainWindow != null)
            {
                _mainWindow.KeyframePanel.UpdateKeyframes();
                _mainWindow.UpdatePropDisplay();
                _mainWindow.UpdateModel();
            }

            if (index != numericInputBox3.Value)
                numericInputBox3.Value = index;
        }

        private void numericInputBox2_ValueChanged(object sender, EventArgs e)
        {
            if (_updating || interpolationViewer._selKey == null)
                return;

            interpolationViewer._selKey._value = numericInputBox2.Value;
            interpolationViewer.Invalidate();
            _targetNode.SignalPropertyChange();
            if (_mainWindow != null)
                _mainWindow.KeyframePanel.UpdateKeyframe(interpolationViewer._selKey._index);

            if (chkSyncStartEnd.Checked)
            {
                if (SelectedKeyframe._prev._index == -1 && SelectedKeyframe._prev._prev != SelectedKeyframe)
                {
                    SelectedKeyframe._prev._prev._tangent = SelectedKeyframe._tangent;
                    SelectedKeyframe._prev._prev._value = SelectedKeyframe._value;
                }

                if (SelectedKeyframe._next._index == -1 && SelectedKeyframe._next._next != SelectedKeyframe)
                {
                    SelectedKeyframe._next._next._tangent = SelectedKeyframe._tangent;
                    SelectedKeyframe._next._next._value = SelectedKeyframe._value;
                }
            }
            if (_mainWindow != null)
            {
                _mainWindow.UpdateModel();
                _mainWindow.UpdatePropDisplay();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            RootChanged();

            if (SelectedKeyframe != null)
            {
                KeyframeEntry prev = SelectedKeyframe;
                for (KeyframeEntry entry = interpolationViewer.KeyRoot._next; (entry != interpolationViewer.KeyRoot); entry = entry._next)
                    if (entry._index == SelectedKeyframe._index)
                    {
                        SelectedKeyframe = entry;
                        break;
                    }
                if (SelectedKeyframe == prev)
                    SelectedKeyframe = null;
            }
        }

        private void chkAllKeys_CheckedChanged(object sender, EventArgs e)
        {
            interpolationViewer.AllKeyframes = !chkViewAll.Checked;
        }
        private void chkLinear_CheckedChanged(object sender, EventArgs e)
        {
            chkRenderTans.Checked = !(interpolationViewer.Linear = chkLinear.Checked);

            if (_targetNode is ISCN0KeyframeHolder)
            {
                if (_targetNode is SCN0LightNode)
                    SCN0LightNode._linear = chkLinear.Checked;
                else if (_targetNode is SCN0FogNode)
                    SCN0FogNode._linear = chkLinear.Checked;
                else if (_targetNode is SCN0CameraNode)
                    SCN0CameraNode._linear = chkLinear.Checked;
            }
            else if (_targetNode is IKeyframeHolder)
            {
                if (_targetNode is CHR0EntryNode)
                    CHR0EntryNode._linear = chkLinear.Checked;
                else if (_targetNode is SRT0TextureNode)
                    SRT0TextureNode._linear = chkLinear.Checked;
            }
            else if (_targetNode is IKeyframeArrayHolder)
                 SHP0VertexSetNode._linear = chkLinear.Checked;
        }
        private void chkGenTans_CheckedChanged(object sender, EventArgs e)
        {
            interpolationViewer.GenerateTangents = chkGenTans.Checked;

            //if (_targetNode is CHR0EntryNode)
            //    CHR0EntryNode._generateTangents = chkGenTans.Checked;
            //else if (_targetNode is SRT0TextureNode)
            //    SRT0TextureNode._generateTangents = chkGenTans.Checked;
            //else if (_targetNode is SHP0VertexSetNode)
            //    SHP0VertexSetNode._generateTangents = chkGenTans.Checked;
            //else if (_targetNode is SCN0LightNode)
            //    SCN0LightNode._generateTangents = chkGenTans.Checked;
            //else if (_targetNode is SCN0CameraNode)
            //    SCN0CameraNode._generateTangents = chkGenTans.Checked;
            //else if (_targetNode is SCN0FogNode)
            //    SCN0FogNode._generateTangents = chkGenTans.Checked;
        }
        private void chkKeyDrag_CheckedChanged(object sender, EventArgs e)
        {
            interpolationViewer.KeyDraggingAllowed = chkKeyDrag.Checked;
        }
        private void chkRenderTans_CheckedChanged(object sender, EventArgs e)
        {
            interpolationViewer.DrawTangents = chkRenderTans.Checked;
        }
        private void chkSyncStartEnd_CheckedChanged(object sender, EventArgs e)
        {
            interpolationViewer.SyncStartEnd = chkSyncStartEnd.Checked;
        }
        private void numTanLen_ValueChanged(object sender, EventArgs e)
        {
            interpolationViewer.TangentLength = numTanLen.Value;
        }
    }
}
