using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.ComponentModel;

namespace TK.NodalEditor
{
    public class SelectableCategory : Category
    {
        public SelectableCategory()
        {
        }

        Color mColorSelected = Color.Pink;
        [XmlIgnore]
        public Color ColorSelected
        {
            get { return mColorSelected; }
            set { mColorSelected = value;}
        }

        [XmlElement("ColorSelected")]
        [BrowsableAttribute(false)]
        public string mXmlSelectedColor
        {
            get { return NodalEditorPreferences.SerializeColor(ColorSelected); }
            set { mColorSelected = NodalEditorPreferences.DeserializeColor(value); }
        }
    }
}
