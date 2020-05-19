////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2019 Tim Stair
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
#warning unify these lookups in both methods... static type dictionary or something... this is bleh
        public static ReferenceReader GetReader(ProjectLayoutReference zReference, IProgressReporter zProgressReporter)
        {
            if (zReference == null)
            {
                return null;
            }

            ReferenceReader zReader = null;

            if (zReference.RelativePath.StartsWith(GoogleSpreadsheetReference.GOOGLE_REFERENCE +
                                                           GoogleSpreadsheetReference.GOOGLE_REFERENCE_SPLIT_CHAR))
            {
                zReader = new GoogleReferenceReader(zReference);
            }
            if (zReference.RelativePath.StartsWith(ExcelSpreadsheetReference.EXCEL_REFERENCE +
                                                           ExcelSpreadsheetReference.EXCEL_REFERENCE_SPLIT_CHAR))
            {
                zReader = new ExcelReferenceReader(zReference);
            }

            if (zReader == null)
            {
                zReader = new CSVReferenceReader(zReference);
            }

            zReader.ProgressReporter = zProgressReporter;
            return zReader;
        }

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

            if (zReferenceReader == null)
            {
                zReferenceReader.ProgressReporter = zProgressReporter;
            }
            return null;
        }            
    }
}
