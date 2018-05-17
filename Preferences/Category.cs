using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using TK.NodalEditor.NodesLayout;

namespace TK.NodalEditor
{
    public class Category
    {
        public Category()
        {
        }

        public Category(string inName, Color inColor)
        {
            mName = inName;
            mColor = Color;
        }
        
        string mName = "Default";
        public string Name
        {
            get { return mName; }
            set { mName = value;}
        }

        bool mVisible = true;
        public bool Visible
        {
            get { return mVisible; }
            set { mVisible = value; }
        }

        protected Color mColor = Color.Red;
        [XmlIgnore]
        public Color Color
        {
            get { return mColor; }
            set { mColor = value; }
        }

        protected bool mDashed = false;
        public bool Dashed
        {
            get { return mDashed; }
            set { mDashed = value; }
        }

        [XmlElement("Color")]
        [BrowsableAttribute(false)]
        public string mXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(Color); }
            set { mColor = NodalEditorPreferences.DeserializeColor(value); }
        }
    }
}
