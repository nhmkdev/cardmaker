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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using CardMaker.Card.Render;
using CardMaker.Card.Shapes;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;

namespace CardMaker.Card
{
    public class CardRenderer
    {
        private static readonly Pen s_zPenDebugBorder = new Pen(Color.FromArgb(196, Color.Red), 1);
        private static readonly Pen s_zPenDebugGuides = new Pen(Color.FromArgb(196, Color.LightPink), 1);
        private static readonly Pen m_zPenElementSelect = Pens.ForestGreen;
        private readonly Pen penDivider = new Pen(Color.FromArgb(64, Color.Blue));

        // it's not spring, it's hard coded!
        private static readonly IDrawGraphic s_zDrawGraphic = new DrawGraphic();
        private static readonly IDrawFormattedText s_zDrawFormattedText = new DrawFormattedText();
        private static readonly IDrawText s_zDrawText = new DrawTextGraphics();
        private static readonly IShapeRenderer s_zShapeRenderer = new ShapeManager();

        /// <summary>
        /// Render order when drawing an element
        /// </summary>
        private static readonly List<IElementRenderProcessor> s_listElementRenderProcessors =
            new List<IElementRenderProcessor>()
            {
                new InputElementRenderProcessor(),
                new TransformElementRenderProcessor(),
                new BackgroundColorElementRenderProcessor(),
                new InlineElementRenderProcessor(s_zShapeRenderer, s_zDrawGraphic),
                new TypeElementRenderProcessor(s_zShapeRenderer, s_zDrawGraphic, s_zDrawFormattedText, s_zDrawText),
                new BorderElementRenderProcessor()
            };

        public Deck CurrentDeck { get; set; }
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
            // Custom Graphics Setting
            zGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
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

            // draw the card background
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
                        IssueManager.Instance.FireChangeElementEvent(zElement.name);

                        // get override Element (overrides based on data source) (this is a copy!)
                        // This takes place before translation to cover the odd case where the variable field is override in the data source
                        var zDrawElement = CurrentDeck.GetOverrideElement(zElement, zDeckLine, bExport);

                        // translate any index values in the csv
                        var zElementString = CurrentDeck.TranslateString(zDrawElement.variable, zDeckLine, zDrawElement, bExport);

                        // get override Element (based on any overrides in the element variable string)
                        zDrawElement = CurrentDeck.GetVariableOverrideElement(zDrawElement, zElementString.OverrideFieldToValueDictionary);

                        // enabled is re-checked due to possible override of the enabled value
                        if (!zElementString.DrawElement || !zDrawElement.enabled)
                        {
                            continue;
                        }

                        // initialize the translated fields on the element to draw
                        zDrawElement.InitializeTranslatedFields();
                   
