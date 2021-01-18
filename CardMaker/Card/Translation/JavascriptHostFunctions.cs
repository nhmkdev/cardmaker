////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2021 Tim Stair
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

using System.Collections.Generic;
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
            if (TranslatorBase.IsDisallowedOverrideField(sField))
            {
                Logger.AddLogLine("[{1}] override not allowed on element: [{0}]".FormatString(m_zElement.name, sField));
            }
            // empty override values are discarded (matches reference overrides)
            else if (!string.IsNullOrWhiteSpace(sValue))
            {
                dictionaryOverrideFieldToValue[sField] = sValue;
            }
        }

    }
}
