using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TK.NodalEditor
{
    /// <summary>
    /// Simple field container that helps in serializing classes inherited from Node and Compound (Deserialization is done with NodeBase.DeserializeCustomField) 
    /// </summary>
    public class CustomField
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Base constructor, for types serializable as strings
        /// </summary>
        /// <param name="inName">Name of the field</param>
        /// <param name="inValue">Default value of the field</param>
        public CustomField(string inName, object inValue)
        {
            Name = inName;
            Value = inValue;
            isNative = true;
        }

        /// <summary>
        /// Constructor for advanced and custom types, requiring a specific XmlSerializer
        /// </summary>
        /// <param name="inName">Name of the field</param>
        /// <param name="inValue">Default value of the field</param>
        /// <param name="inType">Type of the field</param>
        public CustomField(string inName, object inValue, Type inType)
        {
            Name = inName;
            Value = inValue;
            FieldType = inType;
        }

        #endregion
        
        #region MEMBERS

        /// <summary>
        /// Name of the field
        /// </summary>
        public string Name;

        /// <summary>
        /// Value of the field
        /// </summary>
        public object Value;

        /// <summary>
        /// Type of the field, when not serilizable in string
        /// </summary>
        public Type FieldType;

        /// <summary>
        /// Tells if the type is serilizable in string
        /// </summary>
        bool isNative;

        #endregion

        #region METHODS

        /// <summary>
        /// Serialize the field to and Xml Stream 
        /// </summary>
        /// <param name="writer"></param>
        public void Serialize(XmlWriter writer)
        {
            if (isNative)
            {
                writer.WriteElementString(Name, Value.ToString());
            }
            else
            {
                XmlSerializer serializer = NodesSerializer.GetInstance().GetCustomSerializer(FieldType, Name);
                serializer.Serialize(writer, Value);
            }
        }

        #endregion
    }
}
