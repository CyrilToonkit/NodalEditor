namespace TK.NodalEditor.NodesLayout
{
    partial class newPort
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
            this.label1 = new System.Windows.Forms.Label();
            this.NameTB = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.CancelBT = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.TypeTB = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.enumValuesPanel = new System.Windows.Forms.Panel();
            this.enumValuesTB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.enumValuesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(2, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NameTB
            // 
            this.NameTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NameTB.Location = new System.Drawing.Point(52, 2);
            this.NameTB.Name = "NameTB";
            this.NameTB.Size = new System.Drawing.Size(261, 20);
            this.NameTB.TabIndex = 1;
            // 
            // OKButton
            // 
            this.OKButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OKButton.Location = new System.Drawing.Point(3, 3);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(151, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.CancelBT, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.OKButton, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 143);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(315, 29);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // CancelBT
            // 
            this.CancelBT.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CancelBT.Location = new System.Drawing.Point(160, 3);
            this.CancelBT.Name = "CancelBT";
            this.CancelBT.Size = new System.Drawing.Size(152, 23);
            this.CancelBT.TabIndex = 3;
            this.CancelBT.Text = "Cancel";
            this.CancelBT.UseVisualStyleBackColor = true;
            this.CancelBT.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(2, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 85);
            this.label2.TabIndex = 5;
            this.label2.Text = "Type :";
            // 
            // TypeTB
            // 
            this.TypeTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TypeTB.FormattingEnabled = true;
            this.TypeTB.Location = new System.Drawing.Point(52, 2);
            this.TypeTB.Name = "TypeTB";
            this.TypeTB.Size = new System.Drawing.Size(261, 85);
            this.TypeTB.TabIndex = 6;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.NameTB);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(2);
            this.panel1.Size = new System.Drawing.Size(315, 22);
            this.panel1.TabIndex = 7;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.TypeTB);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 22);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(2);
            this.panel2.Size = new System.Drawing.Size(315, 89);
            this.panel2.TabIndex = 8;
            // 
            // enumValuesPanel
            // 
            this.enumValuesPanel.Controls.Add(this.enumValuesTB);
            this.enumValuesPanel.Controls.Add(this.label3);
            this.enumValuesPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.enumValuesPanel.Location = new System.Drawing.Point(0, 111);
            this.enumValuesPanel.Name = "enumValuesPanel";
            this.enumValuesPanel.Padding = new System.Windows.Forms.Padding(2);
            this.enumValuesPanel.Size = new System.Drawing.Size(315, 32);
            this.enumValuesPanel.TabIndex = 9;
            this.enumValuesPanel.Visible = false;
            // 
            // enumValuesTB
            // 
            this.enumValuesTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.enumValuesTB.Location = new System.Drawing.Point(52, 2);
            this.enumValuesTB.Name = "enumValuesTB";
            this.enumValuesTB.Size = new System.Drawing.Size(261, 20);
            this.enumValuesTB.TabIndex = 2;
            this.enumValuesTB.TextChanged += new System.EventHandler(this.enumValuesTB_TextChanged);
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(2, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 28);
            this.label3.TabIndex = 1;
            this.label3.Text = "Enum values :";
            // 
            // newPort
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBT;
            this.ClientSize = new System.Drawing.Size(315, 172);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.enumValuesPanel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "newPort";
            this.ShowIcon = false;
            this.Text = "New Port";
            this.TopMost = true;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.enumValuesPanel.ResumeLayout(false);
            this.enumValuesPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox NameTB;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button CancelBT;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox TypeTB;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel enumValuesPanel;
        private System.Windows.Forms.TextBox enumValuesTB;
        private System.Windows.Forms.Label label3;
    }
}