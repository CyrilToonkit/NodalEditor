using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TK.NodalEditor;
using System.Xml;
using System.Xml.Schema;
using System.ComponentModel;
using System.Drawing;
using OrderedPropertyGrid;

namespace TK.NodalEditor
{
    /// <summary>
    /// This is the base class to describe relations between nodes, precisely between Port instances.
    /// </summary>
    public class Link : NodeBase, IXmlSerializable
    {
        #region CONSTRUCTORS
        /// <summary>
        /// Empty constructor. Should not be used except for testing.
        /// </summary>
        public Link()
        {

        }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="inSource">Source Port</param>
        /// <param name="inTarget">Target Port</param>
        public Link(Port inSource, Port inTarget)
        {
            _source = inSource;
            _target = inTarget;
        }

        /// <summary>
        /// Clone constructor
        /// </summary>
        /// <param name="inLink">The reference Link to clone</param>
        public Link(Link inLink)
        {
            Copy(inLink);
        }

        #endregion
        
        #region MEMBERS

        /// <summary>
        /// internal name of the element, stores the original name of the element
        /// </summary>
        [Browsable(false)]
        public new string NativeName
        {
            get { return _nativeName; }
            set { _nativeName = value; }
        }



        /// <summary>
        /// Indicates if the link was just created or if it's an "old" link we just need to recreate.
        /// @todo We should be able to get rid of this
        /// </summary>
        [BrowsableAttribute(false)]
        public bool IsNew = true;

        /// <summary>
        /// Source Port (That will "drive" Target)
        /// </summary>
        protected Port _source;

        /// <summary>
        /// Target Port (That will be "driven" by the Source)
        /// </summary>
        protected Port _target;

        protected bool _isHierachicallyInteresting = true;

        /// <summary>
        /// Polygon used as a collision object for link selection in the UI
        /// </summary>
        public Point[] polygon = new Point[18];

        /// <summary>
        /// Point used for calculation of the Collision box and Link Painting
        /// </summary>
        Point _point_1_1 = new Point();
        /// <summary>
        /// Point used for calculation of the Collision box and Link Painting
        /// </summary>
        Point _point_1_2 = new Point();
        /// <summary>
        /// Point used for calculation of the Collision box and Link Painting
        /// </summary>
        Point _point_1_3 = new Point();
        /// <summary>
        /// Point used for calculation of the Collision box and Link Painting
        /// </summary>
        Point _mean = new Point();
        /// <summary>
        /// Point used for calculation of the Collision box and Link Painting
        /// </summary>
        Point _point_2_3 = new Point();
        /// <summary>
        /// Point used for calculation of the Collision box and Link Painting
        /// </summary>
        Point _point_2_2 = new Point();
        /// <summary>
        /// Point used for calculation of the Collision box and Link Painting
        /// </summary>
        Point _point_2_1 = new Point();

        #endregion

        #region PROPERTIES
        /// <summary>
        /// Source Port (That will "drive" Target)
        /// </summary>
        [Browsable(false)]
        public Port Source
        {
            get { return _source; }
            set { _source = value; }
        }

        [XmlIgnore]
        [DescriptionAttribute("Source of the link")]
        [Category("Basic"), PropertyOrder(10)]
        public string SourceElement
        {
            get { return (_source == null || _source.PortObj == null) ? "NULL" : _source.PortObj.NiceName; }
        }

        /// <summary>
        /// Target Port (That will be "driven" by the Source)
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Port Target
        {
            get { return _target; }
            set { _target = value; }
        }

        [XmlIgnore]
        [DescriptionAttribute("Target of the link")]
        [Category("Basic"), PropertyOrder(10)]
        public string TargetElement
        {
            get { return (_target == null || _target.PortObj == null) ? "NULL" : _target.PortObj.NiceName; }
        }

        /// <summary>
        /// Indicates if the node is an orphan (Source and/or Target not found at reconnection)
        /// </summary>
        [BrowsableAttribute(false)]
        public bool UnResolved
        {
            get { return (_source.Owner.NativeName == "" || _target.Owner.NativeName == ""); }
        }

        /// <summary>
        /// Validity : PortObjs are not null and the Ports are of the same Type
        /// </summary>
        [BrowsableAttribute(false)]
        public bool IsValid
        {
            get { return (_source.PortObj != null && _target.PortObj != null && _source.NodeElementType == _target.NodeElementType); }
        }

        /// <summary>
        /// Virtual Property that could indicate this link is not consistent in a specific context
        /// <seealso cref="TK_OSCARLib.CG_Constraint">
        /// <seealso cref="TK_OSCARLib.CG_Expression">
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool IsConsistent
        {
            get { return true; }
        }

