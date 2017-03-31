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

using System.Collections.Generic;
using System.Drawing;
using CardMaker.Data;
using CardMaker.XML;
using System.Globalization;

namespace CardMaker.Card.FormattedText.Markup
{
    public class ImageMarkup : MarkupValueBase
    {
        private string m_sImageFile;
        private float m_fXOffset;
        private float m_fYOffset;

        public override bool Aligns => true;

        public ImageMarkup(string sVariable) : base(sVariable){}

        public override bool ProcessMarkup(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            var arrayComponents = m_sVariable.Split(new char[] { ';' });
            if (1 > arrayComponents.Length)
            {
                return false;
            }
            LineNumber = zProcessData.CurrentLine;

            m_sImageFile = arrayComponents[0];

            var zBmp = DrawItem.LoadImageFromCache(m_sImageFile);

            if (null == zBmp)
            {
                return false;
            }

            var fLineHeightPercent = -1f;

            // SOOOOO much duplication
            switch (arrayComponents.Length)
            {
                case 1:
                    TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, zBmp.Width, zBmp.Height);
                    break;
                case 2:
                    float.TryParse(arrayComponents[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out fLineHeightPercent);
                    TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, zBmp.Width, zBmp.Height);
                    break;
                case 3:
                    if (float.TryParse(arrayComponents[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out m_fXOffset) &&
                        float.TryParse(arrayComponents[2].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out m_fYOffset))
                    {
                        TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, zBmp.Width, zBmp.Height);
                    }
                    break;
                case 4:
                    float.TryParse(arrayComponents[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out fLineHeightPercent);
                    if (float.TryParse(arrayComponents[2], out m_fXOffset) &&
                        float.TryParse(arrayComponents[3], out m_fYOffset))
                    {
                        TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, zBmp.Width, zBmp.Height);
                    }
                    break;
                case 5: // width and height are absolute (even overlapping drawing)
                    {
                        int nWidth;
                        int nHeight;
                        if (float.TryParse(arrayComponents[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out m_fXOffset) &&
                            float.TryParse(arrayComponents[2].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out m_fYOffset) &&
                            int.TryParse(arrayComponents[3], out nWidth) &&
                            int.TryParse(arrayComponents[4], out nHeight))
                        {
                            TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, nWidth, nHeight);
                        }
                    }
                    break;
            }

            if (TargetRect == RectangleF.Empty)
            {
                return false;
            }

            m_fXOffset += zProcessData.CurrentXOffset;
            m_fYOffset += zProcessData.CurrentYOffset;

            if (-1f != fLineHeightPercent)
            {
                var aspectRatio = TargetRect.Width / TargetRect.Height;
                var fNewHeight = fLineHeightPercent * (zProcessData.FontHeight == 0f ? 1f : (float)zProcessData.FontHeight);
                var fNewWidth = fNewHeight * aspectRatio;
                TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, fNewWidth, fNewHeight);
            }

            // newline eval
            if (TargetRect.Width + TargetRect.X > zElement.width)
            {
                zProcessData.MoveToNextLine(zElement);
                TargetRect = new RectangleF(0, zProcessData.CurrentY, TargetRect.Width, TargetRect.Height);
            }

            // cap off excessively wide images
            if (TargetRect.Width + TargetRect.X > zElement.width)
            {
                TargetRect = new RectangleF(TargetRect.X, TargetRect.Y, zElement.width, TargetRect.Height);
            }

            // Center the image on the line based on font height or line height (todo figure out which...)
            TargetRect = new RectangleF(TargetRect.X,
                TargetRect.Y + (((float)zProcessData.FontHeight - (float)TargetRect.Height) / 2f), TargetRect.Width, TargetRect.Height); 

            zProcessData.CurrentX += TargetRect.Width;

            return true;
        }

        public override bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            return true;
        }

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            if (zElement.height < (TargetRect.Y + TargetRect.Height))
            {
                // too tall, completely end the draw
                return false;
            }

            // draw border (debugging)
            if (CardMakerInstance.DrawFormattedTextBorder)
            {
                zGraphics.DrawRectangle(Pens.Green, TargetRect.X + m_fXOffset, TargetRect.Y + m_fYOffset, TargetRect.Width, TargetRect.Height);
            }

            // already null checked in the ProcessMarkup
            var zBmp = DrawItem.LoadImageFromCache(m_sImageFile);
            zGraphics.DrawImage(zBmp, TargetRect.X + m_fXOffset, TargetRect.Y + m_fYOffset, TargetRect.Width, TargetRect.Height);

            if (CardMakerInstance.DrawFormattedTextBorder)
            {
                zGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 255, 0, 0)), TargetRect.X, TargetRect.Y, TargetRect.Width, TargetRect.Height);
            }

            return true;
        }
    }
}
