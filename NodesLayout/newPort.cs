using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TK.BaseLib;
using TK.GraphComponents.Dialogs;

namespace TK.NodalEditor.NodesLayout
{
    public partial class newPort : Form
    {
        public newPort()
        {
            InitializeComponent();
            TypeTB.SelectedIndexChanged += new EventHandler(TypeTB_SelectedIndexChanged);
        }

        void TypeTB_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshCustomParams((string)TypeTB.SelectedItem);
        }

        Node node;
        Port port;
        object[] customParams;
        public object[] CustomParams
        {
            get { return customParams; }
            set { customParams = value; }
        }

        List<string> mTypeMetaData = new List<string>();
        public List<string> TypeMetaData
        {
            get { return mTypeMetaData; }
            set { mTypeMetaData = value; }
        }

        List<string> ParamNames;

        public string PortName
        {
            get { return NameTB.Text; }
            set { NameTB.Text = value; }
        }

        public string PortType
        {
            get { return (string)TypeTB.SelectedItem; }
            set { TypeTB.SelectedItem = value; }
        }

        public DialogResult ShowDialog(string name, Node inNode)
        {
            return ShowDialog(name, inNode, null);
        }

        public DialogResult ShowDialog(string name, Node inNode, Port inPort)
        {
            List<string> list = inNode.GetPortTypes();
            node = inNode;
            port = inPort;
            string val = "";
            if (port != null)
            {
                val = port.NodeElementType;
            }

            PortName = name;
            TypeTB.Items.Clear();
            int sel = 0;
            int cnt = 0;

            foreach (string sItem in list)
            {
                if (sItem == val)
                {
                    sel = cnt;
                }
                
                TypeTB.Items.Add(sItem);

                cnt++;
            }

            if (port != null)
            {
                TypeTB.Enabled = false;
            }
            else
            {
                TypeTB.Enabled = true;
            }

            TypeTB.SelectedIndex = sel;
            //RefreshCustomParams((string)TypeTB.SelectedItem);
            DialogResult = DialogResult.No;
            return ShowDialog();
        }

        private void RefreshCustomParams(string type)
        {
            List<Control> toRemove = new List<Control>();

            foreach (Control ctrl in Controls)
            {
                if (ctrl.Name.StartsWith("customContainer_"))
                {
                    toRemove.Add(ctrl);
                }
            }

            foreach (Control ctrl in toRemove)
            {
                Controls.Remove(ctrl);
            }

            //Controls.Clear();

            //Controls.Add(NameTB);
            //Controls.Add(TypeTB);
            //Controls.Add(label1);
            //Controls.Add(label2);
            //Controls.Add(tableLayoutPanel1);

            int offset = 110;

            customParams = port == null ? node.GetPortParams(type) : port.GetPortParams();
            ParamNames = node.GetPortParamNames(type);

            int count = 0;
            foreach (object obj in customParams)
            {
                Label label = new Label();
                label.Text = ParamNames[count] + " :";
                
                Control newCtrl = null;
                if (obj is Enum) // ENUM
                {
                    ListBox lb = new ListBox();
                    lb.Tag = count;
                    lb.SelectedValueChanged += new EventHandler(lb_SelectedValueChanged);
                    foreach (string value in Enum.GetNames(obj.GetType()))
                    {
                        lb.Items.Add(value);
                    }

                    lb.SelectedItem = obj.ToString();

                    lb.Height = 40;
                    newCtrl = lb;
                }
                else 
                {
                    if (obj is bool) // BOOL
                    {
                        CheckBox cb = new CheckBox();
                        cb.Tag = count;
                        cb.CheckedChanged += new EventHandler(cb_CheckedChanged);
                        cb.Checked = (bool)obj;

                        cb.Height = 18;

                        newCtrl = cb;
                    }
                }

                Panel container = new Panel();
                container.Height = newCtrl.Height + 4;

                container.Name = "customContainer_" + ParamNames[count];

                container.Controls.Add(label);
                label.Dock = DockStyle.Left;

                container.Controls.Add(newCtrl);
                newCtrl.Dock = DockStyle.Fill;
                newCtrl.BringToFront();

                Controls.Add(container);
                container.Dock = DockStyle.Bottom;
                container.BringToFront();

                offset += container.Height;

                count++;
            }

            Height = offset + 45;
        }

        void cb_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            customParams[(int)cb.Tag] = cb.Checked;
        }

        void lb_SelectedValueChanged(object sender, EventArgs e)
        {
            ListBox lb = sender as ListBox;
            customParams[(int)lb.Tag] = Enum.Parse(customParams[(int)lb.Tag].GetType(), (string)lb.SelectedItem);
            enumValuesPanel.Visible = (string)lb.SelectedItem == "Enum";
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (enumValuesPanel.Visible && string.IsNullOrEmpty(enumValuesTB.Text))
            {
                TKMessageBox.ShowError("Please give at least one enum value !", "Parameter type incomplete");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void enumValuesTB_TextChanged(object sender, EventArgs e)
        {
            mTypeMetaData = TypesHelper.StringSplit(enumValuesTB.Text);
        }
    }
}
