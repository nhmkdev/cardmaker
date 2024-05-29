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

using System.Collections.Generic;
using System.Drawing;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;

namespace CardMaker.Card.FormattedText.Markup
{
    public class SpaceMarkup : MarkupValueBase
    {
        private float m_fXOffset;
        private float m_fYOffset;

        public bool Optional { get; }
        public int Spaces { get; private set; }

        public override bool Aligns => true;

        public SpaceMarkup() : base("1")
        {
            Spaces = 1;
        }

        public SpaceMarkup(string sVariable) : base(sVariable)
        {
            int nSpaces;
            if (!int.TryParse(sVariable, out nSpaces))
            {
                nSpaces = 1;
                IssueManager.Instance.FireAddIssueEvent($"Invalid value supplied to space markup: {sVariable}");
            }

            Spaces = nSpaces;
        }

        public SpaceMarkup(bool bOptional) : base("1")
        {
            Spaces = 1;
            Optional = bOptional;
        }

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            m_fXOffset = zProcessData.CurrentXOffset;
            m_fYOffset = zProcessData.CurrentYOffset;

            var fWidth = (float)Spaces * ((float)zProcessData.FontSpaceWidth + (float)zElement.wordspace);

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
            RenderDebugBackground(zGraphics, Optional ? Brushes.DarkBlue : Brushes.DeepSkyBlue, m_fXOffset, m_fYOffset);
            return true;
        }
    }
}