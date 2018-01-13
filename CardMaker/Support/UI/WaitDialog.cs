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
using System.Threading;
using System.Windows.Forms;

namespace Support.UI
{
    class WaitDialog : Form
    {
        private static WaitDialog m_zWaitDialog;
        private Thread m_zThread;
        private readonly ThreadStart m_zThreadStart;
        private readonly ParameterizedThreadStart m_zParamThreadStart ;
        private readonly object m_zParamObject;
        private ProgressBar[] m_arrayProgressBars;
        private string[] m_arrayDescriptions;

        private Button btnCancel;
        private Label lblStatus;

		/// <summary>
		/// Creates a new WaitDialog with the specified ThreadStart.
		/// </summary>
		/// <param name="nProgressBarCount">The number of progress bars to include.</param>
		/// <param name="zThreadStart">The ThreadStart to initialize</param>
		/// <param name="sTitle">The title of the wait dialog</param>
		/// <param name="arrayDescriptions">Optional array of strings for each progress bar. (null is allowed)</param>
		/// <param name="nWidth">Desired width of the WaitDialog, must be >100</param>
		public WaitDialog(int nProgressBarCount, ThreadStart zThreadStart, string sTitle, string[] arrayDescriptions, int nWidth)
		{
            m_zThreadStart = zThreadStart;
            Initialize(nProgressBarCount, sTitle, arrayDescriptions, nWidth);
		}

        /// <summary>
        /// Creates a new WaitDialog with the specified ThreadStart.
        /// </summary>
        /// <param name="nProgressBarCount">The number of progress bars to include.</param>
        /// <param name="zThreadStart">The ThreadStart to initialize</param>
        /// <param name="zParamObject">The object to pass to the thread method</param>
        /// <param name="sTitle">The title of the wait dialog</param>
        /// <param name="arrayDescriptions">Optional array of strings for each progress bar. (null is allowed)</param>
        /// <param name="nWidth">Desired width of the WaitDialog, must be >100</param>
        public WaitDialog(int nProgressBarCount, ParameterizedThreadStart zThreadStart, object zParamObject, string sTitle, string[] arrayDescriptions, int nWidth)
        {
            m_zParamThreadStart = zThreadStart;
            m_zParamObject = zParamObject;
            Initialize(nProgressBarCount, sTitle, arrayDescriptions, nWidth);
        }

        /// <summary>
        /// Flag indicating the success/failure of the associated thread.
        /// </summary>
        public bool ThreadSuccess { get; set; }

        /// <summary>
        /// Flag indicating the cancel state of the WaitDialog
        /// </summary>
        public bool Canceled { get; private set; }

        /// <summary>
        /// Sets up the basic controls
        /// </summary>
        /// <param name="nProgressBarCount"></param>
        /// <param name="sTitle"></param>
        /// <param name="arrayDescriptions"></param>
        /// <param name="nWidth"></param>
        private void Initialize(int nProgressBarCount, string sTitle, string[] arrayDescriptions, int nWidth)
        {
            Text = sTitle;
            if (nProgressBarCount == arrayDescriptions?.Length)
            {
                m_arrayDescriptions = arrayDescriptions;
            }
            m_zWaitDialog = this;

            // 
            // btnCancel
            // 
            btnCancel = new Button
            {
                DialogResult = DialogResult.Cancel,
                Location = new Point(88, 56),
                Name = "btnCancel",
                Size = new Size(75, 23),
                TabIndex = 1,
                Text = "Cancel"
            };
            btnCancel.Click += btnCancel_Click;
            // 
            // lblStatus
            // 
            lblStatus = new Label
            {
                Location = new Point(8, 8),
                Name = "lblStatus",
                Size = new Size(232, 40),
                TabIndex = 3,
                Text = "Loading..."
            };
            // 
            // WaitDialog
            // 
            CancelButton = btnCancel;
            ClientSize = new Size(756, 430);
            ControlBox = false;
            Controls.Add(lblStatus);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WaitDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            if (100 < nWidth)
            {
                Width = nWidth;
            }

            AddProgressBars(nProgressBarCount);
            Load += WaitDialog_Load;
        }

        /// <summary>
        /// Adds the specified number of progress bars to the WaitDialog. Also configures the other Controls in the window.
        /// </summary>
        /// <param name="nProgressBarCount">The number of progress bars to add to the WaitDialog.</param>
        private void AddProgressBars(int nProgressBarCount)
        {
            const int HEIGHT_CHANGE = 46;
            const int LABEL_Y_ADJUST = 8;
            const int INDENT = 8;
            const int PROGRESS_HEIGHT = 24;
            const int PROGRESS_WIDTH_ADJUST = 22;
            const int LABEL_WIDTH_ADJUST = 16;
            const int LABEL_SINGLELINE_HEIGHT = 12;
            const int LOWER_WINDOW_BUFFER = 32;

            if (0 != nProgressBarCount)
            {
                m_arrayProgressBars = new ProgressBar[nProgressBarCount];
                for (int nIdx = 0; nIdx < nProgressBarCount; nIdx++)
                {
                    m_arrayProgressBars[nIdx] = new ProgressBar
                    {
                        Maximum = 100,
                        Minimum = 0,
                        Location = new Point(INDENT, (HEIGHT_CHANGE*nIdx) + PROGRESS_HEIGHT),
                        Size = new Size(Width - PROGRESS_WIDTH_ADJUST, PROGRESS_HEIGHT)
                    };
                    Controls.Add(m_arrayProgressBars[nIdx]);
                    
                    if (null == m_arrayDescriptions) continue;

                    var zLabel = new Label
                    {
                        Text = m_arrayDescriptions[nIdx],
                        Location = new Point(INDENT, (HEIGHT_CHANGE*nIdx) + LABEL_Y_ADJUST),
                        Size = new Size(Width - LABEL_WIDTH_ADJUST,LABEL_SINGLELINE_HEIGHT)
                    };
                    Controls.Add(zLabel);
                }
            }
            lblStatus.Location = new Point(INDENT, (HEIGHT_CHANGE * nProgressBarCount) + LABEL_Y_ADJUST);
            lblStatus.Size = new Size(Width - LABEL_WIDTH_ADJUST, LABEL_SINGLELINE_HEIGHT * 2); // allow for 2 lines of text
            btnCancel.Location = new Point((Width / 2) - (btnCancel.Width / 2), lblStatus.Location.Y + lblStatus.Size.Height + LABEL_Y_ADJUST);
            Size = new Size(Size.Width, btnCancel.Location.Y + btnCancel.Size.Height + LOWER_WINDOW_BUFFER);
        }

