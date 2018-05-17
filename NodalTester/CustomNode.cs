using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK.NodalEditor;
using System.Xml;
using System.Xml.Serialization;

namespace NodalTester
{
    [XmlRoot("Node")]
    public class CustomNode : Node
    {
        public string CustomText;

        protected override List<CustomField> GetCustomFields()
        {
            List<CustomField> Fields = new List<CustomField>();

            Fields.Add(new CustomField("CustomText", CustomText));

            return Fields;
        }

        protected override void DeserializeCustomField(XmlReader reader, string inName)
        {
            switch (inName)
            {
                case "CustomText" :
                    CustomText = reader.ReadElementString();
                    break;

                default :
                    reader.Read();
                    break;
            }
        }

        public override string ToString()
        {
            return "Coucou";
        }
    }
}
