using GenericUndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK.NodalEditor.NodesFramework;

namespace TK.NodalEditor
{
    public abstract class NodalEditorMemento : IMemento<NodalDirector>
    {
        private string _name;// = "GenericMemento";

        public abstract IMemento<NodalDirector> Restore(NodalDirector target);
        public virtual string GetName()
        {
            return _name;
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

    }

    public class ReAddNodeMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        Node node;
        Compound parent;
        NodeConnexions connections;
        int XOffset, YOffset;
        public ReAddNodeMemento(string inNameFct, string inInverseNameFct, Node inNode, Compound inParent, NodeConnexions inConnections, int inXOffset, int inYOffset)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            node = inNode;
            parent = inParent;
            connections = inConnections;
            XOffset = inXOffset;
            YOffset = inYOffset;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new DeleteNodeMemento(inverse_name_fct, name_fct, node, node.Parent, new NodeConnexions(node), XOffset, YOffset);
            target._DeleteNode(node);
            return inverse;
        }
    }

    public class DeleteNodeMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        Node removed;
        Compound parent;
        NodeConnexions connections;
        int XOffset, YOffset;
        public DeleteNodeMemento(string inNameFct, string inInverseNameFct, Node inRemoved, Compound inParent, NodeConnexions inConnections, int inXOffset, int inYOffset)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            removed = inRemoved;
            parent = inParent;
            connections = inConnections;
            XOffset = inXOffset;
            YOffset = inYOffset;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new ReAddNodeMemento(inverse_name_fct, name_fct, removed, parent, connections, 0, 0);
            target._AddNode(removed, parent, connections, 0, 0);
            return inverse;
        }
    }

    public class AddNodeMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private string nodeName;
        public AddNodeMemento(string inNameFct, string inInverseNameFct, string inName)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.nodeName = inName;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node removed = target.manager.GetNode(nodeName);
            IMemento<NodalDirector> inverse = new DeleteNodeMemento(inverse_name_fct, name_fct, removed, removed.Parent, null, 0, 0);
            target._DeleteNode(removed);
            return inverse;
        }
    }


    public class DisconnectMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        Link disconnected;
        string mode = null;

        public DisconnectMemento(string inNameFct, string inInverseNameFct, Link inDisconnected)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            disconnected = inDisconnected;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new ConnectMemento(inverse_name_fct, name_fct, disconnected, mode);
            target._Connect(disconnected, mode);
            return inverse;
        }
    }

    public class ConnectMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private Link link;
        private string mode;
        public ConnectMemento(string inNameFct, string inInverseNameFct, Link inLink, string inMode)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.link = inLink;
            this.mode = inMode;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new DisconnectMemento(inverse_name_fct, name_fct, link);
            target._Disconnect(link);
            return inverse;
        }
    }

    class ReconnectMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private string nodeNameIn;
        private string nodeNameOut;

        private string PortNameIn;

        private string PortNameOut;

        private Link reconnected;
        private string Mode;
        public ReconnectMemento(string inNameFct, string inInverseNameFct, string inNodeName, string outNodeName, string inPortName, string outPortName, Link inLink, string inMode)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.nodeNameIn = inNodeName;
            this.nodeNameOut = outNodeName;
            this.PortNameIn = inPortName;
            this.PortNameOut = outPortName;
            this.reconnected = inLink;
            this.Mode = inMode;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node inNode = target.manager.GetNode(nodeNameIn);
            Node outNode = target.manager.GetNode(nodeNameOut);
            int inPort = inNode.GetPort(PortNameIn, false).Index;
            int outPort = outNode.GetPort(PortNameOut, true).Index;

            IMemento<NodalDirector> inverse = new ReconnectMemento(inverse_name_fct, name_fct, reconnected.Target.Owner.FullName, reconnected.Source.Owner.FullName, reconnected.Target.FullName, reconnected.Source.FullName, reconnected, Mode);
            target._ReConnect(inNode, outNode, inPort, outPort, reconnected, Mode);
            return inverse;
        }
    }


    class CopyLinkMemento : NodalEditorMemento
    {
        private string name;
        private string inverse_name;
        private Link copyReconnected;

        public CopyLinkMemento(string inName, string inInverseName, Link inLink)
        {
            this.name = inName;
            this.inverse_name = inInverseName;
            this.Name = inName;
            this.copyReconnected = inLink;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new DisconnectMemento(inverse_name, name, copyReconnected);
            target._Disconnect(copyReconnected);
            return inverse;
        }
    }

    public class DeletePortMemento : NodalEditorMemento
    {
        protected string name_fct;
        protected string inverse_name_fct;
        protected Port port;
        public DeletePortMemento(string inNameFct, string inInverseNameFct, Port inPort)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.port = inPort;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
                IMemento<NodalDirector> inverse = new AddPortMemento(inverse_name_fct, name_fct, port.Owner, port);
                target._AddPort(port);
                return inverse;
        }
    }

    public class AddPortMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private Node node;
        private Port port;
        public AddPortMemento(string inNameFct, string inInverseNameFct, Node inNode, Port inPort)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.node = inNode;
            this.port = inPort;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            port = node.GetPort(port.FullName, true);
            IMemento<NodalDirector> inverse = new DeletePortMemento(inverse_name_fct, name_fct, port);
            target._DeletePort(port);
            return inverse;
        }
    }

    class ParentMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private string NodeName;
        private string CompoundName;
        public ParentMemento(string inNameFct, string inInverseNameFct, string inNodeName, string inParentName)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.NodeName = inNodeName;
            this.CompoundName = inParentName;

        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node Node = target.manager.GetNode(NodeName);
            IMemento<NodalDirector> inverse = new UnParentMemento(inverse_name_fct, name_fct, NodeName, CompoundName);
            target._UnParent(Node);
            return inverse;
        }
    }

    class UnParentMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private string NodeName;
        private string CompoundName;
        public UnParentMemento(string inNameFct, string inInverseNameFct, string inNodeName, string inParentName)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.NodeName = inNodeName;
            this.CompoundName = inParentName;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node Node = target.manager.GetNode(NodeName);
            Compound Compound = target.manager.GetNode(CompoundName) as Compound;
            IMemento<NodalDirector> inverse = new ParentMemento(inverse_name_fct, name_fct, NodeName, CompoundName);
            target._Parent(Node, Compound);
            return inverse;
        }
    }

    class CreateCompoundMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private List<Node> Nodes;
        private Compound Compound;
        public CreateCompoundMemento(string inNameFct, string inInverseNameFct, List<Node> inNodes, Compound inCompound)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            Nodes = inNodes;
            Compound = inCompound;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new ExplodeMemento(inverse_name_fct, name_fct, Compound, Nodes);
            target._Explode(Compound);
            return inverse;
        }
    }

    class ExplodeMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private Compound Compound;
        private List<Node> Nodes;
        public ExplodeMemento(string inNameFct, string inInverseNameFct, Compound inCompound, List<Node> inNodes)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.Compound = inCompound;
            this.Nodes = inNodes;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new CreateCompoundMemento(inverse_name_fct, name_fct, Nodes, Compound);
            target._CreateCompound(Nodes, Compound);
            return inverse;
        }
    }

    class RenameMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private string newName;
        private Node Node;
        private string nodeName;

        public RenameMemento(string inNameFct, string inInverseNameFct, Node inNode, string inNewName)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.newName = inNewName;
            this.Node = inNode;
            this.nodeName = inNode.FullName;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new RenameMemento(inverse_name_fct, name_fct, Node, nodeName);
            target._Rename(Node, nodeName);
            return inverse;
        }
    }

    class MoveNodeMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private Node Node;
        private int x, y;

        public MoveNodeMemento(string inNameFct, string inInverseNameFct, Node inNode)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.Node = inNode;
            this.x = (int)inNode.UIx;
            this.y = (int)inNode.UIy;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new MoveNodeMemento(inverse_name_fct, name_fct, Node);
            target._MoveNode(Node, (int)(x * target.layout.LayoutSize), (int)(y * target.layout.LayoutSize));
            return inverse;
        }
    }

    class SetPropetyMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private Node Node;
        private string propertyName;
        private object value;

        public SetPropetyMemento(string inNameFct, string inInverseNameFct, Node inNode, string inPropertyName)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.Node = inNode;
            this.propertyName = inPropertyName;
            this.value = inNode.GetType().GetProperty(inPropertyName).GetValue(inNode);
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new SetPropetyMemento(inverse_name_fct, name_fct, Node, propertyName);
            target._SetProperty(Node, propertyName, value);
            return inverse;
        }
    }

    class SetPortPropetyMemento : NodalEditorMemento
    {
        private string name_fct;
        private string inverse_name_fct;
        private Port Port;
        private string propertyName;
        private object value;

        public SetPortPropetyMemento(string inNameFct, string inInverseNameFct, Port inPort, string inPropertyName)
        {
            this.name_fct = inNameFct;
            this.inverse_name_fct = inInverseNameFct;
            this.Name = name_fct;
            this.Port = inPort;
            this.propertyName = inPropertyName;
            this.value = Port.GetType().GetProperty(inPropertyName).GetValue(Port);
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new SetPortPropetyMemento(inverse_name_fct, name_fct, Port, propertyName);
            target._SetPortProperty(Port, propertyName, value);
            return inverse;
        }
    }

    //class OverrideDisplayMemento : NodalEditorMemento
    //{
    //    private string name = "OverrideDisplay()";
    //    private string newName;
    //    private Node Node;
    //    private string nodeName;

    //    public OverrideDisplayMemento(Node inNode, string inNewName)
    //    {
    //        this.Name = name;
    //        this.newName = inNewName;
    //        this.Node = inNode;
    //        this.nodeName = inNode.FullName;
    //    }
    //    public override IMemento<NodalDirector> Restore(NodalDirector target)
    //    {
    //        IMemento<NodalDirector> inverse = new OverrideDisplayMemento(Node, nodeName);
    //        target._Rename(Node, nodeName);
    //        return inverse;
    //    }
    //}

    class SelectNodesMemento : NodalEditorMemento
    {
        private string name;
        private string inverse_name;
        private NodeBase[] nodeBase;
        private List<Node> nodes;

        public SelectNodesMemento(string inName, string inInverseName, NodeBase[] inNodesBase, List<Node> inNodes)
        {
            this.name = inName;
            this.inverse_name = inInverseName;
            this.Name = inName;
            this.nodeBase = inNodesBase;
            this.nodes = inNodes;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new DeselectNodesMemento(inverse_name, name, nodeBase, nodes);
            target._DeselectNodes(nodeBase, nodes);
            return inverse;
        }
    }

    class DeselectNodesMemento : NodalEditorMemento
    {
        private string name;
        private string inverse_name;
        private NodeBase[] nodeBase;
        private List<Node> nodes;

        public DeselectNodesMemento(string inName, string inInverseName, NodeBase[] inNodesBase, List<Node> inNodes)
        {
            this.name = inName;
            this.inverse_name = inInverseName;
            this.Name = inName;
            this.nodeBase = inNodesBase;
            this.nodes = inNodes;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new SelectNodesMemento(inverse_name, name, nodeBase, nodes);
            target._SelectNodes(nodeBase, nodes);
            return inverse;
        }
    }

}
