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
            this.button1 = new System.Windows.Forms.Button();
            this.txtResponse1 = new System.Windows.Forms.TextBox();
            this.txtResponse2 = new System.Windows.Forms.TextBox();
            this.txtCounter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.butSubmitByCounter = new System.Windows.Forms.Button();
            this.txtDocBarcode = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.txtRequestID = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(12, 47);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(726, 24);
            this.txtUrl.TabIndex = 0;
            // 
            // butSubmit
            // 
            this.butSubmit.Location = new System.Drawing.Point(567, -1);
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 584);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(763, 22);
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
            this.txtResults.Location = new System.Drawing.Point(12, 206);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResults.Size = new System.Drawing.Size(723, 285);
            this.txtResults.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(236, 94);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(206, 42);
            this.button1.TabIndex = 4;
            this.button1.Text = "Submit PostAsync multiply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtResponse1
            // 
            this.txtResponse1.Location = new System.Drawing.Point(17, 104);
            this.txtResponse1.Name = "txtResponse1";
            this.txtResponse1.Size = new System.Drawing.Size(96, 24);
            this.txtResponse1.TabIndex = 5;
            this.txtResponse1.Text = "2011";
            // 
            // txtResponse2
            // 
            this.txtResponse2.Location = new System.Drawing.Point(134, 105);
            this.txtResponse2.Name = "txtResponse2";
            this.txtResponse2.Size = new System.Drawing.Size(96, 24);
            this.txtResponse2.TabIndex = 6;
            this.txtResponse2.Text = "2012";
            // 
            // txtCounter
            // 
            this.txtCounter.Location = new System.Drawing.Point(17, 161);
            this.txtCounter.Name = "txtCounter";
            this.txtCounter.Size = new System.Drawing.Size(100, 24);
            this.txtCounter.TabIndex = 7;
            this.txtCounter.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "RequestID 1*: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(129, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 17);
            this.label2.TabIndex = 9;
            this.label2.Text = "RequestID 2*: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 141);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 17);
            this.label3.TabIndex = 10;
            this.label3.Text = "Counter:";
            // 
            // butSubmitByCounter
            // 
            this.butSubmitByCounter.Location = new System.Drawing.Point(132, 150);
            this.butSubmitByCounter.Name = "butSubmitByCounter";
            this.butSubmitByCounter.Size = new System.Drawing.Size(153, 41);
            this.butSubmitByCounter.TabIndex = 11;
            this.butSubmitByCounter.Text = "Submit By Counter";
            this.butSubmitByCounter.UseVisualStyleBackColor = true;
            this.butSubmitByCounter.Click += new System.EventHandler(this.butSubmitByCounter_Click);
            // 
            // txtDocBarcode
            // 
            this.txtDocBarcode.Location = new System.Drawing.Point(12, 557);
            this.txtDocBarcode.Name = "txtDocBarcode";
            this.txtDocBarcode.Size = new System.Drawing.Size(231, 24);
            this.txtDocBarcode.TabIndex = 12;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(15, 497);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(720, 24);
            this.txtMessage.TabIndex = 13;
            // 
            // txtRequestID
            // 
            this.txtRequestID.Location = new System.Drawing.Point(12, 527);
            this.txtRequestID.Name = "txtRequestID";
            this.txtRequestID.Size = new System.Drawing.Size(231, 24);
            this.txtRequestID.TabIndex = 14;
            // 
            // frmDocumentImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 606);
            this.Controls.Add(this.txtRequestID);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.txtDocBarcode);
            this.Controls.Add(this.butSubmitByCounter);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCounter);
            this.Controls.Add(this.txtResponse2);
            this.Controls.Add(this.txtResponse1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.butSubmit);
            this.Controls.Add(this.txtUrl);
            this.Font = new System.Drawing.Font("Tahoma", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmDocumentImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OnBase document import";
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtResponse1;
        private System.Windows.Forms.TextBox txtResponse2;
        private System.Windows.Forms.TextBox txtCounter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button butSubmitByCounter;
        private System.Windows.Forms.TextBox txtDocBarcode;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TextBox txtRequestID;
    }
}

