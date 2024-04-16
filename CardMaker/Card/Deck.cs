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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CardMaker.Card.Export;
using CardMaker.Card.FormattedText;
using CardMaker.Card.Translation;
using CardMaker.XML;
using Support.IO;
using Support.Progress;
using Support.UI;

#if MONO_BUILD
using System.Threading;
#endif

namespace CardMaker.Card
{
    public class Deck
    {
        public const string DEFINES_DATA_SUFFIX = "_defines";

        private readonly ReferenceTranslationOverride m_zReferenceTranslationOverride = new ReferenceTranslationOverride();

        protected int m_nCardIndex = -1;
        protected int m_nCardPrintIndex;

        public ProjectLayout CardLayout { get; protected set; }

        public  ITranslatorFactory TranslatorFactory { get; protected set; }

        public TranslatorBase Translator { protected get; set; }

        public List<DeckLine> ValidLines { get; }

        public Dictionary<string, string> Defines => Translator.DictionaryDefines;

        public SubLayoutExportContext SubLayoutExportContext { get; set; }

        public ExportContext ExportContext { get; set; }

        public bool EmptyReference { get; set; }

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

#warning TODO: investigate the complete removal of the CardPrintIndex (may no longer be required)
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
            EmptyReference = false;
            ValidLines = new List<DeckLine>();
            TranslatorFactory = new TranslatorFactory();
        }

        public ElementString TranslateString(string sRawString, DeckLine zDeckLine, ProjectLayoutElement zElement, bool bPrint, string sCacheSuffix = "", bool bSkipCache = false)
        {
            return Translator.TranslateString(this, sRawString, bPrint ? m_nCardPrintIndex : m_nCardIndex, zDeckLine, zElement, sCacheSuffix, bSkipCache);
        }

        public ElementString GetStringFromTranslationCache(string sKey)
        {
            return Translator.GetStringFromTranslationCache(sKey);
        }

        public void ResetTranslationCache(ProjectLayoutElement zElement)
        {
            Translator.ResetTranslationCache(zElement);
        }

#region Cache

        public FormattedTextDataCache GetCachedMarkup(string sElementName)
        {
            return Translator.GetCachedMarkup(sElementName);
        }

#endregion

        public ProjectLayoutElement GetOverrideElement(ProjectLayoutElement zElement, DeckLine zDeckLine, bool bExport)
        {
            return Translator.GetOverrideElement(this, zElement, bExport ? m_nCardPrintIndex : m_nCardIndex, zDeckLine.ElementToOverrideFieldsToValues, zDeckLine);
        }

        public ProjectLayoutElement GetVariableOverrideElement(ProjectLayoutElement zElement, Dictionary<string, string> dictionaryOverrideFieldToValue)
        {
            return Translator.GetVariableOverrideElement(zElement, dictionaryOverrideFieldToValue);
        }

        public bool TryGetDefineValue(string sDefine, Dictionary<string, string> dictionaryDefineToValue, out string sValue)
        {
            return TryGetDictionaryValue(sDefine,
                m_zReferenceTranslationOverride.DefinesToValues,
                dictionaryDefineToValue,
                out sValue);
        }

        public bool TryGetColumnValue(string sColumn, Dictionary<string, string> dictionaryColumnToValue, out string sValue)
        {
            return TryGetDictionaryValue(sColumn,
                m_zReferenceTranslationOverride.ColumnsToValues,
                dictionaryColumnToValue,
                out sValue);
        }

        private bool TryGetDictionaryValue(string sKey, Dictionary<string, string> dictionaryPrimary,
            Dictionary<string, string> dictionarySecondary, out string sValue)
        {
            if (dictionaryPrimary.TryGetValue(sKey, out sValue) || dictionarySecondary.TryGetValue(sKey, out sValue))
            {
                return true;
            }

            sValue = string.Empty;
            return false;
        }

