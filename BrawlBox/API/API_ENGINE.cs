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

namespace BrawlBox.API
{
    public static class API_ENGINE
    {
        static API_ENGINE()
        {
            Plugins = new List<PluginScript>();
            Loaders = new List<PluginLoader>();
            Engine = Python.CreateEngine();
            Runtime = Engine.Runtime;

            // Setup IronPython engine
            Engine.SetSearchPaths(new string[] { "Python" });

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

        public static ScriptEngine Engine { get; set; }
        public static ScriptRuntime Runtime { get; set; }

        public static void CreatePlugin(string path)
        {
            try
            {
                ScriptSource script = Engine.CreateScriptSourceFromFile(path);
                CompiledCode code = script.Compile();
                ScriptScope scope = Engine.CreateScope();
                Plugins.Add(new PluginScript(Path.GetFileNameWithoutExtension(path), script, scope));
            }
            catch (SyntaxErrorException e)
            {
                string msg = "Syntax error in \"{0}\"";
                ShowError(msg, Path.GetFileName(path), e);
            }
            catch (SystemExitException e)
            {
                string msg = "SystemExit in \"{0}\"";
                ShowError(msg, Path.GetFileName(path), e);
            }

            catch (Exception e)
            {
                string msg = "Error loading plugin \"{0}\"";
                ShowError(msg, Path.GetFileName(path), e);
            }
        }
        public static void CreateLoader(string path)
        {
            try
            {
                ScriptSource script = Engine.CreateScriptSourceFromFile(path);
                CompiledCode code = script.Compile();
                ScriptScope scope = Engine.CreateScope();
                script.Execute();
            }
            catch (SyntaxErrorException e)
            {
                string msg = "Syntax error in \"{0}\"";
                ShowError(msg, Path.GetFileName(path), e);
            }
            catch (SystemExitException e)
            {
                string msg = "SystemExit in \"{0}\"";
                ShowError(msg, Path.GetFileName(path), e);
            }

            catch (Exception e)
            {
                string msg = $"Error loading plugin \"{Path.GetFileName(path)}\"";
                ShowError($"{msg}\n{e.Message}", Path.GetFileName(path), e);
            }
        }

        private static void ShowError(string msg, string v, Exception e)
        {
            System.Windows.Forms.MessageBox.Show(msg, v);
        }
    }
    public static class bboxapi
    {
        public static ResourceNode RootNode
        {
            get
            {
                return MainForm.Instance.RootNode.ResourceNode;
            }
        }

        public static void AddLoader(PluginLoader loader) =>
            API_ENGINE.Loaders.Add(loader);
    }
}
