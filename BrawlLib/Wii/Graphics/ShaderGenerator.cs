using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.OpenGL;
using OpenTK.Graphics.OpenGL;
using BrawlLib.Imaging;

namespace BrawlLib.Wii.Graphics
{
    public class ShaderGenerator
    {
        private static string tempShader;
        public static string GenerateVertexShader(MDL0ObjectNode obj)
        {
            Reset();

            MDL0MaterialNode mat = obj.UsableMaterialNode;
            MDL0ShaderNode shader = mat.ShaderNode;

            //w("#version 330\n");

            bool[] data = new bool[12];
            for (int i = 0; i < 12; i++)
                data[i] = obj._manager._faceData[i] != null;

            if (data[0])
                w("in vec3 Position;");
            if (data[1])
                w("in vec3 Normal;");
            for (int i = 0; i < 2; i++)
                if (data[i + 2])
                    w("in vec4 Color{0};", i);
            for (int i = 0; i < 8; i++)
                if (data[i + 4])
                    w("in vec2 UV{0};", i);

            w("uniform mat4 projection_matrix;");
            w("uniform mat4 modelview_matrix;");

            for (int i = 0; i < obj._uvSet.Length; i++)
                if (obj._uvSet[i] != null)
                    w("out vec2 UVSet{0};", i);
            for (int i = 0; i < obj._colorSet.Length; i++)
                if (obj._colorSet[i] != null)
                    w("out vec4 ColorSet{0};", i);

            Start();

            w("gl_Position = projection_matrix * modelview_matrix * vec4(Position, 1.0);");
            //w("gl_Normal = vec4(Normal, 1.0);\n");
            for (int i = 0; i < obj._uvSet.Length; i++)
                if (obj._uvSet[i] != null)
                    w("UVSet{0} = UV{0};", i);
            for (int i = 0; i < obj._colorSet.Length; i++)
                if (obj._colorSet[i] != null)
                    w("ColorSet{0} = Color{0};", i);

            Finish();

            return tempShader;
        }

        public static string GeneratePixelShader(MDL0ObjectNode obj)
        {
            Reset();

            MDL0MaterialNode mat = obj.UsableMaterialNode;
            MDL0ShaderNode shader = mat.ShaderNode;

            //w("#version 330\n");

            foreach (MDL0MaterialRefNode r in mat.Children)
                w("uniform sampler2D Texture{0};", r.Index);

            for (int i = 0; i < obj._uvSet.Length; i++)
                if (obj._uvSet[i] != null)
                    w("in vec2 UVSet{0};", i);
            for (int i = 0; i < obj._colorSet.Length; i++)
                if (obj._colorSet[i] != null)
                    w("in vec2 ColorSet{0};", i);

            w("out vec4 out_color;\n");

            //w("uniform vec4 C1Amb;\n");
            //w("uniform vec4 C2Amb;\n");
            //w("uniform vec4 C1Mat;\n");
            //w("uniform vec4 C2Mat;\n");

            Start();

            foreach (MDL0MaterialRefNode r in mat.Children)
                if (r.TextureCoordId >= 0)
                    w("vec4 tex{0}col = texture2D(Texture{0}, UVSet{1}.st);\n", r.Index, r.TextureCoordId);

            //w("vec4 creg0 = vec4(0.0, 0.0, 0.0, 0.0);\n");
            //w("vec4 creg1 = vec4(0.0, 0.0, 0.0, 0.0);\n");
            //w("vec4 creg2 = vec4(0.0, 0.0, 0.0, 0.0);\n");
            //w("vec4 prev = vec4(0.0, 0.0, 0.0, 0.0);\n");

            //foreach (TEVStage stage in shader.Children)
            //    if (stage.Index < mat.ActiveShaderStages)
            //        w(stage.Write(mat, obj));
            //    else break;

            //if (shader._stages > 0)
            //{
            //    w("prev.rgb = {0};\n", tevCOutputTable[(int)((TEVStage)shader.Children[shader.Children.Count - 1]).ColorRegister]);
            //    w("prev.a = {0};\n", tevAOutputTable[(int)((TEVStage)shader.Children[shader.Children.Count - 1]).AlphaRegister]);
            //}

            w("out_color = tex0col;");
            
            Finish();

            return tempShader;
        }

        public static void Reset()
        {
            tempShader = "";
            tabs = 0;
        }

        public static void Start() { w("void main(void)\n{\n"); }
        public static void Finish() { w("\n}"); }

