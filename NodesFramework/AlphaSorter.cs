using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor
{
    public class AlphaSorter : IComparer<PortObj>
    {
        #region IComparer<NodeBase> Members

        public int Compare(PortObj x, PortObj y)
        {
            return x.Name.CompareTo(y.Name);
        }

        #endregion
    }

    public class NodeAlphaSorter : IComparer<Node>
    {
        #region IComparer<NodeBase> Members

        public static string lastAlpha = "zzzzz";

        public int Compare(Node x, Node y)
        {
            string xName = ((x is Compound) ? NodeAlphaSorter.lastAlpha : string.Empty) + x.FullName;
            string yName = ((y is Compound) ? NodeAlphaSorter.lastAlpha : string.Empty) + y.FullName;

            return xName.CompareTo(yName);
        }

        #endregion
    }
}
