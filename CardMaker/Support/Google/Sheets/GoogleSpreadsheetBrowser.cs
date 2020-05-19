////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2019 Tim Stair
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
using CardMaker.Data;
using Support.IO;
using Support.Progress;
using Support.UI;

namespace Support.Google.Sheets
{
    public partial class GoogleSpreadsheetBrowser : Form
    {
        private readonly bool m_bRequireSheetSelect;

        private IProgressReporter m_zProgressReporter;
        private List<GoogleSheetInfo> m_listGoogleSheets;
        private GoogleSpreadsheet m_zGoogleSpreadsheet;
        public GoogleSheetInfo SelectedSpreadsheet => listViewSpreadsheets.SelectedItems.Count == 0 ? null : (GoogleSheetInfo)listViewSpreadsheets.SelectedItems[0].Tag;
        public string SelectedSheet => listViewSheets.SelectedItems.Count == 0 ? null : (string)listViewSheets.SelectedItems[0].Tag;

        public GoogleSpreadsheetBrowser(GoogleSpreadsheet zGoogleSpreadsheet, bool bRequireSheetSelect)
        {
            m_zGoogleSpreadsheet = zGoogleSpreadsheet;
            m_bRequireSheetSelect = bRequireSheetSelect;
            InitializeComponent();
            listViewSheets.Visible = m_bRequireSheetSelect;
            lblSheets.Visible = m_bRequireSheetSelect;
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            var listNewItems = new List<ListViewItem>();

            foreach (var entry in m_listGoogleSheets)
            {
                if (-1 == entry.Name.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var zLvi = new ListViewItem(entry.Name)
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
            m_zProgressReporter = CardMakerInstance.ProgressReporterFactory.CreateReporter(
                "Getting Google Spreadsheets...",
                new string[] {""}, 
                PerformSheetLookup);
            m_zProgressReporter.StartProcessing(this);
        }

        protected void PerformSheetLookup()
        {
            var listGoogleSheets = PerformSpreadsheetRetrieve(
                () => m_zGoogleSpreadsheet.GetSpreadsheetList(),
                () => listViewSpreadsheets.InvokeAction(() => listViewSpreadsheets.Clear()));

            if (null == listGoogleSheets)
            {
                this.InvokeAction(
                    () => MessageBox.Show(this, "Failed to access Google Spreadsheets", "Access Failed", MessageBoxButtons.OK, MessageBoxIcon.Error));
                this.InvokeAction(Close);
                m_zProgressReporter.Shutdown();
                return;
            }

            m_listGoogleSheets = listGoogleSheets;
            var listNewItems = new List<ListViewItem>();
            foreach (var entry in listGoogleSheets)
            {
                var zLvi = new ListViewItem(entry.Name)
                {
                    Tag = entry
                };
                listNewItems.Add(zLvi);
            }
            listViewSpreadsheets.InvokeAction(() =>
            {
                listViewSpreadsheets.Items.AddRange(listNewItems.ToArray());
            });

            m_zProgressReporter.Shutdown();
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

                    var listSheets = PerformSpreadsheetRetrieve(
                        () => m_zGoogleSpreadsheet.GetSheetNames(((GoogleSheetInfo)zLvi.Tag).Id),
                        () => listViewSheets.InvokeAction(() => listViewSheets.Clear()));

                    if (null == listSheets)
                    {
                        return;
                    }

                    var listNewItems = new List<ListViewItem>();
                    foreach (var sSheetName in listSheets)
                    {
                        var zNewLvi = new ListViewItem(sSheetName)
                        {
                            Tag = sSheetName
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

        private T PerformSpreadsheetRetrieve<T>(Func<T> zRetriever, Action actionOnException)
        {
            try
            {
                return zRetriever();
            }
#warning What is the credentials exception for google now?
            catch (Exception ex)
            {
                Logger.AddLogLine("General exception: " + ex.Message);
            }
            actionOnException?.Invoke();
            return default(T);
        }

        private void listViewSheets_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewSheets);
        }
    }
}
