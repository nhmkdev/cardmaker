////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
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
using System.Drawing;
using System.Reflection;
using System.Xml.Serialization;
using CardMaker.Card;
using CardMaker.Data;
using Support.IO;
using Support.Util;

namespace CardMaker.XML
{
    public class ProjectLayoutElement
    {
        private const string COLOR_HEX_STR = "0x";
        private static readonly List<PropertyInfo> s_listPropertyInfos;

        #region Properties

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public int x { get; set; }

        [XmlAttribute]
        public int y { get; set; }

        [XmlAttribute]
        public int width { get; set; }

        [XmlAttribute]
        public int height { get; set; }

        [XmlAttribute]
        public string variable { get; set; }

        [XmlAttribute]
        public string font { get; set; }

        [XmlAttribute]
        public string elementcolor { get; set; }

        [XmlAttribute]
        public string bordercolor { get; set; }

        [XmlAttribute]
        public string backgroundcolor { get; set; }

        [XmlAttribute]
        public int borderthickness { get; set; }

        [XmlAttribute]
        public float rotation { get; set; }

        [XmlAttribute]
        public int horizontalalign { get; set; }

        [XmlAttribute]
        public int verticalalign { get; set; }

        [XmlAttribute]
        public int lineheight { get; set; }

        [XmlAttribute]
        public int wordspace { get; set; }

        [XmlAttribute]
        public int opacity { get; set; }

        [XmlAttribute]
        public bool enabled { get; set; }

        [XmlAttribute]
        public string outlinecolor { get; set; }

        [XmlAttribute]
        public int outlinethickness { get; set; }

        [XmlAttribute]
        public bool justifiedtext { get; set; }

        [XmlAttribute]
        public bool autoscalefont { get; set; }

        [XmlAttribute]
        public bool lockaspect { get; set; }

        [XmlAttribute]
        public bool keeporiginalsize { get; set; }

        [XmlAttribute]
        public string tilesize { get; set; }

        #endregion

        private Color m_colorElement = Color.Black;
        private Color m_colorOutline = Color.Black;
        private Color m_colorBorder = Color.Black;
        private Color m_colorBackground = CardMakerConstants.NoColor;
        private Font m_fontText;

        public override string ToString()
        {
            return name;
        }

        public ProjectLayoutElement()
        {
        }

        public ProjectLayoutElement(string sName)
        {
            // actual values
            verticalalign = (int)StringAlignment.Near;
            horizontalalign = (int)StringAlignment.Near;
            x = 0;
            y = 0;
            width = 40;
            height = 40;
            borderthickness = 0;
            outlinethickness = 0;
            outlinecolor = "0x000000000";
            rotation = 0;
            bordercolor = "0x000000000";
            font = string.Empty;
            elementcolor = "0x000000000";
            backgroundcolor = "0x00000000";
            type = ElementType.Text.ToString();
            lineheight = 0;
            wordspace = 0;
            autoscalefont = false;
            lockaspect = false;
            tilesize = string.Empty;
            keeporiginalsize = false;
            variable = string.Empty;
            name = sName;
            opacity = 255;
            enabled = true;

            InitializeTranslatedFields();
        }

        public Color GetElementBorderColor()
        {
            return m_colorBorder;
        }

        public Color GetElementColor()
        {
            return m_colorElement;
        }

        public Color GetElementOutlineColor()
        {
            return m_colorOutline;
        }

        public Color GetElementBackgroundColor()
        {
            return m_colorBackground;
        }

        public Font GetElementFont()
        {
            return m_fontText;
        }

        public StringAlignment GetHorizontalAlignment()
        {
            return (StringAlignment) horizontalalign;
        }

        public StringAlignment GetVerticalAlignment()
        {
            return (StringAlignment)verticalalign;
        }

        /// <summary>
        /// Initializes the cache variables for color and font translation
        /// </summary>
        public void InitializeTranslatedFields()
        {
            SetElementBorderColor(TranslateColorString(bordercolor));
            SetElementColor(TranslateColorString(elementcolor));
            SetElementOutlineColor(TranslateColorString(outlinecolor));
            SetElementBackgroundColor(backgroundcolor == null ? Color.FromArgb(0,0,0,0) : TranslateColorString(backgroundcolor));
            m_fontText = !string.IsNullOrEmpty(font)
                ? TranslateFontString(font)
                : null;
        }

        /// <summary>
        /// Performs a partial deepy copy based on the input element, the name field is left unchanged
        /// </summary>
        /// <param name="zElement">The source element to copy from</param>
        /// <param name="bInitializeTranslatedFields">flag indicating whether to reinitialize the translated fields</param>
        public void DeepCopy(ProjectLayoutElement zElement, bool bInitializeTranslatedFields)
        {
            verticalalign = zElement.verticalalign;
            horizontalalign = zElement.horizontalalign;
            x = zElement.x;
            y = zElement.y;
            width = zElement.width;
            height = zElement.height;
            borderthickness = zElement.borderthickness;
            outlinethickness = zElement.outlinethickness;
            outlinecolor = zElement.outlinecolor;
            rotation = zElement.rotation;
            bordercolor = zElement.bordercolor;
            font = zElement.font;
            elementcolor = zElement.elementcolor;
            backgroundcolor = zElement.backgroundcolor;
            type = zElement.type;
            autoscalefont = zElement.autoscalefont;
            lockaspect = zElement.lockaspect;
            keeporiginalsize = zElement.keeporiginalsize;
            variable = zElement.variable;
            opacity = zElement.opacity;
            enabled = zElement.enabled;
            lineheight = zElement.lineheight;
            wordspace = zElement.wordspace;
            tilesize = zElement.tilesize;

            if (bInitializeTranslatedFields)
            {
                InitializeTranslatedFields();
            }
        }

