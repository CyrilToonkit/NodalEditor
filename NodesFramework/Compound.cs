using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using TK.NodalEditor;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml.Schema;
using TK.BaseLib;
using TK.NodalEditor.NodesFramework;
using TK.BaseLib.Processes;

namespace TK.NodalEditor
{
    [Serializable()]
    [XmlRoot("Compound")]
    public class Compound : Node, IXmlSerializable
    {
        #region MEMBERS

        /// <summary>
        /// Nodes contained in the compound
        /// </summary>
        List<Node> _nodes = new List<Node>();

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Nodes contained in the compound
        /// </summary>
        [BrowsableAttribute(false)]
        public List<Node> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        public new ManagerCompanion Companion
        {
            get { return base.Companion; }
            set
            {
                base.Companion = value;
                foreach(Node child in GetChildren(false))
                {
                    child.Companion = value;
                }
            }
        }

        public override string NodeType
        {
            get { return "Compound"; }
        }

        #endregion
        
        #region METHODS

        public override void Init()
        {

        }

        public override void Remove()
        {
            Remove(false);
        }

        /// <summary>
        /// Remove overload used when we don't want to call Node.Remove() on each children
        /// </summary>
        /// <param name="QuickRemove">True if we want to avoid removing children</param>
        public void Remove(bool QuickRemove)
        {
            if (!QuickRemove)
            {
                foreach (Node Rig in Nodes)
                {
                    UnConnectAll(Rig);
                    Rig.Remove();
                }
            }
            RemoveObject();
            Nodes.Clear();
        }

        public override void Copy(Node inNode, bool Resolve)
        {
            Compound com = inNode as Compound;
            if (com != null)
            {
                Inputs = new List<Port>();
                Outputs = new List<Port>();

                _nativeName = com.NativeName;
                _name = com.Name;
                mDescription = com.Description;
                mCategory = com.Category;
                mTags = com.Tags;

                mUIx = com.UIx;
                mUIy = com.UIy;
                mPath = inNode.Path;
                mUser = inNode.User;
                mExportDate = inNode.ExportDate;
                mCheckedOut = inNode.CheckedOut;
                mDisplayState = com.DisplayState;
                mFreezed = com.Freezed;
                mVersion = com.Version;

                RealDeepness = inNode.RealDeepness;

                _nodes = new List<Node>();

                foreach (Node rig in com.Nodes)
                {
                    Node newNode = (Node)Activator.CreateInstance(rig.GetType(), new object[0]);
                    newNode.Copy(rig, Resolve);
                    newNode.Parent = this;
                    _nodes.Add(newNode);
                }
                if (Resolve)
                {
                    com.RefreshPorts();
                    RefreshPorts();
                    DisableAutoRefreshPortsIndices();
                    int Counter = 0;
                    foreach (PortInstance port in com.Inputs)
                    {
                        Inputs[Counter].Name = port.Name;
                        Inputs[Counter].Visible = port.Visible;
                        Counter++;
                    }
                    Counter = 0;
                    foreach (PortInstance port in com.Outputs)
                    {
                        Outputs[Counter].Name = port.Name;
                        Outputs[Counter].Visible = port.Visible;
                        Counter++;
                    }
                    EnableAutoRefreshPortsIndices();
                }

                CopyCustomFields(inNode, Resolve);

                if (Resolve)
                {
                    ResolveAll(false);
                }
            }
        }

        #region Hierarchy

        /// <summary>
        /// Collect the children of this node recursively
        /// </summary>
        /// <param name="IgnoreCompounds">Tells if we have to ignore compounds</param>
        /// <returns>The list of found children</returns>
        public List<Node> GetChildren(bool IgnoreCompounds)
        {
            return GetChildren(IgnoreCompounds, null, "");
        }

        /// <summary>
        /// Collect the children of this node recursively
        /// </summary>
        /// <param name="IgnoreCompounds">Tells if we have to ignore compounds</param>
        /// <param name="inType">Filter by Type</param>
        /// <returns>The list of found children</returns>
        public List<Node> GetChildren(bool IgnoreCompounds, string inType)
        {
            List<Node> allNodes = new List<Node>();
            CollectNodes(allNodes, this, null, IgnoreCompounds, inType);
            return allNodes;
        }

        public List<Node> GetChildren(bool IgnoreCompounds, string inType, bool inRecursive)
        {
            List<Node> allNodes = new List<Node>();
            CollectNodes(allNodes, this, null, IgnoreCompounds, inType, inRecursive);
            return allNodes;
        }

        /// <summary>
        /// Collect the children of this node recursively
        /// </summary>
        /// <param name="IgnoreCompounds">Tells if we have to ignore compounds</param>
        /// <param name="Ignore">A node to ignore</param>
        /// <param name="inType">Filter by Type</param>
        /// <returns>The list of found children</returns>
        public List<Node> GetChildren(bool IgnoreCompounds, Node Ignore, string inType)
        {
            List<Node> allNodes = new List<Node>();
            CollectNodes(allNodes, this, Ignore, IgnoreCompounds, inType);
            return allNodes;
        }

        /// <summary>
        /// Actual parsing method used recursively by GetChildren
        /// </summary>
        /// <param name="Nodes">Collection of nodes being collected</param>
        /// <param name="inParent">Compound from which to search</param>
        /// <param name="Ignore">A node to ignore</param>
        /// <param name="IgnoreCompounds">Tell if we have to add the "inParent"</param>
        /// <param name="inType">Filter by Type</param>
        private void CollectNodes(List<Node> Nodes, Compound inParent, Node Ignore, bool IgnoreCompounds, string inType)
        {
            CollectNodes(Nodes, inParent, Ignore, IgnoreCompounds, inType, true);
        }