        /// <summary>
        /// Populates the specified ListView with the columns and data associated with this Deck
        /// </summary>
        /// <param name="listView">The ListView to operate on</param>
        public void PopulateListViewWithElementColumns(ListView listView)
        {
            var listColumnNames = Translator.ListColumnNames;
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
                var listColumns = CurrentLine.ReferenceLine.Entries;
                if (listColumns.Count > 0)
                {
                    listView.Items.Add(new ListViewItem(listColumns.GetRange(1, listColumns.Count - 1).ToArray()));
                }
            }
        }

#region Cache General

        public void ResetDeckCache()
        {
            if (Translator == null)
            {
                Logger.AddLogLine("Warn: code attempted to clear cache on non-existent translator (this is a mostly harmless bug)");
                return;
            }
            Translator.ResetTranslationCache();
            Translator.ResetMarkupCache();
        }

#endregion

#region Markup Cache
        public void AddCachedMarkup(string sElementName, FormattedTextDataCache zFormattedData)
        {
            Translator.AddCachedMarkup(sElementName, zFormattedData);
        }

        public void ResetMarkupCache(string sElementName)
        {
            Translator.ResetMarkupCache(sElementName);
        }
#endregion

#region Layout Set
        public void SetAndLoadLayout(ProjectLayout zLayout, bool bExporting, ProgressReporterProxy zReporterProxy)
        {
            EmptyReference = false;

            CardLayout = zLayout;

            ResetPrintCardIndex();
            ProjectLayoutReference[] zReferenceData = null;

            if (null != CardLayout.Reference)
            {
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
            }
            new DeckReader(this, zReporterProxy).InitiateReferenceRead(zReferenceData, bExporting);
        }

        public string TranslateFileNameString(string sRawString, int nCardNumber, int nLeftPad)
        {
#warning this takes an awkward path to the translator and back
            return FilenameTranslator.TranslateFileNameString(sRawString, nCardNumber, nLeftPad,
                Translator.DictionaryDefines, this, CardLayout);
        }

        #endregion

        public void ApplySubLayoutDefinesOverrides(Dictionary<string, string> dictionaryDefinesToValues)
        {
            AppendDictionaryWithNewKeys(
                dictionaryDefinesToValues,
                m_zReferenceTranslationOverride.DefinesToValues);
        }

        public void ApplySubLayoutOverrides(Deck zParentDeck, SubLayoutExportSettings zSettings)
        {
            if (zSettings.ApplyDefineValues)
            {
                // parent most defines are applied first
                AppendDictionaryWithNewKeys(
                    zParentDeck.m_zReferenceTranslationOverride.DefinesToValues,
                    m_zReferenceTranslationOverride.DefinesToValues);
                AppendDictionaryWithNewKeys(
                    zParentDeck.Defines,
                    m_zReferenceTranslationOverride.DefinesToValues);
            }

            if (zSettings.ApplyReferenceValues)
            {
                // parent most reference values applied first
                AppendDictionaryWithNewKeys(
                    zParentDeck.m_zReferenceTranslationOverride.ColumnsToValues,
                    m_zReferenceTranslationOverride.ColumnsToValues);
                AppendDictionaryWithNewKeys(
                    zParentDeck.CurrentPrintLine.ColumnsToValues,
                    m_zReferenceTranslationOverride.ColumnsToValues);
            }
        }

        private static void AppendDictionaryWithNewKeys(Dictionary<string, string> dictionarySource,
            Dictionary<string, string> dictionaryDestination)
        {
            dictionarySource.ToList().ForEach(
                kvp =>
                {
                    if (!dictionaryDestination.ContainsKey(kvp.Key))
                    {
                        dictionaryDestination[kvp.Key] = kvp.Value;
                    }
                });
        }

        class ReferenceTranslationOverride
        {
            public Dictionary<string, string> DefinesToValues { get; private set; }
            public Dictionary<string, string> ColumnsToValues { get; private set; }

            public ReferenceTranslationOverride()
            {
                DefinesToValues = new Dictionary<string, string>();
                ColumnsToValues = new Dictionary<string, string>();
            }
        }

    }
}
