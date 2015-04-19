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
        public unsafe virtual void modelPanel1_PreRender(ModelPanelViewport viewport)
        {
            if (viewport != null)
            {
                if (viewport._renderFloor)
                    OnRenderFloor();

                if (viewport._renderAttrib._applyBillboardBones)
                    if (EditingAll && _targetModels != null)
                        foreach (IModel m in _targetModels)
                            m.UpdateBillboards(viewport);
                    else if (TargetModel != null)
                        TargetModel.UpdateBillboards(viewport);
            }
        }

        public unsafe virtual void modelPanel1_PostRender(ModelPanelViewport vp)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.DepthTest);

            if (_targetModels != null)
                foreach (IModel m in _targetModels)
                    PostRender(m, vp);

            if (RenderLightDisplay/* && vp == ModelPanel.CurrentViewport*/)
                OnRenderLightDisplay(vp.LightPosition);

            if (TargetAnimType == NW4RAnimType.SCN)
                RenderSCN0Controls();

            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            RenderTransformControl(vp);
            RenderDepth(vp);
        }

        #region SCN0 Controls
        public unsafe void RenderSCN0Controls()
        {
            if (_scn0 == null)
                return;

            int frame = CurrentFrame - 1;
            if (frame < 0)
                return;

            GL.Color4(Color.Blue);
            GL.Disable(EnableCap.Lighting);

            if (_SCN0LightSet != null)
            {
                int i = 0;
                foreach (SCN0LightNode l in _SCN0LightSet._lights)
                {
                    if (l == null)
                    {
                        i++;
                        continue;
                    }

                    Vector3 start = l.GetStart(frame);
                    Vector3 end = l.GetEnd(frame);

                    //TODO: draw line, sphere, and cone for respective light types
                    //switch (l.LightType)
                    //{
                    //    case BrawlLib.SSBBTypes.LightType.Directional: 
                            GL.Begin(BeginMode.Lines);
                            GL.Color4((Color)l.GetColor(frame, 0));
                            GL.Vertex3((OpenTK.Vector3)start);
                            if (l.SpecularEnabled) GL.Color4((Color)l.GetColor(frame, 1));
                            GL.Vertex3((OpenTK.Vector3)end);
                            GL.End();
                    //        break;
                    //    case BrawlLib.SSBBTypes.LightType.Point:
                            
                    //        GL.Begin(PrimitiveType.Lines);
                    //        GL.Color4((Color)l.GetColor(frame, 0));
                    //        GL.Vertex3((OpenTK.Vector3)start);
                    //        if (l.SpecularEnabled) GL.Color4((Color)l.GetColor(frame, 1));
                    //        GL.Vertex3((OpenTK.Vector3)end);
                    //        GL.End();
                    //        break;
                    //    case BrawlLib.SSBBTypes.LightType.Spotlight:
                    //        GL.Begin(PrimitiveType.Lines);
                    //        GL.Color4((Color)l.GetColor(frame, 0));
                    //        GL.Vertex3((OpenTK.Vector3)start);
                    //        if (l.SpecularEnabled) GL.Color4((Color)l.GetColor(frame, 1));
                    //        GL.Vertex3((OpenTK.Vector3)end);
                    //        GL.End();
                    //        break;
                    //}

                    foreach (ModelPanelViewport v in ModelPanel)
                    {
                        v.ScreenText[l.Name] = v.Camera.Project(start);
                        //v.ScreenText[l.Name] = v.Camera.Project(end);
                    }

                    //GL.Color4(Color.MediumPurple);
                    //GL.Begin(BeginMode.LineStrip);
                    //for (int i = 0; i < MaxFrame; i++)
                    //    GL.Vertex3(l.GetFrameValue(LightKeyframeMode.StartX, i), l.GetFrameValue(LightKeyframeMode.StartY, i), l.GetFrameValue(LightKeyframeMode.StartZ, i));
                    //GL.End();

                    //GL.Color4(Color.ForestGreen);
                    //GL.Begin(BeginMode.LineStrip);
                    //for (int i = 0; i < MaxFrame; i++)
                    //    GL.Vertex3(l.GetFrameValue(LightKeyframeMode.EndX, i), l.GetFrameValue(LightKeyframeMode.EndY, i), l.GetFrameValue(LightKeyframeMode.EndZ, i));
                    //GL.End();

                    //Render these if selected
                    //if ((_lightStartSelected || _lightEndSelected) && l == scn0Editor._light)
                    //{
                    //    Matrix m;
                    //    float s1 = start.TrueDistance(CamLoc) / _orbRadius * 0.1f;
                    //    float e1 = end.TrueDistance(CamLoc) / _orbRadius * 0.1f;
                    //    GLDisplayList axis = GetAxes();
                    //    if (_lightStartSelected)
                    //    {
                    //        m = Matrix.TransformMatrix(new Vector3(s1), new Vector3(), start);

                    //        GL.PushMatrix();
                    //        GL.MultMatrix((float*)&m);

                    //        axis.Call();
                    //        GL.PopMatrix();
                    //    }
                    //    if (_lightEndSelected)
                    //    {
                    //        m = Matrix.TransformMatrix(new Vector3(e1), new Vector3(), end);

                    //        GL.PushMatrix();
                    //        GL.MultMatrix((float*)&m);

                    //        axis.Call();
                    //        GL.PopMatrix();
                    //    }
                    //}
                    i++;
                }
            }

            if (_SCN0Camera != null)
            {
                Vector3 start = _SCN0Camera.GetStart(frame);
                Vector3 end = _SCN0Camera.GetEnd(frame);

                GL.Color4(Color.Black);
                GL.Begin(BeginMode.Lines);

                GL.Vertex3((OpenTK.Vector3)start);
                GL.Vertex3((OpenTK.Vector3)end);

                GL.End();

                foreach (ModelPanelViewport v in ModelPanel)
                {
                    v.ScreenText["Camera Position"] = v.Camera.Project(start);
                    //v.ScreenText["Camera Aim"] = v.Camera.Project(end);
                }

                //GL.Color4(Color.OrangeRed);
                //GL.Begin(BeginMode.LineStrip);
                //for (int i = 0; i < MaxFrame; i++)
                //    GL.Vertex3(c.GetFrameValue(CameraKeyframeMode.PosX, i), c.GetFrameValue(CameraKeyframeMode.PosY, i), c.GetFrameValue(CameraKeyframeMode.PosZ, i));
                //GL.End();

                //GL.Color4(Color.SkyBlue);
                //GL.Begin(BeginMode.LineStrip);
                //for (int i = 0; i < MaxFrame; i++)
                //    GL.Vertex3(c.GetFrameValue(CameraKeyframeMode.AimX, i), c.GetFrameValue(CameraKeyframeMode.AimY, i), c.GetFrameValue(CameraKeyframeMode.AimZ, i));
                //GL.End();

                GL.Color4(Color.Black);

                //Render these if selected
                //if (_lightStartSelected || _lightEndSelected)
                //{
                //    Matrix m;
                //    float s = start.TrueDistance(CamLoc) / _orbRadius * 0.1f;
                //    float e = end.TrueDistance(CamLoc) / _orbRadius * 0.1f;
                //    GLDisplayList axis = GetAxes();
                //    if (_lightStartSelected)
                //    {
                //        m = Matrix.TransformMatrix(new Vector3(s), new Vector3(), start);

                //        GL.PushMatrix();
                //        GL.MultMatrix((float*)&m);

                //        axis.Call();
                //        GL.PopMatrix();
                //    }
                //    if (_lightEndSelected)
                //    {
                //        m = Matrix.TransformMatrix(new Vector3(e), new Vector3(), end);

                //        GL.PushMatrix();
                //        GL.MultMatrix((float*)&m);

                //        axis.Call();
                //        GL.PopMatrix();
                //    }
                //}
            }
        }

        #endregion

        public virtual void PostRender(IModel model, ModelPanelViewport vp)
        {
            if (vp._renderAttrib._renderVertices)
                model.RenderVertices(false, SelectedBone, vp.Camera);
            if (vp._renderAttrib._renderNormals)
                model.RenderNormals();
            if (vp._renderAttrib._renderBones)
                model.RenderBones(vp);

            model.RenderBoxes(
                vp._renderAttrib._renderModelBox,
                vp._renderAttrib._renderObjectBoxes,
                vp._renderAttrib._renderBoneBoxes, 
                vp._renderAttrib._useBindStateBoxes);
        }

        #region Bone Control Rendering
        public unsafe void RenderTransformControl(ModelPanelViewport panel)
        {
            if (_playing)
                return;

            bool hasBone = SelectedBone != null;
            if (hasBone || VertexLoc().HasValue)
            {
                Vector3 pos, rot;
                float radius;

                if (hasBone)
                {
                    pos = BoneLoc(SelectedBone);
                    radius = OrbRadius(SelectedBone, panel.Camera);
                    rot = SelectedBone.Matrix.GetAngles();
                }
                else
                {
                    pos = VertexLoc().Value;
                    radius = VertexOrbRadius(panel.Camera);
                    rot = new Vector3();
                }

                switch (ControlType)
                {
                    case TransformType.Translation:
                        RenderTranslationControl(pos, radius, panel);
                        break;
                    case TransformType.Rotation:
                        RenderRotationControl(pos, radius, rot, panel);
                        break;
                    case TransformType.Scale:
                        RenderScaleControl(pos, radius, panel);
                        break;
                }
            }
        }
        public unsafe void RenderTranslationControl(Vector3 position, float radius, ModelPanelViewport panel)
        {
            GLDisplayList axis = GetAxes();

            //Enter local space
            Matrix m = Matrix.TransformMatrix(new Vector3(radius), new Vector3(), position);
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            axis.Call();

            GL.PopMatrix();

            panel.ScreenText["X"] = panel.Camera.Project(new Vector3(_axisLDist + 0.1f, 0, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            panel.ScreenText["Y"] = panel.Camera.Project(new Vector3(0, _axisLDist + 0.1f, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            panel.ScreenText["Z"] = panel.Camera.Project(new Vector3(0, 0, _axisLDist + 0.1f) * m) - new Vector3(8.0f, 8.0f, 0);
        }
        public unsafe void RenderScaleControl(Vector3 pos, float radius, ModelPanelViewport panel)
        {
            GLDisplayList axis = GetScaleControl();

            //Enter local space
            Matrix m = Matrix.TransformMatrix(new Vector3(radius), new Vector3(), pos);
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            axis.Call();

            GL.PopMatrix();

            panel.ScreenText["X"] = panel.Camera.Project(new Vector3(_axisLDist + 0.1f, 0, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            panel.ScreenText["Y"] = panel.Camera.Project(new Vector3(0, _axisLDist + 0.1f, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            panel.ScreenText["Z"] = panel.Camera.Project(new Vector3(0, 0, _axisLDist + 0.1f) * m) - new Vector3(8.0f, 8.0f, 0);
        }
        public unsafe void RenderRotationControl(Vector3 position, float radius, Vector3 rotate, ModelPanelViewport panel)
        {
            Matrix m = Matrix.TransformMatrix(
                new Vector3(radius), 
                panel.ViewType == ViewportProjection.Perspective ? position.LookatAngles(CamLoc(panel.Camera)) * Maths._rad2degf : panel.Camera._rotation, 
                position);

            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            GLDisplayList sphere = TKContext.GetCircleList();
            GLDisplayList circle = TKContext.GetRingList();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

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

            panel.ScreenText["X"] = panel.Camera.Project(new Vector3(1.1f, 0, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            panel.ScreenText["Y"] = panel.Camera.Project(new Vector3(0, 1.1f, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            panel.ScreenText["Z"] = panel.Camera.Project(new Vector3(0, 0, 1.1f) * m) - new Vector3(8.0f, 8.0f, 0);

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

            GL.Begin(BeginMode.Lines);

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

            GL.Begin(BeginMode.Triangles);

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

            GL.Begin(BeginMode.Lines);

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

            GL.Begin(BeginMode.Triangles);

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

            GL.Begin(BeginMode.Lines);

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

            GL.Begin(BeginMode.Triangles);

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

            GL.Begin(BeginMode.Lines);

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

            GL.Begin(BeginMode.Triangles);

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

            GL.Begin(BeginMode.Lines);

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

            GL.Begin(BeginMode.Triangles);

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

            GL.Begin(BeginMode.Lines);

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

            GL.Begin(BeginMode.Triangles);

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
#if DEBUG
        protected bool _renderDepth;
#endif
        public unsafe virtual void RenderDepth(ModelPanelViewport v)
        {
            if (v._grabbing || v._scrolling || _playing)
                return;

            GL.Color4(Color.Black);
#if DEBUG
            GL.ColorMask(_renderDepth, false, false, false);
#else
            GL.ColorMask(false, false, false, false);
#endif

            if (v._renderAttrib._renderVertices)
                if (EditingAll && _targetModels != null)
                    foreach (IModel m in _targetModels)
                        m.RenderVertices(true, SelectedBone, v.Camera);
                else if (TargetModel != null)
                    TargetModel.RenderVertices(true, SelectedBone, v.Camera);

            if (v._renderAttrib._renderBones)
            {
                //Render invisible depth orbs
                GLDisplayList list = TKContext.GetSphereList();
                bool doScale = v._renderAttrib._scaleBones;
                if (EditingAll)
                {
                    foreach (IModel m in _targetModels)
                        foreach (IBoneNode bone in m.BoneCache)
                            if (bone != SelectedBone)
                                RenderOrb(bone, list, v, doScale);
                }
                else if (TargetModel != null)
                    foreach (IBoneNode bone in _targetModel.BoneCache)
                        if (bone != SelectedBone)
                            RenderOrb(bone, list, v, doScale);
            }

            //Render invisible depth planes for translation and scale controls
            if ((ControlType != TransformType.Rotation && SelectedBone != null) || VertexLoc().HasValue)
            {
                #region Axis Selection Display List

                GLDisplayList selList = new GLDisplayList();

                selList.Begin();

                GL.Begin(BeginMode.Quads);

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
                    Matrix m = Matrix.TransformMatrix(new Vector3(OrbRadius(SelectedBone, v)), new Vector3(), BoneLoc(SelectedBone));
                    GL.PushMatrix();
                    GL.MultMatrix((float*)&m);

                    selList.Call();

                    if (ControlType == TransformType.Translation)
                    {
                        GL.Begin(BeginMode.Quads);

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
                        GL.Begin(BeginMode.Triangles);

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

                if (VertexLoc().HasValue && v._renderAttrib._renderVertices)
                {
                    Matrix m = Matrix.TransformMatrix(new Vector3(VertexOrbRadius(v)), new Vector3(), ((Vector3)VertexLoc()));
                    GL.PushMatrix();
                    GL.MultMatrix((float*)&m);

                    selList.Call();

                    GL.Begin(BeginMode.Quads);

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

        #endregion

        #region Orb Point Distance

        /// <summary>
        /// Gets world-point of specified mouse point projected onto the selected bone's local space if rotating or in world space if translating or scaling.
        /// Intersects the projected ray with the appropriate plane using the snap flags.
        /// </summary>
        public bool GetOrbPoint(Vector2 mousePoint, out Vector3 point, ModelPanelViewport panel)
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
            float radius = CamDistance(center, ModelPanel.CurrentViewport);

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
                    else
                        normal = camera.Normalize(center);

                    break;
            }

            return Maths.LinePlaneIntersect(lineStart, lineEnd, center, normal, out point);
        }

        public bool GetVertexOrbPoint(Vector2 mousePoint, Vector3 center, out Vector3 point, ModelPanelViewport panel)
        {
            Vector3 lineStart = panel.UnProject(mousePoint._x, mousePoint._y, 0.0f);
            Vector3 lineEnd = panel.UnProject(mousePoint._x, mousePoint._y, 1.0f);
            Vector3 camera = panel.Camera.GetPoint();
            Vector3 normal = new Vector3();
            float radius = CamDistance(center, ModelPanel.CurrentViewport);

            switch (ControlType)
            {
                case TransformType.Rotation:
                    if (_snapX)
                        normal = (Matrix.TranslationMatrix(VertexLoc().Value) * new Vector3(1.0f, 0.0f, 0.0f)).Normalize(center);
                    else if (_snapY)
                        normal = (Matrix.TranslationMatrix(VertexLoc().Value) * new Vector3(0.0f, 1.0f, 0.0f)).Normalize(center);
                    else if (_snapZ)
                        normal = (Matrix.TranslationMatrix(VertexLoc().Value) * new Vector3(0.0f, 0.0f, 1.0f)).Normalize(center);
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

                case TransformType.Scale:
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
                    else
                        normal = camera.Normalize(center);

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
                    foreach (Vertex3 v in o.Vertices)
                    {
                        float t = v.WeightedPosition.TrueDistance(point);
                        if (Math.Abs(t) < 0.025f)
                            return v;
                    }
                else
                    foreach (IObject w in TargetModel.Objects)
                        if (w.IsRendering)
                            foreach (Vertex3 v in w.Vertices)
                            {
                                float t = v.WeightedPosition.TrueDistance(point);
                                if (Math.Abs(t) < 0.025f)
                                    return v;
                            }
            }
            else
                foreach (IObject o in TargetModel.Objects)
                    if (o.IsRendering)
                        foreach (Vertex3 v in o.Vertices)
                        {
                            float t = v.WeightedPosition.TrueDistance(point);
                            if (Math.Abs(t) < 0.025f)
                                return v;
                        }
            return null;
        }
        private bool CompareDistanceRecursive(IBoneNode bone, Vector3 point, ref IBoneNode match, ModelPanelViewport v, bool doScale)
        {
            float dist = bone.Matrix.GetPoint().TrueDistance(point) / (doScale ? OrbRadius(bone, v) : 1.0f);
            if (Math.Abs(dist - MDL0BoneNode._nodeRadius) < 0.01f)
            {
                match = bone;
                return true;
            }

            foreach (IBoneNode b in ((ResourceNode)bone).Children)
                if (CompareDistanceRecursive(b, point, ref match, v, doScale))
                    return true;

            return false;
        }

        public unsafe void RenderOrb(IBoneNode bone, GLDisplayList list, ModelPanelViewport v, bool doScale)
        {
            float radius = MDL0BoneNode._nodeRadius * (doScale ? OrbRadius(bone, v) : 1.0f);

            Matrix m = Matrix.TransformMatrix(new Vector3(radius), new Vector3(), bone.Matrix.GetPoint());
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            list.Call();
            GL.PopMatrix();
        }

        #endregion

        public static void OnRenderLightDisplay(Vector4 lightPos)
        {
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();

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

            GL.Begin(BeginMode.Lines);
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

            GL.Begin(BeginMode.Lines);
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

            GL.PopAttrib();
            GL.PopMatrix();
        }

        public static void OnRenderFloor()
        {
            float s = 10.0f, t = 10.0f;
            float e = 30.0f;

            GL.PushAttrib(AttribMask.AllAttribBits);

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Lighting);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);

            //So that the model clips with the floor
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();

            GLTexture bgTex = TKContext.FindOrCreate<GLTexture>("TexBG", GLTexturePanel.CreateBG);
            bgTex.Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.Color4(_floorHue);

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(-e, 0.0f, -e);
            GL.TexCoord2(s, 0.0f);
            GL.Vertex3(e, 0.0f, -e);
            GL.TexCoord2(s, t);
            GL.Vertex3(e, 0.0f, e);
            GL.TexCoord2(0.0f, t);
            GL.Vertex3(-e, 0.0f, e);

            GL.End();

            GL.Disable(EnableCap.Texture2D);

            GL.PopAttrib();
        }
    }
}