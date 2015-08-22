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
            this.numericUpDownZoom = new System.Windows.Forms.NumericUpDown();
            this.panelCardCanvas = new Support.UI.PanelEx();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDownZoom
            // 
            this.numericUpDownZoom.DecimalPlaces = 2;
            this.numericUpDownZoom.Dock = System.Windows.Forms.DockStyle.Top;
            this.numericUpDownZoom.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericUpDownZoom.Location = new System.Drawing.Point(0, 0);
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
            this.numericUpDownZoom.Size = new System.Drawing.Size(292, 20);
            this.numericUpDownZoom.TabIndex = 0;
            this.numericUpDownZoom.TabStop = false;
            this.numericUpDownZoom.Value = new decimal(new int[] {
            100,
            0,
            0,
            131072});
            this.numericUpDownZoom.ValueChanged += new System.EventHandler(this.numericUpDownZoom_ValueChanged);
            // 
            // panelCardCanvas
            // 
            this.panelCardCanvas.AutoScroll = true;
            this.panelCardCanvas.DisableScrollToControl = true;
            this.panelCardCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCardCanvas.Location = new System.Drawing.Point(0, 20);
            this.panelCardCanvas.Name = "panelCardCanvas";
            this.panelCardCanvas.Size = new System.Drawing.Size(292, 253);
            this.panelCardCanvas.TabIndex = 1;
            // 
            // MDICanvas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.panelCardCanvas);
            this.Controls.Add(this.numericUpDownZoom);
            this.Name = "MDICanvas";
            this.ShowIcon = false;
            this.Text = " Canvas";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZoom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Support.UI.PanelEx panelCardCanvas;
        private System.Windows.Forms.NumericUpDown numericUpDownZoom;



    }
}