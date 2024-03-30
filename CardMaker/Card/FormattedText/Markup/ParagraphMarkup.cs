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
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public class ParagraphMarkup : MarkupValueBase
    {
        public ParagraphMarkup(string sVariable) : base(sVariable) { }

        private ProjectLayoutElement m_zElement;

        protected override bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            if (zProcessData.InParagraph)
            {
                return false;
            }
            var arrayParams = m_sVariable.Split(PARAM_SPLITTER, System.StringSplitOptions.RemoveEmptyEntries);
            if (arrayParams.Length < 2)
            {
                return false;
            }

            m_zElement = zElement;

            if (!int.TryParse(arrayParams[0], out var nParagraphStartIndent)
                || !int.TryParse(arrayParams[1], out var nParagraphLineIndent))
            {
                nParagraphStartIndent = 4;
                nParagraphLineIndent = 0;
            }

            zProcessData.InParagraph = true;

            zProcessData.ParagraphStartCharacterIndent = nParagraphStartIndent;
            zProcessData.ParagraphCharacterLineIndent = nParagraphLineIndent;
            zProcessData.ParagraphNextLineIndent = nParagraphStartIndent;
            zProcessData.MoveToNextLine(zElement, false);
            return false;
        }

        public override void CloseMarkup(FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            zProcessData.InParagraph = false;
            zProcessData.ParagraphStartCharacterIndent = 0;
            zProcessData.ParagraphCharacterLineIndent = 0;
            if (m_zElement != null)
            {
                zProcessData.MoveToNextLine(m_zElement);
            }
            
        }
    }
}