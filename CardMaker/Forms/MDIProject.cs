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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CardMaker.Card.Import;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.Events.Managers;
using CardMaker.Forms.Dialogs;
using CardMaker.Properties;
using CardMaker.XML;
using Support.IO;
using Support.UI;
using LayoutEventArgs = CardMaker.Events.Args.LayoutEventArgs;

namespace CardMaker.Forms
{
    public partial class MDIProject : Form
    {
        private static readonly Color DEFAULT_REFERENCE_COLOR = Color.LightGreen;

        private TreeNode m_tnCurrentLayout;

        public MDIProject()
        {
            InitializeComponent();
            ProjectManager.Instance.ProjectOpened += Project_Opened;
            ProjectManager.Instance.LayoutAdded += Layout_Added;

            // layout selection may occur outside of the project window (MDIIssues)
            LayoutManager.Instance.LayoutSelectRequested += LayoutSelect_Requested;
        }

        #region overrides

        protected override CreateParams CreateParams
        {
            get
            {
                const int CP_NOCLOSE_BUTTON = 0x200;
                var zParams = base.CreateParams;
                zParams.ClassStyle = zParams.ClassStyle | CP_NOCLOSE_BUTTON;
                return zParams;
            }
        }

        #endregion

        #region manager events

        void LayoutSelect_Requested(object sender, LayoutEventArgs args)
        {
            if (null == m_tnCurrentLayout || (ProjectLayout)m_tnCurrentLayout.Tag != args.Layout)
            {
                foreach (TreeNode tnNode in treeView.TopNode.Nodes)
                {
                    if ((ProjectLayout)tnNode.Tag == args.Layout)
                    {
                        treeView.SelectedNode = tnNode;
                        break;
                    }
                }
            }
        }

        void Layout_Added(object sender, LayoutEventArgs args)
        {
            AddProjectLayout(args.Layout);
        }

        void Project_Opened(object sender, ProjectEventArgs e)
        {
            ResetTreeToProject(e.Project);
            LayoutManager.Instance.SetActiveLayout(null);
        }

        #endregion

