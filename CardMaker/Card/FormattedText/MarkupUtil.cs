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
            {"fc", typeof (FontColorMarkup)},
            {"xo", typeof (XDrawOffsetMarkup)},
            {"yo", typeof (YDrawOffsetMarkup)},
            {"br", typeof (NewlineMarkup)},
            {"bgc", typeof (BackgroundColorMarkup)},
            {"bgi", typeof (BackgroundImageMarkup)},
            {"spc", typeof (SpaceMarkup)},
            {"push", typeof (PushMarkup)},
            {"px", typeof (PixelMarkup)},
            {"img", typeof (ImageMarkup)},
            {"ls", typeof(LineSpaceMarkup) },
            {"ac", typeof(AlignCenterMarkup) },
            {"ar", typeof(AlignRightMarkup) },
            {"al", typeof(AlignLeftMarkup) },
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
