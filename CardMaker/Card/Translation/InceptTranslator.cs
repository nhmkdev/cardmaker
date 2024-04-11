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
using System.Globalization;
using System.Linq;
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
        private static readonly Regex s_regexElementFields = new Regex(@"(.*)(\&\[)(.+?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexElementOverride = new Regex(@"(.*)(\$\[)(.+?):(.*?)(\])(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexCardCounter = new Regex(@"(.*)(##)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSubCardCounter = new Regex(@"(.*)(#sc;)(\d+)(;)(\d+)(;)(\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexRepeat = new Regex(@"(.*)(#repeat;)(\d+)(;)(.+?)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexRandomNumber = new Regex(@"(.*)(#random;)(-?\d+)(;)(-?\d+)(#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexMath =
            new Regex(@"(.*)(#math;)([+-]?[0-9]*[.]?[0-9]+)([+\-*/%])([+-]?[0-9]*[.]?[0-9]+)[;]?(.*?)(#)(.*)",
                RegexOptions.Compiled);
        private static readonly Regex s_regexIfLogic = new Regex(@"(.*)(#\()(if.*?)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchLogic = new Regex(@"(.*)(#\()(switch.*?)(\)#)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfThenElseStatement = new Regex(@"(if)(.*?)\s([!=><]=|<|>)\s(.*?)(then )(.*?)( else )(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexIfSet = new Regex(@"(\[)(.*?)(\])", RegexOptions.Compiled);
        private static readonly Regex s_regexSwitchStatement = new Regex(@"(switch)(;)(.*?)(;)(.*)", RegexOptions.Compiled);
        private static readonly Regex s_regexPadStatement = new Regex(@"(.*)(#pad)([l|r])(;)(\d+)(;)(\d+)(;)(.*?)(#)(.*)", RegexOptions.Compiled);

        private const string SWITCH = "switch";
        
        private const string SWITCH_DEFAULT = "#default";
        private const string SWITCH_KEY = "#switchkey";
        private const char ARRAY_DELIMITER = ';';
        private static readonly string[] ArrayDelimiter_string = new string[] { ARRAY_DELIMITER.ToString() };
        private static readonly char[] ArrayDelimiter_char = new char[] { ARRAY_DELIMITER };

        public InceptTranslator(Dictionary<string, int> dictionaryColumnNameToIndex,
            Dictionary<string, string> dictionaryDefines,
            List<string> listColumnNames)
            : base(dictionaryColumnNameToIndex, dictionaryDefines, listColumnNames)
        {

        }

        // Translation dictionaries (mapping regex + static method to call)

        private static readonly Dictionary<Regex, Func<Match, TranslationContext, string>> s_dictionaryTranslationFuncs =
            new Dictionary<Regex, Func<Match, TranslationContext, string>>
            {
                { s_regexColumnVariable, TranslateNamedVariables },
                { s_regexColumnVariableSubstring, TranslateSubstrings },
                { s_regexCardVariable, TranslateCardVariables }
            };

        private static readonly Dictionary<Regex, Func<Match, TranslationContext, string>> s_dictionaryLogicFuncs =
            new Dictionary<Regex, Func<Match, TranslationContext, string>>
            {
                { s_regexIfLogic, TranslateIf },
                { s_regexSwitchLogic, TranslateSwitch }
            };

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

            var sOutput = sRawString;
            sOutput = sOutput.Replace("#empty", string.Empty);
            var zElementString = new ElementString();
            LogTranslation(zElement, sOutput);
            var zTranslationContext = new TranslationContext()
            {
                CardIndex = nCardIndex,
                Deck = zDeck,
                DeckLine = zDeckLine,
                Element = zElement,
                ElementString = zElementString,
                TranslatorBase = this
            };

            // define and define substring processing
            sOutput = LoopTranslationMatchMap(sOutput, zTranslationContext, s_dictionaryTranslationFuncs);

            // Translate card counter/index
            sOutput = LoopTranslateRegex(s_regexCardCounter, sOutput, zTranslationContext, TranslateCardCounter);

            // Translate sub card counter/index
            sOutput = LoopTranslateRegex(s_regexSubCardCounter, sOutput, zTranslationContext, TranslateSubCardCounter);

            // Translate element fields
            sOutput = LoopTranslateRegex(s_regexElementFields, sOutput, zTranslationContext, TranslateElementFields);

            // Translate random number
            sOutput = LoopTranslateRegex(s_regexRandomNumber, sOutput, zTranslationContext, TranslationRandomNumber);

            // Translate math (float support)
            sOutput = LoopTranslateRegex(s_regexMath, sOutput, zTranslationContext, TranslateMath);

            // Translate repeat
            sOutput = LoopTranslateRegex(s_regexRepeat, sOutput, zTranslationContext, TranslateRepeat);

            // Translate padding
            sOutput = LoopTranslateRegex(s_regexPadStatement, sOutput, zTranslationContext, TranslatePadding);

            // if / switch processor
            sOutput = LoopTranslationMatchMap(sOutput, zTranslationContext, s_dictionaryLogicFuncs);

            // this is used by TranslateOverrides
            zElementString.OverrideFieldToValueDictionary = new Dictionary<string, string>();
            
            // override processing
            sOutput = LoopTranslateRegex(s_regexElementOverride, sOutput, zTranslationContext, TranslateOverrides);

            zElementString.String = sOutput;
            return zElementString;
        }

        private static Dictionary<string, Func<TranslationContext, string>> s_dictTranslateCardVariables =
            new Dictionary<string, Func<TranslationContext, string>>()
            {
                {"deckindex", (zTranslationContext) =>  (zTranslationContext.CardIndex + 1).ToString()},
                {"cardindex", (zTranslationContext) =>  (zTranslationContext.DeckLine.RowSubIndex + 1).ToString()},
                {"cardcount", (zTranslationContext) =>  zTranslationContext.Deck.CardCount.ToString() },
                {"elementname", (zTranslationContext) => zTranslationContext.Element.name },
                {"refname", (zTranslationContext) => zTranslationContext.DeckLine.ReferenceLine == null
                    ? "No reference info."
                    : zTranslationContext.DeckLine.ReferenceLine.Source },
                {"refline", (zTranslationContext) => zTranslationContext.DeckLine.ReferenceLine == null
                    ? "No reference info."
                    : zTranslationContext.DeckLine.ReferenceLine.LineNumber.ToString() },
                {"layoutname", (zTranslationContext) => zTranslationContext.Deck.CardLayout.Name },
                {"parentlayout", (zTranslationContext) =>  zTranslationContext.Deck.SubLayoutExportContext?.ParentLayoutName ?? string.Empty },
                {"rootlayout", (zTranslationContext) =>  zTranslationContext.Deck.SubLayoutExportContext?.RootLayoutName ?? string.Empty },
                {"exporting", (zTranslationContext) =>  null == zTranslationContext.Deck.ExportContext 
                    ? string.Empty
                    : "1"
                },
                {
                    "exportformat", (zTranslationContext) =>  null == zTranslationContext.Deck.ExportContext
                    ? string.Empty
                    : zTranslationContext.Deck.ExportContext.GetExportImageFormatString()
                },
            };

        private static string TranslateCardVariables(Match zMatch, TranslationContext zTranslationContext)
        {
            // Translate card variables (non-reference information)
            // Groups
            //     1    2    3   4   5
            // @"(.*)(!\[)(.+?)(\])(.*)"
            string sDefineValue;
            var sKey = zMatch.Groups[3].ToString().ToLower();

            if (!s_dictTranslateCardVariables.ContainsKey(sKey))
            {
                IssueManager.Instance.FireAddIssueEvent("Bad card variable: " + sKey);
                sDefineValue = "[BAD NAME: " + sKey + "]";
            }
            else
            {
                sDefineValue = s_dictTranslateCardVariables[sKey](zTranslationContext);
            }

            return zMatch.Groups[1] + sDefineValue + zMatch.Groups[5];
        }

        private static string TranslateNamedVariables(Match zMatch, TranslationContext zTranslationContext)
        {
            // Translate named items (column names / defines)
            //Groups
            //    1    2    3   4   5
            //@"(.*)(@\[)(.+?)(\])(.*)"
            int nIndex;
            string sNamedValue;
            var sKey = zMatch.Groups[3].ToString();

            // check the key for define parameters
            var arrayParams = sKey.Split(new char[] { ',' });
            if (arrayParams.Length > 1)
            {
                sKey = arrayParams[0];
            }

            sKey = sKey.ToLower();

            if (zTranslationContext.Deck.GetDefineValue(
                    sKey, 
                    zTranslationContext.TranslatorBase.DictionaryDefines, 
                    out sNamedValue))
            {
            }
            else if(zTranslationContext.Deck.GetColumnValue(
                        sKey,
                        zTranslationContext.DeckLine.ColumnsToValues, 
                        out sNamedValue))
            {
            }
            else
            {
                IssueManager.Instance.FireAddIssueEvent("Bad reference name: " + sKey);
                sNamedValue = "[BAD NAME: " + sKey + "]";
            }
            if (arrayParams.Length > 1)
            {
                for (int nIdx = 1; nIdx < arrayParams.Length; nIdx++)
                {
                    sNamedValue = sNamedValue.Replace("{" + nIdx + "}", arrayParams[nIdx]);
                }
            }
            var result = zMatch.Groups[1] + sNamedValue + zMatch.Groups[5];
            // perform the #empty replace every time a define is unwrapped
            return result.Replace("#empty", string.Empty);
        }

        private static string TranslateSubstrings(Match zMatch, TranslationContext zTranslationContext)
        {
            // Translate substrings (column names / defines)
            //Groups
            //    1  2    3    4  5    6  7    8   9  
            //@"(.*)(%\[)(.+?)(,)(\d+)(,)(\d+)(\])(.*)
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
                    : string.Empty;
            }


            var result = zMatch.Groups[1] + sValue + zMatch.Groups[9];
            // perform the #empty replace every time a define is unwrapped
            return result.Replace("#empty", string.Empty);
        }

        private static string TranslateCardCounter(Match zMatch, TranslationContext zTranslationContext)
        {
            // Groups                 
            //     1   2    3  4    5  6    7  8   9
            //(@"(.*)(##)(\d+)(;)(\d+)(;)(\d+)(#)(.*)");
            var nStart = Int32.Parse(zMatch.Groups[3].ToString());
            var nChange = Int32.Parse(zMatch.Groups[5].ToString());
            var nLeftPad = Int32.Parse(zMatch.Groups[7].ToString());

            return zMatch.Groups[1] +
                   // nIndex is left as is (not adding 1)
                   (nStart + (zTranslationContext.CardIndex * nChange)).ToString(CultureInfo.InvariantCulture)
                   .PadLeft(nLeftPad, '0') +
                   zMatch.Groups[9];
        }

        private string TranslateSubCardCounter(Match zMatch, TranslationContext zTranslationContext)
        {
            // Groups                 
            //     1   2    3  4    5  6    7  8   9
            //(@"(.*)(#sc;)(\d+)(;)(\d+)(;)(\d+)(#)(.*)");
            var nStart = Int32.Parse(zMatch.Groups[3].ToString());
            var nChange = Int32.Parse(zMatch.Groups[5].ToString());
            var nLeftPad = Int32.Parse(zMatch.Groups[7].ToString());

            var nIndex = zTranslationContext.DeckLine.RowSubIndex;

            return zMatch.Groups[1] +
                   // nIndex is left as is (not adding 1)
                   (nStart + (nIndex * nChange)).ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0') +
                   zMatch.Groups[9];

        }

        private string TranslationRandomNumber(Match zMatch, TranslationContext zTranslationContext)
        {
            // Groups                 
            //    1  2         3      4  5      6  7
            //@"(.*)(#random;)(-?\d+)(;)(-?\d+)(#)(.*)"
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
            return zMatch.Groups[1] + CardMakerInstance.Random.Next(nMin, nMax + 1).ToString() +
                   zMatch.Groups[7];
        }

        private string TranslateMath(Match zMatch, TranslationContext zTranslationContext)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
            // Groups
            //   1   2            3                  4         5                           6    7  8
            //@"(.*)(#math;)([+-]?[0-9]*[.,]?[0-9]+)([+\-*/%])([+-]?[0-9]*[.,]?[0-9]+)[;]?(.*?)(#)(.*)"
            var sResult = "";
            if (ParseUtil.ParseFloat(zMatch.Groups[3].ToString(), out var fAValue) &&
                ParseUtil.ParseFloat(zMatch.Groups[5].ToString(), out var fBValue))
            {
                try
                {
                    var sFormat = zMatch.Groups[6].ToString();
                    var bUseFormat = !string.IsNullOrWhiteSpace(sFormat);
                    float fResult = 0;
                    switch (zMatch.Groups[4].ToString()[0])
                    {
                        case '+':
                            fResult = fAValue + fBValue;
                            break;
                        case '-':
                            fResult = fAValue - fBValue;
                            break;
                        case '*':
                            fResult = fAValue * fBValue;
                            break;
                        case '/':
                            if (fBValue == 0)
                            {
                                throw new Exception("Cannot divide by zero.");
                            }
                            fResult = fAValue / fBValue;
                            break;
                        case '%':
                            fResult = fAValue % fBValue;
                            break;
                    }

                    sResult = bUseFormat
                        ? fResult.ToString(sFormat)
                        : fResult.ToString();
                }
                catch (Exception e)
                {
                    Logger.AddLogLine("Math float translator failed: {0}".FormatString(e));
                }
            }
            return zMatch.Groups[1] + sResult + zMatch.Groups[8];
        }

        private string TranslateRepeat(Match zMatch, TranslationContext zTranslationContext)
        {
            // Groups                 
            //    1  2         3    4  5    6  7
            //@"(.*)(#repeat;)(\d+)(;)(.+?)(#)(.*)"
            int nRepeatCount;
            var zBuilder = new StringBuilder();
            if (int.TryParse(zMatch.Groups[3].ToString(), out nRepeatCount))
            {
                for (var nIdx = 0; nIdx < nRepeatCount; nIdx++)
                {
                    zBuilder.Append(zMatch.Groups[5]);
                }
            }
            else
            {
                Logger.AddLogLine("Unable to parse repeat count: " + zMatch.Groups[3]);
            }

            return zMatch.Groups[1] + zBuilder.ToString() + zMatch.Groups[7];
        }

        private static string TranslatePadding(Match zMatch, TranslationContext zTranslationContext)
        {
            // Translate padding
            //Groups
            // 1   2     3      4  5    6  7  8  9    10 11
            //(.*)(#pad)([l|r])(;)(\d+)(;)(.)(;)(.*?)(#)(.*)

            var sPadded = zMatch.Groups[3].ToString()[0] == 'l'
                ? zMatch.Groups[9].ToString()
                    .PadLeft(int.Parse(zMatch.Groups[5].ToString()), zMatch.Groups[7].ToString()[0])
                : zMatch.Groups[9].ToString()
                    .PadRight(int.Parse(zMatch.Groups[5].ToString()), zMatch.Groups[7].ToString()[0]);

            return zMatch.Groups[1] + sPadded + zMatch.Groups[11];
        }

        private static string TranslateIf(Match zMatch, TranslationContext zTranslationContext)
        {
            // Translate If Logic
            //Groups
            //    1     2    3    4   5 
            //@"(.*)(#\()(if.+)(\)#)(.*)");
            return zMatch.Groups[1] +
                   TranslateIfLogic(zMatch.Groups[3].ToString()) +
                   zMatch.Groups[5];
        }

        private static string TranslateSwitch(Match zMatch, TranslationContext zTranslationContext)
        {
            // Translate Switch Logic
            //Groups                  
            //    1    2         3    4   5 
            //@"(.*)(#\()(switch.+)(\)#)(.*)");
            return zMatch.Groups[1] +
                   TranslateSwitchLogic(zMatch.Groups[3].ToString()) +
                   zMatch.Groups[5];
        }

        private string TranslateElementFields(Match zMatch, TranslationContext zTranslationContext)
        {
            // Override evaluation:
            // Translate element fields
            // Groups
            //    1   2     3    4   5 
            // @"(.*)(\&\[)(.+?)(\])(.*)"
            var sField = zMatch.Groups[3].ToString().ToLower();

            var sTranslated = string.Empty;

            if (IsDisallowedReadField(sField))
            {
                sTranslated = FIELD_READ_DISALLOWED;
                Logger.AddLogLine(
                    "[{1}] override not allowed on element: [{0}]".FormatString(zTranslationContext.Element.name, sField));
            }
            else
            {
                var zProperty = typeof(ProjectLayoutElement).GetProperty(sField);
                if (null != zProperty && zProperty.CanRead)
                {
                    var zMethod = zProperty.GetGetMethod();
                    var zObj = zMethod.Invoke(zTranslationContext.Element, null);
                    sTranslated = zObj.ToString();
                }
                else
                {
                    sTranslated = INVALID_FIELD_READ;
                }
            }

            return zMatch.Groups[1].Value + sTranslated + zMatch.Groups[5].Value;
        }

        private string TranslateOverrides(Match zMatch, TranslationContext zTranslationContext)
        {
            // Override evaluation:
            // Translate card variables (non-reference information)
            // Groups
            //     1     2    3    4    5   6
            // @"(.*)(\$\[)(.+?):(.+?)(\])(.*)
            var sField = zMatch.Groups[3].ToString().ToLower();
            var sValue = zMatch.Groups[4].ToString();

            if (IsDisallowedOverrideField(sField))
            {
                Logger.AddLogLine(
                    "[{1}] override not allowed on element: [{0}]".FormatString(zTranslationContext.Element.name, sField));
            }
            // empty override values are discarded (matches reference overrides)
            else if (!string.IsNullOrWhiteSpace(sValue))
            {
                zTranslationContext.ElementString.OverrideFieldToValueDictionary[sField] = sValue;
            }

            return zMatch.Groups[1].Value + zMatch.Groups[6].Value;
        }

        private static string LoopTranslateRegex(Regex regex, string input, TranslationContext zTranslationContext,
            Func<Match, TranslationContext, string> processFunc)
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

                sOut = TranslateMatch(zMatch, zTranslationContext, processFunc);
                zMatch = regex.Match(sOut);

                nTranslationLoopCount++;
            }
            return sOut;
        }

        private static string LoopTranslationMatchMap(string sInput, TranslationContext zTranslationContext,
            Dictionary<Regex, Func<Match, TranslationContext, string>> dictionaryMatchFuncs)
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
                Func<Match, TranslationContext, string> zCurrentFunc = null;

                foreach (var zKeyValue in dictionaryMatchFuncs)
                {
                    var zMatch = zKeyValue.Key.Match(sOutput);
                    if (zMatch.Success)
                    {
                        // NOTE: Prefers right-most using group 2
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

                sOutput = TranslateMatch(zCurrentMatch, zTranslationContext, zCurrentFunc);
                nTranslationLoopCount++;
            }
            return sOutput;
        }

        private static string TranslateMatch(Match zMatch, TranslationContext zTranslationContext,
            Func<Match, TranslationContext, string> processFunc)
        {
            var sOut = processFunc(zMatch, zTranslationContext);
            LogTranslation(zTranslationContext.Element, sOut);
            return sOut;
        }

        private static string TranslateIfLogic(string sInput)
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
                    bool bCompare = CheckStringsForMatch(zIfMatch.Groups[2].ToString(), zIfMatch.Groups[4].ToString());
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

        private static bool CheckStringsForMatch(string sSet1, string sSet2)
        {
            var hSet1 = GetGroupSet(sSet1);
            var hSet2 = GetGroupSet(sSet2);
            return CheckSetsForMatch(hSet1, hSet2);
        }

        private static bool CheckSetsForMatch(HashSet<string> hs1, HashSet<string> hs2)
        {
            return hs1.FirstOrDefault(hs2.Contains) != null;
        }

        private static HashSet<string> GetGroupSet(string sSet)
        {
            var hSet = new HashSet<string>();
            // Groups                    
            //    1    2   3
            //@"(\[)(.*?)(\])");
            if (s_regexIfSet.IsMatch(sSet))
            {
                var zMatch = s_regexIfSet.Match(sSet);
                var arraySplit = zMatch.Groups[2].ToString().Split(ArrayDelimiter_char);
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

        private static string TranslateSwitchLogic(string sInput)
        {
            //Groups                                   
            //      1  2    3  4   5
            //(switch)(;)(.*?)(;)(.*)");
            // OR
            //      1   2    3   4   5
            //(switch)(**)(.*?)(::)(.*) << Note: ** is any character
            var nDefaultIndex = -1;
            string sKey = null;
            string[] arrayCases = null;
            if (s_regexSwitchStatement.IsMatch(sInput))
            {
                var zSwitchMatch = s_regexSwitchStatement.Match(sInput);
                sKey = zSwitchMatch.Groups[3].ToString();
                arrayCases = zSwitchMatch.Groups[5].ToString().Split(ArrayDelimiter_string, StringSplitOptions.None);
            }
            else
            {
                if (sInput.StartsWith(SWITCH) && sInput.Length > SWITCH.Length + 2)
                {
                    var arraySwitchDelimiter = new[] {sInput.Substring(SWITCH.Length, 2)};
                    var arraySplit = sInput.Split(arraySwitchDelimiter, StringSplitOptions.None);
                    if (arraySplit.Length > 2)
                    {
                        // now trim out the key
                        sKey = arraySplit[1];
                        arrayCases = new string[arraySplit.Length - 2];
                        Array.Copy(arraySplit, 2, arrayCases, 0, arrayCases.Length);
                    }
                }
            }
            if (sKey == null || arrayCases == null)
            {
                return sInput;
            }

            if (0 != (arrayCases.Length % 2))
            {
                // todo: log something?
                return sInput;
            }

            var nIdx = 0;
            var hsKey = GetGroupSet(sKey);
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
                if (CheckSetsForMatch(hsKey, GetGroupSet(arrayCases[nIdx])))
                {
                    return arrayCases[nIdx + 1];
                }
                nIdx += 2;
            }

            if (-1 < nDefaultIndex)
            {
                return arrayCases[nDefaultIndex + 1].Equals(SWITCH_KEY, StringComparison.CurrentCultureIgnoreCase)
                    ? sKey
                    : arrayCases[nDefaultIndex + 1];
            }

            return sInput;
        }

        private static void LogTranslation(ProjectLayoutElement zElement, string sOut)
        {
            var sLog = "Translate[{0}] {1}".FormatString(zElement.name, sOut);
#if DEBUG && FALSE
            System.Diagnostics.Debug.WriteLine(sLog);
#else
            if (CardMakerSettings.LogInceptTranslation)
            {
                Logger.AddLogLine(sLog);
            }
#endif
        }

        class TranslationContext
        {
            public Deck Deck { get; set; }
            public int CardIndex { get; set; }
            public DeckLine DeckLine { get; set; }
            public ProjectLayoutElement Element { get; set; }
            public ElementString ElementString { get; set; }
            public TranslatorBase TranslatorBase { get; set; }
        }
    }
}
