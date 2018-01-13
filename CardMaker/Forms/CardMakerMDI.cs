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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CardMaker.Card;
using CardMaker.Card.Export;
using CardMaker.Card.Shapes;
using CardMaker.Card.Translation;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.Events.Managers;
using CardMaker.Forms.Dialogs;
using CardMaker.XML;
using PdfSharp;
using Support.IO;
using Support.UI;
using LayoutEventArgs = CardMaker.Events.Args.LayoutEventArgs;

namespace CardMaker.Forms
{
    public partial class CardMakerMDI : AbstractDirtyForm
    {
        private readonly List<string> m_listRecentFiles = new List<string>();

        // last export settings for pdf
        private string m_sPdfExportLastFile = "";
        private bool m_bPdfExportLastOpen;
        private int m_nPdfExportLastOrientationIndex;

        public CardMakerMDI()
        {
            InitializeComponent();

            UserAction.OnClearUserActions = () => Logger.AddLogLine("Cleared Undo/Redo.");

            m_sBaseTitle = "Card Maker " + Application.ProductVersion + CardMakerBuild.GetBuildSuffix();

            m_sFileOpenFilter = "CMP files (*.cmp)|*.cmp|All files (*.*)|*.*";

            Icon = Properties.Resources.CardMakerIcon;

            CardMakerInstance.ApplicationIcon = Icon;
            CardMakerInstance.ApplicationForm = this;
        }

        #region Form Events

