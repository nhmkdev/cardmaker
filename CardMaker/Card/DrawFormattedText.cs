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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CardMaker.Card.FormattedText;
using CardMaker.XML;

namespace CardMaker.Card
{
    static public partial class DrawItem
    {
        public static void DrawFormattedText(Graphics zGraphics, Deck zDeck, ProjectLayoutElement zElement, string sInput, Brush zBrush, Font zFont, Color colorFont)
        {
            // check the cache for this item
            var zDataFormattedCache = zDeck.GetCachedMarkup(zElement.name);

            if (null == zDataFormattedCache)
            {
                if (null == zFont) // default to something!
                {
                    // font will show up in red if it's not yet set
                    zFont = s_zDefaultFont;
                    zBrush = Brushes.Red;
                }

                if (255 != zElement.opacity)
                {
                    zBrush = new SolidBrush(Color.FromArgb(zElement.opacity, colorFont));
                }

                zDataFormattedCache = new FormattedTextDataCache();
                var zFormattedData = new FormattedTextData(GetMarkups(sInput));
                var zProcessData = new FormattedTextProcessData();

                // set the initial font
                zProcessData.FontBrush = zBrush;
                zProcessData.SetFont(zFont, zGraphics);

                var listPassMarkups = new List<MarkupBase>(); // only contains the markups that will be actively drawn (for caching)

                // Pass 1:
                // - Create rectangles
                // - Configure per-markup settings based on state of markup stack
                // - Generate list of markups to continue to process (those that are used in the next pass)
                // - Specify Markup rectanlges
                // - Generate markup rows
                var nIdx = 0;
                MarkupBase zMarkup;
                for (nIdx = 0; nIdx < zFormattedData.AllMarkups.Count; nIdx++)
                {
                    zMarkup = zFormattedData.AllMarkups[nIdx];
                    if (zMarkup.ProcessMarkup(zElement, zFormattedData, zProcessData, zGraphics))
                    {
                        zMarkup.LineNumber = zProcessData.CurrentLine;
                        listPassMarkups.Add(zMarkup);
                    }
                }

                // Pass 2:
                // - Trim spaces from line endings
                if (listPassMarkups.Count > 0)
                {
                    nIdx = listPassMarkups.Count - 1;
                    zMarkup = listPassMarkups[nIdx];
                    var currentLineNumber = zMarkup.LineNumber;
                    var bFindNextLine = false;
                    while (nIdx > -1)
                    {
                        zMarkup = listPassMarkups[nIdx];
                        if (zMarkup.LineNumber != currentLineNumber)
                        {
                            currentLineNumber = zMarkup.LineNumber;
                            bFindNextLine = false;
                        }

                        if (!bFindNextLine && zMarkup is SpaceMarkup && ((SpaceMarkup) zMarkup).Optional)
                        {
                            listPassMarkups.RemoveAt(nIdx);
                        }
                        else
                        {
                            bFindNextLine = true;
                        }
                        nIdx--;
                    }
                }

                // Pass 3:
                // - Align lines (horizontal/vertical)

                // Reprocess for align (before backgroundcolor is configured)
                UpdateAlignment(zElement, listPassMarkups);

                // Pass 4: process the remaining items
                nIdx = 0;
                while (nIdx < listPassMarkups.Count)
                {
                    zMarkup = listPassMarkups[nIdx];
                    if (!zMarkup.PostProcessMarkupRectangle(zElement, listPassMarkups, nIdx))
                    {
                        listPassMarkups.RemoveAt(nIdx);
                        nIdx--;
                    }
                    else
                    {
                        zDataFormattedCache.AddMarkup(zMarkup);
                    }
                    nIdx++;
                }
                
                // update the cache
                zDeck.AddCachedMarkup(zElement.name, zDataFormattedCache);
            }

            zDataFormattedCache.Render(zElement, zGraphics);
        }

