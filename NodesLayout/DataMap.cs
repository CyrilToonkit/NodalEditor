using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TK.GraphComponents;

namespace TK.NodalEditor.NodesLayout
{
    public partial class DataMap : Form
    {
        public DataMap()
        {
            InitializeComponent();
        }

        public void Init(string Caption, List<string> OldValues, List<string> RemapValues)
        {
            Text = Caption;

            this.panelLeft.Controls.Clear();
            this.panelRight.Controls.Clear();
            for (int i = 0; i < OldValues.Count; i++)
            {
                TKDropDown box = getCombo(OldValues, i, false);
                this.panelLeft.Controls.Add(box);
                refs.Add(box);
                box.Dock = DockStyle.Top;
                box.BringToFront();
            }

            for (int i = 0; i < RemapValues.Count; i++)
            {
                TKDropDown box = getCombo(RemapValues, 0, true);
                this.panelRight.Controls.Add(box);
                maps.Add(box);
                box.Dock = DockStyle.Top;
                box.BringToFront();
            }
        }

        List<TKDropDown> refs = new List<TKDropDown>();
        List<TKDropDown> maps = new List<TKDropDown>();

        TKDropDown getCombo(List<string> ports, int selected, bool AddDoNothing)
        {
            TKDropDown box = new TKDropDown();

            List<object> items = new List<object>();

            if (AddDoNothing)
            {
                items.Add("Do nothing");
            }

            foreach (string port in ports)
            {
                if (!items.Contains(port))
                {
                    items.Add(port);
                }
            }

            box.Items = items;
            box.SelectedIndex = selected;

            return box;
        }

        public Dictionary<string,List<string>> getMappings()
        {
            Dictionary<string,List<string>> mappings = new Dictionary<string,List<string>>();

            int cnt = 0;
            foreach (TKDropDown ctrl in refs)
            {
                if ((string)maps[cnt].SelectedItem != "Do nothing")
                {
                    if(mappings.ContainsKey((string)ctrl.SelectedItem))
                    {
                        mappings[(string)ctrl.SelectedItem].Add((string)maps[cnt].SelectedItem);
                    }
                    else
                    {
                        List<string> vls = new List<string>();
                        vls.Add((string)maps[cnt].SelectedItem);
                        mappings.Add((string)ctrl.SelectedItem, vls);
                    }
                }

                cnt++;
            }

            return mappings;
        }

        private void OK_BT_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
