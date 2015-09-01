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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CardMaker.Events.Managers;
using Support.UI;

namespace CardMaker.Forms
{
    public partial class MDIIssues : Form
    {
        private static MDIIssues s_zInstance;

        private Point m_zLocation;
        private string m_sCurrentLayoutIndex = string.Empty;
        private string m_sCurrentCardIndex = string.Empty;
        private string m_sCurrentElementName = string.Empty;
        private bool m_bTrackIssues;

        private MDIIssues()
        {
            InitializeComponent();
            IssueManager.Instance.IssueAdded += Instance_IssueAdded;
            IssueManager.Instance.CardInfoChanged += Instance_CardInfoChanged;
            IssueManager.Instance.ElementChanged += Instance_ElementChanged;
        }

        void Instance_ElementChanged(object sender, Events.IssueElementEventArgs args)
        {
            m_sCurrentElementName = args.Name;
        }

        void Instance_CardInfoChanged(object sender, Events.IssueCardInfoEventArgs args)
        {
            m_sCurrentLayoutIndex = args.LayoutIndex.ToString(CultureInfo.InvariantCulture);
            m_sCurrentCardIndex = args.CardIndex.ToString(CultureInfo.InvariantCulture);
        }

        void Instance_IssueAdded(object sender, Events.IssueMessageEventArgs args)
        {
            AddIssue(args.Message);
        }

        public static MDIIssues Instance
        {
            get 
            {
                if (null == s_zInstance)
                {
                    s_zInstance = new MDIIssues();
                }
                return s_zInstance; 
            }
        }

        private void AddIssue(string sIssue)
        {
            if (!m_bTrackIssues) return;

            if (listViewIssues.InvokeActionIfRequired(() => AddIssue(sIssue)))
            {
                var zItem = new ListViewItem(new string[] {
                    m_sCurrentLayoutIndex,
                    m_sCurrentCardIndex,
                    m_sCurrentElementName,
                    sIssue
                });
                listViewIssues.Items.Add(zItem);
            }
        }


        public bool TrackIssues
        {
            set { m_bTrackIssues = value; }
        }

        public void ClearIssues()
        {
            listViewIssues.Items.Clear();
        }

        private void listViewIssues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(1 == listViewIssues.SelectedItems.Count)
            {
                ListViewItem zItem = listViewIssues.SelectedItems[0];
                int nLayout = int.Parse(zItem.SubItems[0].Text);
                int nCard = int.Parse(zItem.SubItems[1].Text);
#warning needs a complex event to pull this off
                CardMakerMDI.Instance.SelectLayoutCardElement(nLayout, nCard, zItem.SubItems[2].Text);
            }
        }

        private void listViewIssues_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewIssues);
        }

        private void MDIIssues_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseReason.UserClosing == e.CloseReason)
            {
                m_zLocation = Location;
                e.Cancel = true;
                Hide();
            }
        }

        private void MDIIssues_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible && !m_zLocation.IsEmpty)
            {
                Location = m_zLocation;
            }
        }
    }
}