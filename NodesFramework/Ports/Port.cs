using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using TK.NodalEditor;
using System.ComponentModel;

namespace TK.NodalEditor
{
    /// <summary>
    /// Port defines a connection point to a Node. I Contains a PortObj, describing the physical object concerned with the port
    /// </summary>
    [XmlInclude(typeof(PortInstance))]
    public class Port : NodeBase, IXmlSerializable
    {
        #region CONSTRUCTORS
        /// <summary>
        /// Empty constructor. Should not be used except for testing.
        /// </summary>
        public Port()
        {
        }

        /// <summary>
        /// Clone constructor
        /// </summary>
        /// <param name="inPort">The reference Port to clone</param>
        public Port(Port inPort)
        {
            Copy(inPort);
        }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="inOwnerRig">Node which contains the Port</param>
        /// <param name="inPortObj">The PortObj in the Port</param>
        /// <param name="inIndex">The position in the ports list</param>
        /// <param name="inIsOutput">true to generate an Output Port, false to generate an Input Port</param>
        public Port(Node inOwnerRig, PortObj inPortObj, int inIndex, bool inIsOutput)
        {
            _name = _nativeName = inPortObj.Name;

            _owner = inOwnerRig;
            _portObj = inPortObj;
            _isOutput = inIsOutput;
            Index = inIndex;
        }

        #endregion

        #region MEMBERS

        /// <summary>
        /// Indicates if the port was dynamically added to the node
        /// @todo Do we really need to specify it here ?
        /// </summary>
        protected bool _isDynamic = false;

        /// <summary>
        /// Determines if this is an Output Port(true), or an Input (false)
        /// </summary>
        protected bool _isOutput;

        /// <summary>
        /// Node which contains the Port
        /// </summary>
        protected Node _owner;

        /// <summary>
        /// The position in the ports list
        /// </summary>
        protected int _index;

        /// <summary>
        /// The actual position in the ports list (considering some ports could be hidden)
        /// </summary>
        protected int _displayIndex;

        /// <summary>
        /// Tells if the port is Visible (internally)
        /// </summary>
        public bool _visible = true;

        /// <summary>
        /// The PortObj, describing the physical object concerned with the port
        /// </summary>
        private PortObj _portObj;

        /// <summary>
        /// The links connected to this Port
        /// </summary>
        protected List<Link> _dependencies = new List<Link>();

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The full name of the Port (this could be overriden to define high-level naming rules)
        /// <seealso cref="CG_XSIRig">
        /// </summary>
        public override string FullName
        {
            get { return Name; }
        }

        
        /// <summary>
        /// Give the port a FullName, considering several nodes could have ports with the same FullName
        /// </summary>
        //private string mUniqueNameCache = null;
        internal string UniqueName
        {
            get 
            {
                return string.Format("{0}_{1}", Owner == null ? string.Empty : Owner.FullName, Name);
            }
        }

        /// <summary>
        /// Returns itself in the case of a simple Port, but could be used to return actual connected port of a Compound
        /// @todo That's the core of what needs to be changed in V2 : Compound ports need to be managed just the same as standard node ports. (See PortContract)
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual Port RealPort
        {
            get { return this; }
        }

        /// <summary>
        /// Convenient method to return the PortObj NodeElementType
        /// </summary>
        [BrowsableAttribute(false)]
        public string NodeElementType
        {
            get { return PortObj.NodeElementType; }
        }

        /// <summary>
        /// true if the Port have got at least a Link, false otherwise
        /// </summary>
        [BrowsableAttribute(false)]
        public bool Linked
        {
            get
            {
                return RealPort.Dependencies.Count > 0;
            }
        }

        /// <summary>
        /// Indicates if the port should be highlighted (when the mouse is overing it)
        /// </summary>
        [BrowsableAttribute(false)]
        public bool HighLight
        {
            get { return PortObj.HighLight; }
        }

        /// <summary>
        /// Convenient method to return wether the PortObj is Default (true) or dynamically cretaed (false)
        /// @todo redundant with IsDynamic ?
        /// </summary>
        [BrowsableAttribute(false)]
        public bool Default
        {
            get
            {
                return PortObj.Default;
            }
        }

