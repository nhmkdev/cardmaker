////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CardMaker.XML;
using Support.IO;

namespace CardMaker.Forms
{
    partial class MDIProject
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.components = new Container();
            this.treeView = new TreeView();
            this.contextMenuStripProject = new ContextMenuStrip(this.components);
            this.addCardLayoutToolStripMenuItem = new ToolStripMenuItem();
            this.addCardLayoutFromTemplateToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem1 = new ToolStripSeparator();
            this.toolStripMenuItemSetProjectNameFormat = new ToolStripMenuItem();
            this.contextMenuStripLayout = new ContextMenuStrip(this.components);
            this.duplicateLayoutToolStripMenuItem = new ToolStripMenuItem();
            this.defineAsTemplateLayoutToolStripMenuItem = new ToolStripMenuItem();
            this.removeCardLayoutToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem2 = new ToolStripSeparator();
            this.exportCardLayoutAsImagesToolStripMenuItem = new ToolStripMenuItem();
            this.exportCardLayoutAsPDFToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem4 = new ToolStripSeparator();
            this.addReferenceToolStripMenuItem = new ToolStripMenuItem();
            this.addGoogleSpreadsheetReferenceToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem3 = new ToolStripSeparator();
            this.toolStripMenuItemSetLayoutNameFormat = new ToolStripMenuItem();
            this.contextMenuStripReference = new ContextMenuStrip(this.components);
            this.setAsDefaultReferenceToolStripMenuItem = new ToolStripMenuItem();
            this.removeReferenceToolStripMenuItem = new ToolStripMenuItem();
            this.contextMenuStripProject.SuspendLayout();
            this.contextMenuStripLayout.SuspendLayout();
            this.contextMenuStripReference.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.AllowDrop = true;
            this.treeView.ContextMenuStrip = this.contextMenuStripProject;
            this.treeView.Dock = DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.LabelEdit = true;
            this.treeView.Location = new Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new Size(192, 335);
            this.treeView.TabIndex = 1;
            this.treeView.BeforeLabelEdit += new NodeLabelEditEventHandler(this.treeView_BeforeLabelEdit);
            this.treeView.AfterLabelEdit += new NodeLabelEditEventHandler(this.treeView_AfterLabelEdit);
            this.treeView.ItemDrag += new ItemDragEventHandler(this.treeView_ItemDrag);
            this.treeView.AfterSelect += new TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.DragDrop += new DragEventHandler(this.treeView_DragDrop);
            this.treeView.DragEnter += new DragEventHandler(this.treeView_DragEnter);
            this.treeView.MouseClick += new MouseEventHandler(this.treeView_MouseClick);
            // 
            // contextMenuStripProject
            // 
            this.contextMenuStripProject.Items.AddRange(new ToolStripItem[] {
            this.addCardLayoutToolStripMenuItem,
            this.addCardLayoutFromTemplateToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripMenuItemSetProjectNameFormat});
            this.contextMenuStripProject.Name = "contextMenuStripTreeView";
            this.contextMenuStripProject.RenderMode = ToolStripRenderMode.System;
            this.contextMenuStripProject.Size = new Size(242, 76);
            this.contextMenuStripProject.Opening += new CancelEventHandler(this.contextMenuStripTreeView_Opening);
            // 
            // addCardLayoutToolStripMenuItem
            // 
            this.addCardLayoutToolStripMenuItem.Name = "addCardLayoutToolStripMenuItem";
            this.addCardLayoutToolStripMenuItem.Size = new Size(241, 22);
            this.addCardLayoutToolStripMenuItem.Text = "Add Card Layout...";
            this.addCardLayoutToolStripMenuItem.Click += new EventHandler(this.addLayoutToolStripMenuItem_Click);
            // 
            // addCardLayoutFromTemplateToolStripMenuItem
            // 
            this.addCardLayoutFromTemplateToolStripMenuItem.Name = "addCardLayoutFromTemplateToolStripMenuItem";
            this.addCardLayoutFromTemplateToolStripMenuItem.Size = new Size(241, 22);
            this.addCardLayoutFromTemplateToolStripMenuItem.Text = "Add Card Layout From Template...";
            this.addCardLayoutFromTemplateToolStripMenuItem.Click += new EventHandler(this.addCardLayoutFromTemplateToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new Size(238, 6);
            // 
            // toolStripMenuItemSetProjectNameFormat
            // 
            this.toolStripMenuItemSetProjectNameFormat.Name = "toolStripMenuItemSetProjectNameFormat";
            this.toolStripMenuItemSetProjectNameFormat.Size = new Size(241, 22);
            this.toolStripMenuItemSetProjectNameFormat.Text = "Set Name Format...";
            this.toolStripMenuItemSetProjectNameFormat.Click += new EventHandler(this.setNameFormatToolStripMenuItem_Click);
            // 
            // contextMenuStripLayout
            // 
            this.contextMenuStripLayout.Items.AddRange(new ToolStripItem[] {
            this.duplicateLayoutToolStripMenuItem,
            this.defineAsTemplateLayoutToolStripMenuItem,
            this.removeCardLayoutToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exportCardLayoutAsImagesToolStripMenuItem,
            this.exportCardLayoutAsPDFToolStripMenuItem,
            this.toolStripMenuItem4,
            this.addReferenceToolStripMenuItem,
            this.addGoogleSpreadsheetReferenceToolStripMenuItem,
            this.toolStripMenuItem3,
            this.toolStripMenuItemSetLayoutNameFormat});
            this.contextMenuStripLayout.Name = "contextMenuStripLayout";
            this.contextMenuStripLayout.RenderMode = ToolStripRenderMode.System;
            this.contextMenuStripLayout.Size = new Size(259, 220);
            // 
            // duplicateLayoutToolStripMenuItem
            // 
            this.duplicateLayoutToolStripMenuItem.Name = "duplicateLayoutToolStripMenuItem";
            this.duplicateLayoutToolStripMenuItem.Size = new Size(258, 22);
            this.duplicateLayoutToolStripMenuItem.Text = "Duplicate Layout";
            this.duplicateLayoutToolStripMenuItem.Click += new EventHandler(this.duplicateLayoutToolStripMenuItem_Click);
            // 
            // defineAsTemplateLayoutToolStripMenuItem
            // 
            this.defineAsTemplateLayoutToolStripMenuItem.Name = "defineAsTemplateLayoutToolStripMenuItem";
            this.defineAsTemplateLayoutToolStripMenuItem.Size = new Size(258, 22);
            this.defineAsTemplateLayoutToolStripMenuItem.Text = "Define As Template Layout...";
            this.defineAsTemplateLayoutToolStripMenuItem.Click += new EventHandler(this.defineAsTemplateLayoutToolStripMenuItem_Click);
            // 
            // removeCardLayoutToolStripMenuItem
            // 
            this.removeCardLayoutToolStripMenuItem.Name = "removeCardLayoutToolStripMenuItem";
            this.removeCardLayoutToolStripMenuItem.Size = new Size(258, 22);
            this.removeCardLayoutToolStripMenuItem.Text = "Remove Card Layout";
            this.removeCardLayoutToolStripMenuItem.Click += new EventHandler(this.removeLayoutToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new Size(255, 6);
            // 
            // exportCardLayoutAsImagesToolStripMenuItem
            // 
            this.exportCardLayoutAsImagesToolStripMenuItem.Name = "exportCardLayoutAsImagesToolStripMenuItem";
            this.exportCardLayoutAsImagesToolStripMenuItem.Size = new Size(258, 22);
            this.exportCardLayoutAsImagesToolStripMenuItem.Text = "Export Card Layout as Images...";
            this.exportCardLayoutAsImagesToolStripMenuItem.Click += new EventHandler(this.exportCardLayoutAsImagesToolStripMenuItem_Click);
            // 
            // exportCardLayoutAsPDFToolStripMenuItem
            // 
            this.exportCardLayoutAsPDFToolStripMenuItem.Name = "exportCardLayoutAsPDFToolStripMenuItem";
            this.exportCardLayoutAsPDFToolStripMenuItem.Size = new Size(258, 22);
            this.exportCardLayoutAsPDFToolStripMenuItem.Text = "Export Card Layout as PDF...";
            this.exportCardLayoutAsPDFToolStripMenuItem.Click += new EventHandler(this.exportCardLayoutAsPDFToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new Size(255, 6);
            // 
            // addReferenceToolStripMenuItem
            // 
            this.addReferenceToolStripMenuItem.Name = "addReferenceToolStripMenuItem";
            this.addReferenceToolStripMenuItem.Size = new Size(258, 22);
            this.addReferenceToolStripMenuItem.Text = "Add Reference...";
            this.addReferenceToolStripMenuItem.Click += new EventHandler(this.addReferenceToolStripMenuItem_Click);
            // 
            // addGoogleSpreadsheetReferenceToolStripMenuItem
            // 
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Name = "addGoogleSpreadsheetReferenceToolStripMenuItem";
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Size = new Size(258, 22);
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Text = "Add Google Spreadsheet Reference...";
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Click += new EventHandler(this.addGoogleSpreadsheetReferenceToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new Size(255, 6);
            // 
            // toolStripMenuItemSetLayoutNameFormat
            // 
            this.toolStripMenuItemSetLayoutNameFormat.Name = "toolStripMenuItemSetLayoutNameFormat";
            this.toolStripMenuItemSetLayoutNameFormat.Size = new Size(258, 22);
            this.toolStripMenuItemSetLayoutNameFormat.Text = "Configure Layout Export...";
            this.toolStripMenuItemSetLayoutNameFormat.Click += new EventHandler(this.setNameFormatToolStripMenuItem_Click);
            // 
            // contextMenuStripReference
            // 
            this.contextMenuStripReference.Items.AddRange(new ToolStripItem[] {
            this.setAsDefaultReferenceToolStripMenuItem,
            this.removeReferenceToolStripMenuItem});
            this.contextMenuStripReference.Name = "contextMenuStripReference";
            this.contextMenuStripReference.RenderMode = ToolStripRenderMode.System;
            this.contextMenuStripReference.Size = new Size(197, 48);
            // 
            // setAsDefaultReferenceToolStripMenuItem
            // 
            this.setAsDefaultReferenceToolStripMenuItem.Name = "setAsDefaultReferenceToolStripMenuItem";
            this.setAsDefaultReferenceToolStripMenuItem.Size = new Size(196, 22);
            this.setAsDefaultReferenceToolStripMenuItem.Text = "Set As Default Reference";
            this.setAsDefaultReferenceToolStripMenuItem.Click += new EventHandler(this.setAsDefaultReferenceToolStripMenuItem_Click);
            // 
            // removeReferenceToolStripMenuItem
            // 
            this.removeReferenceToolStripMenuItem.Name = "removeReferenceToolStripMenuItem";
            this.removeReferenceToolStripMenuItem.Size = new Size(196, 22);
            this.removeReferenceToolStripMenuItem.Text = "Remove Reference";
            this.removeReferenceToolStripMenuItem.Click += new EventHandler(this.removeReferenceToolStripMenuItem_Click);
            // 
            // MDIProject
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(192, 335);
            this.Controls.Add(this.treeView);
            this.Name = "MDIProject";
            this.ShowIcon = false;
            this.Text = " Project";
            this.contextMenuStripProject.ResumeLayout(false);
            this.contextMenuStripLayout.ResumeLayout(false);
            this.contextMenuStripReference.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TreeView treeView;
        private ContextMenuStrip contextMenuStripProject;
        private ToolStripMenuItem addCardLayoutToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItemSetProjectNameFormat;
        private ToolStripMenuItem addCardLayoutFromTemplateToolStripMenuItem;
        private ContextMenuStrip contextMenuStripLayout;
        private ContextMenuStrip contextMenuStripReference;
        private ToolStripMenuItem duplicateLayoutToolStripMenuItem;
        private ToolStripMenuItem removeCardLayoutToolStripMenuItem;
        private ToolStripMenuItem defineAsTemplateLayoutToolStripMenuItem;
        private ToolStripMenuItem addReferenceToolStripMenuItem;
        private ToolStripMenuItem exportCardLayoutAsImagesToolStripMenuItem;
        private ToolStripMenuItem setAsDefaultReferenceToolStripMenuItem;
        private ToolStripMenuItem removeReferenceToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItemSetLayoutNameFormat;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripMenuItem addGoogleSpreadsheetReferenceToolStripMenuItem;
        private ToolStripMenuItem exportCardLayoutAsPDFToolStripMenuItem;
    }
}