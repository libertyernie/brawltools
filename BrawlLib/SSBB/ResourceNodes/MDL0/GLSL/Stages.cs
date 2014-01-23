using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using BrawlLib.Wii.Graphics;
using BrawlLib.IO;
using BrawlLib.Imaging;

namespace BrawlLib.SSBB.ResourceNodes
{
    //Most of this was converted from Dolphin's C++ source
    public unsafe partial class TEVStage : MDL0EntryNode
    {
        [Browsable(false)]
        public bool IndirectActive { get { return (rawCMD & 0x17FE00) != 0; } }

        internal string Write(MDL0MaterialNode mat, MDL0ObjectNode obj)
        {
            MDL0ShaderNode shader = ((MDL0ShaderNode)Parent);

            string stage = "";

            //Get the texture coordinate to use
            int texcoord = (int)TextureCoord;

            //Do we need to use the coordinates?
            bool bHasTexCoord = texcoord < mat.Children.Count;

            //Is there an indirect stage? (CMD is not 0)
            //Is active == 
            //FB true
            //LB false
            //TW max
            //SW max
            //M max
            //0001 0111 1111 1110 0000 0000 = 0x17FE00
            bool bHasIndStage = IndirectActive && bt < mat.IndirectShaderStages;

	        // HACK to handle cases where the tex gen is not enabled
	        if (!bHasTexCoord)
		        texcoord = 0;

            stage += String.Format("//TEV stage {0}\n\n", Index);

            //Add indirect support later

            if (bHasIndStage)
            {
                stage += String.Format("// indirect op\n");

                //Perform the indirect op on the incoming regular coordinates using indtex as the offset coords
                if (Alpha != IndTexAlphaSel.Off)
                    stage += String.Format("alphabump = indtex{0}.{1} {2};\n",
                            (int)TexStage,
                            (int)Alpha,
                            (int)TexFormat);
                
                //Format
                stage += String.Format("vec3 indtevcrd{0} = indtex{1} * {2};\n", Index, (int)TexStage, (int)TexFormat);

                //Bias
                if (Bias != IndTexBiasSel.None)
                    stage += String.Format("indtevcrd{0}.{1} += {2};\n", Index, MDL0MaterialNode.tevIndBiasField[(int)Bias], MDL0MaterialNode.tevIndBiasAdd[(int)TexFormat]);

                //Multiply by offset matrix and scale
                if (Matrix != 0)
                {
                    if ((int)Matrix <= 3)
                    {
                        int mtxidx = 2 * ((int)Matrix - 1);
                        stage += String.Format("vec2 indtevtrans{0} = vec2(dot(" + MDL0MaterialNode.I_INDTEXMTX + "[{1}].xyz, indtevcrd{2}), dot(" + MDL0MaterialNode.I_INDTEXMTX + "[{3}].xyz, indtevcrd{4}));\n",
                            Index, mtxidx, Index, mtxidx + 1, Index);
                    }
                    else if ((int)Matrix < 5 && (int)Matrix <= 7 && bHasTexCoord)
                    {
                        //S
                        int mtxidx = 2 * ((int)Matrix - 5);
                        stage += String.Format("vec2 indtevtrans{0} = " + MDL0MaterialNode.I_INDTEXMTX + "[{1}].ww * uv{2}.xy * indtevcrd{3}.xx;\n", Index, mtxidx, texcoord, Index);
                    }
                    else if ((int)Matrix < 9 && (int)Matrix <= 11 && bHasTexCoord)
                    { 
                        //T
                        int mtxidx = 2 * ((int)Matrix - 9);
                        stage += String.Format("vec2 indtevtrans{0} = " + MDL0MaterialNode.I_INDTEXMTX + "[{1}].ww * uv{2}.xy * indtevcrd{3}.yy;\n", Index, mtxidx, texcoord, Index);
                    }
                    else
                        stage += String.Format("vec2 indtevtrans{0} = 0;\n", Index);
                }
                else
                    stage += String.Format("vec2 indtevtrans{0} = 0;\n", Index);

                #region Wrapping

                // wrap S
                if (S_Wrap == IndTexWrap.NoWrap)
                    stage += String.Format("wrappedcoord.x = uv{0}.x;\n", texcoord);
                else if (S_Wrap == IndTexWrap.Wrap0)
                    stage += String.Format("wrappedcoord.x = 0.0f;\n");
                else
                    stage += String.Format("wrappedcoord.x = fmod( uv{0}.x, {1} );\n", texcoord, MDL0MaterialNode.tevIndWrapStart[(int)S_Wrap]);
                
                // wrap T
                if (T_Wrap == IndTexWrap.NoWrap)
                    stage += String.Format("wrappedcoord.y = uv{0}.y;\n", texcoord);
                else if (T_Wrap == IndTexWrap.Wrap0)
                    stage += String.Format("wrappedcoord.y = 0.0f;\n");
                else
                    stage += String.Format("wrappedcoord.y = fmod( uv{0}.y, {1} );\n", texcoord, MDL0MaterialNode.tevIndWrapStart[(int)T_Wrap]);

                stage += String.Format("tevcoord.xy {0}= wrappedcoord + indtevtrans{1};\n", UsePrevStage ? "+" : "", Index);

                #endregion
            }

            //Check if we need to use Alpha
            if (ColorSelectionA == ColorArg.RasterAlpha || ColorSelectionA == ColorArg.RasterColor
             || ColorSelectionB == ColorArg.RasterAlpha || ColorSelectionB == ColorArg.RasterColor
             || ColorSelectionC == ColorArg.RasterAlpha || ColorSelectionC == ColorArg.RasterColor
             || ColorSelectionD == ColorArg.RasterAlpha || ColorSelectionD == ColorArg.RasterColor
             || AlphaSelectionA == AlphaArg.RasterAlpha || AlphaSelectionB == AlphaArg.RasterAlpha
             || AlphaSelectionC == AlphaArg.RasterAlpha || AlphaSelectionD == AlphaArg.RasterAlpha)
	        {
		        string rasswap = shader.swapModeTable[rswap];
                stage += String.Format("rastemp = {0}.{1};\n", MDL0MaterialNode.tevRasTable[(int)ColorChannel], rasswap);
		        stage += String.Format("crastemp = fract(rastemp * (255.0f/256.0f)) * (256.0f/255.0f);\n");
	        }

	        if (TextureEnabled)
	        {
		        if(!bHasIndStage) //Calculate tevcoord
			        if(bHasTexCoord)
				        stage += String.Format("tevcoord.xy = uv{0}.xy;\n", texcoord);
			        else
				        stage += String.Format("tevcoord.xy = vec2(0.0f, 0.0f);\n");

                string texswap = shader.swapModeTable[tswap];
                int texmap = (int)TextureMapID;

                stage += String.Format("{0} = texture2D(samp{1}, {2}.xy"/* + " * " + MDL0MaterialNode.I_TEXDIMS + "[{3}].xy" */+ ").{4};\n", "textemp", texmap, "tevcoord", texmap, texswap);
	        }
	        else
		        stage += String.Format("textemp = vec4(1.0f, 1.0f, 1.0f, 1.0f);\n");

            //Check if we need to use Konstant Colors
            if (ColorSelectionA == ColorArg.KonstantColorSelection || 
                ColorSelectionB == ColorArg.KonstantColorSelection || 
                ColorSelectionC == ColorArg.KonstantColorSelection || 
                ColorSelectionD == ColorArg.KonstantColorSelection || 
                AlphaSelectionA == AlphaArg.KonstantAlphaSelection ||
                AlphaSelectionB == AlphaArg.KonstantAlphaSelection ||
                AlphaSelectionC == AlphaArg.KonstantAlphaSelection ||
                AlphaSelectionD == AlphaArg.KonstantAlphaSelection)
	        {
                int kc = (int)KonstantColorSelection;
                int ka = (int)KonstantAlphaSelection;

                stage += String.Format("konsttemp = vec4({0}, {1});\n", MDL0MaterialNode.tevKSelTableC[kc], MDL0MaterialNode.tevKSelTableA[ka]);
		        
                if(kc > 7 || ka > 7)
			        stage += String.Format("ckonsttemp = fract(konsttemp * (255.0f/256.0f)) * (256.0f/255.0f);\n");
		        else
			        stage += String.Format("ckonsttemp = konsttemp;\n");
	        }

            if (ColorSelectionA == ColorArg.PreviousColor || ColorSelectionA == ColorArg.PreviousAlpha
             || ColorSelectionB == ColorArg.PreviousColor || ColorSelectionB == ColorArg.PreviousAlpha
             || ColorSelectionC == ColorArg.PreviousColor || ColorSelectionC == ColorArg.PreviousAlpha
             || AlphaSelectionA == AlphaArg.PreviousAlpha || AlphaSelectionB == AlphaArg.PreviousAlpha || AlphaSelectionC == AlphaArg.PreviousAlpha)
		        stage += String.Format("cprev = fract(prev * (255.0f/256.0f)) * (256.0f/255.0f);\n");

	        if (ColorSelectionA == ColorArg.Color0 || ColorSelectionA == ColorArg.Alpha0
	         || ColorSelectionB == ColorArg.Color0 || ColorSelectionB == ColorArg.Alpha0
	         || ColorSelectionC == ColorArg.Color0 || ColorSelectionC == ColorArg.Alpha0
	         || AlphaSelectionA == AlphaArg.Alpha0 || AlphaSelectionB == AlphaArg.Alpha0 || AlphaSelectionC == AlphaArg.Alpha0)
		        stage += String.Format("cc0 = fract(c0 * (255.0f/256.0f)) * (256.0f/255.0f);\n");

	        if (ColorSelectionA == ColorArg.Color1 || ColorSelectionA == ColorArg.Alpha1
	         || ColorSelectionB == ColorArg.Color1 || ColorSelectionB == ColorArg.Alpha1
	         || ColorSelectionC == ColorArg.Color1 || ColorSelectionC == ColorArg.Alpha1
	         || AlphaSelectionA == AlphaArg.Alpha1 || AlphaSelectionB == AlphaArg.Alpha1 || AlphaSelectionC == AlphaArg.Alpha1)
		        stage += String.Format("cc1 = fract(c1 * (255.0f/256.0f)) * (256.0f/255.0f);\n");

	        if (ColorSelectionA == ColorArg.Color2 || ColorSelectionA == ColorArg.Alpha2
	         || ColorSelectionB == ColorArg.Color2 || ColorSelectionB == ColorArg.Alpha2
	         || ColorSelectionC == ColorArg.Color2 || ColorSelectionC == ColorArg.Alpha2
	         || AlphaSelectionA == AlphaArg.Alpha2 || AlphaSelectionB == AlphaArg.Alpha2 || AlphaSelectionC == AlphaArg.Alpha2)
			    stage += String.Format("cc2 = fract(c2 * (255.0f/256.0f)) * (256.0f/255.0f);\n");

            #region Color Channel

            stage += String.Format("// color combine\n{0} = ", MDL0MaterialNode.tevCOutputTable[(int)ColorRegister]);

            if (ColorClamp)
                stage += "saturate(";

		    if (ColorScale > TevScale.MultiplyBy1)
                stage += String.Format("{0} * (", MDL0MaterialNode.tevScaleTable[(int)ColorScale]);

            if (!(ColorSelectionD == ColorArg.Zero && ColorSubtract == false))
                stage += String.Format("{0} {1} ", MDL0MaterialNode.tevCInputTable[(int)ColorSelectionD], MDL0MaterialNode.tevOpTable[ColorSubtract ? 1 : 0]);

            if (ColorSelectionA == ColorSelectionB)
                stage += String.Format("{0}",
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionA + 16]);
            else if (ColorSelectionC == ColorArg.Zero)
                stage += String.Format("{0}",
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionA + 16]);
            else if (ColorSelectionC == ColorArg.One)
                stage += String.Format("{0}",
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionB + 16]);
            else if (ColorSelectionA == ColorArg.Zero)
                stage += String.Format("{0} * {1}",
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionB + 16],
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionC + 16]);
            else if (ColorSelectionB == ColorArg.Zero)
                stage += String.Format("{0} * (vec3(1.0f, 1.0f, 1.0f) - {1})",
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionA + 16],
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionC + 16]);
            else
                stage += String.Format("lerp({0}, {1}, {2})",
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionA + 16],
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionB + 16],
                    MDL0MaterialNode.tevCInputTable[(int)ColorSelectionC + 16]);

            stage += MDL0MaterialNode.tevBiasTable[(int)ColorBias];

            if (ColorClamp) stage += ")";
            if (ColorScale > TevScale.MultiplyBy1) stage += ")";
            
            #endregion

	        stage += ";\n";

            #region Alpha Channel

            stage += String.Format("// alpha combine\n{0} = ", MDL0MaterialNode.tevAOutputTable[(int)AlphaRegister]);

            if (AlphaClamp)
                stage += "saturate(";

		    if (AlphaScale > TevScale.MultiplyBy1)
                stage += String.Format("{0} * (", MDL0MaterialNode.tevScaleTable[(int)AlphaScale]);

		    if(!(AlphaSelectionD == AlphaArg.Zero && AlphaSubtract == false))
                stage += String.Format("{0}.a {1} ", MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionD], MDL0MaterialNode.tevOpTable[AlphaSubtract ? 1 : 0]);

		    if (AlphaSelectionA == AlphaSelectionB)
                stage += String.Format("{0}.a",
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionA + 8]);
		    else if (AlphaSelectionC == AlphaArg.Zero)
                stage += String.Format("{0}.a",
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionA + 8]);
            else if (AlphaSelectionA == AlphaArg.Zero)
                stage += String.Format("{0}.a * {1}.a",
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionB + 8],
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionC + 8]);
            else if (AlphaSelectionB == AlphaArg.Zero)
                stage += String.Format("{0}.a * (1.0f - {1}.a)",
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionA + 8],
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionC + 8]);
		    else
                stage += String.Format("lerp({0}.a, {1}.a, {2}.a)",
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionA + 8],
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionB + 8],
                    MDL0MaterialNode.tevAInputTable[(int)AlphaSelectionC + 8]);

            stage += MDL0MaterialNode.tevBiasTable[(int)AlphaBias];

            if (AlphaClamp) stage += ")";
            if (AlphaScale > TevScale.MultiplyBy1) stage += ")";

            #endregion

            stage += ";\n\n//TEV stage " + Index + " done\n\n";

            return stage;
        }
    }
}