        private void CollectNodes(List<Node> Nodes, Compound inParent, Node Ignore, bool IgnoreCompounds, string inType, bool inRecursive)
        {
            if (inParent != null)// && inParent != Ignore)
            {
                if (!IgnoreCompounds && inParent != Ignore)
                {
                    Nodes.Add(inParent);
                }

                foreach (Node node in inParent.Nodes)
                {
                    if (Ignore == null || node != Ignore)
                    {
                        if (!(node is Compound) && (string.IsNullOrEmpty(inType) || node.NodeElementType == inType))
                        {
                            Nodes.Add(node);
                        }
                    }

                    if (node is Compound && inRecursive == true)
                    {
                        CollectNodes(Nodes, node as Compound, Ignore, IgnoreCompounds, inType);
                    }
                }
            }
        }


        /// <summary>
        /// Collect the nodes with the same NativeName as "newRig", used by UpdateNode in the NodesManager
        /// </summary>
        /// <param name="newRig">Reference node</param>
        /// <param name="IgnoreFreezed">If true, ignores the nodes marked as freezed</param>
        /// <returns>The found nodes</returns>
        public List<Node> FindInstances(Node newRig, bool IgnoreFreezed)
        {
            List<Node> instances = new List<Node>();
            List<Node> nodes = GetChildren(true);
            foreach (Node node in nodes)
            {
                if (!(IgnoreFreezed && node.Freezed))
                {
                    if (newRig.NativeName == node.NativeName)
                    {
                        instances.Add(node);
                    }
                }
            }

            return instances;
        }

        #endregion

        #region Nodes
        /// <summary>
        /// Move a node from this compound to another
        /// </summary>
        /// <param name="SourceNode">Node that is moved out</param>
        /// <param name="TargetCompound">The new Compound that contains the node</param>
        public void MoveNode(Node SourceNode, Compound TargetCompound)
        {
            RemoveNode(SourceNode, false);
            TargetCompound.AddNode(SourceNode);
            MoveObject(SourceNode, TargetCompound);
        }

        /// <summary>
        /// Virtual to be used by inherited classes when a node is moved from this compound to another (called by MoveNode)
        /// </summary>
        /// <param name="SourceNode"></param>
        /// <param name="TargetCompound"></param>
        public virtual void MoveObject(Node SourceNode, Compound TargetCompound)
        {

        }

        /// <summary>
        /// A node is added to this Compound (could be new or moved from another Parent with MoveNode)
        /// </summary>
        /// <param name="SourceNode">The node to be added</param>
        public void AddNode(Node SourceNode)
        {
            if (!Nodes.Contains(SourceNode))
            {
                _nodes.Add(SourceNode);
                RefreshPorts();
            }

            SourceNode.Parent = this;
            NodeAdded(SourceNode);
        }

        /// <summary>
        /// Virtual to be used by inherited classes when a node is added to this (called by AddNode)
        /// </summary>
        /// <param name="SourceNode"></param>
        protected virtual void NodeAdded(Node SourceNode)
        {

        }

        /// <summary>
        /// Remove a node from this compound, it could delete the node, or just be used after a node has moved to another Parent
        /// </summary>
        /// <param name="iNode">Node to remove</param>
        /// <param name="delete">Tells if we want to delete the node</param>
        public void RemoveNode(Node iNode, bool delete)
        {
            //Remove inputs \ outputs
            List<Port> ToRemove = new List<Port>();

            Nodes.Remove(iNode);

            if (delete)
            {
                UnConnectAll(iNode);
                iNode.Remove();
            }
            RefreshPorts();
            //RefreshPortsIndices(); // already done at the end of RefreshPorts
        }

        /// <summary>
        /// Reconnect all children
        /// </summary>
        public virtual void ReConnectAll()
        {
            foreach (Node node in Nodes)
            {
                List<Link> links = node.InDependencies;
                foreach (Link link in links)
                {
                    node.ReConnect(link, "None");
                }
            }
        }

        /// <summary>
        /// Sort children nodes in order of dependancy, this method is meant to be called on the root
        /// </summary>
        public void SortNodes()
        {
            SortNodes(true, false);
        }

