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
using System.Drawing;
using CardMaker.Data;
using CardMaker.XML;
using Support.Util;

namespace CardMaker.Card.FormattedText.Markup
{
    public class FontScaleMarkup : MarkupValueBase
    {
        private const float DEFAULT_SCALE = 1f;

        public FontScaleMarkup(string sVariable) : base(sVariable)
        {
        }

        private float m_zPreviousXScale;
        private float m_zPreviousYScale;

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData,
            FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            m_zPreviousXScale = zProcessData.FontScaleX;
            m_zPreviousYScale = zProcessData.FontScaleY;

            // TODO: adjust the TranslateFontString method (or add a sibling) to not double up on the string split
            var arrayComponents = m_sVariable.Split(CardMakerConstants.FORMATTED_TEXT_PARAM_SEPARATOR_ARRAY,
                StringSplitOptions.None);
            if (2 != arrayComponents.Length)
            {
                return false;
            }

            zProcessData.FontScaleX = ConvertToScale(arrayComponents[0], zProcessData.FontScaleX);
            zProcessData.FontScaleY = ConvertToScale(arrayComponents[1], zProcessData.FontScaleY);

            return false;
        }

        private float ConvertToScale(string sScale, float fCurrent)
        {
            switch (sScale)
            {
                case "-":
                    return fCurrent;
                default:
                    return ParseUtil.ParseFloat(sScale, out var fParsed) ? fParsed : DEFAULT_SCALE;
            }
        }

        public override void CloseMarkup(FormattedTextData zData, FormattedTextProcessData zProcessData,
            Graphics zGraphics)
        {
            zProcessData.FontScaleX = m_zPreviousXScale;
            zProcessData.FontScaleY = m_zPreviousYScale;
        }
    }
}