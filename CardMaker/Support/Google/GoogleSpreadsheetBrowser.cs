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
using Support.IO;
using Support.UI;

namespace Support.Google
{
    public partial class GoogleSpreadsheetBrowser : Form
    {
        private readonly bool m_bRequireSheetSelect;

        private Dictionary<string, string> m_dictionaryNameID;
        private GoogleSpreadsheet m_zGoogleSpreadsheet;
        public string SelectedSpreadsheet => listViewSpreadsheets.SelectedItems.Count == 0 ? null : ((KeyValuePair<string,string>)listViewSpreadsheets.SelectedItems[0].Tag).Key;
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

            foreach (var entry in m_dictionaryNameID)
            {
                if (-1 == entry.Key.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var zLvi = new ListViewItem(entry.Key)
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
                    var dictionaryNameID = PerformSpreadsheetRetrieve(
                        () => m_zGoogleSpreadsheet.GetSpreadsheetList(),
                        () => listViewSpreadsheets.InvokeAction(() => listViewSpreadsheets.Clear()));

                    if (null == dictionaryNameID)
                    {
                        this.InvokeAction(
                            () => MessageBox.Show(this, "Failed to access Google Spreadsheets", "Access Failed", MessageBoxButtons.OK, MessageBoxIcon.Error));
                        this.InvokeAction(Close);
                        WaitDialog.Instance.CloseWaitDialog();
                        return;
                    }

                    m_dictionaryNameID = dictionaryNameID;
                    var listNewItems = new List<ListViewItem>();
                    foreach (var entry in dictionaryNameID)
                    {
                        var zLvi = new ListViewItem(entry.Key)
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

                    var listSheets = PerformSpreadsheetRetrieve(
                        () => m_zGoogleSpreadsheet.GetSheetNames(((KeyValuePair<string, string>)zLvi.Tag).Value),
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
#warning Is the tag necessary here?
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
