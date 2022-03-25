////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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
using System.Windows.Forms;
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
        public const string DEFINES_DATA_POSTFIX = "_defines";

        protected int m_nCardIndex = -1;
        protected int m_nCardPrintIndex;

        public ProjectLayout CardLayout { get; protected set; }

        public  ITranslatorFactory TranslatorFactory { get; protected set; }

        public TranslatorBase Translator { protected get; set; }

        public List<DeckLine> ValidLines { get; }

        public Dictionary<string, string> Defines => Translator.DictionaryDefines;

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

        public ElementString TranslateString(string sRawString, DeckLine zDeckLine, ProjectLayoutElement zElement, bool bPrint, string sCacheSuffix = "")
        {
            return Translator.TranslateString(this, sRawString, bPrint ? m_nCardPrintIndex : m_nCardIndex, zDeckLine, zElement, sCacheSuffix);
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
            return Translator.GetOverrideElement(this, zElement, bExport ? m_nCardPrintIndex : m_nCardIndex, zDeckLine.LineColumns, zDeckLine);
        }

        public ProjectLayoutElement GetVariableOverrideElement(ProjectLayoutElement zElement, Dictionary<string, string> dictionaryOverrideFieldToValue)
        {
            return Translator.GetVariableOverrideElement(zElement, dictionaryOverrideFieldToValue);
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
            return FilenameTranslator.TranslateFileNameString(sRawString, nCardNumber, nLeftPad, CurrentPrintLine,
                Translator.DictionaryDefines, Translator.DictionaryColumnNameToIndex,
                CardLayout);
        }

#endregion
    }
}
