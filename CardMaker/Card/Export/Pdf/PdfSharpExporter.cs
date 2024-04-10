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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CardMaker.Data;
using CardMaker.XML;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Export.Pdf
{
    /// <summary>
    /// PdfSharp wrapper for exporting cards. This is based heavily on inches due to the use of DPI. CM/MM are supported but converted.
    /// </summary>
    public class PdfSharpExporter : CardExportBase
    {
        private readonly PdfSharpExportData m_zExportData = new PdfSharpExportData();
        private readonly PdfDocument m_zDocument;
        private PdfPage m_zCurrentPage;
        private XGraphics m_zPageGfx;
        private readonly string m_sExportFile;
        public int[] ExportCardIndices { get; set; }

        private readonly PageOrientation m_ePageOrientation = PageOrientation.Portrait;

        public PdfSharpExporter(int nLayoutStartIndex, int nLayoutEndIndex, string sExportFile, string sPageOrientation) : 
            this(Enumerable.Range(nLayoutStartIndex, nLayoutEndIndex - nLayoutStartIndex).ToArray(), sExportFile, sPageOrientation)
        {
        }

        public PdfSharpExporter(int[] arrayLayoutIndices, string sExportFile, string sPageOrientation) : base(arrayLayoutIndices)
        {
            m_sExportFile = sExportFile;
            try
            {
                m_ePageOrientation = (PageOrientation)Enum.Parse(typeof(PageOrientation), sPageOrientation);
            }
            catch (Exception)
            {
                ProgressReporter.AddIssue(sPageOrientation + " is an unknown page orientation.");
            }
            m_zDocument = new PdfDocument();
        }

        public PdfSharpExporter(int[] arrayLayoutIndices, string sExportFile, PageOrientation ePageOrientation) : base(arrayLayoutIndices)
        {
            m_sExportFile = sExportFile;
            m_ePageOrientation = ePageOrientation;
            m_zDocument = new PdfDocument();
        }

        public override void ExportThread()
        {
            if (File.Exists(m_sExportFile))
            {
                try
                {
                    File.Delete(m_sExportFile);
                }
                catch (Exception)
                {
                    ProgressReporter.AddIssue("Failed to delete PDF before export: {0}".FormatString(m_sExportFile));
                }

                if (File.Exists(m_sExportFile))
                {
                    DisplayReadOnlyError();
                    ProgressReporter.Shutdown();
                    return;
                }
            }

#if !MONO_BUILD
            Bitmap zBuffer = null;
#endif

            var progressLayoutIdx = ProgressReporter.GetProgressIndex(ProgressName.LAYOUT);
            var progressCardIdx = ProgressReporter.GetProgressIndex(ProgressName.CARD);

            ProgressReporter.ProgressReset(progressLayoutIdx, 0, ExportLayoutIndices.Length, 0);

            // always add a new page initially (necessary for ConfigurePointSizes)
            AddInitialPage();

            Type zLastExporterType = null;

            for(var nExportCounter = 0; nExportCounter < ExportLayoutIndices.Length; nExportCounter++)
            {
                var nIdx = ExportLayoutIndices[nExportCounter];
                ChangeExportLayoutIndex(nIdx);
                m_zExportData.Deck = CurrentDeck;
                if (CurrentDeck.EmptyReference)
                {
                    // empty reference layouts are not exported
                    ProgressReporter.ProgressStep(progressLayoutIdx);
                    continue;
                }

                ProgressReporter.ProgressReset(progressCardIdx, 0, CurrentDeck.CardCount, 0);

                var rectCrop = CurrentDeck.CardLayout.getExportCropDefinition();

                // each layout has its own row export style (potentially)
                var zRowExporter = GetRowExporter();

                // necessary tracking for which index of the layout is to be exported (this is NOT necessarily the index of the card due to the page back functionality)
                var nNextExportIndex = 0;

                // reconfigure the page per layout
                if (!ConfigurePointSizes(CurrentDeck.CardLayout, rectCrop))
                {
                    DisplayError("Unable to setup page size. See log.");
                    ProgressReporter.ThreadSuccess = false;

                    ProgressReporter.Shutdown();
                    return;
                }
                
                if (nExportCounter == 0)
                {
                    // only the page has been setup at this point, not the x-position
                    zRowExporter.SetupNewRowXPosition(nNextExportIndex);
                }
                else
                {
                    // after the first layout always evaluate whether a new page should be added
                    // if the exporter type changes start a new page
                    if (CardMakerSettings.PrintLayoutsOnNewPage || zLastExporterType != zRowExporter.GetType())
                    {
                        AddPage(zRowExporter, nNextExportIndex);
                    }
                    else if (zRowExporter.IsLayoutForcedToNewRow() || zRowExporter.IsRowFull())
                    {
                        MoveToNextRow(zRowExporter, nNextExportIndex);
                    }
                }

                zLastExporterType = zRowExporter.GetType();

                // When moving to the next row it should be a completely empty row. In the case of nesting only 2 layouts deep is supported at this
                // time - the first layout and one additional. Also note this is applied AFTER the above decisions above about moving to the next row.
                m_zExportData.NextRowYAdjust = Math.Max(m_zExportData.NextRowYAdjust, m_zExportData.LayoutPointHeight + m_zExportData.BufferY);

#if !MONO_BUILD
                zBuffer?.Dispose();
                zBuffer = createExportBuffer(CurrentDeck.CardLayout, rectCrop);

                var fOriginalXDpi = zBuffer.HorizontalResolution;
                var fOriginalYDpi = zBuffer.VerticalResolution;
#endif

                foreach (var nCardIdx in GetExportIndices(ExportCardIndices))
                {
                    CurrentDeck.CardPrintIndex = nCardIdx;

#if MONO_BUILD
                    // mono build won't support the optimization so re-create the buffer
                    Bitmap zBuffer = createExportBuffer(CurrentDeck.CardLayout, rectCrop);
#else
                    // minor optimization, reuse the same bitmap (for drawing sake the DPI has to be reset)
                    zBuffer.SetResolution(fOriginalXDpi, fOriginalYDpi);
#endif
                    var bRenderCard = true;
                    // Draw the image into the buffer
                    if(nCardIdx == -1)
                    {
                        Graphics.FromImage(zBuffer).FillRectangle(Brushes.White, 0, 0, zBuffer.Width, zBuffer.Height);
                        // note: some oddities were observed where the buffer was not flood filling
                    }
                    else
                    {
                        bRenderCard = CardRenderer.DrawPrintLineToGraphics(Graphics.FromImage(zBuffer), -rectCrop.X, -rectCrop.Y, true);
                        // if cropping the border needs to be drawn to the cropped size
                        if (bRenderCard && rectCrop != Rectangle.Empty)
                            CardRenderer.DrawBorder(Graphics.FromImage(zBuffer), 0, 0, zBuffer.Width, zBuffer.Height, CurrentDeck.CardLayout, true);
                    }

                    if (bRenderCard)
                    {
                        // apply any export rotation
                        ProcessRotateExport(zBuffer, CurrentDeck.CardLayout, false);

                        // before rendering to the PDF bump the DPI to the desired value
                        zBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

                        var xImage = XImage.FromGdiPlusImage(zBuffer);

                        // before drawing make sure there is space (will move to next row/page)
                        EvaluateDrawLocation(nNextExportIndex, zRowExporter);

                        m_zPageGfx.DrawImage(xImage, m_zExportData.DrawX, m_zExportData.DrawY);

                        // any next row movement is dealt with independent of this call
                        zRowExporter.MoveXToNextColumnPosition();

                        // undo any export rotation
                        ProcessRotateExport(zBuffer, CurrentDeck.CardLayout, true);
                    }

                    nNextExportIndex++;

                    ProgressReporter.ProgressStep(progressCardIdx);
                }
                ProgressReporter.ProgressStep(progressLayoutIdx);
            }

#if !MONO_BUILD
            zBuffer?.Dispose();
#endif

            try
            {
                m_zDocument.Save(m_sExportFile);
                ProgressReporter.ThreadSuccess = true;
            }
            catch (Exception ex)
            {
                DisplayReadOnlyError(ex.Message);
                ProgressReporter.ThreadSuccess = false;
            }

            ProgressReporter.Shutdown();
        }

        /// <summary>
        /// Gets the indices to export (order may vary depending on the context)
        /// </summary>
        /// <returns>List of indices to export</returns>
        private IEnumerable<int> GetExportIndices(int[] arrayOverrideIndices)
        {
            var nItemsThisRow = m_zExportData.GetItemsPerRow(0);

            var arrayCardIndicies = GetCardIndicesArray(CurrentDeck, arrayOverrideIndices).ToList();

            // if exporting layouts as backs adjust the sequence for each row
            if (CurrentDeck.CardLayout.exportPDFAsPageBack && CardMakerSettings.PrintAutoHorizontalCenter)
            {
                var listExportIndices = new List<int>();
                for (var nCardIdx = 0; nCardIdx < arrayCardIndicies.Count; nCardIdx += nItemsThisRow)
                {
                    // juggle the sequence a bit to support printed and glued together (or more risky -- double sided)
                    for (var nSubIdx = (nCardIdx + nItemsThisRow) - 1; nSubIdx >= nCardIdx; nSubIdx--)
                    {
                        if (nSubIdx < arrayCardIndicies.Count)
                        {
                            listExportIndices.Add(nSubIdx);
                        }
                    }
                }
                return listExportIndices;
            }
            return arrayCardIndicies;
        }

        /// <summary>
        /// Updates the point sizes to match that of the PDF exporter
        /// </summary>
        /// <param name="zLayout"></param>
        /// <param name="rectCrop"></param>
        private bool ConfigurePointSizes(ProjectLayout zLayout, Rectangle rectCrop)
        {
            var nWidth = rectCrop.Width == 0 ? zLayout.width : rectCrop.Width;
            var nHeight = rectCrop.Height == 0 ? zLayout.height : rectCrop.Height;
            switch (zLayout.exportRotation)
            {
                case 90:
                case -90:
                    var nTempWidth = nWidth;
                    var nTempHeight = nHeight;
                    nWidth = nTempHeight;
                    nHeight = nTempWidth;
                    break;
            }

            var dPointsPerInchWidth = (double)m_zCurrentPage.Width / (double)m_zCurrentPage.Width.Inch;
            var dInchesWidthPerLayoutItem = (double)nWidth / (double)zLayout.dpi;
            m_zExportData.LayoutPointWidth = dInchesWidthPerLayoutItem * dPointsPerInchWidth;

            var dPointsPerInchHeight = (double)m_zCurrentPage.Height / (double)m_zCurrentPage.Height.Inch;
            var dInchesHeightPerLayoutItem = (double)nHeight / (double)zLayout.dpi;
            m_zExportData.LayoutPointHeight = dInchesHeightPerLayoutItem * dPointsPerInchHeight;

            m_zExportData.BufferX = ((double)zLayout.buffer / (double)zLayout.dpi) * dPointsPerInchWidth;
            m_zExportData.BufferY = ((double)zLayout.buffer / (double)zLayout.dpi) * dPointsPerInchHeight;

            var dAvailableWidth = m_zCurrentPage.Width.Point - (m_zExportData.PageMarginX * 2);
            var dAvailableHeight = m_zCurrentPage.Height.Point - (m_zExportData.PageMarginY * 2);
            var bRet = true;
            if (m_zExportData.LayoutPointWidth > dAvailableWidth)
            {
                Logger.AddLogLine("The layout width {0} will not fit the available page width {1} (values are point sizes). Please reconfigure the page.".FormatString(m_zExportData.LayoutPointWidth,
                    dAvailableWidth));
                bRet = false;
            }

            if (m_zExportData.LayoutPointHeight > dAvailableHeight)
            {
                Logger.AddLogLine("The layout height {0} will not fit the available page height {1} (values are point sizes). Please reconfigure the page.".FormatString(m_zExportData.LayoutPointHeight,
                    dAvailableHeight));
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// Evaluates the position based on the next layout draw to determine if the draw point should be moved
        /// to the next row/page. This respects the horizontal centering option when adding a new page.
        /// </summary>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        private void EvaluateDrawLocation(int nNextExportIndex, PdfRowExporter zRowExporter)
        {
#warning TODO this should probably error or at least warn on truncation (if the object simply won't fit horizontally)
            if (zRowExporter.IsRowFull())
            {
                MoveToNextRow(zRowExporter, nNextExportIndex);
            }

            // if the draw will extend beyond the bottom margin start a new page
            if (m_zExportData.DrawY + m_zExportData.LayoutPointHeight > m_zExportData.PageMarginEndY)
            {
                AddPage(zRowExporter, nNextExportIndex);
            }
        }

        /// <summary>
        /// Adjusts the y position to the next row based on the current layout
        /// respecting the horizontal centering option
        /// </summary>
        /// <param name="zRowExporter"></param>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        private void MoveToNextRow(PdfRowExporter zRowExporter, int nNextExportIndex)
        {
            zRowExporter.SetupNewRowXPosition(nNextExportIndex);
            m_zExportData.DrawY += m_zExportData.NextRowYAdjust;
            // reset the row adjust to be that of the current layout
#warning    (this likely happens more often than needed, should only apply on layout changes)
            m_zExportData.NextRowYAdjust = m_zExportData.LayoutPointHeight + m_zExportData.BufferY;
        }

        /// <summary>
        /// Adds a new page to the PDF based on the current configuration
        /// </summary>
        private void AddInitialPage()
        {
            m_zCurrentPage = m_zDocument.AddPage();

            UpdatePageSize();
            m_zCurrentPage.Orientation = m_ePageOrientation;
            UpdatePageMargins();

            m_zPageGfx = XGraphics.FromPdfPage(m_zCurrentPage);
            m_zExportData.DrawY = m_zExportData.PageMarginY;
        }

        /// <summary>
        /// Adds a new page to the PDF based on the current configuration
        /// </summary>
        /// <param name="zRowExporter">The row exporter to setup the x position with</param>
        /// <param name="nNextExportIndex">Will be used to setup the x position</param>
        private void AddPage(PdfRowExporter zRowExporter, int nNextExportIndex)
        {
            m_zCurrentPage = m_zDocument.AddPage();

            UpdatePageSize();
            m_zCurrentPage.Orientation = m_ePageOrientation;

            m_zPageGfx = XGraphics.FromPdfPage(m_zCurrentPage);
            m_zExportData.DrawY = m_zExportData.PageMarginY;

            zRowExporter.SetupNewRowXPosition(nNextExportIndex);
            m_zExportData.NextRowYAdjust = m_zExportData.LayoutPointHeight + m_zExportData.BufferY;
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
            var dPointsPerInchWidth = (double)m_zCurrentPage.Width / (double)m_zCurrentPage.Width.Inch;
            var dPointsPerInchHeight = (double)m_zCurrentPage.Height / (double)m_zCurrentPage.Height.Inch;

            // working in dpi/inches/pixels in the end for consistency (up for argument!)
            switch (CardMakerSettings.PrintPageMeasurementUnit)
            {
                case MeasurementUnit.Inch:
                    m_zExportData.PageMarginX = (double)CardMakerSettings.PrintPageHorizontalMargin * dPointsPerInchWidth;
                    m_zExportData.PageMarginY = (double)CardMakerSettings.PrintPageVerticalMargin * dPointsPerInchHeight;
                    break;
                case MeasurementUnit.Millimeter:
                    m_zExportData.PageMarginX = MeasurementUtil.GetInchesFromMillimeter((double)CardMakerSettings.PrintPageHorizontalMargin) * dPointsPerInchWidth;
                    m_zExportData.PageMarginY = MeasurementUtil.GetInchesFromMillimeter((double)CardMakerSettings.PrintPageVerticalMargin) * dPointsPerInchHeight;
                    break;
                case MeasurementUnit.Centimeter:
                    m_zExportData.PageMarginX = MeasurementUtil.GetInchesFromCentimeter((double)CardMakerSettings.PrintPageHorizontalMargin) * dPointsPerInchWidth;
                    m_zExportData.PageMarginY = MeasurementUtil.GetInchesFromCentimeter((double)CardMakerSettings.PrintPageVerticalMargin) * dPointsPerInchHeight;
                    break;
            }
            m_zExportData.PageMarginEndX = m_zCurrentPage.Width - m_zExportData.PageMarginX;
            m_zExportData.PageMarginEndY = m_zCurrentPage.Height - m_zExportData.PageMarginY;
        }

        /// <summary>
        /// Creates a new export buffer bitmap based on the layout and any cropping rect
        /// </summary>
        /// <param name="zLayout">The layout to use as a basis for the buffer creation</param>
        /// <param name="rectCrop">The rectangle to use for cropping</param>
        /// <returns></returns>
        private static Bitmap createExportBuffer(ProjectLayout zLayout, Rectangle rectCrop)
        {
            return new Bitmap(
                rectCrop.Width == 0 ? zLayout.width : rectCrop.Width,
                rectCrop.Height == 0 ? zLayout.height : rectCrop.Height);
        }

        /// <summary>
        /// Shows an error for this export
        /// </summary>
        private void DisplayError(string sErrorMessage)
        {
            if (null != CardMakerInstance.ApplicationForm)
            {
                CardMakerInstance.ApplicationForm.InvokeAction(() =>
                {
                    MessageBox.Show(CardMakerInstance.ApplicationForm, sErrorMessage, "PDF Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            else
            {
                ProgressReporter.AddIssue(sErrorMessage);
            }
        }

        /// <summary>
        /// Shows the read-only error (assumes the file is open in a viewer)
        /// </summary>
        private void DisplayReadOnlyError(string extraMessage = "")
        {
            var sMsg = "{0}The destination file may be open in a PDF viewer. Please close it before exporting."
                .FormatString(
                    (string.IsNullOrWhiteSpace(extraMessage)
                        ? ""
                        : extraMessage + " -- "
                    ));
            if (null != CardMakerInstance.ApplicationForm)
            {
                CardMakerInstance.ApplicationForm.InvokeAction(() =>
                {
                    MessageBox.Show(CardMakerInstance.ApplicationForm, sMsg, "PDF Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            else
            {
                ProgressReporter.AddIssue(sMsg);
            }
        }

        private PdfRowExporter GetRowExporter()
        {
            if (CardMakerSettings.PrintAutoHorizontalCenter)
            {
                return new CenterAlignPdfRowExporter(m_zExportData);
            }
            else if (CurrentDeck.CardLayout.exportPDFAsPageBack)
            {
                return new RightAlignPdfRowExporter(m_zExportData);
            }
            return new LeftAlignPdfRowExporter(m_zExportData);
        }
    }
}
