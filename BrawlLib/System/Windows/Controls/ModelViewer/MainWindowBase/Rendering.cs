using BrawlLib.Modeling;
using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    public partial class ModelEditorBase : UserControl
    {
        public unsafe virtual void modelPanel1_PreRender(object sender)
        {
            //This function may be called from a model panel that is not necessarily the currently focused one
            ModelPanel panel = sender as ModelPanel;

            if (panel != null && panel.RenderFloor)
                OnRenderFloor();
        }

        public unsafe virtual void modelPanel1_PostRender(object sender)
        {
            //This function may be called from a model panel that is not necessarily the currently focused one
            ModelPanel panel = sender as ModelPanel;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.DepthTest);

            if (panel.RenderVertices)
                OnRenderVertices();
            if (panel.RenderNormals)
                OnRenderNormals();
            if (RenderLightDisplay)
                OnRenderLightDisplay(panel.LightPosition);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            RenderTransformControl(panel);
            RenderDepth(panel);
        }

        public virtual void OnRenderVertices()
        {
            if (EditingAll && _targetModels != null)
                foreach (IModel m in _targetModels)
                    m.RenderVertices(false, SelectedBone);
            else if (TargetModel != null)
                TargetModel.RenderVertices(false, SelectedBone);
        }

        public virtual void OnRenderNormals()
        {
            if (EditingAll && _targetModels != null)
                foreach (IModel m in _targetModels)
                    m.RenderNormals();
            else if (TargetModel != null)
                TargetModel.RenderNormals();
        }

        #region Bone Control Rendering
        public unsafe void RenderTransformControl(ModelPanel panel)
        {
            if (_playing)
                return;

            if (SelectedBone != null) //Render drag and drop control
            {
                if (ControlType == TransformType.Rotation)
                    RenderRotationControl(panel, SelectedBone);
                else if (ControlType == TransformType.Translation)
                    RenderTranslationControl(BoneLoc(SelectedBone), OrbRadius(SelectedBone, panel));
                else if (ControlType == TransformType.Scale)
                    RenderScaleControl(panel);
            }

            if (VertexLoc() != null && RenderVertices)
            {
                //if (_editType == TransformType.Rotation)
                //    RenderRotationControl(context, ((Vector3)VertexLoc), VertexOrbRadius, _oldAngles);
                //else
                RenderTranslationControl(((Vector3)VertexLoc()), VertexOrbRadius(panel));
            }
        }
        public unsafe void RenderTranslationControl(Vector3 position, float radius)
        {
            GLDisplayList axis = GetAxes();

            //Enter local space
            Matrix m = Matrix.TransformMatrix(new Vector3(radius), new Vector3(), position);
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            axis.Call();

            GL.PopMatrix();

            ModelPanel.ScreenText["X"] = ModelPanel.Project(new Vector3(_axisLDist + 0.1f, 0, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Y"] = ModelPanel.Project(new Vector3(0, _axisLDist + 0.1f, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Z"] = ModelPanel.Project(new Vector3(0, 0, _axisLDist + 0.1f) * m) - new Vector3(8.0f, 8.0f, 0);
        }
        public unsafe void RenderScaleControl(ModelPanel panel)
        {
            GLDisplayList axis = GetScaleControl();

            //Enter local space
            Matrix m = Matrix.TransformMatrix(new Vector3(OrbRadius(SelectedBone, panel)), new Vector3(), BoneLoc(SelectedBone));
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            axis.Call();

            GL.PopMatrix();

            ModelPanel.ScreenText["X"] = ModelPanel.Project(new Vector3(_axisLDist + 0.1f, 0, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Y"] = ModelPanel.Project(new Vector3(0, _axisLDist + 0.1f, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Z"] = ModelPanel.Project(new Vector3(0, 0, _axisLDist + 0.1f) * m) - new Vector3(8.0f, 8.0f, 0);
        }
        public unsafe void RenderRotationControl(ModelPanel panel, IBoneNode bone)
        {
            Vector3 position = BoneLoc(bone);
            float radius = OrbRadius(bone, panel);
            Vector3 rotate = bone.Matrix.GetAngles();

            Matrix m = Matrix.TransformMatrix(new Vector3(radius), position.LookatAngles(CamLoc(panel)) * Maths._rad2degf, position);

            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            GLDisplayList sphere = TKContext.GetCircleList();
            GLDisplayList circle = TKContext.GetRingList();

            //Orb
            GL.Color4(0.7f, 0.7f, 0.7f, 0.15f);
            sphere.Call();

            GL.Disable(EnableCap.DepthTest);

            //Container
            GL.Color4(0.4f, 0.4f, 0.4f, 1.0f);
            circle.Call();

            //Circ
            if (_snapCirc || _hiCirc)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(1.0f, 0.8f, 0.5f, 1.0f);
            GL.Scale(_circOrbScale, _circOrbScale, _circOrbScale);
            circle.Call();

            //Pop
            GL.PopMatrix();

            GL.Enable(EnableCap.DepthTest);

            //Enter local space
            m = Matrix.TransformMatrix(new Vector3(radius), rotate, position);

            ModelPanel.ScreenText["X"] = ModelPanel.Project(new Vector3(1.1f, 0, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Y"] = ModelPanel.Project(new Vector3(0, 1.1f, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Z"] = ModelPanel.Project(new Vector3(0, 0, 1.1f) * m) - new Vector3(8.0f, 8.0f, 0);

            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            //Z
            if (_snapZ || _hiZ)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(0.0f, 0.0f, 1.0f, 1.0f);

            circle.Call();
            GL.Rotate(90.0f, 0.0f, 1.0f, 0.0f);

            //X
            if (_snapX || _hiX)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Red);

            circle.Call();
            GL.Rotate(90.0f, 1.0f, 0.0f, 0.0f);

            //Y
            if (_snapY || _hiY)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Green);

            circle.Call();

            //Pop
            GL.PopMatrix();
        }

#endregion

        #region Scale/Translation Display Lists
        public const float _axisLDist = 2.0f;
        public const float _axisHalfLDist = 0.75f;
        public const float _apthm = 0.075f;
        public const float _dst = 1.5f;
        public GLDisplayList GetAxes()
        {
            //Create the axes.
            GLDisplayList axis = new GLDisplayList();
            axis.Begin();

            //Disable culling so square bases for the arrows aren't necessary to draw
            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.Begin(PrimitiveType.Lines);

            //X

            if ((_snapX && _snapY) || (_hiX && _hiY))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Red);
            GL.Vertex3(_axisHalfLDist, 0.0f, 0.0f);
            GL.Vertex3(_axisHalfLDist, _axisHalfLDist, 0.0f);

            if ((_snapX && _snapZ) || (_hiX && _hiZ))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Red);
            GL.Vertex3(_axisHalfLDist, 0.0f, 0.0f);
            GL.Vertex3(_axisHalfLDist, 0.0f, _axisHalfLDist);

            if (_snapX || _hiX)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Red);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(_dst, 0.0f, 0.0f);

            GL.End();

            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, _apthm, -_apthm);
            GL.Vertex3(_dst, _apthm, _apthm);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, -_apthm, _apthm);
            GL.Vertex3(_dst, -_apthm, -_apthm);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, _apthm, _apthm);
            GL.Vertex3(_dst, -_apthm, _apthm);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, -_apthm, -_apthm);
            GL.Vertex3(_dst, _apthm, -_apthm);

            GL.End();

            GL.Begin(PrimitiveType.Lines);

            //Y

            if ((_snapY && _snapX) || (_hiY && _hiX))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Green);
            GL.Vertex3(0.0f, _axisHalfLDist, 0.0f);
            GL.Vertex3(_axisHalfLDist, _axisHalfLDist, 0.0f);

            if ((_snapY && _snapZ) || (_hiY && _hiZ))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Green);
            GL.Vertex3(0.0f, _axisHalfLDist, 0.0f);
            GL.Vertex3(0.0f, _axisHalfLDist, _axisHalfLDist);

            if (_snapY || _hiY)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Green);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, _dst, 0.0f);

            GL.End();

            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(_apthm, _dst, -_apthm);
            GL.Vertex3(_apthm, _dst, _apthm);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(-_apthm, _dst, _apthm);
            GL.Vertex3(-_apthm, _dst, -_apthm);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(_apthm, _dst, _apthm);
            GL.Vertex3(-_apthm, _dst, _apthm);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(-_apthm, _dst, -_apthm);
            GL.Vertex3(_apthm, _dst, -_apthm);

            GL.End();

            GL.Begin(PrimitiveType.Lines);

            //Z

            if ((_snapZ && _snapX) || (_hiZ && _hiX))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Blue);
            GL.Vertex3(0.0f, 0.0f, _axisHalfLDist);
            GL.Vertex3(_axisHalfLDist, 0.0f, _axisHalfLDist);

            if ((_snapZ && _snapY) || (_hiZ && _hiY))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Blue);
            GL.Vertex3(0.0f, 0.0f, _axisHalfLDist);
            GL.Vertex3(0.0f, _axisHalfLDist, _axisHalfLDist);

            if (_snapZ || _hiZ)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Blue);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, _dst);

            GL.End();

            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(_apthm, -_apthm, _dst);
            GL.Vertex3(_apthm, _apthm, _dst);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(-_apthm, _apthm, _dst);
            GL.Vertex3(-_apthm, -_apthm, _dst);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(_apthm, _apthm, _dst);
            GL.Vertex3(-_apthm, _apthm, _dst);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(-_apthm, -_apthm, _dst);
            GL.Vertex3(_apthm, -_apthm, _dst);

            GL.End();

            axis.End();

            return axis;
        }
        public const float _scaleHalf1LDist = 0.8f;
        public const float _scaleHalf2LDist = 1.2f;
        public GLDisplayList GetScaleControl()
        {
            //Create the axes.
            GLDisplayList axis = new GLDisplayList();
            axis.Begin();

            //Disable culling so square bases for the arrows aren't necessary to draw
            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.Begin(PrimitiveType.Lines);

            //X
            if ((_snapY && _snapZ) || (_hiY && _hiZ))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Red);
            GL.Vertex3(0.0f, _scaleHalf1LDist, 0.0f);
            GL.Vertex3(0.0f, 0.0f, _scaleHalf1LDist);
            GL.Vertex3(0.0f, _scaleHalf2LDist, 0.0f);
            GL.Vertex3(0.0f, 0.0f, _scaleHalf2LDist);

            if (_snapX || _hiX)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Red);

            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(_dst, 0.0f, 0.0f);

            GL.End();

            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, _apthm, -_apthm);
            GL.Vertex3(_dst, _apthm, _apthm);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, -_apthm, _apthm);
            GL.Vertex3(_dst, -_apthm, -_apthm);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, _apthm, _apthm);
            GL.Vertex3(_dst, -_apthm, _apthm);

            GL.Vertex3(_axisLDist, 0.0f, 0.0f);
            GL.Vertex3(_dst, -_apthm, -_apthm);
            GL.Vertex3(_dst, _apthm, -_apthm);

            GL.End();

            GL.Begin(PrimitiveType.Lines);

            //Y
            if ((_snapZ && _snapX) || (_hiZ && _hiX))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Green);
            GL.Vertex3(0.0f, 0.0f, _scaleHalf1LDist);
            GL.Vertex3(_scaleHalf1LDist, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, _scaleHalf2LDist);
            GL.Vertex3(_scaleHalf2LDist, 0.0f, 0.0f);

            if (_snapY || _hiY)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Green);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, _dst, 0.0f);

            GL.End();

            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(_apthm, _dst, -_apthm);
            GL.Vertex3(_apthm, _dst, _apthm);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(-_apthm, _dst, _apthm);
            GL.Vertex3(-_apthm, _dst, -_apthm);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(_apthm, _dst, _apthm);
            GL.Vertex3(-_apthm, _dst, _apthm);

            GL.Vertex3(0.0f, _axisLDist, 0.0f);
            GL.Vertex3(-_apthm, _dst, -_apthm);
            GL.Vertex3(_apthm, _dst, -_apthm);

            GL.End();

            GL.Begin(PrimitiveType.Lines);

            //Z
            if ((_snapX && _snapY) || (_hiX && _hiY))
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Blue);
            GL.Vertex3(0.0f, _scaleHalf1LDist, 0.0f);
            GL.Vertex3(_scaleHalf1LDist, 0.0f, 0.0f);
            GL.Vertex3(0.0f, _scaleHalf2LDist, 0.0f);
            GL.Vertex3(_scaleHalf2LDist, 0.0f, 0.0f);

            if (_snapZ || _hiZ)
                GL.Color4(Color.Yellow);
            else
                GL.Color4(Color.Blue);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, _dst);

            GL.End();

            GL.Begin(PrimitiveType.Triangles);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(_apthm, -_apthm, _dst);
            GL.Vertex3(_apthm, _apthm, _dst);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(-_apthm, _apthm, _dst);
            GL.Vertex3(-_apthm, -_apthm, _dst);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(_apthm, _apthm, _dst);
            GL.Vertex3(-_apthm, _apthm, _dst);

            GL.Vertex3(0.0f, 0.0f, _axisLDist);
            GL.Vertex3(-_apthm, -_apthm, _dst);
            GL.Vertex3(_apthm, -_apthm, _dst);

            GL.End();

            axis.End();

            return axis;
        }

        #endregion

        #region Depth Rendering

        public unsafe virtual void RenderDepth(ModelPanel panel)
        {
            if (!panel._grabbing && !panel._scrolling && !_playing)
            {
                GL.Color4(Color.Black);
                GL.ColorMask(true, false, false, false);

                if (panel.RenderVertices)
                    if (EditingAll && _targetModels != null)
                        foreach (IModel m in _targetModels)
                            m.RenderVertices(true, SelectedBone);
                    else if (TargetModel != null)
                        TargetModel.RenderVertices(true, SelectedBone);

                if (panel.RenderBones)
                {
                    //Render invisible depth orbs
                    GLDisplayList list = TKContext.GetSphereList();
                    if (EditingAll)
                    {
                        foreach (IModel m in _targetModels)
                            foreach (IBoneNode bone in m.BoneCache)
                                if (bone != SelectedBone)
                                    RenderOrb(bone, list);
                    }
                    else if (TargetModel != null)
                        foreach (IBoneNode bone in _targetModel.BoneCache)
                            if (bone != SelectedBone)
                                RenderOrb(bone, list);
                }

                //Render invisible depth planes for translation and scale controls
                if (ControlType != TransformType.Rotation && (SelectedBone != null || VertexLoc() != null))
                {
                    #region Axis Selection Display List

                    GLDisplayList selList = new GLDisplayList();

                    selList.Begin();

                    GL.Begin(PrimitiveType.Quads);

                    //X Axis
                    //XY quad
                    GL.Vertex3(0.0f, -_axisSelectRange, 0.0f);
                    GL.Vertex3(0.0f, _axisSelectRange, 0.0f);
                    GL.Vertex3(_axisLDist, _axisSelectRange, 0.0f);
                    GL.Vertex3(_axisLDist, -_axisSelectRange, 0.0f);
                    //XZ quad
                    GL.Vertex3(0.0f, 0.0f, -_axisSelectRange);
                    GL.Vertex3(0.0f, 0.0f, _axisSelectRange);
                    GL.Vertex3(_axisLDist, 0.0f, _axisSelectRange);
                    GL.Vertex3(_axisLDist, 0.0f, -_axisSelectRange);

                    //Y Axis
                    //YX quad
                    GL.Vertex3(-_axisSelectRange, 0.0f, 0.0f);
                    GL.Vertex3(_axisSelectRange, 0.0f, 0.0f);
                    GL.Vertex3(_axisSelectRange, _axisLDist, 0.0f);
                    GL.Vertex3(-_axisSelectRange, _axisLDist, 0.0f);
                    //YZ quad
                    GL.Vertex3(0.0f, 0.0f, -_axisSelectRange);
                    GL.Vertex3(0.0f, 0.0f, _axisSelectRange);
                    GL.Vertex3(0.0f, _axisLDist, _axisSelectRange);
                    GL.Vertex3(0.0f, _axisLDist, -_axisSelectRange);

                    //Z Axis
                    //ZX quad
                    GL.Vertex3(-_axisSelectRange, 0.0f, 0.0f);
                    GL.Vertex3(_axisSelectRange, 0.0f, 0.0f);
                    GL.Vertex3(_axisSelectRange, 0.0f, _axisLDist);
                    GL.Vertex3(-_axisSelectRange, 0.0f, _axisLDist);
                    //ZY quad
                    GL.Vertex3(0.0f, -_axisSelectRange, 0.0f);
                    GL.Vertex3(0.0f, _axisSelectRange, 0.0f);
                    GL.Vertex3(0.0f, _axisSelectRange, _axisLDist);
                    GL.Vertex3(0.0f, -_axisSelectRange, _axisLDist);

                    GL.End();

                    selList.End();

                    #endregion

                    if (ControlType != TransformType.Rotation && SelectedBone != null)
                    {
                        Matrix m = Matrix.TransformMatrix(new Vector3(OrbRadius(SelectedBone, panel)), new Vector3(), BoneLoc(SelectedBone));
                        GL.PushMatrix();
                        GL.MultMatrix((float*)&m);

                        selList.Call();

                        if (ControlType == TransformType.Translation)
                        {
                            GL.Begin(PrimitiveType.Quads);

                            //XY
                            GL.Vertex3(0.0f, _axisSelectRange, 0.0f);
                            GL.Vertex3(_axisHalfLDist, _axisSelectRange, 0.0f);
                            GL.Vertex3(_axisHalfLDist, _axisHalfLDist, 0.0f);
                            GL.Vertex3(0.0f, _axisHalfLDist, 0.0f);

                            //YZ
                            GL.Vertex3(0.0f, 0.0f, _axisSelectRange);
                            GL.Vertex3(0.0f, _axisHalfLDist, _axisSelectRange);
                            GL.Vertex3(0.0f, _axisHalfLDist, _axisHalfLDist);
                            GL.Vertex3(0.0f, 0.0f, _axisHalfLDist);

                            //XZ
                            GL.Vertex3(_axisSelectRange, 0.0f, 0.0f);
                            GL.Vertex3(_axisSelectRange, 0.0f, _axisHalfLDist);
                            GL.Vertex3(_axisHalfLDist, 0.0f, _axisHalfLDist);
                            GL.Vertex3(_axisHalfLDist, 0.0f, 0.0f);

                            GL.End();
                        }
                        else
                        {
                            GL.Begin(PrimitiveType.Triangles);

                            //XY
                            GL.Vertex3(0.0f, _axisSelectRange, 0.0f);
                            GL.Vertex3(_scaleHalf2LDist, _axisSelectRange, 0.0f);
                            GL.Vertex3(0.0f, _scaleHalf2LDist, 0.0f);

                            //YZ
                            GL.Vertex3(0.0f, 0.0f, _axisSelectRange);
                            GL.Vertex3(0.0f, _scaleHalf2LDist, _axisSelectRange);
                            GL.Vertex3(0.0f, 0.0f, _scaleHalf2LDist);

                            //XZ
                            GL.Vertex3(_axisSelectRange, 0.0f, 0.0f);
                            GL.Vertex3(_axisSelectRange, 0.0f, _scaleHalf2LDist);
                            GL.Vertex3(_scaleHalf2LDist, 0.0f, 0.0f);

                            GL.End();
                        }

                        GL.PopMatrix();
                    }

                    if (VertexLoc() != null && panel.RenderVertices)
                    {
                        Matrix m = Matrix.TransformMatrix(new Vector3(VertexOrbRadius(panel)), new Vector3(), ((Vector3)VertexLoc()));
                        GL.PushMatrix();
                        GL.MultMatrix((float*)&m);

                        selList.Call();

                        GL.Begin(PrimitiveType.Quads);

                        //XY
                        GL.Vertex3(0.0f, _axisSelectRange, 0.0f);
                        GL.Vertex3(_axisHalfLDist, _axisSelectRange, 0.0f);
                        GL.Vertex3(_axisHalfLDist, _axisHalfLDist, 0.0f);
                        GL.Vertex3(0.0f, _axisHalfLDist, 0.0f);

                        //YZ
                        GL.Vertex3(0.0f, 0.0f, _axisSelectRange);
                        GL.Vertex3(0.0f, _axisHalfLDist, _axisSelectRange);
                        GL.Vertex3(0.0f, _axisHalfLDist, _axisHalfLDist);
                        GL.Vertex3(0.0f, 0.0f, _axisHalfLDist);

                        //XZ
                        GL.Vertex3(_axisSelectRange, 0.0f, 0.0f);
                        GL.Vertex3(_axisSelectRange, 0.0f, _axisHalfLDist);
                        GL.Vertex3(_axisHalfLDist, 0.0f, _axisHalfLDist);
                        GL.Vertex3(_axisHalfLDist, 0.0f, 0.0f);

                        GL.End();

                        GL.PopMatrix();
                    }
                }

                GL.ColorMask(true, true, true, true);
            }
        }

        #endregion

        #region Orb Point Distance

        /// <summary>
        /// Gets world-point of specified mouse point projected onto the selected bone's local space if rotating or in world space if translating or scaling.
        /// Intersects the projected ray with the appropriate plane using the snap flags.
        /// </summary>
        public bool GetOrbPoint(Vector2 mousePoint, out Vector3 point, ModelPanel panel)
        {
            IBoneNode bone = SelectedBone;
            if (bone == null)
            {
                point = new Vector3();
                return false;
            }

            Vector3 lineStart = panel.UnProject(mousePoint._x, mousePoint._y, 0.0f);
            Vector3 lineEnd = panel.UnProject(mousePoint._x, mousePoint._y, 1.0f);
            Vector3 center = bone.Matrix.GetPoint();
            Vector3 camera = panel.Camera.GetPoint();
            Vector3 normal = new Vector3();
            float radius = center.TrueDistance(camera) / _orbRadius * 0.1f;

            switch (ControlType)
            {
                case TransformType.Rotation:

                    if (_snapX)
                        normal = (bone.Matrix * new Vector3(1.0f, 0.0f, 0.0f)).Normalize(center);
                    else if (_snapY)
                        normal = (bone.Matrix * new Vector3(0.0f, 1.0f, 0.0f)).Normalize(center);
                    else if (_snapZ)
                        normal = (bone.Matrix * new Vector3(0.0f, 0.0f, 1.0f)).Normalize(center);
                    else if (_snapCirc)
                    {
                        radius *= _circOrbScale;
                        normal = camera.Normalize(center);
                    }
                    else if (Maths.LineSphereIntersect(lineStart, lineEnd, center, radius, out point))
                        return true;
                    else
                        normal = camera.Normalize(center);

                    if (Maths.LinePlaneIntersect(lineStart, lineEnd, center, normal, out point))
                    {
                        point = Maths.PointAtLineDistance(center, point, radius);
                        return true;
                    }

                    break;

                case TransformType.Translation:
                case TransformType.Scale:

                    if (_snapX && _snapY)
                        normal = new Vector3(0.0f, 0.0f, 1.0f);
                    else if (_snapX && _snapZ)
                        normal = new Vector3(0.0f, 1.0f, 0.0f);
                    else if (_snapY && _snapZ)
                        normal = new Vector3(1.0f, 0.0f, 0.0f);
                    else if (_snapX)
                        normal = new Vector3(0.0f, 1.0f, 0.0f);
                    else if (_snapY)
                        normal = new Vector3(1.0f, 0.0f, 0.0f);
                    else if (_snapZ)
                        normal = new Vector3(0.0f, 1.0f, 0.0f);
                    else if (ControlType == TransformType.Scale && _snapX && _snapY && _snapZ)
                        normal = camera.Normalize(center);

                    break;
            }

            return Maths.LinePlaneIntersect(lineStart, lineEnd, center, normal, out point);
        }

        public bool GetVertexOrbPoint(Vector2 mousePoint, Vector3 center, out Vector3 point, ModelPanel panel)
        {
            Vector3 lineStart = panel.UnProject(mousePoint._x, mousePoint._y, 0.0f);
            Vector3 lineEnd = panel.UnProject(mousePoint._x, mousePoint._y, 1.0f);
            Vector3 camera = panel.Camera.GetPoint();
            Vector3 normal = new Vector3();
            float radius = center.TrueDistance(camera) / _orbRadius * 0.1f;

            switch (ControlType)
            {
                case TransformType.Scale:

                    break;

                case TransformType.Rotation:
                //if (_snapX)
                //    normal = (new Vector3(1.0f, 0.0f, 0.0f)).Normalize(center);
                //else if (_snapY)
                //    normal = (new Vector3(0.0f, 1.0f, 0.0f)).Normalize(center);
                //else if (_snapZ)
                //    normal = (new Vector3(0.0f, 0.0f, 1.0f)).Normalize(center);
                //else if (_snapCirc)
                //{
                //    radius *= _circOrbScale;
                //    normal = camera.Normalize(center);
                //}
                //else if (Maths.LineSphereIntersect(lineStart, lineEnd, center, radius, out point))
                //    return true;
                //else
                //    normal = camera.Normalize(center);

                //if (Maths.LinePlaneIntersect(lineStart, lineEnd, center, normal, out point))
                //{
                //    point = Maths.PointAtLineDistance(center, point, radius);
                //    return true;
                //}

                //break;

                case TransformType.Translation:

                    if (_snapX && _snapY)
                        normal = new Vector3(0.0f, 0.0f, 1.0f);
                    else if (_snapX && _snapZ)
                        normal = new Vector3(0.0f, 1.0f, 0.0f);
                    else if (_snapY && _snapZ)
                        normal = new Vector3(1.0f, 0.0f, 0.0f);
                    else if (_snapX)
                        normal = new Vector3(0.0f, 1.0f, 0.0f);
                    else if (_snapY)
                        normal = new Vector3(1.0f, 0.0f, 0.0f);
                    else if (_snapZ)
                        normal = new Vector3(0.0f, 1.0f, 0.0f);

                    break;
            }

            if (!Maths.LinePlaneIntersect(lineStart, lineEnd, center, normal, out point))
            {
                point = new Vector3();
                return false;
            }
            return true;
        }
        public Vertex3 CompareVertexDistance(Vector3 point)
        {
            if (TargetModel == null)
                return null;

            if (TargetModel.SelectedObjectIndex != -1)
            {
                IObject o = TargetModel.Objects[TargetModel.SelectedObjectIndex];
                if (o.IsRendering)
                    foreach (Vertex3 v in o.PrimitiveManager._vertices)
                    {
                        float t = v.WeightedPosition.TrueDistance(point);
                        if (Math.Abs(t) < 0.025f)
                            return v;
                    }
                else
                    foreach (IObject w in TargetModel.Objects)
                        if (w.IsRendering)
                            foreach (Vertex3 v in w.PrimitiveManager._vertices)
                            {
                                float t = v.WeightedPosition.TrueDistance(point);
                                if (Math.Abs(t) < 0.025f)
                                    return v;
                            }
            }
            else
                foreach (IObject o in TargetModel.Objects)
                    if (o.IsRendering)
                        foreach (Vertex3 v in o.PrimitiveManager._vertices)
                        {
                            float t = v.WeightedPosition.TrueDistance(point);
                            if (Math.Abs(t) < 0.025f)
                                return v;
                        }
            return null;
        }
        private static bool CompareDistanceRecursive(IBoneNode bone, Vector3 point, ref IBoneNode match)
        {
            Vector3 center = bone.Matrix.GetPoint();
            float dist = center.TrueDistance(point);

            if (Math.Abs(dist - MDL0BoneNode._nodeRadius) < 0.01)
            {
                match = bone;
                return true;
            }

            foreach (IBoneNode b in ((ResourceNode)bone).Children)
                if (CompareDistanceRecursive(b, point, ref match))
                    return true;

            return false;
        }

        public static unsafe void RenderOrb(IBoneNode bone, GLDisplayList list)
        {
            Matrix m = Matrix.TransformMatrix(new Vector3(MDL0BoneNode._nodeRadius), new Vector3(), bone.Matrix.GetPoint());
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            list.Call();
            GL.PopMatrix();
        }

        #endregion

        public static void OnRenderLightDisplay(Vector4 lightPos)
        {
            GL.Color4(Color.Blue);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            GL.Scale(lightPos._x, lightPos._x, lightPos._x);

            GL.Rotate(90.0f, 1, 0, 0);

            float 
                azimuth = lightPos._y.RemapToRange(-180.0f, 180.0f),
                elevation = lightPos._z.RemapToRange(-180.0f, 180.0f);

            if (Math.Abs(azimuth) == Math.Abs(elevation) && azimuth % 180.0f == 0 && elevation % 180.0f == 0)
            {
                azimuth = 0;
                elevation = 0;
            }

            int i;
            float e = azimuth, x;

            bool flip = false;
            if (e < 0)
            {
                e = -e;
                flip = true;
                GL.Rotate(180.0f, 1, 0, 0);
            }
                
            float f = (float)((int)e);
            float diff = (float)Math.Round(e - f, 1);

            GL.Begin(PrimitiveType.Lines);
            for (i = 0; i < f; i++)
            {
                GL.Vertex2(Math.Cos(i * Maths._deg2radf), Math.Sin(i * Maths._deg2radf));
                GL.Vertex2(Math.Cos((i + 1) * Maths._deg2radf), Math.Sin((i + 1) * Maths._deg2radf));
            }
            for (x = 0; x < diff; x += 0.1f)
            {
                GL.Vertex2(Math.Cos((x + (float)i) * Maths._deg2radf), Math.Sin((x + (float)i) * Maths._deg2radf));
                GL.Vertex2(Math.Cos((x + 0.1f + (float)i) * Maths._deg2radf), Math.Sin((x + 0.1f + (float)i) * Maths._deg2radf));
            }
            GL.End();

            if (flip) GL.Rotate(-180.0f, 1, 0, 0);

            GL.Rotate(90.0f, 0, 1, 0);
            GL.Rotate(90.0f, 0, 0, 1);
            GL.Rotate(180.0f, 1, 0, 0);
            GL.Rotate(90.0f - azimuth, 0, 1, 0);

            e = elevation;

            if (e < 0)
            {
                e = -e;
                GL.Rotate(180.0f, 1, 0, 0);
            }

            f = (float)((int)e);
            diff = (float)Math.Round(e - f, 1);

            GL.Begin(PrimitiveType.Lines);
            for (i = 0; i < f; i++)
            {
                GL.Vertex2(Math.Cos(i * Maths._deg2radf), Math.Sin(i * Maths._deg2radf));
                GL.Vertex2(Math.Cos((i + 1) * Maths._deg2radf), Math.Sin((i + 1) * Maths._deg2radf));
            }
            for (x = 0; x < diff; x += 0.1f)
            {
                GL.Vertex2(Math.Cos((x + (float)i) * Maths._deg2radf), Math.Sin((x + (float)i) * Maths._deg2radf));
                GL.Vertex2(Math.Cos((x + 0.1f + (float)i) * Maths._deg2radf), Math.Sin((x + 0.1f + (float)i) * Maths._deg2radf));
            }

            GL.Vertex2(Math.Cos((x + (float)i) * Maths._deg2radf), Math.Sin((x + (float)i) * Maths._deg2radf));
            GL.Color4(Color.Orange);
            GL.Vertex3(0, 0, 0);
            GL.End();

            GL.Scale(0.01f, 0.01f, 0.01f);
            GL.Rotate(azimuth, 0, 1, 0);
            GL.Enable(EnableCap.DepthTest);
        }

        public static void OnRenderFloor()
        {
            GLTexture _bgTex = TKContext.FindOrCreate<GLTexture>("TexBG", GLTexturePanel.CreateBG);

            float s = 10.0f, t = 10.0f;
            float e = 30.0f;

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);

            GL.Enable(EnableCap.Texture2D);

            _bgTex.Bind();

            GL.Color4(_floorHue);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-e, 0.0f, -e);
            GL.TexCoord2(s, 0.0f);
            GL.Vertex3(e, 0.0f, -e);
            GL.TexCoord2(s, t);
            GL.Vertex3(e, 0.0f, e);
            GL.TexCoord2(0, t);
            GL.Vertex3(-e, 0.0f, e);

            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }
    }
}