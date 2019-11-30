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
using Excel = Microsoft.Office.Interop.Excel;
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

        public void FinalizeReferenceLoad() { }

        private void CleanUpExcel(Excel.Application xlApp, Excel.Workbook xlWorkbook, Excel.Worksheet xlWorksheet, Excel.Range xlRange)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (xlRange != null)
            {
                Marshal.ReleaseComObject(xlRange);
            }

            if (xlWorksheet != null)
            {
                Marshal.ReleaseComObject(xlWorksheet);
            }

            //close and release
            if (xlWorkbook != null)
            {
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);
            }

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
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

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(sSpreadsheetName);

            Excel.Worksheet xlWorksheet = null;
            foreach(Excel.Worksheet sheet in xlWorkbook.Worksheets)
            {
                if (sheet.Name == sSheetName)
                {
                    xlWorksheet = sheet;
                    break;
                }
            }
            if (xlWorksheet == null)
            {
                Logger.AddLogLine("Missing sheet from Excel Spreadsheet." + "[" + sSpreadsheetName + "," + sSheetName + "]");
                CleanUpExcel(xlApp, xlWorkbook, null, null);
                return;
            }

            List<List<string>> listExcelData = new List<List<string>>();
            Excel.Range xlUsedRange = xlWorksheet.UsedRange;

            foreach(Excel.Range row in xlUsedRange.Rows)
            {
                List<string> rowData = new List<string>();
                for (int colIdx = 0; colIdx < row.Columns.Count; colIdx++)
                {
                    var cell = row.Cells[1, colIdx + 1];
                    if (cell.Value2 != null)
                    {
                        rowData.Add(cell.Value2.ToString());
                    }
                }
                listExcelData.Add(rowData);
            }

            if (listExcelData.Count == 0)
            {
                Logger.AddLogLine("Failed to load any data from Excel Spreadsheet." + "[" + sSpreadsheetName + "," + sSheetName + "]");
            }
            else
            {
                if (bRemoveFirstRow && listExcelData.Count > 0)
                {
                    listExcelData.RemoveAt(0);
                }

                listData.AddRange(listExcelData);
            }

            CleanUpExcel(xlApp, xlWorkbook, xlWorksheet, xlUsedRange);
        }

        public void GetDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            GetData(ExcelSpreadsheetReference.parse(ReferencePath), listDefineData, true, Deck.DEFINES_DATA_POSTFIX);
        }

        public void GetProjectDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            if (null == ProjectManager.Instance.ProjectFilePath)
            {
                return;
            }

            GetData(GetDefinesReference(), listDefineData, true);
        }

        public void GetReferenceData(ProjectLayoutReference zReference, List<List<string>> listReferenceData)
        {
            GetData(ExcelSpreadsheetReference.parse(ReferencePath), listReferenceData, false);
        }

        private static ExcelSpreadsheetReference GetDefinesReference()
        {
            var zExcelSpreadSheetReference = ExcelSpreadsheetReference.parseSpreadsheetOnlyReference(
                (string.IsNullOrEmpty(ProjectManager.Instance.LoadedProject.overrideDefineReferenceName)
                    ? Path.GetFileNameWithoutExtension(ProjectManager.Instance.ProjectFilePath)
                    : ProjectManager.Instance.LoadedProject.overrideDefineReferenceName)
            );
            zExcelSpreadSheetReference.SheetName = DEFAULT_DEFINES_SHEET_NAME;
            return zExcelSpreadSheetReference;
        }
    }
}