        public void SortNodesLegacy(bool firstPass, bool inLog)
        {
            if (firstPass)
            {
                if (Companion.DEBUG)
                {
                    Tracer.Instance.BeginCall("SortNodes");
                }

                RealDeepness = 0;

                //Put every realDeepnesses to 0
                List<Node> compounds = GetChildren(false, "Compound");

                foreach (Node node in compounds)
                {
                    node.RealDeepness = 0;
                }
            }

            LevelSorter sorter = new LevelSorter();

            //Get all nodes
            List<Node> nodes = GetChildren(true);

            if (firstPass && inLog)
            {
                List<string> thisNodes = new List<string>();
                foreach (Node node in nodes)
                {
                    thisNodes.Add(node.FullName);
                }

                Companion.LogDebug("Source nodes list :\n -"+TypesHelper.Join(thisNodes, "\n -"));
            }

            //SortedElements ← Empty list that will contain the sorted elements
            List<Node> SortedElements = new List<Node>();

            //Freenodes ← Set of all nodes with no incoming edges
            List<Node> Freenodes = new List<Node>();

            List<Node> DependentNodes = new List<Node>();

            //search freenodes
            foreach (Node node in nodes)
            {
                node.TempInDependencies.Clear();
                node.TempOutDependencies.Clear();

                List<Link> realInDependencies = new List<Link>();
                foreach (Link link in node.InDependencies)
                {
                    if (link.Source.Owner != node && link.IsHierachicallyInteresting)
                    {
                        realInDependencies.Add(link);
                    }
                }

                List<Link> realOutDependencies = new List<Link>();
                foreach (Link link in node.OutDependencies)
                {
                    if (link.Target.Owner != node && link.IsHierachicallyInteresting)
                    {
                        realOutDependencies.Add(link);
                    }
                }

                if (realInDependencies.Count == 0)
                {
                    Freenodes.Add(node);
                }
                else
                {
                    foreach (Link link in realInDependencies)
                    {
                        if (!node.TempInDependencies.Contains(link.Source.Owner))
                        {
                            node.TempInDependencies.Add(link.Source.Owner);
                        }
                    }
                }

                foreach (Link link in realOutDependencies)
                {
                    if (!node.TempOutDependencies.Contains(link.Target.Owner))
                    {
                        node.TempOutDependencies.Add(link.Target.Owner);
                    }
                }
            }

            Freenodes.Sort(sorter);

            while (Freenodes.Count > 0)
            {
                if (firstPass && inLog)
                {
                    Companion.LogDebug("");
                    Companion.LogDebug("FreeNodes :");
                    foreach (Node node in Freenodes)
                    {
                        Companion.LogDebug(" - " + node.FullName);
                    }
                    Companion.LogDebug("");

                    Companion.LogDebug(string.Format("> {0} free nodes, taking {1}", Freenodes.Count, Freenodes[0].FullName));
                }

                Node freeNode = Freenodes[0];
                Freenodes.Remove(freeNode);

                SortedElements.Add(freeNode);

                //Get Dependent Nodes
                foreach (Node childNode in freeNode.TempOutDependencies)
                {
                    if (firstPass && inLog)
                    {
                        Companion.LogDebug("   >- " + childNode.FullName);
                    }

                    foreach (Node parentNode in childNode.TempInDependencies)
                    {
                        if (parentNode == freeNode)
                        {
                            childNode.TempInDependencies.Remove(freeNode);

                            if (childNode.TempInDependencies.Count == 0)
                            {
                                Freenodes.Add(childNode);
                                if (firstPass && inLog)
                                {
                                    Companion.LogDebug("   >X " + childNode.FullName);
                                }
                            }

                            break;
                        }
                    }
                }

                Freenodes.Sort(sorter);
            }

            if (firstPass)//Sort Compounds relative to last node index
            {
                if (inLog)
                {
                    Companion.Log("");
                    Companion.Log(string.Format("Sorted {0} nodes ({1} total)", SortedElements.Count, nodes.Count  ));
                    foreach (Node node in SortedElements)
                    {
                        Companion.Log(" - " + node.FullName);
                    }
                }

                Dictionary<Compound, List<Node>> managedCompounds = new Dictionary<Compound, List<Node>>();
                managedCompounds.Add(this, new List<Node>());
                int CurDeep = 1;

                foreach (Node childNode in SortedElements)
                {
                    childNode.RealDeepness = CurDeep;
                    CurDeep++;

                    CurDeep = InsertCompound(childNode, CurDeep, ref managedCompounds);
                }

                SortNodes(false, inLog);
            }
            else//Sort Compounds relative to first node index
            {
                List<Compound> compounds = new List<Compound>();
                compounds.Add(this);
                int CurDeep = 1;

                foreach (Node childNode in SortedElements)
                {
                    //Insert compound if it's the last node inside
                    if (!compounds.Contains(childNode.Parent))
                    {
                        childNode.Parent.RealDeepness = CurDeep;
                        compounds.Add(childNode.Parent);
                        CurDeep++;
                    }

                    childNode.RealDeepness = CurDeep;
                    CurDeep++;
                }

                if (Companion.DEBUG)
                {
                    Tracer.Instance.EndCall("SortNodes");
                }

                if (inLog)
                {
                    List<Node> allNodes = GetChildren(false);

                    allNodes.Sort();

                    foreach (Node node in allNodes)
                    {
                        Companion.Log(string.Format("{0,-5}: {1}", node.Depth, node.FullName));
                    }
                }
            }
        }

        string sortResult = string.Empty;
        public string SortResult
        {
            get { return sortResult; }
        }

