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
    public partial class ModelEditControl : UserControl, IMainWindow
    {
        #region Pre Render
        private unsafe void modelPanel1_PreRender(object sender, TKContext ctx)
        {
            if (RenderFloor)
            {
                GLTexture _bgTex = ctx.FindOrCreate<GLTexture>("TexBG", GLTexturePanel.CreateBG);

                float s = 10.0f, t = 10.0f;
                float e = 30.0f;

                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.Lighting);
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);

                GL.Enable(EnableCap.Texture2D);

                _bgTex.Bind();

                GL.Color4(StaticMainWindow._floorHue);

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

        #endregion

        #region Post Render

        Vector3 BoneLoc { get { return SelectedBone == null ? new Vector3() : SelectedBone._frameMatrix.GetPoint(); } }

        Vector3? _vertexLoc = null;
        Vector3? VertexLoc
        {
            get
            {
                if (_selectedVertices == null || _selectedVertices.Count == 0)
                    return null;

                if (_vertexLoc != null && _rotating)
                    return _vertexLoc;

                Vector3 average = new Vector3();
                foreach (Vertex3 v in _selectedVertices)
                    average += v.WeightedPosition;
                average /= _selectedVertices.Count;
                return _vertexLoc = average;
            }
        }

        Vector3 CamLoc { get { return ModelPanel._camera.GetPoint(); } }

        public float CamDistance(Vector3 v) { return v.TrueDistance(CamLoc) / _orbRadius * 0.1f; }

        float OrbRadius { get { return BoneLoc.TrueDistance(CamLoc) / _orbRadius * 0.1f; } }
        float VertexOrbRadius { get { return ((Vector3)VertexLoc).TrueDistance(CamLoc) / _orbRadius * 0.1f; } }

        Matrix CamFacingMatrix { get { return Matrix.TransformMatrix(new Vector3(OrbRadius), BoneLoc.LookatAngles(CamLoc) * Maths._rad2degf, BoneLoc); } }

        public TransformType _editType = TransformType.Rotation;
        public enum TransformType
        {
            None = -1,
            Rotation = 0,
            Translation = 1,
            Scale = 2
        }

        public int _hurtBoxType = 0;

        public List<MDL0BoneNode> boneCollisions = new List<MDL0BoneNode>();
        private unsafe void modelPanel1_PostRender(object sender, TKContext context)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.DepthTest);
            
            if (RenderVertices)
                if (_editingAll && _targetModels != null)
                    foreach (MDL0Node m in _targetModels)
                        m.RenderVertices(context, false);
                else if (TargetModel != null)
                    TargetModel.RenderVertices(context, false);

            if (RenderNormals)
                if (_editingAll && _targetModels != null)
                    foreach (MDL0Node m in _targetModels)
                        m.RenderNormals(context);
                else if (TargetModel != null)
                    TargetModel.RenderNormals(context);

            //Show the user where the light source is
            if (_renderLightDisplay)
            {
                GL.Color4(Color.Blue);
                GL.Disable(EnableCap.Lighting);
                GL.Disable(EnableCap.DepthTest);

                GL.Scale(ModelPanel.LightPosition._x, ModelPanel.LightPosition._x, ModelPanel.LightPosition._x);
                
                GL.Rotate(90.0f, 1, 0, 0);

                float azimuth = ModelPanel.LightPosition._y.Clamp180Deg();
                float elevation = ModelPanel.LightPosition._z.Clamp180Deg();

                if (Math.Abs(azimuth) == Math.Abs(elevation) && azimuth % 180.0f == 0 && elevation % 180.0f == 0)
                {
                    azimuth = 0;
                    elevation = 0;
                }

                int i; float x;
                float e = azimuth;

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
            }

            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (RenderCollisions)
            foreach (CollisionNode node in _collisions) node.Render(context, this.Parent as ModelPanel);
            
            #region RenderOverlays
            MDL0BoneNode CamBone0 = null;
            MDL0BoneNode CamBone1 = null;
            MDL0BoneNode DeathBone0 = null;
            MDL0BoneNode DeathBone1 = null;
            List<MDL0BoneNode> ItemBones = new List<MDL0BoneNode>();

            //Get bones and render spawns if checked
            if (_targetModels != null) foreach (MDL0Node m in _targetModels)
                foreach (MDL0BoneNode bone in m._linker.BoneCache)
                {
                    if (bone._name == "CamLimit0N") { CamBone0 = bone; }
                    else if (bone.Name == "CamLimit1N") { CamBone1 = bone; }
                    else if (bone.Name == "Dead0N") { DeathBone0 = bone; }
                    else if (bone.Name == "Dead1N") { DeathBone1 = bone; }
                    else if (bone._name.Contains("Player") && chkSpawns.Checked)
                    {
                        Vector3 position = bone._frameMatrix.GetPoint();
                        if (PointCollides(position)){ GL.Color4(0.0f, 1.0f, 0.0f, 0.5f);}
                        else{ GL.Color4(1.0f, 0.0f, 0.0f, 0.5f); }
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
            if (ItemBones != null && chkItems.Checked) for (int i = 0; i < ItemBones.Count; i += 2)
                {
                    Vector3 pos1 = new Vector3(ItemBones[i]._frameMatrix.GetPoint()._x, ItemBones[i]._frameMatrix.GetPoint()._y + 3.0f, 1.0f);
                    Vector3 pos2 = new Vector3(ItemBones[i+1]._frameMatrix.GetPoint()._x, ItemBones[i+1]._frameMatrix.GetPoint()._y - 3.0f, 1.0f);
                    GL.Color4(0.5f, 0.0f, 1.0f, 0.6f);
                    context.DrawBox(pos1,pos2);
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
                GL.Vertex2(CamBone0._frameMatrix.GetPoint()._x, CamBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(CamBone1._frameMatrix.GetPoint()._x, CamBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(CamBone1._frameMatrix.GetPoint()._x, CamBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(CamBone0._frameMatrix.GetPoint()._x, CamBone1._frameMatrix.GetPoint()._y);
                GL.End();
                GL.Begin(PrimitiveType.LineLoop);
                GL.Color4(Color.Red);
                GL.Vertex2(DeathBone0._frameMatrix.GetPoint()._x, DeathBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone1._frameMatrix.GetPoint()._x, DeathBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone1._frameMatrix.GetPoint()._x, DeathBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone0._frameMatrix.GetPoint()._x, DeathBone1._frameMatrix.GetPoint()._y);
                GL.End();
                GL.Color4(0.0f, 0.5f, 1.0f, 0.3f);
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(CamBone0._frameMatrix.GetPoint()._x, CamBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone0._frameMatrix.GetPoint()._x, DeathBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone1._frameMatrix.GetPoint()._x, DeathBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(CamBone1._frameMatrix.GetPoint()._x, CamBone0._frameMatrix.GetPoint()._y);
                GL.End();
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(CamBone1._frameMatrix.GetPoint()._x, CamBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone1._frameMatrix.GetPoint()._x, DeathBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone0._frameMatrix.GetPoint()._x, DeathBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(CamBone0._frameMatrix.GetPoint()._x, CamBone1._frameMatrix.GetPoint()._y);
                GL.End();
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(CamBone1._frameMatrix.GetPoint()._x, CamBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone1._frameMatrix.GetPoint()._x, DeathBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone1._frameMatrix.GetPoint()._x, DeathBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(CamBone1._frameMatrix.GetPoint()._x, CamBone1._frameMatrix.GetPoint()._y);
                GL.End();
                GL.Begin(PrimitiveType.TriangleFan);
                GL.Vertex2(CamBone0._frameMatrix.GetPoint()._x, CamBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone0._frameMatrix.GetPoint()._x, DeathBone1._frameMatrix.GetPoint()._y);
                GL.Vertex2(DeathBone0._frameMatrix.GetPoint()._x, DeathBone0._frameMatrix.GetPoint()._y);
                GL.Vertex2(CamBone0._frameMatrix.GetPoint()._x, CamBone0._frameMatrix.GetPoint()._y);
                GL.End();
            }
        
            #endregion

            RenderSCN0Controls(context);
            RenderTransformControl(context);

            if (!ModelPanel._grabbing && !ModelPanel._scrolling && !_playing)
            {
                GL.Color4(Color.Black);
                GL.ColorMask(false, false, false, false);

                if (RenderVertices)
                    if (_editingAll && _targetModels != null)
                        foreach (MDL0Node m in _targetModels)
                            m.RenderVertices(context, true);
                    else if (TargetModel != null)
                        TargetModel.RenderVertices(context, true);

                if (RenderBones)
                {
                    //Render invisible depth orbs
                    GLDisplayList list = context.GetSphereList();
                    if (_editingAll)
                    {
                        foreach (MDL0Node m in _targetModels)
                            foreach (MDL0BoneNode bone in m._linker.BoneCache)
                                if (bone != SelectedBone)
                                    RenderOrb(bone, list);
                    }
                    else if (TargetModel != null && TargetModel._linker != null && TargetModel._linker.BoneCache != null)
                        foreach (MDL0BoneNode bone in _targetModel._linker.BoneCache)
                            if (bone != SelectedBone)
                                RenderOrb(bone, list);
                }

                //Render invisible depth planes for translation and scale controls
                if (_editType != TransformType.Rotation && ((SelectedBone != null && RenderBones) || (VertexLoc != null && RenderVertices)))
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

                    if (_editType != TransformType.Rotation && SelectedBone != null)
                    {
                        Matrix m = Matrix.TransformMatrix(new Vector3(OrbRadius), new Vector3(), BoneLoc);
                        GL.PushMatrix();
                        GL.MultMatrix((float*)&m);

                        selList.Call();

                        if (_editType == TransformType.Translation)
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

                    if (VertexLoc != null && RenderVertices)
                    {
                        Matrix m = Matrix.TransformMatrix(new Vector3(VertexOrbRadius), new Vector3(), ((Vector3)VertexLoc));
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
        public bool _renderLightDisplay = false;
        #endregion

        #region SCN0 Controls
        public Point _lightStartPoint, _lightEndPoint, _cameraStartPoint, _cameraEndPoint;
        public unsafe void RenderSCN0Controls(TKContext context)
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

        #region Control Rendering

        public bool _lightEndSelected = false, _lightStartSelected = false;
        public unsafe void RenderTransformControl(TKContext context)
        {
            if (_playing)
                return;

            if (SelectedBone != null) //Render drag and drop control
            {
                if (_editType == TransformType.Rotation)
                    RenderRotationControl(context, BoneLoc, OrbRadius, SelectedBone._frameMatrix.GetAngles());
                else if (_editType == TransformType.Translation)
                    RenderTranslationControl(context, BoneLoc, OrbRadius);
                else if (_editType == TransformType.Scale)
                    RenderScaleControl(context);
            }

            if (VertexLoc != null && RenderVertices)
            {
                //if (_editType == TransformType.Rotation)
                //    RenderRotationControl(context, ((Vector3)VertexLoc), VertexOrbRadius, _oldAngles);
                //else
                RenderTranslationControl(context, ((Vector3)VertexLoc), VertexOrbRadius);
            }
        }
        public unsafe void RenderTranslationControl(TKContext context, Vector3 position, float radius)
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
        public unsafe void RenderScaleControl(TKContext context)
        {
            GLDisplayList axis = GetScaleControl();

            //Enter local space
            Matrix m = Matrix.TransformMatrix(new Vector3(OrbRadius), new Vector3(), BoneLoc);
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            axis.Call();

            GL.PopMatrix();

            ModelPanel.ScreenText["X"] = ModelPanel.Project(new Vector3(_axisLDist + 0.1f, 0, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Y"] = ModelPanel.Project(new Vector3(0, _axisLDist + 0.1f, 0) * m) - new Vector3(8.0f, 8.0f, 0);
            ModelPanel.ScreenText["Z"] = ModelPanel.Project(new Vector3(0, 0, _axisLDist + 0.1f) * m) - new Vector3(8.0f, 8.0f, 0);
        }
        public unsafe void RenderRotationControl(TKContext context, Vector3 position, float radius, Vector3 rotate)
        {
            Matrix m = Matrix.TransformMatrix(new Vector3(radius), position.LookatAngles(CamLoc) * Maths._rad2degf, position);

            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            GLDisplayList sphere = context.GetCircleList();
            GLDisplayList circle = context.GetRingList();

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

        /// <summary>
        /// Gets world-point of specified mouse point projected onto the selected bone's local space if rotating or in world space if translating or scaling.
        /// Intersects the projected ray with the appropriate plane using the snap flags.
        /// </summary>
        public bool GetOrbPoint(Vector2 mousePoint, out Vector3 point)
        {
            MDL0BoneNode bone = SelectedBone;
            if (bone == null)
            {
                point = new Vector3();
                return false;
            }

            Vector3 lineStart = ModelPanel.UnProject(mousePoint._x, mousePoint._y, 0.0f);
            Vector3 lineEnd = ModelPanel.UnProject(mousePoint._x, mousePoint._y, 1.0f);
            Vector3 center = bone._frameMatrix.GetPoint();
            Vector3 camera = ModelPanel._camera.GetPoint();
            Vector3 normal = new Vector3();
            float radius = center.TrueDistance(camera) / _orbRadius * 0.1f;

            switch (_editType)
            {
                case TransformType.Rotation:

                    if (_snapX)
                        normal = (bone._frameMatrix * new Vector3(1.0f, 0.0f, 0.0f)).Normalize(center);
                    else if (_snapY)
                        normal = (bone._frameMatrix * new Vector3(0.0f, 1.0f, 0.0f)).Normalize(center);
                    else if (_snapZ)
                        normal = (bone._frameMatrix * new Vector3(0.0f, 0.0f, 1.0f)).Normalize(center);
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
                    else if (_editType == TransformType.Scale && _snapX && _snapY && _snapZ)
                        normal = camera.Normalize(center);

                    break;
            }

            return Maths.LinePlaneIntersect(lineStart, lineEnd, center, normal, out point);
        }

        private bool GetVertexOrbPoint(Vector2 mousePoint, Vector3 center, out Vector3 point)
        {
            Vector3 lineStart = ModelPanel.UnProject(mousePoint._x, mousePoint._y, 0.0f);
            Vector3 lineEnd = ModelPanel.UnProject(mousePoint._x, mousePoint._y, 1.0f);
            Vector3 camera = ModelPanel._camera.GetPoint();
            Vector3 normal = new Vector3();
            float radius = center.TrueDistance(camera) / _orbRadius * 0.1f;

            switch (_editType)
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
        private bool CompareVertexDistance(Vector3 point, ref Vertex3 match)
        {
            if (TargetModel._polyIndex != -1)
            {
                MDL0ObjectNode o = TargetModel._objList[TargetModel._polyIndex] as MDL0ObjectNode;
                if (o._render)
                    foreach (Vertex3 v in o._manager._vertices)
                    {
                        float t = v.WeightedPosition.TrueDistance(point);
                        if (Math.Abs(t) < 0.025f)
                        {
                            match = v;
                            return true;
                        }
                    }
                else
                    foreach (MDL0ObjectNode w in TargetModel._objList)
                        if (w._render)
                            foreach (Vertex3 v in w._manager._vertices)
                            {
                                float t = v.WeightedPosition.TrueDistance(point);
                                if (Math.Abs(t) < 0.025f)
                                {
                                    match = v;
                                    return true;
                                }
                            }
            }
            else
                foreach (MDL0ObjectNode o in TargetModel._objList)
                    if (o._render)
                        foreach (Vertex3 v in o._manager._vertices)
                        {
                            float t = v.WeightedPosition.TrueDistance(point);
                            if (Math.Abs(t) < 0.025f)
                            {
                                match = v;
                                return true;
                            }
                        }

            return false;
        }
        private bool CompareDistanceRecursive(MDL0BoneNode bone, Vector3 point, ref MDL0BoneNode match)
        {
            Vector3 center = bone._frameMatrix.GetPoint();
            float dist = center.TrueDistance(point);

            if (Math.Abs(dist - MDL0BoneNode._nodeRadius) < 0.01)
            {
                match = bone;
                return true;
            }

            foreach (MDL0BoneNode b in bone.Children)
                if (CompareDistanceRecursive(b, point, ref match))
                    return true;

            return false;
        }

        private unsafe void RenderOrb(MDL0BoneNode bone, GLDisplayList list)
        {
            Matrix m = Matrix.TransformMatrix(new Vector3(MDL0BoneNode._nodeRadius), new Vector3(), bone._frameMatrix.GetPoint());
            GL.PushMatrix();
            GL.MultMatrix((float*)&m);

            list.Call();
            GL.PopMatrix();
        }
    }
}
