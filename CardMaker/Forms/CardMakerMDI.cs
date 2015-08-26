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

using System.Diagnostics;
using System.Linq;
using CardMaker.Card;
using CardMaker.Card.Export;
using CardMaker.Card.Shapes;
using CardMaker.XML;
using PdfSharp;
using Support.IO;
using Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CardMaker.XML.Managers;

namespace CardMaker.Forms
{
    public partial class CardMakerMDI : AbstractDirtyForm
    {
        private const string VISIBLE_SETTING = ".visible";
        public const string GOOGLE_REFERENCE = "google";
        public const char GOOGLE_REFERENCE_SPLIT_CHAR = ';';
        public const string GOOGLE_AUTH_URL = "https://www.nhmk.com/cardmaker_oauth2_v2_request.php";

        const string LAYOUT_TEMPLATE_FILE = "layout_templates.xml";
        const char CHAR_FILE_SPLIT = '|';
        const int MAX_RECENT_PROJECTS = 10;

        private static string s_sLoadedProjectPath;

        private string m_sLoadedProjectFile;
        private Project m_zLoadedProject;
        private CardCanvas m_zDrawCardCanvas;
        private int m_nDestinationCardIndex = -1; 
        private readonly IniManager m_zIniManager = new IniManager("cardmaker", false, true, true);

        private float m_nApplicationDPI = 72f;

        private readonly List<string> m_listRecentFiles = new List<string>();

        // last export settings for pdf
        private string m_sPdfExportLastFile = "";
        private bool m_bPdfExportLastOpen = false;
        private int m_nPdfExportLastOrientationIndex = 0;

        #region Properties

        public static string ProjectPath
        {
            get
            {
                return s_sLoadedProjectPath;
            }
        }

        public static bool GoogleCredentialsInvalid { get; set; }

        public static string GoogleAccessToken { get; set; }

        public static string StartupPath
        {
            get
            {
                return Application.StartupPath + Path.DirectorySeparatorChar;
            }
        }

        // flag for indicating that a user is undoing/redoing 
        public static bool ProcessingUserAction { get; set; }

        public CardCanvas DrawCardCanvas
        {
            get
            {
                return m_zDrawCardCanvas;
            }
        }

        public Project LoadedProject
        {
            get
            {
                return m_zLoadedProject;
            }
        }

        public float ApplicationDPI
        {
            get { return m_nApplicationDPI; }
        }

        public static CardMakerMDI Instance { get; private set; }

        #endregion

        public CardMakerMDI()
        {
            InitializeComponent();

            ProcessingUserAction = false;
            UserAction.OnClearUserActions = () => Logger.AddLogLine("Cleared Undo/Redo.");

            m_sBaseTitle = "Card Maker Beta " + Application.ProductVersion;
            m_sFileOpenFilter = "CMP files (*.cmp)|*.cmp|All files (*.*)|*.*";

            Icon = Properties.Resources.CardMakerIcon;

            Instance = this;
        }

        #region Events

        private void SetupMDIForm(Form zForm, bool bDefaultShow)
        {
            zForm.MdiParent = this;
            var bShow = bDefaultShow;
            bool.TryParse(m_zIniManager.GetValue(zForm.Name + VISIBLE_SETTING, bDefaultShow.ToString()), out bShow);
            if (bShow)
            {
                zForm.Show();
            }
        }

