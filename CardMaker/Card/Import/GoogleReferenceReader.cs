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

using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Google;
using Support.Google;
using Support.Google.Sheets;
using Support.IO;
using Support.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace CardMaker.Card.Import
{
    public class GoogleReferenceReader : ReferenceReader
    {
        private const string DEFAULT_DEFINES_SHEET_NAME = "defines";

        // public required to serialize (keep here for backwards compatibility?)
        public class GoogleCacheItem
        {
            public string Reference { get; set; }
            public List<List<string>> Data { get; set; }
        }

        private readonly GoogleSpreadsheetReference m_zSpreadsheetReference;
        private readonly string m_sCacheKeyBase;

        public override ReferenceType ReferenceReaderType => ReferenceType.Google;

        public GoogleReferenceReader() { }

        public GoogleReferenceReader(ProjectLayoutReference zReference) : this()
        {
            m_sCacheKeyBase = zReference.RelativePath;
            m_zSpreadsheetReference = GoogleSpreadsheetReference.Parse(zReference.RelativePath);
        }

        public override ReferenceReader Initialize()
        {
            LoadCache();

            if (!IsAllDataCached() || CardMakerInstance.ForceDataCacheRefresh)
            {
                var zSpreadsheet =
                    new GoogleSpreadsheet(CardMakerInstance.GoogleInitializerFactory);
                try
                {
                    zSpreadsheet.MakeSimpleSpreadsheetRequest();
                }
                catch (GoogleApiException e)
                {
                    if (GoogleApi.IsAuthorizationError(e))
                    {
                        CardMakerInstance.GoogleCredentialsInvalid = true;
                    }
                }
                catch (Exception e)
                {
                    Logger.AddLogLine("Google Access Error: {0}".FormatString(e.Message));
                }
            }

            return this;
        }

        public override bool IsValid()
        {
            return !CardMakerInstance.GoogleCredentialsInvalid;
        }

        public override void HandleInvalid()
        {
            if (CardMakerInstance.GoogleCredentialsInvalid)
            {
                CardMakerInstance.GoogleCredentialsInvalid = false;
                GoogleAuthManager.Instance.FireGoogleAuthCredentialsErrorEvent(
                    () => LayoutManager.Instance.InitializeActiveLayout());
            }
        }

        private void LoadCache()
        {
            if (CardMakerSettings.EnableGoogleCache)
            {
                if (!GoogleReferenceCache.ReadFromDisk())
                {
                    ProgressReporter.AddIssue("Failed to read cache file: {0}".FormatString(GoogleReferenceCache.GetCacheFilePath()));
                }
            }
        }

        private bool IsAllDataCached()
        {
            return GoogleReferenceCache.IsEntryInCache(GetCacheKey(m_sCacheKeyBase)) &&
                   GoogleReferenceCache.IsEntryInCache(GetCacheKey(GetDefinesReference().GenerateFullReference())) &&
                   GoogleReferenceCache.IsEntryInCache(GetCacheKey(m_sCacheKeyBase, Deck.DEFINES_DATA_SUFFIX));
        }

        private List<ReferenceLine> GetData(GoogleSpreadsheetReference zReference, int nStartRow, string sNameAppend = "")
        {
            var sCacheKey = GetCacheKey(zReference.GenerateFullReference(), sNameAppend);
            var listReferenceLines = new List<ReferenceLine>();
            List<List<string>> listCacheData;
            if (!CardMakerInstance.ForceDataCacheRefresh && GoogleReferenceCache.GetCacheEntry(sCacheKey, out listCacheData))
            {
                ProgressReporter.AddIssue("Loading {0} from local cache".FormatString(sCacheKey));
                // The cache contains all rows
                for (var nRow = nStartRow; nRow < listCacheData.Count; nRow++)
                {
                    listReferenceLines.Add(new ReferenceLine(listCacheData[nRow], zReference.SpreadsheetName, nRow));
                }
                return listReferenceLines;
            }

            var sSpreadsheetName = zReference.SpreadsheetName;
            var sSheetName = zReference.SheetName + sNameAppend;

            var bAuthorizationError = false;
            var bError = false;

            List<List<string>> listGoogleData = null;
            try
            {
                var zGoogleSpreadsheet = new GoogleSpreadsheet(CardMakerInstance.GoogleInitializerFactory);
                if (string.IsNullOrWhiteSpace(zReference.SpreadsheetId))
                {
                    ProgressReporter.AddIssue("WARNING: The reference {0}.{1} is missing the Spreadsheet ID. Please reconfigure this reference."
                        .FormatString(zReference.SpreadsheetName, zReference.SheetName));
                    listGoogleData = zGoogleSpreadsheet.GetSheetContentsBySpreadsheetName(sSpreadsheetName, sSheetName);
                }
                else
                {
                    listGoogleData = zGoogleSpreadsheet.GetSheetContentsBySpreadsheetId(zReference.SpreadsheetId, sSheetName);
                }

                // blank data just means an empty or non-existent sheet (generally okay)
                if (listGoogleData == null)
                {
                    listGoogleData = new List<List<string>>();
                }
            }
            catch (GoogleApiException e)
            {
                ProgressReporter.AddIssue("Google Spreadsheet access exception: " + e.Message);
                bAuthorizationError = GoogleApi.IsAuthorizationError(e);
            }
            catch (Exception e)
            {
                ProgressReporter.AddIssue("General exception: " + e.Message);
                listGoogleData = null;
                bError = true;
            }
            if (bAuthorizationError || bError || listGoogleData == null)
            {
                ProgressReporter.AddIssue("Failed to load any data from Google Spreadsheet." + "[" + sSpreadsheetName + "," + sSheetName + "]" + (bAuthorizationError ? " Google reported a problem with your credentials." : String.Empty));
            }
            else
            {
                GoogleReferenceCache.UpdateCacheEntry(sCacheKey, listGoogleData);
                for (var nRow = nStartRow; nRow < listGoogleData.Count; nRow++)
                {
                    listReferenceLines.Add(new ReferenceLine(listGoogleData[nRow], zReference.SpreadsheetName, nRow));
                }
            }

            return listReferenceLines;
        }

        public override List<ReferenceLine> GetProjectDefineData()
        {
            if (string.IsNullOrEmpty(ProjectManager.Instance.ProjectFilePath))
            {
                return new List<ReferenceLine>();
            }

            return GetData(GetDefinesReference(), 1);
        }

        public override List<ReferenceLine> GetDefineData()
        {
            return GetData(m_zSpreadsheetReference, 1, Deck.DEFINES_DATA_SUFFIX);
        }

        public override List<ReferenceLine> GetReferenceData()
        {
            return GetData(m_zSpreadsheetReference, 0);
        }

        private static GoogleSpreadsheetReference GetDefinesReference()
        {
            var zGoogleSpreadSheetReference = GoogleSpreadsheetReference.ParseSpreadsheetOnlyReference(
                (string.IsNullOrEmpty(ProjectManager.Instance.LoadedProject.overrideDefineReferenceName)
                    ? Path.GetFileNameWithoutExtension(ProjectManager.Instance.ProjectFilePath)
                    : ProjectManager.Instance.LoadedProject.overrideDefineReferenceName)
            );
            zGoogleSpreadSheetReference.SheetName = DEFAULT_DEFINES_SHEET_NAME;
            return zGoogleSpreadSheetReference;
        }

        private string GetCacheKey(string sReference, string sNameAppend = "")
        {
            return sReference + "::" + sNameAppend;
        }
    }
}