        /// <summary>
        /// Updates the alignment of all alignable markups
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="listMarkups"></param>
        private static void UpdateAlignment(ProjectLayoutElement zElement, IEnumerable<MarkupBase> listAllMarkups)
        {
            var listMarkups = listAllMarkups.Where(x => x.Aligns).ToList();

            if (0 == listMarkups.Count)
            {
                return;
            }

            var nVertAlignOffset = GetVerticalAlignOffset(zElement, listMarkups);

            // if the vertical alignment is so far negative that it is outside the bounds adjust it to chop off at the bottom
            if (0 > listMarkups[0].TargetRect.Y + nVertAlignOffset)
            {
                nVertAlignOffset = -listMarkups[0].TargetRect.Y;
            }

            // If this left aligned, just update the vertical alignment and exit
            if (StringAlignment.Near == (StringAlignment)zElement.horizontalalign)
            {
                // check if there is nothing to do
                if (0f == nVertAlignOffset)
                {
                    return;
                }

                // just update the y offset
                foreach (var zMarkup in listMarkups)
                {
                    zMarkup.TargetRect = new RectangleF(zMarkup.TargetRect.X, zMarkup.TargetRect.Y + nVertAlignOffset, zMarkup.TargetRect.Width, zMarkup.TargetRect.Height);
                }
                return;
            }

            // process horizontal alignment (and add the offset for the vertical alignment)
            var currentLineNumber = listMarkups[0].LineNumber;
            var nFirstWord = 0;
            int nLastWord;
            for (var nIdx = 1; nIdx < listMarkups.Count; nIdx++)
            {
                if (listMarkups[nIdx].LineNumber != currentLineNumber)
                {
                    nLastWord = nIdx - 1;
                    UpdateLineAlignment(nFirstWord, nLastWord, zElement, listMarkups, nVertAlignOffset);
                    currentLineNumber = listMarkups[nIdx].LineNumber;
                    nFirstWord = nIdx;
                }
            }
            nLastWord = listMarkups.Count - 1;
            UpdateLineAlignment(nFirstWord, nLastWord, zElement, listMarkups, nVertAlignOffset);
        }

        /// <summary>
        /// Updates the horizontal alignment of a line of markups
        /// </summary>
        /// <param name="nFirst"></param>
        /// <param name="nLast"></param>
        /// <param name="zElement"></param>
        /// <param name="listMarkups">List of Markups (all must have Aligns set to true)</param>
        /// <param name="nVerticalAlignOffset"></param>
        private static void UpdateLineAlignment(int nFirst, int nLast, ProjectLayoutElement zElement, List<MarkupBase> listMarkups, float nVerticalAlignOffset)
        {
            var rectLast = listMarkups[nLast].TargetRect;
            var fXOffset = -1f; // HACK: slight fudge

            switch ((StringAlignment)zElement.horizontalalign)
            {
                case StringAlignment.Center:
                    fXOffset += (zElement.width - (rectLast.X + rectLast.Width)) / 2f;
                    break;
                case StringAlignment.Far:
                    fXOffset += zElement.width - (rectLast.X + rectLast.Width);
                    break;
            }

            for (var nIdx = nFirst; nIdx <= nLast; nIdx++)
            {
                var rectCurrent = listMarkups[nIdx].TargetRect;
                listMarkups[nIdx].TargetRect = new RectangleF(rectCurrent.X + fXOffset, rectCurrent.Y + nVerticalAlignOffset, rectCurrent.Width, rectCurrent.Height);
            }
        }

        /// <summary>
        /// Gets the vertical alignment offset based on the desired alignment (the amount to move all markups "down")
        /// </summary>
        /// <param name="zElement">The element to operate with</param>
        /// <param name="listMarkups">List of Markups (all must have Aligns set to true)</param>
        /// <returns></returns>
        private static float GetVerticalAlignOffset(ProjectLayoutElement zElement, List<MarkupBase> listMarkups)
        {
            if (0 == listMarkups.Count)
            {
                return 0;
            }

            var rectLast = listMarkups[listMarkups.Count - 1].TargetRect;
            var nEndOfLastLine = rectLast.Y + rectLast.Height;
            switch ((StringAlignment)zElement.verticalalign)
            {
                case StringAlignment.Center:
                    return ((float)(zElement.height - nEndOfLastLine)) / 2f;
                case StringAlignment.Far:
                    return (float)zElement.height - nEndOfLastLine;
            }
            return 0;
        }

