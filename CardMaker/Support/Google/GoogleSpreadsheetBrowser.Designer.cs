////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
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

namespace Support.UI
{
    partial class GoogleSpreadsheetBrowser
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
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSheets = new System.Windows.Forms.Label();
            this.listViewSpreadsheets = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewSheets = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            // txtFilter
            // 
            this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilter.Location = new System.Drawing.Point(81, 13);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(411, 20);
            this.txtFilter.TabIndex = 2;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSheets
            // 
            this.lblSheets.Location = new System.Drawing.Point(9, 319);
            this.lblSheets.Name = "lblSheets";
            this.lblSheets.Size = new System.Drawing.Size(63, 20);
            this.lblSheets.TabIndex = 6;
            this.lblSheets.Text = "Sheets:";
            this.lblSheets.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // listViewSpreadsheets
            // 
            this.listViewSpreadsheets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSpreadsheets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewSpreadsheets.FullRowSelect = true;
            this.listViewSpreadsheets.GridLines = true;
            this.listViewSpreadsheets.HideSelection = false;
            this.listViewSpreadsheets.Location = new System.Drawing.Point(12, 39);
            this.listViewSpreadsheets.Name = "listViewSpreadsheets";
            this.listViewSpreadsheets.Size = new System.Drawing.Size(480, 277);
            this.listViewSpreadsheets.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewSpreadsheets.TabIndex = 7;
            this.listViewSpreadsheets.UseCompatibleStateImageBehavior = false;
            this.listViewSpreadsheets.View = System.Windows.Forms.View.Details;
            this.listViewSpreadsheets.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewSpreadsheets_ColumnClick);
            this.listViewSpreadsheets.SelectedIndexChanged += new System.EventHandler(this.listViewSpreadsheets_SelectedIndexChanged);
            this.listViewSpreadsheets.Resize += new System.EventHandler(this.listViewSpreadsheets_Resize);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Spreadsheets";
            this.columnHeader1.Width = 475;
            // 
            // listViewSheets
            // 
            this.listViewSheets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSheets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.listViewSheets.FullRowSelect = true;
            this.listViewSheets.GridLines = true;
            this.listViewSheets.HideSelection = false;
            this.listViewSheets.Location = new System.Drawing.Point(12, 342);
            this.listViewSheets.Name = "listViewSheets";
            this.listViewSheets.Size = new System.Drawing.Size(480, 102);
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
            // GoogleSpreadsheetBrowser
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(504, 485);
            this.Controls.Add(this.listViewSheets);
            this.Controls.Add(this.listViewSpreadsheets);
            this.Controls.Add(this.lblSheets);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.MinimumSize = new System.Drawing.Size(512, 512);
            this.Name = "GoogleSpreadsheetBrowser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Google Spreadsheet Browser";
            this.Load += new System.EventHandler(this.GoogleSpreadsheetBrowser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSheets;
        private System.Windows.Forms.ListView listViewSpreadsheets;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ListView listViewSheets;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}