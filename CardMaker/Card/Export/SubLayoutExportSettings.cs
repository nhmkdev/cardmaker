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

using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Support.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardMaker.Card.Export
{
    public class SubLayoutExportSettings
    {
        public bool ApplyReferenceValues { get; private set; } = true;
        public bool ApplyDefineValues { get; private set; } = true;
        public bool WriteFile { get; private set; } = false;
        public int[] ExportIndices { get; private set; }
        public FileCardExporterFactory.CardMakerExportImageFormat ImageFormat { get; private set; }
        public bool WriteMemoryImage { get; private set; } = true;

        public SubLayoutExportSettings()
        {
        }

        public void ApplySettings(string sParameters)
        {
            WriteMemoryImage = true;
            WriteFile = false;

            if (string.IsNullOrWhiteSpace(sParameters))
            {
                return;
            }

            sParameters.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList().
                ForEach(ApplyFeatureSetting);
        }

        private void ApplyFeatureSetting(string sSettingWithParams)
        {
            var arrayFeatureParameters = sSettingWithParams.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (arrayFeatureParameters.Length == 0)
            {
                return;
            }
            switch (arrayFeatureParameters[0].ToLower())
            {
                case "skipreferences":
                    ApplyReferenceValues = false;
                    break;
                case "skipdefines":
                    ApplyDefineValues = false;
                    break;
                case "exportindices":
                    if (arrayFeatureParameters.Length > 1)
                    {
                        var zCardIndicesResult = ExportUtil.GetCardIndices(arrayFeatureParameters[1]);
                        if (zCardIndicesResult != null)
                        {
                            if (!string.IsNullOrWhiteSpace(zCardIndicesResult.Item1))
                            {
                                Logger.AddLogLine("Unable to determine export indices: " + zCardIndicesResult.Item1);
                            }
                            else
                            {
                                ExportIndices = zCardIndicesResult.Item2;
                            }
                        }
                    }
                    break;
                case "nomemexport":
                    WriteMemoryImage = false;
                    break;
                case "exportformat":
                {
                    if (arrayFeatureParameters.Length > 1)
                    {
                        if (FileCardExporterFactory.StringToCardMakerImageExportFormatDictionary.TryGetValue(
                                arrayFeatureParameters[1].Trim().ToLower(), out var eImageFormat))
                        {
                            WriteFile = true;
                            ImageFormat = eImageFormat;
                        }
                    }
                }
                    break;
            }
        }
    }
}
