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
using CardMaker.XML;
using ClosedXML.Excel;
using Support.Google.Sheets;
using Support.IO;
using Support.UI;
using Support.Util;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace CardMaker.Forms
{
    public partial class MDIProject : Form
    {
        private static readonly Color DEFAULT_REFERENCE_COLOR = Color.LightGreen;

        private TreeNode m_tnCurrentLayout;

        private bool m_bUpdatingTreeNodes = false;

        public MDIProject()
        {
            InitializeComponent();
            ProjectManager.Instance.ProjectOpened += Project_Opened;
            ProjectManager.Instance.LayoutAdded += Layout_Added;

            // layout selection may occur outside of the project window (MDIIssues)
            LayoutManager.Instance.LayoutSelectRequested += LayoutSelect_Requested;
            LayoutManager.Instance.LayoutConfigureRequested += LayoutConfigure_Requested;
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

        void LayoutSelect_Requested(object sender, ProjectLayoutEventArgs args)
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

        void Layout_Added(object sender, ProjectLayoutEventArgs args)
        {
            AddProjectLayout(args.Layout);
        }

        void Project_Opened(object sender, ProjectEventArgs e)
        {
            ResetTreeToProject(e.Project);
            LayoutManager.Instance.SetActiveLayout(null);
        }

        private void LayoutConfigure_Requested(object sender, ProjectLayoutEventArgs args)
        {
            const string NAME = "NAME";
            const string ROTATION = "ROTATION";
            const string EXPORT_WIDTH = "EXPORT_WIDTH";
            const string EXPORT_HEIGHT = "EXPORT_HEIGHT";
            const string EXPORT_CROP = "EXPORT_CROP";
            const string EXPORT_TRANSPARENT = "EXPORT_TRANSPARENT";
            const string EXPORT_BACKGROUND_COLOR = "EXPORT_BACKGROUND_COLOR";
            const string EXPORT_PDF_AS_PAGE_BACK = "EXPORT_PDF_AS_PAGE_BACK";
            const string EXPORT_BORDER = "EXPORT_BORDER";
            const string EXPORT_BORDER_CROSS_SIZE = "EXPORT_BORDER_CROSS_SIZE";

            Type typeObj = treeView.SelectedNode.Tag.GetType();
            var sExistingFormat = string.Empty;
            var zQuery = FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("Configure Layout Export", 550, 300, false));

            if (typeof(Project) == typeObj)
            {
                sExistingFormat = ((Project)treeView.SelectedNode.Tag).exportNameFormat;
            }
            else if (typeof(ProjectLayout) == typeObj)
            {
                var zProjectLayout = ((ProjectLayout)treeView.SelectedNode.Tag);

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
                    var nWidth = zProjectLayout.width;
                    do
                    {
                        nColumns++;
                        nWidth += zProjectLayout.width + zProjectLayout.buffer;
                    } while (nWidth <= zProjectLayout.exportWidth);
                }

                if (zProjectLayout.exportHeight > 0)
                {
                    var nHeight = zProjectLayout.height;
                    do
                    {
                        nRows++;
                        nHeight += zProjectLayout.height + zProjectLayout.buffer;
                    } while (nHeight <= zProjectLayout.exportHeight);
                }

                var numericColumns = zQuery.AddNumericBox("Stitched Columns (changes export width)", nColumns, 0, 100, "COLUMNS");

                var numericRows = zQuery.AddNumericBox("Stitched Rows (changes export height)", nRows, 0, 100, "ROWS");

                var numericExportWidth = zQuery.AddNumericBox("Export Width", zProjectLayout.exportWidth,
                    0, 65536, EXPORT_WIDTH);
                var numericExportHeight = zQuery.AddNumericBox("Export Height", zProjectLayout.exportHeight,
                    0, 65536, EXPORT_HEIGHT);

                zQuery.AddTextBox("Export Crop Definition", zProjectLayout.exportCropDefinition, false, EXPORT_CROP);

                zQuery.AddCheckBox("Export Transparent Background", zProjectLayout.exportTransparentBackground,
                    EXPORT_TRANSPARENT);

                zQuery.AddColorSelect("Export Background Color", zProjectLayout.exportBackgroundColor, EXPORT_BACKGROUND_COLOR, CardMakerSettings.IniManager);

                zQuery.AddCheckBox("Export Layout Border (Draw Border required)", zProjectLayout.exportLayoutBorder, EXPORT_BORDER);
                zQuery.AddNumericBox("Export Layout Border Cross Size", zProjectLayout.exportLayoutBorderCrossSize, 0, int.MaxValue, 1, 0, EXPORT_BORDER_CROSS_SIZE);

                numericColumns.ValueChanged += (o, a) =>
                {
                    numericExportWidth.Value = (zProjectLayout.width * numericColumns.Value) +
                        Math.Max(0, (numericColumns.Value - 1) * zProjectLayout.buffer);
                };

                numericRows.ValueChanged += (o, a) =>
                {
                    numericExportHeight.Value = (zProjectLayout.height * numericRows.Value) +
                        Math.Max(0, (numericRows.Value - 1) * zProjectLayout.buffer);
                };

            }

            zQuery.AddTextBox("Name Format", sExistingFormat ?? string.Empty, false, NAME);

            if (DialogResult.OK == zQuery.ShowDialog(this))
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
                    zProjectLayout.exportCropDefinition = zQuery.GetString(EXPORT_CROP);
                    zProjectLayout.exportTransparentBackground = zQuery.GetBool(EXPORT_TRANSPARENT);
                    zProjectLayout.exportBackgroundColor = zQuery.GetString(EXPORT_BACKGROUND_COLOR);
                    zProjectLayout.exportPDFAsPageBack = zQuery.GetBool(EXPORT_PDF_AS_PAGE_BACK);
                    zProjectLayout.exportLayoutBorder = zQuery.GetBool(EXPORT_BORDER);
                    zProjectLayout.exportLayoutBorderCrossSize = (int)zQuery.GetDecimal(EXPORT_BORDER_CROSS_SIZE);
                }
                LayoutManager.Instance.FireLayoutUpdatedEvent(true);
            }
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
                var sOldName = zLayout.Name;
                zLayout.Name = e.Label;
                ProjectManager.Instance.FireLayoutRenamed(zLayout, sOldName);
                ProjectManager.Instance.FireProjectUpdated(true);
            }
        }

        private void addLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string NAME = "name";
            const string WIDTH = "width";
            const string HEIGHT = "height";
            const string DPI = "dpi";

            var zQuery = FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("New Layout", 450, false));
            zQuery.AddTextBox("Name", "New Layout", false, NAME);
            zQuery.AddNumericBox("Width", 300, 1, Int32.MaxValue, WIDTH);
            zQuery.AddNumericBox("Height", 300, 1, Int32.MaxValue, HEIGHT);
            zQuery.AddNumericBox("DPI", 300, 100, 600, DPI);
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

            var zQuery = FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("Select Layout Template", 600, false));
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
            var zQuery = FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("Template Name", 450, 80, false));
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

