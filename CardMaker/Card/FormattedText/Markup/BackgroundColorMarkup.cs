////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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
using Support.IO;
using Support.Util;

namespace CardMaker.Card.FormattedText.Markup
{
    public class BackgroundColorMarkup : MarkupValueBase
    {
        private List<RectangleF> m_listRectangles;
        private Brush m_zBrush = Brushes.Black;
        private float m_fAdditionalHorizontalPixels;
        private float m_fAdditionalVerticalPixels;
        private float m_fXOffset;
        private float m_fYOffset;

        public BackgroundColorMarkup(string sVariable) : base(sVariable) { }

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData,
            Graphics zGraphics)
        {
            var arrayComponents = m_sVariable.Split(new char[] { ';' });

            var fMarkupXOffset = 0f;
            var fMarkupYOffset = 0f;
            m_fAdditionalVerticalPixels = 0f;

            if (arrayComponents.Length > 0)
            {
                m_zBrush = new SolidBrush(ProjectLayoutElement.TranslateColorString(arrayComponents[0], zElement.opacity));

                if (arrayComponents.Length == 2)
                {
                    ParseUtil.ParseFloat(arrayComponents[1], out m_fAdditionalVerticalPixels);
                }
                else if (arrayComponents.Length > 4)
                {
                    ParseUtil.ParseFloat(arrayComponents[1], out fMarkupXOffset);
                    ParseUtil.ParseFloat(arrayComponents[2], out fMarkupYOffset);
                    ParseUtil.ParseFloat(arrayComponents[3], out m_fAdditionalHorizontalPixels);
                    ParseUtil.ParseFloat(arrayComponents[4], out m_fAdditionalVerticalPixels);
                }
            }

            m_fXOffset = zProcessData.CurrentXOffset + fMarkupXOffset;
            m_fYOffset = zProcessData.CurrentYOffset + fMarkupYOffset;
            return true;
        }

        public override bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            m_listRectangles = new List<RectangleF>();
            if (listAllMarkups == null
                || listAllMarkups.Count == 0
                // need to have a markup after the bgc is started (otherwise just move along)
                || listAllMarkups.Count < nMarkup + 2)
            {
                return true;
            }

            var nMarkupLineNumber = -1;
            float fX = 0, fY = 0, fRight = 0, fBottom = 0;
            var nIdx = nMarkup + 1;

            for (; nIdx < listAllMarkups.Count; nIdx++)
            {
                var zMarkup = listAllMarkups[nIdx];
                // if the markup rectangle does not have size it is not considered for background rendering
                if (RectangleF.Empty != zMarkup.TargetRect)
                {
                    if (zMarkup.LineNumber != nMarkupLineNumber)
                    {
                        if (nMarkupLineNumber != -1)
                        {
                            AddRowRectangle(new RectangleF(fX, fY, fRight - fX, fBottom - fY));
                        }
                        fX = zMarkup.TargetRect.X;
                        fY = zMarkup.TargetRect.Y;
                        fBottom = zMarkup.TargetRect.Bottom;
                        fRight = zMarkup.TargetRect.Right;
                        nMarkupLineNumber = zMarkup.LineNumber;
                    }
                    else
                    {
                        // get the max bounds
                        fY = Math.Min(fY, zMarkup.TargetRect.Y);
                        fX = Math.Min(fX, zMarkup.TargetRect.X);
                        fBottom = Math.Max(fBottom, zMarkup.TargetRect.Bottom);
                        fRight = Math.Max(fRight, zMarkup.TargetRect.Right);
                    }
                }

                // check if the markup is closed
                if (typeof(CloseTagMarkup) == zMarkup.GetType() &&
                    this == ((CloseTagMarkup)zMarkup).MarkupToClose)
                    break;
            }

            // always add the last markup (should never be null)
            AddRowRectangle(new RectangleF(fX, fY, fRight - fX, fBottom - fY));

            return true;
        }

        private void AddRowRectangle(RectangleF rect)
        {
            if (RectangleF.Empty != rect)
            {
                m_listRectangles.Add(rect);
            }
        }

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            // Custom Graphics Setting
            // disable smoothing so the background color has a sharp edge
            SmoothingMode ePreviousSmoothingMode = zGraphics.SmoothingMode;
            zGraphics.SmoothingMode = SmoothingMode.None;
            foreach (var rect in m_listRectangles)
            {
                var rectAdjusted = new RectangleF(rect.X + m_fXOffset, rect.Y + m_fYOffset, rect.Width + m_fAdditionalHorizontalPixels, rect.Height + m_fAdditionalVerticalPixels);

                // do not draw any rectangles outside of the element
                rectAdjusted.Height = Math.Min(rectAdjusted.Bottom - rectAdjusted.Top, zElement.y + zElement.height);
                zGraphics.FillRectangle(m_zBrush, rectAdjusted);
            }
            zGraphics.SmoothingMode = ePreviousSmoothingMode;
            return true;
        }
    }
}