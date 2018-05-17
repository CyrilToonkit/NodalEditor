using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using TK.NodalEditor;
using System.ComponentModel;

namespace TK.NodalEditor
{
    /// <summary>
    /// Defines a Port of a compound, it relates on an actual Port (Reference) of one of the nodes inside the compound.Basically the Compounds copy and synchronise every ports of the contained nodes, which is definitely a bad idea, at least for Performance
    /// @todo Rewrite the Compounds Ports mechanism and get rid of this
    /// </summary>
    public class PortInstance : Port, IXmlSerializable
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Empty constructor. Should not be used except for testing.
        /// </summary>
        public PortInstance()
        {

        }

        /// <summary>
        /// Clone constructor
        /// </summary>
        /// <param name="inPort">The reference PortInstance to clone</param>
        public PortInstance(PortInstance inPortInstance)
        {
            Copy(inPortInstance);
        }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="inReference">The Port that this PortInstance references</param>
        /// <param name="inOwner">Compound which contains the PortInstance</param>
        /// <param name="inIndex">The position in the ports list</param>
        public PortInstance(Port inReference, Node inOwner, int inIndex)
        {
            _name = _nativeName = inReference.PortObj.Name;
            _index = inIndex;
            _isOutput = inReference.IsOutput;
            _isDynamic = inReference.IsDynamic;
            _visible = inReference.Visible;
            _reference = inReference;
            _owner = inOwner;
        }

        #endregion
        
        #region MEMBERS

        /// <summary>
        /// The Port that this PortInstance references
        /// </summary>
        Port _reference;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Tells if the port is Visible depending on the Compound Display State
        /// </summary>
        public override bool IsVisible
        {
            get { return (_visible && Owner.DisplayState == NodeState.Normal || (Owner.DisplayState != NodeState.Collapsed && IsLinked())); }
        }

        /// <summary>
        /// The Port that this PortInstance references
        /// </summary>
        [BrowsableAttribute(false)]
        public Port Reference
        {
            get
            {
                return _reference;
            }
            set
            {
                if (_nativeName == _name)
                {
                    _name = _nativeName = value.PortObj.Name;
                }
                else
                {
                    _nativeName = value.PortObj.Name;
                }

                _reference = value;
            }
        }

        /// <summary>
        /// Returns itself in the case of a simple Port, but could be used to return actual connected port of a Compound
        /// @todo That's the core of what needs to be changed in V2 : Compound ports need to be managed just the same as standard node ports. (See PortContract)
        /// </summary>
        /// 
        //private Port mRealPortCache = null;
        [BrowsableAttribute(false)]
        public override Port RealPort
        {
            get {
                // @todo : check if reference realport can be updated without notice
                /*if (mRealPortCache == null)
                    mRealPortCache = Reference.RealPort;
                */
                return Reference.RealPort;
            }
        }

        /// <summary>
        /// Overrides Port "PortObj" to return the Reference Port one
        /// </summary>
        public override PortObj PortObj
        {
            get { return Reference.PortObj; }
        }

        #endregion
        
        #region METHODS

        public override bool IsLinked()
        {
            if (RealPort.Dependencies.Count == 0)
            {
                return false;
            }

            Compound own = Owner as Compound;
            if (IsOutput)
            {                
                foreach (Link link in RealPort.Dependencies)
                {
                    if (!link.Target.Owner.IsIn(own))
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (Link link in RealPort.Dependencies)
                {
                    if (!link.Source.Owner.IsIn(own))
                    {
                        return true;
                    }
                }
            }
            /*
            if (IsOutput)
            {
                foreach (Link link in RealPort.Dependencies)
                {
                    Node otherNode = link.Target.Owner;
                    if (!otherNode.IsIn(Owner as Compound))
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (Link link in RealPort.Dependencies)
                {
                    Node otherNode = link.Source.Owner;
                    if (!otherNode.IsIn(Owner as Compound))
                    {
                        return true;
                    }
                }
            }*/

            return false;
        }

        public void Copy(PortInstance inPort, bool Resolve)
        {
            _nativeName = inPort.NativeName;
            _name = inPort.Name;
            _visible = inPort.Visible;
            _index = inPort.Index;
            _displayIndex = inPort.DisplayIndex;
            _isDynamic = inPort.IsDynamic;
            _isOutput = inPort.IsOutput;
            _reference = inPort.Reference;
        }


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
            reader.Read();

            _nativeName = reader.ReadElementString();
            _name = reader.ReadElementString();
            _visible = bool.Parse(reader.ReadElementString());
        }

        /// <summary>
        /// Saves its members to the Xml writer 
        /// </summary>
        /// <param name="reader">The writer streaming the xml</param>
        public new void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("NativeName", _nativeName);
            writer.WriteElementString("Name", _name);
            writer.WriteElementString("Visible", Visible.ToString());
        }

        #endregion
        #endregion
    }
}
