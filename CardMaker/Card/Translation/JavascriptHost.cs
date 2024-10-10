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
using System.Drawing.Text;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Microsoft.ClearScript;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Translation
{
    public class JavascriptHost : IDisposable
    {
        private readonly ProjectLayoutElement m_zElement;
        private readonly int m_nCardIndex;
        private readonly Deck m_zDeck;
        private readonly DeckLine m_zDeckLine;

        [NoScriptAccess]
        public Dictionary<string, string> dictionaryOverrideFieldToValue = new Dictionary<string, string>();
        private bool disposedValue;

        [NoScriptAccess]
        public ScriptEngine ScriptEngine { get; }

        //javascript variables
#pragma warning disable IDE1006
        public int deckIndex => m_nCardIndex + 1;
        public int cardIndex => m_zDeckLine.RowSubIndex + 1;
        public int cardCount => m_zDeck.CardCount;
        public string elementName => m_zElement.name;
        public string layoutName => m_zDeck.CardLayout.Name;
        public string refName => m_zDeckLine.ReferenceLine == null ? "No reference info." : m_zDeckLine.ReferenceLine.Source;
        public string refLine => m_zDeckLine.ReferenceLine == null ? "No reference info." : m_zDeckLine.ReferenceLine.LineNumber.ToString();
        
#pragma warning restore IDE1006

        public JavascriptHost(ScriptEngine zEngine, ProjectLayoutElement zElement, int nCardIndex, Deck zDeck, DeckLine zDeckLine)
        {
            ScriptEngine = zEngine;
            m_zElement = zElement;
            m_nCardIndex = nCardIndex;
            m_zDeck = zDeck;
            m_zDeckLine = zDeckLine;
        }

        public int MeasureStringWidth(string sFont, string sText)
        {
            var zFont = ProjectLayoutElement.TranslateFontString(sFont);
            if (zFont == null)
            {
                IssueManager.Instance.FireAddIssueEvent("Invalid font string specified. Defaulting.");
                zFont = FontLoader.DefaultFont;
            }
            return TranslatorBase.MeasureDisplayStringWidth(zFont, sText);
        }

        public void AddOverrideField(string sField, string sValue)
        {
            if (TranslatorBase.IsDisallowedOverrideField(sField))
            {
                Logger.AddLogLine("[{1}] override not allowed on element: [{0}]".FormatString(m_zElement.name, sField));
            }
            // empty override values are discarded (matches reference overrides)
            else if (!string.IsNullOrWhiteSpace(sValue))
            {
                dictionaryOverrideFieldToValue[sField] = sValue;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ScriptEngine.Dispose();
                }

                disposedValue = true;
            }
        }

        [NoScriptAccess]
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
