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

using System.Collections.Generic;
using System.Drawing;
using CardMaker.Data;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public class SpaceMarkup : MarkupValueBase
    {
        public bool Optional { get; }

        public override bool Aligns => true;

        public SpaceMarkup() : base("1")
        {
        }

        public SpaceMarkup(string sVariable) : base(sVariable)
        {
        }

        public SpaceMarkup(bool bOptional) : base("1")
        {
            Optional = bOptional;
        }

        public override bool ProcessMarkup(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            int nSpaces;
            if (!int.TryParse(m_sVariable, out nSpaces))
            {
                return false;
            }

            StringAlignment = zProcessData.CurrentStringAlignment;
            LineNumber = zProcessData.CurrentLine;

            float fWidth = (float)nSpaces * ((float)zProcessData.FontSpaceWidth + (float)zElement.wordspace);

            if (0 == fWidth)
            {
                return false;
            }

            if (zProcessData.CurrentX + fWidth >= zElement.width)
            {
                if (Optional)
                {
                    return false;
                }
                zProcessData.MoveToNextLine(zElement);
            }

            TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, fWidth, zProcessData.FontSpaceHeight);

            zProcessData.CurrentX += fWidth;
            return true;
        }

        public override bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            return true;
        }

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            // draw border (debugging)
            if (CardMakerInstance.DrawFormattedTextBorder)
            {
                zGraphics.FillRectangle(Optional ? Brushes.DarkBlue : Brushes.DeepSkyBlue, TargetRect.X, TargetRect.Y, TargetRect.Width, TargetRect.Height);
            }
            return true;
        }
    }
}