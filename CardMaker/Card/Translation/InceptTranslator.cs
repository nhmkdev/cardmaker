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
using System.Text.RegularExpressions;
using CardMaker.Events.Managers;
using CardMaker.XML;

namespace CardMaker.Card.Translation
{
    /// <summary>
    /// This is the original translator used by CardMaker
    /// </summary>
    public class InceptTranslator : TranslatorBase
    {
        private enum LogicCheck
        {
            Equals,
            NotEquals,
            GreaterThan,
            LessThan,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,
        }

        private static readonly Regex s_regexColumnVariable = new Regex(@"(.*)(@\[)(.+?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexCardVariable = new Regex(@"(.*)(\!\[)(.+?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexCardCounter = new Regex(@"(.*)(##)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSubCardCounter = new Regex(@"(.*)(#sc;)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfLogic = new Regex(@"(.*)(#\()(if.+)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchLogic = new Regex(@"(.*)(#\()(switch.+)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenElseStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*?)(else )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfSet = new Regex(@"(\[)(.*?)(\])", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchStatement = new Regex(@"(switch)(;)(.*?)(;)(.*)", RegexOptions.Compiled);

        public InceptTranslator(Dictionary<string, int> dictionaryColumnNameToIndex, Dictionary<string, string> dictionaryDefines,
            Dictionary<string, Dictionary<string, int>> dictionaryElementOverrides, List<string> listColumnNames)
            : base(dictionaryColumnNameToIndex, dictionaryDefines, dictionaryElementOverrides, listColumnNames)
        {
            
        }

        /// <summary>
        /// Translates the string representing the element. (also handles any nodraw text input)
        /// </summary>
        /// <param name="sRawString"></param>
        /// <param name="nCardIndex"></param>
        /// <param name="zDeckLine"></param>
        /// <param name="zElement"></param>
        /// <returns></returns>
        protected override ElementString TranslateToElementString(string sRawString, int nCardIndex, DeckLine zDeckLine, ProjectLayoutElement zElement)
        {
            List<string> listLine = zDeckLine.LineColumns;

            string sOutput = sRawString;

            sOutput = sOutput.Replace("#empty", string.Empty);

            var zElementString = new ElementString();

            // Translate card variables (non-reference information
            // Groups
            //     1    2    3   4   5
            // @"(.*)(!\[)(.+?)(\])(.*)"
            Match zMatch;
            while (s_regexCardVariable.IsMatch(sOutput))
            {
                zMatch = s_regexCardVariable.Match(sOutput);
                string sDefineValue;
                var sKey = zMatch.Groups[3].ToString().ToLower();

                // NOTE: if this expands into more variables move all this into some other method and use a dictionary lookup
                if (sKey.Equals("cardindex"))
                {
                    sDefineValue = (nCardIndex + 1).ToString();
                }
                else if (sKey.Equals("deckindex"))
                {
                    sDefineValue = (zDeckLine.RowSubIndex + 1).ToString();
                }
                else
                {
                    IssueManager.Instance.FireAddIssueEvent("Bad card variable: " + sKey);
                    sDefineValue = "[BAD NAME: " + sKey + "]";
                }

                sOutput = zMatch.Groups[1] + sDefineValue + zMatch.Groups[5];
            }

            // Translate named items (column names / defines)
            //Groups
            //    1    2    3   4   5
            //@"(.*)(@\[)(.+?)(\])(.*)"
            while (s_regexColumnVariable.IsMatch(sOutput))
            {
                zMatch = s_regexColumnVariable.Match(sOutput);
                int nIndex;
                string sDefineValue;
                var sKey = zMatch.Groups[3].ToString().ToLower();

                // check the key for untranslated components
                var arrayParams = sKey.Split(new char[] { ',' });
                if (arrayParams.Length > 1)
                {
                    sKey = arrayParams[0];
                }

                if (DictionaryDefines.TryGetValue(sKey, out sDefineValue))
                {
                }
                else if (DictionaryColumnNameToIndex.TryGetValue(sKey, out nIndex))
                {
                    sDefineValue = (nIndex >= listLine.Count ? string.Empty : (listLine[nIndex] ?? "").Trim());
                }
                else
                {
                    IssueManager.Instance.FireAddIssueEvent("Bad reference name: " + sKey);
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

                sOutput = zMatch.Groups[1] +
                    // nIndex is left as is (not adding 1)
                    (nStart + (nCardIndex * nChange)).ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0') +
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
                sOutput = zMatch.Groups[1] +
                    sLogicResult +
                    zMatch.Groups[5];
            }

            zElementString.String = sOutput;

            return zElementString;
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
                    bool bSuccess = Decimal.TryParse(zIfMatch.Groups[2].ToString(), out nValue1);
                    bSuccess &= Decimal.TryParse(zIfMatch.Groups[4].ToString(), out nValue2);
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
                foreach (var sEntry in arraySplit)
                {
                    var sItem = sEntry.Trim().ToLower();
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
    }
}
