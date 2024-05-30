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
        private float m_fFontScaleX;
        private float m_fFontScaleY;

        private RectangleF m_rectMeasuredRectangle = RectangleF.Empty;

        public override bool Aligns => true;

        public override bool Appendable => true;

        public TextMarkup(string sVariable)
            : base(sVariable)
        {
        }

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            m_sVariable = zProcessData.ForceTextCaps ? m_sVariable.ToUpper() : m_sVariable;
            m_zFontBrush = zProcessData.FontBrush;
            m_zFont = zProcessData.Font;
            m_fFontHeight = zProcessData.FontHeight;
            m_fXOffset = zProcessData.CurrentXOffset;
            m_fYOffset = zProcessData.CurrentYOffset;
            m_fFontScaleX = zProcessData.FontScaleX;
            m_fFontScaleY = zProcessData.FontScaleY;
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

            m_rectMeasuredRectangle = StringMeasure.MeasureString(
                zGraphics, m_sVariable, m_zFont, m_fFontScaleX, m_fFontScaleY, CardMakerSettings.StringMeasureMethod);

            var fMeasuredWidth = m_rectMeasuredRectangle.Width;

            var fMeasuredHeight = Math.Max(m_rectMeasuredRectangle.Height, m_fFontHeight);

            if (zProcessData.IsXPositionOutsideBounds(zProcessData.CurrentX + fMeasuredWidth))
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

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            var zFormat = StringFormat.GenericTypographic;
            if(StringMeasureMethod.CharacterRanges == CardMakerSettings.StringMeasureMethod)
            {
                zFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near,
                };
            }

            // indicate any text being cut off
            if (zElement.height < TargetRect.Y + TargetRect.Height)
            {
                // completely end the draw
                return false;
            }

            // NOTE: when rendering there is no need for a target rect as that information has already been processed
            var targetX = TargetRect.X + m_fXOffset;
            var targetY = TargetRect.Y + m_fYOffset;

            // draw border (debugging)
            if (CardMakerInstance.DrawFormattedTextBorder)
            {
                zGraphics.DrawRectangle(Pens.Green, targetX, targetY, TargetRect.Width, TargetRect.Height);
            }

            // this measure approach may have an X offset (the other method does not)
            if (StringMeasureMethod.CharacterRanges == CardMakerSettings.StringMeasureMethod)
            {
                targetX -= m_rectMeasuredRectangle.X * m_fFontScaleX;
            }

            if (0 == zElement.outlinethickness)
            {
                try
                {
                    var zOrigTransform = zGraphics.Transform;
                    zGraphics.TranslateTransform(targetX, targetY);
                    zGraphics.ScaleTransform(m_fFontScaleX, m_fFontScaleY);
                    zGraphics.DrawString(m_sVariable, m_zFont, m_zFontBrush, 0, 0, zFormat);
                    zGraphics.Transform = zOrigTransform;
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Unable to render text (font issue?)");
                }
            }
            else
            {
                try
                {
                    var zPath = new GraphicsPath();
                    zPath.AddString(m_sVariable, m_zFont.FontFamily, (int) m_zFont.Style, m_fFontOutlineSize, new PointF(0, 0), zFormat);
                    var zOrigTransform = zGraphics.Transform;
                    zGraphics.TranslateTransform(targetX, targetY);
                    zGraphics.ScaleTransform(m_fFontScaleX, m_fFontScaleY);
                    CardRenderer.DrawElementPath(zElement, zGraphics, zPath, m_zFontBrush);
                    zGraphics.Transform = zOrigTransform;
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Unable to render text (font issue?)");
                }
            }
            return true;
        }

        public override bool TryAppendMarkup(ProjectLayoutElement zElement, FormattedTextData zData,
            FormattedTextProcessData zProcessData, Graphics zGraphics, MarkupBase zMarkupToAppend)
        {
            // only append if the process data hasn't changed since this markup
            if (!zProcessData.Font.Equals(m_zFont)
                || zProcessData.FontBrush != m_zFontBrush
                || zProcessData.FontScaleX != m_fFontScaleX
                || zProcessData.FontScaleY != m_fFontScaleY
                || zProcessData.CurrentXOffset != m_fXOffset
                || zProcessData.CurrentYOffset != m_fYOffset)
            {
                return false;
            }

            var typeMarkupToappend = zMarkupToAppend.GetType();
            var sToTry = string.Empty;
            if (typeof(SpaceMarkup) == typeMarkupToappend)
            {
                sToTry = new string(' ', ((SpaceMarkup)zMarkupToAppend).Spaces);
            }
            else if (typeof(TextMarkup) == typeMarkupToappend)
            {
                sToTry = ((TextMarkup)zMarkupToAppend).Variable;
            }

            // whitespace is allowed (spaces!)
            if (string.IsNullOrEmpty(sToTry))
            {
                return false;
            }

            var rectMeasuredRectangle = StringMeasure.MeasureString(zGraphics, m_sVariable + sToTry,
                m_zFont, m_fFontScaleX, m_fFontScaleY, CardMakerSettings.StringMeasureMethod);
            var fMeasuredWidth = rectMeasuredRectangle.Width;
            var fAdditionalWidth = fMeasuredWidth - m_rectMeasuredRectangle.Width;
            if (zProcessData.IsXPositionOutsideBounds(zProcessData.CurrentX + fAdditionalWidth))
            {
                return false;
            }

            m_sVariable += sToTry;
            TargetRect = new RectangleF(TargetRect.X, TargetRect.Y, rectMeasuredRectangle.Width, TargetRect.Height);
            m_rectMeasuredRectangle = rectMeasuredRectangle;
            zProcessData.CurrentX += fAdditionalWidth;
            return true;
        }
    }
}