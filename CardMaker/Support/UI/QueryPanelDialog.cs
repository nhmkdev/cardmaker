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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Support.UI
{

    public class QueryPanelDialog : QueryPanel
	{
		private Form m_zForm;
        private int m_nMaxDesiredHeight = -1;
        private string m_sButtonPressed = string.Empty;

        public Form Form => m_zForm;

	    /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sTitle">Title of the dialog</param>
		/// <param name="nWidth">Width of the dialog</param>
		/// <param name="bTabbed">Whether the panel should be tabbed</param>
		public QueryPanelDialog(string sTitle, int nWidth, bool bTabbed) 
            : base(null, bTabbed)
		{
            InitForm(sTitle, nWidth, null, null);
            X_LABEL_SIZE = (int)((float)m_zPanel.Width * 0.25);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sTitle">Title of the dialog</param>
		/// <param name="nWidth">Width of the dialog</param>
		/// <param name="nLabelWidth">Width of labels</param>
        /// <param name="bTabbed">Flag indicating whether this should support tabs</param>
		public QueryPanelDialog(string sTitle, int nWidth, int nLabelWidth, bool bTabbed) 
            : base(null, nLabelWidth, bTabbed)
		{
            InitForm(sTitle, nWidth, null, null);
            if (0 < nLabelWidth && nLabelWidth < m_zPanel.Width)
            {
                X_LABEL_SIZE = nLabelWidth;
            }
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sTitle">Title of the dialog</param>
        /// <param name="nWidth">Width of the dialog</param>
        /// <param name="nLabelWidth">Width of labels</param>
        /// <param name="bTabbed">Flag indicating whether this should support tabs</param>
        /// <param name="arrayButtons">Array of button names to support</param>
        public QueryPanelDialog(string sTitle, int nWidth, int nLabelWidth, bool bTabbed, string[] arrayButtons) 
            : base(null, nLabelWidth, bTabbed)
        {
            InitForm(sTitle, nWidth, arrayButtons, null);
            if (0 < nLabelWidth && nLabelWidth < m_zPanel.Width)
            {
                X_LABEL_SIZE = nLabelWidth;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sTitle">Title of the dialog</param>
        /// <param name="nWidth">Width of the dialog</param>
        /// <param name="nLabelWidth">Width of labels</param>
        /// <param name="bTabbed">Flag indicating whether this should support tabs</param>
        /// <param name="arrayButtons">Array of button names to support</param>
        /// <param name="arrayHandlers">The handlers to associated with the buttons (order match with buttons, null is allowed for an event handler)</param>
        public QueryPanelDialog(string sTitle, int nWidth, int nLabelWidth, bool bTabbed, string[] arrayButtons, EventHandler[] arrayHandlers)
            : base(null, nLabelWidth, bTabbed)
        {
            InitForm(sTitle, nWidth, arrayButtons, arrayHandlers);
            if (0 < nLabelWidth && nLabelWidth < m_zPanel.Width)
            {
                X_LABEL_SIZE = nLabelWidth;
            }
        }

		/// <summary>
		/// Initializes the form associated with this QueryDialog
		/// </summary>
		/// <param name="sTitle">Title of the dialog</param>
		/// <param name="nWidth">Width of the dialog</param>
		/// <param name="arrayButtons">The names of the buttons to put on the dialog</param>
		/// <param name="arrayHandlers">event handlers for the buttons</param>
		private void InitForm(string sTitle, int nWidth, string[] arrayButtons, EventHandler[] arrayHandlers)
		{
		    m_zForm = new Form
		    {
		        Size = new Size(nWidth, 300)
		    };
            Button btnDefault = null; // used to set the proper height of the internal panel

            // setup the buttons
            if (null == arrayButtons)
            {
                var btnCancel = new Button();
                var btnOk = new Button();
                btnOk.Click += btnOk_Click;
                btnCancel.Click += btnCancel_Click;
                btnOk.TabIndex = 65001;
                btnCancel.TabIndex = 65002;
                btnCancel.Text = "Cancel";
                btnOk.Text = "OK";
                btnCancel.Location = new Point((m_zForm.ClientSize.Width - (3 * X_CONTROL_BUFFER)) - btnCancel.Size.Width, (m_zForm.ClientSize.Height - Y_CONTROL_BUFFER) - btnCancel.Size.Height);
                btnOk.Location = new Point((btnCancel.Location.X - X_CONTROL_BUFFER) - btnOk.Size.Width, btnCancel.Location.Y);
                btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                m_zForm.AcceptButton = btnOk;
                m_zForm.CancelButton = btnCancel;
                m_zForm.Controls.Add(btnOk);
                m_zForm.Controls.Add(btnCancel);
                btnDefault = btnCancel;
            }
            else
            {
                Button btnPrevious = null;
                for (int nIdx = arrayButtons.Length - 1; nIdx > -1; nIdx--)
                {
                    btnDefault = new Button();
                    var bHandlerSet = false;
                    if (arrayHandlers?[nIdx] != null)
                    {
                        btnDefault.Click += arrayHandlers[nIdx];
                        bHandlerSet = true;
                    }
                    if (!bHandlerSet)
                    {
                        btnDefault.Click += btnGeneric_Click;
                    }
                    btnDefault.TabIndex = 65000 + nIdx;
                    btnDefault.Text = arrayButtons[nIdx];
                    btnDefault.Location = null == btnPrevious
                        ? new Point((m_zForm.ClientSize.Width - (3 * X_CONTROL_BUFFER)) - btnDefault.Size.Width, (m_zForm.ClientSize.Height - Y_CONTROL_BUFFER) - btnDefault.Size.Height)
                        : new Point((btnPrevious.Location.X - X_CONTROL_BUFFER) - btnDefault.Size.Width, btnPrevious.Location.Y);

                    btnDefault.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    m_zForm.Controls.Add(btnDefault);
                    btnPrevious = btnDefault;
                }
            }

            // setup the form
            m_zForm.FormBorderStyle = FormBorderStyle.FixedSingle;
			m_zForm.MaximizeBox = false;
			m_zForm.MinimizeBox = false;
			m_zForm.Name = "QueryDialog";
			m_zForm.ShowInTaskbar = false;
			m_zForm.SizeGripStyle = SizeGripStyle.Hide;
			m_zForm.StartPosition = FormStartPosition.CenterParent;
			m_zForm.Text = sTitle;
			m_zForm.Load += QueryDialog_Load;

            // setup the panel to contain the controls
            m_zPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
		    if (null != btnDefault)
		    {
		        m_zPanel.Size = new Size(m_zForm.ClientSize.Width,
		            m_zForm.ClientSize.Height - (btnDefault.Size.Height + (2*Y_CONTROL_BUFFER)));
		    }
		    m_zPanel.AutoScroll = true;
            m_zPanel.Resize += m_zPanel_Resize;
            m_zForm.Controls.Add(m_zPanel);
		}

        void m_zPanel_Resize(object sender, EventArgs e)
        {
            SetupScrollState(m_zPanel);
        }

        /// <summary>
        /// Sets the title text of the dialog.
        /// </summary>
        /// <param name="sTitle"></param>
        public void SetTitleText(string sTitle)
        {
            m_zForm.Text = sTitle;
        }

        /// <summary>
        /// Flags the dialog to be shown in the task bar.
        /// </summary>
        /// <param name="bShow"></param>
        public void ShowInTaskBar(bool bShow)
        {
            m_zForm.ShowInTaskbar = bShow;
        }


		/// <summary>
		/// Allows the form to be resized. This should be used after all of the controls have been added to set the minimum size.
		/// </summary>
		public void AllowResize()
		{
			m_zForm.MaximizeBox = true;
			m_zForm.FormBorderStyle = FormBorderStyle.Sizable;
		}

        /// <summary>
        /// Sets the icon for the form
        /// </summary>
        /// <param name="zIcon">The icon to use for the dialog. If null is specified the icon is hidden.</param>
        public void SetIcon(Icon zIcon)
        {
            if (null == zIcon)
            {
                m_zForm.ShowIcon = false;
            }
            else
            {
                m_zForm.Icon = zIcon;
            }
        }

		/// <summary>
		/// Shows the dialog (much like the Form.ShowDialog method)
		/// </summary>
		/// <param name="zParentForm"></param>
		/// <returns></returns>
		public DialogResult ShowDialog(IWin32Window zParentForm)
		{
			return m_zForm.ShowDialog(zParentForm);
		}

        /// <summary>
        /// Returns the string on the button pressed on exit.
        /// </summary>
        /// <returns></returns>
        public string GetButtonPressedString()
        {
            return m_sButtonPressed;
        }

        #region Events

        /// <summary>
        /// Handles generic button presses (for those on the bottom of the dialog)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGeneric_Click(object sender, EventArgs e)
        {
            m_zForm.DialogResult = DialogResult.OK;
            m_sButtonPressed = ((Button)sender).Text;
            m_zForm.Close();
        }

		/// <summary>
		/// Handles the Ok button press.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOk_Click(object sender, EventArgs e)
		{
			m_zForm.DialogResult = DialogResult.OK;
            m_sButtonPressed = m_zForm.DialogResult.ToString();
		}

		/// <summary>
		/// Handles the Cancel button press.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnCancel_Click(object sender, EventArgs e)
		{
			m_zForm.DialogResult = DialogResult.Cancel;
            m_sButtonPressed = m_zForm.DialogResult.ToString();
		}

        /// <summary>
        /// Sets the max desired height for the dialog.
        /// </summary>
        /// <param name="nMaxHeight"></param>
        public void SetMaxHeight(int nMaxHeight)
        {
            m_nMaxDesiredHeight = nMaxHeight;
        }

		/// <summary>
		/// The dialog load event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void QueryDialog_Load(object sender, EventArgs e)
		{
            if (null == m_zTabControl)
            {
                int nHeight = GetYPosition() + m_nButtonHeight + (Y_CONTROL_BUFFER * 2);
                if (m_nMaxDesiredHeight > 0)
                {
                    if (nHeight > m_nMaxDesiredHeight)
                    {
                        nHeight = m_nMaxDesiredHeight;
                    }
                }

                m_zForm.ClientSize = new Size(m_zForm.ClientSize.Width, nHeight);
            }
            else
            {
                var nLargestHeight = -1;
                foreach(var zPage in m_zTabControl.TabPages.OfType<TabPage>())
                {
                    if(nLargestHeight < (int)zPage.Tag)
                    {
                        nLargestHeight = (int)zPage.Tag;
                    }
                }
                if (nLargestHeight > 0)
                {
                    nLargestHeight = Math.Min(m_nMaxDesiredHeight, nLargestHeight);
                    // hard coded extra vertical space
                    m_zForm.ClientSize = new Size(m_zForm.ClientSize.Width, nLargestHeight + 60);
                }
            }

            // add the panel controls after the client size has been set (adding them before displayed an odd issue with control anchor/size)
		    FinalizeControls();

            if (0 < m_zPanel.Controls.Count)
            {
                m_zPanel.SelectNextControl(m_zPanel.Controls[m_zPanel.Controls.Count - 1], true, true, true, true);
            }
		}

		#endregion

	}
}
