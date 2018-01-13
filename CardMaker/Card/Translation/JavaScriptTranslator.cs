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
using System.Text;
using CardMaker.XML;
using Microsoft.ClearScript.V8;
using Support.IO;

namespace CardMaker.Card.Translation
{
    public class JavaScriptTranslator : TranslatorBase
    {
        private const string FUNCTION_PREFIX = "function(";

        public JavaScriptTranslator(Dictionary<string, int> dictionaryColumnNameToIndex, Dictionary<string, string> dictionaryDefines,
            Dictionary<string, Dictionary<string, int>> dictionaryElementToFieldColumnOverrides, List<string> listColumnNames)
            : base(dictionaryColumnNameToIndex, dictionaryDefines, dictionaryElementToFieldColumnOverrides, listColumnNames)
        {

        }

        protected override ElementString TranslateToElementString(Deck zDeck, string sRawString, int nCardIndex, DeckLine zDeckLine, ProjectLayoutElement zElement)
        {
            using (var engine = new V8ScriptEngine())
            {
                var sScript = GetJavaScript(nCardIndex, zDeckLine, sRawString);
                try
                {
                    var sValue = engine.Evaluate(sScript);
                    if (sValue is string || sValue is int)
                    {
                        return new ElementString()
                        {
                            String = sValue.ToString()
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

            AddNumericVar(zBuilder, "deckIndex", (nCardIndex + 1).ToString());
            AddNumericVar(zBuilder, "cardIndex", (zDeckLine.RowSubIndex + 1).ToString());

            foreach (var sKey in DictionaryDefines.Keys)
            {
                AddVar(zBuilder, sKey, DictionaryDefines[sKey]);
            }

            for (int nIdx = 0; nIdx < ListColumnNames.Count; nIdx++)
            {
                AddVar(zBuilder, ListColumnNames[nIdx], zDeckLine.LineColumns[nIdx]);
            }
            zBuilder.Append(sDefintion);
            return zBuilder.ToString();
        }

        private void AddNumericVar(StringBuilder zBuilder, string sVar, string sValue)
        {
            zBuilder.Append("this.");
            zBuilder.Append(sVar.Replace(' ', '_'));
            zBuilder.Append("=");
            zBuilder.Append(sValue);
            zBuilder.AppendLine(";");
        }

        private void AddVar(StringBuilder zBuilder, string sVar, string sValue)
        {
            zBuilder.Append("this.");
            zBuilder.Append(sVar.Replace(' ', '_'));
            zBuilder.Append("=");
            // functions or single quoted items are left as-is
            // note this does not tolerate (whitespace)'
            if (sValue.StartsWith(FUNCTION_PREFIX))
            {
                zBuilder.AppendLine(sValue);
            }
            else if (sValue.StartsWith("'"))
            {
                zBuilder.Append(sValue);
                zBuilder.AppendLine(";");
            }
            else if (sValue.StartsWith("~"))
            {
                zBuilder.Append(sValue.Substring(1));
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
