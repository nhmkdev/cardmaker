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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace Support.UI
{
	public class QueryPanel
	{
        // Critical Note: It is important that the user of a QueryPanel set the auto scroll option after all controls are added!

		protected const int X_CONTROL_BUFFER = 8;
        protected const int X_BUTTON_WIDTH = 24;
        protected const int Y_CONTROL_BUFFER = 4;
        protected const int Y_CONTROL_HEIGHT = 20; // default textbox height
        protected const int X_NUMERIC_WIDTH = 100;
        
        protected int X_LABEL_SIZE = 80; // this one varies based on the overall width
        private readonly Dictionary<object, QueryItem> m_dictionaryItems;
        private Control m_zCurrentLayoutControl; // the current panel / tab page to add controls to
        private bool m_bTabbed;
        private int m_nTabIndex; // the tab index value of a control

	    protected Dictionary<Control, List<Control>> m_dictionaryLayoutControlControls = new Dictionary<Control, List<Control>>();

        protected int m_nButtonHeight;
        protected TabControl m_zTabControl;
        protected Panel m_zPanel;

		public enum ControlType
		{
			TextBox,
			ComboBox,
			PullDownBox,
			CheckBox,
			NumBox,
            NumBoxSlider,
            BrowseBox,
			Label,
            Button,
            ListBox,
            DateTimePicker,
			None
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zPanel">Empty Panel to add controls to</param>
        /// <param name="bTabbed">Flag indicating whether to use a tab control</param>
        public QueryPanel(Panel zPanel, bool bTabbed)
		{
            m_dictionaryItems = new Dictionary<object,QueryItem>();
            InitPanel(zPanel, bTabbed);
			X_LABEL_SIZE = (int)((float)m_zPanel.Width * 0.25);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zPanel">Empty Panel to add controls to</param>
        /// <param name="nLabelWidth">Desired with of labels</param>
        /// <param name="bTabbed">Flag indicating whether to use a tab control</param>
        public QueryPanel(Panel zPanel, int nLabelWidth, bool bTabbed)
		{
            m_dictionaryItems = new Dictionary<object, QueryItem>();
            InitPanel(zPanel, bTabbed);
			if(0 < nLabelWidth && nLabelWidth < m_zPanel.Width)
			{
				X_LABEL_SIZE = nLabelWidth;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zPanel">Panel to setup and configure</param>
        /// <param name="bTabbed">Flag indicating whether to use a tab control</param>
        private void InitPanel(Panel zPanel, bool bTabbed)
		{
            m_nButtonHeight = new Button().Height;

            // setup the panel to contain the controls
            m_zPanel = zPanel ?? new Panel();
            m_zPanel.AutoScroll = false;
            m_zCurrentLayoutControl = m_zPanel; // default to this as the main item

            if (bTabbed)
            {
                m_bTabbed = true;
                m_zTabControl = new TabControl
                {
                    Dock = DockStyle.Fill,
                    ClientSize = new Size(0, 0)
                };
                m_zPanel.Controls.Add(m_zTabControl);
                // The SwitchToTab method is used to add items
            }
             
            // initialize
            m_zCurrentLayoutControl.Tag = Y_CONTROL_BUFFER;
		}

        /// <summary>
        /// Performs any finalization process related to the controls (intended for use before showing!)
        /// </summary>
	    public void FinalizeControls()
	    {
	        // add the panel controls after the client size has been set (adding them before displayed an odd issue with control anchor/size)
	        foreach (var zLayoutControl in m_dictionaryLayoutControlControls.Keys)
	        {
                m_dictionaryLayoutControlControls[zLayoutControl].ForEach(zControl => zLayoutControl.Controls.Add(zControl));
	        }
        }

        /// <summary>
        /// Adds the control in a pending state to be placed on the panel by FinalizeControls
        /// </summary>
        /// <param name="zControl">The control to add</param>
	    private void AddPendingControl(Control zControl)
        {
            List<Control> listControls;
            if (!m_dictionaryLayoutControlControls.TryGetValue(m_zCurrentLayoutControl, out listControls))
            {
                listControls = new List<Control>();
                m_dictionaryLayoutControlControls.Add(m_zCurrentLayoutControl, listControls);
            }
            listControls.Add(zControl);
	    }

        /// <summary>
		/// Adds a label
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="nHeight">Label height</param>
		public Label AddLabel(string sLabel, int nHeight)
		{
			Label zLabel = CreateLabel(sLabel);
			zLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
			zLabel.TextAlign = ContentAlignment.MiddleLeft;
            zLabel.Size = new Size(m_zCurrentLayoutControl.ClientSize.Width - (X_CONTROL_BUFFER * 2), nHeight);
            AddToYPosition(zLabel.Height + Y_CONTROL_BUFFER);
		    AddPendingControl(zLabel);
            return zLabel;
		}

		/// <summary>
		/// Adds vertical spacing
		/// </summary>
		/// <param name="nHeight">The amount of space to add.</param>
		public void AddVerticalSpace(int nHeight)
		{
            AddToYPosition(nHeight);
		}

		/// <summary>
		/// Adds a check box with an associated label.
		/// </summary>
		/// <param name="sLabel">Label seting</param>
		/// <param name="bCheckDefault">Default check box state</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		public CheckBox AddCheckBox(string sLabel, bool bCheckDefault, object zQueryKey)
		{
			var zLabel = CreateLabel(sLabel);
		    var zCheck = new CheckBox
		    {
    			Checked = bCheckDefault
		    };
			SetupControl(zCheck, zLabel, ControlType.CheckBox, true, zQueryKey);
            return zCheck;
		}

        /// <summary>
        /// Adds a button
        /// </summary>
        /// <param name="sLabel">Text label of the button</param>
        /// <param name="nDesiredWidth">The desired width of the button</param>
        /// <param name="eHandler">The event handler to associated with the button</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public Button AddButton(string sLabel, int nDesiredWidth, EventHandler eHandler, object zQueryKey)
        {
            var zButton = new Button
            {
                Text = sLabel
            };
            zButton.Size = new Size(nDesiredWidth, zButton.Height);
            if (null != eHandler)
            {
                zButton.Click += eHandler;
            }
            SetupControl(zButton, null, ControlType.Button, false, zQueryKey);
            return zButton;
        }

        /// <summary>
        /// Adds a NumericUpDown
        /// </summary>
        /// <param name="sLabel">Label string</param>
        /// <param name="dDefault">Default value</param>
        /// <param name="dMin">Minimum value</param>
        /// <param name="dMax">Maximum value</param>
        /// <param name="dIncrement">Increment amout</param>
        /// <param name="nDecimalPlaces">decimal places</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public NumericUpDown AddNumericBox(string sLabel, decimal dDefault, decimal dMin, decimal dMax, decimal dIncrement, int nDecimalPlaces, object zQueryKey)
        {
            var zLabel = CreateLabel(sLabel);
            var zNumeric = new NumericUpDown
            {
                Minimum = dMin,
                Maximum = dMax,
                Increment = dIncrement,
                DecimalPlaces = nDecimalPlaces
            };
            if (dMin <= dDefault && dMax >= dDefault)
            {
                zNumeric.Value = dDefault;
            }
            SetupControl(zNumeric, zLabel, ControlType.NumBox, true, zQueryKey);
            return zNumeric;
        }

        /// <summary>
        /// Adds a NumericUpDown
        /// </summary>
        /// <param name="sLabel">Label string</param>
        /// <param name="dDefault">Default value</param>
        /// <param name="dMin">Minimum value</param>
        /// <param name="dMax">Maximum value</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public NumericUpDown AddNumericBox(string sLabel, decimal dDefault, decimal dMin, decimal dMax, object zQueryKey)
        {
            return AddNumericBox(sLabel, dDefault, dMin, dMax, 1, 0, zQueryKey);
        }

        /// <summary>
        /// Adds a NumericUpDown with associated slider
        /// </summary>
        /// <param name="sLabel">Label string</param>
        /// <param name="bFloat">Flag indicating whether the values associated are floating point</param>
        /// <param name="dDefault">Default value</param>
        /// <param name="dMin">Minimum value</param>
        /// <param name="dMax">Maximum value</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public NumericUpDown AddNumericBoxSlider(string sLabel, bool bFloat, decimal dDefault, decimal dMin, decimal dMax, object zQueryKey)
        {
            var zLabel = CreateLabel(sLabel);
            var zNumeric = new NumericUpDown();
            var zTrackBar = new TrackBar();

            zNumeric.Minimum = dMin;
            zNumeric.Maximum = dMax;
            zNumeric.Increment = 1;
            zNumeric.Value = dMin; // default this to a valid number...
            if (bFloat)
            {
                int nZeroDecimalPlaces = 3 - (int)Math.Log10(Math.Max(Math.Abs((double)dMin), Math.Abs((double)dMax)));
                // note the trackbar value is set below using the numeric change event
                if (0 <= nZeroDecimalPlaces)
                {
                    zNumeric.Increment = new Decimal(
                        float.Parse("0." + "1".PadLeft(1 + nZeroDecimalPlaces, '0'), NumberStyles.Any, CultureInfo.InvariantCulture));
                    zNumeric.DecimalPlaces = nZeroDecimalPlaces + 1;
                }
                else
                {
                    zNumeric.Increment = 1;
                    zNumeric.DecimalPlaces = 0;
                }
                zTrackBar.Minimum = 0;
                zTrackBar.Maximum = ((int)(dMax / zNumeric.Increment)) - ((int)(dMin / zNumeric.Increment));
            }
            else
            {
                zTrackBar.Minimum = (int)dMin;
                zTrackBar.Maximum = (int)dMax;
                zTrackBar.Value = (int)dDefault;
            }

            if (dDefault >= dMin && dDefault <= dMax)
            {
                zNumeric.Value = dDefault;
            }

            zNumeric.Location = new Point(GetLabelWidth(zLabel) + (X_CONTROL_BUFFER), GetYPosition());
            zNumeric.Size = new Size(X_NUMERIC_WIDTH, Y_CONTROL_HEIGHT);
            zNumeric.Tag = zTrackBar; // the tag of the numeric is the trackbar
            zNumeric.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            zNumeric.ValueChanged += numeric_ValueChanged;
            zLabel.Height = zNumeric.Height; // adjust the height of the label to match the control to its right

            zTrackBar.Location = new Point(zNumeric.Width + zNumeric.Location.X + X_CONTROL_BUFFER, GetYPosition());
            zTrackBar.Size = new Size(m_zPanel.ClientSize.Width - (zTrackBar.Location.X + X_CONTROL_BUFFER), Y_CONTROL_HEIGHT);
            zTrackBar.Tag = zNumeric; // the tag of the trackbar is the numeric (value changes will affect the numeric)
            zTrackBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            zTrackBar.ValueChanged += numericSlider_ValueChanged;

            if (bFloat)
            {
                // set the trackbar value using the change event
                numeric_ValueChanged(zNumeric, new EventArgs());
            }

            AddPendingControl(zLabel);
            AddPendingControl(zNumeric);
            AddPendingControl(zTrackBar);
            AddToYPosition(zTrackBar.Size.Height + Y_CONTROL_BUFFER);
            var qItem = new QueryItem(ControlType.NumBoxSlider, zNumeric, zTrackBar, ref m_nTabIndex); // the tag of the QueryItem is the trackbar (used when disabling the QueryItem)
            m_dictionaryItems.Add(zQueryKey, qItem);
            return zNumeric;
        }

		/// <summary>
		/// Adds a TextBox
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="sDefaultValue">Default text</param>
        /// <param name="bPassword">Flag indicating that this is a password textbox</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		public TextBox AddTextBox(string sLabel, string sDefaultValue, bool bPassword, object zQueryKey)
		{
			return AddTextBox(sLabel, sDefaultValue, false, bPassword, 0, zQueryKey);
		}

		/// <summary>
		/// Adds a multiline TextBox
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="sDefaultValue">Default text</param>
		/// <param name="nHeight">Height of the TextBox</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		public TextBox AddMultiLineTextBox(string sLabel, string sDefaultValue, int nHeight, object zQueryKey)
		{
			return AddTextBox(sLabel, sDefaultValue, true, false, nHeight, zQueryKey);
		}

		/// <summary>
		/// Adds a TextBox
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="sDefaultValue">Default text</param>
		/// <param name="bMultiLine">Flag indicating whether this is a multi line textbox</param>
        /// <param name="bPassword">Flag indicating that this is a password textbox</param>
		/// <param name="nHeight">Height of the text box. This only applies to those with bMultiLine set to true</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		private TextBox AddTextBox(string sLabel, string sDefaultValue, bool bMultiLine, bool bPassword, int nHeight, object zQueryKey)
		{
            var zLabel = CreateLabel(sLabel);
            var zText = new TextBox();
            if (bPassword)
            {
                zText.PasswordChar = 'x';
            }
            zText.Multiline = bMultiLine;
			zText.Text = sDefaultValue;
			if(bMultiLine)
			{
				zText.AcceptsReturn = true;
                zText.Size = new Size(m_zCurrentLayoutControl.ClientSize.Width - ((X_CONTROL_BUFFER * 2) + GetLabelWidth(zLabel)), nHeight);
				zText.ScrollBars = ScrollBars.Both;
				zText.WordWrap = false;
			}
			else
			{
                zText.Size = new Size(m_zCurrentLayoutControl.ClientSize.Width - ((X_CONTROL_BUFFER * 2) + GetLabelWidth(zLabel)), Y_CONTROL_HEIGHT);
			}
			SetupControl(zText, zLabel, ControlType.TextBox, false, zQueryKey);
            return zText;
        }

		/// <summary>
		/// Adds a ComboBox
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="arrayEntries">Array of strings to be used in the combo box</param>
		/// <param name="nDefaultIndex">Default index of the combo box</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		public ComboBox AddComboBox(string sLabel, string[] arrayEntries, int nDefaultIndex, object zQueryKey)
		{
			return AddComboBox(sLabel, arrayEntries, nDefaultIndex, false, zQueryKey);
		}

		/// <summary>
		/// Adds a ComboBox with the pulldownlist style.
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="arrayEntries">Array of strings to be used in the combo box</param>
		/// <param name="nDefaultIndex">Default index of the combo box</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public ComboBox AddPullDownBox(string sLabel, string[] arrayEntries, int nDefaultIndex, object zQueryKey)
		{
			return AddComboBox(sLabel, arrayEntries, nDefaultIndex, true, zQueryKey);
		}

		/// <summary>
		/// Adds a combo box with the items specified (based on the type specified)
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="arrayEntries">Array of strings to be used in the combo box</param>
		/// <param name="nDefaultIndex">Default index of the combo box</param>
		/// <param name="bPulldown">Flag indicating whether this is a pulldownlist or not</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        private ComboBox AddComboBox(string sLabel, string[] arrayEntries, int nDefaultIndex, bool bPulldown, object zQueryKey)
		{
            var zLabel = CreateLabel(sLabel);
            var zCombo = new ComboBox();
            if (null != arrayEntries)
            {
                foreach (var entry in arrayEntries)
                {
                    zCombo.Items.Add(entry);
                }
                if (zCombo.Items.Count > nDefaultIndex)
                {
                    zCombo.SelectedIndex = nDefaultIndex;
                }
                else if (0 < zCombo.Items.Count)
                {
                    zCombo.SelectedIndex = 0;
                }
            }
			if(bPulldown)
			{
				zCombo.DropDownStyle = ComboBoxStyle.DropDownList;
				SetupControl(zCombo, zLabel, ControlType.PullDownBox, true, zQueryKey);
			}
			else
			{
				zCombo.DropDownStyle = ComboBoxStyle.DropDown;
				SetupControl(zCombo, zLabel, ControlType.ComboBox, true, zQueryKey);
			}
            return zCombo;
		}

        /// <summary>
        /// Adds a ListBox with the specified items and selected items
        /// </summary>
        /// <param name="sLabel">Label string</param>
        /// <param name="arrayEntries">Array of strings as entries</param>
        /// <param name="arraySelected">Array of indicies to select</param>
        /// <param name="bMultiSelect">Flag indicating whether multiple items can be selected</param>
        /// <param name="nHeight">The desired height of the ListBox</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public ListBox AddListBox(string sLabel, string[] arrayEntries, int[] arraySelected, bool bMultiSelect, int nHeight, object zQueryKey)
        {
            var zLabel = CreateLabel(sLabel);
            var zListBox = new ListBox
            {
                SelectionMode = bMultiSelect ? SelectionMode.MultiSimple : SelectionMode.One
            };
            if (null != arrayEntries)
            {
                foreach (var sEntry in arrayEntries)
                {
                    zListBox.Items.Add(sEntry);
                }
                if (null != arraySelected)
                {
                    foreach (var nIndex in arraySelected)
                    {
                        if ((-1 < nIndex) && (nIndex < zListBox.Items.Count))
                        {
                            zListBox.SelectedIndex = nIndex;
                        }
                    }
                }
            }
            zListBox.Height = nHeight;
            SetupControl(zListBox, zLabel, ControlType.ListBox, true, zQueryKey);
            return zListBox;
        }

        /// <summary>
        /// Adds a DateTime picker field
        /// </summary>
        /// <param name="sLabel">Label string</param>
        /// <param name="eFormat">DateTimePickerFormat to control the visual component</param>
        /// <param name="dtValue">The default date time value</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        /// <returns></returns>
        public DateTimePicker AddDateTimePicker(string sLabel, DateTimePickerFormat eFormat, DateTime dtValue, object zQueryKey)
        {
            var zLabel = CreateLabel(sLabel);
            var zPicker = new DateTimePicker
            {
                Format = eFormat,
                Value = dtValue
            };
            switch (eFormat)
            {
                case DateTimePickerFormat.Time:
                    zPicker.ShowUpDown = true;
                    break;
            }
            SetupControl(zPicker, zLabel, ControlType.DateTimePicker, true, zQueryKey);
            return zPicker;
        }

		/// <summary>
		/// Support method to setup the control location/size. This method also adds the control to the form.
		/// </summary>
		/// <param name="zControl">Control to configure</param>
		/// <param name="zLabel">Label Control associated with the Control</param>
		/// <param name="eType">ControlType</param>
		/// <param name="bApplySize">Apply size based on form flag</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		private void SetupControl(Control zControl, Label zLabel, ControlType eType, bool bApplySize, object zQueryKey)
		{
            if (null != zLabel)
            {
                if (bApplySize)
                {
                    zControl.Size = new Size((m_zCurrentLayoutControl.ClientSize.Width - GetLabelWidth(zLabel)) - (X_CONTROL_BUFFER * 2), zControl.Size.Height);
                }

                zControl.Location = new Point(zLabel.Location.X + GetLabelWidth(zLabel), GetYPosition());
                zLabel.Height = zControl.Height;
                AddPendingControl(zLabel);
                AddToYPosition(Math.Max(zLabel.Height, zControl.Height) + Y_CONTROL_BUFFER);
            }
            else
            {
                zControl.Location = new Point((m_zCurrentLayoutControl.ClientSize.Width - zControl.Width) - X_CONTROL_BUFFER, GetYPosition());
                AddToYPosition(zControl.Height + Y_CONTROL_BUFFER);
            }

            zControl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            AddPendingControl(zControl);
            var qItem = new QueryItem(eType, zControl, ref m_nTabIndex);
            m_dictionaryItems.Add(zQueryKey, qItem);
		}

		/// <summary>
		/// Adds a folder browser component
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="sDefault">Default string</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		public TextBox AddFolderBrowseBox(string sLabel, string sDefault, object zQueryKey)
		{
			return AddBrowseBox(sLabel, sDefault, null, zQueryKey);
		}

		/// <summary>
		/// Adds a file browser component.
		/// </summary>
		/// <param name="sLabel">Label string</param>
		/// <param name="sDefault">Default string</param>
		/// <param name="sFilter">File filter (standard format for OpenFileDialog), string.empty for default *.*</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public TextBox AddFileBrowseBox(string sLabel, string sDefault, string sFilter, object zQueryKey)
		{
			return AddBrowseBox(sLabel, sDefault, sFilter, zQueryKey);
		}

		/// <summary>
		/// Adds a browse component (button/textbox/label)
		/// </summary>
		/// <param name="sLabel">Label for the component</param>
		/// <param name="sDefault">Default value</param>
		/// <param name="sFilter">File filter (applies to file browsing only)</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		private TextBox AddBrowseBox(string sLabel, string sDefault, string sFilter, object zQueryKey)
		{
            var zLabel = CreateLabel(sLabel);
            var zButton = new Button();
		    var zTextLocation = new Point(GetLabelWidth(zLabel) + (X_CONTROL_BUFFER), GetYPosition());
		    var zText = new TextBox
		    {
                Text = sDefault,
                Location = zTextLocation,
                Size =
		            new Size(
                        m_zCurrentLayoutControl.ClientSize.Width - (zTextLocation.X + X_BUTTON_WIDTH + (X_CONTROL_BUFFER * 2)),
		                Y_CONTROL_HEIGHT),
                Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top
		    };

            if (null != sFilter)
            {
                zText.Tag = 0 != sFilter.Length
                    ? sFilter
                    : "All files (*.*)|*.*";
            }
            zLabel.Height = zText.Height; // adjust the height of the label to match the control to its right

			zButton.Text = "...";
			zButton.Size = new Size(X_BUTTON_WIDTH, Y_CONTROL_HEIGHT);
            zButton.Location = new Point(m_zCurrentLayoutControl.ClientSize.Width - (zButton.Size.Width + X_CONTROL_BUFFER), GetYPosition());
			zButton.Tag = zText; // the tag of the button is the textbox
			zButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
			zButton.Click += zButton_Click;

		    AddPendingControl(zLabel);
		    AddPendingControl(zText);
		    AddPendingControl(zButton);
            AddToYPosition(zText.Size.Height + Y_CONTROL_BUFFER);
            var qItem = new QueryItem(ControlType.BrowseBox, zText, zButton, ref m_nTabIndex); // the tag of the QueryItem is the button (used when disabling the QueryItem)
            m_dictionaryItems.Add(zQueryKey, qItem);
            return zText;
		}

        /// <summary>
        /// Adds a browse component (button/textbox/label)
        /// </summary>
        /// <param name="sLabel">Label for the component</param>
        /// <param name="sDefault">Default value</param>
        /// <param name="actionBrowseClicked">Function that returns the form to show (or null if it should not)</param>
        /// <param name="actionSelect">Action to take with the dialog and TextBox (if the result is OK)</param>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        public TextBox AddSelectorBox<T>(string sLabel, string sDefault, Func<T> actionBrowseClicked, Action<T, TextBox> actionSelect, object zQueryKey) where T : Form
	    {
	        var zLabel = CreateLabel(sLabel);
	        var zButton = new Button();
	        var zTextLocation = new Point(GetLabelWidth(zLabel) + (X_CONTROL_BUFFER), GetYPosition());
	        var zText = new TextBox
	        {
	            Text = sDefault,
	            Location = zTextLocation,
	            Size =
	                new Size(
	                    m_zCurrentLayoutControl.ClientSize.Width - (zTextLocation.X + X_BUTTON_WIDTH + (X_CONTROL_BUFFER * 2)),
	                    Y_CONTROL_HEIGHT),
	            Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top
	        };

            zLabel.Height = zText.Height; // adjust the height of the label to match the control to its right

	        zButton.Text = "...";
	        zButton.Size = new Size(X_BUTTON_WIDTH, Y_CONTROL_HEIGHT);
	        zButton.Location = new Point(m_zCurrentLayoutControl.ClientSize.Width - (zButton.Size.Width + X_CONTROL_BUFFER), GetYPosition());
	        zButton.Tag = zText; // the tag of the button is the textbox
	        zButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
	        zButton.Click += (sender, args) =>
	        {
	            var zDialog = actionBrowseClicked();

                if (null == zDialog)
	            {
	                return;
	            }

                if (DialogResult.OK == zDialog.ShowDialog(this.m_zPanel))
	            {
	                actionSelect(zDialog, zText);
	            }
            };

	        AddPendingControl(zLabel);
	        AddPendingControl(zText);
	        AddPendingControl(zButton);
	        AddToYPosition(zText.Size.Height + Y_CONTROL_BUFFER);
	        var qItem = new QueryItem(ControlType.BrowseBox, zText, zButton, ref m_nTabIndex); // the tag of the QueryItem is the button (used when disabling the QueryItem)
	        m_dictionaryItems.Add(zQueryKey, qItem);
	        return zText;
	    }

        /// <summary>
        /// Created a label based on the current y position
        /// </summary>
        /// <param name="sLabel">The Label string</param>
        /// <returns></returns>
        private Label CreateLabel(string sLabel)
		{
            var zLabel = new Label();
			if(null != sLabel)
			{
				zLabel.Text = sLabel;
				zLabel.TextAlign = ContentAlignment.MiddleRight;
				zLabel.Location = new Point(X_CONTROL_BUFFER, GetYPosition());
				zLabel.Size = new Size(X_LABEL_SIZE, Y_CONTROL_HEIGHT);
			}
			else
			{
				zLabel.Location = new Point(X_CONTROL_BUFFER, GetYPosition());
				zLabel.Size = new Size(0, Y_CONTROL_HEIGHT);
			}
			return zLabel;
		}

        /// <summary>
        /// Changes to the tab specified and creates it if necessary
        /// </summary>
        /// <param name="sTabName">Name of the tab to change to</param>
        public void ChangeToTab(string sTabName)
        {
            AddTab(sTabName);
        }

        /// <summary>
        /// Creates a Tab
        /// </summary>
        /// <param name="sTabName">Name of the tab to create</param>
        /// <returns></returns>
        public TabPage AddTab(string sTabName)
        {
            if (!m_bTabbed)
            {
                throw new Exception("QueryPanel: Attempted to add tab on non-tabbed QueryPanel.");
            }
            if (m_zTabControl.TabPages.ContainsKey(sTabName))
            {
                TabPage zPage = m_zTabControl.TabPages[sTabName];
                m_zCurrentLayoutControl = zPage;
                return zPage;
            }
            else
            {
                m_zTabControl.TabPages.Add(sTabName, sTabName);
                var zPage = m_zTabControl.TabPages[sTabName];
                zPage.Tag = Y_CONTROL_BUFFER; // stores the current Y Position
                zPage.AutoScroll = true;
                zPage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                zPage.Dock = DockStyle.Fill;
                m_zCurrentLayoutControl = zPage;
                return zPage;
            }
        }

        /// <summary>
		/// Adds the specified control to be enabled when the given control is enabled.
		/// </summary>
        /// <param name="zQueryKey">The parent control to base the enable state on</param>
        /// <param name="zQueryKeyEnable">The control to enable/disable based on the parent control state</param>
		/// <returns>true on success, false otherwise</returns>
		public bool AddEnableControl(object zQueryKey, object zQueryKeyEnable)
		{
            QueryItem zQueryItem, zQueryItemEnable;
            if (m_dictionaryItems.TryGetValue(zQueryKey, out zQueryItem) && m_dictionaryItems.TryGetValue(zQueryKeyEnable, out zQueryItemEnable))
			{
                (zQueryItem).AddEnableControl(zQueryItemEnable);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Adds the specified controls to be enabled when the given control is enabled.
		/// </summary>
        /// <param name="zQueryKey">The parent control to base the enable state on</param>
		/// <param name="arrayQueryKeyEnable">string[] of controls to enable/disable based on the parent control state</param>
		/// <returns>true on success, false otherwise</returns>
		public bool AddEnableControls(object zQueryKey, object[] arrayQueryKeyEnable)
		{
            var bRet = true;
			foreach(var zKey in arrayQueryKeyEnable)
			{
                bRet &= AddEnableControl(zQueryKey, zKey);
			}
			return bRet;
		}

		/// <summary>
		/// This gets the width of the label + the control buffer (or 0 if the label is empty)
		/// </summary>
		/// <param name="zLabel"></param>
		/// <returns></returns>
		private int GetLabelWidth(Label zLabel)
		{
			return (zLabel.Width == 0) ? 0 : zLabel.Width + X_CONTROL_BUFFER;
		}

        public static void SetupScrollState(ScrollableControl scrollableControl)
        {
            Type scrollableControlType = typeof(ScrollableControl);
            MethodInfo setScrollStateMethod = scrollableControlType.GetMethod("SetScrollState", BindingFlags.NonPublic | BindingFlags.Instance);
            setScrollStateMethod.Invoke(scrollableControl, new object[] { 0x10 /*ScrollableControl.ScrollStateFullDrag*/, true });
        }

		#region Value Getters

		/// <summary>
		/// Gets the control associated with the query key
		/// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		/// <returns>The control associated with the query key</returns>
		public Control GetControl(object zQueryKey)
		{
            var zItem = GetQueryItem(zQueryKey);
			if(null != zItem)
			{
				return zItem.QueryControl;
			}
			ThrowBadQueryException();
			return null;
		}

		/// <summary>
		/// Gets the state of the specified check box
		/// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		/// <returns></returns>
		public bool GetBool(object zQueryKey)
		{
            var zItem = GetQueryItem(zQueryKey);
			if(null != zItem)
			{
				if(ControlType.CheckBox == zItem.Type)
				{
					return ((CheckBox)zItem.QueryControl).Checked;
				}
				ThrowWrongTypeException();
			}
			return false;
		}

		/// <summary>
		/// Gets the index of the specified combo box
		/// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		/// <returns></returns>
		public int GetIndex(object zQueryKey)
		{
            var zItem = GetQueryItem(zQueryKey);
			if(null != zItem)
			{
				switch(zItem.Type)
				{
					case ControlType.PullDownBox:
					case ControlType.ComboBox:
						return ((ComboBox)zItem.QueryControl).SelectedIndex;
                    case ControlType.ListBox:
                        return ((ListBox)zItem.QueryControl).SelectedIndex;
				}
				ThrowWrongTypeException();
			}
			return 0;
		}

        /// <summary>
        /// Gets the selected indices of the given control
        /// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        /// <returns>Array of selected indices, NULL if none are selected</returns>
        public int[] GetIndices(object zQueryKey)
        {
            var zItem = GetQueryItem(zQueryKey);
            if (null != zItem)
            {
                switch (zItem.Type)
                {
                    case ControlType.ListBox:
                        var zListBox = (ListBox)zItem.QueryControl;
                        var arrayItems = new int[zListBox.SelectedIndices.Count];
                        for (var nIdx = 0; nIdx < arrayItems.Length; nIdx++)
                        {
                            arrayItems[nIdx] = zListBox.SelectedIndices[nIdx];
                        }
                        return arrayItems;
                }
                ThrowWrongTypeException();
            }
            return null;
        }

		/// <summary>
		/// Gets the string value of the specified control
		/// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		/// <returns></returns>
		public string GetString(object zQueryKey)
		{
            var zItem = GetQueryItem(zQueryKey);
			if(null != zItem)
			{
				switch(zItem.Type)
				{
					case ControlType.BrowseBox:
                    case ControlType.PullDownBox:
					case ControlType.ComboBox:
					case ControlType.TextBox:
						return zItem.QueryControl.Text;
                    case ControlType.ListBox:
                        return (string)((ListBox)zItem.QueryControl).SelectedItem;
                    case ControlType.NumBox:
                        return ((NumericUpDown)zItem.QueryControl).Value.ToString(CultureInfo.CurrentCulture);
				}
				ThrowWrongTypeException();
			}
			return string.Empty;
		}

        /// <summary>
        /// Gets the selected strings of the control
        /// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        /// <returns>Array of selected strings, or NULL if none are selected</returns>
        public string[] GetStrings(object zQueryKey)
        {
            var zItem = GetQueryItem(zQueryKey);
            if (null != zItem)
            {
                switch (zItem.Type)
                {
                    case ControlType.ListBox:
                        var zListBox = (ListBox)zItem.QueryControl;
                        var arrayItems = new string[zListBox.SelectedItems.Count];
                        for (var nIdx = 0; nIdx < arrayItems.Length; nIdx++)
                        {
                            arrayItems[nIdx] = (string)zListBox.SelectedItems[nIdx];
                        }
                        return arrayItems;
                }
                ThrowWrongTypeException();
            }
            return null;
        }

		/// <summary>
		/// 
		/// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		/// <returns></returns>
		public decimal GetDecimal(object zQueryKey)
		{
            var zItem = GetQueryItem(zQueryKey);
			if(null != zItem)
			{
				switch(zItem.Type)
				{
					case ControlType.NumBox:
                    case ControlType.NumBoxSlider:
						return ((NumericUpDown)zItem.QueryControl).Value;
				}
				ThrowWrongTypeException();
			}
			return 0;
		}

        /// <summary>
        /// Returns the DateTime of the specified control.
        /// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
        /// <returns></returns>
        public DateTime GetDateTime(object zQueryKey)
        {
            var zItem = GetQueryItem(zQueryKey);
            if (null != zItem)
            {
                switch (zItem.Type)
                {
                    case ControlType.DateTimePicker:
                        return ((DateTimePicker)zItem.QueryControl).Value;
                }
                ThrowWrongTypeException();
            }
            return DateTime.Now;
        }

		/// <summary>
		/// 
		/// </summary>
        /// <param name="zQueryKey">The query key for requesting the value</param>
		/// <returns></returns>
		private QueryItem GetQueryItem(object zQueryKey)
		{
            QueryItem zQueryItem;
            if (!m_dictionaryItems.TryGetValue(zQueryKey, out zQueryItem))
            {
                ThrowBadQueryException();
            }
            return zQueryItem;
		}

		/// <summary>
		/// Used to throw a general exception when the wrong type is queried
		/// </summary>
		private void ThrowWrongTypeException()
		{
			throw new Exception("QueryDialog: Incorrect type for specified return.");
		}

		/// <summary>
		/// Used to throw a general exception when the query key specified is wrong
		/// </summary>
		private void ThrowBadQueryException()
		{
			throw new Exception("QueryDialog: Invalid Query Key.");
		}
		
		/// <summary>
		/// Class representing an entry/item on the dialog
		/// </summary>
		protected class QueryItem
		{
            private ControlType m_eControlType = ControlType.None;
			private Control m_zControl;
            private readonly List<QueryItem> m_listEnableControls = new List<QueryItem>();
			public object Tag; // always good to have an extra object reference just in case...
	
			public ControlType Type => m_eControlType;

		    public Control QueryControl => m_zControl;

		    /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="eControlType">The ControlType to create</param>
            /// <param name="zControl">The associated control</param>
            /// <param name="nTabIndex"></param>
            public QueryItem(ControlType eControlType, Control zControl, ref int nTabIndex)
            {
                ConfigureQueryItem(eControlType, zControl, null, ref nTabIndex);
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="eControlType">The ControlType to create</param>
            /// <param name="zControl">The associated control</param>
            /// <param name="zTagControl">The Tag control of the query item</param>
            /// <param name="nTabIndex"></param>
            public QueryItem(ControlType eControlType, Control zControl, Control zTagControl, ref int nTabIndex)
            {
                ConfigureQueryItem(eControlType, zControl, zTagControl, ref nTabIndex);
            }

            private void ConfigureQueryItem(ControlType eControlType, Control zControl, Control zTagControl, ref int nTabIndex)
            {
                Tag = zTagControl;
                m_eControlType = eControlType;
                m_zControl = zControl;
                m_zControl.TabIndex = nTabIndex++;
                switch (eControlType)
                {
                    case ControlType.TextBox:
                    case ControlType.ComboBox:
                        zControl.TextChanged += QueryItem_TextChanged;
                        break;
                    case ControlType.CheckBox:
                        ((CheckBox)zControl).CheckedChanged += QueryItem_CheckedChanged;
                        break;
                    case ControlType.BrowseBox:
                        zControl.TextChanged += QueryItem_TextChanged;
                        zTagControl.TabIndex = nTabIndex++;
                        break;
                    case ControlType.NumBoxSlider:
                        zTagControl.TabIndex = nTabIndex++;
                        break;
                }
            }

			/// <summary>
			/// Adds an enable control state for the specified item
			/// </summary>
			/// <param name="zItem"></param>
			public void AddEnableControl(QueryItem zItem)
			{
                if (!m_listEnableControls.Contains(zItem))
				{
                    m_listEnableControls.Add(zItem);
				}
			}

			private void QueryItem_TextChanged(object sender, EventArgs e)
			{
				UpdateEnableStates();
			}

			private void QueryItem_CheckedChanged(object sender, EventArgs e)
			{
				UpdateEnableStates();
			}

			/// <summary>
			/// Updates all of the enable states for the controls (based on the current state of this control)
			/// </summary>
			public void UpdateEnableStates()
			{
			    bool bEnabled;
				switch(m_eControlType)
				{
					case ControlType.TextBox:
					case ControlType.BrowseBox:
					case ControlType.ComboBox:
						bEnabled = 0 < m_zControl.Text.Length;
						break;
					case ControlType.CheckBox:
						bEnabled = ((CheckBox)m_zControl).Checked;
						break;
					default:
						return;
				}
                foreach (QueryItem zItem in m_listEnableControls)
                {
                    zItem.m_zControl.Enabled = bEnabled;
                    switch (zItem.m_eControlType)
                    {
                        case ControlType.BrowseBox:
                        case ControlType.NumBoxSlider:
                            ((Control)zItem.Tag).Enabled = bEnabled;
                            break;
                    }
                }
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Generic button press (used for the browse file/folder box)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void zButton_Click(object sender, EventArgs e)
		{
            var zButton = (Button)sender;
            var zText = (TextBox)zButton.Tag;
            var sFilter = (string)zText.Tag;
			
			if(!string.IsNullOrEmpty(sFilter)) // file browse
			{
			    var ofn = new OpenFileDialog
			    {
				    Filter = sFilter,
				    CheckFileExists = false,
                    FileName = zText.Text
			    };
                if(DialogResult.OK == ofn.ShowDialog())
				{
					zText.Text = ofn.FileName;
				}
			}
			else // folder browse
			{
			    var fbd = new FolderBrowserDialog
			    {
				    ShowNewFolderButton = true,
                    SelectedPath = zText.Text
			    };
				if(DialogResult.OK == fbd.ShowDialog())
				{
					zText.Text = fbd.SelectedPath;
				}
			}
		}

        /// <summary>
        /// Handles generic numeric slider change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericSlider_ValueChanged(object sender, EventArgs e)
        {
            var zTrackBar = (TrackBar)sender;
            var zNumeric = (NumericUpDown)zTrackBar.Tag;
            if (1 > zNumeric.Increment)
            {
                zNumeric.Value = zNumeric.Minimum + ((decimal)zTrackBar.Value * zNumeric.Increment);
            }
            else
            {
                zNumeric.Value = zTrackBar.Value;
            }
        }

        /// <summary>
        /// Handles generic numeric change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void numeric_ValueChanged(object sender, EventArgs e)
        {
            var zNumeric = (NumericUpDown)sender;
            var zTrackBar = (TrackBar)zNumeric.Tag;
            if (1 > zNumeric.Increment)
            {
                zTrackBar.Value = (int)((zNumeric.Value - zNumeric.Minimum) * ((decimal)1 / zNumeric.Increment));
            }
            else
            {
                zTrackBar.Value = (int)zNumeric.Value;
            }
        }

        /// <summary>
        /// Sets the Y position for the current layout control (panel, tab etc.)
        /// </summary>
        /// <param name="nYAmount">Amount to add</param>
        private void AddToYPosition(int nYAmount)
        {
            var nCurrentY = (int)m_zCurrentLayoutControl.Tag;
            nCurrentY += nYAmount;
            m_zCurrentLayoutControl.Tag = nCurrentY;
        }

        /// <summary>
        /// Gets the current y position (based on the current layout control)
        /// </summary>
        /// <returns></returns>
        protected int GetYPosition()
        {
            return (int)m_zCurrentLayoutControl.Tag;
        }

        public void UpdateEnableStates()
        {
            // Update enable states
            foreach (var zItem in m_dictionaryItems.Values)
            {
                zItem.UpdateEnableStates();
            }
        }

		#endregion

	}
}
