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

using CardMaker.Card.FormattedText.Markup;
using NUnit.Framework;
using System;
using CardMaker.Card.FormattedText;

namespace UnitTest.DeckObject
{
    [TestFixture]
    internal class FormattedText
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("<b></b>", new Type[] { typeof(FontStyleBoldMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<i></i>", new Type[] { typeof(FontStyleItalicMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<s></s>", new Type[] { typeof(FontStyleStrikeoutMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<u></u>", new Type[] { typeof(FontStyleUnderlineMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<br>", new Type[] { typeof(NewlineMarkup) })]
        [TestCase("<spc>", new Type[] { typeof(SpaceMarkup) })]
        [TestCase("<spc=1>", new Type[] { typeof(SpaceMarkup) })]
        [TestCase("<spc=100>", new Type[] { typeof(SpaceMarkup) })]
        [TestCase("<ls=15></ls>", new Type[] {typeof(LineSpaceMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<bgc=0xffeeaa></bgc>", new Type[] { typeof(BackgroundColorMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<bgi=c:\\img.png></bgi>", new Type[] { typeof(BackgroundImageMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<bgi=c:\\img.png;1;2;3;4></bgi>", new Type[] { typeof(BackgroundImageMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<fc=0xaabbcc></fc>", new Type[] { typeof(FontColorMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<img=c:\\newimage\\me.png>", new Type[] { typeof(ImageMarkup) })]
        [TestCase("Hi\\nthere!", new Type[] { typeof(TextMarkup), typeof(NewlineMarkup), typeof(TextMarkup) })]
        [TestCase("<img=me.png>", new Type[] { typeof(ImageMarkup) })]
        [TestCase("<push=15>", new Type[] { typeof(PushMarkup) })]
        [TestCase("<push=15;18>", new Type[] { typeof(PushMarkup) })]
        [TestCase("<f=Arial;15;0;0;0;0></f>", new Type[] { typeof(FontMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<fs=15></fs>", new Type[] { typeof(FontSizeMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<yo=15></yo>", new Type[] { typeof(YDrawOffsetMarkup), typeof(CloseTagMarkup) })]
        [TestCase("<xo=15></xo>", new Type[] { typeof(XDrawOffsetMarkup), typeof(CloseTagMarkup) })]
        // Invalid test case thrown in for fun (the close tag is discarded as it is invalid
        [TestCase("<b></i>", new Type[] { typeof(FontStyleBoldMarkup) })]
        public void ValidateMarkupTranslation(string input, Type[] expectedTypes)
        {
            var markups = FormattedTextParser.GetMarkups(input);
            Assert.AreEqual(expectedTypes.Length, markups.Count);
            for (var i = 0; i < expectedTypes.Length; i++)
            {
                Assert.AreEqual(expectedTypes[i], markups[i].GetType());
            }
        }
        // TODO: more tests specific to parsing each markup
    }
}
