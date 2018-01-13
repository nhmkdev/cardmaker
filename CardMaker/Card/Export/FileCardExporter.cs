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
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Export
{
    public class FileCardExporter : CardExportBase, ICardExporter
    {
        private readonly string m_sExportFolder;
        private readonly string m_sOverrideStringFormat;
        private readonly ImageFormat m_eImageFormat;
        private readonly int m_nSkipStitchIndex;

        public FileCardExporter(int nLayoutStartIndex, int nLayoutEndIdx, string sExportFolder, string sOverrideStringFormat, int
            nSkipStitchIndex, System.Drawing.Imaging.ImageFormat eImageFormat)
            : base(nLayoutStartIndex, nLayoutEndIdx)
        {
            if (!sExportFolder.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                sExportFolder += Path.DirectorySeparatorChar;
            }

            m_sExportFolder = sExportFolder;
            m_sOverrideStringFormat = sOverrideStringFormat;

            m_nSkipStitchIndex = nSkipStitchIndex;
            m_eImageFormat = eImageFormat;
        }

        public void ExportThread()
        {
            var zWait = WaitDialog.Instance;

            zWait.ProgressReset(0, 0, ExportLayoutEndIndex - ExportLayoutStartIndex, 0);
            for (var nIdx = ExportLayoutStartIndex; nIdx < ExportLayoutEndIndex; nIdx++)
            {
                ChangeExportLayoutIndex(nIdx);
                var nPadSize = CurrentDeck.CardCount.ToString(CultureInfo.InvariantCulture).Length;
                zWait.ProgressReset(1, 0, CurrentDeck.CardCount, 0);

                var exportWidth = CurrentDeck.CardLayout.exportWidth == 0
                    ? CurrentDeck.CardLayout.width : CurrentDeck.CardLayout.exportWidth;

                var exportHeight = CurrentDeck.CardLayout.exportHeight == 0
                    ? CurrentDeck.CardLayout.height : CurrentDeck.CardLayout.exportHeight;

                if (CurrentDeck.CardLayout.width > exportWidth ||
                    CurrentDeck.CardLayout.height > exportHeight)
                {
                    Logger.AddLogLine(
                        $"ERROR: Layout: [{CurrentDeck.CardLayout.Name}] exportWidth and/or exportHeight too small! (Skipping export)");
                    continue;
                }

                UpdateBufferBitmap(exportWidth, exportHeight);
                var zGraphics = Graphics.FromImage(m_zExportCardBuffer);
                var nCardIdx = 0;
                do
                {
                    var nX = 0;
                    var nY = 0;
                    var nCardsExportedInImage = 0;
                    zGraphics.Clear(CurrentDeck.CardLayout.exportTransparentBackground ?
                        CardMakerConstants.NoColor :
                        Color.White);
                    do
                    {
                        CurrentDeck.ResetDeckCache();
                        CurrentDeck.CardPrintIndex = nCardIdx++;
                        nCardsExportedInImage++;
                        CardRenderer.DrawPrintLineToGraphics(zGraphics, nX, nY, !CurrentDeck.CardLayout.exportTransparentBackground);
                        m_zExportCardBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

                        zWait.ProgressStep(1);

                        int nMoveCount = 1;
                        if (m_nSkipStitchIndex > 0)
                        {
                            var x = ((nCardsExportedInImage + 1)%m_nSkipStitchIndex);
                            if (x == 0)
                            {
                                // shift forward an extra spot to ignore the dummy index
                                nMoveCount = 2;
                            }
                        }

                        var bOutOfSpace = false;
                        for (int nShift = 0; nShift < nMoveCount; nShift++)
                        {
                            nX += CurrentDeck.CardLayout.width + CurrentDeck.CardLayout.buffer;
                            if (nX + CurrentDeck.CardLayout.width > exportWidth)
                            {
                                nX = 0;
                                nY += CurrentDeck.CardLayout.height + CurrentDeck.CardLayout.buffer;
                            }
                            if (nY + CurrentDeck.CardLayout.height > exportHeight)
                            {
                                // no more space
                                bOutOfSpace = true;
                                break;
                            }
                        }

                        if (bOutOfSpace)
                        {
                            break;
                        }
                    } while (nCardIdx < CurrentDeck.CardCount);

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

                } while (nCardIdx < CurrentDeck.CardCount);
                zWait.ProgressStep(0);
            }

            zWait.ThreadSuccess = true;
            zWait.CloseWaitDialog();
        }

        /// <summary>
        /// Updates the export buffer
        /// </summary>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="zGraphics"></param>
        protected override void UpdateBufferBitmap(int nWidth, int nHeight, Graphics zGraphics = null)
        {
            m_zExportCardBuffer?.Dispose();
            m_zExportCardBuffer = null == zGraphics
                ? new Bitmap(nWidth, nHeight)
                : new Bitmap(nWidth, nHeight, zGraphics);
        }
    }
}
