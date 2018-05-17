using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MiniLogger;

namespace TK.NodalEditor.NodesLayout
{
    public partial class TK_NodalEditorUCtrl : UserControl
    {
        public TK_NodalEditorUCtrl()
        {
            InitializeComponent();

            logUCtrl1.Bind(Logger.GetInstance());
        }

        public void Init(string inPath, NodesManager inManager)
        {
            nodesLayout1.Init(inPath, inManager, inputsPad, outputsPad, BreadCrumbs);
        }

        public new NodesLayout Layout
        {
            get { return nodesLayout1; }
        }
    }
}
