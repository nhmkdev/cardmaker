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

namespace CardMaker.Forms
{
    partial class MDIPreview
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
            this.picturePreview = new System.Windows.Forms.PictureBox();
            this.numericZoom = new System.Windows.Forms.NumericUpDown();
            this.panel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picturePreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZoom)).BeginInit();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // picturePreview
            // 
            this.picturePreview.Location = new System.Drawing.Point(0, 0);
            this.picturePreview.Name = "picturePreview";
            this.picturePreview.Size = new System.Drawing.Size(59, 130);
            this.picturePreview.TabIndex = 1;
            this.picturePreview.TabStop = false;
            // 
            // numericZoom
            // 
            this.numericZoom.DecimalPlaces = 2;
            this.numericZoom.Dock = System.Windows.Forms.DockStyle.Top;
            this.numericZoom.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericZoom.Location = new System.Drawing.Point(0, 0);
            this.numericZoom.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.numericZoom.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            this.numericZoom.Name = "numericZoom";
            this.numericZoom.Size = new System.Drawing.Size(248, 20);
            this.numericZoom.TabIndex = 2;
            this.numericZoom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericZoom.Value = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericZoom.ValueChanged += new System.EventHandler(this.numericZoom_ValueChanged);
            // 
            // panel
            // 
            this.panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel.AutoScroll = true;
            this.panel.Controls.Add(this.picturePreview);
            this.panel.Location = new System.Drawing.Point(0, 26);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(248, 248);
            this.panel.TabIndex = 3;
            this.panel.Resize += new System.EventHandler(this.panel_Resize);
            // 
            // MDIPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 276);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.numericZoom);
            this.Name = "MDIPreview";
            this.ShowIcon = false;
            this.Text = " Preview";
            this.VisibleChanged += new System.EventHandler(this.MDIPreview_VisibleChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MDIPreview_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.picturePreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZoom)).EndInit();
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.NumericUpDown numericZoom;
        private System.Windows.Forms.Panel panel;
    }
}