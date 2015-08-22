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

using CardMaker.Forms;
using CardMaker.XML;
using Support.IO;
using System;
using System.Drawing;
using System.Drawing.Printing;

namespace CardMaker.Card.Export.Print
{
    public abstract class CardPrintBase : CardExportBase, IPrintCardExporter
    {
        protected readonly PrintDocument m_zPrintDocument;
        protected LayoutBound m_zActiveBound = null;
        protected LayoutBound m_zPreviousBound = null;
        protected float m_fPrintScaleDenominator = 1f;
        private int m_nExportLayoutCurrentIndex;
        private bool m_bOnNewPage = false;

        protected CardPrintBase(int nLayoutStartIndex, int nLayoutEndIndex, PrintDocument zPrintDocument)
            : base(nLayoutStartIndex, nLayoutEndIndex)
        {

            m_nExportLayoutStartIndex = nLayoutStartIndex;
            m_nExportLayoutEndIndex = nLayoutEndIndex;

            m_zPrintDocument = zPrintDocument;
            m_nExportLayoutCurrentIndex = m_nExportLayoutStartIndex;
            ChangePrintCardCanvas(m_nExportLayoutCurrentIndex); 

        }

        protected abstract void DrawPrintCard(int nX, int nY, Graphics zGraphics, DeckLine zDeckLine);

        protected virtual PrintLayoutState PrintCardsCentered(PrintPageEventArgs e, ref int nX, ref int nY)
        {
            ResetCenteredLayoutX(ref nX, e);

            while (CurrentDeck.CardPrintIndex < CurrentDeck.ValidLines.Count)
            {
                CurrentDeck.ResetDeckCache(); // each card requires a new cache
                if ((nX + m_zActiveBound.Width) > (e.MarginBounds.X + e.MarginBounds.Width))
                {
                    // move to the next line
                    nY += m_zActiveBound.TotalHeight;
                    nX = e.MarginBounds.X;
                    if ((nY + m_zActiveBound.Height) > (e.MarginBounds.Y + e.MarginBounds.Height))
                    {
                        return PrintLayoutState.StartNewPage;
                    }
                    ResetCenteredLayoutX(ref nX, e);
                }

                DrawPrintCard(nX, nY, e.Graphics, CurrentDeck.CurrentPrintLine);
                nX += m_zActiveBound.TotalWidth;
                CurrentDeck.CardPrintIndex++;
            }

            // always move to the next line if continuing a page / switching layouts
            nY += m_zActiveBound.TotalHeight;
            nX = e.MarginBounds.X;

            return PrintLayoutState.ContinuePage;
        }