        public bool SortNodes(bool firstPass, bool inLog)
        {
            bool sorted = true;

            if (!firstPass)
            {
                return sorted;
            }

            if (firstPass)
            {
                if (Companion.DEBUG)
                {
                    Tracer.Instance.BeginCall("SortNodes");
                }

                RealDeepness = 0;

                //Put every realDeepnesses to 0
                List<Node> compounds = GetChildren(false, "Compound");

                foreach (Node node in compounds)
                {
                    node.RealDeepness = 0;
                }
            }

            //Get all nodes
            List<Node> nodes = GetChildren(false);

            if (firstPass && inLog)
            {
                List<string> thisNodes = new List<string>();
                foreach (Node node in nodes)
                {
                    thisNodes.Add(node.FullName);
                }

                Companion.LogDebug("Source nodes list :\n -" + TypesHelper.Join(thisNodes, "\n -"));
            }

            //SortedElements ← Empty list that will contain the sorted elements
            List<Node> SortedElements = new List<Node>();

            //Freenodes ← Set of all nodes with no incoming edges
            List<Node> Freenodes = new List<Node>();

            //search freenodes
            foreach (Node node in nodes)
            {
                node.TempInDependencies.Clear();
                node.TempOutDependencies.Clear();
            }

            foreach (Node node in nodes)
            {
                List<Link> realInDependencies = new List<Link>();
                foreach (Link link in node.InDependencies)
                {
                    if (link.IsHierachicallyInteresting)
                    {
                        if (node.NodeType == "Compound")
                        {
                            Compound comp = node as Compound;
                            if (!link.Source.Owner.IsIn(comp))
                            {
                                realInDependencies.Add(link);
                                if (!link.Source.Owner.TempOutDependencies.Contains(node))
                                    link.Source.Owner.TempOutDependencies.Add(node);
                            }
                        }
                        else if (link.Source.Owner != node)
                        {
                            realInDependencies.Add(link);
                        }
                    }
                }

                //Add fake dependency to the parent compound
                if (node.Parent != null)
                {
                    node.TempInDependencies.Add(node.Parent);
                    node.Parent.TempOutDependencies.Add(node);
                }

                List<Link> realOutDependencies = new List<Link>();
                foreach (Link link in node.OutDependencies)
                {
                    if (link.IsHierachicallyInteresting)
                    {
                        if (node.NodeType == "Compound")
                        {
                            Compound comp = node as Compound;
                            if (!link.Target.Owner.IsIn(comp))
                            {
                                realOutDependencies.Add(link);
                                if(!link.Target.Owner.TempInDependencies.Contains(node))
                                    link.Target.Owner.TempInDependencies.Add(node);
                            }
                        }
                        else if (link.Target.Owner != node)
                        {
                            realOutDependencies.Add(link);
                        }
                    }
                }

                if (realInDependencies.Count + node.TempInDependencies.Count == 0)
                {
                    Freenodes.Add(node);
                }
                else
                {
                    foreach (Link link in realInDependencies)
                    {
                        if (!node.TempInDependencies.Contains(link.Source.Owner))
                        {
                            node.TempInDependencies.Add(link.Source.Owner);
                        }
                    }
                }

                foreach (Link link in realOutDependencies)
                {
                    if (!node.TempOutDependencies.Contains(link.Target.Owner))
                    {
                        node.TempOutDependencies.Add(link.Target.Owner);
                    }
                }
            }

            while (Freenodes.Count > 0)
            {
                if (firstPass && inLog)
                {
                    Companion.LogDebug("");
                    Companion.LogDebug("FreeNodes :");
                    foreach (Node node in Freenodes)
                    {
                        Companion.LogDebug(" - " + node.FullName);
                    }
                    Companion.LogDebug("");

                    Companion.LogDebug(string.Format("> {0} free nodes, taking {1}", Freenodes.Count, Freenodes[0].FullName));
                }

                Node freeNode = Freenodes[0];
                Freenodes.Remove(freeNode);

                SortedElements.Add(freeNode);

                //Get Dependent Nodes
                foreach (Node childNode in freeNode.TempOutDependencies)
                {
                    if (firstPass && inLog)
                    {
                        Companion.LogDebug("   >- " + childNode.FullName);
                    }

                    foreach (Node parentNode in childNode.TempInDependencies)
                    {
                        if (parentNode == freeNode)
                        {
                            childNode.TempInDependencies.Remove(freeNode);

                            if (childNode.TempInDependencies.Count == 0)
                            {
                                Freenodes.Add(childNode);
                                if (firstPass && inLog)
                                {
                                    Companion.LogDebug("   >X " + childNode.FullName);
                                }
                            }

                            break;
                        }
                    }
                }
            }

            string message = string.Format("Sorted {0} nodes ({1} total)", SortedElements.Count, nodes.Count);

            sortResult = string.Empty;
            if(SortedElements.Count != nodes.Count)
            {
                sorted = false;
                sortResult = message;
                if (inLog)
                {
                    Companion.Error(string.Format("These is a problem in your graph ({0}), check that your nodes or compounds do not create cycles.\nYou can use \"IsHierarchicallyInteresting\" property to help ignoring some links if necessary.", message));
                }
            }

            if (inLog)
            {
                int index = 0;

                Companion.Log("");
                Companion.Log(message);
                foreach (Node node in SortedElements)
                {
                    Companion.Log(" - " + index.ToString() + " " + node.FullName);
                    index++;
                }
            }

            int CurDeep = 1;
            
            foreach (Node childNode in SortedElements)
            {
                childNode.RealDeepness = CurDeep;
                CurDeep++;
            }

            if (Companion.DEBUG)
            {
                Tracer.Instance.EndCall("SortNodes");
            }

            if (inLog)
            {
                List<Node> allNodes = GetChildren(false);

                allNodes.Sort();

                foreach (Node node in allNodes)
                {
                    Companion.Log(string.Format("{0,-5}: {1}", node.Depth, node.FullName));
                }
            }

            return sorted;
        }

