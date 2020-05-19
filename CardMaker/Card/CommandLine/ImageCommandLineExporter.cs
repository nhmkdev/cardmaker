////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2019 Tim Stair
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

using System;
using CardMaker.Card.Export;
using CardMaker.Data;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.CommandLine
{
    public class ImageCommandLineExporter : CommandLineExporterBase
    {
        public override bool Validate()
        {
            return true;
        }

        public override bool Export()
        {
            var sExportPath = GetExportPath(true);
#warning is page orientation saved on the layout?
            var zFileCardExporter = new FileCardExporter(GetLayoutIndices(), sExportPath, null, 0, GetImageFormat())
            {
                ExportCardIndices = GetCardIndices()
            };
            zFileCardExporter.ProgressReporter = CardMakerInstance.ProgressReporterFactory.CreateReporter(
                "PDF Export - {0}".FormatString(sExportPath),
                new string[] { ProgressName.LAYOUT, ProgressName.REFERENCE_DATA , ProgressName.CARD },
                zFileCardExporter.ExportThread);
            try
            {
                zFileCardExporter.ProgressReporter.StartProcessing(null);
                return true;
            }
            catch (Exception e)
            {
                Logger.AddLogLine(e.Message);
                Logger.AddLogLine(e.StackTrace);
                return false;
            }
        }
    }
}
