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

//#define LOG_AUTOSCALE

using System;
using System.Collections.Generic;
using System.Drawing;
using CardMaker.Card.FormattedText;
using CardMaker.Card.FormattedText.Alignment;
using CardMaker.Card.FormattedText.Markup;
using CardMaker.Card.Render.Gradient;
using CardMaker.Data;
using CardMaker.Data.Serialization;
using CardMaker.XML;
using Support.IO;

namespace CardMaker.Card
{
    class DrawFormattedText : IDrawFormattedText
    {
        public void Draw(GraphicsContext zGraphicsContext, Deck zDeck, ProjectLayoutElement zElement, string sInput)
        {
            var zGraphics = zGraphicsContext.Graphics;
#if true
            // check the cache for this item
            var zDataFormattedCache = zDeck.GetCachedMarkup(zElement.name);

            // cached, render and exit
            if (null != zDataFormattedCache)
            {
                zDataFormattedCache.Render(zElement, zGraphics);
                return;
            }
#endif

            zDataFormattedCache = new FormattedTextDataCache();
            var zFormattedTextData = new FormattedTextData(FormattedTextParser.GetMarkups(sInput));

            List<MarkupBase> listRenderedMarkups = null;

            if (zElement.autoscalefont)
            {
                listRenderedMarkups = processAutoScale(zGraphics, zElement, zFormattedTextData);
            }
            else
            {
                listRenderedMarkups = processMarkupDefinition(zGraphics, zElement, zElement.GetElementFont().Size, zFormattedTextData);
            }

            // update the cache
            listRenderedMarkups.ForEach((zMarkup) => zDataFormattedCache.AddMarkup(zMarkup));
            zDeck.AddCachedMarkup(zElement.name, zDataFormattedCache);

            // render!
            zDataFormattedCache.Render(zElement, zGraphics);
        }

        protected List<MarkupBase> processAutoScale(Graphics zGraphics, ProjectLayoutElement zElement, FormattedTextData zFormattedTextData)
        {
            var listRenderedMarkups = processMarkupDefinition(zGraphics, zElement, zElement.GetElementFont().Size, zFormattedTextData);

            getOverflow(zElement, listRenderedMarkups, out var fXOverflow, out var fYOverflow);

            if (fYOverflow == 0) return listRenderedMarkups;

#if LOG_AUTOSCALE
            var zWatch = new System.Diagnostics.Stopwatch();
            zWatch.Start();
#endif

            // the original size was already tested above
            var fFontSize = zElement.GetElementFont().Size - 1;

            if (fFontSize <= 1)
            {
                Logger.AddLogLine("Unable to scale font properly. Result would be a font size smaller than 1. Giving up.");
                return listRenderedMarkups;
            }
            // Slow processing loop to seek the best font size
            var dictionaryAutoScaleSuccess = new Dictionary<float, bool>
            {
                {zElement.GetElementFont().Size, false},
            };

            var fLowerBound = 1f;
            var fUpperBound = fFontSize;
            var fLargestFound = 1f;
            // Binary search for the autoscale fontsize
            while (true)
            {
                fFontSize = (float)(fLowerBound + Math.Ceiling((fUpperBound - fLowerBound) / 2));
                if (dictionaryAutoScaleSuccess.ContainsKey(fFontSize))
                {
                    // exit if we hit an already tested key
#if LOG_AUTOSCALE
                    zWatch.Stop();
                    Logger.AddLogLine($"MS spent ${zWatch.ElapsedMilliseconds} Font Size: {fLargestFound}");
#endif
                    // process again with the largest size and exit (slight non-optimization...)
                    return processMarkupDefinition(zGraphics, zElement, fLargestFound, zFormattedTextData);
                }

#if LOG_AUTOSCALE
                Logger.AddLogLine($"Testing font size: {fFontSize} Lower/Upper:{fLowerBound}/{fUpperBound}");
#endif
                listRenderedMarkups = processMarkupDefinition(zGraphics, zElement, fFontSize, zFormattedTextData);
                getOverflow(zElement, listRenderedMarkups, out fXOverflow, out fYOverflow);
                if (fYOverflow > 0)
                {
                    dictionaryAutoScaleSuccess[fFontSize] = false;
                    fUpperBound = fFontSize;
                }
                else
                {
                    dictionaryAutoScaleSuccess[fFontSize] = true;
                    fLowerBound = fFontSize;
                    fLargestFound = Math.Max(fLargestFound, fFontSize);
                }
            }
        }