        private void CardMakerMDI_Load(object sender, EventArgs e)
        {
            // always before any dialogs
            ShapeManager.Init();

            // Setup all the child dialogs
            SetupMDIForm(MDICanvas.Instance, true);
            SetupMDIForm(MDIElementControl.Instance, true);
            SetupMDIForm(MDILayoutControl.Instance, true);
            SetupMDIForm(MDILogger.Instance, true);
            SetupMDIForm(MDIProject.Instance, true);
            SetupMDIForm(MDIIssues.Instance, false);
            SetupMDIForm(MDIDefines.Instance, false);

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

            // create the main drawing canvas
            m_zDrawCardCanvas = MDICanvas.Instance.CardCanvas;

            // make a new project by default

            newToolStripMenuItem_Click(sender, e);

            var sData = m_zIniManager.GetValue(Name);
            var bRestoredFormState = false;
            if (!string.IsNullOrEmpty(sData))
            {
                IniManager.RestoreState(this, sData);
                bRestoredFormState = true;
            }
            foreach (var zForm in MdiChildren)
            {
                sData = m_zIniManager.GetValue(zForm.Name);
                if (!string.IsNullOrEmpty(sData))
                {
                    IniManager.RestoreState(zForm, sData);
                }
            }

            if (!bRestoredFormState)
            {
                Logger.AddLogLine("Restored default form layout.");
#if MONO_BUILD
                MDICanvas.Instance.Size = new Size(457, 300);
                MDICanvas.Instance.Location = new Point(209, 5);
                MDIElementControl.Instance.Size = new Size(768, 379);
                MDIElementControl.Instance.Location = new Point(3, 310);
                MDILayoutControl.Instance.Size = new Size(300, 352);
                MDILayoutControl.Instance.Location = new Point(805, 4);
                MDIProject.Instance.Size = new Size(200, 266);
                MDIProject.Instance.Location = new Point(6, 10);
                MDILogger.Instance.Size = new Size(403, 117);
                MDILogger.Instance.Location = new Point(789, 571);
#else
                MDICanvas.Instance.Size = new Size(457, 300);
                MDICanvas.Instance.Location = new Point(209, 5);
                MDIElementControl.Instance.Size = new Size(579, 290);
                MDIElementControl.Instance.Location = new Point(3, 310);
                MDILayoutControl.Instance.Size = new Size(300, 352);
                MDILayoutControl.Instance.Location = new Point(670, 4);
                MDIProject.Instance.Size = new Size(200, 266);
                MDIProject.Instance.Location = new Point(6, 10);
                MDILogger.Instance.Size = new Size(403, 117);
                MDILogger.Instance.Location = new Point(667, 531);
#endif
            }

            var arrayFiles = m_zIniManager.GetValue(IniSettings.PreviousProjects).Split(new char[] { CHAR_FILE_SPLIT }, StringSplitOptions.RemoveEmptyEntries);
            if (0 < arrayFiles.Length)
            {
                foreach (var sFile in arrayFiles)
                {
                    m_listRecentFiles.Add(sFile);
                }
            }
            LayoutTemplateManager.Instance.LoadLayoutTemplates(StartupPath);

            RestoreReplacementChars();

            var zGraphics = CreateGraphics();
            try
            {
                m_nApplicationDPI = zGraphics.DpiX;
            }
            finally
            {
                zGraphics.Dispose();
            }


            // load the specified project from the command line
            if (!string.IsNullOrEmpty(Program.CommandLineProjectFile))
                InitOpen(Program.CommandLineProjectFile);
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
            m_zDrawCardCanvas.CardRenderer.DrawElementBorder = drawElementBordersToolStripMenuItem.Checked;
            m_zDrawCardCanvas.Invalidate();
        }

        private void drawFormattedTextWordBordersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawFormattedTextWordOutlinesToolStripMenuItem.Checked = !drawFormattedTextWordOutlinesToolStripMenuItem.Checked;
            m_zDrawCardCanvas.CardRenderer.DrawFormattedTextBorder = drawFormattedTextWordOutlinesToolStripMenuItem.Checked;
            m_zDrawCardCanvas.Invalidate();
        }

        private void CardMakerMDI_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_zIniManager.AutoFlush = false;
            m_zIniManager.SetValue(Name, IniManager.GetFormSettings(this));
            foreach (Form zForm in MdiChildren)
            {
                m_zIniManager.SetValue(zForm.Name, IniManager.GetFormSettings(zForm));
                m_zIniManager.SetValue(zForm.Name + VISIBLE_SETTING, zForm.Visible.ToString());
            }
            var zBuilder = new StringBuilder();
            var dictionaryFilenames = new Dictionary<string, object>();
            foreach (string sFile in m_listRecentFiles)
            {
                string sLowerFile = sFile.ToLower();
                if (dictionaryFilenames.ContainsKey(sLowerFile))
                    continue;
                dictionaryFilenames.Add(sLowerFile, null);
                zBuilder.Append(sFile + CHAR_FILE_SPLIT);
            }
            m_zIniManager.SetValue(IniSettings.PreviousProjects, zBuilder.ToString());
            m_zIniManager.FlushIniSettings();
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
                m_zLoadedProject = Core.LoadProject(null);
                MDIProject.Instance.ResetTreeToProject(m_zLoadedProject);
                SetLoadedProjectFile(null);
                // reset the currently loaded file in the AbstractDirtyForm
                SetLoadedFile(string.Empty);

