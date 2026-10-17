////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2026 Tim Stair
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
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;
using System.Linq;
using CardMaker.Events.Managers;

namespace Support.Google.Sheets
{
    public static class GoogleSpreadsheet
    {
        public static Spreadsheet GetSpreadsheet(string sSpreadsheetId)
        {
            var zSheetsService = CreateSheetsService();
            // https://developers.google.com/sheets/api/guides/concepts (specifying the sheet name results in all the data)
            var zResult = zSheetsService.Spreadsheets.Get(sSpreadsheetId).Execute();
            return zResult;
        }

        public static bool DoesChildSheetExist(string sSpreadsheetId, string sSheetName)
        {
            var zSpreadsheetInfo = GetSpreadsheet(sSpreadsheetId);
            var zSheetInfo = zSpreadsheetInfo.Sheets.FirstOrDefault(zSheet => sSheetName == zSheet.Properties.Title);
            return zSheetInfo != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sSpreadsheetId"></param>
        /// <param name="sSheetName"></param>
        /// <param name="bAutoFillBlanks"></param>
        /// <returns></returns>
        public static List<List<string>> GetSheetContentsBySpreadsheetId(string sSpreadsheetId, string sSheetName, bool bAutoFillBlanks = true)
        {
            var zSheetsService = CreateSheetsService();
            // https://developers.google.com/sheets/api/guides/concepts (specifying the sheet name results in all the data)

            if (!DoesChildSheetExist(sSpreadsheetId, sSheetName)) return null;

            var zValueRange = zSheetsService.Spreadsheets.Values.Get(sSpreadsheetId, sSheetName).Execute();
            var listAllRows = new List<List<string>>();

            var nColumnCount = -1;

            foreach (var zRowCells in zValueRange.Values)
            {
                // blank rows are ignored
                if(zRowCells.Count == 0)
                    continue;
                // get the overall column count based on the first line that has actual content (treated as the header row)
                if(nColumnCount == -1)
                    nColumnCount = zRowCells.Count;

                var listColumns = new List<string>();
                foreach (var zCell in zRowCells)
                {
                    listColumns.Add(zCell.ToString());
                }
                // blank rows are not include
                if (listColumns.Count > 0)
                {
                    if (bAutoFillBlanks && listColumns.Count < nColumnCount)
                    {
                        listColumns.AddRange(new int[nColumnCount - listColumns.Count].Select(x => string.Empty).ToList());
                    }
                    listAllRows.Add(listColumns);
                }
            }

            ProcessNewLines(listAllRows);
            return listAllRows;
        }

        public static List<string> GetSheetNames(string sSpreadsheetId)
        {
            var zSheetsService = CreateSheetsService();
            // TODO: this can likely be optimized to not return everything (contents won't be included)
            var zSpreadsheet = zSheetsService.Spreadsheets.Get(sSpreadsheetId).Execute();
            return GetSheetNames(zSpreadsheet);
        }

        public static List<string> GetSheetNames(Spreadsheet zSheet)
        {
            var zSheetsService = CreateSheetsService();
            // TODO: this can likely be optimized to not return everything (contents won't be included)
            return zSheet.Sheets.Select(sheet => sheet.Properties.Title).ToList();
        }

        public static string GetSpreadsheetName(Spreadsheet zSheet)
        {
            return zSheet.Properties.Title;
        }

        /// <summary>
        /// This method is used to verify connectivity and will throw an exception
        /// </summary>
        public static void MakeSimpleSpreadsheetRequest()
        {
#warning TODO: this is a hack to just make any valid call and prove auth is okay
            CreateSheetsService().Spreadsheets.Get(string.Empty).Execute();
        }

        private static SheetsService CreateSheetsService()
        {
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleAuthManager.Instance.UserCredential,
                ApplicationName = CardMakerConstants.APPLICATION_NAME
            });
        }

        /// <summary>
        /// Converts newline characters to newline escape characters
        /// </summary>
        /// <param name="listLines">The list of data representing the sheet of strings</param>
        private static void ProcessNewLines(List<List<string>> listLines)
        {
            foreach (var listLine in listLines)
            {
                for (var nIdx = 0; nIdx < listLine.Count; nIdx++)
                {
                    if (listLine[nIdx] == null)
                    {
                        // however unlikely
                        continue;
                    }
                    listLine[nIdx] = listLine[nIdx].Replace("\n", "\\n");
                }
            }
        }
    }
}