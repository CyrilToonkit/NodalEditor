using GenericUndoRedo;
using MiniLogger;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using TK.BaseLib;
using TK.BaseLib.CSCodeEval;
using TK.GraphComponents.Dialogs;
using TK.NodalEditor.NodesFramework;
using System.IO;

namespace TK.NodalEditor
{
    public class NodalDirector
    {
        public NodesManager manager = null;
        public NodesLayout.NodesLayout layout = null;

        public bool verbose = true;

        public UndoRedoHistory<NodalDirector> history;

        #region Singleton declaration, getters and constructor
        protected static NodalDirector _instance = null;

        protected NodalDirector()
        {
            history = new UndoRedoHistory<NodalDirector>(this);
        }

        public static NodalDirector Get()
        {
            if (_instance == null)
                _instance = new NodalDirector();

            return _instance;
        }

        public static NodalDirector Get(NodesManager inManager, NodesLayout.NodesLayout inLayout)
        {
            if (_instance == null)
                _instance = new NodalDirector();

            _instance.manager = inManager;
            _instance.layout = inLayout;

            return _instance;
        }

        #endregion

        #region undoRedo
        /// <summary>
        /// Checks if there are any stored state available on the undo stack.
        /// </summary>
        /// <returns>true if able to undo, false otherwise</returns>
        public static bool CanUndo()
        {
            return _instance.history.CanUndo;
        }

        /// <summary>
        /// Checks if there are any stored state available on the redo stack.
        /// </summary>
        /// <returns>true if able to redo, false otherwise</returns>
        public static bool CanRedo()
        {
            return _instance.history.CanRedo;
        }

