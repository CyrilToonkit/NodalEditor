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
            this.tgvImageList = new System.Windows.Forms.ImageList(this.components);
            this.stringNodesTreeView1 = new TK.GraphComponents.stringNodesTreeView(this.components);
            this.tK_NodalEditorUCtrl1 = new TK.NodalEditor.NodesLayout.TK_NodalEditorUCtrl();
            this.SuspendLayout();
            // 
            // tgvImageList
            // 
            this.tgvImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.tgvImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.tgvImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // stringNodesTreeView1
            // 
            this.stringNodesTreeView1.CreateRoot = false;
            this.stringNodesTreeView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.stringNodesTreeView1.DrawGrid = false;
            this.stringNodesTreeView1.EnableManageNodes = false;
            this.stringNodesTreeView1.EnableRenameNode = false;
            this.stringNodesTreeView1.EnableReorderNodes = false;
            this.stringNodesTreeView1.Location = new System.Drawing.Point(0, 0);
            this.stringNodesTreeView1.Name = "stringNodesTreeView1";
            this.stringNodesTreeView1.Rows = 10000;
            this.stringNodesTreeView1.Size = new System.Drawing.Size(192, 612);
            this.stringNodesTreeView1.TabIndex = 1;
            // 
            // tK_NodalEditorUCtrl1
            // 
            this.tK_NodalEditorUCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tK_NodalEditorUCtrl1.Location = new System.Drawing.Point(192, 0);
            this.tK_NodalEditorUCtrl1.Name = "tK_NodalEditorUCtrl1";
            this.tK_NodalEditorUCtrl1.Size = new System.Drawing.Size(562, 612);
            this.tK_NodalEditorUCtrl1.TabIndex = 3;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 612);
            this.Controls.Add(this.tK_NodalEditorUCtrl1);
            this.Controls.Add(this.stringNodesTreeView1);
            this.Name = "TestForm";
            this.Text = "TestForm change";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList tgvImageList;
        private TK.GraphComponents.stringNodesTreeView stringNodesTreeView1;
        private TK.NodalEditor.NodesLayout.TK_NodalEditorUCtrl tK_NodalEditorUCtrl1;
    }
}