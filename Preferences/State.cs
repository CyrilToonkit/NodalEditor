using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Drawing.Design;

namespace TK.NodalEditor
{
    public class State
    {
        string mName;
        [DescriptionAttribute("Name of the state")]
        public string Name
        {
          get { return mName; }
          set { mName = value; }
        }

        string mIconPath;
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        [DescriptionAttribute("Path of the icon")]
        public string IconPath
        {
          get { return mIconPath; }
          set { mIconPath = value; }
        }
    }
}
