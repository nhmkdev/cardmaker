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

using CardMaker.Card;
using CardMaker.XML;
using System.Collections.Generic;
using CardMaker.Card.Translation;

namespace UnitTest.DeckObject
{
    internal class TestDeck : Deck
    {
        public TestDeck()
        {
            CardLayout = new ProjectLayout()
            {
                defaultCount = 10
            };
        }

        public TestDeck(ITranslatorFactory zTranslatorFactory)
        {
            CardLayout = new ProjectLayout()
            {
                defaultCount = 10
            };
            TranslatorFactory = zTranslatorFactory;
        }

        public void SetCardIndex(int idx)
        {
            m_nCardIndex = idx;
        }

        public void ProcessLinesPublic(List<List<string>> listLines,
            List<List<string>> listDefineLines,
            string sReferencePath)
        {
            ProcessLines(listLines, listDefineLines, "unittestref");
        }

        public string GetDefine(string key)
        {
            string value;
            return m_zTranslator.DictionaryDefines.TryGetValue(key, out value) ? value : null;
        }

        public void SetDisallowedCharReplacement(char c, string replacement)
        {
            FilenameTranslator.CharReplacement[c] = replacement;
        }
    }
}
