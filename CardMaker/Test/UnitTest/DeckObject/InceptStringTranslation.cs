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

using CardMaker.XML;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using CardMaker.Data;
using Moq;
using Support.Progress;
using Support.UI;
using CardMaker.Card.Translation;

namespace UnitTest.DeckObject
{
    // TODO: "bad name" test!
    [TestFixture]
    internal class InceptStringTranslation
    {
        private const string TEST_ELEMENT_NAME = "testElement";

        private TestDeck _testDeck;
        private ProjectLayoutElement _testElement;
        private Mock<ProgressReporterProxy> _mockProgressReporterProxy;

        [SetUp]
        public void Setup()
        {
            _mockProgressReporterProxy = new Mock<ProgressReporterProxy>();
            _testDeck = new TestDeck();
            _testDeck.SetProgressReporterProxy(_mockProgressReporterProxy.Object);
            _testElement = new ProjectLayoutElement(TEST_ELEMENT_NAME)
            {
                x = 15,
                y = 45,
                verticalalign = (int)StringAlignment.Near,
                horizontalalign = (int)StringAlignment.Center,
                width = 100,
                height = 200,
                opacity = 128,
                enabled = true,
                justifiedtext = false,
            };
        }

        [Test]
        public void ValidateCache()
        {
            const string expected = "sample string with nothing special.";
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(expected, _testDeck.CurrentPrintLine, _testElement, false);
            var secondResult = _testDeck.TranslateString(expected, _testDeck.CurrentPrintLine, _testElement, false);
            // TODO: moq or something to validate the cache was actually hit
            Assert.AreEqual(result, secondResult);
        }

        [TestCase("sample string with nothing special.", ExpectedResult = "sample string with nothing special.")]
        public string ValidateNonTranslate(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            Assert.IsTrue(result.DrawElement);
            return result.String;
        }


        [TestCase("@[TESTCOLUMN1]", ExpectedResult = "102")]
        [TestCase("@[testcolumn1]", ExpectedResult = "102")]
        [TestCase("@[testColumn1]", ExpectedResult = "102")]
        [TestCase("@[testColumn2]", ExpectedResult = "aaa")]
        [TestCase(" @[testColumn1]", ExpectedResult = " 102")]
        [TestCase(" @[testColumn2]", ExpectedResult = " aaa")]
        [TestCase("@[testColumn1] ", ExpectedResult = "102 ")]
        [TestCase("@[testColumn2] ", ExpectedResult = "aaa ")]
        [TestCase("@[testColumn1]@[testColumn2]", ExpectedResult = "102aaa")]
        [TestCase("@[@[testColumn3]]", ExpectedResult = "102")]
        public string ValidateReferenceValue(string sInput)
        {
            var listLines = new List<List<string>>()
            {
                new List<string>() {"count", "testColumn1", "testColumn2", "testColumn3"},
                new List<string>() {"1", "102", "aaa", "testColumn1"}
            };
            var listDefines = new List<List<string>>();
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);
            _testDeck.CardPrintIndex = 0;

            return _testDeck.TranslateString(sInput, _testDeck.CurrentPrintLine, _testElement, false).String;
        }

