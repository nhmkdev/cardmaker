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
using System.Drawing;
using System.Linq;
using CardMaker.Data;
using CardMaker.XML;
using Support.IO;

namespace CardMaker.Card.FormattedText
{
    public enum MarginType
    {
        Left,
        Right
    }

    public class FormattedTextProcessData
    {
        public Font Font { get; private set; }
        public float FontHeight { get; private set; }
        public float FontSpaceWidth { get; private set; }
        public float FontSpaceHeight { get; private set; }
        public Brush FontBrush { get; set; }
        public float FontScaleX { get; set; }
        public float FontScaleY { get; set; }

        public bool ForceTextCaps { get; set; }
        public Color ImageColor { get; set; }

        public float CurrentX { get; set; }
        public float CurrentY { get; set; }
        public float CurrentYOffset { get; set; }
        public float CurrentXOffset { get; set; }

        private Dictionary<Tuple<int, int>, int> m_dictionaryLeftMarginRanges = new Dictionary<Tuple<int, int>, int>();
        private Dictionary<Tuple<int, int>, int> m_dictionaryRightMarginRanges = new Dictionary<Tuple<int, int>, int>();
        public float CurrentMarginLeft { get; set; }
        public float CurrentMarginRight { get; set; }

        public float CurrentLineHeight { get; set; }
        public StringAlignment CurrentStringAlignment { get; set; }

        public bool InParagraph { get; set; }
        public int ParagraphCharacterLineIndent { get; set; }
        public int ParagraphStartCharacterIndent { get; set; }
        public int ParagraphNextLineIndent { get; set; }

        public int CurrentLine { get; private set; }
        public MirrorType CurrentMirrorType { get; set; }

        const string FontStringToTest = "]"; // TODO: is there a large box character with the bounds?

        public FormattedTextProcessData()
        {
            FontScaleX = 1f;
            FontScaleY = 1f;
            CurrentX = 0;
            CurrentY = 0;
            ImageColor = Color.Black;
        }

        public void SetFont(Font zFont, Graphics zGraphics)
        {
            Font = zFont;
            FontHeight = zFont.GetHeight(zGraphics);

            var rectWithSpace = MeasureDisplayStringWidth(zGraphics, " " + FontStringToTest, zFont);
            var rectWithoutSpace = MeasureDisplayStringWidth(zGraphics, FontStringToTest, zFont);

            // NOTE the element word space is ignored! (is that a problem?)
            FontSpaceWidth = rectWithSpace.Width - rectWithoutSpace.Width;
            FontSpaceHeight = Math.Max((float)rectWithSpace.Height, FontHeight);
        }

        public static RectangleF MeasureDisplayStringWidth(Graphics zGraphics, string text, Font font)
        {
            // measurements should be performed at the reset transform
            var matrixOriginalTransform = zGraphics.Transform;
            zGraphics.ResetTransform();
            var zFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
            };
            var rect = new RectangleF(0, 0, 65536, 65536);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };
            zFormat.SetMeasurableCharacterRanges(ranges);
            var regions = zGraphics.MeasureCharacterRanges(text, font, rect, zFormat);
            rect = regions[0].GetBounds(zGraphics);
            zGraphics.Transform = matrixOriginalTransform;
            return rect;
        }

        public void AddFontStyle(FontStyle eStyle, Graphics zGraphics)
        {
            var eNewStyle = Font.Style | eStyle;
            SetFont(FontLoader.GetFont(Font.FontFamily, Font.Size, eNewStyle), zGraphics);
        }

        public void RemoveFontStyle(FontStyle eStyle, Graphics zGraphics)
        {
            var eNewStyle = Font.Style & ~eStyle;
            SetFont(FontLoader.GetFont(Font.FontFamily, Font.Size, eNewStyle), zGraphics);
        }

        public void MoveToNextLine(ProjectLayoutElement zElement, bool bFromMarginUpdate = false)
        {
            CurrentLine++;
            CurrentY += CurrentLineHeight;
            CurrentX = 0;
            if (InParagraph)
            {
                CurrentX += ParagraphNextLineIndent * FontSpaceWidth;
                ParagraphNextLineIndent = ParagraphCharacterLineIndent;
            }
            UpdateMargins(zElement, bFromMarginUpdate);
        }

        public bool IsXPositionOutsideBounds(float fX)
        {
            return fX > CurrentMarginRight;
        }

        public float GetMaxLineWidth()
        {
            return CurrentMarginRight - CurrentMarginLeft;
        }

        /// <summary>
        /// Configures a single margin
        /// </summary>
        /// <param name="eMarginType">The type of margin to add</param>
        /// <param name="nMargin">The amount of the margin</param>
        /// <param name="nTop">The top of the margin (y)</param>
        /// <param name="nBottom">The bottom of the margin (y). This is exclusive.</param>
        public void AddHorizontalMargin(ProjectLayoutElement zElement, MarginType eMarginType, int nMargin, int nTop, int nBottom)
        {
            var dictionaryMargins = getMarginDictionaryByType(eMarginType);
            // right margin is width - margin
            nMargin = eMarginType == MarginType.Right ? zElement.width - nMargin : nMargin;

            if (null == dictionaryMargins)
            {
                Logger.AddLogLine("Unsupported margin type specified." + eMarginType);
                return;
            }

            var tupleYRange = new Tuple<int, int>(nTop, nBottom);
            if (dictionaryMargins.ContainsKey(tupleYRange))
            {
                Logger.AddLogLine("WARNING: Duplicate margin range specified.");
                dictionaryMargins.Remove(tupleYRange);
            }

            // don't allow 0 or larger than the width of the element
            dictionaryMargins.Add(tupleYRange, Math.Min(zElement.width - 1, Math.Max(0, nMargin)));
            UpdateMargins(zElement);
        }

        private Dictionary<Tuple<int, int>, int> getMarginDictionaryByType(MarginType eMarginType)
        {
            switch (eMarginType)
            {
                case MarginType.Left:
                    return m_dictionaryLeftMarginRanges;
                case MarginType.Right:
                    return m_dictionaryRightMarginRanges;
            }
            return null;
        }

        private void UpdateMargins(ProjectLayoutElement zElement, bool bFromMarginUpdate = false)
        {
            // when looking for the margin consider the pixel position of the top of the current font rect as well as the bottom
#warning this is imperfect as the font may change
            CurrentMarginLeft = Math.Max(GetMargin(m_dictionaryLeftMarginRanges, (int) CurrentY, 0),
                GetMargin(m_dictionaryLeftMarginRanges, (int) (CurrentY + FontHeight), 0));

            CurrentMarginRight = Math.Min(GetMargin(m_dictionaryRightMarginRanges, (int)CurrentY, zElement.width),
                GetMargin(m_dictionaryRightMarginRanges, (int)(CurrentY + FontHeight), zElement.width));

            CurrentX = Math.Max(CurrentMarginLeft, CurrentX);
            if (CurrentX > CurrentMarginRight)
            {
                if (bFromMarginUpdate)
                {
                    Logger.AddLogLine("WARNING: Margin configuration is not allowing enough horizontal space for a new line.");
                }
                else
                {
                    MoveToNextLine(zElement, true);
                }
            }
        }

        private static int GetMargin(Dictionary<Tuple<int, int>, int> dictionaryMargins, int nY, int nDefault)
        {
            var zMarginKey = dictionaryMargins.Keys.FirstOrDefault(zTupleRange =>
                zTupleRange.Item1 <= nY && zTupleRange.Item2 > nY);
            return null == zMarginKey ? nDefault : dictionaryMargins[zMarginKey];
        }
    }
}