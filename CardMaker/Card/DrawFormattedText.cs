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
using CardMaker.Card.FormattedText;
using CardMaker.Card.FormattedText.Alignment;
using CardMaker.Card.FormattedText.Markup;
using CardMaker.XML;

namespace CardMaker.Card
{
    class DrawFormattedText : IDrawFormattedText
    {
        public void Draw(Graphics zGraphics, Deck zDeck, ProjectLayoutElement zElement, string sInput)
        {
            var zFont = zElement.GetElementFont();
            var colorFont = zElement.GetElementColor();

            // check the cache for this item
            var zDataFormattedCache = zDeck.GetCachedMarkup(zElement.name);

            // cached, render and exit
            if (null != zDataFormattedCache)
            {
                zDataFormattedCache.Render(zElement, zGraphics);
                return;
            }

            if (null == zFont) // default to something!
            {
                // font will show up in red if it's not yet set
                zFont = FontLoader.DefaultFont;
            }

            var zBrush = 255 == zElement.opacity
                ? new SolidBrush(colorFont)
                : new SolidBrush(Color.FromArgb(zElement.opacity, colorFont));

            zDataFormattedCache = new FormattedTextDataCache();
            var zFormattedData = new FormattedTextData(FormattedTextParser.GetMarkups(sInput));
            var zProcessData = new FormattedTextProcessData
            {
                FontBrush = zBrush,
                CurrentLineHeight = zElement.lineheight,
                CurrentStringAlignment = zElement.GetHorizontalAlignment()
            };

            // set the initial font
            zProcessData.SetFont(zFont, zGraphics);

            var listPassMarkups = new List<MarkupBase>(); // only contains the markups that will be actively drawn (for caching)

            // Pass 1:
            // - Create rectangles
            // - Configure per-markup settings based on state of markup stack
            // - Generate list of markups to continue to process (those that are used in the next pass)
            // - Specify Markup rectanlges
            // - Generate markup rows
            int nIdx;
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

                    if (!bFindNextLine && zMarkup is SpaceMarkup && ((SpaceMarkup)zMarkup).Optional)
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
            AlignmentController.UpdateAlignment(zElement, listPassMarkups);

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

            // render!
            zDataFormattedCache.Render(zElement, zGraphics);
        }
    }
}
