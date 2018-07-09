using MiniLogger;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TK.BaseLib;
using TK.BaseLib.CSCodeEval;
using TK.GraphComponents.Dialogs;

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

        #region Logging

        public static void Log(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Log);
        }

        public static void Info(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Info);
        }

        public static void Warning(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Warning);
        }

        public static void Error(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Error);
        }

        public static void Fatal(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Fatal);
        }

        public static void ShowError(string Message, string Caption)
        {
            TKMessageBox.ShowError(Message, Caption);
        }

        #endregion

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
                            link.Target.Owner.RefreshConnections();
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

        public static bool DisconnectAll(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("DisconnectAll(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);
                    
                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if(nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    foreach(Node Node in nodes)
                    {
                        manager.CurCompound.UnConnectAll(Node);
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }
            
            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        public static bool DisconnectInputs(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("DisconnectInputs(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);

                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if (nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    foreach (Node Node in nodes)
                    {
                        manager.CurCompound.UnConnectInputs(Node);
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        public static bool DisconnectOutputs(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("DisconnectOutputs(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);

                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if (nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    foreach (Node Node in nodes)
                    {
                        manager.CurCompound.UnConnectOutputs(Node);
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }


        public static bool Parent(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("Parent(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);

                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if (nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    if (nodes.Count < 2)
                    {
                        Logger.Log("Please select at least 2 nodes, first any number of nodes to reparent, then at last a Compound to reparent the nodes into ! Compound parent error", LogSeverities.Error);
                        return false;
                    }

                    Compound newParent = nodes[nodes.Count - 1] as Compound;

                    if (newParent == null)
                    {
                        Logger.Log("Last selected node must be a compound to reparent the nodes into ! Compound parent error", LogSeverities.Error);
                        return false;
                    }

                    nodes.Remove(nodes[nodes.Count - 1]);

                    foreach (Node node in nodes)
                    {
                        if (node.Parent != null && node.Parent != newParent)
                        {
                            manager.MoveNodes(new List<Node> { node }, newParent);
                        }
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }

            return true;
        }

        public static bool UnParent(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("UnParent(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);

                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if (nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    foreach (Node node in nodes)
                    {
                        if (node.Parent != null && node.Parent.Parent != null)
                        {
                            manager.MoveNodes(new List<Node> { node }, node.Parent.Parent);
                        }
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }

            return true;
        }

        public static bool ExposeAllPorts(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("ExposeAllPorts(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);

                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if (nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    foreach (Node Node in nodes)
                    {
                        foreach (Port port in Node.Inputs)
                        {
                            PortInstance parentPort = Node.Parent.GetPortFromNode(port);
                            parentPort.Visible = true;
                        }

                        foreach (Port port in Node.Outputs)
                        {
                            PortInstance parentPort = Node.Parent.GetPortFromNode(port);
                            parentPort.Visible = true;
                        }
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }

            return true;
        }

        public static bool HideAllPorts(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("HideAllPorts(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);

                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if (nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    foreach (Node Node in nodes)
                    {
                        foreach (Port port in Node.Inputs)
                        {
                            PortInstance parentPort = Node.Parent.GetPortFromNode(port);

                            if (!parentPort.IsLinked())
                            {
                                parentPort.Visible = false;
                            }
                        }

                        foreach (Port port in Node.Outputs)
                        {
                            PortInstance parentPort = Node.Parent.GetPortFromNode(port);

                            if (!parentPort.IsLinked())
                            {
                                parentPort.Visible = false;
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }

            return true;
        }

        public static bool CreateCompound(List<string> inputNodes)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("CreateCompound(List<string> {0})", inputNodes);
            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inputNodes.Count > 0)
            {
                foreach (string NodeName in inputNodes)
                {
                    Node Node = manager.GetNode(NodeName);

                    if (Node == null) //Node with NodeName do not exist
                    {
                        nodesNameError.Add(NodeName);
                    }
                    else //Node with NodeName exist
                    {
                        nodes.Add(Node);
                    }
                }
                if (nodesNameError.Count > 0)
                {
                    Logger.Log(nom_fct + string.Format("Not all node names in {0} exist", inputNodes), LogSeverities.Error);
                    return false;
                }
                else //All the nodes name exist
                {
                    Compound compound = manager.AddCompound(nodes);

                    if (compound != null)
                    {
                        manager.EnterCompound(compound);
                    }
                }
            }
            else
            {
                Logger.Log(nom_fct + string.Format("{0} is empty", inputNodes), LogSeverities.Error);
                return false;
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Executes arbitrary C# code at runtime
        /// </summary>
        /// <param name="inCode">The code to execute</param>
        public static void Evaluate(string inCode)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            /*
            args.Add("ManagerCompanion Companion", Companion);
            args.Add("RigCreator Creator", Creator);
            args.Add("NodesManager nodesManager", nodesManager);
            args.Add("NodesLayout RigsLayout", RigsLayout);
            */
            InterpreterResult rslt = CSInterpreter.Eval(inCode.Replace("cmds.", "NodalDirector."), string.Empty, "TK_BaseLib.dll;TK_GraphComponents.dll;TK_NodalEditor.dll;", "using System.Collections.Generic;using TK.BaseLib;using TK.BaseLib.CGModel;using TK.GraphComponents.Dialogs;using TK.NodalEditor;using TK.NodalEditor.NodesLayout;", args);
            string msg = "No info !";

            if (!rslt.Success)
            {
                if (rslt.Output is CompilerErrorCollection)
                {
                    CompilerErrorCollection errors = rslt.Output as CompilerErrorCollection;

                    msg = string.Empty;

                    foreach (CompilerError error in errors)
                    {
                        msg += error.ErrorText + "\n";
                    }
                }
                else
                {
                    msg = rslt.Output.ToString();
                }

                ShowError(msg, "Interpreter error");
            }
            else
            {
                msg = rslt.Output == null ? "null" : rslt.Output.ToString();
                Log("Returns : " + msg);
            }
        }


        ///Example to delete
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
