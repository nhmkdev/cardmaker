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

namespace CardMaker.Forms
{
    partial class MDIDefines
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
            this.components = new System.ComponentModel.Container();
            this.listViewDefines = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuDefineOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyDefineAsReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyDefineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkTranslatePrimitiveCharacters = new System.Windows.Forms.CheckBox();
            this.menuDefineOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewDefines
            // 
            this.listViewDefines.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDefines.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewDefines.ContextMenuStrip = this.menuDefineOptions;
            this.listViewDefines.FullRowSelect = true;
            this.listViewDefines.GridLines = true;
            this.listViewDefines.HideSelection = false;
            this.listViewDefines.Location = new System.Drawing.Point(0, 28);
            this.listViewDefines.Name = "listViewDefines";
            this.listViewDefines.Size = new System.Drawing.Size(466, 45);
            this.listViewDefines.TabIndex = 0;
            this.listViewDefines.UseCompatibleStateImageBehavior = false;
            this.listViewDefines.View = System.Windows.Forms.View.Details;
            this.listViewDefines.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewDefines_ColumnClick);
            this.listViewDefines.Resize += new System.EventHandler(this.listViewDefines_Resize);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Define";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            // 
            // menuDefineOptions
            // 
            this.menuDefineOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyDefineAsReferenceToolStripMenuItem,
            this.copyDefineToolStripMenuItem,
            this.copyValueToolStripMenuItem});
            this.menuDefineOptions.Name = "menuDefineOptions";
            this.menuDefineOptions.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuDefineOptions.ShowImageMargin = false;
            this.menuDefineOptions.Size = new System.Drawing.Size(227, 70);
            // 
            // copyDefineAsReferenceToolStripMenuItem
            // 
            this.copyDefineAsReferenceToolStripMenuItem.Name = "copyDefineAsReferenceToolStripMenuItem";
            this.copyDefineAsReferenceToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.copyDefineAsReferenceToolStripMenuItem.Text = "Copy Define as Data Item Reference";
            this.copyDefineAsReferenceToolStripMenuItem.Click += new System.EventHandler(this.copyDefineAsReferenceToolStripMenuItem_Click);
            // 
            // copyDefineToolStripMenuItem
            // 
            this.copyDefineToolStripMenuItem.Name = "copyDefineToolStripMenuItem";
            this.copyDefineToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.copyDefineToolStripMenuItem.Text = "Copy Define";
            this.copyDefineToolStripMenuItem.Click += new System.EventHandler(this.copyDefineToolStripMenuItem_Click);
            // 
            // copyValueToolStripMenuItem
            // 
            this.copyValueToolStripMenuItem.Name = "copyValueToolStripMenuItem";
            this.copyValueToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.copyValueToolStripMenuItem.Text = "Copy Value";
            this.copyValueToolStripMenuItem.Click += new System.EventHandler(this.copyValueToolStripMenuItem_Click);
            // 
            // checkTranslatePrimitiveCharacters
            // 
            this.checkTranslatePrimitiveCharacters.Location = new System.Drawing.Point(0, 2);
            this.checkTranslatePrimitiveCharacters.Name = "checkTranslatePrimitiveCharacters";
            this.checkTranslatePrimitiveCharacters.Size = new System.Drawing.Size(454, 24);
            this.checkTranslatePrimitiveCharacters.TabIndex = 1;
            this.checkTranslatePrimitiveCharacters.Text = "Translate Primitive Characters";
            this.checkTranslatePrimitiveCharacters.UseVisualStyleBackColor = true;
            this.checkTranslatePrimitiveCharacters.CheckedChanged += new System.EventHandler(this.checkTranslatePrimitiveCharacters_CheckedChanged);
            // 
            // MDIDefines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 73);
            this.Controls.Add(this.checkTranslatePrimitiveCharacters);
            this.Controls.Add(this.listViewDefines);
            this.MinimumSize = new System.Drawing.Size(300, 100);
            this.Name = "MDIDefines";
            this.ShowIcon = false;
            this.Text = "Defines";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MDIDefines_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.MDIDefines_VisibleChanged);
            this.menuDefineOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewDefines;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ContextMenuStrip menuDefineOptions;
        private System.Windows.Forms.ToolStripMenuItem copyDefineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyDefineAsReferenceToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkTranslatePrimitiveCharacters;
    }
}