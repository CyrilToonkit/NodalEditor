using GenericUndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK.NodalEditor.NodesFramework;

namespace TK.NodalEditor
{
    abstract class NodalEditorMemento : IMemento<NodalDirector>
    {
        public abstract IMemento<NodalDirector> Restore(NodalDirector target);
    }

    class ReAddNodeMemento : NodalEditorMemento
    {
        Node node;
        Compound parent;
        NodeConnexions connections;
        public ReAddNodeMemento(Node inNode, Compound inParent, NodeConnexions inConnections)
        {
            node = inNode;
            parent = inParent;
            connections = inConnections;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new DeleteNodeMemento(node, node.Parent, new NodeConnexions(node));
            target._DeleteNode(node);
            return inverse;
        }
    }

    class DeleteNodeMemento : NodalEditorMemento
    {
        Node removed;
        Compound parent;
        NodeConnexions connections;
        public DeleteNodeMemento(Node inRemoved, Compound inParent, NodeConnexions inConnections)
        {
            removed = inRemoved;
            parent = inParent;
            connections = inConnections;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new ReAddNodeMemento(removed, parent, connections);
            target._AddNode(removed, parent, connections);
            return inverse;
        }
    }

    class AddNodeMemento : NodalEditorMemento
    {
        private string nodeName;
        public AddNodeMemento(string inName)
        {
            this.nodeName = inName;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node removed = target.manager.GetNode(nodeName);
            IMemento<NodalDirector> inverse = new DeleteNodeMemento(removed, removed.Parent, null);
            target._DeleteNode(removed);
            return inverse;
        }
    }


    class DisconnectMemento : NodalEditorMemento
    {
        Link disconnected;
        string mode = null;

        public DisconnectMemento(Link inDisconnected)
        {
            disconnected = inDisconnected;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new ConnectMemento(disconnected, mode);
            target._Connect(disconnected, mode);
            return inverse;
        }
    }

    class ConnectMemento : NodalEditorMemento
    {
        private Link link;
        private string mode;
        public ConnectMemento(Link inLink, string inMode)
        {
            this.link = inLink;
            this.mode = inMode;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            //Link disconected = target.manager.
            IMemento<NodalDirector> inverse = new DisconnectMemento(link);
            target._Disconnect(link);
            return inverse;
        }
    }

    class ReconnectMemento : NodalEditorMemento
    {
        private string nodeNameIn;
        private string nodeNameOut;

        private string PortNameIn;

        private string PortNameOut;

        private Link reconnected;
        private string Mode;
        public ReconnectMemento(string inNodeName, string outNodeName, string inPortName, string outPortName, Link inLink, string inMode)
        {
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

            
            IMemento<NodalDirector> inverse = new ReconnectMemento(reconnected.Target.Owner.FullName, reconnected.Source.Owner.FullName, reconnected.Target.FullName, reconnected.Source.FullName, reconnected, Mode);
            target._ReConnect(inNode, outNode, inPort, outPort, reconnected, Mode);
            return inverse;
        }
    }


    class CopyLinkMemento : NodalEditorMemento
    {
        private string nodeNameIn;
        private string nodeNameOut;
        private string PortNameIn;
        private string PortNameOut;
        private Link copyReconnected;
        private string Mode;
        public CopyLinkMemento(string inNodeName, string outNodeName, string inPortName, string outPortName, Link inLink, string inMode)
        {
            this.nodeNameIn = inNodeName;
            this.nodeNameOut = outNodeName;

            this.PortNameIn = inPortName;
            this.PortNameOut = outPortName;

            this.copyReconnected = inLink;
            this.Mode = inMode;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node inNode = target.manager.GetNode(nodeNameIn);
            Node outNode = target.manager.GetNode(nodeNameOut);

            int inPort = inNode.GetPort(PortNameIn, false).Index;
            int outPort = outNode.GetPort(PortNameOut, true).Index;


            IMemento<NodalDirector> inverse = new DisconnectMemento(copyReconnected);
            target._Disconnect(copyReconnected);
            return inverse;
        }
    }

    class DisconnectAllMemento : NodalEditorMemento
    {
        private Node Node;
        public DisconnectAllMemento(Node inNode)
        {
            this.Node = inNode;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new DisconnectAllMemento(Node);
            target._DisconnectAll(Node);
            return inverse;
        }
    }
    class ParentMemento : NodalEditorMemento
    {
        private string NodeName;
        private string CompoundName;
        public ParentMemento(string inNodeName, string inParentName)
        {
            this.NodeName = inNodeName;
            this.CompoundName = inParentName;

        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node Node = target.manager.GetNode(NodeName);
            Compound Compound = target.manager.GetNode(CompoundName) as Compound;
            IMemento<NodalDirector> inverse = new UnParentMemento(NodeName, CompoundName);
            target._UnParent(Node);
            return inverse;
        }
    }
    class UnParentMemento : NodalEditorMemento
    {
        private string NodeName;
        private string CompoundName;
        public UnParentMemento(string inNodeName, string inParentName)
        {
            this.NodeName = inNodeName;
            this.CompoundName = inParentName;
        }
        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            Node Node = target.manager.GetNode(NodeName);
            Compound Compound = target.manager.GetNode(CompoundName) as Compound;
            IMemento<NodalDirector> inverse = new ParentMemento(NodeName, CompoundName);
            target._Parent(Node, Compound);
            return inverse;
        }
    }

    class CreateCompoundMemento : NodalEditorMemento
    {
        private List<Node> Nodes;
        private Compound Compound;
        public CreateCompoundMemento(List<Node> inNodes, Compound inCompound)
        {
            Nodes = inNodes;
            Compound = inCompound;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            //Compound Compound = target.manager.GetNode(Nodes[0].Parent.FullName) as Compound;

            IMemento<NodalDirector> inverse = new ExplodeMemento(Compound, Nodes);
            target._Explode(Compound);
            return inverse;
        }
    }

    class ExplodeMemento : NodalEditorMemento
    {
        private Compound Compound;
        private List<Node> Nodes;
        public ExplodeMemento(Compound inCompound, List<Node> inNodes)
        {
            this.Compound = inCompound;
            this.Nodes = inNodes;
        }

        public override IMemento<NodalDirector> Restore(NodalDirector target)
        {
            IMemento<NodalDirector> inverse = new CreateCompoundMemento(Nodes, Compound);
            target._CreateCompound(Nodes, Compound);
            return inverse;
        }
    }
}
