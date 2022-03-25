////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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

namespace CardMaker.Card.FormattedText.Markup
{
    public class FontMarkup : MarkupValueBase
    {
        public FontMarkup(string sVariable) : base(sVariable) { }

        private Font m_zPreviousFont;

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            // TODO: adjust the TranslateFontString method (or add a sibling) to not double up on the string split
            var arrayComponents = m_sVariable.Split(CardMakerConstants.FORMATTED_TEXT_PARAM_SEPARATOR_ARRAY, StringSplitOptions.None);
            if (1 > arrayComponents.Length)
            {
                return false;
            }

            var sNewFont = m_sVariable;

            if (arrayComponents.Length == 1)
            {
                sNewFont = string.Join(CardMakerConstants.FORMATTED_TEXT_PARAM_SEPARATOR, new string[]
                {
                    arrayComponents[0],
                    zProcessData.Font.Size.ToString(),
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Bold),
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Underline),
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Italic),
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Strikeout)
                });
            }
            else if (arrayComponents.Length == 2)
            {
                sNewFont = string.Join(CardMakerConstants.FORMATTED_TEXT_PARAM_SEPARATOR, new string[]
                {
                    arrayComponents[0],
                    string.IsNullOrWhiteSpace(arrayComponents[1]) ? zProcessData.Font.Size.ToString(): arrayComponents[1],
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Bold),
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Underline),
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Italic),
                    ProjectLayoutElement.BoolToNumericString(zProcessData.Font.Strikeout)
                });
            }
            else if (arrayComponents.Length == 6)
            {
                sNewFont = string.Join(CardMakerConstants.FORMATTED_TEXT_PARAM_SEPARATOR, new string[]
                {
                    arrayComponents[0],
                    string.IsNullOrWhiteSpace(arrayComponents[1]) ? zProcessData.Font.Size.ToString(): arrayComponents[1],
                    arrayComponents[2],
                    arrayComponents[3],
                    arrayComponents[4],
                    arrayComponents[5]
                });
            }
            else
            {
                sNewFont = m_sVariable;
            }

            var zNewFont = ProjectLayoutElement.TranslateFontString(sNewFont);
            if (zNewFont != null)
            {
                m_zPreviousFont = zProcessData.Font;
                zProcessData.SetFont(zNewFont, zGraphics);
                return true;
            }
            return false;
        }

        public override void CloseMarkup(FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            if (null != m_zPreviousFont)
            {
                zProcessData.SetFont(m_zPreviousFont, zGraphics);
            }
        }
    }
}
