////////////////////////////////////////////////////////////////////////////////
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public class BackgroundColorMarkup : MarkupBase
    {
        private List<RectangleF> m_listRectangles;
        private readonly Brush m_zBrush = Brushes.Black;

        public BackgroundColorMarkup(string sVariable)
        {
            m_zBrush = new SolidBrush(ProjectLayoutElement.TranslateColorString(sVariable));
        }

        public override bool ProcessMarkup(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData,
            Graphics zGraphics)
        {
            return true;
        }

        public override bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            m_listRectangles = new List<RectangleF>();
            for (var nIdx = nMarkup + 1; nIdx < listAllMarkups.Count; nIdx++)
            {
                var zMarkup = listAllMarkups[nIdx];

                // check if the markup is closed
                if (typeof(CloseTagMarkup) == zMarkup.GetType() &&
                    this == ((CloseTagMarkup)zMarkup).MarkupToClose)
                    break;

                m_listRectangles.Add(zMarkup.TargetRect);
            }

            return true;
        }

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            // Custom Graphics Setting
            // disable smoothing so the background color has a sharp edge
            SmoothingMode ePreviousSmoothingMode = zGraphics.SmoothingMode;
            zGraphics.SmoothingMode = SmoothingMode.None;
            foreach (var rect in m_listRectangles)
            {
                var rectAdjusted = rect;
                // do not draw any rectangles outside of the element
                rectAdjusted.Height = Math.Min(rect.Bottom - rect.Top, zElement.y + zElement.height);
                zGraphics.FillRectangle(m_zBrush, rectAdjusted);
            }
            zGraphics.SmoothingMode = ePreviousSmoothingMode;
            return true;
        }
    }
}