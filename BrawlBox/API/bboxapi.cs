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
using BrawlBox.NodeWrappers;

namespace BrawlBox.API
{
    public static class bboxapi
    {
        static bboxapi()
        {
            ContextMenuHooks = new Dictionary<Type, ToolStripMenuItem[]>();
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
            Runtime.LoadAssembly(typeof(Form).Assembly);

            // Hook the main form's resourceTree selection changed event to add contextMenu items to nodewrapper
            MainForm.Instance.resourceTree.SelectionChanged += ResourceTree_SelectionChanged;

        }
        internal static ScriptEngine Engine { get; set; }
        internal static ScriptRuntime Runtime { get; set; }

        internal static List<PluginScript> Plugins { get; set; }
        internal static List<PluginLoader> Loaders { get; set; }

        internal static Dictionary<Type, ToolStripMenuItem[]> ContextMenuHooks { get; set; }

        internal static void RunScript(string path)
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
                string msg = $"Error running script \"{Path.GetFileName(path)}\"\n{e.Message}";
                ShowMessage(msg, Path.GetFileName(path));
            }
        }
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

        private static void ResourceTree_SelectionChanged(object sender, EventArgs e)
        {
            var resourceTree = (TreeView)sender;
            if ((resourceTree.SelectedNode is BaseWrapper))
            {
                var wrapper = (BaseWrapper)resourceTree.SelectedNode;
                var type = wrapper.GetType();

                if (ContextMenuHooks.ContainsKey(type))
                    wrapper.ContextMenuStrip.Items.AddRange(ContextMenuHooks[type]);

            }
        }

        #region Exposed API members
        public static ResourceNode RootNode
        {
            get
            {
                if (MainForm.Instance.RootNode != null)
                    return MainForm.Instance.RootNode.Resource;
                else
                    return null;
            }
        }
        public static ResourceNode SelectedNode
        {
            get
            {
                return ((BaseWrapper)MainForm.Instance.resourceTree.SelectedNode).Resource;
            }
        }
        public static BaseWrapper SelectedNodeWrapper
        {
            get
            {
                return (BaseWrapper)MainForm.Instance.resourceTree.SelectedNode;
            }
        }

        public static void ShowMessage(string msg, string title)
        {
            MessageBox.Show(msg, title);
        }

        public static void AddLoader(PluginLoader loader)
        {
            Loaders.Add(loader);
        }

        public static void AddContextMenuItem(Type wrapper, params ToolStripMenuItem[] items)
        {
            if (ContextMenuHooks.ContainsKey(wrapper))
                ContextMenuHooks[wrapper].Append(items);
            else
                ContextMenuHooks.Add(wrapper, items);
        }
        #endregion
    }
}
