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

        /// <summary>
        /// Logs a verbose message
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public static void Log(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Log);
        }

        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public static void Info(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Info);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public static void Warning(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Warning);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public static void Error(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Error);
        }

        /// <summary>
        /// Logs a fatal message
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        public static void Fatal(string inMessage)
        {
            Logger.Log(inMessage, LogSeverities.Fatal);
        }

        /// <summary>
        /// Shows an error dialog
        /// </summary>
        /// <param name="Message">Message to log</param>
        /// <param name="Caption">Title of the dialog</param>
        public static void ShowError(string Message, string Caption)
        {
            TKMessageBox.ShowError(Message, Caption);
        }

        #endregion

        /// <summary>
        /// Add Node with inNodeName (and a compound inCompoundName) and location X, Y
        /// </summary>
        /// <param name="inNodeName">Name of inputNode</param>
        /// <param name="inCompoundName">Name of input compound</param>
        /// <param name="X">Location X in pixels</param>
        /// <param name="Y">Location Y in pixels</param>
        /// <returns></returns>
        public static string AddNode(string inNodeName, string inCompoundName, int X, int Y)
        {
            if (manager == null)
                return null;

            string nom_fct = string.Format("AddNode(\"{0}\", \"{1}\", {2}, {3});", inNodeName, inCompoundName, X, Y);

            if (verbose)
                Log(nom_fct);

            string nodeName = null;
            Compound inCompound = null;

            if(string.IsNullOrEmpty(inCompoundName))
            {
                inCompound = manager.Root;
            }
            else
            {
                inCompound = manager.GetNode(inCompoundName) as Compound;
            }

            if (inCompound == null)
            {
                Error(nom_fct + "\n" + string.Format("Compound \"{0}\" is null", inCompoundName));
                return null;
            }

            bool isTrue = false; ;
            foreach (Node Node in manager.AvailableNodes)
            {
                if(inNodeName == Node.FullName)
                {
                    Node node = manager.AddNode(inNodeName, inCompound, X, Y);
                    nodeName = node.FullName;
                    isTrue = true;
                    break;
                }
                else
                {
                    isTrue = false;
                }

            }

            if(isTrue == false)
            {
                Error(nom_fct + "\n" + string.Format("Cannot Add Node with name \"{0}\"", inNodeName));
                return null;
            }


            if (layout == null)
                return nodeName;

            layout.Invalidate();

            return nodeName;
        }

        /// <summary>
        /// Delete a List of nodes
        /// </summary>
        /// <param name="inNodesNames">List of nodes names</param>
        /// <returns></returns>
        public static bool DeleteNodes(List<string> inNodesNames)
        {
            if (manager == null)
                return false;

            if (verbose)
                Log(string.Format("DeleteNodes(new List<string>{{\"{0}\"}});", TypesHelper.Join(inNodesNames, "\",\"")));

            manager.Companion.LaunchProcess("Delete nodes", inNodesNames.Count);

            foreach (string nodeName in inNodesNames)
            {
                Node node = manager.GetNode(nodeName);

                if (node == null)
                {
                    string message = string.Format("Node '{0}' does not exists !", nodeName);
                    Error(message);
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

        /// <summary>
        /// Delete a node
        /// </summary>
        /// <param name="inNodeName">nodes name</param>
        /// <returns></returns>
        public static bool DeleteNode(string inNodeName)
        {
            if (manager == null)
                return false;

            if (verbose)
                Log(string.Format("DeleteNode(\"{0}\");", inNodeName));


            Node node = manager.GetNode(inNodeName);

            if (node == null)
            {
                string message = string.Format("Node '{0}' does not exists !", inNodeName);
                Error(message);
            }
            else
            {
                manager.RemoveNode(node);
                node.Deleted = true;
            }

            if (layout == null)
                return true;

            layout.RefreshPorts();
            layout.Selection.Selection.Clear();
            layout.ChangeFocus(true);
            manager.Companion.EndProcess();

            return true;
        }

        /// <summary>
        /// Disconnect a link
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <param name="inPortName">Name of the port of input node</param>
        /// <param name="outNodeName">Name of output node</param>
        /// <param name="outPortName">Name of the port of output node</param>
        /// <returns></returns>
        public static bool Disconnect(string inNodeName, string inPortName, string outNodeName, string outPortName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("Disconnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\");", inNodeName, inPortName, outNodeName, outPortName);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);
            Node nodeOut = manager.GetNode(outNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }
            if (nodeOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Node \"{0}\" is null", outNodeName));
                return false;
            }

            Port portIn = nodeIn.GetPort(inPortName, false);
            Port portOut = nodeOut.GetPort(outPortName, true);

            if (portIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{0}\" is null", inNodeName, inPortName));
                return false;
            }
            if (portOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{0}\" is null", outNodeName, outPortName));
                return false;
            }

            if (portIn.Dependencies.Count != 0)
            {
                List<Link> linkToDisconnect = new List<Link>();
                foreach (Link link in portIn.Dependencies)
                {

                    if (link.Source == portOut)
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
                    Error(nom_fct + "\n" + string.Format("No link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\"", inPortName, inNodeName, outPortName, outNodeName));
                }
            }
            else
            {
                Error(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Connect a link
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <param name="inPortName">Name of the port of input node</param>
        /// <param name="outNodeName">Name of output node</param>
        /// <param name="outPortName">Name of the port of output node</param>
        /// <param name="inMode">The connection "mode" (basically a modifier Key like "Shift", "Control", "Alt", which can make sense in children classes)</param>
        /// <returns></returns>
        public static bool Connect(string inNodeName, string inPortName, string outNodeName, string outPortName, string inMode)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("Connect(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\");", inNodeName, inPortName, outNodeName, outPortName, inMode);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);
            Node nodeOut = manager.GetNode(outNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }
            if (nodeOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Node \"{0}\" is null", outNodeName));
                return false;
            }

            Port portOut = nodeOut.GetPort(outPortName, false);
            Port portIn = nodeIn.GetPort(inPortName, false);

            if (portIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" is null", inNodeName, inPortName));
                return false;
            }
            if (portOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" is null", outNodeName, outPortName));
                return false;
            }

            string error=string.Empty;

            nodeIn.Connect(portIn.Index, nodeOut, portOut.Index, inMode, out error, nodeIn.Companion.Manager.Preferences.CheckCycles);

            if (error.Length != 0)
            {
                Error(nom_fct + "\n" + "Cannot connect");
            }
  
            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Connect a link with default Mode
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <param name="inPortName">Name of the port of input node</param>
        /// <param name="outNodeName">Name of output node</param>
        /// <param name="outPortName">Name of the port of output node</param>
        /// <returns></returns>
        public static bool Connect(string inNodeName, string inPortName, string outNodeName, string outPortName)
        {
            return Connect(inNodeName, inPortName, outNodeName, outPortName, string.Empty);
        }

        /// <summary>
        /// Reconnect a link
        /// </summary>
        /// <param name="inNodeName">Name of input node where the link was connected at first</param>
        /// <param name="inPortName">Name of input port where the link was connected at first</param>
        /// <param name="outNodeName">Name of output node where the link was connected at first</param>
        /// <param name="outPortName">Name of output port where the link was connected at first</param>
        /// <param name="newinNodeName">Name of input node where we want to reconnect the link</param>
        /// <param name="newinPortName">Name of input port where we want to reconnect the link</param>
        /// <param name="newoutNodeName">Name of output node where we want to reconnect the link</param>
        /// <param name="newoutPortName">Name of output port where we want to reconnect the link</param>
        /// <returns></returns>
        public static bool ReConnect(   string inNodeName, string inPortName, string outNodeName, string outPortName, 
                                        string newinNodeName, string newinPortName, string newoutNodeName, string newoutPortName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("ReConnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\");", inNodeName, inPortName, outNodeName, outPortName, newinNodeName, newinPortName, newoutNodeName, newoutPortName);

            if (verbose)
                Log(nom_fct);

            Node nodeInLocked = manager.GetNode(inNodeName);
            Node nodeOut = manager.GetNode(outNodeName);
            Node newNodeIn = manager.GetNode(newinNodeName);
            Node newNodeOut = manager.GetNode(newoutNodeName);

            if (nodeInLocked == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }
            if (nodeOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Node \"{0}\" is null", outNodeName));
                return false;
            }
            if (newNodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", newinNodeName));
                return false;
            }
            if (newNodeOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Node \"{0}\" is null", newoutNodeName));
                return false;
            }

            Port portOut = nodeOut.GetPort(outPortName, true);
            Port portInLocked = nodeInLocked.GetPort(inPortName, false); 
            Port newPortOut = newNodeOut.GetPort(newoutPortName, true);
            Port newPortIn = newNodeIn.GetPort(newinPortName, false);

            if (portInLocked == null)
            {
                Error(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" is null", inNodeName, inPortName));
                return false;
            }
            if (portOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" is null", outNodeName, outPortName));
                return false;
            }
            if (newPortIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" is null", newinNodeName, newinPortName));
                return false;
            }
            if (newPortOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" is null", newoutNodeName, newoutPortName));
                return false;
            }

            string error = string.Empty;
            List<Link> linkToDisconnect = new List<Link>();
            if (portInLocked.Dependencies.Count != 0)
            {
                
                foreach (Link link in portInLocked.Dependencies)
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
                    Error(nom_fct + "\n" + string.Format("No link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\"", inPortName, inNodeName, outPortName, outNodeName));
                }
            }
            else
            {
                Error(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
            }

            if (linkToDisconnect.Count != 0)
            {
                newNodeIn.Connect(newPortIn.Index, newNodeOut, newPortOut.Index, "", out error, linkToDisconnect[0]);
            }




            if (error.Length != 0)
            {
                Error(nom_fct + "\n" + "Cannot connect");
            }

            if (layout == null)
                return true;

            layout.Invalidate();
            return true;
        }

        /// <summary>
        /// Reconnect a link by keeping the original one
        /// </summary>
        /// <param name="inNodeName">Name of input node where the link was connected at first</param>
        /// <param name="inPortName">Name of input port where the link was connected at first</param>
        /// <param name="outNodeName">Name of output node where the link was connected at first</param>
        /// <param name="outPortName">Name of output port where the link was connected at first</param>
        /// <param name="newinNodeName">Name of input node where we want to reconnect the link</param>
        /// <param name="newinPortName">Name of input port where we want to reconnect the link</param>
        /// <param name="newoutNodeName">Name of output node where we want to reconnect the link</param>
        /// <param name="newoutPortName">Name of output port where we want to reconnect the link</param>
        /// <returns></returns>
        public static bool ReConnectCopy(string inNodeName, string inPortName, string outNodeName, string outPortName,
                                string newinNodeName, string newinPortName, string newoutNodeName, string newoutPortName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("ReConnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\");", inNodeName, inPortName, outNodeName, outPortName, newinNodeName, newinPortName, newoutNodeName, newoutPortName);

            if (verbose)
                Log(nom_fct);

            Node nodeInLocked = manager.GetNode(inNodeName);
            Node nodeOut = manager.GetNode(outNodeName);
            Node newNodeIn = manager.GetNode(newinNodeName);
            Node newNodeOut = manager.GetNode(newoutNodeName);

            if (nodeInLocked == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }
            if (nodeOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Node \"{0}\" is null", outNodeName));
                return false;
            }
            if (newNodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", newinNodeName));
                return false;
            }
            if (newNodeOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Node \"{0}\" is null", newoutNodeName));
                return false;
            }

            Port portOut = nodeOut.GetPort(outPortName, true);
            Port portInLocked = nodeInLocked.GetPort(inPortName, false);
            Port newPortOut = newNodeOut.GetPort(newoutPortName, true);
            Port newPortIn = newNodeIn.GetPort(newinPortName, false);

            if (portInLocked == null)
            {
                Error(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" is null", inNodeName, inPortName));
                return false;
            }
            if (portOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" is null", outNodeName, outPortName));
                return false;
            }
            if (newPortIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" is null", newinNodeName, newinPortName));
                return false;
            }
            if (newPortOut == null)
            {
                Error(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" is null", newoutNodeName, newoutPortName));
                return false;
            }


            Link copyLink = new Link();


            string error = string.Empty;
            List<Link> linkToConnect = new List<Link>();
            if (portInLocked.Dependencies.Count != 0)
            {
                foreach (Link link in portInLocked.Dependencies)
                {
                    if (link.Source.FullName == portOut.FullName)
                    {
                        linkToConnect.Add(link);
                    }
                }
            }
            else
            {
                Error(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
            }

            if (linkToConnect.Count != 0)
            {
                copyLink.Copy(linkToConnect[0]);
                newNodeIn.Connect(newPortIn.Index, newNodeOut, newPortOut.Index, "", out error, copyLink);
            }
            else
            {
                Error(nom_fct + "\n" + string.Format("No link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\"", inPortName, inNodeName, outPortName, outNodeName));
            }

            if (error.Length != 0)
            {
                Error(nom_fct + "\n" + "Cannot connect");
            }

            if (layout == null)
                return true;

            layout.Invalidate();
            return true;
        }

        /// <summary>
        /// Disconnect all links of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of node we want to disconnect the links</param>
        /// <returns></returns>
        public static bool DisconnectAll(string inNodeName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("DisconnectAll(\"{0}\");", inNodeName);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }

            manager.CurCompound.UnConnectAll(nodeIn);


            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Disconnect all links from input ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of node we want to disconnect the links</param>
        /// <returns></returns>
        public static bool DisconnectInputs(string inNodeName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("DisconnectInputs(\"{0}\");", inNodeName);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }

            manager.CurCompound.UnConnectInputs(nodeIn);
        
            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Disconnect all links from output ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of node we want to disconnect the links</param>
        /// <returns></returns>
        public static bool DisconnectOutputs(string inNodeName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("DisconnectOutputs(\"{0}\");", inNodeName);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }

            manager.CurCompound.UnConnectOutputs(nodeIn);

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Parent inputNode with parentCompound
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <param name="parentCompound">Name of compound</param>
        /// <returns></returns>
        public static bool Parent(string inNodeName, string parentCompound)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("Parent(\"{0}\", \"{1}\");", inNodeName, parentCompound);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);
            Compound newParent = manager.GetNode(parentCompound) as Compound;

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }
            if (newParent == null)
            {
                Error(nom_fct + "\n" + string.Format("parent Compound \"{0}\" is null", parentCompound));
                return false;
            }

            if (nodeIn.Parent != null && nodeIn.Parent != newParent)
            {
                manager.MoveNodes(new List<Node> { nodeIn }, newParent);
            }
            else
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" and parent Compound \"{1}\" cannot be parented", inNodeName, parentCompound));
                return false;
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// UnParent inputNode
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <returns></returns>
        public static bool UnParent(string inNodeName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("UnParent(\"{0}\");", inNodeName);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }

            if (nodeIn.Parent != null && nodeIn.Parent.Parent != null)
            {
                manager.MoveNodes(new List<Node> { nodeIn }, nodeIn.Parent.Parent);
            }
            else
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" does not have parent", inNodeName));
                return false;
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Make visible all ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <returns></returns>
        public static bool ExposeAllPorts(string inNodeName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("ExposeAllPorts(\"{0}\");", inNodeName);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }

            foreach (Port port in nodeIn.Inputs)
            {
                PortInstance parentPort = nodeIn.Parent.GetPortFromNode(port);
                parentPort.Visible = true;
            }

            foreach (Port port in nodeIn.Outputs)
            {
                PortInstance parentPort = nodeIn.Parent.GetPortFromNode(port);
                parentPort.Visible = true;
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Hide all ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <returns></returns>
        public static bool HideAllPorts(string inNodeName)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("HideAllPorts(\"{0}\");", inNodeName);

            if (verbose)
                Log(nom_fct);

            Node nodeIn = manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                Error(nom_fct + "\n" + string.Format("input Node \"{0}\" is null", inNodeName));
                return false;
            }

            foreach (Port port in nodeIn.Inputs)
            {
                PortInstance parentPort = nodeIn.Parent.GetPortFromNode(port);

                if (!parentPort.IsLinked())
                {
                    parentPort.Visible = false;
                }
            }

            foreach (Port port in nodeIn.Outputs)
            {
                PortInstance parentPort = nodeIn.Parent.GetPortFromNode(port);

                if (!parentPort.IsLinked())
                {
                    parentPort.Visible = false;
                }
            }

            if (layout == null)
                return true;

            layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Create a compound with a list of input node
        /// </summary>
        /// <param name="inNodeNames">List of input nodes names</param>
        /// <returns></returns>
        public static bool CreateCompound(List<string> inNodeNames)
        {
            if (manager == null)
                return false;

            string nom_fct = string.Format("CreateCompound(\"{0}\");", inNodeNames);

            if (verbose)
                Log(nom_fct);

            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inNodeNames.Count > 0)
            {
                foreach (string NodeName in inNodeNames)
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
                    Error(nom_fct + string.Format("Not all node names in \"{0}\" exist", inNodeNames));
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
                Error(nom_fct + string.Format("\"{0}\" is empty", inNodeNames));
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
    }
}
