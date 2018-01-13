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
using System.Globalization;

namespace Support.Util
{
    /// <summary>
    /// Specialized parsing utilities to allow for cross region parsing of numeric values (1.5 vs. 1,5)
    /// This is necessary due to some users moving data between machines using different regional settings.
    /// </summary>
    public static class ParseUtil
    {
        /// <summary>
        /// Wraps float.TryParse to allow for different formatting for the decimal point. Commas/decimal points are not
        /// supported for the other places in the string (like thousands)
        /// </summary>
        /// <param name="sValue">The string to parse the float from</param>
        /// <param name="fValue">The value to populate</param>
        /// <returns>true on success, false otherwise</returns>
        public static bool ParseFloat(string sValue, out float fValue)
        {
            return Single.TryParse(sValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out fValue);
        }

        /// <summary>
        /// Wraps decimal.TryParse to allow for different formatting for the decimal point. Commas/decimal points are not
        /// supported for the other places in the string (like thousands)
        /// </summary>
        /// <param name="sValue">The string to parse the decimal from</param>
        /// <param name="dValue">The value to populate</param>
        /// <returns>true on success, false otherwise</returns>
        public static bool ParseDecimal(string sValue, out decimal dValue)
        {
            return Decimal.TryParse(sValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out dValue);
        }

        public static int ParseDefault(string sVal, int nDefault)
        {
            int nVal = Int32.TryParse(sVal, out nVal) ? nVal : nDefault;
            return nVal;
        }

        public static bool ParseDefault(string sVal, bool bDefault)
        {
            bool bVal = Boolean.TryParse(sVal, out bVal) ? bVal : bDefault;
            return bVal;
        }
    }
}
