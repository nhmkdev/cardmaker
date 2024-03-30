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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Node0");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Node1");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Node4");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Node7");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Node8");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Node5", new System.Windows.Forms.TreeNode[] {
            treeNode4,
            treeNode5});
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Node6");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Node2", new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode6,
            treeNode7});
            System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Node3");
            this.treeView = new System.Windows.Forms.TreeView();
            this.contextMenuStripProject = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addCardLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCardLayoutFromTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSetProjectNameFormat = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.projectSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.windowsExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripLayout = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.duplicateLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateLayoutCustomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defineAsTemplateLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeCardLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exportCardLayoutAsImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCardLayoutAsPDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.addReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addExcelReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addGoogleSpreadsheetReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSetLayoutExport = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripReference = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setAsDefaultReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeReferenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewEx1 = new Support.UI.TreeViewEx();
            this.contextMenuStripProject.SuspendLayout();
            this.contextMenuStripLayout.SuspendLayout();
            this.contextMenuStripReference.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.AllowDrop = true;
            this.treeView.ContextMenuStrip = this.contextMenuStripProject;
            this.treeView.HideSelection = false;
            this.treeView.LabelEdit = true;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new System.Drawing.Size(192, 335);
            this.treeView.TabIndex = 1;
            this.treeView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_BeforeLabelEdit);
            this.treeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView_AfterLabelEdit);
            this.treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_ItemDrag);
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_DragDrop);
            this.treeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_DragEnter);
            this.treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseClick);
            // 
            // contextMenuStripProject
            // 
            this.contextMenuStripProject.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCardLayoutToolStripMenuItem,
            this.addCardLayoutFromTemplateToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripMenuItemSetProjectNameFormat,
            this.toolStripMenuItem5,
            this.projectSettingsToolStripMenuItem,
            this.toolStripMenuItem6,
            this.windowsExplorerToolStripMenuItem});
            this.contextMenuStripProject.Name = "contextMenuStripTreeView";
            this.contextMenuStripProject.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStripProject.Size = new System.Drawing.Size(255, 132);
            this.contextMenuStripProject.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripTreeView_Opening);
            // 
            // addCardLayoutToolStripMenuItem
            // 
            this.addCardLayoutToolStripMenuItem.Name = "addCardLayoutToolStripMenuItem";
            this.addCardLayoutToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.addCardLayoutToolStripMenuItem.Text = "Add Card Layout...";
            this.addCardLayoutToolStripMenuItem.Click += new System.EventHandler(this.addLayoutToolStripMenuItem_Click);
            // 
            // addCardLayoutFromTemplateToolStripMenuItem
            // 
            this.addCardLayoutFromTemplateToolStripMenuItem.Name = "addCardLayoutFromTemplateToolStripMenuItem";
            this.addCardLayoutFromTemplateToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.addCardLayoutFromTemplateToolStripMenuItem.Text = "Add Card Layout From Template...";
            this.addCardLayoutFromTemplateToolStripMenuItem.Click += new System.EventHandler(this.addCardLayoutFromTemplateToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(251, 6);
            // 
            // toolStripMenuItemSetProjectNameFormat
            // 
            this.toolStripMenuItemSetProjectNameFormat.Name = "toolStripMenuItemSetProjectNameFormat";
            this.toolStripMenuItemSetProjectNameFormat.Size = new System.Drawing.Size(254, 22);
            this.toolStripMenuItemSetProjectNameFormat.Text = "Set Name Format...";
            this.toolStripMenuItemSetProjectNameFormat.Click += new System.EventHandler(this.toolStripMenuItemSetLayoutExport_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(251, 6);
            // 
            // projectSettingsToolStripMenuItem
            // 
            this.projectSettingsToolStripMenuItem.Name = "projectSettingsToolStripMenuItem";
            this.projectSettingsToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.projectSettingsToolStripMenuItem.Text = "Project Settings...";
            this.projectSettingsToolStripMenuItem.Click += new System.EventHandler(this.projectSettingsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(251, 6);
            // 
            // windowsExplorerToolStripMenuItem
            // 
            this.windowsExplorerToolStripMenuItem.Name = "windowsExplorerToolStripMenuItem";
            this.windowsExplorerToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.windowsExplorerToolStripMenuItem.Text = "Show Project Folder...";
            this.windowsExplorerToolStripMenuItem.Click += new System.EventHandler(this.windowsExplorerToolStripMenuItem_Click);
            // 
            // contextMenuStripLayout
            // 
            this.contextMenuStripLayout.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.duplicateLayoutToolStripMenuItem,
            this.duplicateLayoutCustomToolStripMenuItem,
            this.resizeLayoutToolStripMenuItem,
            this.defineAsTemplateLayoutToolStripMenuItem,
            this.removeCardLayoutToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exportCardLayoutAsImagesToolStripMenuItem,
            this.exportCardLayoutAsPDFToolStripMenuItem,
            this.toolStripMenuItem4,
            this.addReferenceToolStripMenuItem,
            this.addExcelReferenceToolStripMenuItem,
            this.addGoogleSpreadsheetReferenceToolStripMenuItem,
            this.toolStripMenuItem3,
            this.toolStripMenuItemSetLayoutExport});
            this.contextMenuStripLayout.Name = "contextMenuStripLayout";
            this.contextMenuStripLayout.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStripLayout.Size = new System.Drawing.Size(269, 264);
            // 
            // duplicateLayoutToolStripMenuItem
            // 
            this.duplicateLayoutToolStripMenuItem.Name = "duplicateLayoutToolStripMenuItem";
            this.duplicateLayoutToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.duplicateLayoutToolStripMenuItem.Text = "Duplicate Layout";
            this.duplicateLayoutToolStripMenuItem.Click += new System.EventHandler(this.duplicateLayoutToolStripMenuItem_Click);
            // 
            // duplicateLayoutCustomToolStripMenuItem
            // 
            this.duplicateLayoutCustomToolStripMenuItem.Name = "duplicateLayoutCustomToolStripMenuItem";
            this.duplicateLayoutCustomToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.duplicateLayoutCustomToolStripMenuItem.Text = "Duplicate Layout (Custom)...";
            this.duplicateLayoutCustomToolStripMenuItem.Click += new System.EventHandler(this.duplicateLayoutCustomToolStripMenuItem_Click);
            // 
            // resizeLayoutToolStripMenuItem
            // 
            this.resizeLayoutToolStripMenuItem.Name = "resizeLayoutToolStripMenuItem";
            this.resizeLayoutToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.resizeLayoutToolStripMenuItem.Text = "Resize Layout...";
            this.resizeLayoutToolStripMenuItem.Click += new System.EventHandler(this.resizeLayoutToolStripMenuItem_Click);
            // 
            // defineAsTemplateLayoutToolStripMenuItem
            // 
            this.defineAsTemplateLayoutToolStripMenuItem.Name = "defineAsTemplateLayoutToolStripMenuItem";
            this.defineAsTemplateLayoutToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.defineAsTemplateLayoutToolStripMenuItem.Text = "Define As Template Layout...";
            this.defineAsTemplateLayoutToolStripMenuItem.Click += new System.EventHandler(this.defineAsTemplateLayoutToolStripMenuItem_Click);
            // 
            // removeCardLayoutToolStripMenuItem
            // 
            this.removeCardLayoutToolStripMenuItem.Name = "removeCardLayoutToolStripMenuItem";
            this.removeCardLayoutToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.removeCardLayoutToolStripMenuItem.Text = "Remove Card Layout";
            this.removeCardLayoutToolStripMenuItem.Click += new System.EventHandler(this.removeLayoutToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(265, 6);
            // 
            // exportCardLayoutAsImagesToolStripMenuItem
            // 
            this.exportCardLayoutAsImagesToolStripMenuItem.Name = "exportCardLayoutAsImagesToolStripMenuItem";
            this.exportCardLayoutAsImagesToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.exportCardLayoutAsImagesToolStripMenuItem.Text = "Export Card Layout as Images...";
            this.exportCardLayoutAsImagesToolStripMenuItem.Click += new System.EventHandler(this.exportCardLayoutAsImagesToolStripMenuItem_Click);
            // 
            // exportCardLayoutAsPDFToolStripMenuItem
            // 
            this.exportCardLayoutAsPDFToolStripMenuItem.Name = "exportCardLayoutAsPDFToolStripMenuItem";
            this.exportCardLayoutAsPDFToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.exportCardLayoutAsPDFToolStripMenuItem.Text = "Export Card Layout as PDF...";
            this.exportCardLayoutAsPDFToolStripMenuItem.Click += new System.EventHandler(this.exportCardLayoutAsPDFToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(265, 6);
            // 
            // addReferenceToolStripMenuItem
            // 
            this.addReferenceToolStripMenuItem.Name = "addReferenceToolStripMenuItem";
            this.addReferenceToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.addReferenceToolStripMenuItem.Text = "Add Reference...";
            this.addReferenceToolStripMenuItem.Click += new System.EventHandler(this.addReferenceToolStripMenuItem_Click);
            // 
            // addExcelReferenceToolStripMenuItem
            // 
            this.addExcelReferenceToolStripMenuItem.Name = "addExcelReferenceToolStripMenuItem";
            this.addExcelReferenceToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.addExcelReferenceToolStripMenuItem.Text = "Add Excel Reference...";
            this.addExcelReferenceToolStripMenuItem.Click += new System.EventHandler(this.addExcelReferenceToolStripMenuItem_Click);
            // 
            // addGoogleSpreadsheetReferenceToolStripMenuItem
            // 
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Name = "addGoogleSpreadsheetReferenceToolStripMenuItem";
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Size = new System.Drawing.Size(268, 22);
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Text = "Add Google Spreadsheet Reference...";
            this.addGoogleSpreadsheetReferenceToolStripMenuItem.Click += new System.EventHandler(this.addGoogleSpreadsheetReferenceToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(265, 6);
            // 
            // toolStripMenuItemSetLayoutExport
            // 
            this.toolStripMenuItemSetLayoutExport.Name = "toolStripMenuItemSetLayoutExport";
            this.toolStripMenuItemSetLayoutExport.Size = new System.Drawing.Size(268, 22);
            this.toolStripMenuItemSetLayoutExport.Text = "Configure Layout Export...";
            this.toolStripMenuItemSetLayoutExport.Click += new System.EventHandler(this.toolStripMenuItemSetLayoutExport_Click);
            // 
            // contextMenuStripReference
            // 
            this.contextMenuStripReference.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setAsDefaultReferenceToolStripMenuItem,
            this.removeReferenceToolStripMenuItem});
            this.contextMenuStripReference.Name = "contextMenuStripReference";
            this.contextMenuStripReference.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStripReference.Size = new System.Drawing.Size(203, 48);
            // 
            // setAsDefaultReferenceToolStripMenuItem
            // 
            this.setAsDefaultReferenceToolStripMenuItem.Name = "setAsDefaultReferenceToolStripMenuItem";
            this.setAsDefaultReferenceToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.setAsDefaultReferenceToolStripMenuItem.Text = "Set As Default Reference";
            this.setAsDefaultReferenceToolStripMenuItem.Click += new System.EventHandler(this.setAsDefaultReferenceToolStripMenuItem_Click);
            // 
            // removeReferenceToolStripMenuItem
            // 
            this.removeReferenceToolStripMenuItem.Name = "removeReferenceToolStripMenuItem";
            this.removeReferenceToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.removeReferenceToolStripMenuItem.Text = "Remove Reference";
            this.removeReferenceToolStripMenuItem.Click += new System.EventHandler(this.removeReferenceToolStripMenuItem_Click);
            // 
            // treeViewEx1
            // 
            this.treeViewEx1.CheckBoxes = true;
            this.treeViewEx1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeViewEx1.Location = new System.Drawing.Point(238, 33);
            this.treeViewEx1.Name = "treeViewEx1";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Node0";
            treeNode2.Name = "Node1";
            treeNode2.Text = "Node1";
            treeNode3.Name = "Node4";
            treeNode3.Text = "Node4";
            treeNode4.Name = "Node7";
            treeNode4.Text = "Node7";
            treeNode5.Name = "Node8";
            treeNode5.Text = "Node8";
            treeNode6.Name = "Node5";
            treeNode6.Text = "Node5";
            treeNode7.Name = "Node6";
            treeNode7.Text = "Node6";
            treeNode8.Name = "Node2";
            treeNode8.Text = "Node2";
            treeNode9.Name = "Node3";
            treeNode9.Text = "Node3";
            this.treeViewEx1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode8,
            treeNode9});
            this.treeViewEx1.Size = new System.Drawing.Size(224, 272);
            this.treeViewEx1.TabIndex = 37;
            // 
            // MDIProject
            // 
            this.ClientSize = new System.Drawing.Size(506, 335);
            this.Controls.Add(this.treeViewEx1);
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
        private ToolStripMenuItem addExcelReferenceToolStripMenuItem;
        private ToolStripMenuItem exportCardLayoutAsImagesToolStripMenuItem;
        private ToolStripMenuItem setAsDefaultReferenceToolStripMenuItem;
        private ToolStripMenuItem removeReferenceToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItemSetLayoutExport;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripMenuItem addGoogleSpreadsheetReferenceToolStripMenuItem;
        private ToolStripMenuItem exportCardLayoutAsPDFToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripMenuItem projectSettingsToolStripMenuItem;
        private ToolStripMenuItem resizeLayoutToolStripMenuItem;
        private ToolStripMenuItem duplicateLayoutCustomToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem6;
        private ToolStripMenuItem windowsExplorerToolStripMenuItem;
        private Support.UI.TreeViewEx treeViewEx1;
    }
}