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
using System.Windows.Forms;
using CardMaker.Card.FormattedText;
using CardMaker.Card.Import;
using CardMaker.Card.Translation;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;
using Support.UI;

#if MONO_BUILD
using System.Threading;
#endif

namespace CardMaker.Card
{
    public class Deck
    {
        public const string DEFINES_DATA_POSTFIX = "_defines";

        protected int m_nCardIndex = -1;
        protected int m_nCardPrintIndex;

        public ProjectLayout CardLayout { get; protected set; }

        protected  ITranslatorFactory TranslatorFactory { get; set; }

        protected TranslatorBase m_zTranslator;

        public List<DeckLine> ValidLines { get; }

        public Dictionary<string, string> Defines => m_zTranslator.DictionaryDefines;

        public int CardIndex
        {
            get
            {
                return m_nCardIndex;
            }
            set
            {
                if (value >= ValidLines.Count || value < 0)
                {
                    return;
                }
                ResetDeckCache();
                m_nCardIndex = value;
            }
        }

        // NOTE - the CardPrintIndex is critical to printing as the value is allowed to equal ValidLines.Count unlike CardIndex (necessary for layout transition etc.)
        public int CardPrintIndex
        {
            get
            {
                return m_nCardPrintIndex;
            }
            set
            {
                if (value > ValidLines.Count || value < 0)
                {
                    return;
                }
                ResetDeckCache();
                m_nCardPrintIndex = value;
            }
        }

        public DeckLine CurrentPrintLine => ValidLines[m_nCardPrintIndex];

        public DeckLine CurrentLine => ValidLines[m_nCardIndex];

        public int CardCount => ValidLines.Count;

        protected void ResetPrintCardIndex()
        {
            m_nCardPrintIndex = 0;
        }

        public Deck()
        {
            ValidLines = new List<DeckLine>();
            TranslatorFactory = new TranslatorFactory();
        }

        protected void ReadData(object zRefData)
        {
            try
            {
                ReadDataCore(zRefData);
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Failed to read data file(s). " + ex.Message);
                WaitDialog.Instance.ThreadSuccess = false;
                WaitDialog.Instance.CloseWaitDialog();
            }
        }

        private void ReadDataCore(object zRefData)
        {
            var zReferenceData = zRefData as ProjectLayoutReference[];

            var listLines = new List<List<string>>();
            var listDefineLines = new List<List<string>>();

            ReferenceReader zRefReader = null;

            if (null == zReferenceData || zReferenceData.Length == 0)
            {
                ReadDefaultProjectDefinitions(listDefineLines);
            }
            else
            {
                WaitDialog.Instance.ProgressReset(0, 0, zReferenceData.Length * 2 + 1, 0);

                for(int nIdx = 0; nIdx < zReferenceData.Length; nIdx++)
                {
                    var zReference = zReferenceData[nIdx];
                    var listRefLines = new List<List<string>>();
                    zRefReader = ReferenceReaderFactory.GetReader(zReference);

                    if (zRefReader == null)
                    {
                        listLines.Clear();
                        Logger.AddLogLine($"Failed to load reference: {zReference.RelativePath}");
                        break;
                    }
                    // 0 index is always the default reference in the case of multi load
                    if (nIdx == 0)
                    {
                        if (!string.IsNullOrEmpty(ProjectManager.Instance.ProjectFilePath))
                        {
                            zRefReader.GetProjectDefineData(zReference, listDefineLines);
                            if (listDefineLines.Count == 0)
                            {
                                Logger.AddLogLine(
                                    "No defines found for project file: {0}".FormatString(
                                        ProjectManager.Instance.ProjectFilePath));
                            }
                        }
                        else
                        {
                            Logger.AddLogLine("No defines loaded for project -- project not yet saved.");
                        }
                        WaitDialog.Instance.ProgressStep(0);
                    }

                    zRefReader.GetReferenceData(zReference, listRefLines);
                    if (listLines.Count > 0 && listRefLines.Count > 0)
                    {
                        // remove the columns row from any non-zero index references
                        listRefLines.RemoveAt(0);
                    }
                    listLines.AddRange(listRefLines);
                    WaitDialog.Instance.ProgressStep(0);

                    var nPriorCount = listDefineLines.Count;
                    zRefReader.GetDefineData(zReference, listDefineLines);
                    if (listDefineLines.Count == nPriorCount)
                    {
                        Logger.AddLogLine(
                            "No defines found for reference: {0}".FormatString(zReference.RelativePath));
                    }

                    zRefReader.FinalizeReferenceLoad();
                    WaitDialog.Instance.ProgressStep(0);                       
                }
            }

            ProcessLines(listLines, listDefineLines, zRefReader?.ReferencePath);
        }

