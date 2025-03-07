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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Xml.Serialization;
using CardMaker.Card;
using CardMaker.Data;
using Support.IO;
using Support.Util;
using CardMaker.Data.Serialization;
using Microsoft.ClearScript;

namespace CardMaker.XML
{
    [DefaultScriptUsage(ScriptAccess.ReadOnly)]
    public class ProjectLayoutElement
    {
        private static readonly List<PropertyInfo> s_listPropertyInfos;

        #region Properties

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public string layoutreference { get; set; }
        
        [XmlAttribute]
        public string elementreference { get; set; }

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
        public int lineheight { get; set; } // aka line spacing (overloaded)

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

        [XmlAttribute]
        public int mirrortype { get; set; }

        [XmlAttribute]
        public bool centerimageonorigin { get; set; }

        [XmlAttribute]
        public string gradient { get; set; }

        [XmlAttribute]
        public int colortype { get; set; }

        [XmlAttribute]
        public string colormatrix { get; set; }

        [XmlAttribute]
        public bool imagemasksurface { get; set; }

        #endregion

        private Color m_colorElement = Color.Black;
        private Color m_colorOutline = Color.Black;
        private Color m_colorBorder = Color.Black;
        private Color m_colorBackground = CardMakerConstants.NoColor;
        private ColorMatrix m_colorMatrix = null;
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
            outlinecolor = GetElementColorString(m_colorOutline);
            rotation = 0;
            bordercolor = GetElementColorString(m_colorBorder);
            font = string.Empty;
            elementcolor = GetElementColorString(m_colorElement);
            backgroundcolor = GetElementColorString(m_colorBackground);
            type = ElementType.Text.ToString();
            lineheight = 0;
            wordspace = 0;
            autoscalefont = false;
            lockaspect = false;
            tilesize = string.Empty;
            keeporiginalsize = false;
            variable = string.Empty;
            name = sName;
            layoutreference = null;
            elementreference = null;
            opacity = 255;
            enabled = true;
            justifiedtext = false;
            mirrortype = (int)MirrorType.None;
            centerimageonorigin = false;
            gradient = string.Empty;
            colormatrix = string.Empty;
            colortype = (int)ElementColorType.Add;
            imagemasksurface = false;

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

        public ColorMatrix GetColorMatrix()
        {
            return m_colorMatrix;
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

        public MirrorType GetMirrorType()
        {
            return mirrortype >= 0 && mirrortype < (int)MirrorType.End ? (MirrorType)mirrortype : MirrorType.None;
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
            SetElementColorMatrix(ColorMatrixSerializer.DeserializeFromString(colormatrix));
            Font zNewFont = null;
            if (!string.IsNullOrEmpty(font))
            {
                zNewFont = TranslateFontString(font);
            }
            SetElementFont(zNewFont);

        }

        /// <summary>
        /// Performs a partial deep copy based on the input element, the name field is left unchanged
        /// </summary>
        /// <param name="zElement">The source element to copy from</param>
        /// <param name="bInitializeTranslatedFields">flag indicating whether to reinitialize the translated fields</param>
        [NoScriptAccess]
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
            justifiedtext = zElement.justifiedtext;
            layoutreference = zElement.layoutreference;
            elementreference = zElement.elementreference;
            mirrortype = zElement.mirrortype;
            centerimageonorigin = zElement.centerimageonorigin;
            gradient = zElement.gradient;
            colortype = zElement.colortype;
            colormatrix = zElement.colormatrix;
            imagemasksurface = zElement.imagemasksurface;

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
            return TranslateColorString(sColor, defaultAlpha, out var bSucceeded);
        }

        public static Color TranslateColorString(string sColor, int defaultAlpha, out bool bSucceeded)
        {
            return ColorSerialization.TranslateColorString(sColor, defaultAlpha, out bSucceeded);
        }

        /// <summary>
        /// Converts a color to the a color formatted string for serialization across the app (0x hex form)
        /// </summary>
        /// <param name="zColor"></param>
        /// <returns></returns>
        public static string GetElementColorString(Color zColor)
        {
            return ColorSerialization.GetElementColorString(zColor);
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
        [NoScriptAccess]
        public void SetElementBorderColor(Color zColor)
        {
            bordercolor = GetElementColorString(zColor);
            m_colorBorder = zColor;
        }

        /// <summary>
        /// Sets the element color and color string
        /// </summary>
        /// <param name="zColor">The color to pull the values from</param>
        [NoScriptAccess]
        public void SetElementColor(Color zColor)
        {
            elementcolor = GetElementColorString(zColor);
            m_colorElement = zColor;
        }

        [NoScriptAccess]
        public void SetElementColorMatrix(ColorMatrix zColorMatrix)
        {
            if (zColorMatrix == null)
            {
                colormatrix = string.Empty;
                m_colorMatrix = null;
            }
            else
            {
                colormatrix = ColorMatrixSerializer.SerializeToString(zColorMatrix);
                m_colorMatrix = zColorMatrix;
            }
        }

        /// <summary>
        /// Sets the outline color and color string
        /// </summary>
        /// <param name="zColor">The color to pull the values from</param>
        [NoScriptAccess]
        public void SetElementOutlineColor(Color zColor)
        {
            outlinecolor = GetElementColorString(zColor);
            m_colorOutline = zColor;
        }

        /// <summary>
        /// Sets the outline color and color string
        /// </summary>
        /// <param name="zColor">The color to pull the values from</param>
        [NoScriptAccess]
        public void SetElementBackgroundColor(Color zColor)
        {
            backgroundcolor = GetElementColorString(zColor);
            m_colorBackground = zColor;
        }

        /// <summary>
        /// Sets the font and font string
        /// </summary>
        /// <param name="zFont">The font to pull the settings from</param>
        [NoScriptAccess]
        public void SetElementFont(Font zFont)
        {
            if (zFont == null)
            {
                zFont = FontLoader.DefaultFont;
            }

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

        public static string BoolToNumericString(bool bValue)
        {
            return bValue ? "1" : "0";
        }

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