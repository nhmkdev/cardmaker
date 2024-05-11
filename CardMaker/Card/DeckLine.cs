////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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
using CardMaker.Card.Import;
using CardMaker.Data;
using Support.Progress;

namespace CardMaker.Card
{
    public class DeckLine
    {
        public Dictionary<string, string> ColumnsToValues { get; private set; }
        public Dictionary<string, Dictionary<string, string>> ElementToOverrideFieldsToValues { get; private set; }

        public int RowSubIndex { get; private set; }
        public ReferenceLine ReferenceLine { get; private set; }

        public DeckLine(int nRowSubIndex, ReferenceLine zReferenceLine, IReadOnlyList<string> listColumnNames, ProgressReporterProxy zReporterProxy) :base()
        {
            ElementToOverrideFieldsToValues = new Dictionary<string, Dictionary<string, string>>();
            ColumnsToValues = new Dictionary<string, string>();
            RowSubIndex = nRowSubIndex;
            ReferenceLine = zReferenceLine;
            // place all the line entries into the appropriate Dictionary
            for (var nIdx = 1; nIdx < listColumnNames.Count; nIdx++)
            {
                var sColumn = listColumnNames[nIdx];
                var bAssigned = false;
#warning find another home for this logic maybe?
                if (sColumn.StartsWith(CardMakerConstants.OVERRIDE_COLUMN))
                {
                    var arraySplit = sColumn.Split(new char[] { ':' });
                    if (3 == arraySplit.Length)
                    {
                        var sElementName = arraySplit[1].Trim();
                        var sElementItemOverride = arraySplit[2].Trim();
                        if (!ElementToOverrideFieldsToValues.ContainsKey(sElementName))
                        {
                            ElementToOverrideFieldsToValues.Add(sElementName, new Dictionary<string, string>());
                        }
                        var dictionaryElementOverrides = ElementToOverrideFieldsToValues[sElementName];
                        if (dictionaryElementOverrides.ContainsKey(sElementItemOverride))
                        {
                            zReporterProxy.AddIssue($"Duplicate override found: {sElementItemOverride}");
                        }
                        dictionaryElementOverrides[sElementItemOverride] = zReferenceLine.Entries.Count > nIdx
                            ? zReferenceLine.Entries[nIdx]
                            : string.Empty;
                        bAssigned = true;
                    }
                }
                if(!bAssigned)
                {
                    ColumnsToValues[sColumn] = zReferenceLine.Entries.Count > nIdx
                        ? zReferenceLine.Entries[nIdx]
                        : string.Empty;
                }
            }
        }
    }
}
