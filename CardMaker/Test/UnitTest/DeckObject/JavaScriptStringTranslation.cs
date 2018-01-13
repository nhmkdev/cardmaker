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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using CardMaker.Data;

namespace UnitTest.DeckObject
{
    // TODO: "bad name" test!

    [TestFixture]
    internal class JavaScriptStringTranslation
    {
        private TestDeck _testDeck;
        private DeckLine _testLine;
        private ProjectLayoutElement _testElement;

        [SetUp]
        public void Setup()
        {
            _testDeck = new TestDeck(new JavaScriptTranslatorFactory());
            _testLine = new DeckLine(new List<string>());
            _testElement = new ProjectLayoutElement("testElement");
        }

        // this test is useless without the moq or something to detect cache
        //[Test]
        public void ValidateCache()
        {
            const string expected = "sample string with nothing special.";
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(expected, _testLine, _testElement, false);
            var secondResult = _testDeck.TranslateString(expected, _testLine, _testElement, false);
            // TODO: moq or something to validate the cache was actually hit
            Assert.AreEqual(result, secondResult);
        }

        [TestCase("'sample string with nothing special.'", Result = "sample string with nothing special.")]
        public string ValidateNonTranslate(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            Assert.IsTrue(result.DrawElement);
            return result.String;
        }

        [TestCase("'#nodraw'", false, Result = "#nodraw")]
        [TestCase("'Test: #nodraw'", false, Result = "Test: #nodraw")]
        [TestCase("'#nodraw [test]'", false, Result = "#nodraw [test]")]
        public string ValidateNoDraw(string input, bool expectedDrawElement)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            Assert.AreEqual(expectedDrawElement, result.DrawElement);
            return result.String;
        }