        private int InsertCompound(Node childNode, int CurDeep, ref Dictionary<Compound, List<Node>> managedCompounds)
        {
            if (childNode.Parent != this)
            {
                //Insert compound if it's the last node inside
                if (!managedCompounds.ContainsKey(childNode.Parent))
                {
                    managedCompounds.Add(childNode.Parent, new List<Node>(childNode.Parent.Nodes));
                }

                if (managedCompounds[childNode.Parent].Contains(childNode))
                {
                    managedCompounds[childNode.Parent].Remove(childNode);
                }

                if (managedCompounds[childNode.Parent].Count == 0)
                {
                    childNode.Parent.RealDeepness = CurDeep;
                    CurDeep++;

                    CurDeep = InsertCompound(childNode.Parent, CurDeep, ref managedCompounds);
                }
            }

            return CurDeep;
        }

        #endregion

        #region Ports

        /// <summary>
        /// Get the real node port by index
        /// @todo Rewrite !!
        /// </summary>
        /// <param name="portIndex">Index of the port</param>
        /// <param name="isOutput">True if searched port is an output</param>
        /// <returns></returns>
        public override Port GetPort(int portIndex, bool isOutput)
        {
            Port port = isOutput ? Outputs[portIndex] : Inputs[portIndex];

            while(port != null && port is PortInstance)
            {
                port = (port as PortInstance).Reference;
            }

            return port;
        }

        private Dictionary<Port, PortInstance> mGetPortFromNodeCache = new Dictionary<Port, PortInstance>();
        /// <summary>
        /// Get the PortInstance from the actual node port
        /// @todo Rewrite !!
        /// </summary>
        /// <param name="nodePort">Actual port</param>
        /// <returns>The PortInstance</returns>
        public PortInstance GetPortFromNode(Port nodePort)
        {
            PortInstance ret;
            if (mGetPortFromNodeCache.TryGetValue(nodePort, out ret))
            {
                return ret;
            }
            else
            {
                List<Port> ports = nodePort.IsOutput ? Outputs : Inputs;
                foreach (Port port in ports)
                {
                    if (port.RealPort.UniqueName == nodePort.RealPort.UniqueName)
                    {
                        ret = port as PortInstance;
                        mGetPortFromNodeCache.Add(nodePort, ret);
                        return ret;
                    }
                }
            }

            return null;
        }

        public void DestroyPortsCache()
        {
            mGetPortFromNodeCache.Clear();
        }

        const string inPrefix = "IN_";
        const string outPrefix = "OUT_";

        /// <summary>
        /// Recreates completely the port Instances if needed
        /// @todo Rewrite !!
        /// </summary>
        public void RefreshPorts()
        {
            Dictionary<string, Port> portsDic = new Dictionary<string, Port>();

            foreach (Port port in Inputs)
            {
                StringBuilder sbuilder = new StringBuilder();
                //portsDic["IN_" + port.RealPort.UniqueName] = port;
                string key = inPrefix + port.RealPort.UniqueName;
                if (!portsDic.ContainsKey(key))
                {
                    portsDic.Add(key, port);
                }
            }

            foreach (Port port in Outputs)
            {
                //portsDic["OUT_" + port.RealPort.UniqueName] = port;
                string key = outPrefix + port.RealPort.UniqueName;
                if (!portsDic.ContainsKey(key))
                {
                    portsDic.Add(key, port);
                }
            }

            mInputs.Clear();
            mOutputs.Clear();

            foreach (Node rig in Nodes)
            {
                // Fill the Inputs / Outputs instances for the compound

                foreach (Port port in rig.Inputs)
                {
                    Port res;
                    if (portsDic.TryGetValue(inPrefix + port.RealPort.UniqueName,out res))
                    {
                        if (res.Name != res.NativeName)
                        {
                            res.Name = res.NativeName;
                        }
                    }
                    else
                    {
                        res = new PortInstance(port, this, 0);
                        
                    }

                    mInputs.Add(res);
                }

                foreach (Port port in rig.Outputs)
                {
                    Port res;
                    if (portsDic.TryGetValue(outPrefix + port.RealPort.UniqueName, out res))
                    {
                        if (res.Name != res.NativeName)
                        {
                            res.Name = res.NativeName;
                        }
                    }
                    else
                    {
                        res = new PortInstance(port, this, 0);
                    }

                    mOutputs.Add(res);
                }
            }

            DestroyPortsCache();
            RefreshPortsIndices();
            if (Parent != null)
            {
                Parent.RefreshPorts();
            }
        }

        public void RefreshVisibilities()
        {
            foreach (Port port in Inputs)
            {
                if (!port.Visible && (port as PortInstance).IsLinked())
                {
                    port.Visible = true;
                }
            }

            foreach (Port port in Outputs)
            {
                if (!port.Visible && (port as PortInstance).IsLinked())
                {
                    port.Visible = true;
                }
            }
        }

        internal override void CollectOutputs(Dictionary<string, Port> portsDic)
        {
            foreach (Node rig in Nodes)
            {
                rig.CollectOutputs(portsDic);
            }
        }

        /// <summary>
        /// Get the visibility from the node and apply it to the PortInstance
        /// @todo rewrite
        /// </summary>
        /// <param name="port">Port from which to get the visibility</param>
        public void RefreshVisibility(Port port)
        {
            Port compPort = GetPortFromNode(port);
            if (compPort != null)
            {
                compPort.Visible = port.Visible;
                if (Parent != null)
                {
                    Parent.RefreshVisibility(compPort);
                }
            }
        }

