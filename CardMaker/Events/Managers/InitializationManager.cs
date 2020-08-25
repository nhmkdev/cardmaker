using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
