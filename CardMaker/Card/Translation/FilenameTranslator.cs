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
using CardMaker.XML;

namespace CardMaker.Card.Translation
{
    public static class FilenameTranslator
    {
        private static readonly Regex s_regexColumnVariable = new Regex(@"(.*)(@\[)(.+?)(\])(.*)", RegexOptions.Compiled);

        private static readonly Dictionary<char, string> s_dictionaryCharReplacement = new Dictionary<char, string>();

        public static Dictionary<char, string> CharReplacement => s_dictionaryCharReplacement;

        public static readonly char[] DISALLOWED_FILE_CHARS_ARRAY = { '\\', '/', ':', '*', '?', '\"', '>', '<', '|' };

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

        /// <summary>
        /// Translates a file export string for naming a file
        /// </summary>
        /// <param name="sRawString"></param>
        /// <param name="nCardNumber"></param>
        /// <param name="nLeftPad"></param>
        /// <param name="zCurrentPrintLine"></param>
        /// <param name="dictionaryDefines"></param>
        /// <param name="dictionaryColumnNameToIndex"></param>
        /// <param name="zLayout"></param>
        /// <returns></returns>
        public static string TranslateFileNameString(string sRawString, int nCardNumber, int nLeftPad, DeckLine zCurrentPrintLine, Dictionary<string, string> dictionaryDefines,
            Dictionary<string, int> dictionaryColumnNameToIndex, ProjectLayout zLayout)
        {
            string sOutput = sRawString;
            var listLine = zCurrentPrintLine.LineColumns;

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
                if (dictionaryDefines.TryGetValue(sKey, out sDefineValue))
                {
                    sOutput = zMatch.Groups[1] + sDefineValue.Trim() + zMatch.Groups[5];
                }
                else if (dictionaryColumnNameToIndex.TryGetValue(sKey, out nIndex))
                {
                    sOutput = zMatch.Groups[1] + listLine[nIndex].Trim() + zMatch.Groups[5];
                }
                else
                {
                    sOutput = zMatch.Groups[1] + "[UNKNOWN]" + zMatch.Groups[5];
                }
            }
            // replace ##, #L, Newlines
            sOutput = 
                sOutput.Replace("##", nCardNumber.ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0'))
                .Replace("#SC", (zCurrentPrintLine.RowSubIndex + 1).ToString(CultureInfo.InvariantCulture).PadLeft(nLeftPad, '0'))
                .Replace("#L", zLayout.Name)
                .Replace(Environment.NewLine, string.Empty);

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
    }
}
