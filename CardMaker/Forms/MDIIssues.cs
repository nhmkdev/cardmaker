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
using System.Globalization;
using System.Windows.Forms;
using CardMaker.Card.Export;
using CardMaker.Events.Args;
using CardMaker.Events.Managers;
using Support.UI;

namespace CardMaker.Forms
{
    public partial class MDIIssues : Form
    {
        private Point m_zLocation;
        private int m_nCurrentLayoutIndex = 0;
        private string m_sCurrentCardIndex = string.Empty;
        private string m_sCurrentElementName = string.Empty;
        private bool m_bTrackIssues;

        public MDIIssues()
        {
            InitializeComponent();
            IssueManager.Instance.IssueAdded += Issue_Added;
            IssueManager.Instance.CardInfoChanged += CardInfo_Changed;
            IssueManager.Instance.ElementChanged += Element_Changed;
            IssueManager.Instance.RefreshRequested += Refresh_Requested;
        }

        #region manager events

        void Refresh_Requested(object sender, IssueRefreshEventArgs args)
        {
            ClearIssues();
            m_bTrackIssues = true;

            var zWait = new WaitDialog(
                2,
                new CompilerCardExporter(0, ProjectManager.Instance.LoadedProject.Layout.Length).ExportThread,
                "Compile",
                new string[] { "Layout", "Card" },
                450);
            zWait.ShowDialog(ParentForm);

            m_bTrackIssues = false;
            Show();
        }

        void Element_Changed(object sender, IssueElementEventArgs args)
        {
            m_sCurrentElementName = args.Name;
        }

        void CardInfo_Changed(object sender, IssueCardInfoEventArgs args)
        {
            m_nCurrentLayoutIndex = args.LayoutIndex;
            m_sCurrentCardIndex = args.CardIndex.ToString(CultureInfo.InvariantCulture);
        }

        void Issue_Added(object sender, IssueMessageEventArgs args)
        {
            AddIssue(args.Message);
        }

        #endregion

        #region form events

        private void listViewIssues_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (1 == listViewIssues.SelectedItems.Count)
            {
                var zItem = listViewIssues.SelectedItems[0];
                var nLayout = (int) zItem.Tag;
                int nCard;

                if (int.TryParse(zItem.SubItems[1].Text, out nCard))
                {
                    // Select the Layout
                    LayoutManager.Instance.FireLayoutSelectRequested(ProjectManager.Instance.LoadedProject.Layout[nLayout]);
                    // Select the Element
                    ElementManager.Instance.FireElementSelectRequestedEvent(LayoutManager.Instance.GetElement(zItem.SubItems[2].Text));
                    // Select the Card Index
                    LayoutManager.Instance.FireDeckIndexChangeRequested(nCard - 1);
                }
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

        #endregion

        private void AddIssue(string sIssue)
        {
            if (!m_bTrackIssues) return;

            if (listViewIssues.InvokeActionIfRequired(() => AddIssue(sIssue)))
            {
                // NOTE: The tag stores the index of the layout
                var zItem = new ListViewItem(new string[] {
                    ProjectManager.Instance.LoadedProject.Layout[m_nCurrentLayoutIndex].Name,
                    m_sCurrentCardIndex,
                    m_sCurrentElementName,
                    sIssue
                });
                zItem.Tag = m_nCurrentLayoutIndex;
                listViewIssues.Items.Add(zItem);
            }
        }

        private void ClearIssues()
        {
            listViewIssues.Items.Clear();
        }
    }
}