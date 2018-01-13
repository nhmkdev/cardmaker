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

using System.Drawing;
using CardMaker.Card;
using CardMaker.Card.Shapes;
using CardMaker.XML;
using Moq;
using NUnit.Framework;

namespace UnitTest.DeckObject
{
    [TestFixture]
    internal class InlineBackgroundElementProcessorTest
    {
        private Mock<IDrawGraphic> m_mockDrawGraphic = new Mock<IDrawGraphic>();
        private Mock<IShapeRenderer> m_mockShapeRenderer = new Mock<IShapeRenderer>();

        private InlineBackgroundElementProcessor backgroundElementProcessor = new InlineBackgroundElementProcessor();
        private Graphics m_zGraphics = Graphics.FromImage(new Bitmap(1024, 1024));

        [TestCase("aaa#bggraphic::images/Faction_empire.bmp#bbb", "images/Faction_empire.bmp", Result = "aaabbb")]
        [TestCase("#bggraphic::images/Faction_empire.bmp#bbb", "images/Faction_empire.bmp", Result = "bbb")]
        [TestCase("aaa#bggraphic::images/Faction_empire.bmp#", "images/Faction_empire.bmp", Result = "aaa")]
        [TestCase("aaa#bggraphic::c:\\images\\Faction_empire.bmp#bbb", "c:\\images\\Faction_empire.bmp", Result = "aaabbb")]
        public string ValidateBasicInlineBackgroundImage(string sInput, string sExpectedElementDefinition)
        {
            var zTestElement = new ProjectLayoutElement();
            ProjectLayoutElement zBgElement = null;
            m_mockDrawGraphic.Setup(h => h.DrawGraphicFile(It.IsAny<Graphics>(), It.IsAny<string>(),
                    It.IsAny<ProjectLayoutElement>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<Graphics, string, ProjectLayoutElement, int, int>((g, s, p, x, y) => zBgElement = p);

            var result = backgroundElementProcessor.ProcessInlineBackgroundGraphic(m_mockDrawGraphic.Object, m_zGraphics, zTestElement, sInput);

            Assert.NotNull(zBgElement);
            Assert.AreEqual(sExpectedElementDefinition, zBgElement.variable);

            return result;
        }

        [TestCase("aaa#bggraphic::images/Faction_empire.bmp::-5::-15::10::15::true::-::1::1#bbb", "images/Faction_empire.bmp", -5, -15, 10, 15, true, "-", 1, 1, Result = "aaabbb")]
        [TestCase("#bggraphic::images/Faction_empire.bmp::-5::-15::10::15::true::-::1::1#bbb", "images/Faction_empire.bmp", -5, -15, 10, 15, true, "-", 1, 1, Result = "bbb")]
        [TestCase("aaa#bggraphic::images/Faction_empire.bmp::-5::-15::10::15::true::-::1::1#", "images/Faction_empire.bmp", -5, -15, 10, 15, true, "-", 1, 1, Result = "aaa")]
        public string ValidateExtendedInlineBackgroundImage(string sInput, string sVariable, int nX, int nY, int nWidthAdjust, int nHeightAdjust, bool bLockAspect,
            string sTileSize, int nHorizontalAlign, int nVerticalAlign)
        {

            var zTestElement = new ProjectLayoutElement();
            ProjectLayoutElement zBgElement = null;
            int nResultXOffset = 0, nResultYOffset = 0;
            m_mockDrawGraphic.Setup(h => h.DrawGraphicFile(It.IsAny<Graphics>(), It.IsAny<string>(),
                    It.IsAny<ProjectLayoutElement>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<Graphics, string, ProjectLayoutElement, int, int>((g, s, p, x, y) =>
                    {
                        zBgElement = p;
                        nResultXOffset = x;
                        nResultYOffset = y;
                    }
                );

            var result = backgroundElementProcessor.ProcessInlineBackgroundGraphic(m_mockDrawGraphic.Object, m_zGraphics, zTestElement, sInput);

            Assert.NotNull(zBgElement);
            Assert.AreEqual(sVariable, zBgElement.variable);
            Assert.AreEqual(nX, nResultXOffset);
            Assert.AreEqual(nY, nResultYOffset);
            Assert.AreEqual(zTestElement.x, zBgElement.x);
            Assert.AreEqual(zTestElement.y, zBgElement.y);
            Assert.AreEqual(nWidthAdjust + zTestElement.width, zBgElement.width);
            Assert.AreEqual(nHeightAdjust + zTestElement.height, zBgElement.height);
            Assert.AreEqual(bLockAspect, zBgElement.lockaspect);
            Assert.AreEqual(sTileSize, zBgElement.tilesize);
            Assert.AreEqual(nHorizontalAlign, zBgElement.horizontalalign);
            Assert.AreEqual(nVerticalAlign, zBgElement.verticalalign);

            return result;
        }

        [TestCase("aaa#bgshape::#roundedrect;0;-;-;30#::0xff00ff#bbb", "#roundedrect;0;-;-;30#", "0xff00ffff", Result = "aaabbb")]
        [TestCase("#bgshape::#roundedrect;0;-;-;30#::0xff00ff#bbb", "#roundedrect;0;-;-;30#", "0xff00ffff", Result = "bbb")]
        [TestCase("aaa#bgshape::#roundedrect;0;-;-;30#::0xff00ff#", "#roundedrect;0;-;-;30#", "0xff00ffff", Result = "aaa")]
        public string ValidateBasicInlineBackgroundShape(string sInput,
            string sVariable, string sColor)
        {
            var zTestElement = new ProjectLayoutElement();
            ProjectLayoutElement zBgElement = null;
            int nResultXOffset = 0, nResultYOffset = 0;
            m_mockShapeRenderer.Setup(h => h.HandleShapeRender(It.IsAny<Graphics>(), It.IsAny<string>(),
                    It.IsAny<ProjectLayoutElement>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<Graphics, string, ProjectLayoutElement, int, int>((g, s, p, x, y) =>
                    {
                        zBgElement = p;
                    }
                );

            var result = backgroundElementProcessor.ProcessInlineShape(m_mockShapeRenderer.Object, m_zGraphics, zTestElement, sInput);

            Assert.NotNull(zBgElement);
            Assert.AreEqual(sVariable, zBgElement.variable);
            Assert.AreEqual(sColor.ToLower(), zBgElement.elementcolor.ToLower());
            Assert.AreEqual(zTestElement.x, zBgElement.x);
            Assert.AreEqual(zTestElement.y, zBgElement.y);

            return result;
        }

        [TestCase("aaa#bgshape::#roundedrect;0;-;-;30#::0xffffff00::-20::-10::40::50::5::0xff0000#bbb", "#roundedrect;0;-;-;30#", "0xffffff00", -20, -10, 40, 50, 5, "0xff0000ff", Result = "aaabbb")]
        [TestCase("#bgshape::#roundedrect;0;-;-;30#::0xffffff00::-20::-10::40::50::5::0xff0000#bbb", "#roundedrect;0;-;-;30#", "0xffffff00", -20, -10, 40, 50, 5, "0xff0000ff", Result = "bbb")]
        [TestCase("aaa#bgshape::#roundedrect;0;-;-;30#::0xffffff00::-20::-10::40::50::5::0xff0000#", "#roundedrect;0;-;-;30#", "0xffffff00", -20, -10, 40, 50, 5, "0xff0000ff", Result = "aaa")]
        public string ValidateExtendedInlineBackgroundShape(string sInput,
            string sVariable, string sColor, int nX, int nY, int nWidth, int nHeight, int nOutlineThickness, string sOutlineColor)
        {
            var zTestElement = new ProjectLayoutElement();
            ProjectLayoutElement zBgElement = null;
            int nResultXOffset = 0, nResultYOffset = 0;
            m_mockShapeRenderer.Setup(h => h.HandleShapeRender(It.IsAny<Graphics>(), It.IsAny<string>(),
                    It.IsAny<ProjectLayoutElement>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<Graphics, string, ProjectLayoutElement, int, int>((g, s, p, x, y) =>
                    {
                        zBgElement = p;
                        nResultXOffset = x;
                        nResultYOffset = y;
                    }
                );

            var result = backgroundElementProcessor.ProcessInlineShape(m_mockShapeRenderer.Object, m_zGraphics, zTestElement, sInput);

            Assert.NotNull(zBgElement);
            Assert.AreEqual(sVariable, zBgElement.variable);
            Assert.AreEqual(sColor.ToLower(), zBgElement.elementcolor.ToLower());
            Assert.AreEqual(nX, nResultXOffset);
            Assert.AreEqual(nY, nResultYOffset);
            Assert.AreEqual(zTestElement.x, zBgElement.x);
            Assert.AreEqual(zTestElement.y, zBgElement.y);
            Assert.AreEqual(nWidth, zBgElement.width);
            Assert.AreEqual(nHeight, zBgElement.height);
            Assert.AreEqual(nOutlineThickness, zBgElement.outlinethickness);
            Assert.AreEqual(sOutlineColor.ToLower(), zBgElement.outlinecolor.ToLower());

            return result;
        }
    }
}
