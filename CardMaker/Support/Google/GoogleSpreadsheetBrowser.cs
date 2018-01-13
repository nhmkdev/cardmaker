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
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Support.IO;

namespace Support.UI
{
    public partial class GoogleSpreadsheetBrowser : Form
    {
        private readonly bool m_bRequireSheetSelect;
        private AtomEntryCollection m_zAllEntries;
        private readonly SpreadsheetsService m_zSpreadsheetsService;

        public AtomEntry SelectedSpreadsheet => listViewSpreadsheets.SelectedItems.Count == 0 ? null : (AtomEntry)listViewSpreadsheets.SelectedItems[0].Tag;

        public AtomEntry SelectedSheet => listViewSheets.SelectedItems.Count == 0 ? null : (AtomEntry)listViewSheets.SelectedItems[0].Tag;

        public GoogleSpreadsheetBrowser(string sAppName, string sClientId, string sGoogleAccessToken, bool bRequireSheetSelect)
        {
            m_bRequireSheetSelect = bRequireSheetSelect;
            InitializeComponent();
            listViewSheets.Visible = m_bRequireSheetSelect;
            lblSheets.Visible = m_bRequireSheetSelect;
            m_zSpreadsheetsService = GoogleSpreadsheet.GetSpreadsheetsService(sAppName, sClientId, sGoogleAccessToken);
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            var listNewItems = new List<ListViewItem>();
            foreach (var entry in m_zAllEntries)
            {
                if (-1 == entry.Title.Text.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var zLvi = new ListViewItem(entry.Title.Text)
                {
                    Tag = entry
                };
                listNewItems.Add(zLvi);
            }
            listViewSpreadsheets.Items.Clear();
            listViewSpreadsheets.Items.AddRange(listNewItems.ToArray());
            listViewSheets.Items.Clear();
        }

        private void GoogleSpreadsheetBrowser_Load(object sender, EventArgs e)
        {
            var zWait = new WaitDialog(1,
                () =>
                {
                    AtomEntryCollection zSheetAtomCollection = getAtomEntryCollection(
                        () => GoogleSpreadsheet.GetSpreadsheetList(m_zSpreadsheetsService),
                        () => listViewSpreadsheets.InvokeAction(() => listViewSpreadsheets.Clear()));

                    if (null == zSheetAtomCollection)
                    {
                        this.InvokeAction(
                            () => MessageBox.Show(this, "Failed to access Google Spreadsheets", "Access Failed", MessageBoxButtons.OK, MessageBoxIcon.Error));
                        this.InvokeAction(Close);
                        WaitDialog.Instance.CloseWaitDialog();
                        return;
                    }

                    m_zAllEntries = zSheetAtomCollection;
                    var listNewItems = new List<ListViewItem>();
                    foreach (var entry in zSheetAtomCollection)
                    {
                        var zLvi = new ListViewItem(entry.Title.Text)
                        {
                            Tag = entry
                        };
                        listNewItems.Add(zLvi);
                    }
                    listViewSpreadsheets.InvokeAction(() =>
                    {
                        listViewSpreadsheets.Items.AddRange(listNewItems.ToArray());
                    });
                    WaitDialog.Instance.CloseWaitDialog();
                },
                "Getting Google Spreadsheets...",
                null,
                400);
            zWait.ShowDialog(this);
        }

        private void listViewSpreadsheets_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewSpreadsheets);
        }

        private void listViewSpreadsheets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_bRequireSheetSelect)
            {
                return;
            }

            if (listViewSpreadsheets.SelectedItems.Count == 1)
            {
                new Thread(() =>
                {
                    Func<ListViewItem> getCurrentSelectedtem = () =>
                        listViewSpreadsheets.SelectedItems.Count == 1 ? listViewSpreadsheets.SelectedItems[0] : null;
                    

                    var zLvi = listViewSpreadsheets.InvokeFunc(getCurrentSelectedtem);

                    AtomEntryCollection zSheetAtomCollection = getAtomEntryCollection(
                        () => GoogleSpreadsheet.GetSheetNames(m_zSpreadsheetsService, ((AtomEntry) zLvi.Tag)),
                        () => listViewSheets.InvokeAction(() => listViewSheets.Clear()));

                    if (null == zSheetAtomCollection)
                    {
                        return;
                    }

                    var listNewItems = new List<ListViewItem>();
                    foreach (var entry in zSheetAtomCollection)
                    {
                        var zNewLvi = new ListViewItem(entry.Title.Text)
                        {
                            Tag = entry
                        };
                        listNewItems.Add(zNewLvi);
                    }
                    if (listViewSpreadsheets.InvokeFunc(getCurrentSelectedtem) == zLvi)
                    {
                        listViewSheets.InvokeAction(() =>
                        {
                            listViewSheets.Items.Clear();
                            listViewSheets.Items.AddRange(listNewItems.ToArray());
                            listViewSpreadsheets.Sort();
                        } );
                    }
                }).Start();
            }
            else
            {
                listViewSheets.Items.Clear();
            }
        }

        private void listViewSpreadsheets_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItemComparer.SortColumn(listViewSpreadsheets, e, false);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (m_bRequireSheetSelect && SelectedSheet == null)
            {
                MessageBox.Show(this, "Please select a sheet.", "Select Sheet", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private AtomEntryCollection getAtomEntryCollection(Func<AtomEntryCollection> zGetter, Action actionOnException)
        {
            try
            {
                return zGetter();
            }
            catch (InvalidCredentialsException ex)
            {
                Logger.AddLogLine("Credentials exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("General exception: " + ex.Message);
            }
            actionOnException?.Invoke();
            return null;
        }

        private void listViewSheets_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewSheets);
        }
    }
}
