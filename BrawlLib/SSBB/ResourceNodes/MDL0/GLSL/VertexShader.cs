using BrawlLib.Imaging;
using BrawlLib.OpenGL;
using BrawlLib.Wii.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    //Most of this was converted from Dolphin's C++ source
    public unsafe partial class MDL0ObjectNode : MDL0EntryNode
    {
        public const int SHADER_POSMTX_ATTRIB = 1;
        public const int SHADER_NORM1_ATTRIB = 6;
        public const int SHADER_NORM2_ATTRIB = 7;

        public const string I_POSNORMALMATRIX = "cpnmtx";
        public const string I_PROJECTION = "cproj";
        public const string I_MATERIALS = "cmtrl";
        public const string I_LIGHTS = "clights";
        public const string I_TEXMATRICES = "ctexmtx";
        public const string I_TRANSFORMMATRICES = "ctrmtx";
        public const string I_NORMALMATRICES = "cnmtx";
        public const string I_POSTTRANSFORMMATRICES = "cpostmtx";
        public const string I_DEPTHPARAMS = "cDepth";

        public const int C_POSNORMALMATRIX = 0;
        public const int C_PROJECTION = (C_POSNORMALMATRIX + 6);
        public const int C_MATERIALS = (C_PROJECTION + 4);
        public const int C_LIGHTS = (C_MATERIALS + 4);
        public const int C_TEXMATRICES = (C_LIGHTS + 40);
        public const int C_TRANSFORMMATRICES = (C_TEXMATRICES + 24);
        public const int C_NORMALMATRICES = (C_TRANSFORMMATRICES + 64);
        public const int C_POSTTRANSFORMMATRICES = (C_NORMALMATRICES + 32);
        public const int C_DEPTHPARAMS = (C_POSTTRANSFORMMATRICES + 64);
        public const int C_VENVCONST_END = (C_DEPTHPARAMS + 4);

        #region Vertex Shader

        public void GenerateVSOutputStruct()
        {
            w("struct VS_OUTPUT\n{\n");
            w("vec4 pos;\n");
            w("vec4 colors_0;\n");
            w("vec4 colors_1;\n");

            if (UsableMaterialNode.Children.Count < 7)
            {
                for (uint i = 0; i < UsableMaterialNode.Children.Count; i++)
                    w("vec3 tex{0};\n", i);
                w("vec4 clipPos;\n");
                w("vec4 Normal;\n");
            }
            else
            {
                // clip position is in w of first 4 texcoords
                //if(g_ActiveConfig.bEnablePixelLighting && ctx.bSupportsPixelLighting)
                //{
                for (int i = 0; i < 8; i++)
                    w("vec4 tex{0};\n", i);
                //}
                //else
                //{
                //    for (uint i = 0; i < MaterialNode.Children.Count; ++i)
                //        s += String.Format("  float{0} tex{1} : TEXCOORD{1};\n", i < 4 ? 4 : 3, i);
                //}
            }
            w("};\n");
        }

        #region Shader Helpers
        public string tempShader;
        public int tabs = 0;
        [Browsable(false)]
        public string Tabs { get { string t = ""; for (int i = 0; i < tabs; i++) t += "\t"; return t; } }
        public void w(string str, params object[] args)
        {
            if (args.Length == 0)
                tabs -= Helpers.FindCount(str, 0, '}');
            bool s = false;
            if (str.LastIndexOf("\n") == str.Length - 1)
            {
                str = str.Substring(0, str.Length - 1);
                s = true;
            }
            str = str.Replace("\n", "\n" + Tabs);
            if (s) str += "\n";
            tempShader += Tabs + (args != null && args.Length > 0 ? String.Format(str, args) : str);
            if (args.Length == 0)
                tabs += Helpers.FindCount(str, 0, '{');
        }

        public static string WriteRegister(string prefix, uint num)
        {
            return "";
        }

        public static string WriteBinding(uint num, TKContext ctx)
        {
            if (!ctx.bSupportsGLSLBinding)
                return "";
            return String.Format("layout(binding = {0}) ", num);
        }

        public static string WriteLocation(TKContext ctx)
        {
            //if (ctx.bSupportsGLSLUBO)
            //    return "";
            return "uniform ";
        }

        public void _assert_(bool arg)
        {
            if (arg != true)
                Console.WriteLine();
        }

        #endregion

        public string GenerateVertexShaderCode(TKContext ctx)
        {
            tempShader = "";
            tabs = 0;

            uint lightMask = 0;
            if (UsableMaterialNode.LightChannels > 0)
                lightMask |= (uint)UsableMaterialNode._chan1._color.Lights | (uint)UsableMaterialNode._chan1._alpha.Lights;
            if (UsableMaterialNode.LightChannels > 1)
                lightMask |= (uint)UsableMaterialNode._chan2._color.Lights | (uint)UsableMaterialNode._chan2._alpha.Lights;

            w("//Vertex Shader\n");

            // A few required defines and ones that will make our lives a lot easier
            if (ctx.bSupportsGLSLBinding || ctx.bSupportsGLSLUBO)
            {
                w("#version 330 compatibility\n");
                if (ctx.bSupportsGLSLBinding)
                    w("#extension GL_ARB_shading_language_420pack : enable\n");
                //if (ctx.bSupportsGLSLUBO)
                //    w("#extension GL_ARB_uniform_buffer_object : enable\n");
            }
            else
                w("#version 120\n");

            if (ctx.bSupportsGLSLATTRBind)
                w("#extension GL_ARB_explicit_attrib_location : enable\n");

            // Silly differences
            w("#define float2 vec2\n");
            w("#define float3 vec3\n");
            w("#define float4 vec4\n");

            // cg to glsl function translation
            w("#define frac(x) fract(x)\n");
            w("#define saturate(x) clamp(x, 0.0f, 1.0f)\n");
            w("#define lerp(x, y, z) mix(x, y, z)\n");

            //// uniforms
            //if (ctx.bSupportsGLSLUBO)
            //    w("layout(std140) uniform VSBlock\n{\n");

            //w("{0}float4 " + I_POSNORMALMATRIX + "[6];\n", WriteLocation(ctx));
            //w("{0}float4 " + I_PROJECTION + "[4];\n", WriteLocation(ctx));
            w("{0}float4 " + I_MATERIALS + "[4];\n", WriteLocation(ctx));
            w("{0}float4 " + I_LIGHTS + "[40];\n", WriteLocation(ctx));

            //Tex effect matrices
            w("{0}float4 " + I_TEXMATRICES + "[24];\n", WriteLocation(ctx)); // also using tex matrices

            w("{0}float4 " + I_TRANSFORMMATRICES + "[64];\n", WriteLocation(ctx));
            //w("{0}float4 " + I_NORMALMATRICES + "[32];\n", WriteLocation(ctx));
            //w("{0}float4 " + I_POSTTRANSFORMMATRICES + "[64];\n", WriteLocation(ctx));
            //w("{0}float4 " + I_DEPTHPARAMS + ";\n", WriteLocation(ctx));

            //if (ctx.bSupportsGLSLUBO) w("};\n");

            GenerateVSOutputStruct();

            if (_normalNode != null)
                w("float3 rawnorm0 = gl_Normal;\n");

            //if (ctx.bSupportsGLSLATTRBind)
            //{
            //    if (_vertexFormat.HasPosMatrix)
            //        Write("layout(location = {0}) ATTRIN float fposmtx;\n", SHADER_POSMTX_ATTRIB);
            //    //if (components & VB_HAS_NRM1)
            //    //    Write("layout(location = {0}) ATTRIN float3 rawnorm1;\n", SHADER_NORM1_ATTRIB);
            //    //if (components & VB_HAS_NRM2)
            //    //    Write("layout(location = {0}) ATTRIN float3 rawnorm2;\n", SHADER_NORM2_ATTRIB);
            //}
            //else
            //{
            //    if (_vertexFormat.HasPosMatrix)
            //        Write("ATTRIN float fposmtx; // ATTR{0},\n", SHADER_POSMTX_ATTRIB);
            //    //if (components & VB_HAS_NRM1)
            //    //    Write("ATTRIN float3 rawnorm1; // ATTR%d,\n", SHADER_NORM1_ATTRIB);
            //    //if (components & VB_HAS_NRM2)
            //    //    Write("ATTRIN float3 rawnorm2; // ATTR%d,\n", SHADER_NORM2_ATTRIB);
            //}

            if (_colorSet[0] != null)
                w("float4 color0 = gl_Color;\n");
            if (_colorSet[1] != null)
                w("float4 color1 = gl_SecondaryColor;\n");

            for (int i = 0; i < 8; i++)
            {
                bool hastexmtx = _arrayFlags.GetHasTexMatrix(i);
                if (_uvSet[i] != null || hastexmtx)
                    w("float{1} tex{0} = gl_MultiTexCoord{0}.xy{2};\n", i, hastexmtx ? 3 : 2, hastexmtx ? "z" : "");
            }

            w("float4 rawpos = gl_Vertex;\n");
            w("void main()\n{\n");
            w("VS_OUTPUT o;\n");

            // transforms
            //if (_vertexFormat.HasPosMatrix)
            //{
            //    w("int posmtx = int(fposmtx);\n");

            //    w("float4 pos = float4(dot(" + I_TRANSFORMMATRICES + "[posmtx], rawpos), dot(" + I_TRANSFORMMATRICES + "[posmtx+1], rawpos), dot(" + I_TRANSFORMMATRICES + "[posmtx+2], rawpos), 1);\n");

            //    if (_normalNode != null)
            //    {
            //        w("int normidx = posmtx >= 32 ? (posmtx-32) : posmtx;\n");
            //        w("float3 N0 = " + I_NORMALMATRICES + "[normidx].xyz, N1 = " + I_NORMALMATRICES + "[normidx+1].xyz, N2 = " + I_NORMALMATRICES + "[normidx+2].xyz;\n");
            //    }

            //    if (_normalNode != null)
            //        w("float3 _norm0 = normalize(float3(dot(N0, rawnorm0), dot(N1, rawnorm0), dot(N2, rawnorm0)));\n");
            //    //if (components & VB_HAS_NRM1)
            //    //    w("float3 _norm1 = float3(dot(N0, rawnorm1), dot(N1, rawnorm1), dot(N2, rawnorm1));\n");
            //    //if (components & VB_HAS_NRM2)
            //    //    w("float3 _norm2 = float3(dot(N0, rawnorm2), dot(N1, rawnorm2), dot(N2, rawnorm2));\n");
            //}
            //else
            //{
            //    w("float4 pos = float4(dot(" + I_POSNORMALMATRIX + "[0], rawpos), dot(" + I_POSNORMALMATRIX + "[1], rawpos), dot(" + I_POSNORMALMATRIX + "[2], rawpos), 1.0f);\n");
            //    if (_normalNode != null)
            //        w("float3 _norm0 = normalize(float3(dot(" + I_POSNORMALMATRIX + "[3].xyz, rawnorm0), dot(" + I_POSNORMALMATRIX + "[4].xyz, rawnorm0), dot(" + I_POSNORMALMATRIX + "[5].xyz, rawnorm0)));\n");
            //    //if (components & VB_HAS_NRM1)
            //    //    w("float3 _norm1 = float3(dot("+I_POSNORMALMATRIX+"[3].xyz, rawnorm1), dot("+I_POSNORMALMATRIX+"[4].xyz, rawnorm1), dot("+I_POSNORMALMATRIX+"[5].xyz, rawnorm1));\n");
            //    //if (components & VB_HAS_NRM2)
            //    //    w("float3 _norm2 = float3(dot("+I_POSNORMALMATRIX+"[3].xyz, rawnorm2), dot("+I_POSNORMALMATRIX+"[4].xyz, rawnorm2), dot("+I_POSNORMALMATRIX+"[5].xyz, rawnorm2));\n");
            //}

            w("float4 pos = gl_ModelViewProjectionMatrix * rawpos;\n");
            if (_normalNode != null)
                w("float3 _norm0 = rawnorm0;\n");

            if (_normalNode == null)
                w("float3 _norm0 = float3(0.0f, 0.0f, 0.0f);\n");

            w("o.pos = pos;\n");//float4(dot("+I_PROJECTION+"[0], pos), dot("+I_PROJECTION+"[1], pos), dot("+I_PROJECTION+"[2], pos), dot("+I_PROJECTION+"[3], pos));\n");

            w("float4 mat, lacc;\nfloat3 ldir, h;\nfloat dist, dist2, attn;\n");

            if (UsableMaterialNode.LightChannels == 0)
            {
                if (_colorSet[0] != null)
                    w("o.colors_0 = color0;\n");
                else
                    w("o.colors_0 = float4(1.0f, 1.0f, 1.0f, 1.0f);\n");
            }

            // TODO: This probably isn't necessary if pixel lighting is enabled.
            tempShader += GenerateLightingShader(I_MATERIALS, I_LIGHTS, "color", "o.colors_");

            if (UsableMaterialNode.LightChannels < 2)
            {
                if (_colorSet[1] != null)
                    w("o.colors_1 = color1;\n");
                else
                    w("o.colors_1 = o.colors_0;\n");
            }

            // transform texcoords
            w("float4 coord = float4(0.0f, 0.0f, 1.0f, 1.0f);\n");
            for (int i = 0; i < UsableMaterialNode.Children.Count; i++)
            {
                MDL0MaterialRefNode texgen = UsableMaterialNode.Children[i] as MDL0MaterialRefNode;

                w("{\n");
                w("//Texgen " + i + "\n");
                switch (texgen.Coordinates)
                {
                    case TexSourceRow.Geometry:
                        _assert_(texgen.InputForm == TexInputForm.AB11);
                        w("coord = rawpos;\n"); // pos.w is 1
                        break;
                    case TexSourceRow.Normals:
                        if (_normalNode != null)
                        {
                            _assert_(texgen.InputForm == TexInputForm.ABC1);
                            w("coord = float4(rawnorm0.xyz, 1.0f);\n");
                        }
                        break;
                    case TexSourceRow.Colors:
                        _assert_(texgen.Type == TexTexgenType.Color0 || texgen.Type == TexTexgenType.Color1);
                        break;
                    case TexSourceRow.BinormalsT:
                    //if (components & VB_HAS_NRM1)
                    //{
                    //      _assert_(texgen.InputForm == TexInputForm.ABC1);
                    //    Write("coord = float4(rawnorm1.xyz, 1.0f);\n");
                    //}
                    //break;
                    case TexSourceRow.BinormalsB:
                    //if (components & VB_HAS_NRM2)
                    //{
                    //    _assert_(texgen.InputForm == TexInputForm.ABC1);
                    //    Write("coord = float4(rawnorm2.xyz, 1.0f);\n");
                    //}
                    //break;
                    default:
                        _assert_(texgen.Coordinates <= TexSourceRow.TexCoord7);
                        int c = texgen.Coordinates - TexSourceRow.TexCoord0;
                        if (_uvSet[c] != null)
                            w("coord = float4(tex{0}.x, tex{0}.y, 1.0f, 1.0f);\n", c);
                        break;
                }

                // first transformation
                switch (texgen.Type)
                {
                    case TexTexgenType.EmbossMap: // calculate tex coords into bump map
                        //if (components & (VB_HAS_NRM1|VB_HAS_NRM2)) 
                        //{
                        //    // transform the light dir into tangent space
                        //    Write("ldir = normalize("+I_LIGHTS+"[{0} + 3].xyz - pos.xyz);\n", texgen.EmbossLight);
                        //    Write("o.tex{0}.xyz = o.tex{1}.xyz + float3(dot(ldir, _norm1), dot(ldir, _norm2), 0.0f);\n", i, texgen.EmbossSource);
                        //}
                        //else
                        //{
                        //    //_assert_(0); // should have normals
                        //    Write("o.tex{0}.xyz = o.tex{1}.xyz;\n", i, texgen.EmbossSource);
                        //}

                        break;
                    case TexTexgenType.Color0:
                    case TexTexgenType.Color1:
                        _assert_(texgen.Coordinates == TexSourceRow.Colors);
                        w("o.tex{0}.xyz = float3(o.colors_{1}.x, o.colors_{1}.y, 1);\n", i, ((int)texgen.Type - (int)TexTexgenType.Color0).ToString());
                        break;
                    case TexTexgenType.Regular:
                    default:
                        if (_arrayFlags.GetHasTexMatrix(i))
                        {
                            w("int tmp = int(tex{0}.z);\n", i);
                            if (texgen.Projection == TexProjection.STQ)
                                w("o.tex{0}.xyz = float3(dot(coord, " + I_TRANSFORMMATRICES + "[tmp]), dot(coord, " + I_TRANSFORMMATRICES + "[tmp+1]), dot(coord, " + I_TRANSFORMMATRICES + "[tmp+2]));\n", i);
                            else
                            {
                                w("o.tex{0}.xyz = float3(dot(coord, " + I_TRANSFORMMATRICES + "[tmp]), dot(coord, " + I_TRANSFORMMATRICES + "[tmp+1]), 1);\n", i);
                                w("o.tex{0}.z = 1.0f", 1);
                            }
                        }
                        else
                        {
                            if (texgen.Projection == TexProjection.STQ)
                                w("o.tex{0}.xyz = float3(dot(coord, " + I_TEXMATRICES + "[{1}]), dot(coord, " + I_TEXMATRICES + "[{2}]), dot(coord, " + I_TEXMATRICES + "[{3}]));\n", i, 3 * i, 3 * i + 1, 3 * i + 2);
                            else
                                w("o.tex{0}.xyz = float3(dot(coord, " + I_TEXMATRICES + "[{1}]), dot(coord, " + I_TEXMATRICES + "[{2}]), 1);\n", i, 3 * i, 3 * i + 1);
                        }
                        break;
                }

                //Dual tex trans always enabled?
                if (texgen.Type == TexTexgenType.Regular)
                {
                    // only works for regular tex gen types?
                    //int postidx = texgen.DualTexFlags.DualMtx;
                    //w("float4 P0 = " + I_POSTTRANSFORMMATRICES + "[{0}];\n"+
                    //  "float4 P1 = " + I_POSTTRANSFORMMATRICES + "[{1}];\n"+
                    //  "float4 P2 = " + I_POSTTRANSFORMMATRICES + "[{2}];\n",
                    //  postidx&0x3f, (postidx+1)&0x3f, (postidx+2)&0x3f);

                    if (texgen.Normalize)
                        w("o.tex{0}.xyz = normalize(o.tex{0}.xyz);\n", i);

                    // multiply by postmatrix
                    //w("o.tex{0}.xyz = float3(dot(P0.xyz, o.tex{0}.xyz) + P0.w, dot(P1.xyz, o.tex{0}.xyz) + P1.w, dot(P2.xyz, o.tex{0}.xyz) + P2.w);\n", i);
                }

                w("}\n");
            }

            // clipPos/w needs to be done in pixel shader, not here
            if (UsableMaterialNode.Children.Count < 7)
                w("o.clipPos = float4(pos.x,pos.y,o.pos.z,o.pos.w);\n");
            else
            {
                w("o.tex0.w = pos.x;\n");
                w("o.tex1.w = pos.y;\n");
                w("o.tex2.w = o.pos.z;\n");
                w("o.tex3.w = o.pos.w;\n");
            }

            //if(g_ActiveConfig.bEnablePixelLighting && ctx.bSupportsPixelLighting)
            //{
            if (UsableMaterialNode.Children.Count < 7)
                w("o.Normal = float4(_norm0.x,_norm0.y,_norm0.z,pos.z);\n");
            else
            {
                w("o.tex4.w = _norm0.x;\n");
                w("o.tex5.w = _norm0.y;\n");
                w("o.tex6.w = _norm0.z;\n");
                if (UsableMaterialNode.Children.Count < 8)
                    w("o.tex7 = pos.xyzz;\n");
                else
                    w("o.tex7.w = pos.z;\n");
            }
            if (_colorSet[0] != null)
                w("o.colors_0 = color0;\n");

            if (_colorSet[1] != null)
                w("o.colors_1 = color1;\n");
            //}

            //write the true depth value, if the game uses depth textures pixel shaders will override with the correct values
            //if not early z culling will improve speed

            // this results in a scale from -1..0 to -1..1 after perspective
            // divide
            //w("o.pos.z = o.pos.w + o.pos.z * 2.0f;\n");

            // Sonic Unleashed puts its final rendering at the near or
            // far plane of the viewing frustrum(actually box, they use
            // orthogonal projection for that), and we end up putting it
            // just beyond, and the rendering gets clipped away. (The
            // primitive gets dropped)
            //w("o.pos.z = o.pos.z * 1048575.0f/1048576.0f;\n");

            // the next steps of the OGL pipeline are:
            // (x_c,y_c,z_c,w_c) = o.pos  //switch to OGL spec terminology
            // clipping to -w_c <= (x_c,y_c,z_c) <= w_c
            // (x_d,y_d,z_d) = (x_c,y_c,z_c)/w_c//perspective divide
            // z_w = (f-n)/2*z_d + (n+f)/2
            // z_w now contains the value to go to the 0..1 depth buffer

            //trying to get the correct semantic while not using glDepthRange
            //seems to get rather complicated

            // Bit ugly here
            // Will look better when we bind uniforms in GLSL 1.3
            // clipPos/w needs to be done in pixel shader, not here

            if (UsableMaterialNode.Children.Count < 7)
            {
                for (uint i = 0; i < UsableMaterialNode.Children.Count; i++)
                    w("gl_TexCoord[{0}].xyz = o.tex{0};\n", i);
                w("gl_TexCoord[{0}] = o.clipPos;\n", UsableMaterialNode.Children.Count);
                //if(g_ActiveConfig.bEnablePixelLighting && ctx.bSupportsPixelLighting)
                w("gl_TexCoord[{0}] = o.Normal;\n", UsableMaterialNode.Children.Count + 1);
            }
            else
            {
                // clip position is in w of first 4 texcoords
                //if (g_ActiveConfig.bEnablePixelLighting && ctx.bSupportsPixelLighting)
                //{
                for (int i = 0; i < 8; i++)
                    w("gl_TexCoord[{0}] = o.tex{0};\n", i);
                //}
                //else
                //{
                //    for (unsigned int i = 0; i < xfregs.numTexGen.numTexGens; ++i)
                //        Write("  gl_TexCoord[%d]%s = o.tex%d;\n", i, i < 4 ? ".xyzw" : ".xyz" , i);
                //}
            }
            w("gl_FrontColor = o.colors_0;\n");
            w("gl_FrontSecondaryColor = o.colors_1;\n");
            w("gl_Position = o.pos;\n");
            w("}\n");

            return tempShader;
        }

        #endregion

        #region Light Shader
        public int temptabs = 0;
        [Browsable(false)]
        public string TempTabs { get { string t = ""; for (int i = 0; i < temptabs; i++) t += "\t"; return t; } }
        public void w(ref string output, string str, params object[] args)
        {
            temptabs = tabs;
            if (args.Length == 0)
                temptabs -= Helpers.FindCount(str, 0, '}');
            bool s = false;
            if (str.LastIndexOf("\n") == str.Length - 1)
            {
                str = str.Substring(0, str.Length - 1);
                s = true;
            }
            str = str.Replace("\n", "\n" + TempTabs);
            if (s) str += "\n";
            output += Tabs + TempTabs + (args != null && args.Length > 0 ? String.Format(str, args) : str);
            if (args.Length == 0)
                temptabs += Helpers.FindCount(str, 0, '{');
        }

        // coloralpha - 1 if color, 2 if alpha
        public string GenerateLightShader(int index, LightChannelControl chan, string lightsName, int coloralpha)
        {
            string s = "";

            string swizzle = "xyzw";
            if (coloralpha == 1) swizzle = "xyz";
            else if (coloralpha == 2) swizzle = "w";

            if (chan.Attenuation == GXAttnFn.None)
            {
                // atten disabled
                switch (chan.DiffuseFunction)
                {
                    case GXDiffuseFn.Disabled:
                        w(ref s, "lacc.{0} += {1}[{2}].{3};\n", swizzle, lightsName, index * 5, swizzle);
                        break;
                    case GXDiffuseFn.Enabled:
                    case GXDiffuseFn.Clamped:
                        w(ref s, "ldir = normalize({0}[{1} + 3].xyz - pos.xyz);\n", lightsName, index * 5);
                        w(ref s, "lacc.{0} += {1}dot(ldir, _norm0)) * {2}[{3}].{4};\n", swizzle, chan.DiffuseFunction != GXDiffuseFn.Enabled ? "max(0.0f," : "(", lightsName, index * 5, swizzle);
                        break;
                }
            }
            else
            {
                // spec and spot
                if (chan.Attenuation == GXAttnFn.Spotlight)
                {
                    // spot
                    w(ref s, "ldir = {0}[{1} + 3].xyz - pos.xyz;\n", lightsName, index * 5);
                    w(ref s, "dist2 = dot(ldir, ldir);\n" +
                            "dist = sqrt(dist2);\n" +
                            "ldir = ldir / dist;\n" +
                            "attn = max(0.0f, dot(ldir, {0}[{1} + 4].xyz));\n", lightsName, index * 5);
                    w(ref s, "attn = max(0.0f, dot({0}[{1} + 1].xyz, float3(1.0f, attn, attn*attn))) / dot({2}[{3} + 2].xyz, float3(1.0f,dist,dist2));\n", lightsName, index * 5, lightsName, index * 5);
                }
                if (chan.Attenuation == GXAttnFn.Specular)
                {
                    // specular
                    w(ref s, "ldir = normalize({0}[{1} + 3].xyz);\n", lightsName, index * 5);
                    w(ref s, "attn = (dot(_norm0, ldir) >= 0.0f) ? max(0.0f, dot(_norm0, {0}[{1} + 4].xyz)) : 0.0f;\n", lightsName, index * 5);
                    w(ref s, "attn = max(0.0f, dot({0}[{1} + 1].xyz, float3(1,attn,attn*attn))) / dot({2}[{3} + 2].xyz, float3(1,attn,attn*attn));\n", lightsName, index * 5, lightsName, index * 5);
                }

                switch (chan.DiffuseFunction)
                {
                    case GXDiffuseFn.Disabled:
                        w(ref s, "lacc.{0} += attn * {1}[{2}].{3};\n", swizzle, lightsName, index * 5, swizzle);
                        break;
                    case GXDiffuseFn.Enabled:
                    case GXDiffuseFn.Clamped:
                        w(ref s, "lacc.{0} += attn * {1}dot(ldir, _norm0)) * {2}[{3}].{4};\n",
                        swizzle,
                        chan.DiffuseFunction != GXDiffuseFn.Enabled ? "max(0.0f," : "(",
                        lightsName,
                        index * 5,
                        swizzle);
                        break;
                }
            }
            w(ref s, "\n");
            return s;
        }

        // vertex shader
        // lights/colors
        // materials name is I_MATERIALS in vs and I_PMATERIALS in ps
        // inColorName is color in vs and colors_ in ps
        // dest is o.colors_ in vs and colors_ in ps
        public string GenerateLightingShader(string materialsName, string lightsName, string inColorName, string dest)
        {
            string s = Tabs + "{\n";
            w(ref s, "//Lighting Section\n");
            for (uint j = 0; j < UsableMaterialNode.LightChannels; j++)
            {
                LightChannelControl color = j == 0 ? UsableMaterialNode._chan1._color : UsableMaterialNode._chan2._color;
                LightChannelControl alpha = j == 0 ? UsableMaterialNode._chan1._alpha : UsableMaterialNode._chan2._alpha;

                if (color.MaterialSource == GXColorSrc.Vertex)
                    if (_colorSet[j] != null)
                        w(ref s, "mat = {0}{1};\n", inColorName, j);
                    else if (_colorSet[0] != null)
                        w(ref s, "mat = {0}0;\n", inColorName);
                    else
                        w(ref s, "mat = vec4(1.0f, 1.0f, 1.0f, 1.0f);\n");
                else
                    w(ref s, "mat = {0}[{1}];\n", materialsName, j + 2);

                if (color.Enabled)
                    if (color.AmbientSource == GXColorSrc.Vertex)
                        if (_colorSet[j] != null)
                            w(ref s, "lacc = {0}{1};\n", inColorName, j);
                        else if (_colorSet[0] != null)
                            w(ref s, "lacc = {0}0;\n", inColorName);
                        else
                            w(ref s, "lacc = vec4(0.0f, 0.0f, 0.0f, 0.0f);\n");
                    else
                        w(ref s, "lacc = {0}[{1}];\n", materialsName, j);
                else
                    w(ref s, "lacc = vec4(1.0f, 1.0f, 1.0f, 1.0f);\n");

                // check if alpha is different
                if (alpha.MaterialSource != color.MaterialSource)
                    if (alpha.MaterialSource == GXColorSrc.Vertex)
                        if (_colorSet[j] != null)
                            w(ref s, "mat.w = {0}{1}.w;\n", inColorName, j);
                        else if (_colorSet[0] != null)
                            w(ref s, "mat.w = {0}0.w;\n", inColorName);
                        else
                            w(ref s, "mat.w = 1.0f;\n");
                    else
                        w(ref s, "mat.w = {0}[{1}].w;\n", materialsName, j + 2);

                if (alpha.Enabled)
                    if (alpha.AmbientSource == GXColorSrc.Vertex)
                        if (_colorSet[j] != null)
                            w(ref s, "lacc.w = {0}{1}.w;\n", inColorName, j);
                        else if (_colorSet[0] != null)
                            w(ref s, "lacc.w = {0}0.w;\n", inColorName);
                        else
                            w(ref s, "lacc.w = 0.0f;\n");
                    else
                        w(ref s, "lacc.w = {0}[{1}].w;\n", materialsName, j);
                else
                    w(ref s, "lacc.w = 1.0f;\n");

                if (color.Enabled && alpha.Enabled)
                {
                    //Both have lighting, test if they use the same lights
                    int mask = 0;
                    if (color.Lights == alpha.Lights)
                    {
                        mask = (int)color.Lights & (int)alpha.Lights;
                        if (mask != 0)
                        {
                            for (int i = 0; i < 8; i++)
                                if ((mask & (1 << i)) != 0)
                                    w(ref s, GenerateLightShader(i, color, lightsName, 3));
                        }
                    }

                    //No shared lights
                    for (int i = 0; i < 8; i++)
                    {
                        if (((mask & (1 << i)) == 0) && ((int)color.Lights & (1 << i)) != 0)
                            w(ref s, GenerateLightShader(i, color, lightsName, 1));
                        if (((mask & (1 << i)) == 0) && ((int)alpha.Lights & (1 << i)) != 0)
                            w(ref s, GenerateLightShader(i, alpha, lightsName, 2));
                    }
                }
                else if (color.Enabled || alpha.Enabled)
                {
                    //Lights are disabled on one channel so process only the active ones
                    LightChannelControl workingchannel = color.Enabled ? color : alpha;
                    int coloralpha = color.Enabled ? 1 : 2;
                    for (int i = 0; i < 8; i++)
                        if (((int)workingchannel.Lights & (1 << i)) != 0)
                            w(ref s, GenerateLightShader(i, workingchannel, lightsName, coloralpha));
                }
                w(ref s, "{0}{1} = mat * saturate(lacc);\n", dest, j);
            }
            w(ref s, "}\n");
            return s;
        }

        #endregion

        //public int _vsBlockLoc = 0, BufferUBO = 0, BufferIndex = 0;
        //public void SetVSBlock()
        //{
        //    GL.GenBuffers(1, out BufferUBO); // Generate the buffer
        //    GL.BindBuffer(BufferTarget.UniformBuffer, BufferUBO); // Bind the buffer for writing
        //    GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(sizeof(float) * 8), (IntPtr)(null), BufferUsageHint.DynamicDraw); // Request the memory to be allocated
        //    GL.BindBufferRange(BufferTarget.UniformBuffer, BufferIndex, BufferUBO, (IntPtr)0, (IntPtr)(sizeof(float) * 8)); // Bind the created Uniform Buffer to the Buffer Index
        //    _vsBlockLoc = GL.GetUniformBlockIndex(shaderProgramHandle, "VSBlock");
        //    GL.UniformBlockBinding(shaderProgramHandle, _vsBlockLoc, BufferIndex);
        //    GL.BindBuffer(BufferTarget.UniformBuffer, BufferUBO);
        //    GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)0, (IntPtr)(sizeof(float) * 8), ref vsData);
        //    GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        //}

        public void SetMultiPSConstant4fv(uint offset, float* f, uint count)
        {
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)(offset * sizeof(float) * 4), (IntPtr)(count * sizeof(float) * 4), (IntPtr)f);
        }

        public void SetMultiVSConstant4fv(uint offset, float* f, uint count)
        {
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)(offset * sizeof(float) * 4), (IntPtr)(count * sizeof(float) * 4), (IntPtr)f);
        }

        public int[] UniformLocations = new int[UniformNames.Length];
        public static readonly string[] UniformNames =
        {
	        // SAMPLERS
	        "samp0","samp1","samp2","samp3","samp4","samp5","samp6","samp7",
	        // PIXEL SHADER UNIFORMS
	        MDL0MaterialNode.I_COLORS,
	        MDL0MaterialNode.I_KCOLORS,
	        MDL0MaterialNode.I_ALPHA,
	        MDL0MaterialNode.I_TEXDIMS,
	        MDL0MaterialNode.I_ZBIAS,
	        MDL0MaterialNode.I_INDTEXSCALE,
	        MDL0MaterialNode.I_INDTEXMTX,
	        MDL0MaterialNode.I_FOG,
	        MDL0MaterialNode.I_PLIGHTS,
	        MDL0MaterialNode.I_PMATERIALS,
	        // VERTEX SHADER UNIFORMS
	        I_POSNORMALMATRIX,
	        I_PROJECTION,
	        I_MATERIALS,
	        I_LIGHTS,
	        I_TEXMATRICES,
	        I_TRANSFORMMATRICES,
	        I_NORMALMATRICES,
	        I_POSTTRANSFORMMATRICES,
	        I_DEPTHPARAMS,
        };

        public void SetProgramVariables(TKContext ctx)
        {
            if (ctx.bSupportsGLSLUBO)
            {
                GL.UniformBlockBinding(_programHandle, 0, 1);
                if (vertexShaderHandle != 0)
                    GL.UniformBlockBinding(_programHandle, 1, 2);
            }

            if (!ctx.bSupportsGLSLUBO)
                for (int a = 8; a < UniformNames.Length; a++)
                    UniformLocations[a] = GL.GetUniformLocation(_programHandle, UniformNames[a]);

            if (!ctx.bSupportsGLSLBinding)
                for (int a = 0; a < 8; a++)
                    if ((UniformLocations[a] = GL.GetUniformLocation(_programHandle, UniformNames[a])) != -1)
                        GL.Uniform1(UniformLocations[a], a);

            // Need to get some attribute locations
            if (vertexShaderHandle != 0 && !ctx.bSupportsGLSLATTRBind)
            {
                // We have no vertex Shader
                GL.BindAttribLocation(_programHandle, SHADER_NORM1_ATTRIB, "rawnorm1");
                GL.BindAttribLocation(_programHandle, SHADER_NORM2_ATTRIB, "rawnorm2");
                GL.BindAttribLocation(_programHandle, SHADER_POSMTX_ATTRIB, "fposmtx");
            }
        }

        public string _fragShaderSource;
        public int _fragShaderHandle;
        public int _programHandle = 0;
        /*
            w("{0}float4 " + I_POSNORMALMATRIX + "[6];\n", WriteLocation(ctx));
            w("{0}float4 " + I_PROJECTION + "[4];\n", WriteLocation(ctx));
            w("{0}float4 " + I_MATERIALS + "[4];\n", WriteLocation(ctx));
            w("{0}float4 " + I_LIGHTS + "[40];\n", WriteLocation(ctx));

            //Tex effect matrices
            w("{0}float4 " + I_TEXMATRICES + "[24];\n", WriteLocation(ctx)); // also using tex matrices
            
            w("{0}float4 " + I_TRANSFORMMATRICES + "[64];\n", WriteLocation(ctx));
            w("{0}float4 " + I_NORMALMATRICES + "[32];\n", WriteLocation(ctx));
            w("{0}float4 " + I_POSTTRANSFORMMATRICES + "[64];\n", WriteLocation(ctx));
	        w("{0}float4 " + I_DEPTHPARAMS + ";\n", WriteLocation(ctx));
         * 
            //24
            //16
            //16
            //160
            //96
            //256
            //128
            //256
            //4
        */
        public void SetLightUniforms(int programHandle)
        {
            int currUniform = GL.GetUniformLocation(programHandle, I_LIGHTS);
            if (currUniform > -1)
            {
                int frame = UsableMaterialNode.renderFrame;
                List<float> values = new List<float>();
                foreach (SCN0LightNode l in UsableMaterialNode._lightSet._lights)
                {
                    //float4 col; float4 cosatt; float4 distatt; float4 pos; float4 dir;

                    RGBAPixel p = (RGBAPixel)l.GetColor(frame, 0);
                    values.Add((float)p.R * RGBAPixel.ColorFactor);
                    values.Add((float)p.G * RGBAPixel.ColorFactor);
                    values.Add((float)p.B * RGBAPixel.ColorFactor);
                    values.Add((float)p.A * RGBAPixel.ColorFactor);
                    Vector3 v = l.GetLightSpot(frame);
                    values.Add(v._x);
                    values.Add(v._y);
                    values.Add(v._z);
                    values.Add(1.0f);
                    v = l.GetLightDistAttn(frame);
                    values.Add(v._x);
                    values.Add(v._y);
                    values.Add(v._z);
                    values.Add(1.0f);
                    v = l.GetStart(frame);
                    values.Add(v._x);
                    values.Add(v._y);
                    values.Add(v._z);
                    values.Add(1.0f);
                    Vector3 v2 = l.GetEnd(frame);
                    Vector3 dir = Matrix.AxisAngleMatrix(v, v2).GetAngles();
                    values.Add(dir._x);
                    values.Add(dir._y);
                    values.Add(dir._z);
                    values.Add(1.0f);
                }

                GL.Uniform4(currUniform, 40, values.ToArray());
            }
        }
        public void SetUniforms(int programHandle, ModelPanel panel)
        {
            int currUniform = -1;

            Matrix projection = panel._projectionMatrix;
            Matrix modelview = panel._camera._matrix;

            currUniform = GL.GetUniformLocation(programHandle, "projection_matrix");
            GL.UniformMatrix4(currUniform, 16, false, (float*)&projection);
            currUniform = GL.GetUniformLocation(programHandle, "modelview_matrix");
            GL.UniformMatrix4(currUniform, 16, false, (float*)&modelview);

            //currUniform = GL.GetUniformLocation(programHandle, I_POSNORMALMATRIX);
            //if (currUniform > -1) GL.Uniform4(currUniform, 6, new float[] 
            //{

            //});
            //currUniform = GL.GetUniformLocation(programHandle, I_PROJECTION);
            //if (currUniform > -1) GL.Uniform4(currUniform, 4, new float[] 
            //{

            //});
            //currUniform = GL.GetUniformLocation(programHandle, I_MATERIALS);
            //if (currUniform > -1) GL.Uniform4(currUniform, 4, new float[] 
            //{
            //    UsableMaterialNode.C1AmbientColor.R * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C1AmbientColor.G * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C1AmbientColor.B * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C1AmbientColor.A * RGBAPixel.ColorFactor,

            //    UsableMaterialNode.C2AmbientColor.R * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C2AmbientColor.G * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C2AmbientColor.B * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C2AmbientColor.A * RGBAPixel.ColorFactor,

            //    UsableMaterialNode.C1MaterialColor.R * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C1MaterialColor.G * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C1MaterialColor.B * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C1MaterialColor.A * RGBAPixel.ColorFactor,

            //    UsableMaterialNode.C2MaterialColor.R * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C2MaterialColor.G * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C2MaterialColor.B * RGBAPixel.ColorFactor,
            //    UsableMaterialNode.C2MaterialColor.A * RGBAPixel.ColorFactor,
            //});
            //currUniform = GL.GetUniformLocation(programHandle, I_TEXMATRICES);
            //if (currUniform > -1)
            //{
            //    List<float> mtxValues = new List<float>();
            //    int i = 0;
            //    foreach (MDL0MaterialRefNode m in UsableMaterialNode.Children)
            //    {
            //        for (int x = 0; x < 12; x++)
            //            mtxValues.Add(m.EffectMatrix[x]);
            //        i++;
            //    }
            //    while (i < 8)
            //    {
            //        for (int x = 0; x < 12; x++)
            //            mtxValues.Add(Matrix43.Identity[x]);
            //        i++;
            //    }
            //    if (mtxValues.Count != 96)
            //        Console.WriteLine();
            //    GL.Uniform4(currUniform, 24, mtxValues.ToArray());
            //}
            //currUniform = GL.GetUniformLocation(programHandle, I_TRANSFORMMATRICES);
            //if (currUniform > -1) GL.Uniform4(currUniform, 64, new float[] 
            //{

            //});
            //currUniform = GL.GetUniformLocation(programHandle, I_NORMALMATRICES);
            //if (currUniform > -1) GL.Uniform4(currUniform, 32, new float[] 
            //{

            //});
            //currUniform = GL.GetUniformLocation(programHandle, I_DEPTHPARAMS);
            //if (currUniform > -1) GL.Uniform4(currUniform, 1, new float[] 
            //{

            //});
        }
    }
}
