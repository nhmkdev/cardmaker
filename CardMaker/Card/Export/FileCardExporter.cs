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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using CardMaker.Data;
using Support.IO;

namespace CardMaker.Card.Export
{
    public class FileCardExporter : CardExportBase
    {
        private readonly string m_sExportFolder;
        private readonly string m_sOverrideStringFormat;
        private readonly ImageFormat m_eImageFormat;
        private readonly int m_nSkipStitchIndex;
        public int[] ExportCardIndices { get; set; }

        public FileCardExporter(int nLayoutStartIndex, int nLayoutEndIdx, string sExportFolder, string sOverrideStringFormat, int nSkipStitchIndex, ImageFormat eImageFormat) 
            : this(Enumerable.Range(nLayoutStartIndex, (nLayoutEndIdx - nLayoutStartIndex) + 1).ToArray(), sExportFolder, sOverrideStringFormat, nSkipStitchIndex, eImageFormat)
        {

        }

        public FileCardExporter(int[] arrayExportLayoutIndices, string sExportFolder, string sOverrideStringFormat, int
            nSkipStitchIndex, ImageFormat eImageFormat)
            : base(arrayExportLayoutIndices)
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

        public override void ExportThread()
        {
            var progressLayoutIdx = ProgressReporter.GetProgressIndex(ProgressName.LAYOUT);
            var progressCardIdx = ProgressReporter.GetProgressIndex(ProgressName.CARD);

            ProgressReporter.ProgressReset(progressLayoutIdx, 0, ExportLayoutIndices.Length, 0);
            foreach (var nIdx in ExportLayoutIndices)
            {
                ChangeExportLayoutIndex(nIdx);
                if (CurrentDeck.EmptyReference)
                {
                    // empty reference layouts are not exported
                    ProgressReporter.ProgressStep(progressLayoutIdx);
                    continue;
                }
                var nPadSize = CurrentDeck.CardCount.ToString(CultureInfo.InvariantCulture).Length;
                ProgressReporter.ProgressReset(progressCardIdx, 0, CurrentDeck.CardCount, 0);

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
                var arrayCardIndices = GetCardIndicesArray(CurrentDeck);
                for(var nCardArrayIdx = 0; nCardArrayIdx < arrayCardIndices.Length; nCardArrayIdx++)
                {
                    int nCardId;
                    var nX = 0;
                    var nY = 0;
                    var nCardsExportedInImage = 0;
                    zGraphics.Clear(CurrentDeck.CardLayout.exportTransparentBackground ?
                        CardMakerConstants.NoColor :
                        Color.White);
                    do
                    {
                        // NOTE: If this loops to create a multi-card image the cardId needs to be updated
                        nCardId = arrayCardIndices[nCardArrayIdx];
                        CurrentDeck.ResetDeckCache();
                        // HACK - the printcard index is 0 based but all other uses of nCardId are 1 based (so ++ it!)
                        CurrentDeck.CardPrintIndex = nCardId++;
                        nCardsExportedInImage++;
                        CardRenderer.DrawPrintLineToGraphics(zGraphics, nX, nY, !CurrentDeck.CardLayout.exportTransparentBackground);
                        m_zExportCardBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

                        ProgressReporter.ProgressStep(progressCardIdx);

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
                        // increment and continue to add cards to this buffer
                        nCardArrayIdx++;
                    } while (nCardArrayIdx < arrayCardIndices.Length);

                    string sFileName;

                    // NOTE: nCardId at this point is 1 more than the actual index ... how convenient for export file names...

                    if (!string.IsNullOrEmpty(m_sOverrideStringFormat))
                    {
                        // check for the super override
                        sFileName = CurrentDeck.TranslateFileNameString(m_sOverrideStringFormat, nCardId, nPadSize);
                    }
                    else if (!string.IsNullOrEmpty(CurrentDeck.CardLayout.exportNameFormat))
                    {
                        // check for the per layout override
                        sFileName = CurrentDeck.TranslateFileNameString(CurrentDeck.CardLayout.exportNameFormat, nCardId, nPadSize);
                    }
                    else // default
                    {
                        sFileName = CurrentDeck.CardLayout.Name + "_" + (nCardId).ToString(CultureInfo.InvariantCulture).PadLeft(nPadSize, '0');
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
                    catch (Exception ex)
                    {
                        ProgressReporter.AddIssue("Invalid Filename or IO error: " + sFileName + " :: " + ex.Message);
                        ProgressReporter.ThreadSuccess = false;
                        ProgressReporter.Shutdown();
                        return;
                    }
                }
                ProgressReporter.ProgressStep(progressLayoutIdx);
            }

            ProgressReporter.ThreadSuccess = true;
            ProgressReporter.Shutdown();
        }

        /// <summary>
        /// Gets the array of indices to export
        /// </summary>
        /// <param name="zDeck">The deck to use if no indices are specified</param>
        /// <returns>Array of card indices to export</returns>
        private int[] GetCardIndicesArray(Deck zDeck)
        {
            if (ExportCardIndices != null)
            {
                if (ExportCardIndices.Any(i => i >= zDeck.CardCount || i < 0))
                {
                    throw new Exception("Invalid card indices specified.");
                }

                return ExportCardIndices;
            }
            else
            {
                return Enumerable.Range(0, zDeck.CardCount).ToArray();
            }
        }

        /// <summary>
        /// Updates the export buffer
        /// </summary>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="zGraphics"></param>
        protected override void UpdateBufferBitmap(int nWidth, int nHeight)
        {
            m_zExportCardBuffer?.Dispose();
            m_zExportCardBuffer = new Bitmap(nWidth, nHeight);
        }
    }
}
