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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using CardMaker.Data;
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.FormattedText.Markup
{
    public class TextMarkup : MarkupValueBase
    {
        private Brush m_zFontBrush;
        private Font m_zFont;
        public float m_fFontHeight;
        public float m_fFontOutlineSize;
        private float m_fXOffset;
        private float m_fYOffset;

        private RectangleF m_rectMeasuredRectangle = RectangleF.Empty;

        public override bool Aligns => true;

        public TextMarkup(string sVariable)
            : base(sVariable)
        {
        }

        public override bool ProcessMarkup(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            m_zFontBrush = zProcessData.FontBrush;
            m_zFont = zProcessData.Font;
            m_fFontHeight = zProcessData.FontHeight;
            m_fXOffset = zProcessData.CurrentXOffset;
            m_fYOffset = zProcessData.CurrentYOffset;
            StringAlignment = zProcessData.CurrentStringAlignment;

            LineNumber = zProcessData.CurrentLine;

            m_fFontOutlineSize = m_zFont.Size;

            // TODO: stop recalculating this, store it in the processData
            if (0 != zElement.outlinethickness)
            {
                switch (m_zFont.Unit)
                {
                    case GraphicsUnit.Point:
                        m_fFontOutlineSize = zGraphics.DpiY * (m_zFont.Size / 72f);
                        break;
                    default:
                        Logger.AddLogLine("This font is using the Unit: {0} (not currently supported)".FormatString(m_zFont.Unit.ToString()));
                        break;
                }
            }

            m_rectMeasuredRectangle = MeasureDisplayStringWidth(zGraphics, m_sVariable, m_zFont);

            var fMeasuredWidth = m_rectMeasuredRectangle.Width;

            var fMeasuredHeight = Math.Max(m_rectMeasuredRectangle.Height, m_fFontHeight);

            if (zProcessData.CurrentX + fMeasuredWidth > zElement.width)
            {
                zProcessData.MoveToNextLine(zElement);
            }

            TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, fMeasuredWidth, fMeasuredHeight);

            zProcessData.CurrentX += fMeasuredWidth;            

            return true;
        }

        public override bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            return true;
        }

        public static RectangleF MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            var zFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
            };
            var rect = new RectangleF(0, 0, 65536, 65536);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };
            var regions = new Region[1];

            zFormat.SetMeasurableCharacterRanges(ranges);

            regions = graphics.MeasureCharacterRanges(text, font, rect, zFormat);
            rect = regions[0].GetBounds(graphics);

            return rect;
        }

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            var zFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
            };
            // indicate any text being cut off
            if (zElement.height < TargetRect.Y + TargetRect.Height)
            {
                // completely end the draw
                return false;
            }

            // NOTE: when rendering there is no need for a target rect as that information has already been processed
            float targetX = TargetRect.X + m_fXOffset;
            float targetY = TargetRect.Y + m_fYOffset;

            // draw border (debugging)
            if (CardMakerInstance.DrawFormattedTextBorder)
            {
                zGraphics.DrawRectangle(Pens.Green, targetX, targetY, TargetRect.Width, TargetRect.Height);
            }

            // when a string is measured there is a bit of an offset to where it renders (into the target rect a few pixels right ---->)
            targetX -= m_rectMeasuredRectangle.X;

            if (0 == zElement.outlinethickness)
            {
                try
                {
                    zGraphics.DrawString(m_sVariable, m_zFont, m_zFontBrush, targetX, targetY, zFormat);
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Unable to render text (font issue?)");
                }
            }
            else
            {
                // prepare to draw text
                var zPath = new GraphicsPath();

                try
                {
                    zPath.AddString(m_sVariable, m_zFont.FontFamily, (int) m_zFont.Style, m_fFontOutlineSize,
                        new PointF(targetX, targetY), zFormat);

                    CardRenderer.DrawPathOutline(zElement, zGraphics, zPath);
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Unable to render text (font issue?)");
                }
                // fill in the outline
                zGraphics.FillPath(m_zFontBrush, zPath);
            }
            return true;
        }
    }
}