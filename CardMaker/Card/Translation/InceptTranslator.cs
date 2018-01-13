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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;
using Support.UI;
using Support.Util;

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

        private const int MAX_TRANSLATION_LOOP_COUNT = 100;

        private static readonly Regex s_regexColumnVariable = new Regex(@"(.*)(@\[)(.+?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexColumnVariableSubstring = new Regex(@"(.*)(%\[)(.+?)(,)(\d+)(,)(\d+)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexCardVariable = new Regex(@"(.*)(\!\[)(.+?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexElementOverride = new Regex(@"(.*)(\$\[)(.+?):(.*?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexCardCounter = new Regex(@"(.*)(##)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSubCardCounter = new Regex(@"(.*)(#sc;)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexRepeat = new Regex(@"(.*)(#repeat;)(\d+)(;)(.+?)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexRandomNumber = new Regex(@"(.*)(#random;)(-?\d+)(;)(-?\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfLogic = new Regex(@"(.*)(#\()(if.*?)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchLogic = new Regex(@"(.*)(#\()(switch.*?)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenElseStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*?)( else )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfSet = new Regex(@"(\[)(.*?)(\])", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchStatement = new Regex(@"(switch)(;)(.*?)(;)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchStatementAlt = new Regex(@"(switch)(::)(.*?)(::)(.*)", RegexOptions.Compiled);
        private static readonly HashSet<string> s_setDisallowedOverrideFields = new HashSet<string>()
        {
            "name",
            "variable"
        };

        private const string SWITCH_DEFAULT = "#default";
        private readonly string[] ArraySwitchDelimiter = new string[]{ ";" };
        private readonly string[] ArraySwitchDelimiterAlt = new string[] { "::" };

        public InceptTranslator(Dictionary<string, int> dictionaryColumnNameToIndex, Dictionary<string, string> dictionaryDefines,
            Dictionary<string, Dictionary<string, int>> dictionaryElementToFieldColumnOverrides, List<string> listColumnNames)
            : base(dictionaryColumnNameToIndex, dictionaryDefines, dictionaryElementToFieldColumnOverrides, listColumnNames)
        {
            
        }

        /// <summary>
        /// Translates the string representing the element. (also handles any nodraw text input)
        /// </summary>
        /// <param name="zDeck"></param>
        /// <param name="sRawString"></param>
        /// <param name="nCardIndex"></param>
        /// <param name="zDeckLine"></param>
        /// <param name="zElement"></param>
        /// <returns></returns>
        protected override ElementString TranslateToElementString(Deck zDeck, string sRawString, int nCardIndex, DeckLine zDeckLine, ProjectLayoutElement zElement)
        {
#warning Investigate using method references instead of anonymous methods (optimization/code easier to read)

            var listLine = zDeckLine.LineColumns;
            var sOutput = sRawString;

            sOutput = sOutput.Replace("#empty", string.Empty);

            var zElementString = new ElementString();

            // TODO: maybe move these into classes so this isn't one mammoth blob

            LogTranslation(zElement, sOutput);

            // Translate card variables (non-reference information
            // Groups
            //     1    2    3   4   5
            // @"(.*)(!\[)(.+?)(\])(.*)"
            Func<Match, string> funcCardVariableProcessor =
                (zMatch =>
                {
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
                    else if (sKey.Equals("cardcount"))
                    {
                        sDefineValue = zDeck.CardCount.ToString();
                    }
                    else if (sKey.Equals("elementname"))
                    {
                        sDefineValue = zElement.name;
                    }
                    else
                    {
                        IssueManager.Instance.FireAddIssueEvent("Bad card variable: " + sKey);
                        sDefineValue = "[BAD NAME: " + sKey + "]";
                    }

                    return zMatch.Groups[1] + sDefineValue + zMatch.Groups[5];
                });

            // Translate named items (column names / defines)
            //Groups
            //    1    2    3   4   5
            //@"(.*)(@\[)(.+?)(\])(.*)"
            Func<Match, string> funcDefineProcessor =
                zMatch =>
                {
                    int nIndex;
                    string sDefineValue;
                    var sKey = zMatch.Groups[3].ToString();

                    // check the key for define parameters
                    var arrayParams = sKey.Split(new char[] { ',' });
                    if (arrayParams.Length > 1)
                    {
                        sKey = arrayParams[0];
                    }

                    sKey = sKey.ToLower();

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
                    var result = zMatch.Groups[1] + sDefineValue + zMatch.Groups[5];
                    // perform the #empty replace every time a define is unwrapped
                    return result.Replace("#empty", string.Empty);
                };

            // Translate substrings (column names / defines)
            //Groups
            //    1  2    3    4  5    6  7    8   9  
            //@"(.*)(%\[)(.+?)(,)(\d+)(,)(\d+)(\])(.*)
            Func<Match, string> funcDefineSubstringProcessor =
                zMatch =>
                {
                    var sValue = zMatch.Groups[3].ToString();
                    int nStartIdx;
                    int nLength;
                    if (!int.TryParse(zMatch.Groups[5].ToString(), out nStartIdx) ||
                        !int.TryParse(zMatch.Groups[7].ToString(), out nLength))
                    {
                        sValue = "[Invalid substring parameters]";
                    }
                    else
                    {
                        sValue = sValue.Length >= nStartIdx + nLength
                            ? sValue.Substring(nStartIdx, nLength)
                            : "[Invalid substring requested]";
                    }


                    var result = zMatch.Groups[1] + sValue + zMatch.Groups[9];
                    // perform the #empty replace every time a define is unwrapped
                    return result.Replace("#empty", string.Empty);
                };

            // define and define substring processing
            sOutput = LoopTranslationMatchMap(sOutput, zElement,
                new Dictionary<Regex, Func<Match, string>>
                {
                    { s_regexColumnVariable, funcDefineProcessor},
                    { s_regexColumnVariableSubstring, funcDefineSubstringProcessor},
                    { s_regexCardVariable, funcCardVariableProcessor }
                });


            // Translate card counter/index
            // Groups                 
            //     1   2    3  4    5  6    7  8   9
            //(@"(.*)(##)(\d+)(;)(\d+)(;)(\d+)(#)(.*)");
            sOutput = LoopTranslateRegex(s_regexCardCounter, sOutput, zElement,
            (zMatch =>
            {
                var nStart = Int32.Parse(zMatch.Groups[3].ToString());
                var nChange = Int32.Parse(zMatch.Groups[5].ToString());
                var nLeftPad = Int32.Parse(zMatch.Groups[7].ToString());

                return zMatch.Groups[1] +
                    // nIndex is left as is (not adding 1)
                    (nStart + (nCardIndex * nChange)).ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0') +
                    zMatch.Groups[9];
            }));

            // Translate sub card counter/index
            // Groups                 
            //     1   2    3  4    5  6    7  8   9
            //(@"(.*)(#sc;)(\d+)(;)(\d+)(;)(\d+)(#)(.*)");
            sOutput = LoopTranslateRegex(s_regexSubCardCounter, sOutput, zElement,
            (zMatch =>
            {
                var nStart = Int32.Parse(zMatch.Groups[3].ToString());
                var nChange = Int32.Parse(zMatch.Groups[5].ToString());
                var nLeftPad = Int32.Parse(zMatch.Groups[7].ToString());

                var nIndex = zDeckLine.RowSubIndex;

                return zMatch.Groups[1] +
                    // nIndex is left as is (not adding 1)
                    (nStart + (nIndex * nChange)).ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0') +
                    zMatch.Groups[9];

            }));

            // Translate random number
            // Groups                 
            //    1  2         3      4  5      6  7
            //@"(.*)(#random;)(-?\d+)(;)(-?\d+)(#)(.*)"
            sOutput = LoopTranslateRegex(s_regexRandomNumber, sOutput, zElement,
                (zMatch =>
                {
                    int nMin;
                    int nMax;

                    if (!int.TryParse(zMatch.Groups[3].ToString(), out nMin) ||
                        !int.TryParse(zMatch.Groups[5].ToString(), out nMax))
                    {
                        return "Failed to parse random min/max";
                    }

                    if (nMin >= nMax)
                    {
                        return "Invalid random specified. Min >= Max";
                    }

                    // max is not inclusive 
                    return zMatch.Groups[1] + CardMakerInstance.Random.Next(nMin, nMax + 1).ToString() + zMatch.Groups[7];
                }));


            // Translate repeat
            // Groups                 
            //    1  2         3    4  5    6  7
            //@"(.*)(#repeat;)(\d+)(;)(.+?)(#)(.*)"
            sOutput = LoopTranslateRegex(s_regexRepeat, sOutput, zElement,
                (zMatch =>
                {
                    int nRepeatCount;
                    var zBuilder = new StringBuilder();
                    if (int.TryParse(zMatch.Groups[3].ToString(), out nRepeatCount))
                    {
                        for (var nIdx = 0; nIdx < nRepeatCount; nIdx++)
                        {
                            zBuilder.Append(zMatch.Groups[5].ToString());
                        }
                    }
                    else
                    {
                        Logger.AddLogLine("Unable to parse repeat count: " + zMatch.Groups[3].ToString());
                    }

                    return zMatch.Groups[1] + zBuilder.ToString() + zMatch.Groups[7];
                }));

            // Translate If Logic
            //Groups
            //    1     2    3    4   5 
            //@"(.*)(#\()(if.+)(\)#)(.*)");
            Func<Match, string> funcIfProcessor =
            (match =>
            {
                var sLogicResult = TranslateIfLogic(match.Groups[3].ToString());
                return match.Groups[1] +
                          sLogicResult +
                          match.Groups[5];
            });

            // Translate Switch Logic
            //Groups                  
            //    1    2         3    4   5 
            //@"(.*)(#\()(switch.+)(\)#)(.*)");
            Func<Match, string> funcSwitchProcessor =
            match =>
            {
                var sLogicResult = TranslateSwitchLogic(match.Groups[3].ToString());
                return match.Groups[1] +
                          sLogicResult +
                          match.Groups[5];
            };

            // if / switch processor
            sOutput = LoopTranslationMatchMap(sOutput, zElement,
                new Dictionary<Regex, Func<Match, string>>
                {
                    { s_regexIfLogic, funcIfProcessor},
                    { s_regexSwitchLogic, funcSwitchProcessor }
                });

            var dictionaryOverrideFieldToValue = new Dictionary<string, string>();
            // Override evaluation:
            // Translate card variables (non-reference information
            // Groups
            //     1     2    3    4    5   6
            // @"(.*)(\$\[)(.+?):(.+?)(\])(.*)
            sOutput = LoopTranslateRegex(s_regexElementOverride, sOutput, zElement,
                    (zMatch =>
                        {
                            var sField = zMatch.Groups[3].ToString().ToLower();
                            var sValue = zMatch.Groups[4].ToString();

                            if (!s_setDisallowedOverrideFields.Contains(sField))
                            {
                                // empty override values are discarded (matches reference overrides)
                                if (!string.IsNullOrWhiteSpace(sValue))
                                {
                                    dictionaryOverrideFieldToValue[sField] = sValue;
                                }
                            }
                            else
                            {
                                Logger.AddLogLine("[{1}] override not allowed on element: [{0}]".FormatString(zElement.name, sField));
                            }

                            return zMatch.Groups[1].Value + zMatch.Groups[6].Value;
                        }
                    ));

            zElementString.String = sOutput;
            zElementString.OverrideFieldToValueDictionary = dictionaryOverrideFieldToValue == null
                ? null
                : dictionaryOverrideFieldToValue;
            return zElementString;
        }

        private static string LoopTranslateRegex(Regex regex, string input, ProjectLayoutElement zElement, Func<Match, string> processFunc)
        {
            var sOut = input;
            var zMatch = regex.Match(sOut);
            var nTranslationLoopCount = 0;
            while (zMatch.Success)
            {
                if (nTranslationLoopCount > MAX_TRANSLATION_LOOP_COUNT)
                {
                    Logger.AddLogLine("Distrupting traslation loop. It appears to be an endless loop.");
                    break;
                }

                sOut = TranslateMatch(zMatch, zElement, processFunc);
                zMatch = regex.Match(sOut);

                nTranslationLoopCount++;
            }
            return sOut;
        }

        private static string LoopTranslationMatchMap(string sInput, ProjectLayoutElement zElement,
            Dictionary<Regex, Func<Match, string>> dictionaryMatchFuncs)
        {
            var nTranslationLoopCount = 0;
            var sOutput = sInput;
            while (true)
            {
                if (nTranslationLoopCount > MAX_TRANSLATION_LOOP_COUNT)
                {
                    Logger.AddLogLine("Distrupting traslation loop. It appears to be an endless loop.");
                    break;
                }
                Match zCurrentMatch = null;
                Func<Match, string> zCurrentFunc = null;

                foreach (var zKeyValue in dictionaryMatchFuncs)
                {
                    var zMatch = zKeyValue.Key.Match(sOutput);
                    if (zMatch.Success)
                    {
#warning PREFERS RIGHT MOST using group 2
                        if (null != zCurrentMatch &&
                            zCurrentMatch.Groups[2].Index > zMatch.Groups[2].Index)
                        {
                            continue;
                        }
                        zCurrentMatch = zMatch;
                        zCurrentFunc = zKeyValue.Value;
                    }
                }

                if (zCurrentMatch == null)
                {
                    break;
                }

                sOutput = TranslateMatch(zCurrentMatch, zElement, zCurrentFunc);
                nTranslationLoopCount++;
            }
            return sOutput;
        }

        private static string TranslateMatch(Match zMatch, ProjectLayoutElement zElement, Func<Match, string> processFunc)
        {
            var sOut = processFunc(zMatch);
            LogTranslation(zElement, sOut);
            return sOut;
        }

        private static void LogTranslation(ProjectLayoutElement zElement, string sOut)
        {
            var sLog = "Translate[{0}] {1}".FormatString(zElement.name, sOut);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(sLog);
#else
            if (CardMakerSettings.LogInceptTranslation)
            {
                Logger.AddLogLine(sLog);
            }
#endif
        }

        private string TranslateIfLogic(string sInput)
        {
            //Groups
            //    1    2  3            4    5   6
            //@"(if)(.*?)([!=><]=|<|>)(.*?)(then )(.*)");
            //Groups                                   
            //    1    2  3            4    5      6    7      8
            //@"(if)(.*?)([!=><]=|<|>)(.*?)(then )(.*?)( else )(.*)");
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
                    bool bSuccess = ParseUtil.ParseDecimal(zIfMatch.Groups[2].ToString(), out nValue1);
                    bSuccess &= ParseUtil.ParseDecimal(zIfMatch.Groups[4].ToString(), out nValue2);
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
            //      1  2    3  4   5
            //(switch)(;)(.*?)(;)(.*)");
            // OR
            //      1   2    3   4   5
            //(switch)(::)(.*?)(::)(.*)
            var nDefaultIndex = -1;
            Match zSwitchMatch = null;
            var arraySwitchDelimiter = ArraySwitchDelimiter;
            if (s_regexSwitchStatement.IsMatch(sInput))
            {
                zSwitchMatch = s_regexSwitchStatement.Match(sInput);
            }
            else if (s_regexSwitchStatementAlt.IsMatch(sInput))
            {
                zSwitchMatch = s_regexSwitchStatementAlt.Match(sInput);
                arraySwitchDelimiter = ArraySwitchDelimiterAlt;
            }
            if (zSwitchMatch == null)
            {
                return sInput;
            }

            var sKey = zSwitchMatch.Groups[3].ToString();
            var arrayCases = zSwitchMatch.Groups[5].ToString().Split(arraySwitchDelimiter, StringSplitOptions.None);
            if (0 != (arrayCases.Length % 2))
            {
                return sInput;
            }

            var nIdx = 0;
            while (nIdx < arrayCases.Length)
            {
                if (arrayCases[nIdx].Equals(SWITCH_DEFAULT, StringComparison.CurrentCultureIgnoreCase))
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

            return sInput;
        }
    }
}
