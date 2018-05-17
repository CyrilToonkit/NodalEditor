using MiniLogger;
using System;
using System.Collections.Generic;
using System.Text;
using TK.BaseLib;

namespace TK.NodalEditor
{
    public class NodalDirector
    {
        public static NodesManager manager = null;
        public static NodesLayout.NodesLayout layout = null;
        public static bool verbose = true;

        public static void RegisterManager(NodesManager inManager)
        {
            manager = inManager;
        }

        public static void RegisterLayout(NodesLayout.NodesLayout inLayout)
        {
            layout = inLayout;
        }

        public static bool DeleteNodes(List<string> inNodesNames)
        {
            if (manager == null)
                return false;

            if (verbose)
                Logger.Log(string.Format("DeleteNodes(new List<string>{{\"{0}\"}})", TypesHelper.Join(inNodesNames, "\",\"")), LogSeverities.Log);

            manager.Companion.LaunchProcess("Delete nodes", inNodesNames.Count);

            foreach (string nodeName in inNodesNames)
            {
                Node node = manager.GetNode(nodeName);

                if (node == null)
                {
                    string message = string.Format("Node '{0}' does not exists !", nodeName);
                    manager.Companion.Error(message);

                    Logger.Log(message, LogSeverities.Error);
                }
                else
                {
                    manager.RemoveNode(node);
                    node.Deleted = true;
                    manager.Companion.ProgressBarIncrement();
                }
            }

            if (layout == null)
                return true;

            layout.RefreshPorts();
            layout.Selection.Selection.Clear();
            layout.ChangeFocus(true);
            manager.Companion.EndProcess();

            return true;
        }

        ///This one is cheating, we should not use Classes here (Link)
        public static bool DeleteLinks(List<Link> selLinks)
        {
            if (manager == null)
                return false;

            List<string> nodesNames = new List<string>();
            foreach (Link node in selLinks)
            {
                nodesNames.Add(node.FullName);
            }

            if (verbose)
                Logger.Log(string.Format("DeleteLinks(new List<string>{{\"{0}\"}})", TypesHelper.Join(nodesNames, "\",\"")), LogSeverities.Log);

            foreach (Link link in selLinks)
            {
                manager.CurCompound.UnConnect(link);
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }
    }
}
