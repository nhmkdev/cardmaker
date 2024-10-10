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
using System.Text;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Support.IO;

namespace CardMaker.Card.Translation
{
    public class JavaScriptTranslator : TranslatorBase
    {
        private const string FUNCTION_PREFIX = "function(";

        public JavaScriptTranslator(Dictionary<string, string> dictionaryDefines, List<string> listColumnNames)
            : base(dictionaryDefines, listColumnNames)
        {

        }

        protected override ElementString TranslateToElementString(Deck zDeck, string sRawString, int nCardIndex, DeckLine zDeckLine, ProjectLayoutElement zElement)
        {
            using (var host = InitializeHost(nCardIndex, zDeck, zDeckLine, zElement))
            {
                try
                {
                    var sValue = host.ScriptEngine.Evaluate(sRawString);
                    if (sValue is string || sValue is int || sValue is double)
                    {
                        return new ElementString()
                        {
                            String = sValue.ToString(),
                            OverrideFieldToValueDictionary = host.dictionaryOverrideFieldToValue
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

        private JavascriptHost InitializeHost(int nCardIndex, Deck zDeck, DeckLine zDeckLine, ProjectLayoutElement zElement)
        {
            var engine = new V8ScriptEngine();
            var hostFunctions = new JavascriptHost(engine, zElement, nCardIndex, zDeck, zDeckLine);
            engine.AddHostObject("host", Microsoft.ClearScript.HostItemFlags.GlobalMembers, hostFunctions);

            foreach (var kvp in DictionaryDefines)
            {
                engine.Global.SetProperty(kvp.Key, kvp.Value);
            }
            foreach (var kvp in zDeckLine.ColumnsToValues)
            {
                engine.Global.SetProperty(kvp.Key, kvp.Value);
            }

            return hostFunctions;
        }
    }
}
