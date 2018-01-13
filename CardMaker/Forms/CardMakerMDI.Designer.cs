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
    partial class CardMakerMDI
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
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.exportProjectThroughPDFSharpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.recentProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawElementBordersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawSelectedElementGuidesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawSelectedElementRotationBoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawFormattedTextWordOutlinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearCacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateIssuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importLayoutsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearGoogleCacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.layoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadReferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorPickerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.layoutTemplatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.illegalFilenameCharacterReplacementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateGoogleCredentialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.samplePDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStripMain
            // 
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.projectToolStripMenuItem,
            this.layoutToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStripMain.Size = new System.Drawing.Size(1112, 24);
            this.menuStripMain.TabIndex = 15;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openProjectToolStripMenuItem,
            this.saveProjectToolStripMenuItem,
            this.saveProjectAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.toolStripMenuItem3,
            this.exportProjectThroughPDFSharpToolStripMenuItem,
            this.toolStripMenuItem1,
            this.recentProjectsToolStripMenuItem,
            this.toolStripMenuItem2,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.newToolStripMenuItem.Text = "&New Project";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openProjectToolStripMenuItem
            // 
            this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
            this.openProjectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openProjectToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.openProjectToolStripMenuItem.Text = "&Open Project...";
            this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.openProjectToolStripMenuItem_Click);
            // 
            // saveProjectToolStripMenuItem
            // 
            this.saveProjectToolStripMenuItem.Name = "saveProjectToolStripMenuItem";
            this.saveProjectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveProjectToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.saveProjectToolStripMenuItem.Text = "&Save Project...";
            this.saveProjectToolStripMenuItem.Click += new System.EventHandler(this.saveProjectToolStripMenuItem_Click);
            // 
            // saveProjectAsToolStripMenuItem
            // 
            this.saveProjectAsToolStripMenuItem.Name = "saveProjectAsToolStripMenuItem";
            this.saveProjectAsToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.saveProjectAsToolStripMenuItem.Text = "Save Project &As...";
            this.saveProjectAsToolStripMenuItem.Click += new System.EventHandler(this.saveProjectAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(241, 6);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItem3.Size = new System.Drawing.Size(244, 22);
            this.toolStripMenuItem3.Text = "Export Project to Images...";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.exportImagesToolStripMenuItem_Click);
            // 
            // exportProjectThroughPDFSharpToolStripMenuItem
            // 
            this.exportProjectThroughPDFSharpToolStripMenuItem.Name = "exportProjectThroughPDFSharpToolStripMenuItem";
            this.exportProjectThroughPDFSharpToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.exportProjectThroughPDFSharpToolStripMenuItem.Text = "Export Project to PDF...";
            this.exportProjectThroughPDFSharpToolStripMenuItem.Click += new System.EventHandler(this.exportProjectToPDFToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(241, 6);
            // 
            // recentProjectsToolStripMenuItem
            // 
            this.recentProjectsToolStripMenuItem.Name = "recentProjectsToolStripMenuItem";
            this.recentProjectsToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.recentProjectsToolStripMenuItem.Text = "Recent Projects";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(241, 6);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(244, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.DropDownOpening += new System.EventHandler(this.editToolStripMenuItem_DropDownOpening);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawElementBordersToolStripMenuItem,
            this.drawSelectedElementGuidesToolStripMenuItem,
            this.drawSelectedElementRotationBoundsToolStripMenuItem,
            this.drawFormattedTextWordOutlinesToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // drawElementBordersToolStripMenuItem
            // 
            this.drawElementBordersToolStripMenuItem.Checked = true;
            this.drawElementBordersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawElementBordersToolStripMenuItem.Name = "drawElementBordersToolStripMenuItem";
            this.drawElementBordersToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.drawElementBordersToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
            this.drawElementBordersToolStripMenuItem.Text = "&Draw Element Borders";
            this.drawElementBordersToolStripMenuItem.Click += new System.EventHandler(this.drawElementBordersToolStripMenuItem_Click);
            // 
            // drawSelectedElementGuidesToolStripMenuItem
            // 
            this.drawSelectedElementGuidesToolStripMenuItem.Checked = true;
            this.drawSelectedElementGuidesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawSelectedElementGuidesToolStripMenuItem.Name = "drawSelectedElementGuidesToolStripMenuItem";
            this.drawSelectedElementGuidesToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
            this.drawSelectedElementGuidesToolStripMenuItem.Text = "Draw Selected Element Guides";
            this.drawSelectedElementGuidesToolStripMenuItem.Click += new System.EventHandler(this.drawSelectedElementGuidesToolStripMenuItem_Click);
            // 
            // drawSelectedElementRotationBoundsToolStripMenuItem
            // 
            this.drawSelectedElementRotationBoundsToolStripMenuItem.Checked = true;
            this.drawSelectedElementRotationBoundsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawSelectedElementRotationBoundsToolStripMenuItem.Name = "drawSelectedElementRotationBoundsToolStripMenuItem";
            this.drawSelectedElementRotationBoundsToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
            this.drawSelectedElementRotationBoundsToolStripMenuItem.Text = "Draw Selected Element Rotation Bounds";
            this.drawSelectedElementRotationBoundsToolStripMenuItem.Click += new System.EventHandler(this.drawSelectedElementRotationBoundsToolStripMenuItem_Click);
            // 
            // drawFormattedTextWordOutlinesToolStripMenuItem
            // 
            this.drawFormattedTextWordOutlinesToolStripMenuItem.Name = "drawFormattedTextWordOutlinesToolStripMenuItem";
            this.drawFormattedTextWordOutlinesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
            this.drawFormattedTextWordOutlinesToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
            this.drawFormattedTextWordOutlinesToolStripMenuItem.Text = "Draw Formatted Text Word Borders";
            this.drawFormattedTextWordOutlinesToolStripMenuItem.Click += new System.EventHandler(this.drawFormattedTextWordBordersToolStripMenuItem_Click);
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearCacheToolStripMenuItem,
            this.updateIssuesToolStripMenuItem,
            this.importLayoutsToolStripMenuItem,
            this.clearGoogleCacheToolStripMenuItem,
            this.projectSettingsToolStripMenuItem});
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.projectToolStripMenuItem.Text = "&Project";
            // 
            // clearCacheToolStripMenuItem
            // 
            this.clearCacheToolStripMenuItem.Name = "clearCacheToolStripMenuItem";
            this.clearCacheToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.clearCacheToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.clearCacheToolStripMenuItem.Text = "&Clear Image Cache";
            this.clearCacheToolStripMenuItem.Click += new System.EventHandler(this.clearCacheToolStripMenuItem_Click);
            // 
            // updateIssuesToolStripMenuItem
            // 
            this.updateIssuesToolStripMenuItem.Name = "updateIssuesToolStripMenuItem";
            this.updateIssuesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.updateIssuesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.updateIssuesToolStripMenuItem.Text = "&Update Known Issues...";
            this.updateIssuesToolStripMenuItem.Click += new System.EventHandler(this.updateIssuesToolStripMenuItem_Click);
            // 
            // importLayoutsToolStripMenuItem
            // 
            this.importLayoutsToolStripMenuItem.Name = "importLayoutsToolStripMenuItem";
            this.importLayoutsToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.importLayoutsToolStripMenuItem.Text = "Import Layouts from Project...";
            this.importLayoutsToolStripMenuItem.Click += new System.EventHandler(this.importLayoutsToolStripMenuItem_Click);
            // 
            // clearGoogleCacheToolStripMenuItem
            // 
            this.clearGoogleCacheToolStripMenuItem.Name = "clearGoogleCacheToolStripMenuItem";
            this.clearGoogleCacheToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.clearGoogleCacheToolStripMenuItem.Text = "Clear All Google Cache Entries";
            this.clearGoogleCacheToolStripMenuItem.Click += new System.EventHandler(this.clearGoogleCacheToolStripMenuItem_Click);
            // 
            // projectSettingsToolStripMenuItem
            // 
            this.projectSettingsToolStripMenuItem.Name = "projectSettingsToolStripMenuItem";
            this.projectSettingsToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.projectSettingsToolStripMenuItem.Text = "Project Settings...";
            this.projectSettingsToolStripMenuItem.Click += new System.EventHandler(this.projectSettingsToolStripMenuItem_Click);
            // 
            // layoutToolStripMenuItem
            // 
            this.layoutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reloadReferencesToolStripMenuItem});
            this.layoutToolStripMenuItem.Name = "layoutToolStripMenuItem";
            this.layoutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.layoutToolStripMenuItem.Text = "Layout";
            // 
            // reloadReferencesToolStripMenuItem
            // 
            this.reloadReferencesToolStripMenuItem.Name = "reloadReferencesToolStripMenuItem";
            this.reloadReferencesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.reloadReferencesToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.reloadReferencesToolStripMenuItem.Text = "Reload References";
            this.reloadReferencesToolStripMenuItem.Click += new System.EventHandler(this.reloadReferencesToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectManagerToolStripMenuItem,
            this.colorPickerToolStripMenuItem,
            this.layoutTemplatesToolStripMenuItem,
            this.illegalFilenameCharacterReplacementToolStripMenuItem,
            this.updateGoogleCredentialsToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // projectManagerToolStripMenuItem
            // 
            this.projectManagerToolStripMenuItem.Name = "projectManagerToolStripMenuItem";
            this.projectManagerToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F7;
            this.projectManagerToolStripMenuItem.Size = new System.Drawing.Size(279, 22);
            this.projectManagerToolStripMenuItem.Text = "Project Manager...";
            this.projectManagerToolStripMenuItem.Click += new System.EventHandler(this.projectManagerToolStripMenuItem_Click);
            // 
            // colorPickerToolStripMenuItem
            // 
            this.colorPickerToolStripMenuItem.Name = "colorPickerToolStripMenuItem";
            this.colorPickerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.colorPickerToolStripMenuItem.Size = new System.Drawing.Size(279, 22);
            this.colorPickerToolStripMenuItem.Text = "Color Picker...";
            this.colorPickerToolStripMenuItem.Click += new System.EventHandler(this.colorPickerToolStripMenuItem_Click);
            // 
            // layoutTemplatesToolStripMenuItem
            // 
            this.layoutTemplatesToolStripMenuItem.Name = "layoutTemplatesToolStripMenuItem";
            this.layoutTemplatesToolStripMenuItem.Size = new System.Drawing.Size(279, 22);
            this.layoutTemplatesToolStripMenuItem.Text = "Remove Layout Templates...";
            this.layoutTemplatesToolStripMenuItem.Click += new System.EventHandler(this.removeLayoutTemplatesToolStripMenuItem_Click);
            // 
            // illegalFilenameCharacterReplacementToolStripMenuItem
            // 
            this.illegalFilenameCharacterReplacementToolStripMenuItem.Name = "illegalFilenameCharacterReplacementToolStripMenuItem";
            this.illegalFilenameCharacterReplacementToolStripMenuItem.Size = new System.Drawing.Size(279, 22);
            this.illegalFilenameCharacterReplacementToolStripMenuItem.Text = "&Illegal File Name Character Replacement...";
            this.illegalFilenameCharacterReplacementToolStripMenuItem.Click += new System.EventHandler(this.illegalFilenameCharacterReplacementToolStripMenuItem_Click);
            // 
            // updateGoogleCredentialsToolStripMenuItem
            // 
            this.updateGoogleCredentialsToolStripMenuItem.Name = "updateGoogleCredentialsToolStripMenuItem";
            this.updateGoogleCredentialsToolStripMenuItem.Size = new System.Drawing.Size(279, 22);
            this.updateGoogleCredentialsToolStripMenuItem.Text = "Update Google Credentials...";
            this.updateGoogleCredentialsToolStripMenuItem.Click += new System.EventHandler(this.updateGoogleCredentialsToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(279, 22);
            this.settingsToolStripMenuItem.Text = "Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.windowToolStripMenuItem.Text = "&Window";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pDFToolStripMenuItem,
            this.samplePDFToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // pDFToolStripMenuItem
            // 
            this.pDFToolStripMenuItem.Name = "pDFToolStripMenuItem";
            this.pDFToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.pDFToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pDFToolStripMenuItem.Text = "&Instructions (PDF)";
            this.pDFToolStripMenuItem.Click += new System.EventHandler(this.pDFToolStripMenuItem_Click);
            // 
            // samplePDFToolStripMenuItem
            // 
            this.samplePDFToolStripMenuItem.Name = "samplePDFToolStripMenuItem";
            this.samplePDFToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.samplePDFToolStripMenuItem.Text = "&Sample (PDF)";
            this.samplePDFToolStripMenuItem.Click += new System.EventHandler(this.samplePDFToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // CardMakerMDI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1112, 696);
            this.Controls.Add(this.menuStripMain);
            this.IsMdiContainer = true;
            this.KeyPreview = true;
            this.Name = "CardMakerMDI";
            this.Text = "MDIParent1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CardMakerMDI_FormClosing);
            this.Load += new System.EventHandler(this.CardMakerMDI_Load);
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProjectAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawElementBordersToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem recentProjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem samplePDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearCacheToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateIssuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem illegalFilenameCharacterReplacementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutTemplatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawFormattedTextWordOutlinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateGoogleCredentialsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem colorPickerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportProjectThroughPDFSharpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importLayoutsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearGoogleCacheToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadReferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawSelectedElementGuidesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem drawSelectedElementRotationBoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectSettingsToolStripMenuItem;
    }
}



