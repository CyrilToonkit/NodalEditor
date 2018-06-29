using System;
using System.Collections.Generic;
using System.Text;
using TK.BaseLib.CustomData;
using System.ComponentModel;
using System.IO;
using TK.BaseLib;

namespace TK.NodalEditor.NodesFramework
{
    public class NodeConnexions
    {
        /// <summary>
        /// Accessor for serializer singleton
        /// </summary>
        [BrowsableAttribute(false)]
        public NodesSerializer Serializer
        {
            get { return NodesSerializer.GetInstance(); }
        }

        public NodeConnexions()
        {
        }

        public NodeConnexions(Node inNode, bool inputs, bool outputs, string excludeType)
        {
            _nodeName = inNode.Name;
            _nodeFullName = inNode.FullName;
            _nodeNativeName = inNode.NativeName;

            if (inputs)
            {
                foreach (Link inputLink in inNode.InDependencies)
                {
                    if (linkMatches(inputLink, excludeType) && (!(inNode is Compound) || !inputLink.Source.Owner.IsIn(inNode as Compound)))
                    {
                        MemoryStream stream = new MemoryStream();

                        Serializer.LinkSerializers[inputLink.NodeElementType].Serialize(stream, inputLink);
                        string linkSerialization = TypesHelper.StringFromStream(stream);
                        stream.Close();

                        _inputs.PushItemInList(inputLink.Target.Owner.FullName + ":" + inputLink.Target.Name, linkSerialization);
                    }
                }
            }

            if (outputs)
            {
                foreach (Link outputLink in inNode.OutDependencies)
                {
                    if (linkMatches(outputLink, excludeType) && (!(inNode is Compound) || !outputLink.Target.Owner.IsIn(inNode as Compound)))
                    {
                        MemoryStream stream = new MemoryStream();

                        Serializer.LinkSerializers[outputLink.NodeElementType].Serialize(stream, outputLink);
                        string linkSerialization = TypesHelper.StringFromStream(stream);
                        stream.Close();

                        _outputs.PushItemInList(outputLink.Target.Owner.FullName + ":" + outputLink.Target.Name, linkSerialization);
                    }
                }
            }
        }

        public NodeConnexions(Node inNode) : this(inNode, "")
        {
        }

        public NodeConnexions(Node node, bool inputs, bool outputs)
            : this(node, inputs, outputs, string.Empty)
        {
        }

        public NodeConnexions(Node inNode, string excludeType)
            : this(inNode, true, true, excludeType)
        {
        }

        public bool linkMatches(Link inLink, string excludePattern)
        {
            if (inLink.NodeElementType != excludePattern)
            {
                return true;
            }

            if (excludePattern == "Deformation")
            {
                List<string> deformationExceptions = new List<string>{"GeoConstrainer"};
                return deformationExceptions.Contains(inLink.Target.Owner.NativeName);
            }

            return false;
        }

        string _nodeName = "";
        string _nodeFullName = "";
        string _nodeNativeName = "";

        KeyValuePreset _inputs = new KeyValuePreset();
        KeyValuePreset _outputs = new KeyValuePreset();

        public string NodeName
        {
            get { return _nodeName; }
            set { _nodeName = value; }
        }

        public string NodeFullName
        {
            get { return _nodeFullName; }
            set { _nodeFullName = value; }
        }

        public string NodeNativeName
        {
            get { return _nodeNativeName; }
            set { _nodeNativeName = value; }
        }

        public KeyValuePreset Inputs
        {
            get { return _inputs; }
            set { _inputs = value; }
        }

        public KeyValuePreset Outputs
        {
            get { return _outputs; }
            set { _outputs = value; }
        }

        public Link GetLink(string inLink, Node inNode)
        {
            string linkType = "";
            string linkPattern = "Link Type=\"";
            int linkTypeIndex = inLink.IndexOf(linkPattern);
            if (linkTypeIndex > -1)
            {
                int linkEndIndex = inLink.IndexOf("\"", linkTypeIndex + linkPattern.Length);
                int length = linkEndIndex - (linkTypeIndex + linkPattern.Length);
                linkType = inLink.Substring(linkTypeIndex + linkPattern.Length, length);
            }
            else
            {
                return null;
            }

            Link link = (Link)Serializer.LinkSerializers[linkType].Deserialize(TypesHelper.StringToStream(inLink));

            return link;
        }

