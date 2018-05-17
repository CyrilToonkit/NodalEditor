using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TK.NodalEditor.NodesLayout
{
    public enum Interactions
    {
        Connect, Select, 
    }

    public enum Operations
    {
        NodesRemoved, NodeAdded, NodeRenamed, NodesMoved
    }

    public delegate void InteractionEventHandler(object sender, InteractionEventArgs e);

    public class InteractionEventArgs : EventArgs
    {
        public InteractionEventArgs(MouseButtons inbuttons ,  Keys inkey)
        {
            buttons = inbuttons;
            key = inkey;
        }

        public MouseButtons buttons;
        public Keys key;
    }

    public delegate void NodesChangedEventHandler(object sender, NodesChangedEventArgs e);

    public class NodesChangedEventArgs : EventArgs
    {
        public NodesChangedEventArgs(Operations inOp, List<Node> inNodes)
        {
            Operation = inOp;
            Nodes = inNodes;
        }

        public NodesChangedEventArgs(Operations inOp, List<Node> inNodes, List<object> inOldValues)
        {
            Operation = inOp;
            Nodes = inNodes;
            oldValues = inOldValues;
        }

        public Operations Operation;
        public List<Node> Nodes;
        public List<object> oldValues = new List<object>();
    }

    public delegate void PortClickEventHandler(object sender, PortClickEventArgs e);

    public class PortClickEventArgs : EventArgs
    {
        public PortClickEventArgs(int inIndex, bool inIsOutput)
        {
            Index = inIndex;
            IsOutput = inIsOutput;
        }

        public int Index;
        public bool IsOutput;
    }

    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);

    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(List<NodeBase> newSelection)
        {
            Selection = newSelection;
        }

        public List<NodeBase> Selection;
    }

    public delegate void LinkSelectionChangedEventHandler(object sender, LinkSelectionChangedEventArgs e);

    public class LinkSelectionChangedEventArgs : EventArgs
    {
        public LinkSelectionChangedEventArgs(Link newSelection)
        {
            Selection = newSelection;
        }

        public Link Selection;
    }

    public delegate void FocusChangedEventHandler(object sender, FocusChangedEventArgs e);

    public class FocusChangedEventArgs : EventArgs
    {
        public FocusChangedEventArgs(List<Compound> inBreadCrumbs)
        {
            BreadCrumbs = inBreadCrumbs;
        }

        public List<Compound> BreadCrumbs;
    }
}