        private void CardMakerMDI_Load(object sender, EventArgs e)
        {
            // always before any dialogs
            ShapeManager.Init();

            ProjectManager.Instance.ProjectOpened += Project_Opened;
            ProjectManager.Instance.ProjectUpdated += Project_Updated;

            LayoutManager.Instance.LayoutUpdated += Layout_Updated;
            LayoutManager.Instance.LayoutLoaded += Layout_Loaded;

            ExportManager.Instance.ExportRequested += Export_Requested;

            // Same handler for both events
            GoogleAuthManager.Instance.GoogleAuthUpdateRequested += GoogleAuthUpdate_Requested;
            GoogleAuthManager.Instance.GoogleAuthCredentialsError += GoogleAuthUpdate_Requested;

            // Setup all the child dialogs
            var zCanvasForm = SetupMDIForm(new MDICanvas(), true);
            var zElementForm = SetupMDIForm(new MDIElementControl(), true);
            var zLayoutForm = SetupMDIForm(new MDILayoutControl(), true);
            var zLoggerForm = SetupMDIForm(new MDILogger(), true);
            var zProjectForm = SetupMDIForm(new MDIProject(), true);
            SetupMDIForm(new MDIIssues(), false);
            SetupMDIForm(new MDIDefines(), false);


            // populate the windows menu
            foreach (var zChild in MdiChildren)
            {
                string sText = string.Empty;
                switch (zChild.Name)
                {
                    case "MDICanvas":
                        sText = "&Canvas";
                        break;
                    case "MDIElementControl":
                        sText = "&Element Control";
                        break;
                    case "MDILayoutControl":
                        sText = "&Layout Control";
                        break;
                    case "MDILogger":
                        sText = "L&ogger";
                        break;
                    case "MDIProject":
                        sText = "&Project";
                        break;
                    case "MDIIssues":
                        sText = "&Issues";
                        break;
                    case "MDIDefines":
                        sText = "&Defines";
                        break;
                }

                ToolStripItem zItem = new ToolStripMenuItem(sText);
                zItem.Tag = zChild;
                windowToolStripMenuItem.DropDownItems.Add(zItem);

                zItem.Click += (zSender, eArgs) =>
                {
                    var zForm = (Form)((ToolStripMenuItem)zSender).Tag;
                    var pointLocation = zForm.Location;
                    zForm.Show();
                    zForm.BringToFront();
                    zForm.Location = pointLocation;
                };
            }

            // make a new project by default
            newToolStripMenuItem_Click(sender, e);

            var sData = CardMakerSettings.IniManager.GetValue(Name);
            var bRestoredFormState = false;
            if (!string.IsNullOrEmpty(sData))
            {
                IniManager.RestoreState(this, sData);
                bRestoredFormState = true;
            }
            foreach (var zForm in MdiChildren)
            {
                sData = CardMakerSettings.IniManager.GetValue(zForm.Name);
                if (!string.IsNullOrEmpty(sData))
                {
                    IniManager.RestoreState(zForm, sData);
                }
            }

            if (!bRestoredFormState)
            {
                Logger.AddLogLine("Restored default form layout.");
#if MONO_BUILD
                zCanvasForm.Size = new Size(457, 300);
                zCanvasForm.Location = new Point(209, 5);
                zElementForm.Size = new Size(768, 379);
                zElementForm.Location = new Point(3, 310);
                zLayoutForm.Size = new Size(300, 352);
                zLayoutForm.Location = new Point(805, 4);
                zProjectForm.Size = new Size(200, 266);
                zProjectForm.Location = new Point(6, 10);
                zLoggerForm.Size = new Size(403, 117);
                zLoggerForm.Location = new Point(789, 571);
#else
                zCanvasForm.Size = new Size(557, 300);
                zCanvasForm.Location = new Point(209, 5);
                zElementForm.Size = new Size(756, 339);
                zElementForm.Location = new Point(3, 310);
                zLayoutForm.Size = new Size(300, 352);
                zLayoutForm.Location = new Point(768, 4);
                zProjectForm.Size = new Size(200, 266);
                zProjectForm.Location = new Point(6, 10);
                zLoggerForm.Size = new Size(337, 291);
                zLoggerForm.Location = new Point(765, 365);
#endif
            }

            var arrayFiles = CardMakerSettings.IniManager.GetValue(IniSettings.PreviousProjects).Split(new char[] {CardMakerConstants.CHAR_FILE_SPLIT }, StringSplitOptions.RemoveEmptyEntries);
            if (0 < arrayFiles.Length)
            {
                foreach (var sFile in arrayFiles)
                {
                    m_listRecentFiles.Add(sFile);
                }
            }
            LayoutTemplateManager.Instance.LoadLayoutTemplates(CardMakerInstance.StartupPath);

            RestoreReplacementChars();

            var zGraphics = CreateGraphics();
            try
            {
                CardMakerInstance.ApplicationDPI = zGraphics.DpiX;
            }
            finally
            {
                zGraphics.Dispose();
            }


            // load the specified project from the command line
            if (!string.IsNullOrEmpty(CardMakerInstance.CommandLineProjectFile))
            {
                InitOpen(CardMakerInstance.CommandLineProjectFile);
            }

#if !DEBUG
            if(CardMakerBuild.IsUnstable())
            {
                MessageBox.Show(
                    "This is an UNSTABLE build of CardMaker. Please make backups of any projects before opening them with this version.");
            }
#endif
        }