        #region form events

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode?.Tag != null)
            {
                var type = treeView.SelectedNode.Tag.GetType();
                if(typeof(Project) == type)
                {
                    treeView.ContextMenuStrip = contextMenuStripProject;
                }
                else if(typeof(ProjectLayout) == type)
                {
                    treeView.ContextMenuStrip = contextMenuStripLayout;
                    if (m_tnCurrentLayout != treeView.SelectedNode)
                    {
                        UpdateSelectedNodeLayoutColor();
                        LayoutManager.Instance.SetActiveLayout((ProjectLayout)m_tnCurrentLayout.Tag);
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
                addCardLayoutFromTemplateToolStripMenuItem.Enabled = 0 < LayoutTemplateManager.Instance.LayoutTemplates.Count;
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
                ProjectManager.Instance.FireProjectUpdated(true);
            }
        }

        private void addLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string NAME = "name";
            const string WIDTH = "width";
            const string HEIGHT = "height";
            const string DPI = "dpi";

            var zQuery = new QueryPanelDialog("New Layout", 450, false);
            zQuery.SetIcon(Resources.CardMakerIcon);
            zQuery.AddTextBox("Name", "New Layout", false, NAME);
            zQuery.AddNumericBox("Width", 300, 1, Int32.MaxValue, WIDTH);
            zQuery.AddNumericBox("Height", 300, 1, Int32.MaxValue, HEIGHT);
            zQuery.AddNumericBox("DPI", 300, 100, 9600, DPI);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var zLayout = new ProjectLayout(zQuery.GetString(NAME))
                {
                    width = (int)zQuery.GetDecimal(WIDTH),
                    height = (int)zQuery.GetDecimal(HEIGHT),
                    dpi = (int)zQuery.GetDecimal(DPI)
                };
                ProjectManager.Instance.AddLayout(zLayout);
                ProjectManager.Instance.FireProjectUpdated(true);
            }
        }

        private void addCardLayoutFromTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string TEMPLATE = "template";
            const string NAME = "name";
            const string COUNT = "count";
            var listTemplateNames = new List<string>();
            LayoutTemplateManager.Instance.LayoutTemplates.ForEach(x => listTemplateNames.Add(x.ToString()));

            var zQuery = new QueryPanelDialog("Select Layout Template", 600, false);
            zQuery.SetIcon(Resources.CardMakerIcon);
            zQuery.AddTextBox("New Layout Name", "New Layout", false, NAME);
            zQuery.AddNumericBox("Number to create", 1, 1, 256, COUNT);
            var zTxtFilter = zQuery.AddTextBox("Template Filter", string.Empty, false, TEMPLATE + NAME);
            var zListBoxTemplates = zQuery.AddListBox("Template", listTemplateNames.ToArray(), null, false, 240, TEMPLATE);
            zListBoxTemplates.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
            zTxtFilter.TextChanged += (o, args) =>
            {
                var txtBox = (TextBox) o;
                zListBoxTemplates.Items.Clear();
                if (string.IsNullOrWhiteSpace(txtBox.Text))
                {
                    listTemplateNames.ForEach(zTemplate => zListBoxTemplates.Items.Add(zTemplate));
                }
                else
                {
                    listTemplateNames.Where(sTemplateName => sTemplateName.ToLower().Contains(txtBox.Text.ToLower())).ToList().ForEach(zTemplate => zListBoxTemplates.Items.Add(zTemplate));
                }
            };

            zQuery.AllowResize();
            while (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var nSelectedIndex = listTemplateNames.IndexOf(zQuery.GetString(TEMPLATE));
                if (-1 == nSelectedIndex)
                {
                    MessageBox.Show("Please select a layout template");
                    continue;
                }

                ProjectLayout zSelectedLayout = LayoutTemplateManager.Instance.LayoutTemplates[nSelectedIndex].Layout;

                for (int nCount = 0; nCount < zQuery.GetDecimal(COUNT); nCount++)
                {
                    var zLayout = new ProjectLayout(zQuery.GetString(NAME));
                    zLayout.DeepCopy(zSelectedLayout);
                    ProjectManager.Instance.AddLayout(zLayout);
                }
                break;
            }
        }

        private void defineAsTemplateLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string NAME = "name";
            //const string COPY_REFS = "copy_refs";
            var zQuery = new QueryPanelDialog("Template Name", 450, 80, false);
            zQuery.SetIcon(Resources.CardMakerIcon);
            zQuery.AddTextBox("Name", "New Template", false, NAME);
            // TODO: is there really a case where the refs should be copied?
            //zQuery.AddCheckBox("Copy References", false, COPY_REFS);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var zLayout = new ProjectLayout();
                zLayout.DeepCopy((ProjectLayout)treeView.SelectedNode.Tag, /*zQuery.GetBool(COPY_REFS)*/ false);
                var zTemplate = new LayoutTemplate(zQuery.GetString(NAME), zLayout);
                if (LayoutTemplateManager.Instance.SaveLayoutTemplate(CardMakerInstance.StartupPath, zTemplate))
                {
                    LayoutTemplateManager.Instance.LayoutTemplates.Add(zTemplate);
                }
            }
        }

        private void removeLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (1 < treeView.SelectedNode.Parent.Nodes.Count)
            {
                if (DialogResult.Yes == MessageBox.Show(this, "Are you sure you want to remove this Layout?", "Remove Layout", MessageBoxButtons.YesNo))
                {

                    var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;

                    // no need to null check, 1 layout must always exist
                    var zProject = ProjectManager.Instance.LoadedProject;
                    var listLayouts = new List<ProjectLayout>(zProject.Layout);
                    listLayouts.Remove(zLayout);
                    zProject.Layout = listLayouts.ToArray();

                    treeView.SelectedNode.Parent.Nodes.Remove(treeView.SelectedNode);

                    ProjectManager.Instance.FireProjectUpdated(true);
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
            ProjectManager.Instance.AddLayout(zLayoutCopy);
        }

        private void resizeLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null == treeView.SelectedNode) return;

            AdjustLayoutSettings(false, (ProjectLayout)treeView.SelectedNode.Tag);
        }

        private void duplicateLayoutCustomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null == treeView.SelectedNode) return;

            AdjustLayoutSettings(true, (ProjectLayout) treeView.SelectedNode.Tag);
        }

        private void addReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sFile = FormUtils.FileOpenHandler("CSV files (*.csv)|*.csv|All files (*.*)|*.*", null, true);
            if (null != sFile)
            {
                var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;
                var bNewDefault = 0 == treeView.SelectedNode.Nodes.Count;
                var tnReference = AddReferenceNode(treeView.SelectedNode, sFile, bNewDefault, zLayout);
                if (null == tnReference)
                {
                    MessageBox.Show(this, "The specified reference is already associated with this layout.", "Reference Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (bNewDefault)
                {
                    tnReference.Parent.Expand();
                    LayoutManager.Instance.RefreshActiveLayout();

                }
                ProjectManager.Instance.FireProjectUpdated(true);
            }
        }

        private void addGoogleSpreadsheetReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!GoogleAuthManager.CheckGoogleCredentials(this))
            {
                return;
            }

            var zDialog = new GoogleSpreadsheetBrowser(GoogleReferenceReader.APP_NAME, GoogleReferenceReader.CLIENT_ID,
                CardMakerInstance.GoogleAccessToken, true);
            if (DialogResult.OK == zDialog.ShowDialog(this))
            {
                var bNewDefault = 0 == treeView.SelectedNode.Nodes.Count;
                var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;
                var tnReference = AddReferenceNode(
                    treeView.SelectedNode,
                    CardMakerConstants.GOOGLE_REFERENCE + CardMakerConstants.GOOGLE_REFERENCE_SPLIT_CHAR +
                    zDialog.SelectedSpreadsheet.Title.Text + CardMakerConstants.GOOGLE_REFERENCE_SPLIT_CHAR +
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
                    LayoutManager.Instance.RefreshActiveLayout();
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                }
                ProjectManager.Instance.FireProjectUpdated(true);            
            }
        }

        private void removeReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                treeView.SelectedNode.Parent.Nodes[0].BackColor = DEFAULT_REFERENCE_COLOR;
                listReferences[0].Default = true;
            }

            if (zLayout == LayoutManager.Instance.ActiveLayout)
            {
                LayoutManager.Instance.SetActiveLayout(zLayout);
            }
            ProjectManager.Instance.FireProjectUpdated(true);
        }

        private void setAsDefaultReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tnNode = treeView.SelectedNode;
            var zReference = (ProjectLayoutReference)tnNode.Tag;

            foreach (TreeNode tnReferenceNode in tnNode.Parent.Nodes)
            {
                tnReferenceNode.BackColor = Color.White;
                var zSubReference = (ProjectLayoutReference)tnReferenceNode.Tag;
                zSubReference.Default = false;
            }
            tnNode.BackColor = DEFAULT_REFERENCE_COLOR;
            zReference.Default = true;

            LayoutManager.Instance.RefreshActiveLayout();
            ProjectManager.Instance.FireProjectUpdated(true);
        }

        private void exportCardLayoutAsImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportManager.Instance.FireExportRequestedEvent(ExportType.Image);
        }

        private void exportCardLayoutAsPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportManager.Instance.FireExportRequestedEvent(ExportType.PDFSharp);
        }

        private void setNameFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string NAME = "NAME";
            const string ROTATION = "ROTATION";
            const string EXPORT_WIDTH = "EXPORT_WIDTH";
            const string EXPORT_HEIGHT = "EXPORT_HEIGHT";
            const string EXPORT_TRANSPARENT = "EXPORT_TRANSPARENT";
            const string EXPORT_PDF_AS_PAGE_BACK = "EXPORT_PDF_AS_PAGE_BACK";

            Type typeObj = treeView.SelectedNode.Tag.GetType();
            string sExistingFormat = string.Empty;
            var zQuery = new QueryPanelDialog("Configure Layout Export", 550, 300, false);
            zQuery.SetIcon(Resources.CardMakerIcon);

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
                zQuery.AddPullDownBox("Export Rotation (PDF/Export)", ProjectLayout.AllowedExportRotations,
                    nDefaultRotationIndex, ROTATION);

                zQuery.AddCheckBox("Export PDF Layout Page as Back", zProjectLayout.exportPDFAsPageBack, EXPORT_PDF_AS_PAGE_BACK);

                var nColumns = 0;
                var nRows = 0;

                if (zProjectLayout.exportWidth > 0)
                {
                    var nWidth = zProjectLayout.width + zProjectLayout.buffer;
                    nColumns = zProjectLayout.exportWidth / nWidth;
                }

                if (zProjectLayout.exportHeight > 0)
                {
                    var nHeight = zProjectLayout.width + zProjectLayout.buffer;
                    nRows = zProjectLayout.exportHeight / nHeight;
                }

                var numericColumns = zQuery.AddNumericBox("Stitched Columns (changes export width)", nColumns, 0, 100, "COLUMNS");

                var numericRows = zQuery.AddNumericBox("Stitched Rows (changes export height)", nRows, 0, 100, "ROWS");

                var numericExportWidth = zQuery.AddNumericBox("Export Width", zProjectLayout.exportWidth,
                    0, 65536, EXPORT_WIDTH);
                var numericExportHeight = zQuery.AddNumericBox("Export Height", zProjectLayout.exportHeight,
                    0, 65536, EXPORT_HEIGHT);
                zQuery.AddCheckBox("Export Transparent Background", zProjectLayout.exportTransparentBackground,
                    EXPORT_TRANSPARENT);

                numericColumns.ValueChanged += (o, args) =>
                {
                    numericExportWidth.Value = (zProjectLayout.width * numericColumns.Value) + 
                        Math.Max(0, (numericColumns.Value - 1) * zProjectLayout.buffer);
                };

                numericRows.ValueChanged += (o, args) =>
                {
                    numericExportHeight.Value = (zProjectLayout.height * numericRows.Value) +
                        Math.Max(0, (numericRows.Value - 1) * zProjectLayout.buffer);
                };
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
                    zProjectLayout.exportPDFAsPageBack = zQuery.GetBool(EXPORT_PDF_AS_PAGE_BACK);
                }
                ProjectManager.Instance.FireProjectUpdated(true);
            }
        }

        private void projectSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectSettingsDialog.ShowProjectSettings(this);
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var tnSource = e.Item as TreeNode;
            if (tnSource?.Tag != null && tnSource.Tag is ProjectLayout)
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

                if (null != tnTarget?.Tag && 
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
                    ProjectManager.Instance.FireProjectUpdated(true);
                }
            }
        }

