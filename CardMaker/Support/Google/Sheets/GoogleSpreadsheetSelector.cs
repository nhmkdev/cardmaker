////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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
using CardMaker.Card.Import;
using Support.IO;
using Support.UI;

namespace Support.Google.Sheets
{
    public partial class GoogleSpreadsheetSelector : Form
    {
        private readonly bool m_bRequireSheetSelect;

        private GoogleSpreadsheet m_zGoogleSpreadsheet;
        public GoogleSheetInfo SelectedSpreadsheet { get; private set;}
        public string SelectedSheet => listViewSheets.SelectedItems.Count == 0 ? null : (string)listViewSheets.SelectedItems[0].Tag;

        public GoogleSpreadsheetSelector(GoogleSpreadsheet zGoogleSpreadsheet, bool bRequireSheetSelect)
        {
            m_zGoogleSpreadsheet = zGoogleSpreadsheet;
            m_bRequireSheetSelect = bRequireSheetSelect;
            InitializeComponent();
            listViewSheets.Visible = m_bRequireSheetSelect;
            lblSheetSelectionNotRequired.Visible = !m_bRequireSheetSelect;
        }

        private void GoogleSpreadsheetBrowser_Load(object sender, EventArgs e)
        {
            ToggleLoadVerifyButton(false);
            ToggleLoadStatus(false);
        }

        private void ToggleLoadVerifyButton(bool bLoading)
        {
            btnLoadVerify.Enabled = !bLoading;
            btnLoadVerify.Text = bLoading ? "Loading" : "Load / Verify";
        }

        private void ToggleLoadStatus(bool bLoaded)
        {
            lblLoadStatus.Text = bLoaded ? "Data Loaded" : "No Data Loaded";
        }

        private void btnLoadVerify_Click(object sender, EventArgs e)
        {
            if (txtSpreadsheetURL.Text.Length > 0 && txtSpreadsheetID.Text.Length > 0)
            {
                MessageBox.Show(this, "Please specify a URL or ID, not both.", "Multiple identifiers specified.", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            ToggleLoadVerifyButton(true);

            var sId = string.IsNullOrWhiteSpace(txtSpreadsheetURL.Text)
                ? txtSpreadsheetID.Text
                : GoogleSpreadsheetReference.ExtractSpreadsheetIDFromURLString(txtSpreadsheetURL.Text);

            new Thread(() =>
            {
                var zSpreadsheet = PerformSpreadsheetRetrieve(
                    () => m_zGoogleSpreadsheet.GetSpreadsheet(sId),
                    () => listViewSheets.InvokeAction(() => listViewSheets.Items.Clear()));

                List<string> listSheets = null;
                if (zSpreadsheet != null)
                {
                    listSheets = m_zGoogleSpreadsheet.GetSheetNames(zSpreadsheet);
                }

                if (null == listSheets)
                {
                    this.InvokeAction(() =>
                    {
                        ToggleLoadVerifyButton(false);
                        ToggleLoadStatus(false);
                    });
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
                listViewSheets.InvokeAction(() =>
                {
                    listViewSheets.Items.Clear();
                    listViewSheets.Items.AddRange(listNewItems.ToArray());
                    listViewSheets.Sort();
                });

                SelectedSpreadsheet = new GoogleSheetInfo()
                {
                    Id = sId,
                    Name = m_zGoogleSpreadsheet.GetSpreadsheetName(zSpreadsheet)
                };


                this.InvokeAction(() =>
                {
                    ToggleLoadVerifyButton(false);
                    ToggleLoadStatus(true);
                });
            }).Start();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (SelectedSpreadsheet == null)
            {
                MessageBox.Show(this, "Please select a spreadsheet.", "Select Spreadsheet", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }
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
            catch (Exception ex)
            {
                Logger.AddLogLine("General exception: " + ex.Message);
            }
            actionOnException?.Invoke();
            return default;
        }

        private void listViewSheets_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewSheets);
        }

        private void MarkDataUnloaded()
        {
            ToggleLoadStatus(false);
            SelectedSpreadsheet = null;
            listViewSheets.Items.Clear();
        }

        private void txtSpreadsheetURL_TextChanged(object sender, EventArgs e)
        {
            MarkDataUnloaded();
        }

        private void txtSpreadsheetID_TextChanged(object sender, EventArgs e)
        {
            MarkDataUnloaded();
        }
    }
}
