using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.IO;
using System.Drawing.Design;
using TK.BaseLib.CustomData;
using System.Drawing;
using System.Security.Principal;
using TK.NodalEditor;
using TK.GraphComponents.CustomData;
using TK.BaseLib;
using TK.NodalEditor.NodesLayout;
using OrderedPropertyGrid;

namespace TK.NodalEditor
{
    /// <summary>
    /// Visibility mode of the node
    /// Normal it shows all visible ports
    /// Minimal it shows only connected ports
    /// Collapsed it is reduced to the name
    /// </summary>
    public enum NodeState
    {
        ///shows all visible ports
        Normal,
        /// shows only connected ports
        Minimal,
        ///The node is reduced to the name
        Collapsed
    }

    /// <summary>
    /// The main class describing a Node, it's inherited to give the Compound. It's meant to be inherited and give special behaviors to Node for specific usages (see "Virtuals to give node behaviours" region).
    /// An effort was made to avoid re-writing the Copy and Serialization methods, through the implementation of virtuals managing the custom fields (
    /// </summary>
    [XmlRoot("Node")]
    public class Node : NodeBase, IXmlSerializable, IComparable
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Empty constructor. Should not be used except for testing.
        /// </summary>
        public Node()
        {

        }

        /// <summary>
        /// Cloning constructor from a reference Node
        /// </summary>
        /// <param name="inRef">Reference node</param>
        /// <param name="resolve">If true, the multiple references towards classes are unified</param>
        public Node(Node inRef, bool resolve)
        {
            this.Copy(inRef, resolve);
        }
        
        #endregion

        #region MEMBERS

        // --------------- UI Specific ---------------

        /// <summary>
        /// x - coordinate of the node when displayed in the UI
        /// </summary>
        protected float mUIx = 0;

        /// <summary>
        /// y - coordinate of the node when displayed in the UI
        /// </summary>
        protected float mUIy = 0;

        /// <summary>
        /// The ManagerCompanion of the current manager
        /// @todo weird we have to keep it here
        /// </summary>
        public ManagerCompanion Companion;

        /// <summary>
        /// With of the node when displayed in the UI (not serialized)
        /// </summary>
        public int UIWidth;

        /// <summary>
        /// With of the node when displayed in the UI (not serialized)
        /// </summary>
        public int UIHeight;

        /// <summary>
        /// X - coordinate of the label
        /// </summary>
        public int UILabelX;

        /// <summary>
        /// y - coordinate of the label
        /// </summary>
        public int UILabelY;

        /// <summary>
        /// Display mode from 3 types : Normal, Minimal, Collapsed
        /// </summary>
        protected NodeState mDisplayState = NodeState.Normal;


        /// <summary>
        /// number of Displayed Inputs
        /// </summary>
        public int InputsCount = 0;

        /// <summary>
        /// number of Displayed Outputs
        /// </summary>
        public int OutputsCount = 0;

        bool deleted = false;
        [BrowsableAttribute(false)]
        public bool Deleted
        {
            get { return deleted; }
            set { deleted = value; }
        }

        // --------------- MetaData ---------------
        /// <summary>
        /// List of contracts the node can fulfill
        /// </summary>
        protected List<PortContract> mPortContracts = new List<PortContract>();

        // --------------- Node Data ---------------

        /// <summary>
        /// Compound containing this node
        /// </summary>
        protected Compound mParent;

        /// <summary>
        /// Objects that stands for the port target
        /// </summary>
        private List<PortObj> mElements = new List<PortObj>();

        /// <summary>
        /// Input ports
        /// </summary>
        protected List<Port> mInputs = new List<Port>();

        /// <summary>
        /// Output ports
        /// </summary>
        protected List<Port> mOutputs = new List<Port>();

        /// <summary>
        /// Indicates if values are set from the Parent Compound
        /// </summary>
        public bool isCompoundGenerated = false;

        // --------------- Versioning ---------------

        /// <summary>
        /// The last user that saved the node
        /// </summary>
        protected string mUser = string.Empty;

        /// <summary>
        /// The ExportDate as ticks
        /// </summary>
        protected long mExportDate = 0;

        /// <summary>
        /// Path from which the node could be refreshed
        /// </summary>
        protected string mPath = "";

        /// <summary>
        /// Indicates if somebody locked the node for modification
        /// </summary>
        protected bool mCheckedOut = false;

        /// <summary>
        /// Version of the node, to determine if we need to Update from node pool
        /// </summary>
        protected int mVersion = 0;

        /*
        /// <summary>
        /// Major Version of the node, corresponding to a release of the node/compound
        /// </summary>
        protected int mMajorVersion = 0;
        */

        /// <summary>
        /// If set to true, the node will never be updated from node pool
        /// </summary>
        protected bool mFreezed = false;

        /// <summary>
        /// Description of the Node
        /// </summary>
        protected string mDescription = string.Empty;

        /// <summary>
        /// Tags of the Node
        /// </summary>
        protected string mTags = string.Empty;

        /// <summary>
        /// Category of the Node
        /// </summary>
        protected string mCategory = string.Empty;

        /// <summary>
        /// If True we can add ports on this Node
        /// </summary>
        protected bool mAllowAddPorts = false;

        /// <summary>
        /// If True we can add inputs on this Node
        /// </summary>
        protected bool mDynamicInputs = true;

        /// <summary>
        /// If True we can add outputs on this Node
        /// </summary>
        protected bool mDynamicOutputs = true;

        /// <summary>
        /// Stored deepness in the graph
        /// </summary>
        [BrowsableAttribute(true)]
        public int RealDeepness = 0;

        public int Depth
        {
            get { return RealDeepness; }
        }

        /// <summary>
        /// List of constraining nodes used for sorting via Compound.SortNodes
        /// </summary>
        [BrowsableAttribute(false)]
        public List<Node> TempInDependencies = new List<Node>();

        /// <summary>
        /// List of constrained nodes used for sorting via Compound.SortNodes
        /// </summary>
        [BrowsableAttribute(false)]
        public List<Node> TempOutDependencies = new List<Node>();

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Tells if the node is valid (to override)
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool IsValid
        {
            get { return true; }
        }

        /// <summary>
        /// x - coordinate of the node when displayed in the UI
        /// </summary>
        [BrowsableAttribute(false)]
        public float UIx
        {
            get { return mUIx; }
            set { mUIx = value; }
        }

        /// <summary>
        /// y - coordinate of the node when displayed in the UI
        /// </summary>
        [BrowsableAttribute(false)]
        public float UIy
        {
            get { return mUIy; }
            set { mUIy = value; }
        }

