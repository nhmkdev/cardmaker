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
using CardMaker.Data;
using CardMaker.XML;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Export
{
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

        private double m_dMarginX = -1;
        private double m_dMarginEndX = -1;
        private double m_dMarginY = -1;
        private double m_dMarginEndY = -1;
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

                if (0 < nIdx)
                {
                    if (CardMakerSettings.PrintLayoutsOnNewPage)
                    {
                        AddPage();    
                    }

                    if (CardMakerSettings.PrintAutoHorizontalCenter || 
                        (m_dDrawX + m_dLayoutPointWidth > m_dMarginEndX)) // this is the case where a layout won't fit in the remaining space of the row
                    {
                        MoveToNextRow();
                    }
                }

                // should be adjusted after the above move to next row
                m_dNextRowYAdjust = Math.Max(m_dNextRowYAdjust, m_dLayoutPointHeight + m_dBufferY);

                if (CardMakerSettings.PrintAutoHorizontalCenter)
                {
                    CenterLayoutPositionOnNewRow();
                }

#if !MONO_BUILD
                if (null != zBuffer)
                {
                    zBuffer.Dispose();
                }
                zBuffer = new Bitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);

                float fOriginalXDpi = zBuffer.HorizontalResolution;
                float fOriginalYDpi = zBuffer.VerticalResolution;
#endif

                for (var nCardIdx = 0; nCardIdx < CurrentDeck.CardCount; nCardIdx++)
                {
                    CurrentDeck.ResetDeckCache();
                    CurrentDeck.CardPrintIndex = nCardIdx;

                    // minor optimization, reuse the same bitmap (for drawing sake the DPI has to be reset)
#if MONO_BUILD
                    Bitmap zBuffer = new Bitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);
#endif

#if !MONO_BUILD
                    zBuffer.SetResolution(fOriginalXDpi, fOriginalYDpi);
#endif

                    CardRenderer.DrawPrintLineToGraphics(Graphics.FromImage(zBuffer));

                    // apply any export rotation
                    ProcessRotateExport(zBuffer, CurrentDeck.CardLayout, false);

                    // before rendering to the PDF bump the DPI to the desired value
                    zBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

                    var xImage = XImage.FromGdiPlusImage(zBuffer);

                    EvaluatePagePosition();

                    m_zPageGfx.DrawImage(xImage, m_dDrawX, m_dDrawY);

                    MoveToNextColumnPosition();

                    // undo any export rotation
                    ProcessRotateExport(zBuffer, CurrentDeck.CardLayout, true);

                    zWait.ProgressStep(1);
                }
                zWait.ProgressStep(0);
            }

#if !MONO_BUILD
            if (null != zBuffer)
            {
                zBuffer.Dispose();
            }
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
        /// Centers the draw point based on the number of items that will be drawn to the row from the
        /// current layout
        /// </summary>
        private void CenterLayoutPositionOnNewRow()
        {
            var dWidth = m_dMarginEndX - m_dMarginX;
            var nItemsThisRow = Math.Floor(Math.Min(
                dWidth / m_dLayoutPointWidth,
                CurrentDeck.ValidLines.Count - CurrentDeck.CardPrintIndex));
            // the last horizontal buffer is not counted
            m_dDrawX = m_dMarginX + (dWidth -
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
        private void EvaluatePagePosition()
        {
            // TODO this should probably error or at least warn on truncation (if the object simply won't fit horizontally)
            if (m_dDrawX + m_dLayoutPointWidth > m_dMarginEndX)
            {
                MoveToNextRow();
            }

            if (m_dDrawY + m_dLayoutPointHeight > m_dMarginEndY)
            {
                AddPage();
                if (CardMakerSettings.PrintAutoHorizontalCenter)
                {
                    CenterLayoutPositionOnNewRow();
                }
            }
        }

        /// <summary>
        /// Adjusts the y position to the next row based on the current layout
        /// respecting the horizontal centering option
        /// </summary>
        private void MoveToNextRow()
        {
            m_dDrawX = m_dMarginX;
            m_dDrawY += m_dNextRowYAdjust;
            if (CardMakerSettings.PrintAutoHorizontalCenter)
            {
                CenterLayoutPositionOnNewRow();
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

            m_zCurrentPage.Width = XUnit.FromInch((double)CardMakerSettings.PrintPageWidth);
            m_zCurrentPage.Height = XUnit.FromInch((double)CardMakerSettings.PrintPageHeight);

            m_zCurrentPage.Orientation = m_ePageOrientation;

            if (m_dMarginX == -1)
            {
                double dPointsPerInchWidth = (double)m_zCurrentPage.Width / (double)m_zCurrentPage.Width.Inch;
                double dPointsPerInchHeight = (double)m_zCurrentPage.Height / (double)m_zCurrentPage.Height.Inch;
                m_dMarginX = (double)CardMakerSettings.PrintPageHorizontalMargin * dPointsPerInchWidth;
                m_dMarginEndX = m_zCurrentPage.Width - m_dMarginX;
                m_dMarginY = (double)CardMakerSettings.PrintPageVerticalMargin * dPointsPerInchHeight;
                m_dMarginEndY = m_zCurrentPage.Height - m_dMarginY;
            }

            m_zPageGfx = XGraphics.FromPdfPage(m_zCurrentPage);
            m_dDrawX = m_dMarginX;
            m_dDrawY = m_dMarginY;

            m_dNextRowYAdjust = m_dLayoutPointHeight + m_dBufferY;
        }
    }
}
