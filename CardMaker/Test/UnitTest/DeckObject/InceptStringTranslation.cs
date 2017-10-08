using CardMaker.Card;
using CardMaker.XML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using CardMaker.Data;
using CardMaker.Events.Managers;
using Support.UI;

namespace UnitTest.DeckObject
{
    // TODO: "bad name" test!
    [TestFixture]
    internal class InceptStringTranslation
    {
        private const string TEST_ELEMENT_NAME = "testElement";

        private TestDeck _testDeck;
        private DeckLine _testLine;
        private ProjectLayoutElement _testElement;

        [SetUp]
        public void Setup()
        {
            _testDeck = new TestDeck();
            _testLine = new DeckLine(new List<string>());
            _testElement = new ProjectLayoutElement(TEST_ELEMENT_NAME);
        }

        [Test]
        public void ValidateCache()
        {
            const string expected = "sample string with nothing special.";
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(expected, _testLine, _testElement, false);
            var secondResult = _testDeck.TranslateString(expected, _testLine, _testElement, false);
            // TODO: moq or something to validate the cache was actually hit
            Assert.AreEqual(result, secondResult);
        }

        [TestCase("sample string with nothing special.", Result = "sample string with nothing special.")]
        public string ValidateNonTranslate(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            Assert.IsTrue(result.DrawElement);
            return result.String;
        }

        [TestCase("![cardIndex]", Result = "0")]
        [TestCase("![deckIndex]", Result = "1")]
        public string ValidateCardVariable(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            return result.String;
        }

        [TestCase("#(if a == a then word else word2)#", Result = "word")]
        [TestCase("#(if a == b then word else word2)#", Result = "word2")]
        [TestCase("#(if a == b then word else word2)# midword #(if c == d then chowder)#", Result = "word2 midword ")]
        [TestCase("#(if 2 > 0 then Deploy Roll: +2)# Deploy Success: +1pt#(switch;+1;+1;;-1;;#default;s)##(if DBA == then #nodraw)#", Result= "Deploy Roll: +2 Deploy Success: +1pt")]
        [TestCase("#(if 1.000 > 1 then A else B)#", Result = "B")]
        [TestCase("#(if 1,000 > 1 then A else B)#", Result = "B")]
        [TestCase("#(if 1.100 > 1 then A else B)#", Result = "A")]
        [TestCase("#(if 1,100 > 1 then A else B)#", Result = "A")]
        [TestCase("#(if 1.100 > 1.05 then A else B)#", Result = "A")]
        [TestCase("#(if 1,100 > 1,05 then A else B)#", Result = "A")]
        [TestCase("#(if 1.100 > 1,05 then A else B)#", Result = "A")]
        [TestCase("#(if 1.100 < 1,05 then A else B)#", Result = "B")]
        [TestCase("#(if 24 < 24 then A else B)#", Result = "B")]
        [TestCase("#(if 24 >= 24 then A else B)#", Result = "A")]
        [TestCase("#(if 24 <= 24 then A else B)#", Result = "A")]
        [TestCase("#(if 26 >= 24 then A else B)#", Result = "A")]
        [TestCase("#(if 26 <= 24 then A else B)#", Result = "B")]
        [TestCase("#(if aaa#(switch;45;35;A;45;b)#a == aaaba then GOOD)#", Result = "GOOD")]
        [TestCase("#(if #(if x == x then a)##(switch;45;35;A;45;b)##(if z == z then a)# == aba then GOOD)#", Result = "GOOD")]
        [TestCase("#(if #(if x == x then a)##(if y == y then b)##(if z == z then a)# == aba then GOOD)#", Result = "GOOD")]
        [TestCase("#(if #(if #(if y == #(switch;45;35;A;45;y)# then x)# == x then a)#ba == #(switch;45;35;A;45;aba)# then GOOD)#", Result = "GOOD")]
        public string ValidateLogic(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            return result.String;
        }

