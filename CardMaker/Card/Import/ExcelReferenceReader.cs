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

using System;
using System.Collections.Generic;
using System.IO;
using CardMaker.Events.Managers;
using CardMaker.XML;
using ClosedXML.Excel;
using Support.IO;
using System.Runtime.InteropServices;

namespace CardMaker.Card.Import
{
    class ExcelReferenceReader : ReferenceReader
    {
        private const string DEFAULT_DEFINES_SHEET_NAME = "defines";

        // SheetName to Variables
        private readonly Dictionary<string, List<List<string>>> m_dictionaryDataCache = new Dictionary<string, List<List<string>>>();

        public string ReferencePath { get; }

        public ExcelReferenceReader() { /* Intentionally do nothing */ }

        public ExcelReferenceReader(ProjectLayoutReference zReference)
        {
            ReferencePath = zReference.RelativePath;
        }

        public void GetData(ExcelSpreadsheetReference zReference, List<List<string>> listData, bool bRemoveFirstRow, string sNameAppend = "")
        {
            var sSpreadsheetName = zReference.SpreadsheetFile;
            var sSheetName = zReference.SheetName + sNameAppend;

            // This covers the case where we try to open the project level defines.
            if (!File.Exists(sSpreadsheetName))
            {
                return;
            }

            // Open the workbook
            var workbook = new XLWorkbook(sSpreadsheetName);

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
                ProgressReporter.AddIssue("Missing sheet from Excel Spreadsheet." + "[" + sSpreadsheetName + "," + sSheetName + "]");
                return;
            }

            // Get all data for the given worksheet
            // For empty rows put an empty string
            List<List<string>> listExcelData = new List<List<string>>();
            IXLRange usedRange = worksheet.RangeUsed();
            // empty sheets just return null
            if (usedRange != null)
            {
                foreach (IXLRangeRow row in usedRange.Rows())
                {
                    List<string> rowData = new List<string>();
                    foreach (IXLCell cell in row.Cells())
                    {
                        rowData.Add(cell.Value.ToString());
                    }

                    listExcelData.Add(rowData);
                }
            }

            if (listExcelData.Count == 0)
            {
                ProgressReporter.AddIssue("Failed to load any data from Excel Spreadsheet." + "[" + sSpreadsheetName + "," + sSheetName + "]");
            }
            else
            {
                if (bRemoveFirstRow && listExcelData.Count > 0)
                {
                    listExcelData.RemoveAt(0);
                }

                listData.AddRange(listExcelData);
            }
        }

        public override void GetDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            GetData(ExcelSpreadsheetReference.parse(ReferencePath), listDefineData, true, Deck.DEFINES_DATA_POSTFIX);
        }

        public override void GetProjectDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            if (null == ProjectManager.Instance.ProjectFilePath)
            {
                return;
            }

            ExcelSpreadsheetReference zExcelSpreadSheetReference = ExcelSpreadsheetReference.parse(zReference.RelativePath);
            zExcelSpreadSheetReference.SheetName = DEFAULT_DEFINES_SHEET_NAME;
            GetData(zExcelSpreadSheetReference, listDefineData, true);
        }

        public override void GetReferenceData(ProjectLayoutReference zReference, List<List<string>> listReferenceData)
        {
            GetData(ExcelSpreadsheetReference.parse(ReferencePath), listReferenceData, false);
        }
    }
}