        /// <summary>
        /// Rename ports of a Node with a search and replace, and calls the same on the father (Used by RenameElements in RigNode and RigCompound)
        /// @todo rewrite !!
        /// </summary>
        /// <param name="Node">Node from which to rename the ports</param>
        /// <param name="Name">search string</param>
        /// <param name="value">replace string</param>
        public void RenamePorts(Node Node, string Name, string value)
        {
            foreach (Port port in Inputs)
            {
                if (port.PortObj.Owner == Node)
                {
                    if (port.NativeName == port.Name)
                    {
                        port.Name = port.NativeName = port.NativeName.Replace(Name, value);
                    }
                    else
                    {
                        port.NativeName = port.NativeName.Replace(Name, value);
                    }
                }
            }

            foreach (Port port in Outputs)
            {
                if (port.PortObj.Owner == Node)
                {
                    if (port.NativeName == port.Name)
                    {
                        port.Name = port.NativeName = port.NativeName.Replace(Name, value);
                    }
                    else
                    {
                        port.NativeName = port.NativeName.Replace(Name, value);
                    }
                }
            }

            if (Parent != null)
            {
                Parent.RenamePorts(Node, Name, value);
            }
        }

        /// <summary>
        /// Synchronises a portInstance name with its reference
        /// @todo rewrite
        /// </summary>
        /// <param name="NodePort">Node port </param>
        /// <param name="value">New name</param>
        public void UpdatePortName(Port NodePort, string value)
        {
            Port compPort = GetPortFromNode(NodePort);
            if (compPort != null && compPort.Name == NodePort.NativeName)
            {
                compPort.Name = value;
            }
        }

        #endregion

        #region Connections

        /// <summary>
        /// Resolve the links of the nodes in this Compound
        /// </summary>
        /// <param name="Remove">If true, links that were not successfully resolved are removed</param>
        public void ResolveAll(bool Remove)
        {
            Dictionary<string, Port> outputPortsDic = new Dictionary<string, Port>();
            //Dictionary<string, Port> inputPortsDic = new Dictionary<string, Port>();
            foreach (Node rig in Nodes)
            {
                rig.CollectOutputs(outputPortsDic);
            }

            bool exists = false;
            // Dependences =========================================================
            List<Link> deps = InDependencies;
            foreach (Link link in deps)
            {
                if (link.UnResolved)
                {
                    exists = false;

                    //Look if the link is already resolved
                    foreach (Link inlink in link.Target.Dependencies)
                    {
                        if (!inlink.UnResolved && inlink.Target.UniqueName == link.Target.UniqueName && inlink.Source.UniqueName == link.Source.UniqueName)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        //Try to resolve
                        if (outputPortsDic.ContainsKey(link.Source.UniqueName))
                        {
                            Port sourcePort = outputPortsDic[link.Source.UniqueName];
                            link.Source = sourcePort;

                            if (!sourcePort.Dependencies.Contains(link))
                            {
                                sourcePort.Dependencies.Add(link);
                            }
                        }

                        //Still unresolved ? remove it
                        if (Remove && link.UnResolved)
                        {
                            link.Target.Dependencies.Remove(link);
                        }
                    }
                    else
                    {
                        link.Target.Dependencies.Remove(link);
                    }
                }
            }
        }

        public override void ReConnect(Link Dep, string Mode)
        {
            Dep.Target.PortObj.Owner.ReConnect(Dep, Mode);
        }

        public override void ReConnect()
        {
            //Sort SubNodes and ReConnect
            List<Node> subNodes = new List<Node>();

            foreach (Node node in Nodes)
            {
                subNodes.Add(node);
            }

            subNodes.Sort();

            foreach (Node node in subNodes)
            {
                node.ReConnect();
            }
        }

        /// <summary>
        /// Remove all links towards external node for a compound, every links for a nodes
        /// </summary>
        /// <param name="Node">Node (or Compound) from which to remove the links</param>
        public void UnConnect(Node Node, bool inputs, bool outputs)
        {
            if (Node is Compound)
            {
                //If this is a compound, remove all links towards external rigs
                Compound curComp = Node as Compound;

                List<Link> ToRemove;

                if (inputs)
                {
                    ToRemove = Node.InDependencies;

                    foreach (Link Dep in ToRemove)
                    {
                        if (!Dep.Source.Owner.IsIn(curComp))
                        {
                            Dep.Delete();
                        }
                    }
                }

                if (outputs)
                {
                    ToRemove = Node.OutDependencies;

                    foreach (Link Dep in ToRemove)
                    {
                        if (!Dep.Target.Owner.IsIn(curComp))
                        {
                            Dep.Delete();
                        }
                    }
                }
            }
            else //If this is a node, simply remove all links
            {
                List<Link> ToRemove = new List<Link>();
                if (inputs)
                {
                    ToRemove.AddRange(Node.InDependencies);
                }

                if (outputs)
                {
                    ToRemove.AddRange(Node.OutDependencies);
                }

                foreach (Link Dep in ToRemove)
                {
                    Dep.Delete();
                }
            }
        }

        public void UnConnectAll(Node Node)
        {
            UnConnect(Node, true, true);
        }

        public void UnConnectInputs(Node Node)
        {
            UnConnect(Node, true, false);
        }

        public void UnConnectOutputs(Node Node)
        {
            UnConnect(Node, false, true);
        }

        /// <summary>
        /// Disconnect every incoming Link on this compound
        /// </summary>
        public override void DisconnectAll()
        {
            UnConnectAll(this);
        }

        #endregion