        private void saveProjectAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitSave(true);
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitSave(false);
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitOpen();
        }
        private void drawElementBordersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawElementBordersToolStripMenuItem.Checked = !drawElementBordersToolStripMenuItem.Checked;
            CardMakerInstance.DrawElementBorder = drawElementBordersToolStripMenuItem.Checked;
            LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
        }

        private void drawSelectedElementGuidesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawSelectedElementGuidesToolStripMenuItem.Checked = !drawSelectedElementGuidesToolStripMenuItem.Checked;
            CardMakerInstance.DrawSelectedElementGuides = drawSelectedElementGuidesToolStripMenuItem.Checked;
            LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
        }

        private void drawSelectedElementRotationBoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawSelectedElementRotationBoundsToolStripMenuItem.Checked = !drawSelectedElementRotationBoundsToolStripMenuItem.Checked;
            CardMakerInstance.DrawSelectedElementRotationBounds = drawSelectedElementRotationBoundsToolStripMenuItem.Checked;
            LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
        }

        private void drawFormattedTextWordBordersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawFormattedTextWordOutlinesToolStripMenuItem.Checked = !drawFormattedTextWordOutlinesToolStripMenuItem.Checked;
            CardMakerInstance.DrawFormattedTextBorder = drawFormattedTextWordOutlinesToolStripMenuItem.Checked;
            LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
        }

        private void CardMakerMDI_FormClosing(object sender, FormClosingEventArgs e)
        {
            CardMakerSettings.IniManager.AutoFlush = false;
            CardMakerSettings.IniManager.SetValue(Name, IniManager.GetFormSettings(this));
            foreach (var zForm in MdiChildren)
            {
                CardMakerSettings.IniManager.SetValue(zForm.Name, IniManager.GetFormSettings(zForm));
                CardMakerSettings.IniManager.SetValue(zForm.Name + CardMakerConstants.VISIBLE_SETTING, zForm.Visible.ToString());
            }
            var zBuilder = new StringBuilder();
            var dictionaryFilenames = new Dictionary<string, object>();
            foreach (var sFile in m_listRecentFiles)
            {
                var sLowerFile = sFile.ToLower();
                if (dictionaryFilenames.ContainsKey(sLowerFile))
                    continue;
                dictionaryFilenames.Add(sLowerFile, null);
                zBuilder.Append(sFile + CardMakerConstants.CHAR_FILE_SPLIT);
            }
            CardMakerSettings.IniManager.SetValue(IniSettings.PreviousProjects, zBuilder.ToString());
            CardMakerSettings.IniManager.FlushIniSettings();
            SaveOnClose(e);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var eCancel = new CancelEventArgs();
            SaveOnEvent(eCancel, true);
            if (!eCancel.Cancel)
            {
                LayoutManager.Instance.SetActiveLayout(null);
                ElementManager.Instance.FireElementSelectedEvent(null);
                ProjectManager.Instance.OpenProject(null);
            }
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            recentProjectsToolStripMenuItem.DropDownItems.Clear();
            foreach (string sFile in m_listRecentFiles)
            {
                recentProjectsToolStripMenuItem.DropDownItems.Add(sFile, null, recentProject_Click);
            }
        }

        private void recentProject_Click(object sender, EventArgs e)
        {
            var zItem = (ToolStripItem)sender;
            var sFile = zItem.Text;
            InitOpen(sFile);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CardMakerInstance.ProcessingUserAction)
            {
                return;
            }

            var redoAction = UserAction.GetRedoAction();
            redoAction?.Invoke(true);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CardMakerInstance.ProcessingUserAction)
            {
                return;
            }

            var undoAction = UserAction.GetUndoAction();
            undoAction?.Invoke(false);
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            undoToolStripMenuItem.Enabled = 0 < UserAction.UndoCount;
            redoToolStripMenuItem.Enabled = 0 < UserAction.RedoCount;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zQuery = new QueryPanelDialog(
                "CardMaker Settings",
                450, 
                250,
#if !MONO_BUILD
                true
#else
                false
#endif
                );
            zQuery.SetIcon(CardMakerInstance.ApplicationIcon);
            zQuery.SetMaxHeight(600);
#if !MONO_BUILD
            zQuery.AddTab("General");
#endif
            zQuery.AddCheckBox("Enable Google Cache", CardMakerSettings.EnableGoogleCache, IniSettings.EnableGoogleCache);
            zQuery.AddCheckBox("Print/Export Layout Border", CardMakerSettings.PrintLayoutBorder, IniSettings.PrintLayoutBorder);
            zQuery.AddPullDownBox("Default Translator Type",
                new string[] { TranslatorType.Incept.ToString(), TranslatorType.JavaScript.ToString() }, (int)CardMakerSettings.DefaultTranslatorType, IniSettings.DefaultTranslator);
            zQuery.AddCheckBox("Log Incept Translation", CardMakerSettings.LogInceptTranslation,
                IniSettings.LogInceptTranslation);

#if !MONO_BUILD
            zQuery.AddTab("PDF Export");
#else
            zQuery.AddVerticalSpace(20);
            zQuery.AddLabel("---- PDF Export Settings ----", 16);
