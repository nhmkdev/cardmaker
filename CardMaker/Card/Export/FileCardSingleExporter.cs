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

using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using CardMaker.Data;
using Support.Progress;
using Support.UI;

namespace CardMaker.Card.Export
{
    public class FileCardSingleExporter : CardExportBase
    {
        private readonly string m_sExportFolder;
        private readonly string m_sOverrideStringFormat;
        private readonly FileCardExporterFactory.CardMakerExportImageFormat m_eImageFormat;
        private readonly int m_nImageExportIndex;

        public FileCardSingleExporter(int nLayoutStartIndex, int nLayoutEndIdx, string sExportFolder, string sOverrideStringFormat, FileCardExporterFactory.CardMakerExportImageFormat eImageFormat, int nImageExportIndex)
            : base(nLayoutStartIndex, nLayoutEndIdx)
        {
            if (!sExportFolder.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                sExportFolder += Path.DirectorySeparatorChar;
            }

            m_sExportFolder = sExportFolder;
            m_sOverrideStringFormat = sOverrideStringFormat;

            m_eImageFormat = eImageFormat;
            m_nImageExportIndex = nImageExportIndex;
        }

        public override void ExportThread()
        {
            var progressLayoutIdx = ProgressReporter.GetProgressIndex(ProgressName.LAYOUT);
            var progressCardIdx = ProgressReporter.GetProgressIndex(ProgressName.CARD);

            ProgressReporter.ProgressReset(progressLayoutIdx, 0, ExportLayoutIndices.Length, 0);
            ChangeExportLayoutIndex(ExportLayoutIndices[0]);
            var nPadSize = CurrentDeck.CardCount.ToString(CultureInfo.InvariantCulture).Length;
            ProgressReporter.ProgressReset(progressCardIdx, 0, CurrentDeck.CardCount, 0);

            UpdateBufferBitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);

            var zGraphics = Graphics.FromImage(m_zExportCardBuffer);
            var nCardIdx = m_nImageExportIndex;
            zGraphics.Clear(CurrentDeck.CardLayout.exportTransparentBackground ?
                CardMakerConstants.NoColor :
                Color.White);
            CurrentDeck.ResetDeckCache();
            CurrentDeck.CardPrintIndex = nCardIdx++;

            // loop through SubLayouts and export them
            foreach (var zSubLayoutContext in SubLayoutExportContext.CreateSubLayoutContexts(CurrentDeck, ProgressReporter))
            {
                var zSubLayoutExporter = new FileCardExporter(zSubLayoutContext.LayoutIndex, zSubLayoutContext.LayoutIndex, m_sExportFolder, null, -1, m_eImageFormat);
                zSubLayoutExporter.CurrentDeck.ApplySubLayoutDefinesOverrides(zSubLayoutContext.DefineOverrides);
                zSubLayoutExporter.CurrentDeck.ApplySubLayoutOverrides(CurrentDeck.Defines, CurrentDeck.CurrentPrintLine.ColumnsToValues, CurrentDeck);
                zSubLayoutExporter.ProgressReporter = new LogOnlyProgressReporter();
                zSubLayoutExporter.ExportThread();
            }

            CardRenderer.DrawPrintLineToGraphics(zGraphics, 0, 0, !CurrentDeck.CardLayout.exportTransparentBackground);

            ProgressReporter.ProgressStep(progressCardIdx);

            string sFileName;

            // NOTE: nCardIdx at this point is 1 more than the actual index ... how convenient for export file names...

            if (!string.IsNullOrEmpty(m_sOverrideStringFormat))
            {
                // check for the super override
                sFileName = CurrentDeck.TranslateFileNameString(m_sOverrideStringFormat, nCardIdx, nPadSize);
            }
            else if (!string.IsNullOrEmpty(CurrentDeck.CardLayout.exportNameFormat))
            {
                // check for the per layout override
                sFileName = CurrentDeck.TranslateFileNameString(CurrentDeck.CardLayout.exportNameFormat, nCardIdx, nPadSize);
            }
            else // default
            {
                sFileName = CurrentDeck.CardLayout.Name + "_" + (nCardIdx).ToString(CultureInfo.InvariantCulture).PadLeft(nPadSize, '0');
            }
            try
            {
                ProcessRotateExport(m_zExportCardBuffer, CurrentDeck.CardLayout, false);
                Save(m_zExportCardBuffer,
                    m_sExportFolder + sFileName + "." + m_eImageFormat.ToString().ToLower(),
                    m_eImageFormat,
                    CurrentDeck.CardLayout.dpi);
                ProcessRotateExport(m_zExportCardBuffer, CurrentDeck.CardLayout, true);
            }
            catch (Exception e)
            {
                ProgressReporter.AddIssue("Invalid Filename or IO error: {0} {1}".FormatString(sFileName, e.Message));
                ProgressReporter.ThreadSuccess = false;
                ProgressReporter.Shutdown();
                return;
            }

            ProgressReporter.ProgressStep(progressLayoutIdx);

            ProgressReporter.ThreadSuccess = true;
            ProgressReporter.Shutdown();
        }
    }
}