                        DrawElement(zGraphics, CurrentDeck, zDrawElement, nX, nY, zElementString.String, bExport);
                        if (!bExport)
                        {
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
                        if (zElement.enabled
                            && CardMakerInstance.DrawElementBorder) // only add enabled items to draw
                        {
                            DrawElementDebugBorder(zGraphics, zElement, nX, nY, ElementManager.Instance.GetSelectedElement() == zElement);
                        }
                    }
                }
            }

            DrawLayoutDividers(zGraphics, bExport);

            // draw the card border
            if ((bExport && CardMakerSettings.PrintLayoutBorder) 
                || (!bExport && CurrentDeck.CardLayout.drawBorder))
            {
                // note that the border is inclusive in the width/height consuming 2 pixels (0 to total-1)
                zGraphics.DrawRectangle(Pens.Black, nX, nY, CurrentDeck.CardLayout.width - 1, CurrentDeck.CardLayout.height - 1);
            }

            zGraphics.Transform = matrixOriginal;
        }

        /// <summary>
        /// Draws the element to the graphics at the specified position
        /// </summary>
        /// <param name="zGraphics">The object to render to</param>
        /// <param name="zDeck">The Deck to operate with</param>
        /// <param name="zElement">The Element to render</param>
        /// <param name="nX">The x position to render at</param>
        /// <param name="nY">The y position to render at</param>
        /// <param name="sInput">The input string to render the element with</param>
        /// <param name="bExport">Flag indicating if this is an export</param>
        private void DrawElement(Graphics zGraphics, Deck zDeck, ProjectLayoutElement zElement,
            int nX, int nY, string sInput, bool bExport)
        {
            s_listElementRenderProcessors.ForEach(zRenderProcessor => sInput = zRenderProcessor.Render(zGraphics, zElement, zDeck, sInput, nX, nY, bExport));

            // always reset the transform
            zGraphics.ResetTransform();
        }

        private void DrawLayoutDividers(Graphics zGraphics, bool bExport)
        {
            if (!bExport && CardMakerInstance.DrawLayoutDividers)
            {
                if (CardMakerInstance.LayoutDividerHorizontalCount > 0)
                {
                    int nSectionWidth = CurrentDeck.CardLayout.width / (CardMakerInstance.LayoutDividerHorizontalCount + 1);
                    var nHorizontalPos = 0;
                    for (var nLine = 0; nLine < CardMakerInstance.LayoutDividerHorizontalCount; nLine++)
                    {
                        nHorizontalPos += nSectionWidth;

                        zGraphics.DrawLine(penDivider, nHorizontalPos, 0, nHorizontalPos, CurrentDeck.CardLayout.height);
                    }
                }
                if (CardMakerInstance.LayoutDividerVerticalCount > 0)
                {
                    int nSectionHeight = CurrentDeck.CardLayout.height / (CardMakerInstance.LayoutDividerVerticalCount + 1);
                    var nVerticalPos = 0;
                    for (var nLine = 0; nLine < CardMakerInstance.LayoutDividerVerticalCount; nLine++)
                    {
                        nVerticalPos += nSectionHeight;
                        zGraphics.DrawLine(penDivider, 0, nVerticalPos, CurrentDeck.CardLayout.width, nVerticalPos);
                    }
                }
            }
        }

        /// <summary>
        /// Renders the element debug border based on the state of the element / application
        /// </summary>
        /// <param name="zGraphics">The object to render to</param>
        /// <param name="zElement">The Element to render</param>
        /// <param name="nX">The x position to render at</param>
        /// <param name="nY">The y position to render at</param>
        /// <param name="bSelected">Flag indicating if this element is currently selected</param>
        public static void DrawElementDebugBorder(Graphics zGraphics, ProjectLayoutElement zElement, int nX, int nY, bool bSelected)
        {
            var matrixPrevious = zGraphics.Transform;
            // note that the border is inclusive in the width/height consuming 2 pixels (0 to total-1)
            zGraphics.TranslateTransform(nX, nY);
            if (bSelected && CardMakerInstance.DrawSelectedElementGuides)
            {
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(0, zElement.y), new PointF(zGraphics.ClipBounds.Width, zElement.y));
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(0, zElement.y + zElement.height - 1),
                    new PointF(zGraphics.ClipBounds.Width, zElement.y + zElement.height));
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(zElement.x, 0), new PointF(zElement.x, zGraphics.ClipBounds.Height));
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(zElement.x + zElement.width - 1, 0),
                    new PointF(zElement.x + zElement.width, zGraphics.ClipBounds.Height));
            }
            zGraphics.DrawRectangle(s_zPenDebugBorder, zElement.x, zElement.y, zElement.width - 1, zElement.height - 1);
            if (bSelected)
            {
                zGraphics.DrawRectangle(m_zPenElementSelect, zElement.x - 2, zElement.y - 2, zElement.width + 3, zElement.height + 3);
            }
            zGraphics.Transform = matrixPrevious;
        }

        /// <summary>
        /// Renders the outline if the specified element has a non-zero outline
        /// </summary>
        /// <param name="zElement">The element to use the thickness value of</param>
        /// <param name="zGraphics">The graphics to render to</param>
        /// <param name="zPath">The path to draw the outline on</param>
        public static void DrawPathOutline(ProjectLayoutElement zElement, Graphics zGraphics, GraphicsPath zPath)
        {
            // draw the outline
            if (0 >= zElement.outlinethickness)
            {
                return;
            }
            var outlinePen = new Pen(Color.FromArgb(zElement.opacity, zElement.GetElementOutlineColor()),
                zElement.outlinethickness)
            {
                LineJoin = LineJoin.Round
            };
#warning This outline pen linejoin should be customizable (as it rounds the corners but corrects other issues!)
            zGraphics.DrawPath(outlinePen, zPath);
        }
    }
}