        protected virtual PrintLayoutState PrintCards(PrintPageEventArgs e, ref int nX, ref int nY, ProjectLayout zPreviousLayout)
        {
            UpdateLayoutBounds(CurrentDeck.CardLayout, zPreviousLayout, e.PageSettings);

            #region Make into shared method when this is broken into CardPrintCanvas and CardPrintCenteredCanvas
            if (-1 == nX)
            {
                nX = e.MarginBounds.X;
            }
            if (-1 == nY)
            {
                nY = e.MarginBounds.Y;
            }

            // always check y before attempting to draw
            if ((nY + m_zActiveBound.Height) > (e.MarginBounds.Y + e.MarginBounds.Height))
            {
                return PrintLayoutState.StartNewPage;
            }
            #endregion

            if (CardMakerMDI.Instance.PrintAutoHorizontalCenter)
            {
                return PrintCardsCentered(e, ref nX, ref nY);
            }
#warning be sure to get the scaled previous layout buffer/width/height! (TODO: what was this in reference to?)
            if (null != zPreviousLayout)
            {
                // check if there is space in the remaining horizontal area for the layout to fit
                if ((nX + m_zActiveBound.Width + Math.Max(m_zActiveBound.Buffer, m_zPreviousBound.Buffer)) <= (e.MarginBounds.X + e.MarginBounds.Width) &&
                    (m_zActiveBound.Height + Math.Max(m_zActiveBound.Buffer, m_zPreviousBound.Buffer)) <= m_zPreviousBound.Height)
                {
                    // expand the buffer area, though only as much as is required based on the current and previous buffer settings
                    var boundX = nX + Math.Max(m_zActiveBound.Buffer, m_zPreviousBound.Buffer);
                    var boundY = nY + Math.Max(m_zActiveBound.Buffer, m_zPreviousBound.Buffer);

                    // allow one level of nesting, then just move onto the next line
                    PrintWithinBoundary(e.Graphics, boundX, boundY, (e.MarginBounds.X + e.MarginBounds.Width) - boundX, m_zPreviousBound.Height);
                    nY += m_zPreviousBound.TotalHeight;
                    nX = e.MarginBounds.X;
                    if ((nY + m_zActiveBound.Height) > (e.MarginBounds.Y + e.MarginBounds.Height))
                    {
                        return PrintLayoutState.StartNewPage;
                    }
                }

                // if the last layout did not leave enough space move to the next line
                if ((nX + m_zActiveBound.Width) > (e.MarginBounds.X + e.MarginBounds.Width))
                {
                    nY += m_zPreviousBound.TotalHeight;
                    nX = e.MarginBounds.X;
                    if ((nY + m_zActiveBound.Height) > (e.MarginBounds.Y + e.MarginBounds.Height))
                    {
                        return PrintLayoutState.StartNewPage;
                    }
                }
            }

            while (CurrentDeck.CardPrintIndex < CurrentDeck.ValidLines.Count)
            {
                CurrentDeck.ResetDeckCache(); // each card requires a new cache
                if ((nX + m_zActiveBound.Width) > (e.MarginBounds.X + e.MarginBounds.Width))
                {
                    // move to the next line
                    nY += m_zActiveBound.TotalHeight;
                    nX = e.MarginBounds.X;
                    if ((nY + m_zActiveBound.Height) > (e.MarginBounds.Y + e.MarginBounds.Height))
                    {
                        return PrintLayoutState.StartNewPage;
                    }
                }

                DrawPrintCard(nX, nY, e.Graphics, CurrentDeck.CurrentPrintLine);
                nX += m_zActiveBound.TotalWidth;
                // NOTE: nY does not increment until the next call to this method (on break of this loop)
                CurrentDeck.CardPrintIndex++;
            }

            return PrintLayoutState.ContinuePage;
        }

        protected virtual PrintLayoutState PrintWithinBoundary(Graphics zGraphics, int nXBound, int nYBound, int nWidth, int nHeight)
        {
            var nX = nXBound;
            var nY = nYBound;
            // always check y before attempting to draw
            if ((nY + m_zActiveBound.Height) > (nYBound + nHeight))
            {
                return PrintLayoutState.StartNewPage;
            }

            while (CurrentDeck.CardPrintIndex < CurrentDeck.ValidLines.Count)
            {
                CurrentDeck.ResetDeckCache(); // each card requires a new cache
                if ((nX + m_zActiveBound.Width) > (nXBound + nWidth))
                {
                    // move to the next line
                    nY += m_zActiveBound.TotalHeight;
                    nX = nXBound;
                    if ((nY + m_zActiveBound.Height) > (nYBound + nHeight))
                    {
                        return PrintLayoutState.StartNewPage;
                    }
                }
                DrawPrintCard(nX, nY, zGraphics, CurrentDeck.CurrentPrintLine);
                nX += m_zActiveBound.TotalWidth;
                CurrentDeck.CardPrintIndex++;
            }

            // note - x/y are maintained by caller

            return PrintLayoutState.ContinuePage;
        }

        protected virtual void ResetCenteredLayoutX(ref int nX, PrintPageEventArgs e)
        {
            var nItemsThisRow = Math.Min(
                (e.MarginBounds.Width + CurrentDeck.CardLayout.buffer) / (CurrentDeck.CardLayout.width + CurrentDeck.CardLayout.buffer),
                CurrentDeck.ValidLines.Count - CurrentDeck.CardPrintIndex);
            nX += (e.MarginBounds.Width -
                ((nItemsThisRow * CurrentDeck.CardLayout.width) +
                ((nItemsThisRow - 1) * CurrentDeck.CardLayout.buffer)))
                / 2;
        }

        protected virtual void UpdateLayoutBounds(ProjectLayout zLayout, ProjectLayout zPreviousLayout, PageSettings zPageSettings)
        {
            if (null == m_zActiveBound || zLayout != m_zActiveBound.Layout)
            {
                m_zActiveBound = new LayoutBound(zLayout, m_fPrintScaleDenominator);
            }
            if (null != zPreviousLayout && (null == m_zPreviousBound || zPreviousLayout != m_zPreviousBound.Layout))
            {
                m_zPreviousBound = new LayoutBound(zPreviousLayout, m_fPrintScaleDenominator);
            }
        }