        // x + (current card index * y) with left padded 0's numbering z
        [TestCase("cardIndex", 0, Result = "1")]
        [TestCase("cardIndex - 1", 0, Result = "0")]
        public string ValidateCounter(string input, int cardIndex)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            _testDeck.SetCardIndex(cardIndex);
            return _testDeck.TranslateString(input, _testLine, _testElement, false).String;
        }

        [TestCase("a", "3", Result = "3")]
        [TestCase("3", "a", Result = "")] // error case in js
        [TestCase("", "a", Result = "")] // error case in js
        [TestCase("-", "a", Result = "")] // error case in js
        [TestCase(" ", "a", Result = "")] // TODO: this is not a valid key and should throw an error in processing
        public string ValidateColumnReference(string keyName, string keyValue)
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>()
                {
                    new List<string>(){keyName},
                    new List<string>() {keyValue}
                },
                new List<List<string>>(),
                null);
            return _testDeck.TranslateString(keyName, _testDeck.ValidLines[0], _testElement, false).String;
        }

        [TestCase("x", "x" , Result = 1)]
        [TestCase("X", "x", Result = 1)]
        public int ValidateAllowedLayout(string layoutName, string allowedLayout)
        {
            _testDeck.CardLayout.Name = layoutName;
            _testDeck.ProcessLinesPublic(
                new List<List<string>>()
                {
                    new List<string>(){"a", "allowed_layout"},
                    new List<string>() {"1", allowedLayout}
                },
                new List<List<string>>(),
                null);
            return _testDeck.CardCount;
        }

        [Test]
        public void ValidateNonAllowedLayout()
        {
            _testDeck.CardLayout.Name = "x";
            _testDeck.ProcessLinesPublic(
                new List<List<string>>()
                {
                    new List<string>(){"a", "allowed_layout"},
                    new List<string>() {"1", "y"}
                },
                new List<List<string>>(),
                null);
            Assert.AreEqual(_testDeck.CardLayout.defaultCount, _testDeck.CardCount);
        }

        [TestCase(1, 0, Result = 1)]
        [TestCase(0, 1, Result = 1)]
        [TestCase(1, 1, Result = 2)]
        [TestCase(5, 32, Result = 37)]
        public int ValidateCardCounts(int cardCountOne, int cardCountTwo)
        {
            var listLines = new List<List<string>>()
            {
                new List<string>() {"count", "a", "b"},
                new List<string>() {cardCountOne.ToString()},
                new List<string>() {cardCountTwo.ToString()}
            };
            _testDeck.ProcessLinesPublic(
                listLines,
                new List<List<string>>(),
                null);
            return _testDeck.CardCount;
        }

        [TestCase("'\\\\c'", Result=",")]
        [TestCase("'\\\\q'", Result = "\"")]
        [TestCase("'&gt;'", Result = ">")]
        [TestCase("'&lt;'", Result = "<")]
        public string ValidateSpecialCharacterTranslation(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            return result.String;
        }

        [Test]
        public void ValidateNewlineSpecialCharacterTranslation()
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString("'\\\\n'", _testLine, _testElement, false);
            Assert.AreEqual(Environment.NewLine, result.String);
        }

        [TestCase("'\\\\c'", Result = "\\c")]
        [TestCase("'\\\\q'", Result = "\\q")]
        public string ValidateSpecialCharacterNonTranslation(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            _testElement.type = ElementType.FormattedText.ToString();
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            return result.String;
        }

        [Test]
        public void ValidateNewlineSpecialCharacterNonTranslation()
        {
            const string slashN = "'\\n'";
            _testElement.type = ElementType.FormattedText.ToString();
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(slashN, _testLine, _testElement, false);
            Assert.AreEqual("\n", result.String);
        }

        [TestCase("a","1","b","2","c","3")]
        public void ValidateDefines(params string[] args)
        {
            var listLines = new List<List<string>>();
            var listDefines = new List<List<string>>() {new List<string>(){"define", "value"}};
            Assert.AreEqual(0, args.Length%2);
            for(var i = 0; i < args.Length; i+=2)
            {
                listDefines.Add(new List<string>(){args[i], args[i+1]});
            }
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);
            for (var i = 0; i < args.Length; i += 2)
            {
                Assert.AreEqual(_testDeck.GetDefine(args[i]), args[i + 1]);
            }
        }

        [TestCase("j + k", Result = "ab")]
        [TestCase("m + k + j", Result = "bbbba")]
        [TestCase("n", Result = "bbb")]
        [TestCase("n + ' at the ' + j + ' end test ' + n", Result = "bbb at the a end test bbb")]
        public string ValidateNestedDefines(string line)
        {
            // TODO: Nested defines with JavaScript is not possible at this time!

            var listLines = new List<List<string>>();
            var listDefines = new List<List<string>>()
            {
                new List<string>() { "define", "value" },
                new List<string>() { "j", "'a'" },//1
                new List<string>() { "k", "'b'" },//2
                new List<string>() { "l", "~k" },//3
                new List<string>() { "m", "~l + k + l" },//4
                new List<string>() { "n", "~this['m']" }//5
            };
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);
            var result = _testDeck.TranslateString(line, _testLine, _testElement, false);
            return result.String;
        }

        [TestCase("action('aa', 'bb')", Result = "aa::bb")]
        [TestCase("action(l4, 'bb')", Result = "bbb::bb")]
        [TestCase("action('','')", Result = @"::")]
        [TestCase("actioncaller(l1)", Result = "a::zork")]
        [TestCase("smallimgtag(thecoin)", Result = @"<img=\images\coin.png;.90;0;3>")]
        [TestCase("smallimgtag('')", Result = @"<img=;.90;0;3>")]
        public string ValidateParameterDefines(string line)
        {
            var listLines = new List<List<string>>();
            var listDefines = new List<List<string>>()
            {
                new List<string>() { "define", "value" },
                new List<string>() { "L1", "a" },
                new List<string>() { "L2", "b" },
                new List<string>() { "L3", "~l2" },
                new List<string>() { "L4", "~l3 + l2 + l3" },
                new List<string>() { "action", "function(x, y) { return x + '::' + y; }" },
                new List<string>() { "actionCaller", "function(x) { return action(x, 'zork'); }" },
                new List<string>() { "smallImgTag", "function(x) { return '<img=' + x + ';.90;0;3>'; }" },
                new List<string>() { "theCoin", @"'\\images\\coin.png'" }
            };
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);
            _testElement.type = ElementType.FormattedText.ToString();
            var result = _testDeck.TranslateString(line, _testLine, _testElement, false);
            return result.String;
        }
        // graphic elements
    }
}
