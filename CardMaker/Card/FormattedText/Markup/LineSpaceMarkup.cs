﻿////////////////////////////////////////////////////////////////////////////////
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

using System.Drawing;
using CardMaker.XML;
using Support.Util;

namespace CardMaker.Card.FormattedText.Markup
{
    public class LineSpaceMarkup : MarkupValueBase
    {
        private float m_fPreviousLineSpacing;

        public LineSpaceMarkup(string sVariable) : base(sVariable) { }

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            if (ParseUtil.ParseFloat(m_sVariable, out var fLineHeight))
            {
                m_fPreviousLineSpacing = zProcessData.CurrentLineSpacing;
                zProcessData.ConfigureLineHeight(fLineHeight);
            }
            return false;
        }

        public override void CloseMarkup(FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            zProcessData.ConfigureLineHeight(m_fPreviousLineSpacing);
        }
    }
}