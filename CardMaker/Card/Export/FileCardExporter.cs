////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Export
{
    public class FileCardExporter : CardExportBase, ICardExporter
    {
        private readonly string m_sExportFolder;
        private readonly string m_sStringFormat;
        private readonly System.Drawing.Imaging.ImageFormat m_eImageFormat;

        public FileCardExporter(int nLayoutStartIndex, int nLayoutEndIdx, string sExportFolder, bool bOverrideLayoutStringFormat, string sStringFormat,
            System.Drawing.Imaging.ImageFormat eImageFormat)
            : base(nLayoutStartIndex, nLayoutEndIdx)
        {
            if (!sExportFolder.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                sExportFolder += Path.DirectorySeparatorChar;
            }

            m_sExportFolder = sExportFolder;
            if (!bOverrideLayoutStringFormat)
            {
                m_sStringFormat = sStringFormat;
            }
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
                    Logger.AddLogLine(string.Format("ERROR: Layout: [{0}] exportWidth and/or exportHeight too small! (Skipping export)", CurrentDeck.CardLayout.Name));
                    continue;
                }

                UpdateBufferBitmap(exportWidth, exportHeight);
                var zGraphics = Graphics.FromImage(m_zExportCardBuffer);
                var nCardIdx = 0;
                do
                {
                    var nX = 0;
                    var nY = 0;
                    zGraphics.Clear(CurrentDeck.CardLayout.exportTransparentBackground ? 
                        Color.FromArgb(0, 0, 0, 0) :
                        Color.White);
                    do
                    {
                        CurrentDeck.ResetDeckCache();
                        CurrentDeck.CardPrintIndex = nCardIdx;
                        CardRenderer.DrawPrintLineToGraphics(zGraphics, nX, nY, !CurrentDeck.CardLayout.exportTransparentBackground);
                        m_zExportCardBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

                        zWait.ProgressStep(1);
                        nCardIdx++;

                        nX += CurrentDeck.CardLayout.width;
                        if (nX + CurrentDeck.CardLayout.width > exportWidth)
                        {
                            nX = 0;
                            nY += CurrentDeck.CardLayout.height;
                        }
                        if (nY + CurrentDeck.CardLayout.height > exportHeight)
                        {
                            // no more space
                            break;
                        }

                    } while (nCardIdx < CurrentDeck.CardCount);

                    string sFileName;

                    if (!string.IsNullOrEmpty(m_sStringFormat))
                    {
                        sFileName = CurrentDeck.TranslateFileNameString(m_sStringFormat, nCardIdx, nPadSize);
                    }
                    else // default
                    {
                        sFileName = CurrentDeck.CardLayout.Name + "_" + (nCardIdx).ToString(CultureInfo.InvariantCulture).PadLeft(nPadSize, '0');
                    }
                    try
                    {
                        ProcessRotateExport(CurrentDeck.CardLayout, true);
                        m_zExportCardBuffer.Save(
                            m_sExportFolder + sFileName +
                            "." + m_eImageFormat.ToString().ToLower(),
                            m_eImageFormat);
                        ProcessRotateExport(CurrentDeck.CardLayout, false);
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
        /// Rotates the export buffer based on the Layout exportRotation setting
        /// </summary>
        /// <param name="zLayout"></param>
        /// <param name="preExport"></param>
        protected void ProcessRotateExport(ProjectLayout zLayout, bool preExport)
        {
            switch (zLayout.exportRotation)
            {
                case 90:
                    m_zExportCardBuffer.RotateFlip(preExport ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone);
                    break;
                case -90:
                    m_zExportCardBuffer.RotateFlip(preExport ? RotateFlipType.Rotate270FlipNone : RotateFlipType.Rotate90FlipNone);
                    break;
            }
        }

        /// <summary>
        /// Updates the export buffer
        /// </summary>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="zGraphics"></param>
        protected override void UpdateBufferBitmap(int nWidth, int nHeight, Graphics zGraphics = null)
        {
            if (null != m_zExportCardBuffer)
            {
                m_zExportCardBuffer.Dispose();
            }
            m_zExportCardBuffer = null == zGraphics
                ? new Bitmap(nWidth, nHeight)
                : new Bitmap(nWidth, nHeight, zGraphics);
        }

    }
}
