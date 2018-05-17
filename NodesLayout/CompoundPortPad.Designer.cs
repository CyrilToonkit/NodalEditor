namespace TK.NodalEditor.NodesLayout
{
    partial class CompoundPortPad
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
            this.RenameTB = new System.Windows.Forms.TextBox();
            this.RenameTL = new System.Windows.Forms.TableLayoutPanel();
            this.DefaultButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.EditGroupBox = new System.Windows.Forms.GroupBox();
            this.RenameTL.SuspendLayout();
            this.EditGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // RenameTB
            // 
            this.RenameTB.Dock = System.Windows.Forms.DockStyle.Top;
            this.RenameTB.Location = new System.Drawing.Point(3, 16);
            this.RenameTB.Name = "RenameTB";
            this.RenameTB.Size = new System.Drawing.Size(141, 20);
            this.RenameTB.TabIndex = 0;
            this.RenameTB.Leave += new System.EventHandler(this.RenameTB_Leave);
            this.RenameTB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RenameTB_KeyPress);
            // 
            // RenameTL
            // 
            this.RenameTL.ColumnCount = 2;
            this.RenameTL.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RenameTL.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RenameTL.Controls.Add(this.CancelButton, 1, 0);
            this.RenameTL.Controls.Add(this.DefaultButton, 0, 0);
            this.RenameTL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RenameTL.Location = new System.Drawing.Point(3, 36);
            this.RenameTL.Name = "RenameTL";
            this.RenameTL.RowCount = 1;
            this.RenameTL.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RenameTL.Size = new System.Drawing.Size(141, 26);
            this.RenameTL.TabIndex = 1;
            // 
            // DefaultButton
            // 
            this.DefaultButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DefaultButton.Location = new System.Drawing.Point(3, 3);
            this.DefaultButton.Name = "DefaultButton";
            this.DefaultButton.Size = new System.Drawing.Size(64, 20);
            this.DefaultButton.TabIndex = 0;
            this.DefaultButton.Text = "Default";
            this.DefaultButton.UseVisualStyleBackColor = true;
            this.DefaultButton.Click += new System.EventHandler(this.DefaultButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CancelButton.Location = new System.Drawing.Point(73, 3);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(65, 20);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // EditGroupBox
            // 
            this.EditGroupBox.Controls.Add(this.RenameTL);
            this.EditGroupBox.Controls.Add(this.RenameTB);
            this.EditGroupBox.ForeColor = System.Drawing.Color.Black;
            this.EditGroupBox.Location = new System.Drawing.Point(0, 21);
            this.EditGroupBox.Name = "EditGroupBox";
            this.EditGroupBox.Size = new System.Drawing.Size(147, 65);
            this.EditGroupBox.TabIndex = 2;
            this.EditGroupBox.TabStop = false;
            this.EditGroupBox.Text = "groupBox1";
            this.EditGroupBox.Visible = false;
            // 
            // CompoundPortPad
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Linen;
            this.Controls.Add(this.EditGroupBox);
            this.Name = "CompoundPortPad";
            this.ParentChanged += new System.EventHandler(this.CompoundPortPad_ParentChanged);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CompoundPortPad_MouseUp);
            this.RenameTL.ResumeLayout(false);
            this.EditGroupBox.ResumeLayout(false);
            this.EditGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox RenameTB;
        private System.Windows.Forms.TableLayoutPanel RenameTL;
        private System.Windows.Forms.Button DefaultButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.GroupBox EditGroupBox;
    }
}
