////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2020 Tim Stair
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

            var listLines = new List<List<string>>();
            var listDefineLines = new List<List<string>>();
            var listReferenceDefineLines = new List<List<string>>();

            ReferenceReader zRefReader = null;

            if (null == zReferenceData || zReferenceData.Length == 0)
            {
                ReadDefaultProjectDefinitions(listDefineLines);
            }
            else
            {
                // 2 per reference + 1 for the project wide defines
                m_zReporterProxy.ProgressReset(0, zReferenceData.Length * 2 + 1, 0);

                for (int nIdx = 0; nIdx < zReferenceData.Length; nIdx++)
                {
                    var zReference = zReferenceData[nIdx];
                    var listRefLines = new List<List<string>>();
                    zRefReader = ReferenceReaderFactory.GetReader(zReference, m_zReporterProxy.ProgressReporter);

                    if (zRefReader == null)
                    {
                        listLines.Clear();
                        m_zReporterProxy.AddIssue($"Failed to load reference: {zReference.RelativePath}");
                        break;
                    }

                    if (!zRefReader.IsValid())
                    {
                        m_zErrorReferenceReader = zRefReader;
                        listLines.Clear();
                        m_zReporterProxy.AddIssue($"Reference reader for reference is in an invalid state: {zReference.RelativePath}");
                        break;
                    }

                    var listReferenceActions = new List<Task>();

                    // 0 index is always the default project reference in the case of multi load
                    // only load it once
                    if (nIdx == 0)
                    {
                        listReferenceActions.Add(Task.Factory.StartNew(
                            () =>
                            {
                                if (!string.IsNullOrEmpty(ProjectManager.Instance.ProjectFilePath))
                                {
                                    zRefReader.GetProjectDefineData(zReference, listDefineLines);
                                    if (listDefineLines.Count == 0)
                                    {
                                        m_zReporterProxy.AddIssue("No defines found for project file: {0}".FormatString(ProjectManager.Instance.ProjectFilePath));
                                    }
                                }
                                else
                                {
                                    m_zReporterProxy.AddIssue("No defines loaded for project -- project not yet saved.");
                                }
                                m_zReporterProxy.ProgressStep();

                            }));
                    }

                    listReferenceActions.Add(Task.Factory.StartNew(
                        () =>
                        {
                            zRefReader.GetReferenceData(zReference, listRefLines);
                            m_zReporterProxy.ProgressStep();
                        }));

                    listReferenceActions.Add(Task.Factory.StartNew(
                        () =>
                        {
                            zRefReader.GetDefineData(zReference, listReferenceDefineLines);
                            m_zReporterProxy.ProgressStep();
                        }));

                    Task.WaitAll(listReferenceActions.ToArray());

                    // check if there are existing lines (from prior references) and remove the column header from the latest ref data
                    if (listLines.Count > 0 && listRefLines.Count > 0)
                    {
                        listRefLines.RemoveAt(0);
                    }
                    listLines.AddRange(listRefLines);

                    zRefReader.FinalizeReferenceLoad();
                    m_zReporterProxy.ProgressStep();
                }
            }

            // Note: the readers trim out the first line for defines data
            listDefineLines.AddRange(listReferenceDefineLines);

            ProcessLines(
                listLines,
                listDefineLines,
                zReferenceData != null && zReferenceData.Length > 0,
                zRefReader?.ReferencePath);
        }

        public void ProcessLines(List<List<string>> listLines,
            List<List<string>> listDefineLines, bool bHasReferences, string sReferencePath)
        {
#warning this method is horribly long

            const string ALLOWED_LAYOUT = "allowed_layout";
            const string OVERRIDE = "override:";

            var nDefaultCount = m_zDeck.CardLayout.defaultCount;
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
                        m_zReporterProxy.AddIssue("Duplicate column found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey);
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
                                m_zReporterProxy.AddIssue("Duplicate override found: {0}".FormatString(sElementItemOverride));
                            }
                            dictionaryOverrides[sElementItemOverride] = dictionaryColumnNames[sKey];
                        }
                    }
                }

                // remove the columns
                listLines.RemoveAt(0);

                // remove any lines that do not contain any values
                int nRow = 0;
                while (nRow < listLines.Count)
                {
                    var listRow = listLines[nRow];
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
                    int nLine = 0;

                    while (nLine < listLines.Count)
                    {
                        // some rows may not include the column at that index
                        if (listLines[nLine].Count > nAllowedLayoutColumn
                            && !m_zDeck.CardLayout.Name.Equals(listLines[nLine][nAllowedLayoutColumn], StringComparison.CurrentCultureIgnoreCase))
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

            m_zDeck.ValidLines.Clear();

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
                        m_zDeck.ValidLines.Add(new DeckLine(listItems, nCount));
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
                            m_zDeck.ValidLines.Add(new DeckLine(arrayDefaultLine));
                        }
                        else // no columns just create an empty row
                        {
                            m_zDeck.ValidLines.Add(new DeckLine(new List<string>()));
                        }
                    }
                    if (!string.IsNullOrEmpty(sReferencePath))
                    {
                        IssueManager.Instance.FireAddIssueEvent(
                            "No lines found for this layout! Generated " + nDefaultCount);
                    }
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
                            var sMsg = "Duplicate define found: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            m_zReporterProxy.AddIssue(sMsg);
                        }
                        else if (dictionaryColumnNames.TryGetValue(sKey.ToLower(), out nIdx))
                        {
                            var sMsg = "Overlapping column name and define found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            m_zReporterProxy.AddIssue(sMsg);
                        }
                        else
                        {
                            dictionaryDefines.Add(sKey.ToLower(), row[1]);
                        }
                    }
                }
            }

            m_zDeck.Translator = m_zDeck.TranslatorFactory.GetTranslator(dictionaryColumnNames, dictionaryDefines, dictionaryElementOverrides, listColumnNames);

#if MONO_BUILD
            Thread.Sleep(100);
#endif
            m_zReporterProxy.Shutdown();
        }

        /// <summary>
        /// Populates the project defines based on the default reference type. If nothing is found on Google then attempt the local CSV.
        /// </summary>
        /// <param name="listDefineLines"></param>
        private void ReadDefaultProjectDefinitions(List<List<string>> listDefineLines)
        {
            m_zReporterProxy.ProgressReset(0, 2, 0);
            ReferenceReader zRefReader;
            if (ReferenceType.Google == ProjectManager.Instance.LoadedProjectDefaultDefineReferenceType)
            {
                zRefReader = ReferenceReaderFactory.GetDefineReader(ReferenceType.Google, m_zReporterProxy.ProgressReporter);
                zRefReader?.GetProjectDefineData(null, listDefineLines);
            }
            m_zReporterProxy.ProgressStep();
            // always attempt to load the local if nothing was pulled from google
            if (0 == listDefineLines.Count)
            {
                zRefReader = ReferenceReaderFactory.GetDefineReader(ReferenceType.CSV, m_zReporterProxy.ProgressReporter);
                zRefReader?.GetProjectDefineData(null, listDefineLines);
            }
            m_zReporterProxy.ProgressStep();
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
    }
}
