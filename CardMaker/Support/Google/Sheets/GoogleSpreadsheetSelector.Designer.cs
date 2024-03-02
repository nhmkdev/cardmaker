////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

namespace Support.Google.Sheets
{
    partial class GoogleSpreadsheetSelector
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.listViewSheets = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtSpreadsheetURL = new System.Windows.Forms.TextBox();
            this.txtSpreadsheetID = new System.Windows.Forms.TextBox();
            this.btnLoadVerify = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblLoadStatus = new System.Windows.Forms.Label();
            this.lblSheetSelectionNotRequired = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(417, 450);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(336, 450);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // listViewSheets
            // 
            this.listViewSheets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSheets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.listViewSheets.FullRowSelect = true;
            this.listViewSheets.GridLines = true;
            this.listViewSheets.HideSelection = false;
            this.listViewSheets.Location = new System.Drawing.Point(12, 185);
            this.listViewSheets.Name = "listViewSheets";
            this.listViewSheets.Size = new System.Drawing.Size(480, 259);
            this.listViewSheets.TabIndex = 8;
            this.listViewSheets.UseCompatibleStateImageBehavior = false;
            this.listViewSheets.View = System.Windows.Forms.View.Details;
            this.listViewSheets.Resize += new System.EventHandler(this.listViewSheets_Resize);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Sheets";
            this.columnHeader2.Width = 475;
            // 
            // txtSpreadsheetURL
            // 
            this.txtSpreadsheetURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSpreadsheetURL.Location = new System.Drawing.Point(121, 42);
            this.txtSpreadsheetURL.Name = "txtSpreadsheetURL";
            this.txtSpreadsheetURL.Size = new System.Drawing.Size(371, 20);
            this.txtSpreadsheetURL.TabIndex = 9;
            this.txtSpreadsheetURL.TextChanged += new System.EventHandler(this.txtSpreadsheetURL_TextChanged);
            // 
            // txtSpreadsheetID
            // 
            this.txtSpreadsheetID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSpreadsheetID.Location = new System.Drawing.Point(121, 72);
            this.txtSpreadsheetID.Name = "txtSpreadsheetID";
            this.txtSpreadsheetID.Size = new System.Drawing.Size(371, 20);
            this.txtSpreadsheetID.TabIndex = 10;
            this.txtSpreadsheetID.TextChanged += new System.EventHandler(this.txtSpreadsheetID_TextChanged);
            // 
            // btnLoadVerify
            // 
            this.btnLoadVerify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadVerify.Location = new System.Drawing.Point(377, 108);
            this.btnLoadVerify.Name = "btnLoadVerify";
            this.btnLoadVerify.Size = new System.Drawing.Size(115, 23);
            this.btnLoadVerify.TabIndex = 11;
            this.btnLoadVerify.Text = "Load / Verify";
            this.btnLoadVerify.UseVisualStyleBackColor = true;
            this.btnLoadVerify.Click += new System.EventHandler(this.btnLoadVerify_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(9, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 20);
            this.label2.TabIndex = 12;
            this.label2.Text = "Spreadsheet URL:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(9, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 20);
            this.label3.TabIndex = 13;
            this.label3.Text = "Spreadsheet ID:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(315, 20);
            this.label1.TabIndex = 14;
            this.label1.Text = "Specify the URL or ID of the Spreadsheet";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLoadStatus
            // 
            this.lblLoadStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLoadStatus.Location = new System.Drawing.Point(377, 134);
            this.lblLoadStatus.Name = "lblLoadStatus";
            this.lblLoadStatus.Size = new System.Drawing.Size(115, 20);
            this.lblLoadStatus.TabIndex = 15;
            this.lblLoadStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSheetSelectionNotRequired
            // 
            this.lblSheetSelectionNotRequired.Location = new System.Drawing.Point(12, 162);
            this.lblSheetSelectionNotRequired.Name = "lblSheetSelectionNotRequired";
            this.lblSheetSelectionNotRequired.Size = new System.Drawing.Size(284, 20);
            this.lblSheetSelectionNotRequired.TabIndex = 16;
            this.lblSheetSelectionNotRequired.Text = "Note: Sheet selection is not required in this context.";
            this.lblSheetSelectionNotRequired.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GoogleSpreadsheetSelector
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(504, 485);
            this.Controls.Add(this.lblSheetSelectionNotRequired);
            this.Controls.Add(this.lblLoadStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSpreadsheetURL);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnLoadVerify);
            this.Controls.Add(this.txtSpreadsheetID);
            this.Controls.Add(this.listViewSheets);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.MinimumSize = new System.Drawing.Size(512, 512);
            this.Name = "GoogleSpreadsheetSelector";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Google Spreadsheet Selector";
            this.Load += new System.EventHandler(this.GoogleSpreadsheetBrowser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListView listViewSheets;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox txtSpreadsheetURL;
        private System.Windows.Forms.TextBox txtSpreadsheetID;
        private System.Windows.Forms.Button btnLoadVerify;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblLoadStatus;
        private System.Windows.Forms.Label lblSheetSelectionNotRequired;
    }
}