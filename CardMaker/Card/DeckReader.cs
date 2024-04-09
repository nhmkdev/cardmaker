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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CardMaker.Card.Import;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.Progress;
using Support.UI;

#if MONO_BUILD
using System.Threading;
#endif

namespace CardMaker.Card
{
    public class DeckReader
    {
        private static readonly Regex s_regexColumnVariable = new Regex(@"(.*)(@\[)(.+?)(\])(.*)", RegexOptions.Compiled);

        protected ProgressReporterProxy m_zReporterProxy;
        private readonly Deck m_zDeck;
        private ReferenceReader m_zErrorReferenceReader;

        public DeckReader(Deck deck, ProgressReporterProxy reporterProxy)
        {
            m_zDeck = deck;
            m_zReporterProxy = reporterProxy;
        }

        protected void ReadData(object zRefData)
        {
            try
            {
                ReadDataCore(zRefData);
            }
            catch (Exception ex)
            {
                ReadDataCore(null);
                m_zReporterProxy.AddIssue("Failed to read data file(s). " + ex.Message);
                m_zReporterProxy.Shutdown();
            }
        }
        
        private void ReadDataCore(object zRefData)
        {
            var zReferenceData = zRefData as ProjectLayoutReference[];

            // these lists are accumulated across references
            var listAllReferenceLines = new List<ReferenceLine>();
            var listAllReferenceDefineLines = new List<ReferenceLine>();
            var listAllProjectDefineLines = new List<ReferenceLine>();

            if (null == zReferenceData || zReferenceData.Length == 0)
            {
                // (special case) if no data is loaded for a reference try loading up the project definitions
                listAllProjectDefineLines = ReadDefaultProjectDefinitions();
            }
            else
            {
                // 2 per reference + 1 for the project wide defines
                m_zReporterProxy.ProgressReset(0, zReferenceData.Length * 2 + 1, 0);

                var bAddedProjectDefinesTask = false;
                foreach(var zReference in zReferenceData)
                {
                    var listRefLines = new List<ReferenceLine>();
                    var listRefDefineLines = new List<ReferenceLine>();
                    var zRefReader = ReferenceReaderFactory.GetReader(zReference, m_zReporterProxy.ProgressReporter);

                    if (zRefReader == null)
                    {
                        // TODO revisit these collections to make sure they are correct...
                        listAllReferenceLines.Clear();
                        m_zReporterProxy.AddIssue($"Failed to load reference: {zReference.RelativePath}");
                        break;
                    }

                    if (!zRefReader.IsValid())
                    {
                        m_zErrorReferenceReader = zRefReader;
                        listAllReferenceLines.Clear();
                        m_zReporterProxy.AddIssue($"Reference reader for reference is in an invalid state: {zReference.RelativePath}");
                        break;
                    }

                    var listReferenceActions = new List<Task>();
                    if (!bAddedProjectDefinesTask)
                    {
                        // only load the project wide defines once
                        listReferenceActions.Add(Task.Factory.StartNew(
                            () =>
                            {
                                if (!string.IsNullOrEmpty(ProjectManager.Instance.ProjectFilePath))
                                {
                                    // the the default project wide ref reader (may differ from the references themselves)
                                    var zProjectDefineReader = ReferenceReaderFactory.GetProjectDefineReader(
                                        ProjectManager.Instance.LoadedProjectDefaultDefineReferenceType, m_zReporterProxy.ProgressReporter);
                                    listAllProjectDefineLines = zProjectDefineReader.GetProjectDefineData() ?? new List<ReferenceLine>();
                                    if (listAllProjectDefineLines.Count == 0)
                                    {
                                        m_zReporterProxy.AddIssue(
                                            "No defines found for project file: {0}".FormatString(ProjectManager
                                                .Instance
                                                .ProjectFilePath));
                                    }
                                }
                                else
                                {
                                    m_zReporterProxy.AddIssue(
                                        "No defines loaded for project -- project not yet saved.");
                                }

                                m_zReporterProxy.ProgressStep();

                            }));
                        bAddedProjectDefinesTask = true;
                    }

                    listReferenceActions.Add(Task.Factory.StartNew(
                        () =>
                        {
                            listRefLines = zRefReader.GetReferenceData();
                            m_zReporterProxy.ProgressStep();
                        }));

                    listReferenceActions.Add(Task.Factory.StartNew(
                        () =>
                        {
                            listRefDefineLines = zRefReader.GetDefineData();
                            m_zReporterProxy.ProgressStep();
                        }));

                    // each reference + defines is loaded before moving onto the next
                    Task.WaitAll(listReferenceActions.ToArray());

                    // check if there are existing lines (from prior references) and remove the column header from the latest ref data
                    if (listAllReferenceLines.Count > 0 && listRefLines.Count > 0)
                    {
                        listRefLines.RemoveAt(0);
                    }
                    listAllReferenceLines.AddRange(listRefLines);
                    listAllReferenceDefineLines.AddRange(listRefDefineLines);

                    m_zReporterProxy.ProgressStep();
                }
            }

            if (CardMakerSettings.EnableGoogleCache && !GoogleReferenceCache.FlushToDisk())
            {
                m_zReporterProxy.AddIssue("Failed to write cache file: {0}".FormatString(GoogleReferenceCache.GetCacheFilePath()));
            }

            // concat all the defines into one list (project + each reference define)
            listAllProjectDefineLines.AddRange(listAllReferenceDefineLines);

            ProcessLines(
                listAllReferenceLines,
                listAllProjectDefineLines,
                zReferenceData != null && zReferenceData.Length > 0);
        }

