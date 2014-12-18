using System;
using BrawlLib.OpenGL;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.Modeling;
using System.Drawing;
using BrawlLib.Wii.Animations;
using System.Collections.Generic;
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
        #region Post Render

        public unsafe override void modelPanel1_PostRender(object sender)
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
            if (RenderBones)
                OnRenderBones();

            GL.Clear(ClearBufferMask.DepthBufferBit);

            RenderBrawlStageData(panel);

            //This should probably be moved to BrawlLib in case a 3rd party wants to use it in their program
            //Ikarus doesn't need it though
            RenderSCN0Controls();

            RenderTransformControl(panel);
            RenderDepth(panel);
        }

        #endregion

        #region SCN0 Controls
        public Point _lightStartPoint, _lightEndPoint, _cameraStartPoint, _cameraEndPoint;
        public bool _lightEndSelected = false, _lightStartSelected = false;
        public unsafe void RenderSCN0Controls()
        {
            if (_scn0 == null)
                return;

            GL.Color4(Color.Blue);
            GL.Disable(EnableCap.Lighting);

            if (scn0Editor._light != null)
            {
                SCN0LightNode l = scn0Editor._light;
                Vector3 start = new Vector3(
                l.GetFrameValue(LightKeyframeMode.StartX, CurrentFrame - 1),
                l.GetFrameValue(LightKeyframeMode.StartY, CurrentFrame - 1),
                l.GetFrameValue(LightKeyframeMode.StartZ, CurrentFrame - 1));
                Vector3 end = new Vector3(
                l.GetFrameValue(LightKeyframeMode.EndX, CurrentFrame - 1),
                l.GetFrameValue(LightKeyframeMode.EndY, CurrentFrame - 1),
                l.GetFrameValue(LightKeyframeMode.EndZ, CurrentFrame - 1));

                //GL.Color4(Color.Coral);
                GL.Begin(PrimitiveType.Lines);

                GL.Vertex3(start._x, start._y, start._z);
                GL.Vertex3(end._x, end._y, end._z);

                GL.End();

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

                ModelPanel.ScreenText["Light Start"] = ModelPanel.Project(start);
                ModelPanel.ScreenText["Light End"] = ModelPanel.Project(end);

                //Render these if selected
                //if (_lightStartSelected || _lightEndSelected)
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
            }

            if (scn0Editor._camera != null)
            {
                SCN0CameraNode c = scn0Editor._camera;
                Vector3 start = new Vector3(
                c.GetFrameValue(CameraKeyframeMode.PosX, CurrentFrame - 1),
                c.GetFrameValue(CameraKeyframeMode.PosY, CurrentFrame - 1),
                c.GetFrameValue(CameraKeyframeMode.PosZ, CurrentFrame - 1));
                Vector3 end = new Vector3(
                c.GetFrameValue(CameraKeyframeMode.AimX, CurrentFrame - 1),
                c.GetFrameValue(CameraKeyframeMode.AimY, CurrentFrame - 1),
                c.GetFrameValue(CameraKeyframeMode.AimZ, CurrentFrame - 1));

                //GL.Color4(Color.Blue);
                GL.Begin(PrimitiveType.Lines);

                GL.Vertex3(start._x, start._y, start._z);
                GL.Vertex3(end._x, end._y, end._z);

                GL.End();

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

                ModelPanel.ScreenText["Camera Position"] = ModelPanel.Project(start);
                ModelPanel.ScreenText["Camera Aim"] = ModelPanel.Project(end);

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

        #region Brawl Stage Data Rendering

        public void RenderBrawlStageData(ModelPanel panel)
        {
            //If you ever make changes to GL attributes (enabled and disabled things)
            //and don't want to keep track of what you changed,
            //you can push all attributes and then pop them when you're done, like this.
            //This will make sure the GL state is back to how it was before you changed it.
            GL.PushAttrib(AttribMask.AllAttribBits);

            if (panel.RenderCollisions)
                foreach (CollisionNode node in _collisions)
                    node.Render();
            
            #region RenderOverlays
            List<MDL0BoneNode> ItemBones = new List<MDL0BoneNode>();

            MDL0Node stgPos = null;

            MDL0BoneNode CamBone0 = null, CamBone1 = null,
                         DeathBone0 = null, DeathBone1 = null;

            //Get bones and render spawns if checked
            if (_targetModel != null &&
                _targetModel is MDL0Node &&
                ((((ResourceNode)_targetModel).Name.Contains("StgPosition")) ||
                ((ResourceNode)_targetModel).Name.Contains("stagePosition")))
                stgPos = _targetModel as MDL0Node;
            else if (_targetModels != null)
                stgPos = _targetModels.Find(x => x is MDL0Node &&
                    ((ResourceNode)x).Name.Contains("StgPosition") ||
                    ((ResourceNode)x).Name.Contains("stagePosition")) as MDL0Node;

            if (stgPos != null) 
                foreach (MDL0BoneNode bone in stgPos._linker.BoneCache)
                {
                    if (bone._name == "CamLimit0N") { CamBone0 = bone; }
                    else if (bone.Name == "CamLimit1N") { CamBone1 = bone; }
                    else if (bone.Name == "Dead0N") { DeathBone0 = bone; }
                    else if (bone.Name == "Dead1N") { DeathBone1 = bone; }
                    else if (bone._name.Contains("Player") && chkSpawns.Checked)
                    {
                        Vector3 position = bone._frameMatrix.GetPoint();

                        if (PointCollides(position))
                            GL.Color4(0.0f, 1.0f, 0.0f, 0.5f);
                        else 
                            GL.Color4(1.0f, 0.0f, 0.0f, 0.5f);

                        TKContext.DrawSphere(position, 5.0f, 32);
                    }
                    else if (bone._name.Contains("Rebirth") && chkSpawns.Checked)
                    {
                        GL.Color4(1.0f, 1.0f, 1.0f, 0.5f);
                        TKContext.DrawSphere(bone._frameMatrix.GetPoint(), 5.0f, 32);
                    }
                    else if (bone._name.Contains("Item"))
                        ItemBones.Add(bone);
                }

            //Render item fields if checked
            if (ItemBones != null && chkItems.Checked) 
                for (int i = 0; i < ItemBones.Count; i += 2)
                {
                    Vector3 pos1 = new Vector3(ItemBones[i]._frameMatrix.GetPoint()._x, ItemBones[i]._frameMatrix.GetPoint()._y + 3.0f, 1.0f);
                    Vector3 pos2 = new Vector3(ItemBones[i+1]._frameMatrix.GetPoint()._x, ItemBones[i+1]._frameMatrix.GetPoint()._y - 3.0f, 1.0f);
                    GL.Color4(0.5f, 0.0f, 1.0f, 0.4f);
                    TKContext.DrawBox(pos1, pos2);
                }

            //Render boundaries if checked
            if (CamBone0 != null && CamBone1 != null && chkBoundaries.Checked)
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Disable(EnableCap.Lighting);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);

                GL.Color4(Color.Blue);
                GL.Begin(PrimitiveType.LineLoop);
                GL.LineWidth(15.0f);

                Vector3
                    camBone0 = CamBone0._frameMatrix.GetPoint(),
                    camBone1 = CamBone1._frameMatrix.GetPoint(),
                    deathBone0 = DeathBone0._frameMatrix.GetPoint(),
                    deathBone1 = DeathBone1._frameMatrix.GetPoint();

                GL.Vertex2(camBone0._x, camBone0._y);
                GL.Vertex2(camBone1._x, camBone0._y);
                GL.Vertex2(camBone1._x, camBone1._y);
                GL.Vertex2(camBone0._x, camBone1._y);
                GL.End();
                GL.Begin(PrimitiveType.LineLoop);
                GL.Color4(Color.Red);
                GL.Vertex2(deathBone0._x, deathBone0._y);
                GL.Vertex2(deathBone1._x, deathBone0._y);
                GL.Vertex2(deathBone1._x, deathBone1._y);
                GL.Vertex2(deathBone0._x, deathBone1._y);
                GL.End();
                GL.Color4(0.0f, 0.5f, 1.0f, 0.3f);
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(camBone0._x, camBone0._y);
                GL.Vertex2(deathBone0._x, deathBone0._y);
                GL.Vertex2(deathBone1._x, deathBone0._y);
                GL.Vertex2(camBone1._x, camBone0._y);
                GL.End();
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(camBone1._x, camBone1._y);
                GL.Vertex2(deathBone1._x, deathBone1._y);
                GL.Vertex2(deathBone0._x, deathBone1._y);
                GL.Vertex2(camBone0._x, camBone1._y);
                GL.End();
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(camBone1._x, camBone0._y);
                GL.Vertex2(deathBone1._x, deathBone0._y);
                GL.Vertex2(deathBone1._x, deathBone1._y);
                GL.Vertex2(camBone1._x, camBone1._y);
                GL.End();
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(camBone0._x, camBone1._y);
                GL.Vertex2(deathBone0._x, deathBone1._y);
                GL.Vertex2(deathBone0._x, deathBone0._y);
                GL.Vertex2(camBone0._x, camBone0._y);
                GL.End();
            }

            #endregion

            GL.PopAttrib();
        }

        #endregion
    }
}