        [TestCase(-5, -6, true)]
        [TestCase(5, 5, true)]
        [TestCase(-5, 5, false)]
        [TestCase(5, 6, false)]
        public void ValidateRandom(int nMin, int nMax, bool bExpectError)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(string.Format("#random;{0};{1}#", nMin, nMax), _testLine, _testElement, false);
            if (bExpectError)
            {
                Assert.AreEqual(result.String, "Invalid random specified. Min >= Max");
            }
            else
            {
                var nResult = int.Parse(result.String);
                Assert.True(nResult >= nMin && nResult <= nMax);
            }
        }

        [TestCase("#(switch;goodkey;badkey;1;goodkey;#nodraw)#", false, Result = "#nodraw")]
        [TestCase("Test: #(switch;goodkey;badkey;1;goodkey;#nodraw)#", false, Result = "Test: #nodraw")]
        [TestCase("Test: #(switch;goodkey;badkey;1;goodkey;#nodraw)#", false, Result = "Test: #nodraw")]
        [TestCase("#(switch;goodkey;badkey;1;goodkey;#nodraw)# [test]", false, Result = "#nodraw [test]")]
        [TestCase("Test: #(switch;goodkey;badkey;1;otherbadkey;2;#default;#nodraw)#", false, Result = "Test: #nodraw")]
        [TestCase("#(if a == a then #nodraw)#", false, Result = "#nodraw")]
        [TestCase("#(if [1;2;3] == [a;3;b] then #nodraw)#", false, Result = "#nodraw")]
        [TestCase("#(if [1;2;3] == [a;b;c] then #nodraw)#", true, Result = "")]
        [TestCase("Test: #(if a == a then #nodraw)#", false, Result = "Test: #nodraw")]
        [TestCase("#nodraw", false, Result = "#nodraw")]
        [TestCase("#(if a<4 == a>4 then 6 &lt; 7 else #nodraw)#", false, Result = "#nodraw")]
        [TestCase("#(if a<4 == a>4 then #nodraw else 6 &lt; 7)#", true, Result = "6 < 7")]
        public string ValidateNoDraw(string input, bool expectedDrawElement)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            Assert.AreEqual(expectedDrawElement, result.DrawElement);
            return result.String;
        }

        // x + (current card index * y) with left padded 0's numbering z
        [TestCase("##0;0;0#", 0, Result = "0")]
        [TestCase("##0;0;0#", 10, Result = "0")]
        [TestCase("##500;10;8#", 1, Result = "00000510")]
        public string ValidateCounter(string input, int cardIndex)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            _testDeck.SetCardIndex(cardIndex);
            return _testDeck.TranslateString(input, _testLine, _testElement, false).String;
        }

        [TestCase("a", "3", Result = "3")]
        [TestCase("3", "a", Result = "a")]
        [TestCase("", "a", Result = "@[]")]
        [TestCase("-", "a", Result = "a")]
        //[TestCase(" ", "a", Result = "a")] // TODO: this is not a valid key and should throw an error in processing
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
            return _testDeck.TranslateString("@["+ keyName + "]", _testDeck.ValidLines[0], _testElement, false).String;
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

        [TestCase("\\c", Result=",")]
        [TestCase("\\q", Result = "\"")]
        [TestCase("&gt;", Result = ">")]
        [TestCase("&lt;", Result = "<")]
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
            var result = _testDeck.TranslateString("\\n", _testLine, _testElement, false);
            Assert.AreEqual(Environment.NewLine, result.String);
        }

        [TestCase("\\c", Result = "\\c")]
        [TestCase("\\q", Result = "\\q")]
        public string ValidateSpecialCharacterNonTranslation(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            _testElement.type = ElementType.FormattedText.ToString();
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            return result.String;
        }

