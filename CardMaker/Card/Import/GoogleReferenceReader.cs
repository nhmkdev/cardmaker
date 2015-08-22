////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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
using CardMaker.Forms;
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
        private SpreadsheetsService m_zSpreadsheetsService;

        public string ReferencePath { get; private set; }

        public GoogleReferenceReader(ProjectLayoutReference zReference)
        {
            ReferencePath = (File.Exists(zReference.RelativePath)
                            ? Path.GetFullPath(zReference.RelativePath)
                            : CardMakerMDI.ProjectPath + zReference.RelativePath);
            m_zSpreadsheetsService = GoogleSpreadsheet.GetSpreadsheetsService(APP_NAME, CLIENT_ID,
                CardMakerMDI.GoogleAccessToken);

        }

        public void GetData(string sGoogleReference, List<List<string>> listData, bool removeFirstRow, string nameAppend = "")
        {
            var arraySettings = sGoogleReference.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (arraySettings.Length < 3)
            {
                return;
            }

            var sSpreadsheetName = arraySettings[1];
            var sSheetName = arraySettings[2] + nameAppend;

            var bCredentialsError = false;

            List<List<String>> listGoogleData;
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
                if (removeFirstRow && listGoogleData.Count > 0)
                {
                    listGoogleData.RemoveAt(0);
                }

                listData.AddRange(listGoogleData);
            }
        }

        public void GetReferenceData(ProjectLayoutReference zReference, List<List<string>> listReferenceData)
        {
            GetData(ReferencePath, listReferenceData, false);
        }

        public void GetProjectDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            var sProjectDefineSheetReference =
                CardMakerMDI.GOOGLE_REFERENCE
                + CardMakerMDI.GOOGLE_REFERENCE_SPLIT_CHAR
                + Path.GetFileNameWithoutExtension(CardMakerMDI.Instance.LoadedFile)
                + CardMakerMDI.GOOGLE_REFERENCE_SPLIT_CHAR
                + "defines";

            GetData(sProjectDefineSheetReference, listDefineData, true);
        }

        public void GetDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            GetData(ReferencePath, listDefineData, true, Deck.DEFINES_DATA_POSTFIX);
        }
    }
}
