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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using CardMaker.Events.Managers;
using CardMaker.XML;
using ClosedXML.Excel;

namespace CardMaker.Card.Import
{
    class ExcelReferenceReader : ReferenceReader
    {
        private const string DEFAULT_DEFINES_SHEET_NAME = "defines";

        public ExcelReferenceReader() {}

        public ExcelReferenceReader(ProjectLayoutReference zReference)
        {
            ReferencePath = zReference.RelativePath;
        }

        public List<ReferenceLine> GetData(ExcelSpreadsheetReference zReference, int nStartRow, string sNameAppend = "")
        {
            var sSheetName = zReference.SheetName + sNameAppend;
            var listReferenceLines = new List<ReferenceLine>();

            // This covers the case where we try to open the project level defines.
            if (!File.Exists(zReference.SpreadsheetFile))
            {
                return listReferenceLines;
            }

            // Open the workbook
            var workbook = new XLWorkbook(zReference.SpreadsheetFile);

            // Get all worksheets and find the one referenced
            IXLWorksheet worksheet = null;
            foreach (IXLWorksheet sheet in workbook.Worksheets)
            {
                if (sheet.Name == sSheetName)
                {
                    worksheet = sheet;
                    break;
                }
            }

            if (worksheet == null)
            {
                ProgressReporter.AddIssue("Missing sheet from Excel Spreadsheet." + "[" + zReference.SpreadsheetFile + "," + sSheetName + "]");
                return listReferenceLines;
            }

            // Get all data for the given worksheet
            // For empty rows put an empty string
            IXLRange usedRange = worksheet.RangeUsed();
            // empty sheets just return null
            if (usedRange != null)
            {
                var zRows = usedRange.Rows().ToList();
                for(var nRow = nStartRow; nRow < zRows.Count(); nRow++)
                {
                    var rowData = new List<string>();
                    foreach (IXLCell cell in zRows[nRow].Cells())
                    {
                        rowData.Add(cell.Value.ToString());
                    }
                    listReferenceLines.Add(new ReferenceLine(rowData, zReference.SpreadsheetFile, nRow));
                }
            }

            if (listReferenceLines.Count == 0)
            {
                ProgressReporter.AddIssue("Failed to load any data from Excel Spreadsheet." + "[" + zReference.SpreadsheetFile + "," + sSheetName + "]");
            }
            return listReferenceLines;
        }

        public override List<ReferenceLine> GetDefineData(ProjectLayoutReference zReference)
        {
            return GetData(ExcelSpreadsheetReference.parse(ReferencePath), 1, Deck.DEFINES_DATA_POSTFIX);
        }

        public override List<ReferenceLine> GetProjectDefineData()
        {
            if (string.IsNullOrEmpty(ProjectManager.Instance.ProjectFilePath))
            {
                return new List<ReferenceLine>();
            }
            var sReferencePath =
                Path.GetDirectoryName(ProjectManager.Instance.ProjectFilePath)
                + Path.DirectorySeparatorChar
                + Path.GetFileNameWithoutExtension(ProjectManager.Instance.ProjectFilePath)
                + "_defines.xlsx";

            var zExcelSpreadSheetReference = new ExcelSpreadsheetReference(sReferencePath, DEFAULT_DEFINES_SHEET_NAME);
            return GetData(zExcelSpreadSheetReference, 1);
        }

        public override List<ReferenceLine> GetReferenceData(ProjectLayoutReference zReference)
        {
            return GetData(ExcelSpreadsheetReference.parse(ReferencePath), 0);
        }
    }
}
