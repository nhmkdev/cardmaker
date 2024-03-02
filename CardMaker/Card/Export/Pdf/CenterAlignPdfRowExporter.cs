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

namespace CardMaker.Card.Export.Pdf
{
    /// <summary>
    /// Draws the row with layouts centered
    /// </summary>
    public class CenterAlignPdfRowExporter : LeftAlignPdfRowExporter
    {
        public CenterAlignPdfRowExporter(PdfSharpExportData zExportData) : base(zExportData)
        {
        }

        public override bool IsLayoutForcedToNewRow()
        {
            return true;
        }

        /// <summary>
        /// Centers the draw point based on the number of items that will be drawn to the row from the
        /// current layout
        /// </summary>
        /// <param name="nNextExportIndex">The next index of the item to be exported (0-X counter)</param>
        public override void SetupNewRowXPosition(int nNextExportIndex)
        {
            var dWidth = m_zExportData.PageMarginEndX - m_zExportData.PageMarginX;
            var nItemsThisRow = (double)m_zExportData.GetItemsPerRow(nNextExportIndex);
            // the last horizontal buffer is not counted
            m_zExportData.DrawX = m_zExportData.PageMarginX + (dWidth -
                                                               ((nItemsThisRow * m_zExportData.LayoutPointWidth) +
                                                                ((nItemsThisRow - 1) * m_zExportData.BufferX)))
                / 2;

        }
    }
}
