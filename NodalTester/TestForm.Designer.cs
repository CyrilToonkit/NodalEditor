namespace NodalTester
{
    partial class TestForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestForm));
            this.tgvImageList = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.sillyMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptstoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.collapsibleGroup1 = new TK.GraphComponents.CollapsibleGroup();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.csInterpreterTab = new System.Windows.Forms.TabPage();
            this.csEditControl = new DevExpress.XtraRichEdit.RichEditControl();
            this.pyInterpreterTab = new System.Windows.Forms.TabPage();
            this.pyEditControl = new DevExpress.XtraRichEdit.RichEditControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.nodalExecuteBT = new System.Windows.Forms.ToolStripButton();
            this.propertyGridControl1 = new DevExpress.XtraVerticalGrid.PropertyGridControl();
            this.tK_NodalEditorUCtrl1 = new TK.NodalEditor.NodesLayout.TK_NodalEditorUCtrl();
            this.menuStrip1.SuspendLayout();
            this.collapsibleGroup1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.csInterpreterTab.SuspendLayout();
            this.pyInterpreterTab.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.propertyGridControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // tgvImageList
            // 
            this.tgvImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            resources.ApplyResources(this.tgvImageList, "tgvImageList");
            this.tgvImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.scriptstoolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.toolStripMenuItem1,
            this.saveToolStripMenuItem,
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            resources.ApplyResources(this.newToolStripMenuItem, "newToolStripMenuItem");
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripMenuItem2,
            this.sillyMethodToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            resources.ApplyResources(this.undoToolStripMenuItem, "undoToolStripMenuItem");
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            resources.ApplyResources(this.redoToolStripMenuItem, "redoToolStripMenuItem");
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // sillyMethodToolStripMenuItem
            // 
            this.sillyMethodToolStripMenuItem.Name = "sillyMethodToolStripMenuItem";
            resources.ApplyResources(this.sillyMethodToolStripMenuItem, "sillyMethodToolStripMenuItem");
            this.sillyMethodToolStripMenuItem.Click += new System.EventHandler(this.sillyMethodToolStripMenuItem_Click);
            // 
            // scriptstoolStripMenuItem
            // 
            this.scriptstoolStripMenuItem.Name = "scriptstoolStripMenuItem";
            resources.ApplyResources(this.scriptstoolStripMenuItem, "scriptstoolStripMenuItem");
            this.scriptstoolStripMenuItem.Click += new System.EventHandler(this.scriptstoolStripMenuItem_Click);
            // 
            // saveFileDialog1
            // 
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // collapsibleGroup1
            // 
            this.collapsibleGroup1.AllowResize = true;
            this.collapsibleGroup1.Collapsed = false;
            this.collapsibleGroup1.CollapseOnClick = true;
            this.collapsibleGroup1.Controls.Add(this.tabControl1);
            this.collapsibleGroup1.Controls.Add(this.toolStrip1);
            resources.ApplyResources(this.collapsibleGroup1, "collapsibleGroup1");
            this.collapsibleGroup1.DockingChanges = TK.GraphComponents.DockingPossibilities.All;
            this.collapsibleGroup1.Name = "collapsibleGroup1";
            this.collapsibleGroup1.OpenedBaseHeight = 150;
            this.collapsibleGroup1.OpenedBaseWidth = 200;
            this.collapsibleGroup1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.csInterpreterTab);
            this.tabControl1.Controls.Add(this.pyInterpreterTab);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // csInterpreterTab
            // 
            this.csInterpreterTab.Controls.Add(this.csEditControl);
            resources.ApplyResources(this.csInterpreterTab, "csInterpreterTab");
            this.csInterpreterTab.Name = "csInterpreterTab";
            this.csInterpreterTab.UseVisualStyleBackColor = true;
            // 
            // csEditControl
            // 
            this.csEditControl.ActiveViewType = DevExpress.XtraRichEdit.RichEditViewType.Simple;
            this.csEditControl.Appearance.Text.Font = ((System.Drawing.Font)(resources.GetObject("csEditControl.Appearance.Text.Font")));
            this.csEditControl.Appearance.Text.Options.UseFont = true;
            resources.ApplyResources(this.csEditControl, "csEditControl");
            this.csEditControl.LayoutUnit = DevExpress.XtraRichEdit.DocumentLayoutUnit.Pixel;
            this.csEditControl.LookAndFeel.SkinName = "Visual Studio 2013 Dark";
            this.csEditControl.Name = "csEditControl";
            this.csEditControl.Options.Behavior.ShowPopupMenu = DevExpress.XtraRichEdit.DocumentCapability.Hidden;
            this.csEditControl.Options.Behavior.TabMarker = "    ";
            this.csEditControl.Options.Comments.Visibility = DevExpress.XtraRichEdit.RichEditCommentVisibility.Hidden;
            this.csEditControl.Views.SimpleView.AllowDisplayLineNumbers = true;
            this.csEditControl.Views.SimpleView.WordWrap = false;
            // 
            // pyInterpreterTab
            // 
            this.pyInterpreterTab.Controls.Add(this.pyEditControl);
            resources.ApplyResources(this.pyInterpreterTab, "pyInterpreterTab");
            this.pyInterpreterTab.Name = "pyInterpreterTab";
            this.pyInterpreterTab.UseVisualStyleBackColor = true;
            // 
            // pyEditControl
            // 
            this.pyEditControl.ActiveViewType = DevExpress.XtraRichEdit.RichEditViewType.Simple;
            this.pyEditControl.Appearance.Text.Font = ((System.Drawing.Font)(resources.GetObject("pyEditControl.Appearance.Text.Font")));
            this.pyEditControl.Appearance.Text.Options.UseFont = true;
            resources.ApplyResources(this.pyEditControl, "pyEditControl");
            this.pyEditControl.LayoutUnit = DevExpress.XtraRichEdit.DocumentLayoutUnit.Pixel;
            this.pyEditControl.LookAndFeel.SkinName = "Visual Studio 2013 Dark";
            this.pyEditControl.Name = "pyEditControl";
            this.pyEditControl.Options.Behavior.ShowPopupMenu = DevExpress.XtraRichEdit.DocumentCapability.Hidden;
            this.pyEditControl.Options.Behavior.TabMarker = "    ";
            this.pyEditControl.Options.Comments.Visibility = DevExpress.XtraRichEdit.RichEditCommentVisibility.Hidden;
            this.pyEditControl.Views.SimpleView.AllowDisplayLineNumbers = true;
            this.pyEditControl.Views.SimpleView.WordWrap = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nodalExecuteBT});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // nodalExecuteBT
            // 
            this.nodalExecuteBT.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.nodalExecuteBT, "nodalExecuteBT");
            this.nodalExecuteBT.Name = "nodalExecuteBT";
            this.nodalExecuteBT.Click += new System.EventHandler(this.nodalExecuteBT_Click);
            // 
            // propertyGridControl1
            // 
            resources.ApplyResources(this.propertyGridControl1, "propertyGridControl1");
            this.propertyGridControl1.Name = "propertyGridControl1";
            // 
            // tK_NodalEditorUCtrl1
            // 
            resources.ApplyResources(this.tK_NodalEditorUCtrl1, "tK_NodalEditorUCtrl1");
            this.tK_NodalEditorUCtrl1.Name = "tK_NodalEditorUCtrl1";
            // 
            // TestForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.propertyGridControl1);
            this.Controls.Add(this.tK_NodalEditorUCtrl1);
            this.Controls.Add(this.collapsibleGroup1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TestForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.collapsibleGroup1.ResumeLayout(false);
            this.collapsibleGroup1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.csInterpreterTab.ResumeLayout(false);
            this.pyInterpreterTab.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.propertyGridControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList tgvImageList;
        private TK.NodalEditor.NodesLayout.TK_NodalEditorUCtrl tK_NodalEditorUCtrl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private TK.GraphComponents.CollapsibleGroup collapsibleGroup1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton nodalExecuteBT;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem sillyMethodToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage csInterpreterTab;
        private System.Windows.Forms.TabPage pyInterpreterTab;
        private DevExpress.XtraRichEdit.RichEditControl pyEditControl;
        private DevExpress.XtraRichEdit.RichEditControl csEditControl;
        private DevExpress.XtraVerticalGrid.PropertyGridControl propertyGridControl1;
        private System.Windows.Forms.ToolStripMenuItem scriptstoolStripMenuItem;
    }
}