        #region IXmlSerializable Members
        /// <summary>
        /// Get Xml schema (not used)
        /// </summary>
        /// <returns>The Schema</returns>
        public new XmlSchema GetSchema()
        {
            return null;
        }
        /// <summary>
        /// Initializes its members from the Xml reader 
        /// </summary>
        /// <param name="reader">The reader streaming the xml</param>
        public new void ReadXml(XmlReader reader)
        {
            int Counter = 0;

            _name = reader.ReadElementString();
            _nativeName = reader.ReadElementString();

            mUIx = TypesHelper.FloatParse(reader.ReadElementString());
            mUIy = TypesHelper.FloatParse(reader.ReadElementString());

            mDisplayState = (NodeState)Enum.Parse(typeof(NodeState), reader.ReadElementString());

            mUser = reader.ReadElementString();
            mExportDate = long.Parse(reader.ReadElementString());
            mPath = reader.ReadElementString();
            mCheckedOut = bool.Parse(reader.ReadElementString());

            mVersion = int.Parse(reader.ReadElementString());
            mFreezed = bool.Parse(reader.ReadElementString());

            mDescription = reader.ReadElementString();
            mTags = reader.ReadElementString();
            mCategory = reader.ReadElementString();

            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Nodes")
            {
                reader.Read(); // Skip ahead to next node
                string type = "Default";

                while (reader.MoveToContent() == XmlNodeType.Element && (reader.LocalName == "Node" || reader.LocalName == "Compound"))
                {
                    Node Rig = null;
                    if (reader.HasAttributes)
                    {
                        type = reader.GetAttribute("Type");
                    }

                    XmlSerializer serializer = null;

                    if (reader.LocalName == "Node")
                    {
                        if (Serializer.NodeSerializers.TryGetValue(type, out serializer))
                        {
                            Rig = (Node)serializer.Deserialize(reader);
                        }
                        else
                        {
                            Rig = (Node)Serializer.NodeSerializers["Default"].Deserialize(reader);
                        }
                    }
                    else
                    {
                        if (Serializer.CompoundSerializers.TryGetValue(type, out serializer))
                        {
                            Rig = (Compound)serializer.Deserialize(reader);
                        }
                        else
                        {
                            throw new Exception("Deserializer " + type + " not found !");
                        }
                    }

                    if (Rig != null)
                    {
                        Rig.Parent = this;
                        _nodes.Add(Rig);
                    }

                    // Fill the Inputs / Outputs instances for the compound
                    RefreshPorts();
                }

                if (reader.LocalName == "Nodes")
                {
                    reader.ReadEndElement();
                }
            }
            else
            {
                throw new Exception(string.Format("'Nodes' Xml element not found !! (line : {0})", (reader as XmlTextReader).LineNumber));
            }

            Counter = 0;

            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Inputs")
            {
                //build a port instance dictionary to filter those who are really serialized here...
                Dictionary<string, Port> inputsDic = new Dictionary<string, Port>();
                foreach (Port input in Inputs)
                {
                    try
                    {
                        inputsDic.Add(input.NativeName, input);
                    }
                    catch (Exception e)
                    {
                        Exception niceException = new Exception(string.Format("Input '{0}' already added in the Compound '{1}' (from '{2}'), cannot add the new input from '{3}'", input.NativeName, FullName, inputsDic[input.NativeName].RealPort.Owner.FullName, input.RealPort.Owner.FullName), e);
                        throw niceException;
                    }
                }

                reader.Read(); // Skip ahead to next node
                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "PortInstance")
                {
                    PortInstance rigPort = (PortInstance)Serializer.PortInstanceSerializer.Deserialize(reader);
                    
                    // Set the values on the previoulsy created port Instances
                    if (inputsDic.ContainsKey(rigPort.NativeName))
                    {
                        PortInstance portInstance = inputsDic[rigPort.NativeName] as PortInstance;
                        portInstance._visible = rigPort.Visible;
                        portInstance.Name = rigPort.Name;
                    }

                    reader.Read();
                }

                if (reader.LocalName == "Inputs")
                {
                    reader.ReadEndElement();
                }
            }
            else
            {
                throw new Exception(string.Format("'Inputs' Xml element not found !! (line : {0})", (reader as XmlTextReader).LineNumber));
            }

            Counter = 0;

            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Outputs")
            {
                //build a port instance dictionary to filter those who are really serialized here...
                Dictionary<string, Port> outputsDic = new Dictionary<string, Port>();
                foreach (Port output in Outputs)
                {
                    try
                    {
                        outputsDic.Add(output.NativeName, output);
                    }
                    catch (Exception e)
                    {
                        Exception niceException = new Exception(string.Format("Output '{0}' already added in the Compound '{1}' (from '{2}'), cannot add the new output from '{3}'", output.NativeName, FullName, outputsDic[output.NativeName].RealPort.Owner.FullName, output.RealPort.Owner.FullName), e);
                        throw niceException;
                    }
                }

                reader.Read(); // Skip ahead to next node
                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "PortInstance")
                {
                    PortInstance rigPort = (PortInstance)Serializer.PortInstanceSerializer.Deserialize(reader);

                    // Set the values on the previoulsy created port Instances
                    if (outputsDic.ContainsKey(rigPort.NativeName))
                    {
                        PortInstance portInstance = outputsDic[rigPort.NativeName] as PortInstance;
                        portInstance._visible = rigPort.Visible;
                        portInstance.Name = rigPort.Name;
                    }

                    reader.Read();
                    Counter++;
                }

