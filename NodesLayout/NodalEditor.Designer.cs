namespace TK.NodalEditor.NodesLayout
{
    partial class TK_NodalEditorUCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MiniLogger.LogPreferences logPreferences1 = new MiniLogger.LogPreferences();
            this.BreadCrumbs = new System.Windows.Forms.ToolStrip();
            this.nodesLayoutPanel = new System.Windows.Forms.Panel();
            this.collapsibleGroup1 = new TK.GraphComponents.CollapsibleGroup();
            this.logUCtrl1 = new MiniLogger.LogUCtrl();
            this.outputsPad = new TK.NodalEditor.NodesLayout.CompoundPortPad();
            this.inputsPad = new TK.NodalEditor.NodesLayout.CompoundPortPad();
            this.nodesLayout1 = new TK.NodalEditor.NodesLayout.NodesLayout();
            this.nodesLayoutPanel.SuspendLayout();
            this.collapsibleGroup1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BreadCrumbs
            // 
            this.BreadCrumbs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(112)))), ((int)(((byte)(112)))));
            this.BreadCrumbs.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.BreadCrumbs.Location = new System.Drawing.Point(0, 0);
            this.BreadCrumbs.Name = "BreadCrumbs";
            this.BreadCrumbs.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.BreadCrumbs.Size = new System.Drawing.Size(508, 25);
            this.BreadCrumbs.TabIndex = 0;
            this.BreadCrumbs.Text = "toolStrip1";
            this.BreadCrumbs.Visible = false;
            // 
            // nodesLayoutPanel
            // 
            this.nodesLayoutPanel.Controls.Add(this.collapsibleGroup1);
            this.nodesLayoutPanel.Controls.Add(this.outputsPad);
            this.nodesLayoutPanel.Controls.Add(this.inputsPad);
            this.nodesLayoutPanel.Controls.Add(this.nodesLayout1);
            this.nodesLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nodesLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.nodesLayoutPanel.Name = "nodesLayoutPanel";
            this.nodesLayoutPanel.Size = new System.Drawing.Size(508, 486);
            this.nodesLayoutPanel.TabIndex = 1;
            // 
            // collapsibleGroup1
            // 
            this.collapsibleGroup1.AllowResize = true;
            this.collapsibleGroup1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.collapsibleGroup1.Collapsed = false;
            this.collapsibleGroup1.CollapseOnClick = true;
            this.collapsibleGroup1.Controls.Add(this.logUCtrl1);
            this.collapsibleGroup1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.collapsibleGroup1.DockingChanges = TK.GraphComponents.DockingPossibilities.None;
            this.collapsibleGroup1.ForeColor = System.Drawing.Color.White;
            this.collapsibleGroup1.Location = new System.Drawing.Point(0, 446);
            this.collapsibleGroup1.Name = "collapsibleGroup1";
            this.collapsibleGroup1.OpenedBaseHeight = 40;
            this.collapsibleGroup1.OpenedBaseWidth = 200;
            this.collapsibleGroup1.Size = new System.Drawing.Size(508, 40);
            this.collapsibleGroup1.TabIndex = 7;
            this.collapsibleGroup1.TabStop = false;
            this.collapsibleGroup1.Text = "Log";
            // 
            // logUCtrl1
            // 
            this.logUCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logUCtrl1.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logUCtrl1.Location = new System.Drawing.Point(3, 16);
            logPreferences1.ShowErrors = true;
            logPreferences1.ShowInfos = true;
            logPreferences1.ShowLogs = true;
            logPreferences1.ShowWarnings = true;
            this.logUCtrl1.LoggingPreferences = logPreferences1;
            this.logUCtrl1.Name = "logUCtrl1";
            this.logUCtrl1.Size = new System.Drawing.Size(502, 21);
            this.logUCtrl1.TabIndex = 5;
            // 
            // outputsPad
            // 
            this.outputsPad.BackColor = System.Drawing.Color.Linen;
            this.outputsPad.Location = new System.Drawing.Point(179, 0);
            this.outputsPad.Name = "outputsPad";
            this.outputsPad.Size = new System.Drawing.Size(150, 486);
            this.outputsPad.TabIndex = 8;
            this.outputsPad.Visible = false;
            // 
            // inputsPad
            // 
            this.inputsPad.BackColor = System.Drawing.Color.Linen;
            this.inputsPad.Location = new System.Drawing.Point(0, 0);
            this.inputsPad.Name = "inputsPad";
            this.inputsPad.Size = new System.Drawing.Size(150, 486);
            this.inputsPad.TabIndex = 0;
            this.inputsPad.Visible = false;
            // 
            // nodesLayout1
            // 
            this.nodesLayout1.AllowDrop = true;
            this.nodesLayout1.BackColor = System.Drawing.Color.White;
            this.nodesLayout1.LayoutSize = 1D;
            this.nodesLayout1.Location = new System.Drawing.Point(3, 3);
            this.nodesLayout1.Name = "nodesLayout1";
            this.nodesLayout1.PortHeight = 18;
            this.nodesLayout1.Size = new System.Drawing.Size(1235, 878);
            this.nodesLayout1.TabIndex = 2;
            this.nodesLayout1.XLoc = 251;
            this.nodesLayout1.YLoc = 240;
            // 
            // TK_NodalEditorUCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.nodesLayoutPanel);
            this.Controls.Add(this.BreadCrumbs);
            this.Name = "TK_NodalEditorUCtrl";
            this.Size = new System.Drawing.Size(508, 486);
            this.nodesLayoutPanel.ResumeLayout(false);
            this.collapsibleGroup1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip BreadCrumbs;
        private System.Windows.Forms.Panel nodesLayoutPanel;
        private CompoundPortPad inputsPad;
        private NodesLayout nodesLayout1;
        private GraphComponents.CollapsibleGroup collapsibleGroup1;
        private MiniLogger.LogUCtrl logUCtrl1;
        private CompoundPortPad outputsPad;

    }
}
