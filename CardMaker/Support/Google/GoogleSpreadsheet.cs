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
using System.Linq;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Support.IO;
using Support.UI;

namespace Support.Google
{
    public class GoogleSpreadsheet
    {
        private GoogleInitializerFactory zInitializerFactory;

        public GoogleSpreadsheet(GoogleInitializerFactory zInitializerFactory)
        {
            this.zInitializerFactory = zInitializerFactory;
        }

        public List<List<string>> GetSheetContentsBySpreadsheetName(string sSpreadsheetName, string sSheetName,
            bool bAutoFillBlanks = true)
        {
            return GetSheetContentsBySpreadsheetId(GetSpreadsheetId(sSpreadsheetName), sSheetName, bAutoFillBlanks);
        }

        /// <summary>
            /// 
            /// </summary>
            /// <param name="sSpreadsheetId"></param>
            /// <param name="sSheetName"></param>
            /// <param name="bAutoFillBlanks"></param>
            /// <returns></returns>
        public List<List<string>> GetSheetContentsBySpreadsheetId(string sSpreadsheetId, string sSheetName, bool bAutoFillBlanks = true)
        {
            var zSheetsService = CreateSheetsService();
            // https://developers.google.com/sheets/api/guides/concepts (specifying the sheet name results in all the data)
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

            processNewLines(listAllRows);
            return listAllRows;
        }

        public List<string> GetSheetNames(string sSpreadsheetId)
        {
            var zSheetsService = CreateSheetsService();
            // TODO: this can likely be optimized to not return everything (contents won't be included)
            var zSpreadSheet = zSheetsService.Spreadsheets.Get(sSpreadsheetId).Execute();
            return zSpreadSheet.Sheets.Select(sheet => sheet.Properties.Title).ToList();
        }

        /// <summary>
        /// Retrieves all the spreadsheets available in the drive with a mapping to the id
        /// </summary>
        /// <returns></returns>
        public string GetSpreadsheetId(string sSpreadsheetName)
        {
            var zDriveService = CreateDriveService();

            var zListRequest = zDriveService.Files.List();
            // lookup only spreadsheets
            zListRequest.Q = "name = '{0}'".FormatString(sSpreadsheetName);
            zListRequest.Fields = "files(id)";

            // references -- 
            // https://www.daimto.com/search-files-on-google-drive-with-c/
            // https://developers.google.com/drive/api/v3/search-parameters
            var zResultFileList = zListRequest.Execute();
            if (zResultFileList.Files.Count == 1)
            {
                return zResultFileList.Files[0].Id;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves all the spreadsheets available in the drive with a mapping to the id
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetSpreadsheetList()
        {
            var dictionaryNameID = new Dictionary<string, string>();
            var zDriveService = CreateDriveService();

            var zListRequest = zDriveService.Files.List();
            // lookup only spreadsheets
            zListRequest.Q = "mimeType='application/vnd.google-apps.spreadsheet'";
            zListRequest.PageSize = 100;
            zListRequest.Fields = "nextPageToken, files(name, id)";

            // references -- 
            // https://www.daimto.com/search-files-on-google-drive-with-c/
            // https://developers.google.com/drive/api/v3/search-parameters
            do
            {
                var zResultFileList = zListRequest.Execute();
                foreach (var zFile in zResultFileList.Files)
                {
                    dictionaryNameID.Add(zFile.Name, zFile.Id);
                }
                zListRequest.PageToken = zResultFileList.NextPageToken;
            } while (zListRequest.PageToken != null);

            return dictionaryNameID;
        }

        /// <summary>
        /// This method is used like an auth check
        /// </summary>
        public void MakeSimpleSpreadsheetRequest()
        {
            var zDriveService = CreateDriveService();

            var zListRequest = zDriveService.Files.List();
            // lookup only spreadsheets
            zListRequest.Q = "mimeType='application/vnd.google-apps.spreadsheet'";
            zListRequest.PageSize = 1;
            zListRequest.Fields = "files(name, id)";

            zListRequest.Execute();
        }

        private SheetsService CreateSheetsService()
        {
            return new SheetsService(zInitializerFactory.CreateInitializer());
        }

        private DriveService CreateDriveService()
        {
            return new DriveService(zInitializerFactory.CreateInitializer());
        }

        /// <summary>
        /// Converts newline characters to newline escape characters
        /// </summary>
        /// <param name="listLines">The list of data representing the sheet of strings</param>
        private static void processNewLines(List<List<string>> listLines)
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