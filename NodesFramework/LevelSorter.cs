using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor.NodesFramework
{
    class LevelSorter : IComparer<Node>
    {
        /// <summary> 
        /// constructor to set the sort column and sort order. 
        /// </summary> 
        /// <param name="strMemberName"></param> 
        /// <param name="sortingOrder"></param> 
        public LevelSorter()
        {
        }

        public int Compare(Node sN1, Node sN2)
        {
            return sN1.Parent.RealDeepness.CompareTo(sN2.Parent.RealDeepness);
        }
    } 
}