#endif
            zQuery.AddPullDownBox("Page Measurement Unit", Enum.GetNames(typeof(MeasurementUnit)), (int) CardMakerSettings.PrintPageMeasurementUnit, IniSettings.PrintPageMeasurementUnit);
            zQuery.AddNumericBox("Page Width", CardMakerSettings.PrintPageWidth, 1, 1024, 1, 2, IniSettings.PrintPageWidth);
            zQuery.AddNumericBox("Page Height", CardMakerSettings.PrintPageHeight, 1, 1024, 1, 2, IniSettings.PrintPageHeight);
            zQuery.AddNumericBox("Page Horizontal Margin", CardMakerSettings.PrintPageHorizontalMargin, 0, 1024, 0.01m, 2, IniSettings.PrintPageHorizontalMargin);
            zQuery.AddNumericBox("Page Vertical Margin", CardMakerSettings.PrintPageVerticalMargin, 0, 1024, 0.01m, 2, IniSettings.PrintPageVerticalMargin);
            zQuery.AddCheckBox("Auto-Center Layouts on Page", CardMakerSettings.PrintAutoHorizontalCenter, IniSettings.PrintAutoCenterLayout);
            zQuery.AddCheckBox("Print Layouts On New Page", CardMakerSettings.PrintLayoutsOnNewPage, IniSettings.PrintLayoutsOnNewPage);
            zQuery.SetIcon(Icon);

            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                CardMakerSettings.PrintPageMeasurementUnit = (MeasurementUnit)zQuery.GetIndex(IniSettings.PrintPageMeasurementUnit);
                CardMakerSettings.PrintPageWidth = zQuery.GetDecimal(IniSettings.PrintPageWidth);
                CardMakerSettings.PrintPageHeight = zQuery.GetDecimal(IniSettings.PrintPageHeight);
                CardMakerSettings.PrintPageHorizontalMargin = zQuery.GetDecimal(IniSettings.PrintPageHorizontalMargin);
                CardMakerSettings.PrintPageVerticalMargin = zQuery.GetDecimal(IniSettings.PrintPageVerticalMargin);
                CardMakerSettings.PrintAutoHorizontalCenter = zQuery.GetBool(IniSettings.PrintAutoCenterLayout);
                CardMakerSettings.PrintLayoutBorder = zQuery.GetBool(IniSettings.PrintLayoutBorder);
                CardMakerSettings.PrintLayoutsOnNewPage = zQuery.GetBool(IniSettings.PrintLayoutsOnNewPage);
                CardMakerSettings.DefaultTranslatorType = (TranslatorType)zQuery.GetIndex(IniSettings.DefaultTranslator);
                CardMakerSettings.LogInceptTranslation = zQuery.GetBool(IniSettings.LogInceptTranslation);

                var bWasGoogleCacheEnabled = CardMakerSettings.EnableGoogleCache;
                CardMakerSettings.EnableGoogleCache = zQuery.GetBool(IniSettings.EnableGoogleCache);
                if (!CardMakerSettings.EnableGoogleCache && bWasGoogleCacheEnabled)
                {
                    DeleteGoogleCache();
                }
            }
        }

        private void colorPickerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new RGBColorSelectDialog().ShowDialog(this);
        }

        private void importLayoutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = m_sFileOpenFilter,
                CheckFileExists = true
            };

            if (DialogResult.OK != ofd.ShowDialog(this))
            {
                return;
            }

            Project zProject = null;
            try
            {
                zProject = ProjectManager.LoadProject(ofd.FileName);
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Error Loading Project File: " + ex.Message);
            }

            if (null == zProject)
            {
                return;
            }

            var zQuery = new QueryPanelDialog("Select Layouts To Import", 500, false);
            const string LAYOUT_QUERY_KEY = "layoutquerykey";
            zQuery.AddListBox("Layouts", zProject.Layout.ToList().Select(projectLayout => projectLayout.Name).ToArray(), null, true, 400, LAYOUT_QUERY_KEY);
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var listIndices = zQuery.GetIndices(LAYOUT_QUERY_KEY).ToList();
                listIndices.ForEach(nIdx =>
                {
                    ProjectManager.Instance.AddLayout(zProject.Layout[nIdx]);
                });
            }
        }

        private void exportProjectToPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportViaPDFSharp(true);
        }

        private void exportImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportImages(true);
        }

        private void projectSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectSettingsDialog.ShowProjectSettings(this);
        }

        private void updateIssuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitSave(false);
            if (Dirty)
            {
                return;
            }

            IssueManager.Instance.FireRefreshRequestedEvent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Card Maker " +
                Environment.NewLine + Environment.NewLine +
                Application.ProductVersion +
