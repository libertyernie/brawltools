using BrawlLib.Imaging;
using BrawlLib.OpenGL;
using BrawlLib.Wii.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    //Most of this was converted from Dolphin's C++ source
    public unsafe partial class MDL0MaterialNode : MDL0EntryNode
    {
        #region Uniforms

        /*
            w("{0}vec4 " + I_COLORS + "[4];\n", WriteLocation(ctx));
		    w("{0}vec4 " + I_KCOLORS + "[4];\n", WriteLocation(ctx));
		    w("{0}vec4 " + I_ALPHA + "[1];\n", WriteLocation(ctx));
		    w("{0}vec4 " + I_TEXDIMS + "[8];\n", WriteLocation(ctx));
		    w("{0}vec4 " + I_ZBIAS + "[2];\n", WriteLocation(ctx));
		    w("{0}vec4 " + I_INDTEXSCALE + "[2];\n", WriteLocation(ctx));
		    w("{0}vec4 " + I_INDTEXMTX + "[6];\n", WriteLocation(ctx));
		    w("{0}vec4 " + I_FOG + "[3];\n", WriteLocation(ctx));
         */

        public void SetUniforms(int programHandle)
        {
            int currUniform = -1;
            currUniform = GL.GetUniformLocation(programHandle, I_COLORS);
            if (currUniform > -1) GL.Uniform4(currUniform, 3, new float[] 
            {
                c1.R * GXColorS10.ColorFactor, c1.G * GXColorS10.ColorFactor, c1.B * GXColorS10.ColorFactor, c1.A * GXColorS10.ColorFactor,
                c2.R * GXColorS10.ColorFactor, c2.G * GXColorS10.ColorFactor, c2.B * GXColorS10.ColorFactor, c2.A * GXColorS10.ColorFactor,
                c3.R * GXColorS10.ColorFactor, c3.G * GXColorS10.ColorFactor, c3.B * GXColorS10.ColorFactor, c3.A * GXColorS10.ColorFactor,
            });
            currUniform = GL.GetUniformLocation(programHandle, I_KCOLORS);
            if (currUniform > -1) GL.Uniform4(currUniform, 4, new float[] 
            {
                k1.R * GXColorS10.ColorFactor, k1.G * GXColorS10.ColorFactor, k1.B * GXColorS10.ColorFactor, k1.A * GXColorS10.ColorFactor,
                k2.R * GXColorS10.ColorFactor, k2.G * GXColorS10.ColorFactor, k2.B * GXColorS10.ColorFactor, k2.A * GXColorS10.ColorFactor,
                k3.R * GXColorS10.ColorFactor, k3.G * GXColorS10.ColorFactor, k3.B * GXColorS10.ColorFactor, k3.A * GXColorS10.ColorFactor,
                k4.R * GXColorS10.ColorFactor, k4.G * GXColorS10.ColorFactor, k4.B * GXColorS10.ColorFactor, k4.A * GXColorS10.ColorFactor,
            });
            currUniform = GL.GetUniformLocation(programHandle, I_ALPHA);
            if (currUniform > -1) GL.Uniform4(currUniform, 1, new float[] 
            {
                _alphaFunc.ref0 * RGBAPixel.ColorFactor,
                _alphaFunc.ref1 * RGBAPixel.ColorFactor,
                0,
                _constantAlpha.Value * RGBAPixel.ColorFactor,
            });
            //currUniform = GL.GetUniformLocation(programHandle, I_TEXDIMS);
            //if (currUniform > -1) GL.Uniform4(currUniform, 8, new float[] 
            //{

            //});
            currUniform = GL.GetUniformLocation(programHandle, I_ZBIAS);
            if (currUniform > -1) GL.Uniform4(currUniform, 2, new float[] 
            {
                
            });
            currUniform = GL.GetUniformLocation(programHandle, I_INDTEXSCALE);
            if (currUniform > -1) GL.Uniform4(currUniform, 2, new float[] 
            {
                
            });
            currUniform = GL.GetUniformLocation(programHandle, I_INDTEXMTX);
            if (currUniform > -1) GL.Uniform4(currUniform, 6, new float[] 
            {
                
            });
            currUniform = GL.GetUniformLocation(programHandle, I_FOG);
            if (currUniform > -1)
            {
                List<float> values = new List<float>();
                if (_fog != null && _animFrame < _fog._colors.Count)
                {
                    ARGBPixel p = _fog._colors[_animFrame];
                    values.Add(p.R * RGBAPixel.ColorFactor);
                    values.Add(p.G * RGBAPixel.ColorFactor);
                    values.Add(p.B * RGBAPixel.ColorFactor);
                    values.Add(p.A * RGBAPixel.ColorFactor);
                }
                else
                {
                    values.Add(0);
                    values.Add(0);
                    values.Add(0);
                    values.Add(0);
                }
                GL.Uniform4(currUniform, 3, values.ToArray());
            }
        }

        #endregion

        #region Shader Helpers

        public AlphaPretest PretestAlpha()
        {
            AlphaPretest preTest = AlphaPretest.Undefined;
            switch (_alphaFunc.Logic)
            {
                case AlphaOp.And:
                    if (_alphaFunc.Comp0 == AlphaCompare.Always && _alphaFunc.Comp1 == AlphaCompare.Always) preTest = AlphaPretest.AlwaysPass;
                    if (_alphaFunc.Comp0 == AlphaCompare.Never || _alphaFunc.Comp1 == AlphaCompare.Never) preTest = AlphaPretest.AlwaysFail;
                    break;
                case AlphaOp.Or:
                    if (_alphaFunc.Comp0 == AlphaCompare.Always || _alphaFunc.Comp1 == AlphaCompare.Always) preTest = AlphaPretest.AlwaysPass;
                    if (_alphaFunc.Comp0 == AlphaCompare.Never && _alphaFunc.Comp1 == AlphaCompare.Never) preTest = AlphaPretest.AlwaysFail;
                    break;
                case AlphaOp.ExclusiveOr:
                    if ((_alphaFunc.Comp0 == AlphaCompare.Always && _alphaFunc.Comp1 == AlphaCompare.Never) || (_alphaFunc.Comp0 == AlphaCompare.Never && _alphaFunc.Comp1 == AlphaCompare.Always))
                        preTest = AlphaPretest.AlwaysPass;
                    if ((_alphaFunc.Comp0 == AlphaCompare.Always && _alphaFunc.Comp1 == AlphaCompare.Always) || (_alphaFunc.Comp0 == AlphaCompare.Never && _alphaFunc.Comp1 == AlphaCompare.Never))
                        preTest = AlphaPretest.AlwaysFail;
                    break;
                case AlphaOp.InverseExclusiveOr:
                    if ((_alphaFunc.Comp0 == AlphaCompare.Always && _alphaFunc.Comp1 == AlphaCompare.Never) || (_alphaFunc.Comp0 == AlphaCompare.Never && _alphaFunc.Comp1 == AlphaCompare.Always))
                        preTest = AlphaPretest.AlwaysFail;
                    if ((_alphaFunc.Comp0 == AlphaCompare.Always && _alphaFunc.Comp1 == AlphaCompare.Always) || (_alphaFunc.Comp0 == AlphaCompare.Never && _alphaFunc.Comp1 == AlphaCompare.Never))
                        preTest = AlphaPretest.AlwaysPass;
                    break;
            }
            return preTest;
        }

        public string tempShader;
        public int tabs = 0;
        [Browsable(false)]
        public string Tabs { get { string t = ""; for (int i = 0; i < tabs; i++) t += "\t"; return t; } }
        public void w(string str, params object[] args)
        {
            if (args.Length == 0)
                tabs -= Helpers.FindCount(str, 0, '}');
            bool s = false;
            int r = str.LastIndexOf("\n");
            if (r == str.Length - 1)
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

        public void _assert_(bool arg)
        {
            if (arg != true)
                Console.WriteLine();
        }

        #endregion

        public enum PSGRENDER_MODE
        {
            PSGRENDER_NORMAL, // Render normally, without destination alpha
            PSGRENDER_DSTALPHA_ALPHA_PASS, // Render normally first, then render again for alpha
            PSGRENDER_DSTALPHA_DUAL_SOURCE_BLEND, // Use dual-source blending
            PSGRENDER_ZCOMPLOCK //Render to Depth Channel only with no depth dextures enabled
        };

        public enum AlphaPretest
        {
            Undefined,
            AlwaysPass,
            AlwaysFail
        }

        public bool DepthTextureEnable;
        public string GeneratePixelShaderCode(MDL0ObjectNode Object, PSGRENDER_MODE PSGRenderMode, TKContext ctx)
        {
            tempShader = "";

            int numStages = ShaderNode.Children.Count;
            int numTexgen = Children.Count;

            w("//Pixel Shader for TEV stages\n");
            w("//{0} TEV stages, {1} texgens, {2} IND stages\n", numStages, numTexgen, IndirectShaderStages);

            int nIndirectStagesUsed = 0;
            if (IndirectShaderStages > 0)
                for (int i = 0; i < numStages; ++i)
                {
                    TEVStage stage = ShaderNode.Children[i] as TEVStage;
                    if (stage.IndirectActive && stage.bt < IndirectShaderStages)
                        nIndirectStagesUsed |= 1 << stage.bt;
                }

            DepthTextureEnable = (/*bpmem.ztex2.op != ZTEXTURE_DISABLE && */!ZCompareLoc && EnableDepthTest && EnableDepthUpdate)/* || g_ActiveConfig.bEnablePerPixelDepth*/;

            //A few required defines and ones that will make our lives a lot easier
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

            w("#define saturate(x) clamp(x, 0.0f, 1.0f)\n");
            w("#define lerp(x, y, z) mix(x, y, z)\n");

            w("float fmod(float x, float y)\n");
            w("{\n");
            w("float z = fract(abs(x / y)) * abs(y);\n");
            w("return (x < 0) ? -z : z;\n");
            w("}\n");

            for (uint i = 0; i < 8; i++)
                w("{0}uniform sampler2D samp{1};\n", WriteBinding(i, ctx), i);

            w("\n");

            w("uniform vec4 " + I_COLORS + "[4];\n");
            w("uniform vec4 " + I_KCOLORS + "[4];\n");
            w("uniform vec4 " + I_ALPHA + ";\n");
            w("uniform vec4 " + I_TEXDIMS + "[8];\n");
            w("uniform vec4 " + I_ZBIAS + "[2];\n");
            w("uniform vec4 " + I_INDTEXSCALE + "[2];\n");
            w("uniform vec4 " + I_INDTEXMTX + "[6];\n");
            w("uniform vec4 " + I_FOG + "[3];\n");

            // Compiler will optimize these out by itself.
            w("uniform vec4 " + I_PLIGHTS + "[40];\n");
            w("uniform vec4 " + I_PMATERIALS + "[4];\n");

            w("vec4 ocol0;\n");

            if (DepthTextureEnable)
                w("float depth;\n");

            w("vec4 rawpos = gl_FragCoord;\n");

            w("vec4 colors_0 = gl_Color;\n");
            w("vec4 colors_1 = gl_SecondaryColor;\n");

            if (numTexgen < 7)
            {
                for (int i = 0; i < numTexgen; i++)
                    w("vec3 uv{0} = gl_TexCoord[{0}].xyz;\n", i);
                w("vec4 clipPos = gl_TexCoord[{0}];\n", numTexgen);
                w("vec4 Normal = gl_TexCoord[{0}];\n", numTexgen + 1);
            }
            else
                for (int i = 0; i < 8; ++i)
                    w("vec4 uv{0} = gl_TexCoord[{0}];\n", i);

            w("void main()\n{\n");

            AlphaPretest Pretest = PretestAlpha();
            //if (PSGRenderMode == PSGRENDER_MODE.PSGRENDER_DSTALPHA_ALPHA_PASS && !DepthTextureEnable && Pretest >= 0)
            //{
            //    if (Pretest == AlphaPretest.AlwaysFail)
            //    {
            //        w("ocol0 = vec4(0);\n");
            //        w("discard;\n");

            //        if(PSGRenderMode != PSGRENDER_MODE.PSGRENDER_DSTALPHA_DUAL_SOURCE_BLEND)
            //            w("gl_FragColor = ocol0;\n");

            //        w("return;\n");
            //    }
            //    else
            //        w("ocol0 = " + I_ALPHA + ".aaaa;\n");

            //    w("}\n");

            //    return tempShader;
            //}

            w("vec4 c0 = " + I_COLORS + "[1], c1 = " + I_COLORS + "[2], c2 = " + I_COLORS + "[3], prev = vec4(0.0f, 0.0f, 0.0f, 0.0f), textemp = vec4(0.0f, 0.0f, 0.0f, 0.0f), rastemp = vec4(0.0f, 0.0f, 0.0f, 0.0f), konsttemp = vec4(0.0f, 0.0f, 0.0f, 0.0f);\n");
            w("vec3 comp16 = vec3(1.0f, 255.0f, 0.0f), comp24 = vec3(1.0f, 255.0f, 255.0f*255.0f);\n");
            w("vec4 alphabump = vec4(0.0f,0.0f,0.0f,0.0f);\n");
            w("vec3 tevcoord = vec3(0.0f, 0.0f, 0.0f);\n");
            w("vec2 wrappedcoord=vec2(0.0f,0.0f), tempcoord=vec2(0.0f,0.0f);\n");
            w("vec4 cc0 = vec4(0.0f,0.0f,0.0f,0.0f), cc1 = vec4(0.0f,0.0f,0.0f,0.0f);\n");
            w("vec4 cc2 = vec4(0.0f,0.0f,0.0f,0.0f), cprev = vec4(0.0f,0.0f,0.0f,0.0f);\n");
            w("vec4 crastemp=vec4(0.0f,0.0f,0.0f,0.0f), ckonsttemp = vec4(0.0f,0.0f,0.0f,0.0f);\n\n");

            if (Children.Count < 7)
            {
                w("vec3 _norm0 = normalize(Normal.xyz);\n\n");
                w("vec3 pos = vec3(clipPos.x,clipPos.y,Normal.w);\n");
            }
            else
            {
                w("vec3 _norm0 = normalize(vec3(uv4.w,uv5.w,uv6.w));\n\n");
                w("vec3 pos = vec3(uv0.w,uv1.w,uv7.w);\n");
            }

            w("vec4 mat, lacc;\nvec3 ldir, h;\nfloat dist, dist2, attn;\n");

            Object.tabs = tabs;
            tempShader += Object.GenerateLightingShader(I_PMATERIALS, I_PLIGHTS, "colors_", "colors_");

            //if (numTexgen < 7)
            //    w("clipPos = vec4(rawpos.x, rawpos.y, clipPos.z, clipPos.w);\n");
            //else
            //    w("vec4 clipPos = vec4(rawpos.x, rawpos.y, uv2.w, uv3.w);\n");

            // HACK to handle cases where the tex gen is not enabled
            if (numTexgen == 0)
                w("vec3 uv0 = vec3(0.0f, 0.0f, 0.0f);\n");
            else
            {
                for (int i = 0; i < numTexgen; ++i)
                {
                    // optional perspective divides
                    if (((MDL0MaterialRefNode)Children[i]).Projection == TexProjection.STQ)
                    {
                        w("if (uv{0}.z != 0.0f)\n", i);
                        w("    uv{0}.xy = uv{0}.xy / uv{0}.z;\n", i);
                    }

                    //w("uv{0}.xy = uv{0}.xy * "+I_TEXDIMS+"[{0}].zw;\n", i);
                }
            }

            for (int i = 0; i < IndirectShaderStages; i++)
            {
                if ((nIndirectStagesUsed & (1 << i)) != 0)
                {
                    uint texcoord = (ShaderNode._swapBlock._Value16.Value >> ((i + 1) * 3) & 7);

                    if (texcoord < numTexgen)
                        w("tempcoord = uv{0}.xy * " + I_INDTEXSCALE + "[{1}].{2};\n", texcoord, i / 2, (i & 1) != 0 ? "zw" : "xy");
                    else
                        w("tempcoord = vec2(0.0f, 0.0f);\n");

                    SampleTexture(String.Format("vec3 indtex{0}", i), "tempcoord", "abg", (ShaderNode._swapBlock._Value16.Value >> (i * 3) & 7));
                }
            }

            foreach (TEVStage stage in ShaderNode.Children)
                if (stage.Index < ActiveShaderStages)
                    w(stage.Write(this, Object));
                else break;

            if (numStages > 0)
            {
                w("prev.rgb = {0};\n", tevCOutputTable[(int)((TEVStage)ShaderNode.Children[numStages - 1]).ColorRegister]);
                w("prev.a = {0};\n", tevAOutputTable[(int)((TEVStage)ShaderNode.Children[numStages - 1]).AlphaRegister]);
            }

            // emulation of unsigned 8 overflow when casting
            //w("prev = fract(4.0f + prev * (255.0f/256.0f)) * (256.0f/255.0f);\n");

            if (Pretest == AlphaPretest.AlwaysFail)
            {
                w("ocol0 = vec4(0.0f);\n");

                //if (DepthTextureEnable)
                //    w("depth = 1.0f;\n");

                //if (PSGRenderMode == PSGRENDER_MODE.PSGRENDER_DSTALPHA_DUAL_SOURCE_BLEND)
                //    w("ocol1 = vec4(0.0f);\n");
                //else
                w("gl_FragColor = ocol0;\n");

                if (DepthTextureEnable)
                    w("gl_FragDepth = depth;\n");

                w("discard;\n");

                w("return;\n");
            }
            else
            {
                if (Pretest != AlphaPretest.AlwaysPass)
                    WriteAlphaTest(PSGRenderMode);

                //if (_fog != null && (_fog.Type != 0) || DepthTextureEnable)
                //{
                //    // the screen space depth value = far z + (clip z / clip w) * z range
                //    w("float zCoord = " + I_ZBIAS + "[1].x + (clipPos.z / clipPos.w) * " + I_ZBIAS + "[1].y;\n");
                //}

                //if (DepthTextureEnable)
                //{
                //    // use the texture input of the last texture stage (textemp), hopefully this has been read and is in correct format...
                //    if (/*bpmem.ztex2.op != ZTEXTURE_DISABLE && */!ZCompLoc && EnableDepthTest && EnableDepthUpdate)
                //    {
                //        //if (bpmem.ztex2.op == ZTEXTURE_ADD)
                //        //    Write("zCoord = dot("+I_ZBIAS+"[0].xyzw, textemp.xyzw) + "+I_ZBIAS+"[1].w + zCoord;\n");
                //        //else
                //            w("zCoord = dot(" + I_ZBIAS + "[0].xyzw, textemp.xyzw) + " + I_ZBIAS + "[1].w;\n");

                //        // scale to make result from frac correct
                //        w("zCoord = zCoord * (16777215.0f/16777216.0f);\n");
                //        w("zCoord = fract(zCoord);\n");
                //        w("zCoord = zCoord * (16777216.0f/16777215.0f);\n");
                //    }
                //    w("depth = zCoord;\n");
                //}

                //if (PSGRenderMode == PSGRENDER_MODE.PSGRENDER_DSTALPHA_ALPHA_PASS)
                //    w("ocol0 = vec4(prev.rgb, " + I_ALPHA + ".a);\n");
                //else
                {
                    WriteFog();
                    w("ocol0 = prev;\n");
                }

                //if (PSGRenderMode == PSGRENDER_MODE.PSGRENDER_DSTALPHA_DUAL_SOURCE_BLEND)
                //{
                //    w("ocol1 = ocol0;\n");
                //    w("ocol0.a = " + I_ALPHA + ".a;\n");
                //}

                //if (DepthTextureEnable)
                //    w("gl_FragDepth = depth;\n");
                //if (PSGRenderMode != PSGRENDER_MODE.PSGRENDER_DSTALPHA_DUAL_SOURCE_BLEND)
                w("gl_FragColor = ocol0;\n");

            }
            w("}\n");

            return tempShader;
        }

        public void SampleTexture(string destination, string texcoords, string texswap, uint texmap)
        {
            tempShader += String.Format("{0}=texture2D(samp{1},{2}.xy"/* + " * " + I_TEXDIMS + "[{3}].xy" */+ ").{4};\n", destination, texmap, texcoords, texmap, texswap);
        }

        public void WriteFog()
        {
            if (_fog == null || _fog.Type == FogType.None) return; //no Fog

            if ((int)_fog.Type < 8) // perspective ze = A/(B - (Zs >> B_SHF)
                w("float ze = " + I_FOG + "[1].x / (" + I_FOG + "[1].y - (zCoord / " + I_FOG + "[1].w));\n");
            else // orthographic ze = a*Zs    (here, no B_SHF)
                w("float ze = " + I_FOG + "[1].x * zCoord;\n");

            // x_adjust = sqrt((x-center)^2 + k^2)/k
            // ze *= x_adjust
            //this is completely theorical as the real hardware seems to use a table instead of calculate the values.
            //if(bpmem.fogRange.Base.Enabled)
            //{
            w("float x_adjust = (2.0f * (clipPos.x / " + I_FOG + "[2].y)) - 1.0f - " + I_FOG + "[2].x;\n");
            w("x_adjust = sqrt(x_adjust * x_adjust + " + I_FOG + "[2].z * " + I_FOG + "[2].z) / " + I_FOG + "[2].z;\n");
            w("ze *= x_adjust;\n");
            //}

            w("float fog = saturate(ze - " + I_FOG + "[1].z);\n");

            if ((int)_fog.Type > 3)
                w(tevFogFuncsTable[(int)_fog.Type]);
            else
            {
                //if ((int)_fog.Type != 2)
                //WARN_LOG(VIDEO, "Unknown Fog Type! %08x", bpmem.fog.c_proj_fsel.fsel);
            }

            w("prev.rgb = lerp(prev.rgb, " + I_FOG + "[0].rgb, fog);\n");
        }

        public void WriteAlphaTest(PSGRENDER_MODE mode)
        {
            // using discard then return works the same in cg and dx9 but not in dx11
            w("if(!(");

            int comp0index = (int)_alphaFunc.Comp0 % 8;
            int comp1index = (int)_alphaFunc.Comp1 % 8;

            w(tevAlphaFuncsTable[comp0index] + "{1}" + tevAlphaFuncsTable[comp1index].Replace("{0}", "{2}"), alphaRef[0], tevAlphaFunclogicTable[(int)_alphaFunc.Logic % 4], alphaRef[1]);//lookup the first component from the alpha function table
            w("))\n{\n");

            w("ocol0 = vec4(0.0, 0.0, 0.0, 0.0);\n");
            if (mode == PSGRENDER_MODE.PSGRENDER_DSTALPHA_DUAL_SOURCE_BLEND)
                w("ocol1 = vec4(0.0, 0.0, 0.0, 0.0);\n");
            if (DepthTextureEnable)
                w("depth = 1.f;\n");

            // HAXX: zcomploc is a way to control whether depth test is done before
            // or after texturing and alpha test. PC GPU does depth test before texturing ONLY if depth value is
            // not updated during shader execution.
            // We implement "depth test before texturing" by discarding the fragment
            // when the alpha test fail. This is not a correct implementation because
            // even if the depth test fails the fragment could be alpha blended.
            // this implemnetation is a trick to  keep speed.
            // the correct, but slow, way to implement a correct zComploc is :
            // 1 - if zcomplock is enebled make a first pass, with color channel write disabled updating only
            // depth channel.
            // 2 - in the next pass disable depth chanel update, but proccess the color data normally
            // this way is the only CORRECT way to emulate perfectly the zcomplock behaviour
            if (!(ZCompareLoc && EnableDepthUpdate))
            {
                w("discard;\n");
                w("return;\n");
            }

            w("}\n");
        }

        #region Table Variables

        public static readonly string[] tevAlphaFuncsTable =
        {
            "(false)",                                      //AlphaCompare.Never 0
            "(prev.a <= {0} - (0.25f/255.0f))",             //ALPHACMP_LESS 1
            "(abs(prev.a - {0}) < (0.5f/255.0f))",          //ALPHACMP_EQUAL 2
            "(prev.a < {0} + (0.25f/255.0f))",              //ALPHACMP_LEQUAL 3
            "(prev.a >= {0} + (0.25f/255.0f))",             //ALPHACMP_GREATER 4
            "(abs(prev.a - {0}) >= (0.5f/255.0f))",         //ALPHACMP_NEQUAL 5
            "(prev.a > {0} - (0.25f/255.0f))",              //ALPHACMP_GEQUAL 6
            "(true)"                                        //AlphaCompare.Always 7
        };

        // THPS3 does not calculate ZCompLoc correctly if there is a margin
        // of error included.  This table removes that margin for ALPHACMP_LESS
        // and ALPHACMP_GREATER.  The other functions are to be confirmed.
        public static readonly string[] tevAlphaFuncsTableZCompLoc =
        {
            "(false)",                                      //AlphaCompare.Never 0
            "(prev.a <= {0})",                              //ALPHACMP_LESS 1
            "(abs(prev.a - {0}) < (0.5f/255.0f))",          //ALPHACMP_EQUAL 2
            "(prev.a < {0} + (0.25f/255.0f))",              //ALPHACMP_LEQUAL 3
            "(prev.a >= {0})",                              //ALPHACMP_GREATER 4
            "(abs(prev.a - {0}) >= (0.5f/255.0f))",         //ALPHACMP_NEQUAL 5
            "(prev.a > {0} - (0.25f/255.0f))",              //ALPHACMP_GEQUAL 6
            "(true)"                                        //AlphaCompare.Always 7
        };

        public static readonly string[] tevAlphaFunclogicTable =
        {
            " && ", // and
            " || ", // or
            " != ", // xor
            " == "  // xnor
        };

        public static readonly string[] alphaRef =
        {
            I_ALPHA + ".r",
            I_ALPHA + ".g"
        };

        public static string[] tevFogFuncsTable =
        {
            "",                                                             //No Fog
            "",                                                             //?
            "",                                                             //Linear
            "",                                                             //?
            "  fog = 1.0f - pow(2.0f, -8.0f * fog);\n",                     //exp
            "  fog = 1.0f - pow(2.0f, -8.0f * fog * fog);\n",               //exp2
            "  fog = pow(2.0f, -8.0f * (1.0f - fog));\n",                   //backward exp
            "  fog = 1.0f - fog;\n   fog = pow(2.0f, -8.0f * fog * fog);\n" //backward exp2
        };

        public const string I_COLORS = "color";
        public const string I_KCOLORS = "kclr";
        public const string I_ALPHA = "alphaRef";
        public const string I_TEXDIMS = "texdim"; // width | height << 16 | wrap_s << 28 | wrap_t << 30
        public const string I_ZBIAS = "czbias";
        public const string I_INDTEXSCALE = "cindscale";
        public const string I_INDTEXMTX = "cindmtx";
        public const string I_FOG = "cfog";
        public const string I_PLIGHTS = "cLights";
        public const string I_PMATERIALS = "cmtrl";

        public const int C_COLORMATRIX = 0;
        public const int C_COLORS = 0;
        public const int C_KCOLORS = 4;
        public const int C_ALPHA = 8;
        public const int C_TEXDIMS = 9;
        public const int C_ZBIAS = 17;
        public const int C_INDTEXSCALE = 19;
        public const int C_INDTEXMTX = 21;
        public const int C_FOG = 27;

        public const int C_PLIGHTS = 30;
        public const int C_PMATERIALS = 70;
        public const int C_PENVCONST_END = 74;
        public const int PIXELSHADERUID_MAX_VALUES = 70;
        public const int PIXELSHADERUID_MAX_VALUES_SAFE = 120;

        public static readonly string[] tevKSelTableC = // KCSEL
        {
	        "1.0f,1.0f,1.0f",       // 1   = 0x00
	        "0.875f,0.875f,0.875f", // 7_8 = 0x01
	        "0.75f,0.75f,0.75f",    // 3_4 = 0x02
	        "0.625f,0.625f,0.625f", // 5_8 = 0x03
	        "0.5f,0.5f,0.5f",       // 1_2 = 0x04
	        "0.375f,0.375f,0.375f", // 3_8 = 0x05
	        "0.25f,0.25f,0.25f",    // 1_4 = 0x06
	        "0.125f,0.125f,0.125f", // 1_8 = 0x07
	        "ERROR", // 0x08
	        "ERROR", // 0x09
	        "ERROR", // 0x0a
	        "ERROR", // 0x0b
	        I_KCOLORS+"[0].rgb", // K0 = 0x0C
	        I_KCOLORS+"[1].rgb", // K1 = 0x0D
	        I_KCOLORS+"[2].rgb", // K2 = 0x0E
	        I_KCOLORS+"[3].rgb", // K3 = 0x0F
	        I_KCOLORS+"[0].rrr", // K0_R = 0x10
	        I_KCOLORS+"[1].rrr", // K1_R = 0x11
	        I_KCOLORS+"[2].rrr", // K2_R = 0x12
	        I_KCOLORS+"[3].rrr", // K3_R = 0x13
	        I_KCOLORS+"[0].ggg", // K0_G = 0x14
	        I_KCOLORS+"[1].ggg", // K1_G = 0x15
	        I_KCOLORS+"[2].ggg", // K2_G = 0x16
	        I_KCOLORS+"[3].ggg", // K3_G = 0x17
	        I_KCOLORS+"[0].bbb", // K0_B = 0x18
	        I_KCOLORS+"[1].bbb", // K1_B = 0x19
	        I_KCOLORS+"[2].bbb", // K2_B = 0x1A
	        I_KCOLORS+"[3].bbb", // K3_B = 0x1B
	        I_KCOLORS+"[0].aaa", // K0_A = 0x1C
	        I_KCOLORS+"[1].aaa", // K1_A = 0x1D
	        I_KCOLORS+"[2].aaa", // K2_A = 0x1E
	        I_KCOLORS+"[3].aaa", // K3_A = 0x1F
        };

        public static readonly string[] tevKSelTableA = // KASEL
        {
	        "1.0f",  // 1   = 0x00
	        "0.875f",// 7_8 = 0x01
	        "0.75f", // 3_4 = 0x02
	        "0.625f",// 5_8 = 0x03
	        "0.5f",  // 1_2 = 0x04
	        "0.375f",// 3_8 = 0x05
	        "0.25f", // 1_4 = 0x06
	        "0.125f",// 1_8 = 0x07
	        "ERROR", // 0x08
	        "ERROR", // 0x09
	        "ERROR", // 0x0a
	        "ERROR", // 0x0b
	        "ERROR", // 0x0c
	        "ERROR", // 0x0d
	        "ERROR", // 0x0e
	        "ERROR", // 0x0f
	        I_KCOLORS+"[0].r", // K0_R = 0x10
	        I_KCOLORS+"[1].r", // K1_R = 0x11
	        I_KCOLORS+"[2].r", // K2_R = 0x12
	        I_KCOLORS+"[3].r", // K3_R = 0x13
	        I_KCOLORS+"[0].g", // K0_G = 0x14
	        I_KCOLORS+"[1].g", // K1_G = 0x15
	        I_KCOLORS+"[2].g", // K2_G = 0x16
	        I_KCOLORS+"[3].g", // K3_G = 0x17
	        I_KCOLORS+"[0].b", // K0_B = 0x18
	        I_KCOLORS+"[1].b", // K1_B = 0x19
	        I_KCOLORS+"[2].b", // K2_B = 0x1A
	        I_KCOLORS+"[3].b", // K3_B = 0x1B
	        I_KCOLORS+"[0].a", // K0_A = 0x1C
	        I_KCOLORS+"[1].a", // K1_A = 0x1D
	        I_KCOLORS+"[2].a", // K2_A = 0x1E
	        I_KCOLORS+"[3].a", // K3_A = 0x1F
        };

        public static readonly string[] tevScaleTable = // CS
        {
	        "1.0f",  // SCALE_1
	        "2.0f",  // SCALE_2
	        "4.0f",  // SCALE_4
	        "0.5f",  // DIVIDE_2
        };

        public static readonly string[] tevBiasTable = // TB
        {
	        "",       // ZERO,
	        "+ 0.5f",  // ADDHALF,
	        "- 0.5f",  // SUBHALF,
	        "",
        };

        public static readonly string[] tevOpTable = { // TEV
	        "+",      // TEVOP_ADD = 0,
	        "-",      // TEVOP_SUB = 1,
        };

        public static readonly string[] tevCInputTable = // CC
        {
	        "(prev.rgb)",               // CPREV,
	        "(prev.aaa)",               // APREV,
	        "(c0.rgb)",                 // C0,
	        "(c0.aaa)",                 // A0,
	        "(c1.rgb)",                 // C1,
	        "(c1.aaa)",                 // A1,
	        "(c2.rgb)",                 // C2,
	        "(c2.aaa)",                 // A2,
	        "(textemp.rgb)",            // TEXC,
	        "(textemp.aaa)",            // TEXA,
	        "(rastemp.rgb)",            // RASC,
	        "(rastemp.aaa)",            // RASA,
	        "vec3(1.0f, 1.0f, 1.0f)", // ONE
	        "vec3(0.5f, 0.5f, 0.5f)", // HALF
	        "(konsttemp.rgb)",          // KONST
	        "vec3(0.0f, 0.0f, 0.0f)", // ZERO

	        //added extra values to map clamped values
	        "(cprev.rgb)",               // CPREV,
	        "(cprev.aaa)",               // APREV,
	        "(cc0.rgb)",                 // C0,
	        "(cc0.aaa)",                 // A0,
	        "(cc1.rgb)",                 // C1,
	        "(cc1.aaa)",                 // A1,
	        "(cc2.rgb)",                 // C2,
	        "(cc2.aaa)",                 // A2,
	        "(textemp.rgb)",             // TEXC,
	        "(textemp.aaa)",             // TEXA,
	        "(crastemp.rgb)",            // RASC,
	        "(crastemp.aaa)",            // RASA,
	        "vec3(1.0f, 1.0f, 1.0f)",  // ONE
	        "vec3(0.5f, 0.5f, 0.5f)",  // HALF
	        "(ckonsttemp.rgb)", //"konsttemp.rgb",  // KONST
	        "vec3(0.0f, 0.0f, 0.0f)",  // ZERO

	        "PADERROR", "PADERROR", "PADERROR", "PADERROR"
        };

        public static readonly string[] tevAInputTable = // CA
        {
	        "prev",            // APREV,
	        "c0",              // A0,
	        "c1",              // A1,
	        "c2",              // A2,
	        "textemp",         // TEXA,
	        "rastemp",         // RASA,
	        "konsttemp",       // KONST,  (hw1 had quarter)
	        "vec4(0.0f, 0.0f, 0.0f, 0.0f)", // ZERO
	        ///added extra values to map clamped values
	        "cprev",            // APREV,
	        "cc0",              // A0,
	        "cc1",              // A1,
	        "cc2",              // A2,
	        "textemp",         // TEXA,
	        "crastemp",         // RASA,
	        "ckonsttemp",       // KONST,  (hw1 had quarter)
	        "vec4(0.0f, 0.0f, 0.0f, 0.0f)", // ZERO
	        "PADERROR", "PADERROR", "PADERROR", "PADERROR",
	        "PADERROR", "PADERROR", "PADERROR", "PADERROR",
        };

        public static readonly string[] tevRasTable = 
        {
	        "colors_0",
	        "colors_1",
	        "ERROR", //2
	        "ERROR", //3
	        "ERROR", //4
	        "alphabump", // use bump alpha
	        "(alphabump*(255.0f/248.0f))", //normalized
	        "vec4(0.0f, 0.0f, 0.0f, 0.0f)", // zero
        };

        //const string *tevTexFunc[] = { "tex2D", "texRECT" };

        public static readonly string[] tevCOutputTable = { "prev.rgb", "c0.rgb", "c1.rgb", "c2.rgb" };
        public static readonly string[] tevAOutputTable = { "prev.a", "c0.a", "c1.a", "c2.a" };
        public static readonly string[] tevIndAlphaSel = { "", "x", "y", "z" };
        //public static readonly string[] tevIndAlphaScale = { "", "*32", "*16", "*8" };
        public static readonly string[] tevIndAlphaScale = { "*(248.0f/255.0f)", "*(224.0f/255.0f)", "*(240.0f/255.0f)", "*(248.0f/255.0f)" };
        public static readonly string[] tevIndBiasField = { "", "x", "y", "xy", "z", "xz", "yz", "xyz" }; // indexed by bias
        public static readonly string[] tevIndBiasAdd = { "-128.0f", "1.0f", "1.0f", "1.0f" }; // indexed by fmt
        public static readonly string[] tevIndWrapStart = { "0.0f", "256.0f", "128.0f", "64.0f", "32.0f", "16.0f", "0.001f" };
        public static readonly string[] tevIndFmtScale = { "255.0f", "31.0f", "15.0f", "7.0f" };

        #endregion
    }
}