        /// <summary>
        /// Gets the overflow (Y value for now) amount based the list of rendered markups
        /// </summary>
        /// <param name="zElement">The element to evaluate</param>
        /// <param name="listRenderedMarkups">The markups to look for overflow with</param>
        /// <param name="fXOverflow">The out horizontal overflow (not yet applicable)</param>
        /// <param name="fYOverflow">The out vertical overflow (not yet applicable)</param>
        protected void getOverflow(ProjectLayoutElement zElement, List<MarkupBase> listRenderedMarkups, out float fXOverflow, out float fYOverflow)
        {
            fYOverflow = 0;
            fXOverflow = 0;
            foreach (var zMarkup in listRenderedMarkups)
            {
                if (zMarkup.TargetRect.Y + zMarkup.TargetRect.Height > zElement.height)
                {
                    fYOverflow = Math.Max(fYOverflow,
                        (zMarkup.TargetRect.Y + zMarkup.TargetRect.Height) - zElement.height);
                }
                // TODO: consider the need for X overflow
            }
        }

        /// <summary>
        /// Processes the formatted text data into rendered markups.
        /// </summary>
        /// <param name="zGraphics">The graphics to use to plan the render</param>
        /// <param name="zElement">The element</param>
        /// <param name="fMainFontSize">The font size to use for the default font</param>
        /// <param name="zFormattedTextData">The processed formatted text data</param>
        /// <returns>List of markups to be rendered</returns>
        protected List<MarkupBase> processMarkupDefinition(Graphics zGraphics, ProjectLayoutElement zElement, float fMainFontSize, FormattedTextData zFormattedTextData)
        {
            var colorFont = zElement.GetElementColor();
            Brush zBrush = 255 == zElement.opacity
                ? new SolidBrush(colorFont)
                : new SolidBrush(Color.FromArgb(zElement.opacity, colorFont));

            if (!string.IsNullOrWhiteSpace(zElement.gradient))
            {
                var zGradientDefinition = GradientProcessor.ProcessGradientStringToBrush(zElement);
                zBrush = zGradientDefinition == null ? zBrush : zGradientDefinition.Brush;
            }

            var zProcessData = new FormattedTextProcessData
            {
                AutoScaleFont = zElement.autoscalefont,
                FontBrush = zBrush,
                CurrentLineSpacing = zElement.lineheight,
                CurrentStringAlignment = zElement.GetHorizontalAlignment(),
                CurrentMarginLeft = 0,
                CurrentMarginRight = zElement.width,
                // these are defaulted because FormattedText elements cannot set these in the UI
                // (ie. don't use stuff set when the element was a Graphic, if it ever was)
                ImageColor = Color.Black,
                CurrentColorMatrix = ColorMatrixSerializer.GetIdentityColorMatrix(),
                CurrentColorType = ElementColorType.Add
            };

            var zFont = zElement.GetElementFont();
            if (null == zFont) // default to something!
            {
                // font will show up in red if it's not yet set
                zFont = FontLoader.DefaultFont;
            }

            var zScaledFont = FontLoader.GetFont(zFont.FontFamily, fMainFontSize, zFont.Style);

            // set the initial font
            zProcessData.SetFont(zScaledFont, zGraphics);

            var listPassMarkups =
                new List<MarkupBase>(); // only contains the markups that will be actively drawn (for caching)

            // Pass 1:
            // - Create rectangles
            // - Configure per-markup settings based on state of markup stack
            // - Generate list of markups to continue to process (those that are used in the next pass)
            // - Specify Markup rectanlges
            // - Generate markup rows (LineNumber is usable AFTER this process)
            int nIdx;
            MarkupBase zMarkup;
            for (nIdx = 0; nIdx < zFormattedTextData.AllMarkups.Count; nIdx++)
            {
                zMarkup = zFormattedTextData.AllMarkups[nIdx];
                if (zMarkup.ProcessMarkup(zElement, zFormattedTextData, zProcessData, zGraphics))
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
                    continue;
                }
                nIdx++;
            }

            return listPassMarkups;
        }
    }
}
