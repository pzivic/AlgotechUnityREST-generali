namespace WindowsFormsApp
{
    partial class frmDocumentImport
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
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.butSubmit = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.butSetDocumentKeyword = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(43, 47);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(726, 24);
            this.txtUrl.TabIndex = 0;
            // 
            // butSubmit
            // 
            this.butSubmit.Location = new System.Drawing.Point(43, 77);
            this.butSubmit.Name = "butSubmit";
            this.butSubmit.Size = new System.Drawing.Size(168, 42);
            this.butSubmit.TabIndex = 1;
            this.butSubmit.Text = "Submit PostAsync";
            this.butSubmit.UseVisualStyleBackColor = true;
            this.butSubmit.Click += new System.EventHandler(this.butSubmit_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 283);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(795, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // txtResults
            // 
            this.txtResults.Location = new System.Drawing.Point(43, 140);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.Size = new System.Drawing.Size(716, 103);
            this.txtResults.TabIndex = 3;
            // 
            // butSetDocumentKeyword
            // 
            this.butSetDocumentKeyword.Location = new System.Drawing.Point(589, -1);
            this.butSetDocumentKeyword.Name = "butSetDocumentKeyword";
            this.butSetDocumentKeyword.Size = new System.Drawing.Size(206, 42);
            this.butSetDocumentKeyword.TabIndex = 4;
            this.butSetDocumentKeyword.Text = "Set Document Keyword";
            this.butSetDocumentKeyword.UseVisualStyleBackColor = true;
            this.butSetDocumentKeyword.Visible = false;
            this.butSetDocumentKeyword.Click += new System.EventHandler(this.butSetDocumentKeyword_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 305);
            this.Controls.Add(this.butSetDocumentKeyword);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.butSubmit);
            this.Controls.Add(this.txtUrl);
            this.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "frmMain";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Button butSubmit;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Button butSetDocumentKeyword;
    }
}

