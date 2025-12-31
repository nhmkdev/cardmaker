namespace CardMaker.Forms.Dialogs
{
    partial class DefineReferencesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefineReferencesDialog));
            this.btnAddCSV = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAddGoogle = new System.Windows.Forms.Button();
            this.btnAddExcel = new System.Windows.Forms.Button();
            this.listViewReferences = new System.Windows.Forms.ListView();
            this.headerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.headerType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.headerDefinition = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // btnAddCSV
            // 
            this.btnAddCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddCSV.Location = new System.Drawing.Point(12, 415);
            this.btnAddCSV.Name = "btnAddCSV";
            this.btnAddCSV.Size = new System.Drawing.Size(75, 23);
            this.btnAddCSV.TabIndex = 1;
            this.btnAddCSV.Text = "Add CSV";
            this.btnAddCSV.UseVisualStyleBackColor = true;
            this.btnAddCSV.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(255, 415);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(632, 415);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(713, 415);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnAddGoogle
            // 
            this.btnAddGoogle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddGoogle.Location = new System.Drawing.Point(174, 415);
            this.btnAddGoogle.Name = "btnAddGoogle";
            this.btnAddGoogle.Size = new System.Drawing.Size(75, 23);
            this.btnAddGoogle.TabIndex = 0;
            this.btnAddGoogle.Text = "Add Google";
            this.btnAddGoogle.UseVisualStyleBackColor = true;
            this.btnAddGoogle.Click += new System.EventHandler(this.btnAddGoogle_Click);
            // 
            // btnAddExcel
            // 
            this.btnAddExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddExcel.Location = new System.Drawing.Point(93, 415);
            this.btnAddExcel.Name = "btnAddExcel";
            this.btnAddExcel.Size = new System.Drawing.Size(75, 23);
            this.btnAddExcel.TabIndex = 6;
            this.btnAddExcel.Text = "Add Excel";
            this.btnAddExcel.UseVisualStyleBackColor = true;
            this.btnAddExcel.Click += new System.EventHandler(this.btnAddExcel_Click);
            // 
            // listViewReferences
            // 
            this.listViewReferences.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewReferences.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerName,
            this.headerType,
            this.headerDefinition});
            this.listViewReferences.FullRowSelect = true;
            this.listViewReferences.GridLines = true;
            this.listViewReferences.HideSelection = false;
            this.listViewReferences.Location = new System.Drawing.Point(12, 12);
            this.listViewReferences.Name = "listViewReferences";
            this.listViewReferences.Size = new System.Drawing.Size(776, 394);
            this.listViewReferences.TabIndex = 7;
            this.listViewReferences.UseCompatibleStateImageBehavior = false;
            this.listViewReferences.View = System.Windows.Forms.View.Details;
            this.listViewReferences.Resize += new System.EventHandler(this.listViewReferences_Resize);
            // 
            // headerName
            // 
            this.headerName.Text = "Name";
            this.headerName.Width = 237;
            // 
            // headerType
            // 
            this.headerType.Text = "Type";
            this.headerType.Width = 88;
            // 
            // headerDefinition
            // 
            this.headerDefinition.Text = "Definition";
            this.headerDefinition.Width = 446;
            // 
            // DefineReferencesDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listViewReferences);
            this.Controls.Add(this.btnAddExcel);
            this.Controls.Add(this.btnAddGoogle);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnAddCSV);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DefineReferencesDialog";
            this.ShowInTaskbar = false;
            this.Text = "Define References";
            this.Load += new System.EventHandler(this.DefineReferencesDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnAddCSV;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAddGoogle;
        private System.Windows.Forms.Button btnAddExcel;
        private System.Windows.Forms.ListView listViewReferences;
        private System.Windows.Forms.ColumnHeader headerName;
        private System.Windows.Forms.ColumnHeader headerType;
        private System.Windows.Forms.ColumnHeader headerDefinition;
    }
}