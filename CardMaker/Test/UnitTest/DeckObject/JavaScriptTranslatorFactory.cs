using System.Collections.Generic;
using CardMaker.Card.Translation;

namespace UnitTest.DeckObject
{
    class JavaScriptTranslatorFactory : ITranslatorFactory
    {
        public TranslatorBase GetTranslator(Dictionary<string, int> dictionaryColumnNames, Dictionary<string, string> dictionaryDefines,
            Dictionary<string, Dictionary<string, int>> dictionaryElementOverrides, List<string> listColumnNames)
        {
            return new JavaScriptTranslator(dictionaryColumnNames, dictionaryDefines, dictionaryElementOverrides, listColumnNames);
        }
    }
}
