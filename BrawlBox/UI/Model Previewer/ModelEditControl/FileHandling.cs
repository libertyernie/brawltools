using System;
using BrawlLib.OpenGL;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.Modeling;
using System.Drawing;
using BrawlLib.Wii.Animations;
using System.Collections.Generic;
using BrawlLib.SSBBTypes;
using BrawlLib.IO;
using BrawlLib;
using System.Drawing.Imaging;
using Gif.Components;
using OpenTK.Graphics.OpenGL;
using BrawlLib.Imaging;

namespace System.Windows.Forms
{
    public partial class ModelEditControl : ModelEditorBase
    {
        private bool LoadExternal()
        {
            dlgOpen.Filter = "All Compatible Files (*.pac, *.pcs, *.brres, *.chr0, *.srt0, *.pat0, *.vis0, *.shp0, *.scn0, *.clr0, *.mrg)|*.pac;*.pcs;*.brres;*.chr0;*.srt0;*.pat0;*.vis0;*.shp0;*.scn0;*.clr0;*.mrg";
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {
                ResourceNode node = null;
                leftPanel.listAnims.BeginUpdate();
                try
                {
                    if ((node = NodeFactory.FromFile(null, dlgOpen.FileName)) != null)
                    {
                        if (!CloseExternal())
                            return false;

                        if (!leftPanel.LoadAnims(node, TargetAnimType))
                            MessageBox.Show(this, "No animations could be found in external file.", "Error");
                        else
                        {
                            _animationSearchNodes.Add(_externalAnimationsNode = node);
                            ModelPanel.AddReference(_externalAnimationsNode);
                            node = null;
                            return true;
                        }
                    }
                    else
                        MessageBox.Show(this, "Unable to recognize input file.");
                }
                #if !DEBUG
                catch (Exception x) { MessageBox.Show(this, x.ToString()); }
                #endif
                finally
                {
                    if (node != null)
                        node.Dispose();
                    leftPanel.listAnims.EndUpdate();
                }
            }
            return false;
        }
        private bool CloseExternal()
        {
            if (_externalAnimationsNode != null)
            {
                if (_externalAnimationsNode.IsDirty)
                {
                    DialogResult res = MessageBox.Show(this, "You have made changes to an external file. Would you like to save those changes?", "Closing external file.", MessageBoxButtons.YesNoCancel);
                    if (((res == DialogResult.Yes) && (!SaveExternal(false))) || (res == DialogResult.Cancel))
                        return false;
                }

                _animationSearchNodes.Remove(_externalAnimationsNode);

                ModelPanel.RemoveReference(_externalAnimationsNode);
                leftPanel._closing = true;
                leftPanel.listAnims.Items.Clear();
                leftPanel._closing = false;
                _externalAnimationsNode.Dispose();
                _externalAnimationsNode = null;

                if (SelectedBone != null)
                    SelectedBone.BoneColor = SelectedBone.NodeColor = Color.Transparent;

                leftPanel.UpdateAnimations(TargetAnimType);
                SetAnimation(TargetAnimType, null);
                GetFiles(NW4RAnimType.None);
                UpdatePropDisplay();
                UpdateModel();
            }
            return true;
        }
        private bool SaveExternal(bool As)
        {
            if ((_externalAnimationsNode == null) || ((!_externalAnimationsNode.IsDirty) && !As))
                return true;

            try
            {
                if (As)
                    using (SaveFileDialog d = new SaveFileDialog())
                    {
                        d.InitialDirectory = _externalAnimationsNode._origPath.Substring(0, _externalAnimationsNode._origPath.LastIndexOf('\\'));
                        d.Filter = String.Format("(*{0})|*{0}", Path.GetExtension(_externalAnimationsNode._origPath));
                        d.Title = "Please choose a location to save this file.";
                        if (d.ShowDialog(this) == DialogResult.OK)
                        {
                            _externalAnimationsNode.Merge();
                            _externalAnimationsNode.Export(d.FileName);
                        }
                    }
                else
                {
                    _externalAnimationsNode.Merge();
                    _externalAnimationsNode.Export(_externalAnimationsNode._origPath);
                }
                return true;
            }
            catch (Exception x) { MessageBox.Show(this, x.ToString()); }
            return false;
        }

        public void btnOpenClose_Click(object sender, EventArgs e)
        {
            if (btnOpenClose.Text == "Load")
            {
                if (LoadExternal())
                    btnOpenClose.Text = leftPanel.Load.Text = "Close";
            }
            else if (btnOpenClose.Text == "Close" && CloseExternal())
                btnOpenClose.Text = leftPanel.Load.Text = "Load";
        }
        public void btnSave_Click(object sender, EventArgs e) { SaveExternal(false); }
        private void btnSaveAs_Click(object sender, EventArgs e) { SaveExternal(true); }
        
        private void OpenFile(string file)
        {
            ResourceNode node = null;
            try
            {
                if ((node = NodeFactory.FromFile(null, file)) != null)
                {
                    if (_targetModels == null)
                        _targetModels = new List<IModel>();

                    List<IModel> modelList = ModelPanel.CollectModels(node);
                    foreach (IModel m in modelList)
                        AppendTarget(m);

                    if (TargetModel == null)
                        TargetModel = _targetModels[0];

                    models.SelectedItem = TargetModel;
                }
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Error loading model(s) from file."); }
        }

        public void AppendTarget(CollisionNode collision)
        {
            if (!_collisions.Contains(collision))
                _collisions.Add(collision);

            if (!models.Items.Contains(collision))
                models.Items.Add(collision);

            foreach (CollisionObject o in collision._objects)
                o._render = true;
        }
    }
}