        protected virtual void PrintLoop(PrintPageEventArgs e, int nX, int nY, ProjectLayout zPreviousLayout)
        {
            int nPreviousIndex = -1;
            if (-1 == nX) // only check the index when attempting to print on a new page (if nothing can be printed on a single page things aren't right)
            {
                nPreviousIndex = CurrentDeck.CardPrintIndex;
            }
            switch (PrintCards(e, ref nX, ref nY, zPreviousLayout))
            {
                case PrintLayoutState.ContinuePage:
                    if (CardMakerMDI.Instance.PrintLayoutsOnNewPage && !m_bOnNewPage &&
                        m_nExportLayoutCurrentIndex + 1 < m_nExportLayoutEndIndex)
                    {
                        m_bOnNewPage = true;
                        e.HasMorePages = true;
                        return;                        
                    }
                    m_bOnNewPage = false;
                    m_nExportLayoutCurrentIndex++;
                    if (m_nExportLayoutCurrentIndex < m_nExportLayoutEndIndex)
                    {
                        ProjectLayout zLastLayout = CurrentDeck.CardLayout;
                        ChangePrintCardCanvas(m_nExportLayoutCurrentIndex);
                        PrintLoop(e, nX, nY, zLastLayout);
                    }
                    break;
                case PrintLayoutState.StartNewPage:
                    if (nPreviousIndex == CurrentDeck.CardPrintIndex)
                    {
                        //ShowErrorMessage("You're trying to print something larger than a page can handle!");
                        Logger.AddLogLine("You're trying to print something larger than a page can handle!");
                        e.HasMorePages = false;
                        return;
                    }
                    e.HasMorePages = true;
                    return;
            }
        }

        protected virtual void UpdateOptimalPageOrientation(QueryPageSettingsEventArgs e)
        {
            UpdateLayoutBounds(CurrentDeck.CardLayout, null, e.PageSettings);
            e.PageSettings.Landscape = false;
            int nPrintAreaWidth = e.PageSettings.Bounds.Width - (e.PageSettings.Margins.Left * 2);
            int nPrintAreaHeight = e.PageSettings.Bounds.Height - (e.PageSettings.Margins.Top * 2);
            int n1 = (nPrintAreaWidth + m_zActiveBound.Buffer) / (m_zActiveBound.TotalWidth);
            int n2 = (nPrintAreaHeight + m_zActiveBound.Buffer) / (m_zActiveBound.TotalHeight);
            int n3 = (nPrintAreaWidth + m_zActiveBound.Buffer) / (m_zActiveBound.TotalHeight);
            int n4 = (nPrintAreaHeight + m_zActiveBound.Buffer) / (m_zActiveBound.TotalWidth);
            e.PageSettings.Landscape = (n1 * n2) < (n3 * n4); // (portrait < landscape)

            // flip the margins if necessary (they aren't applied automatically on swap)
            e.PageSettings.Margins.Left = e.PageSettings.Margins.Right =
                e.PageSettings.Landscape ? m_zPrintDocument.DefaultPageSettings.Margins.Top : m_zPrintDocument.DefaultPageSettings.Margins.Left;
            e.PageSettings.Margins.Top = e.PageSettings.Margins.Bottom =
                e.PageSettings.Landscape ? m_zPrintDocument.DefaultPageSettings.Margins.Left : m_zPrintDocument.DefaultPageSettings.Margins.Top;
        }

        #region IPrintCardExporter

        public void PrintPage(object sender, PrintPageEventArgs e)
        {
            e.HasMorePages = false;
            PrintLoop(e, -1, -1, null);
        }

        public void QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            UpdateOptimalPageOrientation(e);
        }

        public virtual void OnComplete(object sender) { }

        #endregion

        protected class LayoutBound
        {
            public int TotalWidth { get; private set; }
            public int TotalHeight { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Buffer { get; private set; }
            public ProjectLayout Layout { get; private set; }

            public LayoutBound(ProjectLayout zLayout, float fDenominator)
            {
                Width = (int)((float)zLayout.width / fDenominator);
                Height = (int)((float)zLayout.height / fDenominator);
                Buffer = (int)((float)zLayout.buffer / fDenominator);
                TotalWidth = Width + Buffer;
                TotalHeight = Height + Buffer;
                Layout = zLayout;
            }
        }
    }
}
