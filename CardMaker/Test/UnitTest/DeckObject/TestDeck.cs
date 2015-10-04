using CardMaker.Card;
using CardMaker.XML;
using System.Collections.Generic;
using CardMaker.Card.Translation;

namespace UnitTest.DeckObject
{
    internal class TestDeck : Deck
    {
        public TestDeck()
        {
            CardLayout = new ProjectLayout()
            {
                defaultCount = 10
            };
        }

        public void SetCardIndex(int idx)
        {
            m_nCardIndex = idx;
        }

        public void ProcessLinesPublic(List<List<string>> listLines,
            List<List<string>> listDefineLines,
            string sReferencePath)
        {
            ProcessLines(listLines, listDefineLines, "unittestref");
        }

        public string GetDefine(string key)
        {
            string value;
            return m_zTranslator.DictionaryDefines.TryGetValue(key, out value) ? value : null;
        }

        public void SetDisallowedCharReplacement(char c, string replacement)
        {
            FilenameTranslator.CharReplacement[c] = replacement;
        }
    }
}