                ResetToNoLayout();
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
            string sFile = zItem.Text;
            InitOpen(sFile);
        }

        #endregion

        #region Support Methods

        public static string FileOpenHandler(string sFilter, TextBox zText, bool bCheckFileExists)
        {
            var ofd = new OpenFileDialog
            {
                Filter = sFilter,
                CheckFileExists = bCheckFileExists
            };
            if (DialogResult.OK == ofd.ShowDialog())
            {
                if (null != zText)
                {
                    zText.Text = ofd.FileName;
                }
                return ofd.FileName;
            }
            return null;
        }

        private void SetLoadedProjectFile(string sProjectFile)
        {
            m_sLoadedProjectFile = sProjectFile;
            s_sLoadedProjectPath = string.IsNullOrEmpty(m_sLoadedProjectFile) ? null : (Path.GetDirectoryName(sProjectFile) + Path.DirectorySeparatorChar);
        }

        public void SelectLayoutCardElement(int nLayout, int nCard, string sElement)
        {
            m_nDestinationCardIndex = nCard;

            // block any bad layout index requests
            if (MDIProject.Instance.ProjectTreeView.Nodes[0].Nodes.Count > nLayout)
            {
                if (MDIProject.Instance.ProjectTreeView.SelectedNode != MDIProject.Instance.ProjectTreeView.Nodes[0].Nodes[nLayout])
                {
                    MDIProject.Instance.ProjectTreeView.SelectedNode = MDIProject.Instance.ProjectTreeView.Nodes[0].Nodes[nLayout];
                }
                else
                {
                    // if the node is the same the assignment does not trigger anything, do the update manually
                    UpdateProjectLayoutTreeNode();
                }
                MDILayoutControl.Instance.ChangeSelectedElement(sElement);
            }
        }

        public void UpdateProjectLayoutTreeNode()
        {
            MDIProject.Instance.UpdateSelectedNodeLayoutColor(); // sets up the selected project-layout node

            if (null == MDIProject.Instance.GetCurrentProjectLayout())
                return;

            m_zDrawCardCanvas.SetCardLayout(MDIProject.Instance.GetCurrentProjectLayout());

            // disable event firing while configuring the UI
            MDILayoutControl.Instance.FireElementChangeEvents = false;

            MDILayoutControl.Instance.UpdateLayoutInfo();

            // restore event firing
            MDILayoutControl.Instance.FireElementChangeEvents = true;

            if (-1 != m_nDestinationCardIndex)
            {
                MDILayoutControl.Instance.SetSelectedCardIndex(m_nDestinationCardIndex);
                m_nDestinationCardIndex = -1;
            }
            else
            {
                MDILayoutControl.Instance.ChangeCardIndex(0);
            }
            UserAction.ClearUndoRedoStacks();

            MDIDefines.Instance.UpdateDefines();
        }

        public void ShowErrorMessage(string sMessage)
        {
            MessageBox.Show(this, sMessage, "CardMaker Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void DrawCurrentCardIndex()
        {
            // update the control size
            m_zDrawCardCanvas.UpdateSize();
            // todo optimize with rectangle invalidation
            m_zDrawCardCanvas.Invalidate();
        }

        #endregion

        #region AbstractDirtyForm overrides

        protected override bool SaveFormData(string sFileName)
        {
            if (m_zLoadedProject.Save(sFileName, m_sLoadedProjectFile))
            {
                SetLoadedProjectFile(sFileName);
                return true;
            }
            return false;
        }

        protected override bool OpenFormData(string sFileName)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                m_zLoadedProject = Core.LoadProject(sFileName);
                MDIProject.Instance.ResetTreeToProject(m_zLoadedProject);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Failed to load: " + sFileName + "::" + ex);
            }
            Cursor = Cursors.Default;
            if (null != m_zLoadedProject)
            {
                SetLoadedProjectFile(sFileName);
                ResetToNoLayout();
                m_listRecentFiles.Remove(sFileName);
                m_listRecentFiles.Insert(0, sFileName);
                while (MAX_RECENT_PROJECTS < m_listRecentFiles.Count)
                {
                    m_listRecentFiles.RemoveAt(MAX_RECENT_PROJECTS);
                }

                bool bHasExternalReference = m_zLoadedProject.HasExternalReference();

                if (bHasExternalReference)
                {
                    UpdateGoogleAuth(null, () =>
                    {
                        MessageBox.Show(this, "You will be unable to view the layouts for any references that are Google Spreadsheets.", "Reference Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                }

                return true;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Resets the application to a state where no Layout is selected
        /// </summary>
        private void ResetToNoLayout()
        {
            // must be called first (wipes out the selected Layout)
            MDIProject.Instance.ResetCurrentProjectLayout();

            MDICanvas.Instance.Reset();

            MDILayoutControl.Instance.UpdateLayoutInfo();
            MDIElementControl.Instance.UpdateElementValues(null);
        }

        private void exportProjectToPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportViaPDFSharp(true);
        }

        public void ExportViaPDFSharp(bool bExportAllLayouts)
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
            var nEndLayoutIdx = MDIProject.Instance.LayoutCount;
            if (!bExportAllLayouts)
            {
                int nIdx = MDIProject.Instance.GetCurrentLayoutIndex();
                if (-1 == nIdx)
                {
                    ShowErrorMessage("Unable to determine the current layout. Please select a layout in the tree view and try again.");
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
            zWait.ShowDialog(this);

            if (zWait.ThreadSuccess && 
                m_bPdfExportLastOpen &&
                File.Exists(m_sPdfExportLastFile))
            {
                Process.Start(m_sPdfExportLastFile);
            }
        }

        private void exportImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportImages(true);
        }

        public void ExportImages(bool bExportAllLayouts)
        {
            var zQuery = new QueryPanelDialog("Export to Images", 750, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            const string FORMAT = "FORMAT";
            const string NAME_FORMAT = "NAME_FORMAT";
            const string NAME_FORMAT_LAYOUT_OVERRIDE = "NAME_FORMAT_LAYOUT_OVERRIDE";
            const string FOLDER = "FOLDER";
            var arrayImageFormats = new ImageFormat[] { 
                ImageFormat.Bmp,
                ImageFormat.Emf,
                ImageFormat.Exif,
                ImageFormat.Gif,
                ImageFormat.Icon,
                ImageFormat.Jpeg,
                ImageFormat.Png,
                ImageFormat.Tiff,
                ImageFormat.Wmf
            };
            var arrayImageFormatStrings = new string[arrayImageFormats.Length];
            for (int nIdx = 0; nIdx < arrayImageFormats.Length; nIdx++)
            {
                arrayImageFormatStrings[nIdx] = arrayImageFormats[nIdx].ToString();
            }


            var nDefaultFormatIndex = 0;
            var lastImageFormat = m_zIniManager.GetValue(IniSettings.LastImageExportFormat, string.Empty);
            // TODO: .NET 4.x offers enum.parse... when the project gets to that version
            if (lastImageFormat != string.Empty)
            {
                for (int nIdx = 0; nIdx < arrayImageFormats.Length; nIdx++)
                {
                    if (arrayImageFormats[nIdx].ToString().Equals(lastImageFormat))
                    {
                        nDefaultFormatIndex = nIdx;
                        break;
                    }
                }
            }

            zQuery.AddPullDownBox("Format", arrayImageFormatStrings, nDefaultFormatIndex, FORMAT);

            var sDefinition = m_zLoadedProject.exportNameFormat; // default to the project level definition
            if (!bExportAllLayouts)
            {
                sDefinition = MDIProject.Instance.GetCurrentProjectLayout().exportNameFormat;
            }
            else
            {
                zQuery.AddCheckBox("Override Layout File Name Formats", false, NAME_FORMAT_LAYOUT_OVERRIDE);
            }

            zQuery.AddTextBox("File Name Format (optional)", sDefinition ?? string.Empty, false, NAME_FORMAT);

            if (bExportAllLayouts)
            {
                // associated check box and the file format text box
                zQuery.AddEnableControl(NAME_FORMAT_LAYOUT_OVERRIDE, NAME_FORMAT);

            }
            zQuery.AddFolderBrowseBox("Output Folder", Directory.Exists(m_zLoadedProject.lastExportPath) ? m_zLoadedProject.lastExportPath : string.Empty, FOLDER);
            
            zQuery.UpdateEnableStates();

            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                string sFolder = zQuery.GetString(FOLDER);
                if (!Directory.Exists(sFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(sFolder);
                    }
                    catch (Exception)
                    {
                    }
                }
                if (Directory.Exists(sFolder))
                {
                    m_zLoadedProject.lastExportPath = sFolder;
                    var nStartLayoutIdx = 0;
                    var nEndLayoutIdx = MDIProject.Instance.LayoutCount;
                    var bOverrideLayout = false;
                    if (!bExportAllLayouts)
                    {
                        int nIdx = MDIProject.Instance.GetCurrentLayoutIndex();
                        if (-1 == nIdx)
                        {
                            ShowErrorMessage("Unable to determine the current layout. Please select a layout in the tree view and try again.");
                            return;
                        }
                        nStartLayoutIdx = nIdx;
                        nEndLayoutIdx = nIdx + 1;
                    }
                    else
                    {
                        bOverrideLayout = zQuery.GetBool(NAME_FORMAT_LAYOUT_OVERRIDE);
                    }

                    m_zIniManager.SetValue(IniSettings.LastImageExportFormat, arrayImageFormats[zQuery.GetIndex(FORMAT)].ToString());

                    ICardExporter zFileCardExporter = new FileCardExporter(nStartLayoutIdx, nEndLayoutIdx, sFolder, bOverrideLayout, zQuery.GetString(NAME_FORMAT), 
                        arrayImageFormats[zQuery.GetIndex(FORMAT)]);
#if true
                    var zWait = new WaitDialog(
                        2,
                        zFileCardExporter.ExportThread,
                        "Export",
                        new string[] { "Layout", "Card" },
                        450);
                    zWait.ShowDialog(this);
#else // non threaded
                    ExportImagesThread(zThreadObject);
#endif
                }
                else
                {
                    ShowErrorMessage("The folder specified does not exist!");
                }
            }
        }
        
        private void updateIssuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitSave(false);
            if (Dirty)
            {
                return;
            }

            MDIIssues.Instance.ClearIssues();
            MDIIssues.Instance.TrackIssues = true;

            var zWait = new WaitDialog(
                2,
                new CompilerCardExporter(0, MDIProject.Instance.LayoutCount).ExportThread,
                "Compile",
                new string[] { "Layout", "Card" },
                450);
            zWait.ShowDialog(this);

            MDIIssues.Instance.TrackIssues = false;
            MDIIssues.Instance.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Card Maker Beta" + 
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
            string sHelpFile = StartupPath + "Card_Maker.pdf";
            if(File.Exists(sHelpFile))
                System.Diagnostics.Process.Start(sHelpFile);
        }

        private void samplePDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sSampleFile = StartupPath + "Card_Maker_Basic_Project.pdf";
            if (File.Exists(sSampleFile))
                System.Diagnostics.Process.Start(sSampleFile);
        }

        private void clearCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DrawItem.DumpImages();
            DrawItem.DumpOpacityImages();
            m_zDrawCardCanvas.Invalidate();
        }

        private void illegalFilenameCharacterReplacementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zQuery = new QueryPanelDialog("Illegal File Name Character Replacement", 350, false);
            zQuery.SetIcon(Properties.Resources.CardMakerIcon);
            var arrayBadChars = Deck.DISALLOWED_FILE_CHARS_ARRAY;
            var arrayReplacementChars = m_zIniManager.GetValue(IniSettings.ReplacementChars, string.Empty).Split(new char[] { CHAR_FILE_SPLIT });
            if (arrayReplacementChars.Length == Deck.DISALLOWED_FILE_CHARS_ARRAY.Length)
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
                    zBuilder.Append(zQuery.GetString(nIdx.ToString(CultureInfo.InvariantCulture)) + CHAR_FILE_SPLIT);
                }
                zBuilder.Remove(zBuilder.Length - 1, 1); // remove last char
                m_zIniManager.SetValue(IniSettings.ReplacementChars, zBuilder.ToString());
                RestoreReplacementChars();
            }
        }

        private void RestoreReplacementChars()
        {
            string[] arrayReplacementChars = m_zIniManager.GetValue(IniSettings.ReplacementChars, string.Empty).Split(new char[] { CHAR_FILE_SPLIT });
            if (arrayReplacementChars.Length == Deck.DISALLOWED_FILE_CHARS_ARRAY.Length)
            {
                Deck.IllegalCharReplacementArray = arrayReplacementChars;
            }
            else
            {
                Logger.AddLogLine("Note: No replacement chars have been configured.");
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
            zQuery.AddListBox("Templates", listItems.ToArray(), null, true, 120, TEMPLATE);
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
                        LayoutTemplateManager.Instance.DeleteLayoutTemplate(StartupPath, listOldTemplates[nIdx]);
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

        private void refreshLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshLayout(DrawCardCanvas.ActiveDeck.CardIndex + 1);
        }

        public void RefreshLayout(int destinationIndex = -1)
        {
            m_nDestinationCardIndex = destinationIndex;
            UpdateProjectLayoutTreeNode();
        }

        private void updateGoogleCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateGoogleAuth();
        }

        #region Google Auth

        public void UpdateGoogleAuth(Action zSuccessAction = null, Action zCancelAction = null)
        {
            var zDialog = new GoogleCredentialsDialog();
            zDialog.Icon = Icon;
            DialogResult zResult = zDialog.ShowDialog(this);
            switch (zResult)
            {
                case DialogResult.OK:
                    GoogleAccessToken = zDialog.GoogleAccessToken;
                    Logger.AddLogLine("Updated Google Credentials");
                    if (null != zSuccessAction) zSuccessAction();
                    break;
                default:
                    if (null != zCancelAction) zCancelAction();
                    break;
            }
        }

        #endregion


        public void OpenProjectFile(string sPath)
        {
            InitOpen(sPath);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProcessingUserAction)
                return;

            Action<bool> redoAction = UserAction.GetRedoAction();
            if (null != redoAction)
            {
                redoAction(true);
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProcessingUserAction)
                return;

            Action<bool> undoAction = UserAction.GetUndoAction();
            if (null != undoAction)
            {
                undoAction(false);
            }
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            undoToolStripMenuItem.Enabled = 0 < UserAction.UndoCount;
            redoToolStripMenuItem.Enabled = 0 < UserAction.RedoCount;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zQuery = new QueryPanelDialog("CardMaker Settings", 450, 250, true);
            zQuery.SetIcon(Instance.Icon);
            zQuery.SetMaxHeight(600);
            zQuery.AddTab("General");
            zQuery.AddCheckBox("Print/Export Layout Border", PrintLayoutBorder, IniSettings.PrintLayoutBorder);

            zQuery.AddTab("PDF Export");
            zQuery.AddNumericBox("Page Width (inches)", PrintPageWidth, 1, 1024, 1, 2, IniSettings.PrintPageWidth);
            zQuery.AddNumericBox("Page Height (inches)", PrintPageHeight, 1, 1024, 1, 2, IniSettings.PrintPageHeight);
            zQuery.AddNumericBox("Page Horizontal Margin (inches)", PrintPageHorizontalMargin, 0, 1024, 0.01m, 2, IniSettings.PrintPageHorizontalMargin);
            zQuery.AddNumericBox("Page Vertical Margin (inches)", PrintPageVerticalMargin, 0, 1024, 0.01m, 2, IniSettings.PrintPageVerticalMargin);
            zQuery.AddCheckBox("Auto-Center Layouts on Page", PrintAutoHorizontalCenter, IniSettings.PrintAutoCenterLayout);
            zQuery.AddCheckBox("Print Layouts On New Page", PrintLayoutsOnNewPage, IniSettings.PrintLayoutsOnNewPage);
            zQuery.SetIcon(Icon);

            if (DialogResult.OK == zQuery.ShowDialog(this))
            {
                PrintPageWidth = zQuery.GetDecimal(IniSettings.PrintPageWidth);
                PrintPageHeight = zQuery.GetDecimal(IniSettings.PrintPageHeight);
                PrintPageHorizontalMargin = zQuery.GetDecimal(IniSettings.PrintPageHorizontalMargin);
                PrintPageVerticalMargin = zQuery.GetDecimal(IniSettings.PrintPageVerticalMargin);
                PrintAutoHorizontalCenter = zQuery.GetBool(IniSettings.PrintAutoCenterLayout);
                PrintLayoutBorder = zQuery.GetBool(IniSettings.PrintLayoutBorder);
                PrintLayoutsOnNewPage = zQuery.GetBool(IniSettings.PrintLayoutsOnNewPage);
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
                zProject = Core.LoadProject(ofd.FileName);
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Error Loading Project File: " + ex.ToString());
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
                listIndices.ForEach(idx =>
                {
                    Core.InitializeElementCache(zProject.Layout[idx]);
                    MDIProject.Instance.AddProjectLayout(zProject.Layout[idx], LoadedProject);
                });
                MarkDirty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the layout needs to be reloaded</returns>
        public bool HandleInvalidGoogleCredentials()
        {
            if (GoogleCredentialsInvalid)
            {
                GoogleCredentialsInvalid = false;
                CardMakerMDI.Instance.UpdateGoogleAuth(new Action(() => RefreshLayout()));
                return true;
            }
            return false;
        }
    }
}
