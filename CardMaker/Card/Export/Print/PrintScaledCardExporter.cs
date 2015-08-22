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
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace CardMaker.Card.Export.Print
{
    public class PrintScaledCardExporter : CardPrintBase
    {
        public PrintScaledCardExporter(int nLayoutStartIndex, int nLayoutEndIndex, PrintDocument zPrintDocument)
            : base(nLayoutStartIndex, nLayoutEndIndex, zPrintDocument)
        {
        }

        public override void OnComplete(object sender) { }
      
        protected override void DrawPrintCard(int nX, int nY, Graphics zGraphics, DeckLine zDeckLine)
        {
            if (m_zLastLayout != CurrentDeck.CardLayout)
            {
                UpdateBufferBitmap(
                    CurrentDeck.CardLayout.width,
                    CurrentDeck.CardLayout.height);
                m_zLastLayout = CurrentDeck.CardLayout;
            }

#warning more control over various settings might be useful...
            var zGraphicsBuffer = Graphics.FromImage(m_zExportCardBuffer);

            CardRenderer.DrawCard(0, 0, zGraphicsBuffer, zDeckLine, true, true);
            
            // Custom Graphics Setting
            var eCQ = zGraphics.CompositingQuality;
            var eIM = zGraphics.InterpolationMode;
            var eSM = zGraphics.SmoothingMode;

            zGraphics.CompositingQuality = CompositingQuality.HighQuality;
            zGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            zGraphics.SmoothingMode = SmoothingMode.HighQuality;
            //zGraphics.TextRenderingHint
            //zGraphics.CompositingMode
            // NOTE: width and height output size MUST be specified.
            zGraphics.DrawImage(
                m_zExportCardBuffer, 
                (float)nX, 
                (float)nY,
                (float)CurrentDeck.CardLayout.width,
                (float)CurrentDeck.CardLayout.height
                );
            //Logger.AddLogLine("Drawing to " + nX + ":" + nY + "::" + m_zExportCardBuffer.Width + "," + m_zExportCardBuffer.Height);

            zGraphics.CompositingQuality = eCQ;
            zGraphics.InterpolationMode = eIM;
            zGraphics.SmoothingMode = eSM;
        }
        // NOTE: this just did more harm than good...
#if false
        protected override void UpdateLayoutBounds(ProjectLayout zLayout, ProjectLayout zPreviousLayout, PageSettings zPageSettings)
        {
            m_fPrintScaleDenominator = (float)CardMakerMDI.Instance.PrintDPI / (float)zPageSettings.PrinterResolution.X;
            Logger.AddLogLine("Print DPI: " + CardMakerMDI.Instance.PrintDPI + " PrinterRes: " + (float)zPageSettings.PrinterResolution.X + " RESULT: " + m_fPrintScaleDenominator);
            base.UpdateLayoutBounds(zLayout, zPreviousLayout, zPageSettings);
        }
#endif
    }
}
