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
using System.Text;
using System.Windows.Forms;
using Support.IO;
using Support.UI;

namespace CardMaker.Forms
{
    public partial class MDILogger : Form, LoggerI
    {
        public MDILogger()
        {
            InitializeComponent();
            Logger.InitLogger(this, false);
        }

        #region overrides

        protected override CreateParams CreateParams
        {
            get
            {
                const int CP_NOCLOSE_BUTTON = 0x200;
                CreateParams zParams = base.CreateParams;
                zParams.ClassStyle = zParams.ClassStyle | CP_NOCLOSE_BUTTON;
                return zParams;
            }
        }

        #endregion

        #region form events

        private void copyLineToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (-1 != listBoxLog.SelectedIndex)
            {
                Clipboard.SetText(listBoxLog.SelectedItem.ToString());
            }
        }

        private void copyAllTextToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zBuilder = new StringBuilder();
            foreach (string sItem in listBoxLog.Items)
            {
                zBuilder.Append(sItem + Environment.NewLine);
            }
            Clipboard.SetText(zBuilder.ToString());
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBoxLog.Items.Clear();
        }

        #endregion

        #region LoggerI

        public void AddLogLines(string[] arrayLines)
        {
            if (listBoxLog.InvokeActionIfRequired(() => AddLogLines(arrayLines)))
            {
                listBoxLog.BeginUpdate();
                foreach (string sLine in arrayLines)
                {
                    listBoxLog.SelectedIndex = listBoxLog.Items.Add(DateTime.Now.ToString("HH:mm:ss.ff") + "::" + sLine);
                }
                listBoxLog.SelectedIndex = -1;
                listBoxLog.EndUpdate();
            }
        }

        public void ClearLog()
        {
            listBoxLog.InvokeAction(() => listBoxLog.Items.Clear());
        }

        #endregion
    }
}