        public void ProcessLines(List<ReferenceLine> listLines,
            List<ReferenceLine> listDefineLines, bool bHasReferences)
        {
#warning this method is horribly long

            var nDefaultCount = m_zDeck.CardLayout.defaultCount;
            var listColumnNames = new List<string>();
            var dictionaryColumnNames = new Dictionary<string, int>();
            var dictionaryDefines = new Dictionary<string, string>();

            // Line Processing
            if (0 < listLines.Count)
            {
                // Read the column names
                var listAllColumnNames = listLines[0].Entries;
                for (var nIdx = 0; nIdx < listAllColumnNames.Count; nIdx++)
                {
                    var sKey = listAllColumnNames[nIdx].ToLower().Trim();
                    listColumnNames.Add(sKey);
                    if (!dictionaryColumnNames.ContainsKey(sKey))
                    {
                        dictionaryColumnNames.Add(sKey, nIdx);
                    }
                    else
                    {
                        // report an issue with the source
                        IssueManager.Instance.FireAddIssueEvent("Duplicate column found in: " + listLines[0].Source + "::" + "Column [" + nIdx + "]: " + sKey);
                        m_zReporterProxy.AddIssue("Duplicate column found in: " + listLines[0].Source + "::" + "Column [" + nIdx + "]: " + sKey);
                    }
                }

                // determine the allowed layout column index
                int nAllowedLayoutColumn;
                if (!dictionaryColumnNames.TryGetValue(CardMakerConstants.ALLOWED_LAYOUT_COLUMN, out nAllowedLayoutColumn))
                {
                    nAllowedLayoutColumn = -1;
                }

                // remove the columns
                listLines.RemoveAt(0);

                // remove any lines that do not contain any values
                int nRow = 0;
                while (nRow < listLines.Count)
                {
                    var listRow = listLines[nRow].Entries;
                    var bEmptyRow = true;
                    foreach (string sCol in listRow)
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
                    var nLine = 0;

                    while (nLine < listLines.Count)
                    {
                        // some rows may not include the column at that index
                        if (listLines[nLine].Entries.Count > nAllowedLayoutColumn
                            && !IsLayoutAllowed(listLines[nLine].Entries[nAllowedLayoutColumn]))
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

            // Define Processing
            if (listDefineLines.Count > 0)
            {
                // Note: the removal of the first row happens in the reader(s) for defines
                foreach (var zReferenceDefineLine in listDefineLines)
                {
                    var listRowEntries = zReferenceDefineLine.Entries;
                    if (listRowEntries.Count >= 1)
                    {
                        string sKey = listRowEntries[0];
                        int nIdx;
                        string sVal;
                        if (dictionaryDefines.TryGetValue(sKey.ToLower(), out sVal))
                        {
                            var sMsg = "Duplicate define found: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            m_zReporterProxy.AddIssue(sMsg);
                        }
                        else if (dictionaryColumnNames.TryGetValue(sKey.ToLower(), out nIdx))
                        {
                            var sMsg = "Overlapping column name and define found in: " + zReferenceDefineLine.Source + "::" + "Column [" + nIdx + "]: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            m_zReporterProxy.AddIssue(sMsg);
                        }
                        else
                        {
                            // add the define value or empty string
                            dictionaryDefines.Add(sKey.ToLower(),
                                listRowEntries.Count == 1 ? string.Empty : listRowEntries[1]
                            );
                        }
                    }
                }
            }

            m_zDeck.ValidLines.Clear();

            // create the duplicated lines (with count > 1)
            foreach (var zReferenceLine in listLines)
            {
                var listRowEntries = zReferenceLine.Entries;
                if (0 < listRowEntries.Count)
                {
                    var nNumber = GetCardCount(listRowEntries[0].Trim(), dictionaryDefines);
                    for (var nSubRow = 0; nSubRow < nNumber; nSubRow++)
                    {
                        m_zDeck.ValidLines.Add(new DeckLine(nSubRow, zReferenceLine, listColumnNames, m_zReporterProxy));
                    }
                }
            }

            // always create a line
            if (0 == m_zDeck.ValidLines.Count)
            {
                // if there are no entries in the deck we note it for the sake of exports (no need to create a default junk item)
                if (bHasReferences) m_zDeck.EmptyReference = true;

                if (0 < nDefaultCount)
                {
                    // create the default number of lines.
                    for (int nIdx = 0; nIdx < nDefaultCount; nIdx++)
                    {
                        if (0 < listColumnNames.Count) // create each line and the correct number of columns
                        {
                            var arrayDefaultLine = new List<string>();
                            for (int nCol = 0; nCol < arrayDefaultLine.Count; nCol++)
                            {
                                arrayDefaultLine.Add(string.Empty);
                            }
                            m_zDeck.ValidLines.Add(new DeckLine(0, ReferenceLine.CreateDefaultInternalReferenceLine(arrayDefaultLine), listColumnNames, m_zReporterProxy));
                        }
                        else // no columns just create an empty row
                        {
                            m_zDeck.ValidLines.Add(new DeckLine(0, ReferenceLine.CreateDefaultInternalReferenceLine(new List<string>()), new List<string>(), m_zReporterProxy));
                        }
                    }
                    if (bHasReferences)
                    {
                        IssueManager.Instance.FireAddIssueEvent(
                            "No lines found for this layout! Generated {0} lines".FormatString(nDefaultCount));
                    }
                }
            }

            m_zDeck.Translator = m_zDeck.TranslatorFactory.GetTranslator(dictionaryColumnNames, dictionaryDefines, listColumnNames);

#if MONO_BUILD
            Thread.Sleep(100);
#endif
            m_zReporterProxy.Shutdown();
        }

        /// <summary>
        /// Populates the project defines based on the default reference type. Always attempt the local CSV on zero data.
        /// </summary>
        /// <param name="listDefineLines"></param>
        private List<ReferenceLine> ReadDefaultProjectDefinitions()
        {
            m_zReporterProxy.ProgressReset(0, 2, 0);
            var zRefReader = ReferenceReaderFactory.GetProjectDefineReader(
                ProjectManager.Instance.LoadedProjectDefaultDefineReferenceType, m_zReporterProxy.ProgressReporter);
            var listDefineLines = zRefReader.GetProjectDefineData() ?? new List<ReferenceLine>();
            m_zReporterProxy.ProgressStep();
            // always attempt to load the local CSV if nothing was pulled from the other sources
            if (0 == listDefineLines.Count && ProjectManager.Instance.LoadedProjectDefaultDefineReferenceType != ReferenceType.CSV)
            {
                zRefReader = ReferenceReaderFactory.GetProjectDefineReader(ReferenceType.CSV, m_zReporterProxy.ProgressReporter);
                listDefineLines = zRefReader?.GetProjectDefineData() ?? new List<ReferenceLine>();
            }
            m_zReporterProxy.ProgressStep();
            return listDefineLines;
        }

        public void InitiateReferenceRead(ProjectLayoutReference[] arrayProjectLayoutReferences, bool bExporting)
        {
            if (m_zReporterProxy != null)
            {
                ReadData(arrayProjectLayoutReferences);
            }
            else
            {
                var zProgressReporter = CardMakerInstance.ProgressReporterFactory.CreateReporter(
                    "Loading Reference Data",
                    new string[] { ProgressName.REFERENCE_DATA },
                    ReadData,
                    arrayProjectLayoutReferences
                );
                zProgressReporter.CancelButtonVisible = false;
                m_zReporterProxy = new ProgressReporterProxy()
                {
                    ProgressIndex = 0, // known from above
                    ProgressReporter = zProgressReporter,
                    ProxyOwnsReporter = true
                };
                CardMakerInstance.ApplicationForm.InvokeAction(() => zProgressReporter.StartProcessing(CardMakerInstance.ApplicationForm));
            }

            if (!bExporting)
            {
                // if a ref reader had an issue handle it (google for example due to auth)
                m_zErrorReferenceReader?.HandleInvalid();
            }
        }

        private bool IsLayoutAllowed(string sAllowedEntry)
        {
            var arrayAllowedLayouts = sAllowedEntry == null ? new string[] { } : sAllowedEntry.Split(CardMakerConstants.ALLOWED_COLUMN_SEPARATOR);
            if (sAllowedEntry.Trim().Equals("*"))
            {
                return true;
            }
            return null != arrayAllowedLayouts.FirstOrDefault((sAllowedLayout) =>
                m_zDeck.CardLayout.Name.Equals(sAllowedLayout, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        private int GetCardCount(string sCountString, Dictionary<string, string> dictionaryDefines)
        {
            var sInterpretedCount = sCountString;
            if (s_regexColumnVariable.IsMatch(sCountString))
            {
                var zMatch = s_regexColumnVariable.Match(sCountString);
                var sKey = zMatch.Groups[3].ToString().ToLower();
                m_zDeck.GetDefineValue(sKey, dictionaryDefines, out sInterpretedCount);
            }
            if (!int.TryParse(sInterpretedCount.Trim(), out var nCount))
            {
                IssueManager.Instance.FireAddIssueEvent("Invalid card count found: [" + sInterpretedCount + "] The first column should always have a number value.");
                nCount = 1;
            }

            return nCount;
        }
    }
}
