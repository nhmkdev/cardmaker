using System;
using System.Drawing;

namespace Support.IO
{
    public static class ColorSerialization
    {
        private const string COLOR_HEX_STR = "0x";

        public static Color TranslateColorString(string sColor, int defaultAlpha, out bool bSucceeded)
        {
            if (string.IsNullOrEmpty(sColor))
            {
                bSucceeded = false;
                return Color.Black;
            }

            sColor = sColor.Trim();
            if (sColor.StartsWith(COLOR_HEX_STR, StringComparison.CurrentCultureIgnoreCase))
            {
                sColor = sColor.Remove(0, 2);
            }
            else
            {
                Color colorByName = Color.FromName(sColor);
                // no named color will be set this way
                if (colorByName.A != 0)
                {
                    bSucceeded = true;
                    return colorByName;
                }
            }

            var colorResult = Color.Black;
            try
            {
                switch (sColor.Length)
                {
                    case 6: //0xRGB (hex RGB)
                        colorResult = Color.FromArgb(
                            defaultAlpha,
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                        break;
                    case 8: //0xRGBA (hex RGBA)
                        colorResult = Color.FromArgb(
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                        break;
                    case 9: //RGB (int RGB)
                        colorResult = Color.FromArgb(
                            defaultAlpha,
                            Math.Min(255, Int32.Parse(sColor.Substring(0, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(3, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(6, 3))));
                        break;
                    case 12: //RGBA (int RGBA)
                        colorResult = Color.FromArgb(
                            Math.Min(255, Int32.Parse(sColor.Substring(9, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(0, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(3, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(6, 3))));
                        break;
                    default:
                        bSucceeded = false;
                        return colorResult;
                }
                bSucceeded = true;
                return colorResult;

            }
            catch (Exception)
            {
                Logger.AddLogLine("Unsupported color string found: " + sColor);
                bSucceeded = false;
            }
            return colorResult;
        }

        /// <summary>
        /// Converts a color to the a color formatted string for serialization across the app (0x hex form)
        /// </summary>
        /// <param name="zColor"></param>
        /// <returns></returns>
        public static string GetElementColorString(Color zColor)
        {
            return COLOR_HEX_STR +
                zColor.R.ToString("X").PadLeft(2, '0') +
                zColor.G.ToString("X").PadLeft(2, '0') +
                zColor.B.ToString("X").PadLeft(2, '0') +
                zColor.A.ToString("X").PadLeft(2, '0');
        }
    }
}
