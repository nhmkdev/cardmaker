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
            return float.TryParse(sValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out fValue);
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
            return decimal.TryParse(sValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out dValue);
        }

    }
}