#if MONO_BUILD
                " [Mono Build]" +
#endif
 Environment.NewLine + Environment.NewLine +
                "Written by Tim Stair" +
                Environment.NewLine + Environment.NewLine +
                "Enjoy!"
                , "About", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void pDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sHelpFile = CardMakerInstance.StartupPath + "Card_Maker.pdf";
            if (File.Exists(sHelpFile))
            {
                Process.Start(sHelpFile);
            }
        }

        private void samplePDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sSampleFile = CardMakerInstance.StartupPath + "Card_Maker_Basic_Project.pdf";
            if (File.Exists(sSampleFile))
            {
                Process.Start(sSampleFile);
            }
        }

        private void clearCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutManager.Instance.ClearImageCache();
        }

        private void illegalFilenameCharacterReplacementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zQuery = new QueryPanelDialog("Illegal File Name Character Replacement", 350, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            var arrayBadChars = FilenameTranslator.DISALLOWED_FILE_CHARS_ARRAY;
            var arrayReplacementChars = CardMakerSettings.IniManager.GetValue(IniSettings.ReplacementChars, string.Empty).Split(new char[] { CardMakerConstants.CHAR_FILE_SPLIT });
            if (arrayReplacementChars.Length == FilenameTranslator.DISALLOWED_FILE_CHARS_ARRAY.Length)
            {
                // from ini
                for (int nIdx = 0; nIdx < arrayBadChars.Length; nIdx++)
                {
                    zQuery.AddTextBox(arrayBadChars[nIdx].ToString(CultureInfo.InvariantCulture), arrayReplacementChars[nIdx], false, nIdx.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                // default
                for (int nIdx = 0; nIdx < arrayBadChars.Length; nIdx++)
                {
                    zQuery.AddTextBox(arrayBadChars[nIdx].ToString(CultureInfo.InvariantCulture), string.Empty, false, nIdx.ToString(CultureInfo.InvariantCulture));
                }
            }
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var zBuilder = new StringBuilder();
                for (int nIdx = 0; nIdx < arrayBadChars.Length; nIdx++)
                {
                    zBuilder.Append(zQuery.GetString(nIdx.ToString(CultureInfo.InvariantCulture)) + CardMakerConstants.CHAR_FILE_SPLIT);
                }
                zBuilder.Remove(zBuilder.Length - 1, 1); // remove last char
                CardMakerSettings.IniManager.SetValue(IniSettings.ReplacementChars, zBuilder.ToString());
                RestoreReplacementChars();
            }
        }



        private void removeLayoutTemplatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string TEMPLATE = "template";
            var listItems = new List<string>();
            LayoutTemplateManager.Instance.LayoutTemplates.ForEach(x => listItems.Add(x.ToString()));

            var zQuery = new QueryPanelDialog("Remove Layout Templates", 450, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            zQuery.AddLabel("Select the templates to remove.", 20);
            zQuery.AddListBox("Templates", listItems.ToArray(), null, true, 240, TEMPLATE)
                .Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            zQuery.AllowResize();
            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                var arrayRemove = zQuery.GetIndices(TEMPLATE);
                if (0 == arrayRemove.Length)
                {
                    return;
                }
                var trimmedList = new List<LayoutTemplate>();
                int removalIdx = 0;
                List<LayoutTemplate> listOldTemplates = LayoutTemplateManager.Instance.LayoutTemplates;
                for (int nIdx = 0; nIdx < listOldTemplates.Count; nIdx++)
                {
                    if (removalIdx < arrayRemove.Length && nIdx == arrayRemove[removalIdx])
                    {
                        removalIdx++;
                        // delete failures are logged
                        LayoutTemplateManager.Instance.DeleteLayoutTemplate(CardMakerInstance.StartupPath, listOldTemplates[nIdx]);
                    }
                    else
                    {
                        trimmedList.Add(listOldTemplates[nIdx]);
                    }
                }
                LayoutTemplateManager.Instance.LayoutTemplates = trimmedList;
            }
        }

        private void projectManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ProjectManagerUI().ShowDialog(this);
        }

        private void reloadReferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutManager.Instance.RefreshActiveLayout();
        }

        private void updateGoogleCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoogleAuthManager.UpdateGoogleAuth(this);
        }

        private void clearGoogleCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DeleteGoogleCache() && LayoutManager.Instance.ActiveDeck != null)
            {
                LayoutManager.Instance.InitializeActiveLayout();
            }
        }

