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
using System.Text;
using CardMaker.XML;
using Microsoft.ClearScript.V8;
using Support.IO;

namespace CardMaker.Card.Translation
{
    class JavaScriptTranslator : TranslatorBase
    {
        private const string FUNCTION_PREFIX = "function(";

        public JavaScriptTranslator(Dictionary<string, int> dictionaryColumnNameToIndex, Dictionary<string, string> dictionaryDefines,
            Dictionary<string, Dictionary<string, int>> dictionaryElementOverrides, List<string> listColumnNames)
            : base(dictionaryColumnNameToIndex, dictionaryDefines, dictionaryElementOverrides, listColumnNames)
        {

        }

        protected override ElementString TranslateToElementString(string sRawString, int nCardIndex, DeckLine zDeckLine, ProjectLayoutElement zElement)
        {
            using (var engine = new V8ScriptEngine())
            {
                var sScript = GetJavaScript(nCardIndex, zDeckLine, sRawString);
                try
                {
                    var sValue = engine.Evaluate(sScript);
                    if (sValue is string)
                    {
                        return new ElementString()
                        {
                            String = (string)sValue
                        };
                    }
                    else
                    {
                        Logger.AddLogLine(sValue.GetType().ToString());
                    }
                }
                catch (Exception e)
                {
                    Logger.AddLogLine(e.Message);
                }
            }
            return new ElementString()
            {
                String = string.Empty
            };
        }

        private string GetJavaScript(int nCardIndex, DeckLine zDeckLine, string sDefintion)
        {
            var zBuilder = new StringBuilder();
            if (string.IsNullOrWhiteSpace(sDefintion))
            {
                return "''";
            }

            AddVar(zBuilder, "deckIndex", (nCardIndex + 1).ToString());
            AddVar(zBuilder, "cardIndex", (zDeckLine.RowSubIndex + 1).ToString());

            foreach (var sKey in DictionaryDefines.Keys)
            {
                AddVar(zBuilder, sKey, ConvertQuoteEscape(DictionaryDefines[sKey]));
            }

            for (int nIdx = 0; nIdx < ListColumnNames.Count; nIdx++)
            {
                AddVar(zBuilder, ListColumnNames[nIdx], ConvertQuoteEscape(zDeckLine.LineColumns[nIdx]));
            }
            zBuilder.Append(sDefintion);
            return zBuilder.ToString();
        }

        private string ConvertQuoteEscape(string sInput)
        {
            if (sInput.StartsWith("~'"))
            {
                return sInput.Substring(1);
            }
            return sInput;
        }

        private void AddVar(StringBuilder zBuilder, string sVar, string sValue)
        {
            zBuilder.Append("this.");
            zBuilder.Append(sVar.Replace(' ', '_'));
            zBuilder.Append("=");
            // functions or single quoted items are left as-is
            // note this does not tolerate (whitespace)'
            if (sValue.StartsWith(FUNCTION_PREFIX) || sValue.StartsWith("'"))
            {
                zBuilder.Append(sValue);
                zBuilder.AppendLine(";");
            }
            else
            {
                zBuilder.Append("'");
                zBuilder.Append(sValue);
                zBuilder.AppendLine("';");
            }
        }
    }
}
