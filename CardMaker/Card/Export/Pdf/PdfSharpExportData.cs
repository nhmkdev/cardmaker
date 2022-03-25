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

namespace CardMaker.Card.Export.Pdf
{
    public class PdfSharpExportData
    {
        public double DrawX { get; set; }
        public double DrawY { get; set; }
        
        public double LayoutPointWidth { get; set; }
        public double LayoutPointHeight { get; set; }
        
        public double BufferX { get; set; }
        public double BufferY { get; set; }

        public double PageMarginX { get; set; }
        public double PageMarginEndX { get; set; }
        public double PageMarginY { get; set; }
        public double PageMarginEndY { get; set; }
        public double NextRowYAdjust { get; set; }
        public Deck Deck { get; set; }

        /// <summary>
        /// Gets the number of items that would fit per row based on the current layout and page
        /// </summary>
        /// <returns>The number of items that would fit in the row</returns>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        /// <param name="nValidLineCount">The number of valid lines in total</param>
        public int GetItemsPerRow(int nNextExportIndex)
        {
            // remaining space in row based on layout vs. remaining cards to fit (last row may have fewer than the space would fit)
            return (int)Math.Floor(
                Math.Min(
                    (PageMarginEndX - PageMarginX) / LayoutPointWidth,
#warning should this be Deck.CardCount not validlines?
                    Deck.ValidLines.Count - nNextExportIndex)
            );
        }
    }
}