        /// <summary>
        /// Display mode from 3 types : Normal, Minimal, Collapsed
        /// </summary>
        [BrowsableAttribute(false)]
        public NodeState DisplayState
        {
            get { return mDisplayState; }
            set { mDisplayState = value; RefreshPortsIndices(); }
        }

        /// <summary>
        /// List of contracts the node can fulfill
        /// </summary>
        [BrowsableAttribute(false)]
        public List<PortContract> PortContracts
        {
            get
            {
                return mPortContracts;
            }
            set { mPortContracts = value; }
        }

        /// <summary>
        /// The last user that saved the node
        /// </summary>
        [CategoryAttribute("Versioning")]
        [ReadOnlyAttribute(true)]
        [DescriptionAttribute("The last user that saved the node")]
        public string User
        {
            get { return mUser; }
            set { mUser = value; }
        }

        /// <summary>
        /// The ExportDate as ticks
        /// </summary>
        [BrowsableAttribute(false)]
        public long ExportDate
        {
            get { return mExportDate; }
            set { mExportDate = value; }
        }

        /// <summary>
        /// Tells if the node is up to date regardring its extenal file
        /// </summary>
        [CategoryAttribute("Versioning")]
        public virtual bool UpToDate
        {
            get { return true; }
        }

        /// <summary>
        /// Date and time at which the node was saved
        /// </summary>
        [CategoryAttribute("Versioning")]
        [DescriptionAttribute("Date and time at which the node was saved")]
        public string ModificationDate
        {
            get
            {
                DateTime time = new DateTime(mExportDate);
                return time.ToShortDateString() + " " + time.ToShortTimeString();
            }
        }

        /// <summary>
        /// Path from which the node could be refreshed
        /// </summary>
        [CategoryAttribute("Versioning")]
        [DescriptionAttribute("Path from which the node could be refreshed")]
        public string Path
        {
            get { return mPath; }
            set { mPath = value; }
        }

        /// <summary>
        /// Real name of the element, in case we use external modifiers
        /// </summary>
        [BrowsableAttribute(false)]
        public override string FullName
        {
            get { return Name; }
            set
            {
                UpdateName(value);
                _name = value;

                if (Companion != null)
                {
                    Companion.Manager.OnNodesChanged(new NodesChangedEventArgs(Operations.NodeRenamed, new List<Node> { this }));
                }
            }
        }

        /// <summary>
        /// Indicates if somebody locked the node for modification
        /// </summary>
        [CategoryAttribute("Versioning")]
        [ReadOnlyAttribute(true)]
        [DescriptionAttribute("Indicates if somebody locked the node for modification")]
        public bool CheckedOut
        {
            get { return mCheckedOut; }
            set { mCheckedOut = value; }
        }

        /// <summary>
        /// Version of the node, to determine if we need to Update from node pool
        /// </summary>
        [CategoryAttribute("Versioning")]
        [ReadOnlyAttribute(true)]
        [DescriptionAttribute("Version of the node, to determine if we need to Update from node pool")]
        public int Version
        {
            get { return mVersion; }
            set { mVersion = value; }
        }

        /// <summary>
        /// If set to true, the node will never be updated from node pool
        /// </summary>
        [CategoryAttribute("Versioning")]
        [DescriptionAttribute("If set to true, the node will never be updated from node pool")]
        public bool Freezed
        {
            get { return mFreezed; }
            set { mFreezed = value; }
        }


        /// <summary>
        /// Description of the Node
        /// </summary>
        [BrowsableAttribute(false)]
        public string Description
        {
            get { return mDescription; }
            set { mDescription = value; }
        }

        /// <summary>
        /// Tags of the Node
        /// </summary>
        [BrowsableAttribute(false)]
        public string Tags
        {
            get { return mTags; }
            set { mTags = value; }
        }

        /// <summary>
        /// Category of the Node
        /// </summary>
        [BrowsableAttribute(false)]
        public string Category
        {
            get { return mCategory; }
            set { mCategory = value; }
        }

        /// <summary>
        /// If True we can add ports on this Node
        /// </summary>
        [BrowsableAttribute(false)]
        public bool AllowAddPorts
        {
            get { return mAllowAddPorts; }
            set { mAllowAddPorts = value; }
        }


        /// <summary>
        /// If True we can add inputs on this Node
        /// </summary>
        [BrowsableAttribute(false)]
        public bool DynamicInputs
        {
            get { return mDynamicInputs; }
            set { mDynamicInputs = value; }
        }

        /// <summary>
        /// If True we can add outputs on this Node
        /// </summary>
        [BrowsableAttribute(false)]
        public bool DynamicOutputs
        {
            get { return mDynamicOutputs; }
            set { mDynamicOutputs = value; }
        }

        /// <summary>
        /// Compound containing this node
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual Compound Parent
        {
            get { return mParent; }
            set { mParent = value; }
        }

        /// <summary>
        /// Objects that stands for the port target
        /// </summary>
        [CategoryAttribute("Port Objects")]
        [DescriptionAttribute("Objects that stands for the port target")]
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<PortObj> Elements
        {
            get { return mElements; }
            set { mElements = value; }
        }

        /// <summary>
        /// Input ports
        /// </summary>
        [CategoryAttribute("Ports")]
        [DescriptionAttribute("Input ports")]
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public virtual List<Port> Inputs
        {
            get { return mInputs; }
            set { mInputs = value; }
        }

        /// <summary>
        /// Output ports
        /// </summary>
        [CategoryAttribute("Ports")]
        [DescriptionAttribute("Output ports")]
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public virtual List<Port> Outputs
        {
            get { return mOutputs; }
            set { mOutputs = value; }
        }

        /// <summary>
        /// Gets the links "constraining" this Node
        /// </summary>
        [BrowsableAttribute(false)]
        public List<Link> InDependencies
        {
            get
            {
                List<Link> deps = new List<Link>();

                foreach (Port port in Inputs)
                {
                    foreach (Link dep in port.RealPort.Dependencies)
                    {
                        deps.Add(dep);
                    }
                }

                return deps;
            }
        }

        /// <summary>
        /// Gets the links "constrained" to Node
        /// </summary>
        [BrowsableAttribute(false)]
        public List<Link> OutDependencies
        {
            get
            {
                List<Link> deps = new List<Link>();

                foreach (Port port in Outputs)
                {
                    foreach (Link dep in port.RealPort.Dependencies)
                    {
                        deps.Add(dep);
                    }
                }

                return deps;
            }
        }

