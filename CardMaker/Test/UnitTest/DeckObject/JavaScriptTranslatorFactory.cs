using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardMaker.Card.Translation;
using CardMaker.Data;
using CardMaker.Events.Managers;
using Support.IO;
using Support.UI;

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
