using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml;
using OrderedPropertyGrid;

namespace TK.NodalEditor
{
    /// <summary>
    /// Base class for node elements. It have a name , can be selected and have convenience methods for xml serialization
    /// </summary>
    [TypeConverter(typeof(PropertySorter))]
    [DefaultProperty("Name")]
    public class NodeBase
    {
        #region MEMBERS

        /// <summary>
        /// Base name of the element
        /// </summary>
        public string _name = "NewNodeElement";

        /// <summary>
        /// internal name of the element, stores the original name of the element
        /// </summary>
        protected string _nativeName = "NewNodeElement";

        /// <summary>
        /// Tells if the element is selected
        /// </summary>
        bool _selected;

        #endregion
        
        #region PROPERTIES

        /// <summary>
        /// Base name of the element
        /// </summary>
        //[CategoryAttribute("Basic")]
        [DescriptionAttribute("Name of the element")]
        [Category("Basic"), PropertyOrder(10)]
        public string Name
        {
            get { return _name; }
            set 
            {
                /*
                _name = value; 
                //UpdateName(GetFullName(value));
                UpdateName(value);
                */

                UpdateName(GetFullName(value));
                _name = value;
            }
        }

        /// <summary>
        /// internal name of the element, stores the original name of the element
        /// </summary>
        //[CategoryAttribute("Basic")]
        [DescriptionAttribute("Original name of the element")]
        [ReadOnlyAttribute(true)]
        [Category("Basic"), PropertyOrder(10)]
        public string NativeName
        {
            get { return _nativeName; }
            set { _nativeName = value; }
        }

        /// <summary>
        /// Real name of the element, in case we use external modifiers
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual string FullName
        {
            get { return Name; }
            set { Name = value; }
        }

        /// <summary>
        /// Tells if the element is selected
        /// </summary>
        [BrowsableAttribute(false)]
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        /// <summary>
        /// Accessor for serializer singleton
        /// </summary>
        [BrowsableAttribute(false)]
        public NodesSerializer Serializer
        {
            get { return NodesSerializer.GetInstance(); }
        }

        #endregion        
        
        #region METHODS
        
        /// <summary>
        /// Method called when the name is changed
        /// </summary>
        /// <param name="value">New name</param>
        protected virtual void UpdateName(string value)
        {
        }

        /// <summary>
        /// Get the FullName of the object, allows for name modifiers in inherited classes such as TK_OSCARLib.RigNode
        /// </summary>
        /// <param name="value">The name</param>
        /// <returns>The FullName</returns>
        public virtual string GetFullName(string value)
        {
            return value;
        }

        /// <summary>
        /// Get the Name from the FullName (opposite to GetFullName)
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public virtual string GetName(string fullName)
        {
            return fullName;
        }

        // --- Virtuals for Serialization ---

        /// <summary>
        /// Get the list of custom fields of inherited classes for serialization
        /// </summary>
        /// <returns>The list of CustomField objects</returns>
        protected virtual List<CustomField> GetCustomFields()
        {
            return new List<CustomField>();
        }

        /// <summary>
        /// Read custom fields from the xml stream for deserialization of inherited classes
        /// </summary>
        /// <param name="reader">reader stream from the xml</param>
        /// <param name="inName">Name of the field</param>
        protected virtual void DeserializeCustomField(XmlReader reader, string inName)
        {
            reader.Read();
        }

        /// <summary>
        /// ToString override, returning the FullName and NativeName
        /// </summary>
        /// <returns>FullName and NativeName</returns>
        public override string ToString()
        {
            return FullName + (FullName != NativeName ? " (instance of \"" + NativeName + "\")" : string.Empty);
        }

        #endregion
    }
}
