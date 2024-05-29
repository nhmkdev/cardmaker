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

using System;
using System.Drawing;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public abstract class MarginHorizontalMarkupBase : MarkupValueBase
    {
        public MarginHorizontalMarkupBase(string sVariable) : base(sVariable) { }

        protected abstract MarginType GetMarginType();

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            var arrayComponents = m_sVariable.Split(new char[] { ';' });
            if (3 != arrayComponents.Length)
            {
                return false;
            }

            if (!int.TryParse(arrayComponents[0], out var nXMargin) || !int.TryParse(arrayComponents[1], out var nYTop) || !int.TryParse(arrayComponents[2], out var nYBottom))
            {
                return false;
            }
            if (nXMargin >= 0 && nYTop < nYBottom)
            {
                zProcessData.AddHorizontalMargin(zElement, GetMarginType(), 
                    Math.Min(nXMargin, zElement.width - 1), 
                    Math.Max(nYTop, 0), 
                    Math.Min(nYBottom, zElement.height));
            }
            return false;
        }
    }
}