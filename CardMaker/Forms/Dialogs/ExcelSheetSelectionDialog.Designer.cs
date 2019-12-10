namespace CardMaker.Forms.Dialogs
{
    partial class ExcelSheetSelectionDialog
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
            this.cmbSheetName = new System.Windows.Forms.ComboBox();
            this.btnSelectSheet = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbSheetName
            // 
            this.cmbSheetName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSheetName.FormattingEnabled = true;
            this.cmbSheetName.Location = new System.Drawing.Point(12, 12);
            this.cmbSheetName.Name = "cmbSheetName";
            this.cmbSheetName.Size = new System.Drawing.Size(514, 24);
            this.cmbSheetName.TabIndex = 0;
            // 
            // btnSelectSheet
            // 
            this.btnSelectSheet.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSelectSheet.Location = new System.Drawing.Point(12, 58);
            this.btnSelectSheet.Name = "btnSelectSheet";
            this.btnSelectSheet.Size = new System.Drawing.Size(269, 51);
            this.btnSelectSheet.TabIndex = 1;
            this.btnSelectSheet.Text = "Select Sheet";
            this.btnSelectSheet.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(401, 58);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(125, 51);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ExcelSheetSelectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 121);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSelectSheet);
            this.Controls.Add(this.cmbSheetName);
            this.Name = "ExcelSheetSelectionDialog";
            this.Text = "Select Sheet";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbSheetName;
        private System.Windows.Forms.Button btnSelectSheet;
        private System.Windows.Forms.Button btnCancel;
    }
}