////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using CardMaker.Data;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Export
{
    public class FileCardSingleExporter : CardExportBase, ICardExporter
    {
        private readonly string m_sExportFolder;
        private readonly string m_sOverrideStringFormat;
        private readonly ImageFormat m_eImageFormat;
        private readonly int m_nImageExportIndex;

        public FileCardSingleExporter(int nLayoutStartIndex, int nLayoutEndIdx, string sExportFolder, string sOverrideStringFormat, ImageFormat eImageFormat, int nImageExportIndex)
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

        public void ExportThread()
        {
            var zWait = WaitDialog.Instance;

            zWait.ProgressReset(0, 0, ExportLayoutEndIndex - ExportLayoutStartIndex, 0);
            ChangeExportLayoutIndex(ExportLayoutStartIndex);
            var nPadSize = CurrentDeck.CardCount.ToString(CultureInfo.InvariantCulture).Length;
            zWait.ProgressReset(1, 0, CurrentDeck.CardCount, 0);

            UpdateBufferBitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);

            var zGraphics = Graphics.FromImage(m_zExportCardBuffer);
            var nCardIdx = m_nImageExportIndex;
            zGraphics.Clear(CurrentDeck.CardLayout.exportTransparentBackground ?
                CardMakerConstants.NoColor :
                Color.White);
            CurrentDeck.ResetDeckCache();
            CurrentDeck.CardPrintIndex = nCardIdx++;
            CardRenderer.DrawPrintLineToGraphics(zGraphics, 0, 0, !CurrentDeck.CardLayout.exportTransparentBackground);
            m_zExportCardBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

            zWait.ProgressStep(1);

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
                m_zExportCardBuffer.Save(
                    m_sExportFolder + sFileName +
                    "." + m_eImageFormat.ToString().ToLower(),
                    m_eImageFormat);
                ProcessRotateExport(m_zExportCardBuffer, CurrentDeck.CardLayout, true);
            }
            catch (Exception)
            {
                Logger.AddLogLine("Invalid Filename or IO error: " + sFileName);
                zWait.ThreadSuccess = false;
                zWait.CloseWaitDialog();
                return;
            }

            zWait.ProgressStep(0);

            zWait.ThreadSuccess = true;
            zWait.CloseWaitDialog();
        }
    }
}
