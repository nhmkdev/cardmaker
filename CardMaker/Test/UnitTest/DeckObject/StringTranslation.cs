using CardMaker;
using CardMaker.Card;
using CardMaker.XML;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UnitTest.DeckObject
{
    // TODO: "bad name" test!

    [TestFixture]
    internal class StringTranslation
    {
        private TestDeck _testDeck;
        private DeckLine _testLine;
        private ProjectLayoutElement _testElement;

        [SetUp]
        public void Setup()
        {
            _testDeck = new TestDeck();
            _testLine = new DeckLine(new List<string>());
            _testElement = new ProjectLayoutElement("testElement");
        }

        [Test]
        public void ValidateCache()
        {
            const string expected = "sample string with nothing special.";
            var result = _testDeck.TranslateString(expected, _testLine, _testElement, false);
            var secondResult = _testDeck.TranslateString(expected, _testLine, _testElement, false);
            // TODO: moq or something to validate the cache was actually hit
            Assert.AreEqual(result, secondResult);
        }

        [TestCase("sample string with nothing special.", Result = "sample string with nothing special.")]
        public string ValidateNonTranslate(string input)
        {
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            Assert.IsTrue(result.DrawElement);
            return result.String;
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
        [TestCase("#nodraw", true, Result = "#nodraw")]
        [TestCase("#(if a<4 == a>4 then 6 &lt; 7 else #nodraw)#", false, Result = "#nodraw")]
        [TestCase("#(if a<4 == a>4 then #nodraw else 6 &lt; 7)#", true, Result = "6 < 7")]
        public string ValidateNoDraw(string input, bool expectedDrawElement)
        {
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
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            return result.String;
        }

        [Test]
        public void ValidateNewlineSpecialCharacterTranslation()
        {
            var result = _testDeck.TranslateString("\\n", _testLine, _testElement, false);
            Assert.AreEqual(Environment.NewLine, result.String);
        }

        [TestCase("\\c", Result = "\\c")]
        [TestCase("\\q", Result = "\\q")]
        public string ValidateSpecialCharacterNonTranslation(string input)
        {
            _testElement.type = ElementType.FormattedText.ToString();
            var result = _testDeck.TranslateString(input, _testLine, _testElement, false);
            return result.String;
        }

        [Test]
        public void ValidateNewlineSpecialCharacterNonTranslation()
        {
            const string slashN = "\\n";
            _testElement.type = ElementType.FormattedText.ToString();
            var result = _testDeck.TranslateString(slashN, _testLine, _testElement, false);
            Assert.AreEqual(slashN, result.String);
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

        [TestCase("@[1]@[2]", Result = "ab")]
        [TestCase("@[4]@[2]@[1]", Result = "bbbba")]
        [TestCase("@[5]", Result = "bbb")]
        [TestCase("@[5] at the @[1] end test @[5]", Result = "bbb at the a end test bbb")]
        public String ValidateNestedDefines(string line)
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

        [TestCase("@[action,aa,bb]", Result = "aa::bb")]
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

        [TestCase("##?", 15, 8, Result = "00000015bb")]
        [TestCase("@[a] ##", 123456, 4, Result = "1 123456")]
        [TestCase("@[aa] ##", 123456, 4, Result = "2 123456")]
        [TestCase("@[bb] ##?", 123456, 8, Result = "[UNKNOWN] 00123456bb")]
        public string TestFileNameExport(string input, int cardNumber, int leftPad)
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>()
                {
                    new List<string>(){"a"},
                    new List<string>() {"1"}
                },
                new List<List<string>>()
                {
                    new List<string>(){"define", "value"},
                    new List<string>(){"aa", "2"}
                },
                null);
            _testDeck.SetDisallowedCharReplacement('?', "bb");
            return _testDeck.TranslateFileNameString(input, cardNumber, leftPad);
        }

        // graphic elements
    }
}
