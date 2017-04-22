﻿////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public class FontColorMarkup : MarkupValueBase
    {
        private Brush m_zPreviousBrush = Brushes.Black;
        public FontColorMarkup(string sVariable) : base(sVariable) { }

        public override bool ProcessMarkup(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            m_zPreviousBrush = zProcessData.FontBrush;
            zProcessData.FontBrush = new SolidBrush(ProjectLayoutElement.TranslateColorString(m_sVariable));
            return false;
        }

        public override void CloseMarkup(FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            zProcessData.FontBrush = m_zPreviousBrush;
        }
    }
}