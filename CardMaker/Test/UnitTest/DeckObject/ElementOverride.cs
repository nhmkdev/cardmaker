﻿using CardMaker.XML;
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
            return _testDeck.GetOverrideElement(_testElement, _testDeck.ValidLines[0].LineColumns, _testDeck.ValidLines[0], false);            
        }

        [TestCase("bordercolor", "0xFF33CCFF")]
        [TestCase("elementcolor", "0x44556677")]
        [TestCase("outlinecolor", "0xAABBCCDD")]
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
