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
using System.Linq;
using CardMaker.Card.FormattedText.Markup;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Alignment
{
    public class HorizontalJustifiedAlignmentProcessor : HorizontalAlignmentProcessor
    {
        public override void UpdateLineAlignment(int nFirst, int nLast, bool bLastLine, ProjectLayoutElement zElement, List<MarkupBase> listMarkups, float fVerticalOffset, IEnumerable<MarkupBase> listAllMarkups)
        {
            // detect if this is the last line of markups - if so don't bother with jusity alignment
            if (bLastLine)
            {
                base.UpdateLineAlignment(nFirst, nLast, bLastLine, zElement, listMarkups, fVerticalOffset, listAllMarkups);
                return;
            }
            // detect if this line is followed by a line break - if so don't bother with justify alignment on this line
            var lastLineMarkup = listMarkups[nLast];
            var theBigList = listAllMarkups.ToList();
            var lastLineMarkupIndex = theBigList.IndexOf(lastLineMarkup);
            for (var nIdx = lastLineMarkupIndex + 1; nIdx < theBigList.Count; nIdx++)
            {
                if (theBigList[nIdx] is NewlineMarkup)
                {
                    // no justified alignment due to this line ending with an explicit line break
                    base.UpdateLineAlignment(nFirst, nLast, bLastLine, zElement, listMarkups, fVerticalOffset, listAllMarkups);
                    return;
                }
                if (theBigList[nIdx].Aligns)
                {
                    // the next rendered thing aligns so treat this line as though it is part of a paragraph
                    break;
                }
                // default to justify alignment
            }

            // TODO: the space markups are completely ignored by justified (who cares?)
            var listTextMarkups = listMarkups.GetRange(nFirst, (nLast - nFirst) + 1).Where(zMarkup => !(zMarkup is SpaceMarkup)).ToList();
            var fTotalTextWidth = listTextMarkups.Sum(zMarkup => zMarkup.TargetRect.Width);
            var fDifference = (float)zElement.width - fTotalTextWidth;

            var fXOffset = fDifference / ((float)listTextMarkups.Count - 1);

            //Logger.AddLogLine("TotalTextWidth: {0} Difference: {1} SpaceSize: {2} listTextMarkups: {3}".FormatString(fTotalTextWidth, fDifference, fXOffset, listTextMarkups.Count));
            var fCurrentPosition = listMarkups[nFirst].TargetRect.X;
            for (var nIdx = nFirst; nIdx <= nLast; nIdx++)
            {
                if (listMarkups[nIdx].Aligns && !(listMarkups[nIdx] is SpaceMarkup))
                {
                    var rectCurrent = listMarkups[nIdx].TargetRect;
                    listMarkups[nIdx].TargetRect = new RectangleF(fCurrentPosition,
                        rectCurrent.Y + fVerticalOffset, rectCurrent.Width, rectCurrent.Height);
                    fCurrentPosition += listMarkups[nIdx].TargetRect.Width + fXOffset;
                }
            }
        }
    }
}
