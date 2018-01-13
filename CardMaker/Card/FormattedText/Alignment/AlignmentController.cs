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
    public class AlignmentController
    {
        private enum HorizontalStringAlignment
        {
            Near = StringAlignment.Near,
            Center = StringAlignment.Center,
            Far = StringAlignment.Far,
            Justified
        }

        private static readonly Dictionary<HorizontalStringAlignment, HorizontalAlignmentProcessor> s_dictionaryHorizontalAlignmentProcessors
            = new Dictionary<HorizontalStringAlignment, HorizontalAlignmentProcessor>()
            {
                { HorizontalStringAlignment.Near, new HorizontalLeftAlignmentProcessor() },
                { HorizontalStringAlignment.Center, new HorizontalCenterAlignmentProcessor() },
                { HorizontalStringAlignment.Far, new HorizontalRightAlignmentProcessor() },
                { HorizontalStringAlignment.Justified, new HorizontalJustifiedAlignmentProcessor() },
            };

        private static readonly Dictionary<StringAlignment, VerticalAlignmentProcessor> s_dictionaryVerticalAlignmentProcessors
            = new Dictionary<StringAlignment, VerticalAlignmentProcessor>()
            {
                { StringAlignment.Near, new VerticalTopAlignmentProcessor() },
                { StringAlignment.Center, new VerticalMiddleAlignmentProcessor() },
                { StringAlignment.Far, new VerticalBottomAlignmentProcessor() }
            };

        /// <summary>
        /// Updates the alignment of all alignable markups
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="listAllMarkups"></param>
        public static void UpdateAlignment(ProjectLayoutElement zElement, IEnumerable<MarkupBase> listAllMarkups)
        {
            var listAlignedMarkups = listAllMarkups.Where(x => x.Aligns).ToList();

            if (0 == listAlignedMarkups.Count)
            {
                return;
            }

            var fVertAlignOffset = s_dictionaryVerticalAlignmentProcessors[zElement.GetVerticalAlignment()]
                .GetVerticalAlignOffset(zElement, listAlignedMarkups);

            // if the vertical alignment is so far negative that it is outside the bounds adjust it to chop off at the bottom
            if (0 > listAlignedMarkups[0].TargetRect.Y + fVertAlignOffset)
            {
                fVertAlignOffset = -listAlignedMarkups[0].TargetRect.Y;
            }

            // process horizontal alignment (and add the offset for the vertical alignment)
            // build a list of the index of the first markup of each line
            var listFirstMarkupIndexOfLine = new List<int>();
            var nCurrentLineNumber = -1;
            for (var nMarkupIdx = 0; nMarkupIdx < listAlignedMarkups.Count; nMarkupIdx++)
            {
                if (nCurrentLineNumber != listAlignedMarkups[nMarkupIdx].LineNumber)
                {
                    listFirstMarkupIndexOfLine.Add(nMarkupIdx);
                    nCurrentLineNumber = listAlignedMarkups[nMarkupIdx].LineNumber;
                }
            }

            // iterate over the list of indices of the first markup of each line and align the entire line
            for (var nIdx = 0; nIdx < listFirstMarkupIndexOfLine.Count; nIdx++)
            {
                var bLastLine = nIdx + 1 >= listFirstMarkupIndexOfLine.Count;
                UpdateLineAlignment(
                    listFirstMarkupIndexOfLine[nIdx],
                    bLastLine ? listAlignedMarkups.Count - 1 : listFirstMarkupIndexOfLine[nIdx + 1] - 1,
                    bLastLine, 
                    zElement, 
                    listAlignedMarkups, 
                    fVertAlignOffset, 
                    listAllMarkups);
            }
        }

        /// <summary>
        /// Updates the horizontal alignment and the vertical position of a line of markups 
        /// </summary>
        /// <param name="nFirst"></param>
        /// <param name="nLast"></param>
        /// <param name="zElement"></param>
        /// <param name="listAlignedMarkups">List of Markups (all must have Aligns set to true)</param>
        /// <param name="fVerticalOffset"></param>
        private static void UpdateLineAlignment(int nFirst, int nLast, bool isLastLine, ProjectLayoutElement zElement, List<MarkupBase> listAlignedMarkups, float fVerticalOffset, IEnumerable<MarkupBase> listAllMarkups)
        {
            HorizontalAlignmentProcessor horizontalAlignmentProcessor;
            if (zElement.justifiedtext)
            {
                horizontalAlignmentProcessor =
                    s_dictionaryHorizontalAlignmentProcessors[HorizontalStringAlignment.Justified];
                horizontalAlignmentProcessor.UpdateLineAlignment(nFirst, nLast, isLastLine, zElement, listAlignedMarkups, fVerticalOffset, listAllMarkups);
                return;
            }

            if (nFirst > nLast)
            {
                return;
            }

            // divide the line up based on sets of like alignment
            var alignSetList = new List<SubLineAlignmentSet>();
            var zCurrentAlignmentSet = new SubLineAlignmentSet(
                (HorizontalStringAlignment)listAlignedMarkups[nFirst].StringAlignment,
                nFirst);
            alignSetList.Add(zCurrentAlignmentSet);
            for (var nIdx = nFirst+1; nIdx <= nLast; nIdx++)
            {
                if ((HorizontalStringAlignment) listAlignedMarkups[nIdx].StringAlignment ==
                    zCurrentAlignmentSet.HorizontalStringAlignment)
                {
                    zCurrentAlignmentSet.LastIndex = nIdx;
                }
                else
                {
                    zCurrentAlignmentSet = new SubLineAlignmentSet(
                                    (HorizontalStringAlignment)listAlignedMarkups[nIdx].StringAlignment,
                                    nIdx);
                    alignSetList.Add(zCurrentAlignmentSet);
                }
            }

            // update the alignment of the markups in the line (based on the type of alignment)
            foreach (var zSubLineAlignmentSet in alignSetList)
            {
                horizontalAlignmentProcessor = s_dictionaryHorizontalAlignmentProcessors[zSubLineAlignmentSet.HorizontalStringAlignment];
                horizontalAlignmentProcessor.UpdateLineAlignment(
                    zSubLineAlignmentSet.FirstIndex, 
                    zSubLineAlignmentSet.LastIndex, 
                    isLastLine, 
                    zElement, 
                    listAlignedMarkups, 
                    fVerticalOffset, 
                    listAllMarkups);
            }
        }

        class SubLineAlignmentSet
        {
            public HorizontalStringAlignment HorizontalStringAlignment { get; }
            public int FirstIndex { get; }
            public int LastIndex { get; set; }

            public SubLineAlignmentSet(HorizontalStringAlignment eHorizontalStringAlignment, int nFirstIndex)
            {
                HorizontalStringAlignment = eHorizontalStringAlignment;
                FirstIndex = nFirstIndex;
                LastIndex = nFirstIndex;
            }
        }
    }
}