#endregion

        /// <summary>
        /// Updates the selected layout node color (if applicable)
        /// </summary>
        private void UpdateSelectedNodeLayoutColor()
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

        /// <summary>
        /// Resets the treeview to the specified project
        /// </summary>
        /// <param name="zProject">The project to display</param>
        private void ResetTreeToProject(Project zProject)
        {
            if (null != treeView)
            {
                treeView.Nodes.Clear();
                var tnRoot = new TreeNode("Layouts")
                {
                    Tag = zProject
                };
                treeView.Nodes.Add(tnRoot);
                foreach (var zLayout in zProject.Layout)
                {
                    // no need to update the project
                    AddProjectLayout(zLayout);

                    LayoutManager.InitializeElementCache(zLayout);
                }
                tnRoot.ExpandAll();
            }
            m_tnCurrentLayout = null;
        }

        /// <summary>
        /// Adds a project layout tree node
        /// </summary>
        /// <param name="zLayout"></param>
        /// <returns></returns>
        private void AddProjectLayout(ProjectLayout zLayout)
        {
            TreeNode tnLayout = treeView.Nodes[0].Nodes.Add(zLayout.Name);
            tnLayout.Tag = zLayout;

            if (null != zLayout.Reference)
            {
                foreach (ProjectLayoutReference zReference in zLayout.Reference)
                {
                    // no need to update the layout
                    AddReferenceNode(tnLayout, zReference, null);
                }
                tnLayout.Expand();
            }
        }

        /// <summary>
        /// UI facing method for adding a reference node (for use from the context menu to add a new reference)
        /// </summary>
        /// <param name="tnLayout"></param>
        /// <param name="sFile"></param>
        /// <param name="bSetAsDefault"></param>
        /// <param name="zLayout"></param>
        /// <returns>The new Reference tree node or null if there is an existing reference by the same definition</returns>
        private static TreeNode AddReferenceNode(TreeNode tnLayout, string sFile, bool bSetAsDefault,
            ProjectLayout zLayout)
        {
            var sProjectPath = ProjectManager.Instance.ProjectPath;
            var zReference = new ProjectLayoutReference
            {
                Default = bSetAsDefault,
                RelativePath = IOUtils.GetRelativePath(sProjectPath,
                    sFile)
            };
            return AddReferenceNode(tnLayout, zReference, zLayout);
        }

        /// <summary>
        /// Internal/Project load handling for adding a reference node.
        /// </summary>
        /// <param name="tnLayout"></param>
        /// <param name="zReference"></param>
        /// <param name="zLayout">The layout to update the references for (may be null if no update is needed - ie. project loading)</param>
        /// <returns></returns>
        private static TreeNode AddReferenceNode(TreeNode tnLayout, ProjectLayoutReference zReference,
            ProjectLayout zLayout)
        {
            var sProjectPath = ProjectManager.Instance.ProjectPath;
            var sFullReferencePath = zReference.RelativePath;
            if (!String.IsNullOrEmpty(sProjectPath))
            {
                sFullReferencePath = sProjectPath + Path.DirectorySeparatorChar + zReference.RelativePath;
            }

            if (zLayout != null && zLayout.Reference != null)
            {
                // duplicate check
                foreach (var zExistingReference in zLayout.Reference)
                {
                    if (zExistingReference.RelativePath.Equals(zReference.RelativePath,
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        return null;
                    }
                }
            }

            var tnReference = new TreeNode(Path.GetFileName(sFullReferencePath))
            {
                BackColor = zReference.Default ? DEFAULT_REFERENCE_COLOR : Color.White,
                ToolTipText = zReference.RelativePath,
                Tag = zReference
            };
            tnLayout.Nodes.Add(tnReference);

            if (null != zLayout)
            {
                // update the ProjectLayout
                var listReferences = new List<ProjectLayoutReference>();
                if (null != zLayout.Reference)
                {
                    listReferences.AddRange(zLayout.Reference);
                }
                listReferences.Add(zReference);
                zLayout.Reference = listReferences.ToArray();
            }

            return tnReference;
        }

        private void AdjustLayoutSettings(bool bCreateNew, ProjectLayout zLayout)
        {
            var zQuery = new QueryPanelDialog("Resize Layout", 450, false);
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            const string LAYOUT_NAME = "layoutName";
            const string CENTER_ELEMENTS = "centerElements";
            const string WIDTH = "width";
            const string HEIGHT = "height";

            if (bCreateNew)
            {
                zQuery.AddTextBox("Layout Name", zLayout.Name + " copy", false, LAYOUT_NAME);
            }
            zQuery.AddNumericBox("Width", zLayout.width, 1, int.MaxValue, WIDTH);
            zQuery.AddNumericBox("Height", zLayout.height, 1, int.MaxValue, HEIGHT);
            zQuery.AddCheckBox("Center Elements", false, CENTER_ELEMENTS);

            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var zLayoutAdjusted = bCreateNew ? new ProjectLayout(zQuery.GetString(LAYOUT_NAME)) : zLayout;

                var nOriginalWidth = zLayout.width;
                var nOriginalHeight = zLayout.height;

                if (bCreateNew)
                {
                    zLayoutAdjusted.DeepCopy(zLayout);
                }

                zLayoutAdjusted.width = (int)zQuery.GetDecimal(WIDTH);
                zLayoutAdjusted.height = (int)zQuery.GetDecimal(HEIGHT);

                if (zQuery.GetBool(CENTER_ELEMENTS) && null != zLayoutAdjusted.Element)
                {
                    var pointOldCenter = new Point(nOriginalWidth / 2, nOriginalHeight / 2);
                    var pointNewCenter = new Point(zLayoutAdjusted.width / 2, zLayoutAdjusted.height / 2);
                    var nXAdjust = pointNewCenter.X - pointOldCenter.X;
                    var nYAdjust = pointNewCenter.Y - pointOldCenter.Y;
                    foreach (var zElement in zLayoutAdjusted.Element)
                    {
                        zElement.x += nXAdjust;
                        zElement.y += nYAdjust;
                    }
                }

                UserAction.ClearUndoRedoStacks();

                if (bCreateNew)
                {
                    ProjectManager.Instance.AddLayout(zLayoutAdjusted);
                }
                else
                {
                    LayoutManager.Instance.FireLayoutUpdatedEvent(true);
                }
            }
        }
    }
}