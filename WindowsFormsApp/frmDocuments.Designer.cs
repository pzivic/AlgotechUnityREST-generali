using System.Windows.Forms;

namespace WindowsFormsApp
{
    partial class frmDocuments
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
            this.txtDateFrom = new System.Windows.Forms.TextBox();
            this.txtDateTo = new System.Windows.Forms.TextBox();
            this.comboBoxDocumentType = new System.Windows.Forms.ComboBox();
            this.butSubmit = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.butDocumentImport = new System.Windows.Forms.Button();
            this.txtCounter = new System.Windows.Forms.TextBox();
            this.txtResponse = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDateFrom
            // 
            this.txtDateFrom.Location = new System.Drawing.Point(43, 57);
            this.txtDateFrom.Name = "txtDateFrom";
            this.txtDateFrom.Size = new System.Drawing.Size(100, 25);
            this.txtDateFrom.TabIndex = 0;
            this.txtDateFrom.Text = "Datum od";
            // 
            // txtDateTo
            // 
            this.txtDateTo.Location = new System.Drawing.Point(149, 57);
            this.txtDateTo.Name = "txtDateTo";
            this.txtDateTo.Size = new System.Drawing.Size(100, 25);
            this.txtDateTo.TabIndex = 1;
            this.txtDateTo.Text = "Datum do";
            // 
            // comboBoxDocumentType
            // 
            this.comboBoxDocumentType.FormattingEnabled = true;
            this.comboBoxDocumentType.Location = new System.Drawing.Point(43, 88);
            this.comboBoxDocumentType.Name = "comboBoxDocumentType";
            this.comboBoxDocumentType.Size = new System.Drawing.Size(755, 25);
            this.comboBoxDocumentType.TabIndex = 2;
            // 
            // butSubmit
            // 
            this.butSubmit.Location = new System.Drawing.Point(43, 119);
            this.butSubmit.Name = "butSubmit";
            this.butSubmit.Size = new System.Drawing.Size(75, 34);
            this.butSubmit.TabIndex = 3;
            this.butSubmit.Text = "Prikaži";
            this.butSubmit.UseVisualStyleBackColor = true;
            this.butSubmit.Click += new System.EventHandler(this.butSubmit_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 567);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(933, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // listView1
            // 
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(43, 159);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(755, 359);
            this.listView1.TabIndex = 6;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // butDocumentImport
            // 
            this.butDocumentImport.Location = new System.Drawing.Point(124, 119);
            this.butDocumentImport.Name = "butDocumentImport";
            this.butDocumentImport.Size = new System.Drawing.Size(111, 34);
            this.butDocumentImport.TabIndex = 7;
            this.butDocumentImport.Text = "Import";
            this.butDocumentImport.UseVisualStyleBackColor = true;
            this.butDocumentImport.Click += new System.EventHandler(this.butDocumentImport_Click);
            // 
            // txtCounter
            // 
            this.txtCounter.Location = new System.Drawing.Point(254, 128);
            this.txtCounter.Name = "txtCounter";
            this.txtCounter.Size = new System.Drawing.Size(100, 25);
            this.txtCounter.TabIndex = 10;
            this.txtCounter.Text = "1";
            // 
            // txtResponse
            // 
            this.txtResponse.Location = new System.Drawing.Point(360, 128);
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.Size = new System.Drawing.Size(96, 25);
            this.txtResponse.TabIndex = 8;
            this.txtResponse.Text = "2011";
            // 
            // frmDocuments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 589);
            this.Controls.Add(this.txtCounter);
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.butDocumentImport);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.butSubmit);
            this.Controls.Add(this.comboBoxDocumentType);
            this.Controls.Add(this.txtDateTo);
            this.Controls.Add(this.txtDateFrom);
            this.Font = new System.Drawing.Font("Segoe UI Variable Display", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmDocuments";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dokumenta";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDateFrom;
        private System.Windows.Forms.TextBox txtDateTo;
        private System.Windows.Forms.ComboBox comboBoxDocumentType;
        private System.Windows.Forms.Button butSubmit;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ListView listView1;
        private Button butDocumentImport;
        private TextBox txtCounter;
        private TextBox txtResponse;
    }
}