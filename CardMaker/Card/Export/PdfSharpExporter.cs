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
using System.Collections.Generic;
using System.Drawing;
using CardMaker.Data;
using CardMaker.XML;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Export
{
    /// <summary>
    /// PdfSharp wrapper for exporting cards. This is based heavily on inches due to the use of DPI. CM/MM are supported but converted.
    /// </summary>
    public class PdfSharpExporter : CardExportBase
    {
        private readonly PdfDocument m_zDocument;
        private PdfPage m_zCurrentPage;
        private double m_dDrawX;
        private double m_dDrawY;
        private double m_dLayoutPointWidth;
        private double m_dLayoutPointHeight;
        private double m_dBufferX;
        private double m_dBufferY;
        private XGraphics m_zPageGfx;
        private readonly string m_sExportFile;

        private double m_dPageMarginX = -1;
        private double m_dPageMarginEndX = -1;
        private double m_dPageMarginY = -1;
        private double m_dPageMarginEndY = -1;
        private double m_dNextRowYAdjust = -1;

        private readonly PageOrientation m_ePageOrientation = PageOrientation.Portrait;

        public PdfSharpExporter(int nLayoutStartIndex, int nLayoutEndIndex, string sExportFile, string sPageOrientation) : base(nLayoutStartIndex, nLayoutEndIndex)
        {
            
            m_sExportFile = sExportFile;
            try
            {
                m_ePageOrientation = (PageOrientation)Enum.Parse(typeof(PageOrientation), sPageOrientation);
            }
            catch (Exception)
            {
                Logger.AddLogLine(sPageOrientation + " is an unknow page orientation.");
            }
            m_zDocument = new PdfDocument();
            AddPage();
        }

        public void ExportThread()
        {
            var zWait = WaitDialog.Instance;

#if !MONO_BUILD
            Bitmap zBuffer = null;
#endif

            zWait.ProgressReset(0, 0, ExportLayoutEndIndex - ExportLayoutStartIndex, 0);
            for (var nIdx = ExportLayoutStartIndex; nIdx < ExportLayoutEndIndex; nIdx++)
            {
                ChangeExportLayoutIndex(nIdx);
                zWait.ProgressReset(1, 0, CurrentDeck.CardCount, 0);

                ConfigurePointSizes(CurrentDeck.CardLayout);

                // necessary tracking for which index of the layout is to be exported (this is NOT necessarily the index of the card due to the page back functionality)
                var nNextExportIndex = 0;

                if (0 < nIdx)
                {
                    if (CardMakerSettings.PrintLayoutsOnNewPage)
                    {
                        AddPage();    
                    }

                    if (CardMakerSettings.PrintAutoHorizontalCenter || 
                        (m_dDrawX + m_dLayoutPointWidth > m_dPageMarginEndX)) // this is the case where a layout won't fit in the remaining space of the row
                    {
                        MoveToNextRow(nNextExportIndex);
                    }
                }

                // should be adjusted after the above move to next row
                m_dNextRowYAdjust = Math.Max(m_dNextRowYAdjust, m_dLayoutPointHeight + m_dBufferY);

                if (CardMakerSettings.PrintAutoHorizontalCenter)
                {
                    CenterLayoutPositionOnNewRow(nNextExportIndex);
                }

#if !MONO_BUILD
                zBuffer?.Dispose();
                zBuffer = new Bitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);

                float fOriginalXDpi = zBuffer.HorizontalResolution;
                float fOriginalYDpi = zBuffer.VerticalResolution;
#endif

                foreach (var nCardIdx in GetExportIndices())
                {
                    CurrentDeck.ResetDeckCache();

                    CurrentDeck.CardPrintIndex = nCardIdx;

#if MONO_BUILD
                    // mono build won't support the optimization so re-create the buffer
                    Bitmap zBuffer = new Bitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);
#endif

#if !MONO_BUILD
                    // minor optimization, reuse the same bitmap (for drawing sake the DPI has to be reset)
                    zBuffer.SetResolution(fOriginalXDpi, fOriginalYDpi);
#endif
                    if(nCardIdx == -1)
                    {
                        Graphics.FromImage(zBuffer).FillRectangle(Brushes.White, 0, 0, zBuffer.Width, zBuffer.Height);
                        // note: some oddities were observed where the buffer was not flood filling
                    }
                    else
                    {
                        CardRenderer.DrawPrintLineToGraphics(Graphics.FromImage(zBuffer));
                    }

                    // apply any export rotation
                    ProcessRotateExport(zBuffer, CurrentDeck.CardLayout, false);

                    // before rendering to the PDF bump the DPI to the desired value
                    zBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

                    var xImage = XImage.FromGdiPlusImage(zBuffer);

                    EvaluatePagePosition(nNextExportIndex);

                    m_zPageGfx.DrawImage(xImage, m_dDrawX, m_dDrawY);

                    MoveToNextColumnPosition();

                    // undo any export rotation
                    ProcessRotateExport(zBuffer, CurrentDeck.CardLayout, true);

                    nNextExportIndex++;

                    zWait.ProgressStep(1);
                }
                zWait.ProgressStep(0);
            }

#if !MONO_BUILD
            zBuffer?.Dispose();
#endif

            try
            {
                m_zDocument.Save(m_sExportFile);
                zWait.ThreadSuccess = true;
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Error saving PDF (is it open?) " + ex.Message);
                zWait.ThreadSuccess = false;
            }

            zWait.CloseWaitDialog();
        }

        /// <summary>
        /// Gets the indices to export (order may vary depending on the context)
        /// </summary>
        /// <returns>List of indices to export</returns>
        private IEnumerable<int> GetExportIndices()
        {
            var listExportIndices = new List<int>();
            var nItemsThisRow = GetItemsPerRow(0);

            // if exporting layouts as backs adjust the sequence for each row
            if (CurrentDeck.CardLayout.exportPDFAsPageBack)
            {
                for (var nCardIdx = 0; nCardIdx < CurrentDeck.CardCount; nCardIdx += nItemsThisRow)
                {
                    // juggle the sequence a bit to support printed and glued together (or more risky -- double sided)
                    for (var nSubIdx = (nCardIdx + nItemsThisRow) - 1; nSubIdx >= nCardIdx; nSubIdx--)
                    {
                        if (CardMakerSettings.PrintAutoHorizontalCenter)
                        {
                            // -1 card index is not used for centered + back page
                            if (nSubIdx < CurrentDeck.CardCount)
                            {
                                listExportIndices.Add(nSubIdx);
                            }
                        }
                        else
                        {
                            // trickery, add a -1 item as a placeholder to shift over the back facing card
                            listExportIndices.Add(nSubIdx >= CurrentDeck.CardCount ? -1 : nSubIdx);
                        }
                    }
                }
            }
            // standard left to right, top to bottom export
            else
            {
                for (var nCardIdx = 0; nCardIdx < CurrentDeck.CardCount; nCardIdx++)
                {
                    listExportIndices.Add(nCardIdx);
                }
            }
            return listExportIndices;
        }

        /// <summary>
        /// Centers the draw point based on the number of items that will be drawn to the row from the
        /// current layout
        /// </summary>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        private void CenterLayoutPositionOnNewRow(int nNextExportIndex)
        {
            var dWidth = m_dPageMarginEndX - m_dPageMarginX;
            var nItemsThisRow = (double)GetItemsPerRow(nNextExportIndex);
            // the last horizontal buffer is not counted
            m_dDrawX = m_dPageMarginX + (dWidth -
                         ((nItemsThisRow*m_dLayoutPointWidth) +
                          ((nItemsThisRow - 1) * m_dBufferX)))
                        /2;

        }

        /// <summary>
        /// Updates the point sizes to match that of the PDF exporter
        /// </summary>
        /// <param name="zLayout"></param>
        private void ConfigurePointSizes(ProjectLayout zLayout)
        {
            int nWidth = zLayout.width;
            int nHeight = zLayout.height;
            switch (zLayout.exportRotation)
            {
                case 90:
                case -90:
                    nWidth = zLayout.height;
                    nHeight = zLayout.width;
                    break;
            }

            double dPointsPerInchWidth = (double)m_zCurrentPage.Width / (double)m_zCurrentPage.Width.Inch;
            double dInchesWidthPerLayoutItem = (double)nWidth / (double)zLayout.dpi;
            m_dLayoutPointWidth = dInchesWidthPerLayoutItem * dPointsPerInchWidth;

            double dPointsPerInchHeight = (double)m_zCurrentPage.Height / (double)m_zCurrentPage.Height.Inch;
            double dInchesHeightPerLayoutItem = (double)nHeight / (double)zLayout.dpi;
            m_dLayoutPointHeight = dInchesHeightPerLayoutItem * dPointsPerInchHeight;

            m_dBufferX = ((double)zLayout.buffer / (double)zLayout.dpi) * dPointsPerInchWidth;
            m_dBufferY = ((double)zLayout.buffer / (double)zLayout.dpi) * dPointsPerInchHeight;
        }

        /// <summary>
        /// Evaluates the position based on the next layout draw to determine if the draw point should be moved
        /// to the next row/page. This respects the horizontal centering option when adding a new page.
        /// </summary>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        private void EvaluatePagePosition(int nNextExportIndex)
        {
            // TODO this should probably error or at least warn on truncation (if the object simply won't fit horizontally)
            if (m_dDrawX + m_dLayoutPointWidth > m_dPageMarginEndX)
            {
                MoveToNextRow(nNextExportIndex);
            }

            if (m_dDrawY + m_dLayoutPointHeight > m_dPageMarginEndY)
            {
                AddPage();
                if (CardMakerSettings.PrintAutoHorizontalCenter)
                {
                    CenterLayoutPositionOnNewRow(nNextExportIndex);
                }
            }
        }

        /// <summary>
        /// Adjusts the y position to the next row based on the current layout
        /// respecting the horizontal centering option
        /// </summary>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        private void MoveToNextRow(int nNextExportIndex)
        {
            m_dDrawX = m_dPageMarginX;
            m_dDrawY += m_dNextRowYAdjust;
            if (CardMakerSettings.PrintAutoHorizontalCenter)
            {
                CenterLayoutPositionOnNewRow(nNextExportIndex);
            }
            // reset the row adjust to be that of the current layout
            m_dNextRowYAdjust = m_dLayoutPointHeight + m_dBufferY;
        }

        /// <summary>
        /// Adjusts the x position to the next column based on the current layout
        /// </summary>
        private void MoveToNextColumnPosition()
        {
            m_dDrawX += m_dLayoutPointWidth + m_dBufferX;
        }

        /// <summary>
        /// Adds a new page to the PDF based on the current configuration
        /// </summary>
        private void AddPage()
        {
            m_zCurrentPage = m_zDocument.AddPage();

            UpdatePageSize();

            m_zCurrentPage.Orientation = m_ePageOrientation;

            UpdatePageMargins();

            m_zPageGfx = XGraphics.FromPdfPage(m_zCurrentPage);
            // start within the margin
            m_dDrawX = m_dPageMarginX;
            m_dDrawY = m_dPageMarginY;

            m_dNextRowYAdjust = m_dLayoutPointHeight + m_dBufferY;
        }

        /// <summary>
        /// Updates the page size settings based on the cross application settings (and measurement type)
        /// </summary>
        private void UpdatePageSize()
        {
            switch (CardMakerSettings.PrintPageMeasurementUnit)
            {
                    case MeasurementUnit.Inch:
                        m_zCurrentPage.Width = XUnit.FromInch((double)CardMakerSettings.PrintPageWidth);
                        m_zCurrentPage.Height = XUnit.FromInch((double)CardMakerSettings.PrintPageHeight);
                        break;
                    case MeasurementUnit.Millimeter:
                        m_zCurrentPage.Width = XUnit.FromMillimeter((double)CardMakerSettings.PrintPageWidth);
                        m_zCurrentPage.Height = XUnit.FromMillimeter((double)CardMakerSettings.PrintPageHeight);
                        break;
                    case MeasurementUnit.Centimeter:
                        m_zCurrentPage.Width = XUnit.FromCentimeter((double)CardMakerSettings.PrintPageWidth);
                        m_zCurrentPage.Height = XUnit.FromCentimeter((double)CardMakerSettings.PrintPageHeight);
                        break;
            }
        }

        /// <summary>
        /// Updates the page margins based on the application settings
        /// </summary>
        private void UpdatePageMargins()
        {
            if (m_dPageMarginX != -1)
            {
                return;
            }

            double dPointsPerInchWidth = (double)m_zCurrentPage.Width / (double)m_zCurrentPage.Width.Inch;
            double dPointsPerInchHeight = (double)m_zCurrentPage.Height / (double)m_zCurrentPage.Height.Inch;

            // working in dpi/inches/pixels in the end for consistency (up for argument!)
            switch (CardMakerSettings.PrintPageMeasurementUnit)
            {
                case MeasurementUnit.Inch:
                    m_dPageMarginX = (double)CardMakerSettings.PrintPageHorizontalMargin * dPointsPerInchWidth;
                    m_dPageMarginY = (double)CardMakerSettings.PrintPageVerticalMargin * dPointsPerInchHeight;
                    break;
                case MeasurementUnit.Millimeter:
                    m_dPageMarginX = MeasurementUtil.GetInchesFromMillimeter((double)CardMakerSettings.PrintPageHorizontalMargin) * dPointsPerInchWidth;
                    m_dPageMarginY = MeasurementUtil.GetInchesFromMillimeter((double)CardMakerSettings.PrintPageVerticalMargin) * dPointsPerInchHeight;
                    break;
                case MeasurementUnit.Centimeter:
                    m_dPageMarginX = MeasurementUtil.GetInchesFromCentimeter((double)CardMakerSettings.PrintPageHorizontalMargin) * dPointsPerInchWidth;
                    m_dPageMarginY = MeasurementUtil.GetInchesFromCentimeter((double)CardMakerSettings.PrintPageVerticalMargin) * dPointsPerInchHeight;
                    break;
            }
            m_dPageMarginEndX = m_zCurrentPage.Width - m_dPageMarginX;
            m_dPageMarginEndY = m_zCurrentPage.Height - m_dPageMarginY;
        }

        /// <summary>
        /// Gets the number of items that would fit per row based on the current layout and page
        /// </summary>
        /// <returns>The number of items that would fit in the row</returns>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        private int GetItemsPerRow(int nNextExportIndex)
        {
            // remaining space in row based on layout vs. remaining cards to fit (last row may have fewer than the space would fit)
            return (int)Math.Floor(
                Math.Min(
                    (m_dPageMarginEndX - m_dPageMarginX) / m_dLayoutPointWidth,
                    CurrentDeck.ValidLines.Count - nNextExportIndex)
                );
        }
    }
}
