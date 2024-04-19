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
using System.Drawing.Imaging;
using Support.Util;

namespace CardMaker.Data.Serialization
{
    public static class ColorMatrixSerializer
    {
        private static readonly char[] ARRAY_SPLIT_CHARS = new char[] { ';', '\n', '\r' };

        /// <summary>
        /// Serialize ColorMatrix to a string
        /// </summary>
        /// <param name="zColorMatrix"></param>
        /// <returns>String representation or string.Empty on error</returns>
        public static string SerializeToString(ColorMatrix zColorMatrix)
        {
            if (zColorMatrix == null)
            {
                return string.Empty;
            }

            var arrayEntries = new string[25];
            var nIdx = 0;
            for (var y = 0; y < 5; y++)
            {
                for (var x = 0; x < 5; x++)
                {
                    arrayEntries[nIdx] = zColorMatrix[y, x].ToString();
                    nIdx++;
                }
            }

            return string.Join(";", arrayEntries);
        }

        /// <summary>
        /// Deserialize ColorMatrix from string
        /// </summary>
        /// <param name="sColorMatrix"></param>
        /// <returns>ColorMatrix or null on error</returns>
        public static ColorMatrix DeserializeFromString(string sColorMatrix)
        {
            return DeserializeFromString(sColorMatrix, null);
        }

        /// <summary>
        /// Deserialize ColorMatrix from string
        /// </summary>
        /// <param name="sColorMatrix"></param>
        /// <param name="zDefaultColorMatrix"></param>
        /// <returns>ColorMatrix or default on error</returns>
        public static ColorMatrix DeserializeFromString(string sColorMatrix, ColorMatrix zDefaultColorMatrix)
        {
            if (string.IsNullOrWhiteSpace(sColorMatrix))
            {
                return zDefaultColorMatrix;
            }

            var arraySplit = sColorMatrix.Split(ARRAY_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
            if (arraySplit.Length == 25)
            {
                var zColorMatrix = new ColorMatrix();
                var nIdx = 0;
                for (var y = 0; y < 5; y++)
                {
                    for (var x = 0; x < 5; x++)
                    {
                        if (ParseUtil.ParseFloat(arraySplit[nIdx], out var fVal))
                        {
                            zColorMatrix[y, x] = fVal;
                            nIdx++;
                        }
                        else
                        {
                            return zDefaultColorMatrix;
                        }
                    }
                }
                return zColorMatrix;
            }

            return zDefaultColorMatrix;
        }

        public static ColorMatrix GetIdentityColorMatrix()
        {
            var zColorMatrix = new ColorMatrix();
            zColorMatrix.Matrix00 = 
                zColorMatrix.Matrix11 = 
                    zColorMatrix.Matrix22 = 
                        zColorMatrix.Matrix33 =
                            zColorMatrix.Matrix44 = 1;
            return zColorMatrix;
        }
    }
}
