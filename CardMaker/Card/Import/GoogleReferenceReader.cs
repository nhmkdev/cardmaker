////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
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
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Import
{
    public class GoogleReferenceReader : ReferenceReader
    {
        public const string APP_NAME = "CardMaker";
        public const string CLIENT_ID = "455195524701-cmdvv6fl5ru9uftin99kjmhojt36mnm9.apps.googleusercontent.com";
        private readonly SpreadsheetsService m_zSpreadsheetsService;

        public class GoogleCacheItem
        {
            public string Reference { get; set; }
            public List<List<string>> Data { get; set; } 
        }

        private readonly Dictionary<string, List<List<string>>> m_dictionaryDataCache = new Dictionary<string, List<List<string>>>();

        private bool m_bCacheUpdated;

        public string ReferencePath { get; }

        public GoogleReferenceReader()
        {
            m_zSpreadsheetsService = GoogleSpreadsheet.GetSpreadsheetsService(APP_NAME, CLIENT_ID,
                CardMakerInstance.GoogleAccessToken);

            LoadCache();

            if (!IsAllDataCached() || CardMakerInstance.ForceDataCacheRefresh)
            {
                // local cache is not enough to load this reference, check token access
                if (!GoogleApi.VerifyAccessToken(CardMakerInstance.GoogleAccessToken))
                {
                    CardMakerInstance.GoogleCredentialsInvalid = true;
                }
            }
        }

        public GoogleReferenceReader(ProjectLayoutReference zReference) : this()
        {
            ReferencePath = zReference.RelativePath;
        }

        private void LoadCache()
        {
            if (!CardMakerSettings.EnableGoogleCache)
            {
                return;
            }

            List<GoogleCacheItem> listCacheItems = null;
            if (SerializationUtils.DeserializeFromXmlFile(Path.Combine(CardMakerInstance.StartupPath, CardMakerConstants.GOOGLE_CACHE_FILE), CardMakerConstants.XML_ENCODING,
                ref listCacheItems))
            {
                foreach (var zCacheItem in listCacheItems)
                {
                    m_dictionaryDataCache.Add(zCacheItem.Reference, zCacheItem.Data);
                }
            }            
        }

        private bool IsAllDataCached()
        {
            return m_dictionaryDataCache.ContainsKey(GetCacheKey(ReferencePath)) &&
                   m_dictionaryDataCache.ContainsKey(GetCacheKey(GetDefinesReference())) &&
                   m_dictionaryDataCache.ContainsKey(GetCacheKey(ReferencePath, Deck.DEFINES_DATA_POSTFIX));
        }

        public void GetData(string sGoogleReference, List<List<string>> listData, bool bRemoveFirstRow, string sNameAppend = "")
        {
            var arraySettings = sGoogleReference.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (arraySettings.Length < 3)
            {
                return;
            }

            string sCacheKey = GetCacheKey(sGoogleReference, sNameAppend);
            List<List<string>> listCacheData;
            if (!CardMakerInstance.ForceDataCacheRefresh && m_dictionaryDataCache.TryGetValue(sCacheKey, out listCacheData))
            {
                Logger.AddLogLine("Loading {0} from local cache".FormatString(sCacheKey));
                listData.AddRange(listCacheData);
                return;
            }

            var sSpreadsheetName = arraySettings[1];
            var sSheetName = arraySettings[2] + sNameAppend;

            var bCredentialsError = false;

            List<List<string>> listGoogleData;
            try
            {
                listGoogleData = GoogleSpreadsheet.GetSpreadsheet(m_zSpreadsheetsService, sSpreadsheetName, sSheetName);
            }
            catch (InvalidCredentialsException e)
            {
                Logger.AddLogLine("Credentials exception: " + e.Message);
                bCredentialsError = true;
                listGoogleData = null;
            }
            catch (Exception e)
            {
                Logger.AddLogLine("General exception: " + e.Message);
                listGoogleData = null;
            }

            if (null == listGoogleData)
            {
                Logger.AddLogLine("Failed to load data from Google Spreadsheet." + "[" + sSpreadsheetName + "," + sSheetName + "]" + (bCredentialsError ? " Google reported a problem with your credentials." : string.Empty));
            }
            else
            {
                if (bRemoveFirstRow && listGoogleData.Count > 0)
                {
                    listGoogleData.RemoveAt(0);
                }

                listData.AddRange(listGoogleData);
                if (m_dictionaryDataCache.ContainsKey(sCacheKey))
                {
                    m_dictionaryDataCache.Remove(sCacheKey);
                }
                m_dictionaryDataCache.Add(sCacheKey, listGoogleData);
                m_bCacheUpdated = true;
            }
        }

        public void GetReferenceData(ProjectLayoutReference zReference, List<List<string>> listReferenceData)
        {
            GetData(ReferencePath, listReferenceData, false);
        }

        public void GetProjectDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            if (null == ProjectManager.Instance.ProjectFilePath)
            {
                return;
            }

            var sProjectDefineSheetReference = GetDefinesReference();

            GetData(sProjectDefineSheetReference, listDefineData, true);
        }

        public void GetDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            GetData(ReferencePath, listDefineData, true, Deck.DEFINES_DATA_POSTFIX);
        }

        private string GetDefinesReference()
        {
            return CardMakerConstants.GOOGLE_REFERENCE
                    + CardMakerConstants.GOOGLE_REFERENCE_SPLIT_CHAR
                    + (string.IsNullOrEmpty(ProjectManager.Instance.LoadedProject.overrideDefineReferenceName) ?
                        Path.GetFileNameWithoutExtension(ProjectManager.Instance.ProjectFilePath) :
                        ProjectManager.Instance.LoadedProject.overrideDefineReferenceName)
                    + CardMakerConstants.GOOGLE_REFERENCE_SPLIT_CHAR
                    + "defines";
        }

        private string GetCacheKey(string sReference, string sNameAppend = "")
        {
            return sReference + "::" + sNameAppend;
        }

        public void FinalizeReferenceLoad()
        {
            if (!m_bCacheUpdated || !CardMakerSettings.EnableGoogleCache)
            {
                return;
            }

            List<GoogleCacheItem> listCacheItems = new List<GoogleCacheItem>();
            foreach (var zPair in m_dictionaryDataCache)
            {
                listCacheItems.Add(new GoogleCacheItem()
                {
                    Reference = zPair.Key,
                    Data = zPair.Value
                });
            }
            SerializationUtils.SerializeToXmlFile(CardMakerConstants.GOOGLE_CACHE_FILE, listCacheItems,
                CardMakerConstants.XML_ENCODING);            
        }
    }
}