        /// <summary>
        /// Markup parser
        /// </summary>
        /// <param name="sInput"></param>
        /// <returns></returns>
        public static List<MarkupBase> GetMarkups(string sInput)
        {
            var listMarkups = new List<MarkupBase>();

            var nIdx = 0;
            var bEscapeNext = false;
            var bInTag = false;
            var bCloseTag = false;

            var sTagName = string.Empty;
            var sBuilder = new StringBuilder();

            while (sInput.Length > nIdx)
            {
                if (bEscapeNext)
                {
                    bEscapeNext = false;
                    switch (sInput[nIdx])
                    {
                        case '<':
                        case '>':
                        case '\\':
                            sBuilder.Append(sInput[nIdx]);
                            nIdx++;
                            continue;
                        default:
                            // append the missing \
                            sBuilder.Append("\\");
                            break;
                    }
                }

                var cCurrent = sInput[nIdx];
                var cLast = nIdx > 0 ? sInput[nIdx - 1] : ' ';

                switch (cCurrent)
                {
                    case '\\':
                        if (!bInTag)
                        {
                            bEscapeNext = true;
                        }
                        else
                        {
                            sTagName += "\\";
                        }
                        break;
                    case '<':
                        if (!bInTag && sBuilder.Length > 0)
                        {
                            listMarkups.Add(new TextMarkup(sBuilder.ToString()));
                        }
                        bInTag = true;
                        sTagName = string.Empty;
                        sBuilder = new StringBuilder();
                        break;
                    case '>':
                        bInTag = false;
                        if (bCloseTag)
                        {
                            // find the tag to actually close
                            var zMarkupTypeToSeek = MarkupBase.GetMarkupType(sTagName);
                            if (null != zMarkupTypeToSeek)
                            {
                                for (int nMarkup = listMarkups.Count - 1; nMarkup > -1; nMarkup--)
                                {
                                    if (listMarkups[nMarkup].GetType() == zMarkupTypeToSeek)
                                    {
                                        sTagName = string.Empty;
                                        listMarkups.Add(new CloseTagMarkup(listMarkups[nMarkup]));
                                        break;
                                    }
                                }
                            }
                            bCloseTag = false;
                            // otherwise it is a junk markup, toss it out
                        }
                        else
                        {
                            var zMarkup = MarkupBase.GetMarkup(sTagName);
                            if (null != zMarkup)
                            {
                                // prevent continued adds of the same tag
                                sTagName = string.Empty;
                                listMarkups.Add(zMarkup);
                            }
                        }
                        break;
                    case ' ':
                        if (!bInTag && sBuilder.Length > 0)
                        {
                            listMarkups.Add(new TextMarkup(sBuilder.ToString()));
                            sBuilder = new StringBuilder();
                        }
                        if (bInTag)
                        {
                            sTagName += cCurrent;
                        }
                        else
                        {

                            if (listMarkups.Count == 0)
                            {
                                break;
                            }
                            var zMarkupBase = listMarkups[listMarkups.Count - 1];
                            if (!(zMarkupBase is SpaceMarkup))
                            {
                                // create a trimmable space (can be trimmed from end of line)
                                listMarkups.Add(new SpaceMarkup(true));
                            }
                        }
                        break;
                    default:
                        if (bInTag)
                        {
                            // this is just lazy, not checking if this is actually after the <
                            if ('/' == cCurrent && cLast == '<')
                            {
                                bCloseTag = true;
                            }
                            else
                            {
                                sTagName += cCurrent;
                            }
                        }
                        else
                        {
                            sBuilder.Append(cCurrent);
                        }
                        break;
                }
                nIdx++;
            }

            // trailing escape code char
            if (bEscapeNext)
                sBuilder.Append("\\");

            if (!bInTag && sBuilder.Length > 0)
            {
                listMarkups.Add(new TextMarkup(sBuilder.ToString()));
            }
            return listMarkups;
        }
    }
}
