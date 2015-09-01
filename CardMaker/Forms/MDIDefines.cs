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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CardMaker.Events.Managers;
using Support.UI;

namespace CardMaker.Forms
{
    public partial class MDIDefines : Form
    {
        private Point m_zLocation;
        private static MDIDefines s_zInstance;

        public MDIDefines()
        {
            InitializeComponent();
            LayoutManager.Instance.LayoutLoaded += Instance_LayoutLoaded;
        }

        void Instance_LayoutLoaded(object sender, Events.LayoutEventArgs args)
        {
            listViewDefines.Items.Clear();

            if (null == args.Deck)
            {
                return;
            }

            var listItems = new List<ListViewItem>();
            foreach (var zDefine in args.Deck.Defines)
            {
                listItems.Add(new ListViewItem(new string[] { zDefine.Key, zDefine.Value }));
            }
            listViewDefines.Items.AddRange(listItems.ToArray());
        }

        public static MDIDefines Instance
        {
            get
            {
                if (null == s_zInstance)
                {
                    s_zInstance = new MDIDefines();
                }
                return s_zInstance;
            }
        }

        private void listViewDefines_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItemComparer.SortColumn(listViewDefines, e, false);
        }

        private void listViewDefines_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewDefines);
        }

        private void MDIDefines_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseReason.UserClosing == e.CloseReason)
            {
                m_zLocation = Location;
                e.Cancel = true;
                Hide();
            }
        }

        private void MDIDefines_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible && !m_zLocation.IsEmpty)
            {
                Location = m_zLocation;
            }
        }

        private void copyDefineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 == listViewDefines.SelectedItems.Count)
            {
                Clipboard.SetText(listViewDefines.SelectedItems[0].SubItems[0].Text);
            }
        }

        private void copyValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 == listViewDefines.SelectedItems.Count)
            {
                Clipboard.SetText(listViewDefines.SelectedItems[0].SubItems[1].Text);
            }
        }

        private void copyDefineAsReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 == listViewDefines.SelectedItems.Count)
            {
                Clipboard.SetText(string.Format("@[{0}]", listViewDefines.SelectedItems[0].SubItems[0].Text));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    copyDefineAsReferenceToolStripMenuItem_Click(null, null);
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