        /// <summary>
        /// Undo last operation
        /// </summary>
        /// <returns>true if something was "undoed", false otherwise</returns>
        public static bool Undo()
        {
            if( _instance.history.CanUndo)
            {
                _instance.history.Undo();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Redo last "undoed" operation
        /// </summary>
        /// <returns>true if something was "redoed", false otherwise</returns>
        public static bool Redo()
        {
            if (_instance.history.CanRedo)
            {
                _instance.history.Redo();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear the entire undo and redo stacks.
        /// </summary>
        public static void ClearHistory()
        {
            _instance.history.Clear();
        }

        #endregion

        #region Active commands internals

        /// <summary>
        /// Adds a node (that was removed) given its instance
        /// </summary>
        /// <param name="inNode">Node to (re)add</param>
        /// <param name="inParent">Compound where we want to add the Node to</param>
        /// <param name="inConnexions">Connections that needs to be reapplied</param>
        internal void _AddNode(Node inNode, Compound inParent, NodeConnexions inConnexions)
        {
            inNode.Deleted = false;

            Node createdNode = _instance.manager.AddNode(inNode, inParent, (int)(inNode.UIx / layout.LayoutSize), (int)(inNode.UIy / layout.LayoutSize));

            if (inConnexions != null)
                inConnexions.Reconnect(createdNode);

            _instance.layout.Selection.UpdateSelection();

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        /// <summary>
        /// Deletes a Node given its instance
        /// </summary>
        /// <param name="removed">Node to be removed</param>
        internal void _DeleteNode(Node removed)
        {
            _instance.manager.RemoveNode(removed);
            removed.Deleted = true;

            if (_instance.layout == null)
                return;

            _instance.layout.RefreshPorts();
            _instance.layout.Selection.Selection.Clear();
            _instance.layout.ChangeFocus(true);
            _instance.manager.Companion.EndProcess();
        }

        /// <summary>
        /// Connect a link (that was removed) given its instance
        /// </summary>
        /// <param name="Link">Link to (re)connect</param>
        /// <param name="inMode"></param>
        internal void _Connect(Link inLink, string inMode)
        {
            string error = string.Empty;
            Link connectedLink = inLink.Target.Owner.Connect(inLink.Target.Index, inLink.Source.Owner, inLink.Source.Index, inMode, out error, inLink);


            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        /// <summary>
        /// Disconnect a link given its instance
        /// </summary>
        /// <param name="disconnected">Link to be disconnected</param>
        internal void _Disconnect(Link disconnected)
        {
            _instance.manager.CurCompound.UnConnect(disconnected);

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        /// <summary>
        /// ReConnect a link (that was disconnected) given its instance
        /// </summary>
        /// <param name="inNode"></param>
        /// <param name="outNode"></param>
        /// <param name="inPort"></param>
        /// <param name="outPort"></param>
        /// <param name="inLink"></param>
        /// <param name="inMode"></param>
        internal void _ReConnect(Node inNode, Node outNode, int inPort, int outPort, Link inLink, string inMode)
        {
            string error = string.Empty;
            _instance.manager.CurCompound.UnConnect(inLink);
            inNode.Connect(inPort, outNode, outPort, inMode, out error, inLink);


            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        /// <summary>
        /// Copy a link given its instance
        /// </summary>
        /// <param name="inNode"></param>
        /// <param name="inPort"></param>
        /// <param name="outNode"></param>
        /// <param name="outPort"></param>
        /// <param name="inLink"></param>
        /// <param name="inMode"></param>
        internal void _CopyLink(Node inNode, int inPort, Node outNode, int outPort, Link inLink, string inMode)
        {
            string error = string.Empty;
            Link copyLink = (Link)Activator.CreateInstance(inLink.GetType(), new object[0]);
            copyLink.Copy(inLink);
            inNode.Connect(inPort, outNode, outPort, inMode, out error, copyLink);


            if (_instance.layout == null)
            return;

            _instance.layout.Invalidate();
        }

        internal void _Parent(Node inNode, Compound inParent)
        {
            if (inNode.Parent != null && inNode.Parent != inParent)
            {
                _instance.manager.MoveNodes(new List<Node> { inNode }, inParent);
            }

            if (_instance.layout == null)
                return;

            _instance.layout.RefreshPorts();
            _instance.layout.Selection.Selection.Clear();
            _instance.layout.ChangeFocus(true);

            _instance.layout.Invalidate();
        }

        internal void _UnParent(Node inNode)
        {
            if (inNode.Parent != null && inNode.Parent.Parent != null)
            {
                _instance.manager.MoveNodes(new List<Node> { inNode }, inNode.Parent.Parent);
            }

            if (_instance.layout == null)
                return;

            _instance.layout.RefreshPorts();
            _instance.layout.Selection.Selection.Clear();
            _instance.layout.ChangeFocus(true);

            _instance.layout.Invalidate();
        }

        /// <summary>
        /// Create a compound
        /// </summary>
        /// <param name="inNodes">List of nodes</param>
        /// <param name="inCompound">Compound</param>
        internal void _CreateCompound(List<Node> inNodes, Compound inCompound)
        {
            Compound compound = _instance.manager.AddCompound(inNodes, inCompound);

            if (compound != null)
            {
                _instance.manager.EnterCompound(compound);
            }

            if (_instance.layout == null)
                return;

            _instance.layout.ChangeFocus(true);
            _instance.layout.Frame(_instance.manager.CurCompound.Nodes);
            _instance.layout.Invalidate();
        }

        /// <summary>
        /// Explode a compound given its instance
        /// </summary>
        /// <param name="inCompound">Compound we want to explode</param>
        internal void _Explode(Compound inCompound)
        {
            if(inCompound == _instance.manager.CurCompound)
            {
                _instance.manager.ExitCompound();
                _instance.layout.ChangeFocus(false);
            }

            _instance.manager.ExplodeCompound(inCompound);

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        /// <summary>
        /// Rename a node given its instance
        /// </summary>
        /// <param name="inNode">Node we want to rename</param>
        /// <param name="inNewName">New name for inNode</param>
        internal void _Rename(Node inNode, string inNewName)
        {
            inNode.FullName = _instance.manager.SetNodeUniqueName(inNewName, inNode);

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        #endregion
        
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

        #region Action Commands
 
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
            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("AddNode(\"{0}\", \"{1}\", {2}, {3});", inNodeName, inCompoundName, X, Y);

            if (_instance.verbose)
                Log(nom_fct);

            string nodeName = null;
            Compound inCompound = null;

            if(string.IsNullOrEmpty(inCompoundName) || inCompoundName == _instance.manager.Root.FullName)
            {
                inCompound = _instance.manager.Root;
            }
            else
            {
                inCompound = _instance.manager.GetNode(inCompoundName) as Compound;
            }

            if (inCompound == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Compound \"{0}\" does not exists!", inCompoundName));
            }

            bool isTrue = false;
            foreach (Node Node in _instance.manager.AvailableNodes)
            {
                if(inNodeName == Node.FullName)
                {
                    Node node = _instance.manager.AddNode(inNodeName, inCompound, X, Y);
                    _instance.history.Do(new AddNodeMemento(node.FullName));
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
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("No Node named \"{0}\"!", inNodeName));
            }


            if (_instance.layout == null)
                return nodeName;

            _instance.layout.ChangeFocus(false);
            _instance.layout.Invalidate();

            return nodeName;
        }

        /// <summary>
        /// Delete a List of nodes
        /// </summary>
        /// <param name="inNodesNames">List of nodes names</param>
        /// <returns></returns>
        public static bool DeleteNodes(List<string> inNodesNames)
        {
            if (_instance.manager == null)
                return false;

            if (_instance.verbose)
                Log(string.Format("DeleteNodes(new List<string>{{\"{0}\"}});", TypesHelper.Join(inNodesNames, "\",\"")));

            _instance.manager.Companion.LaunchProcess("Delete nodes", inNodesNames.Count);

            _instance.history.BeginCompoundDo();

            foreach (string nodeName in inNodesNames)
            {
                Node node = _instance.manager.GetNode(nodeName);

                if (node == null)
                {
                    string message = string.Format("Node '{0}' does not exists !", nodeName);
                    throw new NodalDirectorException(message);
                }
                else
                {
                    _instance.history.Do(new DeleteNodeMemento(node, node.Parent, new NodeConnexions(node)));
                    _instance.manager.RemoveNode(node);
                    node.Deleted = true;

                    _instance.manager.Companion.ProgressBarIncrement();
                }
            }

            _instance.history.EndCompoundDo();

            if (_instance.layout == null)
                return true;

            _instance.layout.RefreshPorts();
            _instance.layout.Selection.Selection.Clear();
            _instance.layout.ChangeFocus(true);
            _instance.manager.Companion.EndProcess();

            return true;
        }

        /// <summary>
        /// Delete a node
        /// </summary>
        /// <param name="inNodeName">nodes name</param>
        /// <returns></returns>
        public static bool DeleteNode(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            if (_instance.verbose)
                Log(string.Format("DeleteNode(\"{0}\");", inNodeName));


            Node node = _instance.manager.GetNode(inNodeName);

            if (node == null)
            {
                string message = string.Format("Node '{0}' does not exists !", inNodeName);
                throw new NodalDirectorException(message);
            }
            else
            {
                _instance.history.Do(new DeleteNodeMemento(node, node.Parent, new NodeConnexions(node)));
                _instance.manager.RemoveNode(node);
                node.Deleted = true;
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.RefreshPorts();
            _instance.layout.Selection.Selection.Clear();
            _instance.layout.ChangeFocus(true);
            _instance.manager.Companion.EndProcess();

            return true;
        }

        /// <summary>
        /// Duplicate a node
        /// </summary>
        /// <param name="inNodeName">Name of node we want to duplicate</param>
        /// <returns></returns>
        public static string Duplicate(string inNodeName)
        {
            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("Duplicate(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Input Node \"{0}\" does not exists!", inNodeName));
            }

            Node newNode = _instance.manager.Copy(nodeIn, _instance.manager.CurCompound, (int)((nodeIn.UIx) + (30 * _instance.layout.LayoutSize) / _instance.layout.LayoutSize), (int)((nodeIn.UIy) - (10 * _instance.layout.LayoutSize) / _instance.layout.LayoutSize));

            if (newNode == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot duplicate \"{0}\"", inNodeName));
            }

            if (_instance.layout == null)
                return null;


            _instance.layout.ChangeFocus(true);
            _instance.layout.Frame(_instance.manager.CurCompound.Nodes);
            _instance.layout.Invalidate();

            return newNode.FullName;
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
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("Disconnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\");", inNodeName, inPortName, outNodeName, outPortName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);
            Node nodeOut = _instance.manager.GetNode(outNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }
            if (nodeOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", outNodeName));
            }

            Port portIn = nodeIn.GetPort(inPortName, false);
            Port portOut = nodeOut.GetPort(outPortName, true);

            if (portIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{0}\" does not exist!", inNodeName, inPortName));
            }
            if (portOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{0}\" does not exist!", outNodeName, outPortName));
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
                        _instance.manager.CurCompound.UnConnect(link);
                        _instance.history.Do(new DisconnectMemento(link));
                    }
                }
                else
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\" does not exist!", inPortName, inNodeName, outPortName, outNodeName));
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

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
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("Connect(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\");", inNodeName, inPortName, outNodeName, outPortName, inMode);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);
            Node nodeOut = _instance.manager.GetNode(outNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }
            if (nodeOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", outNodeName));
            }

            Port portOut = nodeOut.GetPort(outPortName, true);
            Port portIn = nodeIn.GetPort(inPortName, false);

            if (portIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
            }
            if (portOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", outNodeName, outPortName));
            }

            string error=string.Empty;

            Link connected = nodeIn.Connect(portIn.Index, nodeOut, portOut.Index, inMode, out error, nodeIn.Companion.Manager.Preferences.CheckCycles);
            _instance.history.Do(new ConnectMemento(connected, inMode));

            if (error.Length != 0)
            {
                throw new NodalDirectorException(nom_fct + "\n" + "Cannot connect");
            }
  
            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

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
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("ReConnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\");", inNodeName, inPortName, outNodeName, outPortName, newinNodeName, newinPortName, newoutNodeName, newoutPortName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeInLocked = _instance.manager.GetNode(inNodeName);
            Node nodeOut = _instance.manager.GetNode(outNodeName);
            Node newNodeIn = _instance.manager.GetNode(newinNodeName);
            Node newNodeOut = _instance.manager.GetNode(newoutNodeName);

            if (nodeInLocked == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }
            if (nodeOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", outNodeName));
            }
            if (newNodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", newinNodeName));
            }
            if (newNodeOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", newoutNodeName));
            }

            Port portOut = nodeOut.GetPort(outPortName, true);
            Port portInLocked = nodeInLocked.GetPort(inPortName, false); 
            Port newPortOut = newNodeOut.GetPort(newoutPortName, true);
            Port newPortIn = newNodeIn.GetPort(newinPortName, false);

            if (portInLocked == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
            }
            if (portOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", outNodeName, outPortName));
            }
            if (newPortIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", newinNodeName, newinPortName));
            }
            if (newPortOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", newoutNodeName, newoutPortName));
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
                        _instance.manager.CurCompound.UnConnect(link);
                        
                    }
                }
                else
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\" does not exist!", inPortName, inNodeName, outPortName, outNodeName));
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
            }

            if (linkToDisconnect.Count != 0)
            {
                newNodeIn.Connect(newPortIn.Index, newNodeOut, newPortOut.Index, "", out error, linkToDisconnect[0]);
                _instance.history.Do(new ReconnectMemento(inNodeName, outNodeName, inPortName, outPortName, linkToDisconnect[0], ""));
            }

            if (error.Length != 0)
            {
                throw new NodalDirectorException(nom_fct + "\n" + "Cannot ReConnect");
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();
            return true;
        }

        /// <summary>
        /// Create copy of a link
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
        public static bool CopyLink(    string inNodeName, string inPortName, string outNodeName, string outPortName,
                                        string newinNodeName, string newinPortName, string newoutNodeName, string newoutPortName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("CopyLink(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\");", inNodeName, inPortName, outNodeName, outPortName, newinNodeName, newinPortName, newoutNodeName, newoutPortName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeInLocked = _instance.manager.GetNode(inNodeName);
            Node nodeOut = _instance.manager.GetNode(outNodeName);
            Node newNodeIn = _instance.manager.GetNode(newinNodeName);
            Node newNodeOut = _instance.manager.GetNode(newoutNodeName);

            if (nodeInLocked == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }
            if (nodeOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", outNodeName));
            }
            if (newNodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", newinNodeName));
            }
            if (newNodeOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", newoutNodeName));
            }

            Port portOut = nodeOut.GetPort(outPortName, true);
            Port portInLocked = nodeInLocked.GetPort(inPortName, false);
            Port newPortOut = newNodeOut.GetPort(newoutPortName, true);
            Port newPortIn = newNodeIn.GetPort(newinPortName, false);

            if (portInLocked == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
            }
            if (portOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", outNodeName, outPortName));
            }
            if (newPortIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", newinNodeName, newinPortName));
            }
            if (newPortOut == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", newoutNodeName, newoutPortName));
            }

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
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
            }

            Link copyLink = (Link)Activator.CreateInstance(linkToConnect[0].GetType(), new object[0]);
            if (linkToConnect.Count != 0)
            {
                copyLink.Copy(linkToConnect[0]);
                newNodeIn.Connect(newPortIn.Index, newNodeOut, newPortOut.Index, "", out error, copyLink);
                _instance.history.Do(new CopyLinkMemento(inNodeName, outNodeName, inPortName, outPortName, copyLink, ""));
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\" does not exist!", inPortName, inNodeName, outPortName, outNodeName));
            }

            if (error.Length != 0)
            {
                throw new NodalDirectorException(nom_fct + "\n" + "Cannot Copy the link");
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();
            return true;
        }

        /// <summary>
        /// Disconnect all links of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of node we want to disconnect the links</param>
        /// <returns></returns>
        public static bool DisconnectAll(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("DisconnectAll(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            _instance.history.BeginCompoundDo();
            if (nodeIn is Compound)
            {
                //If this is a compound, remove all links towards external rigs
                Compound curComp = nodeIn as Compound;

                List<Link> ToRemove;

                    ToRemove = nodeIn.InDependencies;

                    foreach (Link Dep in ToRemove)
                    {
                        if (!Dep.Source.Owner.IsIn(curComp))
                        {
                            _instance.history.Do(new DisconnectMemento(Dep));
                            _instance.manager.CurCompound.UnConnect(Dep);
                        }
                    }


                    ToRemove = nodeIn.OutDependencies;

                    foreach (Link Dep in ToRemove)
                    {
                        if (!Dep.Target.Owner.IsIn(curComp))
                        {
                            _instance.history.Do(new DisconnectMemento(Dep));
                            _instance.manager.CurCompound.UnConnect(Dep);
                        }
                    }
            }
            else //If this is a node, simply remove all links
            {
                List<Link> ToRemove = new List<Link>();

                ToRemove.AddRange(nodeIn.InDependencies);
                ToRemove.AddRange(nodeIn.OutDependencies);

               
                foreach (Link Dep in ToRemove)
                {
                    _instance.history.Do(new DisconnectMemento(Dep));
                    _instance.manager.CurCompound.UnConnect(Dep);
                }
            }
            _instance.history.EndCompoundDo();
            //_instance.manager.CurCompound.UnConnectAll(nodeIn);


            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Disconnect all links from input ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of node we want to disconnect the links</param>
        /// <returns></returns>
        public static bool DisconnectInputs(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("DisconnectInputs(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }


            //_instance.manager.CurCompound.UnConnectInputs(nodeIn);
            _instance.history.BeginCompoundDo();
            if (nodeIn is Compound)
            {
                //If this is a compound, remove all links towards external rigs
                Compound curComp = nodeIn as Compound;

                List<Link> ToRemove;

                ToRemove = nodeIn.InDependencies;

                foreach (Link Dep in ToRemove)
                {
                    if (!Dep.Source.Owner.IsIn(curComp))
                    {
                        _instance.history.Do(new DisconnectMemento(Dep));
                        _instance.manager.CurCompound.UnConnect(Dep);
                    }
                }
            }
            else //If this is a node, simply remove all links
            {
                List<Link> ToRemove = new List<Link>();

                ToRemove.AddRange(nodeIn.InDependencies);
                
                foreach (Link Dep in ToRemove)
                {
                    _instance.history.Do(new DisconnectMemento(Dep));
                    _instance.manager.CurCompound.UnConnect(Dep);
                }
            }
            _instance.history.EndCompoundDo();

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Disconnect all links from output ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of node we want to disconnect the links</param>
        /// <returns></returns>
        public static bool DisconnectOutputs(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("DisconnectOutputs(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            //_instance.manager.CurCompound.UnConnectOutputs(nodeIn);

            _instance.history.BeginCompoundDo();
            if (nodeIn is Compound)
            {
                //If this is a compound, remove all links towards external rigs
                Compound curComp = nodeIn as Compound;

                List<Link> ToRemove;


                    ToRemove = nodeIn.OutDependencies;

                    foreach (Link Dep in ToRemove)
                    {
                        if (!Dep.Target.Owner.IsIn(curComp))
                        {
                            _instance.history.Do(new DisconnectMemento(Dep));
                            _instance.manager.CurCompound.UnConnect(Dep);
                        }
                    }
                
            }
            else //If this is a node, simply remove all links
            {
                List<Link> ToRemove = new List<Link>();

                ToRemove.AddRange(nodeIn.OutDependencies);
                

                foreach (Link Dep in ToRemove)
                {
                    _instance.history.Do(new DisconnectMemento(Dep));
                    _instance.manager.CurCompound.UnConnect(Dep);
                }
            }
            _instance.history.EndCompoundDo();

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Parent inputNode with parentCompound
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <param name="parentCompound">Name of compound</param>
        /// <returns></returns>
        public static bool ParentNode(string inNodeName, string parentCompound)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("ParentNode(\"{0}\", \"{1}\");", inNodeName, parentCompound);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);
            Compound newParent = _instance.manager.GetNode(parentCompound) as Compound;

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }
            if (newParent == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("parent Compound \"{0}\" does not exist!", parentCompound));
            }


            if (nodeIn.Parent != null && nodeIn.Parent != newParent)
            {
                _instance.history.Do(new ParentMemento(nodeIn.FullName, newParent.FullName));
                _instance.manager.MoveNodes(new List<Node> { nodeIn }, newParent);
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" and parent Compound \"{1}\" cannot be parented", inNodeName, parentCompound));
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Parent a List of inputNodes with parentCompound
        /// </summary>
        /// <param name="inNodeNames">List of input node Names</param>
        /// <param name="parentCompound">Name of compound</param>
        /// <returns></returns>
        public static bool ParentNodes(List<string> inNodeNames, string parentCompound)
        {
            if (_instance.manager == null)
                return false;


            string nom_fct = string.Format("ParentNodes(\"{0}\", \"{1}\");", TypesHelper.Join(inNodeNames, "\",\""), parentCompound);

            if (_instance.verbose)
                Log(nom_fct);

            List<Node> Nodes = new List<Node>();
            Compound newParent = _instance.manager.GetNode(parentCompound) as Compound;


            if (newParent == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("parent Compound \"{0}\" does not exist!", parentCompound));
            }

            foreach (string NodeName in inNodeNames)
            {
                Node nodeIn = _instance.manager.GetNode(NodeName);
                if (nodeIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Input Node \"{0}\" does not exist!", NodeName));
                }
                else
                {
                    if (nodeIn.Parent != null && nodeIn.Parent != newParent)
                    {
                        Nodes.Add(nodeIn);
                    }
                    else
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Input Node \"{0}\" and parent Compound \"{1}\" cannot be parented", nodeIn.FullName, parentCompound));
                    }
                    
                }
            }

            _instance.history.BeginCompoundDo();
            foreach (Node Node in Nodes)
            {
                _instance.history.Do(new ParentMemento(Node.FullName, newParent.FullName));
                _instance.manager.MoveNodes(new List<Node> { Node }, newParent);
            }
            _instance.history.EndCompoundDo();

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// UnParent inputNode
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <returns></returns>
        public static bool UnParentNode(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("UnParentNode(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }
            _instance.history.BeginCompoundDo();
            if (nodeIn.Parent != null && nodeIn.Parent.Parent != null)
            {
                _instance.history.Do(new UnParentMemento(nodeIn.FullName, nodeIn.Parent.FullName));
                _instance.manager.MoveNodes(new List<Node> { nodeIn }, nodeIn.Parent.Parent);
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not have parent", inNodeName));
            }
            _instance.history.EndCompoundDo();
            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// UnParent a List of inputNode
        /// </summary>
        /// <param name="inNodeNames">List of input node Names</param>
        /// <returns></returns>
        public static bool UnParentNodes(List<string> inNodeNames)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("UnParentNodes(\"{0}\");", TypesHelper.Join(inNodeNames, "\",\""));

            if (_instance.verbose)
                Log(nom_fct);

            List<Node> Nodes = new List<Node>();

            foreach(string NodeName in inNodeNames)
            {
                Node nodeIn = _instance.manager.GetNode(NodeName);

                if (nodeIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", NodeName));
                }
                else
                {
                    if (nodeIn.Parent != null && nodeIn.Parent.Parent != null)
                    {
                        Nodes.Add(nodeIn);
                    }
                    else
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Input Node \"{0}\" does not have parent", NodeName));
                    }
                }
            }

            _instance.history.BeginCompoundDo();
            foreach(Node Node in Nodes)
            {
                _instance.history.Do(new UnParentMemento(Node.FullName, Node.Parent.FullName));
                _instance.manager.MoveNodes(new List<Node> { Node }, Node.Parent.Parent);
            }
            _instance.history.EndCompoundDo();

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Make visible all ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <returns></returns>
        public static bool ExposeAllPorts(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("ExposeAllPorts(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
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

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Hide all ports of inputNode
        /// </summary>
        /// <param name="inNodeName">Name of input node</param>
        /// <returns></returns>
        public static bool HideAllPorts(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("HideAllPorts(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
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

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Create a compound with a list of input node
        /// </summary>
        /// <param name="inNodeNames">List of input nodes names</param>
        /// <returns></returns>
        public static bool CreateCompound(List<string> inNodeNames)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = "CreateCompound(new List {\"TestNode1\"});";


            if (_instance.verbose)
                Log(nom_fct);

            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inNodeNames.Count > 0)
            {
                foreach (string NodeName in inNodeNames)
                {
                    Node Node = _instance.manager.GetNode(NodeName);

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
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Not all node names in \"{0}\" exist", inNodeNames));
                }
                else //All the nodes name exist
                {
                    Compound compound = _instance.manager.AddCompound(nodes);

                    if (compound != null)
                    {
                        _instance.manager.EnterCompound(compound);
                        _instance.history.Do(new CreateCompoundMemento(nodes, compound));
                    }
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + "Cannot Create Compound, List<string> has no element!");
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Explode a compound
        /// </summary>
        /// <param name="inCompoundName">Compound Name to explode</param>
        /// <returns></returns>
        public static bool Explode(string inCompoundName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("Explode(\"{0}\");", inCompoundName);

            if (_instance.verbose)
                Log(nom_fct);

            Compound Compound = _instance.manager.GetNode(inCompoundName) as Compound;

            if (Compound != null)
            {
                if (Compound == _instance.manager.CurCompound)
                {
                    _instance.manager.ExitCompound();
                    _instance.layout.ChangeFocus(true);
                }

                _instance.history.Do(new ExplodeMemento(Compound, new List<Node>(Compound.Nodes)));
                _instance.manager.ExplodeCompound(Compound);
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Compound \"{0}\" does not exist!", inCompoundName));
            }

            if (_instance.layout == null)
                return false;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Rename a node or compound
        /// </summary>
        /// <param name="inName">Node or Compound Name</param>
        /// <param name="inNewName">New name</param>
        /// <returns></returns>
        public static string Rename(string inName, string inNewName)
        {
            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("Rename(\"{0}\", \"{1}\");", inNewName, inNewName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inName);
            
            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inName));
            }

            string UniqueName = _instance.manager.SetNodeUniqueName(inNewName, nodeIn);
            _instance.history.Do(new RenameMemento(nodeIn, inNewName));

            if (UniqueName == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Rename Input Node \"{0}\" with \"{1}\"", inName, inNewName));
            }
            else
            {
                nodeIn.FullName = UniqueName;
            }

            if (_instance.layout == null)
                return null;

            _instance.layout.Invalidate();

            return nodeIn.FullName;
        }

        #endregion

        #region Getters Commands

        /// <summary>
        /// Getting selected nodes
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSelectedNodes()
        {
            List<string> NodeNames = new List<string>();

            foreach (NodeBase elem in _instance.layout.Selection.Selection)
            {
                if (elem is Node)
                {
                    NodeNames.Add((elem as Node).FullName);
                }
            }

            return NodeNames;
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Select a List of nodes
        /// </summary>
        /// <param name="inNodeNames">List of node Names</param>
        /// <returns></returns>
        public static bool SelectNodes(List<string> inNodeNames)
        {
            return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Default);
        }

        /// <summary>
        /// Select a List of nodes with selection Types
        /// </summary>
        /// <param name="inNodeNames">List of node Names</param>
        /// <param name="inType">Type of Selection : "Default", "Add", "Toggle", "RemoveFrom"</param>
        /// <returns></returns>
        public static bool SelectNodes(List<string> inNodeNames, string inType)
        {
            if(inType == "Default")
                return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Default);
            else if (inType == "Add")
                return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Add);
            else if (inType == "Toggle")
                return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Toggle);
            else if (inType == "RemoveFrom")
                return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.RemoveFrom);
            else
            {
                throw new NodalDirectorException("Cannot Select the Nodes, Wrong inType");
            }
        }

        /// <summary>
        /// Select a List of nodes with selection Types
        /// </summary>
        /// <param name="inNodeNames">List of node Names</param>
        /// <param name="inType">Type of Selection : Default, Add, Toggle, RemoveFrom</param>
        /// <returns></returns>
        public static bool SelectNodes(List<string> inNodeNames, NodesLayout.TypeOfSelection inType)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("SelectNodes(new List<string>{{\"{0}\"}});", TypesHelper.Join(inNodeNames, "\",\""));

            if (_instance.verbose)
                Log(nom_fct);

            List<string> nodesNameError = new List<string>();
            List<Node> nodes = new List<Node>();

            if (inNodeNames.Count > 0)
            {
                foreach (string NodeName in inNodeNames)
                {
                    Node Node = _instance.manager.GetNode(NodeName);

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
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Not all node names exist in List<string>{{\"{0}\"}} ", TypesHelper.Join(inNodeNames, "\",\"")));
                }
                else //All the nodes name exist
                {
                    switch (inType)
                    {
                        case NodesLayout.TypeOfSelection.Default:
                            _instance.layout.Selection.Select(nodes);
                            break;
                        case NodesLayout.TypeOfSelection.Add:
                            foreach (Node node in nodes)
                            {
                                _instance.layout.Selection.AddToSelection(node);
                            }
                            break;
                        case NodesLayout.TypeOfSelection.Toggle:
                            foreach (Node node in nodes)
                            {
                                _instance.layout.Selection.ToggleSelection(node);
                            }
                            break;
                        case NodesLayout.TypeOfSelection.RemoveFrom:
                            foreach (Node node in nodes)
                            {
                                _instance.layout.Selection.RemoveFromSelection(node);
                            }
                            break;
                    }
                }
            }
            else
            {
                _instance.layout.Selection.DeselectAll();
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            return true;
        }

        /// <summary>
        /// Open a file
        /// </summary>
        /// <param name="inPath">Path where the file is. Be carefull to double the "\" in "\\"</param>
        /// <returns></returns>
        public static bool Open(string inPath)
        {
            return Open(inPath, true);
        }

        /// <summary>
        /// Open a file
        /// </summary>
        /// <param name="inPath">Path where the file is. Be carefull to double the "\" in "\\"</param>
        /// <param name="inForce"></param>
        /// <returns></returns>
        public static bool Open(string inPath, bool inForce)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("Open(\"{0}\", {1});", inPath, inForce);

            if (_instance.verbose)
                Log(nom_fct);

            Compound openedComp = null;

            using (FileStream fileStream = new FileStream(inPath, FileMode.Open))
            {
                openedComp = NodesSerializer.GetInstance().CompoundSerializers["Default"].Deserialize(fileStream) as Compound;
            }

            if (openedComp != null)
            {
                _instance.manager.NewLayout(openedComp, false);
                _instance.layout.ChangeFocus(true);
                _instance.layout.Frame(_instance.manager.CurCompound.Nodes);
                _instance.layout.Invalidate();
            }

            return true;
        }

        /// <summary>
        /// New layout
        /// </summary>
        /// <returns></returns>
        public static bool New()
        {
            return New(true);
        }

        /// <summary>
        /// New layout
        /// </summary>
        /// <param name="inForce"></param>
        /// <returns></returns>
        public static bool New(bool inForce)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("New({0});", inForce);

            if (_instance.verbose)
                Log(nom_fct);

            _instance.manager.NewLayout();
            _instance.layout.Invalidate();

            return true;
        }

        #endregion

        /// <summary>
        /// Executes arbitrary C# code at runtime
        /// </summary>
        /// <param name="inCode">The code to execute</param>
        public static void Evaluate(string inCode)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            InterpreterResult rslt = CSInterpreter.Eval(inCode.Replace("cmds.", "NodalDirector."), string.Empty, "TK_BaseLib.dll;TK_GraphComponents.dll;TK_NodalEditor.dll;", "using System.Collections.Generic;using TK.BaseLib;using TK.BaseLib.CGModel;using TK.GraphComponents.Dialogs;using TK.NodalEditor;using TK.NodalEditor.NodesLayout;", args, false);
            string msg = "No info !";

            if (!rslt.Success)
            {
                if (rslt.Output is CompilerErrorCollection)
                {
                    CompilerErrorCollection errors = rslt.Output as CompilerErrorCollection;

                    msg = string.Empty;

                    foreach (CompilerError error in errors)
                    {
                        msg += string.Format("Error in \"Interpreter\" line {0} > {1}\n{2}\n", error.Line - 9, inCode.Split("\n".ToCharArray())[error.Line - 10], error.ErrorText);
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
