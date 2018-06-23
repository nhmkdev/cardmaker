using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Translation
{
    public class JavascriptHostFunctions
    {
        private ProjectLayoutElement m_zElement;

        public Dictionary<string, string> dictionaryOverrideFieldToValue = new Dictionary<string, string>();

        public JavascriptHostFunctions(ProjectLayoutElement zElement)
        {
            m_zElement = zElement;
        }

        public void AddOverrideField(string sField, string sValue)
        {
            if (!TranslatorBase.IsDisallowedOverrideField(sField))
            {
                // empty override values are discarded (matches reference overrides)
                if (!string.IsNullOrWhiteSpace(sValue))
                {
                    dictionaryOverrideFieldToValue[sField] = sValue;
                }
            }
            else
            {
                Logger.AddLogLine(
                    "[{1}] override not allowed on element: [{0}]".FormatString(m_zElement.name, sField));
            }
        }

    }
}
