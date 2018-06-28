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

        public override string NodeElementType
        {
            get
            {
                return "CustomNode";
            }
        }

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

        public override void CopyCustomFields(Node inNode, bool Resolve)
        {
            CustomNode node = inNode as CustomNode;

            CustomText = node.CustomText;
        }

        public override string ToString()
        {
            return CustomText;
        }
    }
}
