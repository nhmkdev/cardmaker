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

namespace CardMaker.Card.Export.Pdf
{
    /// <summary>
    /// Row exporting base class. This is necessary as alignment within a row is unique.
    /// </summary>
    public abstract class PdfRowExporter
    {
        protected PdfSharpExportData m_zExportData;

        public PdfRowExporter(PdfSharpExportData zExportData)
        {
            m_zExportData = zExportData;
        }

        /// <summary>
        /// Sets up the current row, adjusting the x value accordingly
        /// </summary>
        public abstract void SetupNewRowXPosition(int nNextExportIndex);

        /// <summary>
        /// Moves the x value to the next draw location. The resulting location may be invalid for drawing.
        /// </summary>
        public abstract void MoveXToNextColumnPosition();
        
        /// <summary>
        /// Determines if the row is full based on the current layout
        /// </summary>
        /// <returns>true if full, false otherwise</returns>
        public abstract bool IsRowFull();

        public virtual bool IsLayoutForcedToNewRow()
        {
            return false;
        }
    }
}