        /// <summary>
        /// Translates a color string
        /// </summary>
        /// <param name="sColor">The color string to translate</param>
        /// <returns>The color, or Color.Black by default</returns>
        public static Color TranslateColorString(string sColor, int defaultAlpha = 255)
        {
            if (string.IsNullOrEmpty(sColor))
            {
                return Color.Black;
            }

            sColor = sColor.Trim();
            if (sColor.StartsWith(COLOR_HEX_STR, StringComparison.CurrentCultureIgnoreCase))
            {
                sColor = sColor.Remove(0, 2);
            }

            Color colorByName = Color.FromName(sColor);
            // no named color will be set this way
            if (colorByName.A != 0)
            {
                return colorByName;
            }

            try
            {
                switch (sColor.Length)
                {
                    case 6: //0xRGB (hex RGB)
                        return Color.FromArgb(
                            defaultAlpha,
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                    case 8: //0xRGBA (hex RGBA)
                        return Color.FromArgb(
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)),
                            Math.Min(255,
                                Int32.Parse(sColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                    case 9: //RGB (int RGB)
                        return Color.FromArgb(
                            defaultAlpha,
                            Math.Min(255, Int32.Parse(sColor.Substring(0, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(3, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(6, 3))));
                    case 12: //RGBA (int RGBA)
                        return Color.FromArgb(
                            Math.Min(255, Int32.Parse(sColor.Substring(9, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(0, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(3, 3))),
                            Math.Min(255, Int32.Parse(sColor.Substring(6, 3))));
                }
            }
            catch (Exception)
            {
                Logger.AddLogLine("Unsupported color string found: " + sColor);
            }
            return Color.Black;
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

        /// <summary>
        /// Translates a font string to a font
        /// </summary>
        /// <param name="fontString">The font string to translate</param>
        /// <returns>The font, otherwise null on error</returns>
        public static Font TranslateFontString(string fontString)
        {
            var arraySplit = fontString.Split(new char[] { ';' });
            float fFontSize;
            if (6 != arraySplit.Length || !ParseUtil.ParseFloat(arraySplit[1], out fFontSize))
            {
                return null;
            }
            return FontLoader.GetFont(
                arraySplit[0], 
                fFontSize,
                (arraySplit[2].Equals("1") ? FontStyle.Bold : FontStyle.Regular) |
                (arraySplit[3].Equals("1") ? FontStyle.Underline : FontStyle.Regular) |
                (arraySplit[4].Equals("1") ? FontStyle.Italic : FontStyle.Regular) |
                (arraySplit[5].Equals("1") ? FontStyle.Strikeout : FontStyle.Regular));
        }

        /// <summary>
        /// Sets the border color and color string
        /// </summary>
        /// <param name="zColor">The color to pull the values from</param>
        public void SetElementBorderColor(Color zColor)
        {
            bordercolor = GetElementColorString(zColor);
            m_colorBorder = zColor;
        }

        /// <summary>
        /// Sets the element color and color string
        /// </summary>
        /// <param name="zColor">The color to pull the values from</param>
        public void SetElementColor(Color zColor)
        {
            elementcolor = GetElementColorString(zColor);
            m_colorElement = zColor;
        }

        /// <summary>
        /// Sets the outline color and color string
        /// </summary>
        /// <param name="zColor">The color to pull the values from</param>
        public void SetElementOutlineColor(Color zColor)
        {
            outlinecolor = GetElementColorString(zColor);
            m_colorOutline = zColor;
        }

        /// <summary>
        /// Sets the outline color and color string
        /// </summary>
        /// <param name="zColor">The color to pull the values from</param>
        public void SetElementBackgroundColor(Color zColor)
        {
            backgroundcolor = GetElementColorString(zColor);
            m_colorBackground = zColor;
        }

        /// <summary>
        /// Sets the font and font string
        /// </summary>
        /// <param name="zFont">The font to pull the settings from</param>
        public void SetElementFont(Font zFont)
        {
            font = zFont.Name + ";" + zFont.Size + ";" +
                   (zFont.Bold ? "1" : "0") + ";" +
                   (zFont.Underline ? "1" : "0") + ";" +
                   (zFont.Italic ? "1" : "0") + ";" +
                   (zFont.Strikeout ? "1" : "0");
            m_fontText = zFont;
        }

        /// <summary>
        /// Gets the set of PropertyInfo objects associated with this type (sorted by name)
        /// </summary>
        public static PropertyInfo[] SortedPropertyInfos => s_listPropertyInfos.ToArray();

        static ProjectLayoutElement()
        {
            s_listPropertyInfos = new List<PropertyInfo>(typeof(ProjectLayoutElement).GetProperties(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));
            s_listPropertyInfos.Sort((zPropA, zPropB) => zPropA.Name.CompareTo(zPropB.Name));
        }

    }
}