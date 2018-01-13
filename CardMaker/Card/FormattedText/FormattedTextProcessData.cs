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

using System;
using System.Drawing;
using CardMaker.Card.FormattedText.Markup;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText
{

    public class FormattedTextProcessData
    {
        public Font Font { get; private set; }
        public float FontHeight { get; private set; }
        public float FontSpaceWidth { get; private set; }
        public float FontSpaceHeight { get; private set; }
        public Brush FontBrush { get; set; }

        public float CurrentX { get; set; }
        public float CurrentY { get; set; }
        public float CurrentYOffset { get; set; }
        public float CurrentXOffset { get; set; }

        public float CurrentLineHeight { get; set; }
        public StringAlignment CurrentStringAlignment { get; set; }

        public int CurrentLine { get; private set; }

        const string FontStringToTest = "]";

        public FormattedTextProcessData()
        {
            CurrentX = 0;
            CurrentY = 0;
        }

        public void SetFont(Font zFont, Graphics zGraphics)
        {
            Font = zFont;
            FontHeight = zFont.GetHeight(zGraphics);

            var rectWithSpace = TextMarkup.MeasureDisplayStringWidth(zGraphics, " " + FontStringToTest, zFont);
            var rectWithoutSpace = TextMarkup.MeasureDisplayStringWidth(zGraphics, FontStringToTest, zFont);

            // NOTE the element word space is ignored! (is that a problem?)
            FontSpaceWidth = rectWithSpace.Width - rectWithoutSpace.Width;
            FontSpaceHeight = Math.Max(rectWithSpace.Height, FontHeight);
        }

        public void AddFontStyle(FontStyle eStyle, Graphics zGraphics)
        {
            SetFont(new Font(Font, Font.Style | eStyle), zGraphics);
        }

        public void RemoveFontStyle(FontStyle eStyle, Graphics zGraphics)
        {
            FontStyle eNewStyle = Font.Style & ~eStyle;
            SetFont(new Font(Font, eNewStyle), zGraphics);
        }

        public void MoveToNextLine(ProjectLayoutElement zElement)
        {
            CurrentLine++;
            CurrentX = 0;
            CurrentY += CurrentLineHeight;
        }
    }
}