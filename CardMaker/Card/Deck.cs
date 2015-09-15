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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CardMaker.Card.FormattedText;
using CardMaker.Card.Import;
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
        public static readonly char[] DISALLOWED_FILE_CHARS_ARRAY = { '\\', '/', ':', '*', '?', '\"', '>', '<', '|' };

        protected static readonly Dictionary<char, string> s_dictionaryCharReplacement = new Dictionary<char, string>();

        public const string DEFINES_DATA_POSTFIX = "_defines";

        private static readonly Regex s_regexColumnVariable = new Regex(@"(.*)(@\[)(.+?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexCardCounter = new Regex(@"(.*)(##)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSubCardCounter = new Regex(@"(.*)(#sc;)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfLogic = new Regex(@"(.*)(#\()(if.+)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchLogic = new Regex(@"(.*)(#\()(switch.+)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenElseStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*?)(else )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfSet = new Regex(@"(\[)(.*?)(\])", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchStatement = new Regex(@"(switch)(;)(.*?)(;)(.*)", RegexOptions.Compiled);

        private Dictionary<string, int> m_dictionaryColumnNames = new Dictionary<string, int>();
        protected Dictionary<string, string> m_dictionaryDefines = new Dictionary<string, string>();
        private Dictionary<string, Dictionary<string, int>> m_dictionaryElementOverrides = new Dictionary<string, Dictionary<string, int>>();
        private List<string> m_listColumnNames = new List<string>(); // used for populating the list view in the element control window

        private readonly Dictionary<string, ElementString> m_dictionaryElementStringCache = new Dictionary<string, ElementString>();
        private readonly Dictionary<string, FormattedTextDataCache> m_dictionaryMarkupCache = new Dictionary<string, FormattedTextDataCache>();

        protected int m_nCardIndex = -1;
        protected int m_nCardPrintIndex;

        protected ProjectLayout m_zCardLayout;

        public List<DeckLine> ValidLines { get; private set; }

        public Dictionary<string, string> Defines
        {
            get { return m_dictionaryDefines; }
        }

        public ProjectLayout CardLayout 
        {
            get
            {
                return m_zCardLayout;
            }
            private set
            {
                m_zCardLayout = value;
            }
        }

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

        public DeckLine CurrentPrintLine
        {
            get
            {
                return ValidLines[m_nCardPrintIndex];
            }
        }

        public DeckLine CurrentLine
        {
            get
            {
                return ValidLines[m_nCardIndex];
            }
        }

        public int CardCount
        {
            get
            {
                return ValidLines.Count;
            }
        }

        protected void ResetPrintCardIndex()
        {
            m_nCardPrintIndex = 0;
        }

        public static string[] IllegalCharReplacementArray
        {
            set
            {
                if (value.Length == DISALLOWED_FILE_CHARS_ARRAY.Length)
                {
                    s_dictionaryCharReplacement.Clear();
                    for (int nIdx = 0; nIdx < value.Length; nIdx++)
                    {
                        s_dictionaryCharReplacement.Add(DISALLOWED_FILE_CHARS_ARRAY[nIdx], value[nIdx]);
                    }
                }
            }
        }

        public Deck()
        {
            ValidLines = new List<DeckLine>();
        }

        protected void ReadData(object zRefData)
        {
            try
            {
                ReadDataCore(zRefData);
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Failed to read data file(s). " + ex.ToString());
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

            if (null != zReferenceData)
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
                        Logger.AddLogLine(string.Format("Failed to load reference: {0}", zReference.RelativePath));
                        break;
                    }
                    // 0 index is always the default reference in the case of multi load
                    if (nIdx == 0)
                    {
                        if (!string.IsNullOrEmpty(CardMakerInstance.LoadedProjectFilePath))
                        {
                            zRefReader.GetProjectDefineData(zReference, listDefineLines);
                            if (listDefineLines.Count == 0)
                            {
                                Logger.AddLogLine(
                                    "No defines found for project file: {0}".FormatString(
                                        CardMakerInstance.LoadedProjectFilePath));
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

            ProcessLines(listLines, listDefineLines, null == zRefReader ? null : zRefReader.ReferencePath);
        }

        protected void ProcessLines(List<List<string>> listLines, 
            List<List<string>> listDefineLines,
            string sReferencePath)
        {
#warning this method is horribly long

            const string ALLOWED_LAYOUT = "allowed_layout";
            const string OVERRIDE = "override:";

            var nDefaultCount = m_zCardLayout.defaultCount;
            m_dictionaryColumnNames = new Dictionary<string, int>();
            m_listColumnNames = new List<string>();
            m_dictionaryElementOverrides = new Dictionary<string, Dictionary<string, int>>();
            m_dictionaryDefines = new Dictionary<string, string>();


            // Line Processing
            if (0 < listLines.Count)
            {
                // Read the column names
                List<string> listColumnNames = listLines[0];
                for (int nIdx = 0; nIdx < listColumnNames.Count; nIdx++)
                {
                    string sKey = listColumnNames[nIdx].ToLower().Trim();
                    m_listColumnNames.Add(sKey);
                    if (!m_dictionaryColumnNames.ContainsKey(sKey))
                    {
                        m_dictionaryColumnNames.Add(sKey, nIdx);
                    }
                    else
                    {
                        IssueManager.Instance.FireAddIssueEvent("Duplicate column found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey);
                        Logger.AddLogLine("Duplicate column found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey);
                    }
                }

                // determine the allowed layout column index
                int nAllowedLayoutColumn;
                if (!m_dictionaryColumnNames.TryGetValue(ALLOWED_LAYOUT, out nAllowedLayoutColumn))
                {
                    nAllowedLayoutColumn = -1;
                }

                // construct the override dictionary
                foreach (string sKey in m_listColumnNames)
                {
                    if (sKey.StartsWith(OVERRIDE))
                    {
                        string[] arraySplit = sKey.Split(new char[] { ':' });
                        if (3 == arraySplit.Length)
                        {
                            string sElementName = arraySplit[1].Trim();
                            string sElementItemOverride = arraySplit[2].Trim();
                            if (!m_dictionaryElementOverrides.ContainsKey(sElementName))
                            {
                                m_dictionaryElementOverrides.Add(sElementName, new Dictionary<string, int>());
                            }
                            Dictionary<string, int> dictionaryOverrides = m_dictionaryElementOverrides[sElementName];
                            if (dictionaryOverrides.ContainsKey(sElementItemOverride))
                            {
                                Logger.AddLogLine("Duplicate override found: {0}".FormatString(sElementItemOverride));
                            }
                            dictionaryOverrides[sElementItemOverride] = m_dictionaryColumnNames[sKey];
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
                    if (0 < m_listColumnNames.Count)// create each line and the correct number of columns
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
                        if (m_dictionaryDefines.TryGetValue(sKey.ToLower(), out sVal))
                        {
                            string sMsg = "Duplicate define found: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            Logger.AddLogLine(sMsg);
                        }
                        else if (m_dictionaryColumnNames.TryGetValue(sKey.ToLower(), out nIdx))
                        {
                            string sMsg = "Overlapping column name and define found in: " + sReferencePath + "::" + "Column [" + nIdx + "]: " + sKey;
                            IssueManager.Instance.FireAddIssueEvent(sMsg);
                            Logger.AddLogLine(sMsg);
                        }
                        else
                        {
                            m_dictionaryDefines.Add(sKey.ToLower(), row[1]);
                        }
                    }
                }
            }

#if MONO_BUILD
            Thread.Sleep(100);
#endif
            WaitDialog.Instance.ThreadSuccess = true;
            WaitDialog.Instance.CloseWaitDialog();
        }

        /// <summary>
        /// Translates a file export string for naming a file
        /// </summary>
        /// <param name="sRawString"></param>
        /// <param name="nCardNumber"></param>
        /// <param name="nLeftPad"></param>
        /// <returns></returns>
        public string TranslateFileNameString(string sRawString, int nCardNumber, int nLeftPad)
        {
            string sOutput = sRawString;
            var zDeckLine = CurrentPrintLine;
            var listLine = zDeckLine.LineColumns;

            // Translate named items (column names / defines)
            //Groups
            //    1    2    3   4   5
            //@"(.*)(@\[)(.+?)(\])(.*)"
            while (s_regexColumnVariable.IsMatch(sOutput))
            {
                var zMatch = s_regexColumnVariable.Match(sOutput);
                int nIndex;
                string sDefineValue;
                string sKey = zMatch.Groups[3].ToString().ToLower();
                if (m_dictionaryDefines.TryGetValue(sKey, out sDefineValue))
                {
                    sOutput = zMatch.Groups[1] + sDefineValue.Trim() + zMatch.Groups[5];
                }
                else if (m_dictionaryColumnNames.TryGetValue(sKey, out nIndex))
                {
                    sOutput = zMatch.Groups[1] + listLine[nIndex].Trim() + zMatch.Groups[5];
                }
                else
                {
                    sOutput = zMatch.Groups[1] + "[UNKNOWN]" + zMatch.Groups[5];
                }
            }
            // replace ##, #L, Newlines
            sOutput = sOutput.Replace("##", nCardNumber.ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0')).Replace("#L", CardLayout.Name).Replace(Environment.NewLine, string.Empty);

            // last chance: replace unsupported characters (for file name)
            var zBuilder = new StringBuilder();
            foreach (char c in sOutput)
            {
                string sReplace;
                if (s_dictionaryCharReplacement.TryGetValue(c, out sReplace))
                {
                    // quadruple check against bad chars!
                    if (-1 == sReplace.IndexOfAny(DISALLOWED_FILE_CHARS_ARRAY, 0))
                    {
                        zBuilder.Append(sReplace);
                    }
                }
                else
                {
                    if (-1 == c.ToString().IndexOfAny(DISALLOWED_FILE_CHARS_ARRAY))
                    {
                        zBuilder.Append(c);
                    }
                }
            }
            return zBuilder.ToString();
        }

        /// <summary>
        /// Translates the string representing the element. (also handles any nodraw text input)
        /// </summary>
        /// <param name="sRawString"></param>
        /// <param name="listLine"></param>
        /// <param name="zElement"></param>
        /// <param name="bPrint"></param>
        /// <returns></returns>
        public ElementString TranslateString(string sRawString, DeckLine zDeckLine, ProjectLayoutElement zElement, bool bPrint, string sCacheSuffix = "")
        {
            List<string> listLine = zDeckLine.LineColumns;

            string sCacheKey = zElement.name + sCacheSuffix;
            ElementString zCached;
            if (m_dictionaryElementStringCache.TryGetValue(sCacheKey, out zCached))
            {
                return zCached;
            } 
            
            string sOutput = sRawString;

            sOutput = sOutput.Replace("#empty", string.Empty);

            var zElementString = new ElementString();

            // Translate named items (column names / defines)
            //Groups
            //    1    2    3   4   5
            //@"(.*)(@\[)(.+?)(\])(.*)"
            Match zMatch;
            while (s_regexColumnVariable.IsMatch(sOutput))
            {
                zMatch = s_regexColumnVariable.Match(sOutput);
                int nIndex;
                string sDefineValue;
                var sKey = zMatch.Groups[3].ToString().ToLower();

                // check the key for untranslated components
                var arrayParams = sKey.Split(new char[] {','});
                if (arrayParams.Length > 1)
                {
                    sKey = arrayParams[0];
                }

                if (m_dictionaryDefines.TryGetValue(sKey, out sDefineValue))
                {
                }
                else if (m_dictionaryColumnNames.TryGetValue(sKey, out nIndex))
                {
                    sDefineValue = (nIndex >= listLine.Count ? string.Empty : listLine[nIndex].Trim());
                }
                else
                {
                    IssueManager.Instance.FireAddIssueEvent("Bad reference key: " + sKey);
                    sDefineValue = "[BAD NAME: " + sKey + "]";
                }
                if (arrayParams.Length > 1)
                {
                    for (int nIdx = 1; nIdx < arrayParams.Length; nIdx++)
                    {
                        sDefineValue = sDefineValue.Replace("{" + nIdx + "}", arrayParams[nIdx]);
                    }
                }
                sOutput = zMatch.Groups[1] + sDefineValue + zMatch.Groups[5];
            }
            
            // Translate card counter/index
            // Groups                 
            //     1   2    3  4    5  6    7  8   9
            //(@"(.*)(##)(\d+)(;)(\d+)(;)(\d+)(#)(.*)");
            while (s_regexCardCounter.IsMatch(sOutput))
            {
                zMatch = s_regexCardCounter.Match(sOutput);
                var nStart = Int32.Parse(zMatch.Groups[3].ToString());
                var nChange = Int32.Parse(zMatch.Groups[5].ToString());
                var nLeftPad = Int32.Parse(zMatch.Groups[7].ToString());

                var nIndex = bPrint ? m_nCardPrintIndex : m_nCardIndex;

                sOutput = zMatch.Groups[1] +
                    // nIndex is left as is (not adding 1)
                    (nStart + (nIndex * nChange)).ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0') +
                    zMatch.Groups[9];
            }

            // Translate sub card counter/index
            // Groups                 
            //     1   2    3  4    5  6    7  8   9
            //(@"(.*)(#sc;)(\d+)(;)(\d+)(;)(\d+)(#)(.*)");
            while (s_regexSubCardCounter.IsMatch(sOutput))
            {
                zMatch = s_regexSubCardCounter.Match(sOutput);
                var nStart = Int32.Parse(zMatch.Groups[3].ToString());
                var nChange = Int32.Parse(zMatch.Groups[5].ToString());
                var nLeftPad = Int32.Parse(zMatch.Groups[7].ToString());

                var nIndex = zDeckLine.RowSubIndex;

                sOutput = zMatch.Groups[1] +
                    // nIndex is left as is (not adding 1)
                    (nStart + (nIndex * nChange)).ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0') +
                    zMatch.Groups[9];
            }

            // Translate If Logic
            //Groups
            //    1     2    3    4   5 
            //@"(.*)(#\()(if.+)(\)#)(.*)");
            while (s_regexIfLogic.IsMatch(sOutput))
            {
                zMatch = s_regexIfLogic.Match(sOutput);
                string sLogicResult = TranslateIfLogic(zMatch.Groups[3].ToString());
                if (sLogicResult.Trim().Equals("#nodraw", StringComparison.CurrentCultureIgnoreCase))
                    zElementString.DrawElement = false;
                sOutput = zMatch.Groups[1] +
                    sLogicResult +
                    zMatch.Groups[5];
            }

            // Translate Switch Logic
            //Groups                  
            //    1     2        3    4   5 
            //@"(.*)(#\()(switch.+)(\)#)(.*)");
            while (s_regexSwitchLogic.IsMatch(sOutput))
            {
                zMatch = s_regexSwitchLogic.Match(sOutput);
                string sLogicResult = TranslateSwitchLogic(zMatch.Groups[3].ToString());
                if (sLogicResult.Trim().Equals("#nodraw", StringComparison.CurrentCultureIgnoreCase))
                    zElementString.DrawElement = false;
                
                sOutput = zMatch.Groups[1] +
                    sLogicResult +
                    zMatch.Groups[5];
            }

            switch ((ElementType)Enum.Parse(typeof(ElementType), zElement.type))
            {
                case ElementType.Text:
                    sOutput = sOutput.Replace("\\n", Environment.NewLine);
                    sOutput = sOutput.Replace("\\q", "\"");
                    sOutput = sOutput.Replace("\\c", ",");
                    sOutput = sOutput.Replace("&gt;", ">");
                    sOutput = sOutput.Replace("&lt;", "<");
                    break;
                case ElementType.FormattedText:
                    sOutput = sOutput.Replace("<c>", ",");
                    sOutput = sOutput.Replace("<q>", "\"");
                    sOutput = sOutput.Replace("&gt;", ">");
                    sOutput = sOutput.Replace("&lt;", "<");
                    break;
            }

            zElementString.String = sOutput;

            AddStringToTranslationCache(sCacheKey, zElementString);

            return zElementString;
        }

        public ProjectLayoutElement GetOverrideElement(ProjectLayoutElement zElement, List<string> arrayLine, DeckLine zDeckLine, bool bExport)
        {
            Dictionary<string, int> dictionaryOverrideColumns;
            string sNameLower = zElement.name.ToLower();
            m_dictionaryElementOverrides.TryGetValue(sNameLower, out dictionaryOverrideColumns);
            if (null == dictionaryOverrideColumns)
            {
                return zElement;
            }

            var zOverrideElement = new ProjectLayoutElement();
            zOverrideElement.DeepCopy(zElement, false);
            zOverrideElement.name = zElement.name;

            foreach (string sKey in dictionaryOverrideColumns.Keys)
            {
                Type zType = typeof(ProjectLayoutElement);
                PropertyInfo zProperty = zType.GetProperty(sKey);
                if (null != zProperty && zProperty.CanWrite)
                {
                    MethodInfo zMethod = zProperty.GetSetMethod();
                    int nOverrideValueColumnIdx = dictionaryOverrideColumns[sKey];
                    if (arrayLine.Count <= nOverrideValueColumnIdx)
                    {
                        continue;
                    }
                    string sValue = arrayLine[nOverrideValueColumnIdx].Trim();

                    // Note: TranslateString maintains an element name based cache, the key is critical to make this translation unique
                    sValue = TranslateString(sValue, zDeckLine, zOverrideElement, bExport, sKey).String;

                    if (!string.IsNullOrEmpty(sValue))
                    {
                        if (zProperty.PropertyType == typeof(string))
                        {
                            zMethod.Invoke(zOverrideElement, new object[] { sValue });
                        }
                        else if (zProperty.PropertyType == typeof(float))
                        {
                            float fValue;
                            if (float.TryParse(sValue, out fValue))
                            {
                                zMethod.Invoke(zOverrideElement, new object[] { fValue });
                            }
                        }
                        else if (zProperty.PropertyType == typeof(bool))
                        {
                            bool bValue;
                            if (bool.TryParse(sValue, out bValue))
                            {
                                zMethod.Invoke(zOverrideElement, new object[] { bValue });
                            }
                        }
                        else if (zProperty.PropertyType == typeof(Int32))
                        {
                            int nValue;
                            if (int.TryParse(sValue, out nValue))
                            {
                                zMethod.Invoke(zOverrideElement, new object[] { nValue });
                            }
                        }
                    }
                }
            }
            zOverrideElement.InitializeCache(); // any cached items must be recached
            return zOverrideElement;
        }

        private enum LogicCheck
        {
            Equals,
            NotEquals,
            GreaterThan,
            LessThan,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,
        }

        private string TranslateIfLogic(string sInput)
        {
            //Groups
            //    1    2  3            4    5   6
            //@"(if)(.*?)([!=><]=|<|>)(.*?)(then )(.*)");
            //Groups                                   
            //    1    2  3            4    5      6    7      8
            //@"(if)(.*?)([!=><]=|<|>)(.*?)(then )(.*?)(else )(.*)");
            Match zIfMatch = null;
            string sOutput = string.Empty;
            bool bHasElse = false;
            if (s_regexIfThenElseStatement.IsMatch(sInput))
            {
                zIfMatch = s_regexIfThenElseStatement.Match(sInput);
                bHasElse = true;
            }
            else if (s_regexIfThenStatement.IsMatch(sInput))
            {
                zIfMatch = s_regexIfThenStatement.Match(sInput);
            }
            if (null != zIfMatch)
            {
                string sCompareType = zIfMatch.Groups[3].ToString();
                var eCheck = LogicCheck.Equals;
                switch (sCompareType)
                {
                    case "!=":
                        eCheck = LogicCheck.NotEquals;
                        break;
                    case "==":
                        eCheck = LogicCheck.Equals;
                        break;
                    case ">":
                        eCheck = LogicCheck.GreaterThan;
                        break;
                    case "<":
                        eCheck = LogicCheck.LessThan;
                        break;
                    case ">=":
                        eCheck = LogicCheck.GreaterThanOrEqualTo;
                        break;
                    case "<=":
                        eCheck = LogicCheck.LessThanOrEqualTo;
                        break;
                }
                if (eCheck == LogicCheck.Equals || eCheck == LogicCheck.NotEquals)
                {
                    bool bCompare = CompareIfSet(zIfMatch.Groups[2].ToString(), zIfMatch.Groups[4].ToString());
                    if (eCheck == LogicCheck.NotEquals)
                    {
                        bCompare = !bCompare;
                    }
                    if (bCompare)
                    {
                        sOutput = zIfMatch.Groups[6].ToString();
                    }
                    else
                    {
                        if (bHasElse)
                        {
                            sOutput = zIfMatch.Groups[8].ToString();
                        }
                    }
                }
                else // numeric check
                {
                    decimal nValue1;
                    decimal nValue2;
                    bool bSuccess = decimal.TryParse(zIfMatch.Groups[2].ToString(), out nValue1);
                    bSuccess &= decimal.TryParse(zIfMatch.Groups[4].ToString(), out nValue2);
                    if (!bSuccess)
                    {
                        return string.Empty; // a mess!
                    }
                    bool bCompare = false;
                    switch (eCheck)
                    {
                        case LogicCheck.GreaterThan:
                            bCompare = nValue1 > nValue2;
                            break;
                        case LogicCheck.GreaterThanOrEqualTo:
                            bCompare = nValue1 >= nValue2;
                            break;
                        case LogicCheck.LessThan:
                            bCompare = nValue1 < nValue2;
                            break;
                        case LogicCheck.LessThanOrEqualTo:
                            bCompare = nValue1 <= nValue2;
                            break;
                    }
                    if (bCompare)
                    {
                        sOutput = zIfMatch.Groups[6].ToString();
                    }
                    else
                    {
                        if (bHasElse)
                        {
                            sOutput = zIfMatch.Groups[8].ToString();
                        }
                    }
                }
            }
            return sOutput;
        }

        private bool CompareIfSet(string sSet1, string sSet2)
        {
            var hSet1 = GetIfSet(sSet1);
            var hSet2 = GetIfSet(sSet2);
            foreach (string sKey in hSet1)
            {
                if (hSet2.Contains(sKey))
                {
                    return true;
                }
            }
            return false;
        }

        private HashSet<string> GetIfSet(string sSet)
        {
            var hSet = new HashSet<string>();
            // Groups                    
            //    1    2   3
            //@"(\[)(.*?)(\])");
            if (s_regexIfSet.IsMatch(sSet))
            {
                var zMatch = s_regexIfSet.Match(sSet);
                var arraySplit = zMatch.Groups[2].ToString().Split(new char[] { ';' });
                for (var nIdx = 0; nIdx < arraySplit.Length; nIdx++)
                {
                    var sItem = arraySplit[nIdx].Trim().ToLower();
                    if (!hSet.Contains(sItem))
                    {
                        hSet.Add(sItem);
                    }
                }
            }
            else
            {
                hSet.Add(sSet.ToLower().Trim());
            }
            return hSet;

        }

        private string TranslateSwitchLogic(string sInput)
        {
            //Groups                                   
            //        1  2    3  4   5
            //@"(switch)(;)(.*?)(;)(.*)");
            var nDefaultIndex = -1;
            if (s_regexSwitchStatement.IsMatch(sInput))
            {
                var zMatch = s_regexSwitchStatement.Match(sInput);
                var sKey = zMatch.Groups[3].ToString();
                var arrayCases = zMatch.Groups[5].ToString().Split(new char[] { ';' });
                if (0 == (arrayCases.Length % 2))
                {
                    var nIdx = 0;
                    while (nIdx < arrayCases.Length)
                    {
                        if (arrayCases[nIdx].Equals("#default", StringComparison.CurrentCultureIgnoreCase))
                        {
                            nDefaultIndex = nIdx;
                        }
                        if (arrayCases[nIdx].Equals(sKey, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return arrayCases[nIdx + 1];
                        }
                        nIdx += 2;
                    }
                    if (-1 < nDefaultIndex)
                    {
                        return arrayCases[nDefaultIndex + 1];
                    }
                }
            }
            return sInput;
        }

        /// <summary>
        /// Populates the specified ListView with the columns and data associated with this Deck
        /// </summary>
        /// <param name="listView">The ListView to operate on</param>
        public void PopulateListViewWithElementColumns(ListView listView)
        {
            var arrayColumnSizes = new int[m_listColumnNames.Count];
            for (var nCol = 0; nCol < m_listColumnNames.Count; nCol++)
            {
                arrayColumnSizes[nCol] = 100;
            }
            for (var nCol = 0; nCol < listView.Columns.Count && nCol < arrayColumnSizes.Length; nCol++)
            {
                arrayColumnSizes[nCol] = listView.Columns[nCol].Width;
            }

            listView.Columns.Clear();
            listView.Items.Clear();

            for (var nIdx = 1; nIdx < m_listColumnNames.Count; nIdx++)
            {
                ListViewAssist.AddColumn(m_listColumnNames[nIdx], arrayColumnSizes[nIdx], listView);
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
            ResetTranslationCache();
            ResetMarkupCache();
        }

        #endregion

        #region Translation Cache

        public void ResetTranslationCache(ProjectLayoutElement zElement)
        {
            if (m_dictionaryElementStringCache.ContainsKey(zElement.name))
            {
                m_dictionaryElementStringCache.Remove(zElement.name);
            }
        }

        private void ResetTranslationCache()
        {
            m_dictionaryElementStringCache.Clear();
        }

        private void AddStringToTranslationCache(string sKey, ElementString zElementString)
        {
            if (m_dictionaryElementStringCache.ContainsKey(sKey))
            {
                m_dictionaryElementStringCache.Remove(sKey);
                //Logger.AddLogLine("String Cache: Replace?!");
            }
            m_dictionaryElementStringCache.Add(sKey, zElementString);
        }

        public ElementString GetStringFromTranslationCache(string sKey)
        {
            if (m_dictionaryElementStringCache.ContainsKey(sKey))
                return m_dictionaryElementStringCache[sKey];
            return null;
        }

        #endregion

        #region Markup Cache

        public FormattedTextDataCache GetCachedMarkup(string sElementName)
        {
            FormattedTextDataCache zCached;
            if (m_dictionaryMarkupCache.TryGetValue(sElementName, out zCached))
            {
                return zCached;
            }
            return null;
        }

        public void AddCachedMarkup(string sElementName, FormattedTextDataCache zFormattedData)
        {
            m_dictionaryMarkupCache.Add(sElementName, zFormattedData);
        }

        public void ResetMarkupCache(string sElementName)
        {
            if (m_dictionaryMarkupCache.ContainsKey(sElementName))
            {
                m_dictionaryMarkupCache.Remove(sElementName);
            }
        }

        private void ResetMarkupCache()
        {
            m_dictionaryMarkupCache.Clear();
        }

        #endregion

        #region Layout Set
#warning todo: make a deck loader interface so this can be handled from the command line

        public void SetAndLoadLayout(ProjectLayout zLayout, bool bExporting)
        {
            CardLayout = zLayout;

            ResetPrintCardIndex();
            ResetDeckCache();

            var bReferenceFound = false;

            if (null != m_zCardLayout.Reference)
            {
                ProjectLayoutReference[] zReferenceData = null;

                if (m_zCardLayout.combineReferences)
                {
                    var listReferences = new List<ProjectLayoutReference>();
                    ProjectLayoutReference zDefaultReference = null;
                    foreach (var zReference in m_zCardLayout.Reference)
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
                    foreach (var zReference in m_zCardLayout.Reference)
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

        #endregion

    }
}
