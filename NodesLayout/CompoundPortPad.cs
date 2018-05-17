using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace TK.NodalEditor.NodesLayout
{
    /// <summary>
    /// UserControl used to display Compound ports. It is used twice, one for Inputs (default) and one for Outputs, if "righty" is set to true.
    /// </summary>
    public partial class CompoundPortPad : UserControl
    {
        const int MINWIDTH = 50;
        const int ICONSIZE = 28;

        int portCount = 0;

        NodesLayout nodesLayout;
        Graphics measurer;

        Bitmap DeleteIcon;

        public Compound node;

        bool IsInitialised = false;

        public List<Port> Ports
        {
            get { return (righty ? node.Outputs : node.Inputs); }   
        }

        public CompoundPortPad()
        {
            InitializeComponent();
            DoubleBuffered = true;
            DeleteIcon = global::TK.NodalEditor.Properties.Resources.Hide;

            measurer = this.CreateGraphics();
        }

        public void Init(NodesLayout inLayout, bool OutputPorts)
        {
            nodesLayout = inLayout;
            righty = OutputPorts;
        }

        public void SetPorts(Compound inCtrl)
        {
            node = inCtrl;
            RefreshWidth();
            IsInitialised = true;
        }

        public void RefreshWidth()
        {
            if (Visible && node != null)
            {
                int minWidth = 0;
                int newWidth = 0;

                int counter = 0;

                foreach (Port port in Ports)
                {
                    if (port.Visible)
                    {
                        newWidth = (int)Math.Ceiling(measurer.MeasureString(port.FullName, nodesLayout.Preferences.CompoundPadFont).Width);
                        if (newWidth > minWidth)
                        {
                            minWidth = newWidth;
                        }

                        counter++;
                    }
                }

                Width = Math.Max(MINWIDTH, minWidth + ICONSIZE + 12);

                if (righty)
                {
                    Location = new Point(Parent.Width - Width, 0);
                }

                Invalidate();
            }
        }

        public bool righty = false;

        private void CompoundPortPad_ParentChanged(object sender, EventArgs e)
        {
            Parent.Resize += new EventHandler(Parent_Resize);
        }

        void Parent_Resize(object sender, EventArgs e)
        {
            if (righty)
            {
                Location = new Point(Parent.Width - Width, 0);
            }

            Height = Parent.Height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            portCount = 0;

            if (IsInitialised)
            {
                e.Graphics.Clear(nodesLayout.Preferences.CompoundPadColor);

                int Offset = 21;
                int LeftOffset = (righty ? 1 : Width - 8);
                int RightOffset = 10;

                e.Graphics.DrawRectangle(nodesLayout.FramePen, 0, 0, Width - 1, Height - 1);

                foreach (Port port in Ports)
                {
                    if (port.Visible)
                    {
                        portCount++;
                        if (!righty)
                        {
                            RightOffset = (int)(Width - 10 - e.Graphics.MeasureString(port.FullName, nodesLayout.Preferences.CompoundPadFont).Width);
                        }

                        e.Graphics.DrawImage(DeleteIcon, righty ? Width - ICONSIZE : 0, Offset - 8, ICONSIZE, ICONSIZE);
                        e.Graphics.DrawString(port.FullName, nodesLayout.Preferences.CompoundPadFont, nodesLayout.CompoundPadBrush, new PointF(RightOffset, Offset));
                        nodesLayout.DrawPortPlug(e.Graphics, nodesLayout.GetBrush(port.NodeElementType), LeftOffset, Offset + 2, 6);
                        Offset += (int)(20);
                    }
                }
            }
        }

        private void CompoundPortPad_MouseUp(object sender, MouseEventArgs e)
        {
            Port portData = GetPortClick(e.Location);

            if (portData != null)
            {
                if ((righty ? e.X > Width - ICONSIZE : e.X < ICONSIZE))
                {
                    if (!(portData as PortInstance).IsLinked())
                    {
                        portData.Visible = false;
                        RefreshWidth();
                    }
                    else
                    {
                        nodesLayout.log.AddLog("Cannot hide a linked port !", 10, 2);
                        nodesLayout.Invalidate();
                    }
                }
                else
                {
                    EditGroupBox.Tag = portData;
                    EditGroupBox.Text = portData.NativeName;
                    RenameTB.Text = portData.Name;
                    EditGroupBox.Width = Width - 2;
                    EditGroupBox.Location = new Point(1, 20 + portData.DisplayIndex * 20);

                    EditGroupBox.Visible = true;
                    RenameTB.Focus();
                }
            }
        }

        public Port GetPortClick(Point hitPoint)
        {
            if (new Rectangle(0, 20, Width, Height).Contains(hitPoint))
            {
                double rawIndex = ((double)hitPoint.Y - 20.0) / (20.0);

                if (rawIndex > 0 && rawIndex < portCount)
                {
                    int index = (int)Math.Floor(rawIndex);

                    int custDisplayIndex = 0;
                    foreach (Port curPort in Ports)
                    {
                        if (curPort.Visible)
                        {
                            if (index == custDisplayIndex)
                            {
                                return curPort;
                            }

                            custDisplayIndex++;
                        }
                    }
                }
            }

            return null;
        }

        internal void ExposePort(Port inPort)
        {
            Port port = GetPort(inPort);
            port.Visible = true;
            RefreshWidth();
        }

        public Port GetPort(Port inPort)
        {
            return node.GetPortFromNode(inPort);
        }

        private void RenameTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == char.Parse("\r"))
            {
                if (RenameTB.Visible == true)
                {
                    if (nodesLayout.IsValidName(RenameTB.Text))
                    {
                        Port data = EditGroupBox.Tag as Port;

                        if (data != null)
                        {
                            data.Name = RenameTB.Text;
                            RefreshWidth();
                            nodesLayout.RefreshPorts();
                            nodesLayout.Invalidate();
                            EditGroupBox.Tag = null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Sorry but " + RenameTB.Text + " is not a valid name !", "Error");
                    }
                }

                EditGroupBox.Visible = false;
            }
        }

        private void RenameTB_Leave(object sender, EventArgs e)
        {
            if (!DefaultButton.Focused && !CancelButton.Focused)
            {
                EditGroupBox.Visible = false;
            }
        }

        private void DefaultButton_Click(object sender, EventArgs e)
        {
            if (EditGroupBox.Visible == true)
            {
                Port data = EditGroupBox.Tag as Port;

                if (data != null)
                {
                    data.Name = data.NativeName;
                    RefreshWidth();
                    nodesLayout.RefreshPorts();
                    nodesLayout.Invalidate();
                    EditGroupBox.Tag = null;
                }
                EditGroupBox.Visible = false;
            }
            
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            EditGroupBox.Visible = false;
        }
    }
}
