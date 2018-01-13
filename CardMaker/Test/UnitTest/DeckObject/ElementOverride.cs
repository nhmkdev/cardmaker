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

using CardMaker.XML;
using NUnit.Framework;
using System.Collections.Generic;

namespace UnitTest.DeckObject
{
    [TestFixture]
    internal class ElementOverride
    {
        private TestDeck _testDeck;
        private ProjectLayoutElement _testElement;

        [SetUp]
        public void Setup()
        {
            _testDeck = new TestDeck();
            _testElement = new ProjectLayoutElement("testElement");
        }

        protected ProjectLayoutElement GetOverrideElement(string overrideName, string overrideValue)
        {
            _testDeck.ProcessLinesPublic(
                new List<List<string>>()
                {
                    new List<string>(){"a", "override:testElement:" + overrideName},
                    new List<string>() {"1", overrideValue}
                },
                new List<List<string>>(),
                null);
            Assert.AreEqual(1, _testDeck.ValidLines.Count);
            return _testDeck.GetOverrideElement(_testElement, _testDeck.ValidLines[0], false);            
        }

        [TestCase("bordercolor", "0xFF33CCFF")]
        [TestCase("elementcolor", "0x44556677")]
        [TestCase("outlinecolor", "0xAABBCCDD")]
        [TestCase("backgroundcolor", "0xAABBCCDD")]
        [TestCase("font", "Arial Narrow;11;0;0;0;0")]
        [TestCase("variable", "this is a test!")]
        [TestCase("type", "Text")]
        public void TestOverrideString(string overrideName, string overrideValue)
        {
            var overridenElement = GetOverrideElement(overrideName, overrideValue);
            var result = typeof(ProjectLayoutElement).GetProperty(overrideName).GetValue(overridenElement, null) as string;
            Assert.AreEqual(overrideValue, result);
        }

        [TestCase("x", 25)]
        [TestCase("y", 45)]
        [TestCase("width", 25)]
        [TestCase("height", 10)]
        [TestCase("borderthickness", 800)]
        [TestCase("opacity", 128)]
        [TestCase("outlinethickness", 42)]
        [TestCase("lineheight", 255)]
        [TestCase("wordspace", -15)]
        [TestCase("wordspace", 55)]
        [TestCase("verticalalign", 1)]
        [TestCase("horizontalalign", 2)]
        public void TestOverrideInt(string overrideName, int overrideValue)
        {
            var overridenElement = GetOverrideElement(overrideName, overrideValue.ToString());
            var result = (int)typeof(ProjectLayoutElement).GetProperty(overrideName).GetValue(overridenElement, null);
            Assert.AreEqual(overrideValue, result);
        }

        [TestCase("opacity", "80", 80)]
        [TestCase("opacity", "0x80", 128)]
        [TestCase("opacity", "C4", 196)]
        [TestCase("opacity", "0xC4", 196)]
        [TestCase("opacity", "c4", 196)]
        [TestCase("opacity", "0xc4", 196)]
        public void TestOverrideIntStrings(string overrideName, string overrideValueString, int overrideValue)
        {
            var overridenElement = GetOverrideElement(overrideName, overrideValueString);
            var result = (int)typeof(ProjectLayoutElement).GetProperty(overrideName).GetValue(overridenElement, null);
            Assert.AreEqual(overrideValue, result);
        }

        [TestCase("autoscalefont", true)]
        [TestCase("enabled", false)]
        [TestCase("lockaspect", true)]
        public void TestOverrideBool(string overrideName, bool overrideValue)
        {
            var overridenElement = GetOverrideElement(overrideName, overrideValue.ToString());
            var result = (bool)typeof(ProjectLayoutElement).GetProperty(overrideName).GetValue(overridenElement, null);
            Assert.AreEqual(overrideValue, result);
        }

        [TestCase("rotation", 67.8f)]
        public void TestOverrideFloat(string overrideName, float overrideValue)
        {
            var overridenElement = GetOverrideElement(overrideName, overrideValue.ToString());
            var result = (float)typeof(ProjectLayoutElement).GetProperty(overrideName).GetValue(overridenElement, null);
            Assert.AreEqual(overrideValue, result);
        }
    }
}