#if false // this is no longer true
        [Test]
        public void ValidateNewlineSpecialCharacterNonTranslation()
        {
            const string slashN = "\\n";
            _testElement.type = ElementType.FormattedText.ToString();
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(slashN, _testLine, _testElement, false);
            Assert.AreEqual(slashN, result.String);
        }
#endif

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

        [TestCase("@[1]@[2]", Result = "ab")]
        [TestCase("@[4]@[2]@[1]", Result = "bbbba")]
        [TestCase("@[5]", Result = "bbb")]
        [TestCase("@[5] at the @[1] end test @[5]", Result = "bbb at the a end test bbb")]
        public string ValidateNestedDefines(string line)
        {
            var listLines = new List<List<string>>();
            var listDefines = new List<List<string>>()
            {
                new List<string>() { "define", "value" },
                new List<string>() { "1", "a" },
                new List<string>() { "2", "b" },
                new List<string>() { "3", "@[2]" },
                new List<string>() { "4", "@[3]@[2]@[3]" },
                new List<string>() { "4ex", "4" },
                new List<string>() { "5", "@[@[4ex]]" }
            };
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);
            var result = _testDeck.TranslateString(line, _testLine, _testElement, false);
            return result.String;
        }

        [TestCase("@[loopA]")]
        [TestCase("@[loopB]")]
        [TestCase("@[loopC]")]
        [TestCase("@[loopD]")]
        public void ValidateLoopingDefines(string line)
        {
            var listLines = new List<List<string>>();
            var listDefines = new List<List<string>>()
            {
                new List<string>() { "define", "value" },
                new List<string>() { "loopA", "@[loopB]" },
                new List<string>() { "loopB", "@[loopA]" },
                new List<string>() { "loopC", "@[loopC]" },
                new List<string>() { "loopD", "#(if a == a then @[loopD])#" },
            };
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);
            _testDeck.TranslateString(line, _testLine, _testElement, false);
        }

        [TestCase("@[action,aa,bb]", Result = "aa::bb")]
        [TestCase("@[action,Aa,bB]", Result = "Aa::bB")]
        [TestCase("@[action,@[L4],bb]", Result = "bbb::bb")]
        [TestCase("@[actionCaller,@[L1]]", Result = "a::zork")]
        [TestCase("@[smallImgTag,@[theCoin]]", Result = @"<img=\images\coin.png;.90;0;3>")]
        [TestCase("@[smallImgTag, @[theCoin]]", Result = @"<img= \images\coin.png;.90;0;3>")]
        [TestCase("@[smallImgTag,\t@[theCoin]]", Result = "<img=\t\\images\\coin.png;.90;0;3>")]
        [TestCase("@[smallImgTag,]", Result = @"<img=;.90;0;3>")]
        [TestCase("@[action,,]", Result = @"::")]
        [TestCase("@[smallImgTag]", Result = @"<img={1};.90;0;3>")]
        [TestCase("@[L1,@[L2],@[L4]]", Result = @"a")]
        public String ValidateParameterDefines(string line)
        {
            var listLines = new List<List<string>>();
            var listDefines = new List<List<string>>()
            {
                new List<string>() { "define", "value" },
                new List<string>() { "L1", "a" },
                new List<string>() { "L2", "b" },
                new List<string>() { "L3", "@[L2]" },
                new List<string>() { "L4", "@[L3]@[L2]@[L3]" },
                new List<string>() { "action", "{1}::{2}" },
                new List<string>() { "actionCaller", "@[action,{1},zork]" },
                new List<string>() { "smallImgTag", "<img={1};.90;0;3>" },
                new List<string>() { "theCoin", @"\images\coin.png" }
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
            const string PREFIX = "PREFIX";
            const string SUFFIX = "SUFFIX";
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
                PREFIX +
                " " +
                getOverride("x", X) +
                getOverride("y", Y) +
                getOverride("width", WIDTH) +
                getOverride("height", HEIGHT) +
                getOverride("borderthickness", BORDERTHICKNESS) +
                getOverride("opacity", OPACITY) +
                getOverride("outlinethickness", OUTLINETHICKNESS) +
                getOverride("lineheight", LINEHEIGHT) +
                getOverride("wordspace", WORDSPACE) +
                getOverride("autoscalefont", AUTOSCALEFONT) +
                getOverride("enabled", ENABLED) +
                getOverride("lockaspect", LOCKASPECT) +
                getOverride("keeporiginalsize", KEEPORIGINALSIZE) +
                getOverride("justifiedtext", JUSTIFIEDTEXT) +
                getOverride("rotation", ROTATION) +
                getOverride("bordercolor", BORDERCOLOR) +
                getOverride("elementcolor", ELEMENTCOLOR) +
                getOverride("outlinecolor", OUTLINECOLOR) +
                getOverride("font", FONT) +
                getOverride("verticalalign", VERTICALALIGN) +
                getOverride("horizontalalign", HORIZONTALALIGN) +
                getOverride("type", TYPE) +
                // un-overrid-able
                getOverride("name", NAME) +
                getOverride("variable", VARIABLE) +
                " " + 
                SUFFIX
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

            Assert.AreEqual(PREFIX + "  " + SUFFIX, zElementString.String);
        }

        [Test]
        public void ValidateDefinitionBasedEmptyOverride()
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>(),
                new List<List<string>>(),
                null);

            const string PREFIX = "PREFIX";
            const string SUFFIX = "SUFFIX";

            const string BASE_VARIABLE = "theVariable";

            _testElement.variable = BASE_VARIABLE;

            var zElementString = _testDeck.TranslateString(
                PREFIX +
                " " +
                getOverride("x", string.Empty) +
                " " +
                SUFFIX
                , _testLine, _testElement, false);
            Assert.True(0 == zElementString.OverrideFieldToValueDictionary.Count);
            _testDeck.GetVariableOverrideElement(_testElement, zElementString.OverrideFieldToValueDictionary);

            Assert.AreEqual(PREFIX + "  " + SUFFIX, zElementString.String);
        }

        private string getOverride(string sOverrideField, object zOverrideValue)
        {
            return "$[{0}:{1}]".FormatString(sOverrideField, zOverrideValue);
        }

        [TestCase("##?", 15, 8, Result = "00000015bb")]
        [TestCase("@[a] ##", 123456, 4, Result = "1 123456")]
        [TestCase("@[aa] ##", 123456, 4, Result = "2 123456")]
        [TestCase("@[bb] ##?", 123456, 8, Result = "[UNKNOWN] 00123456bb")]
        [TestCase("@[a] ## #SC", 0, 8, Result = "1 00000000 00000001")]
        [TestCase("@[a] ## #SC", 1, 8, Result = "1 00000001 00000001")]
        [TestCase("@[a] ## #SC", 2, 8, Result = "1 00000002 00000002")]
        [TestCase("@[a] ## #SC", 3, 8, Result = "1 00000003 00000003")]
        [TestCase("@[a] ## #SC", 4, 8, Result = "1 00000004 00000004")]
        [TestCase("@[a] ## #SC", 5, 8, Result = "1 00000005 00000001")]
        public string TestFileNameExport(string input, int cardNumber, int leftPad)
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>()
                {
                    new List<string>(){"count", "a", "b"},
                    new List<string>() {"1", "1", "3"},
                    new List<string>(){"4", "1", "2"},
                    new List<string>(){"1", "1", "2"}
                },
                new List<List<string>>()
                {
                    new List<string>(){"define", "value"},
                    new List<string>(){"aa", "2"}
                },
                null);
            _testDeck.SetDisallowedCharReplacement('?', "bb");
            _testDeck.CardPrintIndex = cardNumber;
            return _testDeck.TranslateFileNameString(input, cardNumber, leftPad);
        }

        // graphic elements
    }
}
