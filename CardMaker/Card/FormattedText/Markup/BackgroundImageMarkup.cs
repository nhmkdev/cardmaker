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

using System.Drawing;
using CardMaker.Data;
using CardMaker.XML;
using Support.Util;

namespace CardMaker.Card.FormattedText.Markup
{
    public class BackgroundImageMarkup : BaseImageMarkup
    {
        public BackgroundImageMarkup(string sVariable) : base(sVariable) { }

        private float m_fXOffset;
        private float m_fYOffset;
        private int m_nWidth;
        private int m_nHeight;

        public override bool Aligns => true;

        /// <summary>
        /// Processes the image markup and if the image is valid associates it with the current process data.
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="zData"></param>
        /// <param name="zProcessData"></param>
        /// <param name="zGraphics"></param>
        /// <returns>false -  The BackgroundImageMarkup.Render is called as part of a TextMarkup</returns>
        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            var arrayComponents = m_sVariable.Split(new char[] { ';' });
            if (1 > arrayComponents.Length)
            {
                return false;
            }

            LineNumber = zProcessData.CurrentLine;

            m_sImageFile = arrayComponents[0];
            m_colorImage = zProcessData.ImageColor;
            m_eColorType = zProcessData.CurrentColorType;
            m_zColorMatrix = zProcessData.CurrentColorMatrix;
            m_eMirrorType = zProcessData.CurrentMirrorType;

            var zBmp = LoadImage(zElement);

            m_fXOffset = zProcessData.CurrentXOffset;
            m_fYOffset = zProcessData.CurrentYOffset;

            if (null != zBmp)
            {
                switch (arrayComponents.Length)
                {
                    case 1:
                        m_nWidth = zBmp.Width;
                        m_nHeight = zBmp.Height;
                        TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, 0, 0);
                        return true;
                    case 5:
                        {
                            if (ParseUtil.ParseFloat(arrayComponents[1], out m_fXOffset) &&
                                ParseUtil.ParseFloat(arrayComponents[2], out m_fYOffset) &&
                                int.TryParse(arrayComponents[3], out m_nWidth) &&
                                int.TryParse(arrayComponents[4], out m_nHeight))
                            {
                                TargetRect = new RectangleF(zProcessData.CurrentX, zProcessData.CurrentY, 0, 0);
                                return true;
                            }
                        }
                        break;
                }
            }

            return false;
        }

        public override bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            var zBmp = LoadImage(zElement);

            if (null != zBmp)
            {
                zGraphics.DrawImage(zBmp, TargetRect.X + m_fXOffset, TargetRect.Y + m_fYOffset, m_nWidth, m_nHeight);
            }

            if (CardMakerInstance.DrawFormattedTextBorder)
            {
                zGraphics.FillRectangle(new SolidBrush(Color.FromArgb(32, 0, 255, 0)), TargetRect.X, TargetRect.Y, m_nWidth, m_nHeight);
            }
            return true;
        }
    }
}