using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Drawing;
using System.ComponentModel;
using TK.NodalEditor;
using TK.BaseLib.CustomData;

namespace TK.NodalEditor
{
    /// <summary>
    /// A PortObj is the "physical" object that relates to one or more Ports, and that is affected by connexions.
    /// </summary>
    public class PortObj : NodeBase, IXmlSerializable
    {
        #region CONSTRUCTORS
        /// <summary>
        /// Base constructor
        /// </summary>
        public PortObj()
        {
        }

        /// <summary>
        /// Clone constructor
        /// </summary>
        /// <param name="inRef">The PortObj that have to be cloned</param>
        public PortObj(PortObj inRef)
        {
            this.Copy(inRef);
        }

        #endregion

        #region MEMBERS

        /// <summary>
        /// Category that can be used by higher objects to separate PortObjs into simple groups
        /// <seealso cref="TK_OSCARLib.CG_PortParam">
        /// </summary>
        protected string _category;

        /// <summary>
        /// Tells if this PortObj is by default (true) or was dynamically created (false)
        /// </summary>
        bool _default = true;

        /// <summary>
        /// Node carrying the PortObj
        /// </summary>
        protected Node _owner = null;

        /// <summary>
        /// PortObj can be linked to an input port (can be driven)
        /// </summary>
        bool _isInput = true;

        /// <summary>
        /// PortObj can be linked to an output port (can drive)
        /// </summary>
        bool _isOutput = true;
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Category that can be used by higher objects to separate PortObjs into simple groups
        /// <seealso cref="TK_OSCARLib.CG_PortParam">
        /// </summary>
        [BrowsableAttribute(false)]
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        /// <summary>
        /// Tells if this PortObj is by default (true) or was dynamically created (false)
        /// </summary>
        [BrowsableAttribute(false)]
        public bool Default
        {
            get { return _default; }
            set { _default = value; }
        }

        /// <summary>
        /// Node carrying the PortObj
        /// </summary>
        [BrowsableAttribute(false)]
        public Node Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        /// <summary>
        /// FullName of the PortObj (simply its name at this stage, containing the name of the owner Node)
        /// </summary>
        public override string FullName
        {
            get { return Name; }
        }

        /// <summary>
        /// Name without the owner node name
        /// </summary>
        [BrowsableAttribute(false)]
        public string ShortName
        {
            get
            {
                return Name.Contains(Owner.FullName) ? Name.Substring(Owner.FullName.Length + 1) : Name;
            }
        }

        /// <summary>
        /// PortObj can be linked to an input port (can be driven)
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool IsInput
        {
            get { return _isInput; }
            set { _isInput = value; }
        }

        /// <summary>
        /// Determines if the PortObj should make its input Port visible by default
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool ExposeInput
        {
            get { return true; }
        }

        /// <summary>
        /// PortObj can be linked to an output port (can drive)
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool IsOutput
        {
            get { return _isOutput; }
            set { _isOutput = value; }
        }

        /// <summary>
        /// Determines if the PortObj should make its output Port visible by default
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool ExposeOutput
        {
            get { return true; }
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
        /// Tells if this PortObj is driven by any other PortObj through a Link
        /// </summary>
        [BrowsableAttribute(false)]
        public bool IsConnected
        {
            get
            {
                foreach (Link link in Owner.InDependencies)
                {
                    if (link.Target.PortObj == this)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Tells if this PortObj drive any other PortObj through a Link
        /// </summary>
        [BrowsableAttribute(false)]
        public bool IsConnecting
        {
            get
            {
                foreach (Link link in Owner.OutDependencies)
                {
                    if (link.Source.PortObj == this)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates if the port should be highlighted (when the mouse is overing it)
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool HighLight
        {
            get { return false; }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Collects the ports that uses this PortObj
        /// </summary>
        /// <returns></returns>
        public List<Port> GetPorts()
        {
            List<Port> ports = new List<Port>();

            foreach (Port port in Owner.Inputs)
            {
                if (port.PortObj == this)
                {
                    ports.Add(port);
                    break;
                }
            }

            foreach (Port port in Owner.Outputs)
            {
                if (port.PortObj == this)
                {
                    ports.Add(port);
                    break;
                }
            }

            return ports;
        }

        /// <summary>
        /// Copy the PortObj (Initialize its variables FROM the given PortObj)
        /// </summary>
        /// <param name="inLink">PortObj to copy into this PortObj instance</param>
        public void Copy(PortObj inPort)
        {
            _nativeName = inPort.NativeName;
            _name = inPort.Name;
            _owner = inPort.Owner;
            _default = inPort.Default;
            _isInput = inPort._isInput;
            _isOutput = inPort._isOutput;

            CopyCustomFields(inPort, true);
        }

        /// <summary>
        /// Virtual method called used by inherited classes to serialize their added members
        /// <seealso cref="TK_OSCARLib.PortParam">
        /// <seealso cref="TK_OSCARLib.RigElement">
        /// </summary>
        protected virtual void CopyCustomFields(PortObj inPortObj, bool Resolve)
        {

        }

        /// <summary>
        /// Update method for fields in inherited classes
        /// </summary>
        /// <param name="refObj">The PortObj used as reference for the update</param>
        /// <param name="All">Indicates if we have to match every members</param>
        public virtual void Update(PortObj refObj, bool All)
        {

        }

        /// <summary>
        /// Used to return a list of custom values (Parameters)
        /// </summary>
        /// <returns>The parameters values</returns>
        public virtual object[] GetPortParams()
        {
            return new object[0];
        }


        /// <summary>
        /// Method called in Node.Update
        /// @todo Related with the "Reference mechanism". It need some more reflection
        /// </summary>
        public virtual void ResolveCustomObjects()
        {

        }

        /// <summary>
        /// Computes a message for substitution of this PortObj with another
        /// @todo could be more elegant, move that to a static ?
        /// </summary>
        /// <param name="portObj"></param>
        /// <returns></returns>
        public virtual string Substitute(PortObj portObj)
        {
            string message = " * Substitute " + Name + " with " + portObj.Name + "\n";
            return message;
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
            _category = reader.ReadElementString();
            _default = bool.Parse(reader.ReadElementString());

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
            writer.WriteElementString("Category", _category);
            writer.WriteElementString("Default", _default.ToString());

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

        /// <summary>
        /// Return ""
        /// @todo Why ?!?
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "";
        }

        #endregion

        public string NiceName
        {
            get { return string.Format("{0}.{1} ({2})",Owner.FullName, ShortName, NodeElementType); }
        }
    }
}
