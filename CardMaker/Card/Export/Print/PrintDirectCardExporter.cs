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

using System.Drawing;
using System.Drawing.Printing;

namespace CardMaker.Card.Export.Print
{
    public class PrintDirectCardCardExporter : CardPrintBase
    {
        public PrintDirectCardCardExporter(int nLayoutStartIndex, int nLayoutEndIndex, PrintDocument zPrintDocument)
            : base(nLayoutStartIndex, nLayoutEndIndex, zPrintDocument){}

        //public override void OnComplete(object sender) { }

        protected override void DrawPrintCard(int nX, int nY, Graphics zGraphics, DeckLine zDeckLine)
        {
#if false
            if (m_zLastLayout != CurrentDeck.CardLayout)
            {
                UpdateBufferBitmap(
                    CurrentDeck.CardLayout.width,
                    CurrentDeck.CardLayout.height);
                m_zLastLayout = CurrentDeck.CardLayout;
            }
#endif
            // draw directly into the Graphics of the print object
            // store off the previous clip bounds (restored after the card is drawn)
            //RectangleF rectOriginal = zGraphics.ClipBounds;
            // set the clipping for the graphics to the region of the card only (avoid overdraw)
            //zGraphics.SetClip(new RectangleF(nX, nY, CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height));

            //direct drawing the card is MAGICAL with the exception of adobe transparency handling in pdfs
            CardRenderer.DrawCard(nX, nY, zGraphics, zDeckLine, true, true);

            //zGraphics.SetClip(rectOriginal);

        }
    }
}
