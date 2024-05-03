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

using System.Drawing;

namespace Support.UI
{
    public enum StringMeasureMethod : int
    {
        CharacterRanges = 0,
        MeasureStringGenericTypoGraphic
    }

    public static class StringMeasure
    {
        public static RectangleF MeasureString(Graphics zGraphics, string sText, Font zFont,
            float fFontScaleX, float fFontScaleY, StringMeasureMethod eMeasureMethod)
        {
            switch (eMeasureMethod)
            {
                case StringMeasureMethod.MeasureStringGenericTypoGraphic:
                    return MeasureStringWidthViaMeasureStringGenericTypoGraphic(zGraphics, sText, zFont, fFontScaleX, fFontScaleY);
                case StringMeasureMethod.CharacterRanges:
                default:
                    return MeasureStringWidthViaMeasureCharacterRanges(zGraphics, sText, zFont, fFontScaleX, fFontScaleY);
            }
        }

        private static RectangleF MeasureStringWidthViaMeasureStringGenericTypoGraphic(
            Graphics zGraphics, string sText, Font zFont, float fFontScaleX, float fFontScaleY)
        {
            // measurements should be performed at the reset transform
            var matrixOriginalTransform = zGraphics.Transform;
            zGraphics.ResetTransform();
            zGraphics.ScaleTransform(fFontScaleX, fFontScaleY);
            var zSize = zGraphics.MeasureString(sText, zFont, int.MaxValue, StringFormat.GenericTypographic);
            zGraphics.Transform = matrixOriginalTransform;
            return new RectangleF(0, 0, zSize.Width, zSize.Height);
        }

        private static RectangleF MeasureStringWidthViaMeasureCharacterRanges(
            Graphics zGraphics, string sText, Font zFont, float fFontScaleX, float fFontScaleY)
        {
            // measurements should be performed at the reset transform
            var matrixOriginalTransform = zGraphics.Transform;
            zGraphics.ResetTransform();
            zGraphics.ScaleTransform(fFontScaleX, fFontScaleY);
            var zFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
            };
            var rect = new RectangleF(0, 0, 65536, 65536);
            CharacterRange[] ranges = { new CharacterRange(0, sText.Length) };
            zFormat.SetMeasurableCharacterRanges(ranges);
            var regions = zGraphics.MeasureCharacterRanges(sText, zFont, rect, zFormat);
            rect = regions[0].GetBounds(zGraphics);
#warning Wasn't this already applied to the graphics above via transform? Is this needed in the other method?
            rect.Width *= fFontScaleX;
            rect.Height *= fFontScaleY;
            zGraphics.Transform = matrixOriginalTransform;
            return rect;
        }
    }
}