#warning TODO: revisit this, ProjectManager should probably contain this logic like AddLayout

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
            LayoutManager.ShowAdjustLayoutSettingsDialog(false, (ProjectLayout)treeView.SelectedNode.Tag, this);
        }

        private void duplicateLayoutCustomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null == treeView.SelectedNode) return;
            LayoutManager.ShowAdjustLayoutSettingsDialog(true, (ProjectLayout)treeView.SelectedNode.Tag, this);
        }

        private void tryToAddReferenceNode(string sReferenceData)
        {
                var zLayout = (ProjectLayout)treeView.SelectedNode.Tag;
                var bNewDefault = 0 == treeView.SelectedNode.Nodes.Count;
                var tnReference = AddReferenceNode(treeView.SelectedNode, sReferenceData, bNewDefault, zLayout);
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

        private void addReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sFile = FormUtils.FileOpenHandler("CSV files (*.csv)|*.csv|All files (*.*)|*.*", null, true);
            if (null != sFile)
            {
                tryToAddReferenceNode(sFile);
            }
        }

        private void addExcelReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sFile = FormUtils.FileOpenHandler("Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*", null, true);
            if (null != sFile)
            {
                var sheets = new List<string>();
                using (var zFileStream = new FileStream(sFile, FileMode.Open, FileAccess.Read,
                           FileShare.ReadWrite))
                {
                    // Open File
                    var workbook = new XLWorkbook(zFileStream);

                    // Grab all the sheet names
                    foreach (IXLWorksheet sheet in workbook.Worksheets)
                    {
                        sheets.Add(sheet.Name);
                    }
                }

                // Let the user select a sheet from the spreadsheet they selected
                const string EXCEL_SHEET_SELECT = "excel_sheet_select";
                var zExcelSheetSelectionQueryDialog = FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("Select Sheet", 400, false));
                zExcelSheetSelectionQueryDialog.AddPullDownBox("Sheet", sheets.ToArray(), 0, EXCEL_SHEET_SELECT);
                if (zExcelSheetSelectionQueryDialog.ShowDialog(this) == DialogResult.OK)
                {
                    tryToAddReferenceNode(ExcelSpreadsheetReference.SerializeToReferenceString(sFile, zExcelSheetSelectionQueryDialog.GetString(EXCEL_SHEET_SELECT)));
                }
            }
        }

        private void addGoogleSpreadsheetReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!GoogleAuthManager.CheckGoogleCredentials(this))
            {
                return;
            }
            var zDialog =
                new GoogleSpreadsheetSelector(new GoogleSpreadsheet(CardMakerInstance.GoogleInitializerFactory), true);
            if (DialogResult.OK == zDialog.ShowDialog(this))
            {
                var zGoogleSpreadsheetReference = new GoogleSpreadsheetReference(zDialog.SelectedSpreadsheet)
                {
                    SheetName = zDialog.SelectedSheet
                };
                tryToAddReferenceNode(zGoogleSpreadsheetReference.GenerateFullReference());
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

        private void toolStripMenuItemSetLayoutExport_Click(object sender, EventArgs e)
        {
            LayoutManager.Instance.FireLayoutConfigureRequested();
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

        private void windowsExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(ProjectManager.Instance.ProjectPath)) return;
            ProcessUtil.StartProcess(ProjectManager.Instance.ProjectPath, "open");
        }

        private void expandAllNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bUpdatingTreeNodes = true;
            treeView.ExpandAll();
            m_bUpdatingTreeNodes = false;
            UpdateProjectCollapsedNodes();
            ProjectManager.Instance.FireProjectUpdated(true);
        }

        private void collapseAllNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bUpdatingTreeNodes = true;
            treeView.CollapseAll();
            treeView.Nodes[0].Expand();
            m_bUpdatingTreeNodes = false;
            UpdateProjectCollapsedNodes();
            ProjectManager.Instance.FireProjectUpdated(true);
        }

        private void treeView_AfterCollapseOrExpand(object sender, TreeViewEventArgs e)
        {
            if (m_bUpdatingTreeNodes)
            {
                return;
            }
            UpdateProjectCollapsedNodes();
            ProjectManager.Instance.FireProjectUpdated(true);
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
                m_bUpdatingTreeNodes = true;
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

                tnRoot.Expand();
                m_bUpdatingTreeNodes = false;
                RestoreProjectCollapsedNodes();
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
        /// <param name="sReferenceData"></param>
        /// <param name="bSetAsDefault"></param>
        /// <param name="zLayout"></param>
        /// <returns>The new Reference tree node or null if there is an existing reference by the same definition</returns>
        private static TreeNode AddReferenceNode(TreeNode tnLayout, string sReferenceData, bool bSetAsDefault,
            ProjectLayout zLayout)
        {
            var sProjectPath = ProjectManager.Instance.ProjectPath;
            var zReference = new ProjectLayoutReference
            {
                Default = bSetAsDefault,
                RelativePath = sReferenceData,
            };
            ReferenceUtil.UpdateReferenceRelativePath(zReference, sProjectPath, null);
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

        #region Collapsed Node Handling
        /// <summary>
        /// Updates the project tracking of the collapsed nodes
        /// </summary>
        private void UpdateProjectCollapsedNodes()
        {
            if (treeView.Nodes.Count == 0)
            {
                return;
            }
            var listCollapsedNodes = new List<int>();
            var zChildNodes = treeView.Nodes[0].Nodes;
            for (var nIdx = 0; nIdx < zChildNodes.Count; nIdx++)
            {
                if (!zChildNodes[nIdx].IsExpanded)
                {
                    listCollapsedNodes.Add(nIdx);
                }
            }

            ProjectManager.Instance.LoadedProject.collapsedNodes = string.Join(";", listCollapsedNodes);
        }

        /// <summary>
        /// Restores the tree node states based on the project
        /// </summary>
        private void RestoreProjectCollapsedNodes()
        {
            if (treeView.Nodes.Count == 0)
            {
                return;
            }
            m_bUpdatingTreeNodes = true;
            var sCollapsedNodes = ProjectManager.Instance.LoadedProject.collapsedNodes ?? string.Empty;

            var enumerableIndices = sCollapsedNodes.Split(new char[] { ';' }).ToList().Select(sNodeIndex =>
            {
                if (int.TryParse(sNodeIndex, out var nIdx))
                {
                    return nIdx;
                }

                return -1;
            });
            var setCollapsedNodes = new HashSet<int>(enumerableIndices);
            var zChildNodes = treeView.Nodes[0].Nodes;
            for (var nIdx = 0; nIdx < zChildNodes.Count; nIdx++)
            {
                if (setCollapsedNodes.Contains(nIdx))
                {
                    zChildNodes[nIdx].Collapse(true);
                }
            }
            m_bUpdatingTreeNodes = false;
        }
        #endregion

    }
}