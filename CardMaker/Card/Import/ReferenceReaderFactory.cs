////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2020 Tim Stair
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

using CardMaker.Data;
using CardMaker.XML;
using Support.Progress;

namespace CardMaker.Card.Import
{
    public static class ReferenceReaderFactory
    {
        /// <summary>
        /// Gets the reference reader based on the reference relative path.
        /// NOTE: this uses a constructor that passes in the reference
        /// </summary>
        /// <param name="zReference">The reference to get the reader for</param>
        /// <param name="zProgressReporter">ProgressReporter for the Reader to use</param>
        /// <returns>Reference reader (defaults to CSV)</returns>
        public static ReferenceReader GetReader(ProjectLayoutReference zReference, IProgressReporter zProgressReporter)
        {
            if (zReference == null)
            {
                return null;
            }
            ReferenceReader zReferenceReader = null;
            if (zReference.RelativePath.StartsWith(GoogleSpreadsheetReference.GOOGLE_REFERENCE +
                                                           GoogleSpreadsheetReference.GOOGLE_REFERENCE_SPLIT_CHAR))
            {
                zReferenceReader = new GoogleReferenceReader(zReference);
            }
            if (zReference.RelativePath.StartsWith(ExcelSpreadsheetReference.EXCEL_REFERENCE +
                                                           ExcelSpreadsheetReference.EXCEL_REFERENCE_SPLIT_CHAR))
            {
                zReferenceReader = new ExcelReferenceReader(zReference);
            }
            if (null == zReferenceReader)
            {
                zReferenceReader = new CSVReferenceReader(zReference);
            }

            zReferenceReader.ProgressReporter = zProgressReporter;
            return zReferenceReader.Initialize();
        }

        /// <summary>
        /// Gets the reference reader based on the type
        /// </summary>
        /// <param name="eReferenceType">The type of reference to get the reader for</param>
        /// <param name="zProgressReporter">ProgressReporter for the Reader to use</param>
        /// <returns>Reference reader (defaults to null)</returns>
        public static ReferenceReader GetDefineReader(ReferenceType eReferenceType, IProgressReporter zProgressReporter)
        {
            ReferenceReader zReferenceReader = null;
            switch (eReferenceType)
            {
                case ReferenceType.CSV:
                    zReferenceReader = new CSVReferenceReader();
                    break;
                case ReferenceType.Google:
                    zReferenceReader = new GoogleReferenceReader();
                    break;
                case ReferenceType.Excel:
                    zReferenceReader = new ExcelReferenceReader();
                    break;
            }

            if (zReferenceReader != null)
            {
                zReferenceReader.ProgressReporter = zProgressReporter;
            }
            return zReferenceReader.Initialize();
        }            
    }
}
