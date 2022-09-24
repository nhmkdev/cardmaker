////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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

using CardMaker.Card.Translation;
using CardMaker.Data;
using Support.IO;

namespace CardMaker.Events.Managers
{
    /// <summary>
    /// Provides cross application initialization functionality (at startup and during runtime)
    /// </summary>
    public class InitializationManager
    {
        public static void Initialize()
        {

        }

        public static void RestoreReplacementChars()
        {
            var arrayReplacementChars = CardMakerSettings.IniManager.GetValue(IniSettings.ReplacementChars, string.Empty).Split(new char[] { CardMakerConstants.CHAR_FILE_SPLIT });
            if (arrayReplacementChars.Length == FilenameTranslator.DISALLOWED_FILE_CHARS_ARRAY.Length)
            {
                FilenameTranslator.IllegalCharReplacementArray = arrayReplacementChars;
            }
            else
            {
                Logger.AddLogLine("Note: No replacement chars have been configured.");
            }
        }
    }
}