        /// <summary>
        /// Deepness in the graph (calculated if needed)
        /// </summary>
        [BrowsableAttribute(true)]
        public int Deepness
        {
            get
            {
                int deep = 0;
                Compound parent = Parent;
                while (parent != null)
                {
                    parent = parent.Parent;
                    deep++;
                }

                return deep;
            }
        }

        /// <summary>
        /// Search the graph up to the root
        /// </summary>
        [BrowsableAttribute(false)]
        public Compound Root
        {
            get
            {
                Node CurNode = this;
                Compound CurParent = Parent;

                while (CurParent != null)
                {
                    CurNode = CurParent;
                    CurParent = CurNode.Parent;
                }

                return CurNode as Compound;
            }
        }

        // --------------- virtuals for inherited classes ---------------

        /// <summary>
        /// Used in inherited classes to define states of the node
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual string States
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Used in inherited classes to define the type for Serialization / Deserialization
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual string NodeElementType
        {
            get
            {
                return "Default";
            }
        }

        /// <summary>
        /// Simply return a Type, used for sorting or filtering
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual string NodeType
        {
            get { return "Node"; }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Initialisation code called when a Node is created
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// Remove this node (used to delete data associated with the node)
        /// </summary>
        public virtual void Remove()
        {
            RemoveObject();
        }

        /// <summary>
        /// Copy the Node (Initialize its variables FROM the given Node)
        /// </summary>
        /// <param name="inNode">Node to copy into this Node instance</param>
        /// <param name="Resolve">If true, reassign PortObj objects from Elements and refreshes the ports indices</param>
        public virtual void Copy(Node inNode, bool Resolve)
        {
            Inputs = new List<Port>();
            Outputs = new List<Port>();

            _nativeName = inNode.NativeName;
            _name = inNode.Name;
            mDescription = inNode.Description;
            mCategory = inNode.Category;
            mTags = inNode.Tags;

            mUIx = inNode.UIx;
            mUIy = inNode.UIy;
            mPath = inNode.Path;
            mUser = inNode.User;
            mExportDate = inNode.ExportDate;
            mCheckedOut = inNode.CheckedOut;
            mDisplayState = inNode.DisplayState;
            mFreezed = inNode.Freezed;
            mVersion = inNode.Version;

            RealDeepness = inNode.RealDeepness;

            mAllowAddPorts = inNode.AllowAddPorts;
            mDynamicInputs = inNode.DynamicInputs;
            mDynamicOutputs = inNode.DynamicOutputs;

            mElements.Clear();

            foreach (PortObj element in inNode.Elements)
            {
                PortObj newElement = (PortObj)Activator.CreateInstance(element.GetType(), new object[0]);
                newElement.Copy(element);
                newElement.Owner = this;
                mElements.Add(newElement);
            }

            mInputs.Clear();

            foreach (Port port in inNode.Inputs)
            {
                Port newPort = new Port(port);
                newPort.Owner = this;
                mInputs.Add(newPort);
            }

            mOutputs.Clear();

            foreach (Port port in inNode.Outputs)
            {
                Port newPort = new Port(port);
                newPort.Owner = this;
                mOutputs.Add(newPort);
            }

            CopyCustomFields(inNode, Resolve);

            if (Resolve)
            {
                ResolveObjets();
                RefreshPortsIndices();
            }
        }

        /// <summary>
        /// Called before copy to give opportunity to clean or update data (Reading from rig or guide for OSCAR)
        /// </summary>
        public virtual void UpdateBeforeCopy()
        {

        }

        protected override void UpdateName(string value)
        {
            if (_name != null && FullName != value)
            {
                RenameElements(FullName, value);
            }
        }

        public virtual void RenameElements(string Name, string value)
        {
            foreach (PortObj element in Elements)
            {
                element.Name = element.Name.Replace(Name + "_", value + "_");
            }

            foreach (Port port in Inputs)
            {
                //if the name is not overwritten, change it
                if (port.Name == port.NativeName)
                {
                    port.Name = port.NativeName = port.PortObj.Name;
                }
                else //if overwritten, change AccessName only
                {
                    port.NativeName = port.PortObj.Name;
                }
            }

            foreach (Port port in Outputs)
            {
                string oldName = port.Name;

                //if the name is not overwritten, change it
                if (oldName == port.NativeName)
                {
                    port.Name = port.NativeName = port.PortObj.Name;
                }
                else //if overwritten, change AccessName only
                {
                    port.NativeName = port.PortObj.Name;
                }
            }

            if (Parent != null)
            {
                Parent.RenamePorts(this, Name, value);
                Parent.RefreshPorts();
            }
        }

        #endregion        

        /// <summary>
        /// Update a node, mapping the ports
        /// </summary>
        /// <param name="RefNode">Reference Node</param>
        /// <param name="All">Apdates all if true</param>
        /// <returns></returns>
        public PortModifications Update(Node RefNode, bool All)
        {
            PortModifications mods = new PortModifications();
            List<PortObj> newElements = new List<PortObj>();

            foreach (PortObj obj in RefNode.Elements)
            {
                newElements.Add(obj);
            }

            List<PortObj> oldElements = new List<PortObj>();

            //Parse old list to Update found elements
            foreach (PortObj obj in Elements)
            {
                //Find new corresponding obj
                PortObj otherObj = FindPortObj(obj, newElements);

                if (otherObj == null)
                {
                    //not found, delete it
                    oldElements.Add(obj);
                }
                else
                {                   
                    //found, update it
                    newElements.Remove(otherObj);
                    obj.Update(otherObj, All);
                }
            }

            //Remove unused elements
            foreach (PortObj obj in oldElements)
            {
                if (obj.Default)
                {
                    foreach (Port input in Inputs)
                    {
                        if (input.PortObj == obj)
                        {
                            Inputs.Remove(input);
                            mods.OldPorts.Add(input);
                            break;
                        }
                    }

                    foreach (Port output in Outputs)
                    {
                        if (output.PortObj == obj)
                        {
                            Outputs.Remove(output);
                            mods.OldPorts.Add(output);
                            break;
                        }
                    }

                    Elements.Remove(obj);
                }
            }

            //Add new Elements
            foreach (PortObj obj in newElements)
            {
                PortObj newObj = (PortObj)Activator.CreateInstance(obj.GetType());
                newObj.Copy(obj);
                newObj.Name = newObj.Name.Replace(obj.Owner.FullName + "_", FullName + "_");
                List<Port> ports = NodesFactory.AddPortObj(this, newObj);
                
                Port refPort = RefNode.GetPort(obj, false);
                if (refPort != null)
                {
                    ports[0].Visible = refPort.Visible;
                }
                refPort = RefNode.GetPort(obj, true);
                if (refPort != null)
                {
                    ports[1].Visible = refPort.Visible;
                }

                mods.NewPorts.AddRange(ports);
                newObj.ResolveCustomObjects();
            }

            ResolveObjets();

            //Base properties
            if (All)
            {
                mAllowAddPorts = RefNode.AllowAddPorts;
                mDynamicInputs = RefNode.DynamicInputs;
                mDynamicOutputs = RefNode.DynamicOutputs;
                mCategory = RefNode.Category;
                mPortContracts = RefNode.PortContracts;
            }
            else
            {
                mVersion = RefNode.Version;
                mUser = RefNode.User;
                mExportDate = RefNode.ExportDate;
                mCheckedOut = RefNode.CheckedOut;
            }

            UpdateCustomFields(RefNode, All);

            mPath = RefNode.Path;

            return mods;
        }

        public virtual void UpdateCustomFields(Node RefNode, bool All)
        {
            
        }

        /// <summary>
        /// Reassign the PortObj objects of Ports from the Elements List
        /// </summary>
        public void ResolveObjets()
        {
            // Create an elements dictionary
            Dictionary<string, PortObj> objsDic = new Dictionary<string, PortObj>();
            foreach (PortObj element in Elements)
            {
                if (!objsDic.ContainsKey(element.Name))
                {
                    objsDic.Add(element.Name, element);
                }
            }

            // Resolve ports
            foreach (Port rigport in Inputs)
            {
                rigport.PortObj = objsDic[rigport.PortObj.Name];
            }

            foreach (Port rigport in Outputs)
            {
                rigport.PortObj = objsDic[rigport.PortObj.Name];
            }

            ResolveCustomObjects();
        }

        /// <summary>
        /// Get the basic info (Description, Tags, Category) from an xml serialization. This will work even if the serialization was changed
        /// </summary>
        /// <param name="FileName">Path of the xml serialization</param>
        /// <returns>The basic Info of the Node as [Description, Tags, Category]</returns>
        public static List<string> GetBaseInfo(string FileName)
        {
            List<string> infos = new List<string>();

            FileInfo rigFile = new FileInfo(FileName);
            if (rigFile.Exists)
            {
                XmlReader reader = null;
                try
                {
                    reader = XmlReader.Create(rigFile.FullName);

                    reader.Read();

                    reader.ReadToFollowing("Description");
                    if (reader.LocalName == "Description")
                    {
                        infos.Add(reader.ReadElementString("Description"));
                    }

                    reader.ReadToFollowing("Tags");
                    if (reader.LocalName == "Tags")
                    {
                        infos.Add(reader.ReadElementString("Tags"));
                    }

                    reader.ReadToFollowing("Category");
                    if (reader.LocalName == "Category")
                    {
                        infos.Add(reader.ReadElementString("Category"));
                    }
                }
                catch (Exception) { return null; }

                if (reader != null)
                {
                    reader.Close();
                }
            }

            return infos;
        }

        #region Hierarchy

        /// <summary>
        /// Tells if this instance is contained in the given Compound (directly or several levels away)
        /// </summary>
        /// <param name="RigCompound"></param>
        /// <returns></returns>
        public bool IsIn(Compound RigCompound)
        {
            Compound par = Parent;
            while ((par != RigCompound) && (par != null))
                par = par.Parent;
            return (par == RigCompound);
            /*
            if (Parent == RigCompound)
            {
                return true;
            }
            else if (Parent != null)
            {
                return Parent.IsIn(RigCompound);
            }
            return false;
             */ 
        }

        /// <summary>
        /// Get the list of dependant nodes (Branch)
        /// </summary>
        /// <param name="Recursive">If true, get the dependant nodes recursively</param>
        /// <returns>The list of dependant nodes</returns>
        public List<Node> GetDependentNodes(bool Recursive)
        {
            List<Node> Depend = new List<Node>();
            GetDependentNodes(this, Recursive, Depend, this.Parent);

            return Depend;
        }

        /// <summary>
        /// Get the list of dependant nodes (Branch)
        /// </summary>
        /// <param name="curNode">The node on which to find the dependancies</param>
        /// <param name="Recursive">If true, get the dependant nodes recursively</param>
        /// <param name="Depend">The list of dependant nodes</param>
        /// <param name="inParent">Original Parent for the search (cannot search outside)</param>
        private static void GetDependentNodes(Node curNode, bool Recursive, List<Node> Depend, Compound inParent)
        {
            foreach (Link link in curNode.OutDependencies)
            {
                Node target = link.Target.Owner;
                if (target.Parent == inParent)
                {
                    if (!Depend.Contains(target))
                    {
                        Depend.Add(target);
                        GetDependentNodes(target, Recursive, Depend, inParent);
                    }
                }
                else if (Recursive)
                {
                    Compound parent = target.Level(inParent) as Compound;
                    if (parent != null)
                    {
                        foreach (Node node in parent.Nodes)
                        {
                            if (!Depend.Contains(node))
                            {
                                Depend.Add(node);
                                GetDependentNodes(node, true, Depend, inParent);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return the node by which this instance is available on te given Compound (itself if it's contained in the given Compound or the Compound containing it)
        /// </summary>
        /// <param name="CurCompound">The compound from which to search</param>
        /// <returns>The node or Compound at the "CurCompound" level</returns>
        public Node Level(Compound CurCompound)
        {
            if (IsIn(CurCompound))
            {
                if (Parent == CurCompound)
                {
                    return this;
                }

                Node curNode = this;

                while (curNode.Parent != null)
                {
                    if (curNode.Parent == CurCompound)
                    {
                        return curNode;
                    }

                    curNode = curNode.Parent;
                }

                return null;
            }

            return null;
        }

        #endregion

        #region Connections

        /// <summary>
        /// Connects the Node
        /// </summary>
        /// <param name="InputIndex">Index of the Input port to link</param>
        /// <param name="OuputRig">Node that is "constraining" this instance</param>
        /// <param name="OutputIndex">Index of the output port</param>
        /// <param name="Mode">Define a specific link mode</param>
        /// <param name="Error">Error message</param>
        /// <returns>The new Link</returns>
        public Link Connect(int InputIndex, Node OuputRig, int OutputIndex, string Mode, out string Error)
        {
            return Connect(InputIndex, OuputRig, OutputIndex, Mode, out Error, Companion.Manager.Preferences.CheckCycles);
        }

        /// <summary>
        /// Connects the Node
        /// </summary>
        /// <param name="InputIndex">Index of the Input port to link</param>
        /// <param name="OuputRig">Node that is "constraining" this instance</param>
        /// <param name="OutputIndex">Index of the output port</param>
        /// <param name="Mode">Define a specific link mode</param>
        /// <param name="Error">Error message</param>
        /// <param name="CheckCycle">Check if the link creates a cycle</param>
        /// <returns>The new Link</returns>
        public Link Connect(int InputIndex, Node OuputRig, int OutputIndex, string Mode, out string Error, bool CheckCycle)
        {
            Error = "";

            if (Inputs.Count > InputIndex && OuputRig.Outputs.Count > OutputIndex)
            {
                Port port = GetRealInput(InputIndex);

                Link newDep = null;

                bool exists = false;

                //Check existence
                foreach (Link dep in port.Dependencies)
                {
                    //=>                Unicity problem
                    if (dep.Target.PortObj.Name == port.PortObj.Name && dep.Target.Index == port.Index && dep.Source.PortObj.Name == OuputRig.Outputs[OutputIndex].PortObj.Name && dep.Source.Index == OuputRig.Outputs[OutputIndex].Index)
                    {
                        newDep = dep;
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    Port cGport = OuputRig.GetRealOutput(OutputIndex);

                    //Ports type
                    if (cGport.NodeElementType == port.NodeElementType)
                    {
                        //Circular dependency
                        if (!CheckCycle || !cGport.Owner.Depend(port.Owner))
                        {
                            //Short-circuit dependency
                            if (cGport.PortObj != port.PortObj)
                            {
                                //Links number restrictions
                                if (cGport.IsLinked() && cGport.IsRestricted())
                                {
                                    Error = cGport.FullName + " is restricted to only one connection !!";
                                }
                                else
                                {
                                    //Everything is fine, create the link
                                    newDep = CreateLink(cGport.NodeElementType, cGport, port);
                                    newDep.Name = port.Owner.FullName + "_To_" + cGport.Owner.FullName;
                                    if (newDep != null)
                                    {
                                        if (!port.Visible)
                                        {
                                            port.Visible = true;
                                        }
                                        if (!cGport.Visible)
                                        {
                                            cGport.Visible = true;
                                        }
                                        port.Dependencies.Add(newDep);
                                        cGport.Dependencies.Add(newDep);
                                        RefreshPortsIndices();
                                        OuputRig.RefreshPortsIndices();
                                        Root.SortNodes();
                                        RefreshConnections();
                                    }
                                }
                            }
                            else
                            {
                                Error = cGport.FullName + " cannot be connected to itself";
                            }
                        }
                        else
                        {
                            Error = cGport.Owner.FullName + " already depends on " + port.Owner.FullName + " (circular dependency)";
                        }
                    }
                    else
                    {
                        Error = "Cannot connect port of type \"" + cGport.NodeElementType.ToString() + "\" with a port of type \"" + port.NodeElementType.ToString() + "\".";
                    }
                }
                else
                {
                    newDep.IsNew = false;
                }

                if (Mode != "MOCK")
                {
                    ReConnect(newDep, Mode);
                }

                return newDep;
            }
            else
            {
                Error = "The requested ports have not been found !";
            }

            return null;
        }
        
        /// <summary>
        /// Tells if the current instance "depend on" another Node
        /// </summary>
        /// <param name="Node">The Node to check</param>
        /// <returns>True if the given Node "constrain" this one</returns>
        public bool Depend(Node Node)
        {
            return Depend(Node, false);
        }

        public bool Depend(Node Node, bool strict)
        {
            foreach (Link dep in InDependencies)
            {
                //Constrained to itself
                if (dep.Source.Owner == this || (!strict && !dep.IsHierachicallyInteresting))
                {
                    continue;
                }

                if (dep.Source.Owner == Node || dep.Source.Owner.Depend(Node, strict))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Disconnect every incoming Link on this node
        /// </summary>
        public virtual void DisconnectAll()
        {
            foreach (Port port in Inputs)
            {
                port.Dependencies.Clear();
            }
        }

        #endregion
        
        #region Ports


        private bool mAutoRefreshPortsIndicesDisabled = false;
        public void EnableAutoRefreshPortsIndices()
        {
            mAutoRefreshPortsIndicesDisabled = false;
            RefreshPortsIndices();
        }
        public void DisableAutoRefreshPortsIndices()
        {
            mAutoRefreshPortsIndicesDisabled = true;
        }


        /// <summary>
        /// Refreshes the Port.Index and Port.DisplayIndex properties
        /// </summary>
        public void RefreshPortsIndices()
        {
            if (mAutoRefreshPortsIndicesDisabled)
                return;
            int counter = 0;
            int counterVis = 0;

            foreach (Port port in Inputs)
            {
                port.Index = counter;
                counter++;

                if (port.IsVisible)
                {
                    port.DisplayIndex = counterVis;
                    counterVis++;
                }
                else
                {
                    port.DisplayIndex = -1;
                }
            }

            InputsCount = counterVis;
            counter = 0;
            counterVis = 0;

            foreach (Port port in Outputs)
            {
                port.Index = counter;
                counter++;

                if (port.IsVisible)
                {
                    port.DisplayIndex = counterVis;
                    counterVis++;
                }
                else
                {
                    port.DisplayIndex = -1;
                }
            }

            OutputsCount = counterVis;
        }

        /// <summary>
        /// Get the input port at the given index, taking in account it can be a Compound
        /// @todo rewrite
        /// </summary>
        /// <param name="InputIndex">Index of the port</param>
        /// <returns>The input port at the given index</returns>
        Port GetRealInput(int InputIndex)
        {
            Port port = Inputs[InputIndex];

            while (port is PortInstance)
            {
                port = (port as PortInstance).Reference;
            }

            return port;
        }

        /// <summary>
        /// Get the Output port at the given index, taking in account it can be a Compound
        /// @todo rewrite
        /// </summary>
        /// <param name="OutputIndex">Index of the port</param>
        /// <returns>The Output port at the given index</returns>
        Port GetRealOutput(int OutputIndex)
        {
            Port port = Outputs[OutputIndex];

            while (port is PortInstance)
            {
                port = (port as PortInstance).Reference;
            }

            return port;
        }

        /// <summary>
        /// Get a port from its PortObj
        /// </summary>
        /// <param name="element">The PortObj contained in the port</param>
        /// <param name="IsOutput">If True search in Outputs</param>
        /// <returns>The found Port</returns>
        protected Port GetPort(PortObj element, bool IsOutput)
        {
            if (element.IsOutput && IsOutput || element.IsInput && !IsOutput)
            {
                List<Port> ports = IsOutput ? Outputs : Inputs;

                foreach (Port port in ports)
                {
                    if (port.PortObj == element)
                    {
                        return port;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a port from its Display Index
        /// </summary>
        /// <param name="DisplayIndex">DisplayIndex</param>
        /// <returns>The found Port</returns>
        public Port GetPort(int DisplayIndex)
        {
            List<Port> ports = null;

            if (DisplayIndex >= 1000)
            {
                DisplayIndex -= 1000;
                ports = Outputs;
            }
            else
            {
                ports = Inputs;
            }

            foreach (Port port in ports)
            {
                if (port.DisplayIndex == DisplayIndex)
                {
                    return port;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the port with the given name
        /// </summary>
        /// <param name="name">Name of the port</param>
        /// <param name="isOutput">If True search in Outputs</param>
        /// <returns>The port with the given name</returns>
        public Port GetPort(string name, bool IsOutput)
        {
            List<Port> ports = IsOutput ? Outputs : Inputs;

            foreach (Port port in ports)
            {
                if (port.Name == name)
                {
                    return port;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the port at the given index
        /// @todo rewrite
        /// </summary>
        /// <param name="portIndex">Index of the port</param>
        /// <param name="isOutput">If True search in Outputs</param>
        /// <returns>The port at the given index</returns>
        public virtual Port GetPort(int portIndex, bool isOutput)
        {
            return isOutput ? Outputs[portIndex] : Inputs[portIndex];
        }

        /// <summary>
        /// Get a PortObj (object exposed through a Port) from its FullName 
        /// </summary>
        /// <param name="inName">The FullName of the PortObj</param>
        /// <returns>The found PortObj</returns>
        public PortObj GetElement(string inName)
        {
            foreach (PortObj element in Elements)
            {
                if (element.FullName == inName)
                {
                    return element;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells if the given PortObj have an Input Link
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool IsConstrainedElement(PortObj element)
        {
            Port port = GetPort(element, false);
            return (port != null && port.Dependencies.Count > 0);
        }

        /// <summary>
        /// Collect all output ports in a dictionary, used by Compound.ResolveAll, to resolve Links
        /// </summary>
        /// <param name="portsDic"></param>
        internal virtual void CollectOutputs(Dictionary<string, Port> portsDic)
        {
            // Outputs ======
            for (int counter = 0; counter < Outputs.Count; counter++)
            {
                Port rigport = Outputs[counter];

                if (portsDic.ContainsKey(rigport.UniqueName))
                {
                    rigport = portsDic[rigport.UniqueName];
                }
                else
                {
                    portsDic.Add(rigport.UniqueName, rigport);
                }
            }
        }

        #endregion

        #region Virtuals to give node behaviours
        
        /// <summary>
        /// Called when the node is created, used in inherited classes to create specific objects
        /// </summary>
        /// <param name="inName">New name of the node</param>
        public virtual void Create(string inName)
        {
            Name = inName;
        }

        /// <summary>
        /// Called when the node is removed, used in inherited classes to removed specific objects
        /// </summary>
        protected virtual void RemoveObject()
		{

		}

        /// <summary>
        /// Called when a node needs reconnection (after creation, copy...)
        /// </summary>
        public virtual void ReConnect()
        {
        }

        /// <summary>
        /// Called to connect or reconnect a specific Link (used by the main Node.Connect method)
        /// </summary>
        /// <param name="dep">Link to reconnect</param>
        /// <param name="Mode">Specific mode of connection</param>
        public virtual void ReConnect(Link dep, string Mode)
        {

        }

        /// <summary>
        /// UnConnect a specific Link (calls UnConnectObject and remove the dependencies)
        /// </summary>
        /// <param name="Dep">Link to unconnect</param>
        public virtual void UnConnect(Link Dep)
        {
            if(Dep.Target.Dependencies.Contains(Dep))
            {
                Dep.Target.Owner.UnConnectObject(Dep);
                Dep.Target.Dependencies.Remove(Dep);
                Dep.Source.Dependencies.Remove(Dep);
                Dep.Target.Owner.RefreshConnections();
            }
        }

        public virtual void RefreshConnections()
        {
        }

        /// <summary>
        /// Called when a Link is removed, to help inherited classes to disconnect their specific objects
        /// </summary>
        /// <param name="Dep">Link that is removed</param>
        public virtual void UnConnectObject(Link Dep)
        {

        }

        /// <summary>
        /// Define a custom Color for this Node (Transparent mean no override)
        /// </summary>
        public virtual Color CustomColor
        {
            get { return Color.Transparent; }
            set { }
        }

        /// <summary>
        /// Create a Link with the given Category, this is how ingerited classes can create custom Links
        /// </summary>
        /// <param name="inCategory">Category of the Link</param>
        /// <param name="inSource">Source of the Link</param>
        /// <param name="inTarget">Target of the Link</param>
        /// <returns></returns>
        public virtual Link CreateLink(string inCategory, Port inSource, Port inTarget)
        {
            return new Link(inSource, inTarget);
        }

        /// <summary>
        /// Virtual used by inherited classes to copy their custom fields from the given Node instance.It's called by Node.Copy, after Copying members, just before Calling the Resolve method
        /// </summary>
        /// <param name="inNode">Node from which to copy the properties</param>
        /// <param name="Resolve">Indicates if we have to call the Resolve method that reassign the correct instances in properties</param>
        public virtual void CopyCustomFields(Node inNode, bool Resolve)
        {

        }

        /// <summary>
        /// Called by ResolveObjets, used by inherited classes to resolve (check and reconnect correct instances) custom classes 
        /// </summary>
        public virtual void ResolveCustomObjects()
        {

        }

        /// <summary>
        /// Returns the Ports types that can be created on this Node
        /// </summary>
        /// <returns>Ports types</returns>
        public virtual List<string> GetPortTypes()
        {
            List<string> types = new List<string>();

            types.Add("Default");

            return types;
        }

        #endregion

        /// <summary>
        /// Set the ports visibility, all inputs or all outputs
        /// </summary>
        /// <param name="isOutput">If true, acts on outputs</param>
        /// <param name="inVisible">Visibility value</param>
        internal void SetPortsVisibility(bool isOutput, bool inVisible)
        {
            List<Port> ports = isOutput ? Outputs : Inputs;

            foreach (Port port in ports)
            {
                if (inVisible || !port.Linked)
                {
                    port.Visible = inVisible;
                }
            }
        }

        /// <summary>
        /// Add a port (or several) with the given PortObj
        /// </summary>
        /// <param name="obj">PortObj exposed by the port</param>
        /// <returns>List of ports created (a PortObj can create an input and an output)</returns>
        public List<Port> AddPort(PortObj obj)
        {
            List<Port> ports = NodesFactory.AddPortObj(this, obj);

            foreach (Port port in ports)
            {
                port.IsDynamic = true;
            }

            RefreshPortsIndices();
            if (Parent != null)
            {
                Parent.RefreshPorts();
            }

            return ports;
        }

        /// <summary>
        /// Insert a port (or several) with the given PortObj
        /// </summary>
        /// <param name="obj">PortObj exposed by the port</param>
        /// <param name="index">Index to insert the new PortObj (though the port indices)</param>
        /// <returns>List of ports created (a PortObj can create an input and an output)</returns>
        public List<Port> AddPort(PortObj obj, int index)
        {
            List<Port> ports = NodesFactory.AddPortObj(this, obj, index);

            foreach (Port port in ports)
            {
                port.IsDynamic = true;
            }

            RefreshPortsIndices();
            if (Parent != null)
            {
                Parent.RefreshPorts();
            }

            return ports;
        }

        public virtual Port NewPort(string inName, string type, bool isOutput, object[] Params, List<string> TypeMetaData)
        {
            PortObj portObj = new PortObj();
            portObj.Default = false;
            portObj.NativeName = portObj.Name = GetPortObjUniqueName(inName);
            portObj.IsInput = !isOutput;
            portObj.IsOutput = isOutput;

            return AddPort(portObj)[(isOutput ? 1 : 0)];
        }

        protected string GetPortObjUniqueName(string p)
        {
            List<string> otherNames = new List<string>();
            foreach (PortObj obj in Elements)
            {
                otherNames.Add(obj.Name);
            }

            return NodesManager.SetUniqueName(p, otherNames);
        }

        public void RemovePort(Port inPort)
        {
            List<Port> ports = inPort.IsOutput ? Outputs : Inputs;
            List<Port> otherPorts = inPort.IsOutput ? Inputs : Outputs;
            Port otherPort = null;

            if (!inPort.Default && !inPort.Linked && ports.Contains(inPort))
            {
                //Check if we have to delete the other one
                foreach (Port port in otherPorts)
                {
                    if (port.PortObj == inPort.PortObj)
                    {
                        otherPort = port;
                        break;
                    }
                }

                if (otherPort == null || !otherPort.Linked)
                {
                    ports.Remove(inPort);

                    if (otherPort != null)
                    {
                        otherPorts.Remove(otherPort);
                    }

                    Elements.Remove(inPort.PortObj);
                    RemovePortObj(inPort.PortObj);

                    RefreshPortsIndices();
                    if (Parent != null)
                    {
                        Parent.RefreshPorts();
                    }
                }
            }
        }

        public virtual void RemovePortObj(PortObj portObj)
        {

        }

        public virtual List<string> GetPortParamNames(string type)
        {
            return new List<string>();
        }

        public virtual object[] GetPortParams(string type)
        {
            return new object[0];
        }

        public PortObj FindPortObj(PortObj inObj, List<PortObj> inObjs)
        {
            foreach (PortObj obj in inObjs)
            {
                if (obj.NativeName == inObj.NativeName)
                {
                    return obj;
                }
            }

            return null;
        }

        public void CreateVersion(string RigPath)
        {
            mVersion += 1;
            mExportDate = DateTime.Now.Ticks;
            mUser = WindowsIdentity.GetCurrent().Name.Split("\\".ToCharArray())[1];
            mPath = RigPath;
        }

        public virtual void PropertyChanged(string propertyName, object newValue)
        {

        }

        public virtual string Substitute(Node newNode)
        {
            string message = "*** Substitute " + FullName + " with " + newNode.Name + "\n";

            //Base properties
            newNode.CustomColor = CustomColor;
            newNode.DisplayState = DisplayState;
            
            Dictionary<string, PortObj> newObjs = new Dictionary<string,PortObj>();

            //Get relations
            foreach (PortObj obj in newNode.Elements)
            {
                newObjs.Add(obj.ShortName, obj);
            }

            PortObj newObj = null;

            foreach (PortObj obj in Elements)
            {
                newObj = null;

                if (newObjs.ContainsKey(obj.ShortName))
                {
                    if(obj.NodeElementType == newObjs[obj.ShortName].NodeElementType)
                    {
                        newObj = newObjs[obj.ShortName];
                    }
                }
                else
                {
                    if (!obj.Default)
                    {
                        //Add the port
                        newObj = newNode.AddDynamicPort(obj);

                        if (newObj != null)
                        {
                            message += " Dynamic port added successfully (" + newObj.Name + ")\n";
                        }
                        else
                        {
                            message += " WARNING : Can't add dynamic port : " + obj.Name + "!\n";
                        }
                    }
                }

                if (newObj != null)
                {
                    //Get a match !

                    //Substitute PortObject
                    message += obj.Substitute(newObj);

                    //Substitute links
                    List<Port> ports = obj.GetPorts();
                    List<Port> newPorts = newObj.GetPorts();
                    List<Link> Removed = new List<Link>();

                    int counter = 0;
                    foreach (Port port in ports)
                    {
                        if (port.Dependencies.Count > 0)
                        {
                            if (port.IsOutput)
                            {
                                foreach (Link link in port.Dependencies)
                                {
                                    message += " Link output switched (" + link.Name + ")\n";
                                    link.Source = newPorts[counter];
                                    newPorts[counter].Dependencies.Add(link);
                                    Removed.Add(link);
                                }

                                foreach (Link link in Removed)
                                {
                                    port.Dependencies.Remove(link);
                                }

                                Removed.Clear();
                            }
                            else
                            {
                                foreach (Link link in port.Dependencies)
                                {
                                    message += " Link input switched (" + link.Name + ")\n";
                                    link.Target = newPorts[counter];
                                    newPorts[counter].Dependencies.Add(link);
                                    Removed.Add(link);
                                }

                                foreach (Link link in Removed)
                                {
                                    port.Dependencies.Remove(link);
                                }

                                Removed.Clear();
                            }
                        }

                        counter++;
                    }
                }
                else
                {
                    message += " WARNING : No match for " + obj.Name + ", skipped !\n";
                }
            }

            return message;
        }

        public virtual PortObj AddDynamicPort(PortObj obj)
        {
            return null;
        }

        #region IXmlSerializable Members

        /// <summary>
        /// Get Xml schema (not used)
        /// </summary>
        /// <returns>The Schema</returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Initializes its members from the Xml reader 
        /// </summary>
        /// <param name="reader">The reader streaming the xml</param>
        public void ReadXml(XmlReader reader)
        {
            reader.Read();

            _name = reader.ReadElementString();
            _nativeName = reader.ReadElementString();

            mUIx = TypesHelper.FloatParse(reader.ReadElementString());
            mUIy = TypesHelper.FloatParse(reader.ReadElementString());

            mDisplayState = (NodeState)Enum.Parse(typeof(NodeState), reader.ReadElementString());

            mPortContracts = new List<PortContract>();
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "PortContracts")
            {
                reader.Read(); // Skip ahead to next node
                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "PortContract")
                {
                    PortContract contract = (PortContract)Serializer.PortConstractSerializer.Deserialize(reader);
                    mPortContracts.Add(contract);
                }

                if (reader.LocalName == "PortContracts")
                {
                    reader.ReadEndElement();
                }
            }
            else
            {
                throw new Exception(string.Format("'PortContracts' Xml element not found !! (line : {0})", (reader as XmlTextReader).LineNumber));
            }

            mUser = reader.ReadElementString();
            mExportDate = long.Parse(reader.ReadElementString());
            mPath = reader.ReadElementString();
            mCheckedOut = bool.Parse(reader.ReadElementString());

            mVersion = int.Parse(reader.ReadElementString());
            mFreezed = bool.Parse(reader.ReadElementString());
            mAllowAddPorts = bool.Parse(reader.ReadElementString());
            mDynamicInputs = bool.Parse(reader.ReadElementString());
            mDynamicOutputs = bool.Parse(reader.ReadElementString());

            mDescription = reader.ReadElementString();
            mTags = reader.ReadElementString();
            mCategory = reader.ReadElementString();

            mElements = new List<PortObj>();

            string type = "Default";

            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "PortObjs")
            {
                reader.Read(); // Skip ahead to next node
                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "PortObj")
                {
                    if (reader.HasAttributes)
                    {
                        type = reader.GetAttribute("Type");
                    }

                    PortObj element = (PortObj)Serializer.PortObjSerializers[type].Deserialize(reader);
                    Elements.Add(element);
                    element.Owner = this;
                    reader.Read();
                }

                if (reader.LocalName == "PortObjs")
                {
                    reader.ReadEndElement();
                }
            }
            else
            {
                throw new Exception(string.Format("'PortObjs' Xml element not found !! (line : {0})", (reader as XmlTextReader).LineNumber));
            }

            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Inputs")
            {
                reader.Read(); // Skip ahead to next node
                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Port")
                {
                    Port rigPort = (Port)Serializer.PortSerializer.Deserialize(reader);
                    rigPort.Owner = this;
                    rigPort.PortObj.Owner = this;
                    Inputs.Add(rigPort);
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

            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Outputs")
            {
                reader.Read(); // Skip ahead to next node
                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Port")
                {
                    Port rigPort = (Port)Serializer.PortSerializer.Deserialize(reader);
                    rigPort.Owner = this;
                    Outputs.Add(rigPort);
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
            else
            {
                throw new Exception(string.Format("'CustomFields' Xml element not found !! (line : {0})", (reader as XmlTextReader).LineNumber));
            }

            reader.ReadEndElement();

            ResolveObjets();
            RefreshPortsIndices();
        }

        /// <summary>
        /// Saves its members to the Xml writer 
        /// </summary>
        /// <param name="reader">The writer streaming the xml</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Type", NodeElementType);

            writer.WriteElementString("Name", _name);
            writer.WriteElementString("NativeName", _nativeName);

            writer.WriteElementString("UIx", mUIx.ToString());
            writer.WriteElementString("UIy", mUIy.ToString());

            writer.WriteElementString("DisplayState", mDisplayState.ToString());

            writer.WriteStartElement("PortContracts");
            foreach (PortContract contract in PortContracts)
            {
                Serializer.PortConstractSerializer.Serialize(writer, contract);
            }
            writer.WriteEndElement();

            writer.WriteElementString("User", mUser);
            writer.WriteElementString("ExportDate", mExportDate.ToString());
            writer.WriteElementString("Path", mPath);
            writer.WriteElementString("CheckedOut", mCheckedOut.ToString());

            writer.WriteElementString("Version", mVersion.ToString());
            writer.WriteElementString("Freezed", mFreezed.ToString());
            writer.WriteElementString("AllowAddPorts", mAllowAddPorts.ToString());
            writer.WriteElementString("DynamicInputs", mDynamicInputs.ToString());
            writer.WriteElementString("DynamicOutputs", mDynamicOutputs.ToString());

            writer.WriteElementString("Description", mDescription);
            writer.WriteElementString("Tags", mTags);
            writer.WriteElementString("Category", mCategory);

            writer.WriteStartElement("PortObjs");
            foreach (PortObj element in Elements)
            {
                Serializer.PortObjSerializers[element.NodeElementType].Serialize(writer, element);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Inputs");
            foreach (Port port in Inputs)
            {
                Serializer.PortSerializer.Serialize(writer, port);
            }
            writer.WriteEndElement();


            writer.WriteStartElement("Outputs");
            foreach (Port port in Outputs)
            {
                Serializer.PortSerializer.Serialize(writer, port);
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

        #region IComparable<Node> Members

        /// <summary>
        /// Compare this node in order of deepness
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            Node otherNode = other as Node;
            if (otherNode == null)
            {
                throw new ArgumentException("Object is not a Node");
            }

            return RealDeepness.CompareTo(otherNode.RealDeepness);
        }

        #endregion

        public virtual List<Port> RestrictedPorts()
        {
            return new List<Port>();
        }

        public virtual Port GetOutput()
        {
            for (int j = Outputs.Count - 1; j >= 0; j--)
            {
                Port port = Outputs[j];
                if (port.NodeElementType == "3DObject")
                {
                    return port;
                }
            }

            return null;
        }
    }
}
