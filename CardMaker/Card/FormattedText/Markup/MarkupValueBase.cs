﻿////////////////////////////////////////////////////////////////////////////////
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

namespace CardMaker.Card.FormattedText.Markup
{
    public abstract class MarkupValueBase : MarkupBase
    {
        // TODO: more things should use this
        protected static readonly string[] PARAM_SPLITTER = new string[] { CardMakerConstants.FORMATTED_TEXT_PARAM_SEPARATOR };

        public string Variable => m_sVariable;
        protected string m_sVariable;

        protected MarkupValueBase() { }
        protected MarkupValueBase(string sVariable)
        {
            m_sVariable = sVariable;
        }

        protected void RenderDebugBackground(Graphics zGraphics, Brush zBrush, float fXOffset, float fYOffset)
        {
            // draw border (debugging)
            if (CardMakerInstance.DrawFormattedTextBorder)
            {
                zGraphics.FillRectangle(zBrush, TargetRect.X + fXOffset, TargetRect.Y + fYOffset, TargetRect.Width, TargetRect.Height);
            }
        }
    }
}