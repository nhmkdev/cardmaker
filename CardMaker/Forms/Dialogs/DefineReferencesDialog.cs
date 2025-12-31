using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CardMaker.Card.Import;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using ClosedXML.Excel;
using Support.Google.Sheets;
using Support.UI;

namespace CardMaker.Forms.Dialogs
{
    public partial class DefineReferencesDialog : Form
    {
        public List<ProjectLayoutReference> Result { get; private set; }
        private string m_sProjectPath = string.Empty;

        public static void ShowDefineReferences(IWin32Window zParentForm)
        {
            var zDefineReferencesDialog = new DefineReferencesDialog(
                ProjectManager.Instance.LoadedProject.DefineReferences?.ToList()
                ?? new List<ProjectLayoutReference>(), ProjectManager.Instance.ProjectPath);
            if (DialogResult.OK == zDefineReferencesDialog.ShowDialog(zParentForm))
            {
                ProjectManager.Instance.LoadedProject.DefineReferences = zDefineReferencesDialog.Result.ToArray();
                LayoutManager.Instance.RefreshActiveLayout();
                ProjectManager.Instance.FireProjectUpdated(true);
            }
        }

        public DefineReferencesDialog(List<ProjectLayoutReference> listReferences, string projectPath)
        {
            InitializeComponent();
            Result = listReferences;
            m_sProjectPath = projectPath;
        }

        private void DefineReferencesDialog_Load(object sender, EventArgs e)
        {
            Result.ForEach(AddReference);
            listViewReferences_Resize(sender, e);
        }

        private void AddReference(ProjectLayoutReference zReference)
        {
            var zSpreadsheetReference = ReferenceUtil.GetSpreadsheetReference(zReference);
            listViewReferences.Items.Add(new ListViewItem(new string[]
            {
                zSpreadsheetReference.DisplayName,
                zSpreadsheetReference.ReferenceType.ToString(),
                zReference.RelativePath // full definition
            }) { Tag = zReference });
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // TODO: support xlsx, csv and google (use the existing reference add dialog flow if available)

            var sFile = FormUtils.FileOpenHandler("CSV files (*.csv)|*.csv|All files (*.*)|*.*", null, true);
            if (null != sFile)
            {
                AddReference(ReferenceUtil.UpdateReferenceRelativePath(
                    new ProjectLayoutReference()
                    {
                        RelativePath = sFile
                    },
                    m_sProjectPath,
                    null));
            }
        }

        private void btnAddExcel_Click(object sender, EventArgs e)
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
                    var sReference = ExcelSpreadsheetReference.SerializeToReferenceString(
                        sFile,
                        zExcelSheetSelectionQueryDialog.GetString(EXCEL_SHEET_SELECT));
                    AddReference(ReferenceUtil.UpdateReferenceRelativePath(
                        new ProjectLayoutReference()
                        {
                            RelativePath = sReference,
                        },
                        m_sProjectPath,
                        null));
                }
            }
        }

        private void btnAddGoogle_Click(object sender, EventArgs e)
        {
#warning TODO: this code is duplicated, can easily be shared
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
                AddReference(new ProjectLayoutReference()
                {
                    RelativePath = zGoogleSpreadsheetReference.SerializeToReferenceString()
                });
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var listSelectedIndices = new List<int>();
            foreach (var zEntry in listViewReferences.SelectedIndices)
            {
                listSelectedIndices.Add((int)zEntry);
            }
            listSelectedIndices.Sort();
            for (var nIdx = listSelectedIndices.Count - 1; nIdx >= 0; nIdx--)
            {
                listViewReferences.Items.RemoveAt(listSelectedIndices[nIdx]);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Result = new List<ProjectLayoutReference>();
            foreach (var zEntry in listViewReferences.Items)
            {
                Result.Add((ProjectLayoutReference)((ListViewItem)zEntry).Tag);
            }
            // de-dupe by relative path
            Result = Result.GroupBy(x => x.RelativePath).Select(x => x.First()).ToList();
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void listViewReferences_Resize(object sender, EventArgs e)
        {
            ListViewAssist.ResizeColumnHeaders(listViewReferences);
        }
    }
}
