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

using System.Collections.Generic;
using System.Linq;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Support.IO;
using Support.UI;

namespace Support.Google.Sheets
{
    public class GoogleSpreadsheet
    {
        private const string SPREADSHEET_MIMETYPE_QUERY = "mimeType='application/vnd.google-apps.spreadsheet'";

        private GoogleInitializerFactory zInitializerFactory;

        public GoogleSpreadsheet(GoogleInitializerFactory zInitializerFactory)
        {
            this.zInitializerFactory = zInitializerFactory;
        }

        public List<List<string>> GetSheetContentsBySpreadsheetName(string sSpreadsheetName, string sSheetName,
            bool bAutoFillBlanks = true)
        {
            var sSpreadsheetId = GetSpreadsheetId(sSpreadsheetName);
            return sSpreadsheetId == null 
                ? null
                : GetSheetContentsBySpreadsheetId(sSpreadsheetId, sSheetName, bAutoFillBlanks);
        }

        public Spreadsheet GetSpreadsheet(string sSpreadsheetId)
        {
            var zSheetsService = CreateSheetsService();
            // https://developers.google.com/sheets/api/guides/concepts (specifying the sheet name results in all the data)
            var zResult = zSheetsService.Spreadsheets.Get(sSpreadsheetId).Execute();
            return zResult;
        }

        public bool DoesChildSheetExist(string sSpreadsheetId, string sSheetName)
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
        public List<List<string>> GetSheetContentsBySpreadsheetId(string sSpreadsheetId, string sSheetName, bool bAutoFillBlanks = true)
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

            processNewLines(listAllRows);
            return listAllRows;
        }

        public List<string> GetSheetNames(string sSpreadsheetId)
        {
            var zSheetsService = CreateSheetsService();
            // TODO: this can likely be optimized to not return everything (contents won't be included)
            var zSpreadsheet = zSheetsService.Spreadsheets.Get(sSpreadsheetId).Execute();
            return GetSheetNames(zSpreadsheet);
        }

        public List<string> GetSheetNames(Spreadsheet zSheet)
        {
            var zSheetsService = CreateSheetsService();
            // TODO: this can likely be optimized to not return everything (contents won't be included)
            return zSheet.Sheets.Select(sheet => sheet.Properties.Title).ToList();
        }

        public string GetSpreadsheetName(Spreadsheet zSheet)
        {
            return zSheet.Properties.Title;
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
            zListRequest.Q = "name = '{0}' AND {1}".FormatString(sSpreadsheetName, SPREADSHEET_MIMETYPE_QUERY);
            zListRequest.Fields = "files(id)";

            // references -- 
            // https://www.daimto.com/search-files-on-google-drive-with-c/
            // https://developers.google.com/drive/api/v3/search-parameters
            var zResultFileList = zListRequest.Execute();
            if (zResultFileList.Files.Count > 0)
            {
                if(zResultFileList.Files.Count > 1)
                    Logger.AddLogLine("WARNING: There are {0} Spreadsheets with the name {1}. Only the first found will be used. Please re-add the reference to correct this issue.".FormatString(zResultFileList.Files.Count, sSpreadsheetName));
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
        public List<GoogleSheetInfo> GetSpreadsheetList()
        {
            var listSheetInfos = new List<GoogleSheetInfo>();
            var zDriveService = CreateDriveService();

            var zListRequest = zDriveService.Files.List();
            // lookup only spreadsheets
            zListRequest.Q = SPREADSHEET_MIMETYPE_QUERY;
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
                    listSheetInfos.Add(new GoogleSheetInfo()
                    {
                        Name = zFile.Name,
                        Id = zFile.Id
                    });
                }
                zListRequest.PageToken = zResultFileList.NextPageToken;
            } while (zListRequest.PageToken != null);

            return listSheetInfos;
        }

        /// <summary>
        /// This method is used like an auth check
        /// </summary>
        public void MakeSimpleSpreadsheetRequest()
        {
#warning need a new way to check this, drive access is dead anyway
            var zDriveService = CreateDriveService();

            var zListRequest = zDriveService.Files.List();
            // lookup only spreadsheets
            zListRequest.Q = SPREADSHEET_MIMETYPE_QUERY;
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