        private static int tabs = 0;
        private static string Tabs { get { string t = ""; for (int i = 0; i < tabs; i++) t += "\t"; return t; } }
        private static void w(string str, params object[] args)
        {
            str += "\n";

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

        public static void SetUniforms(MDL0ObjectNode obj)
        {
            //MDL0MaterialNode mat = obj.UsableMaterialNode;

            //int pHandle = obj._programHandle;
            //int u = -1;

            //u = GL.GetUniformLocation(pHandle, "C1Amb");
            //if (u > -1) 
            //    GL.Uniform4(u, 
            //    mat.C1AmbientColor.R * RGBAPixel.ColorFactor,
            //    mat.C1AmbientColor.G * RGBAPixel.ColorFactor,
            //    mat.C1AmbientColor.B * RGBAPixel.ColorFactor,
            //    mat.C1AmbientColor.A * RGBAPixel.ColorFactor);

            //u = GL.GetUniformLocation(pHandle, "C2Amb");
            //if (u > -1) 
            //    GL.Uniform4(u, 
            //    mat.C2AmbientColor.R * RGBAPixel.ColorFactor,
            //    mat.C2AmbientColor.G * RGBAPixel.ColorFactor,
            //    mat.C2AmbientColor.B * RGBAPixel.ColorFactor,
            //    mat.C2AmbientColor.A * RGBAPixel.ColorFactor);

            //u = GL.GetUniformLocation(pHandle, "C1Mat");
            //if (u > -1)
            //    GL.Uniform4(u,
            //    mat.C1MaterialColor.R * RGBAPixel.ColorFactor,
            //    mat.C1MaterialColor.G * RGBAPixel.ColorFactor,
            //    mat.C1MaterialColor.B * RGBAPixel.ColorFactor,
            //    mat.C1MaterialColor.A * RGBAPixel.ColorFactor);

            //u = GL.GetUniformLocation(pHandle, "C2Mat");
            //if (u > -1)
            //    GL.Uniform4(u,
            //    mat.C2MaterialColor.R * RGBAPixel.ColorFactor,
            //    mat.C2MaterialColor.G * RGBAPixel.ColorFactor,
            //    mat.C2MaterialColor.B * RGBAPixel.ColorFactor,
            //    mat.C2MaterialColor.A * RGBAPixel.ColorFactor);
        }

        public static readonly string[] tevCOutputTable = { "prev.rgb", "c0.rgb", "c1.rgb", "c2.rgb" };
        public static readonly string[] tevAOutputTable = { "prev.a", "c0.a", "c1.a", "c2.a" };
        public static readonly string[] tevIndAlphaSel = { "", "x", "y", "z" };
        public static readonly string[] tevIndAlphaScale = { "", "*32", "*16", "*8" };
        //public static readonly string[] tevIndAlphaScale = { "*(248.0f/255.0f)", "*(224.0f/255.0f)", "*(240.0f/255.0f)", "*(248.0f/255.0f)" };
        public static readonly string[] tevIndBiasField = { "", "x", "y", "xy", "z", "xz", "yz", "xyz" }; // indexed by bias
        public static readonly string[] tevIndBiasAdd = { "-128.0f", "1.0f", "1.0f", "1.0f" }; // indexed by fmt
        public static readonly string[] tevIndWrapStart = { "0.0f", "256.0f", "128.0f", "64.0f", "32.0f", "16.0f", "0.001f" };
        public static readonly string[] tevIndFmtScale = { "255.0f", "31.0f", "15.0f", "7.0f" };

        /*
            * gl_LightSource[] is a built-in array for all lights.
            struct gl_LightSourceParameters 
            {   
               vec4 ambient;              // Aclarri   
               vec4 diffuse;              // Dcli   
               vec4 specular;             // Scli   
               vec4 position;             // Ppli   
               vec4 halfVector;           // Derived: Hi   
               vec3 spotDirection;        // Sdli   
               float spotExponent;        // Srli   
               float spotCutoff;          // Crli                              
                                          // (range: [0.0,90.0], 180.0)   
               float spotCosCutoff;       // Derived: cos(Crli)                 
                                          // (range: [1.0,0.0],-1.0)   
               float constantAttenuation; // K0   
               float linearAttenuation;   // K1   
               float quadraticAttenuation;// K2  
            };    
            uniform gl_LightSourceParameters gl_LightSource[gl_MaxLights];
            *
            * access the values set with glMaterial using the GLSL built-in variables gl_FrontMateral and gl_BackMaterial.
            struct gl_MaterialParameters  
            {   
               vec4 emission;    // Ecm   
               vec4 ambient;     // Acm   
               vec4 diffuse;     // Dcm   
               vec4 specular;    // Scm   
               float shininess;  // Srm  
            };  
            uniform gl_MaterialParameters gl_FrontMaterial;  
            uniform gl_MaterialParameters gl_BackMaterial; 
            */
    }
}
