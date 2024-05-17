namespace Support.UI
{
    partial class GradientDialog
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnSourceColor = new System.Windows.Forms.Button();
            this.btnDestinationColor = new System.Windows.Forms.Button();
            this.panelSourceColor = new System.Windows.Forms.Panel();
            this.panelDestinationColor = new System.Windows.Forms.Panel();
            this.tabGradients = new System.Windows.Forms.TabControl();
            this.tabLeftToRight = new System.Windows.Forms.TabPage();
            this.lblAngle = new System.Windows.Forms.Label();
            this.numericLTRAngle = new System.Windows.Forms.NumericUpDown();
            this.tabPoints = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.numericPDestinationY = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericPDestinationX = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericPSourceY = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericPSourceX = new System.Windows.Forms.NumericUpDown();
            this.tabPointsNormalized = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.numericPNDestinationY = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericPNDestinationX = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.numericPNSourceY = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.numericPNSourceX = new System.Windows.Forms.NumericUpDown();
            this.btnRemoveGradient = new System.Windows.Forms.Button();
            this.tabGradients.SuspendLayout();
            this.tabLeftToRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericLTRAngle)).BeginInit();
            this.tabPoints.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPDestinationY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPDestinationX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPSourceY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPSourceX)).BeginInit();
            this.tabPointsNormalized.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPNDestinationY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPNDestinationX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPNSourceY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPNSourceX)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(244, 193);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(325, 193);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(12, 193);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSourceColor
            // 
            this.btnSourceColor.Location = new System.Drawing.Point(12, 12);
            this.btnSourceColor.Name = "btnSourceColor";
            this.btnSourceColor.Size = new System.Drawing.Size(119, 23);
            this.btnSourceColor.TabIndex = 6;
            this.btnSourceColor.Text = "Source Color";
            this.btnSourceColor.UseVisualStyleBackColor = true;
            this.btnSourceColor.Click += new System.EventHandler(this.btnSourceColor_Click);
            // 
            // btnDestinationColor
            // 
            this.btnDestinationColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDestinationColor.Location = new System.Drawing.Point(244, 13);
            this.btnDestinationColor.Name = "btnDestinationColor";
            this.btnDestinationColor.Size = new System.Drawing.Size(125, 23);
            this.btnDestinationColor.TabIndex = 7;
            this.btnDestinationColor.Text = "Destination Color";
            this.btnDestinationColor.UseVisualStyleBackColor = true;
            this.btnDestinationColor.Click += new System.EventHandler(this.btnDestinationColor_Click);
            // 
            // panelSourceColor
            // 
            this.panelSourceColor.Location = new System.Drawing.Point(137, 12);
            this.panelSourceColor.Name = "panelSourceColor";
            this.panelSourceColor.Size = new System.Drawing.Size(31, 23);
            this.panelSourceColor.TabIndex = 8;
            // 
            // panelDestinationColor
            // 
            this.panelDestinationColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDestinationColor.Location = new System.Drawing.Point(375, 13);
            this.panelDestinationColor.Name = "panelDestinationColor";
            this.panelDestinationColor.Size = new System.Drawing.Size(31, 23);
            this.panelDestinationColor.TabIndex = 9;
            // 
            // tabGradients
            // 
            this.tabGradients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabGradients.Controls.Add(this.tabLeftToRight);
            this.tabGradients.Controls.Add(this.tabPoints);
            this.tabGradients.Controls.Add(this.tabPointsNormalized);
            this.tabGradients.Location = new System.Drawing.Point(12, 42);
            this.tabGradients.Name = "tabGradients";
            this.tabGradients.SelectedIndex = 0;
            this.tabGradients.Size = new System.Drawing.Size(388, 145);
            this.tabGradients.TabIndex = 10;
            // 
            // tabLeftToRight
            // 
            this.tabLeftToRight.Controls.Add(this.lblAngle);
            this.tabLeftToRight.Controls.Add(this.numericLTRAngle);
            this.tabLeftToRight.Location = new System.Drawing.Point(4, 22);
            this.tabLeftToRight.Name = "tabLeftToRight";
            this.tabLeftToRight.Padding = new System.Windows.Forms.Padding(3);
            this.tabLeftToRight.Size = new System.Drawing.Size(380, 119);
            this.tabLeftToRight.TabIndex = 0;
            this.tabLeftToRight.Text = "Left To Right";
            this.tabLeftToRight.UseVisualStyleBackColor = true;
            // 
            // lblAngle
            // 
            this.lblAngle.Location = new System.Drawing.Point(121, 6);
            this.lblAngle.Name = "lblAngle";
            this.lblAngle.Size = new System.Drawing.Size(127, 20);
            this.lblAngle.TabIndex = 1;
            this.lblAngle.Text = "Angle:";
            this.lblAngle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericLTRAngle
            // 
            this.numericLTRAngle.Location = new System.Drawing.Point(254, 6);
            this.numericLTRAngle.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numericLTRAngle.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.numericLTRAngle.Name = "numericLTRAngle";
            this.numericLTRAngle.Size = new System.Drawing.Size(120, 20);
            this.numericLTRAngle.TabIndex = 0;
            this.numericLTRAngle.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // tabPoints
            // 
            this.tabPoints.Controls.Add(this.label3);
            this.tabPoints.Controls.Add(this.numericPDestinationY);
            this.tabPoints.Controls.Add(this.label4);
            this.tabPoints.Controls.Add(this.numericPDestinationX);
            this.tabPoints.Controls.Add(this.label2);
            this.tabPoints.Controls.Add(this.numericPSourceY);
            this.tabPoints.Controls.Add(this.label1);
            this.tabPoints.Controls.Add(this.numericPSourceX);
            this.tabPoints.Location = new System.Drawing.Point(4, 22);
            this.tabPoints.Name = "tabPoints";
            this.tabPoints.Padding = new System.Windows.Forms.Padding(3);
            this.tabPoints.Size = new System.Drawing.Size(380, 119);
            this.tabPoints.TabIndex = 1;
            this.tabPoints.Text = "Points";
            this.tabPoints.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(218, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "Destination Y:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPDestinationY
            // 
            this.numericPDestinationY.Location = new System.Drawing.Point(317, 32);
            this.numericPDestinationY.Name = "numericPDestinationY";
            this.numericPDestinationY.Size = new System.Drawing.Size(60, 20);
            this.numericPDestinationY.TabIndex = 8;
            this.numericPDestinationY.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(215, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Destination X:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPDestinationX
            // 
            this.numericPDestinationX.Location = new System.Drawing.Point(317, 3);
            this.numericPDestinationX.Name = "numericPDestinationX";
            this.numericPDestinationX.Size = new System.Drawing.Size(60, 20);
            this.numericPDestinationX.TabIndex = 6;
            this.numericPDestinationX.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Source Y:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPSourceY
            // 
            this.numericPSourceY.Location = new System.Drawing.Point(76, 32);
            this.numericPSourceY.Name = "numericPSourceY";
            this.numericPSourceY.Size = new System.Drawing.Size(60, 20);
            this.numericPSourceY.TabIndex = 4;
            this.numericPSourceY.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Source X:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPSourceX
            // 
            this.numericPSourceX.Location = new System.Drawing.Point(76, 3);
            this.numericPSourceX.Name = "numericPSourceX";
            this.numericPSourceX.Size = new System.Drawing.Size(60, 20);
            this.numericPSourceX.TabIndex = 2;
            this.numericPSourceX.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // tabPointsNormalized
            // 
            this.tabPointsNormalized.Controls.Add(this.label5);
            this.tabPointsNormalized.Controls.Add(this.numericPNDestinationY);
            this.tabPointsNormalized.Controls.Add(this.label6);
            this.tabPointsNormalized.Controls.Add(this.numericPNDestinationX);
            this.tabPointsNormalized.Controls.Add(this.label7);
            this.tabPointsNormalized.Controls.Add(this.numericPNSourceY);
            this.tabPointsNormalized.Controls.Add(this.label8);
            this.tabPointsNormalized.Controls.Add(this.numericPNSourceX);
            this.tabPointsNormalized.Location = new System.Drawing.Point(4, 22);
            this.tabPointsNormalized.Name = "tabPointsNormalized";
            this.tabPointsNormalized.Size = new System.Drawing.Size(380, 119);
            this.tabPointsNormalized.TabIndex = 2;
            this.tabPointsNormalized.Text = "Points Normalized";
            this.tabPointsNormalized.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(209, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(102, 20);
            this.label5.TabIndex = 17;
            this.label5.Text = "Destination Y:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPNDestinationY
            // 
            this.numericPNDestinationY.DecimalPlaces = 2;
            this.numericPNDestinationY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericPNDestinationY.Location = new System.Drawing.Point(317, 32);
            this.numericPNDestinationY.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericPNDestinationY.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericPNDestinationY.Name = "numericPNDestinationY";
            this.numericPNDestinationY.Size = new System.Drawing.Size(60, 20);
            this.numericPNDestinationY.TabIndex = 16;
            this.numericPNDestinationY.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericPNDestinationY.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(206, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 20);
            this.label6.TabIndex = 15;
            this.label6.Text = "Destination X:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPNDestinationX
            // 
            this.numericPNDestinationX.DecimalPlaces = 2;
            this.numericPNDestinationX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericPNDestinationX.Location = new System.Drawing.Point(317, 3);
            this.numericPNDestinationX.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericPNDestinationX.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericPNDestinationX.Name = "numericPNDestinationX";
            this.numericPNDestinationX.Size = new System.Drawing.Size(60, 20);
            this.numericPNDestinationX.TabIndex = 14;
            this.numericPNDestinationX.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericPNDestinationX.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(3, 30);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 20);
            this.label7.TabIndex = 13;
            this.label7.Text = "Source Y:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPNSourceY
            // 
            this.numericPNSourceY.DecimalPlaces = 2;
            this.numericPNSourceY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericPNSourceY.Location = new System.Drawing.Point(76, 32);
            this.numericPNSourceY.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericPNSourceY.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericPNSourceY.Name = "numericPNSourceY";
            this.numericPNSourceY.Size = new System.Drawing.Size(60, 20);
            this.numericPNSourceY.TabIndex = 12;
            this.numericPNSourceY.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(3, 3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(67, 20);
            this.label8.TabIndex = 11;
            this.label8.Text = "Source X:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // numericPNSourceX
            // 
            this.numericPNSourceX.DecimalPlaces = 2;
            this.numericPNSourceX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericPNSourceX.Location = new System.Drawing.Point(76, 3);
            this.numericPNSourceX.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericPNSourceX.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericPNSourceX.Name = "numericPNSourceX";
            this.numericPNSourceX.Size = new System.Drawing.Size(60, 20);
            this.numericPNSourceX.TabIndex = 10;
            this.numericPNSourceX.ValueChanged += new System.EventHandler(this.OnConfigurationChange);
            // 
            // btnRemoveGradient
            // 
            this.btnRemoveGradient.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveGradient.Location = new System.Drawing.Point(92, 193);
            this.btnRemoveGradient.Name = "btnRemoveGradient";
            this.btnRemoveGradient.Size = new System.Drawing.Size(112, 23);
            this.btnRemoveGradient.TabIndex = 11;
            this.btnRemoveGradient.Text = "Remove Gradient";
            this.btnRemoveGradient.UseVisualStyleBackColor = true;
            this.btnRemoveGradient.Click += new System.EventHandler(this.btnRemoveGradient_Click);
            // 
            // GradientDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(412, 228);
            this.Controls.Add(this.btnRemoveGradient);
            this.Controls.Add(this.tabGradients);
            this.Controls.Add(this.panelDestinationColor);
            this.Controls.Add(this.panelSourceColor);
            this.Controls.Add(this.btnDestinationColor);
            this.Controls.Add(this.btnSourceColor);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximumSize = new System.Drawing.Size(428, 267);
            this.MinimumSize = new System.Drawing.Size(428, 267);
            this.Name = "GradientDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Gradient";
            this.Load += new System.EventHandler(this.GradientDialog_Load);
            this.tabGradients.ResumeLayout(false);
            this.tabLeftToRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericLTRAngle)).EndInit();
            this.tabPoints.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericPDestinationY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPDestinationX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPSourceY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPSourceX)).EndInit();
            this.tabPointsNormalized.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericPNDestinationY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPNDestinationX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPNSourceY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPNSourceX)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnSourceColor;
        private System.Windows.Forms.Button btnDestinationColor;
        private System.Windows.Forms.Panel panelSourceColor;
        private System.Windows.Forms.Panel panelDestinationColor;
        private System.Windows.Forms.TabControl tabGradients;
        private System.Windows.Forms.TabPage tabLeftToRight;
        private System.Windows.Forms.TabPage tabPoints;
        private System.Windows.Forms.Label lblAngle;
        private System.Windows.Forms.NumericUpDown numericLTRAngle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericPDestinationY;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericPDestinationX;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericPSourceY;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericPSourceX;
        private System.Windows.Forms.TabPage tabPointsNormalized;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericPNDestinationY;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericPNDestinationX;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericPNSourceY;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericPNSourceX;
        private System.Windows.Forms.Button btnRemoveGradient;
    }
}