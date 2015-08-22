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

using CardMaker.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using CardMaker.XML;

namespace CardMaker.Card
{
    public class CardRenderer
    {
        public Deck CurrentDeck { get; set; }
        public bool DrawElementBorder { get; set; }
        public bool DrawFormattedTextBorder { get; set; }
        public float ZoomLevel { get; set; }

        public void DrawPrintLineToGraphics(Graphics zGraphics)
        {
            DrawCard(0, 0, zGraphics, CurrentDeck.CurrentPrintLine, true, true);
        }

        public void DrawPrintLineToGraphics(Graphics zGraphics, int nX, int nY, bool bDrawBackground)
        {
            DrawCard(nX, nY, zGraphics, CurrentDeck.CurrentPrintLine, true, bDrawBackground);
        }

        public void DrawCard(int nX, int nY, Graphics zGraphics, DeckLine zDeckLine, bool bExport, bool bDrawBackground)
        {
            List<string> listLine = zDeckLine.LineColumns;
#warning this thing is the main choke point of the app, minimize and cache!
            // Custom Graphics Setting
            zGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            zGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            //zGraphics.SmoothingMode = SmoothingMode.HighQuality;

            var matrixOriginal = zGraphics.Transform;

            // Zoom does not apply to export
            if (!bExport)
            {
                zGraphics.ScaleTransform(ZoomLevel, ZoomLevel);
                // Custom Graphics Setting

                zGraphics.InterpolationMode = 1.0f != ZoomLevel
                    ? InterpolationMode.NearestNeighbor
                    : InterpolationMode.Bilinear;
            }

            //Logger.Clear();
            ProjectLayoutElement zSelectedElement = null;

            if (!bExport)
            {
                zSelectedElement = MDILayoutControl.Instance.GetSelectedLayoutElement();
            }

            // draw the background
            if (bDrawBackground)
            {
                zGraphics.FillRectangle(Brushes.White, nX, nY, CurrentDeck.CardLayout.width,
                    CurrentDeck.CardLayout.height);
            }

            // All drawing is handled in reverse element order

            if (null != CurrentDeck.CardLayout.Element)
            {
                for (var nIdx = CurrentDeck.CardLayout.Element.Length - 1; nIdx > -1; nIdx--)
                {
                    var zElement = CurrentDeck.CardLayout.Element[nIdx];
                    if (zElement.enabled) // only add enabled items to draw
                    {
                        MDIIssues.Instance.SetElementName(zElement.name);

                        // get override Element
                        ProjectLayoutElement zOverrideElement = CurrentDeck.GetOverrideElement(zElement, listLine);
                        var zDrawElement = zOverrideElement;

                        // translate any index values in the csv
                        var zElementString = CurrentDeck.TranslateString(zDrawElement.variable, zDeckLine, zDrawElement, bExport);

                        // enabled is re-checked due to possible override of the enabled value
                        if (!zElementString.DrawElement || !zDrawElement.enabled)
                        {
                            continue;
                        }

                        var eType = DrawItem.GetElementType(zDrawElement.type);

                        //NOTE: removed transform backup (draw element resets it anyway...)
                        //if (!bExport) // backup is only necessary for zoomed canvas
                        //{
                            //matrixPrevious = zGraphics.Transform;
                        //}
                        DrawItem.DrawElement(zGraphics, CurrentDeck, zDrawElement, eType, nX, nY, zElementString.String);
                        if (!bExport)
                        {
                            //zGraphics.Transform = matrixPrevious;
                            zGraphics.ScaleTransform(ZoomLevel, ZoomLevel);
                        }
                    }
                }

                if (!bExport)
                {
                    // draw all selections and element borders after everything else
                    for (var nIdx = CurrentDeck.CardLayout.Element.Length - 1; nIdx > -1; nIdx--)
                    {
                        ProjectLayoutElement zElement = CurrentDeck.CardLayout.Element[nIdx];
                        if (zElement.enabled) // only add enabled items to draw
                        {
                            var bDrawSelection = zSelectedElement == zElement;

                            if (DrawElementBorder)
                            {
                                var matrixPrevious = zGraphics.Transform;
                                DrawItem.DrawElementDebugBorder(zGraphics, zElement, nX, nY, bDrawSelection);
                                zGraphics.Transform = matrixPrevious;
                            }
                        }
                    }
                }
            }

            // draw the card border
            if ((bExport && CardMakerMDI.Instance.PrintLayoutBorder) || (!bExport && CurrentDeck.CardLayout.drawBorder))
            {
                // note that the border is inclusive in the width/height consuming 2 pixels (0 to total-1)
                zGraphics.DrawRectangle(Pens.Black, nX, nY, CurrentDeck.CardLayout.width - 1, CurrentDeck.CardLayout.height - 1);
            }

            zGraphics.Transform = matrixOriginal;
        }
    }
}
