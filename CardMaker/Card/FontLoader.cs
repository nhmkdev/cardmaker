using System;
using System.Drawing;
using Support.IO;
using Support.UI;

namespace CardMaker.Card
{
    public static class FontLoader
    {
        public static Font DefaultFont => new Font("Arial", 12);

        public static Font GetFont(FontFamily zFontFamily, float fSize, FontStyle eFontStyle)
        {
#if true
            return GetFont(zFontFamily.Name, fSize, eFontStyle);
#else
            try
            {
                return new Font(zFontFamily, fSize, eFontStyle);
            }
            catch (Exception)
            {
                Logger.AddLogLine("Font load failed: {0} {1} {2} (attempting to find valid style)".FormatString(zFontFamily.Name, fSize, eFontStyle));
            }
            foreach (var eStyle in Enum.GetValues(typeof(FontStyle)))
            {
                try
                {
                    var zFont = new Font(zFontFamily, fSize, (FontStyle)eStyle);
                    Logger.AddLogLine("Font load succeded: {0} {1} {2} (attempting to find valid style)".FormatString(zFontFamily.Name, fSize, eStyle));
                    return zFont;
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Font load failed: {0} {1} {2} (attempting to find valid style)".FormatString(zFontFamily.Name, fSize, eStyle));
                }
            }
            return DefaultFont;
#endif
        }

        public static Font GetFont(string sFontName, float fSize, FontStyle eFontStyle)
        {
            // sFontName IS NOT a family name, this is different somehow
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
                try
                {
                    var zFont = new Font(sFontName, fSize, (FontStyle)eStyle);
                    Logger.AddLogLine("Font load succeded: {0} {1} {2} (attempting to find valid style)".FormatString(sFontName, fSize, eStyle));
                    return zFont;
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Font load failed: {0} {1} {2} (attempting to find valid style)".FormatString(sFontName, fSize, eStyle));
                }
            }
            return DefaultFont;
        }
    }
}