        [TestCase("![cardcount]", ExpectedResult = "10")]
        [TestCase("![elementname]", ExpectedResult = TEST_ELEMENT_NAME)]
        [TestCase("![deckIndex]", ExpectedResult = "7")]
        [TestCase("![cardIndex]", ExpectedResult = "2")]
        [TestCase("![layoutname]", ExpectedResult = TestDeck.LAYOUT_NAME)]
        public string ValidateCardVariable(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>
            {
                new List<string>{"count", "var"},
                new List<string>{"3", "aaa"},
                new List<string>{"2", "bbb"},
                new List<string>{"5", "ccc"},
            }, new List<List<string>>(), "test");
            _testDeck.CardPrintIndex = 6;
#warning: should do a test with the non print card index
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, true);
            return result.String;
        }

        [TestCase("&[x]", ExpectedResult = "15")]
        [TestCase("&[y]", ExpectedResult = "45")]
        [TestCase("&[verticalalign]", ExpectedResult = "0")]
        [TestCase("&[horizontalalign]", ExpectedResult = "1")]
        [TestCase("&[width]", ExpectedResult = "100")]
        [TestCase("&[height]", ExpectedResult = "200")]
        [TestCase("aa&[opacity]", ExpectedResult = "aa128")]
        [TestCase("&[enabled]bb", ExpectedResult = "Truebb")]
        [TestCase("aa&[justifiedtext]bb", ExpectedResult = "aaFalsebb")]
        [TestCase("aa&[unknown]bb", ExpectedResult = "aaINVALID_FIELD_READbb")]
        [TestCase("aa&[variable]bb", ExpectedResult = "aaFIELD_READ_DISALLOWEDbb")]
        public string ValidateElementFields(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        [TestCase("aa#padl;5;0;123#bb", ExpectedResult = "aa00123bb")]
        [TestCase("aa#padr;5;0;123#bb", ExpectedResult = "aa12300bb")]
        [TestCase("aa#padl;3;0;123#bb", ExpectedResult = "aa123bb")]
        [TestCase("aa#padr;3;0;123#bb", ExpectedResult = "aa123bb")]
        public string ValidatePadding(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        // todo if/switch tests with bggraphic and shape

        [TestCase("#(if a == a then word else word2)#", ExpectedResult = "word")]
        [TestCase("#(if a == b then word else word2)#", ExpectedResult = "word2")]
        [TestCase("#(if a == b then word else word2)# midword #(if c == d then chowder)#", ExpectedResult = "word2 midword ")]
        [TestCase("#(if 2 > 0 then Deploy Roll: +2)# Deploy Success: +1pt#(switch;+1;+1;;-1;;#default;s)##(if DBA == then #nodraw)#", ExpectedResult = "Deploy Roll: +2 Deploy Success: +1pt")]
        [TestCase("#(if 1.000 > 1 then A else B)#", ExpectedResult = "B")]
        [TestCase("#(if 1,000 > 1 then A else B)#", ExpectedResult = "B")]
        [TestCase("#(if 1.100 > 1 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 1,100 > 1 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 1.100 > 1.05 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 1,100 > 1,05 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 1.100 > 1,05 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 1.100 < 1,05 then A else B)#", ExpectedResult = "B")]
        [TestCase("#(if 24 < 24 then A else B)#", ExpectedResult = "B")]
        [TestCase("#(if 24 >= 24 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 24 <= 24 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 26 >= 24 then A else B)#", ExpectedResult = "A")]
        [TestCase("#(if 26 <= 24 then A else B)#", ExpectedResult = "B")]
        [TestCase("#(if aaa#(switch;45;35;A;45;b)#a == aaaba then GOOD)#", ExpectedResult = "GOOD")]
        [TestCase("#(if #(if x == x then a)##(switch;45;35;A;45;b)##(if z == z then a)# == aba then GOOD)#", ExpectedResult = "GOOD")]
        [TestCase("#(if #(if x == x then a)##(if y == y then b)##(if z == z then a)# == aba then GOOD)#", ExpectedResult = "GOOD")]
        [TestCase("#(if #(if #(if y == #(switch;45;35;A;45;y)# then x)# == x then a)#ba == #(switch;45;35;A;45;aba)# then GOOD)#", ExpectedResult = "GOOD")]
        public string ValidateLogic(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        [TestCase("#(if [1;2;3] == [1] then good else bad)#", ExpectedResult = "good")]
        [TestCase("#(if [1] == [1;2;3] then good else bad)#", ExpectedResult = "good")]
        [TestCase("#(if [1] == [2;3] then good)#", ExpectedResult = "")]
        [TestCase("#(if 1 == [1;2;3] then good)#", ExpectedResult = "good")]
        [TestCase("#(if 1 == [2;3] then good)#", ExpectedResult = "")]
        [TestCase("#(if [1;2;3] == 1 then good)#", ExpectedResult = "good")]
        [TestCase("#(if [2;3] == 1 then good)#", ExpectedResult = "")]
        [TestCase("#(if [1;2;3] == [a;3;b] then good)#", ExpectedResult = "good")]
        [TestCase("#(if [1;2;3] == [a;b;c] then good else bad)#", ExpectedResult = "bad")]
        [TestCase("#(if [1;2;3] == [3;4;5] then good)#", ExpectedResult = "good")]
        public string ValidateGroupedIfStatements(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        // note this result sucks -- should just be nothing (empty string)
        [TestCase("#(switch;[45];[15];nothing)#", ExpectedResult = "switch;[45];[15];nothing")]
        // NOTE: the delimiter change to support grouped cases (2 char)
        [TestCase("#(switch::[45;15]::15::nothing::#default::stuff)#", ExpectedResult = "nothing")]
        [TestCase("#(switch::[45;15]::[15]::nothing::#default::stuff)#", ExpectedResult = "nothing")]
        [TestCase("#(switch::15::[15]::nothing::#default::stuff)#", ExpectedResult = "nothing")]
        [TestCase("#(switch::[35;ab]::[15;45]::nothing::#default::stuff)#", ExpectedResult = "stuff")]
        [TestCase("#(switch::weapon::[weapon;potion;equipment]::item::#default::#switchkey)#", ExpectedResult = "item")]
        [TestCase("#(switch::weapon::other::??::[weapon;potion;equipment]::item::#default::#switchkey)#", ExpectedResult = "item")]
        [TestCase("#(switch::unknown::[weapon;potion;equipment]::item::#default::#switchkey)#", ExpectedResult = "unknown")]
        public string ValidateGroupedSwitchStatements(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        // note this result sucks -- should just be nothing (empty string)
        [TestCase("#(switch;45;15;nothing)#", ExpectedResult = "switch;45;15;nothing")]
        [TestCase("#(switch;45;15;nothing;#default;stuff)#", ExpectedResult = "stuff")]
        [TestCase("#(switch;45;15;nothing;45;;#default;stuff)#", ExpectedResult = "")]
        [TestCase("#(switch;;15;nothing;;;#default;stuff)#", ExpectedResult = "")]
        [TestCase("#(switch;;15;nothing;;result;#default;stuff)#", ExpectedResult = "result")]
        [TestCase("#(switch;85;15;nothing;;result;#default;#switchkey)#", ExpectedResult = "85")]
        [TestCase("#(switch;85;15;nothing;;result;#default;)#", ExpectedResult = "")]
        // this is proof of the switch using the ; delimiter is broken
        [TestCase("#(switch;;15;nothing;#empty;result;#default;<img=test.png;.8)#", ExpectedResult = "switch;;15;nothing;;result;#default;<img=test.png;.8")]
        public string ValidateSwitchLogic(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");

            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        // note this result sucks -- should just be nothing (empty string)
        [TestCase("#(switch//45//15//nothing)#", ExpectedResult = "switch//45//15//nothing")]
        [TestCase("#(switch-=45-=15-=nothing-=#default-=weird)#", ExpectedResult = "weird")]
        [TestCase("#(switch//45//15//nothing//#default//stuff)#", ExpectedResult = "stuff")]
        [TestCase("#(switch////15//nothing//45////#default//stuff)#", ExpectedResult = "stuff")]
        [TestCase("#(switch//45//15//nothing//45////#default//stuff)#", ExpectedResult = "")]
        [TestCase("#(switch////15//nothing///////#default//stuff)#", ExpectedResult = "")]
        [TestCase("#(switch////15//nothing////result//#default//stuff)#", ExpectedResult = "result")]
        [TestCase("#(switch////15//nothing//#empty//result//#default//stuff)#", ExpectedResult = "result")]
        [TestCase("#(switch////15//nothing//#empty///img.png//#default//stuff)#", ExpectedResult = "/img.png")]
        [TestCase("#(switch//45//15//nothing//#empty//result//#default//<img=test.png;.8>)#", ExpectedResult = "<img=test.png;.8>")]
        [TestCase("#(switch//45//15//nothing//#empty//result//#default//<img=test.png;.8>)#", ExpectedResult = "<img=test.png;.8>")]
        [TestCase("#(switch--45--15--nothing--#empty--result--#default--<img=test.png;.8>)#", ExpectedResult = "<img=test.png;.8>")]
        [TestCase("#(switch----45--15--nothing--#empty--result--#default--<img=test.png;.8>)#", ExpectedResult = "switch----45--15--nothing----result--#default--<img=test.png;.8>")]
        public string ValidateAltSwitchLogic(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");

            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        [TestCase(-5, -6, true)]
        [TestCase(5, 5, true)]
        [TestCase(-5, 5, false)]
        [TestCase(5, 6, false)]
        public void ValidateRandom(int nMin, int nMax, bool bExpectError)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(string.Format("#random;{0};{1}#", nMin, nMax), _testDeck.CurrentPrintLine, _testElement, false);
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

        [TestCase("aaa#random;-5;6#bbb", "aaa", "bbb")]
        [TestCase("#random;-5;6#bbb", "", "bbb")]
        [TestCase("aaa#random;-5;6#", "aaa", "")]
        [TestCase("#random;-5;6##bggraphic::images/Faction_empire.bmp#", "", "#bggraphic::images/Faction_empire.bmp#")]
        public void ValidateRandomTranslation(string sInput, string sExpectedStart, string sExpectedEnd)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var sResult = _testDeck.TranslateString(sInput, _testDeck.CurrentPrintLine, _testElement, false).String;
            Console.WriteLine(sResult);
            Assert.True(sResult.StartsWith(sExpectedStart));
            Assert.True(sResult.EndsWith(sExpectedEnd));
        }

        // TODO: most of these are just tests of logic... not #nodraw testing
        [TestCase("#(switch;goodkey;badkey;1;goodkey;#nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("Test: #(switch;goodkey;badkey;1;goodkey;#nodraw)#", false, ExpectedResult = "Test: #nodraw")]
        [TestCase("Test: #(switch;goodkey;badkey;1;goodkey;#nodraw)#", false, ExpectedResult = "Test: #nodraw")]
        [TestCase("#(switch;goodkey;badkey;1;goodkey;#nodraw)# [test]", false, ExpectedResult = "#nodraw [test]")]
        [TestCase("Test: #(switch;goodkey;badkey;1;otherbadkey;2;#default;#nodraw)#", false, ExpectedResult = "Test: #nodraw")]
        [TestCase("#(if a == a then #nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("#(if [1;2;3] == [a;3;b] then #nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("#(if [1;2;3] == [a;b;c] then #nodraw)#", true, ExpectedResult = "")]
        [TestCase("Test: #(if a == a then #nodraw)#", false, ExpectedResult = "Test: #nodraw")]
        [TestCase("#(if == then #nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("#(if #empty == then #nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("#(if == #empty then #nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("#nodraw", false, ExpectedResult = "#nodraw")]
        [TestCase("#(if a<4 == a>4 then 6 &lt; 7 else #nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("#(if [;] == [] then #nodraw)#", false, ExpectedResult = "#nodraw")]
        [TestCase("#(if a<4 == a>4 then #nodraw else 6 &lt; 7)#", true, ExpectedResult = "6 < 7")]
        public string ValidateNoDraw(string input, bool expectedDrawElement)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            Assert.AreEqual(expectedDrawElement, result.DrawElement);
            return result.String;
        }

        [TestCase("aa#cnva;0xff00ff#bb", ExpectedResult = "aa1bb")]
        [TestCase("aa#cnva;0xff00ff#", ExpectedResult = "aa1")]
        [TestCase("#cnva;0xff00ff#bb", ExpectedResult = "1bb")]
        [TestCase("#cnv;0x804020ff#", ExpectedResult = "0.5019608;0.2509804;0.1254902;1")]
        [TestCase("#cnvrgba;0x804020ff#", ExpectedResult = "0.5019608;0.2509804;0.1254902;1")]
        [TestCase("#cnvargb;0x804020ff#", ExpectedResult = "1;0.5019608;0.2509804;0.1254902")]
        [TestCase("#cnva;0x804020ff#", ExpectedResult = "1")]
        [TestCase("#cnvr;0x804020ff#", ExpectedResult = "0.5019608")]
        [TestCase("#cnvg;0x804020ff#", ExpectedResult = "0.2509804")]
        [TestCase("#cnvb;0x804020ff#", ExpectedResult = "0.1254902")]
        [TestCase("#cnvb;20ff#", ExpectedResult = InceptTranslator.CNV_BAD_COLOR_VALUE)]
        public string ValidateColorNormalizedValue(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }


        [TestCase("#math;0+0#", ExpectedResult = "0")]
        [TestCase("#math;1+3#", ExpectedResult = "4")]
        [TestCase("aa#math;1+3#bb", ExpectedResult = "aa4bb")]
        [TestCase("aa#math;1+3#3-4", ExpectedResult = "aa43-4")]
        [TestCase("#math;0-0#", ExpectedResult = "0")]
        [TestCase("#math;1-10#", ExpectedResult = "-9")]
        [TestCase("#math;10-9#", ExpectedResult = "1")]
        [TestCase("#math;0*0#", ExpectedResult = "0")]
        [TestCase("#math;7*8#", ExpectedResult = "56")]
        [TestCase("#math;-7*8#", ExpectedResult = "-56")]
        [TestCase("#math;0/0#", ExpectedResult = "")]
        [TestCase("#math;49/6#", ExpectedResult = "8.166667")]
        [TestCase("#math;1/4#", ExpectedResult = "0.25")]
        [TestCase("#math;1/4;0#", ExpectedResult = "0")]
        [TestCase("#math;12/3#", ExpectedResult = "4")]
        [TestCase("#math;12%3#", ExpectedResult = "0")]
        [TestCase("#math;12%5#", ExpectedResult = "2")]
        [TestCase("#math;1.5+1.75#", ExpectedResult = "3.25")]
        [TestCase("#math;10*1.5#", ExpectedResult = "15")]
        [TestCase("#math;10*@[scale_factor]#", ExpectedResult = "15")]
        [TestCase("#math;1.5-1.75#", ExpectedResult = "-0.25")]
        [TestCase("#math;1.5*3#", ExpectedResult = "4.5")]
        [TestCase("aa#math;1.5*-3;#bb", ExpectedResult = "aa-4.5bb")]
        [TestCase("aa#math;1.5*-3;#4-5", ExpectedResult = "aa-4.54-5")]
        [TestCase("#math;0/0#", ExpectedResult = "")]
        [TestCase("#math;1/4#", ExpectedResult = "0.25")]
        [TestCase("#math;1/3#", ExpectedResult = "0.3333333")]
        [TestCase("#math;1/4;0.0#", ExpectedResult = "0.3")] // rounds up!
        [TestCase("#math;1/3;0.000#", ExpectedResult = "0.333")]
        [TestCase("#math;#math;1/3;0.00#*100#", ExpectedResult = "33")]
        [TestCase("#math;12/3#", ExpectedResult = "4")]
        [TestCase("#math;12%5#", ExpectedResult = "2")]
        [TestCase("#math;12.6%5#", ExpectedResult = "2.6")]
        public string ValidateMath(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>()
            {
                new List<string>{"scale_factor", "1.5"}
            }, "test");
            return _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false).String;
        }

        // x + (current card index * y) with left padded 0's numbering z
        [TestCase("##0;0;0#", 0, ExpectedResult = "0")]
        [TestCase("##0;0;0#", 10, ExpectedResult = "0")]
        [TestCase("##500;10;8#", 1, ExpectedResult = "00000510")]
        public string ValidateCounter(string input, int cardIndex)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            _testDeck.SetCardIndex(cardIndex);
            return _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false).String;
        }

        [TestCase("a", "3", ExpectedResult = "3")]
        [TestCase("3", "a", ExpectedResult = "a")]
        [TestCase("", "a", ExpectedResult = "@[]")]
        [TestCase("-", "a", ExpectedResult = "a")]
        //[TestCase(" ", "a", ExpectedResult = "a")] // TODO: this is not a valid key and should throw an error in processing
        public string ValidateColumnReference(string keyName, string keyValue)
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>()
                {
                    new List<string>(){"count", keyName},
                    new List<string>() {"1", keyValue}
                },
                new List<List<string>>(),
                null);
            return _testDeck.TranslateString("@["+ keyName + "]", _testDeck.ValidLines[0], _testElement, false).String;
        }

        [TestCase("x", "x" , ExpectedResult = 1)]
        [TestCase("X", "x", ExpectedResult = 1)]
        [TestCase("X", "x;y", ExpectedResult = 1)]
        [TestCase("x", "x;y", ExpectedResult = 1)]
        [TestCase("Y", "x;y", ExpectedResult = 1)]
        [TestCase("y", "x;y", ExpectedResult = 1)]
        [TestCase("z", "*", ExpectedResult = 1)]
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

        [TestCase("y", "x")]
        [TestCase("y", "w;x;z")]
        [TestCase("y", "w;x;*")]
        public void ValidateNonAllowedLayout(string layoutName, string allowedLayout)
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

        [TestCase("\\c", ExpectedResult = ",")]
        [TestCase("\\q", ExpectedResult = "\"")]
        [TestCase("&gt;", ExpectedResult = ">")]
        [TestCase("&lt;", ExpectedResult = "<")]
        public string ValidateSpecialCharacterTranslation(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        [Test]
        public void ValidateNewlineSpecialCharacterTranslation()
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            var result = _testDeck.TranslateString("\\n", _testDeck.CurrentPrintLine, _testElement, false);
            Assert.AreEqual(Environment.NewLine, result.String);
        }

        [TestCase("\\c", ExpectedResult = "\\c")]
        [TestCase("\\q", ExpectedResult = "\\q")]
        public string ValidateSpecialCharacterNonTranslation(string input)
        {
            _testDeck.ProcessLinesPublic(new List<List<string>>(), new List<List<string>>(), "test");
            _testElement.type = ElementType.FormattedText.ToString();
            var result = _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false);
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

        [TestCase("@[1]@[2]", ExpectedResult = "ab")]
        [TestCase("@[4]@[2]@[1]", ExpectedResult = "bbbba")]
        [TestCase("@[5]", ExpectedResult = "bbb")]
        [TestCase("@[5] at the @[1] end test @[5]", ExpectedResult = "bbb at the a end test bbb")]
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
            var result = _testDeck.TranslateString(line, _testDeck.CurrentPrintLine, _testElement, false);
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
            _testDeck.TranslateString(line, _testDeck.CurrentPrintLine, _testElement, false);
        }

        [TestCase("@[action,aa,bb]", ExpectedResult = "aa::bb")]
        [TestCase("@[action,Aa,bB]", ExpectedResult = "Aa::bB")]
        [TestCase("@[action,@[L4],bb]", ExpectedResult = "bbb::bb")]
        [TestCase("@[actionCaller,@[L1]]", ExpectedResult = "a::zork")]
        [TestCase("@[smallImgTag,@[theCoin]]", ExpectedResult = @"<img=\images\coin.png;.90;0;3>")]
        [TestCase("@[smallImgTag, @[theCoin]]", ExpectedResult = @"<img= \images\coin.png;.90;0;3>")]
        [TestCase("@[smallImgTag,\t@[theCoin]]", ExpectedResult = "<img=\t\\images\\coin.png;.90;0;3>")]
        [TestCase("@[smallImgTag,]", ExpectedResult = @"<img=;.90;0;3>")]
        [TestCase("@[action,,]", ExpectedResult = @"::")]
        [TestCase("@[smallImgTag]", ExpectedResult = @"<img={1};.90;0;3>")]
        [TestCase("@[L1,@[L2],@[L4]]", ExpectedResult = @"a")]
        public string ValidateParameterDefines(string line)
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
            var result = _testDeck.TranslateString(line, _testDeck.CurrentPrintLine, _testElement, false);
            return result.String;
        }

        [TestCase("%[0,0,1]", ExpectedResult = @"0")]
        [TestCase("%[0,0,2]", ExpectedResult = "")]
        [TestCase("A%[0,0,1]", ExpectedResult = @"A0")]
        [TestCase("A%[0,0,2]", ExpectedResult = @"A")]
        [TestCase("%[0,0,1]B", ExpectedResult = @"0B")]
        [TestCase("%[0,0,2]B", ExpectedResult = @"B")]
        [TestCase("%[@[L4],2,1]", ExpectedResult = @"b")]
        [TestCase("%[sample,4,2]", ExpectedResult = "le" )]
        [TestCase("%[sam,le,4,2]", ExpectedResult = "le")]
        [TestCase("%[sam,le,4,2],5,2]", ExpectedResult = "le,5,2]")]
        [TestCase("%[@[L5],6,3]", ExpectedResult = "eme")]
        public string ValidateSubStringFunctionality(string line)
        {
            var listLines = new List<List<string>>();
            var listDefines = new List<List<string>>()
            {
                new List<string>() { "define", "value" },
                new List<string>() { "L1", "a" },
                new List<string>() { "L2", "b" },
                new List<string>() { "L3", "@[L2]" },
                new List<string>() { "L4", "@[L3]@[L2]@[L3]" },
                new List<string>() { "L5", "![elementname]" },
            };
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);
            _testElement.type = ElementType.FormattedText.ToString();
            var result = _testDeck.TranslateString(line, _testDeck.CurrentPrintLine, _testElement, false);
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
                PREFIX +
                " " +
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
                " " + 
                SUFFIX
                , _testDeck.CurrentPrintLine, _testElement, false);
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
                GetOverride("x", string.Empty) +
                " " +
                SUFFIX
                , _testDeck.CurrentPrintLine, _testElement, false);
            Assert.True(0 == zElementString.OverrideFieldToValueDictionary.Count);
            _testDeck.GetVariableOverrideElement(_testElement, zElementString.OverrideFieldToValueDictionary);

            Assert.AreEqual(PREFIX + "  " + SUFFIX, zElementString.String);
        }

        private string GetOverride(string sOverrideField, object zOverrideValue)
        {
            return "$[{0}:{1}]".FormatString(sOverrideField, zOverrideValue);
        }

        [TestCase("##?", 15, 8, ExpectedResult = "00000015bb")]
        [TestCase("@[a] ##", 123456, 4, ExpectedResult = "1 123456")]
        [TestCase("@[aa] ##", 123456, 4, ExpectedResult = "2 123456")]
        [TestCase("@[bb] ##?", 123456, 8, ExpectedResult = "[UNKNOWN] 00123456bb")]
        [TestCase("@[a] ## #SC", 0, 8, ExpectedResult = "1 00000000 00000001")]
        [TestCase("@[a] ## #SC", 1, 8, ExpectedResult = "1 00000001 00000001")]
        [TestCase("@[a] ## #SC", 2, 8, ExpectedResult = "1 00000002 00000002")]
        [TestCase("@[a] ## #SC", 3, 8, ExpectedResult = "1 00000003 00000003")]
        [TestCase("@[a] ## #SC", 4, 8, ExpectedResult = "1 00000004 00000004")]
        [TestCase("@[a] ## #SC", 5, 8, ExpectedResult = "1 00000005 00000001")]
        public string ValidateFileNameExport(string input, int cardNumber, int leftPad)
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

        [TestCase("A#repeat;-1;a#B", ExpectedResult = "A#repeat;-1;a#B")]
        [TestCase("A#repeat;0;a#B", ExpectedResult = "AB")]
        [TestCase("A#repeat;1;##B", ExpectedResult = "A#B")]
        [TestCase("A#repeat;1;##B3#", ExpectedResult = "A#B3#")]
        [TestCase("A#repeat;3;##B3#", ExpectedResult = "A###B3#")]
        [TestCase("A#repeat;2;@[1]#B", ExpectedResult = "AaaB")]
        [TestCase("A#repeat;2;@[4]#B", ExpectedResult = "AbbbbbbB")]
        [TestCase("A#repeat;2;@[4]##repeat;3;xyz#B", ExpectedResult = "AbbbbbbxyzxyzxyzB")]
        [TestCase("#repeat;%[@[loot],1,2];<img=icons\\@[cointypeimage%[@[loot],0,1]];.80>#", ExpectedResult = "<img=icons\\gold.png;.80><img=icons\\gold.png;.80>")]
        [TestCase("#repeat;%[@[loot],1,2];<img=icons\\@[cointypeimage%[@[loot],0,1]];.80>#", ExpectedResult = "<img=icons\\gold.png;.80><img=icons\\gold.png;.80>")]
        public string ValidateRepeatTranslator(string input)
        {
            var listLines = new List<List<string>>()
            {
                new List<string>() {"count", "loot"},
                new List<string>() {"1", "102"}
            };
            var listDefines = new List<List<string>>()
            {
                new List<string>() { "define", "value" },
                new List<string>() { "1", "a" },
                new List<string>() { "2", "b" },
                new List<string>() { "3", "@[2]" },
                new List<string>() { "4", "@[3]@[2]@[3]" },
                new List<string>() { "4ex", "4" },
                new List<string>() { "5", "@[@[4ex]]" },
                new List<string>() { "cointypeimage1", "gold.png"}
            };
            _testDeck.ProcessLinesPublic(
                listLines,
                listDefines,
                null);

            return _testDeck.TranslateString(input, _testDeck.CurrentPrintLine, _testElement, false).String;
        }

        // graphic elements
    }
}
