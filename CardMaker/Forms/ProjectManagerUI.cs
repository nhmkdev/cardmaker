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
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Forms
{
    public partial class ProjectManagerUI : Form
    {
        public ProjectManagerUI()
        {
            InitializeComponent();
            txtFolder.Text = CardMakerSettings.ProjectManagerRoot;
            UpdateProjects();
        }

        #region form events

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                Description = "Select Project Root"
            };
            if (DialogResult.OK == fbd.ShowDialog(this))
            {
                txtFolder.Text = fbd.SelectedPath;
                CardMakerSettings.ProjectManagerRoot = fbd.SelectedPath;
                UpdateProjects();
            }
        }

        private void listViewProjects_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewProjects);
        }

        private void listViewProjects_DoubleClick(object sender, EventArgs e)
        {
            if (1 == listViewProjects.SelectedItems.Count)
            {
                ProjectManager.Instance.OpenProject((string)listViewProjects.SelectedItems[0].Tag);
                Close();
            }
        }

        #endregion

        /// <summary>
        /// Updates the list of available projects
        /// </summary>
        private void UpdateProjects()
        {
            if (Directory.Exists(txtFolder.Text))
            {
                var zWait = new WaitDialog(0, UpdateThread, "Scanning Directory", new string[] { string.Empty }, 300);
                zWait.ShowDialog(this);
            }
        }

        /// <summary>
        /// Thread used to load the various project files in the project folder
        /// </summary>
        private void UpdateThread()
        {
            WaitDialog.Instance.SetStatusText("Scanning for Projects...");
            var arrayProjects = Directory.GetFiles(txtFolder.Text, "*.cmp", SearchOption.AllDirectories);
            foreach (string sProject in arrayProjects)
            {
                Project zProject = null;
                if (SerializationUtils.DeserializeFromXmlFile(sProject,Encoding.Default, ref zProject))
                {
                    var zItem = new ListViewItem(new[]
                    {
                        sProject.Remove(0, txtFolder.Text.Length),
                        File.GetCreationTime(txtFolder.Text).ToString(CultureInfo.InvariantCulture),
                        File.GetLastWriteTime(txtFolder.Text).ToString(CultureInfo.InvariantCulture),
                        zProject.Layout.Length.ToString(CultureInfo.InvariantCulture)
                    })
                    {
                        Tag = sProject
                    };
                    listViewProjects.InvokeAction(() => listViewProjects.Items.Add(zItem));
                }
            }
            WaitDialog.Instance.ThreadSuccess = true;
            WaitDialog.Instance.CloseWaitDialog();
        }
    }
}
