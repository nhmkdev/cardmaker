﻿////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2016 Tim Stair
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
using CardMaker.Card.FormattedText.Markup;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Alignment
{
    public abstract class VerticalAlignmentProcessor
    {
        public virtual float GetVerticalAlignOffset(ProjectLayoutElement zElement, List<MarkupBase> listMarkups)
        {
            return 0f;
        }

        protected float GetLargestMarkupHeight(ProjectLayoutElement zElement, List<MarkupBase> listMarkups)
        {
            if (0 == listMarkups.Count)
            {
                return 0;
            }

            var zLastMarkup = listMarkups[listMarkups.Count - 1];
            var nLineNumber = zLastMarkup.LineNumber;
            // find the largest total height on the last row (inclusive of the Y position)
            var fLargestTotal = zLastMarkup.TargetRect.Y + zLastMarkup.TargetRect.Height;
            var nIdx = listMarkups.Count - 2;
            while (nIdx > -1)
            {
                var zMarkup = listMarkups[nIdx];
                if (nLineNumber == zMarkup.LineNumber)
                {
                    fLargestTotal = Math.Max(fLargestTotal, zMarkup.TargetRect.Y + zMarkup.TargetRect.Height);
                }
                else
                {
                    break;
                }
                nIdx--;
            }
            return fLargestTotal;
        }
    }
}
