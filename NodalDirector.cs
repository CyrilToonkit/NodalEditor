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
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;

namespace TK.NodalEditor
{
    public class NodalDirector
    {
        public delegate void DelegateHaveChanged(bool NewStatus);
        public static event DelegateHaveChanged ChangedStatus;
        public bool haveChanged = false;
        
        public void ChangeOnStatus()
        {
            if(ChangedStatus != null)
            {
                ChangedStatus(haveChanged);
            }
        }

        //public event HaveChangedEventHandler HaveChangedEvent;
        //public delegate void HaveChangedEventHandler(object sender, NodesChangedEventArgs e);

        //public class NodesChangedEventArgs : EventArgs
        //{
        //    public NodesChangedEventArgs(bool HaveChanged)
        //    {
        //        _changed = HaveChanged;
        //    }

        //    public bool _changed;
        //}

        public NodesManager manager = null;
        public NodesLayout.NodesLayout layout = null;

        public bool verbose = true;

        public UndoRedoHistory<NodalDirector> history;
        public UndoRedoHistory<NodalDirector> historyUI;

        #region Singleton declaration, getters and constructor
        protected static NodalDirector _instance = null;

        protected NodalDirector()
        {
            history = new UndoRedoHistory<NodalDirector>(this);
            historyUI = new UndoRedoHistory<NodalDirector>(this);
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

        public static bool CanUndoUI()
        {
            return _instance.historyUI.CanUndo;
        }

        /// <summary>
        /// Checks if there are any stored state available on the redo stack.
        /// </summary>
        /// <returns>true if able to redo, false otherwise</returns>
        public static bool CanRedo()
        {
            return _instance.history.CanRedo;
        }

        public static bool CanRedoUI()
        {
            return _instance.historyUI.CanRedo;
        }

        /// <summary>
        /// Undo last operation
        /// </summary>
        /// <returns>true if something was "undoed", false otherwise</returns>
        public static bool Undo()
        {
            if (_instance.history.CanUndo)
            {
                _instance.history.Undo();
                return true;
            }
            return false;
        }

        public static bool UndoUI()
        {
            if (_instance.historyUI.CanUndo)
            {
                _instance.historyUI.Undo();
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

        public static bool RedoUI()
        {
            if (_instance.historyUI.CanRedo)
            {
                _instance.historyUI.Redo();
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
            _instance.historyUI.Clear();
        }

        #endregion

        #region Active commands internals

        /// <summary>
        /// Adds a node (that was removed) given its instance
        /// </summary>
        /// <param name="inNode">Node to (re)add</param>
        /// <param name="inParent">Compound where we want to add the Node to</param>
        /// <param name="inConnexions">Connections that needs to be reapplied</param>
        internal void _AddNode(Node inNode, Compound inParent, NodeConnexions inConnexions, int inXOffset, int inYOffset)
        {
            inNode.Deleted = false;

            Node createdNode = _instance.manager.AddNode(inNode, inParent, (int)(inNode.UIx - (inXOffset / layout.LayoutSize)), (int)(inNode.UIy - (inYOffset / layout.LayoutSize)));


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

        public void _DeletePort(Port inPort)
        {
            inPort.Owner.RemovePort(inPort);
            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
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
        //internal void _CopyLink(Node inNode, int inPort, Node outNode, int outPort, Link inLink, string inMode)
        //{
        //    string error = string.Empty;
        //    Link copyLink = (Link)Activator.CreateInstance(inLink.GetType(), new object[0]);
        //    copyLink.Copy(inLink);
        //    inNode.Connect(inPort, outNode, outPort, inMode, out error, copyLink);


        //    if (_instance.layout == null)
        //        return;

        //    _instance.layout.Invalidate();
        //}

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
            if (inCompound == _instance.manager.CurCompound)
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

        internal void _MoveNode(Node inNode, int inX, int inY)
        {
            inNode.UIx = (int)(inX * (1 / _instance.layout.LayoutSize));
            inNode.UIy = (int)(inY * (1 / _instance.layout.LayoutSize));

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        internal void _Paste(Node inNode, int inXOffset, int inYOffset, string inSearch, string inReplace)
        {
            int XOffset = inXOffset - (int)_instance.manager.ClipBoard[0].UIx;
            int YOffset = inYOffset - (int)_instance.manager.ClipBoard[0].UIy;

            if (string.IsNullOrEmpty(inSearch))
            {
                foreach (Node node in _instance.manager.ClipBoard)
                {
                    _instance.manager.Copy(node, _instance.manager.CurCompound, (int)((node.UIx + XOffset - 30) / _instance.layout.LayoutSize), (int)((node.UIy + YOffset - 10) / _instance.layout.LayoutSize));
                }
            }
            else
            {
                foreach (Node node in _instance.manager.ClipBoard)
                {
                    _instance.manager.Copy(node, _instance.manager.CurCompound, (int)((node.UIx + XOffset - 30) / _instance.layout.LayoutSize), (int)((node.UIy + YOffset - 10) / _instance.layout.LayoutSize), inSearch, inReplace);
                }
            }

            if (_instance.layout == null)
                return;

            _instance.layout.ChangeFocus(true);
            _instance.layout.Frame(_instance.manager.CurCompound.Nodes);
            _instance.layout.Invalidate();

        }

        internal void _SetProperty(Node inNode, string inPropertyName, object inValue)
        {
            switch (inPropertyName)
            {
                case "Name":
                    Rename(inNode.FullName, (string)inValue);
                    break;
                default:
                    var prop = inNode.GetType().GetProperty(inPropertyName);
                    Type inValueType = inValue.GetType();
                    prop.SetValue(inNode, inValue);
                    break;
            }

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        internal void _SetPortProperty(Port inPort, string inPropertyName, object inValue)
        {
            switch (inPropertyName)
            {
                case "Visible":
                    inPort.Visible = (bool)inValue;
                    break;
            }

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        internal void _SelectNodes(NodeBase[] inNodesBase, List<Node> inNodes)
        {
            List<Node> nodes = new List<Node>();
            for(int i =0; i< inNodesBase.Length; i++)
            {
                nodes.Add((Node)inNodesBase[i]);
            }
            if(inNodes != null)
            {
                foreach (Node node in inNodes)
                {
                    if (!nodes.Contains(node))
                        nodes.Add(node);
                }
            }
            _instance.layout.Selection.Select(nodes);

            if (_instance.layout == null)
                return;

            _instance.layout.Invalidate();
        }

        internal void _DeselectNodes(NodeBase[] inNodesBase, List<Node> inNodes)
        {

            //foreach (Node node in inNodesBase)
            //{
            //    _instance.layout.Selection.RemoveFromSelection(node);
            //}

            //List<Node> nodes = new List<Node>();
            //for (int i = 0; i < inNodesBase.Length; i++)
            //{
            //    nodes.Add((Node)inNodesBase[i]);
            //}
            //_instance.layout.Selection.Select(nodes);
            if (inNodes == null)
            {
                foreach (Node node in inNodesBase)
                {
                    _instance.layout.Selection.RemoveFromSelection(node);
                }
            }
            else if (inNodes != null)
            {
                foreach(Node node in inNodes)
                {
                    _instance.layout.Selection.RemoveFromSelection(node);
                }
            }

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
                Info(nom_fct);

            string nodeName = null;
            Compound inCompound = null;

            if (string.IsNullOrEmpty(inCompoundName) || inCompoundName == _instance.manager.Root.FullName)
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
                if (inNodeName == Node.FullName)
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

            if (isTrue == false)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("No Node named \"{0}\"!", inNodeName));
            }
            

            if (_instance.layout == null)
                return nodeName;

            _instance.layout.ChangeFocus(false);
            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
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
                Info(string.Format("DeleteNodes(new List<string>{{\"{0}\"}});", TypesHelper.Join(inNodesNames, "\",\"")));

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
                    _instance.history.Do(new DeleteNodeMemento(node, node.Parent, new NodeConnexions(node), 0, 0));
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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
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
                Info(string.Format("DeleteNode(\"{0}\");", inNodeName));


            Node node = _instance.manager.GetNode(inNodeName);

            if (node == null)
            {
                string message = string.Format("Node '{0}' does not exists !", inNodeName);
                throw new NodalDirectorException(message);
            }
            else
            {
                _instance.history.Do(new DeleteNodeMemento(node, node.Parent, new NodeConnexions(node), 0, 0));
                _instance.manager.RemoveNode(node);
                node.Deleted = true;
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.RefreshPorts();
            _instance.layout.Selection.Selection.Clear();
            _instance.layout.ChangeFocus(true);
            _instance.manager.Companion.EndProcess();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
            return true;
        }

        /// <summary>
        /// Delete a port
        /// </summary>
        /// <param name="inNodeName">Node name of the port</param>
        /// <param name="inPortName">Port name</param>
        /// <returns></returns>
        public static bool DeletePort(string inNodeName, string inPortName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("DeletePort(\"{0}\", \"{1}\");", inNodeName, inPortName);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            Port portIn = nodeIn.GetPort(inPortName, false);

            if (portIn == null)
            {
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portIn = nodeIn.GetPort(inPortName2, false);
                if (portIn == null)
                {
                    return false;
                }
            }

            nodeIn.RemovePort(portIn);

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
            return true;
        }

        /// <summary>
        /// Duplicate a node
        /// </summary>
        /// <param name="inNodeName">Name of node we want to duplicate</param>
        /// <returns></returns>
        public static string Duplicate(string inNodeName)
        {
            return Duplicate(inNodeName, null, null);
        }

        public static string Duplicate(string inNodeName, string inSearch, string inReplace)
        {

            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("Duplicate(\"{0}\", \"{1}\", \"{2}\");", inNodeName, inSearch, inReplace);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Input Node \"{0}\" does not exists!", inNodeName));
            }

            Node newNode = new Node();
            int inXOffset = (int)(30 / _instance.layout.LayoutSize);
            int inYOffset = (int)(-10 / _instance.layout.LayoutSize);

            if (string.IsNullOrEmpty(inSearch))
            {
                //newNode = _instance.manager.Copy(nodeIn, _instance.manager.CurCompound, (int)(nodeIn.UIx + (30 / _instance.layout.LayoutSize)), (int)(nodeIn.UIy - (10 / _instance.layout.LayoutSize)));
                newNode = _instance.manager.Copy(nodeIn, _instance.manager.CurCompound, (int)(nodeIn.UIx + inXOffset), (int)(nodeIn.UIy + inYOffset));
                if (newNode == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot duplicate \"{0}\"", inNodeName));
                }
                _instance.history.Do(new ReAddNodeMemento(newNode, newNode.Parent, new NodeConnexions(newNode), inXOffset, inYOffset));
            }
            else
            {
                //newNode = _instance.manager.Copy(nodeIn, _instance.manager.CurCompound, (int)(nodeIn.UIx + (30 / _instance.layout.LayoutSize)), (int)(nodeIn.UIy - (10 / _instance.layout.LayoutSize)), inSearch, inReplace);
                newNode = _instance.manager.Copy(nodeIn, _instance.manager.CurCompound, (int)(nodeIn.UIx + inXOffset), (int)(nodeIn.UIy + inYOffset));
                if (newNode == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot duplicate \"{0}\"", inNodeName));
                }
                _instance.history.Do(new ReAddNodeMemento(newNode, newNode.Parent, new NodeConnexions(newNode), inXOffset, inYOffset));
            }

            if (_instance.layout == null)
                return null;


            _instance.layout.ChangeFocus(true);
            _instance.layout.Frame(_instance.manager.CurCompound.Nodes);
            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
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
        //public static bool Disconnect(string inNodeName, string inPortName, string outNodeName, string outPortName)
        //{
        //    if (_instance.manager == null)
        //        return false;

        //    string nom_fct = string.Format("Disconnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\");", inNodeName, inPortName, outNodeName, outPortName);

        //    if (_instance.verbose)
        //        Log(nom_fct);

        //    Node nodeIn = _instance.manager.GetNode(inNodeName);
        //    Node nodeOut = _instance.manager.GetNode(outNodeName);

        //    if (nodeIn == null)
        //    {
        //        throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
        //    }
        //    if (nodeOut == null)
        //    {
        //        throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", outNodeName));
        //    }

        //    Port portIn = nodeIn.GetPort(inPortName, false);
        //    Port portOut = nodeOut.GetPort(outPortName, true);

        //    if (portIn == null)
        //    {
        //        string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
        //        portIn = nodeIn.GetPort(inPortName2, false);
        //        if (portIn == null)
        //        {
        //            throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{0}\" does not exist!", inNodeName, inPortName));
        //        }
        //    }
        //    if (portOut == null)
        //    {
        //        string outPortName2 = string.Format("{0}_{1}", outNodeName, outPortName);
        //        portOut = nodeOut.GetPort(outPortName2, true);
        //        if (portOut == null)
        //        {
        //            throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{0}\" does not exist!", outNodeName, outPortName));
        //        }
        //    }

        //    if (portIn.Dependencies.Count != 0)
        //    {
        //        List<Link> linkToDisconnect = new List<Link>();
        //        foreach (Link link in portIn.Dependencies)
        //        {

        //            if (link.Source == portOut)
        //            {
        //                linkToDisconnect.Add(link);
        //            }
        //        }

        //        if (linkToDisconnect.Count != 0)
        //        {
        //            foreach (Link link in linkToDisconnect)
        //            {
        //                _instance.manager.CurCompound.UnConnect(link);
        //                _instance.history.Do(new DisconnectMemento(link));
        //            }
        //        }
        //        else
        //        {
        //            throw new NodalDirectorException(nom_fct + "\n" + string.Format("Link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\" does not exist!", inPortName, inNodeName, outPortName, outNodeName));
        //        }
        //    }
        //    else
        //    {
        //        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
        //    }

        //    if (_instance.layout == null)
        //        return true;

        //    _instance.layout.Invalidate();

        //    _instance.haveChanged = true;
        //    return true;
        //}

        public static bool Disconnect(string inNodeName, string inPortName, string outNodeName, string outPortName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("Disconnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\");", inNodeName, inPortName, outNodeName, outPortName);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn, nodeOut;
            Port portIn, portOut;
            List<Link> linkToDisconnect = new List<Link>();

            if (String.IsNullOrEmpty(inNodeName) && String.IsNullOrEmpty(inPortName))
            {
                nodeOut = _instance.manager.GetNode(outNodeName);
                if (nodeOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", outNodeName));
                }
                portOut = nodeOut.GetPort(outPortName, true);
                if (portOut == null)
                {
                    string outPortName2 = string.Format("{0}_{1}", outNodeName, outPortName);
                    portOut = nodeOut.GetPort(outPortName2, true);
                    if (portOut == null)
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{0}\" does not exist!", outNodeName, outPortName));
                    }
                }

                if(portOut.Dependencies.Count != 0)
                {
                    linkToDisconnect = new List<Link>(portOut.Dependencies);
                }
                else
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", outPortName, outNodeName));
                }

            }
            else if(String.IsNullOrEmpty(outNodeName) && String.IsNullOrEmpty(outPortName))
            {
                nodeIn = _instance.manager.GetNode(inNodeName);
                if (nodeIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
                }
                portIn = nodeIn.GetPort(inPortName, false);
                if (portIn == null)
                {
                    string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                    portIn = nodeIn.GetPort(inPortName2, false);
                    if (portIn == null)
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{0}\" does not exist!", inNodeName, inPortName));
                    }
                }

                if(portIn.Dependencies.Count != 0)
                {
                    linkToDisconnect = new List<Link>(portIn.Dependencies);
                }
                else
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
                }
            }
            else if(!String.IsNullOrEmpty(inNodeName) && !String.IsNullOrEmpty(inPortName) && !String.IsNullOrEmpty(outNodeName) && !String.IsNullOrEmpty(outPortName))
            {
                nodeIn = _instance.manager.GetNode(inNodeName);
                nodeOut = _instance.manager.GetNode(outNodeName);

                if (nodeIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
                }
                if (nodeOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Node \"{0}\" does not exist!", outNodeName));
                }

                portIn = nodeIn.GetPort(inPortName, false);
                portOut = nodeOut.GetPort(outPortName, true);

                if (portIn == null)
                {
                    string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                    portIn = nodeIn.GetPort(inPortName2, false);
                    if (portIn == null)
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{0}\" does not exist!", inNodeName, inPortName));
                    }
                }
                if (portOut == null)
                {
                    string outPortName2 = string.Format("{0}_{1}", outNodeName, outPortName);
                    portOut = nodeOut.GetPort(outPortName2, true);
                    if (portOut == null)
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{0}\" does not exist!", outNodeName, outPortName));
                    }
                }
                if (portIn.Dependencies.Count != 0)
                {

                    foreach (Link link in portIn.Dependencies)
                    {

                        if (link.Source == portOut)
                        {
                            linkToDisconnect.Add(link);
                        }
                    }
                }
                else
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
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

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
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
                Info(nom_fct);

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
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portIn = nodeIn.GetPort(inPortName2, false);
                if (portIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
                }
            }
            if (portOut == null)
            {
                string outPortName2 = string.Format("{0}_{1}", outNodeName, outPortName);
                portOut = nodeOut.GetPort(outPortName2, true);
                if (portOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", outNodeName, outPortName));
                }
            }

            string error = string.Empty;

            Link connected = nodeIn.Connect(portIn.Index, nodeOut, portOut.Index, inMode, out error, nodeIn.Companion.Manager.Preferences.CheckCycles);
            _instance.history.Do(new ConnectMemento(connected, inMode));

            if (error.Length != 0)
            {
                throw new NodalDirectorException(nom_fct + "\n" + "Cannot connect");
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
        public static bool ReConnect(string inNodeName, string inPortName, string outNodeName, string outPortName,
                                        string newinNodeName, string newinPortName, string newoutNodeName, string newoutPortName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("ReConnect(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\");", inNodeName, inPortName, outNodeName, outPortName, newinNodeName, newinPortName, newoutNodeName, newoutPortName);

            if (_instance.verbose)
                Info(nom_fct);

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
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portInLocked = nodeInLocked.GetPort(inPortName2, false);
                if (portInLocked == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
                }
            }
            if (portOut == null)
            {
                string outPortName2 = string.Format("{0}_{1}", outNodeName, outPortName);
                portOut = nodeOut.GetPort(outPortName2, true);
                if (portOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", outNodeName, outPortName));
                }
            }
            if (newPortIn == null)
            {
                string newinPortName2 = string.Format("{0}_{1}", newinNodeName, newinPortName);
                newPortIn = newNodeIn.GetPort(newinPortName2, false);
                if (newPortIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", newinNodeName, newinPortName));
                }
            }
            if (newPortOut == null)
            {
                string newoutPortName2 = string.Format("{0}_{1}", newoutNodeName, newoutPortName);
                newPortOut = newNodeOut.GetPort(newoutPortName2, true);
                if (newPortOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", newoutNodeName, newoutPortName));
                }
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
            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
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
        public static bool CopyLink(string inNodeName, string inPortName, string outNodeName, string outPortName,
                                        string newinNodeName, string newinPortName, string newoutNodeName, string newoutPortName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("CopyLink(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\");", inNodeName, inPortName, outNodeName, outPortName, newinNodeName, newinPortName, newoutNodeName, newoutPortName);

            if (_instance.verbose)
                Info(nom_fct);

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
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portInLocked = nodeInLocked.GetPort(inPortName2, false);
                if (portInLocked == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
                }
            }
            if (portOut == null)
            {
                string outPortName2 = string.Format("{0}_{1}", outNodeName, outPortName);
                portOut = nodeOut.GetPort(outPortName2, true);
                if (portOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", outNodeName, outPortName));
                }
            }
            if (newPortIn == null)
            {
                string newinPortName2 = string.Format("{0}_{1}", newinNodeName, newinPortName);
                newPortIn = newNodeIn.GetPort(newinPortName2, false);
                if (newPortIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", newinNodeName, newinPortName));
                }
            }
            if (newPortOut == null)
            {
                string newoutPortName2 = string.Format("{0}_{1}", newoutNodeName, newoutPortName);
                newPortOut = newNodeOut.GetPort(newoutPortName2, true);
                if (newPortOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", newoutNodeName, newoutPortName));
                }
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
                _instance.history.Do(new CopyLinkMemento(copyLink));
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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
            return true;
        }

        /// <summary>
        /// Copy links from a node (for pasting on other one)
        /// </summary>
        /// <param name="inNodeName">Node name</param>
        /// <returns></returns>
        public static bool CopyLinks(string inNodeName)
        {
            _instance.manager.ClipBoardLink = null;

            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("CopyLinks(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            _instance.manager.ClipBoardLink = new NodeConnexions(nodeIn);

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
            return true;
        }

        /// <summary>
        /// Paste links previously copy 
        /// </summary>
        /// <param name="inNodeName">Node name</param>
        /// <returns></returns>
        public static bool PasteLinks(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("PasteLinks(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            if (_instance.manager.ClipBoardLink != null)
            {
                Node node = _instance.manager.GetNode(_instance.manager.ClipBoardLink.NodeFullName);
                
                if (node != null)
                {
                    List<Link> links = node.InDependencies;
                    links.AddRange(node.OutDependencies);
                    if (links != null)
                    {
                        _instance.history.BeginCompoundDo();
                        foreach (Link link in links)
                        {
                            if (link.Source.Owner == node)
                            {
                                foreach (Port port in nodeIn.Outputs)
                                {
                                    string error = string.Empty;
                                    if (link.Source.PortObj.ShortName == port.PortObj.ShortName)
                                    {
                                        Link copyLink = (Link)Activator.CreateInstance(link.GetType(), new object[0]);
                                        copyLink.Copy(link);
                                        link.Target.Owner.Connect(link.Target.Index, nodeIn, port.Index, "", out error, copyLink);
                                        if (error.Length == 0)
                                        {
                                            _instance.history.Do(new CopyLinkMemento(copyLink));
                                        }
                                    }
                                }
                            }
                            else if (link.Target.Owner == node)
                            {
                                foreach (Port port in nodeIn.Inputs)
                                {
                                    string error = string.Empty;
                                    if (link.Target.PortObj.ShortName == port.PortObj.ShortName)
                                    {
                                        Link copyLink = (Link)Activator.CreateInstance(link.GetType(), new object[0]);
                                        copyLink.Copy(link);
                                        nodeIn.Connect(port.Index, link.Source.Owner, link.Source.Index, "", out error, copyLink);
                                        if (error.Length == 0)
                                        {
                                            _instance.history.Do(new CopyLinkMemento(copyLink));
                                        }
                                    }
                                }
                            }
                        }
                        _instance.history.EndCompoundDo();

                    }
                    else
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + "Cannot Paste Links, there is no links to paste");
                    }
                }
                else
                {
                    throw new NodalDirectorException(nom_fct + "\n" + "Cannot Paste Links");
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + "Cannot Paste Links");
            }

            if (_instance.layout == null)
                return false;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();
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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

            List<Node> Nodes = new List<Node>();

            foreach (string NodeName in inNodeNames)
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
            foreach (Node Node in Nodes)
            {
                _instance.history.Do(new UnParentMemento(Node.FullName, Node.Parent.FullName));
                _instance.manager.MoveNodes(new List<Node> { Node }, Node.Parent.Parent);
            }
            _instance.history.EndCompoundDo();

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

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
                Info(nom_fct);

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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return nodeIn.FullName;
        }

        /// <summary>
        /// Copy a list of nodes
        /// </summary>
        /// <param name="inNodeNames">List of node names</param>
        /// <returns></returns>
        public static bool Copy(List<string> inNodeNames)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("Copy(new List<string>{{\"{0}\"}});", TypesHelper.Join(inNodeNames, "\",\""));

            if (_instance.verbose)
                Info(nom_fct);

            List<Node> Nodes = new List<Node>();

            foreach (string NodeName in inNodeNames)
            {
                Node nodeIn = _instance.manager.GetNode(NodeName);
                if (nodeIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Input Node \"{0}\" does not exist!", NodeName));
                }
                else
                {
                    Nodes.Add(nodeIn);
                }
            }

            _instance.manager.ClipBoard = Nodes;

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return true;
        }

        public static List<string> Paste(int inXOffset, int inYOffset)
        {
            return Paste(inXOffset, inYOffset, null, null);
        }

        /// <summary>
        /// Paste nodes
        /// </summary>
        /// <param name="inXOffset"></param>
        /// <param name="inYOffset"></param>
        /// <param name="inSearch"></param>
        /// <param name="inReplace"></param>
        /// <returns></returns>
        public static List<string> Paste(int inXOffset, int inYOffset, string inSearch, string inReplace)
        {
            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("Paste({0}, {1}, \"{2}\", \"{3}\");", inXOffset, inYOffset, inSearch, inReplace);

            if (_instance.verbose)
                Info(nom_fct);

            List<string> pasteNodeName = new List<string>();

            if (_instance.manager.ClipBoard != null)
            {
                _instance.history.BeginCompoundDo();
                if (string.IsNullOrEmpty(inSearch))
                {
                    foreach (Node node in _instance.manager.ClipBoard)
                    {
                        Node copyNode = new Node();
                        //copyNode = _instance.manager.Copy(node, _instance.manager.CurCompound, (int)((node.UIx + (inXOffset)) / _instance.layout.LayoutSize), (int)((node.UIy + (inYOffset)) / _instance.layout.LayoutSize));
                        copyNode = _instance.manager.Copy(node, _instance.manager.CurCompound, (int)(node.UIx + (inXOffset)), (int)(node.UIy + (inYOffset)));
                        if (copyNode == null)
                        {
                            throw new NodalDirectorException(nom_fct + "\n" + "Cannot Paste");
                        }
                        _instance.history.Do(new ReAddNodeMemento(copyNode, copyNode.Parent, new NodeConnexions(copyNode), inXOffset, inYOffset));
                        pasteNodeName.Add(copyNode.FullName);
                    }
                }
                else
                {
                    foreach (Node node in _instance.manager.ClipBoard)
                    {
                        Node copyNode = new Node();
                        //copyNode = _instance.manager.Copy(node, _instance.manager.CurCompound, (int)((node.UIx + (inXOffset)) / _instance.layout.LayoutSize), (int)((node.UIy + (inYOffset)) / _instance.layout.LayoutSize), inSearch, inReplace);
                        copyNode = _instance.manager.Copy(node, _instance.manager.CurCompound, (int)(node.UIx + (inXOffset)), (int)(node.UIy + (inYOffset)), inSearch, inReplace);
                        if (copyNode == null)
                        {
                            throw new NodalDirectorException(nom_fct + "\n" + "Cannot Paste");
                        }
                        _instance.history.Do(new ReAddNodeMemento(copyNode, copyNode.Parent, new NodeConnexions(copyNode), inXOffset, inYOffset));
                        pasteNodeName.Add(copyNode.FullName);
                    }
                }
                _instance.history.EndCompoundDo();
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + "Cannot Paste");
            }

            if (_instance.layout == null)
                return null;

            _instance.layout.ChangeFocus(true);
            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return pasteNodeName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inNodeName"></param>
        /// <returns></returns>
        public static bool IsCompound(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("IsCompound(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Info(nom_fct);

            Compound nodeIn = _instance.manager.GetNode(inNodeName) as Compound;

            if (nodeIn == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if node exist
        /// </summary>
        /// <param name="inNodeName">Node name</param>
        /// <returns></returns>
        public static bool NodeExist(string inNodeName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("NodeExist(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if port exist
        /// </summary>
        /// <param name="inNodeName"></param>
        /// <param name="inPortName"></param>
        /// <param name="inIsOutput"></param>
        /// <returns></returns>
        public static bool PortExist(string inNodeName, string inPortName, bool inIsOutput)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("PortExist(\"{0}\", \"{1}\", {2});", inNodeName, inPortName, inIsOutput);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                return false;
            }

            Port portIn = nodeIn.GetPort(inPortName, inIsOutput);

            if (portIn == null)
            {
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portIn = nodeIn.GetPort(inPortName2, inIsOutput);
                if (portIn == null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if port has a link
        /// </summary>
        /// <param name="inNodeName"></param>
        /// <param name="inPortName"></param>
        /// <param name="inIsOutput"></param>
        /// <returns></returns>
        public static bool PortHasLinks(string inNodeName, string inPortName, bool inIsOutput)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("PortHasLinks(\"{0}\", \"{1}\", {2});", inNodeName, inPortName, inIsOutput);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                return false;
            }

            Port portIn = nodeIn.GetPort(inPortName, inIsOutput);

            if (portIn == null)
            {
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portIn = nodeIn.GetPort(inPortName2, inIsOutput);
                if (portIn == null)
                {
                    return false;
                }
            }

            if (portIn.Dependencies.Count == 0)
                return false;

            return true;
        }

        public static object Input(string inType, string Message, string Caption)
        {
            return Input(inType, Message, Caption, "");
        }

        /// <summary>
        /// Enter input Value
        /// </summary>
        /// <param name="inType">Input type : "string", "bool", "float", "double", "int"</param>
        /// <param name="Message"></param>
        /// <param name="Caption"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static object Input(string inType, string Message, string Caption, string defaultValue)
        {
            TK.GraphComponents.Dialogs.InputTypes type;
            RichDialogResult rslt;

            switch (inType)
            {
                case "double":
                    type = TK.GraphComponents.Dialogs.InputTypes.Double;
                    rslt = TKMessageBox.ShowInput(type, Message, Caption, defaultValue);
                    if (rslt.Result != System.Windows.Forms.DialogResult.OK)
                    {
                        return null;
                    }
                    return (double)rslt.Data;
                case "bool":
                    type = TK.GraphComponents.Dialogs.InputTypes.Bool;
                    rslt = TKMessageBox.ShowInput(type, Message, Caption, defaultValue);
                    if (rslt.Result != System.Windows.Forms.DialogResult.OK)
                    {
                        return null;
                    }
                    return (bool)rslt.Data;
                case "float":
                    type = TK.GraphComponents.Dialogs.InputTypes.Float;
                    rslt = TKMessageBox.ShowInput(type, Message, Caption, defaultValue);
                    if (rslt.Result != System.Windows.Forms.DialogResult.OK)
                    {
                        return null;
                    }
                    return (float)rslt.Data;
                case "int":
                    type = TK.GraphComponents.Dialogs.InputTypes.Int;
                    rslt = TKMessageBox.ShowInput(type, Message, Caption, defaultValue);
                    if (rslt.Result != System.Windows.Forms.DialogResult.OK)
                    {
                        return null;
                    }
                    return (int)rslt.Data;
                case "string":
                    type = TK.GraphComponents.Dialogs.InputTypes.String;
                    rslt = TKMessageBox.ShowInput(type, Message, Caption, defaultValue);
                    if (rslt.Result != System.Windows.Forms.DialogResult.OK)
                    {
                        return null;
                    }
                    return (string)rslt.Data;
                default:
                    throw new NodalDirectorException("Wrong Type");
            }
        }

        public static bool CommandScript()
        {
            //-----------------------------------------------------------------------------------
            //---------------------------  NodalDirector.New()  ---------------------------------
            //-----------------------------------------------------------------------------------
            New();

            //-----------------------------------------------------------------------------------
            //--------------------------  NodalDirector.GetChildren()  --------------------------
            //-----------------------------------------------------------------------------------
            List<string> children = GetChildren("", true, false, "");

            //The Root compound should be empty
            if (children.Count > 1)
            {
                Error("Root compound should be empty after New()");
            }

            //-----------------------------------------------------------------------------------
            //------------------------  NodalDirector.GetNodesPreset()  -------------------------
            //-----------------------------------------------------------------------------------
            List<string> NodeName = GetNodesPreset();

            if (NodeName.Count == 0)
            {
                Error("No node available");
            }

            //-----------------------------------------------------------------------------------
            //----------------------------  NodalDirector.AddNode()  ----------------------------
            //-----------------------------------------------------------------------------------
            string nodeIn = AddNode(NodeName[0], null, 50, 50);

            //-----------------------------------------------------------------------------------
            //---------------------------  NodalDirector.NodeExist()  ---------------------------
            //-----------------------------------------------------------------------------------
            if (!NodeExist(nodeIn))
            {
                Error("Node does not exist");
            }
            //-----------------------------------------------------------------------------------
            //----------------------------  NodalDirector.Undo()  -------------------------------
            //-----------------------------------------------------------------------------------
            Undo();

            children = GetChildren("", true, false, "");
            if (children.Count > 1)
            {
                Error("Undo AddNode does not work");
            }

            //-----------------------------------------------------------------------------------
            //----------------------------  NodalDirector.Redo()  -------------------------------
            //-----------------------------------------------------------------------------------
            Redo();

            children = GetChildren("", true, false, "");
            if (children.Count < 2)
            {
                Error("Redo AddNode does not work");
            }

            string nodeOut = AddNode(NodeName[0], null, 100, 100);
            if (!NodeExist(nodeOut))
            {
                Error("Node does not exist");
            }


            //if (GetChildren("",true,false,"").Count == 3)
            //{
            bool connected = false;
            bool disconnected = false;
            bool copylink = false;
            bool error = false;

            //-----------------------------------------------------------------------------------
            //-------------------------  NodalDirector.GetInputPort()  --------------------------
            //-----------------------------------------------------------------------------------
            List<string> inputPorts = GetInputPort(nodeIn);

            //-----------------------------------------------------------------------------------
            //-------------------------  NodalDirector.GetOutputPort()  -------------------------
            //-----------------------------------------------------------------------------------
            List<string> outputPorts = GetOutputPort(nodeOut);

            if(inputPorts.Count != 0 && outputPorts.Count != 0)
            {

            //-----------------------------------------------------------------------------------
            //----------------------------  NodalDirector.Connect()  ----------------------------
            //-----------------------------------------------------------------------------------
                connected = Connect(nodeIn, inputPorts[0], nodeOut, outputPorts[0]);
            }

            if (!connected)
            {
                Error("Cannot connect");
            }

            List<string> dependents = GetDependentNodes(nodeOut, false);

            if (inputPorts.Count >= 2)
            {
            //-----------------------------------------------------------------------------------
            //---------------------------  NodalDirector.CopyLink()  ----------------------------
            //-----------------------------------------------------------------------------------
                copylink = CopyLink(nodeIn, inputPorts[0], nodeOut, outputPorts[0], nodeIn, inputPorts[1], nodeOut, outputPorts[0]);

                if (!copylink)
                {
                    Error("Cannot copylink");
                }
            }

            //-----------------------------------------------------------------------------------
            //--------------------------  NodalDirector.Disconnect()  ---------------------------
            //-----------------------------------------------------------------------------------
            disconnected = Disconnect(nodeIn, inputPorts[0], nodeOut, outputPorts[0]);

            if (!disconnected)
            {
                Error("Cannot disconnect");
            }

            error = ReConnect(nodeIn, inputPorts[0], nodeOut, outputPorts[1], 
                                nodeIn, inputPorts[0], nodeOut, outputPorts[0]);

            if (!error)
            {
                Error("Cannot reconnect");
            }


            //}



            return true;
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

        /// <summary>
        /// Getting nodes existing
        /// </summary>
        /// <returns></returns>
        public static List<string> GetNodesPreset()
        {
            List<string> nodes = new List<string>();
            foreach (Node node in _instance.manager.AvailableNodes)
            {
                nodes.Add(node.FullName);
            }
            return nodes;
        }

        public static List<string> GetInputPort(string inNodeName)
        {
            List<string> inputPortName = new List<string>();
            List<Port> inputPort = new List<Port>();

            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("GetInputPort(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            foreach(Port port in nodeIn.Inputs)
            {
                inputPortName.Add(port.FullName);
            }
            
            return inputPortName;
        }

        public static List<string> GetOutputPort(string inNodeName)
        {
            List<string> outputPortName = new List<string>();

            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("GetInputPort(\"{0}\");", inNodeName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            foreach (Port port in nodeIn.Outputs)
            {
                outputPortName.Add(port.FullName);
            }

            return outputPortName;
        }

        /// <summary>
        /// Getting children of a compound
        /// </summary>
        /// <param name="inCompoundName">Compound name</param>
        /// <param name="inRecursive">Recursivity : true or false</param>
        /// <param name="IgnoreCompounds">Ignore the compound return only node names or not</param>
        /// <param name="inType"></param>
        /// <returns></returns>
        public static List<string> GetChildren(string inCompoundName, bool inRecursive, bool IgnoreCompounds, string inType)
        {
            List<string> children = new List<string>();
            List<Node> nodes = new List<Node>();

            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("GetChildren(\"{0}\", {1}, {2}, \"{3}\");", inCompoundName, inRecursive, IgnoreCompounds, inType);

            if (_instance.verbose)
                Log(nom_fct);

            Compound CompoundIn;

            if (String.IsNullOrEmpty(inCompoundName))
            {
                CompoundIn = _instance.manager.Root;
            }
            else
            {
                CompoundIn = _instance.manager.GetNode(inCompoundName) as Compound;

                if (CompoundIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Compound \"{0}\" does not exist!", CompoundIn));
                }
            }

            nodes = CompoundIn.GetChildren(IgnoreCompounds, inType, inRecursive);

            foreach(Node node in nodes)
            {
                children.Add(node.FullName);
            }

            if (_instance.layout == null)
                return null;

            _instance.layout.Invalidate();

            return children;
        }

        public static List<string> GetDependentNodes(string inNodeName, bool inRecursive)
        {
            List<string> dependents = new List<string>();
            List<Node> nodes = new List<Node>();

            if (_instance.manager == null)
                return null;

            string nom_fct = string.Format("GetDependentNodes(\"{0}\", {1});", inNodeName, inRecursive);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            //Compound CompoundIn;

            //if (String.IsNullOrEmpty(inCompoundName))
            //{
            //    CompoundIn = nodeIn.Parent;
            //}
            //else
            //{
            //    CompoundIn = _instance.manager.GetNode(inCompoundName) as Compound;

            //    if (CompoundIn == null)
            //    {
            //        throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Compound \"{0}\" does not exist!", CompoundIn));
            //    }
            //}

            nodes = nodeIn.GetDependentNodes(inRecursive);

            foreach (Node node in nodes)
            {
                dependents.Add(node.FullName);
            }

            if (_instance.layout == null)
                return null;

            _instance.layout.Invalidate();

            return dependents;
        }

        public static Dictionary<string, string> propertyPossibilities = new Dictionary<string, string>()
        {
            {"name", "Name" },
            {"freezed", "Freezed" },
            {"inputs", "Inputs" },
            {"outputs", "Outputs" },
            {"customcolor", "CustomColor" }
        };

        public static Dictionary<string, string> propertyPortPossibilities = new Dictionary<string, string>()
        {
            {"visible", "Visible" },
            {"visibility", "Visible" },
        };

        public static Dictionary<string, string> propertyElementPossibilities = new Dictionary<string, string>()
        {
            {"visible", "Visible" },
            {"visibility", "Visible" },
        };

        public static Dictionary<string, string> propertyLinkPossibilities = new Dictionary<string, string>()
        {
            {"name", "Name" }
        };

        public static string GetPropertyPossibilities(string inProperty)
        {
            return GetAliases(inProperty, propertyPossibilities);
        }

        public static string GetPortPropertyPossibilities(string inProperty)
        {
            return GetAliases(inProperty, propertyPortPossibilities);
        }

        public static string GetElementPropertyPossibilities(string inProperty)
        {
            return GetAliases(inProperty, propertyElementPossibilities);
        }

        public static string GetLinkPropertyPossibilities(string inProperty)
        {
            return GetAliases(inProperty, propertyLinkPossibilities);
        }

        public static string GetAliases(string inProperty, Dictionary<string, string> inDictionary)
        {
            string value = "";
            if (inDictionary.TryGetValue(inProperty, out value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        //public static bool SetProperty(string inNodeName, string inPropertyName, object inValue)
        //{
        //    if (_instance.manager == null)
        //        return false;

        //    string nom_fct = string.Format("SetProperty(\"{0}\", \"{1}\", {2});", inNodeName, inPropertyName, inValue);

        //    if (_instance.verbose)
        //        Log(nom_fct);

        //    Node nodeIn = _instance.manager.GetNode(inNodeName);

        //    if (nodeIn == null)
        //    {
        //        throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
        //    }

        //    string value = "";
        //    if(propertyPossibilities.TryGetValue(inPropertyName, out value))
        //    {
        //        inPropertyName = value;
        //    }

        //    var prop = nodeIn.GetType().GetProperty(inPropertyName);

        //    if (prop!=null)
        //    {
        //        AttributeCollection attributes = TypeDescriptor.GetProperties(nodeIn)[inPropertyName].Attributes;
        //        if (attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes) || attributes[typeof(BrowsableAttribute)].Equals(BrowsableAttribute.No))
        //        {
        //            throw new NodalDirectorException(nom_fct + "\n" + "Cannot Set Property");
        //        }
        //        else
        //        {
        //            Type inValueType = inValue.GetType();

        //            if (inValueType == prop.PropertyType)
        //            {
        //                switch (inPropertyName)
        //                {
        //                    case "Name":
        //                        Rename(nodeIn.FullName, (string)inValue);
        //                        break;
        //                    default:
        //                        prop.SetValue(nodeIn, inValue);
        //                        break;
        //                }

        //            }
        //            else
        //            {
        //                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Property \"{0}\" with value {1}", inPropertyName, inValue));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Property \"{0}\"", inPropertyName));
        //    }

        //    return true;
        //}

        /// <summary>
        /// Set Node property
        /// </summary>
        /// <param name="inNodeName">Node name</param>
        /// <param name="inPropertyName">Property name : "Name", "CustomColor", "Freezed", "Path"</param>
        /// <param name="inValue">Node value to change</param>
        /// <returns></returns>
        public static bool SetProperty(string inNodeName, string inPropertyName, object inValue)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("SetProperty(\"{0}\", \"{1}\", {2});", inNodeName, inPropertyName, inValue);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            string value = GetPropertyPossibilities(inPropertyName);
            if (value != null)
            {
                inPropertyName = value;
            }

            switch (inPropertyName)
            {
                case "Name":
                    nom_fct = string.Format("SetProperty(\"{0}\", \"{1}\", \"{2}\");", inNodeName, inPropertyName, inValue);
                    if (_instance.verbose)
                        Info(nom_fct);
                    if (inValue.GetType() == typeof(string))
                    {
                        int n;
                        bool isNum = int.TryParse((string)inValue, out n);
                        if (isNum == false)
                        {
                            //_instance.history.Do(new SetPropetyMemento(nodeIn, inPropertyName));
                            _instance.verbose = false;
                            Rename(nodeIn.FullName, (string)inValue);
                            _instance.verbose = true;
                        }
                        else
                        {
                            throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Property \"{0}\" with value \"{1}\"", inPropertyName, inValue));
                        }
                    }
                    else
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Property \"{0}\" with value \"{1}\"", inPropertyName, inValue));
                    }
                    break;
                default:
                    if (_instance.verbose)
                        Info(nom_fct);
                    var prop = nodeIn.GetType().GetProperty(inPropertyName);

                    if (prop != null)
                    {
                        AttributeCollection attributes = TypeDescriptor.GetProperties(nodeIn)[inPropertyName].Attributes;
                        if (attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes) || attributes[typeof(BrowsableAttribute)].Equals(BrowsableAttribute.No))
                        {
                            throw new NodalDirectorException(nom_fct + "\n" + "Cannot Set Property");
                        }
                        else
                        {
                            Type inValueType = inValue.GetType();

                            if (inValueType == prop.PropertyType)
                            {
                                _instance.history.Do(new SetPropetyMemento(nodeIn, inPropertyName));
                                prop.SetValue(nodeIn, inValue);
                            }
                            else
                            {
                                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Property \"{0}\" with value {1}", inPropertyName, inValue));
                            }
                        }
                    }
                    else
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Property \"{0}\"", inPropertyName));
                    }
                    break;
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return true;
        }

        /// <summary>
        /// Set Port property
        /// </summary>
        /// <param name="inNodeName">Node name</param>
        /// <param name="inPortName">Port name</param>
        /// <param name="inIsOutput">False : input Port, True : output Port</param>
        /// <param name="inPropertyName">Property name : "Visible"</param>
        /// <param name="inValue">Port value to change</param>
        /// <returns></returns>
        public static bool SetPortProperty(string inNodeName, string inPortName, bool inIsOutput, string inPropertyName, object inValue)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("SetPortsProperty(\"{0}\", \"{1}\", {2}, \"{3}\", {4});", inNodeName, inPortName, inIsOutput, inPropertyName, inValue);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            Port portIn = nodeIn.GetPort(inPortName, inIsOutput);

            if (portIn == null)
            {
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portIn = nodeIn.GetPort(inPortName2, inIsOutput);
                if (portIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
                }
            }

            string value = GetPortPropertyPossibilities(inPropertyName);
            if (value != null)
            {
                inPropertyName = value;
            }

            switch (inPropertyName)
            {
                case "Visible":
                    if (inValue.GetType() == typeof(Boolean))
                    {
                        _instance.history.Do(new SetPortPropetyMemento(portIn, inPropertyName));
                        portIn.Visible = (bool)inValue;
                    }
                    else
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Port Property \"{0}\" with value {1}", inPropertyName, inValue));
                    }
                    break;
                default:
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Port Property with property \"{0}\"", inPropertyName));
            }

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return true;
        }

        public static bool SetElementProperty(string inNodeName, string inPortObjName, string inPropertyName, object inValue)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("SetElementProperty(\"{0}\", \"{1}\", \"{2}\", {3});", inNodeName, inPortObjName, inPropertyName, inValue);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            PortObj portObjIn = nodeIn.GetElement(inPortObjName);

            if (portObjIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("PortObj \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortObjName));
            }

            string value = GetElementPropertyPossibilities(inPropertyName);
            if (value != null)
            {
                inPropertyName = value;
            }

            //switch (inPropertyName)
            //{
            //    case "Visible":
            //        if (inValue.GetType() == typeof(Boolean))
            //        {
            //            portIn.Visible = (bool)inValue;
            //        }
            //        else
            //        {
            //            throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Port Property \"{0}\" with value {1}", inPropertyName, inValue));
            //        }
            //        break;
            //    default:
            //        throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Set Port Property with property \"{0}\"", inPropertyName));
            //}

            if (_instance.layout == null)
                return true;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return true;
        }

        /// <summary>
        /// Get Node property
        /// </summary>
        /// <param name="inNodeName">Node name</param>
        /// <param name="inPropertyName">Property name : "Name", "CustomColor", "Freezed", "Path"</param>
        /// <returns></returns>
        public static object GetProperty(string inNodeName, string inPropertyName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("GetProperty(\"{0}\", \"{1}\");", inNodeName, inPropertyName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            string value = GetPropertyPossibilities(inPropertyName);
            if (value != null)
            {
                inPropertyName = value;
            }

            PropertyInfo prop = nodeIn.GetType().GetProperty(inPropertyName);

            if (prop != null)
            {
                AttributeCollection attributes = TypeDescriptor.GetProperties(nodeIn)[inPropertyName].Attributes;
                if (attributes[typeof(BrowsableAttribute)].Equals(BrowsableAttribute.No))
                {
                    throw new NodalDirectorException(nom_fct + "\n" + "Cannot Get Property");
                }
                else
                {
                    Console.WriteLine("Get " + prop.GetValue(nodeIn));
                    return prop.GetValue(nodeIn);
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Get Property \"{0}\"", inPropertyName));
            }
        }

        public static object GetPortProperty(string inNodeName, string inPortName, bool inIsOutput, string inPropertyName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("GetPortProperty(\"{0}\", \"{1}\", \"{2}\");", inNodeName, inPortName, inPropertyName);

            if (_instance.verbose)
                Log(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            Port portIn = nodeIn.GetPort(inPortName, inIsOutput);

            if (portIn == null)
            {
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portIn = nodeIn.GetPort(inPortName2, inIsOutput);
                if (portIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
                }
            }

            string value = GetPortPropertyPossibilities(inPropertyName);
            if (value != null)
            {
                inPropertyName = value;
            }

            PropertyInfo prop = portIn.GetType().GetProperty(inPropertyName);

            if (prop != null)
            {
                AttributeCollection attributes = TypeDescriptor.GetProperties(portIn)[inPropertyName].Attributes;
                if (attributes[typeof(BrowsableAttribute)].Equals(BrowsableAttribute.No))
                {
                    throw new NodalDirectorException(nom_fct + "\n" + "Cannot Get Property");
                }
                else
                {
                    Console.WriteLine("Get " + prop.GetValue(portIn));
                    return prop.GetValue(portIn);
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Get Port Property \"{0}\"", inPropertyName));
            }
        }

        public static object GetLinkProperty(string inNodeName, string inPortName, string outNodeName, string outPortName, string inPropertyName)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("GetLinkProperty(\"{0}\", \"{1}\", \"{2}\", \"{3}\", \"{4}\");", inNodeName, inPortName, outNodeName, outPortName, inPropertyName);

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
            Port portOut = nodeIn.GetPort(outPortName, true);

            if (portIn == null)
            {
                string inPortName2 = string.Format("{0}_{1}", inNodeName, inPortName);
                portIn = nodeIn.GetPort(inPortName2, false);
                if (portIn == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Port \"{0}\" from \"{1}\" does not exist!", inNodeName, inPortName));
                }
            }

            if (portOut == null)
            {
                string outPortName2 = string.Format("{0}_{1}", outNodeName, outPortName);
                portOut = nodeOut.GetPort(outPortName2, true);
                if (portOut == null)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("output Port \"{0}\" from \"{1}\" does not exist!", outNodeName, outPortName));
                }
            }

            string value = GetLinkPropertyPossibilities(inPropertyName);
            if (value != null)
            {
                inPropertyName = value;
            }

            Link linki = new Link();
            if (portIn.Dependencies.Count != 0)
            {
                foreach (Link link in portIn.Dependencies)
                {

                    if (link.Source == portOut)
                    {
                        linki = link;
                        break;
                    }
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Port \"{0}\" from Node \"{1}\" has no link", inPortName, inNodeName));
            }

            if (linki != null)
            {
                PropertyInfo prop = linki.GetType().GetProperty(inPropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (null != prop && prop.CanWrite)
                {
                    AttributeCollection attributes = TypeDescriptor.GetProperties(nodeIn)[inPropertyName].Attributes;
                    if (attributes[typeof(BrowsableAttribute)].Equals(BrowsableAttribute.No))
                    {
                        throw new NodalDirectorException(nom_fct + "\n" + "Cannot Get Link Property");
                    }
                    else
                    {
                        Console.WriteLine("Get " + prop.GetValue(linki));
                        return prop.GetValue(linki);
                    }
                }
                else
                {
                    throw new NodalDirectorException(nom_fct + "\n" + string.Format("Cannot Get Link Property \"{0}\"", inPropertyName));
                }
            }
            else
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("Link between port \"{0}\" from Node \"{1}\" and port \"{2}\" from Node \"{3}\" does not exist!", inPortName, inNodeName, outPortName, outNodeName));
            }


        }

        #endregion

        #region UI Commands

        ///// <summary>
        ///// Select a List of nodes
        ///// </summary>
        ///// <param name="inNodeNames">List of node Names</param>
        ///// <returns></returns>
        //public static bool SelectNodes(List<string> inNodeNames)
        //{
        //    return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Default);
        //}

        ///// <summary>
        ///// Select a List of nodes with selection Types
        ///// </summary>
        ///// <param name="inNodeNames">List of node Names</param>
        ///// <param name="inType">Type of Selection : "Default", "Add", "Toggle", "RemoveFrom"</param>
        ///// <returns></returns>
        //public static bool SelectNodes(List<string> inNodeNames, string inType)
        //{
        //    if(inType == "Default")
        //        return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Default);
        //    else if (inType == "Add")
        //        return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Add);
        //    else if (inType == "Toggle")
        //        return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.Toggle);
        //    else if (inType == "RemoveFrom")
        //        return SelectNodes(inNodeNames, NodesLayout.TypeOfSelection.RemoveFrom);
        //    else
        //    {
        //        throw new NodalDirectorException("Cannot Select the Nodes, Wrong inType");
        //    }
        //}

        ///// <summary>
        ///// Select a List of nodes with selection Types
        ///// </summary>
        ///// <param name="inNodeNames">List of node Names</param>
        ///// <param name="inType">Type of Selection : Default, Add, Toggle, RemoveFrom</param>
        ///// <returns></returns>
        //public static bool SelectNodes(List<string> inNodeNames, NodesLayout.TypeOfSelection inType)
        //{
        //    if (_instance.manager == null)
        //        return false;

        //    string nom_fct = string.Format("SelectNodes(new List<string>{{\"{0}\"}});", TypesHelper.Join(inNodeNames, "\",\""));

        //    if (_instance.verbose)
        //        Info(nom_fct);

        //    List<string> nodesNameError = new List<string>();
        //    List<Node> nodes = new List<Node>();

        //    if (inNodeNames.Count > 0)
        //    {
        //        foreach (string NodeName in inNodeNames)
        //        {
        //            Node Node = _instance.manager.GetNode(NodeName);

        //            if (Node == null) //Node with NodeName do not exist
        //            {
        //                nodesNameError.Add(NodeName);
        //            }
        //            else //Node with NodeName exist
        //            {
        //                nodes.Add(Node);
        //            }
        //        }
        //        if (nodesNameError.Count > 0)
        //        {
        //            throw new NodalDirectorException(nom_fct + "\n" + string.Format("Not all node names exist in List<string>{{\"{0}\"}} ", TypesHelper.Join(inNodeNames, "\",\"")));
        //        }
        //        else //All the nodes name exist
        //        {
        //            switch (inType)
        //            {
        //                case NodesLayout.TypeOfSelection.Default:
        //                    _instance.layout.Selection.Select(nodes);
        //                    break;
        //                case NodesLayout.TypeOfSelection.Add:
        //                    foreach (Node node in nodes)
        //                    {
        //                        _instance.layout.Selection.AddToSelection(node);
        //                    }
        //                    break;
        //                case NodesLayout.TypeOfSelection.Toggle:
        //                    foreach (Node node in nodes)
        //                    {
        //                        _instance.layout.Selection.ToggleSelection(node);
        //                    }
        //                    break;
        //                case NodesLayout.TypeOfSelection.RemoveFrom:
        //                    foreach (Node node in nodes)
        //                    {
        //                        _instance.layout.Selection.RemoveFromSelection(node);
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        _instance.layout.Selection.DeselectAll();
        //    }

        //    if (_instance.layout == null)
        //        return true;

        //    _instance.layout.Invalidate();

        //    _instance.haveChanged = true;

        //    return true;
        //}

        /// <summary>
        /// Select a List of nodes
        /// </summary>
        /// <param name="inNodeNames">List of node Names</param>
        /// <returns></returns>
        public static bool SelectNodes(List<string> inNodeNames)
        {
            return SelectNodes(inNodeNames, "Default");
        }

        /// <summary>
        /// Select a List of nodes with selection Types
        /// </summary>
        /// <param name="inNodeNames">List of node Names</param>
        /// <param name="inType">Type of Selection : "Default", "Add", "Toggle", "RemoveFrom"</param>
        /// <returns></returns>
        public static bool SelectNodes(List<string> inNodeNames, string inType)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("SelectNodes(new List<string>{{\"{0}\"}});", TypesHelper.Join(inNodeNames, "\",\""));

            if (_instance.verbose)
                Info(nom_fct);

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
                    if (inType == "Default")
                    {
                        _instance.layout.Selection.Select(nodes);
                        NodeBase[] nb_cp = new NodeBase[_instance.layout.Selection.Selection.Count];
                        _instance.layout.Selection.Selection.CopyTo(nb_cp);
                        _instance.history.Do(new SelectNodesMemento(nb_cp, null));
                    }
                    else if (inType == "Add")
                    {
                        NodeBase[] nb_cp = new NodeBase[_instance.layout.Selection.Selection.Count];
                        _instance.layout.Selection.Selection.CopyTo(nb_cp);
                        _instance.history.Do(new SelectNodesMemento(nb_cp, nodes));
                        foreach (Node node in nodes)
                        {
                            _instance.layout.Selection.AddToSelection(node);
                        }

                    }
                    else if (inType == "Toggle")
                    {
                        NodeBase[] nb_cp = new NodeBase[_instance.layout.Selection.Selection.Count];
                        _instance.layout.Selection.Selection.CopyTo(nb_cp);
                        _instance.history.Do(new SelectNodesMemento(nb_cp, nodes));
                        foreach (Node node in nodes)
                        {
                            _instance.layout.Selection.ToggleSelection(node);
                        }
                    }
                    else if (inType == "RemoveFrom")
                    {
                        NodeBase[] nb_cp = new NodeBase[_instance.layout.Selection.Selection.Count];
                        _instance.layout.Selection.Selection.CopyTo(nb_cp);
                        _instance.history.Do(new DeselectNodesMemento(nb_cp, nodes));
                        foreach (Node node in nodes)
                        {
                            _instance.layout.Selection.RemoveFromSelection(node);
                        }
                    }
                    else
                    {
                        throw new NodalDirectorException("Cannot Select the Nodes, Wrong inType");
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

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return true;
        }

        /// <summary>
        /// Move node in X and Y
        /// </summary>
        /// <param name="inNodeName"></param>
        /// <param name="inX">Position X in pixel</param>
        /// <param name="inY">Position Y in pixel</param>
        /// <returns></returns>
        public static bool MoveNode(string inNodeName, int inX, int inY)
        {
            if (_instance.manager == null)
                return false;

            string nom_fct = string.Format("MoveNode(\"{0}\", {1}, {2});", inNodeName, inX, inY);

            if (_instance.verbose)
                Info(nom_fct);

            Node nodeIn = _instance.manager.GetNode(inNodeName);

            if (nodeIn == null)
            {
                throw new NodalDirectorException(nom_fct + "\n" + string.Format("input Node \"{0}\" does not exist!", inNodeName));
            }

            _instance.historyUI.Do(new MoveNodeMemento(nodeIn));
            nodeIn.UIx = (int)(inX * (1 / _instance.layout.LayoutSize));
            nodeIn.UIy = (int)(inY * (1 / _instance.layout.LayoutSize));

            if (_instance.layout == null)
                return false;

            _instance.layout.Invalidate();

            _instance.haveChanged = true;
            _instance.ChangeOnStatus();

            return true;
        }

        /// <summary>
        /// Open a file
        /// </summary>
        /// <param name="inPath">Path where the file is. Be carefull to double the "\" in "\\"</param>
        /// <returns></returns>
        public static bool Open(string inPath)
        {
            return Open(inPath, false);
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
                Info(nom_fct);

            if (_instance.haveChanged == true && inForce == false)
            {
                bool isconfirmed = TKMessageBox.Confirm("Current file have unsaved modifications, Are you sure you want to lose modifications ?", "WARNING");
                if (!isconfirmed)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + "Open file has been canceled");
                }
            }

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

            _instance.haveChanged = false;
            _instance.ChangeOnStatus();
            return true;
        }

        /// <summary>
        /// New layout
        /// </summary>
        /// <returns></returns>
        public static bool New()
        {
            return New(false);
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
                Info(nom_fct);

            if (_instance.haveChanged == true && inForce == false)
            {
                bool isconfirmed = TKMessageBox.Confirm("Current file have unsaved modifications, Are you sure you want to lose modifications ?", "WARNING");
                if (!isconfirmed)
                {
                    throw new NodalDirectorException(nom_fct + "\n" + "New has been canceled");
                }
            }

            _instance.manager.NewLayout();
            _instance.layout.Invalidate();

            _instance.haveChanged = false;
            _instance.ChangeOnStatus();
            return true;
        }

        public static bool Save()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            System.Windows.Forms.DialogResult rslt = sfd.ShowDialog();

            if (rslt == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter myWriter = null;

                try
                {
                    myWriter = new StreamWriter(sfd.FileName);

                    NodesSerializer.GetInstance().CompoundSerializers["Default"].Serialize(myWriter, _instance.manager.Root);
                    _instance.haveChanged = false;
                    _instance.ChangeOnStatus();
                    return true;
                }
                finally
                {
                    if (myWriter != null)
                        myWriter.Close();
                }
            }
            return false;
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

        /// <summary>
        /// Executes arbitrary Python code at runtime
        /// </summary>
        /// <param name="inCode">The code to execute</param>
        public static void EvaluatePython(string inCode)
        {
            //throw new NodalDirectorException("PYTHON INTERPRETER NOT IMPLEMENTED !\n"+inCode);
            _instance.manager.Execute(inCode);
        }
    }
}