                if (reader.LocalName == "Outputs")
                {
                    reader.ReadEndElement();
                }
            }
            else
            {
                throw new Exception(string.Format("'Outputs' Xml element not found !! (line : {0})", (reader as XmlTextReader).LineNumber));
            }

            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "CustomFields")
            {
                reader.Read(); // Skip ahead to next node
                while (reader.MoveToContent() == XmlNodeType.Element)
                {
                    DeserializeCustomField(reader, reader.LocalName);
                }

                if (reader.LocalName == "CustomFields")
                {
                    reader.ReadEndElement();
                }
            }

            reader.Read();
            RefreshPorts();
            // RefreshPortsIndices(); //AlreadyDone at the end of RefreshPorts
        }
        /// <summary>
        /// Saves its members to the Xml writer 
        /// </summary>
        /// <param name="reader">The writer streaming the xml</param>
        public new void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Type", NodeElementType);

            writer.WriteElementString("Name", _name);
            writer.WriteElementString("NativeName", _nativeName);

            writer.WriteElementString("UIx", mUIx.ToString());
            writer.WriteElementString("UIy", mUIy.ToString());

            writer.WriteElementString("DisplayState", mDisplayState.ToString());

            writer.WriteElementString("User", mUser);
            writer.WriteElementString("ExportDate", mExportDate.ToString());
            writer.WriteElementString("Path", mPath);
            writer.WriteElementString("CheckedOut", mCheckedOut.ToString());

            writer.WriteElementString("Version", mVersion.ToString());
            writer.WriteElementString("Freezed", mFreezed.ToString());

            writer.WriteElementString("Description", mDescription);
            writer.WriteElementString("Tags", mTags);
            writer.WriteElementString("Category", mCategory);

            writer.WriteStartElement("Nodes");

            foreach (Node rig in _nodes)
            {
                if (rig is Compound)
                {
                    Serializer.CompoundSerializers[rig.NodeElementType].Serialize(writer, rig);
                }
                else
                {
                    Serializer.NodeSerializers[rig.NodeElementType].Serialize(writer, rig);
                }
            }

            writer.WriteEndElement();

            writer.WriteStartElement("Inputs");
            foreach (PortInstance port in Inputs)
            {
                Serializer.PortInstanceSerializer.Serialize(writer, port);
            }
            writer.WriteEndElement();


            writer.WriteStartElement("Outputs");
            foreach (PortInstance port in Outputs)
            {
                Serializer.PortInstanceSerializer.Serialize(writer, port);
            }
            writer.WriteEndElement();

            List<CustomField> Fields = GetCustomFields();

            if (Fields.Count > 0)
            {
                writer.WriteStartElement("CustomFields");
                foreach (CustomField field in Fields)
                {
                    field.Serialize(writer);
                }
                writer.WriteEndElement();
            }
        }

        #endregion

        #endregion


        public List<string> CheckCycles()
        {
            List<string> cycle = new List<string>();

            List<Node> nodes = GetChildren(true);
            nodes.Sort();

            if (nodes.Count == 0)
            {
                return cycle;
            }

            List<string> securedNodes = new List<string>();

            //Freenodes ← Set of all nodes with no incoming edges
            List<Node> Freenodes = new List<Node>();

            //search freenodes
            foreach (Node node in nodes)
            {
                if (node.InInterestingDependencies.Count == 0)
                {
                    Freenodes.Add(node);
                    securedNodes.Add(node.FullName);
                }
            }

            if (Freenodes.Count == 0)
            {
                //If we have no "free" nodes we should have a problem anyway...let's take the first one
                Freenodes.Add(nodes[0]);
            }

            List<string> parsedNodes = new List<string>();

            foreach (Node node in Freenodes)
            {
                cycle = WalkGraph(node, parsedNodes, securedNodes);

                if (cycle.Count > 0)
                {
                     break;
                }
            }

            return cycle;
        }

        private List<string> WalkGraph(Node node, List<string> parsedNodes, List<string> securedNodes)
        {
            List<string> cycle = new List<string>();

            if (parsedNodes.Contains(node.FullName))
            {
                int index = parsedNodes.IndexOf(node.FullName);
                List<string> cycling = new List<string>();

                for (int i = index; i < parsedNodes.Count; i++)
                {
                    cycling.Add(parsedNodes[i]);
                }

                return cycling;
            }
            else
            {
                parsedNodes.Add(node.FullName);
            }

            List<Link> outs = node.OutInterestingDependencies;
            List<Node> dependentNodes = new List<Node>();
            List<List<string>> parsedNodesPaths = new List<List<string>>();

            //Leaf
            if (outs.Count == 0)
            {
                foreach (string parsedName in parsedNodes)
                {
                    if (!securedNodes.Contains(parsedName))
                    {
                        securedNodes.Add(parsedName);
                    }
                }

                return cycle;
            }

            foreach (Link link in outs)
            {
                List<string> locallyParsedNodes = new List<string>();
                Node constrained = link.Target.Owner;

                if (!locallyParsedNodes.Contains(constrained.FullName))
                {
                    locallyParsedNodes.Add(constrained.FullName);
                    dependentNodes.Add(constrained);
                    parsedNodesPaths.Add(new List<string>(parsedNodes));
                }
            }

            
            int counter = 0;

            foreach (Node dependentNode in dependentNodes)
            {
                if (securedNodes.Contains(dependentNode.FullName))
                {
                    continue;
                }

                cycle = WalkGraph(dependentNode, parsedNodesPaths[counter], securedNodes);

                if (cycle.Count > 0)
                {
                    return cycle;
                }

                counter++;
            }

            return cycle;
        }
    }
}
