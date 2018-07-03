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

        ///This one is not cheating
        public static bool Disconnect(string inputNode, string inputPort, string outputNode, string outputPort)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("Disconnect(string {0}, string {0}, string {0}, string {0})", inputNode, inputPort, outputNode, outputNode);
            /*
            if (verbose)
                Logger.Log(string.Format("DeleteLinks(new List<string>{{\"{0}\"}})", TypesHelper.Join(outputNode, "\",\"")), LogSeverities.Log);
            */

            Node nodeIn = manager.GetNode(inputNode);
            Node nodeOut = manager.GetNode(outputNode);

            if (nodeIn == null)
            {
                Logger.Log(nom_fct+string.Format("input Node {0} is null", inputNode), LogSeverities.Error);
                return false;
            }
            if (nodeOut == null)
            {
                Logger.Log(nom_fct+string.Format("output Node {0} is null", outputNode), LogSeverities.Error);
                return false;
            }

            Port portIn = nodeIn.GetPort(inputPort, false);
            Port portOut = nodeOut.GetPort(outputPort, false);

            if (portIn == null)
            {
                Logger.Log(nom_fct+string.Format("input Port {0} from {0} is null", inputNode, inputPort), LogSeverities.Error);
                return false;
            }
            if (portOut == null)
            {
                Logger.Log(nom_fct+string.Format("output Port {0} from {0} is null", outputNode, outputPort), LogSeverities.Error);
                return false;
            }

            if (portIn.Dependencies.Count != 0)
            {
                List<Link> linkToDisconnect = new List<Link>();
                foreach (Link link in portIn.Dependencies)
                {

                    if (link.Source.FullName == portOut.FullName)
                    {
                        linkToDisconnect.Add(link);
                    }
                }

                if (linkToDisconnect.Count != 0)
                {
                    foreach (Link link in linkToDisconnect)
                    {
                        manager.CurCompound.UnConnect(link);

                    }
                }
                else
                {
                    Logger.Log(nom_fct+string.Format("No link between port {0} from Node {0} and port {0} from Node {0}", inputPort, inputNode), LogSeverities.Error);
                }
            }
            else
            {
                Logger.Log(nom_fct+string.Format("Port {0} from Node {0} has no link", inputPort, inputNode), LogSeverities.Error);
            }
            //Port port = node.GetPort();
            //port.Dependencies[0].Target.Fu

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        public static bool Connect(string inputNode, string inputPort, string outputNode, string outputPort)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("Connect(string {0}, string {0}, string {0}, string {0})", inputNode, inputPort, outputNode, outputNode);

            /*
            if (verbose)
                Logger.Log(string.Format("DeleteLinks(new List<string>{{\"{0}\"}})", TypesHelper.Join(outputNode, "\",\"")), LogSeverities.Log);
            */
            Node nodeIn = manager.GetNode(inputNode);
            Node nodeOut = manager.GetNode(outputNode);

            if (nodeIn == null)
            {
                Logger.Log(nom_fct + string.Format("input Node {0} is null", inputNode), LogSeverities.Error);
                return false;
            }
            if (nodeOut == null)
            {
                Logger.Log(nom_fct + string.Format("output Node {0} is null", outputNode), LogSeverities.Error);
                return false;
            }

            Port portOut = nodeOut.GetPort(outputPort, false);
            Port portIn = nodeIn.GetPort(inputPort, false);

            if (portIn == null)
            {
                Logger.Log(nom_fct + string.Format("input Port {0} from {0} is null", inputNode, inputPort), LogSeverities.Error);
                return false;
            }
            if (portOut == null)
            {
                Logger.Log(nom_fct + string.Format("output Port {0} from {0} is null", outputNode, outputPort), LogSeverities.Error);
                return false;
            }

            string error=string.Empty;

            nodeIn.Connect(portIn.Index, nodeOut, portOut.Index, "", out error);

            if (error.Length != 0)
            {
                Logger.Log(nom_fct+"Cannot connect", LogSeverities.Error);
            }
  
            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        //EN COURS
        public static bool ReConnect(   string inputNode, string inputPort, string outputNode, string outputPort, 
                                        string newInputNode, string newInputPort, string newOutputNode, string newOutputPort)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("ReConnect(string {0}, string {0}, string {0}, string {0})", inputNode, inputPort, outputNode, outputNode);

            /*
            if (verbose)
                Logger.Log(string.Format("DeleteLinks(new List<string>{{\"{0}\"}})", TypesHelper.Join(outputNode, "\",\"")), LogSeverities.Log);
            */
            Node nodeIn = manager.GetNode(inputNode);
            Node nodeOut = manager.GetNode(outputNode);
            Node newNodeIn = manager.GetNode(newInputNode);
            Node newNodeOut = manager.GetNode(newOutputNode);

            if (nodeIn == null)
            {
                Logger.Log(nom_fct + string.Format("input Node {0} is null", inputNode), LogSeverities.Error);
                return false;
            }
            if (nodeOut == null)
            {
                Logger.Log(nom_fct + string.Format("output Node {0} is null", outputNode), LogSeverities.Error);
                return false;
            }
            if (newNodeIn == null)
            {
                Logger.Log(nom_fct + string.Format("input Node {0} is null", newInputNode), LogSeverities.Error);
                return false;
            }
            if (newNodeOut == null)
            {
                Logger.Log(nom_fct + string.Format("output Node {0} is null", newOutputNode), LogSeverities.Error);
                return false;
            }

            Port portOut = nodeOut.GetPort(outputPort, false);
            Port portIn = nodeIn.GetPort(inputPort, false);
            Port newPortOut = nodeOut.GetPort(newOutputPort, false);
            Port newPortIn = nodeIn.GetPort(newInputPort, false);

            if (portIn == null)
            {
                Logger.Log(nom_fct + string.Format("input Port {0} from {0} is null", inputNode, inputPort), LogSeverities.Error);
                return false;
            }
            if (portOut == null)
            {
                Logger.Log(nom_fct + string.Format("output Port {0} from {0} is null", outputNode, outputPort), LogSeverities.Error);
                return false;
            }
            if (newPortIn == null)
            {
                Logger.Log(nom_fct + string.Format("input Port {0} from {0} is null", newInputNode, newInputPort), LogSeverities.Error);
                return false;
            }
            if (newPortOut == null)
            {
                Logger.Log(nom_fct + string.Format("output Port {0} from {0} is null", newOutputNode, newOutputPort), LogSeverities.Error);
                return false;
            }


            if (portIn.Dependencies.Count != 0)
            {
                List<Link> linkToDisconnect = new List<Link>();
                foreach (Link link in portIn.Dependencies)
                {
                    if (link.Source.FullName == portOut.FullName)
                    {
                        linkToDisconnect.Add(link);
                    }
                }

                if (linkToDisconnect.Count != 0)
                {
                    foreach (Link link in linkToDisconnect)
                    {
                        if (link.Target.Dependencies.Contains(link))
                        {
                            link.Target.Owner.UnConnectObject(link);
                            link.Target.Dependencies.Remove(link);
                            //link.Target.Owner.RefreshConnections();
                        }

                    }
                }
                else
                {
                    Logger.Log(nom_fct + string.Format("No link between port {0} from Node {0} and port {0} from Node {0}", inputPort, inputNode), LogSeverities.Error);
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("Port {0} from Node {0} has no link", inputPort, inputNode), LogSeverities.Error);
            }

            Connect(inputNode, inputPort, newOutputNode, newOutputPort);

            //Port port = node.GetPort();
            //port.Dependencies[0].Target.Fu

            if (layout == null)
                return true;

            layout.Invalidate();
            return true;
        }


        //EN COURS
        public static bool DisconnectAll(List<Node> nodes)
        {
            if (manager == null)
                return false;

            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    manager.CurCompound.UnConnectAll(Node);
                }
            }
            else
            {
                //Node Node = nodeMenuStrip.Tag as Node;
                //manager.CurCompound.UnConnectAll(Node);
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }


        //EN COURS
        public static bool DisconnectInputs(List<Node> nodes)
        {
            if (manager == null)
                return false;

            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    manager.CurCompound.UnConnectInputs(Node);
                }
            }
            else
            {
                //Node Node = nodeMenuStrip.Tag as Node;
                //manager.CurCompound.UnConnectInputs(Node);
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        //EN COURS
        public static bool DisconnectOutputs(List<string> nodesName)
        {
            if (manager == null)
                return false;

            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    manager.CurCompound.UnConnectOutputs(Node);
                }
            }
            else
            {
                //Node Node = nodeMenuStrip.Tag as Node;
                //manager.CurCompound.UnConnectOutputs(Node);
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        //EN COURS
        public static bool CreateCompound(List<Node> nodes)
        {
            if (manager == null)
                return false;

            if (nodes.Count > 0)
            {
                Compound compound = manager.AddCompound(nodes);

                if (compound != null)
                {
                    manager.EnterCompound(compound);
                }
            }

            if (layout == null)
                return true;

            layout.Invalidate();

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
