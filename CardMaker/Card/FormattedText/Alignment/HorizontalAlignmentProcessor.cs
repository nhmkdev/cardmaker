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
using CardMaker.Card.FormattedText.Markup;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Alignment
{
    public abstract class HorizontalAlignmentProcessor
    {
        /// <summary>
        /// Updates the position of the markups based on horizontal alignment
        /// </summary>
        /// <param name="nFirst">First index of the listMarkups in the line (start of line)</param>
        /// <param name="nLast">Last index of the listMarkups in the line (end of line) -- inclusive</param>
        /// <param name="bLastLine">Indicates this is the last line (specific to justified)</param>
        /// <param name="zElement">The element being rendered</param>
        /// <param name="listMarkups">List of Markups (all must have Aligns set to true)</param>
        /// <param name="fVerticalOffset">Any vertical offset to apply</param>
        /// <param name="listAllMarkups">List of all markups (even those with Aligns set to false)  (specific to justified)</param>
        public virtual void UpdateLineAlignment(int nFirst, int nLast, bool bLastLine, ProjectLayoutElement zElement,
            List<MarkupBase> listMarkups, float fVerticalOffset, IEnumerable<MarkupBase> listAllMarkups)
        {
            var fHorizontalOffset = GetHorizontalOffset(zElement, listMarkups[nLast].TargetRect);
            for (var nIdx = nFirst; nIdx <= nLast; nIdx++)
            {
                var rectCurrent = listMarkups[nIdx].TargetRect;
                listMarkups[nIdx].TargetRect = new RectangleF(rectCurrent.X + fHorizontalOffset, rectCurrent.Y + fVerticalOffset, rectCurrent.Width, rectCurrent.Height);
            }
        }

        public virtual float GetHorizontalOffset(ProjectLayoutElement zElement, RectangleF rectLast)
        {
            return 0f;
        }
    }
}
