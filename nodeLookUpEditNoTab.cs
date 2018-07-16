using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TK.NodalEditor
{
    public partial class nodeLookUpEditNoTab : LookUpEdit
    {
        public nodeLookUpEditNoTab()
        {
            InitializeComponent();
        }

        public nodeLookUpEditNoTab(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        protected override bool IsInputKey(Keys KeyData)
        {
            if (KeyData == Keys.Tab)
            {
                return true;
            }

            return base.IsInputKey(KeyData);
        }
    }
}
