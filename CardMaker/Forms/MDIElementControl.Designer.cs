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

namespace CardMaker.Forms
{
    partial class MDIElementControl
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
			this.groupBoxElement = new System.Windows.Forms.GroupBox();
			this.comboElementMirror = new System.Windows.Forms.ComboBox();
			this.label17 = new System.Windows.Forms.Label();
			this.groupBackgroundColor = new System.Windows.Forms.GroupBox();
			this.btnNullBackgroundColor = new System.Windows.Forms.Button();
			this.panelBackgroundColor = new System.Windows.Forms.Panel();
			this.btnElementBackgroundColor = new System.Windows.Forms.Button();
			this.btnAssist = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabPageFont = new System.Windows.Forms.TabPage();
			this.labelGradientAngle = new System.Windows.Forms.Label();
			this.numericGradientDegrees = new System.Windows.Forms.NumericUpDown();
			this.checkBoxUseGradient = new System.Windows.Forms.CheckBox();
			this.panelGradientColor = new System.Windows.Forms.Panel();
			this.btnElementGradientColor = new System.Windows.Forms.Button();
			this.checkJustifiedText = new System.Windows.Forms.CheckBox();
			this.checkBoxItalic = new System.Windows.Forms.CheckBox();
			this.checkBoxBold = new System.Windows.Forms.CheckBox();
			this.numericWordSpace = new System.Windows.Forms.NumericUpDown();
			this.lblLineSpace = new System.Windows.Forms.Label();
			this.numericLineSpace = new System.Windows.Forms.NumericUpDown();
			this.checkFontAutoScale = new System.Windows.Forms.CheckBox();
			this.comboFontName = new System.Windows.Forms.ComboBox();
			this.panelFontColor = new System.Windows.Forms.Panel();
			this.btnElementFontColor = new System.Windows.Forms.Button();
			this.checkBoxUnderline = new System.Windows.Forms.CheckBox();
			this.label12 = new System.Windows.Forms.Label();
			this.checkBoxStrikeout = new System.Windows.Forms.CheckBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.comboTextHorizontalAlign = new System.Windows.Forms.ComboBox();
			this.numericFontSize = new System.Windows.Forms.NumericUpDown();
			this.comboTextVerticalAlign = new System.Windows.Forms.ComboBox();
			this.lblWordSpacing = new System.Windows.Forms.Label();
			this.tabPageShape = new System.Windows.Forms.TabPage();
			this.panelShapeColor = new System.Windows.Forms.Panel();
			this.comboShapeType = new System.Windows.Forms.ComboBox();
			this.btnElementShapeColor = new System.Windows.Forms.Button();
			this.propertyGridShape = new System.Windows.Forms.PropertyGrid();
			this.tabPageGraphic = new System.Windows.Forms.TabPage();
			this.checkCenterOnOrigin = new System.Windows.Forms.CheckBox();
			this.panelGraphicColor = new System.Windows.Forms.Panel();
			this.btnElementGraphicColor = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.txtTileSize = new System.Windows.Forms.TextBox();
			this.checkKeepOriginalSize = new System.Windows.Forms.CheckBox();
			this.btnSetSizeToImage = new System.Windows.Forms.Button();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.comboGraphicHorizontalAlign = new System.Windows.Forms.ComboBox();
			this.comboGraphicVerticalAlign = new System.Windows.Forms.ComboBox();
			this.checkLockAspect = new System.Windows.Forms.CheckBox();
			this.groupBoxOutline = new System.Windows.Forms.GroupBox();
			this.panelOutlineColor = new System.Windows.Forms.Panel();
			this.label11 = new System.Windows.Forms.Label();
			this.numericElementOutLineThickness = new System.Windows.Forms.NumericUpDown();
			this.btnElementOutlineColor = new System.Windows.Forms.Button();
			this.listViewElementColumns = new System.Windows.Forms.ListView();
			this.label8 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.numericElementOpacity = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.numericElementRotation = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBoxElementBorder = new System.Windows.Forms.GroupBox();
			this.panelBorderColor = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.numericElementBorderThickness = new System.Windows.Forms.NumericUpDown();
			this.btnElementBorderColor = new System.Windows.Forms.Button();
			this.numericElementH = new System.Windows.Forms.NumericUpDown();
			this.numericElementW = new System.Windows.Forms.NumericUpDown();
			this.numericElementY = new System.Windows.Forms.NumericUpDown();
			this.numericElementX = new System.Windows.Forms.NumericUpDown();
			this.btnElementBrowseImage = new System.Windows.Forms.Button();
			this.txtElementVariable = new System.Windows.Forms.TextBox();
			this.comboElementType = new System.Windows.Forms.ComboBox();
			this.contextMenuReferenceStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.contextMenuStripAssist = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.groupBoxElement.SuspendLayout();
			this.groupBackgroundColor.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPageFont.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericGradientDegrees)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericWordSpace)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericLineSpace)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericFontSize)).BeginInit();
			this.tabPageShape.SuspendLayout();
			this.tabPageGraphic.SuspendLayout();
			this.groupBoxOutline.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericElementOutLineThickness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementOpacity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementRotation)).BeginInit();
			this.groupBoxElementBorder.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericElementBorderThickness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementH)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementW)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementX)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBoxElement
			// 
			this.groupBoxElement.Controls.Add(this.comboElementMirror);
			this.groupBoxElement.Controls.Add(this.label17);
			this.groupBoxElement.Controls.Add(this.groupBackgroundColor);
			this.groupBoxElement.Controls.Add(this.btnAssist);
			this.groupBoxElement.Controls.Add(this.tabControl);
			this.groupBoxElement.Controls.Add(this.groupBoxOutline);
			this.groupBoxElement.Controls.Add(this.listViewElementColumns);
			this.groupBoxElement.Controls.Add(this.label8);
			this.groupBoxElement.Controls.Add(this.label14);
			this.groupBoxElement.Controls.Add(this.numericElementOpacity);
			this.groupBoxElement.Controls.Add(this.label7);
			this.groupBoxElement.Controls.Add(this.numericElementRotation);
			this.groupBoxElement.Controls.Add(this.label6);
			this.groupBoxElement.Controls.Add(this.label5);
			this.groupBoxElement.Controls.Add(this.label4);
			this.groupBoxElement.Controls.Add(this.label3);
			this.groupBoxElement.Controls.Add(this.label2);
			this.groupBoxElement.Controls.Add(this.groupBoxElementBorder);
			this.groupBoxElement.Controls.Add(this.numericElementH);
			this.groupBoxElement.Controls.Add(this.numericElementW);
			this.groupBoxElement.Controls.Add(this.numericElementY);
			this.groupBoxElement.Controls.Add(this.numericElementX);
			this.groupBoxElement.Controls.Add(this.btnElementBrowseImage);
			this.groupBoxElement.Controls.Add(this.txtElementVariable);
			this.groupBoxElement.Controls.Add(this.comboElementType);
			this.groupBoxElement.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxElement.Location = new System.Drawing.Point(0, 0);
			this.groupBoxElement.Margin = new System.Windows.Forms.Padding(4);
			this.groupBoxElement.Name = "groupBoxElement";
			this.groupBoxElement.Padding = new System.Windows.Forms.Padding(4);
			this.groupBoxElement.Size = new System.Drawing.Size(1070, 821);
			this.groupBoxElement.TabIndex = 11;
			this.groupBoxElement.TabStop = false;
			this.groupBoxElement.Text = "Element";
			// 
			// comboElementMirror
			// 
			this.comboElementMirror.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboElementMirror.FormattingEnabled = true;
			this.comboElementMirror.Location = new System.Drawing.Point(316, 23);
			this.comboElementMirror.Margin = new System.Windows.Forms.Padding(4);
			this.comboElementMirror.Name = "comboElementMirror";
			this.comboElementMirror.Size = new System.Drawing.Size(139, 24);
			this.comboElementMirror.TabIndex = 49;
			this.comboElementMirror.SelectedIndexChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(239, 23);
			this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(57, 26);
			this.label17.TabIndex = 48;
			this.label17.Text = "Mirror:";
			this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBackgroundColor
			// 
			this.groupBackgroundColor.Controls.Add(this.btnNullBackgroundColor);
			this.groupBackgroundColor.Controls.Add(this.panelBackgroundColor);
			this.groupBackgroundColor.Controls.Add(this.btnElementBackgroundColor);
			this.groupBackgroundColor.Location = new System.Drawing.Point(464, 23);
			this.groupBackgroundColor.Margin = new System.Windows.Forms.Padding(4);
			this.groupBackgroundColor.Name = "groupBackgroundColor";
			this.groupBackgroundColor.Padding = new System.Windows.Forms.Padding(4);
			this.groupBackgroundColor.Size = new System.Drawing.Size(120, 91);
			this.groupBackgroundColor.TabIndex = 44;
			this.groupBackgroundColor.TabStop = false;
			this.groupBackgroundColor.Text = "Background";
			// 
			// btnNullBackgroundColor
			// 
			this.btnNullBackgroundColor.Location = new System.Drawing.Point(68, 54);
			this.btnNullBackgroundColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnNullBackgroundColor.Name = "btnNullBackgroundColor";
			this.btnNullBackgroundColor.Size = new System.Drawing.Size(43, 25);
			this.btnNullBackgroundColor.TabIndex = 44;
			this.btnNullBackgroundColor.Text = "X";
			this.btnNullBackgroundColor.UseVisualStyleBackColor = true;
			this.btnNullBackgroundColor.Click += new System.EventHandler(this.btnNullBackgroundColor_Click);
			// 
			// panelBackgroundColor
			// 
			this.panelBackgroundColor.Location = new System.Drawing.Point(12, 54);
			this.panelBackgroundColor.Margin = new System.Windows.Forms.Padding(4);
			this.panelBackgroundColor.Name = "panelBackgroundColor";
			this.panelBackgroundColor.Size = new System.Drawing.Size(48, 25);
			this.panelBackgroundColor.TabIndex = 43;
			// 
			// btnElementBackgroundColor
			// 
			this.btnElementBackgroundColor.Location = new System.Drawing.Point(12, 22);
			this.btnElementBackgroundColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementBackgroundColor.Name = "btnElementBackgroundColor";
			this.btnElementBackgroundColor.Size = new System.Drawing.Size(100, 25);
			this.btnElementBackgroundColor.TabIndex = 20;
			this.btnElementBackgroundColor.Text = "Color";
			this.btnElementBackgroundColor.UseVisualStyleBackColor = true;
			this.btnElementBackgroundColor.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// btnAssist
			// 
			this.btnAssist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAssist.Location = new System.Drawing.Point(1025, 362);
			this.btnAssist.Margin = new System.Windows.Forms.Padding(4);
			this.btnAssist.Name = "btnAssist";
			this.btnAssist.Size = new System.Drawing.Size(33, 25);
			this.btnAssist.TabIndex = 47;
			this.btnAssist.Text = "+";
			this.btnAssist.UseVisualStyleBackColor = true;
			this.btnAssist.Click += new System.EventHandler(this.btnAssist_Click);
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tabPageFont);
			this.tabControl.Controls.Add(this.tabPageShape);
			this.tabControl.Controls.Add(this.tabPageGraphic);
			this.tabControl.Location = new System.Drawing.Point(16, 119);
			this.tabControl.Margin = new System.Windows.Forms.Padding(4);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(1046, 147);
			this.tabControl.TabIndex = 46;
			// 
			// tabPageFont
			// 
			this.tabPageFont.Controls.Add(this.labelGradientAngle);
			this.tabPageFont.Controls.Add(this.numericGradientDegrees);
			this.tabPageFont.Controls.Add(this.checkBoxUseGradient);
			this.tabPageFont.Controls.Add(this.panelGradientColor);
			this.tabPageFont.Controls.Add(this.btnElementGradientColor);
			this.tabPageFont.Controls.Add(this.checkJustifiedText);
			this.tabPageFont.Controls.Add(this.checkBoxItalic);
			this.tabPageFont.Controls.Add(this.checkBoxBold);
			this.tabPageFont.Controls.Add(this.numericWordSpace);
			this.tabPageFont.Controls.Add(this.lblLineSpace);
			this.tabPageFont.Controls.Add(this.numericLineSpace);
			this.tabPageFont.Controls.Add(this.checkFontAutoScale);
			this.tabPageFont.Controls.Add(this.comboFontName);
			this.tabPageFont.Controls.Add(this.panelFontColor);
			this.tabPageFont.Controls.Add(this.btnElementFontColor);
			this.tabPageFont.Controls.Add(this.checkBoxUnderline);
			this.tabPageFont.Controls.Add(this.label12);
			this.tabPageFont.Controls.Add(this.checkBoxStrikeout);
			this.tabPageFont.Controls.Add(this.label13);
			this.tabPageFont.Controls.Add(this.label10);
			this.tabPageFont.Controls.Add(this.comboTextHorizontalAlign);
			this.tabPageFont.Controls.Add(this.numericFontSize);
			this.tabPageFont.Controls.Add(this.comboTextVerticalAlign);
			this.tabPageFont.Controls.Add(this.lblWordSpacing);
			this.tabPageFont.Location = new System.Drawing.Point(4, 25);
			this.tabPageFont.Margin = new System.Windows.Forms.Padding(4);
			this.tabPageFont.Name = "tabPageFont";
			this.tabPageFont.Padding = new System.Windows.Forms.Padding(4);
			this.tabPageFont.Size = new System.Drawing.Size(1038, 118);
			this.tabPageFont.TabIndex = 0;
			this.tabPageFont.Text = "Font";
			this.tabPageFont.UseVisualStyleBackColor = true;
			// 
			// labelGradientAngle
			// 
			this.labelGradientAngle.Location = new System.Drawing.Point(240, 82);
			this.labelGradientAngle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labelGradientAngle.Name = "labelGradientAngle";
			this.labelGradientAngle.Size = new System.Drawing.Size(73, 26);
			this.labelGradientAngle.TabIndex = 49;
			this.labelGradientAngle.Text = "Angle:";
			this.labelGradientAngle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericGradientDegrees
			// 
			this.numericGradientDegrees.Location = new System.Drawing.Point(321, 82);
			this.numericGradientDegrees.Margin = new System.Windows.Forms.Padding(4);
			this.numericGradientDegrees.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
			this.numericGradientDegrees.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
			this.numericGradientDegrees.Name = "numericGradientDegrees";
			this.numericGradientDegrees.Size = new System.Drawing.Size(64, 22);
			this.numericGradientDegrees.TabIndex = 50;
			this.numericGradientDegrees.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// checkBoxUseGradient
			// 
			this.checkBoxUseGradient.Location = new System.Drawing.Point(130, 82);
			this.checkBoxUseGradient.Margin = new System.Windows.Forms.Padding(4);
			this.checkBoxUseGradient.Name = "checkBoxUseGradient";
			this.checkBoxUseGradient.Size = new System.Drawing.Size(129, 25);
			this.checkBoxUseGradient.TabIndex = 48;
			this.checkBoxUseGradient.Text = "Use Gradient";
			this.checkBoxUseGradient.UseVisualStyleBackColor = true;
			this.checkBoxUseGradient.CheckedChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// panelGradientColor
			// 
			this.panelGradientColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelGradientColor.Location = new System.Drawing.Point(91, 82);
			this.panelGradientColor.Margin = new System.Windows.Forms.Padding(4);
			this.panelGradientColor.Name = "panelGradientColor";
			this.panelGradientColor.Size = new System.Drawing.Size(27, 25);
			this.panelGradientColor.TabIndex = 47;
			// 
			// btnElementGradientColor
			// 
			this.btnElementGradientColor.Location = new System.Drawing.Point(0, 82);
			this.btnElementGradientColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementGradientColor.Name = "btnElementGradientColor";
			this.btnElementGradientColor.Size = new System.Drawing.Size(83, 25);
			this.btnElementGradientColor.TabIndex = 46;
			this.btnElementGradientColor.Text = "Gradient";
			this.btnElementGradientColor.UseVisualStyleBackColor = true;
			this.btnElementGradientColor.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// checkJustifiedText
			// 
			this.checkJustifiedText.Location = new System.Drawing.Point(836, 7);
			this.checkJustifiedText.Margin = new System.Windows.Forms.Padding(4);
			this.checkJustifiedText.Name = "checkJustifiedText";
			this.checkJustifiedText.Size = new System.Drawing.Size(103, 25);
			this.checkJustifiedText.TabIndex = 52;
			this.checkJustifiedText.Text = "Justified";
			this.checkJustifiedText.UseVisualStyleBackColor = true;
			this.checkJustifiedText.CheckedChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// checkBoxItalic
			// 
			this.checkBoxItalic.Location = new System.Drawing.Point(335, 9);
			this.checkBoxItalic.Margin = new System.Windows.Forms.Padding(4);
			this.checkBoxItalic.Name = "checkBoxItalic";
			this.checkBoxItalic.Size = new System.Drawing.Size(109, 25);
			this.checkBoxItalic.TabIndex = 51;
			this.checkBoxItalic.Text = "Italic";
			this.checkBoxItalic.UseVisualStyleBackColor = true;
			this.checkBoxItalic.CheckedChanged += new System.EventHandler(this.HandleFontSettingChange);
			// 
			// checkBoxBold
			// 
			this.checkBoxBold.Location = new System.Drawing.Point(243, 9);
			this.checkBoxBold.Margin = new System.Windows.Forms.Padding(4);
			this.checkBoxBold.Name = "checkBoxBold";
			this.checkBoxBold.Size = new System.Drawing.Size(95, 25);
			this.checkBoxBold.TabIndex = 50;
			this.checkBoxBold.Text = "Bold";
			this.checkBoxBold.UseVisualStyleBackColor = true;
			this.checkBoxBold.CheckedChanged += new System.EventHandler(this.HandleFontSettingChange);
			// 
			// numericWordSpace
			// 
			this.numericWordSpace.Location = new System.Drawing.Point(561, 39);
			this.numericWordSpace.Margin = new System.Windows.Forms.Padding(4);
			this.numericWordSpace.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
			this.numericWordSpace.Name = "numericWordSpace";
			this.numericWordSpace.Size = new System.Drawing.Size(64, 22);
			this.numericWordSpace.TabIndex = 48;
			this.numericWordSpace.ValueChanged += new System.EventHandler(this.HandleFontSettingChange);
			// 
			// lblLineSpace
			// 
			this.lblLineSpace.Location = new System.Drawing.Point(447, 7);
			this.lblLineSpace.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblLineSpace.Name = "lblLineSpace";
			this.lblLineSpace.Size = new System.Drawing.Size(107, 26);
			this.lblLineSpace.TabIndex = 47;
			this.lblLineSpace.Text = "Line Spacing:";
			this.lblLineSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericLineSpace
			// 
			this.numericLineSpace.Location = new System.Drawing.Point(561, 6);
			this.numericLineSpace.Margin = new System.Windows.Forms.Padding(4);
			this.numericLineSpace.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.numericLineSpace.Name = "numericLineSpace";
			this.numericLineSpace.Size = new System.Drawing.Size(64, 22);
			this.numericLineSpace.TabIndex = 46;
			this.numericLineSpace.ValueChanged += new System.EventHandler(this.HandleFontSettingChange);
			// 
			// checkFontAutoScale
			// 
			this.checkFontAutoScale.Location = new System.Drawing.Point(836, 37);
			this.checkFontAutoScale.Margin = new System.Windows.Forms.Padding(4);
			this.checkFontAutoScale.Name = "checkFontAutoScale";
			this.checkFontAutoScale.Size = new System.Drawing.Size(129, 25);
			this.checkFontAutoScale.TabIndex = 45;
			this.checkFontAutoScale.Text = "Auto-Scale";
			this.checkFontAutoScale.UseVisualStyleBackColor = true;
			this.checkFontAutoScale.CheckedChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// comboFontName
			// 
			this.comboFontName.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.comboFontName.DropDownHeight = 200;
			this.comboFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFontName.DropDownWidth = 500;
			this.comboFontName.FormattingEnabled = true;
			this.comboFontName.IntegralHeight = false;
			this.comboFontName.ItemHeight = 28;
			this.comboFontName.Location = new System.Drawing.Point(0, 0);
			this.comboFontName.Margin = new System.Windows.Forms.Padding(4);
			this.comboFontName.Name = "comboFontName";
			this.comboFontName.Size = new System.Drawing.Size(225, 34);
			this.comboFontName.TabIndex = 36;
			this.comboFontName.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboFontName_DrawItem);
			this.comboFontName.DropDown += new System.EventHandler(this.comboFontName_DropDown);
			this.comboFontName.SelectedIndexChanged += new System.EventHandler(this.comboFontName_SelectedIndexChanged);
			this.comboFontName.DropDownClosed += new System.EventHandler(this.comboFontName_DropDownClosed);
			// 
			// panelFontColor
			// 
			this.panelFontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelFontColor.Location = new System.Drawing.Point(91, 49);
			this.panelFontColor.Margin = new System.Windows.Forms.Padding(4);
			this.panelFontColor.Name = "panelFontColor";
			this.panelFontColor.Size = new System.Drawing.Size(27, 25);
			this.panelFontColor.TabIndex = 44;
			// 
			// btnElementFontColor
			// 
			this.btnElementFontColor.Location = new System.Drawing.Point(0, 49);
			this.btnElementFontColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementFontColor.Name = "btnElementFontColor";
			this.btnElementFontColor.Size = new System.Drawing.Size(83, 25);
			this.btnElementFontColor.TabIndex = 23;
			this.btnElementFontColor.Text = "Color";
			this.btnElementFontColor.UseVisualStyleBackColor = true;
			this.btnElementFontColor.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// checkBoxUnderline
			// 
			this.checkBoxUnderline.Location = new System.Drawing.Point(222, 49);
			this.checkBoxUnderline.Margin = new System.Windows.Forms.Padding(4);
			this.checkBoxUnderline.Name = "checkBoxUnderline";
			this.checkBoxUnderline.Size = new System.Drawing.Size(109, 25);
			this.checkBoxUnderline.TabIndex = 42;
			this.checkBoxUnderline.Text = "Underline";
			this.checkBoxUnderline.UseVisualStyleBackColor = true;
			this.checkBoxUnderline.CheckedChanged += new System.EventHandler(this.HandleFontSettingChange);
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(620, 6);
			this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(101, 26);
			this.label12.TabIndex = 32;
			this.label12.Text = "H Alignment:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBoxStrikeout
			// 
			this.checkBoxStrikeout.Location = new System.Drawing.Point(130, 49);
			this.checkBoxStrikeout.Margin = new System.Windows.Forms.Padding(4);
			this.checkBoxStrikeout.Name = "checkBoxStrikeout";
			this.checkBoxStrikeout.Size = new System.Drawing.Size(95, 25);
			this.checkBoxStrikeout.TabIndex = 41;
			this.checkBoxStrikeout.Text = "Strikeout";
			this.checkBoxStrikeout.UseVisualStyleBackColor = true;
			this.checkBoxStrikeout.CheckedChanged += new System.EventHandler(this.HandleFontSettingChange);
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(620, 38);
			this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(101, 26);
			this.label13.TabIndex = 33;
			this.label13.Text = "V Alignment:";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(323, 37);
			this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(43, 26);
			this.label10.TabIndex = 40;
			this.label10.Text = "Size:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboTextHorizontalAlign
			// 
			this.comboTextHorizontalAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboTextHorizontalAlign.FormattingEnabled = true;
			this.comboTextHorizontalAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right"});
			this.comboTextHorizontalAlign.Location = new System.Drawing.Point(729, 6);
			this.comboTextHorizontalAlign.Margin = new System.Windows.Forms.Padding(4);
			this.comboTextHorizontalAlign.Name = "comboTextHorizontalAlign";
			this.comboTextHorizontalAlign.Size = new System.Drawing.Size(97, 24);
			this.comboTextHorizontalAlign.TabIndex = 34;
			this.comboTextHorizontalAlign.SelectedIndexChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// numericFontSize
			// 
			this.numericFontSize.Location = new System.Drawing.Point(371, 38);
			this.numericFontSize.Margin = new System.Windows.Forms.Padding(4);
			this.numericFontSize.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.numericFontSize.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
			this.numericFontSize.Name = "numericFontSize";
			this.numericFontSize.Size = new System.Drawing.Size(64, 22);
			this.numericFontSize.TabIndex = 39;
			this.numericFontSize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numericFontSize.ValueChanged += new System.EventHandler(this.HandleFontSettingChange);
			// 
			// comboTextVerticalAlign
			// 
			this.comboTextVerticalAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboTextVerticalAlign.FormattingEnabled = true;
			this.comboTextVerticalAlign.Items.AddRange(new object[] {
            "Top",
            "Middle",
            "Bottom"});
			this.comboTextVerticalAlign.Location = new System.Drawing.Point(729, 39);
			this.comboTextVerticalAlign.Margin = new System.Windows.Forms.Padding(4);
			this.comboTextVerticalAlign.Name = "comboTextVerticalAlign";
			this.comboTextVerticalAlign.Size = new System.Drawing.Size(97, 24);
			this.comboTextVerticalAlign.TabIndex = 35;
			this.comboTextVerticalAlign.SelectedIndexChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// lblWordSpacing
			// 
			this.lblWordSpacing.Location = new System.Drawing.Point(447, 38);
			this.lblWordSpacing.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblWordSpacing.Name = "lblWordSpacing";
			this.lblWordSpacing.Size = new System.Drawing.Size(107, 26);
			this.lblWordSpacing.TabIndex = 49;
			this.lblWordSpacing.Text = "Word Spacing:";
			this.lblWordSpacing.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPageShape
			// 
			this.tabPageShape.Controls.Add(this.panelShapeColor);
			this.tabPageShape.Controls.Add(this.comboShapeType);
			this.tabPageShape.Controls.Add(this.btnElementShapeColor);
			this.tabPageShape.Controls.Add(this.propertyGridShape);
			this.tabPageShape.Location = new System.Drawing.Point(4, 25);
			this.tabPageShape.Margin = new System.Windows.Forms.Padding(4);
			this.tabPageShape.Name = "tabPageShape";
			this.tabPageShape.Padding = new System.Windows.Forms.Padding(4);
			this.tabPageShape.Size = new System.Drawing.Size(1038, 118);
			this.tabPageShape.TabIndex = 1;
			this.tabPageShape.Text = "Shape";
			this.tabPageShape.UseVisualStyleBackColor = true;
			// 
			// panelShapeColor
			// 
			this.panelShapeColor.Location = new System.Drawing.Point(76, 49);
			this.panelShapeColor.Margin = new System.Windows.Forms.Padding(4);
			this.panelShapeColor.Name = "panelShapeColor";
			this.panelShapeColor.Size = new System.Drawing.Size(27, 25);
			this.panelShapeColor.TabIndex = 44;
			// 
			// comboShapeType
			// 
			this.comboShapeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboShapeType.FormattingEnabled = true;
			this.comboShapeType.Location = new System.Drawing.Point(4, 4);
			this.comboShapeType.Margin = new System.Windows.Forms.Padding(4);
			this.comboShapeType.Name = "comboShapeType";
			this.comboShapeType.Size = new System.Drawing.Size(160, 24);
			this.comboShapeType.TabIndex = 26;
			this.comboShapeType.SelectedIndexChanged += new System.EventHandler(this.comboShapeType_SelectedIndexChanged);
			// 
			// btnElementShapeColor
			// 
			this.btnElementShapeColor.Location = new System.Drawing.Point(0, 49);
			this.btnElementShapeColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementShapeColor.Name = "btnElementShapeColor";
			this.btnElementShapeColor.Size = new System.Drawing.Size(65, 25);
			this.btnElementShapeColor.TabIndex = 24;
			this.btnElementShapeColor.Text = "Color";
			this.btnElementShapeColor.UseVisualStyleBackColor = true;
			this.btnElementShapeColor.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// propertyGridShape
			// 
			this.propertyGridShape.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.propertyGridShape.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.propertyGridShape.HelpVisible = false;
			this.propertyGridShape.LineColor = System.Drawing.SystemColors.ControlDark;
			this.propertyGridShape.Location = new System.Drawing.Point(173, 4);
			this.propertyGridShape.Margin = new System.Windows.Forms.Padding(4);
			this.propertyGridShape.Name = "propertyGridShape";
			this.propertyGridShape.Size = new System.Drawing.Size(695, 64);
			this.propertyGridShape.TabIndex = 25;
			this.propertyGridShape.ToolbarVisible = false;
			this.propertyGridShape.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridShape_PropertyValueChanged);
			// 
			// tabPageGraphic
			// 
			this.tabPageGraphic.Controls.Add(this.checkCenterOnOrigin);
			this.tabPageGraphic.Controls.Add(this.panelGraphicColor);
			this.tabPageGraphic.Controls.Add(this.btnElementGraphicColor);
			this.tabPageGraphic.Controls.Add(this.label9);
			this.tabPageGraphic.Controls.Add(this.txtTileSize);
			this.tabPageGraphic.Controls.Add(this.checkKeepOriginalSize);
			this.tabPageGraphic.Controls.Add(this.btnSetSizeToImage);
			this.tabPageGraphic.Controls.Add(this.label15);
			this.tabPageGraphic.Controls.Add(this.label16);
			this.tabPageGraphic.Controls.Add(this.comboGraphicHorizontalAlign);
			this.tabPageGraphic.Controls.Add(this.comboGraphicVerticalAlign);
			this.tabPageGraphic.Controls.Add(this.checkLockAspect);
			this.tabPageGraphic.Location = new System.Drawing.Point(4, 25);
			this.tabPageGraphic.Margin = new System.Windows.Forms.Padding(4);
			this.tabPageGraphic.Name = "tabPageGraphic";
			this.tabPageGraphic.Size = new System.Drawing.Size(1038, 118);
			this.tabPageGraphic.TabIndex = 2;
			this.tabPageGraphic.Text = "Graphic";
			this.tabPageGraphic.UseVisualStyleBackColor = true;
			// 
			// checkCenterOnOrigin
			// 
			this.checkCenterOnOrigin.Location = new System.Drawing.Point(397, 5);
			this.checkCenterOnOrigin.Margin = new System.Windows.Forms.Padding(4);
			this.checkCenterOnOrigin.Name = "checkCenterOnOrigin";
			this.checkCenterOnOrigin.Size = new System.Drawing.Size(209, 30);
			this.checkCenterOnOrigin.TabIndex = 46;
			this.checkCenterOnOrigin.Text = "Center On Origin";
			this.checkCenterOnOrigin.UseVisualStyleBackColor = true;
			this.checkCenterOnOrigin.CheckedChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// panelGraphicColor
			// 
			this.panelGraphicColor.Location = new System.Drawing.Point(76, 49);
			this.panelGraphicColor.Margin = new System.Windows.Forms.Padding(4);
			this.panelGraphicColor.Name = "panelGraphicColor";
			this.panelGraphicColor.Size = new System.Drawing.Size(27, 25);
			this.panelGraphicColor.TabIndex = 45;
			// 
			// btnElementGraphicColor
			// 
			this.btnElementGraphicColor.Location = new System.Drawing.Point(0, 49);
			this.btnElementGraphicColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementGraphicColor.Name = "btnElementGraphicColor";
			this.btnElementGraphicColor.Size = new System.Drawing.Size(65, 25);
			this.btnElementGraphicColor.TabIndex = 44;
			this.btnElementGraphicColor.Text = "Color";
			this.btnElementGraphicColor.UseVisualStyleBackColor = true;
			this.btnElementGraphicColor.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(179, 39);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(173, 26);
			this.label9.TabIndex = 43;
			this.label9.Text = "Tile Size:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtTileSize
			// 
			this.txtTileSize.Location = new System.Drawing.Point(360, 39);
			this.txtTileSize.Margin = new System.Windows.Forms.Padding(4);
			this.txtTileSize.Name = "txtTileSize";
			this.txtTileSize.Size = new System.Drawing.Size(132, 22);
			this.txtTileSize.TabIndex = 42;
			this.txtTileSize.TextChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// checkKeepOriginalSize
			// 
			this.checkKeepOriginalSize.Location = new System.Drawing.Point(200, 4);
			this.checkKeepOriginalSize.Margin = new System.Windows.Forms.Padding(4);
			this.checkKeepOriginalSize.Name = "checkKeepOriginalSize";
			this.checkKeepOriginalSize.Size = new System.Drawing.Size(189, 30);
			this.checkKeepOriginalSize.TabIndex = 41;
			this.checkKeepOriginalSize.Text = "Keep Original Size";
			this.checkKeepOriginalSize.UseVisualStyleBackColor = true;
			this.checkKeepOriginalSize.CheckedChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// btnSetSizeToImage
			// 
			this.btnSetSizeToImage.Location = new System.Drawing.Point(831, 7);
			this.btnSetSizeToImage.Margin = new System.Windows.Forms.Padding(4);
			this.btnSetSizeToImage.Name = "btnSetSizeToImage";
			this.btnSetSizeToImage.Size = new System.Drawing.Size(120, 59);
			this.btnSetSizeToImage.TabIndex = 40;
			this.btnSetSizeToImage.Text = "Set Size To Image";
			this.btnSetSizeToImage.UseVisualStyleBackColor = true;
			this.btnSetSizeToImage.Click += new System.EventHandler(this.btnSetSizeToImage_Click);
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(615, 7);
			this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(101, 26);
			this.label15.TabIndex = 36;
			this.label15.Text = "H Alignment:";
			this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(615, 39);
			this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(101, 26);
			this.label16.TabIndex = 37;
			this.label16.Text = "V Alignment:";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboGraphicHorizontalAlign
			// 
			this.comboGraphicHorizontalAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboGraphicHorizontalAlign.FormattingEnabled = true;
			this.comboGraphicHorizontalAlign.Items.AddRange(new object[] {
            "Left",
            "Center",
            "Right"});
			this.comboGraphicHorizontalAlign.Location = new System.Drawing.Point(724, 7);
			this.comboGraphicHorizontalAlign.Margin = new System.Windows.Forms.Padding(4);
			this.comboGraphicHorizontalAlign.Name = "comboGraphicHorizontalAlign";
			this.comboGraphicHorizontalAlign.Size = new System.Drawing.Size(97, 24);
			this.comboGraphicHorizontalAlign.TabIndex = 38;
			this.comboGraphicHorizontalAlign.SelectedIndexChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// comboGraphicVerticalAlign
			// 
			this.comboGraphicVerticalAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboGraphicVerticalAlign.FormattingEnabled = true;
			this.comboGraphicVerticalAlign.Items.AddRange(new object[] {
            "Top",
            "Middle",
            "Bottom"});
			this.comboGraphicVerticalAlign.Location = new System.Drawing.Point(724, 41);
			this.comboGraphicVerticalAlign.Margin = new System.Windows.Forms.Padding(4);
			this.comboGraphicVerticalAlign.Name = "comboGraphicVerticalAlign";
			this.comboGraphicVerticalAlign.Size = new System.Drawing.Size(97, 24);
			this.comboGraphicVerticalAlign.TabIndex = 39;
			this.comboGraphicVerticalAlign.SelectedIndexChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// checkLockAspect
			// 
			this.checkLockAspect.Location = new System.Drawing.Point(4, 4);
			this.checkLockAspect.Margin = new System.Windows.Forms.Padding(4);
			this.checkLockAspect.Name = "checkLockAspect";
			this.checkLockAspect.Size = new System.Drawing.Size(189, 30);
			this.checkLockAspect.TabIndex = 0;
			this.checkLockAspect.Text = "Lock Aspect Ratio";
			this.checkLockAspect.UseVisualStyleBackColor = true;
			this.checkLockAspect.CheckedChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// groupBoxOutline
			// 
			this.groupBoxOutline.Controls.Add(this.panelOutlineColor);
			this.groupBoxOutline.Controls.Add(this.label11);
			this.groupBoxOutline.Controls.Add(this.numericElementOutLineThickness);
			this.groupBoxOutline.Controls.Add(this.btnElementOutlineColor);
			this.groupBoxOutline.Location = new System.Drawing.Point(795, 23);
			this.groupBoxOutline.Margin = new System.Windows.Forms.Padding(4);
			this.groupBoxOutline.Name = "groupBoxOutline";
			this.groupBoxOutline.Padding = new System.Windows.Forms.Padding(4);
			this.groupBoxOutline.Size = new System.Drawing.Size(195, 91);
			this.groupBoxOutline.TabIndex = 44;
			this.groupBoxOutline.TabStop = false;
			this.groupBoxOutline.Text = "Outline";
			// 
			// panelOutlineColor
			// 
			this.panelOutlineColor.Location = new System.Drawing.Point(107, 22);
			this.panelOutlineColor.Margin = new System.Windows.Forms.Padding(4);
			this.panelOutlineColor.Name = "panelOutlineColor";
			this.panelOutlineColor.Size = new System.Drawing.Size(71, 25);
			this.panelOutlineColor.TabIndex = 43;
			// 
			// label11
			// 
			this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label11.Location = new System.Drawing.Point(8, 57);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(91, 25);
			this.label11.TabIndex = 22;
			this.label11.Text = "Thickness:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericElementOutLineThickness
			// 
			this.numericElementOutLineThickness.Location = new System.Drawing.Point(107, 57);
			this.numericElementOutLineThickness.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementOutLineThickness.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.numericElementOutLineThickness.Name = "numericElementOutLineThickness";
			this.numericElementOutLineThickness.Size = new System.Drawing.Size(71, 22);
			this.numericElementOutLineThickness.TabIndex = 21;
			this.numericElementOutLineThickness.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// btnElementOutlineColor
			// 
			this.btnElementOutlineColor.Location = new System.Drawing.Point(12, 22);
			this.btnElementOutlineColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementOutlineColor.Name = "btnElementOutlineColor";
			this.btnElementOutlineColor.Size = new System.Drawing.Size(87, 25);
			this.btnElementOutlineColor.TabIndex = 20;
			this.btnElementOutlineColor.Text = "Color";
			this.btnElementOutlineColor.UseVisualStyleBackColor = true;
			this.btnElementOutlineColor.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// listViewElementColumns
			// 
			this.listViewElementColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewElementColumns.FullRowSelect = true;
			this.listViewElementColumns.GridLines = true;
			this.listViewElementColumns.HideSelection = false;
			this.listViewElementColumns.Location = new System.Drawing.Point(20, 270);
			this.listViewElementColumns.Margin = new System.Windows.Forms.Padding(4);
			this.listViewElementColumns.MultiSelect = false;
			this.listViewElementColumns.Name = "listViewElementColumns";
			this.listViewElementColumns.Size = new System.Drawing.Size(1038, 84);
			this.listViewElementColumns.TabIndex = 35;
			this.listViewElementColumns.UseCompatibleStateImageBehavior = false;
			this.listViewElementColumns.View = System.Windows.Forms.View.Details;
			this.listViewElementColumns.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewElementColumns_MouseClick);
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.label8.Location = new System.Drawing.Point(8, 410);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(81, 402);
			this.label8.TabIndex = 34;
			this.label8.Text = "Definition:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(304, 60);
			this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(69, 25);
			this.label14.TabIndex = 33;
			this.label14.Text = "Opacity:";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericElementOpacity
			// 
			this.numericElementOpacity.Location = new System.Drawing.Point(381, 60);
			this.numericElementOpacity.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementOpacity.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericElementOpacity.Name = "numericElementOpacity";
			this.numericElementOpacity.Size = new System.Drawing.Size(75, 22);
			this.numericElementOpacity.TabIndex = 32;
			this.numericElementOpacity.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericElementOpacity.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(304, 90);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(69, 25);
			this.label7.TabIndex = 31;
			this.label7.Text = "Rotation:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericElementRotation
			// 
			this.numericElementRotation.Location = new System.Drawing.Point(381, 90);
			this.numericElementRotation.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementRotation.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
			this.numericElementRotation.Minimum = new decimal(new int[] {
            359,
            0,
            0,
            -2147483648});
			this.numericElementRotation.Name = "numericElementRotation";
			this.numericElementRotation.Size = new System.Drawing.Size(75, 22);
			this.numericElementRotation.TabIndex = 30;
			this.numericElementRotation.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(156, 90);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(57, 25);
			this.label6.TabIndex = 29;
			this.label6.Text = "Height:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 60);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(57, 25);
			this.label5.TabIndex = 28;
			this.label5.Text = "X:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(156, 60);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(57, 25);
			this.label4.TabIndex = 27;
			this.label4.Text = "Y:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 90);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 25);
			this.label3.TabIndex = 26;
			this.label3.Text = "Width:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 22);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 26);
			this.label2.TabIndex = 23;
			this.label2.Text = "Type:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxElementBorder
			// 
			this.groupBoxElementBorder.Controls.Add(this.panelBorderColor);
			this.groupBoxElementBorder.Controls.Add(this.label1);
			this.groupBoxElementBorder.Controls.Add(this.numericElementBorderThickness);
			this.groupBoxElementBorder.Controls.Add(this.btnElementBorderColor);
			this.groupBoxElementBorder.Location = new System.Drawing.Point(592, 23);
			this.groupBoxElementBorder.Margin = new System.Windows.Forms.Padding(4);
			this.groupBoxElementBorder.Name = "groupBoxElementBorder";
			this.groupBoxElementBorder.Padding = new System.Windows.Forms.Padding(4);
			this.groupBoxElementBorder.Size = new System.Drawing.Size(195, 91);
			this.groupBoxElementBorder.TabIndex = 24;
			this.groupBoxElementBorder.TabStop = false;
			this.groupBoxElementBorder.Text = "Border";
			// 
			// panelBorderColor
			// 
			this.panelBorderColor.Location = new System.Drawing.Point(107, 22);
			this.panelBorderColor.Margin = new System.Windows.Forms.Padding(4);
			this.panelBorderColor.Name = "panelBorderColor";
			this.panelBorderColor.Size = new System.Drawing.Size(71, 25);
			this.panelBorderColor.TabIndex = 43;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.Location = new System.Drawing.Point(8, 57);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 25);
			this.label1.TabIndex = 22;
			this.label1.Text = "Thickness:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numericElementBorderThickness
			// 
			this.numericElementBorderThickness.Location = new System.Drawing.Point(107, 57);
			this.numericElementBorderThickness.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementBorderThickness.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.numericElementBorderThickness.Name = "numericElementBorderThickness";
			this.numericElementBorderThickness.Size = new System.Drawing.Size(71, 22);
			this.numericElementBorderThickness.TabIndex = 21;
			this.numericElementBorderThickness.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// btnElementBorderColor
			// 
			this.btnElementBorderColor.Location = new System.Drawing.Point(12, 22);
			this.btnElementBorderColor.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementBorderColor.Name = "btnElementBorderColor";
			this.btnElementBorderColor.Size = new System.Drawing.Size(87, 25);
			this.btnElementBorderColor.TabIndex = 20;
			this.btnElementBorderColor.Text = "Color";
			this.btnElementBorderColor.UseVisualStyleBackColor = true;
			this.btnElementBorderColor.Click += new System.EventHandler(this.btnColor_Click);
			// 
			// numericElementH
			// 
			this.numericElementH.Location = new System.Drawing.Point(221, 90);
			this.numericElementH.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementH.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
			this.numericElementH.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericElementH.Name = "numericElementH";
			this.numericElementH.Size = new System.Drawing.Size(75, 22);
			this.numericElementH.TabIndex = 17;
			this.numericElementH.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericElementH.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// numericElementW
			// 
			this.numericElementW.Location = new System.Drawing.Point(73, 90);
			this.numericElementW.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementW.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
			this.numericElementW.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericElementW.Name = "numericElementW";
			this.numericElementW.Size = new System.Drawing.Size(75, 22);
			this.numericElementW.TabIndex = 16;
			this.numericElementW.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericElementW.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// numericElementY
			// 
			this.numericElementY.Location = new System.Drawing.Point(221, 60);
			this.numericElementY.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementY.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
			this.numericElementY.Minimum = new decimal(new int[] {
            65536,
            0,
            0,
            -2147483648});
			this.numericElementY.Name = "numericElementY";
			this.numericElementY.Size = new System.Drawing.Size(75, 22);
			this.numericElementY.TabIndex = 15;
			this.numericElementY.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// numericElementX
			// 
			this.numericElementX.Location = new System.Drawing.Point(73, 60);
			this.numericElementX.Margin = new System.Windows.Forms.Padding(4);
			this.numericElementX.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
			this.numericElementX.Minimum = new decimal(new int[] {
            65536,
            0,
            0,
            -2147483648});
			this.numericElementX.Name = "numericElementX";
			this.numericElementX.Size = new System.Drawing.Size(75, 22);
			this.numericElementX.TabIndex = 14;
			this.numericElementX.ValueChanged += new System.EventHandler(this.HandleElementValueChange);
			// 
			// btnElementBrowseImage
			// 
			this.btnElementBrowseImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnElementBrowseImage.Location = new System.Drawing.Point(1029, 789);
			this.btnElementBrowseImage.Margin = new System.Windows.Forms.Padding(4);
			this.btnElementBrowseImage.Name = "btnElementBrowseImage";
			this.btnElementBrowseImage.Size = new System.Drawing.Size(33, 25);
			this.btnElementBrowseImage.TabIndex = 12;
			this.btnElementBrowseImage.Text = "...";
			this.btnElementBrowseImage.UseVisualStyleBackColor = true;
			this.btnElementBrowseImage.Click += new System.EventHandler(this.btnElementBrowseImage_Click);
			// 
			// txtElementVariable
			// 
			this.txtElementVariable.AcceptsReturn = true;
			this.txtElementVariable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtElementVariable.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtElementVariable.HideSelection = false;
			this.txtElementVariable.Location = new System.Drawing.Point(97, 362);
			this.txtElementVariable.Margin = new System.Windows.Forms.Padding(4);
			this.txtElementVariable.Multiline = true;
			this.txtElementVariable.Name = "txtElementVariable";
			this.txtElementVariable.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtElementVariable.Size = new System.Drawing.Size(922, 452);
			this.txtElementVariable.TabIndex = 1;
			this.txtElementVariable.WordWrap = false;
			this.txtElementVariable.TextChanged += new System.EventHandler(this.HandleElementValueChange);
			this.txtElementVariable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtElementVariable_KeyDown);
			// 
			// comboElementType
			// 
			this.comboElementType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboElementType.FormattingEnabled = true;
			this.comboElementType.Location = new System.Drawing.Point(73, 23);
			this.comboElementType.Margin = new System.Windows.Forms.Padding(4);
			this.comboElementType.Name = "comboElementType";
			this.comboElementType.Size = new System.Drawing.Size(139, 24);
			this.comboElementType.TabIndex = 0;
			this.comboElementType.SelectedIndexChanged += new System.EventHandler(this.comboElementType_SelectedIndexChanged);
			// 
			// contextMenuReferenceStrip
			// 
			this.contextMenuReferenceStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuReferenceStrip.Name = "contextMenuReferenceStrip";
			this.contextMenuReferenceStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.contextMenuReferenceStrip.Size = new System.Drawing.Size(61, 4);
			// 
			// contextMenuStripAssist
			// 
			this.contextMenuStripAssist.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuStripAssist.Name = "contextMenuStripAssist";
			this.contextMenuStripAssist.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.contextMenuStripAssist.Size = new System.Drawing.Size(61, 4);
			// 
			// MDIElementControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1070, 821);
			this.Controls.Add(this.groupBoxElement);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(1002, 406);
			this.Name = "MDIElementControl";
			this.ShowIcon = false;
			this.Text = " Element Control";
			this.Load += new System.EventHandler(this.MDIElementControl_Load);
			this.groupBoxElement.ResumeLayout(false);
			this.groupBoxElement.PerformLayout();
			this.groupBackgroundColor.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabPageFont.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericGradientDegrees)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericWordSpace)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericLineSpace)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericFontSize)).EndInit();
			this.tabPageShape.ResumeLayout(false);
			this.tabPageGraphic.ResumeLayout(false);
			this.tabPageGraphic.PerformLayout();
			this.groupBoxOutline.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericElementOutLineThickness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementOpacity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementRotation)).EndInit();
			this.groupBoxElementBorder.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericElementBorderThickness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementH)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementW)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericElementX)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxElement;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown numericElementOpacity;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericElementRotation;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboTextVerticalAlign;
        private System.Windows.Forms.ComboBox comboTextHorizontalAlign;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnElementFontColor;
        private System.Windows.Forms.GroupBox groupBoxElementBorder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericElementBorderThickness;
        private System.Windows.Forms.Button btnElementBorderColor;
        private System.Windows.Forms.Button btnElementBrowseImage;
        private System.Windows.Forms.TextBox txtElementVariable;
        private System.Windows.Forms.ComboBox comboElementType;
        private System.Windows.Forms.NumericUpDown numericElementH;
        private System.Windows.Forms.NumericUpDown numericElementW;
        private System.Windows.Forms.NumericUpDown numericElementY;
        private System.Windows.Forms.NumericUpDown numericElementX;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListView listViewElementColumns;
        private System.Windows.Forms.ComboBox comboFontName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numericFontSize;
        private System.Windows.Forms.CheckBox checkBoxUnderline;
        private System.Windows.Forms.CheckBox checkBoxStrikeout;
        private System.Windows.Forms.Button btnElementShapeColor;
        private System.Windows.Forms.PropertyGrid propertyGridShape;
        private System.Windows.Forms.ComboBox comboShapeType;
        private System.Windows.Forms.Panel panelBorderColor;
        private System.Windows.Forms.Panel panelShapeColor;
        private System.Windows.Forms.Panel panelFontColor;
        private System.Windows.Forms.GroupBox groupBoxOutline;
        private System.Windows.Forms.Panel panelOutlineColor;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericElementOutLineThickness;
        private System.Windows.Forms.Button btnElementOutlineColor;
        private System.Windows.Forms.CheckBox checkFontAutoScale;
        private System.Windows.Forms.CheckBox checkLockAspect;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageFont;
        private System.Windows.Forms.TabPage tabPageShape;
        private System.Windows.Forms.TabPage tabPageGraphic;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox comboGraphicHorizontalAlign;
        private System.Windows.Forms.ComboBox comboGraphicVerticalAlign;
        private System.Windows.Forms.Label lblLineSpace;
        private System.Windows.Forms.NumericUpDown numericLineSpace;
        private System.Windows.Forms.NumericUpDown numericWordSpace;
        private System.Windows.Forms.Label lblWordSpacing;
        private System.Windows.Forms.Button btnSetSizeToImage;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.CheckBox checkBoxBold;
        private System.Windows.Forms.ContextMenuStrip contextMenuReferenceStrip;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAssist;
        private System.Windows.Forms.Button btnAssist;
        private System.Windows.Forms.CheckBox checkKeepOriginalSize;
        private System.Windows.Forms.CheckBox checkJustifiedText;
        private System.Windows.Forms.GroupBox groupBackgroundColor;
        private System.Windows.Forms.Button btnNullBackgroundColor;
        private System.Windows.Forms.Panel panelBackgroundColor;
        private System.Windows.Forms.Button btnElementBackgroundColor;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtTileSize;
        private System.Windows.Forms.Panel panelGraphicColor;
        private System.Windows.Forms.Button btnElementGraphicColor;
        private System.Windows.Forms.ComboBox comboElementMirror;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.CheckBox checkCenterOnOrigin;
		private System.Windows.Forms.Panel panelGradientColor;
		private System.Windows.Forms.Button btnElementGradientColor;
		private System.Windows.Forms.CheckBox checkBoxUseGradient;
		private System.Windows.Forms.Label labelGradientAngle;
		private System.Windows.Forms.NumericUpDown numericGradientDegrees;
	}
}