        public string Reconnect(Node selNode, List<Node> oldSources, List<Node> newSources)
        {
            string error = "";

            Inputs.SyncDic();
            Outputs.SyncDic();

            List<Node> conNodes = new List<Node>();

            //Inputs
            foreach (string inputKey in Inputs.Keys)
            {
                string portName = inputKey.Split(':')[1];

                List<object> linksSerializations;
                object ser = Inputs.GetValue(inputKey);
                if (ser is List<object>)
                {
                    linksSerializations = (List<object>)ser;
                }
                else
                {
                    linksSerializations = new List<object> { ser };
                }

                foreach (object linkSerial in linksSerializations)
                {
                    Link inputLink = GetLink((string)linkSerial, selNode);
                    Port inputPort = selNode.GetPort(portName.Replace(NodeFullName, selNode.FullName), false);

                    if (inputPort != null)
                    {
                        Node inputNode = selNode.Companion.Manager.GetNode(inputLink.Source.Owner.FullName);

                        int index = oldSources.IndexOf(inputNode);
                        if (index != -1)
                        {
                            inputNode = newSources[index];
                        }

                        if (inputNode != null)
                        {
                            Port outputPort = inputNode.GetPort(inputLink.Source.FullName.Replace(inputLink.Source.Owner.FullName, inputNode.FullName), true);

                            if (outputPort != null)
                            {
                                //Ready to connect
                                string conError = "";
                                bool valid = true;

                                List<Link> incompatibleLinks = new List<Link>();

                                foreach (Link dep in inputPort.Dependencies)
                                {
                                    if (dep.Source == outputPort)
                                    {
                                        valid = false;
                                        error += string.Format("Link \"{0} => {1}\" already exists !\n", dep.Source.FullName, dep.Target.FullName);
                                    }
                                    else if (!dep.IsCompatibleWith(inputLink))
                                    {
                                        incompatibleLinks.Add(dep);
                                    }
                                }

                                foreach (Link incompatibleLink in incompatibleLinks)
                                {
                                    selNode.UnConnect(incompatibleLink);
                                    error += string.Format("Link \"{0} => {1}\" was not compatible and was removed !\n", incompatibleLink.Source.FullName, incompatibleLink.Target.FullName);
                                }

                                if (valid)
                                {
                                    Link newLink = selNode.Connect(inputPort.Index, inputNode, outputPort.Index, "MOCK", out conError);
                                    error += conError + (conError == string.Empty ? "" : "\n");

                                    //Respect old link values (via Copy)
                                    if (!conNodes.Contains(selNode))
                                    {
                                        conNodes.Add(selNode);
                                    }
                                    //By mocking Connect we shound be able to avoid double connection for nothing
                                    //selNode.UnConnectObject(newLink);

                                    if (newLink != null)
                                    {
                                        string oldName = newLink.Name;

                                        Port oldTarget = newLink.Target;
                                        Port oldSource = newLink.Source;

                                        newLink.Copy(inputLink);

                                        newLink.Target = oldTarget;
                                        newLink.Source = oldSource;

                                        newLink.Name = oldName;

                                        newLink.IsNew = true;
                                    }
                                }
                            }
                            else
                            {
                                error += string.Format("Can't find Port '{0}' on Node {1}\n", inputLink.Source.FullName, inputLink.Source.Owner.FullName);
                            }
                        }
                        else
                        {
                            error += string.Format("Can't find Node '{0}'\n", inputLink.Source.Owner.FullName);
                        }
                    }
                    else
                    {
                        error += string.Format("Can't find Port '{0}' on Node {1}\n", portName, selNode.FullName);
                    }
                }
            }

            //Outputs
            foreach (string outputKey in Outputs.Keys)
            {
                string nodeName = outputKey.Split(':')[0];
                string portName = outputKey.Split(':')[1];

                List<object> linksSerializations;
                object ser = Outputs.GetValue(outputKey);
                if (ser is List<object>)
                {
                    linksSerializations = (List<object>)ser;
                }
                else
                {
                    linksSerializations = new List<object> { ser };
                }

                foreach (object linksSerial in linksSerializations)
                {
                    Link outputLink = GetLink((string)linksSerial, selNode);
                    Port outputPort = selNode.GetPort(outputLink.Source.FullName.Replace(NodeFullName, selNode.FullName), true);

                    if (outputPort != null)
                    {
                        Node inputNode = selNode.Companion.Manager.GetNode(nodeName);

                        if (inputNode != null)
                        {
                            Port inputPort = inputNode.GetPort(portName, false);

                            if (inputPort != null)
                            {
                                //Ready to connect
                                string conError = "";
                                bool valid = true;
                                //Link oldCon = null;

                                List<Link> incompatibleLinks = new List<Link>();

                                foreach (Link dep in inputPort.Dependencies)
                                {
                                    if (dep.Source == outputPort)
                                    {
                                        valid = false;
                                        error += string.Format("Link \"{0} => {1}\" already exists !\n", dep.Source.FullName, dep.Target.FullName);
                                    }
                                    else if (!dep.IsCompatibleWith(outputLink))
                                    {
                                        incompatibleLinks.Add(dep);
                                    }
                                }

                                foreach (Link incompatibleLink in incompatibleLinks)
                                {
                                    inputNode.UnConnect(incompatibleLink);
                                    error += string.Format("Link \"{0} => {1}\" was not compatible and was removed !\n", incompatibleLink.Source.FullName, incompatibleLink.Target.FullName);
                                }

                                if (valid)
                                {
                                    Link newLink = inputNode.Connect(inputPort.Index, selNode, outputPort.Index, "MOCK", out conError);
                                    error += conError + (conError == string.Empty ? "" : "\n");

                                    //Respect old link values (via Copy)
                                    if (!conNodes.Contains(inputNode))
                                    {
                                        conNodes.Add(inputNode);
                                    }
                                    //By mocking Connect we shound be able to avoid double connection for nothing
                                    //inputNode.UnConnectObject(newLink);

                                    if (newLink != null)
                                    {
                                        string oldName = newLink.Name;

                                        Port oldTarget = newLink.Target;
                                        Port oldSource = newLink.Source;

                                        newLink.Copy(outputLink);

                                        newLink.Target = oldTarget;
                                        newLink.Source = oldSource;

                                        newLink.Name = oldName;

                                        newLink.IsNew = true;
                                    }
                                }
                            }
                            else
                            {
                                error += string.Format("Can't find Port '{0}' on Node {1}", portName, nodeName);
                            }
                        }
                        else
                        {
                            error += string.Format("Can't find Node '{0}'", nodeName);
                        }
                    }
                    else
                    {
                        error += string.Format("Can't find Port '{0}' on Node {1}", outputLink.Source.FullName, selNode.FullName);
                    }
                }
            }

            foreach (Node conNode in conNodes)
            {
                conNode.ReConnect();
            }

            return error;
        }

        public string Reconnect(Node selNode)
        {
            return Reconnect(selNode, new List<Node>(), new List<Node>());
        }
    }
}
