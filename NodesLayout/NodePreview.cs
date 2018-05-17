using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TK.NodalEditor.NodesLayout
{
    public class NodePreview : Panel
    {
        public NodePreview()
            : base()
        {
        }

        public NodePreview(NodesLayout inlayout, Node innodePreview) : base()
        {
            Set(inlayout, innodePreview);
        }

        public void Set(NodesLayout nodesLayout, Node node)
        {
            layout = nodesLayout;
            nodePreview = node;
            Width = node.UIWidth;
            Height = node.UIHeight;

            set = true;
        }

        bool set = false;
        NodesLayout layout;
        Node nodePreview;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (set)
            {
                layout.DrawNode(e.Graphics, nodePreview, 0, 0, 1);
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}
