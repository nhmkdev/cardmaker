////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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
using CardMaker.Card.Import;
using CardMaker.Data;
using CardMaker.Events.Managers;
using Moq;
using Support.Progress;

namespace UnitTest.DeckObject
{
    // TODO: "bad name" test!

    [TestFixture]
    internal class JavaScriptStringTranslation
    {
        private const string TEST_ELEMENT_NAME = "testElement";

        private TestDeck _testDeck;
        private DeckLine _testLine;
        private ProjectLayoutElement _testElement;
        private Mock<ProgressReporterProxy> _mockProgressReporterProxy;

        [SetUp]
        public void Setup()
        {
            // JavaScriptTranslator uses a few settings from the project file
            ProjectManager.Instance.OpenProject(null);
            _mockProgressReporterProxy = new Mock<ProgressReporterProxy>();
            _testDeck = new TestDeck(new JavaScriptTranslatorFactory());
            _testDeck.SetProgressReporterProxy(_mockProgressReporterProxy.Object);
            _testLine = new DeckLine(ReferenceLine.CreateDefaultInternalReferenceLine(new List<string>()));
            _testElement = new ProjectLayoutElement(TEST_ELEMENT_NAME);
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

        [TestCase("'sample string with nothing special.'", ExpectedResult = "sample string with nothing special.")]
        public string ValidateNonTranslate(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            Assert.IsTrue(result.DrawElement);
            return result.String;
        }

        [TestCase("'#nodraw'", false, ExpectedResult = "#nodraw")]
        [TestCase("'Test: #nodraw'", false, ExpectedResult = "Test: #nodraw")]
        [TestCase("'#nodraw [test]'", false, ExpectedResult = "#nodraw [test]")]
        public string ValidateNoDraw(string input, bool expectedDrawElement)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            Assert.AreEqual(expectedDrawElement, result.DrawElement);
            return result.String;
        }

        // x + (current card index * y) with left padded 0's numbering z
        [TestCase("cardIndex", 0, ExpectedResult = "1")]
        [TestCase("cardIndex - 1", 0, ExpectedResult = "0")]
        public string ValidateCounter(string input, int cardIndex)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            _testDeck.SetCardIndex(cardIndex);
            return _testDeck.TranslateString(input, _testLine, _testElement, false).String;
        }

        [TestCase("a", "3", ExpectedResult = "3")]
        [TestCase("3", "a", ExpectedResult = "")] // error case in js
        [TestCase("", "a", ExpectedResult = "")] // error case in js
        [TestCase("-", "a", ExpectedResult = "")] // error case in js
        [TestCase(" ", "a", ExpectedResult = "")] // TODO: this is not a valid key and should throw an error in processing
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

        [TestCase("x", "x" , ExpectedResult = 1)]
        [TestCase("X", "x", ExpectedResult = 1)]
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

        [TestCase(1, 0, ExpectedResult = 1)]
        [TestCase(0, 1, ExpectedResult = 1)]
        [TestCase(1, 1, ExpectedResult = 2)]
        [TestCase(5, 32, ExpectedResult = 37)]
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

        [TestCase("'\\\\c'", ExpectedResult = ",")]
        [TestCase("'\\\\q'", ExpectedResult = "\"")]
        [TestCase("'&gt;'", ExpectedResult = ">")]
        [TestCase("'&lt;'", ExpectedResult = "<")]
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

        [TestCase("'\\\\c'", ExpectedResult = "\\c")]
        [TestCase("'\\\\q'", ExpectedResult = "\\q")]
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

        [TestCase("j + k", ExpectedResult = "ab")]
        [TestCase("m + k + j", ExpectedResult = "bbbba")]
        [TestCase("n", ExpectedResult = "bbb")]
        [TestCase("n + ' at the ' + j + ' end test ' + n", ExpectedResult = "bbb at the a end test bbb")]
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

        [TestCase("action('aa', 'bb')", ExpectedResult = "aa::bb")]
        [TestCase("action(l4, 'bb')", ExpectedResult = "bbb::bb")]
        [TestCase("action('','')", ExpectedResult = @"::")]
        [TestCase("actioncaller(l1)", ExpectedResult = "a::zork")]
        [TestCase("smallimgtag(thecoin)", ExpectedResult = @"<img=\images\coin.png;.90;0;3>")]
        [TestCase("smallimgtag('')", ExpectedResult = @"<img=;.90;0;3>")]
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

