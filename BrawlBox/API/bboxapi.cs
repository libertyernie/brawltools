using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using IronPython.Runtime.Exceptions;
using System.Reflection;
using BrawlLib.SSBB.ResourceNodes;
using System.Windows.Forms;

namespace BrawlBox.API
{
    public static class bboxapi
    {
        static bboxapi()
        {
            Plugins = new List<PluginScript>();
            Loaders = new List<PluginLoader>();
            Engine = Python.CreateEngine();
            Runtime = Engine.Runtime;

            // Setup IronPython engine
            Engine.SetSearchPaths(new string[] { $"{ Application.StartupPath }/Python" });

            //Import BrawlBox and Brawllib
            Assembly mainAssembly = Assembly.GetExecutingAssembly();
            Assembly brawllib = Assembly.GetAssembly(typeof(ResourceNode));

            Runtime.LoadAssembly(mainAssembly);
            Runtime.LoadAssembly(brawllib);
            Runtime.LoadAssembly(typeof(String).Assembly);
            Runtime.LoadAssembly(typeof(Uri).Assembly);
        }

        internal static List<PluginScript> Plugins { get; set; }
        internal static List<PluginLoader> Loaders { get; set; }

        internal static ScriptEngine Engine { get; set; }
        internal static ScriptRuntime Runtime { get; set; }

        internal static void CreatePlugin(string path, bool loader)
        {
            try
            {
                ScriptSource script = Engine.CreateScriptSourceFromFile(path);
                CompiledCode code = script.Compile();
                ScriptScope scope = Engine.CreateScope();
                if (!loader)
                    Plugins.Add(new PluginScript(Path.GetFileNameWithoutExtension(path), script, scope));
                else
                    script.Execute();
            }
            catch (SyntaxErrorException e)
            {
                string msg = $"Syntax error in \"{Path.GetFileName(path)}\"\n{e.Message}";
                ShowMessage(msg, Path.GetFileName(path));
            }
            catch (SystemExitException e)
            {
                string msg = $"SystemExit in \"{Path.GetFileName(path)}\"\n{e.Message}";
                ShowMessage(msg, Path.GetFileName(path));
            }

            catch (Exception e)
            {
                string msg = $"Error loading plugin or loader \"{Path.GetFileName(path)}\"\n{e.Message}";
                ShowMessage(msg, Path.GetFileName(path));
            }
        }
        public static void ShowMessage(string msg, string title)
        {
            MessageBox.Show(msg, title);
        }

        public static ResourceNode RootNode
        {
            get
            {
                return MainForm.Instance.RootNode.Resource;
            }
        }
        public static BaseWrapper SelectedNode
        {
            get
            {
                return (BaseWrapper)MainForm.Instance.resourceTree.SelectedNode;
            }
        }

        public static void AddLoader(PluginLoader loader) =>
            Loaders.Add(loader);
    }
}
