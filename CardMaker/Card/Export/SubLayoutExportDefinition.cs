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

using CardMaker.Events.Managers;
using Support.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using CardMaker.Data;

namespace CardMaker.Card.Export
{
    public class SubLayoutExportDefinition
    {
        public int LayoutIndex { get; private set; }
        public string LayoutName { get; private set; }
        public SubLayoutExportSettings Settings { get; private set; }
        public Dictionary<string, string> DefineOverrides { get; private set; }

        private SubLayoutExportDefinition()
        {
        }

        public SubLayoutExportDefinition(
            int nLayoutIndex, 
            string sLayoutName, 
            Dictionary<string, string> dictionaryDefineOverrides,
            SubLayoutExportSettings zSettings)
        {
            LayoutIndex = nLayoutIndex;
            LayoutName = sLayoutName;
            DefineOverrides = dictionaryDefineOverrides;
            Settings = zSettings;
        }

        public static List<SubLayoutExportDefinition> CreateSubLayoutExportDefinitions(Deck zDeck, IProgressReporter zProgressReporter)
        {
            var listSubLayoutExportDefinitions = new List<SubLayoutExportDefinition>();

            var nIdx = 0;
#warning optimize this by having it centralized and based on current project state
            var dictionaryLayoutNameToLayout =
                ProjectManager.Instance.LoadedProject.Layout.ToDictionary(layout => layout.Name.ToUpper(), layout => nIdx++);
            foreach (var zElement in zDeck.CardLayout.Element.Where(e => e.type == ElementType.SubLayout.ToString()))
            {
                var zSettings = new SubLayoutExportSettings();
                var dictionaryDefineOverrides = new Dictionary<string, string>();
#warning translation is based on current line and forced print (does that matter?)
                var zElementString = zDeck.TranslateString(zElement.variable, zDeck.CurrentPrintLine, zElement, true, string.Empty, true);
                var arraySplit = zElementString.String.Split(new string[] { "::" }, StringSplitOptions.None);
                var sLayoutName = string.Empty;
                if (arraySplit.Length >= 1)
                {
                    sLayoutName = arraySplit[0].Trim();
                }
                if (arraySplit.Length >= 2)
                {
                    zSettings.ApplySettings(arraySplit[1].Trim());
                }

                if (arraySplit.Length >= 3)
                {
                    var arrayDefinesSplit = arraySplit[2].Split(new string[] { ";;" }, StringSplitOptions.None);
                    for (var nDefineIdx = 0; nDefineIdx < arrayDefinesSplit.Length; nDefineIdx += 2)
                    {
                        var sKey = arrayDefinesSplit[nDefineIdx];
                        var sValue = nDefineIdx + 1 >= arrayDefinesSplit.Length
                            ? string.Empty
                            : arrayDefinesSplit[nDefineIdx + 1];
                        dictionaryDefineOverrides[sKey] = sValue;
                    }
                }
                if (dictionaryLayoutNameToLayout.TryGetValue(sLayoutName.ToUpper(), out var nLayoutIdx))
                {
                    listSubLayoutExportDefinitions.Add(new SubLayoutExportDefinition(
                        nLayoutIdx, 
                        ProjectManager.Instance.LoadedProject.Layout[nLayoutIdx].Name, 
                        dictionaryDefineOverrides,
                        zSettings));
                }
                else
                {
                    zProgressReporter.AddIssue(
                        $"Invalid layout specified {zElement.variable} in element {zElement.name}");
                }
            }

            return listSubLayoutExportDefinitions;
        }
    }
}
