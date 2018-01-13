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
    partial class RGBColorSelectDialog
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
            this.panelColor = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.numericRed = new System.Windows.Forms.NumericUpDown();
            this.numericBlue = new System.Windows.Forms.NumericUpDown();
            this.numericGreen = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.panelPreviousColors = new System.Windows.Forms.Panel();
            this.toolTipPreviouscolor = new System.Windows.Forms.ToolTip(this.components);
            this.txtHexColor = new System.Windows.Forms.TextBox();
            this.pictureYellowToRed = new Support.UI.PictureBoxSelectable();
            this.pictureGreenToYellow = new Support.UI.PictureBoxSelectable();
            this.pictureTealToGreen = new Support.UI.PictureBoxSelectable();
            this.pictureBlueToTeal = new Support.UI.PictureBoxSelectable();
            this.picturePurpleToBlue = new Support.UI.PictureBoxSelectable();
            this.pictureRedToPurple = new Support.UI.PictureBoxSelectable();
            this.pictureColorHue = new Support.UI.PictureBoxSelectable();
            this.checkBoxAddZeroX = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureYellowToRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureGreenToYellow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureTealToGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBlueToTeal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePurpleToBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureRedToPurple)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureColorHue)).BeginInit();
            this.SuspendLayout();
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(12, 300);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(64, 59);
            this.panelColor.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(320, 336);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(391, 336);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // numericRed
            // 
            this.numericRed.BackColor = System.Drawing.Color.Tomato;
            this.numericRed.Location = new System.Drawing.Point(85, 300);
            this.numericRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericRed.Name = "numericRed";
            this.numericRed.Size = new System.Drawing.Size(46, 20);
            this.numericRed.TabIndex = 2;
            this.numericRed.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
            // 
            // numericBlue
            // 
            this.numericBlue.BackColor = System.Drawing.Color.CornflowerBlue;
            this.numericBlue.Location = new System.Drawing.Point(189, 300);
            this.numericBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericBlue.Name = "numericBlue";
            this.numericBlue.Size = new System.Drawing.Size(46, 20);
            this.numericBlue.TabIndex = 4;
            this.numericBlue.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
            // 
            // numericGreen
            // 
            this.numericGreen.BackColor = System.Drawing.Color.Chartreuse;
            this.numericGreen.Location = new System.Drawing.Point(137, 300);
            this.numericGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericGreen.Name = "numericGreen";
            this.numericGreen.Size = new System.Drawing.Size(46, 20);
            this.numericGreen.TabIndex = 3;
            this.numericGreen.ValueChanged += new System.EventHandler(this.numeric_ValueChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(82, 276);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 21);
            this.label1.TabIndex = 9;
            this.label1.Text = "Fine Tune:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelPreviousColors
            // 
            this.panelPreviousColors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPreviousColors.Location = new System.Drawing.Point(276, 277);
            this.panelPreviousColors.Name = "panelPreviousColors";
            this.panelPreviousColors.Size = new System.Drawing.Size(180, 20);
            this.panelPreviousColors.TabIndex = 4;
            this.panelPreviousColors.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelPreviousColors_MouseDown);
            this.panelPreviousColors.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelPreviousColors_MouseMove);
            // 
            // toolTipPreviouscolor
            // 
            this.toolTipPreviouscolor.AutomaticDelay = 0;
            this.toolTipPreviouscolor.AutoPopDelay = 0;
            this.toolTipPreviouscolor.InitialDelay = 0;
            this.toolTipPreviouscolor.IsBalloon = true;
            this.toolTipPreviouscolor.ReshowDelay = 100;
            this.toolTipPreviouscolor.ToolTipTitle = "Color";
            this.toolTipPreviouscolor.UseAnimation = false;
            this.toolTipPreviouscolor.UseFading = false;
            // 
            // txtHexColor
            // 
            this.txtHexColor.Location = new System.Drawing.Point(85, 327);
            this.txtHexColor.Name = "txtHexColor";
            this.txtHexColor.ReadOnly = true;
            this.txtHexColor.Size = new System.Drawing.Size(150, 20);
            this.txtHexColor.TabIndex = 15;
            // 
            // pictureYellowToRed
            // 
            this.pictureYellowToRed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureYellowToRed.Location = new System.Drawing.Point(431, 12);
            this.pictureYellowToRed.Name = "pictureYellowToRed";
            this.pictureYellowToRed.SelectionIndicator = Support.UI.PictureBoxSelectable.IndicatorType.None;
            this.pictureYellowToRed.Size = new System.Drawing.Size(25, 259);
            this.pictureYellowToRed.TabIndex = 14;
            this.pictureYellowToRed.TabStop = false;
            this.pictureYellowToRed.Xposition = 0;
            this.pictureYellowToRed.Yposition = 0;
            this.pictureYellowToRed.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            this.pictureYellowToRed.MouseEnter += new System.EventHandler(this.pictureColor_MouseEnter);
            this.pictureYellowToRed.MouseLeave += new System.EventHandler(this.pictureColor_MouseLeave);
            this.pictureYellowToRed.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            // 
            // pictureGreenToYellow
            // 
            this.pictureGreenToYellow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureGreenToYellow.Location = new System.Drawing.Point(400, 12);
            this.pictureGreenToYellow.Name = "pictureGreenToYellow";
            this.pictureGreenToYellow.SelectionIndicator = Support.UI.PictureBoxSelectable.IndicatorType.None;
            this.pictureGreenToYellow.Size = new System.Drawing.Size(25, 259);
            this.pictureGreenToYellow.TabIndex = 13;
            this.pictureGreenToYellow.TabStop = false;
            this.pictureGreenToYellow.Xposition = 0;
            this.pictureGreenToYellow.Yposition = 0;
            this.pictureGreenToYellow.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            this.pictureGreenToYellow.MouseEnter += new System.EventHandler(this.pictureColor_MouseEnter);
            this.pictureGreenToYellow.MouseLeave += new System.EventHandler(this.pictureColor_MouseLeave);
            this.pictureGreenToYellow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            // 
            // pictureTealToGreen
            // 
            this.pictureTealToGreen.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureTealToGreen.Location = new System.Drawing.Point(369, 12);
            this.pictureTealToGreen.Name = "pictureTealToGreen";
            this.pictureTealToGreen.SelectionIndicator = Support.UI.PictureBoxSelectable.IndicatorType.None;
            this.pictureTealToGreen.Size = new System.Drawing.Size(25, 259);
            this.pictureTealToGreen.TabIndex = 12;
            this.pictureTealToGreen.TabStop = false;
            this.pictureTealToGreen.Xposition = 0;
            this.pictureTealToGreen.Yposition = 0;
            this.pictureTealToGreen.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            this.pictureTealToGreen.MouseEnter += new System.EventHandler(this.pictureColor_MouseEnter);
            this.pictureTealToGreen.MouseLeave += new System.EventHandler(this.pictureColor_MouseLeave);
            this.pictureTealToGreen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            // 
            // pictureBlueToTeal
            // 
            this.pictureBlueToTeal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBlueToTeal.Location = new System.Drawing.Point(338, 12);
            this.pictureBlueToTeal.Name = "pictureBlueToTeal";
            this.pictureBlueToTeal.SelectionIndicator = Support.UI.PictureBoxSelectable.IndicatorType.None;
            this.pictureBlueToTeal.Size = new System.Drawing.Size(25, 259);
            this.pictureBlueToTeal.TabIndex = 11;
            this.pictureBlueToTeal.TabStop = false;
            this.pictureBlueToTeal.Xposition = 0;
            this.pictureBlueToTeal.Yposition = 0;
            this.pictureBlueToTeal.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            this.pictureBlueToTeal.MouseEnter += new System.EventHandler(this.pictureColor_MouseEnter);
            this.pictureBlueToTeal.MouseLeave += new System.EventHandler(this.pictureColor_MouseLeave);
            this.pictureBlueToTeal.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            // 
            // picturePurpleToBlue
            // 
            this.picturePurpleToBlue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picturePurpleToBlue.Location = new System.Drawing.Point(307, 12);
            this.picturePurpleToBlue.Name = "picturePurpleToBlue";
            this.picturePurpleToBlue.SelectionIndicator = Support.UI.PictureBoxSelectable.IndicatorType.None;
            this.picturePurpleToBlue.Size = new System.Drawing.Size(25, 259);
            this.picturePurpleToBlue.TabIndex = 10;
            this.picturePurpleToBlue.TabStop = false;
            this.picturePurpleToBlue.Xposition = 0;
            this.picturePurpleToBlue.Yposition = 0;
            this.picturePurpleToBlue.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            this.picturePurpleToBlue.MouseEnter += new System.EventHandler(this.pictureColor_MouseEnter);
            this.picturePurpleToBlue.MouseLeave += new System.EventHandler(this.pictureColor_MouseLeave);
            this.picturePurpleToBlue.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            // 
            // pictureRedToPurple
            // 
            this.pictureRedToPurple.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureRedToPurple.Location = new System.Drawing.Point(276, 12);
            this.pictureRedToPurple.Name = "pictureRedToPurple";
            this.pictureRedToPurple.SelectionIndicator = Support.UI.PictureBoxSelectable.IndicatorType.None;
            this.pictureRedToPurple.Size = new System.Drawing.Size(25, 259);
            this.pictureRedToPurple.TabIndex = 1;
            this.pictureRedToPurple.TabStop = false;
            this.pictureRedToPurple.Xposition = 0;
            this.pictureRedToPurple.Yposition = 0;
            this.pictureRedToPurple.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            this.pictureRedToPurple.MouseEnter += new System.EventHandler(this.pictureColor_MouseEnter);
            this.pictureRedToPurple.MouseLeave += new System.EventHandler(this.pictureColor_MouseLeave);
            this.pictureRedToPurple.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseColor);
            // 
            // pictureColorHue
            // 
            this.pictureColorHue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureColorHue.Location = new System.Drawing.Point(12, 12);
            this.pictureColorHue.Name = "pictureColorHue";
            this.pictureColorHue.SelectionIndicator = Support.UI.PictureBoxSelectable.IndicatorType.None;
            this.pictureColorHue.Size = new System.Drawing.Size(259, 259);
            this.pictureColorHue.TabIndex = 0;
            this.pictureColorHue.TabStop = false;
            this.pictureColorHue.Xposition = 0;
            this.pictureColorHue.Yposition = 0;
            this.pictureColorHue.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleMouseHueColor);
            this.pictureColorHue.MouseEnter += new System.EventHandler(this.pictureColor_MouseEnter);
            this.pictureColorHue.MouseLeave += new System.EventHandler(this.pictureColor_MouseLeave);
            this.pictureColorHue.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouseHueColor);
            // 
            // checkBoxAddZeroX
            // 
            this.checkBoxAddZeroX.Checked = true;
            this.checkBoxAddZeroX.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddZeroX.Location = new System.Drawing.Point(242, 327);
            this.checkBoxAddZeroX.Name = "checkBoxAddZeroX";
            this.checkBoxAddZeroX.Size = new System.Drawing.Size(72, 20);
            this.checkBoxAddZeroX.TabIndex = 16;
            this.checkBoxAddZeroX.Text = "Add 0x";
            this.checkBoxAddZeroX.UseVisualStyleBackColor = true;
            this.checkBoxAddZeroX.CheckedChanged += new System.EventHandler(this.checkBoxAddZeroX_CheckedChanged);
            // 
            // RGBColorSelectDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(466, 367);
            this.Controls.Add(this.checkBoxAddZeroX);
            this.Controls.Add(this.txtHexColor);
            this.Controls.Add(this.panelPreviousColors);
            this.Controls.Add(this.pictureYellowToRed);
            this.Controls.Add(this.pictureGreenToYellow);
            this.Controls.Add(this.pictureTealToGreen);
            this.Controls.Add(this.pictureBlueToTeal);
            this.Controls.Add(this.picturePurpleToBlue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericGreen);
            this.Controls.Add(this.numericBlue);
            this.Controls.Add(this.numericRed);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.pictureRedToPurple);
            this.Controls.Add(this.pictureColorHue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(472, 392);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(472, 392);
            this.Name = "RGBColorSelectDialog";
            this.Text = "Select Color";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RGBColorSelectDialog_FormClosing);
            this.Load += new System.EventHandler(this.RGBColorSelectDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureYellowToRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureGreenToYellow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureTealToGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBlueToTeal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturePurpleToBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureRedToPurple)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureColorHue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBoxSelectable pictureColorHue;
        private PictureBoxSelectable pictureRedToPurple;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.NumericUpDown numericRed;
        private System.Windows.Forms.NumericUpDown numericBlue;
        private System.Windows.Forms.NumericUpDown numericGreen;
        private System.Windows.Forms.Label label1;
        private PictureBoxSelectable picturePurpleToBlue;
        private PictureBoxSelectable pictureBlueToTeal;
        private PictureBoxSelectable pictureTealToGreen;
        private PictureBoxSelectable pictureGreenToYellow;
        private PictureBoxSelectable pictureYellowToRed;
        private System.Windows.Forms.Panel panelPreviousColors;
        private System.Windows.Forms.ToolTip toolTipPreviouscolor;
        private System.Windows.Forms.TextBox txtHexColor;
        private System.Windows.Forms.CheckBox checkBoxAddZeroX;
    }
}