        [Test]
        public void ValidateDefinitionBasedOverrides()
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>(),
                new List<List<string>>(),
                null);
            const string RESULT = "RESULT";
            const int X = 45;
            const int Y = 55;
            const int WIDTH = 65;
            const int HEIGHT = 75;
            const int BORDERTHICKNESS = 85;
            const int OPACITY = 95;
            const int OUTLINETHICKNESS = 105;
            const int LINEHEIGHT = 110;
            const int WORDSPACE = 115;
            const bool AUTOSCALEFONT = true;
            const bool ENABLED = false;
            const bool LOCKASPECT = true;
            const string TILESIZE = "45x55";
            const bool KEEPORIGINALSIZE = true;
            const bool JUSTIFIEDTEXT = true;
            const float ROTATION = 55;
            const string BORDERCOLOR = "0xFF0000FF";
            const string ELEMENTCOLOR = "0x00FF00FF";
            const string OUTLINECOLOR = "0x0000FFFF";
            const string FONT = "font";
            const int VERTICALALIGN = 120;
            const int HORIZONTALALIGN = 125;
            const string TYPE = "theType";

            // these can't be overriden
            const string VARIABLE = "theVariable";
            const string NAME = "newName";

            const string BASE_VARIABLE = "theVariable";

            _testElement.variable = BASE_VARIABLE;

            var zElementString = _testDeck.TranslateString(
                GetOverride("x", X) +
                GetOverride("y", Y) +
                GetOverride("width", WIDTH) +
                GetOverride("height", HEIGHT) +
                GetOverride("borderthickness", BORDERTHICKNESS) +
                GetOverride("opacity", OPACITY) +
                GetOverride("outlinethickness", OUTLINETHICKNESS) +
                GetOverride("lineheight", LINEHEIGHT) +
                GetOverride("wordspace", WORDSPACE) +
                GetOverride("autoscalefont", AUTOSCALEFONT) +
                GetOverride("enabled", ENABLED) +
                GetOverride("lockaspect", LOCKASPECT) +
                GetOverride("tilesize", TILESIZE) +
                GetOverride("keeporiginalsize", KEEPORIGINALSIZE) +
                GetOverride("justifiedtext", JUSTIFIEDTEXT) +
                GetOverride("rotation", ROTATION) +
                GetOverride("bordercolor", BORDERCOLOR) +
                GetOverride("elementcolor", ELEMENTCOLOR) +
                GetOverride("outlinecolor", OUTLINECOLOR) +
                GetOverride("font", FONT) +
                GetOverride("verticalalign", VERTICALALIGN) +
                GetOverride("horizontalalign", HORIZONTALALIGN) +
                GetOverride("type", TYPE) +
                // un-overrid-able
                GetOverride("name", NAME) +
                GetOverride("variable", VARIABLE) +
                $"'{RESULT}'"
                , _testLine, _testElement, false);
            Assert.NotNull(zElementString.OverrideFieldToValueDictionary);
            _testDeck.GetVariableOverrideElement(_testElement, zElementString.OverrideFieldToValueDictionary);


            Assert.AreEqual(X, _testElement.x);
            Assert.AreEqual(Y, _testElement.y);
            Assert.AreEqual(WIDTH, _testElement.width);
            Assert.AreEqual(HEIGHT, _testElement.height);
            Assert.AreEqual(BORDERTHICKNESS, _testElement.borderthickness);
            Assert.AreEqual(OPACITY, _testElement.opacity);
            Assert.AreEqual(OUTLINETHICKNESS, _testElement.outlinethickness);
            Assert.AreEqual(LINEHEIGHT, _testElement.lineheight);
            Assert.AreEqual(WORDSPACE, _testElement.wordspace);
            Assert.AreEqual(AUTOSCALEFONT, _testElement.autoscalefont);
            Assert.AreEqual(ENABLED, _testElement.enabled);
            Assert.AreEqual(LOCKASPECT, _testElement.lockaspect);
            Assert.AreEqual(TILESIZE, _testElement.tilesize);
            Assert.AreEqual(KEEPORIGINALSIZE, _testElement.keeporiginalsize);
            Assert.AreEqual(JUSTIFIEDTEXT, _testElement.justifiedtext);
            Assert.AreEqual(ROTATION, _testElement.rotation);
            Assert.AreEqual(BORDERCOLOR, _testElement.bordercolor);
            Assert.AreEqual(ELEMENTCOLOR, _testElement.elementcolor);
            Assert.AreEqual(OUTLINECOLOR, _testElement.outlinecolor);
            Assert.AreEqual(FONT, _testElement.font);
            Assert.AreEqual(VERTICALALIGN, _testElement.verticalalign);
            Assert.AreEqual(HORIZONTALALIGN, _testElement.horizontalalign);
            Assert.AreEqual(TYPE, _testElement.type);

            // these should be unchanged
            Assert.AreEqual(BASE_VARIABLE, _testElement.variable);
            Assert.AreEqual(TEST_ELEMENT_NAME, _testElement.name);

            Assert.AreEqual(RESULT, zElementString.String);
        }

        [Test]
        public void ValidateDefinitionBasedEmptyOverride()
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>(),
                new List<List<string>>(),
                null);

            const string RESULT = "RESULT";

            const string BASE_VARIABLE = "theVariable";

            _testElement.variable = BASE_VARIABLE;

            var zElementString = _testDeck.TranslateString(
                GetOverride("x", string.Empty) + 
                $"'{RESULT}'"
                , _testLine, _testElement, false);
            Assert.True(0 == zElementString.OverrideFieldToValueDictionary.Count);
            _testDeck.GetVariableOverrideElement(_testElement, zElementString.OverrideFieldToValueDictionary);

            Assert.AreEqual(RESULT, zElementString.String);
        }

        private string GetOverride(string sOverrideField, object zOverrideValue)
        {
            return $"AddOverrideField('{sOverrideField}', '{zOverrideValue}');{Environment.NewLine}";
        }
        // graphic elements
    }
}
