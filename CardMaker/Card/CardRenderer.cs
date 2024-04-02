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

using System;
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
        private static readonly IDrawText s_zDrawText =
#if false
            new DrawTextTextRenderer();
#else
            new DrawTextGraphics();
#endif
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
                        var zElement = ProjectManager.Instance.LookupElementReference(CurrentDeck.CardLayout.Element[nIdx]);
                        if (zElement.enabled) // only add enabled items to draw
                        {
                            DrawElementDebugBorder(zGraphics, zElement, nX, nY, ElementManager.Instance.GetSelectedElement() == zElement);
                        }
                    }
                }
            }

            DrawLayoutDividers(zGraphics, bExport);
            DrawBorder(zGraphics, nX, nY, CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height, CurrentDeck.CardLayout, bExport);

            zGraphics.Transform = matrixOriginal;
        }

        /// <summary>
        /// Draws a single pixel border using the specified inputs
        /// </summary>
        /// <param name="zGraphics">Graphics to draw on</param>
        /// <param name="nX">x position</param>
        /// <param name="nY">y position</param>
        /// <param name="nWidth">width</param>
        /// <param name="nHeight">height</param>
        /// <param name="bLayoutDrawBorder">Flag indicating if the layout has the draw border flag set</param>
        /// <param name="bExport">Flag indicating if this is an export</param>
        public static void DrawBorder(Graphics zGraphics, int nX, int nY, int nWidth, int nHeight, ProjectLayout zLayout, bool bExport)
        {
            // draw the card border
            if (bExport
                && zLayout.exportLayoutBorder
                && zLayout.drawBorder)
            {
                if (zLayout.exportLayoutBorderCrossSize == 0)
                {
                    DrawSolidBorder(zGraphics, nX, nY, nWidth, nHeight);
                }
                else
                {
                    DrawBorderEdges(zGraphics, nX, nY, nWidth, nHeight, zLayout.exportLayoutBorderCrossSize);
                }
            }
            else if(!bExport && zLayout.drawBorder)
            {
                DrawSolidBorder(zGraphics, nX, nY, nWidth, nHeight);
            }
        }

        private static void DrawSolidBorder(Graphics zGraphics, int nX, int nY, int nWidth, int nHeight)
        {
            var nLastHorizontalPixel = nWidth - 1;
            var nLastVerticalPixel = nHeight - 1;
            zGraphics.DrawRectangle(Pens.Black, nX, nY, nLastHorizontalPixel, nLastVerticalPixel);
        }

        private static void DrawBorderEdges(Graphics zGraphics, int nX, int nY, int nWidth, int nHeight, int nBorderCrossSize)
        {
            var nLastHorizontalPixel = nWidth - 1;
            var nLastVerticalPixel = nHeight - 1;
            var nDesiredLineEdgeSize = nBorderCrossSize;

            // UL
            zGraphics.DrawLine(Pens.Black, nX, nY, Math.Min(nDesiredLineEdgeSize, nLastHorizontalPixel), nY);
            zGraphics.DrawLine(Pens.Black, nX, nY, nX, Math.Min(nDesiredLineEdgeSize, nLastVerticalPixel));

            // LL
            zGraphics.DrawLine(Pens.Black, nX, nY + nLastVerticalPixel,
                Math.Min(nDesiredLineEdgeSize, nLastHorizontalPixel), nY + nLastVerticalPixel);
            zGraphics.DrawLine(Pens.Black, nX, nY + nLastVerticalPixel, nX,
                nY + nLastVerticalPixel - Math.Min(nDesiredLineEdgeSize, nLastVerticalPixel));

            // UR
            zGraphics.DrawLine(Pens.Black, nX + nLastHorizontalPixel, nY,
                nX + nLastHorizontalPixel - Math.Min(nDesiredLineEdgeSize, nLastVerticalPixel), nY);
            zGraphics.DrawLine(Pens.Black, nX + nLastHorizontalPixel, nY, nX + nLastHorizontalPixel,
                nY + Math.Min(nDesiredLineEdgeSize, nLastVerticalPixel));

            // LR
            zGraphics.DrawLine(Pens.Black, nX + nLastHorizontalPixel, nY + nLastVerticalPixel,
                nX + nLastHorizontalPixel,
                nY + nLastVerticalPixel - Math.Min(nDesiredLineEdgeSize, nLastVerticalPixel));
            zGraphics.DrawLine(Pens.Black, nX + nLastHorizontalPixel, nY + nLastVerticalPixel,
                nX + nLastHorizontalPixel - Math.Min(nDesiredLineEdgeSize, nLastHorizontalPixel),
                nY + nLastVerticalPixel);
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
            foreach (var zRenderProcessor in s_listElementRenderProcessors)
            {
                sInput = zRenderProcessor.Render(zGraphics, zElement, zDeck, sInput, nX, nY, bExport);
            }

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
            // draw the element guides
            if (CardMakerInstance.DrawElementBorder || CardMakerInstance.AlwaysDrawSelectionBorder)
            {
                // note that the border is inclusive in the width/height consuming 2 pixels (0 to total-1)
                if (bSelected && CardMakerInstance.DrawSelectedElementGuides)
                {
                    zGraphics.DrawLine(s_zPenDebugGuides, new PointF(0, zElement.y),
                        new PointF(zGraphics.ClipBounds.Width, zElement.y));
                    zGraphics.DrawLine(s_zPenDebugGuides, new PointF(0, zElement.y + zElement.height - 1),
                        new PointF(zGraphics.ClipBounds.Width, zElement.y + zElement.height));
                    zGraphics.DrawLine(s_zPenDebugGuides, new PointF(zElement.x, 0),
                        new PointF(zElement.x, zGraphics.ClipBounds.Height));
                    zGraphics.DrawLine(s_zPenDebugGuides, new PointF(zElement.x + zElement.width - 1, 0),
                        new PointF(zElement.x + zElement.width, zGraphics.ClipBounds.Height));
                }
            }

            if (CardMakerInstance.DrawElementBorder)
            {
                zGraphics.DrawRectangle(s_zPenDebugBorder, zElement.x, zElement.y, zElement.width - 1,
                    zElement.height - 1);
            }

            // draw the selection border
            if ((CardMakerInstance.DrawElementBorder || CardMakerInstance.AlwaysDrawSelectionBorder) && bSelected)
            {
                zGraphics.DrawRectangle(m_zPenElementSelect, zElement.x - 2, zElement.y - 2, zElement.width + 3, zElement.height + 3);
            }
        }

        /// <summary>
        /// Gets a new brush based on the passed in element and color.
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Brush GetElementOpacityBrush(ProjectLayoutElement zElement, Color color)
        {
            return new SolidBrush(GetElementOpacityColor(zElement, color));
        }

        /// <summary>
        /// Gets a new pen based on the passed in element, color, and thickness.
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="color"></param>
        /// <param name="nThickness"></param>
        /// <returns></returns>
        public static Pen GetElementOpacityPen(ProjectLayoutElement zElement, Color color, int nThickness)
        {
            return new Pen(GetElementOpacityColor(zElement, color), nThickness);
        }

        /// <summary>
        /// Gets the color based on the element opacity
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color GetElementOpacityColor(ProjectLayoutElement zElement, Color color)
        {
            return 255 != zElement.opacity
                ? Color.FromArgb(zElement.opacity, color)
                : color;
        }

        /// <summary>
        /// Renders the element path and outline based on the passed in parameters. If the opacity of the element is 255 everything will be rendered using direct graphics calls.
        /// If the element opacity is not 255 the outline and fill must be rendered as regions. This will appear to be somewhat pixelated in comparison to the other mode.
        /// This is necessary due to the need for clipping due to opacity. Without using regions the clipping results in 'space' between the shape and outline.
        /// </summary>
        /// <param name="zElement">The element to use the thickness value of</param>
        /// <param name="zGraphics">The graphics to render to</param>
        /// <param name="zPath">The path to draw the shape and outline on</param>
        /// <param name="zFillBrush">The optional brush to fill the path with</param>
        public static void DrawElementPath(ProjectLayoutElement zElement, Graphics zGraphics, GraphicsPath zPath, Brush zFillBrush)
        {
            // draw the outline
            if (0 < zElement.outlinethickness)
            {
                var outlinePen = GetElementOpacityPen(zElement, zElement.GetElementOutlineColor(), zElement.outlinethickness);
                outlinePen.LineJoin = LineJoin.Round;

                if (zElement.opacity < 255)
                {
                    var zPriorClip = zGraphics.Clip;
                    zGraphics.SetClip(new Region(zPath), CombineMode.Exclude);
                    zGraphics.DrawPath(outlinePen, zPath);
                    zGraphics.Clip = zPriorClip;
                }
                else
                {
                    zGraphics.DrawPath(outlinePen, zPath);
                }
            }

            // no brush nothing else to draw
            if (zFillBrush == null)
            {
                return;
            }

            // draw the actual content (text/shapes)
            if (zElement.opacity < 255)
            {
                zGraphics.FillRegion(zFillBrush, new Region(zPath));
            }
            else
            {
                zGraphics.FillPath(zFillBrush, zPath);
            }
        }
    }
}
