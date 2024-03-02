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

using CardMaker.Data;

namespace CardMaker.Card
{
    public static class MeasurementUtil
    {
        public const string PIXEL = "Pixel";

        public static double GetInchesFromMillimeter(double dMillimeter)
        {
            return dMillimeter / 25.4d;
        }

        public static double GetInchesFromCentimeter(double dCentimeter)
        {
            return dCentimeter / 2.54d;
        }

        public static double GetMillimetersFromInch(double dMillimeter)
        {
            return dMillimeter * 25.4d;
        }

        public static double GetCentimetersFromInch(double dCentimeter)
        {
            return dCentimeter * 2.54d;
        }

        /// <summary>
        /// Gets the width/height measurement value based on the dpi, measurement unit
        /// </summary>
        /// <param name="dWidth">Width in pixels</param>
        /// <param name="dHeight">Height in pixels</param>
        /// <param name="dDpi">The DPI</param>
        /// <param name="sMeasurementUnit">The measurement unit to translate to</param>
        /// <param name="dMeasuredWidth">The resulting width</param>
        /// <param name="dMeasuredHeight">The resulting height</param>
        public static void GetMeasurement(decimal dWidth, decimal dHeight, decimal dDpi, string sMeasurementUnit, out decimal dMeasuredWidth, out decimal dMeasuredHeight)
        {
            dMeasuredWidth = dWidth / dDpi;
            dMeasuredHeight = dHeight / dDpi;
            switch (sMeasurementUnit)
            {
                case nameof(MeasurementUnit.Inch):
                    // already calculated
                    break;
                case nameof(MeasurementUnit.Millimeter):
                    dMeasuredWidth = (decimal)GetMillimetersFromInch((double)dMeasuredWidth);
                    dMeasuredHeight = (decimal)GetMillimetersFromInch((double)dMeasuredHeight);
                    break;
                case nameof(MeasurementUnit.Centimeter):
                    dMeasuredWidth = (decimal)GetCentimetersFromInch((double)dMeasuredWidth);
                    dMeasuredHeight = (decimal)GetCentimetersFromInch((double)dMeasuredHeight);
                    break;
                case MeasurementUtil.PIXEL:
                    dMeasuredWidth = dWidth;
                    dMeasuredHeight = dHeight;
                    break;
            }
        }

        /// <summary>
        /// Gets the pixel measurement based on the specified dpi and measurement unit
        /// </summary>
        /// <param name="dWidth">The measurement width</param>
        /// <param name="dHeight">The measurement height</param>
        /// <param name="dDpi">The DPI</param>
        /// <param name="sMeasurementUnit">The measurement unit to translate from</param>
        /// <param name="nWidth">The pixel width</param>
        /// <param name="nHeight">The pixel height</param>
        public static void GetPixelMeasurement(decimal dWidth, decimal dHeight, decimal dDpi, string sMeasurementUnit, out int nWidth, out int nHeight)
        {
            switch (sMeasurementUnit)
            {
                case nameof(MeasurementUnit.Inch):
                    nWidth = (int)(dWidth * dDpi);
                    nHeight = (int)(dHeight * dDpi);
                    break;
                case nameof(MeasurementUnit.Millimeter):
                    nWidth = (int)(GetInchesFromMillimeter((double)dWidth) * (double)dDpi);
                    nHeight = (int)(GetInchesFromMillimeter((double)dHeight) * (double)dDpi);
                    break;
                case nameof(MeasurementUnit.Centimeter):
                    nWidth = (int)(GetInchesFromCentimeter((double)dWidth) * (double)dDpi);
                    nHeight = (int)(GetInchesFromCentimeter((double)dHeight) * (double)dDpi);
                    break;
                default:
                    nWidth = (int) dWidth;
                    nHeight = (int) dHeight;
                    break;
            }
        }
    }
}
