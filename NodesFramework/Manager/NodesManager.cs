using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TK.NodalEditor.NodesFramework;
using TK.NodalEditor.NodesLayout;

namespace TK.NodalEditor
{
    /// <summary>
    /// Main class  that holds the instances of the Node class that represent the current nodes loaded.
    /// </summary>
    /// <remarks>
    /// The nodes are available through the BreadCrumbs member, that contains the list of the compounds being currently explored incuding the first one, that is the root of all other nodes, pretty much like a scene of a 3D software contains the objects.
    /// It contains Helpers for the navigation in the NodalEditor itself with its BreadCrumbs member, it performs Node searching, Entering, Jumping and Exploding compounds
    /// </remarks>
    public class NodesManager : INodalEditorCommands
    {
        public event NodesChangedEventHandler NodesChangedEvent;

        public virtual void OnNodesChanged(NodesChangedEventArgs e)
        {
            NodesChangedEvent(this, e);
        }

        #region CONSTRUCTORS

        /*DEPRECATE
        /// <summary>
        /// Empty constructor. Should not be used appart for testing because it lacks a ManagerCompanion
        /// </summary>
        public NodesManager()
        {

        }*/

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="companion">Helper that manages UI specific actions</param>
        public NodesManager(ManagerCompanion companion)
        {
            Companion = companion;
            Companion.Manager = this;
            CreateMode = true;

            //Add empty event handlers
            NodesChangedEvent += NodesManager_NodesChangedEvent;
        }

        private void NodesManager_NodesChangedEvent(object sender, NodesChangedEventArgs e)
        {
        }

        #endregion

        #region MEMBERS

        /// <summary>
        /// Indicates if we are in "CreateMode" used when we want the virtual methods or events to be called "live" when manipulations are made on the nodes. The other mode, called "ExecuteMode", will call other methods only when an "Execute" command is called.
        /// </summary>
        public bool CreateMode = false;

        /// <summary>
        /// Helper that manages UI specific actions such as storing a Processes queue, displaying ProgressBars, software-specific logs and setting and restoring software environments. This class may be inherited to create software specific actions.
        /// </summary>
        public ManagerCompanion Companion;

        /// <summary>
        /// The lists of nodes available for creation
        /// </summary>
        public List<Node> AvailableNodes = new List<Node>();

        /// <summary>
        /// An empty compound for compound creation
        /// </summary>
        public Compound AvailableCompound;

        /// <summary>
        /// Contains the list of the compounds being currently explored incuding the first one, that is the root of all other nodes.
        /// </summary>
        public List<Compound> BreadCrumbs = new List<Compound>();

