////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2020 Tim Stair
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
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public class PushMarkup : MarkupValueBase
    {
        private float m_fXOffset;
        private float m_fYOffset;

        public override bool Aligns => true;

        private PushMarkup()
        {
        }

        public PushMarkup(string sVariable) : base(sVariable)
        {
        }

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            m_fXOffset = zProcessData.CurrentXOffset;
            m_fYOffset = zProcessData.CurrentYOffset;

            var arrayComponents = m_sVariable.Split(new char[] { ';' });
            if (1 > arrayComponents.Length)
            {
                return false;
            }

            int nXPush;
            if (!int.TryParse(arrayComponents[0], out nXPush))
            {
                return false;
            }

            var nYPush = 0;
            if (2 <= arrayComponents.Length)
            {
                if (!int.TryParse(arrayComponents[1], out nYPush))
                {
                    return false;
                }
            }

            TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, 
                Math.Max(0, nXPush),
                Math.Max(0, nYPush));

            zProcessData.CurrentX += nXPush;
            zProcessData.CurrentY += nYPush;

            if (zProcessData.CurrentX > zElement.width)
            {
                zProcessData.MoveToNextLine(zElement);
            }

            return true;
        }

        public override bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            return true;
        }

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            RenderDebugBackground(zGraphics, Brushes.DeepSkyBlue, m_fXOffset, m_fYOffset);
            return true;
        }
    }
}
