namespace D365CodeReviewer
{
    partial class MainForm
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
            this.lblFDD = new System.Windows.Forms.Label();
            this.txtFDDPath = new System.Windows.Forms.TextBox();
            this.btnBrowseFDD = new System.Windows.Forms.Button();
            this.lblAXPP = new System.Windows.Forms.Label();
            this.txtAXPPPath = new System.Windows.Forms.TextBox();
            this.btnBrowseAXPP = new System.Windows.Forms.Button();
            this.lblOutput = new System.Windows.Forms.Label();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnStartReview = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblImplementedCount = new System.Windows.Forms.Label();
            this.lblMissingCount = new System.Windows.Forms.Label();
            this.lblDiscrepanciesCount = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDiscrepancies = new System.Windows.Forms.TabPage();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.lstDiscrepancies = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.btnExportToExcel = new System.Windows.Forms.Button();
            this.chkGenerateHTML = new System.Windows.Forms.CheckBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabDiscrepancies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFDD
            // 
            this.lblFDD.AutoSize = true;
            this.lblFDD.Location = new System.Drawing.Point(20, 98);
            this.lblFDD.Name = "lblFDD";
            this.lblFDD.Size = new System.Drawing.Size(193, 20);
            this.lblFDD.TabIndex = 0;
            this.lblFDD.Text = "Functional Design Document:";
            // 
            // txtFDDPath
            // 
            this.txtFDDPath.Location = new System.Drawing.Point(219, 95);
            this.txtFDDPath.Name = "txtFDDPath";
            this.txtFDDPath.ReadOnly = true;
            this.txtFDDPath.Size = new System.Drawing.Size(497, 27);
            this.txtFDDPath.TabIndex = 1;
            // 
            // btnBrowseFDD
            // 
            this.btnBrowseFDD.Location = new System.Drawing.Point(722, 94);
            this.btnBrowseFDD.Name = "btnBrowseFDD";
            this.btnBrowseFDD.Size = new System.Drawing.Size(94, 29);
            this.btnBrowseFDD.TabIndex = 2;
            this.btnBrowseFDD.Text = "Browse...";
            this.btnBrowseFDD.UseVisualStyleBackColor = true;
            this.btnBrowseFDD.Click += new System.EventHandler(this.btnBrowseFDD_Click);
            // 
            // lblAXPP
            // 
            this.lblAXPP.AutoSize = true;
            this.lblAXPP.Location = new System.Drawing.Point(20, 139);
            this.lblAXPP.Name = "lblAXPP";
            this.lblAXPP.Size = new System.Drawing.Size(169, 20);
            this.lblAXPP.TabIndex = 3;
            this.lblAXPP.Text = "D365FO Project/Solution:";
            // 
            // txtAXPPPath
            // 
            this.txtAXPPPath.Location = new System.Drawing.Point(219, 136);
            this.txtAXPPPath.Name = "txtAXPPPath";
            this.txtAXPPPath.ReadOnly = true;
            this.txtAXPPPath.Size = new System.Drawing.Size(497, 27);
            this.txtAXPPPath.TabIndex = 4;
            // 
            // btnBrowseAXPP
            // 
            this.btnBrowseAXPP.Location = new System.Drawing.Point(722, 135);
            this.btnBrowseAXPP.Name = "btnBrowseAXPP";
            this.btnBrowseAXPP.Size = new System.Drawing.Size(94, 29);
            this.btnBrowseAXPP.TabIndex = 5;
            this.btnBrowseAXPP.Text = "Browse...";
            this.btnBrowseAXPP.UseVisualStyleBackColor = true;
            this.btnBrowseAXPP.Click += new System.EventHandler(this.btnBrowseAXPP_Click);
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(20, 180);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(168, 20);
            this.lblOutput.TabIndex = 6;
            this.lblOutput.Text = "Output Report Location:";
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Location = new System.Drawing.Point(219, 177);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.ReadOnly = true;
            this.txtOutputPath.Size = new System.Drawing.Size(497, 27);
            this.txtOutputPath.TabIndex = 7;
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Location = new System.Drawing.Point(722, 176);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(94, 29);
            this.btnBrowseOutput.TabIndex = 8;
            this.btnBrowseOutput.Text = "Browse...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnStartReview
            // 
            this.btnStartReview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnStartReview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStartReview.ForeColor = System.Drawing.Color.White;
            this.btnStartReview.Location = new System.Drawing.Point(323, 222);
            this.btnStartReview.Name = "btnStartReview";
            this.btnStartReview.Size = new System.Drawing.Size(182, 40);
            this.btnStartReview.TabIndex = 9;
            this.btnStartReview.Text = "Start Code Review";
            this.btnStartReview.UseVisualStyleBackColor = false;
            this.btnStartReview.Click += new System.EventHandler(this.btnStartReview_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(20, 268);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(796, 10);
            this.progressBar.TabIndex = 10;
            this.progressBar.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 281);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(162, 20);
            this.lblStatus.TabIndex = 11;
            this.lblStatus.Text = "Ready to start analysis...";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblImplementedCount);
            this.groupBox1.Controls.Add(this.lblMissingCount);
            this.groupBox1.Controls.Add(this.lblDiscrepanciesCount);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(20, 304);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(796, 74);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Summary";
            // 
            // lblImplementedCount
            // 
            this.lblImplementedCount.AutoSize = true;
            this.lblImplementedCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblImplementedCount.ForeColor = System.Drawing.Color.Green;
            this.lblImplementedCount.Location = new System.Drawing.Point(693, 35);
            this.lblImplementedCount.Name = "lblImplementedCount";
            this.lblImplementedCount.Size = new System.Drawing.Size(18, 20);
            this.lblImplementedCount.TabIndex = 5;
            this.lblImplementedCount.Text = "0";
            // 
            // lblMissingCount
            // 
            this.lblMissingCount.AutoSize = true;
            this.lblMissingCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMissingCount.ForeColor = System.Drawing.Color.Blue;
            this.lblMissingCount.Location = new System.Drawing.Point(434, 35);
            this.lblMissingCount.Name = "lblMissingCount";
            this.lblMissingCount.Size = new System.Drawing.Size(18, 20);
            this.lblMissingCount.TabIndex = 4;
            this.lblMissingCount.Text = "0";
            // 
            // lblDiscrepanciesCount
            // 
            this.lblDiscrepanciesCount.AutoSize = true;
            this.lblDiscrepanciesCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDiscrepanciesCount.ForeColor = System.Drawing.Color.Red;
            this.lblDiscrepanciesCount.Location = new System.Drawing.Point(144, 35);
            this.lblDiscrepanciesCount.Name = "lblDiscrepanciesCount";
            this.lblDiscrepanciesCount.Size = new System.Drawing.Size(18, 20);
            this.lblDiscrepanciesCount.TabIndex = 3;
            this.lblDiscrepanciesCount.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(575, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 20);
            this.label4.TabIndex = 2;
            this.label4.Text = "Implemented: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(328, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 20);
            this.label3.TabIndex = 1;
            this.label3.Text = "Missing Items:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Discrepancies:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDiscrepancies);
            this.tabControl1.Location = new System.Drawing.Point(20, 384);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(796, 307);
            this.tabControl1.TabIndex = 13;
            // 
            // tabDiscrepancies
            // 
            this.tabDiscrepancies.Controls.Add(this.txtDetails);
            this.tabDiscrepancies.Controls.Add(this.lstDiscrepancies);
            this.tabDiscrepancies.Location = new System.Drawing.Point(4, 29);
            this.tabDiscrepancies.Name = "tabDiscrepancies";
            this.tabDiscrepancies.Padding = new System.Windows.Forms.Padding(3);
            this.tabDiscrepancies.Size = new System.Drawing.Size(788, 274);
            this.tabDiscrepancies.TabIndex = 0;
            this.tabDiscrepancies.Text = "Discrepancies";
            this.tabDiscrepancies.UseVisualStyleBackColor = true;
            // 
            // txtDetails
            // 
            this.txtDetails.Location = new System.Drawing.Point(485, 6);
            this.txtDetails.Multiline = true;
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ReadOnly = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetails.Size = new System.Drawing.Size(297, 262);
            this.txtDetails.TabIndex = 1;
            // 
            // lstDiscrepancies
            // 
            this.lstDiscrepancies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lstDiscrepancies.FullRowSelect = true;
            this.lstDiscrepancies.Location = new System.Drawing.Point(6, 6);
            this.lstDiscrepancies.Name = "lstDiscrepancies";
            this.lstDiscrepancies.Size = new System.Drawing.Size(473, 262);
            this.lstDiscrepancies.TabIndex = 0;
            this.lstDiscrepancies.UseCompatibleStateImageBehavior = false;
            this.lstDiscrepancies.View = System.Windows.Forms.View.Details;
            this.lstDiscrepancies.SelectedIndexChanged += new System.EventHandler(this.lstDiscrepancies_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 290;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Severity";
            this.columnHeader3.Width = 80;
            // 
            // btnExportToExcel
            // 
            this.btnExportToExcel.Location = new System.Drawing.Point(677, 697);
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.Size = new System.Drawing.Size(139, 29);
            this.btnExportToExcel.TabIndex = 14;
            this.btnExportToExcel.Text = "Export to CSV";
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // chkGenerateHTML
            // 
            this.chkGenerateHTML.AutoSize = true;
            this.chkGenerateHTML.Checked = true;
            this.chkGenerateHTML.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateHTML.Location = new System.Drawing.Point(219, 229);
            this.chkGenerateHTML.Name = "chkGenerateHTML";
            this.chkGenerateHTML.Size = new System.Drawing.Size(98, 24);
            this.chkGenerateHTML.TabIndex = 15;
            this.chkGenerateHTML.Text = "HTML too";
            this.chkGenerateHTML.UseVisualStyleBackColor = true;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblTitle.Location = new System.Drawing.Point(160, 33);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(556, 37);
            this.lblTitle.TabIndex = 16;
            this.lblTitle.Text = "D365FO Functional Design Code Reviewer";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(56, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(98, 83);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(837, 744);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.chkGenerateHTML);
            this.Controls.Add(this.btnExportToExcel);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnStartReview);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.btnBrowseAXPP);
            this.Controls.Add(this.txtAXPPPath);
            this.Controls.Add(this.lblAXPP);
            this.Controls.Add(this.btnBrowseFDD);
            this.Controls.Add(this.txtFDDPath);
            this.Controls.Add(this.lblFDD);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "D365FO Code Reviewer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabDiscrepancies.ResumeLayout(false);
            this.tabDiscrepancies.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblFDD;
        private TextBox txtFDDPath;
        private Button btnBrowseFDD;
        private Label lblAXPP;
        private TextBox txtAXPPPath;
        private Button btnBrowseAXPP;
        private Label lblOutput;
        private TextBox txtOutputPath;
        private Button btnBrowseOutput;
        private Button btnStartReview;
        private ProgressBar progressBar;
        private Label lblStatus;
        private GroupBox groupBox1;
        private Label lblImplementedCount;
        private Label lblMissingCount;
        private Label lblDiscrepanciesCount;
        private Label label4;
        private Label label3;
        private Label label2;
        private TabControl tabControl1;
        private TabPage tabDiscrepancies;
        private TextBox txtDetails;
        private ListView lstDiscrepancies;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Button btnExportToExcel;
        private CheckBox chkGenerateHTML;
        private Label lblTitle;
        private PictureBox pictureBox1;
    }
}