        /// <summary>
        /// Indicates if the port was dynamically added to the node
        /// </summary>
        [BrowsableAttribute(false)]
        public bool IsDynamic
        {
            get { return _isDynamic; }
            set { _isDynamic = value; }
        }

        /// <summary>
        /// Determines if this is an Output Port(true), or an Input (false)
        /// </summary>
        [BrowsableAttribute(false)]
        public bool IsOutput
        {
            get { return _isOutput; }
            set { _isOutput = value; }
        }

        /// <summary>
        /// Node which contains the Port
        /// </summary>
        [BrowsableAttribute(false)]
        public Node Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        /// <summary>
        /// The position in the ports list
        /// </summary>
        [BrowsableAttribute(false)]
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// The actual position in the ports list (considering some ports could be hidden)
        /// </summary>
        [BrowsableAttribute(false)]
        public int DisplayIndex
        {
            get { return _displayIndex; }
            set { _displayIndex = value; }
        }

        /// <summary>
        /// Tells if the port is Visible (internally)
        /// </summary>
        [CategoryAttribute("Basic")]
        [DescriptionAttribute("Indicates if the port should show in the UI")]
        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                Owner.RefreshPortsIndices();
                if (!Owner.isCompoundGenerated && Owner.Parent != null)
                {
                    Owner.Parent.RefreshVisibility(this);
                }
            }
        }

        /// <summary>
        /// Tells if the port is Visible depending on the node Display State
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool IsVisible
        {
            get { return (_visible && Owner.DisplayState == NodeState.Normal || (Owner.DisplayState != NodeState.Collapsed && IsLinked())); }
        }

        /// <summary>
        /// The PortObj, describing the physical object concerned with the port
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual PortObj PortObj
        {
            get { return _portObj; }
            set { _portObj = value; }
        }

        /// <summary>
        /// The links connected to this Port
        /// </summary>
        [CategoryAttribute("Debug")]
        [DescriptionAttribute("Dependencies of the port")]
        public List<Link> Dependencies
        {
            get { return _dependencies; }
            set { _dependencies = value; }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Tells if the Port have got at least a link, and allow override for PortInstance
        /// @todo As we have to get rid of PortInstance, this should not be usefull
        /// </summary>
        /// <returns>true if the Port have got at least a link, false otherwise</returns>
        public virtual bool IsLinked()
        {
            return Linked;
        }

        internal virtual bool IsRestricted()
        {
            return Owner.RestrictedPorts().Contains(this);
        }

        /// <summary>
        /// Give the ability to the node to rename its ports.
        /// </summary>
        /// <param name="value"></param>
        protected override void UpdateName(string value)
        {
            if (Owner.Parent != null)
            {
                if (value != null)
                    Owner.Parent.UpdatePortName(this, value);
                else
                    Owner.Parent.UpdatePortName(this, Name);
                //mUniqueNameCache = Owner.FullName + "_" + Name;
            }
        }

        /// <summary>
        /// Copy the Port (Initialize its variables FROM the given Port)
        /// </summary>
        /// <param name="inPort">Port to copy into this Port instance</param>
        public void Copy(Port inPort)
        {
            _nativeName = inPort.NativeName;
            _name = inPort.Name;
            _visible = inPort.Visible;
            _displayIndex = inPort.DisplayIndex;
            _isDynamic = inPort.IsDynamic;
            _isOutput = inPort.IsOutput;
            _portObj = (PortObj)Activator.CreateInstance(inPort.PortObj.GetType(), new object[0]);
            _portObj.Copy(inPort.PortObj);
            if (!IsOutput)
            {
                CopyDependencies(inPort);
            }
            else
            {
                _owner = (Node)Activator.CreateInstance(inPort.Owner.GetType(), new object[0]);
                _owner.NativeName = "";
                _owner.Name = inPort.Owner.Name;

            }
        }

        /// <summary>
        /// Copy the dependencies (Link instances) of the given Port in this Port instance
        /// </summary>
        /// <param name="port">Port from which to copy the Dependenies</param>
        protected void CopyDependencies(Port port)
        {
            List<Link> deps = new List<Link>();
            foreach (Link dep in port.Dependencies)
            {
                Link copydep = (Link)Activator.CreateInstance(dep.GetType(), new object[0]);
                copydep.Copy(dep);
                if (!port.IsOutput)
                {
                    copydep.Target = this;

                    //Create a fake output port, to be resolved by Compound.ResolveAll...
                    copydep.Source = new Port();
                    copydep.Source.Owner = (Node)Activator.CreateInstance(dep.Source.Owner.GetType(), new object[0]);
                    copydep.Source.Owner.NativeName = "";
                    copydep.Source.Owner.Name = dep.Source.Owner.Name;
                    copydep.Source.Owner.CopyCustomFields(dep.Source.Owner, true);
                    copydep.Source.Name = copydep.Source.NativeName = dep.Source.Name;
                }
                else
                {
                    copydep.Source = this;

                    //Create a fake input port, to be resolved by Compound.ResolveAll...
                    copydep.Target = new Port();
                    copydep.Target.Owner = (Node)Activator.CreateInstance(dep.Target.Owner.GetType(), new object[0]);
                    copydep.Target.Owner.NativeName = "";
                    copydep.Target.Owner.Name = dep.Target.Owner.Name;
                    copydep.Target.Owner.CopyCustomFields(dep.Target.Owner, true);
                    copydep.Target.Name = copydep.Target.NativeName = dep.Target.Name;
                }

                deps.Add(copydep);
            }

            _dependencies = deps;
        }

        /// <summary>
        /// Used to return the PortObj's list of custom values (Parameters)
        /// </summary>
        /// <returns>The PortObj's parameters values</returns>
        public object[] GetPortParams()
        {
            return PortObj.GetPortParams();
        }

        /// <summary>
        /// The Port name with mention of the Ouput/Input nature
        /// @todo Used only for comparison ?!?...Have a closer look at this
        /// </summary>
        /// <returns>Port name with mention of the Ouput/Input nature</returns>
        public string DetailedName()
        {
            return DetailedName(Name);
        }

        /// <summary>
        /// The Port name with mention of the Ouput/Input nature
        /// </summary>
        /// <param name="name">The name to use</param>
        /// <returns>Port name with mention of the Ouput/Input nature</returns>
        public string DetailedName(string name)
        {
            return (IsOutput ? "OUT:" : "IN:") + name;
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

            _visible = bool.Parse(reader.ReadElementString());
            _isDynamic = bool.Parse(reader.ReadElementString());
            _isOutput = bool.Parse(reader.ReadElementString());

            _portObj = new PortObj();
            _portObj.Name = reader.ReadElementString();
            Owner = new Node();
            Owner.NativeName = "";
            Owner.Name = reader.ReadElementString();

            string type = "Default";
            if (reader.LocalName == "Links")
            {
                reader.Read(); // Skip ahead to next node

                while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Link")
                {
                    if (reader.HasAttributes)
                    {
                        type = reader.GetAttribute("Type");
                    }

                    XmlSerializer linkSerializer = null;

                    if (!Serializer.LinkSerializers.TryGetValue(type, out linkSerializer))
                    {
                        linkSerializer = Serializer.LinkSerializers["Default"];
                    }

                    Link dep = (Link)linkSerializer.Deserialize(reader);
                    dep.Target = this;
                    Dependencies.Add(dep);
                }

                if (reader.LocalName == "Links")
                {
                    reader.Read();
                }
            }

            reader.ReadEndElement();
        }

        /// <summary>
        /// Saves its members to the Xml writer 
        /// </summary>
        /// <param name="reader">The writer streaming the xml</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Name", _name);
            writer.WriteElementString("NativeName", _nativeName);

            writer.WriteElementString("Visible", _visible.ToString());
            writer.WriteElementString("IsDynamic", _isOutput.ToString());
            writer.WriteElementString("IsOutput", _isOutput.ToString());

            writer.WriteElementString("PortObj", _portObj.Name.ToString());
            writer.WriteElementString("OwnerRig", Owner.FullName.ToString());

            writer.WriteStartElement("Links");
            if (!IsOutput)
            {
                foreach (Link dep in Dependencies)
                {
                    Serializer.LinkSerializers[dep.NodeElementType].Serialize(writer, dep);
                }
            }
            writer.WriteEndElement();
        }

        #endregion

        /// <summary>
        /// Override of ToString()
        /// @todo return "" if not visible ? Seems weird with insight
        /// </summary>
        /// <returns>The string version of the object</returns>
        public override string ToString()
        {
            return PortObj.ToString();
        }

        #endregion
    }
}