        List<Node> _clipboard = new List<Node>();
        /// <summary>
        /// Contains the list of nodes stored by the NodesLayout for copying and pasting nodes.
        /// </summary>
        public List<Node> ClipBoard
        {
            get
            {
                //Each time we get the clipboard,check if it's still valid
                // (Same parent compound, no deleted nodes)
                if(_clipboard.Count > 0)
                {
                    Compound comp = _clipboard[0].Parent;
                    foreach (Node node in _clipboard)
                    {
                        if (node.Deleted || node.Parent != comp)
                        {
                            _clipboard.Clear();
                            break;
                        }
                    }
                 }

                return _clipboard;
            }
            set { _clipboard = value; }
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Root compound, the "scene" that contains all other nodes and can be saved and loaded
        /// </summary>
        public Compound Root
        {
            get { return (BreadCrumbs.Count > 0 ? BreadCrumbs[0] : null); }
        }
        
        /// <summary>
        /// Current compound that is explored in the UI
        /// </summary>
        public Compound CurCompound
        {
            get { return (BreadCrumbs.Count > 0 ? BreadCrumbs[BreadCrumbs.Count-1] : null); }
        }

        #endregion

        #region METHODS

        // --- Naming ---

        /// <summary>
        /// Find a unique name, going through all existing nodes (calls SetNodeUniqueName(NewName, curNode, false))
        /// </summary>
        /// <param name="NewName">The new base name to be tried</param>
        /// <returns>A unique name</returns>
        public string SetNodeUniqueName(string NewName, Node curNode)
        {
            return SetNodeUniqueName(NewName, curNode, true);
        }

        /// <summary>
        /// Find a unique name, going through all existing nodes (collect all existing names and calls SetUniqueName, providing this list)
        /// </summary>
        /// <param name="NewName">The new base name to be tried</param>
        /// <param name="curNode">The current node, that must be excluded from search</param>
        /// <returns>A unique name</returns>
        public string SetNodeUniqueName(string NewName, Node curNode, bool exclude)
        {
            List<Node> allNodes = Root.GetChildren(false, exclude ? curNode : null, "");

            List<string> otherNames = new List<string>();
            foreach (Node iNode in allNodes)
            {
                otherNames.Add(iNode.FullName);
            }

            if (curNode.NodeType == "Node" && curNode.NodeElementType != "Geometry")
            {
                int caret = 1;
                int padding = 0;

                while (Char.IsNumber(NewName[NewName.Length - caret]))
                {
                    caret++;
                }

                int increment = -1;

                if (caret > 1)
                {
                    string strIncrement = NewName.Substring(NewName.Length - caret + 1, caret - 1);
                    increment = Int32.Parse(strIncrement);
                    padding = strIncrement.Length;
                    NewName = NewName.Substring(0, NewName.Length - caret + 1);
                }
                
                bool safe = false;
                while (!safe)
                {
                    safe = true;

                    string suffixedName = NewName + (increment > -1 ? increment.ToString("D" + padding.ToString()) : "");

                    List<Port> ports = new List<Port>(curNode.Inputs);
                    ports.AddRange(curNode.Outputs);

                    foreach (Port port in ports)
                    {
                        if (port.Name.Length > curNode.FullName.Length && port.Name.Substring(curNode.FullName.Length).ToLower().Contains(suffixedName.ToLower()))
                        {
                            increment++;
                            safe = false;
                            break;
                        }
                    }
                }

                NewName = NewName + (increment > -1 ? increment.ToString("D" + padding.ToString()) : "");
            }

            return SetUniqueName(NewName, otherNames);
        }

        /// <summary>
        /// Find a unique name, going through all existing nodes (this is the real unicity algorithm that used a flat names list)
        /// </summary>
        /// <param name="name">The new base name to be tried</param>
        /// <param name="otherNames">The list of existing names</param>
        /// <returns>A unique name</returns>
        public static string SetUniqueName(string name, List<string> otherNames)
        {
            bool Unique = false;
            bool ActualUnique;

            while (Unique == false)
            {
                ActualUnique = true;

                foreach (string otherName in otherNames)
                {
                    if (otherName.ToLower() == name.ToLower())
                    {
                        ActualUnique = false;
                    }
                }

                if (ActualUnique)
                {
                    Unique = true;
                }
                else
                {
                    name = RenumberString(name);
                }
            }

            return name;
        }

        /// <summary>
        /// Increments a numbering suffix in a string ("foo" gives "foo1", "foo1" gives "foo2"...)
        /// </summary>
        /// <param name="name">The base name to be incremented</param>
        /// <returns>The modified string</returns>
        public static string RenumberString(string name)
        {
            int caret = 1;
            int padding = 0;

            while (Char.IsNumber(name[name.Length - caret]))
            {
                caret++;
            }

            int increment = -1;

            if (caret > 1)
            {
                string strIncrement = name.Substring(name.Length - caret + 1, caret - 1);
                increment = Int32.Parse(strIncrement) + 1;
                padding = strIncrement.Length;
                name = name.Substring(0, name.Length - caret + 1) + increment.ToString("D" + padding.ToString());
            }
            else
            {
                name += "1";
            }

            return name;
        }

        /// <summary>
        /// Overload of GetNode(string FullName, Compound Root, bool recur)
        /// </summary>
        /// <param name="FullName">Name of the node</param>
        /// <returns>The found node or null</returns>
        public Node GetNode(string FullName)
        {
            return GetNode(FullName, Root, true);
        }

        /// <summary>
        /// Overload of GetNode(string FullName, Compound Root, bool recur)
        /// </summary>
        /// <param name="FullName">Name of the node</param>
        /// <param name="compound">Compound from which begin the search</param>
        /// <returns>The found node or null</returns>
        public Node GetNode(string FullName, Compound compound)
        {
            return GetNode(FullName, compound, false);
        }

        /// <summary>
        /// Search a node by name, from a root (parent), and recursively or not.
        /// </summary>
        /// <param name="FullName">Name of the node</param>
        /// <param name="Root">Compound from which begin the search</param>
        /// <param name="recur">Indicate if we need to search recursively</param>
        /// <returns>The found node or null</returns>
        public Node GetNode(string FullName, Compound Root, bool recur)
        {
            foreach (Node node in Root.Nodes)
            {
                if (node.FullName == FullName)
                {
                    return node;
                }

                if (node is Compound && recur)
                {
                    Node found = GetNode(FullName, node as Compound, true);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        public PortObj GetElement(string FullName)
        {
            foreach (Node node in Root.Nodes)
            {
                if(!(node is Compound))
                {
                    foreach (PortObj elem in node.Elements)
                    {
                        if (elem.FullName == FullName)
                        {
                            return elem;
                        }
                    }
                }
            }

            return null;
        }

        public List<Node> GetNodes(string inName, bool inConsiderNodes, bool inConsiderCompounds)
        {
            return GetNodes(inName, Root, true, inConsiderNodes, inConsiderCompounds);
        }

        /// <summary>
        /// Give the list of nodes with the gven name, given that only FullNames must be unique.
        /// </summary>
        /// <param name="FullName">Name of the node</param>
        /// <param name="Root">Compound from which begin the search</param>
        /// <param name="recur">Indicate if we need to search recursively</param>
        /// <returns>The found nodes</returns>
        public List<Node> GetNodes(string inName, Compound Root, bool recur, bool inConsiderNodes, bool inConsiderCompounds, bool regexp)
        {
            List<Node> matchingNodes = new List<Node>();
            Regex reg = null;
            if (regexp)
            {
                reg = new Regex(inName);
            }

            foreach (Node node in Root.Nodes)
            {
                if (node is Compound)
                {
                    if (inConsiderCompounds)
                    {
                        if (regexp)
                        {
                            if (reg.Match(node.Name).Success)
                            {
                                matchingNodes.Add(node);
                            }
                        }
                        else
                        {
                            if (node.Name == inName)
                            {
                                matchingNodes.Add(node);
                            }
                        }
                    }

                    if (recur)
                    {
                        matchingNodes.AddRange(GetNodes(inName, node as Compound, true, inConsiderNodes, inConsiderCompounds, regexp));
                    }
                }
                else if (inConsiderNodes)
                {
                    if (regexp)
                    {
                        if (reg.Match(node.Name).Success)
                        {
                            matchingNodes.Add(node);
                        }
                    }
                    else
                    {
                        if (node.Name == inName)
                        {
                            matchingNodes.Add(node);
                        }
                        
                    }
                }
            }

            return matchingNodes;
        }

        /// <summary>
        /// Give the list of nodes with the gven name, given that only FullNames must be unique.
        /// </summary>
        /// <param name="FullName">Name of the node</param>
        /// <param name="Root">Compound from which begin the search</param>
        /// <param name="recur">Indicate if we need to search recursively</param>
        /// <returns>The found nodes</returns>
        public List<Node> GetNodes(string inName, Compound Root, bool recur, bool inConsiderNodes, bool inConsiderCompounds)
        {
            return GetNodes(inName, Root, recur, inConsiderNodes, inConsiderCompounds, false);
        }
        /// <summary>
        /// Find a node in the "AvailableNodes" list (library for creation)
        /// </summary>
        /// <param name="NativeName"></param>
        /// <returns></returns>
        public Node GetAvailableNode(string NativeName)
        {
            foreach (Node node in AvailableNodes)
            {
                if (node.Name == NativeName)
                {
                    return node;
                }
            }

            return null;
        }

        // --- Navigation ---

        // Compounds management =================================================================

        /// <summary>
        /// Add a new Compound, containing all nodes given as argument
        /// </summary>
        /// <param name="content">Nodes to be added to the compound</param>
        /// <returns>The new Compound</returns>
        //public Compound AddCompound(List<Node> content)
        //{
        //    Compound NewComp = (Compound)Activator.CreateInstance(AvailableCompound.GetType(), new object[0], new object[0]);
        //    NewComp.Copy(AvailableCompound, true);

        //    AddNode(NewComp, CurCompound, (int)content[0].UIx, (int)content[0].UIy);

        //    //Move Nodes
        //    foreach (Node Node in content)
        //    {
        //        CurCompound.MoveNode(Node, NewComp);
        //    }

        //    Root.SortNodes();

        //    //Manage ports visibilities (hide unlinked ports)

        //    foreach (Port port2 in NewComp.Inputs)
        //    {
        //        if (!(port2 as PortInstance).IsLinked())
        //        {
        //            port2.Visible = false;
        //        }
        //    }

        //    foreach (Port port2 in NewComp.Outputs)
        //    {
        //        if (!(port2 as PortInstance).IsLinked())
        //        {
        //            port2.Visible = false;
        //        }
        //    }

        //    OnNodesChanged(new NodesChangedEventArgs(Operations.NodesMoved, content));
        //    return NewComp;
        //}

        public Compound AddCompound(List<Node> content)
        {
            return AddCompound(content, null);
        }

        public Compound AddCompound(List<Node> content, Compound inCompound)
        {
            Compound NewComp = null;
            if (inCompound == null)
            {
                NewComp = (Compound)Activator.CreateInstance(AvailableCompound.GetType(), new object[0], new object[0]);
                NewComp.Copy(AvailableCompound, true);
            }
            else
            {
                NewComp = inCompound;
                inCompound.Deleted = false;
            }

            AddNode(NewComp, CurCompound, (int)content[0].UIx, (int)content[0].UIy);

            //Move Nodes
            foreach (Node Node in content)
            {
                CurCompound.MoveNode(Node, NewComp);
            }

            Root.SortNodes();

            //Manage ports visibilities (hide unlinked ports)

            foreach (Port port2 in NewComp.Inputs)
            {
                if (!(port2 as PortInstance).IsLinked())
                {
                    port2.Visible = false;
                }
            }

            foreach (Port port2 in NewComp.Outputs)
            {
                if (!(port2 as PortInstance).IsLinked())
                {
                    port2.Visible = false;
                }
            }

            OnNodesChanged(new NodesChangedEventArgs(Operations.NodesMoved, content));
            return NewComp;
        }


        /// <summary>
        /// Add a new Compound, containing all nodes given as argument
        /// </summary>
        /// <param name="content">Nodes to be added to the compound</param>
        /// <returns>The new Compound</returns>
        public void MoveNodes(List<Node> content, Compound NewComp)
        {
            //Move Nodes
            foreach (Node Node in content)
            {
                Compound oldParent = content[0].Parent;
                if (content[0].Parent != NewComp)
                {
                    content[0].Parent.MoveNode(Node, NewComp);
                    oldParent.RefreshVisibilities();
                    NewComp.RefreshVisibilities();
                }
            }

            Root.SortNodes();

            OnNodesChanged(new NodesChangedEventArgs(Operations.NodesMoved, content));
        }


        /// <summary>
        /// Enters the given compound
        /// </summary>
        /// <param name="inNode">The Compound to enter</param>
        public void EnterCompound(Compound inNode)
        {
            if (!BreadCrumbs.Contains(inNode))
            {
                BreadCrumbs.Add(inNode);
            }
        }

        /// <summary>
        /// Explode (Empty and remove) the given compound
        /// </summary>
        /// <param name="comp">The Compound to remove</param>
        public void ExplodeCompound(Compound comp)
        {
            if (comp != null)
            {
                //Collect children in a separate collection
                float avgX = 0;
                float avgY = 0;

                List<Node> oldChildren = new List<Node>();
                foreach (Node node in comp.Nodes)
                {
                    avgX += node.UIx;
                    avgY += node.UIy;
                    oldChildren.Add(node);
                }

                avgX /= oldChildren.Count;
                avgY /= oldChildren.Count;

                //calculate offset
                avgX = comp.UIx - avgX;
                avgY = comp.UIy - avgY;

                //Move Nodes
                foreach (Node node in oldChildren)
                {
                    node.UIx += avgX;
                    node.UIy += avgY;

                    comp.MoveNode(node, CurCompound);
                }

                CurCompound.RemoveNode(comp, true);
                OnNodesChanged(new NodesChangedEventArgs(Operations.NodesRemoved, new List<Node>{comp}));
            }
        }

        /// <summary>
        /// Exits the given compound
        /// </summary>
        /// <param name="inNode">The Compound to exit</param>
        public void ExitCompound()
        {
            if (BreadCrumbs.Count > 1)
            {
                Compound curComp = CurCompound;
                BreadCrumbs.RemoveAt(BreadCrumbs.Count - 1);

            }
        }

        /// <summary>
        /// Jump (enters) the compound given as an index of already explored Compounds (BreadCrumbs)
        /// </summary>
        /// <param name="index">Index of the compound to enter</param>
        public void JumpCompound(int index)
        {
            if (index < BreadCrumbs.Count)
            {
                int remove = BreadCrumbs.Count - index - 1;

                for (; remove > 0; remove--)
                {
                    BreadCrumbs.RemoveAt(BreadCrumbs.Count - 1);
                }
            }
        }

        /// <summary>
        /// Jump (enters) the compound given as argument
        /// </summary>
        /// <param name="inComp">Compound to jump to</param>
        public void JumpCompound(Compound inComp)
        {
            int index = BreadCrumbs.IndexOf(inComp);
            if (index != -1)
            {
                JumpCompound(index);
            }
            else
            {
                JumpCompound(0);

                //Get path
                Compound parent = inComp.Parent;
                List<Compound> path = new List<Compound>();
                path.Add(inComp);

                while (parent != Root)
                {
                    path.Add(parent);
                    parent = parent.Parent;
                }

                //Recreate the breadcrumbs path
                if (path.Count > 0)
                {
                    path.Reverse();
                    foreach (Compound comp in path)
                    {
                        BreadCrumbs.Add(comp);
                    }
                }
            }
        }

        /// <summary>
        /// Create a new "scene", containing the Compound given as argument if one.
        /// </summary>
        /// <param name="inNode">The Compound to enter or null, to create an empty scene</param>
        /// <param name="Create">Indicates if the given Compound should be created (false is used if a "ready-to-go" compound was loaded by other means).</param>
        public void NewLayout(Compound inNode, bool Create)
        {
            if (CurCompound != null)
            {
                JumpCompound(0);

                if (Root != null)
                {
                    Root.Remove(true);
                }
            }
            else
            {
            }

            if (inNode == null)
            {
                inNode = (Compound)Activator.CreateInstance(AvailableCompound.GetType(), new object[0], new object[0]);
                inNode.Copy(AvailableCompound, true);
            }
            else
            {
                Compound newRoot = (Compound)Activator.CreateInstance(inNode.GetType(), new object[0], new object[0]);
                newRoot.Copy(inNode, true);
                inNode = newRoot;
            }

            BreadCrumbs.Clear();
            BreadCrumbs.Add(inNode);
            inNode.Companion = Companion;
            inNode.Create(inNode.Name);

            if (inNode.Nodes.Count > 0)
            {
                Root.SortNodes();

                List<Node> toAdd = inNode.GetChildren(true);

                if (Create)
                {
                    Companion.LaunchProcess("Open Compound " + inNode.FullName, toAdd.Count);

                    //Sort SubNodes and create
                    List<Node> subNodes = new List<Node>();

                    foreach (Node node in inNode.Nodes)
                    {
                        subNodes.Add(node);
                    }

                    subNodes.Sort();

                    foreach (Node node in subNodes)
                    {
                        AddNode(node, Root, (int)node.UIx, (int)node.UIy);
                        Companion.ProgressBarIncrement();
                    }

                    Companion.EndProcess();
                }
                else
                {
                    List<Compound> treated = new List<Compound>();

                    foreach (Node node in toAdd)
                    {
                        if (node.Parent != null && !treated.Contains(node.Parent))
                        {
                            node.Parent.RefreshPortsIndices();
                            treated.Add(node.Parent);
                        }
                        node.RefreshPortsIndices();
                    }
                }
            }

            Root.RefreshPortsIndices();

            EnterCompound(Root);
        }

        /// <summary>
        /// Create an empty scene
        /// </summary>
        public void NewLayout()
        {
            NewLayout(null, true);
        }

        /// <summary>
        /// Add a node in the given compound
        /// </summary>
        /// <param name="inName">Name of the node to add (from the AvalaibleNodes List)</param>
        /// <param name="inParent">Parent compound of the node</param>
        /// <param name="X">X position of the node</param>
        /// <param name="Y">Y position of the node</param>
        /// <returns>The new Node</returns>
        public Node AddNode(string inName, Compound inParent, int X, int Y, string name)
        {
            Node node = GetAvailableNode(inName);
            Node NewNode = null;
            if (node != null)
            {
                NewNode = (Node)Activator.CreateInstance(node.GetType(), new object[0], new object[0]);
                NewNode.Copy(node, true);

                AddNode(NewNode, inParent, X, Y, name);
            }

            return NewNode;
        }

        public Node AddNode(string inName, Compound inParent, int X, int Y)
        {
            return AddNode(inName, inParent, X, Y, string.Empty);
        }


        /// <summary>
        /// Add a node in the given compound
        /// </summary>
        /// <param name="index">Index of the node to add (from the AvalaibleNodes List)</param>
        /// <param name="inParent">Parent compound of the node</param>
        /// <param name="X">X position of the node</param>
        /// <param name="Y">Y position of the node</param>
        /// <returns>The new Node</returns>
        public Node AddNode(int index, Compound inParent, int X, int Y, string name)
        {
            if (index < AvailableNodes.Count)
            {
                return AddNode(AvailableNodes[index], CurCompound, X, Y, true, name);
            }

            return null;
        }

        public Node AddNode(int index, Compound inParent, int X, int Y)
        {
            return AddNode(index, inParent, X, Y, string.Empty);
        }

        /// <summary>
        /// Add the given Node in the given Compound
        /// </summary>
        /// <param name="NewNode">Node to add</param>
        /// <param name="inParent">Parent compound of the Node</param>
        /// <param name="X">X position of the node</param>
        /// <param name="Y">Y position of the node</param>
        /// <returns>The new Node</returns>
        public Node AddNode(Node NewNode, Compound inParent, int X, int Y, string name)
        {
            return AddNode(NewNode, inParent, X, Y, false, name);
        }

        public Node AddNode(Node NewNode, Compound inParent, int X, int Y)
        {
            return AddNode(NewNode, inParent, X, Y, string.Empty);
        }

        /// <summary>
        /// Add the given Node in the given Compound
        /// </summary>
        /// <param name="NewNode">Node to add</param>
        /// <param name="inParent">Parent compound of the Node</param>
        /// <param name="X">X position of the node</param>
        /// <param name="Y">Y position of the node</param>
        /// <returns>The new Node</returns>
        public Node AddNode(Node NewNode, Compound inParent, int X, int Y, bool asCopy, string name)
        {
            NodeConnexions cons = null;

            if (asCopy)
            {
                cons = new NodeConnexions(NewNode);

                Node copy = (Node)Activator.CreateInstance(NewNode.GetType(), new object[0], new object[0]);
                copy.Copy(NewNode, true);
                copy.DisconnectAll();
                copy.Init();

                NewNode = copy;
            }

            if (name == string.Empty)
            {
                name = NewNode.FullName;
            }

            string uniqueName = SetNodeUniqueName(name, NewNode);

            NewNode.UIx = X;
            NewNode.UIy = Y;
            NewNode.FullName = uniqueName;
            NewNode.Companion = Companion;
            inParent.AddNode(NewNode);

            NewNode.Create(uniqueName);

            Root.SortNodes();

            Compound comp = NewNode as Compound;

            if (comp != null)
            {
                //Sort SubNodes and create
                List<Node> subNodes = new List<Node>();

                foreach (Node node in comp.Nodes)
                {
                    subNodes.Add(node);
                }

                subNodes.Sort();

                Companion.LaunchProcess("Add Compound " + comp.FullName, subNodes.Count);

                foreach (Node node in subNodes)
                {
                    AddNode(node, comp, (int)node.UIx, (int)node.UIy);
                    Companion.ProgressBarIncrement();
                }

                Companion.EndProcess();
                comp.RefreshPortsIndices();
            }

            OnNodesChanged(new NodesChangedEventArgs(Operations.NodeAdded, new List<Node>{NewNode}));

            if (cons != null)
            {
                cons.Reconnect(NewNode);
            }

            return NewNode;
        }

        public Node AddNode(Node NewNode, Compound inParent, int X, int Y, bool asCopy)
        {
            return AddNode(NewNode, inParent, X, Y, asCopy, string.Empty);
        }

        /// <summary>
        /// Remove a specific Node
        /// </summary>
        /// <param name="node">Node to remove</param>
        public void RemoveNode(Node node)
        {
            node.Parent.RemoveNode(node, true);
            OnNodesChanged(new NodesChangedEventArgs(Operations.NodesRemoved, new List<Node>{node}));
        }

        /// <summary>
        /// Get the current list of Links (displaying in the current compound)
        /// </summary>
        /// <returns>The list of Links</returns>
        public List<Link> GetLinks()
        {
            return GetLinks(false);
        }
       
        /// <summary>
        /// Get the list of Links
        /// </summary>
        /// <param name="All">Indicate if we have to collect all links (true) or just those showing in the current Compound</param>
        /// <returnsThe list of Links></returns>
        internal List<Link> GetLinks(bool All)
        {
            Compound currentRoot = All ? Root : CurCompound;

            List<Link> links = new List<Link>();
            Compound parent;
            foreach (Link link in currentRoot.InDependencies)
            {
                if (link.Source.Owner != null)
                {
                    if (All && !links.Contains(link))
                    {
                        links.Add(link);
                    }
                    else
                    {
                        parent = GetCommonParent(link.Source.Owner, link.Target.Owner);
                        if ((parent == CurCompound || CurCompound.IsIn(parent)) && !links.Contains(link))
                        {
                            links.Add(link);
                        }
                    }
                }
            }

            if (currentRoot != Root)
            {
                foreach (Link link in currentRoot.OutDependencies)
                {
                    if (link.Source.Owner != null)
                    {
                        parent = GetCommonParent(link.Source.Owner, link.Target.Owner);
                        if ((parent == CurCompound || CurCompound.IsIn(parent)) && !links.Contains(link))
                        {
                            links.Add(link);
                        }
                    }
                }
            }

            return links;
        }

        /// <summary>
        /// Return the first common Compound of the two nodes, to know if a link between the two should be showing
        /// </summary>
        /// <param name="node">First node</param>
        /// <param name="node_2">Second node</param>
        /// <returns>The common Compound of the two Nodes</returns>
        private Compound GetCommonParent(Node node, Node node_2)
        {
            int deep1 = node.Deepness;
            int deep2 = node_2.Deepness;

            Compound parent1 = node.Parent;
            Compound parent2 = node_2.Parent;

            while (parent1 != parent2)
            {
                if (deep1 > deep2)
                {
                    parent1 = parent1.Parent;
                    deep1--;
                }
                else
                {
                    if (deep1 < deep2)
                    {
                        parent2 = parent2.Parent;
                        deep2--;
                    }
                    else
                    {
                        parent1 = parent1.Parent;
                        parent2 = parent2.Parent;

                        if (parent1 == null || parent2 == null)
                        {
                            return Root;
                        }
                    }
                }
            }

            return parent1;
        }

        /// <summary>
        /// Copy the given node
        /// </summary>
        /// <param name="Node">The source Node for the copy</param>
        /// <returns>The cloned node</returns>
        public Node Copy(Node Node)
        {
            return Copy(Node, Node.Parent, (int)Node.UIx + 20, (int)Node.UIy + 20);
        }

        /// <summary>
        /// Copy the given node
        /// </summary>
        /// <param name="Node">The source Node for the copy</param>
        /// <param name="Parent">The new parent Compound for the cloned Node</param>
        /// <param name="X">X position of the cloned node</param>
        /// <param name="Y">Y position of the cloned node</param>
        /// <returns>The cloned node</returns>
        public Node Copy(Node Node, Compound Parent, int X, int Y, string inSearch, string inReplace)
        {
            Node.UpdateBeforeCopy();

            Node node = (Node)Activator.CreateInstance(Node.GetType(), new object[0], new object[0]);
            node.Copy(Node, true);
            node.UIx += 20;
            node.UIy += 20;
            if (node is Compound)
            {
                (node as Compound).ResolveAll(false);
            }

            if (!String.IsNullOrEmpty(inSearch))
            {
                if (node is Compound)
                {
                    searchAndReplaceInCompound(node as Compound, inSearch, inReplace);
                }
                else
                {
                    if (node.FullName.Contains(inSearch))
                    {
                        node.FullName = node.FullName.Replace(inSearch, inReplace);
                    }
                }
            }

            AddNode(node, Parent, X, Y);
            Root.ResolveAll(true);

            node.ReConnect();

            Root.SortNodes();

            return node;
        }

        private void searchAndReplaceInCompound(Compound compound, string inSearch, string inReplace)
        {
            if (compound.FullName.Contains(inSearch))
            {
                compound.FullName = compound.FullName.Replace(inSearch, inReplace);
            }

            foreach(Node node in compound.Nodes)
            {
                if (node is Compound)
                {
                    searchAndReplaceInCompound(node as Compound, inSearch, inReplace);
                }
                else
                {
                    if (node.FullName.Contains(inSearch))
                    {
                        node.FullName = node.FullName.Replace(inSearch, inReplace);
                    }
                }
            }
        }

        private void searchAndReplaceCompound(Compound compound)
        {
            throw new NotImplementedException();
        }

        public Node Copy(Node Node, Compound Parent, int X, int Y)
        {
            return Copy(Node, Parent, X, Y, "", "");
        }

        /// <summary>
        /// Tells if a node with the given name exist
        /// </summary>
        /// <param name="fullName">FullName of the node to search for</param>
        /// <returns>true if a node exists, false otherwise</returns>
        public bool NodeExists(string fullName)
        {
            return NodeExists(fullName, Root);
        }

        /// <summary>
        /// Tells if a node with the given name exist under the given Compound
        /// </summary>
        /// <param name="fullName">FullName of the node to search for</param>
        /// <param name="parent">Parent Compound from which to start the search</param>
        /// <returns>true if a node exists, false otherwise</returns>
        public bool NodeExists(string fullName, Compound parent)
        {
            foreach (Node node in parent.Nodes)
            {
                if (node.FullName == fullName)
                {
                    return true;
                }

                if (node is Compound)
                {
                    if (NodeExists(fullName, node as Compound))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Expand the nodes
        /// </summary>
        /// <param name="selOnly">Only the selection is updated if true, all otherwise.</param>
        public void Expand(bool selOnly)
        {
            List<Node> nodes = CurCompound.Nodes;

            foreach (Node node in CurCompound.Nodes)
            {
                if (!selOnly || node.Selected)
                {
                    node.DisplayState = NodeState.Normal;
                }
            }
        }

        /// <summary>
        /// Minmize the nodes
        /// </summary>
        /// <param name="selOnly">Only the selection is updated if true, all otherwise.</param>
        public void Minimize(bool selOnly)
        {
            foreach (Node node in CurCompound.Nodes)
            {
                if (!selOnly || node.Selected)
                {
                    node.DisplayState = NodeState.Minimal;
                }
            }
        }

        /// <summary>
        /// Collapse the nodes
        /// </summary>
        /// <param name="selOnly">Only the selection is updated if true, all otherwise.</param>
        public void Collapse(bool selOnly)
        {
            foreach (Node node in CurCompound.Nodes)
            {
                if (!selOnly || node.Selected)
                {
                    node.DisplayState = NodeState.Collapsed;
                }
            }
        }

        /// <summary>
        /// Give the Node by which the given Node is reachable from the current compound (or itself if it's in the current compound).
        /// </summary>
        /// <param name="node">The node to find</param>
        /// <returns>The Node by which the given Node is reachable from the current compound or itself</returns>
        public Node LevelNode(Node node)
        {
            return node.Level(CurCompound);
        }

        /// <summary>
        /// Give the whole branch from a given Node
        /// </summary>
        /// <param name="Node">Root Node of the branch</param>
        /// <returns>The whole branch</returns>
        internal List<Node> GetBranch(Node Node)
        {
            List<Node> branch = new List<Node>();

            List<Node> depend = Node.GetDependentNodes(true);

            foreach (Node node in depend)
            {
                Node leveled = LevelNode(node);
                if(leveled != null && !branch.Contains(leveled))
                {
                    branch.Add(leveled);
                }
            }

            branch.Add(Node);

            return branch;
        }

        /// <summary>
        /// Update a node already loaded in the current "scene" from a new version in the library (remapping its ports and adding new ones if necessary)
        /// </summary>
        /// <param name="inNode">The node that was changed</param>
        /// <returns>The list of modified nodes</returns>
        public List<Node> UpdateNode(Node inNode)
        {
            //Check usage !!

            bool NeedRemap = false;

            List<Node> UsedNodes = Root.FindInstances(inNode, true);
            DialogResult rslt = DialogResult.Cancel;
            DataMap map = new DataMap();
            PortModifications mods = null;
            List<PortModifications> allMods = new List<PortModifications>();
            Dictionary<string, List<string>> mappings = new Dictionary<string,List<string>>();

            if (UsedNodes.Count > 0)
            {
                switch (MessageBox.Show(inNode.NativeName + " was updated, and " + UsedNodes.Count + " instances are loaded in the nodal Editor, do you want to update them ?\n(Choosing \"No\" will Freeze these nodes)", "Update Node", MessageBoxButtons.YesNo))
                {
                    case DialogResult.No:
                        foreach (Node node in UsedNodes)
                        {
                            node.Freezed = true;
                        }
                        break;
                    default:
                        foreach (Node node in UsedNodes)
                        {
                            if (mods == null)
                            {
                                mods = node.Update(inNode, false);
                                allMods.Add(mods);

                                //The first time we can ask for remapping
                                //Only if ports were removed
                                if (mods.OldPorts.Count > 0)
                                {
                                    NeedRemap = true;

                                    //if ports were added we can ask for remapping data
                                    if (mods.NewPorts.Count > 0)
                                    {
                                        List<string> OldVals = new List<string>();
                                        foreach (Port port in mods.OldPorts)
                                        {
                                            if (!OldVals.Contains(port.Name))
                                            {
                                                OldVals.Add(port.Name);
                                            }
                                        }

                                        List<string> NewVals = new List<string>();
                                        foreach (Port port in mods.NewPorts)
                                        {
                                            if (!NewVals.Contains(port.Name))
                                            {
                                                NewVals.Add(port.Name);
                                            }
                                        }

                                        map.Init(inNode.Name + " ports remapping", OldVals, NewVals);
                                        rslt = map.ShowDialog();
                                    }
                                }
                            }
                            else
                            {
                                mods = node.Update(inNode, false);
                                allMods.Add(mods);
                            }
                        }

                        if(NeedRemap)
                        {
                            //Perform remap
                            if (rslt == DialogResult.OK)
                            {
                                mappings = map.getMappings();
                            }

                            foreach (PortModifications portMod in allMods)
                            {
                                foreach (Port old in portMod.OldPorts)
                                {
                                    //Remap or delete ?
                                    Port newPort = null;

                                    //Find remapped port
                                    if (mappings.ContainsKey(old.NativeName))
                                    {
                                        string portName = mappings[old.NativeName][0];
                                        foreach (Port port in portMod.NewPorts)
                                        {
                                            if (port.DetailedName() == old.DetailedName(portName))
                                            {
                                                newPort = port;
                                                break;
                                            }
                                        }
                                    }

                                    foreach (Link link in old.Dependencies)
                                    {
                                        //if we found a remap, reassign on the link
                                        if (newPort != null)
                                        {
                                            newPort.Dependencies.Add(link);

                                            if (old.IsOutput)
                                            {
                                                link.Source = newPort;
                                            }
                                            else
                                            {
                                                link.Target = newPort;
                                            }
                                        }
                                        else
                                        {//if we can't found a remap, remove the link
                                            if (old.IsOutput)
                                            {
                                                link.Target.Dependencies.Remove(link);
                                            }
                                            else
                                            {
                                                link.Source.Dependencies.Remove(link);
                                            }
                                        }
                                    }
                                    
                                    old.Dependencies.Clear();
                                }
                            }

                            //Refresh ports indices and PortInstances
                            foreach (Node refreshedNode in UsedNodes)
                            {
                                refreshedNode.RefreshPortsIndices();
                                if (refreshedNode.Parent != null)
                                {
                                    refreshedNode.Parent.RefreshPorts();
                                }
                            }

                            Root.ResolveAll(true);
                        }

                        /*
                            if (!mods.IsEmpty())
                            {
                                if (rslt == DialogResult.OK)
                                {
                                    
                                    foreach (Link link in links)
                                    {
                                        if (mods.OldPorts.Contains(link.Target))
                                        {
                                            if(mappings.ContainsKey(link.Target.NativeName))
                                            {
                                                string portName = mappings[link.Target.NativeName][0];
                                                
                                                Port newPort = null;
                                                foreach(Port port in mods.NewPorts)
                                                {
                                                    if(port.NativeName == portName)
                                                    {
                                                        newPort = port;
                                                        break;
                                                    }
                                                }

                                                if (newPort != null)
                                                {
                                                    link.Target = newPort;
                                                }
                                                else
                                                {
                                                    //remove the link
                                                    links.Remove(link);
                                                    link.Target.Dependencies.Remove(link);
                                                    link.Source.Dependencies.Remove(link);
                                                }
                                            }
                                            else
                                            {
                                                //remove the link
                                                links.Remove(link);
                                                link.Target.Dependencies.Remove(link);
                                                link.Source.Dependencies.Remove(link);
                                            }
                                        }
                                    }

                                    foreach (Link link in links)
                                    {
                                        if (mods.OldPorts.Contains(link.Source))
                                        {
                                            if (mappings.ContainsKey(link.Source.NativeName))
                                            {
                                                string portName = mappings[link.Source.NativeName][0];

                                                Port newPort = null;
                                                foreach (Port port in mods.NewPorts)
                                                {
                                                    if (port.NativeName == portName)
                                                    {
                                                        newPort = port;
                                                        break;
                                                    }
                                                }

                                                if (newPort != null)
                                                {
                                                    link.Source = newPort;
                                                }
                                                else
                                                {
                                                    //remove the link
                                                    link.Target.Dependencies.Remove(link);
                                                    link.Source.Dependencies.Remove(link);
                                                }
                                            }
                                            else
                                            {
                                                //remove the link
                                                link.Target.Dependencies.Remove(link);
                                                link.Source.Dependencies.Remove(link);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //No remap

                                    foreach (Link link in links)
                                    {
                                        if(mods.OldPorts.Contains(link.Target))
                                        {
                                            //remove the link
                                            link.Target.Dependencies.Remove(link);
                                            link.Source.Dependencies.Remove(link);
                                        }
                                    }
                                }
                            }
                        }*/

                        break;
                }
            }

            return (NeedRemap ? UsedNodes : new List<Node>());
        }

        /// <summary>
        /// Substitute a node with another one, checking for port names that fits for remapping
        /// </summary>
        /// <param name="inNode">Existing Node to be sustituted</param>
        /// <param name="index">Index of the new node from the library</param>
        public void SubstituteNode(Node inNode, int index)
        {
            Node newNode = AddNode(index, inNode.Parent, (int)inNode.UIx, (int)inNode.UIy);
            //Base properties
            
            //Wait to apply the Name
            string oldName = inNode.Name;
            inNode.UpdateBeforeCopy();
            string message = inNode.Substitute(newNode);

            inNode.Parent.RemoveNode(inNode, true);
            newNode.Name = oldName;

            Companion.Log(message);
        }

        #endregion

        public int GetBreadCrumbsIndex(Compound comp)
        {
            int index = 0;
            foreach (Compound breadComp in BreadCrumbs)
            {
                if (breadComp.FullName == comp.FullName)
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        public NodalEditorPreferences Preferences = null;

        #region INodalEditorCommands

        public bool DeleteNode(string inNodeName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
