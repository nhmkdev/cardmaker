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

using CardMaker.Card.Import;
using CardMaker.XML;
using Support.IO;
using Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CardMaker.Forms
{
    public partial class MDIProject : Form
    {
        private static MDIProject s_zInstance;
        private TreeNode m_tnCurrentLayout;

        private MDIProject()
        {
            InitializeComponent();
        }

        public static MDIProject Instance
        {
            get
            {
                s_zInstance = s_zInstance ?? new MDIProject();
                return s_zInstance;
            }
        }

        public TreeView ProjectTreeView
        {
            get
            {
                return treeView;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CP_NOCLOSE_BUTTON = 0x200;
                CreateParams mdiCp = base.CreateParams;
                mdiCp.ClassStyle = mdiCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return mdiCp;
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (null != treeView.SelectedNode && null != treeView.SelectedNode.Tag)
            {
                Type type = treeView.SelectedNode.Tag.GetType();
                if(typeof(Project) == type)
                {
                    treeView.ContextMenuStrip = contextMenuStripProject;
                }
                else if(typeof(ProjectLayout) == type)
                {
                    treeView.ContextMenuStrip = contextMenuStripLayout;
                    if (m_tnCurrentLayout != treeView.SelectedNode)
                    {
                        CardMakerMDI.Instance.UpdateProjectLayoutTreeNode();
                    }
                }
                else if(typeof(ProjectLayoutReference) == type)
                {
                    treeView.ContextMenuStrip = contextMenuStripReference;
                }
            }
        }

        private void contextMenuStripTreeView_Opening(object sender, CancelEventArgs e)
        {
            var zNode = treeView.SelectedNode;

            if (null != zNode)
            {
                addCardLayoutFromTemplateToolStripMenuItem.Enabled = 0 < CardMakerMDI.Instance.LayoutTemplates.Count;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void treeView_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Right == e.Button)
            {
                TreeNode tNode = treeView.GetNodeAt(treeView.PointToClient(Cursor.Position));
                if (null != tNode)
                {
                    treeView.SelectedNode = tNode;
                }
            }
        }

        private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if ((null == treeView.SelectedNode.Tag) ||
                (typeof(ProjectLayout) != treeView.SelectedNode.Tag.GetType()))
            {
                e.CancelEdit = true;
            }
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // e.label is null on user cancel
            if (typeof(ProjectLayout) == treeView.SelectedNode.Tag.GetType() && null != e.Label)
            {
                var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;
                zLayout.Name = e.Label;
                CardMakerMDI.Instance.MarkDirty();
            }
        }

        private void addLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string NAME = "name";
            const string WIDTH = "width";
            const string HEIGHT = "height";
            const string DPI = "dpi";

            var zQuery = new QueryPanelDialog("New Layout", 450, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            zQuery.AddTextBox("Name", "New Layout", false, NAME);
            zQuery.AddNumericBox("Width", 300, 1, int.MaxValue, WIDTH);
            zQuery.AddNumericBox("Height", 300, 1, int.MaxValue, HEIGHT);
            zQuery.AddNumericBox("DPI", 300, 100, 9600, DPI);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var zLayout = new ProjectLayout(zQuery.GetString(NAME))
                {
                    width = (int)zQuery.GetDecimal(WIDTH),
                    height = (int)zQuery.GetDecimal(HEIGHT),
                    dpi = (int)zQuery.GetDecimal(DPI)
                };
                Project.AddProjectLayout(treeView.SelectedNode, zLayout, CardMakerMDI.Instance.LoadedProject);
                CardMakerMDI.Instance.MarkDirty();
            }
        }

        private void addCardLayoutFromTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string TEMPLATE = "template";
            const string NAME = "name";
            const string COUNT = "count";
            var listItems = new List<string>();
            CardMakerMDI.Instance.LayoutTemplates.ForEach(x => listItems.Add(x.ToString()));

            var zQuery = new QueryPanelDialog("Select Layout Template", 450, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            zQuery.AddTextBox("New Layout Name", "New Layout", false, NAME);
            zQuery.AddNumericBox("Number to create", 1, 1, 256, COUNT);
            zQuery.AddListBox("Template", listItems.ToArray(), null, false, 120, TEMPLATE);
            zQuery.AllowResize();
            while(DialogResult.OK == zQuery.ShowDialog(this))
            {
                int nSelectedIndex = zQuery.GetIndex(TEMPLATE);
                if(-1 == nSelectedIndex)
                {
                    MessageBox.Show("Please select a layout template");
                    continue;
                }

                for (int nCount = 0; nCount < zQuery.GetDecimal(COUNT); nCount++)
                {
                    var zLayout = new ProjectLayout(zQuery.GetString(NAME));
                    zLayout.DeepCopy(CardMakerMDI.Instance.LayoutTemplates[nSelectedIndex].Layout);
                    Project.AddProjectLayout(treeView.SelectedNode, zLayout, CardMakerMDI.Instance.LoadedProject);
                }
                break;
            }
        }

        private void defineAsTemplateLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string NAME = "name";
            var zQuery = new QueryPanelDialog("Template Name", 450, 80, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            zQuery.AddTextBox("Name", "New Template", false, NAME);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var zLayout = new ProjectLayout();
                zLayout.DeepCopy((ProjectLayout)treeView.SelectedNode.Tag);
                var zTemplate = new LayoutTemplate(zQuery.GetString(NAME), zLayout);
                CardMakerMDI.Instance.LayoutTemplates.Add(zTemplate);
                CardMakerMDI.Instance.SaveTemplates();
            }
        }

        private void removeLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 < treeView.SelectedNode.Parent.Nodes.Count)
            {
                if (DialogResult.Yes == MessageBox.Show(this, "Are you sure you want to remove this Layout?", "Remove Layout", MessageBoxButtons.YesNo))
                {
                    CardMakerMDI.Instance.LoadedProject.RemoveProjectLayout(treeView.SelectedNode);
                    CardMakerMDI.Instance.MarkDirty();
                }
            }
            else
            {
                Logger.AddLogLine("Cannot remove the last Layout!");
            }
        }

        private void duplicateLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;
            var zLayoutCopy = new ProjectLayout(zLayout.Name + " copy");
            zLayoutCopy.DeepCopy(zLayout);
            var tnLayout = Project.AddProjectLayout(treeView.SelectedNode.Parent, zLayoutCopy, CardMakerMDI.Instance.LoadedProject);
            tnLayout.ExpandAll();
            CardMakerMDI.Instance.MarkDirty();
        }

        private void addReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sFile = CardMakerMDI.FileOpenHandler("CSV files (*.csv)|*.csv|All files (*.*)|*.*", null, true);
            if (null != sFile)
            {
                CardMakerMDI zCardMakerMDI = CardMakerMDI.Instance;
                var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;
                var bNewDefault = 0 == treeView.SelectedNode.Nodes.Count;
                var tnReference = Project.AddReferenceNode(treeView.SelectedNode, sFile, bNewDefault, zLayout);
                if (null == tnReference)
                {
                    MessageBox.Show(this, "The specified reference is already associated with this layout.", "Reference Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (bNewDefault)
                {
                    tnReference.Parent.Expand();
                    // reinit canvas
                    zCardMakerMDI.UpdateProjectLayoutTreeNode();
                    zCardMakerMDI.DrawCurrentCardIndex();
                }
                zCardMakerMDI.MarkDirty();
            }
        }

        private void addGoogleSpreadsheetReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CardMakerMDI.GoogleAccessToken))
            {
                if(DialogResult.Cancel == MessageBox.Show(this,
                    "You do not appear to have any Google credentials configured. Press OK to configure.",
                    "Google Credentials Missing",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information))
                {
                    return;
                }
                CardMakerMDI.Instance.UpdateGoogleAuth();
                return;
            }

            var zDialog = new GoogleSpreadsheetBrowser(GoogleReferenceReader.APP_NAME, GoogleReferenceReader.CLIENT_ID,
                CardMakerMDI.GoogleAccessToken, true);
            if (DialogResult.OK == zDialog.ShowDialog(this))
            {
                var zCardMakerMDI = CardMakerMDI.Instance;
                var bNewDefault = 0 == treeView.SelectedNode.Nodes.Count;
                var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;
                var tnReference = Project.AddReferenceNode(
                    treeView.SelectedNode,
                    CardMakerMDI.GOOGLE_REFERENCE + CardMakerMDI.GOOGLE_REFERENCE_SPLIT_CHAR +
                    zDialog.SelectedSpreadsheet.Title.Text + CardMakerMDI.GOOGLE_REFERENCE_SPLIT_CHAR +
                    zDialog.SelectedSheet.Title.Text,
                    bNewDefault,
                    zLayout);
                if (null == tnReference)
                {
                    MessageBox.Show(this, "The specified reference is already associated with this layout.", "Reference Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (bNewDefault)
                {
                    tnReference.Parent.Expand();
                    // reinit canvas
                    zCardMakerMDI.UpdateProjectLayoutTreeNode();
                    zCardMakerMDI.DrawCurrentCardIndex();
                }
                zCardMakerMDI.MarkDirty();                
            }
        }

        private void removeReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zCardMakerMDI = CardMakerMDI.Instance;
            var tnNode = treeView.SelectedNode;
            var zReference = (ProjectLayoutReference)treeView.SelectedNode.Tag;
            var zLayout = (ProjectLayout)treeView.SelectedNode.Parent.Tag;

            var listReferences = new List<ProjectLayoutReference>(zLayout.Reference);
            listReferences.Remove(zReference);
            zLayout.Reference = listReferences.ToArray();

            tnNode.Parent.Nodes.Remove(tnNode);

            // default to the last item
            if (1 == listReferences.Count)
            {
                treeView.SelectedNode.Parent.Nodes[0].BackColor = Project.DEFAULT_REFERENCE_COLOR;
                listReferences[0].Default = true;
            }

            // reinit canvas
            if (zCardMakerMDI.DrawCardCanvas.ActiveDeck.CardLayout != null)
            {
                zCardMakerMDI.DrawCardCanvas.SetCardLayout(null);
                zCardMakerMDI.UpdateProjectLayoutTreeNode();
                zCardMakerMDI.DrawCurrentCardIndex();
            }
            zCardMakerMDI.MarkDirty();
        }

        private void setAsDefaultReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zCardMakerMDI = CardMakerMDI.Instance;
            var tnNode = treeView.SelectedNode;
            var zReference = (ProjectLayoutReference)tnNode.Tag;

            foreach (TreeNode tnReferenceNode in tnNode.Parent.Nodes)
            {
                tnReferenceNode.BackColor = Color.White;
                var zSubReference = (ProjectLayoutReference)tnReferenceNode.Tag;
                zSubReference.Default = false;
            }
            tnNode.BackColor = Project.DEFAULT_REFERENCE_COLOR;
            zReference.Default = true;

            // reinit canvas
            if (zCardMakerMDI.DrawCardCanvas.ActiveDeck.CardLayout != null)
            {
                zCardMakerMDI.DrawCardCanvas.SetCardLayout(null);
                zCardMakerMDI.UpdateProjectLayoutTreeNode();
                zCardMakerMDI.DrawCurrentCardIndex();
            }
            zCardMakerMDI.MarkDirty();
        }

        public ProjectLayout GetCurrentProjectLayout()
        {
            if (null != m_tnCurrentLayout)
            {
                return (ProjectLayout)m_tnCurrentLayout.Tag;
            }
            return null;
        }

        public ProjectLayout GetProjectLayoutFromNode(int nIdx)
        {
            return (ProjectLayout)treeView.Nodes[0].Nodes[nIdx].Tag;
        }

        public int GetCurrentLayoutIndex()
        {
            int nIdx = 0;
            foreach(TreeNode tNode in treeView.Nodes[0].Nodes)
            {
                if (m_tnCurrentLayout == tNode)
                {
                    return nIdx;
                }
                nIdx++;
            }
            return -1;
        }

        public int LayoutCount
        {
            get
            {
                return treeView.Nodes[0].Nodes.Count;
            }
        }

        public void UpdateSelectedNodeLayoutColor()
        {
            if (null != treeView.SelectedNode && 1 == treeView.SelectedNode.Level) // only do this on layout level nodes
            {
                if (null != m_tnCurrentLayout)
                {
                    m_tnCurrentLayout.BackColor = Color.White; // restore the previous
                }
                m_tnCurrentLayout = treeView.SelectedNode;
                m_tnCurrentLayout.BackColor = Color.LightBlue;
            }
        }

        private void printCardLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CardMakerMDI.Instance.PrintInit(false, -1, -1);
        }

        private void printPreviewCardLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CardMakerMDI.Instance.PrintInit(true, -1, -1);
        }

        private void exportCardLayoutAsImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CardMakerMDI.Instance.ExportImages(false);
        }

        private void exportCardLayoutAsPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CardMakerMDI.Instance.ExportViaPDFSharp(false);
        }

        private void setNameFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string NAME = "NAME";
            const string ROTATION = "ROTATION";
            const string EXPORT_WIDTH = "EXPORT_WIDTH";
            const string EXPORT_HEIGHT = "EXPORT_HEIGHT";
            const string EXPORT_TRANSPARENT = "EXPORT_TRANSPARENT";

            Type typeObj = treeView.SelectedNode.Tag.GetType();
            string sExistingFormat = string.Empty;
            var zQuery = new QueryPanelDialog("Configure Layout Export", 450, 200, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);

            if (typeof(Project) == typeObj)
            {
                sExistingFormat = ((Project)treeView.SelectedNode.Tag).exportNameFormat;
            }
            else if (typeof(ProjectLayout) == typeObj)
            {
                var zProjectLayout = ((ProjectLayout) treeView.SelectedNode.Tag);

                sExistingFormat = zProjectLayout.exportNameFormat;
                var nDefaultRotationIndex =
                    Math.Max(0, ProjectLayout.AllowedExportRotations.ToList()
                        .IndexOf(zProjectLayout.exportRotation.ToString()));
                zQuery.AddPullDownBox("Export Rotation (Print/PDF/Export)", ProjectLayout.AllowedExportRotations,
                    nDefaultRotationIndex, ROTATION);
                zQuery.AddNumericBox("Export Width", zProjectLayout.exportWidth,
                    0, 65536, EXPORT_WIDTH);
                zQuery.AddNumericBox("Export Height", zProjectLayout.exportHeight,
                    0, 65536, EXPORT_HEIGHT);
                zQuery.AddCheckBox("Export Transparent Background", zProjectLayout.exportTransparentBackground,
                    EXPORT_TRANSPARENT);
            }

            zQuery.AddTextBox("Name Format", sExistingFormat ?? string.Empty, false, NAME);

            if(DialogResult.OK == zQuery.ShowDialog(this))
            {
                if (typeof(Project) == typeObj)
                {
                    ((Project)treeView.SelectedNode.Tag).exportNameFormat = zQuery.GetString(NAME);
                }
                else if (typeof(ProjectLayout) == typeObj)
                {
                    var zProjectLayout = ((ProjectLayout)treeView.SelectedNode.Tag);
                    zProjectLayout.exportNameFormat = zQuery.GetString(NAME);
                    zProjectLayout.exportRotation = int.Parse(zQuery.GetString(ROTATION));
                    zProjectLayout.exportWidth = int.Parse(zQuery.GetString(EXPORT_WIDTH));
                    zProjectLayout.exportHeight = int.Parse(zQuery.GetString(EXPORT_HEIGHT));
                    zProjectLayout.exportTransparentBackground = zQuery.GetBool(EXPORT_TRANSPARENT);
                }
                CardMakerMDI.Instance.MarkDirty();
            }
        }


        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var tnSource = e.Item as TreeNode;
            if (null != tnSource && 
                null != tnSource.Tag && 
                tnSource.Tag is ProjectLayout)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                var tnTarget = ((TreeView)sender).GetNodeAt(pt);
                var tnDrag = (TreeNode)e.Data.GetData(typeof(TreeNode));

                if (null != tnTarget && 
                    null != tnTarget.Tag && 
                    tnDrag != tnTarget &&
                    tnTarget.Tag is ProjectLayout &&
                    tnTarget.TreeView == tnDrag.TreeView)
                {
                    int nDragIdx = tnDrag.Index;
                    int nTargetIdx = tnTarget.Index;
                    tnTarget.Parent.Nodes.RemoveAt(nDragIdx);
                    tnTarget.Parent.Nodes.Insert(nTargetIdx, tnDrag);
                    var zProject = tnTarget.Parent.Tag as Project;
                    if (null != zProject)
                    {
                        var listLayouts = zProject.Layout.ToList();
                        listLayouts.RemoveAt(nDragIdx);
                        listLayouts.Insert(nTargetIdx, (ProjectLayout) tnDrag.Tag);
                        zProject.Layout = listLayouts.ToArray();
                    }
                    CardMakerMDI.Instance.MarkDirty();
                }
            }
        }

    }
}