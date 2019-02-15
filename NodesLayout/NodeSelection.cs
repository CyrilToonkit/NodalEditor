using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace TK.NodalEditor.NodesLayout
{
    public class SelectionManager
    {
        //Selection Management
        NodesManager Manager;
 
        public SelectionManager(NodesManager inManager)
        {
            Manager = inManager;

            if (Selection.Count > 0)
            {
                DeselectAll();
            }
        }

        List<NodeBase> mSelection = new List<NodeBase>();
        public List<NodeBase> Selection
        {
            get { return mSelection; }
            set { mSelection = value; }
        }

        List<Link> SelectedLinks = new List<Link>();

        public void UpdateSelection()
        {
            PutToFalse();
            foreach (NodeBase CurItem in Selection)
            {
                if (CurItem != null)
                {
                    CurItem.Selected = true;
                }
            }
        }

        internal void SelectAll()
        {
            Selection.Clear();
            foreach (NodeBase CurCtrl in Manager.CurCompound.Nodes)
            {
                Selection.Add(CurCtrl);
                CurCtrl.Selected = true;
            }
        }

        public void PutToFalse()
        {
            foreach (NodeBase CurItem in Manager.CurCompound.Nodes)
            {
                CurItem.Selected = false;
            }
        }

        public void DeselectAll()
        {
            foreach (NodeBase CurItem in Manager.CurCompound.Nodes)
            {
                CurItem.Selected = false;
            }
            Selection.Clear();
        }

        public void Select(NodeBase CurGroup)
        {
            if (Selection.Count > 0)
                Selection.Clear();

            AddToSelection(CurGroup);
        }

        internal void Select(Rectangle rectangle, double Size, Keys ModifierKeys)
        {
            List<Node> SelNodes = GetRectangleNodes(rectangle, Size);
            Modify(SelNodes, ModifierKeys);
        }

        internal List<Node> GetRectangleNodes(Rectangle rectangle, double Size)
        {
            List<Node> AllNodes = Manager.CurCompound.Nodes;
            List<Node> SelNodes = new List<Node>();

            foreach (Node CurItem in AllNodes)
            {
                if (rectangle.IntersectsWith(new Rectangle(new Point((int)(CurItem.UIx * Size), (int)(CurItem.UIy * Size)), new Size((int)(CurItem.UIWidth * Size), (int)(CurItem.UIHeight * Size)))))
                {
                    SelNodes.Add(CurItem);
                }
            }

            return SelNodes;
        }


        public void Select(List<Node> Ctrls)
        {
            Selection.Clear();

            foreach (Node elem in Ctrls)
            {
                Selection.Add(elem as NodeBase);
            }

            UpdateSelection();
        }

        public void AddToSelection(NodeBase CurItem)
        {
            Selection.Add(CurItem);
            UpdateSelection();
        }

        public void ToggleSelection(NodeBase CurItem)
        {
            if (CurItem.Selected)
                Selection.Remove(CurItem);
            else
                Selection.Add(CurItem);

            UpdateSelection();
        }

        public void RemoveFromSelection(NodeBase CurItem)
        {
            Selection.Remove(CurItem);
            UpdateSelection();
        }

        public List<Node> GetSelectedNodes()
        {
            List<Node> nodeSel = new List<Node>();

            foreach (NodeBase elem in Selection)
            {
                if (elem is Node)
                {
                    nodeSel.Add(elem as Node);
                }
            }

            return nodeSel;
        }

        internal void Modify(Node Node, Keys ModifierKeys)
        {
            List<string> nodesName = new List<string> { Node.FullName };
            switch (ModifierKeys)
            {
                case Keys.Shift:
                    //AddToSelection(Node);
                    NodalDirector.SelectNodes(nodesName, "Add");
                    break;

                case Keys.Control:
                    //ToggleSelection(Node);
                    NodalDirector.SelectNodes(nodesName, "Toggle");
                    break;
                case Keys.Alt:
                    //RemoveFromSelection(Node);
                    NodalDirector.SelectNodes(nodesName, "RemoveFrom");
                    break;
                default:
                    //Select(Node);
                    NodalDirector.SelectNodes(nodesName, "Default");
                    break;
            }
        }

        internal void Modify(List<Node> Nodes, Keys ModifierKeys)
        {
            List<string> nodesName = new List<string>();
            foreach (Node node in Nodes)
            {
                nodesName.Add(node.FullName);
            }
            switch (ModifierKeys)
            {
                case Keys.Shift:
                    //foreach (Node Node in Nodes)
                    //{
                    //    AddToSelection(Node);
                    //}
                    NodalDirector.SelectNodes(nodesName, "Add");
                    break;

                case Keys.Control:
                    //foreach (Node Node in Nodes)
                    //{
                    //    ToggleSelection(Node);
                    //}
                    NodalDirector.SelectNodes(nodesName, "Toggle");
                    break;
                case Keys.Alt:
                    //foreach (Node Node in Nodes)
                    //{
                    //    RemoveFromSelection(Node);
                    //}
                    NodalDirector.SelectNodes(nodesName, "RemoveFrom");
                    break;
                default:
                    //Select(Nodes);
                    NodalDirector.SelectNodes(nodesName, "Default");
                    break;
            }
        }

        public Node GetSelectedNode()
        {
            List<Node> nodes = GetSelectedNodes();
            return nodes.Count > 0 ? GetSelectedNodes()[0] : null;
        }

        public void DeselectLinks()
        {
            foreach (Link link in SelectedLinks)
            {
                link.Selected = false;
            }

            SelectedLinks.Clear();
        }

        public void SelectLink(Link HitLink)
        {
            DeselectLinks();

            SelectedLinks.Add(HitLink);
            HitLink.Selected = true;
        }

        public List<Link> GetSelectedLinks()
        {
            return SelectedLinks;
        }
    }
}