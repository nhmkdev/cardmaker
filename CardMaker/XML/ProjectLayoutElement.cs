////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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
using System.Xml.Serialization;
using Support.IO;

namespace CardMaker.XML
{
    public class ProjectLayoutElement
    {
        private const string COLOR_HEX_STR = "0x";

        #region Properties

        [XmlAttributeAttribute]
        public string name { get; set; }

        [XmlAttributeAttribute]
        public string type { get; set; }

        [XmlAttributeAttribute]
        public int x { get; set; }

        [XmlAttributeAttribute]
        public int y { get; set; }

        [XmlAttributeAttribute]
        public int width { get; set; }

        [XmlAttributeAttribute]
        public int height { get; set; }

        [XmlAttributeAttribute]
        public string variable { get; set; }

        [XmlAttributeAttribute]
        public string font { get; set; }

        [XmlAttributeAttribute]
        public string elementcolor { get; set; }

        [XmlAttributeAttribute]
        public string bordercolor { get; set; }

        [XmlAttributeAttribute]
        public int borderthickness { get; set; }

        [XmlAttributeAttribute]
        public float rotation { get; set; }

        [XmlAttributeAttribute]
        public int horizontalalign { get; set; }

        [XmlAttributeAttribute]
        public int verticalalign { get; set; }

        [XmlAttributeAttribute]
        public int lineheight { get; set; }

        [XmlAttributeAttribute]
        public int wordspace { get; set; }

        [XmlAttributeAttribute]
        public int opacity { get; set; }

        [XmlAttributeAttribute]
        public bool enabled { get; set; }

        [XmlAttributeAttribute]
        public string outlinecolor { get; set; }

        [XmlAttributeAttribute]
        public int outlinethickness { get; set; }

        [XmlAttributeAttribute]
        public bool autoscalefont { get; set; }

        [XmlAttributeAttribute]
        public bool lockaspect { get; set; }

        [XmlAttributeAttribute]
        public bool keeporiginalsize { get; set; }

        #endregion

        private Color m_colorElement = Color.Black;
        private Color m_colorOutline = Color.Black;
        private Color m_colorBorder = Color.Black;
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
            verticalalign = 0;
            horizontalalign = 0;
            x = 0;
            y = 0;
            width = 40;
            height = 40;
            borderthickness = 0;
            outlinethickness = 0;
            outlinecolor = "000000000";
            rotation = 0;
            bordercolor = "000000000";
            font = string.Empty;
            elementcolor = "000000000";
            type = ElementType.Text.ToString();
            lineheight = 0;
            wordspace = 0;
            autoscalefont = false;
            lockaspect = false;
            keeporiginalsize = false;
            variable = string.Empty;
            name = sName;
            opacity = 255;
            enabled = true;

            InitializeCache();
        }

        /// <summary>
        /// Initializes the cache variables for color and font translation
        /// </summary>
        public void InitializeCache()
        {
            SetElementBorderColor(TranslateColorString(bordercolor));
            SetElementColor(TranslateColorString(elementcolor));
            SetElementOutlineColor(TranslateColorString(outlinecolor));
            m_fontText = !string.IsNullOrEmpty(font)
                ? TranslateFontString(font)
                : null;
        }

        /// <summary>
        /// Performs a partial deepy copy based on the input element, the name field is left unchanged
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="bInitializeCache"></param>
        public void DeepCopy(ProjectLayoutElement zElement, bool bInitializeCache)
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
            type = zElement.type;
            autoscalefont = zElement.autoscalefont;
            lockaspect = zElement.lockaspect;
            keeporiginalsize = zElement.keeporiginalsize;
            variable = zElement.variable;
            opacity = zElement.opacity;
            enabled = zElement.enabled;
            lineheight = zElement.lineheight;
            wordspace = zElement.wordspace;

            if (bInitializeCache)
            {
                InitializeCache();
            }
        }

        public static Color TranslateColorString(string sColor)
        {
            if (null != sColor)
            {
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
                        case 6: //0xRGB
                            return Color.FromArgb(
                                Math.Min(255,
                                    Int32.Parse(sColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber)),
                                Math.Min(255,
                                    Int32.Parse(sColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)),
                                Math.Min(255,
                                    Int32.Parse(sColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                        case 8: //0xRGBA
                            return Color.FromArgb(
                                Math.Min(255,
                                    Int32.Parse(sColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)),
                                Math.Min(255,
                                    Int32.Parse(sColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber)),
                                Math.Min(255,
                                    Int32.Parse(sColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber)),
                                Math.Min(255,
                                    Int32.Parse(sColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)));
                        case 9: //RGB
                            return Color.FromArgb(
                                Math.Min(255, Int32.Parse(sColor.Substring(0, 3))),
                                Math.Min(255, Int32.Parse(sColor.Substring(3, 3))),
                                Math.Min(255, Int32.Parse(sColor.Substring(6, 3))));
                        case 12: //RGBA
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

            }
            return Color.Black;
        }

        public static string GetElementColorString(Color zColor)
        {
            return COLOR_HEX_STR +
                zColor.R.ToString("X").PadLeft(2, '0') + 
                zColor.G.ToString("X").PadLeft(2, '0') +
                zColor.B.ToString("X").PadLeft(2, '0') +
                zColor.A.ToString("X").PadLeft(2, '0');
        }

        public static Font TranslateFontString(string fontString)
        {
            var arraySplit = fontString.Split(new char[] { ';' });
            float fFontSize;
            if (6 != arraySplit.Length || !float.TryParse(arraySplit[1], out fFontSize))
            {
                return null;
            }
            return new Font(arraySplit[0], float.Parse(arraySplit[1]),
                (arraySplit[2].Equals("1") ? FontStyle.Bold : FontStyle.Regular) |
                (arraySplit[3].Equals("1") ? FontStyle.Underline : FontStyle.Regular) |
                (arraySplit[4].Equals("1") ? FontStyle.Italic : FontStyle.Regular) |
                (arraySplit[5].Equals("1") ? FontStyle.Strikeout : FontStyle.Regular));
        }

        // NOTE: Any properties will anger the XSD generated XML/CS file.

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

        public Font GetElementFont()
        {
            return m_fontText;
        }

        public void SetElementBorderColor(Color zColor)
        {
            bordercolor = GetElementColorString(zColor);
            m_colorBorder = zColor;
        }

        public void SetElementColor(Color zColor)
        {
            elementcolor = GetElementColorString(zColor);
            m_colorElement = zColor;
        }

        public void SetElementOutlineColor(Color zColor)
        {
            outlinecolor = GetElementColorString(zColor);
            m_colorOutline = zColor;
        }

        public void SetElementFont(Font zFont)
        {
            font = zFont.Name + ";" + zFont.Size + ";" +
                   (zFont.Bold ? "1" : "0") + ";" +
                   (zFont.Underline ? "1" : "0") + ";" +
                   (zFont.Italic ? "1" : "0") + ";" +
                   (zFont.Strikeout ? "1" : "0");
            m_fontText = zFont;
        }

    }
}