using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.ComponentModel;
using TK.NodalEditor.NodesLayout;

namespace TK.NodalEditor
{
    public class LinkState
    {
        public LinkState()
        {
        }

        public LinkState(string inName, LinksArrows inStartArrow, LinksArrows inEndArrow)
        {
            mName = inName;
            mStartArrow = inStartArrow;
            mEndArrow = inEndArrow;
        }
        
        string mName = "Default";
        public string Name
        {
            get { return mName; }
            set { mName = value;}
        }

        LinksArrows mStartArrow = LinksArrows.None;
        public LinksArrows StartArrow
        {
            get { return mStartArrow; }
            set { mStartArrow = value;}
        }

        LinksArrows mEndArrow = LinksArrows.SharpArrow;
        public LinksArrows EndArrow
        {
            get { return mEndArrow; }
            set { mEndArrow = value;}
        }
    }
}
