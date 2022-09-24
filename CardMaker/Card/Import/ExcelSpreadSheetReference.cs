////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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

namespace CardMaker.Card.Import
{
    class ExcelSpreadsheetReference
    {
        public const string EXCEL_REFERENCE = "excel";
        public const char EXCEL_REFERENCE_SPLIT_CHAR = ';';

        private enum ExcelSpreadsheetOnlyReferenceIndex : int
        {
            SpreadsheetFile = 0,
            SheetName = 1
        }

        public string SpreadsheetFile { get; set; }
        public string SheetName { get; set; }

        public ExcelSpreadsheetReference() { }
        public ExcelSpreadsheetReference(string sFilename)
        {
            SpreadsheetFile = sFilename;
        }
        public ExcelSpreadsheetReference(string sFilename, string sSheet)
        {
            SpreadsheetFile = sFilename;
            SheetName = sSheet;
        }

        public static ExcelSpreadsheetReference parse(string sInput)
        {
            if(string.IsNullOrWhiteSpace(sInput))
                throw new Exception("Unable to read empty Excel Spreadsheet reference.");

            var arrayComponents = sInput.Split(EXCEL_REFERENCE_SPLIT_CHAR);
            if (arrayComponents.Length == 3)
            {
                return new ExcelSpreadsheetReference(arrayComponents[(int)ExcelSpreadsheetOnlyReferenceIndex.SpreadsheetFile + 1],
                    arrayComponents[(int)ExcelSpreadsheetOnlyReferenceIndex.SheetName + 1]);
            }

            throw new Exception("Unable to read invalid Excel Spreadsheet reference.");
        }

        public static ExcelSpreadsheetReference parseSpreadsheetOnlyReference(string sInput)
        {
            if (string.IsNullOrWhiteSpace(sInput))
                throw new Exception("Unable to read empty Excel Spreadsheet reference.");

            var arrayComponents = sInput.Split(EXCEL_REFERENCE_SPLIT_CHAR);
            if (arrayComponents.Length == 1)
            {
                return new ExcelSpreadsheetReference(arrayComponents[(int)ExcelSpreadsheetOnlyReferenceIndex.SpreadsheetFile]);
            }
            throw new Exception("Unable to read invalid Google Spreadsheet reference.");
        }

        public static string generateFullReference(string sSpreadsheetFile, string sSheetName)
        {
            return EXCEL_REFERENCE
                   + EXCEL_REFERENCE_SPLIT_CHAR
                   + sSpreadsheetFile
                   + EXCEL_REFERENCE_SPLIT_CHAR
                   + sSheetName;
        }
    }
}
