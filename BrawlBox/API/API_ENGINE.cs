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
            Engine = Python.CreateEngine();
            Runtime = Engine.Runtime;
            AddAssemblies();
        }

        internal static List<PluginScript> Plugins { get; set; }

        public static ScriptEngine Engine { get; set; }
        public static ScriptRuntime Runtime { get; set; }

        public static void AddAssemblies()
        {
            Assembly mainAssembly = Assembly.GetExecutingAssembly();
            Assembly brawllib = Assembly.GetAssembly(typeof(ResourceNode));

            Runtime.LoadAssembly(mainAssembly);
            Runtime.LoadAssembly(brawllib);

        }
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

        private static void ShowError(string msg, string v, Exception e)
        {
            System.Windows.Forms.MessageBox.Show(msg, v);
        }

        public static void AddPlugin(PluginScript plugin) =>
            Plugins.Add(plugin);
    }
    public static class bboxapi
    {
        public static ResourceNode RootNode { get { return Program.RootNode; } }
        public static string String = "Hello World~!";
    }
}
