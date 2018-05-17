using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using TK.BaseLib.CustomData;
using TK.BaseLib;

namespace TK.NodalEditor
{
    /// <summary>
    /// Enumeration of the node-related types
    /// </summary>
    public enum NodeElement
    {
        /// A node with Ports and Links
        Node,
        /// A group of several nodes that can have links as well that relates to nodes
        Compound,
        /// A "physical" object in a node that relates to one or more Ports
        PortObj,
        /// A relation between nodes
        Link
    }

    /// <summary>
    /// Singleton that holds the different serializers for node-related types. It is used to "configure" in which specific type the xml should be deserialized, considering every inherited classes of Node or Compound will have the same xml output. 
    /// </summary>
    public class NodesSerializer
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Private singleton constructor
        /// </summary>
        private NodesSerializer()
        {
            NodeSerializers.Add("Default", new XmlSerializer(typeof(Node)));
            CompoundSerializers.Add("Default", new XmlSerializer(typeof(Compound)));
            PortObjSerializers.Add("Default", new XmlSerializer(typeof(PortObj)));
            LinkSerializers.Add("Default", new XmlSerializer(typeof(Link)));

            PortSerializer = new XmlSerializer(typeof(Port));
            PortConstractSerializer = new XmlSerializer(typeof(PortContract));
            PortInstanceSerializer = new XmlSerializer(typeof(PortInstance));
        }

        #endregion

		#region MEMBERS

        /// <summary>
        /// Singleton static instance
        /// </summary>
        private static NodesSerializer _instance = null;

        /// <summary>
        /// List of custom serializers, for types that are not node-related
        /// </summary>
        Dictionary<string, XmlSerializer> _customSerializers = new Dictionary<string, XmlSerializer>();

        /// <summary>
        /// Serializers for Node
        /// </summary>
        public Dictionary<string, XmlSerializer> NodeSerializers = new Dictionary<string, XmlSerializer>();

        /// <summary>
        /// Serializers for Compound
        /// </summary>
        public Dictionary<string, XmlSerializer> CompoundSerializers = new Dictionary<string, XmlSerializer>();

        /// <summary>
        /// Serializers for PortObj
        /// </summary>
        public Dictionary<string, XmlSerializer> PortObjSerializers = new Dictionary<string, XmlSerializer>();

        /// <summary>
        /// Serializers for Link
        /// </summary>
        public Dictionary<string, XmlSerializer> LinkSerializers = new Dictionary<string, XmlSerializer>();

        /// <summary>
        /// The port serializer (only one is required, because it don't have to be inherited
        /// </summary>
        public XmlSerializer PortSerializer;

        /// <summary>
        /// The portContract serializer (only one is required, because it don't have to be inherited
        /// </summary>
        public XmlSerializer PortConstractSerializer;

        /// <summary>
        /// The portInstance serializer (only one is required, because it don't have to be inherited
        /// </summary>
        public XmlSerializer PortInstanceSerializer;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Get the singleton instance of this class
        /// </summary>
        /// <returns></returns>
        public static NodesSerializer GetInstance()
        {
            if (_instance == null)
                _instance = new NodesSerializer();

            return _instance;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Create a base-element serializer and add it to the dictionary
        /// </summary>
        /// <param name="inElementType">Type of node-relatyed element serializer to create</param>
        /// <param name="inName">Name of the serializer</param>
        /// <param name="inType">Actual type of the class</param>
        public void AddSerializer(NodeElement inElementType, string inName, Type inType)
        {
            switch (inElementType)
            {
                case NodeElement.Node:

                    if (NodeSerializers.ContainsKey(inName))
                    {
                        NodeSerializers[inName] = new XmlSerializer(inType);
                    }
                    else
                    {
                        NodeSerializers.Add(inName, new XmlSerializer(inType));
                    }
                    break;

                case NodeElement.Compound:

                    if (CompoundSerializers.ContainsKey(inName))
                    {
                        CompoundSerializers[inName] = new XmlSerializer(inType);
                    }
                    else
                    {
                        CompoundSerializers.Add(inName, new XmlSerializer(inType));
                    }

                    break;

                case NodeElement.PortObj:

                    if (PortObjSerializers.ContainsKey(inName))
                    {
                        PortObjSerializers[inName] = new XmlSerializer(inType);
                    }
                    else
                    {
                        PortObjSerializers.Add(inName, new XmlSerializer(inType));
                    }

                    break;

                case NodeElement.Link:

                    if (LinkSerializers.ContainsKey(inName))
                    {
                        LinkSerializers[inName] = new XmlSerializer(inType);
                    }
                    else
                    {
                        LinkSerializers.Add(inName, new XmlSerializer(inType));
                    }

                    break;
            }
        }

        /// <summary>
        /// Get a custom serializer, or create it and add it to the list if it doesn't exists
        /// </summary>
        /// <param name="inType">The type to serialize</param>
        /// <param name="RootName">The name given to the type in the xml</param>
        /// <returns>The found (or created) serializer</returns>
        public XmlSerializer GetCustomSerializer(Type inType, string RootName)
        {
            if (!_customSerializers.ContainsKey(RootName))
            {
                _customSerializers.Add(RootName, new XmlSerializer(inType, new XmlRootAttribute(RootName)));
            }

            return _customSerializers[RootName];
        }

        /// <summary>
        /// Little static helper to serialize a color
        /// </summary>
        /// <param name="color">Color to serialize</param>
        /// <returns>The serialized string</returns>
        public static string SerializeColor(Color color)
        {
            if (color.IsNamedColor)
                return string.Format("{0}:{1}",
                    ColorFormat.NamedColor, color.Name);
            else
                return string.Format("{0}:{1}:{2}:{3}:{4}",
                    ColorFormat.ARGBColor,
                    color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Little static helper to deserialize a color
        /// </summary>
        /// <param name="color">The serialized string</param>
        /// <returns>Deserialized Color</returns>
        public static Color DeserializeColor(string color)
        {
            byte a, r, g, b;

            string[] pieces = color.Split(new char[] { ':' });

            ColorFormat colorType = (ColorFormat)
                Enum.Parse(typeof(ColorFormat), pieces[0], true);

            switch (colorType)
            {
                case ColorFormat.NamedColor:
                    return Color.FromName(pieces[1]);

                case ColorFormat.ARGBColor:
                    a = byte.Parse(pieces[1]);
                    r = byte.Parse(pieces[2]);
                    g = byte.Parse(pieces[3]);
                    b = byte.Parse(pieces[4]);

                    return Color.FromArgb(a, r, g, b);
            }
            return Color.Empty;
        }

        #endregion
    }
}