        /// <summary>
        /// Virtual Property that could indicate this link must be unique (no other links can be connected to the same input)
        /// <seealso cref="TK_OSCARLib.CG_Constraint">
        /// <seealso cref="TK_OSCARLib.CG_Expression">
        /// </summary>
        [BrowsableAttribute(false)]
        public virtual bool IsUnique
        {
            get { return false; }
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

        protected LinkState _state = new LinkState();
        [BrowsableAttribute(false)]
        public virtual LinkState State
        {
            get
            {
                return _state;
            }
        }

        [BrowsableAttribute(false)]
        public virtual bool Selectable
        {
            get
            {
                return true;
            }
        }

        #endregion
        
        #region METHODS

        /// <summary>
        /// Delete the link
        /// </summary>
        public void Delete()
        {
            Target.PortObj.Owner.UnConnect(this);
        }

        /// <summary>
        /// Copy the Link (Initialize its variables FROM the given Link)
        /// </summary>
        /// <param name="inLink">Link to copy into this Link instance</param>
        public void Copy(Link inLink)
        {
            _name = inLink.Name;
            IsNew = inLink.IsNew;

            _source = inLink.Source;
            _target = inLink.Target;

            CopyCustomFields(inLink, true);
        }

        /// <summary>
        /// Virtual method called used by inherited classes to copy their added members
        /// <seealso cref="TK_OSCARLib.CG_Constraint">
        /// <seealso cref="TK_OSCARLib.CG_Expression">
        /// </summary>
        protected virtual void CopyCustomFields(Link inLink, bool Resolve)
        {

        }

        /// <summary>
        /// Calculates the collision box from the two Point instances representing Source and Target
        /// </summary>
        /// <param name="point_1">Source Point Position in the Layout</param>
        /// <param name="point_2">Target Point Position in the Layout</param>
        /// <param name="LayoutSize">Size of the Layout</param>
        internal void SetPolygon(Point point_1, Point point_2, double LayoutSize)
        {
            double hyp = Math.Sqrt(Math.Pow(point_2.Y - point_1.Y, 2) + Math.Pow(point_1.X - point_2.X, 2));
            double angle = Math.Acos(Math.Abs(point_2.Y - point_1.Y) / hyp) + Math.PI / 2;

            _mean.X = (int)((point_1.X + point_2.X) / 2.0);
            _mean.Y = (int)((point_1.Y + point_2.Y) / 2.0);

            float XOffset = 25f;
            float YOffset = 20f;

            //selection polygon
            if (Target.Owner == Source.Owner)//Cycling
            {
                polygon[0].X = (int)(point_1.X + XOffset * LayoutSize);
                polygon[0].Y = (int)(point_1.Y + (YOffset + 5) * LayoutSize);

                polygon[1].X = (int)(point_1.X + XOffset * LayoutSize);
                polygon[1].Y = (int)(point_1.Y + (YOffset - 9) * LayoutSize);

                polygon[2].X = point_1.X;
                polygon[2].Y = (int)(point_1.Y - 9 * LayoutSize);

                polygon[3].X = _mean.X;
                polygon[3].Y = _mean.Y;

                polygon[4].X = _mean.X;
                polygon[4].Y = _mean.Y;

                //RealCenter Upper
                polygon[5].X = _mean.X;
                polygon[5].Y = _mean.Y;

                polygon[6].X = _mean.X;
                polygon[6].Y = _mean.Y;

                polygon[7].X = _mean.X;
                polygon[7].Y = _mean.Y;

                polygon[8].X = point_2.X;
                polygon[8].Y = (int)(point_2.Y - 9 * LayoutSize);

                polygon[9].X = (int)(point_2.X - XOffset * LayoutSize); 
                polygon[9].Y = (int)(point_2.Y + (YOffset - 9) * LayoutSize);

                polygon[10].X = (int)(point_2.X - XOffset * LayoutSize); 
                polygon[10].Y = (int)(point_2.Y + (YOffset + 5) * LayoutSize);

                polygon[11].X = point_2.X;
                polygon[11].Y = (int)(point_2.Y + 5 * LayoutSize);

                polygon[12].X = _mean.X;
                polygon[12].Y = _mean.Y;

                polygon[13].X = _mean.X;
                polygon[13].Y = _mean.Y;

                //RealCenter Lower
                polygon[14].X = _mean.X;
                polygon[14].Y = _mean.Y;

                polygon[15].X = _mean.X;
                polygon[15].Y = _mean.Y;

                polygon[16].X = _mean.X;
                polygon[16].Y = _mean.Y;

                polygon[17].X = point_1.X;
                polygon[17].Y = (int)(point_1.Y + 5 * LayoutSize);
            }
            else
            {
                if (point_2.Y < point_1.Y)
                {
                    angle = -angle;
                }

                if (point_2.X < point_1.X)
                {
                    angle = Math.PI - angle;
                }

                _point_1_1.X = (int)((6 * point_1.X + point_2.X) / 7.0);
                _point_1_1.Y = (int)((59 * point_1.Y + point_2.Y) / 60);

                _point_1_2.X = (int)((2 * point_1.X + point_2.X) / 3.0);
                _point_1_2.Y = (int)((9 * point_1.Y + point_2.Y) / 10);

                _point_1_3.X = (int)((1.15 * point_1.X + point_2.X) / 2.15);
                _point_1_3.Y = (int)((2 * point_1.Y + point_2.Y) / 3);

                _point_2_3.X = (int)((1.15 * point_2.X + point_1.X) / 2.15);
                _point_2_3.Y = (int)((2 * point_2.Y + point_1.Y) / 3);

                _point_2_2.X = (int)((2 * point_2.X + point_1.X) / 3.0);
                _point_2_2.Y = (int)((9 * point_2.Y + point_1.Y) / 10);

                _point_2_1.X = (int)((6 * point_2.X + point_1.X) / 7.0);
                _point_2_1.Y = (int)((59 * point_2.Y + point_1.Y) / 60);

                int Xoffset = (int)(Math.Sin(angle) * 7 * LayoutSize);
                int Yoffset = (int)(-Math.Cos(angle) * 7 * LayoutSize);

                polygon[0].X = point_1.X - Xoffset;
                polygon[0].Y = point_1.Y + Yoffset;

                polygon[1].X = point_1.X + Xoffset;
                polygon[1].Y = point_1.Y - Yoffset;

                polygon[2].X = _point_1_1.X + Xoffset;
                polygon[2].Y = _point_1_1.Y - Yoffset;

                polygon[3].X = _point_1_2.X + Xoffset;
                polygon[3].Y = _point_1_2.Y - Yoffset;

                polygon[4].X = _point_1_3.X + Xoffset;
                polygon[4].Y = _point_1_3.Y - Yoffset;

                polygon[5].X = _mean.X + Xoffset;
                polygon[5].Y = _mean.Y - Yoffset;

                polygon[6].X = _point_2_3.X + Xoffset;
                polygon[6].Y = _point_2_3.Y - Yoffset;

                polygon[7].X = _point_2_2.X + Xoffset;
                polygon[7].Y = _point_2_2.Y - Yoffset;

                polygon[8].X = _point_2_1.X + Xoffset;
                polygon[8].Y = _point_2_1.Y - Yoffset;

                polygon[9].X = point_2.X + Xoffset;
                polygon[9].Y = point_2.Y - Yoffset;

                polygon[10].X = point_2.X - Xoffset;
                polygon[10].Y = point_2.Y + Yoffset;

                polygon[11].X = _point_2_1.X - Xoffset;
                polygon[11].Y = _point_2_1.Y + Yoffset;

                polygon[12].X = _point_2_2.X - Xoffset;
                polygon[12].Y = _point_2_2.Y + Yoffset;

                polygon[13].X = _point_2_3.X - Xoffset;
                polygon[13].Y = _point_2_3.Y + Yoffset;

                polygon[14].X = _mean.X - Xoffset;
                polygon[14].Y = _mean.Y + Yoffset;

                polygon[15].X = _point_1_3.X - Xoffset;
                polygon[15].Y = _point_1_3.Y + Yoffset;

                polygon[16].X = _point_1_2.X - Xoffset;
                polygon[16].Y = _point_1_2.Y + Yoffset;

                polygon[17].X = _point_1_1.X - Xoffset;
                polygon[17].Y = _point_1_1.Y + Yoffset;
            }
        }

        /// <summary>
        /// Indicates if a Point is contained into the collision box
        /// </summary>
        /// <param name="point">The Point to be tested (typically the Mouse Position at clicking)</param>
        /// <returns>true if the Point is in the collision box, false otherwise</returns>
        internal bool Contains(Point point)
        {
            return PointInPolygon(point, polygon);
        }

        /// <summary>
        /// Indicates if a Point is contained into the collision box
        /// </summary>
        /// <param name="point">The Point to be tested (typically the Mouse Position at clicking)</param>
        /// <param name="poly">The Polygon that could contain the Point</param>
        /// <returns>true if the Point is in the collision box, false otherwise</returns>
        bool PointInPolygon(Point p, Point[] poly)
        {
            Point p1, p2;
            bool inside = false;

            if (poly.Length < 3)
            {
                return inside;
            }

            Point oldPoint = new Point(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                Point newPoint = new Point(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)

                    && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X)

                     < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;

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

            _source = (Port)Serializer.PortSerializer.Deserialize(reader);

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

            reader.ReadEndElement();

            IsNew = false;
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

            Serializer.PortSerializer.Serialize(writer, _source);

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


        public virtual bool IsHierachicallyInteresting
        {
            get
            {
                return _isHierachicallyInteresting;
            }
            set
            {
                _isHierachicallyInteresting = value;
            }
        }

        public virtual bool IsCompatibleWith(Link outputLink)
        {
            return false;
        }
    }
}
