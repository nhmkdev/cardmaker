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

using System;
using System.Drawing;
using Support.IO;
using Support.UI;

namespace CardMaker.Card
{
    public static class FontLoader
    {
        public static readonly Font DefaultFont = new Font("Arial", 12);

        /// <summary>
        /// Gets the font based on the input font family. This attempts to verify the font style is valid.
        /// </summary>
        /// <param name="zFontFamily">The font family to attempt to load</param>
        /// <param name="fSize">The size to apply</param>
        /// <param name="eFontStyle">The style to apply</param>
        /// <returns>The font with the style applied. Will default the font style to a valid one if an invalid one is passed in. Ultimately will default
        /// to the application default if all fails.</returns>
        public static Font GetFont(FontFamily zFontFamily, float fSize, FontStyle eFontStyle)
        {
            if (zFontFamily.IsStyleAvailable(eFontStyle))
            {
                try
                {
                    return new Font(zFontFamily.Name, fSize, eFontStyle);
                }
                catch (Exception)
                {
                    Logger.AddLogLine("FontFamily load failed: {0} {1} {2} (attempting to find valid style)".FormatString(zFontFamily.Name, fSize, eFontStyle));
                }
            }
            else
            {
                Logger.AddLogLine("FontFamily load failed: {0} {1} {2} (attempting to find valid style)".FormatString(zFontFamily.Name, fSize, eFontStyle));
            }

            foreach (var eStyle in Enum.GetValues(typeof(FontStyle)))
            {
                var eAltFontStyle = (FontStyle) eStyle;
                // skip the original style
                if(eAltFontStyle == eFontStyle) continue;
                if (zFontFamily.IsStyleAvailable(eAltFontStyle))
                {
                    try
                    {
                        return new Font(zFontFamily.Name, fSize, eAltFontStyle);
                    }
                    catch (Exception)
                    {
                        Logger.AddLogLine("FontFamily load failed: {0} {1} {2} (attempting to find valid style)".FormatString(zFontFamily.Name, fSize, eAltFontStyle));
                    }
                }
            }
            Logger.AddLogLine("FontFamily load failed: {0} {1} (defaulting due to no valid style)".FormatString(zFontFamily.Name, fSize));
            return DefaultFont;
        }

        /// <summary>
        /// Gets the font based on the input font name
        /// </summary>
        /// <param name="sFontName">The font to attempt to load</param>
        /// <param name="fSize">The size to apply</param>
        /// <param name="eFontStyle">The style to apply</param>
        /// <returns>The font with the style applied. Will default the font style to a valid one if an invalid one is passed in. Ultimately will default
        /// to the application default if all fails.</returns>
        public static Font GetFont(string sFontName, float fSize, FontStyle eFontStyle)
        {
            // sFontName IS NOT a family name
            try
            {
                return new Font(sFontName, fSize, eFontStyle);
            }
            catch (Exception)
            {
                Logger.AddLogLine("Font load failed: {0} {1} {2} (attempting to find valid style)".FormatString(sFontName, fSize, eFontStyle));
            }
            foreach (var eStyle in Enum.GetValues(typeof(FontStyle)))
            {
                var eAltFontStyle = (FontStyle)eStyle;
                // skip the original style
                if (eAltFontStyle == eFontStyle) continue;
                try
                {
                    return new Font(sFontName, fSize, eAltFontStyle);
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Font load failed: {0} {1} {2} (attempting to find valid style)".FormatString(sFontName, fSize, eAltFontStyle));
                }
            }
            return DefaultFont;
        }
    }
}