        /// <summary>
        /// Controls the visibility of the cancel button
        /// </summary>
        public bool CancelButtonVisibile
        {
            get { return btnCancel.Visible; }
            set { btnCancel.Visible = value; }
        }

        /// <summary>
        /// Sets the status text
        /// </summary>
        /// <param name="sText"></param>
        public void SetStatusText(string sText)
        {
            lblStatus.InvokeAction(() => lblStatus.Text = sText);
        }

        /// <summary>
        /// Maintains the "semi-singleton" concept of the wait dialog. The WaitDialog should be initialized by
        /// a thread then can be accessed by using this Method in another thread.
        /// </summary>
        /// <returns></returns>
        public static WaitDialog Instance
        {
            get
            {
                m_zWaitDialog = m_zWaitDialog ?? new WaitDialog(1, null, "Untitled", null, 0);
                return m_zWaitDialog;
            }
        }


        /// <summary>
        /// Resets the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to reset (0 based)</param>
        /// <param name="nMin">The minimum value to set on the progress bar</param>
        /// <param name="nMax">The maximum value to set on the progress bar</param>
        /// <param name="nStartVal">The starting value to set on the progress bar</param>
        public void ProgressReset(int nProgressBar, int nMin, int nMax, int nStartVal)
        {
            if (m_arrayProgressBars.Length <= nProgressBar) { return; }
            if (m_arrayProgressBars[nProgressBar].InvokeActionIfRequired(() => ProgressReset(nProgressBar, nMin, nMax, nStartVal)))
            {
                if (m_arrayProgressBars.Length <= nProgressBar) { return; }
                m_arrayProgressBars[nProgressBar].Minimum = nMin;
                m_arrayProgressBars[nProgressBar].Maximum = nMax;
                m_arrayProgressBars[nProgressBar].Value = nStartVal;
                m_arrayProgressBars[nProgressBar].Step = 1;
            }
        }

        /// <summary>
        /// Sets the value on the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to set (0 based)</param>
        /// <param name="nValue">The value to set</param>
        public void ProgressSet(int nProgressBar, int nValue)
        {
            if (m_arrayProgressBars.Length <= nProgressBar) { return; }
            if (m_arrayProgressBars[nProgressBar].InvokeActionIfRequired(() => ProgressSet(nProgressBar, nValue)))
            {
                if (m_arrayProgressBars.Length <= nProgressBar) { return; }
                if (m_arrayProgressBars[nProgressBar].Minimum <= nValue && m_arrayProgressBars[nProgressBar].Maximum >= nValue)
                {
                    m_arrayProgressBars[nProgressBar].Value = nValue;
                }
            }
        }

        /// <summary>
        /// Steps the value on the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to step (0 based)</param>
        public void ProgressStep(int nProgressBar)
        {
            if (m_arrayProgressBars.Length <= nProgressBar) { return; }
            if (m_arrayProgressBars[nProgressBar].InvokeActionIfRequired(() => ProgressStep(nProgressBar)))
            {
                if (m_arrayProgressBars.Length <= nProgressBar) { return; }
                if (m_arrayProgressBars[nProgressBar].Value < m_arrayProgressBars[nProgressBar].Maximum)
                {
                    m_arrayProgressBars[nProgressBar].Value++;
                }
            }
        }

        /// <summary>
        /// Gets the value of the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to get (0 based)</param>
        /// <returns></returns>
        public int ProgressGet(int nProgressBar)
        {
            if (m_arrayProgressBars.Length <= nProgressBar) { return -1; }
            return m_arrayProgressBars[nProgressBar].Value;
        }

        /// <summary>
        /// Used in place of the Close method to avoid threading issues
        /// </summary>
        public void CloseWaitDialog()
        {
            this.InvokeAction(Close);
            m_zWaitDialog = null;
        }

        /// <summary>
        /// When the dialog is shown the associated thread is started.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitDialog_Load(object sender, EventArgs e)
        {
            // start the associated thread when the dialog loads
            if (null != m_zThreadStart)
            {
                m_zThread = new Thread(m_zThreadStart);
                m_zThread.Start();
            }
            else if (null != m_zParamThreadStart)
            {
                m_zThread = new Thread(m_zParamThreadStart);
                m_zThread.Start(m_zParamObject);
            }
        }

        /// <summary>
        /// Cancels the associated thread. (returns from Show/ShowDialog)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Canceled = true;
            if (null != m_zThread)
            {
                try
                {
                    m_zThread.Abort();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
