////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2017 Tim Stair
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
    partial class MDICanvas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MDICanvas));
            this.numericUpDownZoom = new System.Windows.Forms.NumericUpDown();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.verticalCenterButton = new System.Windows.Forms.ToolStripButton();
            this.horizontalCenterButton = new System.Windows.Forms.ToolStripButton();
            this.customAlignButton = new System.Windows.Forms.ToolStripButton();
            this.customVerticalAlignButton = new System.Windows.Forms.ToolStripButton();
            this.customHoritonalAlignButton = new System.Windows.Forms.ToolStripButton();
            this.panelCardCanvas = new Support.UI.PanelEx();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoom)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // numericUpDownZoom
            // 
            this.numericUpDownZoom.DecimalPlaces = 2;
            this.numericUpDownZoom.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownZoom.Location = new System.Drawing.Point(4, 2);
            this.numericUpDownZoom.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            131072});
            this.numericUpDownZoom.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownZoom.Name = "numericUpDownZoom";
            this.numericUpDownZoom.Size = new System.Drawing.Size(59, 20);
            this.numericUpDownZoom.TabIndex = 0;
            this.numericUpDownZoom.TabStop = false;
            this.numericUpDownZoom.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.numericUpDownZoom.ValueChanged += new System.EventHandler(this.numericUpDownZoom_ValueChanged);
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripSeparator1,
            this.verticalCenterButton,
            this.customVerticalAlignButton,
            this.horizontalCenterButton,
            this.customHoritonalAlignButton,
            this.customAlignButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(292, 25);
            this.toolStrip.TabIndex = 2;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.AutoSize = false;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(64, 22);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // verticalCenterButton
            // 
            this.verticalCenterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.verticalCenterButton.Image = ((System.Drawing.Image)(resources.GetObject("verticalCenterButton.Image")));
            this.verticalCenterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.verticalCenterButton.Name = "verticalCenterButton";
            this.verticalCenterButton.Size = new System.Drawing.Size(23, 22);
            this.verticalCenterButton.Text = "toolStripButton1";
            this.verticalCenterButton.ToolTipText = "Center Elements Vertically";
            this.verticalCenterButton.Click += new System.EventHandler(this.verticalCenterButton_Click);
            // 
            // horizontalCenterButton
            // 
            this.horizontalCenterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.horizontalCenterButton.Image = ((System.Drawing.Image)(resources.GetObject("horizontalCenterButton.Image")));
            this.horizontalCenterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.horizontalCenterButton.Name = "horizontalCenterButton";
            this.horizontalCenterButton.Size = new System.Drawing.Size(23, 22);
            this.horizontalCenterButton.Text = "toolStripButton1";
            this.horizontalCenterButton.ToolTipText = "Center Elements Horizontally";
            this.horizontalCenterButton.Click += new System.EventHandler(this.horizontalCenterButton_Click);
            // 
            // customAlignButton
            // 
            this.customAlignButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.customAlignButton.Image = ((System.Drawing.Image)(resources.GetObject("customAlignButton.Image")));
            this.customAlignButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.customAlignButton.Name = "customAlignButton";
            this.customAlignButton.Size = new System.Drawing.Size(23, 22);
            this.customAlignButton.Text = "toolStripButton1";
            this.customAlignButton.ToolTipText = "Custom Align Elements";
            this.customAlignButton.Click += new System.EventHandler(this.customAlignElementButton_Click);
            // 
            // customVerticalAlignButton
            // 
            this.customVerticalAlignButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.customVerticalAlignButton.Image = ((System.Drawing.Image)(resources.GetObject("customVerticalAlignButton.Image")));
            this.customVerticalAlignButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.customVerticalAlignButton.Name = "customVerticalAlignButton";
            this.customVerticalAlignButton.Size = new System.Drawing.Size(23, 22);
            this.customVerticalAlignButton.Text = "toolStripButton1";
            this.customVerticalAlignButton.ToolTipText = "Custom Align Elements Vertically";
            this.customVerticalAlignButton.Click += new System.EventHandler(this.customVerticalAlignButton_Click);
            // 
            // customHoritonalAlignButton
            // 
            this.customHoritonalAlignButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.customHoritonalAlignButton.Image = ((System.Drawing.Image)(resources.GetObject("customHoritonalAlignButton.Image")));
            this.customHoritonalAlignButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.customHoritonalAlignButton.Name = "customHoritonalAlignButton";
            this.customHoritonalAlignButton.Size = new System.Drawing.Size(23, 22);
            this.customHoritonalAlignButton.Text = "toolStripButton2";
            this.customHoritonalAlignButton.ToolTipText = "Custom Align Elements Horizontally";
            this.customHoritonalAlignButton.Click += new System.EventHandler(this.customHoritonalAlignButton_Click);
            // 
            // panelCardCanvas
            // 
            this.panelCardCanvas.AutoScroll = true;
            this.panelCardCanvas.DisableScrollToControl = true;
            this.panelCardCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCardCanvas.Location = new System.Drawing.Point(0, 25);
            this.panelCardCanvas.Name = "panelCardCanvas";
            this.panelCardCanvas.Size = new System.Drawing.Size(292, 248);
            this.panelCardCanvas.TabIndex = 1;
            // 
            // MDICanvas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.numericUpDownZoom);
            this.Controls.Add(this.panelCardCanvas);
            this.Controls.Add(this.toolStrip);
            this.Name = "MDICanvas";
            this.ShowIcon = false;
            this.Text = " Canvas";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoom)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Support.UI.PanelEx panelCardCanvas;
        private System.Windows.Forms.NumericUpDown numericUpDownZoom;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton verticalCenterButton;
        private System.Windows.Forms.ToolStripButton horizontalCenterButton;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton customAlignButton;
        private System.Windows.Forms.ToolStripButton customVerticalAlignButton;
        private System.Windows.Forms.ToolStripButton customHoritonalAlignButton;
    }
}