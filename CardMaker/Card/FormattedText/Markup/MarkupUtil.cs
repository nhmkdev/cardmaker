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

namespace CardMaker.Card.FormattedText.Markup
{
    public static class MarkupUtil
    {
        private static readonly Dictionary<string, Type> s_dictionaryMarkupTypes = new Dictionary<string, Type>()
        {
            {"b", typeof (FontStyleBoldMarkup)},
            {"i", typeof (FontStyleItalicMarkup)},
            {"s", typeof (FontStyleStrikeoutMarkup)},
            {"u", typeof (FontStyleUnderlineMarkup)},
            {"f", typeof (FontMarkup)},
            {"fs", typeof (FontSizeMarkup)},
            {"fscl", typeof (FontScaleMarkup)},
            {"fc", typeof (FontColorMarkup)},
            {"xo", typeof (XDrawOffsetMarkup)},
            {"yo", typeof (YDrawOffsetMarkup)},
            {"br", typeof (NewlineMarkup)},
            {"bgc", typeof (BackgroundColorMarkup)},
            {"bgi", typeof (BackgroundImageMarkup)},
            {"ct", typeof(ColorTypeMarkup)},
            {"cm", typeof(ColorMatrixMarkup)},
            {"ic", typeof(ImageColorMarkup)},
            {"spc", typeof (SpaceMarkup)},
            {"push", typeof (PushMarkup)},
            {"px", typeof (PixelMarkup)},
            {"p", typeof(ParagraphMarkup)},
            {"img", typeof (ImageMarkup)},
            {"ls", typeof(LineSpaceMarkup) },
            {"caps", typeof(AllCapsMarkup)},
            {"ac", typeof(AlignCenterMarkup) },
            {"ar", typeof(AlignRightMarkup) },
            {"al", typeof(AlignLeftMarkup) },
            {"mh", typeof(MirrorHorizontalMarkup)},
            {"mv", typeof(MirrorVerticalMarkup)},
            {"mgnl", typeof(MarginLeftMarkup)},
            {"mgnr", typeof(MarginRightMarkup)},
            {"marginleft", typeof(MarginLeftMarkup)},
            {"marginright", typeof(MarginRightMarkup)},
        };

        public static Type GetMarkupType(string sInput)
        {
            Type zType;
            if (s_dictionaryMarkupTypes.TryGetValue(sInput.ToLower(), out zType))
            {
                return zType;
            }
            return null;
        }

    }
}
