using BrawlLib.OpenGL;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrawlLib.Modeling
{
    public interface IModel : IRenderedObject
    {
        InfluenceManager Influences { get; }
        IBoneNode[] BoneCache { get; }
        IBoneNode[] RootBones { get; }
        bool IsTargetModel { get; set; }
        
        void ResetToBindState();
        void ApplyCHR(CHR0Node node, float index);
        void ApplySRT(SRT0Node node, float index);
        void ApplySHP(SHP0Node node, float index);
        void ApplyPAT(PAT0Node node, float index);
        void ApplyVIS(VIS0Node node, float index);
        void ApplyCLR(CLR0Node node, float index);
        void SetSCN0(SCN0Node node);
        void SetSCN0Frame(float index);
        
        void RenderVertices(bool depthPass, IBoneNode weightTarget);
        void RenderNormals();

        int SelectedObjectIndex { get; set; }
        IObject[] Objects { get; }
    }

    public class ModelRenderAttributes
    {
        public bool _renderPolygons = true;
        public bool _renderWireframe = false;
        public bool _renderBones = false;
        public bool _renderVertices = false;
        public bool _renderNormals = false;
        public bool _dontRenderOffscreen = false;
        public bool _renderBox = false;
        public bool _applyBillboardBones = true;
    }
}