#endregion

#region Manager Events

        private void Export_Requested(object sender, ExportEventArgs args)
        {
            switch (args.ExportType)
            {
                case ExportType.Image:
                    ExportImages(false);
                    break;
                case ExportType.PDFSharp:
                    ExportViaPDFSharp(false);
                    break;
            }
        }

        private void GoogleAuthUpdate_Requested(object sender, GoogleAuthEventArgs args)
        {
            GoogleAuthManager.UpdateGoogleAuth(this, args.SuccessAction, args.CancelAction);
        }


        void Project_Updated(object sender, ProjectEventArgs e)
        {
            if (e.DataChange)
            {
                MarkDirty();
            }
        }

        void Layout_Updated(object sender, LayoutEventArgs e)
        {
            if (e.DataChange)
            {
                MarkDirty();
            }
        }

        void Layout_Loaded(object sender, LayoutEventArgs e)
        {
            UserAction.ClearUndoRedoStacks();
        }

        private void Project_Opened(object sender, ProjectEventArgs e)
        {
            var sProjectFilePath = string.IsNullOrEmpty(e.ProjectFilePath) ? string.Empty : e.ProjectFilePath;
            SetLoadedFile(sProjectFilePath);
        }

#endregion

#region AbstractDirtyForm overrides

        protected override bool SaveFormData(string sFileName)
        {
            var bSaved = ProjectManager.Instance.Save(sFileName);
            if (bSaved)
            {
                UpdateProjectsList(sFileName);
            }
            return bSaved;
        }

        protected override bool OpenFormData(string sFileName)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ProjectManager.Instance.OpenProject(sFileName);
            }
            catch (Exception ex)
            {
                FormUtils.ShowErrorMessage("Failed to load: " + sFileName + "::" + ex);
            }
            Cursor = Cursors.Default;
            if (null != ProjectManager.Instance.LoadedProject)
            {
                UpdateProjectsList(sFileName);

                bool bHasExternalReference = ProjectManager.Instance.LoadedProject.HasExternalReference();

                if (bHasExternalReference)
                {
                    GoogleAuthManager.UpdateGoogleAuth(this, null, () =>
                    {
                        MessageBox.Show(this, "You will be unable to view the layouts for any references that are Google Spreadsheets.", "Reference Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                }

                return true;
            }
            return false;
        }

#endregion

        private void ExportViaPDFSharp(bool bExportAllLayouts)
        {
            var zQuery = new QueryPanelDialog("Export to PDF (via PDFSharp)", 750, false);
            zQuery.SetIcon(Icon);
            const string ORIENTATION = "orientation";
            const string OUTPUT_FILE = "output_file";
            const string OPEN_ON_EXPORT = "open_on_export";

            zQuery.AddPullDownBox("Page Orientation", 
                new string[]
                {
                    PageOrientation.Portrait.ToString(),
                    PageOrientation.Landscape.ToString()
                },
                m_nPdfExportLastOrientationIndex,
                ORIENTATION);

            zQuery.AddFileBrowseBox("Output File", m_sPdfExportLastFile, "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*", OUTPUT_FILE);
            zQuery.AddCheckBox("Open PDF on Export", m_bPdfExportLastOpen, OPEN_ON_EXPORT);

            if (DialogResult.OK != zQuery.ShowDialog(this))
            {
                return;
            }

            var nStartLayoutIdx = 0;
            var nEndLayoutIdx = ProjectManager.Instance.LoadedProject.Layout.Length;
            if (!bExportAllLayouts)
            {
                int nIdx = ProjectManager.Instance.GetLayoutIndex(LayoutManager.Instance.ActiveLayout);
                if (-1 == nIdx)
                {
                    FormUtils.ShowErrorMessage("Unable to determine the current layout. Please select a layout in the tree view and try again.");
                    return;
                }
                nStartLayoutIdx = nIdx;
                nEndLayoutIdx = nIdx + 1;
            }

            m_sPdfExportLastFile = zQuery.GetString(OUTPUT_FILE);
            m_bPdfExportLastOpen = zQuery.GetBool(OPEN_ON_EXPORT);
            m_nPdfExportLastOrientationIndex = zQuery.GetIndex(ORIENTATION);

            if (!m_sPdfExportLastFile.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase))
            {
                m_sPdfExportLastFile += ".pdf";
            }

            var zFileCardExporter = new PdfSharpExporter(nStartLayoutIdx, nEndLayoutIdx, m_sPdfExportLastFile, zQuery.GetString(ORIENTATION));

            var zWait = new WaitDialog(
                2,
                zFileCardExporter.ExportThread,
                "Export",
                new string[] { "Layout", "Card" },
                450);
#if true
            zWait.ShowDialog(this);
#else
            zFileCardExporter.ExportThread();
#endif

            if (zWait.ThreadSuccess && 
                m_bPdfExportLastOpen &&
                File.Exists(m_sPdfExportLastFile))
            {
                Process.Start(m_sPdfExportLastFile);
            }
        }

        private void ExportImages(bool bExportAllLayouts)
        {
            ICardExporter zFileCardExporter = FileCardExporterFactory.BuildFileCardExporter(bExportAllLayouts);
            if (null == zFileCardExporter)
            {
                return;
            }

#if true
            var zWait = new WaitDialog(
                2,
                zFileCardExporter.ExportThread,
                "Export",
                new string[] { "Layout", "Card" },
                450);
            zWait.ShowDialog(this);
#else // non threaded
            zFileCardExporter.ExportThread();
#endif
        }



        private void RestoreReplacementChars()
        {
            string[] arrayReplacementChars = CardMakerSettings.IniManager.GetValue(IniSettings.ReplacementChars, string.Empty).Split(new char[] { CardMakerConstants.CHAR_FILE_SPLIT });
            if (arrayReplacementChars.Length == FilenameTranslator.DISALLOWED_FILE_CHARS_ARRAY.Length)
            {
                FilenameTranslator.IllegalCharReplacementArray = arrayReplacementChars;
            }
            else
            {
                Logger.AddLogLine("Note: No replacement chars have been configured.");
            }
        }

        private Form SetupMDIForm(Form zForm, bool bDefaultShow)
        {
            zForm.MdiParent = this;
            bool bShow;
            if (!bool.TryParse(
                    CardMakerSettings.IniManager.GetValue(zForm.Name + CardMakerConstants.VISIBLE_SETTING,
                        bDefaultShow.ToString()), out bShow))
            {
                bShow = bDefaultShow;
            }
            if (bShow)
            {
                zForm.Show();
            }
            return zForm;
        }

        private void UpdateProjectsList(string sFileName)
        {
            m_listRecentFiles.Remove(sFileName);
            m_listRecentFiles.Insert(0, sFileName);
            while (CardMakerConstants.MAX_RECENT_PROJECTS < m_listRecentFiles.Count)
            {
                m_listRecentFiles.RemoveAt(CardMakerConstants.MAX_RECENT_PROJECTS);
            }
        }

        private bool DeleteGoogleCache()
        {
            try
            {
                File.Delete(Path.Combine(CardMakerInstance.StartupPath, CardMakerConstants.GOOGLE_CACHE_FILE));
                Logger.AddLogLine("Cleared Google Cache");
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Failed to delete Google Cache File: {0}".FormatString(ex.Message));
            }
            return false;
        }
    }
}