        protected void ProcessLines(List<List<string>> listLines, 
            List<List<string>> listDefineLines,
            string sReferencePath)
        {
#warning this method is horribly long

            const string ALLOWED_LAYOUT = "allowed_layout";
            const string OVERRIDE = "override:";

            var nDefaultCount = CardLayout.defaultCount;
            var listColumnNames = new List<string>();
            var dictionaryColumnNames = new Dictionary<string, int>();
            var dictionaryElementOverrides = new Dictionary<string, Dictionary<string, int>>();
            var dictionaryDefines = new Dictionary<string, string>();
            
            // Line Processing
            if (0 < listLines.Count)
            {
                // Read the column names
                var listAllColumnNames = listLines[0];
                for (int nIdx = 0; nIdx < listAllColumnNames.Count; nIdx++)
                {
                    string sKey = listAllColumnNames[nIdx].ToLower().Trim();
                    listColumnNames.Add(sKey);
                    if (!dictionaryColumnNames.ContainsKey(sKey))
                    {
                        dictionaryColumnNames.Add(sKey, nIdx);
                    }
                    else
                    {
                        IssueManager.Instance.FireAddIssueEvent("Duplicate column found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey);
                        Logger.AddLogLine("Duplicate column found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey);
                    }
                }

                // determine the allowed layout column index
                int nAllowedLayoutColumn;
                if (!dictionaryColumnNames.TryGetValue(ALLOWED_LAYOUT, out nAllowedLayoutColumn))
                {
                    nAllowedLayoutColumn = -1;
                }

                // construct the override dictionary
                foreach (string sKey in listColumnNames)
                {
                    if (sKey.StartsWith(OVERRIDE))
                    {
                        string[] arraySplit = sKey.Split(new char[] { ':' });
                        if (3 == arraySplit.Length)
                        {
                            string sElementName = arraySplit[1].Trim();
                            string sElementItemOverride = arraySplit[2].Trim();
                            if (!dictionaryElementOverrides.ContainsKey(sElementName))
                            {
                                dictionaryElementOverrides.Add(sElementName, new Dictionary<string, int>());
                            }
                            Dictionary<string, int> dictionaryOverrides = dictionaryElementOverrides[sElementName];
                            if (dictionaryOverrides.ContainsKey(sElementItemOverride))
                            {
                                Logger.AddLogLine("Duplicate override found: {0}".FormatString(sElementItemOverride));
                            }
                            dictionaryOverrides[sElementItemOverride] = dictionaryColumnNames[sKey];
                        }
                    }
                }

                // remove the columns
                listLines.RemoveAt(0);

                // remove any lines that do not contain any values
                int nRow = 0;
                while(nRow < listLines.Count)
                {
                    var listRow = listLines[nRow];
                    var bEmptyRow = true;
                    foreach(string sCol in listRow)
                    {
                        if (0 < sCol.Trim().Length)
                        {
                            bEmptyRow = false;
                            break;
                        }
                    }
                    if (bEmptyRow)
                    {
                        listLines.RemoveAt(nRow);
                    }
                    else
                    {
                        nRow++;
                    }
                }

                // remove any layout elements that do not match the card layout
                if (-1 != nAllowedLayoutColumn)
                {
                    int nLine = 0;

                    while (nLine < listLines.Count)
                    {
                        if (!CardLayout.Name.Equals(listLines[nLine][nAllowedLayoutColumn], StringComparison.CurrentCultureIgnoreCase))
                        {
                            listLines.RemoveAt(nLine);
                        }
                        else
                        {
                            nLine++;
                        }
                    }
                }

                // indicate if no lines remain!
                if (0 == listLines.Count)
                {
                    IssueManager.Instance.FireAddIssueEvent("No lines found?! allowed_layout may be cutting them!");
                }

            }

            ValidLines.Clear();

            // create the duplicated lines (with count > 0)
            foreach (List<string> listItems in listLines)
            {
                if (0 < listItems.Count)
                {
                    int nNumber;
                    if (!int.TryParse(listItems[0].Trim(), out nNumber))
                    {
                        IssueManager.Instance.FireAddIssueEvent("Invalid card count found: [" + listItems[0] + "] The first column should always have a number value.");
                        nNumber = 1;
                    }
                    for (var nCount = 0; nCount < nNumber; nCount++)
                    {
                        ValidLines.Add(new DeckLine(listItems, nCount));
                    }
                }
            }
            // always create a line
            if (0 == ValidLines.Count && 0 < nDefaultCount)
            {
                // create the default number of lines.
                for (int nIdx = 0; nIdx < nDefaultCount; nIdx++)
                {
                    if (0 < listColumnNames.Count)// create each line and the correct number of columns
                    {
                        var arrayDefaultLine = new List<string>();
                        for (int nCol = 0; nCol < arrayDefaultLine.Count; nCol++)
                        {
                            arrayDefaultLine.Add(string.Empty);
                        }
                        ValidLines.Add(new DeckLine(arrayDefaultLine));
                    }
                    else // no columns just create an empty row
                    {
                        ValidLines.Add(new DeckLine(new List<string>()));
                    }
                }
                if (!string.IsNullOrEmpty(sReferencePath))
                {
                    IssueManager.Instance.FireAddIssueEvent("No lines found for this layout! Generated " + nDefaultCount);
                }
            }

            // Define Processing
            // remove the column names
            if (listDefineLines.Count > 0)
            {
                // the removal of the first row happens in the reader(s) for defines
//                listDefineLines.RemoveAt(0);
                foreach (var row in listDefineLines)
                {
                    if (row.Count > 1)
                    {
                        string sKey = row[0];
                        int nIdx;
                        string sVal;
                        if (dictionaryDefines.TryGetValue(sKey.ToLower(), out sVal))
                        {
                            string sMsg = "Duplicate define found: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            Logger.AddLogLine(sMsg);
                        }
                        else if (dictionaryColumnNames.TryGetValue(sKey.ToLower(), out nIdx))
                        {
                            string sMsg = "Overlapping column name and define found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            Logger.AddLogLine(sMsg);
                        }
                        else
                        {
                            dictionaryDefines.Add(sKey.ToLower(), row[1]);
                        }
                    }
                }
            }

            m_zTranslator = TranslatorFactory.GetTranslator(dictionaryColumnNames, dictionaryDefines, dictionaryElementOverrides, listColumnNames);

#if MONO_BUILD
            Thread.Sleep(100);
#endif
            WaitDialog.Instance.ThreadSuccess = true;
            WaitDialog.Instance.CloseWaitDialog();
        }

        /// <summary>
        /// Populates the project defines based on the default reference type. If nothing is found on Google then attempt the local CSV.
        /// </summary>
        /// <param name="listDefineLines"></param>
        private void ReadDefaultProjectDefinitions(List<List<string>> listDefineLines)
        {
            WaitDialog.Instance.ProgressReset(0, 0, 2, 0);
            ReferenceReader zRefReader;
            if (ReferenceType.Google == ProjectManager.Instance.LoadedProjectDefaultDefineReferenceType)
            {
                zRefReader = ReferenceReaderFactory.GetDefineReader(ReferenceType.Google);
                zRefReader?.GetProjectDefineData(null, listDefineLines);
            }
            WaitDialog.Instance.ProgressStep(0);
            // always attempt to load the local if nothing was pulled from google
            if (0 == listDefineLines.Count)
            {
                zRefReader = ReferenceReaderFactory.GetDefineReader(ReferenceType.CSV);
                zRefReader?.GetProjectDefineData(null, listDefineLines);
            }
            WaitDialog.Instance.ProgressStep(0);
        }

        public ElementString TranslateString(string sRawString, DeckLine zDeckLine, ProjectLayoutElement zElement, bool bPrint, string sCacheSuffix = "")
        {
            return m_zTranslator.TranslateString(this, sRawString, bPrint ? m_nCardPrintIndex : m_nCardIndex, zDeckLine, zElement, sCacheSuffix);
        }

        public ElementString GetStringFromTranslationCache(string sKey)
        {
            return m_zTranslator.GetStringFromTranslationCache(sKey);
        }

        public void ResetTranslationCache(ProjectLayoutElement zElement)
        {
            m_zTranslator.ResetTranslationCache(zElement);
        }

        #region Cache

        public FormattedTextDataCache GetCachedMarkup(string sElementName)
        {
            return m_zTranslator.GetCachedMarkup(sElementName);
        }

        #endregion

        public ProjectLayoutElement GetOverrideElement(ProjectLayoutElement zElement, DeckLine zDeckLine, bool bExport)
        {
            return m_zTranslator.GetOverrideElement(this, zElement, bExport ? m_nCardPrintIndex : m_nCardIndex, zDeckLine.LineColumns, zDeckLine);
        }

        public ProjectLayoutElement GetVariableOverrideElement(ProjectLayoutElement zElement, Dictionary<string, string> dictionaryOverrideFieldToValue)
        {
            return m_zTranslator.GetVariableOverrideElement(zElement, dictionaryOverrideFieldToValue);
        }

        /// <summary>
        /// Populates the specified ListView with the columns and data associated with this Deck
        /// </summary>
        /// <param name="listView">The ListView to operate on</param>
        public void PopulateListViewWithElementColumns(ListView listView)
        {
            var listColumnNames = m_zTranslator.ListColumnNames;
            var arrayColumnSizes = new int[listColumnNames.Count];
            for (var nCol = 0; nCol < listColumnNames.Count; nCol++)
            {
                arrayColumnSizes[nCol] = 100;
            }
            for (var nCol = 0; nCol < listView.Columns.Count && nCol < arrayColumnSizes.Length; nCol++)
            {
                arrayColumnSizes[nCol] = listView.Columns[nCol].Width;
            }

            listView.Columns.Clear();
            listView.Items.Clear();

            for (var nIdx = 1; nIdx < listColumnNames.Count; nIdx++)
            {
                ListViewAssist.AddColumn(listColumnNames[nIdx], arrayColumnSizes[nIdx], listView);
            }

            if (-1 != m_nCardIndex)
            {
                var listLines = CurrentLine.LineColumns;
                if (listLines.Count > 0)
                {
                    listView.Items.Add(new ListViewItem(listLines.GetRange(1, listLines.Count - 1).ToArray()));
                }
            }
        }

#region Cache General

        public void ResetDeckCache()
        {
            if (m_zTranslator == null)
            {
                Logger.AddLogLine("Warn: code attempted to clear cache on non-existent translator (this is a mostly harmless bug)");
                return;
            }
            m_zTranslator.ResetTranslationCache();
            m_zTranslator.ResetMarkupCache();
        }

        #endregion

        #region Markup Cache

        public void AddCachedMarkup(string sElementName, FormattedTextDataCache zFormattedData)
        {
            m_zTranslator.AddCachedMarkup(sElementName, zFormattedData);
        }

        public void ResetMarkupCache(string sElementName)
        {
            m_zTranslator.ResetMarkupCache(sElementName);
        }
        #endregion


        #region Layout Set
#warning todo: make a deck loader interface so this can be handled from the command line

        public void SetAndLoadLayout(ProjectLayout zLayout, bool bExporting)
        {
            CardLayout = zLayout;

            ResetPrintCardIndex();

            var bReferenceFound = false;

            if (null != CardLayout.Reference)
            {
                ProjectLayoutReference[] zReferenceData = null;

                if (CardLayout.combineReferences)
                {
                    var listReferences = new List<ProjectLayoutReference>();
                    ProjectLayoutReference zDefaultReference = null;
                    foreach (var zReference in CardLayout.Reference)
                    {
                        if (zReference.Default)
                        {
                            zDefaultReference = zReference;
                        }
                        else
                        {
                            listReferences.Add(zReference);
                        }
                    }
                    // move the default reference to the front of the set
                    if (null != zDefaultReference)
                    {
                        listReferences.Insert(0, zDefaultReference);
                    }
                    zReferenceData = listReferences.Count == 0 ? null : listReferences.ToArray();
                }
                else
                {
                    foreach (var zReference in CardLayout.Reference)
                    {
                        if (zReference.Default)
                        {
                            zReferenceData = new ProjectLayoutReference[] { zReference };
                            break;
                        }
                    }
                }
                var zWait = new WaitDialog(
                    1,
                    ReadData,
                    zReferenceData,
                    "Loading Data",
                    null,
                    400);
#warning this needs to be pulled into a deck loader
                CardMakerInstance.ApplicationForm.InvokeAction(() => zWait.ShowDialog(CardMakerInstance.ApplicationForm));
                if (!bExporting)
                {
                    if (CardMakerInstance.GoogleCredentialsInvalid)
                    {
                        CardMakerInstance.GoogleCredentialsInvalid = false;
                        GoogleAuthManager.Instance.FireGoogleAuthCredentialsErrorEvent(
                            () => LayoutManager.Instance.InitializeActiveLayout());
                    }
                }
                bReferenceFound = zWait.ThreadSuccess;
            }

            if (!bReferenceFound)
            {
                // setup the placeholder single card
                var zWait = new WaitDialog(
                    1,
                    ReadData,
                    null,
                    "Loading Data",
                    null,
                    400)
                {
                    CancelButtonVisibile = false
                };
                CardMakerInstance.ApplicationForm.InvokeAction(() => zWait.ShowDialog(CardMakerInstance.ApplicationForm));
            }
        }

        public string TranslateFileNameString(string sRawString, int nCardNumber, int nLeftPad)
        {
            return FilenameTranslator.TranslateFileNameString(sRawString, nCardNumber, nLeftPad, CurrentPrintLine,
                m_zTranslator.DictionaryDefines, m_zTranslator.DictionaryColumnNameToIndex,
                CardLayout);
        }

#endregion
    }
}
