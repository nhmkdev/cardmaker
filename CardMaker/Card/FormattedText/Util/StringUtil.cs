using System.Text;

namespace CardMaker.Card.FormattedText.Util
{
    public static class StringUtil
    {
        /// <summary>
        /// Convert a string to title case. Words are separated by whitespace. Any non-whitespace character is treated is a character
        /// to be capitalized at the start of a word.
        /// </summary>
        /// <param name="sInput"></param>
        /// <returns></returns>
        public static string ConvertToTitleCase(string sInput)
        {
            if (string.IsNullOrWhiteSpace(sInput))
            {
                return sInput;
            }

            var stringBuilder = new StringBuilder(sInput.Length);
            var upperCaseNextChar = true;
            foreach (var c in sInput)
            {
                if (char.IsWhiteSpace(c))
                {
                    stringBuilder.Append(c);
                    upperCaseNextChar = true;
                }
                else if (upperCaseNextChar)
                {
                    stringBuilder.Append(char.ToUpper(c));
                    upperCaseNextChar = false;
                }
                else
                {
                    stringBuilder.Append(char.ToLower(c));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
