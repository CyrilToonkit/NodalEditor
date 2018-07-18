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
}
